using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.InventorySystem {
    public class MarihuanaItem : Item {

        //public string DoorGroup { get => (string)Data["DoorGroup"]; set { Data["DoorGroup"] = value; } }

        public MarihuanaItem(item item) : base(item) { }

        public MarihuanaItem(configitem configItem, int amount, int quality) : base(configItem, quality, amount) {

        }
    }
}
