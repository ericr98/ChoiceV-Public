using AltV.Net.Elements.Entities;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller {
    public class CompanyNewspaperFunctionality : CompanyFunctionality {
        public InventorySpot StorageSpot;
        
        public CompanyNewspaperFunctionality() : base() { }
        
        public CompanyNewspaperFunctionality(Company company) : base(company) { }
        
        public override string getIdentifier() {
            return "NEWSPAPER";
        }

        public override void onLoad() {
            Company.registerCompanyAdminElement(
                "NEWSPAPER_ADMIN",
                getNewspaperAdminMenu,
                onNewspaperAdminCallback
            );
            //Company.registerCompanyInteractElement(
            //    "NEWSPAPER_SELL",
            //    getSellNewspaperMenu,
            //    onSellNewspaperCallback
            //);
            
            Company.registerCompanySelfElement(
                "NEWSPAPER_CREATE",
                getCreateNewspaperMenu,
                onCreateNewspaperCallback,
                "CREATE_NEWSPAPER"
            );
            
            if(Company.hasSetting("NEWSPAPER_STORAGE")) {
                StorageSpot = InventorySpot.getById(int.Parse(Company.getSetting("NEWSPAPER_STORAGE")));
            }
        }

        private MenuElement getNewspaperAdminMenu(IPlayer player) {
            var menu = new Menu("Zeitungsfunktionalität", "Verwalte die Funktionlitäten der Firma");
            
            menu.addMenuItem(new ClickMenuItem("Zeitungslager erstellen", "Erstelle ein Lager für Zeitungen", "", "NEWSPAPER_STORAGE_CREATE"));

            return menu;
        }

        private void onNewspaperAdminCallback(Company company, IPlayer player, string subevent, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            if(subevent == "NEWSPAPER_STORAGE_CREATE") {
                player.sendNotification(Constants.NotifactionTypes.Info, "Erstelle nun den Collision Shape für das Lager", "Collision Shape erstellen");
                CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (position, width, height, rotation) => {
                    var inventorySpot = InventorySpot.create(InventorySpotType.Company, $"Zeitungslager: {Company.ShortName}", position, width, height, rotation, 500, []);
                    
                    if(Company.hasSetting("NEWSPAPER_STORAGE")) {
                        var spotId = int.Parse(Company.getSetting("NEWSPAPER_STORAGE"));
                        InventorySpot.getById(spotId).remove();
                    }
                    
                    Company.setSetting("NEWSPAPER_STORAGE", inventorySpot.Id.ToString());
                    StorageSpot = inventorySpot;
                    
                    player.sendNotification(Constants.NotifactionTypes.Success, "Das Lager wurde erfolgreich erstellt", "Lager erstellt", Constants.NotifactionImages.Newspaper);
                });
            }
        }

        public override List<string> getSinglePermissionsGranted() {
            return ["CREATE_NEWSPAPER"];
        }

        private MenuElement getCreateNewspaperMenu(Company company, IPlayer player) {
            var menu = new Menu("Zeitungskonfiguration", "Was möchtest du tun?");

            var createMenu = new Menu("Zeitung erstellen", "Erstelle eine neue Zeitung");
            createMenu.addMenuItem(new InputMenuItem("Name", "Name der Zeitung", "Name der Zeitung", ""));
            createMenu.addMenuItem(new InputMenuItem("Preis", "Preis der Zeitung", "Preis der Zeitung", InputMenuItemTypes.number, ""));
            createMenu.addMenuItem(new FileMenuItem("Dokument", "Das PDF Dokument der Zeitung", "", ""));
            createMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle die Zeitung", "", "NEWSPAPER_CREATE", MenuItemStyle.green));
            menu.addMenuItem(new MenuMenuItem(createMenu.Name, createMenu));

            foreach(var newspaper in NewspaperController.getNewspapersForCompany(Company)) {
                menu.addMenuItem(new StaticMenuItem(newspaper.Name, $"Die Zeitung {newspaper.Name} wurde für den Preis ${newspaper.Price} insgesamt {newspaper.DistributedAmount} mal verkauft", newspaper.DistributedAmount.ToString()));
            }
            
            return menu;
        }

        private void onCreateNewspaperCallback(Company company, IPlayer player, string subevent, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            if(subevent == "NEWSPAPER_CREATE") {
                var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;
                var name = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>().input;
                var price = decimal.Parse(evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>().input);
                var file = evt.elements[2].FromJson<FileMenuItem.FileMenuItemEvent>();

                NoteController.createFlipbookFile($"COMPANY_{Company.Id}_NEWSPAPER_{name}", name, Company.Id.ToString(), name, file.fileData, ""); 
                NewspaperController.createNewspaper($"COMPANY_{Company.Id}_NEWSPAPER_{name}", Company, name, 0, price);
                
                player.sendNotification(Constants.NotifactionTypes.Success, $"Die Zeitung {name} wurde erfolgreich erstellt", "Zeitung erstellt", Constants.NotifactionImages.Newspaper);
            } 
        }

        private MenuElement getSellNewspaperMenu(IPlayer player, IPlayer target) {
            var newspapers = NewspaperController.getNewspapersForCompany(Company);
            var newspaperItems = player.getInventory().getItems<FlipBook>(i => newspapers.Any(n => n.Identifier == i.FlipBookIdentifier)).GroupBy(i => i.Name).ToList();

            if(newspaperItems.Count > 0) {
                var menu = new Menu("Zeitung verkaufen", "Welche Zeitung verkaufen?");
                foreach(var category in newspaperItems) {
                    var firstItem = category.First();
                    var newspaper = newspapers.Find(n => n.Identifier == firstItem.FlipBookIdentifier);
                   
                    menu.addMenuItem(new ClickMenuItem($"{firstItem.Name} verkaufen", $"Verkaufe eine {firstItem.Name} für ${newspaper.Price}", $"$ {newspaper.Price}", "NEWSPAPER_SELL")
                        .withData(new Dictionary<string, dynamic> { { "Newspaper", newspaper } })
                        .needsConfirmation("Zeitung verkaufen?", $"Wirklich eine {firstItem.Name} für ${newspaper.Price} verkaufen?"));
                }

                return menu;
            } else {
                return new StaticMenuItem("Keine Zeitungen vorhanden", "Du hast keine Zeitungen zum Verkaufen", "");
            }
        }

        private void onSellNewspaperCallback(Company company, IPlayer player, string subevent, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            if(subevent == "NEWSPAPER_SELL") {
                var newspaper = data["Newspaper"] as Newspaper;
                var flipbook = player.getInventory().getItems<FlipBook>(i => i.FlipBookIdentifier == newspaper.Identifier).First();
                
                //TODO Make Menu for the other Player to confirm
            }
        }

        public override void onRemove() {
            Company.unregisterCompanyElement("NEWSPAPER_SELL");
        }

        public override SelectionInfo getSelectionInfo() {
            return new SelectionInfo(getIdentifier(), "Zeitungsfunktionalität", "Ermöglicht das Erstellen und Verkaufen von Zeitungen");
        }
    }
}