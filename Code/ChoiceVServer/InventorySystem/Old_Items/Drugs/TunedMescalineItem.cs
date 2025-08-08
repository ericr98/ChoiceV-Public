//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.Controller;
//using ChoiceVServer.Model.Database;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using static ChoiceVServer.Base.Constants;


//namespace ChoiceVServer.InventorySystem {
//    class TunedMescalineItem : Item {

//        public string ItemType { get => (string)Data["OriginItemType"]; set { Data["OriginItemType"] = value; } }
//        public TunedMescalineItem(items item) : base(item) { }

//        public TunedMescalineItem(configitems configItem, string originItemName, string type) : base(configItem) {
//            Description = $"{originItemName} verfeinert mit Meskalin";
//            ItemType = type;
//        }

//        public override void use(IPlayer player) {
//            var animName = "SMOKE_JOINT";
//            var effectName = "michealspliff";
//            var tripType = TripTypes.ClownTrip; // TODO: Create some Trips in Trip Controller
//            if (ItemType == "Joint") {

//            }
//            if (ItemType == "Blunt") {
//                effectName = "SaltonSea";
//            }
//            if (ItemType == "Cigarette") {
//                animName = "SMOKE_CIGARETTE";
//                effectName = "trevorspliff";
//            }
//            if (player.hasData("LastSmoke")) {
//                var lastDate = DateTime.Parse(player.getData("LastSmoke"));
//                if (lastDate.AddSeconds(30) > DateTime.Now) {
//                    player.sendNotification(Constants.NotifactionTypes.Warning, "Du hast gerade erst etwas geraucht!", "Gerade geraucht!");
//                    return;
//                }
//            }
//            if (player.HasData("DrugInvoke")) {
//                var removeInvoke = player.getData("DrugInvoke");
//                InvokeController.RemoveTimedInvoke(removeInvoke);
//            }
//            if (player.HasData("SmokeBlock")) {
//                player.sendNotification(Constants.NotifactionTypes.Info, "Du bist viel zu breit um jetzt noch was zu rauchen!", "Zu breit", Constants.NotifactionImages.Marihuana);
//                return;
//            }
//            player.setPermanentData("LastSmoke", DateTime.Now.ToString());
//            SoundController.playSoundAtCoords(player.Position, 10f, SoundController.Sounds.LighterSound);
//            var anim = AnimationController.getItemAnimationByName(animName);
//            AnimationController.animationTask(player, anim, () => {
//                player.setTimeCycle(effectName, 1f);
//                player.setPermanentData("MescalineEffect", effectName);
//                player.setPermanentData("MescalineTripType", tripType.ToString());
//            });
//            player.setPermanentData("SmokeBlock", "true");
//            var invoke = InvokeController.AddTimedInvoke("Mescaline-Invoke", (ivk) => {
//                player.stopTimeCycle();
//            }, TimeSpan.FromMinutes(5), true);
//            player.setData("DrugInvoke", invoke);
//        }
//    }
//}

