using System.Text.Json.Serialization;

namespace ChoiceVSharedApiModels.Vehicles;

public class VehicleColoringApiModel
{
    public VehicleColoringApiModel(){}
    
    public VehicleColoringApiModel(int vehicleId, int primaryColor, int secondaryColor, string primaryColorRgb, string secondaryColorRgb, int pearlColor) {
        VehicleId = vehicleId;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        PrimaryColorRGB = primaryColorRgb;
        SecondaryColorRGB = secondaryColorRgb;
        PearlColor = pearlColor;
    }

    [JsonPropertyName("vehicleId")]
    public int VehicleId { get; set; }

    [JsonPropertyName("primaryColor")]
    public int PrimaryColor { get; set; }

    [JsonPropertyName("secondaryColor")]
    public int SecondaryColor { get; set; }

    [JsonPropertyName("primaryColorRGB")]
    public string PrimaryColorRGB { get; set; }

    [JsonPropertyName("secondaryColorRGB")]
    public string SecondaryColorRGB { get; set; }

    [JsonPropertyName("pearlColor")]
    public int PearlColor { get; set; }
}
