#nullable enable
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ChoiceVExternApi;
using ChoiceVExternApi.Shared;
using ChoiceVExternApi.Shared.Attributes;
using ChoiceVServer.Controller.Web.ExternApi.Vehicle;
using ChoiceVServer.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace ChoiceVServer.Controller.Web.ExternApi.SupportKeyInfo;

public class SupportKeyInfoHandler : IBasicExternApiHandler {
    private const string Prefix = "supportkeyinfo";

    public void registerRoutes(RouteHandler routeHandler) {
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetAllRoute);
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetBySupportKeyInfoIdRoute);
    }
    
    [NoQueryParams]
    private static async Task handleGetAllRoute(HttpListenerContext context, string[] segments) {
        await using var db = new ChoiceVDb();

        var allSupportKeyInfos = await db.supportkeyinfos.ToListAsync();

        await context.sendResponseAsync(await allSupportKeyInfos.convertToApiModel()).ConfigureAwait(false);
    }
    
    [RequiredQueryParams("supportKeyInfoId")]
    private static async Task handleGetBySupportKeyInfoIdRoute(HttpListenerContext context, string[] segments) {
        int? supportKeyInfoId = context.getQueryParameter<int>("supportKeyInfoId");

        if (!supportKeyInfoId.HasValue) {
            await context.sendResponseAsync("Invalid supportKeyInfoId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        await using var db = new ChoiceVDb();
        
        var allSupportKeyInfo = await db.supportkeyinfos.FirstOrDefaultAsync(x => x.id == supportKeyInfoId.Value);
        if(allSupportKeyInfo is null) {
            await context.sendResponseAsync("Not Found.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        await context.sendResponseAsync(await allSupportKeyInfo.convertToApiModel()).ConfigureAwait(false);
    }
}
