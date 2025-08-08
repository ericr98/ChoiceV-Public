//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.Controller;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Model.Menu;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using static ChoiceVServer.Base.Constants;

//namespace ChoiceVServer.InventorySystem {
//    public class PlasticBagContoller : ChoiceVScript {

//        public PlasticBagContoller() {
//            EventController.addMenuEvent("ADD_ITEM", onItemAdd);
//            EventController.addMenuEvent("REMOVE_ITEM", onItemRemove);
//        }

//        private bool onItemAdd(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var item = (Item)data["Item"];
//            var plasticBag = (PlasticBag)data["PlasticBag"];
//            InputMenuItem.InputMenuItemEvent inputItem = menuItemCefEvent as InputMenuItem.InputMenuItemEvent;
//            var amount = 0;
//            var parseCheck = int.TryParse(inputItem.input, out amount);
//            var weight = item.Weight;
//            var newWeight = Convert.ToDouble(weight) * Convert.ToDouble(amount);
//            if(!parseCheck) {
//                player.sendNotification(NotifactionTypes.Warning, "Der Input muss eine Zahl sein", "keine Zahl");
//                return true;
//            }
//            if(amount < 1) {
//                player.sendNotification(NotifactionTypes.Warning, "Der Inpt muss mindestens 1 betragen!", "falscher Input");
//                return true;
//            }
//            if(newWeight > plasticBag.bagFreeSpace) {
//                player.sendNotification(NotifactionTypes.Warning, "Das Item ist zu groß für diesen Beutel", "Item zu groß");
//                return true;
//            }
//            if(item.StackAmount < amount) {
//                player.sendNotification(NotifactionTypes.Warning, "So viele Items hast du nicht!", "Zu wenig Items");
//                return true;
//            }
//            player.getInventory().moveItem(plasticBag.PlasticBagContent, item, amount);
//            refreshItem(plasticBag);
//            plasticBag.bagFreeSpace -= newWeight;
//            InventoryController.showInventory(player, player.getInventory());
//            return true;
//        }

//        private bool onItemRemove(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var item = (Item)data["Item"];
//            var plasticBag = (PlasticBag)data["PlasticBag"];
//            InputMenuItem.InputMenuItemEvent inputItem = menuItemCefEvent as InputMenuItem.InputMenuItemEvent;
//            var amount = 0;
//            var parseCheck = int.TryParse(inputItem.input, out amount);
//            if(!parseCheck) {
//                player.sendNotification(NotifactionTypes.Warning, "Der Input muss eine Zahl sein", "keine Zahl");
//                return true;
//            }
//            if(item.StackAmount < amount) {
//                player.sendNotification(NotifactionTypes.Warning, "So viele Items sind nicht in dem Beutel!", "Zu wenig Items");
//                return true;
//            }
//            var moveCheck = plasticBag.PlasticBagContent.moveItem(player.getInventory(), item, amount);
//            refreshItem(plasticBag);
//            if(!moveCheck) {
//                player.sendNotification(NotifactionTypes.Warning, "Dein Inventar ist voll", "");
//            } else {
//                plasticBag.bagFreeSpace += Convert.ToDouble(item.Weight) * Convert.ToDouble(amount);
//            }
//            InventoryController.showInventory(player, player.getInventory());
//            return true;
//        }

//        public static void refreshItem(PlasticBag plasticBag) {
//            var inventory = plasticBag.PlasticBagContent;
//            var item = inventory.getAllItems().FirstOrDefault(x => true);
//            if(item != null) {
//                var name = item.Name;
//                plasticBag.Description = $"Inhalt :{item.StackAmount} Gramm {name}";
//                plasticBag.PlasticBagContentItem = item;
//            } else {
//                plasticBag.Description = "Inhalt: Leer";
//                plasticBag.PlasticBagContentItem = null;
//            }
//        }
//    }

//    public class PlasticBag : Item {
//        public double bagFreeSpace { get => (double)Data["bagSize"]; set { Data["bagSize"] = value; } }
//        public Inventory PlasticBagContent { get => InventoryController.loadInventory((int)Data["InventoryId"]); set => Data["InventoryId"] = value.Id; }
//        public Item PlasticBagContentItem { get => (Item)Data["Item"]; set { Data["Item"] = value; } }
//        public List<Predicate<Item>> BagPredicates = new List<Predicate<Item>>{
//                {i => i is Weed }, //Predicates
//                {i => i is Cocaine },
//                {i => i is MescalinePowder },
//        };
//        public PlasticBag(items item) : base(item) {
//            PlasticBagContoller.refreshItem(this);
//        }

//        public PlasticBag(configitems configItem) : base(configItem) {
//            Data = new ExtendedDictionary<string, dynamic>(new Dictionary<string, dynamic>());
//            Description = "Inhalt: Leer";
//            bagFreeSpace = double.Parse(configItem.additionalInfo);
//            PlasticBagContent = InventoryController.createInventory(0, 999, InventoryTypes.PlasticBag);
//        }

//        public override void use(IPlayer player) {
//            var menu = new Menu("Plastikbeutel", "Was möchtest du machen?");
//            var outputMenu = new Menu("Plastikbeutel", "Lege Items aus den Beutel");
//            var inputMenu = new Menu("Plastikbeutel", "Lege Items in den Beutel");
//            foreach (var item in player.getInventory().getAllItems()) {
//                var test = CompanyController.getCompanies(player);
//                var work = false;
//                foreach (var predicate in BagPredicates) {
//                    if (predicate(item)) {
//                        work = true;
//                        break;
//                    }
//                }

//                if (work) {
//                    var dictionary = new Dictionary<string, dynamic> {
//                            {"Item", item },
//                            {"PlasticBag", this }
//                };
//                    inputMenu.addMenuItem(new InputMenuItem($"{item.Name} reinlegen", $"Legt das {item.Name} in den Beutel", "", "ADD_ITEM").withData(dictionary));

//                }
//            }
//            foreach (var item in PlasticBagContent.getAllItems()) {
//                var dictionary = new Dictionary<string, dynamic> {
//                            {"Item", item },
//                            {"PlasticBag", this }
//                };
//                outputMenu.addMenuItem(new InputMenuItem($"{item.Name} rausnehmen", $"Nimmt das {item.Name} aus dem Beutel", "", "REMOVE_ITEM").withData(dictionary));

//            }

//            menu.addMenuItem(new MenuMenuItem("Item in den Beutel legen", inputMenu));
//            menu.addMenuItem(new MenuMenuItem("Item aus dem Beutel nehmen", outputMenu));
//            player.showMenu(menu);
//        }
//    }
//}
