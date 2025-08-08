using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Model.Menu.CheckBoxMenuItem;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Phone {
    public class CompanyPhoneModule : CompanyFunctionality {
        public CompanyPhoneModule() : base() { }

        public CompanyPhoneModule(Company company) : base(company) {
            Company = company;
        }


        public override string getIdentifier() {
            return "COMPANY_PHONE_FUNCTIONALITY";
        }

        public override List<string> getSinglePermissionsGranted() {
            return new List<string> { "COMPANY_PHONE" };
        }

        public override SelectionInfo getSelectionInfo() {
            return new SelectionInfo(getIdentifier(), "Telefonsystem", "Ermöglicht es Personen das Firmentelefon zu übernehmen");
        }

        public override void onLoad() {
            Company.registerCompanyAdminElement(
                "COMPANY_PHONE_ADMIN",
                adminPhoneNumbersGenerator,
                adminOnPhoneNumbers
            );

            Company.registerCompanySelfElement(
               "COMPANY_PHONE",
                phoneGenerator,
                onPhone,
                "COMPANY_PHONE"
            );
        }

        public static bool hasPlayerAccessToNumber(IPlayer player, long number) {
            if(player.hasData("COMPANY_NUMBERS")) {
                var dic = ((Dictionary<long, Company>)player.getData("COMPANY_NUMBERS"));
                if(dic.ContainsKey(number) && CompanyController.hasPlayerPermission(player, dic[number], "COMPANY_PHONE")) {
                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }

        }

        private MenuElement phoneGenerator(Company company, IPlayer player) {
            var numbers = Company.getSettings("PHONE_NUMBERS");
            var menu = new Menu("Firmentelefon", "Was möchtest du tun?");

            var dic = new Dictionary<long, Company>();

            if(player.hasData("COMPANY_NUMBERS")) {
                dic = (Dictionary<long, Company>)player.getData("COMPANY_NUMBERS");
            }

            foreach(var number in numbers) {
                menu.addMenuItem(new CheckBoxMenuItem($"{number.settingsValue} annehmen", $"Nehme Anrufe an die Nummer {number.settingsValue} mit Telefonnummer {number.settingsName} an", dic.ContainsKey(long.Parse(number.settingsName)), "TOGGLE_NUMBER", MenuItemStyle.normal, false, false)
                    .withData(new Dictionary<string, dynamic> { { "Number", number} }));
            }

            return menu;
        }

        private void onPhone(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(subEvent == "TOGGLE_NUMBER") {
                var number = (configcompanysetting)data["Number"];
                var evt = menuItemCefEvent as CheckBoxMenuItemEvent;
                if(!evt.check) {
                    if(player.hasData("COMPANY_NUMBERS")) {
                        ((Dictionary<long, Company>)player.getData("COMPANY_NUMBERS")).Remove(long.Parse(number.settingsName));

                        if(((Dictionary<long, Company>)player.getData("COMPANY_NUMBERS")).Count == 0) {
                            player.resetData("COMPANY_NUMBERS");
                        }
                    }

                    player.sendNotification(Constants.NotifactionTypes.Warning, $"Du nimmst keine Anrufe mehr an die Nummer {number.settingsValue} an!", "Nummer entfernt", Constants.NotifactionImages.Company, "COMPANY_PHONE_NUMBER");
                } else {
                    if(player.hasData("COMPANY_NUMBERS")) {
                        ((Dictionary<long, Company>)player.getData("COMPANY_NUMBERS"))[(long.Parse(number.settingsName))] = Company;
                    } else {
                        player.setData("COMPANY_NUMBERS", new Dictionary<long, Company> { { long.Parse(number.settingsName), Company } });
                    }

                    player.sendNotification(Constants.NotifactionTypes.Info, $"Du nimmst nun Anrufe an die Nummer {number.settingsValue} an!", "Nummer hinzugefügt", Constants.NotifactionImages.Company, "COMPANY_PHONE_NUMBER");
                }
            } 
        }

        private MenuElement adminPhoneNumbersGenerator(IPlayer player) {
            var menu = new Menu("Telefonnummern", "Was möchtest du tun?");

            var newMenu = new Menu("Neue Nummer hinzufügen", "Füge die nummer hinzu");
            newMenu.addMenuItem(new InputMenuItem("Nummer", "Die hinzuzufügende Nummer", "", InputMenuItemTypes.number, ""));
            newMenu.addMenuItem(new InputMenuItem("Kommentar/Benutzung", "Eine Benutzung für die Nummer (z.B. Chief-Büro, Notruf, etc.)", "", ""));
            newMenu.addMenuItem(new MenuStatsMenuItem("Nummer hinzufügen", "Füge die Nummer hinzu", "ADD_NUMBER", MenuItemStyle.green));
            menu.addMenuItem(new MenuMenuItem(newMenu.Name, newMenu));


            foreach(var number in Company.getSettings("PHONE_NUMBERS")) {
                menu.addMenuItem(new ClickMenuItem($"{number.settingsValue} entfernen", $"Entferne die Nummer {number.settingsValue} mit der Telefonnummer {number.settingsName} aus dem System", "", "REMOVE_NUMBER", MenuItemStyle.red)
                                       .withData(new Dictionary<string, dynamic> { { "Number", number } }));
            }

            return menu;
        }

        private void adminOnPhoneNumbers(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(subEvent == "ADD_NUMBER") {
                var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
                var numberEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
                var commentEvt = evt.elements[1].FromJson<InputMenuItemEvent>();

                Company.setSetting(long.Parse(numberEvt.input).ToString(), commentEvt.input, "PHONE_NUMBERS");

                Logger.logDebug(LogCategory.Support, LogActionType.Created, player, $"Company-Phone number {numberEvt.input} with comment {commentEvt.input} added");
                player.sendNotification(Constants.NotifactionTypes.Info, "Nummer hinzugefügt", $"Die Nummer {numberEvt.input} mit Kommentar {commentEvt.input} wurde hinzugefügt");
            } else if(subEvent == "REMOVE_NUMBER") {
                var number = data["Number"];
                Company.deleteSetting(number, "PHONE_NUMBERS");

                Logger.logDebug(LogCategory.Support, LogActionType.Removed, player, $"Company-Phone number {number} removed");
                player.sendBlockNotification("Nummer entfernt", $"Die Nummer {number} wurde entfernt");
            }
        }

        public override void onRemove() {
            Company.deleteSettings("PHONE_NUMBERS");
            Company.unregisterCompanyElement("COMPANY_PHONE");
        }
    }
}
