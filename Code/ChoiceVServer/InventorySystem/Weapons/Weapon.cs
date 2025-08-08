using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.InventorySystem {

    public class Weapon : EquipItem {
        public string WeaponName;
        public WeaponType WeaponType;

        public int TempAmmoSpent = 0;

        public List<WeaponPart> WeaponParts { get => JsonConvert.DeserializeObject<List<WeaponPart>>(Data["WeaponParts"]); set { Data["WeaponParts"] = value.ToJson(); } }

        public int? AmountOfAmmunation { get => (int?)Data["AmountOfAmmunation"]; set { Data["AmountOfAmmunation"] = value; } }

        public Weapon(item item) : base(item) { }

        public Weapon(configitem configItem, int amount, int quality) : base(configItem) {
            processAdditionalInfo(configItem.additionalInfo);
            WeaponParts = new List<WeaponPart>();
        }

        public Weapon(configitem configItem, List<WeaponPart> weaponparts) : base(configItem) {
            processAdditionalInfo(configItem.additionalInfo);
            if(weaponparts != null) {
                WeaponParts = weaponparts;
            } else {
                var list = new List<WeaponPart>();
                foreach(var part in WeaponTypeToWeaponParts[WeaponType]) {
                    if(IsWeaponPartSpecific[part]) {
                        list.Add(new WeaponPart(part, WeaponType, null, WeaponName, Name));
                    } else {
                        list.Add(new WeaponPart(part, WeaponType));
                    }
                }

                WeaponParts = list;
            }

            updateDescription();
        }

        public override void use(IPlayer player) {
            //TODO NEW_FEATURE
            return;
            base.use(player);

            if(!IsEquipped) {
                var menu = new Menu("Waffen Interaktion", "Was möchtest du tun?");
                var data = new Dictionary<string, dynamic> {
                    {"Item", this},
                };
                menu.addMenuItem(new ClickMenuItem("Auseinanderbauen", "Zerlege die Waffe in Einzelteile", "", "WEAPON_DISASSEMBLE").withData(data));
                player.showMenu(menu);
            } else {
                player.sendBlockNotification("Die Waffe ist ausgerüstet!", "Waffe blockiert!", NotifactionImages.Gun);
            }
        }

        public override void equip(IPlayer player) {
            if(player.hasLiteMode()) {
                player.sendBlockNotification("Du darfst im LiteMode keine Waffen ausrüsten! Schließe deine Einreise ab um alle Funktionen freizuschalten!", "LiteMode Block!");
                return;
            }
            WeaponController.equipWeaponToPlayer(player, this);

            base.equip(player);
        }

        public override void fastEquip(IPlayer player) {
            equip(player);
        }

        public override void unequip(IPlayer player) {
            WeaponController.unequipWeaponToPlayer(player, this);

            base.unequip(player);
        }

        public override void fastUnequip(IPlayer player) {
            unequip(player);
        }

        public override void processAdditionalInfo(string info) {
            base.processAdditionalInfo(info);
            WeaponName = info;
            var cf = WeaponController.getConfigWeapon(WeaponName);
            EquipType = cf.equipSlot;
            WeaponType = (WeaponType)Enum.Parse(typeof(WeaponType), cf.weaponType);
        }

        public override void updateDescription() {
            var desc = "Komponenten: ";
            foreach(var part in WeaponParts) {
                desc += part.ToInfo();
            }

            Description = desc;

            base.updateDescription();
        }
    }
}
