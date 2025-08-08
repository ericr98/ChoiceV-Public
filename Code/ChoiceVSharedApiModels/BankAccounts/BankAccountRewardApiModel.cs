using System.Text.Json.Serialization;

namespace ChoiceVSharedApiModels.BankAccounts;

public class BankAccountRewardApiModel {
    
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("bankAccountId")]
    public long BankAccountId { get; set; }

    [JsonPropertyName("codeReward")]
    public string CodeReward { get; set; }

    [JsonPropertyName("data")]
    public string Data { get; set; }
}
