//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.Controller;
//using ChoiceVServer.Model.Database;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;
//using System.Text;

//namespace ChoiceVServer.InventorySystem {
//    class Joint : Item {
//        public string WeedType { get => (string)Data["WeedType"]; set { Data["WeedType"] = value; } }
//        public Joint(items item) : base(item) { }

//        public Joint(configitems configItem, string type, int quality) : base(configItem, -1, 1) {
//            Data = new ExtendedDictionary<string, dynamic>(new Dictionary<string, dynamic>());
//            Description = $"Ein Joint mit {type}";
//            WeedType = type;
//            Quality = quality;
//        }

//        public override void use(IPlayer player) {
//            var anim = AnimationController.getItemAnimationByName("SMOKE_JOINT");
//            if (player.hasData("LastSmoke")) {
//                var lastDate = DateTime.Parse(player.getData("LastSmoke"));
//                if (lastDate.AddSeconds(30) > DateTime.Now) {
//                    player.sendNotification(Constants.NotifactionTypes.Warning, "Du hast gerade erst etwas geraucht!", "Gerade geraucht!");
//                    return;
//                }
//            }
//            if (player.hasData("SmokeBlock")) {
//                player.sendNotification(Constants.NotifactionTypes.Info, "Du bist viel zu breit um jetzt noch was zu rauchen!", "Zu breit", Constants.NotifactionImages.Marihuana);
//                return;
//            }
//            player.setPermanentData("LastSmoke", DateTime.Now.ToString());
//            SoundController.playSoundAtCoords(player.Position, 10f, SoundController.Sounds.LighterSound);
//            AnimationController.animationTask(player, anim, () => {
//                WeedController.showWeedEffect(player, WeedType, true);
//            }, null, false);
//        }
//    }
//}
