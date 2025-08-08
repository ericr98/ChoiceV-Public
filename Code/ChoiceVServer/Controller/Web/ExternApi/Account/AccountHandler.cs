#nullable enable
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ChoiceVExternApi;
using ChoiceVExternApi.Shared;
using ChoiceVExternApi.Shared.Attributes;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;
using ChoiceVSharedApiModels.Accounts;

namespace ChoiceVServer.Controller.Web.ExternApi.Account;

public class AccountHandler : IBasicExternApiHandler {
    private const string Prefix = "account";

    public void registerRoutes(RouteHandler routeHandler) {
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetAllRoute);
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetByAccountIdRoute);
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetByDiscordIdRoute);
        routeHandler.registerRoute(HttpMethod.Put, Prefix + "/ban", handlePutBanRoute);
        routeHandler.registerRoute(HttpMethod.Put, Prefix + "/unban", handlePutUnbanRoute);
        routeHandler.registerRoute(HttpMethod.Put, Prefix + "/kick", handlePutKickRoute);
        routeHandler.registerRoute(HttpMethod.Put, Prefix + "/lightmode/remove", handlePutRemoveLightmodeRoute);
        routeHandler.registerRoute(HttpMethod.Post, Prefix + "/add", handlePutAddRoute);
    }

    [RequireAdminLevel(AdminLevelEnum.Rank3)]
    [RequiredQueryParams("accountId")]
    private static async Task handlePutRemoveLightmodeRoute(HttpListenerContext context, string[] segments) {
        int? accountId = context.getQueryParameter<int>("accountId");
        
        if(!accountId.HasValue) {
            await context.sendResponseAsync("Invalid accountId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }
        
        await using var db = new ChoiceVDb();
        var account = await AccountHelper.getByIdAsync(accountId.Value, db);
        if(account == null) {
            await context.sendResponseAsync("Account not found.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        if(!((AccountFlag)account.flag).HasFlag(AccountFlag.LiteModeActivated)) {
            await context.sendResponseAsync("Account has no LiteModeActivated-Flag.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        account.flag ^= (int)AccountFlag.LiteModeActivated;
        
        db.Update(account);
        var changes = await db.SaveChangesAsync();
        if(changes == 0) {
            await context.sendResponseAsync("Failed to update the database.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }
        
        await context.sendResponseAsync(account).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the route to add a new account
    /// Requires query parameters "socialClubName" and "discordId".
    /// Route: POST /account/add?socialClubName={socialClubName}&discordId={discordId}
    /// </summary>
    [RequireAdminLevel(AdminLevelEnum.Rank2)]
    [RequiredQueryParams("socialClubName", "discordId")]
    private static async Task handlePutAddRoute(HttpListenerContext context, string[] segments) {
        var discordId = context.getQueryParameter("discordId");
        var socialClubName = context.getQueryParameter("socialClubName");

        if(discordId is null) {
            await context.sendResponseAsync("Invalid discordId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }
        if(socialClubName is null) {
            await context.sendResponseAsync("Invalid socialClubName.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }
        
        var account = await AccountHelper.addAsync(socialClubName, discordId).ConfigureAwait(false);
        if(account is null) {
            await context.sendResponseAsync("SocialClubName already exists.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }
        
        await context.sendResponseAsync(account).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Handles the route to kick a user by account ID.
    /// Requires query parameters "accountId" and "message".
    /// Route: PUT /account/kick?accountId={accountId}&message={message}
    /// </summary>
    [RequireAdminLevel(AdminLevelEnum.Rank2)]
    [RequiredQueryParams("accountId", "message")]
    private static async Task handlePutKickRoute(HttpListenerContext context, string[] segments) {
        int? accountId = context.getQueryParameter<int>("accountId");
        var message = context.getQueryParameter("message")!;
        
        if(!accountId.HasValue) {
            await context.sendResponseAsync("Invalid accountId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }
        
        message = Uri.UnescapeDataString(message);

        var player = ChoiceVAPI.FindPlayerByAccountId(accountId.Value);

        var isKicked = false;
        if(player is not null) {
            player.Kick(message);
            isKicked = true;
        }

        await context.sendResponseAsync(new AccountKickResultApiModel(isKicked)).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the route to ban a user by account ID.
    /// Requires query parameters "accountId" and "message".
    /// Route: PUT /account/ban?accountId={accountId}&message={message}
    /// </summary>
    [RequireAdminLevel(AdminLevelEnum.Rank2)]
    [RequiredQueryParams("accountId", "message")]
    private static async Task handlePutBanRoute(HttpListenerContext context, string[] segments) {
        int? accountId = context.getQueryParameter<int>("accountId");
        var message = context.getQueryParameter("message");
        
        if(!accountId.HasValue) {
            await context.sendResponseAsync("Invalid accountId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }
        message = Uri.UnescapeDataString(message!);

        var isBanned = await AccountHelper.banAsync(accountId.Value, message, true).ConfigureAwait(false);
        var isKicked = false;

        var player = ChoiceVAPI.FindPlayerByAccountId(accountId.Value);
        if(player is not null) {
            player.Kick(message);
            isKicked = true;
        }

        await context.sendResponseAsync(new AccountBanResultApiModel(isBanned, isKicked)).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the route to unban a user by account ID.
    /// Requires the query parameter "accountId".
    /// Route: PUT /account/unban?accountId={accountId}
    /// </summary>
    [RequireAdminLevel(AdminLevelEnum.Rank2)]
    [RequiredQueryParams("accountId")]
    private static async Task handlePutUnbanRoute(HttpListenerContext context, string[] segments) {
        int? accountId = context.getQueryParameter<int>("accountId");
        
        if(!accountId.HasValue) {
            await context.sendResponseAsync("Invalid accountId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        var isUnbanned = await AccountHelper.unbanAsync(accountId.Value).ConfigureAwait(false);

        await context.sendResponseAsync(new AccountUnbanResultApiModel(isUnbanned)).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the route to retrieve account information by account ID.
    /// Requires the query parameter "accountId".
    /// Route: GET /account?accountId={accountId}
    /// </summary>
    [RequiredQueryParams("accountId")]
    private static async Task handleGetByAccountIdRoute(HttpListenerContext context, string[] segments) {
        int? accountId = context.getQueryParameter<int>("accountId");
        
        if(!accountId.HasValue) {
            await context.sendResponseAsync("Invalid accountId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        var account = await AccountHelper.getByIdConvertedAsync(accountId.Value).ConfigureAwait(false);

        await context.sendResponseAsync(account).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Handles the route to retrieve account information by Discord ID.
    /// Requires the query parameter "discordId".
    /// Route: GET /account?discordId={discordId}
    /// </summary>
    [RequiredQueryParams("discordId")]
    private static async Task handleGetByDiscordIdRoute(HttpListenerContext context, string[] segments) {
        var discordId = context.getQueryParameter("discordId");
        
        var account = await AccountHelper.getByDiscordIdAsync(discordId!).ConfigureAwait(false);

        await context.sendResponseAsync(account).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the route to retrieve all accounts.
    /// Route: GET /account
    /// </summary>
    [NoQueryParams]
    private static async Task handleGetAllRoute(HttpListenerContext context, string[] segments) {
        var list = await AccountHelper.getAllAccountsAsync().ConfigureAwait(false);

        await context.sendResponseAsync(list).ConfigureAwait(false);
    }
}
