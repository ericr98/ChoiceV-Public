using System.Text.Json.Serialization;

namespace ChoiceVExternApi.Shared.DiscordAuthentication;

public class DiscordGuildModel {
    [JsonPropertyName("roles")]
    public List<DiscordRoleModel> Roles { get; set; } = new();

    public List<string> getRoleNames(DiscordGuildMemberModel member)
    {
        var roleNames = new List<string>();

        foreach (var memberRoleId in member.Roles)
        {
            var role = Roles.FirstOrDefault(r => r.Id == memberRoleId);
            if (role != null)
            {
                roleNames.Add(role.Name);
            }
        }

        return roleNames;
    }
}
