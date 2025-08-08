using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Menu;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.InputMenuItem;

namespace ChoiceVServer.Controller {
    public class CompanyAdditionalFeaturesController : ChoiceVScript {
        public CompanyAdditionalFeaturesController() {
            EventController.addMenuEvent("CONFIRM_HIRE_PERSON", onConfirmHirePerson);


            PedController.addNPCModuleGenerator("Spricht nur mit Firmen Modul", onNPCOnlyTalksToCompanyModuleGenerator, onNPCOnlyTalksToCompanyModuleCallback);
        }

        private bool onConfirmHirePerson(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var company = (Company)data["Company"];
            var asCeo = (bool)data["AsCEO"];

            if(company.hireEmployee(player, player.getMainBankAccount(), 0, 0, asCeo) != null) {
                var employeer = (IPlayer)data["Employeer"];

                employeer.sendNotification(Constants.NotifactionTypes.Success, "Die Person hat dein Angebot angenommen!", "Firmenanstellung angenommen", Constants.NotifactionImages.Company);
                player.sendNotification(Constants.NotifactionTypes.Success, "Du hast das Angebot angenommen!", "Firmenanstellung angenommen", Constants.NotifactionImages.Company);
            }

            return true;
        }

        private List<MenuItem> onNPCOnlyTalksToCompanyModuleGenerator(ref Type codeType) {
            codeType = typeof(NPCOnlyTalksToCompanyModule);

            var list = new List<MenuItem>();

            var companies = CompanyController.AllCompanies.Select(c => c.Value.Name).ToArray();
            for (var i = 0; i < 10; i++) {
                list.Add(new InputMenuItem($"Firma {i + 1} wählen", "Wähle die Firma, die der NPC ansprechen soll", "", "").withOptions(companies));
            }

            return list;
        }

        private void onNPCOnlyTalksToCompanyModuleCallback(IPlayer player, MenuStatsMenuItem.MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
            var counter = 0;

            var list = new List<int>();
            while(counter < evt.elements.Length) {
                var subEvt = evt.elements[counter].FromJson<InputMenuItemEvent>();

                if(subEvt.input != null && subEvt.input != "") {
                    var company = CompanyController.AllCompanies.FirstOrDefault(c => c.Value.Name == subEvt.input);
                    if(company.Value != null) {
                        list.Add(company.Value.Id);
                    }
                } else { 
                    break;
                }

                counter++;
            }

            creationFinishedCallback(new Dictionary<string, dynamic> {
                { "Companies", list.ToJson() }
            });
        }
    }
}
