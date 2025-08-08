#nullable enable
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ChoiceVExternApi;
using ChoiceVExternApi.Shared;
using ChoiceVExternApi.Shared.Attributes;

namespace ChoiceVServer.Controller.Web.ExternApi.BankAccount;

public class BankAccountHandler : IBasicExternApiHandler {

    private const string Prefix = "bankaccount";

    public void registerRoutes(RouteHandler routeHandler) {
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetAllRoute);
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetByBankaccountIdRoute);
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetByCharacterIdRoute);
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetByCompanyIdRoute);
    }

    /// <summary>
    /// Handles the route to get bank accounts by company ID.
    /// Requires query parameter "companyId".
    /// Route: GET /bankaccount?companyId={companyId}
    /// </summary>
    [RequiredQueryParams("companyId")]
    private static async Task handleGetByCompanyIdRoute(HttpListenerContext context, string[] segments) {
        int? companyId = context.getQueryParameter<int>("companyId");

        if(!companyId.HasValue) {
            await context.sendResponseAsync("Invalid companyId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        var companyBankAccounts = await BankAccountHelper.getBankAccountsByCompanyIdAsync(companyId.Value).ConfigureAwait(false);

        await context.sendResponseAsync(companyBankAccounts).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the route to get bank accounts by character ID.
    /// Requires query parameter "characterId".
    /// Route: GET /bankaccount?characterId={characterId}
    /// </summary>
    [RequiredQueryParams("characterId")]
    private static async Task handleGetByCharacterIdRoute(HttpListenerContext context, string[] segments) {
        int? characterId = context.getQueryParameter<int>("characterId");

        if(!characterId.HasValue) {
            await context.sendResponseAsync("Invalid characterId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        var characterBankAccounts = await BankAccountHelper.getBankAccountsByCharacterIdAsync(characterId.Value).ConfigureAwait(false);

        await context.sendResponseAsync(characterBankAccounts).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the route to get a bank account by bank account ID.
    /// Requires query parameter "bankaccountId".
    /// Route: GET /bankaccount?bankaccountId={bankaccountId}
    /// </summary>
    [RequiredQueryParams("bankaccountId")]
    private static async Task handleGetByBankaccountIdRoute(HttpListenerContext context, string[] segments) {
        int? bankaccountId = context.getQueryParameter<int>("bankaccountId");

        if(!bankaccountId.HasValue) {
            await context.sendResponseAsync("Invalid bankaccountId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        var bankAccount = await BankAccountHelper.getBankAccountByIdAsync(bankaccountId.Value).ConfigureAwait(false);

        await context.sendResponseAsync(bankAccount).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the route to get all bank accounts.
    /// No query parameters are required.
    /// Route: GET /bankaccount
    /// </summary>
    [NoQueryParams]
    private static async Task handleGetAllRoute(HttpListenerContext context, string[] segments) {
        var allBankAccounts = await BankAccountHelper.getAllBankAccountsAsync().ConfigureAwait(false);

        await context.sendResponseAsync(allBankAccounts).ConfigureAwait(false);
    }
}