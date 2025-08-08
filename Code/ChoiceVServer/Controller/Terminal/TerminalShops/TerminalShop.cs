using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.FsDatabase;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.InputMenuItem;

namespace ChoiceVServer.Controller {
    public abstract class TerminalShop {
        public string Identifier { get; private set; }
        public string Name { get; private set; }

        public TerminalShop(string identifier, string name) {
            Identifier = identifier;
            Name = name;
        }

        public abstract Menu getBuyableOptions(IPlayer player);
        public abstract Menu getAlreadyBoughtListing(IPlayer player);
        public abstract bool hasPlayerBoughtSomething(IPlayer player);

        public abstract void onPlayerLand(IPlayer player, ref executive_person_file file);
    }

    public class NPCTerminalShopModule : NPCModule {
        private TerminalShop Shop;

        public NPCTerminalShopModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, settings) {
            Shop = TerminalShopController.getShopByIdentifier(settings["ShopIdentifier"]);
        }

        public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
            return new StaticMenuItem("Terminal-Shop", "Dierser NPC bietet einen Terminal Shop an", $"{Shop.Name}");
        }

        public override List<MenuItem> getMenuItems(IPlayer player) {
            var terminalShopMenu = new Menu(Shop.Name, "Was möchtest du tun?");

            var shoppingMenu = Shop.getBuyableOptions(player);
            terminalShopMenu.addMenuItem(new MenuMenuItem(shoppingMenu.Name, shoppingMenu));

            if(Shop.hasPlayerBoughtSomething(player)) {
                var alreadyMenu = Shop.getAlreadyBoughtListing(player);
                if(alreadyMenu.getMenuItemCount() <= 0) {
                    alreadyMenu.addMenuItem(new StaticMenuItem("Noch nichts gekauft", "Noch nichts gekauft!", "", MenuItemStyle.yellow));
                }

                terminalShopMenu.addMenuItem(new MenuMenuItem(alreadyMenu.Name, alreadyMenu));
            }

            return new List<MenuItem> { new MenuMenuItem(terminalShopMenu.Name, terminalShopMenu) };
        }

        public override void onRemove() { }
    }

    public class TerminalShopController : ChoiceVScript {
        private static List<TerminalShop> AllShops = new();

        public TerminalShopController() {
            PedController.addNPCModuleGenerator("Terminal-Shop", terminalShopModuleGenrator, terminalShopModuleCallback);

            InteractionController.addPlayerInteractionElement(
                new ConditionalGeneratedPlayerInteractionMenuElement(
                    (s, t) => new InputMenuItem("Spieler Marken geben/nehmen", "Gib, oder nimm, einem Spieler Marken für das Terminal. Eine negative Zahl nimmt dem Spieler Marken weg!", "", "SUPPORT_TERMINAL_GIVE_PLAYER_TOKENS", MenuItemStyle.yellow).needsConfirmation("Marken geben/nehmen?", "Wirklich Marken geben/nehmen"),
                    s => s is IPlayer sender && sender.getCharacterData().AdminMode,
                    t => t is IPlayer target && target.hasState(Constants.PlayerStates.InTerminal)
                )
            );
            EventController.addMenuEvent("SUPPORT_TERMINAL_GIVE_PLAYER_TOKENS", onSupportTerminalGivePlayerTokens);
        }

        private bool onSupportTerminalGivePlayerTokens(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as InputMenuItemEvent;

            var target = (IPlayer)data["InteractionTarget"];

            var number = int.Parse(evt.input);

            addOrRemovePlayerTokens(target, number);

            if(number > 0) {
                player.sendNotification(Constants.NotifactionTypes.Success, $"Spieler erfolgreich {number} Marken gegeben!", "");
            } else {
                player.sendNotification(Constants.NotifactionTypes.Info, $"Spieler erfolgreich {number} Marken genommen!", "");
            }
            return true;
        }

        private List<MenuItem> terminalShopModuleGenrator(ref Type codeType) {
            codeType = typeof(NPCTerminalShopModule);

            return new List<MenuItem> { new ListMenuItem("Shopart", "Wähle die Shopart aus", AllShops.Select(s => s.Name).ToArray(), "") };
        }

        private void terminalShopModuleCallback(IPlayer player, MenuStatsMenuItem.MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
            var shopSelectionEvt = evt.elements[0].FromJson<ListMenuItem.ListMenuItemEvent>();

            var shop = AllShops.FirstOrDefault(s => s.Name == shopSelectionEvt.currentElement);

            creationFinishedCallback.Invoke(new Dictionary<string, dynamic> { { "ShopIdentifier", shop.Identifier } });
        }

        public static TerminalShop getShopByIdentifier(string identifier) {
            return AllShops.FirstOrDefault(s => s.Identifier == identifier);
        }

        public static void addTerminalShop(TerminalShop shop) {
            AllShops.Add(shop);
        }

        private class PlayerTerminalHudUpdateCefEvent : IPlayerCefEvent {
            public string Event { get; private set; }
            public int tokens;

            public PlayerTerminalHudUpdateCefEvent(string evt, int tokens) {
                Event = evt;
                this.tokens = tokens;
            }
        }

        public static void refreshPlayerTerminalDisplay(IPlayer player) {
            var tokens = getPlayerTokens(player);
            if(tokens > 0) {
                player.emitCefEventNoBlock(new PlayerTerminalHudUpdateCefEvent("UPDATE_TERMINAL_HUD", tokens));
            } else {
                player.emitCefEventNoBlock(new PlayerTerminalHudUpdateCefEvent("REMOVE_TERMINAL_HUD", tokens));
            }
        }

        public static int getPlayerTokens(IPlayer player) {
            if(player.hasData("NOT_YET_LEFT_TERMINAL")) {
                return Convert.ToInt32((string)player.getData("TERMINAL_TOKENS"));
            } else {
                return 0;
            }
        }

        public static bool addOrRemovePlayerTokens(IPlayer player, int change) {
            var tokens = getPlayerTokens(player);

            if(change > 0 || tokens >= Math.Abs(change)) {
                tokens += change;

                player.setPermanentData("TERMINAL_TOKENS", tokens.ToString());
                refreshPlayerTerminalDisplay(player);
                return true;
            } else {
                return false;
            }
        }

        public static void onPlayerLeaveAirport(IPlayer player, ref executive_person_file file) {
            foreach(var shop in AllShops) {
                try {
                    shop.onPlayerLand(player, ref file);
                } catch(Exception) {
                    player.sendBlockNotification("Es gab einen Fehler bei einem der Shops!", "");
                }
            }
        }
    }
}
