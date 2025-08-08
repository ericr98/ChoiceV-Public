using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Model.Database;
using System;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.InventorySystem {
    public enum HuntingRifleTypes : int {
        LowCaliber = 0, //.223
        HighCaliber = 1, //.300
    }

    public class HuntingRifle : LongWeapon {
        public HuntingRifleTypes HuntingType { get; private set; }

        public HuntingRifle(item item) : base(item) { }

        public HuntingRifle(configitem configItem, int amount, int quality) : base(configItem, amount, quality) {
            processAdditionalInfo(configItem.additionalInfo);
        }

        public override void processAdditionalInfo(string info) {
            HuntingType = (HuntingRifleTypes)int.Parse(info);
            WeaponName = "WEAPON_MARKSMANRIFLE";
            var cf = WeaponController.getConfigWeapon(WeaponName);
            EquipType = cf.equipSlot;
            WeaponType = (WeaponType)Enum.Parse(typeof(WeaponType), cf.weaponType);
        }

        public override void equip(IPlayer player) {
            base.equip(player);

            switch(HuntingType) {
                case HuntingRifleTypes.LowCaliber:
                    player.AddWeaponComponent(AltV.Net.Enums.WeaponModel.MarksmanRifle, ChoiceVAPI.Hash("COMPONENT_MARKSMANRIFLE_CLIP_01"));
                    break;
                case HuntingRifleTypes.HighCaliber:
                    player.AddWeaponComponent(AltV.Net.Enums.WeaponModel.MarksmanRifle, ChoiceVAPI.Hash("COMPONENT_MARKSMANRIFLE_CLIP_02"));
                    break;
            }
        }
    }

    public class HuntingAmmunation : WeaponAmmunation {
        private HuntingRifleTypes HuntingType;

        public HuntingAmmunation(item item) : base(item) { }

        public HuntingAmmunation(configitem configItem, int amount, int quality) : base(configItem, amount, -1) {
            processAdditionalInfo(configItem.additionalInfo);
        }

        public override void processAdditionalInfo(string info) {
            HuntingType = (HuntingRifleTypes)int.Parse(info);
            WeaponType = WeaponType.Sniper;
        }

        public override bool additionalEquipCheck(Weapon weapon) {
            if(weapon is HuntingRifle) {
                return (weapon as HuntingRifle).HuntingType == HuntingType;
            } else {
                return true;
            }
        }
    }
}
