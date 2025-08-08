using System.Text.Json.Serialization;
using ChoiceVSharedApiModels.BankAccounts.Enums;

namespace ChoiceVSharedApiModels.BankAccounts;

public class BankAccountApiModel {
    public BankAccountApiModel() { }
    
    public BankAccountApiModel(
        long id, 
        BankCompaniesApiEnum bankCompany,
        BankAccountTypeApiEnum accountType, 
        string name, 
        decimal balance,
        BankAccountOwnerTypeApiEnum ownerType,
        string ownerValue, 
        int pin,
        bool isFrozen, 
        DateTime creationDate,
        bool isDeactivated,
        long? connectedPhoneNumber, 
        bool isInfinite) 
    {
        Id = id;
        BankCompany = bankCompany;
        AccountType = accountType;
        Name = name;
        Balance = balance;
        OwnerType = ownerType;
        OwnerValue = ownerValue;
        Pin = pin;
        IsFrozen = isFrozen;
        CreationDate = creationDate;
        IsDeactivated = isDeactivated;
        ConnectedPhoneNumber = connectedPhoneNumber;
        IsInfinite = isInfinite;
    }
    
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("bankCompany")]
    public BankCompaniesApiEnum BankCompany { get; set; }

    [JsonPropertyName("accountType")]
    public BankAccountTypeApiEnum AccountType { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("balance")]
    public decimal Balance { get; set; }

    [JsonPropertyName("ownerType")]
    public BankAccountOwnerTypeApiEnum OwnerType { get; set; }

    [JsonPropertyName("ownerValue")]
    public string OwnerValue { get; set; }

    [JsonPropertyName("pin")]
    public int Pin { get; set; }

    [JsonPropertyName("isFrozen")]
    public bool IsFrozen { get; set; }

    [JsonPropertyName("creationDate")]
    public DateTime CreationDate { get; set; }

    [JsonPropertyName("isDeactivated")]
    public bool IsDeactivated { get; set; }

    [JsonPropertyName("connectedPhoneNumber")]
    public long? ConnectedPhoneNumber { get; set; }

    [JsonPropertyName("isInfinite")]
    public bool IsInfinite { get; set; }
    
    [JsonPropertyName("interestModel")]
    public BankAccountInterestApiModel? InterestModel { get; set; }

    [JsonPropertyName("transactions")]
    public List<BankTransactionApiModel> Transactions { get; set; } = [];
    
    [JsonPropertyName("atmWithdraws")]
    public List<BankAtmWithdrawApiModel> AtmWithdraws { get; set; } = [];
    
    [JsonPropertyName("bankWithdraws")]
    public List<BankBankWithdrawApiModel> BankWithdraws { get; set; } = [];
}
