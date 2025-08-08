using System.Text.Json.Serialization;

namespace ChoiceVSharedApiModels.BankAccounts;

public class BankTransactionApiModel {

    public BankTransactionApiModel()
    {
    }

    public BankTransactionApiModel(int id, long? fromBankAccountId, long toBankAccountId, decimal amount, string message, decimal cost, DateTime date, DateTime due, bool isPending) {
        Id = id;
        FromBankAccountId = fromBankAccountId;
        ToBankAccountId = toBankAccountId;
        Amount = amount;
        Message = message;
        Cost = cost;
        Date = date;
        Due = due;
        IsPending = isPending;
    }
    
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("fromBankAccountId")]
    public long? FromBankAccountId { get; set; }

    [JsonPropertyName("toBankAccountId")]
    public long ToBankAccountId { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("cost")]
    public decimal Cost { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("due")]
    public DateTime Due { get; set; }
    
    [JsonPropertyName("isPending")]
    public bool IsPending { get; set; }
}
