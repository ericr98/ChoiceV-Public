//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.Controller;
//using ChoiceVServer.Model.Database;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using static ChoiceVServer.Base.Constants;

//namespace ChoiceVServer.InventorySystem {
//    public class LongpapePack : Item {
//        public int PapeCounter { get => (int)Data["PapeCounter"]; set { Data["PapeCounter"] = value; } }
//        public LongpapePack(items item) : base(item) {
//            updateDescription();
//        }

//        public LongpapePack(configitems configItem) : base(configItem) {
//            Data = new ExtendedDictionary<string, dynamic>(new Dictionary<string, dynamic>());
//            PapeCounter = 40;
//            updateDescription();
//        }

//        public override void use(IPlayer player) {
//            if (PapeCounter <= 0) {
//                player.sendNotification(NotifactionTypes.Warning, "Die Packung ist leer", "Packung leer");
//                return;
//            }
//            var configItem = InventoryController.AllConfigItems[LONGPAPE_ID];
//            var longPape = new Longpape(configItem);
//            var itemCheck = player.getInventory().addItem(longPape);
//            if (!itemCheck) {
//                player.sendNotification(NotifactionTypes.Warning, "Dein Inventar ist voll", "");
//            } else {
//                PapeCounter = PapeCounter - 1;
//                updateDescription();
//            }
//            InventoryController.showInventory(player, player.getInventory());

//        }

//        public override void updateDescription() {
//            Description = $"Inhalt: {PapeCounter} Papes";
//        }
//    }
//}
