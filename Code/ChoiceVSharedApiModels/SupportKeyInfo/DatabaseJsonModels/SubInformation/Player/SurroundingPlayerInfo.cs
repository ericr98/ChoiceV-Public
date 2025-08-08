using System.Numerics;
using ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels.SubInformation.InventorySpot;
using ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels.SubInformation.Shared;

namespace ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels.SubInformation.Player;

public record SurroundingPlayerInfo(int CharId, Position Position, float DistanceToSender, int Health, InventoryInfo Inventory, ClothingInfo ClothingInfo);
