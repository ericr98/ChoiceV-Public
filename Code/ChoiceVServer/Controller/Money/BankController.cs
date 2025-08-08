using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Controller.Phone;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller {
    public class NPCBankModule : NPCModule {
        public NPCBankModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped) { }

        public NPCBankModule(ChoiceVPed ped) : base(ped) { }

        public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
            return new StaticMenuItem("Bank Angestellter Modul", "Ein Module für Bankangstelle NPCs", "");
        }

        public override List<MenuItem> getMenuItems(IPlayer player) {
            var list = new List<MenuItem>();

            foreach(var account in BankController.getPlayerBankAccounts(player)) {
                var accountMenu = new Menu(account.id.ToString(), $"{account.name}");
                accountMenu.addMenuItem(new StaticMenuItem("Kontostand", $"Auf dem Konto befinden sich ${account.balance}", $"${account.balance}"));

                if((BankAccountType)account.accountType == BankAccountType.GiroKonto) {
                    var data = new Dictionary<string, dynamic>() {
                        {"AccountId", account.id},
                    };

                    var inputMenu = new Menu("Bargeld einzahlen", "Zahle Bargeld in ein Girokonto ein");
                    inputMenu.addMenuItem(new InputMenuItem("Einzahlmenge", "Gib die Menge an Geld ein, die du einzahlen möchtest", $"${player.getCash()}", ""));
                    inputMenu.addMenuItem(new InputMenuItem("Herkunft", "Gib die Herkunft bzw. den Grund der erhöhten Bargeldmenge an", $"z.B. Lotteriegewinn", ""));
                    inputMenu.addMenuItem(new MenuStatsMenuItem("Bargeld einzahlen", "Zahle das angegebene Bargeld an. Falsch angegebene Daten sind rechtlich belangbar!", "ON_BANK_STORE_INPUT_MONEY", MenuItemStyle.green).withData(data).needsConfirmation("Geld einzahlen?", "Geld wirklich einzahlen?"));
                    accountMenu.addMenuItem(new MenuMenuItem(inputMenu.Name, inputMenu));

                    var outputMenu = new Menu("Bargeld auszahlen", "Lasse dir Geld aus deinem Konto auszahlen");
                    outputMenu.addMenuItem(new InputMenuItem("Auszahlmenge", "Gib die Menge an Geld ein, die du einzahlen möchtest", $"${account.balance}", ""));
                    outputMenu.addMenuItem(new InputMenuItem("Grund", "Gib den Grund der Bargeldauszahlung an", $"z.B. Autokauf", ""));
                    outputMenu.addMenuItem(new MenuStatsMenuItem("Bargeld auszahlen", "Zahle das angegebene Bargeld an. Falsch angegebene Daten sind rechtlich belangbar!", "ON_BANK_STORE_WITHDRAW_MONEY", MenuItemStyle.green).withData(data).needsConfirmation("Geld einzahlen?", "Geld wirklich einzahlen?"));
                    accountMenu.addMenuItem(new MenuMenuItem(outputMenu.Name, outputMenu));
                }

                list.Add(new MenuMenuItem(accountMenu.Name, accountMenu, Constants.BankCompaniesToMenuStyle[(BankCompanies)account.bankId]));
            }

            return list;
        }

        public override void onRemove() { }
    }

    public class BankController : ChoiceVScript {
        public static List<Bank> AllBanks = new List<Bank>();

        private static object BankMutex = new object();

        public BankController() {
            createBanks();
            InvokeController.AddTimedInvoke("Bank-Updater", updateBanks, TimeSpan.FromMinutes(2), true);

            AccountReward.AccountDataValueChangedDelegate += onValueChange;
            PedController.addNPCModuleGenerator("Bank-Modul", bankModuleGenerator, bankModuleCallback);

            EventController.addMenuEvent("ON_BANK_STORE_INPUT_MONEY", onBankStoreInputMoney);
            EventController.addMenuEvent("ON_BANK_STORE_WITHDRAW_MONEY", onBankStoreWithdrawMoney);
        }

        private bool onBankStoreInputMoney(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            withDrawPutInViaBank(player, data, menuItemCefEvent as MenuStatsMenuItemEvent, false);

            return true;
        }

        private bool onBankStoreWithdrawMoney(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            withDrawPutInViaBank(player, data, menuItemCefEvent as MenuStatsMenuItemEvent, true);

            return true;
        }

        private static void withDrawPutInViaBank(IPlayer player, Dictionary<string, dynamic> data, MenuStatsMenuItemEvent evt, bool withdraw) {
            var amountEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var reasonEvt = evt.elements[1].FromJson<InputMenuItemEvent>();

            var accountNumber = (int)data["AccountId"];

            decimal amount = 0;
            try {
                amount = Math.Abs(decimal.Parse(amountEvt.input));
            } catch(Exception) {
                player.sendBlockNotification("Die Eingabe war falsch!", "Eingabe falsch!");
            }

            if(!withdraw) {
                amount = -amount;

                if(Math.Abs(amount) > player.getCash()) {
                    player.sendBlockNotification($"Du hast nicht genügend Bargeld dabei!", "Betrag zu hoch", NotifactionImages.ATM);
                    return;
                }
            }

            using(var db = new ChoiceVDb()) {
                var dbAcc = db.bankaccounts.FirstOrDefault(b => b.id == accountNumber);
                if(dbAcc != null) {
                    if(dbAcc.balance < amount) {
                        player.sendBlockNotification("Du hast nicht genüged Geld auf dem Konto", "Nicht genung Geld", NotifactionImages.ATM);
                        return;
                    } else if(dbAcc.balance <= 0 && amount > 0) {
                        player.sendBlockNotification("Es muss mehr als $0 abgehoben werden", "Mehr als $0 angeben", NotifactionImages.ATM);
                        return;
                    }

                    var withDraw = new bankbankwithdraw {
                        amount = amount,
                        date = DateTime.Now,
                        from = dbAcc.id,
                        reason = reasonEvt.input,
                    };
                    db.bankbankwithdraws.Add(withDraw);

                    dbAcc.balance -= amount;

                    if(amount > 0) {
                        player.addCash(amount);
                        player.sendNotification(NotifactionTypes.Success, $"Du hast ${amount} abgehoben", $"${amount} abgehoben", NotifactionImages.ATM);
                    } else {
                        player.removeCash(Math.Abs(amount));
                        player.sendNotification(NotifactionTypes.Success, $"Du hast ${Math.Abs(amount)} eingezahlt", $"${Math.Abs(amount)} eingezahlt", NotifactionImages.ATM);
                    }

                    db.SaveChanges();
                }
            }
        }

        private List<MenuItem> bankModuleGenerator(ref Type codeType) {
            codeType = typeof(NPCBankModule);

            return new List<MenuItem>();
        }

        private void bankModuleCallback(IPlayer player, MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
            creationFinishedCallback.Invoke(new Dictionary<string, dynamic> { });
        }

        private void onValueChange(AccountReward reward) {
            using(var db = new ChoiceVDb()) {
                var dbReward = db.bankaccountrewards.FirstOrDefault(r => r.id == reward.Id);

                if(dbReward != null) {
                    dbReward.data = reward.Data.ToJson();
                    db.SaveChanges();
                }
            }
        }

        private void createBanks() {
            using(var db = new ChoiceVDb()) {
                var libNumber = PhoneController.findPhoneNumberByComment("Liberty Bank");
                libNumber ??= PhoneController.createNewPhoneNumber("Liberty Bank");
                
                var mazeNumber = PhoneController.findPhoneNumberByComment("Maze Bank");
                mazeNumber ??= PhoneController.createNewPhoneNumber("Maze Bank");

                var fleecaNumber = PhoneController.findPhoneNumberByComment("Fleeca Bank");
                fleecaNumber ??= PhoneController.createNewPhoneNumber("Fleeca Bank");

                AllBanks.Add(new Bank(BankCompanies.LibertyBank, "Liberty Bank", libNumber.number, 655));
                AllBanks.Add(new Bank(BankCompanies.MazeBank, "Maze Bank", libNumber.number, 775));
                AllBanks.Add(new Bank(BankCompanies.FleecaBank, "Fleeca Bank", libNumber.number, 858));
            }

            using (var db = new ChoiceVDb()) {
                foreach(var acc in db.bankaccounts) {
                    var bank = AllBanks.FirstOrDefault(b => b.Id == acc.bankId);
                    bank.addKonto((BankAccountType)acc.accountType);
                }
            }
        }

        private void updateBanks(IInvoke obj) {
            lock(BankMutex) {
                using(var db = new ChoiceVDb()) {
                    var bankNameList = ((BankCompanies[])Enum.GetValues(typeof(BankCompanies))).Select(b => b.ToString()).ToList();

                    //Do transactions
                    foreach(var dbTrans in db.banktransactions.Include(b => b.toNavigation)) {
                        if(dbTrans.due <= DateTime.Now) {
                            //make transaction
                            dbTrans.toNavigation.balance += dbTrans.amount;
                            var dbTransLog = new banktransactionslog {
                                id = dbTrans.id,
                                from = dbTrans.from,
                                to = dbTrans.to,
                                amount = dbTrans.amount,
                                cost = dbTrans.cost,
                                date = dbTrans.date,
                                due = dbTrans.due,
                                message = dbTrans.message
                            };

                            var comString = ((BankCompanies)dbTrans.toNavigation.bankId).ToString();
                            var bank = AllBanks.FirstOrDefault(b => b.Id == dbTrans.toNavigation.bankId);
                            if (dbTrans.toNavigation.connectedPhonenumber != null) {
                                PhoneController.sendSMSToNumber(bank.BankPhoneNumber, dbTrans.toNavigation.connectedPhonenumber ?? -1, getTransferSMSString(dbTrans.from ?? -1, dbTrans.to, dbTrans.amount, dbTrans.cost, false, dbTrans.message));
                            }
                            
                            db.banktransactions.Remove(dbTrans);
                            db.banktransactionslogs.Add(dbTransLog);
                        }
                    };

                    //Do interest
                    foreach(var interest in db.bankaccountinterests
                            .Include(b => b.account)
                            .Include(b => b.account.banktransactionslogfromNavigations)
                            .Include(b => b.account.banktransactionslogtoNavigations)
                            ) {

                        if(interest.nextInterest <= DateTime.Now) {
                            var rewards = interest.account.bankaccountrewards.Select(r =>
                            (AccountReward)Activator.CreateInstance(Type.GetType("ChoiceVServer.Controller." + r.codeReward, false), r)
                            ).ToList();

                            foreach(var reward in rewards) {
                                var effect = reward.takeEffectOnInterest(interest);
                                if(effect == RewardReturnValue.Runout) {
                                    var dbReward = db.bankaccountrewards.FirstOrDefault(b => b.id == reward.Id);
                                    if(dbReward != null) {
                                        db.bankaccountrewards.Remove(dbReward);
                                    }
                                }
                            }

                            decimal viableAmount = 0;
                            foreach(var viableTo in interest.account.banktransactionslogtoNavigations) {
                                if(viableTo.due <= DateTime.Now - Constants.BANK_DEPOSIT_MIN_TIME) {
                                    viableAmount += viableTo.amount;
                                }
                            }

                            foreach(var viableFrom in interest.account.banktransactionslogfromNavigations) {
                                viableAmount -= viableFrom.amount;
                            }

                            var interestAmount = Convert.ToDecimal(Math.Round((float)viableAmount * interest.interestPercent, 2));
                            var now = DateTime.Now;
                            interestAmount = interestAmount > Constants.BANK_DEPOSIT_MAX_INTEREST ? Constants.BANK_DEPOSIT_MAX_INTEREST : interestAmount;
                            interest.nextInterest = now + Constants.BANK_DEPOSIT_INTEREST_FREQUENCY + TimeSpan.FromMinutes(5);
                            if(interestAmount > 0) {
                                interest.interestAmount += interestAmount;

                                var dbTrans = new banktransaction {
                                    to = interest.account.id,
                                    from = null,
                                    amount = interestAmount,
                                    cost = 0,
                                    date = now,
                                    due = now,
                                    message = "Zinsertrag",
                                };

                                db.banktransactions.Add(dbTrans);
                            }
                        }
                    }

                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Create a BankAccount for a player
        /// </summary>
        /// <param name="player">Owner of the BankAccount</param>
        /// <param name="accountType">The Type of BankAccount</param>
        /// <param name="startBalance">The starting balance</param>
        /// <param name="bank">The chosen Bank</param>
        /// <returns>An object containing all valuable Info of the bankaccount</returns>
        public static bankaccount createBankAccount(IPlayer player, BankAccountType accountType, decimal startBalance, Bank bank, bool isInfinite) {
            if(player.getMainPhoneNumber() == -1) {
                player.sendBlockNotification("Du brauchst ein Smartphone mit gültiger SIM Karte (ausgerüstet)", "Kein Smartphone ausgerüstet!", NotifactionImages.ATM);
                return null;
            }

            var rnd = new Random();
            using(var db = new ChoiceVDb()) {
                var charId = player.getCharacterId().ToString();
                var others = db.bankaccounts.Where(b => b.ownerType == (int)BankAccountOwnerType.Player && b.ownerValue == charId).ToList();

                //Create new Bankaccount number
                var number = createBankAccountNumber(bank, rnd);
                var pin = rnd.Next(1111, 9999);

                var dbAccount = new bankaccount {
                    id = number,
                    accountType = (int)accountType,
                    balance = startBalance,
                    bankId = bank.Id,
                    creationDate = DateTime.Now,
                    name = player.getCharacterName(),
                    isFrozen = 0,
                    ownerType = (int)BankAccountOwnerType.Player,
                    ownerValue = charId,
                    pin = pin,
                    connectedPhonenumber = player.getMainPhoneNumber(),
                    isInfinite = isInfinite,
                };

                db.bankaccounts.Add(dbAccount);
                db.SaveChanges();

                if(others.Count == 0) {
                    //Its the first account for the player. this account contains rewards
                    var rewards = bank.getCurrentRewards(accountType, AllBanks);
                    foreach(var reward in rewards) {
                        var dbReward = new bankaccountreward {
                            accountId = dbAccount.id,
                            codeReward = reward.GetType().Name,
                            data = reward.Data.ToJson(),
                        };

                        if(reward.takeEffectOnAccount(dbAccount) != RewardReturnValue.Runout) {
                            db.bankaccountrewards.Add(dbReward);
                        }
                    }


                    db.SaveChanges();
                }

                if(accountType == BankAccountType.DepositKonto) {
                    var dbInterest = new bankaccountinterest {
                        accountId = number,
                        interestPercent = Constants.BANK_DEPOSIT_INTEREST_PERCENT,
                        interestAmount = 0,
                        nextInterest = DateTime.Now + Constants.BANK_DEPOSIT_INTEREST_FREQUENCY,
                    };

                    db.bankaccountinterests.Add(dbInterest);
                    db.SaveChanges();
                }

                if(accountType == BankAccountType.GiroKonto && player.getMainBankAccount() == -1) {
                    var dbPlayer = db.characters.FirstOrDefault(c => c.id == player.getCharacterId());
                    if(dbPlayer != null) {
                        dbPlayer.bankaccount = number;
                        var notImg = NotifactionImages.Liberty;
                        switch(bank.BankType) {
                            case BankCompanies.FleecaBank:
                                notImg = NotifactionImages.Fleeca;
                                break;
                            case BankCompanies.MazeBank:
                                notImg = NotifactionImages.Maze;
                                break;
                        }

                        db.SaveChanges();
                        player.sendNotification(NotifactionTypes.Info, "Dieser Account ist nun dein Hauptkonto. Deinen Hauptkonto kannst du an einem unserer ATM ändern", "Haupt-Bankaccount gesetzt", notImg);
                    } else {
                        player.ban("Du hast keinen Character! Melde dich sofort im Support!");
                    }
                }

                bank.addKonto(accountType);

                return dbAccount;
            }
        }

        /// <summary>
        /// Create a BankAccount for a company
        /// </summary>
        /// <param name="company">Owner-Company of the BankAccount</param>
        /// <param name="startBalance">The starting balance</param>
        /// <param name="bank">The chosen Bank</param>
        /// <returns>An object containing all valuable Info of the bankaccount</returns>
        public static bankaccount createBankAccount(Company company, decimal startBalance, Bank bank, bool isInfinite) {
            var rnd = new Random();
            using(var db = new ChoiceVDb()) {
                var compId = company.Id.ToString();
                var others = db.bankaccounts.Where(b => b.ownerType == (int)BankAccountOwnerType.Company && b.ownerValue == compId).ToList();

                //Create new Bankaccount number
                var number = createBankAccountNumber(bank, rnd);
                var pin = rnd.Next(1111, 9999);

                var dbAccount = new bankaccount {
                    id = number,
                    accountType = (int)BankAccountType.CompanyKonto,
                    balance = startBalance,
                    bankId = bank.Id,
                    creationDate = DateTime.Now,
                    name = company.Name,
                    isFrozen = 0,
                    ownerType = (int)BankAccountOwnerType.Company,
                    ownerValue = compId,
                    pin = pin,
                    isInfinite = isInfinite,
                };

                db.bankaccounts.Add(dbAccount);
                db.SaveChanges();


                if(others.Count == 0) {
                    //Its the first account for the player. this account contains rewards
                    var rewards = bank.getCurrentRewards(BankAccountType.CompanyKonto, AllBanks);
                    foreach(var reward in rewards) {
                        var dbReward = new bankaccountreward {
                            accountId = dbAccount.id,
                            codeReward = reward.GetType().Name,
                            data = reward.Data.ToJson(),
                        };

                        if(reward.takeEffectOnAccount(dbAccount) != RewardReturnValue.Runout) {
                            db.bankaccountrewards.Add(dbReward);
                        }
                    }

                    db.SaveChanges();
                }

                bank.addKonto(BankAccountType.CompanyKonto);

                return dbAccount;
            }
        }

        /// <summary>
        /// Create a BankAccount for a Controller/System
        /// </summary>
        /// <param name="company">Owner-Controller/System of the BankAccount</param>
        /// <param name="startBalance">The starting balance</param>
        /// <param name="bank">The chosen Bank</param>
        /// <returns>An object containing all valuable Info of the bankaccount</returns>
        public static bankaccount createBankAccount(Type type, string name, BankAccountType accountType, decimal startBalance, Bank bank, bool isInfinite) {
            var rnd = new Random();
            using(var db = new ChoiceVDb()) {
                var contName = type.Name;

                //Create new Bankaccount number
                var number = createBankAccountNumber(bank, rnd);
                var pin = rnd.Next(1111, 9999);

                var dbAccount = new bankaccount {
                    id = number,
                    accountType = (int)accountType,
                    balance = startBalance,
                    bankId = bank.Id,
                    creationDate = DateTime.Now,
                    name = name,
                    isFrozen = 0,
                    isDeactivated = 0,
                    ownerType = (int)BankAccountOwnerType.Controller,
                    ownerValue = contName,
                    pin = pin,
                    isInfinite = isInfinite,
                };

                db.bankaccounts.Add(dbAccount);
                db.SaveChanges();

                if(accountType == BankAccountType.DepositKonto) {
                    var dbInterest = new bankaccountinterest {
                        accountId = number,
                        interestPercent = Constants.BANK_DEPOSIT_INTEREST_PERCENT,
                        interestAmount = 0,
                        nextInterest = DateTime.Now + Constants.BANK_DEPOSIT_INTEREST_FREQUENCY,
                    };

                    db.bankaccountinterests.Add(dbInterest);
                    db.SaveChanges();
                }

                bank.addKonto(accountType);

                return dbAccount;
            }
        }

        private static long createBankAccountNumber(Bank bank, Random rnd) {
            using(var db = new ChoiceVDb()) {
                var number = ChoiceVAPI.longRandom(000111111L, 000999999L, rnd);
                number += bank.PreAccountNumber * 1000000;
                var already = db.bankaccounts.FirstOrDefault(b => b.id == number);
                while(already != null) {
                    number = ChoiceVAPI.longRandom(000111111L, 000999999L, rnd);
                    number += bank.PreAccountNumber * 1000000;
                    already = db.bankaccounts.FirstOrDefault(b => b.id == number);
                }

                return number;
            }
        }

        public static bool getTransferMoneyBlocked(long fromAccount, long toAccount, decimal amount, out string returnMessage) {
           using(var db = new ChoiceVDb()) {
                var fromDb = db.bankaccounts.Find(fromAccount);
                var toDb = db.bankaccounts.Find(toAccount);

                return getTransferMoneyBlocked(fromDb, toDb, -1, false, amount, out returnMessage);
           } 
        }

        public static bool getTransferMoneyBlocked(bankaccount fromAccount, bankaccount toAccount, int pin, bool checkPin, decimal amount, out string returnMessage) {
            if(fromAccount == null || toAccount == null) {
                returnMessage = "Ein ausgewähltes Konto existiert nicht!";
                return false;
            } else if(checkPin && fromAccount.pin != pin) {
                returnMessage = "Der PIN ist falsch!";
                return false;
            } else if(amount <= 0) {
                returnMessage = "Der Geldbetrag ist ungültig!";
                return false;
            } else if(fromAccount.balance < amount && !fromAccount.isInfinite) {
                returnMessage = "Das ausgewählte Konto hat nicht genügend Geld!";
                return false;
            } else if(fromAccount == toAccount) {
                returnMessage = "Es kann nicht auf das selbe Konto Geld überwiesen werden!";
                return false;
            } else if(fromAccount.isFrozen == 1 || toAccount.isFrozen == 1) {
                returnMessage = "Ein Konto ist eingefroren!";
                return false;
            } else if(toAccount.bankId != fromAccount.bankId && (toAccount.accountType == (int)BankAccountType.DepositKonto ||
                      fromAccount.accountType == (int)BankAccountType.DepositKonto)) {
                returnMessage = "Transaktion auf Festgeldkonto nur von Konto derselben Bank möglich!";
                return false;
            }

            returnMessage = null;
            return true;
        }
        
        /// <summary>
        /// Transfers money from one account to another.
        /// </summary>
        /// <param name="returnMessage">Returns a message for display</param>
        /// <param name="pin">When pin != -1, then a pin check is made</param>
        /// <returns>If transaction worked</returns>
        public static bool transferMoney(long fromAccount, long toAccount, decimal amount, string message, out string returnMessage, bool checkPin, int pin = -1) {
            lock(BankMutex) {
                using(var db = new ChoiceVDb()) {
                    var l = db.bankaccounts.Include(b => b.bankaccountrewards).Where(b => b.id == fromAccount || b.id == toAccount).ToList();
                    var fromDb = l.FirstOrDefault(b => b.id == fromAccount);
                    var toDb = l.FirstOrDefault(b => b.id == toAccount);

                    var cost = 0m;

                    if(!getTransferMoneyBlocked(fromDb, toDb, pin, checkPin, amount, out returnMessage)) {
                        return false;
                    } 

                    var rewards = fromDb.bankaccountrewards.ToList().Select<bankaccountreward, AccountReward>(r =>
                        (AccountReward)Activator.CreateInstance(Type.GetType("ChoiceVServer.Controller." + r.codeReward, false), r)
                    ).ToList();


                    if(toDb.accountType == (int)BankAccountType.DepositKonto || fromDb.accountType == (int)BankAccountType.DepositKonto) {
                        //Deposit Transaction
                        var now = DateTime.Now;
                        var dbTrans = new banktransaction {
                            from = fromDb.id,
                            to = toDb.id,
                            amount = amount,
                            cost = 0,
                            date = now,
                            due = now,
                            message = message,
                        };

                        rewardTakeEffect(rewards, dbTrans);

                        db.banktransactions.Add(dbTrans);

                        fromDb.balance -= amount;
                        db.SaveChanges();
                        returnMessage = $"Überweisung von ${amount} getätigt. Dies wird wenige Minuten dauern.";
                    } else if(fromDb.bankId == toDb.bankId || fromDb.accountType == (int)BankAccountType.CompanyKonto || toDb.accountType == (int)BankAccountType.CompanyKonto) {
                        //Free and fast or Company transaction

                        var now = DateTime.Now;
                        var dbTrans = new banktransaction {
                            from = fromDb.id,
                            to = toDb.id,
                            amount = amount,
                            cost = 0,
                            date = now,
                            due = now,
                            message = message,
                        };

                        rewardTakeEffect(rewards, dbTrans);

                        db.banktransactions.Add(dbTrans);
                        fromDb.balance -= amount;
                        db.SaveChanges();
                        returnMessage = $"Überweisung von ${amount} getätigt. Sie wird wenige Minuten dauern.";
                    } else {
                        //slow and costing transaction
                        var now = DateTime.Now;
                        cost = Math.Round(Constants.DIFFERENT_BANK_TRANSCATION_PERCENT * amount, 2);
                        if(cost > Constants.DIFFERENT_BANK_TRANSACTION_COST) {
                            cost = Constants.DIFFERENT_BANK_TRANSACTION_COST;
                        }

                        var dbTrans = new banktransaction {
                            from = fromDb.id,
                            to = toDb.id,
                            amount = amount,
                            cost = cost,
                            date = now,
                            due = now + Constants.DIFFERENT_BANK_TRANSACTION_TIME,
                            message = message,
                        };
                        rewardTakeEffect(rewards, dbTrans);

                        db.banktransactions.Add(dbTrans);
                        fromDb.balance -= amount + cost;
                        db.SaveChanges();
                        returnMessage = $"Überweisung von ${amount} mit Kosten von ${cost} getätigt. Sie wird {Constants.DIFFERENT_BANK_TRANSACTION_TIME.Minutes}min dauern.";
                    }

                    var bank = AllBanks.FirstOrDefault(b => b.Id == fromDb.bankId);
                    if(bank != null) {
                        var number = bank.BankPhoneNumber;
                        if(fromDb.connectedPhonenumber != null) {
                            PhoneController.sendSMSToNumber(number, fromDb.connectedPhonenumber ?? -1, getTransferSMSString(fromAccount, toAccount, amount, cost, true, message));
                        }
                    } else {
                        Logger.logWarning(LogCategory.System, LogActionType.Event, "At least one BankCompany doesnt have a phone number!");
                    }

                    return true;
                }
            }
        }

        public static bool putMoneyInAccount(long toAccount, decimal amount, string message, out string returnMessage) {
            lock(BankMutex) {
                using(var db = new ChoiceVDb()) {
                    var dbAccount = db.bankaccounts.Find(toAccount);

                    if(dbAccount == null) {
                        returnMessage = "Das ausgewähltes Konto existiert nicht!";
                        return false;
                    } else if(amount <= 0) {
                        returnMessage = "Der Geldbetrag ist ungültig!";
                        return false;
                    } else if(dbAccount.isFrozen == 1) {
                        returnMessage = "Das Konto ist eingefroren!";
                        return false;
                    }

                    var now = DateTime.Now;
                    var dbTrans = new banktransaction {
                        from = null,
                        to = dbAccount.id,
                        amount = amount,
                        cost = 0,
                        date = now,
                        due = now,
                        message = message,
                    };

                    db.banktransactions.Add(dbTrans);
                    db.SaveChanges();
                    returnMessage = $"Überweisung von ${amount} getätigt. Sie wird wenige Minuten dauern.";
                    return true;
                }
            }
        }

        public static bool verifyBankaccount(long accountNumber, Predicate<bankaccount> predicate) {
            using(var db = new ChoiceVDb()) {
                var acc = db.bankaccounts.Find(accountNumber);

                if(acc != null) {
                    return predicate(acc);
                } else {
                    return false;
                }
            }
        }

        public static long getBankPhoneNumber(BankCompanies company) {
            using(var db = new ChoiceVDb()) {
                return AllBanks.FirstOrDefault(b => b.BankType == company).BankPhoneNumber;
            }
        }

        private static string getTransferSMSString(long from, long to, decimal amount, decimal cost, bool sendToFrom, string message) {
            if(sendToFrom) {
                if(cost <= 0) {
                    return $"${amount} werden von Konto {from} auf Konto {to} mit Grund: \"{message}\" überwiesen.";
                } else {
                    return $"${amount} werden von Konto {from} auf Konto {to} mit Grund: \"{message}\" überwiesen. Es fallen Kosten von ${cost} an.";
                }
            } else {
                if(from != -1) {
                    return $"${amount} wurden auf Konto {to} von Konto {from} mit Grund: \"{message}\" eingegangen.";
                } else {
                    return $"${amount} sind auf Konto {to} mit Grund: \"{message}\" eingegangen.";
                }
            }
        }

        private static void rewardTakeEffect(List<AccountReward> rewards, banktransaction transaction) {
            foreach(var reward in rewards) {
                reward.takeEffectOnTransaction(transaction);
            }
        }

        public static Bank getBankByType(BankCompanies type) {
            return AllBanks.FirstOrDefault(b => b.BankType == type);
        }


        public static Company getBankAccountCompany(long accountNumber) {
            using(var db = new ChoiceVDb()) {
                var acc = db.bankaccounts.Find(accountNumber);

                if(acc != null && acc.ownerType == (int)BankAccountOwnerType.Company) {
                    return CompanyController.getCompanyById(int.Parse(acc.ownerValue));
                } else {
                    return null;
                }
            }
        }

        public static List<bankaccount> getPlayerBankAccounts(IPlayer player) {
            var charId = player.getCharacterId().ToString();
            var admin = player.getCharacterData().AdminMode && player.getAdminLevel() >= 3;
            var companies = CompanyController.getCompaniesWithPermission(player, "BANK_MANAGEMENT").Select(c => c.Id.ToString()).ToList();

            using(var db = new ChoiceVDb()) {
                var list = db.bankaccounts
                    .ToList()
                    .Where(b =>
                    b.isDeactivated != 1 &&
                    (b.ownerType == (int)(BankAccountOwnerType.Player) && b.ownerValue == charId ||
                    b.ownerType == (int)(BankAccountOwnerType.Company) && companies.Exists(c => c == b.ownerValue) ||
                    b.ownerType == (int)(BankAccountOwnerType.Controller) && admin)).ToList();
                return list;
            }
        }

        public static List<bankaccount> getControllerBankaccounts(Type type) {
            using(var db = new ChoiceVDb()) {

                var list = db.bankaccounts
                    .ToList()
                    .Where(b =>
                    b.isDeactivated != 1 &&
                    b.ownerType == (int)(BankAccountOwnerType.Controller) && b.ownerValue == type.Name).ToList();
                return list;
            }
        }
    }
}
