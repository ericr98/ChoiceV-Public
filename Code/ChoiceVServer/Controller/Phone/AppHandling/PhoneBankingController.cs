using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Controller.Phone.PhoneController;

namespace ChoiceVServer.Controller.Phone {
    internal class PhoneBankingController : ChoiceVScript {
        public PhoneBankingController() {
            EventController.addCefEvent("PHONE_BANKING_REQUEST_BANKACCOUNTS", onSmartphoneRequestBankaccounts);
            EventController.addCefEvent("PHONE_BANKING_REQUEST_TRANSACTIONS", onSmartphoneRequestTransactions);
            EventController.addCefEvent("PHONE_BANKING_NEW_TRANSACTION", onSmartphoneMakeTransaction);
        }

        #region RequestBankAccounts

        private class PhoneAnswerBankaccounts : PhoneAnswerEvent {
            private class BankAccountModel {
                public string type;
                public long number;
                public string name;
                public decimal balance;
                public string company;

                public BankAccountModel(BankAccountType type, long number, string name, decimal balance, string company) {
                    this.type = Constants.BankAccountTypeToName[type];
                    this.number = number;
                    this.name = name;
                    this.balance = balance;
                    this.company = company;
                }
            }

            public string[] accounts;

            public PhoneAnswerBankaccounts(IPlayer player, List<bankaccount> accounts) : base("PHONE_BANKING_ANSWER_BANKACCOUNTS") {
                this.accounts = accounts.Select(a => new BankAccountModel((BankAccountType)a.accountType, a.id, a.name, a.balance, BankCompaniesToPhoneString[(BankCompanies)a.bankId]).ToJson()).ToArray();
            }
        }

        private void onSmartphoneRequestBankaccounts(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var accounts = BankController.getPlayerBankAccounts(player);
            player.emitCefEventNoBlock(new PhoneAnswerBankaccounts(player, accounts));
        }

        #endregion

        #region RequestBankTransactions

        private class PhoneRequestBankTransactionsCefEvent {
            public long accountId;
        }

        private class PhoneAnswerBankTransactions : PhoneAnswerEvent {
            private class BankTransactionModel {
                public long from;
                public long to;
                public decimal balance;
                public string note;
                public string date;

                public BankTransactionModel(long from, long to, decimal balance, string note, DateTime date) {
                    this.from = from;
                    this.to = to;
                    this.balance = balance;
                    this.note = note;
                    this.date = date.ToString("yyyy-MM-ddTHH:mm:ss");
                }
            }

            public long number;
            public string[] transactions;

            public PhoneAnswerBankTransactions(long number, List<banktransaction> transactionList, List<banktransactionslog> transactionLogList, List<bankatmwithdraw> withdrawList, List<bankbankwithdraw> bankWithdrawList) : base("PHONE_BANKING_ANSWER_TRANSACTIONS") {
                this.number = number;
                var transactions1 = transactionList
                    .Select(t =>
                        new BankTransactionModel(t.from ?? -1, t.to, t.amount, t.message, t.date).ToJson())
                    .ToArray();

                var transactions2 = transactionLogList
                    .Select(t =>
                        new BankTransactionModel(t.from ?? -1, t.to, t.amount, t.message, t.date).ToJson())
                    .ToArray();

                var withdraws = withdrawList
                    .Select(w =>
                        new BankTransactionModel(w.amount > 0 ? w.from : -1, w.amount > 0 ? -1 : w.from, Math.Abs(-w.amount), w.amount > 0 ? "ATM-Abhebung" : "ATM-Einzahlung", w.date).ToJson())
                    .ToArray();

                var bankWithdraws = bankWithdrawList
                    .Select(w =>
                        new BankTransactionModel(w.amount > 0 ? w.from : -1, w.amount > 0 ? -1 : w.from, Math.Abs(-w.amount), w.amount > 0 ? "Bank-Abhebung" : "Bank-Einzahlung", w.date).ToJson())
                    .ToArray();

                this.transactions = transactions1.Concat(transactions2).Concat(withdraws).Concat(bankWithdraws).ToArray();
            }
        }

        private void onSmartphoneRequestTransactions(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = new PhoneRequestBankTransactionsCefEvent();
            cefData.PopulateJson(evt.Data);

            using(var db = new ChoiceVDb()) {
                var transactions = db.banktransactions.Where(t => t.from == cefData.accountId && t.date >= DateTime.UtcNow.Date.AddDays(-30)).ToList();

                var transactions2 = db.banktransactionslogs.Where(t => (t.from == cefData.accountId || t.to == cefData.accountId) && t.date >= DateTime.UtcNow.Date.AddDays(-30)).ToList();
                var withDraws = db.bankatmwithdraws.Where(w => w.from == cefData.accountId && w.date >= DateTime.UtcNow.Date.AddDays(-30)).ToList();
                var bankWithDraws = db.bankbankwithdraws.Where(w => w.from == cefData.accountId && w.date >= DateTime.UtcNow.Date.AddDays(-30)).ToList();


                player.emitCefEventNoBlock(new PhoneAnswerBankTransactions(cefData.accountId, transactions, transactions2, withDraws, bankWithDraws));
            }
        }

        #endregion

        #region New Transaction

        private class PhoneNewBankTransactionsCefEvent {
            public long from;
            public long to;
            public int pin;
            public string owner;
            public string use;
            public decimal amount;
        }

        private void onSmartphoneMakeTransaction(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = new PhoneNewBankTransactionsCefEvent();
            cefData.PopulateJson(evt.Data);

            var mess = "";
            var company = BankController.getBankAccountCompany(cefData.from);
            if(company == null) {
                BankController.transferMoney(cefData.from, cefData.to, cefData.amount, cefData.use, out mess, true, cefData.pin);
            } else {
                if(CompanyController.hasPlayerPermission(player, company, "BANK_MANAGEMENT")) {
                    BankController.transferMoney(cefData.from, cefData.to, cefData.amount, cefData.use, out mess, false);
                } else {
                    mess = "Du hast keine Berechtigung für diese Aktion!";
                }
            }
            sendPhoneNotificationToPlayer(player, mess);
        }

        #endregion
    }

}
