//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.Controller;
//using ChoiceVServer.Model.Database;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ChoiceVServer.InventorySystem {
//    class Cocaine : Item {

//        public Cocaine(items item) : base(item) {
//            updateDescription();
//        }

//        public Cocaine(configitems configItem, int stackAmount) : base(configItem, -1, stackAmount) {
//            updateDescription();
//        }

//        public override void use(IPlayer player) {
//            var anim = AnimationController.getItemAnimationByName("SMOKE_JOINT");
//            player.setPermanentData("LastSmoke", DateTime.Now.ToString());
//            SoundController.playSoundAtCoords(player.Position, 10f, SoundController.Sounds.LighterSound);
//            AnimationController.animationTask(player, anim, () => {
//                player.setTimeCycle("SaltonSea", 1f);
//                DrugController.setCocaineLevel(player);
//                player.setPermanentData("CocaineEffect", "SaltonSea");
//            }, null, false);
//            InvokeController.AddTimedInvoke("Cocaine-Invoke", (ivk) => {
//                player.stopTimeCycle();
//                player.resetPermantData("CocaineEffect");
//            }, TimeSpan.FromMinutes(10), true);
//        }

//        public override void updateDescription() {
//            Description = $"Pulver so weiß wie Schnee";
//        }
//    }
//}
