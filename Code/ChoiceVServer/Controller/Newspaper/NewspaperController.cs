using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller {
    public class Newspaper {
        public string Identifier;
        public Company Owner;
        public string Name;
        public decimal Price;
        public int DistributedAmount;
        
        public Newspaper(string identifier, Company owner, string name, int distributedAmount, decimal price) {
            Identifier = identifier;
            Owner = owner;
            Name = name;
            DistributedAmount = distributedAmount;
            Price = price;
        }
    }
    
    public class NewspaperController : ChoiceVScript {
        private static List<Newspaper> AllNewspapers = new List<Newspaper>();

        public NewspaperController() {
            EventController.MainReadyDelegate += onMainReady; 
            NoteController.FlipbookDeletedDelegate += onFlipbookDeleted;
            
            InteractionController.addInteractableObjects([
                ChoiceVAPI.Hash("prop_news_disp_02a_s").ToString(),
                ChoiceVAPI.Hash("prop_news_disp_03a").ToString(),
                ChoiceVAPI.Hash("prop_news_disp_02d").ToString(),
                ChoiceVAPI.Hash("prop_news_disp_02b").ToString(),
                ChoiceVAPI.Hash("prop_news_disp_03c").ToString(),
                ChoiceVAPI.Hash("prop_news_disp_06a").ToString(),
                ChoiceVAPI.Hash("prop_news_disp_02c").ToString(),
                ChoiceVAPI.Hash("prop_news_disp_02a").ToString(),
                ChoiceVAPI.Hash("prop_news_disp_02e").ToString(),
                ChoiceVAPI.Hash("prop_news_disp_05a").ToString(),
                ChoiceVAPI.Hash("prop_news_disp_01a").ToString(),
            ], "NEWSPAPER_STAND");

            InteractionController.addObjectInteractionCallback("NEWSPAPER_STAND", "Zeitungspapierständer", onNewspaperStandInteraction);
            
            EventController.addMenuEvent("BUY_NEWSPAPER", onBuyNewspaper);
        }

        private void onFlipbookDeleted(string identifier) {
            if(AllNewspapers.Exists(n => n.Identifier == identifier)) {
                AllNewspapers.Remove(AllNewspapers.Find(n => n.Identifier == identifier));
                
                using(var db = new ChoiceVDb()) {
                    db.newspapers.Remove(db.newspapers.First(n => n.flipbookIdentifier == identifier));
                    db.SaveChanges();
                }
            }
        }

        private static void onMainReady() {
            using(var db = new ChoiceVDb()) {
                foreach(var dbNewspaper in db.newspapers) {
                    var company = CompanyController.getCompanyById(dbNewspaper.companyId);
                    AllNewspapers.Add(new Newspaper(dbNewspaper.flipbookIdentifier, company, company.Name, dbNewspaper.distributedAmount, dbNewspaper.price));
                }
            }
        }

        public static List<Newspaper> getNewspapersForCompany(Company company) {
            return AllNewspapers.FindAll(n => n.Owner == company);
        }

        public static Newspaper getNewspaperByName(string name) {
            return AllNewspapers.Find(n => n.Name.ToLower().Equals(name.ToLower()));
        }
        
        public static Newspaper createNewspaper(string identifier, Company company, string name, int distributedAmount, decimal price) {
            var newspaper = new Newspaper(identifier, company, name, distributedAmount, price);
            AllNewspapers.Add(newspaper);

            using(var db = new ChoiceVDb()) {
                db.newspapers.Add(new newspaper {
                    flipbookIdentifier = identifier,
                    companyId = company.Id,
                    price = price,
                    distributedAmount = distributedAmount,
                });

                db.SaveChanges();     
            }
            
            return newspaper;
        }

        private void onNewspaperStandInteraction(IPlayer player, string modelname, string info, Position objectposition, float objectheading, bool isbroken, ref Menu menu) {
            var newsMenu = new Menu("Zeitungsständer", "Wähle eine Zeitung aus");

            foreach(var newspaper in AllNewspapers) {
                var spot = newspaper.Owner.getFunctionality<CompanyNewspaperFunctionality>().StorageSpot;
                if(spot == null) {
                    continue;
                }
                
                if(spot.Inventory.hasItem<FlipBook>(i => i.FlipBookIdentifier == newspaper.Identifier)) {
                   newsMenu.addMenuItem(new ClickMenuItem(newspaper.Name, $"Kaufe die Zeitung für ${newspaper.Price}", $"$ {newspaper.Price}", "BUY_NEWSPAPER")
                        .withData(new Dictionary<string, dynamic>{ {"Newspaper", newspaper }})
                        .needsConfirmation("Zeitung kaufen?", $"Wirklich für ${newspaper.Price} kaufen?")); 
                }
            }
            
            if(newsMenu.getMenuItemCount() <= 0) {
                newsMenu.addMenuItem(new StaticMenuItem("Keine Zeitungen", "Aktuell sind keine Zeitungen verfügbar", "", MenuItemStyle.yellow));
                return;
            }

            menu.addMenuItem(new MenuMenuItem(newsMenu.Name, newsMenu));
        }

        private bool onBuyNewspaper(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var newspaper = data["Newspaper"] as Newspaper;
            
            if(player.removeCash(newspaper.Price)) {
                var spot = newspaper.Owner.getFunctionality<CompanyNewspaperFunctionality>().StorageSpot;
                if(spot == null) {
                    player.sendBlockNotification("Die Zeitung konnte nicht gefunden werden!", "Zeitung nicht gefunden", Constants.NotifactionImages.Shop);
                    return true;
                }
                
                var item = spot.Inventory.getItems<FlipBook>(i => i.FlipBookIdentifier == newspaper.Identifier).First();
                if(item.moveToInventory(player.getInventory())) {
                   player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast die Zeitung {newspaper.Name} gekauft!", "Zeitung gekauft", Constants.NotifactionImages.Shop);

                   var amount = Math.Max(new Random().Next(25, 36), spot.Inventory.getSimilarItemsAmount(item));
                   
                   spot.Inventory.removeSimelarItems(item, amount);
                   CompanySubventionController.triggerManualSubvention(player, newspaper.Owner, CompanySubventionController.SubventionType.NewspaperMachine, amount * newspaper.Price);
                } else {
                     player.sendBlockNotification("Du hast nicht genug Platz in deinem Inventar um die Zeitung zu kaufen!", "Nicht genug Platz", Constants.NotifactionImages.Shop);
                }
            } else {
                player.sendBlockNotification("Du hast nicht genug Bargeld um die Zeitung zu kaufen!", "Nicht genug Bargeld", Constants.NotifactionImages.Shop);
            }
            
            return true; 
        }
    }
}