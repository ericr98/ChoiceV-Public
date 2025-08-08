using System.Linq;
using System.Threading.Tasks;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Model.Database;
using ChoiceVSharedApiModels.Server;
using Microsoft.EntityFrameworkCore;

namespace ChoiceVServer.Controller.Web.ExternApi.Server;

public static class ServerHelper {
    public static async Task<CurrentServerInfosApiModel> getCurrentServerInfosApiModelAsync() {
        var allPlayers = ChoiceVAPI.GetAllPlayers();
        var playerOnlineCount = allPlayers.Count;

        var allCompanies = CompanyController.AllCompanies.Select(x => x.Value).ToArray(); 

        var policeInDutyCount = countEmployeesInDuty(allCompanies, CompanyType.Police);
        var sheriffInDutyCount = countEmployeesInDuty(allCompanies, CompanyType.Sheriff);
        var medicInDutyCount = countEmployeesInDuty(allCompanies, CompanyType.Medic);

        await using var db = new ChoiceVDb();

        var accountsDb = await db.accounts.ToListAsync().ConfigureAwait(false);
        var overallAccountsCount = accountsDb.Count;
        var whitelistedAccountsCount = accountsDb.Count(x => x.state == 1);
        var bannedAccountsCount = accountsDb.Count(x => x.state == 2);

        return new CurrentServerInfosApiModel(
            playerOnlineCount,
            overallAccountsCount,
            whitelistedAccountsCount,
            bannedAccountsCount,
            policeInDutyCount,
            sheriffInDutyCount,
            medicInDutyCount
        );
    }

    private static int countEmployeesInDuty(Companies.Company[] companies, CompanyType type) {
        return companies
            .FirstOrDefault(x => x.CompanyType == type)?
            .Employees.Count(x => x.InDuty) ?? 0;
    }
}
