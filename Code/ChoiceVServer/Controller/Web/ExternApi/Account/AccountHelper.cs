#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;
using ChoiceVSharedApiModels.Accounts;
using Microsoft.EntityFrameworkCore;

namespace ChoiceVServer.Controller.Web.ExternApi.Account;

public static class AccountHelper {
    public static AccountApiModel convertToApiModel(this account account) {
        var stateParsed = Enum.TryParse<AccountStateApiEnum>(account.state.ToString(), out var stateEnum);

        var player = ChoiceVAPI.FindPlayerByAccountId(account.id);
        var isCurrentlyOnline = player is not null;

        var hasLightmodeFlag = ((AccountFlag)account.flag).HasFlag(AccountFlag.LiteModeActivated);
        
        var response = new AccountApiModel(
            account.id,
            account.name,
            account.socialclubName,
            account.discordId,
            account.teamspeakId,
            account.lastLogin,
            stateParsed ? stateEnum : AccountStateApiEnum.Undefined,
            account.stateReason,
            account.failedLogins,
            account.adminLevel,
            account.charAmount,
            account.strikes,
            account.flag,
            hasLightmodeFlag,
            isCurrentlyOnline
        );

        return response;
    }

    public static async Task<List<AccountApiModel>> getAllAccountsAsync() {
        await using var db = new ChoiceVDb();

        var response = await db.accounts
            .AsNoTracking()
            .Select(x => x.convertToApiModel())
            .ToListAsync();

        return response;
    }

    public static async Task<account?> getByIdAsync(int accountId, ChoiceVDb db) {
        var account = await db.accounts.FirstOrDefaultAsync(x => x.id == accountId);
        return account;
    }
    
    public static async Task<account?> getByIdAsync(int accountId) {
        await using var db = new ChoiceVDb();
        return await getByIdAsync(accountId, db);
    }
    
    public static async Task<AccountApiModel?> getByIdConvertedAsync(int accountId) {
        var account = await getByIdAsync(accountId);
        if(account is null) return null;

        var response = account.convertToApiModel();

        return response;
    }

    public static async Task<AccountApiModel?> getByDiscordIdAsync(string discordId) {
        await using var db = new ChoiceVDb();

        var account = await db.accounts.AsNoTracking().FirstOrDefaultAsync(x => x.discordId == discordId);
        if(account is null) return null;

        var response = account.convertToApiModel();

        return response;
    }
    
    public static async Task<AccountApiModel?> getBySocialclubNameAsync(string socialClubName) {
        await using var db = new ChoiceVDb();

        var account = await db.accounts.AsNoTracking().FirstOrDefaultAsync(x => x.socialclubName == socialClubName);
        if(account is null) return null;

        var response = account.convertToApiModel();

        return response;
    }
    
    public static async Task<AccountApiModel?> addAsync(string socialClubName, string discordId) {
        await using var db = new ChoiceVDb();

        var check = await getBySocialclubNameAsync(socialClubName).ConfigureAwait(false);

        if(check is not null) {
            return null;
        }

        var dbAcc = new account {
            socialclubName = socialClubName,
            name = socialClubName,
            discordId = discordId,
            state = 1,
            adminLevel = 3,
            flag = (int)AccountFlag.LiteModeActivated,
        };

        await db.accounts.AddAsync(dbAcc).ConfigureAwait(false);
        await db.SaveChangesAsync().ConfigureAwait(false);

        var response = dbAcc.convertToApiModel();
        
        return response;
    }

    public static bool ban(int accountId, string reason = "Gebannt!", bool overrideAdmin = false) {
        return banAsync(accountId, reason, overrideAdmin).GetAwaiter().GetResult();
    }

    public static async Task<bool> banAsync(int accountId, string reason = "Gebannt!", bool overrideAdmin = false) {
        await using var db = new ChoiceVDb();

        var accountDb = db.accounts.FirstOrDefault(a => a.id == accountId);
        if(accountDb is null) return false;

        if(overrideAdmin && accountDb.adminLevel > 3) {
            accountDb.adminLevel = 2;
        }

        accountDb.state = (int)AccountStateApiEnum.Banned;
        accountDb.stateReason = reason;

        db.accounts.Update(accountDb);

        var changes = await db.SaveChangesAsync();
        return changes > 0;
    }

    public static bool unban(int accountId) {
        return unbanAsync(accountId).GetAwaiter().GetResult();
    }

    public static async Task<bool> unbanAsync(int accountId) {
        await using var db = new ChoiceVDb();

        var accountDb = db.accounts.FirstOrDefault(a => a.id == accountId);
        if(accountDb is null) return false;

        accountDb.state = (int)AccountStateApiEnum.Whitelisted;
        accountDb.stateReason = string.Empty;

        await db.SaveChangesAsync();
        return true;
    }
}
