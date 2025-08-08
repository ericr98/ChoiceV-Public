using AltV.Net.Elements.Entities;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Phone;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Money {
    public class ATMController : ChoiceVScript {
        public static List<ATM> AllAtms = new List<ATM>();

        private static decimal MAX_ATM_OUTPUT_AMOUNT_PER_DAY = 7500;
        private static decimal MAX_ATM_INPUT_AMOUNT_PER_DAY = 7500;
        public ATMController() {
            loadATMs();

            ATM.ATMCashChangeDelegate += onATMCashChange;
            ATM.ATMInteractionDelegate += onATMInteraction;

            EventController.addMenuEvent("ATM_WITHDRAW_MONEY", onAtmWithdraw);
            EventController.addMenuEvent("ATM_PUT_IN_MONEY", onAtmPutInMoney);
            EventController.addMenuEvent("ACCOUNT_CHANGE_PHONENUMBER", onAtmChangeAccountPhoneNumber);
            EventController.addMenuEvent("ACCOUNT_SET_AS_MAIN", onAtmSetAccountToMain);
            EventController.addMenuEvent("ATM_CREATE_ACCOUNT", onAtmCreateAccount);


            var addAtmMenu = new Menu("ATM hinzufügen", "Füge einen ATM an der Position hinzu");
            addAtmMenu.addMenuItem(new InputMenuItem("Name/Position", "Gib hier den Namen (z.B. die Strasse, den Ort) des ATMS ein", "", ""));
            addAtmMenu.addMenuItem(new InputMenuItem("Firma", "0: Liberty Bank, 1: Maze Bank, 2: Fleeca Bank", "", ""));
            addAtmMenu.addMenuItem(new CheckBoxMenuItem("Auf Karte anzeigen", "Ob ATM auf Karte makiert ist", false, ""));
            addAtmMenu.addMenuItem(new MenuStatsMenuItem("Bestätigen", "Erstelle einen Automaten an der Position", "SUPPORT_CREATE_NEW_ATM", MenuItemStyle.green));
            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    () => addAtmMenu,
                    3,
                    SupportMenuCategories.Bankensystem,
                    "ATM hinzufügen"
                )
            );

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    () => new ClickMenuItem("Alle ATMs anzeigen/verstecken", "Lasse dir alle ATMs als Blips auf der Karte anzeigen", "", "SUPPORT_TOGGLE_ALL_ATMS"),
                    1,
                    SupportMenuCategories.Bankensystem
                )
            );

            EventController.addMenuEvent("SUPPORT_CREATE_NEW_ATM", onSupportCreateATM);
            EventController.addMenuEvent("SUPPORT_REMOVE_ATM", onSupportRemoveATM);

            EventController.addMenuEvent("SUPPORT_TOGGLE_ALL_ATMS", onSupportToggleAllAtms);
        }
        private bool onSupportRemoveATM(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var atm = (ATM)data["Atm"];

            using(var db = new ChoiceVDb()) {
                var cfAtm = db.configatms.FirstOrDefault(a => a.id == atm.Id);

                if(cfAtm != null) {
                    db.configatms.Remove(cfAtm);
                    db.SaveChanges();

                    loadATMs();
                    player.sendNotification(NotifactionTypes.Success, "Du hast den ATM erfolgreich entfernt!", "ATM entfernt");
                }
            }
            return true;
        }

        private bool onSupportToggleAllAtms(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {

            if(player.hasData("SUPPORT_ATMS_TOGGLED")) {
                foreach(var atm in AllAtms) {
                    BlipController.destroyBlipByName(player, $"ATM_{atm.Id}");
                    BlipController.destroyBlip(player, atm.CollisionShape.Position);
                }
                player.resetData("SUPPORT_ATMS_TOGGLED");
            } else {
                foreach(var atm in AllAtms) {
                    var color = 1;
                    if(atm.Company == BankCompanies.LibertyBank) {
                        color = 29;
                    } else if(atm.Company == BankCompanies.FleecaBank) {
                        color = 25;
                    }
                    BlipController.createPointBlip(player, atm.Name, atm.CollisionShape.Position, color, 277, 255, $"ATM_{atm.Id}");
                }
            }
            return true;
        }


        private class DailyIOAmountATM {
            public DateTime Date;
            public decimal Amount;
        }

        private bool onAtmWithdraw(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var atm = (ATM)data["Atm"];
            var accountId = (int)data["AccountId"];
            var evt = data["PreviousCefEvent"] as InputMenuItemEvent;

            var amount = Math.Abs(Math.Round(decimal.Parse(evt.input), 2));

            if(amount > MAX_ATM_OUTPUT_AMOUNT_PER_DAY) {
                player.sendBlockNotification($"Der Betrag kann nicht größer sein als ${MAX_ATM_OUTPUT_AMOUNT_PER_DAY}!", "Betrag zu hoch", NotifactionImages.ATM);
                return true;
            }

            var list = new List<DailyIOAmountATM>();
            var alreadyDailyAmount = 0m;
            if(player.hasData($"ATM_DAILY_AMOUNT_IN_{(int)atm.Company}")) {
                var dailyAmounts = ((string)player.getData($"ATM_DAILY_AMOUNT_IN_{(int)atm.Company}")).FromJson<List<DailyIOAmountATM>>();
                foreach(var dailyAmount in dailyAmounts) {
                    if(dailyAmount.Date == DateTime.Today) {
                        if(dailyAmount.Amount > 0) {
                            alreadyDailyAmount += dailyAmount.Amount;
                        }
                        list.Add(dailyAmount);
                    }
                }
            }

            var restAmount = MAX_ATM_OUTPUT_AMOUNT_PER_DAY - alreadyDailyAmount;

            if(restAmount <= 0) {
                player.sendBlockNotification($"Du kannst am Tag max. ${MAX_ATM_OUTPUT_AMOUNT_PER_DAY} an ATMs auszahlen lassen. Du hast dieses Limit überschritten! Gehe zu einer Bankfiliale!", "Auszahlunglimit überschritten", NotifactionImages.ATM);
                return true;
            }

            var available = atm.withdrawCash(Math.Min(amount, restAmount));

            list.Add(new DailyIOAmountATM { Date = DateTime.Today, Amount = available });
            player.setPermanentData($"ATM_DAILY_AMOUNT_IN_{(int)atm.Company}", list.ToJson());

            doWithDrawOrPutIn(player, atm, accountId, available);

            return true;
        }

        private bool onAtmPutInMoney(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var atm = (ATM)data["Atm"];
            var accountId = (int)data["AccountId"];
            var evt = data["PreviousCefEvent"] as InputMenuItemEvent;

            var amount = Math.Abs(Math.Round(decimal.Parse(evt.input), 2));

            if(amount > player.getCash()) {
                player.sendBlockNotification($"Du hast nicht genügend Bargeld dabei!", "Betrag zu hoch", NotifactionImages.ATM);
                return true;
            }

            if(amount > MAX_ATM_INPUT_AMOUNT_PER_DAY) {
                player.sendBlockNotification($"Der Betrag kann nicht größer sein als ${MAX_ATM_INPUT_AMOUNT_PER_DAY}!", "Betrag zu hoch", NotifactionImages.ATM);
                return true;
            }

            var list = new List<DailyIOAmountATM>();
            var alreadyDailyAmount = 0m;
            if(player.hasData($"ATM_DAILY_AMOUNT_OUT_{(int)atm.Company}")) {
                var dailyAmounts = ((string)player.getData($"ATM_DAILY_AMOUNT_OUT_{(int)atm.Company}")).FromJson<List<DailyIOAmountATM>>();
                foreach(var dailyAmount in dailyAmounts) {
                    if(dailyAmount.Date == DateTime.Today) {
                        if(dailyAmount.Amount < 0) {
                            alreadyDailyAmount += Math.Abs(dailyAmount.Amount);
                        }
                        list.Add(dailyAmount);
                    }
                }
            }

            var restAmount = MAX_ATM_INPUT_AMOUNT_PER_DAY - alreadyDailyAmount;

            if(restAmount <= 0) {
                player.sendBlockNotification($"Du kannst am Tag max. ${MAX_ATM_INPUT_AMOUNT_PER_DAY} an ATMs einzahlen. Du hast dieses Limit überschritten! Gehe zu einer Bankfiliale!", "Einzahlungslimit überschritten", NotifactionImages.ATM);
                return true;
            }

            list.Add(new DailyIOAmountATM { Date = DateTime.Today, Amount = -Math.Min(amount, restAmount) });

            player.setPermanentData($"ATM_DAILY_AMOUNT_OUT_{(int)atm.Company}", list.ToJson());

            atm.refill(Math.Min(amount, restAmount));
            doWithDrawOrPutIn(player, atm, accountId, -Math.Min(amount, restAmount));

            return true;
        }

        private static void doWithDrawOrPutIn(IPlayer player, ATM atm, int accountNumber, decimal amount) {
            using(var db = new ChoiceVDb()) {
                var dbAcc = db.bankaccounts.FirstOrDefault(b => b.id == accountNumber);
                if(dbAcc != null) {
                    var cost = (BankCompanies)dbAcc.bankId == atm.Company ? 0 : Constants.ATM_DIFFERENT_BANK_WITHDRAW_COST;

                    if(dbAcc.balance < cost + amount) {
                        player.sendBlockNotification("Du hast nicht genüged Geld auf dem Konto", "Nicht genung Geld", NotifactionImages.ATM);
                        return;
                    } else if(dbAcc.balance <= 0 && amount > 0) {
                        player.sendBlockNotification("Es muss mehr als $0 abgehoben werden", "Mehr als $0 angeben", NotifactionImages.ATM);
                        return;
                    }

                    var withDraw = new bankatmwithdraw {
                        atmId = atm.Id,
                        amount = amount >= 0 ? amount + cost : amount - cost,
                        cost = cost,
                        date = DateTime.Now,
                        from = dbAcc.id,
                    };
                    db.bankatmwithdraws.Add(withDraw);

                    dbAcc.balance -= cost + amount;

                    if(amount > 0) {
                        player.addCash(amount);
                        player.sendNotification(NotifactionTypes.Success, $"Du hast ${amount} abgehoben. Es hat dich ${cost} gekostet", $"${amount} abgehoben", NotifactionImages.ATM);
                    } else {
                        player.removeCash(Math.Abs(amount));
                        player.sendNotification(NotifactionTypes.Success, $"Du hast ${Math.Abs(amount)} eingezahlt. Es hat dich ${cost} gekostet", $"${Math.Abs(amount)} eingezahlt", NotifactionImages.ATM);
                    }

                    db.SaveChanges();
                }
            }
        }

        private bool onAtmSetAccountToMain(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var accountId = (int)data["AccountId"];

            try {
                using(var db = new ChoiceVDb()) {
                    var charId = player.getCharacterId();
                    var dbChar = db.characters.Find(charId);
                    if(dbChar != null) {
                        dbChar.bankaccount = accountId;
                        db.SaveChanges();
                    }
                }
                player.sendNotification(NotifactionTypes.Success, "Konto erfolgreich als Hauptkonto gesetzt!", "Hauptkonto gesetzt", NotifactionImages.ATM);
            } catch(Exception) {
                player.sendBlockNotification("Etwas ist schiefgelaufen. Melde dich im Support. Code: MoneyDuck", "Code: MoneyDuck", NotifactionImages.ATM);
            }

            return true;
        }

        private bool onAtmChangeAccountPhoneNumber(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var atm = (ATM)data["Atm"];
            var accountId = (int)data["AccountId"];
            var evt = data["PreviousCefEvent"] as InputMenuItemEvent;
            try {
                var number = long.Parse(evt.input);
                using(var db = new ChoiceVDb()) {
                    var dbAcc = db.bankaccounts.FirstOrDefault(b => b.id == accountId);
                    if(dbAcc != null) {
                        dbAcc.connectedPhonenumber = number;
                        db.SaveChanges();
                    }
                }
            } catch(Exception) {
                player.sendBlockNotification("Die eingegebene Nummer ist keine gültige Telefonnummer!", "Falsche Eingabe", NotifactionImages.ATM);
            }

            return true;
        }

        private bool onSupportCreateATM(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var companyEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var blipEvt = evt.elements[2].FromJson<CheckBoxMenuItem>();

            try {
                var comp = (BankCompanies)int.Parse(companyEvt.input);

                CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (pos, width, height, rotation) => {
                    var dbATM = new configatm {
                        position = pos.ToJson(),
                        width = width,
                        height = height,
                        rotation = rotation,
                        location = nameEvt.input,
                        company = (int)comp,
                        deposit = 50000,
                        showBlip = blipEvt.Checked ? 1 : 0,
                    };

                    using(var db = new ChoiceVDb()) {
                        db.configatms.Add(dbATM);
                        db.SaveChanges();
                    }
                    player.sendNotification(NotifactionTypes.Success, "ATM erfolgreich erstellt!", "ATM erstellt");
                    loadATMs();
                });

            } catch(Exception) {
                player.sendBlockNotification("Eine Eingabe war nicht im richtigen Format!", "Eingabe falsch");
            }

            return true;
        }

        private void loadATMs() {
            foreach(var atm in AllAtms) {
                atm.remove();
            }

            AllAtms = new List<ATM>();

            using(var db = new ChoiceVDb()) {
                foreach(var dbAtm in db.configatms) {
                    AllAtms.Add(new ATM(dbAtm.id, dbAtm.location, dbAtm.position.FromJson(), dbAtm.width, dbAtm.height, dbAtm.rotation, (BankCompanies)dbAtm.company, dbAtm.deposit, dbAtm.lastHeistDate ?? DateTime.MinValue));
                }
            }
        }

        private void onATMCashChange(ATM atm) {
            using(var db = new ChoiceVDb()) {
                var dbAtm = db.configatms.FirstOrDefault(a => a.id == atm.Id);
                if(dbAtm != null) {
                    dbAtm.deposit = atm.Deposit;
                }
            }
        }

        private void onATMInteraction(ATM atm, IPlayer player) {
            var menu = new Menu($"ATM ({atm.Name})", "Was möchtest du tun?");
            var withDrawMenu = new Menu("Geld abheben/einzahlen", $"Automat der {Constants.BankCompaniesToName[atm.Company]}");
            var kontoMenu = new Menu("Kontoverwaltung", $"Verwalte Kontos der {Constants.BankCompaniesToName[atm.Company]}");

            foreach(var account in BankController.getPlayerBankAccounts(player)) {
                var data = new Dictionary<string, dynamic>() {
                    {"Atm", atm },
                    {"AccountId", account.id},
                };

                //ATM Stuff
                if((BankAccountType)account.accountType == BankAccountType.GiroKonto) {
                    var compMenu = new Menu(account.id.ToString(), $"{account.name}");

                    compMenu.addMenuItem(new StaticMenuItem("Kontostand", $"Auf dem Konto befinden sich ${account.balance}", $"${account.balance}"));
                    if((BankCompanies)account.bankId != atm.Company) {
                        compMenu.addMenuItem(new StaticMenuItem("Abhebegebühr", $"Durch eine andere Bank kostet das Abheben ${Constants.ATM_DIFFERENT_BANK_WITHDRAW_COST}", $"${Constants.ATM_DIFFERENT_BANK_WITHDRAW_COST}", MenuItemStyle.yellow));
                    }
                    if(atm.Deposit > 0) {
                        compMenu.addMenuItem(new InputMenuItem($"Geld abheben (max. ${MAX_ATM_OUTPUT_AMOUNT_PER_DAY}/d)", $"Hebe Geld vom Konto ab. Du kannst max. ${MAX_ATM_OUTPUT_AMOUNT_PER_DAY} pro Tag pro Bank über ATMs auszahlen. Für größere Einzahlungen gehe zur einer Bankfiliale. Alle sind möglich.", $"${account.balance}", "ATM_WITHDRAW_MONEY").withData(data).needsConfirmation("Geld abeheben?", "Wirklich Geld abheben?"));
                    } else {
                        menu.addMenuItem(new StaticMenuItem("ATM leer", "Der ATM hat kein Bargeld mehr. Warte auf eine Nachfüllung", "", MenuItemStyle.red));
                    }

                    if((BankCompanies)account.bankId == atm.Company) {
                        compMenu.addMenuItem(new InputMenuItem($"Geld einzahlen (max. ${MAX_ATM_INPUT_AMOUNT_PER_DAY}/d)", $"Zahle Geld in das Konto ein. Du kannst max. ${MAX_ATM_INPUT_AMOUNT_PER_DAY} pro Tag pro Bank über ATMs einzahlen. Für größere Einzahlungen gehe zur einer Bankfiliale. Alle sind möglich.", $"${player.getCash()}", "ATM_PUT_IN_MONEY").withData(data).needsConfirmation("Geld einzahlen?", "Wirklich Geld einzahlen?"));
                    } else {
                        compMenu.addMenuItem(new StaticMenuItem($"Geld einzahlen nicht möglich", $"Du kannst an ATMs nur Geld auf Konten der jeweiligen Besitzerbank einzahlen", "", MenuItemStyle.yellow));
                    }
                    withDrawMenu.addMenuItem(new MenuMenuItem(compMenu.Name, compMenu, Constants.BankCompaniesToMenuStyle[(BankCompanies)account.bankId]));
                }

                //Konto Stuff
                if((BankCompanies)account.bankId == atm.Company) {
                    var kontoSubMenu = new Menu("Kontoverwaltung", $"Verwalte: {account.id}");
                    kontoSubMenu.addMenuItem(new StaticMenuItem("Kontotyp", $"Das Konto hat den Typ: {BankAccountTypeToName[(BankAccountType)account.accountType]}", $"{BankAccountTypeToName[(BankAccountType)account.accountType]}"));
                    kontoSubMenu.addMenuItem(new StaticMenuItem("Kontostand", $"Der Kontostand beträgt ${account.balance}", $"{account.balance}"));
                    kontoSubMenu.addMenuItem(new InputMenuItem("Telefonnummer ändern", "Telefonnummer über welche Infos kommen", $"{account.connectedPhonenumber ?? 0}", "ACCOUNT_CHANGE_PHONENUMBER").withData(data).needsConfirmation("Wirklich ändern?", "Telefonnummer wirklich ändern?"));

                    if((BankAccountType)account.accountType == BankAccountType.GiroKonto) {
                        kontoSubMenu.addMenuItem(new ClickMenuItem("Als Hauptkonto setzen", "Über das Hauptkonto werden alle automatischen Zahlungen getätigt", "", "ACCOUNT_SET_AS_MAIN").withData(data).needsConfirmation("Als Hauptkonto setzen?", "Wirklich als Hauptkonto setzen?"));
                    }

                    kontoMenu.addMenuItem(new MenuMenuItem($"{account.id}", kontoSubMenu));
                }


            }

            menu.addMenuItem(new MenuMenuItem(withDrawMenu.Name, withDrawMenu));
            if(kontoMenu.getMenuItemCount() == 0) {
                kontoMenu.addMenuItem(new StaticMenuItem("Keine verwaltbaren Kontos!", "Du besitzt keine verwaltbaren Konten bei dieser Bank!", "", MenuItemStyle.yellow));
            }
            menu.addMenuItem(new MenuMenuItem(kontoMenu.Name, kontoMenu));

            var newAccountMenu = new Menu("Neues Konto erstellen", $"Neues Konto bei der {Constants.BankCompaniesToName[atm.Company]}");

            foreach(var accountType in ((BankAccountType[])Enum.GetValues(typeof(BankAccountType))).Where(b => b != BankAccountType.CompanyKonto).ToList()) {
                using(var db = new ChoiceVDb()) {
                    var charString = player.getCharacterId().ToString();
                    var others = db.bankaccounts.Where(b => b.ownerType == (int)BankAccountOwnerType.Player && b.ownerValue == charString && b.accountType == (int)accountType).ToList();

                    var typeMenu = new Menu(BankAccountTypeToName[accountType], $"Erstelle ein {accountType}");

                    var rewards = new List<AccountReward>();
                    if(others.Count > 0) {
                        typeMenu.addMenuItem(new StaticMenuItem("Keine Vorteile verfügbar", "Es sind keine Vorteile verfügbar, da dies nicht das Erstkonto dieses Types ist.", "", MenuItemStyle.yellow));
                    } else {
                        var rewardsMenu = new Menu("Vorteile für Erstkonten", "Alle Vorteile für die Erstellung von Erstkonten");
                        rewards = BankController.getBankByType(atm.Company).getCurrentRewards(accountType, BankController.AllBanks);
                        foreach(var reward in rewards) {
                            rewardsMenu.addMenuItem(reward.getMenuItemRepresentative());
                        }
                        typeMenu.addMenuItem(new MenuMenuItem(rewardsMenu.Name, rewardsMenu));
                    }
                    var data = new Dictionary<string, dynamic> {
                        {"Atm", atm },
                        { "AccountType", accountType },
                    };

                    if(others.Count < 5) {
                        var numberString = others.Count > 0 ? "0 Boni (kein Erstkonto)" : $"{rewards.Count} Boni";
                        typeMenu.addMenuItem(new ClickMenuItem("Konto erstellen", $"Wirklich Konto mit {numberString} erstellen", $"{rewards.Count} Boni", "ATM_CREATE_ACCOUNT", MenuItemStyle.green).withData(data).needsConfirmation("Wirklich erstellen", $"Mit {rewards.Count} Boni erstellen?"));
                    } else {
                        typeMenu.addMenuItem(new StaticMenuItem("Erstellung nicht möglich", "Du kannst nur maximal 5 Konten eines Typen erstellen!", "Max 5 Konten", MenuItemStyle.red));
                    }

                    newAccountMenu.addMenuItem(new MenuMenuItem(typeMenu.Name, typeMenu));
                }
            }

            menu.addMenuItem(new MenuMenuItem(newAccountMenu.Name, newAccountMenu));

            if(player.getCharacterData().AdminMode && player.getAdminLevel() >= 3) {
                var data = new Dictionary<string, dynamic>() {
                    {"Atm", atm },
                };
                menu.addMenuItem(new ClickMenuItem("ATM entfernen", "Entferne den ATM", "", "SUPPORT_REMOVE_ATM", MenuItemStyle.red).withData(data).needsConfirmation("ATM entfernen?", "ATM wirklich entfernen?"));
            }

            player.showMenu(menu);
        }

        private bool onAtmCreateAccount(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var accountType = (BankAccountType)data["AccountType"];
            var atm = (ATM)data["Atm"];

            var bank = BankController.getBankByType(atm.Company);

            var account = BankController.createBankAccount(player, accountType, 0, bank, false);
            if(account != null) {
                player.sendNotification(NotifactionTypes.Success, "Konto wurde erstellt. Ihre PIN wird ihnen bald per SMS geschickt! (Alternative Benachrichtigung)", "Konto erstellt", NotifactionImages.ATM);
                if(account.connectedPhonenumber != null) {
                    PhoneController.sendSMSToNumber(BankController.getBankPhoneNumber(atm.Company), account.connectedPhonenumber ?? -1, $"Konto mit Kontonummer: {account.id} und Startguthaben von ${account.balance} mit PIN {account.pin} bei ihrer {Constants.BankCompaniesToName[atm.Company]} erstellt.");
                } else {
                    player.sendNotification(NotifactionTypes.Warning, $"Ihre Bankkonto PIN ist: {account.pin}", "Konto erstellt", NotifactionImages.ATM);
                }
            }
            return true;
        }
    }
}
