using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ChoiceVExternApi;
using ChoiceVExternApi.Shared;
using ChoiceVExternApi.Shared.Attributes;
using ChoiceVServer.Base;

namespace ChoiceVServer.Controller.Web.ExternApi.Server;

public class ServerHandler : IBasicExternApiHandler {
    private const string Prefix = "server";

    public void registerRoutes(RouteHandler routeHandler) {
        routeHandler.registerRoute(HttpMethod.Get, Prefix + "/info", handleGetInfoRoute);
    }

    /// <summary>
    /// Handles the route to get current server information.
    /// No query parameters are required.
    /// Route: GET /server/info
    /// </summary>
    [NoQueryParams]
    private static async Task handleGetInfoRoute(HttpListenerContext context, string[] segments) {
        try {
            var currentServerInfo = await ServerHelper.getCurrentServerInfosApiModelAsync();

            await context.sendResponseAsync(currentServerInfo).ConfigureAwait(false);
        } catch(Exception e) {
            Logger.logException(e);
            await context.sendResponseAsync(HttpStatusCode.InternalServerError.ToString(), HttpStatusCode.InternalServerError).ConfigureAwait(false);
        }
    }
}
