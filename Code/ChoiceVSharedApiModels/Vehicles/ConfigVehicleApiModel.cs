namespace ChoiceVSharedApiModels.Vehicles;

using System.Text.Json.Serialization;

public class ConfigVehicleApiModel {
    public ConfigVehicleApiModel() { }

    public ConfigVehicleApiModel(int id, DateTime createDate, int model, string modelName, string displayName, int classId, long specialFlag, int seats, int inventorySize, float fuelMax, int fuelType, string startPoint, string endPoint, float powerMultiplier, string extras, int windowCount, int tyreCount, int doorCount, decimal price, int trunkInBack, int useable, string producerName, int needsRecheck, int hasNumberplate) {
        Id = id;
        CreateDate = createDate;
        Model = model;
        ModelName = modelName;
        DisplayName = displayName;
        ClassId = classId;
        SpecialFlag = specialFlag;
        Seats = seats;
        InventorySize = inventorySize;
        FuelMax = fuelMax;
        FuelType = fuelType;
        StartPoint = startPoint;
        EndPoint = endPoint;
        PowerMultiplier = powerMultiplier;
        Extras = extras;
        WindowCount = windowCount;
        TyreCount = tyreCount;
        DoorCount = doorCount;
        Price = price;
        TrunkInBack = trunkInBack;
        Useable = useable;
        ProducerName = producerName;
        NeedsRecheck = needsRecheck;
        HasNumberplate = hasNumberplate;
    }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("createDate")]
    public DateTime CreateDate { get; set; }

    [JsonPropertyName("model")]
    public int Model { get; set; }

    [JsonPropertyName("modelName")]
    public string ModelName { get; set; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("classId")]
    public int ClassId { get; set; }

    [JsonPropertyName("specialFlag")]
    public long SpecialFlag { get; set; }

    [JsonPropertyName("seats")]
    public int Seats { get; set; }

    [JsonPropertyName("inventorySize")]
    public int InventorySize { get; set; }

    [JsonPropertyName("fuelMax")]
    public float FuelMax { get; set; }

    [JsonPropertyName("fuelType")]
    public int FuelType { get; set; }

    [JsonPropertyName("startPoint")]
    public string StartPoint { get; set; }

    [JsonPropertyName("endPoint")]
    public string EndPoint { get; set; }

    [JsonPropertyName("powerMultiplier")]
    public float PowerMultiplier { get; set; }

    [JsonPropertyName("extras")]
    public string Extras { get; set; }

    [JsonPropertyName("windowCount")]
    public int WindowCount { get; set; }

    [JsonPropertyName("tyreCount")]
    public int TyreCount { get; set; }

    [JsonPropertyName("doorCount")]
    public int DoorCount { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("trunkInBack")]
    public int TrunkInBack { get; set; }

    [JsonPropertyName("useable")]
    public int Useable { get; set; }

    [JsonPropertyName("producerName")]
    public string ProducerName { get; set; }

    [JsonPropertyName("needsRecheck")]
    public int NeedsRecheck { get; set; }

    [JsonPropertyName("hasNumberplate")]
    public int HasNumberplate { get; set; }
}
