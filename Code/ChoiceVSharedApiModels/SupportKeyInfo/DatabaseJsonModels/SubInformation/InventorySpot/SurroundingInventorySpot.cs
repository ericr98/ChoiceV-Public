using System.Numerics;
using ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels.SubInformation.Shared;

namespace ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels.SubInformation.InventorySpot;

public record SurroundingInventorySpot(int Id, Position Position, InventoryInfo Inventory);