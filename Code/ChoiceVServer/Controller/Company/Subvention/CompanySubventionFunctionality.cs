using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;


namespace ChoiceVServer.Controller {
    public class CompanySubventionFunctionality : CompanyFunctionality {
        public record SubventionElement(CompanySubventionController.SubventionType Type, decimal FlatValue, int Multiplier);
        
        private Dictionary<CompanySubventionController.SubventionType, SubventionElement> Elements;
        
        public CompanySubventionFunctionality() : base() { }

        public CompanySubventionFunctionality(Company company) : base(company) {
            
        }
        
        public override string getIdentifier() {
            return "COMPANY_SUBSTITUTION";
        }
        public override void onLoad() {
            Company.registerCompanyAdminElement(
                "COMPANY_SUBSTITUTION_MENU",
                adminMenuGenerator,
                onAdminCallback);

            Elements = Company.getSetting("SUBSTITUTION_ELEMENTS")?.FromJson<Dictionary<CompanySubventionController.SubventionType, SubventionElement>>() ?? [];
        }
        
        public override void onRemove() {
            Company.unregisterCompanyElement("COMPANY_SUBSTITUTION_MENU");
            
            Company.deleteSetting("SUBSTITUTION_ELEMENTS");
        }
        
        private MenuElement adminMenuGenerator(IPlayer player) {
            var menu = new Menu("Subventionen einstellen", "Was möchtest du tun?");

            var createMenu = new Menu("Erstellen", "Gibt die Daten ein");
            var types = Enum.GetValues<CompanySubventionController.SubventionType>().Select(t => t.ToString()).ToArray();
            createMenu.addMenuItem(new ListMenuItem("Typ", "Der Typ der Subvention", types, ""));
            createMenu.addMenuItem(new InputMenuItem("Wert", "Der Wert den die Firma für das Event kriegen soll. Nur einmal gerechnet", "", ""));
            createMenu.addMenuItem(new InputMenuItem("Multiplikator", "Wie oft der Wert mal genommen werden soll (Wie viele NPCs diese Aktion auch machen)", "", ""));
            createMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle die angegebene Subvention", "CREATE", MenuItemStyle.green)); 
            menu.addMenuItem(new MenuMenuItem(createMenu.Name, createMenu, MenuItemStyle.green));

            foreach(var sub in Elements.Values) {
                var (currentAmount, totalEvents, totalAmount)  = CompanySubventionController.getCurrentSubventionAmountForCompany(Company, sub.Type);

                var subMenu = new Menu(sub.Type.ToString(), "Was möchtest du tun?");
                subMenu.addMenuItem(new StaticMenuItem("Insgesamte Events", "Die Anzahl an insgesamt Subventionsevents", totalEvents.ToString()));
                subMenu.addMenuItem(new StaticMenuItem("Insgesamter Wert", "Der insgesamte Wert den die Firma durch diesen Subsitutionstypen erhalten hat", totalAmount.ToString()));

                var data = new Dictionary<string, dynamic> { { "Sub", sub } };
                subMenu.addMenuItem(new InputMenuItem("Aktueller Wert", "Der aktuelle Wert den die Firma an Subsitutionen für diesen Typen erhalten hat", "", "CHANGE_AMOUNT")
                    .withStartValue(currentAmount.ToString())
                    .withData(data)
                    .needsConfirmation("Aktuellen Wert ändern?", "Subventionswert wirklich ändern?"));
                
                subMenu.addMenuItem(new InputMenuItem("Wert", "Der aktuelle Wert der Subvention", "", "CHANGE_VAL")
                    .withStartValue(sub.FlatValue.ToString())
                    .withData(data)
                    .needsConfirmation("Wert ändern?", "Wert wirklich ändern?"));
                
                subMenu.addMenuItem(new InputMenuItem("Multiplikator", "Der aktuelle Multiplikator der Subvention", "", "CHANGE_MULT")
                    .withStartValue(sub.Multiplier.ToString())
                    .withData(data)
                    .needsConfirmation("Multiplikator ändern?", "Multiplikator wirklich ändern?"));
                
                subMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche diesen Typen", "", "DELETE", MenuItemStyle.red)
                    .withData(data)
                    .needsConfirmation("Subvention löschen?", "Aktuelles Guthaben wird mitgelöscht?"));
                
                menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
            }
            
            return menu;
        }
        
        private void onAdminCallback(Company company, IPlayer player, string subevent, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            if(subevent == "CREATE") {
                var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;

                var typeEvt = evt.elements[0].FromJson<ListMenuItem.ListMenuItemEvent>();
                var valueEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();
                var multiplierEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();

                var newSub = new SubventionElement(Enum.Parse<CompanySubventionController.SubventionType>(typeEvt.currentElement), decimal.Parse(valueEvt.input), int.Parse(multiplierEvt.input));
                
                Elements.Add(newSub.Type, newSub);
                player.sendNotification(Constants.NotifactionTypes.Success, "Subventionselement erfolgreich erstellt!", "");
            } else if(subevent == "DELETE") {
                var el = (SubventionElement)data["Sub"];
                
                Elements.Remove(el.Type);
                CompanySubventionController.removeSubvention(Company, el.Type);
                player.sendNotification(Constants.NotifactionTypes.Warning, "Element erfolgreich entfernt", "");
            } else if(subevent == "CHANGE_AMOUNT") {
                var evt = menuitemcefevent as InputMenuItem.InputMenuItemEvent;
                var el = (SubventionElement)data["Sub"];
                
               CompanySubventionController.changeCurrentSubventionAmount(Company, el.Type, decimal.Parse(evt.input)); 
               player.sendNotification(Constants.NotifactionTypes.Success, "Wert erfolgreich geändert", "");
            } else if(subevent == "CHANGE_VAL") {
                var evt = menuitemcefevent as InputMenuItem.InputMenuItemEvent;
                var el = (SubventionElement)data["Sub"];
                
                Elements[el.Type] = el with {
                    FlatValue = decimal.Parse(evt.input),
                };
                player.sendNotification(Constants.NotifactionTypes.Success, "Wert erfolgreich geändert", "");
            } else if(subevent == "CHANGE_MULT") {
                var evt = menuitemcefevent as InputMenuItem.InputMenuItemEvent;
                var el = (SubventionElement)data["Sub"];
                                
                
                Elements[el.Type] = el with {
                    Multiplier = int.Parse(evt.input),
                };
                player.sendNotification(Constants.NotifactionTypes.Success, "Multiplikator erfolgreich geändert", "");
            }
            
            
            Company.setSetting("SUBSTITUTION_ELEMENTS", Elements.ToJson());
        }
        
        public override SelectionInfo getSelectionInfo() {
            return new SelectionInfo(getIdentifier(), "Subventionsfunktionlität", "Ermöglicht es der Firma bei gewissen Events Subvention zu erhalten");
        }
        
        public SubventionElement getSubventionElement(CompanySubventionController.SubventionType type) {
            return Elements.ContainsKey(type) ? Elements[type] : null;
        }
    }
}
