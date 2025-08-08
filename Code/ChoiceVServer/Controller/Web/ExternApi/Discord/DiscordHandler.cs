using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ChoiceVExternApi;
using ChoiceVExternApi.Shared;
using ChoiceVExternApi.Shared.Attributes;
using ChoiceVServer.Controller.Discord;

namespace ChoiceVServer.Controller.Web.ExternApi.Discord;

public class DiscordHandler : IBasicExternApiHandler {
    private const string Prefix = "discord";
    
    public void registerRoutes(RouteHandler routeHandler) {
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetAllRoute);
    }
    
    [NoQueryParams]
    private static async Task handleGetAllRoute(HttpListenerContext context, string[] segments) {
        var discordMembers = await DiscordController.getAllGuildMembersAsync();

        await context.sendResponseAsync(discordMembers.convertToApiModel()).ConfigureAwait(false);
    }
}
