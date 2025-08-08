using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.FsDatabase;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Terminal.TerminalShops {
    public class IllegalTerminalShop : TerminalShop {
        public const int TERMINAL_WARRANT_COST = 2;
        public const int TERMINAL_ILLEGAL_ITEM_COST = 3;

        public IllegalTerminalShop() : base("ILLEGAL_SHOP", "Verdächtige Person") {

        }

        public override Menu getBuyableOptions(IPlayer player) {
            if(Config.IsStressTestActive) {
                return new Menu("Verdächtige Person", "Diese Funktion ist nicht im Stresstest verfügbar. Keine Käufe möglich!");
            }

            var menu = new Menu("Verdächtige Person", "Was möchtest du kaufen?");
            menu.addMenuItem(new StaticMenuItem("Information", "Alle gekauften Waren/Leistungen werden erst NACH dem Landen des Flugzeuges dem Inventar hinzugefügt!", "siehe Beschreibung"));

            var tokens = TerminalShopController.getPlayerTokens(player);

            if(!player.hasData("TERMINAL_WARRANT")) {
                if(tokens >= TERMINAL_WARRANT_COST) {
                    var warrantMenu = new Menu("Mit Haftbefehl einreisen", "Reise mit einem Haftbefehl ein");
                    warrantMenu.addMenuItem(new StaticMenuItem("Informationen", "MIT SUPPORT ABSREPCHEN! Schreibe dir alle Details vor und kopiere sie einfach nur noch in das Feld! Mit dieser Option reist du mit einem ausstehenden Haftbefehl ein. Es sind Straftaten in anderen US Bundesstaaten möglich. Der Eintrag in der Polizeiakte wird eine Notiz beinhalten, dass Verhandlung und Verwahrung in San Andreas stattfinden soll. Du kannst Ausstellungsgrund, einen ausstellenden Richter (keine Spieler Charaktere!), die schwere der Straftat (empfohlene Haftzeit in Stunden) und andere Details spezifizieren. Du wirst dadurch ggf. direkt am Flughafen festgenommen, kannst dich aber vielleicht auch rausreden. Als Gegenleistung erhälst du einen Startbonus beim Crime-Netzwerk.", "Siehe Beschreibung"));
                    warrantMenu.addMenuItem(new InputMenuItem("Haftgrund/-gründe", "Der Grund warum der Haftbefehl ausgestellt wurde.", "max 100 Zeichen", ""));
                    warrantMenu.addMenuItem(new InputMenuItem("Haftdetails", "Der Grund warum der Haftbefehl ausgestellt wurde.", "max. 200 Zeichen", ""));
                    warrantMenu.addMenuItem(new InputMenuItem("Richtername", "Der Name des Haftbefehl ausstellenden Richters", "max. 45 Zeichen", ""));
                    warrantMenu.addMenuItem(new InputMenuItem("Haftbefehldatum", "Das Datum der Ausstellung", "max. 45 Zeichen", ""));
                    warrantMenu.addMenuItem(new InputMenuItem("Empfohlene Haftstrafe", "Die empfohlene Länge der Haftzeit", "in Stunden", ""));
                    warrantMenu.addMenuItem(new MenuStatsMenuItem($"Bestätigen (Kosten: {TERMINAL_WARRANT_COST})", "Bestätige die oben stehenden Informationen.", "TERMINAL_BUY_OUTSTANDING_WARRANT", MenuItemStyle.green).needsConfirmation("So bestätigen?", "Wirklich so bestätigen?"));

                    menu.addMenuItem(new MenuMenuItem(warrantMenu.Name, warrantMenu));
                } else {
                    menu.addMenuItem(new StaticMenuItem("Haftbefehl nicht ausstellbar!", "Du kannst dir keinen Haftbefehl ausstellen lassen, weil dieser zu teuer ist!", $"zu teuer ({TERMINAL_WARRANT_COST}M)", MenuItemStyle.yellow));
                }
            } else {
                menu.addMenuItem(new ClickMenuItem("Haftbefehl anzeigen lassen", "Lasse dir deinen aktuellen Eintrag in die Polizeiakte anzeigen", "", "TERMINAL_SHOW_OUTSTANDING_WARRANT"));
            }

            if(!player.hasData("TERMINAL_ILLEGAL_ITEMS")) {
                if(tokens > TERMINAL_ILLEGAL_ITEM_COST) {
                    menu.addMenuItem(new ClickMenuItem("Mit Schmuggelware starten", "Starte mit Schmuggelware im Gepäck. Versuche sie rauszuschmuggeln für Profit, oder wandere direkt ins Gefängnis. Welche es sein wird findest du erst im Flughafen heraus.", $"{TERMINAL_ILLEGAL_ITEM_COST} Marken", "TERMINAL_BUY_ILLEGAL_ITEM"));
                } else {
                    menu.addMenuItem(new StaticMenuItem("Schmuggelware nicht ausstellbar!", "Du kannst nicht mit Schmuggelware starten, weil dies zu teuer ist!", $"zu teuer ({TERMINAL_ILLEGAL_ITEM_COST}M)", MenuItemStyle.yellow));
                }
            } else {
                menu.addMenuItem(new StaticMenuItem("Schmuggelware bereits erworben", "Du hast bereits Schmuggelware erworben!", ""));
            }

            return menu;
        }

        public override Menu getAlreadyBoughtListing(IPlayer player) {
            var menu = new Menu("Bereits Gekauftes zurückgeben", "Was möchtest du zurückgeben?");
            if(player.hasData("TERMINAL_WARRANT")) {
                menu.addMenuItem(new ClickMenuItem("Haftbefehl zurückgeben", "Gib den Haftbefehl der auf dich ausgestellt ist zurück. Erhalte die Marken zurück", $"{TERMINAL_WARRANT_COST} Marken", "TERMINAL_RETURN_OUTSTANDING_WARRANT", MenuItemStyle.red));
            }

            if(player.hasData("TERMINAL_ILLEGAL_ITEMS")) {
                menu.addMenuItem(new ClickMenuItem("Schmuggelware zurückgeben", "Gib die Schmuggelware zurück. Erhalte die Marken zurück", $"{TERMINAL_ILLEGAL_ITEM_COST} Marken", "TERMINAL_RETURN_ILLEGAL_ITEMS", MenuItemStyle.red));
            }

            return menu;
        }

        public override bool hasPlayerBoughtSomething(IPlayer player) {
            return player.hasData("TERMINAL_WARRANT") || player.hasData("TERMINAL_ILLEGAL_ITEMS");
        }

        public override void onPlayerLand(IPlayer player, ref executive_person_file file) {
            if(player.hasData("TERMINAL_WARRANT")) {
                var text = ((string)player.getData("TERMINAL_WARRANT")).FromJson<string>();

                file.info += text;

                player.resetPermantData("TERMINAL_WARRANT");
            }

            if(player.hasData("TERMINAL_ILLEGAL_ITEMS")) {
                var rand = new Random();
                var cfgs = InventoryController.getConfigItemsForType<FenceJewelry>().Concat(InventoryController.getConfigItemsForType<DrugItem>()).ToList();
                var randAmount = rand.Next(5, 6);
                
                for(var i = 0; i < randAmount; i++) {
                    var item = cfgs[rand.Next(0, cfgs.Count)];
                    var items = InventoryController.createItems(item, rand.Next(6, 10));

                    foreach(var it in items) {
                        player.getInventory().addItem(it);
                    }
                }

                player.resetPermantData("TERMINAL_ILLEGAL_ITEMS");
            }
        }
    }

    public class TerminalIllegalShopController : ChoiceVScript {
        public TerminalIllegalShopController() {
            TerminalShopController.addTerminalShop(new IllegalTerminalShop());

            EventController.addMenuEvent("TERMINAL_BUY_OUTSTANDING_WARRANT", onTerminalBuyOutstandingWarrant);
            EventController.addMenuEvent("TERMINAL_SHOW_OUTSTANDING_WARRANT", onTerminalShowOutstandingWarrant);
            EventController.addMenuEvent("TERMINAL_RETURN_OUTSTANDING_WARRANT", onTerminalReturnOutstandingWarrant);

            EventController.addMenuEvent("TERMINAL_BUY_ILLEGAL_ITEM", onTerminalBuyIllegalItem);
            EventController.addMenuEvent("TERMINAL_RETURN_ILLEGAL_ITEMS", onTerminalReturnIllegalItems);
        }

        private bool onTerminalBuyOutstandingWarrant(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(TerminalShopController.addOrRemovePlayerTokens(player, -IllegalTerminalShop.TERMINAL_WARRANT_COST)) {
                var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

                var reasonEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
                if(checkIfStringToLong(player, "Haftgrund/-gründe", reasonEvt.input, 100)) {
                    TerminalShopController.addOrRemovePlayerTokens(player, IllegalTerminalShop.TERMINAL_WARRANT_COST);
                    return false;
                }

                var detailsEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
                if(checkIfStringToLong(player, "Haftdetails", detailsEvt.input, 200))  {
                    TerminalShopController.addOrRemovePlayerTokens(player, IllegalTerminalShop.TERMINAL_WARRANT_COST);
                    return false;
                }

                var judgeNameEvt = evt.elements[3].FromJson<InputMenuItemEvent>();
                if(checkIfStringToLong(player, "Richtername", judgeNameEvt.input, 45)) {
                    TerminalShopController.addOrRemovePlayerTokens(player, IllegalTerminalShop.TERMINAL_WARRANT_COST);
                    return false;
                }

                var dateEvt = evt.elements[4].FromJson<InputMenuItemEvent>();
                if(checkIfStringToLong(player, "Datum", dateEvt.input, 45)) {
                    TerminalShopController.addOrRemovePlayerTokens(player, IllegalTerminalShop.TERMINAL_WARRANT_COST);
                    return false;
                }

                var sentenceEvt = evt.elements[5].FromJson<InputMenuItemEvent>();
                if(checkIfStringToLong(player, "Haftzeit", sentenceEvt.input, 45)) {
                    TerminalShopController.addOrRemovePlayerTokens(player, IllegalTerminalShop.TERMINAL_WARRANT_COST);
                    return false;
                }

                var text =
                    $"Der/die ehrenwerte Richter/in {judgeNameEvt.input} stellt hiermit einen Haftbefehl gegen {player.getCharacterShortName()} zum Grund {reasonEvt.input} aus. \n" +
                    $"Für die Tat mit folgenden Details:\n{detailsEvt.input}\nist am {dateEvt.input} ein Haftbefehl ausgestellt worden.\n" +
                    $"Der/die ehrenwerte Richter/in {judgeNameEvt.input} empfiehlt eine Verhandlung und ggf. Verwahrung im Bundesstaat des Ausführens des Haftbefehls und eine Haftzeit von {sentenceEvt.input}.";


                Note.showNoteEvent(player, "Polizeiakten-Eintrag:", text, true);
                player.sendNotification(Constants.NotifactionTypes.Info, "Dir wird der Text in deiner Polizeiakte angezeigt. Überprüfe ihn auf Fehler", "Text angezeigt", Constants.NotifactionImages.Plane);

                //toJson to keep the formating!
                player.setPermanentData("TERMINAL_WARRANT", text.ToJson());
            }

            return true;
        }

        private static bool checkIfStringToLong(IPlayer player, string name, string str, int maxLength) {
            if(str.Length > maxLength) {
                player.sendBlockNotification($"Die Eingabe: {name} war zu lang. Sie darf max {maxLength} Zeichen haben!", "Zu viele Zeichen!", Constants.NotifactionImages.Plane);
                return true;
            } else {
                return false;
            }
        }

        private bool onTerminalShowOutstandingWarrant(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var text = ((string)player.getData("TERMINAL_WARRANT")).FromJson<string>();

            Note.showNoteEvent(player, "Polizeiakten-Eintrag:", text, true);

            return true;
        }

        private bool onTerminalBuyIllegalItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(TerminalShopController.addOrRemovePlayerTokens(player, -IllegalTerminalShop.TERMINAL_ILLEGAL_ITEM_COST)) {
                player.setPermanentData("TERMINAL_ILLEGAL_ITEMS", "1");
                player.sendNotification(Constants.NotifactionTypes.Info, "Du hast Schmuggelware erworben. Du weißt aber nicht welche", "Schmuggelware erworben", Constants.NotifactionImages.Plane);
            }

            return true;
        }

        private bool onTerminalReturnOutstandingWarrant(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(TerminalShopController.addOrRemovePlayerTokens(player, IllegalTerminalShop.TERMINAL_WARRANT_COST)) {
                player.resetPermantData("TERMINAL_WARRANT");
                player.sendNotification(Constants.NotifactionTypes.Warning, "Du hast den ausgestellten Haftbefehl zurückgegeben. Du hast die Marken wieder erhalten", "Haftbefehl zurückgegeben", Constants.NotifactionImages.Plane);
            }

            return true;
        }

        private bool onTerminalReturnIllegalItems(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(TerminalShopController.addOrRemovePlayerTokens(player, IllegalTerminalShop.TERMINAL_ILLEGAL_ITEM_COST)) {
                player.resetPermantData("TERMINAL_ILLEGAL_ITEMS");
                player.sendNotification(Constants.NotifactionTypes.Warning, "Du hast die Schmugglerware zurückgegeben. Du hast die Marken wieder erhalten", "Schmugglerware zurückgegeben", Constants.NotifactionImages.Plane);
            }

            return true;
        }
    }
}
