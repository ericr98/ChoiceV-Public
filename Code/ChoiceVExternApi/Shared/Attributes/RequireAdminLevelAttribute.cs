#nullable enable
using System.Net;
using System.Reflection;
using ChoiceVExternApi.Shared.DiscordAuthentication;

namespace ChoiceVExternApi.Shared.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class RequireAdminLevelAttribute(AdminLevelEnum requiredLevel) : Attribute {
    public AdminLevelEnum RequiredLevel { get; } = requiredLevel;
}
/// <summary>
/// Provides extension methods for <see cref="RequireAdminLevelAttribute"/>.
/// </summary>
public static class RequireAdminLevelAttributeExtensions {
    /// <summary>
    /// Checks if the user has the required admin level to access the method.
    /// </summary>
    /// <param name="handler">The handler function that requires admin level check.</param>
    /// <param name="context">The HTTP listener context containing user information.</param>
    /// <param name="guildId"></param>
    /// <param name="botToken"></param>
    /// <param name="isDevServer"></param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the user meets the required level; otherwise, false.</returns>
    public static async Task<bool> checkRequireAdminLevelAttribute(
        this Func<HttpListenerContext, string[], Task> handler,
        HttpListenerContext context,
        ulong? guildId,
        string? botToken,
        bool isDevServer) {

        var attribute = getRequireAdminLevelAttribute(handler);
        if(attribute == null) return true;

        if(guildId is null) return false;
        if(botToken is null) return false;

        var discordUser = await context.verifyDiscordUserAsync().ConfigureAwait(false);
        if(discordUser == null || !ulong.TryParse(discordUser.Id, out var discordId)) {
            return false;
        }

        var guildMember = await context.getGuildMemberWithRoleNamesAsync(guildId.Value, botToken).ConfigureAwait(false);
        if(guildMember is null) {
            return false;
        }

        var adminLevel = guildMember.getAdminLevelByDiscordId(isDevServer);
        return adminLevel >= (int)attribute.RequiredLevel;
    }

    /// <summary>
    /// Retrieves the <see cref="RequireAdminLevelAttribute"/> from the handler method.
    /// </summary>
    /// <param name="handler">The handler function.</param>
    /// <returns>The associated attribute or null if not found.</returns>
    private static RequireAdminLevelAttribute? getRequireAdminLevelAttribute(Func<HttpListenerContext, string[], Task> handler) {
        var attributes = handler.GetMethodInfo().GetCustomAttributes(typeof(RequireAdminLevelAttribute), false);
        return attributes.Length == 1 ? attributes[0] as RequireAdminLevelAttribute : null;
    }
}
public enum AdminLevelEnum {
    Player,
    Rank1,
    Rank2,
    Rank3,
    Rank4,
}
