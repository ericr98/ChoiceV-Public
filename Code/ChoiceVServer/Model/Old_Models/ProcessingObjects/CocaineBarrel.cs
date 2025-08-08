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
//    public class CocaineBarrel : ProcessingObject {
//        public CocaineBarrel(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) { }

//        public CocaineBarrel(Item placeableItem, IPlayer player, Position playerPosition, Rotation playerRotation) : base(placeableItem, player, playerPosition, playerRotation, 130, false) {
//            CurrentObjectModel = "prop_barrel_02a";
//        }


//        public override void loadPredicates() {
//            base.loadPredicates();
//            MovePredicate = new Predicate<Item>(
//              i => !(i is Chemical || i.ConfigId == COCA_LEAF_ID || i.ConfigId == WATER_ID || i.ConfigId == COCA_FAILED_PASTE_ID || i.ConfigId == COCA_PASTE_ID || i.ConfigId == GREEN_MUSH_ID || i.StackAmount >= 1)
//            );

//            Recipe = new List<Predicate<Item>> {
//                { i => i.ConfigId == WATER_ID && i.StackAmount >=5 && i.StackAmount <= 7 },
//                { i => i.ConfigId == SULFURIC_ACID_ID && i.StackAmount >= 1 && i.StackAmount <= 2}, //Schwefelsäure
//                { i => i.ConfigId == COCA_LEAF_ID && i.StackAmount >= 80 && i.StackAmount <= 120}, //Kokablatt
//                { i => i.ConfigId == KEROSENE_ID && i.StackAmount == 1 }, //Kerosin
//                { i => i.ConfigId == POTASSIUM_CARBONATE_ID && i.StackAmount >= 1 && i.StackAmount <= 2}, //Kaliumcarbonat
//                { i => i.ConfigId == SODIUM_ID && i.StackAmount >= 1 && i.StackAmount <= 2}, //Natriumcarbonat
//            };
//        }

//        public override void startProcessing(IPlayer player) {
//            var check = Inventory.getAllItems().FirstOrDefault(x => x.ConfigId == COCA_LEAF_ID);
//            if (check == null) {
//                player.sendNotification(NotifactionTypes.Warning, "Was willst du denn hier verarbeiten?", "", NotifactionImages.System);
//                return;
//            }
//            base.startProcessing(player);
//            FinishDate = DateTime.Now + TimeSpan.FromSeconds(30);
//        }

//        public override void finishProcessing(bool worked) {
//            var barrelInv = Inventory;
//            base.finishProcessing(worked);
//            InvokeController.AddTimedInvoke("Finish_Timer", (i) => {
//                var mushConfigItem = InventoryController.AllConfigItems[GREEN_MUSH_ID];
//                var mushItem = new Item(mushConfigItem, -1, 85);
//                barrelInv.addItem(mushItem);
//                if (worked) {
//                    var configItem = InventoryController.AllConfigItems[COCA_PASTE_ID];
//                    var item = new CocainePaste(configItem, 1);
//                    barrelInv.addItem(item);
//                } else {
//                    var configItem = InventoryController.AllConfigItems[COCA_FAILED_PASTE_ID];
//                    var item = new FailedCocainePaste(configItem, 1);
//                    barrelInv.addItem(item);
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

//            return new ClickMenuItem("Verarbeitung starten", "Öffnet den Deckel und lässt Sauerstoff in die Tonne", "", "START_PROCESSING_PROCESS", MenuItemStyle.normal).withData(data);
//        }
//    }
//}
