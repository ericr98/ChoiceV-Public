using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.FsDatabase;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.Terminal.TerminalShops {
    public class CharacterTerminalShop : TerminalShop {
        public const int TATTOO_PRICE = 3;
        public const int HAIR_GROWTH_STOP = 2;

        public CharacterTerminalShop() : base("CHARACTER_SHOP", "Umdesigner") {

        }

        public override Menu getBuyableOptions(IPlayer player) {
            var tokens = TerminalShopController.getPlayerTokens(player);

            var menu = new Menu("Umdesigner", "Was möchtest du kaufen?");

            menu.addMenuItem(new ClickMenuItem("Charakter umdesignen", "Gehe in den den Charaktereditor um ein Facelift durchzuführen", "", "ON_TERMINAL_OPEN_CHAR_EDITOR").needsConfirmation("Charaktereditor öffnen?", "Wirklich öffnen?"));

            if(tokens >= TATTOO_PRICE) {
                menu.addMenuItem(new ClickMenuItem("Tattoo kaufen", "Kaufe dir ein Tattoo", $"{TATTOO_PRICE} Marken", "ON_TERMINAL_BUY_TATTOO").needsConfirmation("Tattoo kaufen?", "Wirklich Tattoo kaufen?"));
            } else {
                menu.addMenuItem(new StaticMenuItem("Tattoo kaufen", "Kaufe dir ein Tattoo", $"zu teuer ({TATTOO_PRICE}M)", MenuItemStyle.yellow));
            }

            menu.addMenuItem(new ClickMenuItem("Haarwachstum aussetzen", "MIT DEM SUPPORT ABSREPCHEN. Lässt Haarwachstum aussetzen. Dadurch werden die Haare nie länger. Nur i.V.m mit Haarausfall (Glatze) ODER Perücke auszuspielen.", $"{HAIR_GROWTH_STOP} Marken", "ON_TERMINAL_ADD_NO_HAIR_GROWTH"));

            return menu;
        }

        public override Menu getAlreadyBoughtListing(IPlayer player) {
            var menu = new Menu("Bereits Gekauftes zurückgeben", "Welches Tattoo möchtest du zurückgeben?");

            foreach(var tattoo in TattooController.getPlayerTattoos(player)) {
                var data = new Dictionary<string, dynamic> {
                    { "Tattoo", tattoo }
                };

                menu.addMenuItem(new ClickMenuItem($"{tattoo.LocalizedName} zurückgeben", $"Gib das Tatto mit Namen {tattoo.LocalizedName} in der Zone: {tattoo.Zone} zurück", "", "TERMINAL_RETURN_TATTOO", MenuItemStyle.red).withData(data).needsConfirmation("Tattoo zurückgeben?", $"Für {TATTOO_PRICE}M zurückgeben?"));
            }

            if(player.hasData("NO_HAIR_GROWTH")) {
                menu.addMenuItem(new ClickMenuItem("\"Kein Haarwachstum\" zurückgeben", $"Gib das Aussetzen des Haarwachstums zurück. Zu wirst {HAIR_GROWTH_STOP} zurückerhalten", "", "TERMINAL_RETURN_NO_HAIR_GROWTH").needsConfirmation("Haarwachstum wieder zulassen?", $"Für {HAIR_GROWTH_STOP}M zurückgeben?"));
            }

            return menu;
        }

        public override bool hasPlayerBoughtSomething(IPlayer player) {
            return true;
        }

        public override void onPlayerLand(IPlayer player, ref executive_person_file file) { }
    }

    public class CharacterTerminalController : ChoiceVScript {
        public CharacterTerminalController() {
            TerminalShopController.addTerminalShop(new CharacterTerminalShop());

            EventController.addMenuEvent("ON_TERMINAL_OPEN_CHAR_EDITOR", onTerminalOpenCharEditor);
            EventController.addMenuEvent("ON_TERMINAL_BUY_TATTOO", onTerminalBuyTattoo);

            EventController.addMenuEvent("TERMINAL_RETURN_TATTOO", onTerminalReturnTattoo);

            EventController.addMenuEvent("ON_TERMINAL_ADD_NO_HAIR_GROWTH", onTerminallAddNoHairGrowth);
            EventController.addMenuEvent("TERMINAL_RETURN_NO_HAIR_GROWTH", onTerminalReturnHairGrowth);
        }

        private bool onTerminalOpenCharEditor(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            ConnectionController.openCharCreator(player, true);

            return true;
        }

        private bool onTerminalBuyTattoo(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            player.showMenu(TattooController.getTattooMenu(player, player, true, false, () => {
                return TerminalShopController.addOrRemovePlayerTokens(player, -CharacterTerminalShop.TATTOO_PRICE);
            }), false);

            return true;
        }

        private bool onTerminalReturnTattoo(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var tattoo = (configtattoo)data["Tattoo"];

            if(TattooController.removePlayerTattoo(player, tattoo)) {
                TerminalShopController.addOrRemovePlayerTokens(player, CharacterTerminalShop.TATTOO_PRICE);

                player.sendNotification(Constants.NotifactionTypes.Info, $"Dein {tattoo.Name} wurde entfernt und du hast {CharacterTerminalShop.TATTOO_PRICE} Marken zurückerhalten!", "Tattoo entfernt", Constants.NotifactionImages.Plane);
            }

            return true;
        }

        private bool onTerminallAddNoHairGrowth(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(TerminalShopController.addOrRemovePlayerTokens(player, -CharacterTerminalShop.HAIR_GROWTH_STOP)) {
                player.setPermanentData("NO_HAIR_GROWTH", "1");
                player.sendNotification(Constants.NotifactionTypes.Info, $"Du hast die \"Kein Haarwachstum\" Option gekauft!", "Kein Haarwachstum erworben", Constants.NotifactionImages.Plane);
            }

            return true;
        }

        private bool onTerminalReturnHairGrowth(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            TerminalShopController.addOrRemovePlayerTokens(player, CharacterTerminalShop.HAIR_GROWTH_STOP);
            player.resetPermantData("NO_HAIR_GROWTH");
            player.sendNotification(Constants.NotifactionTypes.Warning, $"Du hast die \"Kein Haarwachstum\" Option zurückgegeben und {CharacterTerminalShop.HAIR_GROWTH_STOP} Marken zurückerhalten!", "Kein Haarwachstum zurückgegeben", Constants.NotifactionImages.Plane);

            return true;
        }
    }
}
