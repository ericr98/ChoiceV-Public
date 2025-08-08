#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChoiceVServer.Model.Database;
using ChoiceVSharedApiModels.BankAccounts;
using ChoiceVSharedApiModels.BankAccounts.Enums;
using ChoiceVSharedApiModels.Companys;
using Microsoft.EntityFrameworkCore;

namespace ChoiceVServer.Controller.Web.ExternApi.BankAccount;

public static class BankAccountHelper {

    public static List<BankAccountApiModel> convertToApiModel(this List<bankaccount> bankAccounts) {
        return bankAccounts.Select(x => x.convertToApiModel()).ToList();
    }

    public static BankAccountApiModel convertToApiModel(this bankaccount bankAccount) {
        var companyParsed = Enum.TryParse<BankCompaniesApiEnum>(bankAccount.bankId.ToString(), out var companyEnum);
        var accountTypeParsed = Enum.TryParse<BankAccountTypeApiEnum>(bankAccount.accountType.ToString(), out var accountTypeEnum);
        var ownerTypeParsed = Enum.TryParse<BankAccountOwnerTypeApiEnum>(bankAccount.ownerType.ToString(), out var ownerTypeEnum);

        var isFrozen = bankAccount.isFrozen == 1;
        var isDeactivated = bankAccount.isDeactivated == 1;

        var response = new BankAccountApiModel(
            bankAccount.id,
            companyParsed ? companyEnum : BankCompaniesApiEnum.Undefined,
            accountTypeParsed ? accountTypeEnum : BankAccountTypeApiEnum.Undefined,
            bankAccount.name,
            bankAccount.balance,
            ownerTypeParsed ? ownerTypeEnum : BankAccountOwnerTypeApiEnum.Undefined,
            bankAccount.ownerValue,
            bankAccount.pin,
            isFrozen,
            bankAccount.creationDate,
            isDeactivated,
            bankAccount.connectedPhonenumber,
            bankAccount.isInfinite
        );

        return response;
    }

    public static BankAccountInterestApiModel convertToApiModel(this bankaccountinterest bankAccountInterest) {
        var response = new BankAccountInterestApiModel(
            bankAccountInterest.accountId,
            bankAccountInterest.interestPercent,
            bankAccountInterest.interestAmount,
            bankAccountInterest.nextInterest
        );

        return response;
    }

    public static void convertToApiModel(this List<BankTransactionApiModel> transactions, List<banktransaction> bankTransactions, List<banktransactionslog> bankTransactionLogs) {
        transactions.AddRange(bankTransactions.ConvertAll(x => x.convertToApiModel()));
        transactions.AddRange(bankTransactionLogs.ConvertAll(x => x.convertToApiModel()));
    }

    public static BankTransactionApiModel convertToApiModel(this banktransaction bankTransaction) {
        var response = new BankTransactionApiModel(
            bankTransaction.id,
            bankTransaction.from,
            bankTransaction.to,
            bankTransaction.amount,
            bankTransaction.message,
            bankTransaction.cost,
            bankTransaction.date,
            bankTransaction.due,
            true
        );

        return response;
    }

    public static BankTransactionApiModel convertToApiModel(this banktransactionslog bankTransaction) {
        var response = new BankTransactionApiModel(
            bankTransaction.id,
            bankTransaction.from,
            bankTransaction.to,
            bankTransaction.amount,
            bankTransaction.message,
            bankTransaction.cost,
            bankTransaction.date,
            bankTransaction.due,
            false
        );

        return response;
    }

    public static async Task setBankAccountTransactionsAsync(this BankAccountApiModel bankaccountApiModel, ChoiceVDb db) {
        var transactions = await db.banktransactions
            .AsNoTracking()
            .Where(x =>
                x.from == bankaccountApiModel.Id ||
                x.to == bankaccountApiModel.Id)
            .ToListAsync()
            .ConfigureAwait(false);

        var transactionLogs = await db.banktransactionslogs
            .AsNoTracking()
            .Where(x =>
                x.from == bankaccountApiModel.Id ||
                x.to == bankaccountApiModel.Id)
            .ToListAsync()
            .ConfigureAwait(false);

        bankaccountApiModel.Transactions.convertToApiModel(transactions, transactionLogs);
    }

    public static async Task setBankAccountInterestModelAsync(this BankAccountApiModel bankaccountApiModel, ChoiceVDb db) {
        var bankAccountInterest = await db.bankaccountinterests.AsNoTracking()
            .FirstOrDefaultAsync(x => x.accountId == bankaccountApiModel.Id)
            .ConfigureAwait(false);
        if(bankAccountInterest is null) return;

        bankaccountApiModel.InterestModel = bankAccountInterest.convertToApiModel();
    }

    public static BankAtmWithdrawApiModel convertToApIModel(this bankatmwithdraw bankAtmWithDraw) {
        var response = new BankAtmWithdrawApiModel(
            bankAtmWithDraw.id,
            bankAtmWithDraw.from,
            bankAtmWithDraw.atmId,
            bankAtmWithDraw.amount,
            bankAtmWithDraw.cost,
            bankAtmWithDraw.date
        );

        return response;
    }

    public static async Task setAtmWithdrawsAsync(this BankAccountApiModel bankAccountApiModel, ChoiceVDb db) {
        var atmWithdraws = await db.bankatmwithdraws.AsNoTracking()
            .Where(x => x.from == bankAccountApiModel.Id)
            .ToListAsync().ConfigureAwait(false);

        bankAccountApiModel.AtmWithdraws = atmWithdraws.ConvertAll(x => x.convertToApIModel());
    }

    public static BankBankWithdrawApiModel convertToApIModel(this bankbankwithdraw bankBankWithDraw) {
        var response = new BankBankWithdrawApiModel(
            bankBankWithDraw.id,
            bankBankWithDraw.from,
            bankBankWithDraw.amount,
            bankBankWithDraw.reason,
            bankBankWithDraw.date
        );

        return response;
    }

    public static async Task setBankWithdrawsAsync(this BankAccountApiModel bankAccountApiModel, ChoiceVDb db) {
        var bankWithdraws = await db.bankbankwithdraws.AsNoTracking()
            .Where(x => x.from == bankAccountApiModel.Id)
            .ToListAsync().ConfigureAwait(false);

        bankAccountApiModel.BankWithdraws = bankWithdraws.ConvertAll(x => x.convertToApIModel());
    }

    public static async Task<List<BankAccountApiModel>> getAllBankAccountsAsync() {
        await using var db = new ChoiceVDb();

        var allBankAccounts = await db.bankaccounts.AsNoTracking().ToListAsync().ConfigureAwait(false);

        var bankAccountApiList = allBankAccounts.convertToApiModel();

        return bankAccountApiList;
    }

    public static async Task<List<BankAccountApiModel>> getBankAccountsByCompanyIdAsync(int companyId) {
        var allBankAccounts = await getAllBankAccountsAsync();

        var response = allBankAccounts
            .Where(x =>
                x.OwnerType == BankAccountOwnerTypeApiEnum.Company &&
                x.OwnerValue == companyId.ToString())
            .ToList();

        return response;
    }

    public static async Task<List<BankAccountApiModel>> getBankAccountsByCharacterIdAsync(int characterId) {
        var allBankAccounts = await getAllBankAccountsAsync();

        var response = allBankAccounts
            .Where(x =>
                x.OwnerType == BankAccountOwnerTypeApiEnum.Player &&
                x.OwnerValue == characterId.ToString())
            .ToList();

        return response;
    }

    public static async Task<BankAccountApiModel?> getBankAccountByIdAsync(int bankAccountId) {
        await using var db = new ChoiceVDb();

        var bankaccount = await db.bankaccounts.AsNoTracking().FirstOrDefaultAsync(x => x.id == bankAccountId);
        if(bankaccount is null) return null;

        var bankaccountApiModel = bankaccount.convertToApiModel();

        await bankaccountApiModel.setBankAccountInterestModelAsync(db).ConfigureAwait(false);
        await bankaccountApiModel.setBankAccountTransactionsAsync(db).ConfigureAwait(false);
        await bankaccountApiModel.setAtmWithdrawsAsync(db).ConfigureAwait(false);
        await bankaccountApiModel.setBankWithdrawsAsync(db).ConfigureAwait(false);

        return bankaccountApiModel;
    }
}
