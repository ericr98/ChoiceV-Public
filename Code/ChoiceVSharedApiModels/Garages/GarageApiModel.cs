using System.Text.Json.Serialization;
using ChoiceVSharedApiModels.Garages.Enums;

namespace ChoiceVSharedApiModels.Garages;

public class GarageApiModel
{
    public GarageApiModel() {}

    public GarageApiModel(int id, string name, string position, float rotation, GarageTypeApiEnum type, GarageOwnerTypeApiEnum ownerType, int ownerId, int slots) {
        Id = id;
        Name = name;
        Position = position;
        Rotation = rotation;
        Type = type;
        OwnerType = ownerType;
        OwnerId = ownerId;
        Slots = slots;
    }
    
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("position")]
    public string Position { get; set; }

    [JsonPropertyName("rotation")]
    public float Rotation { get; set; }

    [JsonPropertyName("type")]
    public GarageTypeApiEnum Type { get; set; }

    [JsonPropertyName("ownerType")]
    public GarageOwnerTypeApiEnum OwnerType { get; set; }

    [JsonPropertyName("ownerId")]
    public int OwnerId { get; set; }

    [JsonPropertyName("slots")]
    public int Slots { get; set; }
}
