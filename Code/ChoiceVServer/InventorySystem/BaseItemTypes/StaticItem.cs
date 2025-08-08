using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.InventorySystem {
    public class StaticItem : Item {
        public string AdditionalInfo;

        public StaticItem(item item) : base(item) { }

        public StaticItem(configitem configItem, int amount, int quality) : base(configItem, quality, amount) { }

        public override object Clone() {
            return this.MemberwiseClone();
        }

        public override void processAdditionalInfo(string info) {
            AdditionalInfo = info;
        }
    }
}
