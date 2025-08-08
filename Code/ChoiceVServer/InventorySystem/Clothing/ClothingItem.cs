using AltV.Net.Elements.Entities;
using Bogus.DataSets;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Controller.OrderSystem;
using ChoiceVServer.Controller.OrderSystem.OrderComponents.OrderItems;
using ChoiceVServer.Model.Database;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public class ClothingItem : EquipItem, NoKeepEquippedItem {
        public bool IsProp => isProp();
        public int ComponentId { get; protected set; }
        public int Drawable { get => (int)Data["Drawable"]; protected set { Data["Drawable"] = value; } }
        public int Texture { get => (int)Data["Texture"]; protected set { Data["Texture"] = value; } }

        public string Gender { get => (string)Data["Gender"]; protected set { Data["Gender"] = value; } }
        public string Dlc { get => Data.hasKey("Dlc") ? (string)Data["Dlc"] : null; protected set { Data["Dlc"] = value; } }

        public ClothingItem(item item) : base(item) { }

        //Generic Constructor for order system generation
        public ClothingItem(configitem configItem, int amount, int quality) : base(configItem) { }

        public ClothingItem(configitem configItem, int drawableId, int textureId, string name, char gender, string dlc = null) : base(configItem) {
            Drawable = drawableId;
            Texture = textureId;

            Gender = gender.ToString();

            Description = name;

            updateDescription();
            Dlc = dlc;
        }

        protected virtual bool isProp() {
            return false;
        }

        //Alles fixen wo TODO CLOTH sowie TODO MOD_CLOTH steht!

        public override void use(IPlayer player) {
            if(IsEquipped) {
                unequip(player);
            } else {
                equip(player);
            }
        }
        
        //Use flag to activate Hats in Vehicles (Maybe do CharacterSettings) 
        //ALLOW_HEAD_PROP_IN_VEHICLE

        public override void equip(IPlayer player) {
            if(changeNotAllowed(player)) {
                player.sendBlockNotification("Du kannst aktuell deine Kleidung nicht ändern!", "Kleidung nicht änderbar", Constants.NotifactionImages.System);
                return;
            }

            var gender = player.getCharacterData().Gender.ToString();
            if(Gender != "U" && gender != Gender) {
                player.sendBlockNotification("Das Kleidungsstück ist für ein anderes Geschlecht!", "Kleidung passt nicht", Constants.NotifactionImages.System);
                return;
            }

            var already = player.getInventory().getItem<ClothingItem>(i => i.ComponentId == ComponentId && i.IsEquipped);
            if(already != null) {
                already.IsEquipped = false;
            }

            var dressAnim = getAnimationForComponentId(ComponentId);
            AnimationController.animationTask(player, dressAnim, () => {
                CamController.checkIfCamSawAction(player, "Kleidung angezogen", $"Person hat neue Kleidung angezogen");
                equipStep(player);

                base.equip(player);
            });
        }

        public override void fastEquip(IPlayer player) {
            equipStep(player);

            base.fastEquip(player);
        }

        public virtual void equipStep(IPlayer player, bool alsoLoad = true) {
            var cl = ClothingController.getPlayerClothing(player);

            var drawableId = Drawable;
            if(Gender == "U") {
                drawableId = ClothingController.transformUniversalClothesDrawable(player.getCharacterData().Gender.ToString(), ComponentId, Drawable);
            }

            cl.UpdateClothSlot(ComponentId, drawableId, Texture, Dlc);

            if(alsoLoad) {
                ClothingController.loadPlayerClothing(player, cl);
            }
        }

        public override void unequip(IPlayer player) {
            if(changeNotAllowed(player)) {
                player.sendBlockNotification("Du kannst aktuell deine Kleidung nicht ändern!", "Kleidung nicht änderbar", Constants.NotifactionImages.System);
                return;
            }

            var undressAnim = getAnimationForComponentId(ComponentId);
            var naked = Constants.NakedMen;
            if(player.getCharacterData().Gender == 'F') {
                naked = Constants.NakedFemale;
            }

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

        protected virtual void unequipStep(IPlayer player) {
            var naked = Constants.NakedMen;
            if(player.getCharacterData().Gender == 'F') {
                naked = Constants.NakedFemale;
            }

            var cl = ClothingController.getPlayerClothing(player);
            cl.UpdateClothSlot(ComponentId, naked.GetSlot(ComponentId, false).Drawable, naked.GetSlot(ComponentId, false).Texture);
            ClothingController.loadPlayerClothing(player, cl);
        }

        public override void processAdditionalInfo(string info) {
            ComponentId = int.Parse(info);

            switch(ComponentId) {
                case 11:
                    EquipType = "top";
                    break;
                case 4:
                    EquipType = "legs";
                    break;
                case 6:
                    EquipType = "shoes";
                    break;
                case 7:
                    EquipType = "accessoire";
                    break;
                case 9:
                    EquipType = "armor";
                    break;
                case 1:
                    EquipType = "mask";
                    break;

            }
        }

        protected virtual Animation getAnimationForComponentId(int componentId) {
            if(componentId == 11) {
                return AnimationController.getAnimationByName("CLOTHING_REMOVE_TOP");
            } else if(componentId == 4) {
                return AnimationController.getAnimationByName("CLOTHING_REMOVE_LEGS");
            } else if(componentId == 6) {
                return AnimationController.getAnimationByName("CLOTHING_REMOVE_SHOES");
            } else if(componentId == 7) {
                return AnimationController.getAnimationByName("CLOTHING_REMOVE_ACCESSOIRE");
            } else if(componentId == 1) {
                return AnimationController.getAnimationByName("EQUIP_HAT");
            } else if(componentId == 9) {
                return AnimationController.getAnimationByName("EQUIP_ARMORWEST");
            } else {
                return null;
            }
        }

        public bool changeNotAllowed(IPlayer player) {
            return player.getInventory().hasItem<FullClothingItem>(i => i.IsEquipped);
        }

        public virtual void onConnectEquip(IPlayer player) {
            IsEquipped = true;
        }

        public override void setOrderData(OrderItem orderItem) {
            var clothingOrderItem = orderItem as OrderClothingItem;
            var configVariation = ClothingController.getClothingVariation(clothingOrderItem.ConfigElementId, clothingOrderItem.ClothingVariation);

            ComponentId = configVariation.clothing.componentid;
            Drawable = configVariation.clothing.drawableid;
            Texture = clothingOrderItem.ClothingVariation;

            Gender = configVariation.clothing.gender;
            Dlc = configVariation.clothing.dlc;

            Description = $"({Gender}) {configVariation.clothing.name}: {configVariation.name}";

            updateDescription();
        }
    }
}
