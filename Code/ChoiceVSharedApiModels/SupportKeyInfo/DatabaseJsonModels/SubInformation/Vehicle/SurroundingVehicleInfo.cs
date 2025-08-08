using System.Numerics;
using ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels.SubInformation.InventorySpot;
using ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels.SubInformation.Shared;

namespace ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels.SubInformation.Vehicle;

public record SurroundingVehicleInfo(int DbId, int ModelId, Position Position, float CurrentSpeed, List<PassengerInfo> Passengers, InventoryInfo Inventory);

