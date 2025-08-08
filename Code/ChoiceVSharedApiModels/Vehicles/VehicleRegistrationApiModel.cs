using System.Text.Json.Serialization;

namespace ChoiceVSharedApiModels.Vehicles;

public class VehicleRegistrationApiModel {
    public VehicleRegistrationApiModel(){}

    public VehicleRegistrationApiModel(int id, int vehicleId, int? ownerId, int? companyOwnerId, string numberPlate, DateTime start, DateTime? end) {
        Id = id;
        VehicleId = vehicleId;
        OwnerId = ownerId;
        CompanyOwnerId = companyOwnerId;
        NumberPlate = numberPlate;
        Start = start;
        End = end;
    }
    
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("vehicleId")]
    public int VehicleId { get; set; }

    [JsonPropertyName("ownerId")]
    public int? OwnerId { get; set; }

    [JsonPropertyName("companyOwnerId")]
    public int? CompanyOwnerId { get; set; }

    [JsonPropertyName("numberPlate")]
    public string NumberPlate { get; set; }

    [JsonPropertyName("start")]
    public DateTime Start { get; set; }

    [JsonPropertyName("end")]
    public DateTime? End { get; set; }
}
