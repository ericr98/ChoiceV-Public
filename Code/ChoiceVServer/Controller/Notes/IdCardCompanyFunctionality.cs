using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller {
        
    public class IdCardCompanyFunctionality : CompanyFunctionality {
        public record IdCardData(string Name, string Type, string Icon); 
        
        private List<IdCardData> IdCards = [];
        
        public IdCardCompanyFunctionality() : base() { }
        
        public IdCardCompanyFunctionality(Company company) : base(company) { }
        
        public override string getIdentifier() {
            return "ID_CARD_COMPANY_FUNCTIONALITY";
        }

        public override void onLoad() {
            if(Company.hasSetting("ID_CARDS")) {
                IdCards = Company.getSetting("ID_CARDS").FromJson<List<IdCardData>>();
            }
            
            Company.registerCompanyAdminElement(
                "ID_CARD",
                idCardGenerator,
                onIdCard
            );
            
            Company.registerCompanyInteractElement(
                "ID_CARD_INTERACT",
                idCardInteractGenerator,
                onIdCardInteract,
                "ID_CARD_CREATION"
            );
        }

        private MenuElement idCardInteractGenerator(IPlayer player, IPlayer target) {
            if(!Company.hasEmployee(target.getCharacterId())) return null;
            
            var menu = new Menu("Ausweise", "Was möchtest du tun?");
            
            foreach(var idCard in IdCards) {
                menu.addMenuItem(new ClickMenuItem($"{idCard.Name} herstellen", "Erstelle den Ausweis", "", "CREATE_ID_CARD", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> {{"Target", target}, {"Element", idCard}})
                    .needsConfirmation("Ausweis erstellen?", "Möchtest du den Ausweis wirklich erstellen?"));
            }
            
            return menu;
        }

        private void onIdCardInteract(Company company, IPlayer player, string subevent, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var element = (IdCardData)data["Element"]; 
            var target = (IPlayer)data["Target"];
            
            if(subevent == "CREATE_ID_CARD") {
                var cfg = InventoryController.getConfigItem(i => i.additionalInfo == element.Type);
                
                var idCard = new CompanyIdCard(cfg, element.Icon, target, company);

                if(player.getInventory().addItem(idCard)) {
                    player.sendNotification(Constants.NotifactionTypes.Success, "Der Ausweis wurde erfolgreich erstellt", "Ausweis erstellt");
                    
                    Logger.logDebug(LogCategory.Player, LogActionType.Created, player, $"ID Card {element.Name} created for {target.getCharacterId()}");
                } else {
                    player.sendBlockNotification("Kein Platz im Inventar für den Ausweis!", "Ausweis nicht erstellbar");
                }
            } 
        }

        private MenuElement idCardGenerator(IPlayer player) {
            var menu = new Menu("Ausweise", "Was möchtest du aktivieren?");

            var addOption = new Menu("Hinzufügen", "Gib die Optionen ein");
            addOption.addMenuItem(new InputMenuItem("Austellnamen", "Gib den Namen des Ausweises ein", "", ""));
            addOption.addMenuItem(new ListMenuItem("Typ", "Wähle den Typ des Ausweises", [ "NORMAL", "DEPARTMENT", "BADGE" ], ""));
            addOption.addMenuItem(new InputMenuItem("Icon", "Gib den Icon Namen ein", "Icon Name", ""));
            addOption.addMenuItem(new MenuStatsMenuItem("Hinzufügen", "Füge den Ausweis hinzu", "ADD_ID_CARD", MenuItemStyle.green));
            menu.addMenuItem(new MenuMenuItem("Hinzufügen", addOption, MenuItemStyle.green));

            foreach(var idCard in IdCards) {
                var showMenu = new Menu(idCard.Name, "Was möchtest du tun?");
                showMenu.addMenuItem(new StaticMenuItem("Typ", idCard.Type, idCard.Type));
                showMenu.addMenuItem(new StaticMenuItem("Icon", idCard.Icon, idCard.Icon));
                showMenu.addMenuItem(new MenuStatsMenuItem("Löschen", "Lösche den Ausweis", "DELETE_ID_CARD", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> {{"Name", idCard.Name}})
                    .needsConfirmation("Ausweis löschen?", "Möchtest du den Ausweis wirklich löschen?"));
                
                menu.addMenuItem(new MenuMenuItem(showMenu.Name, showMenu));
            }
            
            return menu;
        }

        private void onIdCard(Company company, IPlayer player, string subevent, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            if(subevent == "ADD_ID_CARD") {
                var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;
                var name = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>().input;
                var type = evt.elements[1].FromJson<ListMenuItem.ListMenuItemEvent>().currentElement;
                var icon = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>().input;
                
                IdCards.Add(new IdCardData(name, type, icon));
                
                Company.setSetting("ID_CARDS", IdCards.ToJson());
                
                player.sendNotification(Constants.NotifactionTypes.Success, "Ausweis hinzugefügt", "Der Ausweis wurde erfolgreich hinzugefügt");
                
                Logger.logDebug(LogCategory.Support, LogActionType.Created, player, $"ID Card {name} added to {Company.Name}");
            } else if(subevent == "DELETE_ID_CARD") {
                var idCardName = data["Name"];
                
                IdCards = IdCards.Where(c => c.Name != idCardName).ToList();
                
                Company.setSetting("ID_CARDS", IdCards.ToJson());
                player.sendBlockNotification("Ausweis gelöscht", "Der Ausweis wurde erfolgreich gelöscht");
                
                Logger.logDebug(LogCategory.Support, LogActionType.Removed, player, $"ID Card {idCardName} removed from {Company.Name}");
            }
        }

        public override void onRemove() {
            Company.unregisterCompanyElement("ID_CARD");
            Company.unregisterCompanyElement("ID_CARD_INTERACT");
        }

        public override SelectionInfo getSelectionInfo() {
            return new SelectionInfo(getIdentifier(), "Ausweisfunktionalität", "Ermöglicht das Ausstellen von ID Cards für diese Firma");
        }
    }
}