using System.Text.Json.Serialization;

namespace ChoiceVSharedApiModels.BankAccounts;

public class BankAccountInterestApiModel {
    
    public BankAccountInterestApiModel() {}
    
    public BankAccountInterestApiModel(long bankAccountId, float interestPercent, decimal interestAmount, DateTime nextInterest) {
        BankAccountId = bankAccountId;
        InterestPercent = interestPercent;
        InterestAmount = interestAmount;
        NextInterest = nextInterest;
    }
    
    [JsonPropertyName("bankAccountId")]
    public long BankAccountId { get; set; }

    [JsonPropertyName("interestPercent")]
    public float InterestPercent { get; set; }

    [JsonPropertyName("interestAmount")]
    public decimal InterestAmount { get; set; }

    [JsonPropertyName("nextInterest")]
    public DateTime NextInterest { get; set; }
}
