//using AltV.Net.Data;
//using AltV.Net.Elements.Entities;
//using AltV.Net.Enums;
//using ChoiceVServer.Base;
//using ChoiceVServer.Controller;
//using ChoiceVServer.Model;
//using ChoiceVServer.Model.Database;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using static ChoiceVServer.Base.Constants;


//namespace ChoiceVServer.InventorySystem {
//    class FailedMoonshine : Item { 

//        public FailedMoonshine(items item) : base(item) { }

//        public FailedMoonshine(configitems configItem, int stackAmount) : base(configItem, -1, stackAmount) {
//            Description = "Trübe Flüssigkeit";
//        }

//        public override void use(IPlayer player) {
//            InvokeController.AddTimedInvoke("FailedMoonshineEffectStart", (ivk) => {
//                player.setTimeCycle("telescope", 8f);
//                player.sendNotification(NotifactionTypes.Danger, "Du fühlst dich richtig schlecht", "");
//                player.emitClientEvent("PLAY_ANIM", "missfam5_blackout", "vomit", 5000, 49, -1);
//            }, TimeSpan.FromSeconds(5), false);
//            InvokeController.AddTimedInvoke("FailedMoonshineEffectEnd", (ivk) => {
//                player.stopTimeCycle();
//            }, TimeSpan.FromSeconds(10), false);


//        }
//    }
//}
