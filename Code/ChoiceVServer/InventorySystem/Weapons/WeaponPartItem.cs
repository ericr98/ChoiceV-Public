using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using Newtonsoft.Json;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.InventorySystem {
    public class WeaponPart {
        public WeaponPartType WeaponPartType;
        public WeaponType WeaponType;
        public string GTAComponent;

        public string WeaponName;
        public string ShowName;

        public WeaponPart(WeaponPartType weaponPartType, WeaponType weaponType, string gtaComponent = null, string weaponName = null, string showName = null) {
            WeaponPartType = weaponPartType;
            WeaponType = weaponType;
            GTAComponent = gtaComponent;

            WeaponName = weaponName;
            ShowName = showName;
        }

        public string ToInfo() {
            if(GTAComponent != null) {
                return GTAComponent;
            } else {
                return "";
            }
        }

        public override string ToString() {
            if(WeaponName == null) {
                return "Komponente für ein/eine: " + WeaponType.ToString();
            } else {
                return "Komponente für ein/eine: " + ShowName.ToString();
            }
        }
    }

    public class WeaponPartItem : Item {

        public WeaponPart WeaponPart { get => JsonConvert.DeserializeObject<WeaponPart>(Data["WeaponPart"]); set { Data["WeaponPart"] = value.ToJson(); } }

        public WeaponPartItem(item item) : base(item) { }

        public WeaponPartItem(configitem configItem, WeaponPart weaponPart) : base(configItem) {
            WeaponPart = weaponPart; // new WeaponPart((WeaponPartType)Enum.Parse(typeof(WeaponPartType), configItem.additionalInfo), weaponType, gtaComponent);
            updateDescription();
        }

        public override void updateDescription() {
            base.updateDescription();
            Description = WeaponPart.ToString();
        }

        public override void use(IPlayer player) {
            base.use(player);
            WeaponController.assembleWeapon(player, this);
        }
    }
}
