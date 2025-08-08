using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Controller.OrderSystem;
using ChoiceVServer.Controller.OrderSystem.OrderComponents.OrderItems;
using ChoiceVServer.Model.Database;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem {
    public class ClothingProp : ClothingItem {
        public ClothingProp(item item) : base(item) { }
        
        //Generic Constructor for order system generation
        public ClothingProp(configitem configItem, int amount, int quality) : base(configItem, amount, quality) { }
        
        public ClothingProp(configitem configItem, configclothingpropvariation variation) : base(configItem, variation.prop.drawableid, variation.variation, $"({variation.prop.gender}) {variation.prop.name}: {variation.name}", variation.prop.gender.ToCharArray()[0], variation.prop.dlc) { }

        protected override bool isProp() {
            return true;
        }

        public override void equipStep(IPlayer player, bool alsoLoad = true) {
            var cl = ClothingController.getPlayerClothing(player);
            cl.UpdateAccessorySlot(ComponentId, Drawable, Texture, Dlc);
            ClothingController.loadPlayerClothing(player, cl);

            makeHairReplacementCheck(player);
        }

        public override void onConnectEquip(IPlayer player) {
            makeHairReplacementCheck(player);
            base.onConnectEquip(player);
        }

        private void makeHairReplacementCheck(IPlayer player) {
            if (ComponentId == 0) {
                var hairReplacement = FaceFeatureController.getHelmetReplacementHair(player);
                if (hairReplacement != null) {
                    var style = player.getCharacterData().Style.ToJson().FromJson<characterstyle>();
                    style.hairStyle = hairReplacement.gtaId;
                    player.setStyle(style);
                }
            }
        }

        protected override void unequipStep(IPlayer player) {
            var naked = Constants.NakedMen;
            if(player.getCharacterData().Gender == 'F') {
                naked = Constants.NakedFemale;
            }

            var cl = ClothingController.getPlayerClothing(player);
            cl.UpdateAccessorySlot(ComponentId, naked.GetSlot(ComponentId, true).Drawable, naked.GetSlot(ComponentId, true).Texture);
            ClothingController.loadPlayerClothing(player, cl);
            
            if(ComponentId == 0) {
                FaceFeatureController.resetPlayerStyle(player);
            }
        }

        public override void processAdditionalInfo(string info) {
            ComponentId = int.Parse(info);

            switch(ComponentId) {
                case 0:
                    EquipType = "hat";
                    break;
                case 1:
                    EquipType = "glasses";
                    break;
                case 2:
                    EquipType = "ears";
                    break;
                case 6:
                    EquipType = "watch";
                    break;
                case 7:
                    EquipType = "bracelet";
                    break;
            }
        }

        protected override Animation getAnimationForComponentId(int componentId) {
            return AnimationController.getAnimationByName($"EQUIP_{EquipType.ToUpper()}");
        }

        public override void setOrderData(OrderItem orderItem) {
            var clothingOrderItem = orderItem as OrderClothingPropItem;
            var configVariation = ClothingController.getConfigPropVariation(clothingOrderItem.ConfigElementId, clothingOrderItem.PropVariation);
            
            ComponentId = configVariation.prop.componentid;
            Drawable = configVariation.prop.drawableid;
            Texture = clothingOrderItem.PropVariation;

            Gender = configVariation.prop.gender;
            Dlc = configVariation.prop.dlc;


            ComponentId = configVariation.prop.componentid;

            Description = $"({Gender}) {configVariation.prop.name}: {configVariation.name}";

            updateDescription();
        }
    }
}
