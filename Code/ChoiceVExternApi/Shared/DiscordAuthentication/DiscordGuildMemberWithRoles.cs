namespace ChoiceVExternApi.Shared.DiscordAuthentication;

public class DiscordGuildMemberWithRoles
{
    public string? Avatar { get; set; }
    public string? Banner { get; set; }
    public DateTime? CommunicationDisabledUntil { get; set; }
    public int Flags { get; set; }
    public DateTime JoinedAt { get; set; }
    public string? Nick { get; set; }
    public bool Pending { get; set; }
    public DateTime? PremiumSince { get; set; }
    public DiscordUserModel User { get; set; }
    public bool Mute { get; set; }
    public bool Deaf { get; set; }
    public string Bio { get; set; }
    public List<string> RoleNames { get; set; } = new(); // Die Namen der Rollen
}