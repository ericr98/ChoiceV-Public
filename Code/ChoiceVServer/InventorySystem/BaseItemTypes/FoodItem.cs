using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Character;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem {
    [Flags]
    public enum SpecialFoodType: uint {
        None = 0,
        CombineableItem = 1,
    }
    
    public class FoodItem : Item {
        public SpecialFoodType SpecialFoodType { get; private set; }
        
        public double Hunger;
        public double Thirst;
        public double Energy;

        public FoodItem(item item) : base(item) {
            init(item.config);
        }

        public FoodItem(configitem configItem, int amount, int quality) : base(configItem, quality, amount) {
            init(configItem);
        }

        public FoodItem(configitem configItem, int quality) : base(configItem, quality, null) {
            init(configItem);
        }

        protected void init(configitem configItem) {
            if(configItem.codeItem == nameof(FoodItem) && !string.IsNullOrEmpty(configItem.additionalInfo)) {
                SpecialFoodType = (SpecialFoodType)uint.Parse(configItem.additionalInfo ?? "0");
            }

            if(configItem.configitemsfoodadditionalinfo == null) {
                Hunger = 0;
                Thirst = 0;
                Energy = 0;

                Logger.logError($"Item with Name  {configItem.configItemId}:{configItem.name} is a Food Item and had no configitemsfoodadditionalinfo", $"configitemsfoodadditionalinfo für {configItem.configItemId}:{configItem.name} nicht eingetragen!");
                return;
            }

            if(configItem.configitemsfoodadditionalinfo.category != null) {
                Hunger = configItem.configitemsfoodadditionalinfo.categoryNavigation.hunger;
                Thirst = configItem.configitemsfoodadditionalinfo.categoryNavigation.thirst;
                Energy = configItem.configitemsfoodadditionalinfo.categoryNavigation.energy;
            } else {
                Hunger = configItem.configitemsfoodadditionalinfo.hunger ?? 0;
                Thirst = configItem.configitemsfoodadditionalinfo.thirst ?? 0;
                Energy = configItem.configitemsfoodadditionalinfo.energy ?? 0;
            }
        }

        public override void use(IPlayer player) {
            base.use(player);

            var data = player.getCharacterData();

            data.Hunger += Hunger;
            data.Thirst += Thirst;
            data.Energy += Energy;

            if(data.Hunger > 100) { data.Hunger = 100; }
            if(data.Thirst > 100) { data.Thirst = 100; }
            if(data.Energy > 100) { data.Energy = 100; }

            player.updateHud();
        }

        public override object Clone() {
            return this.MemberwiseClone();
        }

        public override List<CharacterType> canBeUsedByCharacterTypes() {
            return [CharacterType.Player, CharacterType.Cat, CharacterType.Dog];
        }

        protected override Animation getUseAnimation(IPlayer player) {
            var charType = player.getCharacterType();
            if(charType == CharacterType.Player) {
                return ItemAnimation;
            } else if(charType == CharacterType.Dog){
                var eatAnim = AnimationController.getAnimationByName("DOG_EAT");
                if(eatAnim != null) {
                    eatAnim.AccompanyingFacialDict = "creatures@rottweiler@amb@world_dog_barking@idle_a";
                    eatAnim.AccompanyingFacialName = "idle_a_facial";
                }

                return eatAnim;
            } else {
                return null;
            }
        }
    }
}
