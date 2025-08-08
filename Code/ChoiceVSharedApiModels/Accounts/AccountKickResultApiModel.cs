using System.Text.Json.Serialization;

namespace ChoiceVSharedApiModels.Accounts;

public class AccountKickResultApiModel {
    
    public AccountKickResultApiModel(){}
    
    public AccountKickResultApiModel(bool isKicked) {
        IsKicked = isKicked;
    }
    
    [JsonPropertyName("isKicked")]
    public bool IsKicked { get; set; }
}
