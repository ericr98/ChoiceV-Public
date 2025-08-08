#nullable enable
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ChoiceVExternApi;
using ChoiceVExternApi.Shared;
using ChoiceVExternApi.Shared.Attributes;

namespace ChoiceVServer.Controller.Web.ExternApi.Inventory;

public class InventoryHandler : IBasicExternApiHandler {
    private const string Prefix = "inventory";

    public void registerRoutes(RouteHandler routeHandler) {
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetByCharacterIdRoute);
    }
    
    /// <summary>
    /// Handles the route to get the inventory by character ID.
    /// Requires query parameter "characterId".
    /// Route: GET /inventory?characterId={characterId}
    /// </summary>
    [RequiredQueryParams("characterId")]
    private static async Task handleGetByCharacterIdRoute(HttpListenerContext context, string[] segments) {
        int? characterId = context.getQueryParameter<int>("characterId");

        if(!characterId.HasValue) {
            await context.sendResponseAsync("Invalid characterId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        var inventory = await InventoryHelper.getByCharacterIdAsync(characterId.Value).ConfigureAwait(false);
        if (inventory is null) {
            await context.sendResponseAsync("Inventory not found.", HttpStatusCode.NotFound).ConfigureAwait(false);
            return;
        }

        await context.sendResponseAsync(inventory).ConfigureAwait(false);
    }
}
