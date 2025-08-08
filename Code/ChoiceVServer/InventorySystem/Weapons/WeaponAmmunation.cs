using ChoiceVServer.Model.Database;
using System;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.InventorySystem {
    public class WeaponAmmunation : EquipItem {
        public WeaponType WeaponType;

        public WeaponAmmunation(item item) : base(item) { }

        public WeaponAmmunation(configitem configItem, int amount, int quality) : base(configItem, amount) {
            WeaponType = (WeaponType)Enum.Parse(typeof(WeaponType), configItem.additionalInfo);
        }

        public override void processAdditionalInfo(string info) {
            base.processAdditionalInfo(info);

            WeaponType = (WeaponType)Enum.Parse(typeof(WeaponType), info);
        }

        /// <summary>
        /// Is checked when Weapon is equipped. Override for exp. different calibers
        public virtual bool additionalEquipCheck(Weapon weapon) { return true; }
    }
}
