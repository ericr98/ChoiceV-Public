//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.Controller;
//using ChoiceVServer.Model.Database;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using static ChoiceVServer.Base.Constants;

//namespace ChoiceVServer.InventorySystem {
//    class DriedMescalineCactus : Item {
//        public DriedMescalineCactus(items item) : base(item) { }

//        public DriedMescalineCactus(configitems configItem, int stackAmount) : base(configItem, -1, stackAmount) {
//            Description = "Getrockneter Meskalin Kaktus";
//        }

//        public override void use(IPlayer player) {
//            if (player.getInventory().getItem(x => x is Crusher) != null) {
//                player.sendNotification(NotifactionTypes.Warning, "Mit deinem Crusher kannst du den Kaktus zerkleinern!", "Nutz den Crusher!");
//            } else {
//                player.sendNotification(NotifactionTypes.Warning, "Mit der Hand kannst du den Kaktus nicht zerkleinern!", "");
//            }
//        }
//    }
//}
