//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.InventorySystem;
//using ChoiceVServer.Model.Menu;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using static ChoiceVServer.Base.Constants;

//namespace ChoiceVServer.Controller {
//    class MescalineController : ChoiceVScript {
//        public MescalineController() {
//            EventController.addMenuEvent("MESCALINE_TUNE_ITEM", onTuneItem);
//        }

//        private bool onTuneItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var powder = (Item)null;
//            if (!(data.ContainsKey("MescItem"))){
//                player.sendNotification(Constants.NotifactionTypes.Warning, "Du hast kein Meskalin!", "Kein Meskalin!");
//                return true;
//            } else {
//                powder = data["MescItem"];
//            }
//            if (data.ContainsKey("JointItem")) {
//                var anim = AnimationController.getItemAnimationByName("PICKAXE"); //TODO: REPLACE WITH ANIM
//                AnimationController.animationTask(player, anim, () => {
//                    var joint = (Item)data["JointItem"];
//                    var configItem = InventoryController.AllConfigItems[MESCALINE_JOINT_ID];
//                    var item = new TunedMescalineItem(configItem, joint.Name, "Joint");
//                    player.getInventory().removeItem(joint, 1);
//                    player.getInventory().addItem(item);
//                });
//            }
//            if (data.ContainsKey("BluntItem")) {
//                var anim = AnimationController.getItemAnimationByName("PICKAXE"); //TODO: REPLACE WITH ANIM
//                AnimationController.animationTask(player, anim, () => {
//                    var blunt = (Item)data["BluntItem"];
//                    var configItem = InventoryController.AllConfigItems[MESCALINE_BLUNT_ID];
//                    var item = new TunedMescalineItem(configItem, blunt.Name, "Blunt");
//                    player.getInventory().removeItem(blunt, 1);
//                    player.getInventory().addItem(item);
//                });
//            }
//            if (data.ContainsKey("CigaretteItem")) {
//                var anim = AnimationController.getItemAnimationByName("PICKAXE"); //TODO: REPLACE WITH ANIM
//                AnimationController.animationTask(player, anim, () => {
//                    var cigarette = (Item)data["CigaretteItem"];
//                    var configItem = InventoryController.AllConfigItems[MESCALINE_CIGARETTE_ID];
//                    var item = new TunedMescalineItem(configItem, cigarette.Name, "Cigarette");
//                    player.getInventory().removeItem(cigarette, 1);
//                    player.getInventory().addItem(item);
//                });
//            }

//            return true;
//        }

//        public static void showMesaclineMenu(IPlayer player) {
//            var menu = new Menu("Meskalin Pulver", "Wie möchtest du es konsumieren?");
//            var playerInvList = player.getInventory().getAllItems();
//            var mescalinePowder = player.getInventory().getItem(x => x is MescalinePowder);
//            var counter = 0;
//            var data = new Dictionary<string, dynamic>();
//            data.Add("MescItem", mescalinePowder);
//            foreach (var item in playerInvList) {
//                if (item is Joint) {
//                    menu.addMenuItem(new ClickMenuItem($"{item.Name} verfeinern", $"Verfeinert einen {item.Name} Joint mit Meskalin", "", "MESCALINE_TUNE_ITEM").withData(new Dictionary<string, dynamic> { { "MescItem", mescalinePowder }, { "JointItem", item } }));
//                    counter++;
//                }
//                if (item is Blunt) {
//                    menu.addMenuItem(new ClickMenuItem($"{item.Name} verfeinern", $"Verfeinert einen {item.Name} Blunt mit Meskalin", "", "MESCALINE_TUNE_ITEM").withData(new Dictionary<string, dynamic> { { "MescItem", mescalinePowder }, { "BluntItem", item } }));
//                    counter++;
//                }
//                if (item is Cigarette) {
//                    menu.addMenuItem(new ClickMenuItem($"{item.Name} verfeinern", $"Verfeinert eine {item.Name}  mit Meskalin", "", "MESCALINE_TUNE_ITEM").withData(new Dictionary<string, dynamic> { { "MescItem", mescalinePowder }, { "CigaretteItem", item } }));
//                    counter++;
//                }
//            }
//            if (counter == 0) {
//                menu.addMenuItem(new StaticMenuItem("Nichts zum verfeinern", "Du hast nichts zum verfeinern!", "", MenuItemStyle.red));
//            }
//            player.showMenu(menu);
//        }
//    }
//}
