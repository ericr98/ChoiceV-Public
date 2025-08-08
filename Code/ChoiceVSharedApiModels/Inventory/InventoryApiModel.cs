using System.Text.Json.Serialization;

namespace ChoiceVSharedApiModels.Inventory;

public class InventoryApiModel
{
    [JsonPropertyName("characterId")]
    public int CharacterId { get; set; }
    
    [JsonPropertyName("items")]
    public List<InventoryItemApiModel> Items { get; set; }
}