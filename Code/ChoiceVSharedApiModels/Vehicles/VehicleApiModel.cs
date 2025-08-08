using System.Text.Json.Serialization;

namespace ChoiceVSharedApiModels.Vehicles;

public class VehicleApiModel {
    public VehicleApiModel() {}
    
    public VehicleApiModel(int id, int modelId, int chassisNumber, string position, string rotation, int? garageId, int? registeredCompanyId, int dimension, DateTime lastMoved, string numberPlate, DateTime createDate, float fuel, int drivenDistance, int keyLockVersion, float dirtLevel, DateTime? randomlySpawnedDate) {
        Id = id;
        ModelId = modelId;
        ChassisNumber = chassisNumber;
        Position = position;
        Rotation = rotation;
        GarageId = garageId;
        RegisteredCompanyId = registeredCompanyId;
        Dimension = dimension;
        LastMoved = lastMoved;
        NumberPlate = numberPlate;
        CreateDate = createDate;
        Fuel = fuel;
        DrivenDistance = drivenDistance;
        KeyLockVersion = keyLockVersion;
        DirtLevel = dirtLevel;
        RandomlySpawnedDate = randomlySpawnedDate;
    }
    
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("modelId")]
    public int ModelId { get; set; }

    [JsonPropertyName("chassisNumber")]
    public int ChassisNumber { get; set; }

    [JsonPropertyName("position")]
    public string Position { get; set; }

    [JsonPropertyName("rotation")]
    public string Rotation { get; set; }

    [JsonPropertyName("garageId")]
    public int? GarageId { get; set; }

    [JsonPropertyName("registeredCompanyId")]
    public int? RegisteredCompanyId { get; set; }

    [JsonPropertyName("dimension")]
    public int Dimension { get; set; }

    [JsonPropertyName("lastMoved")]
    public DateTime LastMoved { get; set; }

    [JsonPropertyName("numberPlate")]
    public string NumberPlate { get; set; }

    [JsonPropertyName("createDate")]
    public DateTime CreateDate { get; set; }

    [JsonPropertyName("fuel")]
    public float Fuel { get; set; }

    [JsonPropertyName("drivenDistance")]
    public int DrivenDistance { get; set; }

    [JsonPropertyName("keyLockVersion")]
    public int KeyLockVersion { get; set; }

    [JsonPropertyName("dirtLevel")]
    public float DirtLevel { get; set; }

    [JsonPropertyName("randomlySpawnedDate")]
    public DateTime? RandomlySpawnedDate { get; set; }
    
    [JsonPropertyName("config")]
    public ConfigVehicleApiModel? Config { get; set; }
    
    [JsonPropertyName("coloring")]
    public VehicleColoringApiModel? Coloring { get; set; }

    [JsonPropertyName("registrations")]
    public List<VehicleRegistrationApiModel> Registrations { get; set; } = [];
}

