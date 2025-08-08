using System.Diagnostics;
using System.Net;
using ChoiceVExternApi.Shared;
using ChoiceVExternApi.Shared.Attributes;

namespace ChoiceVExternApi;

public class RouteHandler(RouteHandlerConfig config) {
    private readonly Dictionary<(string method, string path), List<Func<HttpListenerContext, string[], Task>>> Routes = new();

    /// <summary>
    /// Registers a route with a specified HTTP method and path, associating it with a handler function.
    /// </summary>
    /// <param name="method">The HTTP method (e.g., GET, POST) for the route.</param>
    /// <param name="path">The path for the route, relative to the API version prefix.</param>
    /// <param name="handler">The function to handle requests to this route.</param>
    public void registerRoute(HttpMethod method, string path, Func<HttpListenerContext, string[], Task> handler) {
        var fullPath = config.routePrefix + path;
        var key = (method.ToString(), fullPath);

        if(!Routes.TryGetValue(key, out var route)) {
            route = [];
            Routes[key] = route;
        }

        route.Add(handler);
        Console.WriteLine($"Registered route: {fullPath}");
    }

    /// <summary>
    /// Handles an incoming HTTP request by matching it to a registered route and invoking the appropriate handler.
    /// </summary>
    /// <param name="context">The context of the HTTP request, containing request and response information.</param>
    public async Task handleRequestAsync(HttpListenerContext context) {
        if(context.Request.Url == null) return;

        var segments = context.Request.Url.AbsolutePath.Split('/');
        var path = context.Request.Url.AbsolutePath;
        var method = context.Request.HttpMethod;

        if(!Routes.TryGetValue((method, path), out var handlers)) {
            await context.sendResponseAsync("404 - Not Found", HttpStatusCode.NotFound).ConfigureAwait(false);
            return;
        }

        if(handlers.Count > 1) {
            await handleMultipleHandlersAsync(context, handlers, segments);
            return;
        }

        await handleSingleHandlerAsync(context, handlers, segments);
    }

    /// <summary>
    /// Handles a request with a single registered handler by executing the handler function.
    /// </summary>
    /// <param name="context">The context of the HTTP request.</param>
    /// <param name="handlers">The list of handler functions for the route.</param>
    /// <param name="segments">The segments of the request URL path.</param>
    private async Task handleSingleHandlerAsync(
        HttpListenerContext context,
        List<Func<HttpListenerContext, string[], Task>> handlers,
        string[] segments) {
        var handler = handlers[0];
        var handlerResult = await checkHandlerAsync(context, segments, handler);
        if(handlerResult.Handled) return;

        await context.sendResponseAsync(handlerResult.Response, handlerResult.StatusCode!.Value).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles a request with multiple registered handlers by attempting each handler until one succeeds.
    /// </summary>
    /// <param name="context">The context of the HTTP request.</param>
    /// <param name="handlers">The list of handler functions for the route.</param>
    /// <param name="segments">The segments of the request URL path.</param>
    private async Task handleMultipleHandlersAsync(
        HttpListenerContext context,
        List<Func<HttpListenerContext, string[], Task>> handlers,
        string[] segments) {
        foreach(var handler in handlers) {
            var handlerResult = await checkHandlerAsync(context, segments, handler);
            if(handlerResult.Handled) return;
        }

        await context.sendResponseAsync("No Handler found.", HttpStatusCode.BadGateway).ConfigureAwait(false);
    }

    /// <summary>
    /// Checks if a handler can process the request based on attributes and executes it if possible.
    /// </summary>
    /// <param name="context">The context of the HTTP request.</param>
    /// <param name="segments">The segments of the request URL path.</param>
    /// <param name="handler">The handler function to be checked and potentially executed.</param>
    /// <returns>A result indicating whether the handler processed the request and any response details.</returns>
    private async Task<CheckHandlerResult> checkHandlerAsync(
        HttpListenerContext context,
        string[] segments,
        Func<HttpListenerContext, string[], Task> handler) {
        if(!handler.checkRequiredQueryParamsAttribute(context)) return new CheckHandlerResult(false, HttpStatusCode.BadRequest, "Invalid query parameters.");
        if(!handler.checkNoQueryParamsAttribute(context)) return new CheckHandlerResult(false, HttpStatusCode.BadRequest, "Invalid query parameters.");
        if(!await handler.checkRequireAdminLevelAttribute(context, config.guildId, config.botToken, config.isDevServer).ConfigureAwait(false)) return new CheckHandlerResult(false, HttpStatusCode.Unauthorized, "AuthErrorMessage");

        await handler(context, segments);
        return new CheckHandlerResult();
    }
}
internal class CheckHandlerResult {
    public CheckHandlerResult() { }

    public CheckHandlerResult(bool handled, HttpStatusCode statusCode, string response) {
        Handled = handled;
        StatusCode = statusCode;
        Response = response;
    }

    public bool Handled { get; set; } = true;
    public HttpStatusCode? StatusCode { get; set; }
    public string? Response { get; set; }
}
