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
//    public class CocaineBowl : ProcessingObject {
//        public CocaineBowl(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) {
//        }

//        public CocaineBowl(Item placeableItem, IPlayer player, Position playerPosition, Rotation playerRotation) : base(placeableItem, player, playerPosition, playerRotation, 130, false) {
//            CurrentObjectModel = "prop_peanut_bowl_01";
//        }


//        public override void loadPredicates() {
//            base.loadPredicates();
//            MovePredicate = new Predicate<Item>(
//              i => !(i is Chemical || i.ConfigId == COCA_LEAF_ID || i.ConfigId == WATER_ID || i.ConfigId == COCA_FAILED_PASTE_ID || i.ConfigId == COCA_PASTE_ID || i.ConfigId == GREEN_MUSH_ID || i.StackAmount >= 1 || i is Cocaine));

//            Recipe = new List<Predicate<Item>> {
//                { i => i.ConfigId == COCA_PASTE_ID && i.StackAmount == 1 },
//                { i => i.ConfigId == ACETONE_ID && i.StackAmount == 1}, //Aceton
//                { i => i.ConfigId == AMMONIA_ID && i.StackAmount >= 1 && i.StackAmount <= 2}, //Ammoniak
//                { i => i.ConfigId == CALCIUM_CARBONATE && i.StackAmount >= 1 && i.StackAmount <= 2}, //Calciumcarbonat
//            };
//        }

//        public override void startProcessing(IPlayer player) {
//            var check = Inventory.getAllItems().FirstOrDefault(x => x.ConfigId == COCA_PASTE_ID);
//            if (check == null) {
//                player.sendNotification(NotifactionTypes.Warning, "Was willst du denn hier verarbeiten?", "", NotifactionImages.System);
//                return;
//            }
//            if (check.StackAmount > 1) {
//                player.sendNotification(NotifactionTypes.Warning, "Immer nur ein Kilo Paste pro Verarbeitung", "", NotifactionImages.System);
//                return;
//            }
//            base.startProcessing(player);
//            FinishDate = DateTime.Now + TimeSpan.FromSeconds(10);
//        }

//        public override void finishProcessing(bool worked) {
//            var bowlInv = Inventory;
//            base.finishProcessing(worked);
//            InvokeController.AddTimedInvoke("Finish_Timer", (i) => {
//                if (worked) {
//                    var configItem = InventoryController.AllConfigItems[COCAINE_ID];
//                    var item = new Cocaine(configItem, 400);
//                    bowlInv.addItem(item);
//                } else {
//                    var configItem = InventoryController.AllConfigItems[COCA_FAILED_PASTE_ID];
//                    var item = new FailedCocainePaste(configItem, 1);
//                    bowlInv.addItem(item);
//                }
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

//            return new ClickMenuItem("Verarbeitung starten", "Warte bis die Chemikalien sich mit der Paste binden", "", "START_PROCESSING_PROCESS", MenuItemStyle.normal).withData(data);
//        }
//    }
//}
