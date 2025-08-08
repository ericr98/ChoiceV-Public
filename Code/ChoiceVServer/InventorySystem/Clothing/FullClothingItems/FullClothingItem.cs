using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bogus.DataSets.Name;

namespace ChoiceVServer.InventorySystem {
    public abstract class FullClothingItem : EquipItem, NoKeepEquippedItem {
        protected string AdditionalInfo { get; set; }

        public FullClothingItem(item item) : base(item) {
            AdditionalInfo = item.config.additionalInfo;
        }

        public FullClothingItem(configitem configItem, int amount, int quality) : base(configItem) {
            AdditionalInfo = configItem.additionalInfo;
        }

        public override void use(IPlayer player) {
            if (IsEquipped) {
                unequip(player);
            } else {
                equip(player);
            }
        }

        public override void equip(IPlayer player) {
            var already = player.getInventory().getItem<FullClothingItem>(i => i.IsEquipped);
            Animation undressAnim = null;
            if (already != null) {
                undressAnim = AnimationController.getAnimationByName("UNDRESS_SET");
            }


            AnimationController.animationTask(player, undressAnim, () => {
                if(already != null) {
                    already.fastUnequip(player);
                }

                var dressAnim = AnimationController.getAnimationByName("DRESS_SET");
                AnimationController.animationTask(player, dressAnim, () => {
                    CamController.checkIfCamSawAction(player, "Kleidung angezogen", $"Person hat neue Kleidung angezogen");
                    equipStep(player);

                    base.equip(player);
                });
            });
        }

        public override void fastEquip(IPlayer player) {
            equipStep(player);

            base.fastEquip(player);
        }

        protected virtual void equipStep(IPlayer player) {
            var gender = player.getCharacterData().Gender.ToString();
            var components = getComponents(gender);

            var cl = ClothingController.getPlayerClothing(player);
            foreach (var (drawableId, component) in components) {
                cl.UpdateClothSlot(drawableId, component.Drawable, component.Texture, component.Dlc);
            }

            ClothingController.loadPlayerClothing(player, cl);

            onEquipAdditional(player);
        }

        protected virtual void onEquipAdditional(IPlayer player) { }

        public override void unequip(IPlayer player) {
            var undressAnim = AnimationController.getAnimationByName("UNDRESS_SET");

            AnimationController.animationTask(player, undressAnim, () => {
                CamController.checkIfCamSawAction(player, "Kleidung ausgezogen", $"Person hat aktuelle Kleidung ausgezogen");
                unequipStep(player);

                base.unequip(player);
            });
        }

        public override void fastUnequip(IPlayer player) {
            unequipStep(player);

            base.fastUnequip(player);
        }

        protected virtual void onUnequipAdditional(IPlayer player) { }

        protected virtual void unequipStep(IPlayer player) {
            var gender = player.getCharacterData().Gender.ToString();
            var components = getComponents(gender);
            var naked = Constants.NakedMen;
            if (gender == "F") {
                naked = Constants.NakedFemale;
            }

            var cl = ClothingController.getPlayerClothing(player);
            foreach (var (drawableId, component) in components) {
                cl.UpdateClothSlot(drawableId, naked.GetSlot(drawableId, false).Drawable, naked.GetSlot(drawableId, false).Texture);
            }

            var equippedClothes = player.getInventory().getItems<ClothingItem>(i => i.IsEquipped);
            foreach (var item in equippedClothes) {
                item.equipStep(player, false);
            }

            ClothingController.loadPlayerClothing(player, cl);

            onUnequipAdditional(player);
        }


        public abstract List<(int, ClothingComponent)> getComponents(string gender);
        public abstract bool allowsMaks(string gender);

        public virtual void onConnectEquip(IPlayer player) {
            IsEquipped = true;
        }
    }
}
