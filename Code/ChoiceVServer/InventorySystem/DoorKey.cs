using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.InventorySystem {
    public class DoorKey : Item {
        public string DoorGroup { get => (string)Data["DoorGroup"]; set { Data["DoorGroup"] = value; } }
        public int DoorId { get => (int)Data["DoorId"]; set { Data["DoorId"] = value; } }
        public int LockIndex { get => (int)Data["LockIndex"]; set { Data["LockIndex"] = value; } }
        public int Dimension { get => (int)Data["Dimension"]; set { Data["Dimension"] = value; } }

        public DoorKey(item item) : base(item) { }

        public DoorKey(configitem configItem, int lockIndex, int dimension, string doorGroup = "", int doorId = -1, string description = "") : base(configItem) {
            DoorGroup = doorGroup;
            DoorId = doorId;
            LockIndex = lockIndex;
            Description = description;
            Dimension = dimension;
        }
    }
}
