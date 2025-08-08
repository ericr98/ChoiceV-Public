using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Character;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem {
    public class EquipItem : Item {
        public bool IsEquipped = false;
        public string EquipType;

        public EquipItem(item item) : base(item) { }

        public EquipItem(configitem configItem) : base(configItem) { }

        public EquipItem(configitem configItem, int? amount) : base(configItem, -1, amount) { }

        //Constructor for generic generation
        public EquipItem(configitem configItem, int amount, int quality) : base(configItem) { }

        public override void use(IPlayer player) {
            base.use(player);
        }

        public void evaluateEquip(IPlayer player) {
            if(!canBeEquippedCharacterTypes().Contains(player.getCharacterType())) {
                player.sendBlockNotification("Deine Art Charakter kann dieses Item nicht benutzen", "Benutzen fehlgeschlagen");
                return;
            }
            if(!IsEquipped) {
                equip(player);
            } else {
                unequip(player);
            }
        }

        public virtual void equip(IPlayer player) {
            IsEquipped = true;
        }

        public virtual void unequip(IPlayer player) {
            IsEquipped = false;
        }

        public virtual void fastEquip(IPlayer player) {
            IsEquipped = true;
        }

        public virtual void fastUnequip(IPlayer player) {
            IsEquipped = false;
        }

        public override bool isBlocked() {
            return IsEquipped;
        }

        public override object Clone() {
            return this.MemberwiseClone();
        }
        
        public virtual List<CharacterType> canBeEquippedCharacterTypes() {
            return [CharacterType.Player];
        }
    }
}
