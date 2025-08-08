using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.OrderSystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.InventorySystem;

public class VehicleRepairItem : Item {
    public int VehicleClass {
        get => (int)Data["VehicleClass"];
        set => Data["VehicleClass"] = value;
    }

    public VehicleDamagePartType RepairType {
        get => (VehicleDamagePartType)Data["RepairType"];
        set => Data["RepairType"] = value;
    }

    public VehicleRepairItem(item item) : base(item) { }

    public VehicleRepairItem(configitem configItem, int amount, int quality) : base(configItem, quality) {
        VehicleClass = -1;
        RepairType = (VehicleDamagePartType)int.Parse(configItem.additionalInfo);
    }

    public VehicleRepairItem(configitem configItem, int vehicleClass) : base(configItem) {
        VehicleClass = vehicleClass;
        RepairType = (VehicleDamagePartType)int.Parse(configItem.additionalInfo);

        updateDescription();
    }

    public UseableMenuItem getRepairItemMenuItem(string itemEvent = "VEHICLE_REPAIR_PART") {
        return new ClickMenuItem(Name + " benutzen", $"Benutze {Name} für {VehicleController.getVehicleClassName(VehicleClass)} zur Reparatur", "", itemEvent);
    }

    public override void updateDescription() {
        Description = $"{Name} Ersatzteil für {VehicleController.getVehicleClassName(VehicleClass)}";
        base.updateDescription();
    }

    public override void setOrderData(OrderItem orderItem) {
        var vehicleSpecificItem = (VehicleSpecificOrderItem)orderItem;
        VehicleClass = vehicleSpecificItem.VehicleClass;
    }
}