using AltV.Net.Elements.Entities;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.InventorySystem {
    public class FireExtinguisher : EquipItem, NoEquipSlotItem {
        public FireExtinguisher(item item) : base(item) { }

        public FireExtinguisher(configitem configItem, int amount, int quality) : base(configItem) { }


        public override void equip(IPlayer player) {
            base.equip(player);

            player.GiveWeapon(AltV.Net.Enums.WeaponModel.FireExtinguisher, -1, true);
        }

        public override void unequip(IPlayer player) {
            base.unequip(player);

            player.RemoveWeapon(AltV.Net.Enums.WeaponModel.FireExtinguisher);
        }

        public override void fastUnequip(IPlayer player) {
            base.fastUnequip(player);

            player.RemoveWeapon(AltV.Net.Enums.WeaponModel.FireExtinguisher);
        }
    }
}
