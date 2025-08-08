using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.InventorySystem {
    public class DriedMarihuanaItem : Item {

        //public string DoorGroup { get => (string)Data["DoorGroup"]; set { Data["DoorGroup"] = value; } }

        public DriedMarihuanaItem(item item) : base(item) { }

        public DriedMarihuanaItem(configitem configItem, int amount, int quality) : base(configItem, quality, amount) {

        }
    }
}
