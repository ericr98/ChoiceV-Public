//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.Controller;
//using ChoiceVServer.Model.Database;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace ChoiceVServer.InventorySystem {
//    class MescalineCactus : Item {
//        public MescalineCactus(items item) : base(item) { }

//        public MescalineCactus(configitems configItem, int stackAmount) : base(configItem, -1, stackAmount) {
//            Description = "Feuchter Meskalin Kaktus";
//        }

//        public override void use(IPlayer player) {
//            player.sendNotification(Constants.NotifactionTypes.Warning, "Dir ist ziemlich übel", "Übelkeit"); //TODO EFFECT 
//            player.setTimeCycle("SALTONSEA", 1f);
//            var invoke = InvokeController.AddTimedInvoke("Meskalin-Invoke", (ivk) => {
//                player.stopTimeCycle();
//            }, TimeSpan.FromMinutes(0.5), true);
//        }
//    }
//}
