using System.Diagnostics;
using System.Net;
using System.Reflection;
using ChoiceVExternApi.Shared;

namespace ChoiceVExternApi;

public class ExternApi(ExternApiConfig config) {
    private HttpListener? Listener;
    private readonly RouteHandler RouteHandler = new(new RouteHandlerConfig("/api/v1/", config.discordServerId, config.discordBotToken, config.isDevServer));
    private readonly DigestAuthHandler DigestAuthHandler = new(new DigestAuthHandlerConfig(config.neededAuthUsername, config.neededAuthPassword));
    private const string AuthHeader = "Authorization";
    private const string AuthErrorMessage = "Authentication failed!";
    private const string InternalServerErrorMessage = "InternalServerError";

    /// <summary>
    /// Starts the external API asynchronously. Initializes the listener and waits for incoming requests.
    /// </summary>
    public async Task startAsync() {
        if(config.apiUrl is null) {
            Debug.WriteLine("Extern API could not be started. Url not set.");
            return;
        }
        if(config.neededAuthUsername is null) {
            return;
        }
        if(config.neededAuthPassword is null) {
            return;
        }

        initializeListener();

        Console.WriteLine($"Extern API started and waiting for a request on {config.apiUrl}...");

        while(true) {
            var context = await Listener!.GetContextAsync().ConfigureAwait(false);
            _ = handleRequestAsync(context).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Initializes the HTTP listener and registers routes for all classes implementing IBasicExternApiHandler.
    /// </summary>
    private void initializeListener() {
        Listener = new HttpListener();
        Listener.Prefixes.Add(config.apiUrl!);

        #region RegisterRoutes for IBasicExternApiHandler's

        var typesImplementingInterface = config.handlerAssembly
            .GetTypes()
            .Where(t => typeof(IBasicExternApiHandler).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
            .ToList();

        foreach(var instance in typesImplementingInterface.Select(type => Activator.CreateInstance(type) as IBasicExternApiHandler)) {
            instance?.registerRoutes(RouteHandler);
        }

        #endregion

        Listener.Start();
    }

    /// <summary>
    /// Handles incoming HTTP requests asynchronously. Checks authorization and routes the request.
    /// </summary>
    private async Task handleRequestAsync(HttpListenerContext context) {
        try {
            var (isAuthorized, sendedChallenge) = await isAuthorizedAsync(context).ConfigureAwait(false);

            if(!isAuthorized && !sendedChallenge) {
                await context.sendResponseAsync(AuthErrorMessage, HttpStatusCode.BadRequest).ConfigureAwait(false);
                return;
            }

            if(isAuthorized) {
                await RouteHandler.handleRequestAsync(context).ConfigureAwait(false);
            }
        } catch(Exception ex) {
            Console.WriteLine(ex.Message);
            await context.sendResponseAsync(InternalServerErrorMessage, HttpStatusCode.InternalServerError).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously checks if the request is authorized using HTTP authentication headers.
    /// </summary>
    /// <param name="context">The HttpListenerContext containing the request and response objects.</param>
    /// <returns>
    /// A tuple where the first boolean indicates if the request is authorized,
    /// and the second boolean indicates if a challenge was sent to the client.
    /// </returns>
    private async Task<(bool, bool)> isAuthorizedAsync(HttpListenerContext context) {
        var authHeader = context.Request.Headers[AuthHeader];

        if(authHeader is null)
            return (false, false);

        if(authHeader.StartsWith("Digest", StringComparison.OrdinalIgnoreCase)) {
            if(await DigestAuthHandler.authenticateAsync(authHeader, context.Request.HttpMethod).ConfigureAwait(false))
                return (true, false);

            await DigestAuthHandler.sendDigestChallengeAsync(context).ConfigureAwait(false);
            return (false, true);

        }

        return (false, false);
    }
}
