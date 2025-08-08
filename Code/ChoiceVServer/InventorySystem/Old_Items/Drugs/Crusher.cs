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
//    public class CrusherController : ChoiceVScript {
//        public List<Predicate<Item>> CrushPredicates = new List<Predicate<Item>>{
//            {i => i is Weed}, //Predicates
//            { i => i is DriedMescalineCactus },
//            };
//        public CrusherController() {
//            EventController.addMenuEvent("CRUSHER_CRUSH", onCrushChoose);
//            EventController.addMenuEvent("START_CRUSH", onCrushStart);
//        }

//        private bool onCrushStart(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var item = (Item)data["Item"];
//            var crusher = (Crusher)data["Crusher"];
//            InputMenuItem.InputMenuItemEvent inputItem = menuItemCefEvent as InputMenuItem.InputMenuItemEvent;
//            var inputCheck = int.TryParse(inputItem.input, out var amount);
//            if(inputCheck) {
//                if(amount < 1) {
//                    player.sendNotification(NotifactionTypes.Warning, "Der Inpt muss mindestens 1 betragen!", "falscher Input");
//                    return true;
//                }
//                if(amount > 5) {
//                    player.sendNotification(NotifactionTypes.Info, "Mehr als 5 Gramm passt nicht in den Crusher", "Crusher voll");
//                    return true;
//                }
//                if(item.StackAmount >= amount) {
//                    var anim = AnimationController.getItemAnimationByName("PICKAXE"); //REPLACE WITH ANIM
//                    AnimationController.animationTask(player, anim, () => {
//                        if(item is Weed) {
//                            var type = (string)item.Data["WeedType"];
//                            var crushedConfigItemId = WeedController.getWeedItemId(type, WeedItemTypes.CrushedWeed);
//                            var configItem = InventoryController.AllConfigItems[crushedConfigItemId];
//                            var crushedWeed = new CrushedWeed(configItem, item.Quality, amount, type);
//                            player.getInventory().removeItem(item, amount);
//                            player.getInventory().addItem(crushedWeed);
//                            crusher.SkuffCounter += 1;
//                        }
//                        if(item is DriedMescalineCactus) {
//                            var powderConfigItem = InventoryController.AllConfigItems[MESCALINE_POWDER_ID];
//                            var mescalinePowder = new MescalinePowder(powderConfigItem, amount);
//                            player.getInventory().removeItem(item, amount);
//                            player.getInventory().addItem(mescalinePowder);
//                            InventoryController.showInventory(player, player.getInventory());
//                        }
//                    });
//                } else {
//                    player.sendNotification(NotifactionTypes.Warning, "So viel hast du nicht!", "zu wenig");
//                    return true;
//                }
//            } else {
//                player.sendNotification(NotifactionTypes.Warning, "Der Input muss eine Zahl sein!", "Keine Zahl");
//            }

//            return true;
//        }

//        private bool onCrushChoose(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var crusher = (Crusher)data["Crusher"];
//            var menu = new Menu("Item Crushen", "Crushed ein Item");
//            foreach(var item in player.getInventory().getAllItems()) {
//                var work = false;
//                foreach(var predicate in CrushPredicates) {
//                    if(predicate(item)) {
//                        work = true;
//                        break;
//                    }
//                }

//                if(work) {
//                    if(item is Weed) {
//                        var dictionary = new Dictionary<string, dynamic> {
//                            {"Item", item },
//                            {"Crusher", crusher },
//                        };
//                        var type = item.Data["WeedType"];
//                        menu.addMenuItem(new InputMenuItem(type, $"Crushed das {item.Name}", "", "START_CRUSH").withData(dictionary));
//                    }
//                    if(item is DriedMescalineCactus) {
//                        var dictionary = new Dictionary<string, dynamic> {
//                            {"Item", item },
//                            {"Crusher", crusher },
//                    };
//                        menu.addMenuItem(new InputMenuItem("Meskalin-Kaktus", $"Crushed das {item.Name}", "", "START_CRUSH").withData(dictionary));
//                    }
//                }
//            }
//            player.showMenu(menu);
//            return true;
//        }
//    }

//    public class Crusher : Item {
//        public int SkuffCounter { get => (int)Data["Counter"]; set { Data["Counter"] = value; } }
//        public Crusher(items item) : base(item) { }

//        public Crusher(configitems configItem) : base(configItem) {
//            Description = "Zum zerkleinern von Kräutern";
//            SkuffCounter = 0;
//        }

//        public override void use(IPlayer player) {
//            var dictionary = new Dictionary<string, dynamic> {
//                    {"Crusher", this },
//                };
//            var menu = new Menu("Crusher", "Wähle deine Aktion");
//            menu.addMenuItem(new ClickMenuItem("Item Crushen", "Zerkleinert das Item", "", "CRUSHER_CRUSH").withData(dictionary));
//            //menu.addMenuItem(new ClickMenuItem("Skuff Fach öffnen", "Öffnet das Skuff Fach", "", "CRUSHER_SKUFF").withData(dictionary)); //Maybe on a Update
//            player.showMenu(menu);
//        }
//    }
//}
