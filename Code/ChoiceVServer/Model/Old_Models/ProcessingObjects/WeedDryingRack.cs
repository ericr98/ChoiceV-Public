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
//    public class WeedDryingRack : ProcessingObject {
//        public string LastWeedType { get => (string)Data["LastWeedType"]; set { Data["LastWeedType"] = value; } }
//        public WeedDryingRack(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) { }

//        public WeedDryingRack(Item placeableItem, IPlayer player, Position playerPosition, Rotation playerRotation) : base(placeableItem, player, playerPosition, playerRotation, 130, false) {
//            CurrentObjectModel = "prop_weed_01";
//        }


//        public override void loadPredicates() {
//            base.loadPredicates();
//            MovePredicate = new Predicate<Item>(
//              i => !(i is WeedStick || i is DriedWeedStick)
//            );

//            Recipe = new List<Predicate<Item>> {
//                { i => i is WeedStick }
//            };
//        }

//        public override void startProcessing(IPlayer player) {
//            var check = Inventory.getAllItems().FirstOrDefault(x => x is WeedStick);
//            if (check == null) {
//                player.sendNotification(NotifactionTypes.Warning, "Was willst du denn hier trocknen?", "", NotifactionImages.System);
//                return;
//            }
//            base.startProcessing(player);
//            FinishDate = DateTime.Now + TimeSpan.FromSeconds(10); //TODO CHANGE TO REAL TIME
//        }

//        public override void checkRecipeOnFinish() {
//            var worked = true;

//            foreach (var item in Inventory.getAllItems()) {
//                if (!(item is WeedStick)) {
//                    worked = false;
//                }
//            }
//            finishProcessing(worked);
//        }

//        public override void finishProcessing(bool worked) {
//            var rackInv = Inventory;

//            InvokeController.AddTimedInvoke("Finish_Timer", (i) => {
//                var itemList = Inventory.getAllItems();
//                if (worked) {
//                    var r = new Random();
//                    foreach (var weedItem in itemList.ToArray()) {
//                        var type = (string)weedItem.Data["WeedType"];
//                        var weedStickConfigId = WeedController.getWeedItemId(type, WeedItemTypes.DriedStick);
//                        var configItem = InventoryController.AllConfigItems[weedStickConfigId];
//                        var item = new DriedWeedStick(configItem, weedItem.Quality, weedItem.StackAmount ?? 1, true, type);
//                        rackInv.removeItem(weedItem, weedItem.StackAmount ?? 1);
//                        rackInv.addItem(item);
//                        LastWeedType = type;
//                    }
//                }
//                InProcess = false;
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
//                EvidenceController.createWeedEvidence(player, LastWeedType);
//            }
//        }

//        public override MenuItem getProcessStartMenuItem() {
//            var data = new Dictionary<string, dynamic> {
//                {"placeable", this }
//            };

//            return new ClickMenuItem("Trocknen starten", "Trocknet die Knospen", "", "START_PROCESSING_PROCESS", MenuItemStyle.normal).withData(data);
//        }
//    }
//}
