using System.Text.Json.Serialization;

namespace ChoiceVSharedApiModels.Accounts;

public class AccountApiModel
{
    public AccountApiModel() {}

    public AccountApiModel(
        int id, 
        string name, 
        string socialClubName, 
        string discordId, 
        string teamspeakId, 
        DateTime? lastLogin, 
        AccountStateApiEnum state, 
        string? stateReason, 
        int failedLogins, 
        int adminLevel, 
        int charAmount, 
        int strikes,
        int flag, 
        bool hasLightmodeFlag,
        bool isCurrentlyOnline)
    {
        Id = id;
        Name = name;
        SocialClubName = socialClubName;
        DiscordId = discordId;
        TeamspeakId = teamspeakId;
        LastLogin = lastLogin;
        State = state;
        StateReason = stateReason;
        FailedLogins = failedLogins;
        AdminLevel = adminLevel;
        CharAmount = charAmount;
        Strikes = strikes;
        Flag = flag;
        HasLightmodeFlag = hasLightmodeFlag;
        IsCurrentlyOnline = isCurrentlyOnline;
    }

    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("socialClubName")] 
    public string SocialClubName { get; set; } = null!;

    [JsonPropertyName("discordId")] 
    public string DiscordId { get; set; } = null!;
    
    [JsonPropertyName("teamspeakId")] 
    public string TeamspeakId { get; set; }
    
    [JsonPropertyName("lastLogin")]
    public DateTime? LastLogin { get; set; }

    [JsonPropertyName("state")]
    public AccountStateApiEnum State { get; set; }

    [JsonPropertyName("stateReason")]
    public string? StateReason { get; set; }
    
    [JsonPropertyName("failedLogins")] 
    public int FailedLogins { get; set; }
    
    [JsonPropertyName("adminLevel")] 
    public int AdminLevel { get; set; }
    
    [JsonPropertyName("charAmount")] 
    public int CharAmount { get; set; }
    
    [JsonPropertyName("strikes")] 
    public int Strikes { get; set; }
    
    [JsonPropertyName("flag")] 
    public int Flag { get; set; }
    
    [JsonPropertyName("hasLightmodeFlag")] 
    public bool HasLightmodeFlag { get; set; }
    
    [JsonPropertyName("isCurrentlyOnline")]
    public bool IsCurrentlyOnline { get; set; }
}