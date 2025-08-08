using ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels.SubInformation.InventorySpot;
using ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels.SubInformation.Player;
using ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels.SubInformation.Vehicle;

namespace ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels;

public record SupportKeySurroundingInfo(
    List<SurroundingPlayerInfo> PlayerInfos, 
    List<SurroundingVehicleInfo> VehicleInfos, 
    List<SurroundingInventorySpot> Spots
);