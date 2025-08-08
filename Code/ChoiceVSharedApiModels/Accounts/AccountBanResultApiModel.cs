using System.Text.Json.Serialization;

namespace ChoiceVSharedApiModels.Accounts;

public class AccountBanResultApiModel {
    public AccountBanResultApiModel(){}
    
    public AccountBanResultApiModel(bool isBanned, bool isKicked) {
        IsBanned = isBanned;
        IsKicked = isKicked;
    }
    
    [JsonPropertyName("isBanned")]
    public bool IsBanned { get; set; }
    
    [JsonPropertyName("isKicked")]
    public bool IsKicked { get; set; }
}
