using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.Collectables {
    public class CollectableFlower : Collectable {
        public CollectableFlower(CollectableAreaTypes areaType, Position position) : base(areaType, position, 1.5f, 1.5f) {
            
        }

        protected override void loadModels() {
            Models = new List<string> {
                "prop_flower1", "prop_flower2", "prop_flower3", "prop_flower4",
                "prop_flower5", "prop_flower6", "prop_flower7", "prop_flower8",
                "prop_flower9", "prop_flower10", "prop_flower11", "prop_flower12",
                "prop_flower13"
            };
        }

        protected override float getZOffsetForModel(string model) {
            var r = new Random();
            if(model == "prop_flower11") {
                return -0.08f;
            } else {
                return r.NextFloat(-0.15f, -0.05f);
            }
        }

        protected override Rotation getRotationForModel(string model) {
            return new DegreeRotation(0, 0, new Random().Next(0, 360));
        }

        protected override Animation getAnimationForModel(string model) {
            return AnimationController.getAnimationByName(Constants.KNEEL_DOWN_ANIMATION);
        }

        public override void onCollectStep(IPlayer player) {
            var cfg = InventoryController.getConfigItem(i => i.additionalInfo == Object.ModelName);
            var item = InventoryController.createItem(cfg, 1);

            if (player.getInventory().addItem(item)) {
                player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast eine {item.Name} gesammelt!", $"{item.Name} erhalten");
                base.onCollectStep(player);
            } else {
                player.sendNotification(Constants.NotifactionTypes.Danger, "Dein Inventar ist voll!", "Inventar voll");
            }
        }

        protected override string getObjetModel() {
            switch (AreaType) {
                case CollectableAreaTypes.AllFlowers:
                    return Models[new Random().Next(0, Models.Count)];
                case CollectableAreaTypes.FlowerMountain:
                    return "prop_flower1";
                default:
                    return null;
            }
        }

    }
}
