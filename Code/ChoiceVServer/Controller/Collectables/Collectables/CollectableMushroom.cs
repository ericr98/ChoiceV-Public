using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.Collectables {
    public class CollectableMushroom : Collectable {

        public CollectableMushroom(CollectableAreaTypes areaType, Position position) : base(areaType, position, 1.5f, 1.5f) {

        }

        protected override void loadModels() {
            Models = new List<string> { "prop_stoneshroom1", "prop_stoneshroom2" };
        }

        protected override float getZOffsetForModel(string model) {
            return -0.1f;
        }

        protected override Animation getAnimationForModel(string model) {
            return AnimationController.getAnimationByName(Constants.KNEEL_DOWN_ANIMATION);
        }

        public override void onCollectStep(IPlayer player) {
            base.onCollectStep(player);

            //TODO GENERATE RANDOM ARTEFACT ITEM
            ChoiceVAPI.SendChatMessageToPlayer(player, "Erfolgreich1!");
            var cf = InventoryController.getConfigById(37);
            var item = new StaticItem(cf, -1, 1);
            player.getInventory().addItem(item);
        }

        public override void destroy() {
            base.destroy();
        }
    }
}
