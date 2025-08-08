using System.Text.Json.Serialization;

namespace ChoiceVExternApi.Shared.DiscordAuthentication;

public class DiscordUserModel {
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("discriminator")]
    public string Discriminator { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("public_flags")]
    public int PublicFlags { get; set; }

    [JsonPropertyName("flags")]
    public int Flags { get; set; }

    [JsonPropertyName("banner")]
    public string? Banner { get; set; }

    [JsonPropertyName("accent_color")]
    public int? AccentColor { get; set; }

    [JsonPropertyName("global_name")]
    public string? GlobalName { get; set; }

    [JsonPropertyName("avatar_decoration_data")]
    public string? AvatarDecorationData { get; set; }

    [JsonPropertyName("banner_color")]
    public string? BannerColor { get; set; }

    [JsonPropertyName("clan")]
    public string? Clan { get; set; }
}
