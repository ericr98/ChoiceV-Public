//using AltV.Net.Data;
//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.Controller;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.InventorySystem;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Model.Menu;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using static ChoiceVServer.Base.Constants;

//namespace ChoiceVServer.Model.PlaceObjects {
//    public class Destillery : ProcessingObject {

//        public int currentTemp { get => (int)Data["CurrentTemp"]; set { Data["CurrentTemp"] = value; } }
//        public bool activeFire { get => (bool)Data["ActiveFire"]; set { Data["ActiveFire"] = value; } }
//        public string CurrentType;
//        public List<int> tempList = new List<int>();
//        //public List<> currentViews = new List<>();
//        public Destillery(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) { }

//        public Destillery(Item placeableItem, IPlayer player, Position playerPosition, Rotation playerRotation) : base(placeableItem, player, playerPosition, playerRotation, 130, false, true, true, true) {
//            CurrentObjectModel = "prop_still";
//            currentTemp = 80;
//            activeFire = false;
//            CurrentType = "null";
//        }

//        public override void loadPredicates() {
//            base.loadPredicates();
//            MovePredicate = new Predicate<Item>(
//              i => !(i.ConfigId == CORN_ID || i.ConfigId == APPLE_ID || i.ConfigId == WATER_ID || i.ConfigId == RASPBERRY_ID || i.ConfigId == PEAR_ID || i.ConfigId == CHERRY_ID || i.ConfigId == YEAST_ID || i.ConfigId == BOTTLE_ID || i.ConfigId == MOONSHINE_ID || i.ConfigId == FAILEDMOONSHINE_ID)
//            );
//            var predicate = (Predicate<Item>)null;
//            if (CurrentType == "Apple") { predicate = new Predicate<Item>(i => i.ConfigId == APPLE_ID && i.StackAmount >= 8 && i.StackAmount <= 12); }
//            if (CurrentType == "Corn") { predicate = new Predicate<Item>(i => i.ConfigId == CORN_ID && i.StackAmount >= 8 && i.StackAmount <= 12); }
//            if (CurrentType == "Raspberry") { predicate = new Predicate<Item>(i => i.ConfigId == RASPBERRY_ID && i.StackAmount >= 65 && i.StackAmount <= 75); }
//            if (CurrentType == "Pear") { predicate = new Predicate<Item>(i => i.ConfigId == PEAR_ID && i.StackAmount >= 8 && i.StackAmount <= 12); }
//            if (CurrentType == "Cherry") { predicate = new Predicate<Item>(i => i.ConfigId == CHERRY_ID && i.StackAmount >= 65 && i.StackAmount <= 75); }
//            if (CurrentType == "mixed") { predicate = new Predicate<Item>(i => i.ConfigId == COCA_PASTE_ID); }
//            Recipe = new List<Predicate<Item>> {
//                { i => i.ConfigId == WATER_ID && i.StackAmount >= 8 && i.StackAmount <= 12}, //Wasser
//                { i => i.ConfigId == YEAST_ID && i.StackAmount >= 1 && i.StackAmount <= 2}, //Hefe
//            };
//            if (predicate != null) {
//                Recipe.Add(predicate);
//            }

//        }

//        public override void startProcessing(IPlayer player) {
//            activeFire = true;
//            var d = (DegreeRotation)Rotation;
//            CurrentObjectModel = "prop_still";
//            var newObject = ObjectController.createObject(CurrentObjectModel, Position - new Position(0, 0, 0.97f), new DegreeRotation(0, 0, d.Yaw + 90), 200, true);
//            ObjectController.deleteObject(Object);
//            Object = newObject;
//        }

//        public override void stopProcessing(IPlayer player) {
//            activeFire = false;
//            var d = (DegreeRotation)Rotation;
//            CurrentObjectModel = "prop_still";
//            var newObject = ObjectController.createObject(CurrentObjectModel, Position - new Position(0, 0, 0.97f), new DegreeRotation(0, 0, d.Yaw + 90), 200, true);
//            ObjectController.deleteObject(Object);
//            Object = newObject;

//        }

//        public override void finishProcessing(bool worked) {
//            var destilleryInv = Inventory;
//            base.finishProcessing(worked);
//            InvokeController.AddTimedInvoke("Finish_Timer", (i) => {
//                var tempSum = 0;
//                var amount = 0;
//                foreach (var temp in tempList) {
//                    tempSum = tempSum + temp;
//                    amount++;
//                }
//                var averageTemp = tempSum / amount;
//                if (averageTemp >= 80 && averageTemp <= 100) {
//                    if (worked) {
//                        if (CurrentType == "Apple" || CurrentType == "Corn" || CurrentType == "Raspberry" || CurrentType == "Pear" || CurrentType == "Pear" || CurrentType == "Cherry") {
//                            var configItem = InventoryController.AllConfigItems[MOONSHINE_ID];
//                            var item = new Moonshine(configItem, CurrentType, 5);
//                            destilleryInv.addItem(item);
//                        } else {
//                            var configItem = InventoryController.AllConfigItems[FAILEDMOONSHINE_ID];
//                            var item = new FailedMoonshine(configItem, 5);
//                            destilleryInv.addItem(item);
//                        }
//                    } else {
//                        var configItem = InventoryController.AllConfigItems[FAILEDMOONSHINE_ID];
//                        var item = new FailedMoonshine(configItem, 5);
//                        destilleryInv.addItem(item);
//                    }
//                } else {
//                    var configItem = InventoryController.AllConfigItems[FAILEDMOONSHINE_ID];
//                    var item = new FailedMoonshine(configItem, 5);
//                    destilleryInv.addItem(item);
//                }
//                tempList.Clear();
//            }, TimeSpan.FromSeconds(2), false);
//        }

//        public override void initialize(bool register = true) {
//            var d = (DegreeRotation)Rotation;
//            Object = ObjectController.createObject(CurrentObjectModel, Position - new Position(0, 0, 0.97f), new DegreeRotation(0, 0, d.Yaw + 90), 200, true);
//            base.initialize(register);
//        }

//        public override void onDestroy(IPlayer player) {
//            base.onDestroy(player);
//            Random random = new Random();
//            int randomnumber = random.Next(0, 99);
//            if (randomnumber >= 29) {
//                player.sendNotification(NotifactionTypes.Danger, "Das war aber nicht so ordentlich", "unnordentlich", NotifactionImages.System);
//                EvidenceController.createCocaineEvidence(player);
//            }
//        }

//        public override MenuItem getProcessStartMenuItem() {
//            var data = new Dictionary<string, dynamic> {
//                {"placeable", this }
//            };

//            return new ClickMenuItem("Feuer an", "Macht das Feuer an", "", "START_PROCESSING_PROCESS", MenuItemStyle.normal).withData(data);
//        }

//        public override MenuItem getProcessStopMenuItem() {
//            var data = new Dictionary<string, dynamic> {
//                {"placeable", this }
//            };

//            return new ClickMenuItem("Feuer aus", "Macht das Feuer aus", "", "STOP_PROCESSING_PROCESS", MenuItemStyle.normal).withData(data);
//        }

//        public override MenuItem getExtraMenuItem() {
//            return new StaticMenuItem($"Thermometer: {currentTemp}", "Zeigt die aktuelle Temperatur", "", MenuItemStyle.normal);
//        }

//        public override void onInterval() {
//            if (activeFire) {
//                if (currentTemp < 150) {
//                    currentTemp = currentTemp + 2;
//                } else {
//                    //Explosion
//                    base.destroyProp();
//                }
//            } else {
//                if (currentTemp > 20) {
//                    currentTemp = currentTemp - 1;
//                }
//            }
//            setType();
//            ProcessCheck();
//            loadPredicates();
//            if (InProcess) {
//                tempList.Add(currentTemp);
//            }
//            base.activeMenuViewCheck();
//            base.onInterval();
//        }

//        private void ProcessCheck() {
//            if(CurrentType != "null") {
//                if (FinishDate == DateTime.MaxValue) {
//                FinishDate = DateTime.Now + TimeSpan.FromMinutes(2);
//                }

//                InProcess = true;
//            } else {
//                FinishDate = DateTime.MaxValue;
//                InProcess = false;
//            }
//        }

//        public void setType() {
//            var type = "null";
//            var destInventory = Inventory;
//            var appleCheck = false;
//            var cornCheck = false;
//            var raspberryCheck = false;
//            var pearCheck = false;
//            var cherryCheck = false;
//            if (destInventory == null) {
//                return;
//            }
//            foreach (var item in destInventory.getAllItems()) {
//                if (item.ConfigId == APPLE_ID) {
//                    appleCheck = true;
//                    type = "Apple";
//                }
//                if (item.ConfigId == CORN_ID) {
//                    cornCheck = true;
//                    type = "Corn";
//                }
//                if (item.ConfigId == RASPBERRY_ID) {
//                    raspberryCheck = true;
//                    type = "Raspberry";
//                }
//                if (item.ConfigId == PEAR_ID) {
//                    pearCheck = true;
//                    type = "Pear";
//                }
//                if (item.ConfigId == CHERRY_ID) {
//                    cherryCheck = true;
//                    type = "Cherry";
//                }
//            }
//            var result = appleCheck ^ cornCheck ^ raspberryCheck ^ pearCheck ^ cherryCheck;
//            if (result) {
//                CurrentType = type; //Nur eine Sorte von Früchten
//            }
//            else if (type == "null") { //Garkeine Sorte
//                CurrentType = type;
//            } else {
//                type = "mixed"; //Mehrere Sorten
//                CurrentType = type;
//            }
//        }
//    }
//}
