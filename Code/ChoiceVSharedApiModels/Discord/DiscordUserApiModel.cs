namespace ChoiceVSharedApiModels.Discord;

public class DiscordUserApiModel {
    public DiscordUserApiModel(){}

    public DiscordUserApiModel(ulong discordId, string username) {
        DiscordId = discordId;
        Username = username;
    }
    public ulong DiscordId { get; set; }
    public string Username { get; set; }
}
