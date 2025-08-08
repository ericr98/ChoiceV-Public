using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.InventorySystem.Clothing {
    public class BadgesController : ChoiceVScript {
        public BadgesController() {
            ClothingController.addOnConnectClothesCheck(2, onVehicleClothingCheck);
        }

        private void onVehicleClothingCheck(IPlayer player, ref ClothingPlayer cloth) {
            //TODO
        }
    }

    public class Badges : EquipItem {
        public Badges(item item) : base(item) {

        }

        public Badges(configitem configItem, int percent) : base(configItem) {

        }

        public override void equip(IPlayer player) {
            base.equip(player);
        }
    }
}
