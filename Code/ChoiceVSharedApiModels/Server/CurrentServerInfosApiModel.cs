using System.Text.Json.Serialization;

namespace ChoiceVSharedApiModels.Server;

public class CurrentServerInfosApiModel {
    public CurrentServerInfosApiModel(){}

    public CurrentServerInfosApiModel(
        int playerOnlineCount, 
        int overallAccountCount, 
        int whitelistedAccountsCount, 
        int bannedAccountsCount,
        int policeInDutyCount,
        int sheriffInDutyCount,
        int medicInDutyCount) {
        PlayerOnlineCount = playerOnlineCount;
        OverallAccountCount = overallAccountCount;
        WhitelistedAccountsCount = whitelistedAccountsCount;
        BannedAccountsCount = bannedAccountsCount;
        PoliceInDutyCount = policeInDutyCount;
        SheriffInDutyCount = sheriffInDutyCount;
        MedicInDutyCount = medicInDutyCount;
    }
    
    [JsonPropertyName("playerOnlineCount")]
    public int PlayerOnlineCount { get; set; }
    
    [JsonPropertyName("overallAccountCount")]
    public int OverallAccountCount { get; set; }
    
    [JsonPropertyName("whitelistedAccountsCount")]
    public int WhitelistedAccountsCount { get; set; }
    
    [JsonPropertyName("bannedAccountsCount")]
    public int BannedAccountsCount { get; set; }
    
    [JsonPropertyName("policeInDutyCount")]
    public int PoliceInDutyCount { get; set; }
    
    [JsonPropertyName("sheriffInDutyCount")]
    public int SheriffInDutyCount { get; set; }
    
    [JsonPropertyName("medicInDutyCount")]
    public int MedicInDutyCount { get; set; }
}
