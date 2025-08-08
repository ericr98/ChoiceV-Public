using System.Text.Json.Serialization;

namespace ChoiceVExternApi.Shared.DiscordAuthentication;

public class DiscordGuildMemberModel {
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("banner")]
    public string? Banner { get; set; }

    [JsonPropertyName("communication_disabled_until")]
    public DateTime? CommunicationDisabledUntil { get; set; }

    [JsonPropertyName("flags")]
    public int Flags { get; set; }

    [JsonPropertyName("joined_at")]
    public DateTime JoinedAt { get; set; }

    [JsonPropertyName("nick")]
    public string? Nick { get; set; }

    [JsonPropertyName("pending")]
    public bool Pending { get; set; }

    [JsonPropertyName("premium_since")]
    public DateTime? PremiumSince { get; set; }

    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = new();

    [JsonPropertyName("unusual_dm_activity_until")]
    public DateTime? UnusualDmActivityUntil { get; set; }

    [JsonPropertyName("user")]
    public DiscordUserModel User { get; set; }

    [JsonPropertyName("mute")]
    public bool Mute { get; set; }

    [JsonPropertyName("deaf")]
    public bool Deaf { get; set; }

    [JsonPropertyName("bio")]
    public string Bio { get; set; }
}
