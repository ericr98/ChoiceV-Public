//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.InventorySystem;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Model.Menu;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using static ChoiceVServer.Base.Constants;

//namespace ChoiceVServer.Controller {
//    public class WeedController : ChoiceVScript {
//        public static List<weedType> weedTypeList = new List<weedType>();
//        public WeedController() {
//            EventController.addMenuEvent("CREATE_JOINT", onCreateJoint);
//            EventController.addMenuEvent("SMOKE_BONG", onSmokeBong);
//            FillList();
//        }

//        private bool onSmokeBong(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var type = (string)data["WeedType"];
//            if (player.hasData("LastSmoke")) {
//                var lastDate = DateTime.Parse(player.getData("LastSmoke"));
//                if (lastDate.AddSeconds(30) > DateTime.Now) {
//                    player.sendNotification(Constants.NotifactionTypes.Warning, "Du hast gerade erst etwas geraucht!", "Gerade geraucht!");
//                    return true;
//                }
//            }
//            if (player.hasData("SmokeBlock")) {
//                player.sendNotification(Constants.NotifactionTypes.Info, "Du bist viel zu breit um jetzt noch was zu rauchen!", "Zu breit", Constants.NotifactionImages.Marihuana);
//                return true;
//            }
//            player.setPermanentData("LastSmoke", DateTime.Now.ToString());
//            var id = WeedController.getWeedItemId(type, WeedItemTypes.CrushedWeed);
//            var item = player.getInventory().getItem(x => x.ConfigId == id);
//            if (item != null) {
//                var anim = AnimationController.getItemAnimationByName("SMOKE_JOINT"); //TODO: BONG ANIM
//                SoundController.playSoundAtCoords(player.Position, 10f, SoundController.Sounds.LighterSound);
//                AnimationController.animationTask(player, anim, () => {
//                    player.getInventory().removeItem(item, 1);
//                    showWeedEffect(player, type, false, false, true);
//                }, null, false);
//            } else {
//                player.sendNotification(NotifactionTypes.Warning, "Du hast nichts zum rauchen!", "Kein Marihuana");
//            }
//            return true;
//        }

//        private bool onCreateJoint(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var jointCheck = (bool)data["JointCheck"];
//            var anim = AnimationController.getAnimationByName(Constants.KNEEL_DOWN_ANIMATION);
//            AnimationController.animationTask(player, anim, () => {
//                var weed = (Item)data["Item"];
//                var pape = player.getInventory().getItem(x => x.ConfigId == LONGPAPE_ID);
//                var cigarette = player.getInventory().getItem(x => x.ConfigId == CIGARETTE_ID);
//                var weedType = (string)weed.Data["WeedType"];
//                player.getInventory().removeItem(pape, 1);
//                if (jointCheck) {
//                    player.getInventory().removeItem(weed, 1);
//                    player.getInventory().removeItem(cigarette, 1);
//                    var configItem = InventoryController.AllConfigItems[JOINT_ID];
//                    var item = new Joint(configItem, weedType, weed.Quality);
//                    player.getInventory().addItem(item);
//                } else {
//                    player.getInventory().removeItem(weed, 2);
//                    var configItem = InventoryController.AllConfigItems[BLUNT_ID];
//                    var item = new Blunt(configItem, weedType, weed.Quality);
//                    player.getInventory().addItem(item);
//                }
//            });
//            return true;
//        }

//        public static void showWeedMenu(IPlayer player, bool papeCheck = false, bool bongCheck = false) {
//            var menu = new Menu("Marihuana", "Was möchtest du machen?");
//            var pape = player.getInventory().getAllItems().FirstOrDefault(x => x.ConfigId == LONGPAPE_ID);
//            var bong = player.getInventory().getAllItems().FirstOrDefault(x => x.ConfigId == BONG_ID);
//            var weed = player.getInventory().getAllItems().FirstOrDefault(x => x is CrushedWeed);
//            var weedType = "";
//            var itemCounter = 0;
//            if (weed == null) {
//                player.sendNotification(NotifactionTypes.Warning, "Du hast nichts zum rauchen!", "");
//                return;
//            } else {
//                weedType = (string)weed.Data["WeedType"];
//            }
//            if (pape != null && !bongCheck) {
//                var cigaretteCheck = player.getInventory().getAllItems().FirstOrDefault(x => x.ConfigId == CIGARETTE_ID);
//                if (cigaretteCheck != null) {
//                    var jointMenu = new Menu("Joint drehen", "dreht aus 1 Gramm einen Joint");
//                    foreach (var joint in player.getInventory().getAllItems()) {
//                        if (joint is CrushedWeed) {
//                            var type = (string)joint.Data["WeedType"];
//                            jointMenu.addMenuItem(new ClickMenuItem($"{type} Joint drehen", $"Dreht einen {type} Joint", "", "CREATE_JOINT").withData(new Dictionary<string, dynamic> { { "Item", joint }, { "JointCheck", true } }));
//                            itemCounter++;
//                        }
//                    }
//                    if (itemCounter == 0) {
//                        jointMenu.addMenuItem(new StaticMenuItem($"Du hast nichts zum drehen", $"Du hast nichts zum drehen im Inventar!", ""));

//                    }
//                    menu.addMenuItem(new MenuMenuItem("Joint drehen", jointMenu));
//                }
//                if (weed.StackAmount >= 2) {
//                    var bluntMenu = new Menu("Blunt drehen", "dreht aus 2 Gramm einen Blunt");
//                    foreach (var joint in player.getInventory().getAllItems()) {
//                        if (joint is CrushedWeed) {
//                            var type = (string)joint.Data["WeedType"];
//                            bluntMenu.addMenuItem(new ClickMenuItem($"{type} Blunt drehen", $"Dreht einen {type} Blunt", "", "CREATE_JOINT").withData(new Dictionary<string, dynamic> { { "Item", joint }, { "JointCheck", false } }));
//                            itemCounter++;                        
//                        }
//                    }
//                    if (itemCounter == 0) {
//                        bluntMenu.addMenuItem(new StaticMenuItem($"Du hast nichts zum drehen", $"Du hast nichts zum drehen im Inventar!", ""));

//                    }
//                    menu.addMenuItem(new MenuMenuItem("Blunt drehen", bluntMenu));
//                }
//            }
//            if (bong != null && !papeCheck) {
//                var bongMenu = new Menu("Bong rauchen", "Was möchtest du rauchen?");
//                foreach (var bongWeed in player.getInventory().getAllItems()) {
//                    if (bongWeed is CrushedWeed) {
//                        var type = (string)bongWeed.Data["WeedType"];
//                        bongMenu.addMenuItem(new ClickMenuItem($"{type} rauchen", $"raucht {type} in der Bong", "", "SMOKE_BONG").withData(new Dictionary<string, dynamic> { { "WeedType", weedType } }));
//                        itemCounter++;
//                    }
//                }
//                if (itemCounter == 0) {
//                    bongMenu.addMenuItem(new StaticMenuItem($"Du hast nichts zum drehen", $"Du hast nichts zum drehen im Inventar!", ""));

//                }
//                menu.addMenuItem(new MenuMenuItem("Bong rauchen", bongMenu));
//            }
//            player.showMenu(menu);
//        }

//        public static void showWeedEffect(IPlayer player, string type, bool joint = false, bool blunt = false, bool bong = false) {
//            using (var db = new ChoiceVDb()) {
//                var effect = "drug_wobbly";
//                var strength = 0.1f;
//                double length = 5;
//                var invoke = (IInvoke)null;
//                var typeCheck = db.configweedeffects.FirstOrDefault(x => x.type == type);
//                if (typeCheck != null) {
//                    effect = typeCheck.effect;
//                    strength = (float)typeCheck.strength;
//                    length = typeCheck.length;
//                }
//                if (joint) {
//                    DrugController.setWeedLevel(player, WeedItems.Joint);
//                }
//                if (blunt) {
//                    strength += 0.1f;
//                    DrugController.setWeedLevel(player, WeedItems.Blunt);
//                }
//                if (bong) {
//                    strength += 0.2f;
//                    DrugController.setWeedLevel(player, WeedItems.Bong);
//                }
//                //TODO PERM DATA
//                if (player.hasData("WeedEffect")) {
//                    if (float.Parse(player.getData("WeedEffect")) < 1) {
//                        strength += 0.3f;
//                    }
//                    long removeInvokeID = long.Parse(player.getData("DrugInvoke"));
//                    InvokeController.RemoveTimedInvoke(removeInvokeID);
//                }
//                if (strength >= 0.6f) {
//                    strength = 0.6f;
//                }
//                player.setPermanentData("WeedEffect", strength.ToString());
//                player.setPermanentData("WeedEffectType", effect);
//                player.setTimeCycle(effect, strength);
//                invoke = InvokeController.AddTimedInvoke("Weed-Invoke", (ivk) => {
//                    player.stopTimeCycle();
//                    player.resetPermantData("WeedEffect");
//                }, TimeSpan.FromMinutes(length), false);
//                player.setPermanentData("DrugInvoke", invoke.InvokeId.ToString());
//            }
//        }

//        #region WeedTypes
//        public static int getWeedItemId(string weedType, WeedItemTypes weedItemType) {
//            var id = 0;
//            var typeString = "";

//            if (weedItemType == WeedItemTypes.Stick) { typeString = "Stick"; }
//            if (weedItemType == WeedItemTypes.Sappling) { typeString = "Sappling"; }
//            if (weedItemType == WeedItemTypes.Weed) { typeString = "Weed"; }
//            if (weedItemType == WeedItemTypes.CrushedWeed) { typeString = "CrushedWeed"; }
//            if (weedItemType == WeedItemTypes.DriedStick) { typeString = "DriedStick"; }
//            var idCheck = weedTypeList.FirstOrDefault(x => x.type == weedType && x.itemType == typeString);
//            if (idCheck != null) {
//                id = idCheck.itemId;
//            } else {
//                Logger.logError("Gibts nicht");
//            }
//            return id;
//        }

//        private void FillList() {
//            using (var db = new ChoiceVDb()) {
//                foreach (var weedType in db.configweedtypes) {
//                    var weed = new weedType {
//                        type = weedType.type,
//                        itemType = weedType.itemType,
//                        itemId = weedType.resultItemId,
//                    };
//                    weedTypeList.Add(weed);
//                }
//            }
//        }
//        #endregion
//    }

//    public class weedType {
//        public string type { get; set; }
//        public string itemType { get; set; }
//        public int itemId { get; set; }

//    }
//}

