using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.Collectables {

    public class CollectableCactus : Collectable {
        public CollectableCactus(CollectableAreaTypes areaType, Position position) : base(areaType, position, 1.5f, 1.5f) {

        }

        protected override void loadModels() {
            Models = new List<string> { "prop_peyote_water_01", "prop_peyote_highland_01", "prop_peyote_lowland_01" };
        }

        protected override float getZOffsetForModel(string model) {
            return -0.025f;
        }

        protected override Animation getAnimationForModel(string model) {
            return AnimationController.getAnimationByName(Constants.KNEEL_DOWN_ANIMATION);
        }

        public override void onCollectStep(IPlayer player) {
            configitem configitem = null;
            switch(Object.ModelName) { 
                case "prop_peyote_water_01":
                    configitem = InventoryController.getConfigItemByCodeIdentifier("PEYOTE_WATER");
                    break;
                case "prop_peyote_highland_01":
                    configitem = InventoryController.getConfigItemByCodeIdentifier("PEYOTE_HIGHLAND");
                    break;
                case "prop_peyote_lowland_01":
                    configitem = InventoryController.getConfigItemByCodeIdentifier("PEYOTE_LOWLAND");
                    break;
            }

            var item = new StaticItem(configitem, 1, -1);
            if (player.getInventory().addItem(item)) {
                player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast einen {item.Name} aufgenommen!", $"{item.Name} aufgenommen", Constants.NotifactionImages.Package);
                base.onCollectStep(player);
            } else {
                player.sendBlockNotification("Du hast keinen Platz den Peyote aufzunehmen", "Zu wenig Platz", Constants.NotifactionImages.Package);
            }
        }

        protected override string getObjetModel() {
            switch(AreaType) {
                case CollectableAreaTypes.AllCactus:
                    return Models[new Random().Next(0, Models.Count)];
                case CollectableAreaTypes.CactusWater:
                    return "prop_peyote_water_01";
                case CollectableAreaTypes.CactusHighland:
                    return "prop_peyote_highland_01";
                case CollectableAreaTypes.CactusLowland:
                    return "prop_peyote_lowland_01";
                default:
                    return null;
            }
        }

        public override void destroy() {
            base.destroy();
        }
    }
}
