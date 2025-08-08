using System.Numerics;
using System.Text.Json.Serialization;
using ChoiceVSharedApiModels.BankAccounts.Enums;

namespace ChoiceVSharedApiModels.BankAccounts;

public class AtmApiModel {
    
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("position")]
    public Vector3 Position { get; set; }

    [JsonPropertyName("width")]
    public float Width { get; set; }

    [JsonPropertyName("height")]
    public float Height { get; set; }

    [JsonPropertyName("rotation")]
    public float Rotation { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; }

    [JsonPropertyName("showBlip")]
    public bool ShowBlip { get; set; }

    [JsonPropertyName("company")]
    public BankCompaniesApiEnum Company { get; set; }

    [JsonPropertyName("deposit")]
    public decimal Deposit { get; set; }

    [JsonPropertyName("lastHeistDate")]
    public DateTime? LastHeistDate { get; set; }
}
