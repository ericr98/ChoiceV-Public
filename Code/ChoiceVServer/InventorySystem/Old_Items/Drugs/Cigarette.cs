//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.Controller;
//using ChoiceVServer.Model.Database;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ChoiceVServer.InventorySystem {
//    class Cigarette : Item {

//        public Cigarette(items item) : base(item) { }

//        public Cigarette(configitems configItem) : base(configItem, -1, 1) {
//            Description = $"Eine Zigarette";
//        }

//        public override void use(IPlayer player) {
//            if (player.hasData("LastSmoke")) {
//                var lastDate = (DateTime)player.getData("LastSmoke");
//                if (lastDate.AddSeconds(30) > DateTime.Now) {
//                    player.sendNotification(Constants.NotifactionTypes.Warning, "Du hast gerade erst etwas geraucht!", "Gerade geraucht!");
//                    return;
//                }
//            }
//            player.setPermanentData("LastSmoke", DateTime.Now.ToString());
//            SoundController.playSoundAtCoords(player.Position, 10f, SoundController.Sounds.LighterSound);
//            var anim = AnimationController.getItemAnimationByName("SMOKE_CIGARETTE");
//            AnimationController.playItemAnimation(player, anim, null, false);
//        }
//    }
//}
