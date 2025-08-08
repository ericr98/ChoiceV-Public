using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem.Vehicle {
    public class VehicleMotorRepairItem : Item {
        private bool OnlyRepair;
        private List<string> RepairableIdentifiers;

        public VehicleMotorRepairItem(item item) : base(item) { }

        public VehicleMotorRepairItem(configitem configItem, int amount, int quality) : base(configItem, quality) {

        }

        public override void processAdditionalInfo(string info) {
            var split = info.Split("#");

            OnlyRepair = bool.Parse(split[0]);
            RepairableIdentifiers = split[0].Split(",").ToList();
        }
    }
}
