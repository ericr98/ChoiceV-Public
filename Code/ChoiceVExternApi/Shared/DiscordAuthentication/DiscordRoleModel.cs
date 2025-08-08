using System.Text.Json.Serialization;

namespace ChoiceVExternApi.Shared.DiscordAuthentication;

public class DiscordRoleModel {
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
