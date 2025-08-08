using AltV.Net.Enums;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.OrderSystem;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.InventorySystem;

public class VehicleTuningItem : Item {
    private bool HasBeenStolen {get => Data["HasBeenStolen"]; set => Data["HasBeenStolen"] = value; }
    
    public VehicleTuningItem(item item) : base(item) { }

    public VehicleTuningItem(configitem configItem, int amount, int quality) : base(configItem, quality) {
        VehicleClass = -1;
        ModIndex = null;
    }

    public VehicleTuningItem(configitem configItem, int vehicleClass, string modName, int? modelId, bool hasBeenStolen = false) : base(configItem) {
        VehicleClass = vehicleClass;
        ModName = modName;
        ModIndex = null;
        Model = modelId;

        if(hasBeenStolen) {
            HasBeenStolen = true;
        }

        updateDescription();
    }

    public override void processAdditionalInfo(string info) {
        var split = info.Split('#');
        ModType = (VehicleModType)int.Parse(split[0]);
        BuildInToolFlag = (SpecialToolFlag)int.Parse(split[1]);
    }

    public static VehicleModType getModTypeForConfigItem(configitem cfg) {
        var split = cfg.additionalInfo.Split('#');
        return (VehicleModType)int.Parse(split[0]);
    }
    
    public static SpecialToolFlag getToolFlagForConfigItem(configitem cfg) {
        var split = cfg.additionalInfo.Split('#');
        return (SpecialToolFlag)int.Parse(split[1]);
    }

    public int VehicleClass { get => (int)Data["VehicleClass"]; set => Data["VehicleClass"] = value; }
    public int? Model { get => (int?)Data["Model"]; set => Data["Model"] = value; }
    public int? ModIndex { get => (int?)Data["ModIndex"]; set => Data["ModIndex"] = value; }
    public string ModName { get => (string)Data["ModName"]; set => Data["ModName"] = value; }
    public VehicleModType ModType { get => (VehicleModType)Data["ModType"]; set => Data["ModType"] = value; }

    public SpecialToolFlag BuildInToolFlag;

    public sealed override void updateDescription() {
        Description = string.IsNullOrWhiteSpace(ModName)
            ? $"{Name} für {VehicleController.getVehicleClassName(VehicleClass)}"
            : $"{ModName} für {VehicleController.getVehicleModelById((Model ?? -1)).DisplayName}";

        base.updateDescription();
    }

    public override void setOrderData(OrderItem orderItem) {
        var vehicleSpecificItem = (VehicleSpecificOrderItem)orderItem;
        VehicleClass = vehicleSpecificItem.VehicleClass;
    }
}