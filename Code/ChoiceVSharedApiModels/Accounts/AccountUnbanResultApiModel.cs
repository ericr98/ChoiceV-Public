using System.Text.Json.Serialization;

namespace ChoiceVSharedApiModels.Accounts;

public class AccountUnbanResultApiModel {
    
    public AccountUnbanResultApiModel(){}
    
    public AccountUnbanResultApiModel(bool isBanned) {
        IsUnbanned = isBanned;
    }
    
    [JsonPropertyName("isUnbanned")]
    public bool IsUnbanned { get; set; }
}
