using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using ChoiceVExternApi.Shared.Attributes;

namespace ChoiceVExternApi.Shared.DiscordAuthentication;

public static class DiscordVerificationService {
    private static readonly HttpClient HttpClient = new HttpClient();

    public static async Task<DiscordUserModel?> verifyDiscordUserAsync(this HttpListenerContext context) {
        var accessToken = context.Request.Headers["X"];
        if(string.IsNullOrEmpty(accessToken)) return null;

        var request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await HttpClient.SendAsync(request);

        if(!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        var discordUser = JsonSerializer.Deserialize<DiscordUserModel>(content);

        return discordUser;
    }

    public static async Task<DiscordGuildMemberModel?> getGuildMemberAsync(this HttpListenerContext context, ulong guildId) {
        var accessToken = context.Request.Headers["X"];
        if(string.IsNullOrEmpty(accessToken)) return null;

        var request = new HttpRequestMessage(HttpMethod.Get, $"https://discord.com/api/v10/users/@me/guilds/{guildId}/member");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await HttpClient.SendAsync(request);

        if(!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        var discordGuildMember = JsonSerializer.Deserialize<DiscordGuildMemberModel>(content);

        return discordGuildMember;
    }

    public static async Task<List<DiscordRoleModel>> getGuildRolesAsync(ulong guildId, string botToken) {
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://discord.com/api/v10/guilds/{guildId}/roles");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bot", botToken);

        var response = await HttpClient.SendAsync(request);

        if(!response.IsSuccessStatusCode) return [];

        var json = await response.Content.ReadAsStringAsync();
        var roles = JsonSerializer.Deserialize<List<DiscordRoleModel>>(json);

        return roles ?? [];
    }

    public static async Task<DiscordGuildMemberWithRoles?> getGuildMemberWithRoleNamesAsync(this HttpListenerContext context, ulong guildId, string botToken) {
        var guildMember = await context.getGuildMemberAsync(guildId);
        if(guildMember is null) return null;

        var guildRoles = await getGuildRolesAsync(guildId, botToken);

        var roleNames = guildMember.Roles
            .Select(roleId => guildRoles.FirstOrDefault(role => role.Id == roleId)?.Name)
            .Where(roleName => roleName != null)
            .ToList();

        return new DiscordGuildMemberWithRoles {
            User = guildMember.User,
            Avatar = guildMember.Avatar,
            Banner = guildMember.Banner,
            CommunicationDisabledUntil = guildMember.CommunicationDisabledUntil,
            Flags = guildMember.Flags,
            JoinedAt = guildMember.JoinedAt,
            Nick = guildMember.Nick,
            Pending = guildMember.Pending,
            PremiumSince = guildMember.PremiumSince,
            Mute = guildMember.Mute,
            Deaf = guildMember.Deaf,
            Bio = guildMember.Bio,
            RoleNames = roleNames
        };
    }

    public static int getAdminLevelByDiscordId(this DiscordGuildMemberWithRoles guildMember, bool isDevServer) {
        if(isDevServer) {
            return (int)AdminLevelEnum.Rank4;
        }

        var regex = new Regex(@"\(([1-9]+)\) .*");
        var permissionRoleName = guildMember.RoleNames.FirstOrDefault(r => regex.IsMatch(r));

        if(permissionRoleName == null) return 0;

        var match = regex.Match(permissionRoleName);
        return int.Parse(match.Groups[1].Value);
    }
}
