using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Shopsystem.Model;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Money {
    public class MoneyController : ChoiceVScript {
        public MoneyController() {
            InteractionController.addPlayerInteractionElement(
                new ConditionalPlayerInteractionMenuElement(
                    () => new InputMenuItem("Bargeld geben", "Gib dem Spieler Bargeld", "z.B. 100", "INTERACTION_GIVE_PLAYER_MONEY").needsConfirmation("Spieler Bargeld geben?", "Dem Spieler wirklich Bargeld geben?"),
                    s => s is IPlayer && (s as IPlayer).getCash() > 0,
                    t => t is IPlayer
                )
            );

            CharacterController.addSelfMenuElement(
                new UnconditionalPlayerSelfMenuElement(
                    "Bargeld bündeln",
                    generateCashToItemMenu
                )
            );

            EventController.addMenuEvent("INTERACTION_GIVE_PLAYER_MONEY", onPlayerGivePlayerMoney);
            EventController.addMenuEvent("PLAYER_CREATE_CASH_BUNDLE", onPlayerCreateCashBundle);
            EventController.addMenuEvent("PLAYER_CHANGE_BUNDLE_FOR_CASH", onPlayerChangeBundleForCash);

            EventController.addMenuEvent("SHOP_CLICK_BUY_ITEM", onShopBuyClickItem);
            EventController.addMenuEvent("SHOP_CLICK_SELL_ITEM", onShopSellClickItem);
        }

        private Menu generateCashToItemMenu(IPlayer player) {
            var menu = new Menu("Bargeld bündeln", "Bündel dein Bargeld.");
            menu.addMenuItem(new StaticMenuItem("Bargeld", $"Du hast aktuell ${player.getCash()} dabei.", $"${player.getCash()}"));
            menu.addMenuItem(new StaticMenuItem("Bündelgröße", $"Ein Bündel umfasst stets ${CashBundle.CASH_BUNDLE_AMOUNT}", $"${CashBundle.CASH_BUNDLE_AMOUNT}"));
            menu.addMenuItem(new InputMenuItem("Bündelanzahl", "Wähle wieviel Bündel zu schnüren möchtest", "", ""));
            menu.addMenuItem(new MenuStatsMenuItem("Bündel erstellen", "Schnüre Bündel von Bargeld", "PLAYER_CREATE_CASH_BUNDLE", MenuItemStyle.green).needsConfirmation("Bünel schnüren?", "Bündel wirklich schnüren?"));

            return menu;
        }

        private bool onPlayerGivePlayerMoney(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var targetId = (int)data["InteractionTargetId"];
            var target = ChoiceVAPI.FindPlayerByCharId(targetId);

            var amountEvt = data["PreviousCefEvent"] as InputMenuItemEvent;

            try {
                if(target != null) {
                    if(target.Position.Distance(player.Position) < Constants.MAX_PLAYER_INTERACTION_RANGE) {
                        var amount = decimal.Parse(amountEvt.input);
                        amount = decimal.Round(amount, 2);
                        if(player.getCash() >= amount) {
                            lock(player) {
                                player.removeCash(amount);
                                target.addCash(amount);

                                player.sendNotification(Constants.NotifactionTypes.Info, $"Du hast der Person ${amount} gegeben!", $"${amount} gegeben", Constants.NotifactionImages.System);
                                target.sendNotification(Constants.NotifactionTypes.Info, $"Dir wurden ${amount} zugesteckt!", $"${amount} erhalten", Constants.NotifactionImages.System);
                            }
                        } else {
                            player.sendBlockNotification("Du hast nicht genug Geld dabei!", "Zu wenig Geld", Constants.NotifactionImages.System);
                        }
                    } else {
                        player.sendBlockNotification("Der Spieler ist zu weit weg!", "Spieler weg", Constants.NotifactionImages.System);
                    }
                } else {
                    player.sendBlockNotification("Der Spieler ist weg!", "Spieler weg", Constants.NotifactionImages.System);
                }
            } catch(Exception) {
                player.sendBlockNotification("Die Eingabe war ungültig!", "Eingabe ungültig", Constants.NotifactionImages.System);
            }

            return true;
        }

        private bool onPlayerCreateCashBundle(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var statsEvt = (MenuStatsMenuItemEvent)data["PreviousCefEvent"];

            var amountEvt = statsEvt.elements[2].FromJson<InputMenuItemEvent>();

            try {
                decimal size = CashBundle.CASH_BUNDLE_AMOUNT;
                var amount = int.Parse(amountEvt.input);

                if(amount <= 0) {
                    throw new Exception();
                }

                if(size * amount <= player.getCash()) {
                    var cfg = InventoryController.getConfigItemForType<CashBundle>();
                    if(player.getInventory().MaxWeight - player.getInventory().CurrentWeight - (cfg.weight * amount) > 0) {
                        var anim = AnimationController.getAnimationByName("WORK_FRONT");
                        AnimationController.animationTask(player, anim, () => {
                            if(player.removeCash(amount * size)) {
                                var bundle = new CashBundle(cfg, amount);
                                player.getInventory().addItem(bundle);
                                player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast {amount} Geldbündel à ${size} geschnürt.", "Bündel geschnürt");
                            } else {
                                player.sendBlockNotification($"Etwas ist schiefgelaufen! Geh in den Support: Code CashBär", "Code CashBär!");
                            }
                        });
                    } else {
                        player.sendBlockNotification($"Du hast nicht genug Platz im Inventar für die Bündel!", "Zu wenig Platz");
                    }
                } else {
                    player.sendBlockNotification($"Dir fehlen ${size * amount - player.getCash()} für {amount} ${size} Bündel!", "Zu wenig Geld");
                }
            } catch(Exception) {
                player.sendBlockNotification("Eine Eingabe war ungültig", "Eingabe ungültig");
            }

            return true;
        }

        private bool onPlayerChangeBundleForCash(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var amountEvt = menuItemCefEvent as InputMenuItemEvent;

            try {
                var bundle = player.getInventory().getItem<CashBundle>(i => true);

                var wantAmount = decimal.Parse(amountEvt.input);
                decimal maxAmount = (bundle.StackAmount ?? 1) * CashBundle.CASH_BUNDLE_AMOUNT;

                if(wantAmount > maxAmount) {
                    wantAmount = maxAmount;
                } else if(wantAmount <= 0) {
                    throw new Exception();
                }

                var anim = AnimationController.getAnimationByName("WORK_FRONT");
                AnimationController.animationTask(player, anim, () => {
                    var toCashBundles = (int)Math.Floor(wantAmount / CashBundle.CASH_BUNDLE_AMOUNT);
                    if(player.getInventory().removeItem(bundle, toCashBundles)) {
                        player.addCash(toCashBundles * CashBundle.CASH_BUNDLE_AMOUNT);
                        player.sendNotification(Constants.NotifactionTypes.Success, $"Es wurden {toCashBundles} zu ${wantAmount} umgewandelt. Dir verbleiben noch ${maxAmount - wantAmount} in Bündeln.", "Bündel umgewandelt");
                    } else {
                        player.sendBlockNotification($"Etwas ist schiefgelaufen! Geh in den Support: Code CashBär", "Code CashBär!");
                    }
                });
            } catch(Exception) {
                player.sendBlockNotification("Eine Eingabe war ungültig", "Eingabe ungültig");
            }

            return true;
        }

        public delegate void PaymentMethodCallback(IPlayer player, bool successfullyPayed);
        public static List<MenuItem> getPaymentMethodsMenu(IPlayer player, decimal price, string productName, long receivingBankAccount, PaymentMethodCallback successfullBuyCallback) {
            var companies = CompanyController.getCompaniesWithPermission(player, "PAY_WITH_BANK");
            var options = new string[] { "Bar", "Hauptkonto" };
            if(companies.Count > 0) {
                options = options.Concat(companies.Select(c => $"Firmenkonto: {c.ShortName}")).ToArray();
            }
            
            return new List<MenuItem>() {
                new StaticMenuItem("Preis", $"Der Preis für {productName} ist ${price}", $"${price}"),
                new ListMenuItem("Zahlungsmethode wählen", "Wähle deine Zahlungsmethode", options, ""),
                new MenuStatsMenuItem("Kaufen", $"Kaufe {productName} für ${price}", $"${price}", "SHOP_CLICK_BUY_ITEM", MenuItemStyle.green)
                .needsConfirmation($"Für ${price} kaufen?", $"{productName} kaufen?")
                .withData(new Dictionary<string, dynamic> { { "Price", price }, { "ProductName", productName }, { "Callback", successfullBuyCallback }, { "BankAccount", receivingBankAccount } })
            };
        }

        private bool onShopBuyClickItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var paymentMethod = evt.elements[1].FromJson<ListMenuItemEvent>();

            var price = (decimal)data["Price"];
            var callback = (PaymentMethodCallback)data["Callback"];
            var receivingBankAccount = (long)data["BankAccount"];
            var productName = (string)data["ProductName"];

            if(paymentMethod.currentElement == "Bar") {
                if(player.removeCash(price)) {
                    BankController.putMoneyInAccount(receivingBankAccount, price, $"Einkauf von {productName}", out var failMessage);
                    player.sendNotification(NotifactionTypes.Success, $"{productName} erfolgreich für ${price} mittels des Bargeld gekauft", "Waren gekauft", NotifactionImages.Shop);
                    callback(player, true);
                } else {
                    player.sendBlockNotification("Du hast nicht genug Bargeld dabei!", "Zu wenig Geld", NotifactionImages.Shop);
                    callback(player, false);
                }
            } else if(paymentMethod.currentElement == "Hauptkonto") {
                if(BankController.transferMoney(player.getMainBankAccount(), receivingBankAccount, price, $"Einkauf von {productName}", out var failMessage, false)) {                  
                    player.sendNotification(NotifactionTypes.Success, $"{productName} erfolgreich für ${price} mittels des Hauptkontos gekauft", "Waren gekauft", NotifactionImages.Shop);
                    callback(player, true);
                } else {
                    player.sendBlockNotification($"Der Einkauf konnte nicht abgeschlossen werden! {failMessage}", "Fehler aufgetreten", NotifactionImages.Shop);
                    callback(player, false);
                }
            } else if(paymentMethod.currentElement.StartsWith("Firmenkonto")) {
                var companyShortName = paymentMethod.currentElement.Split(": ")[1];
                var company = CompanyController.findCompany(c => c.ShortName == companyShortName);

                if(CompanyController.hasPlayerPermission(player, company, "PAY_WITH_BANK")) {
                    if(BankController.transferMoney(company.CompanyBankAccount, receivingBankAccount, price, $"Einkauf von {productName}", out var failMessage, false)) {                  
                        player.sendNotification(NotifactionTypes.Success, $"{productName} erfolgreich für ${price} mittels des Firmenkontos von {company.Name} gekauft", "Waren gekauft", NotifactionImages.Shop);
                        callback(player, true);
                    } else {
                        player.sendBlockNotification($"Der Einkauf konnte nicht abgeschlossen werden! {failMessage}", "Fehler aufgetreten", NotifactionImages.Shop);
                        callback(player, false);
                    }   
                }
            }
            return true;
        }
        
        public delegate void PaymentMethodConfirmation(IPlayer player, bool willWork, string failMessage, Action sendMoneyCallback);
        public static List<MenuItem> getReceivingPaymentsMethodsMenu(IPlayer player, decimal price, string productName, long sendingBankAccount, PaymentMethodConfirmation callback) {
            var companies = CompanyController.getCompanies(player);
            var options = new string[] { "Bar", "Hauptkonto" };
            if(companies.Count > 0) {
                options = options.Concat(companies.Select(c => $"Firmenkonto: {c.ShortName}")).ToArray();
            }
            return new List<MenuItem>() {
                new StaticMenuItem("Preis", $"Der Preis für {productName} ist ${price}", $"${price}"),
                new ListMenuItem("Zahlungsempfänger wählen", "Wähle deinen Zahlungempfänger", options, ""),
                new MenuStatsMenuItem("Verkaufen", $"Verkaufe {productName} für ${price}", $"${price}", "SHOP_CLICK_SELL_ITEM", MenuItemStyle.green)
                    .needsConfirmation($"Für ${price} kaufen?", $"{productName} kaufen?")
                    .withData(new Dictionary<string, dynamic> { { "Price", price }, { "ProductName", productName }, { "Callback", callback }, { "BankAccount", sendingBankAccount } })
            };
        }
        
        private bool onShopSellClickItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var paymentMethod = evt.elements[1].FromJson<ListMenuItemEvent>();
         
            var price = (decimal)data["Price"];
            var callback = (PaymentMethodConfirmation)data["Callback"];
            var sendingBankAccount = (long)data["BankAccount"];
            var productName = (string)data["ProductName"];
         
            if(paymentMethod.currentElement == "Bar") {
                callback(player, true, "", () => {
                    player.addCash(price);
                    player.sendNotification(NotifactionTypes.Success, $"{productName} erfolgreich für ${price} mittels Bargeld verkauft", "Waren verkauft", NotifactionImages.Shop);    
                });
            } else if(paymentMethod.currentElement == "Hauptkonto") {
                if(BankController.getTransferMoneyBlocked(sendingBankAccount, player.getMainBankAccount(), price, out var returnMessage)) {
                    callback(player, true, returnMessage, () => {
                        if(BankController.transferMoney(sendingBankAccount, player.getMainBankAccount(), price, $"Verkauf von {productName}", out var returnMessage2, false)) {
                            player.sendNotification(NotifactionTypes.Success, $"{productName} erfolgreich für ${price} mittels des Hauptkontos als Zahlungempfängers verkauft", "Waren verkauft", NotifactionImages.Shop);
                        } else {
                            player.sendBlockNotification($"Es gab eine Fehler. Bitte melde dich im Support. Fehler bei Bezahlmenü", "Fehler aufgetreten");
                            Logger.logError(
                                $"Payment was not possible even though a check was made before! fromBankAccount: {sendingBankAccount}, toBankAccount: {player.getMainBankAccount()}, price: {price}, productName: {productName}, failMessage: {returnMessage2}",
                                "Dev Team melden. Fehler bei Bezahlmenü", player);
                        }
                    });
                } else {
                    player.sendBlockNotification($"Der Einkauf konnte nicht abgeschlossen werden! {returnMessage}", "Fehler aufgetreten", NotifactionImages.Shop);
                    callback(player, false, returnMessage, () => {});
                }
            } else if(paymentMethod.currentElement.StartsWith("Firmenkonto")) {
                var companyShortName = paymentMethod.currentElement.Split(": ")[1];
                var company = CompanyController.findCompany(c => c.ShortName == companyShortName);
         
                if(BankController.getTransferMoneyBlocked(sendingBankAccount, company.CompanyBankAccount, price, out var returnMessage)) {
                    callback(player, true, returnMessage, () => {
                        if(BankController.transferMoney(sendingBankAccount, company.CompanyBankAccount, price, $"Verkauf von {productName}", out var returnMessage2, false)) {
                            player.sendNotification(NotifactionTypes.Success, $"{productName} erfolgreich für ${price} mittels des Firmenkontos von {company.Name} als Zahlungsempfängers verkauft", "Waren verkauft", NotifactionImages.Shop);
                        } else {
                            player.sendBlockNotification($"Es gab eine Fehler. Bitte melde dich im Support. Fehler bei Bezahlmenü", "Fehler aufgetreten");
                            Logger.logError(
                                $"Payment was not possible even though a check was made before! fromBankAccount: {sendingBankAccount}, toBankAccount: {player.getMainBankAccount()}, price: {price}, productName: {productName}, failMessage: {returnMessage2}",
                                "Dev Team melden. Fehler bei Bezahlmenü", player);
                        }
                    });
                } else {
                    player.sendBlockNotification($"Der Einkauf konnte nicht abgeschlossen werden! {returnMessage}", "Fehler aufgetreten", NotifactionImages.Shop);
                    callback(player, false, returnMessage, () => {});
                }   
            }
            return true;
        }
    }
}
