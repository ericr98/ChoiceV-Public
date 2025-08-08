using System.Text.Json.Serialization;

namespace ChoiceVSharedApiModels.BankAccounts;

public class BankBankWithdrawApiModel {

    public BankBankWithdrawApiModel()
    {
    }
    
    public BankBankWithdrawApiModel(int id, long fromBankAccountId, decimal amount, string reason, DateTime date) {
        Id = id;
        FromBankAccountId = fromBankAccountId;
        Amount = amount;
        Reason = reason;
        Date = date;
    }
    
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("fromBankAccountId")]
    public long FromBankAccountId { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }
}
