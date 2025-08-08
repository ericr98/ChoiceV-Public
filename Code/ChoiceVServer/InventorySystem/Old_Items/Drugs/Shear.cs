//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.Controller;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Model.Menu;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using static ChoiceVServer.Base.Constants;

//namespace ChoiceVServer.InventorySystem {
//    public class ShearController : ChoiceVScript {
//        public ShearController() {
//            EventController.addMenuEvent("SHEAR_CUT_WEED", onCutWeed);
//        }

//        private bool onCutWeed(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var r = new Random();
//            InputMenuItem.InputMenuItemEvent inputItem = menuItemCefEvent as InputMenuItem.InputMenuItemEvent;
//            var inputCheck = int.TryParse(inputItem.input, out var inputAmount);
//            if(inputCheck) {
//                var item = (Item)data["Item"];
//                var type = (string)item.Data["WeedType"];
//                var driedCheck = item.Data["WeedDried"];
//                if(!driedCheck) {
//                    player.sendNotification(NotifactionTypes.Warning, "Feuchte Äste lassen sich schlecht schneiden!", "Feuchter Ast");
//                    return true;
//                }
//                if(inputAmount < 1) {
//                    player.sendNotification(NotifactionTypes.Warning, "Der Inpt muss mindestens 1 betragen!", "falscher Input");
//                    return true;
//                }
//                if(item.StackAmount >= inputAmount) {
//                    var weedConigItemId = WeedController.getWeedItemId(type, WeedItemTypes.Weed);
//                    var configItem = InventoryController.AllConfigItems[weedConigItemId];
//                    var amount = 0;
//                    var counter = 0;
//                    while(counter != inputAmount) {
//                        if(item.Quality >= 3) {
//                            amount = amount + r.Next(10, 18);
//                        } else {
//                            amount = amount + r.Next(10, 12);
//                        }
//                        counter++;
//                    }
//                    var weedBud = new Weed(configItem, item.Quality, amount, true, type);
//                    player.getInventory().removeItem(item, inputAmount);
//                    player.getInventory().addItem(weedBud);
//                    return true;
//                } else {
//                    player.sendNotification(NotifactionTypes.Warning, "So viel hast du nicht!", "zu wenig");
//                    return true;
//                }
//            } else {
//                player.sendNotification(NotifactionTypes.Warning, "Der Input muss eine Zahl sein!", "Keine Zahl");
//                return true;
//            }
//        }
//    }

//    public class Shear : Item {
//        public List<Predicate<Item>> MovePredicates = new List<Predicate<Item>>{
//                {i => i is DriedWeedStick }, //Predicates
//            };
//        public Shear(items item) : base(item) { }

//        public Shear(configitems configItem) : base(configItem) {
//            Description = "Eine Gartenschere";
//        }

//        public override void use(IPlayer player) {
//            var menu = new Menu("Gartenschere", "Beschneide die Items");
//            var counter = 0;
//            foreach (var item in player.getInventory().getAllItems()) {
//                var work = false;
//                foreach (var predicate in MovePredicates) {
//                    if (predicate(item)) {
//                        work = true;
//                        break;
//                    }
//                }

//                if (work) {
//                    var data = new Dictionary<string, dynamic> {
//                            {"Item", item }
//                        };
//                    if (item is Weed) {
//                        var type = item.Data["WeedType"];
//                        menu.addMenuItem(new InputMenuItem(type, "Beschneidet die angegebene Anzahl an Ästen", "", "SHEAR_CUT_WEED").withData(data));
//                    } else {
//                        menu.addMenuItem(new InputMenuItem(item.Name, "Beschneidet die angegebene Anzahl an Ästen", "", "SHEAR_CUT_WEED").withData(data));
//                    }
//                    counter++;
//                }
//            }
//            if (counter == 0) {
//                menu.addMenuItem(new StaticMenuItem("Du hast nichts zum beschneiden", "Keine Items zum beschneiden!", "", MenuItemStyle.red));
//            }
//            player.showMenu(menu);
//        }
//    }
//}