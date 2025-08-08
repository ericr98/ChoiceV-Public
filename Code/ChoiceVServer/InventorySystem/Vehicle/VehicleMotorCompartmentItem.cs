using ChoiceVServer.Controller.Vehicles;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.InventorySystem {

    public class VehicleMotorCompartmentItem : StaticItem {
        public VehicleMotorCompartmentRepairType RepairType;

        public VehicleMotorCompartmentItem(item item) : base(item) { }

        public VehicleMotorCompartmentItem(configitem configItem, int amount, int quality) : base(configItem, amount, quality) {
            updateDescription();
        }

        public override void processAdditionalInfo(string info) {
            RepairType = (VehicleMotorCompartmentRepairType)int.Parse(info);
        }
    }
}
