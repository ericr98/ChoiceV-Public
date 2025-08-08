#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ChoiceVExternApi;
using ChoiceVExternApi.Shared;
using ChoiceVExternApi.Shared.Attributes;
using ChoiceVServer.Base;
using ChoiceVSharedApiModels.Characters;

namespace ChoiceVServer.Controller.Web.ExternApi.Character;

public class CharacterHandler : IBasicExternApiHandler {
    private const string Prefix = "character";

    public void registerRoutes(RouteHandler routeHandler) {
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetAllRoute);
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetByCharacterIdRoute);
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetByAccountIdRoute);
        routeHandler.registerRoute(HttpMethod.Get, Prefix + "/live", handleGetLiveRoute);
        routeHandler.registerRoute(HttpMethod.Put, Prefix + "/dimension", handlePutDimensionRoute);
        routeHandler.registerRoute(HttpMethod.Put, Prefix + "/dead", handlePutDeadRoute);
        routeHandler.registerRoute(HttpMethod.Put, Prefix + "/crimeflag", handlePutCrimeFlagActiveRoute);
    }

    [RequireAdminLevel(AdminLevelEnum.Rank3)]
    [RequiredQueryParams("characterId", "state")]
    private static async Task handlePutCrimeFlagActiveRoute(HttpListenerContext context, string[] segments) {
        int? characterId = context.getQueryParameter<int>("characterId");
        bool? state = context.getQueryParameter<bool>("state");

        if(!characterId.HasValue) {
            await context.sendResponseAsync("Invalid characterId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        if(!state.HasValue) {
            await context.sendResponseAsync("Invalid state.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }
        
        var (characterDataUpdated, onlinePlayerUpdated, oldFlag, newFlag) = await CharacterHelper.setCrimeFlagActiveAsync(characterId.Value, state.Value).ConfigureAwait(false);
        if(!characterDataUpdated) {
            await context.sendResponseAsync("Something went wrong.", HttpStatusCode.InternalServerError).ConfigureAwait(false);
            return;
        }

        if(!oldFlag.HasValue) {
            await context.sendResponseAsync("Something went wrong. 002", HttpStatusCode.InternalServerError).ConfigureAwait(false);
            return;
        }
        if(!newFlag.HasValue) {
            await context.sendResponseAsync("Something went wrong. 003", HttpStatusCode.InternalServerError).ConfigureAwait(false);
            return;
        }
        
        await context.sendResponseAsync(new CharacterSetCrimeFlagActiveResultApiModel(onlinePlayerUpdated, oldFlag.Value, newFlag.Value, characterId.Value)).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Handles the route to set a character's permadeath state.
    /// Requires admin level Rank3 and query parameters "characterId" and "state".
    /// Route: PUT /character/dead?characterId={characterId}&state={state}
    /// </summary>
    [RequireAdminLevel(AdminLevelEnum.Rank3)]
    [RequiredQueryParams("characterId", "state")]
    private static async Task handlePutDeadRoute(HttpListenerContext context, string[] segments) {
        int? characterId = context.getQueryParameter<int>("characterId");
        bool? state = context.getQueryParameter<bool>("state");

        if(!characterId.HasValue) {
            await context.sendResponseAsync("Invalid characterId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        if(!state.HasValue) {
            await context.sendResponseAsync("Invalid state.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        var (characterDataUpdated, onlinePlayerUpdated) = await CharacterHelper.setPermadeathActivatedAsync(characterId.Value, state.Value).ConfigureAwait(false);
        if(!characterDataUpdated) {
            await context.sendResponseAsync("Something went wrong.", HttpStatusCode.InternalServerError).ConfigureAwait(false);
            return;
        }

        await context.sendResponseAsync(new CharacterSetPermadeathActivatedResultApiModel(onlinePlayerUpdated, !state.Value, state.Value, characterId.Value)).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the route to change a character's dimension.
    /// Requires admin level Rank2 and query parameters "characterId" and "dimensionId".
    /// Route: PUT /character/dimension?characterId={characterId}&dimensionId={dimensionId}
    /// </summary>
    [RequireAdminLevel(AdminLevelEnum.Rank2)]
    [RequiredQueryParams("characterId", "dimension")]
    private static async Task handlePutDimensionRoute(HttpListenerContext context, string[] segments) {
        int? characterId = context.getQueryParameter<int>("characterId");
        int? dimension = context.getQueryParameter<int>("dimension");

        if(!characterId.HasValue) {
            await context.sendResponseAsync("Invalid characterId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        if(!dimension.HasValue) {
            await context.sendResponseAsync("Invalid dimension.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        var player = ChoiceVAPI.FindPlayerByCharId(characterId.Value);
        if(player == null) {
            await context.sendResponseAsync("Player not found.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        var oldDimension = player.changeDimension(dimension.Value);
        if(!oldDimension.HasValue) {
            await context.sendResponseAsync("ChangeDimension failed.", HttpStatusCode.InternalServerError).ConfigureAwait(false);
            return;
        }

        var response = new CharacterChangeDimensionResultApiModel(oldDimension.Value, dimension.Value, characterId.Value);

        await context.sendResponseAsync(response).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the route to get all live characters.
    /// No query parameters are required.
    /// Route: GET /character/live
    /// </summary>
    [NoQueryParams]
    private static async Task handleGetLiveRoute(HttpListenerContext context, string[] segments) {
        try {
            var allPlayers = ChoiceVAPI.GetAllPlayers();

            var list = new List<CharacterApiModel>();

            var onlinePlayersCharacterId = allPlayers.Select(x => x.getCharacterId());
            foreach(var charId in onlinePlayersCharacterId) {
                var character = await CharacterHelper.getCharacterByIdAsync(charId).ConfigureAwait(false);
                if(character is null) continue;
                list.Add(character);
            }

            await context.sendResponseAsync(list).ConfigureAwait(false);

        } catch(Exception e) {
            Logger.logException(e);
            await context.sendResponseAsync(HttpStatusCode.InternalServerError.ToString(), HttpStatusCode.InternalServerError).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles the route to get characters by account ID.
    /// Requires query parameter "accountId".
    /// Route: GET /character?accountId={accountId}
    /// </summary>
    [RequiredQueryParams("accountId")]
    private static async Task handleGetByAccountIdRoute(HttpListenerContext context, string[] segments) {
        int? accountId = context.getQueryParameter<int>("accountId");

        if(!accountId.HasValue) {
            await context.sendResponseAsync("Invalid accountId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        var account = await CharacterHelper.getAllCharactersByAccountIdAsync(accountId.Value).ConfigureAwait(false);

        await context.sendResponseAsync(account).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the route to get a character by character ID.
    /// Requires query parameter "characterId".
    /// Route: GET /character?characterId={characterId}
    /// </summary>
    [RequiredQueryParams("characterId")]
    private static async Task handleGetByCharacterIdRoute(HttpListenerContext context, string[] segments) {
        int? characterId = context.getQueryParameter<int>("characterId");

        if(!characterId.HasValue) {
            await context.sendResponseAsync("Invalid characterId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        var account = await CharacterHelper.getCharacterByIdAsync(characterId.Value).ConfigureAwait(false);

        await context.sendResponseAsync(account).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the route to get all characters.
    /// No query parameters are required.
    /// Route: GET /character
    /// </summary>
    [NoQueryParams]
    private static async Task handleGetAllRoute(HttpListenerContext context, string[] segments) {
        var list = await CharacterHelper.getAllCharactersAsync().ConfigureAwait(false);

        await context.sendResponseAsync(list).ConfigureAwait(false);
    }
}
