using System.Text.Json.Serialization;

namespace ChoiceVSharedApiModels.Inventory;

public class InventoryItemApiModel
{
    public InventoryItemApiModel() {}
    
    public InventoryItemApiModel(int id, string name, string description, int quantity)
    {
        Id = id;
        Name = name;
        Description = description;
        Quantity = quantity;
    }

    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}