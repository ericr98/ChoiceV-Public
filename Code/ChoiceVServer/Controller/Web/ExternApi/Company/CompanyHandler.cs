#nullable enable
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ChoiceVExternApi;
using ChoiceVExternApi.Shared;
using ChoiceVExternApi.Shared.Attributes;

namespace ChoiceVServer.Controller.Web.ExternApi.Company;

public class CompanyHandler : IBasicExternApiHandler {
    private const string Prefix = "company";

    public void registerRoutes(RouteHandler routeHandler) {
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetAllRoute);
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetByCharacterIdRoute);
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetByCompanyIdRoute);
    }

    /// <summary>
    /// Handles the route to get companies by character ID.
    /// Requires query parameter "characterId".
    /// Route: GET /company?characterId={characterId}
    /// </summary>
    [RequiredQueryParams("characterId")]
    private static async Task handleGetByCharacterIdRoute(HttpListenerContext context, string[] segments) {
        int? characterId = context.getQueryParameter<int>("characterId");

        if (!characterId.HasValue) {
            await context.sendResponseAsync("Invalid characterId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        var characterCompanies = CompanyController.AllCompanies
            .Select(x => x.Value)
            .Where(company => company.Employees.Any(e => e.CharacterId == characterId.Value))
            .Select(company => company.convertToApiModel())
            .ToList();

        await context.sendResponseAsync(characterCompanies);
    }

    /// <summary>
    /// Handles the route to get a company by company ID.
    /// Requires query parameter "companyId".
    /// Route: GET /company?companyId={companyId}
    /// </summary>
    [RequiredQueryParams("companyId")]
    private static async Task handleGetByCompanyIdRoute(HttpListenerContext context, string[] segments) {
        int? companyId = context.getQueryParameter<int>("companyId");

        if (!companyId.HasValue) {
            await context.sendResponseAsync("Invalid companyId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        var company = CompanyController.getCompanyById(companyId.Value);
        if (company is null) {
            await context.sendResponseAsync("Company not found.", HttpStatusCode.NotFound).ConfigureAwait(false);
            return;
        }

        await context.sendResponseAsync(company.convertToApiModel()).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the route to get all companies.
    /// No query parameters are required.
    /// Route: GET /company
    /// </summary>
    [NoQueryParams]
    private static async Task handleGetAllRoute(HttpListenerContext context, string[] segments) {
        var companyApiList = CompanyController.AllCompanies
            .Select(x => x.Value.convertToApiModel())
            .ToList();

        await context.sendResponseAsync(companyApiList).ConfigureAwait(false);
    }
}
