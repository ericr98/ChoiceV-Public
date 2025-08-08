using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.InventorySystem {
    public class FoodMultiUseItem : FoodItem {
        private int MaxUses;
        private int UsesLeft { get => (int)Data["UsesLeft"]; set { Data["UsesLeft"] = value; } }

        public FoodMultiUseItem(item item) : base(item) { }

        public FoodMultiUseItem(configitem configItem, int amount, int quality) : base(configItem, quality) {
            UsesLeft = int.Parse(configItem.additionalInfo);
        }

        public override void use(IPlayer player) {
            UsesLeft = UsesLeft - 1;

            var data = player.getCharacterData();

            data.Hunger += Hunger / MaxUses;
            data.Thirst += Thirst / MaxUses;
            data.Energy += Energy / MaxUses;

            if (data.Hunger > 100) { data.Hunger = 100; }
            if (data.Thirst > 100) { data.Thirst = 100; }
            if (data.Energy > 100) { data.Energy = 100; }

            player.updateHud();


            if (UsesLeft <= 0) {
                destroy();
            } else {
                updateDescription();
            }
        }

        public override object Clone() {
            return MemberwiseClone();
        }

        public override void updateDescription() {
            if(!string.IsNullOrEmpty(ConfigItem.description)) {
                Description = $"Übrig: {UsesLeft} | {ConfigItem.description}";
            } else {
                Description = $"Übrig: {UsesLeft}";
            }

            base.updateDescription();
        }

        public override void processAdditionalInfo(string info) {
            var uses = int.Parse(info);
            MaxUses = uses;

            updateDescription();
        }
    }
}
