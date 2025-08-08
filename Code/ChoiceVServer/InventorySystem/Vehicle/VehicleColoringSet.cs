using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.OrderSystem;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.InventorySystem;

public class VehicleColoringSet : Item {
    public enum VehicleColorType {
        None = -1,
        Metallic = 0,
        Matt = 1,
        Brushed = 2,
        Spezial = 3,
        Util = 4,
        Worn = 5,
    }

    public VehicleColorType ColorType {
        get => (VehicleColorType)Data["ColorType"];
        set => Data["ColorType"] = value;
    }

    public int GTAColor {
        get => (int)Data["GTAColor"];
        set => Data["GTAColor"] = value;
    }

    public VehicleColoringSet(item item) : base(item) { }

    public VehicleColoringSet(configitem configItem, int amount, int quality) : base(configItem, quality) {
        ColorType = VehicleColorType.None;
        GTAColor = -1;
    }

    public VehicleColoringSet(configitem configItem, VehicleColorType colorType) : base(configItem) {
        ColorType = colorType;
        GTAColor = -1;

        updateDescription();
    }

    public override void updateDescription() {
        var gtaColor = "";
        if(GTAColor != -1) {
            gtaColor = "in Farbe ";
            gtaColor += VehicleColoringController.getVehicleColorById(GTAColor).name;
        }

        Description = $"{Name} vom Typ {ColorType} {gtaColor}";
        base.updateDescription();
    }

    public override void setOrderData(OrderItem orderItem) {
        ColorType = (VehicleColorType)int.Parse(orderItem.OrderOption);

        updateDescription();
    }
}