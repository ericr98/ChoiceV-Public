using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.InventorySystem;

public class ModKitItem : Item {
    public ModKitItem(item item) : base(item) { }

    public ModKitItem(configitem configItem, int vehicleClass) : base(configItem) {
        VehicleClass = vehicleClass;
        Level = 1;

        updateDescription();
    }

    public int VehicleClass {
        get => (int)Data["VehicleClass"];
        set => Data["VehicleClass"] = value;
    }

    public int Level {
        get => (int)Data["Level"];
        set => Data["Level"] = value;
    }

    public void buildOnLevel(int level) {
        Level = level;
    }

    public override void updateDescription() {
        Description = $"{Name} ModKit Level {Level} für {VehicleController.getVehicleClassName(VehicleClass)}";
        base.updateDescription();
    }
}