using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static ChoiceVServer.Model.Menu.CheckBoxMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.OrderSystem {
    public class OrderSystemCategoriesFunctionality : CompanyFunctionality {
        public OrderSystemCategoriesFunctionality() : base() { }

        public OrderSystemCategoriesFunctionality(Company company) : base(company) {
            Company = company;
        }

        public override string getIdentifier() {
            return "ORDER_CATEGORIES_FUNCTIONALITY";
        }

        public override SelectionInfo getSelectionInfo() {
            return new SelectionInfo(getIdentifier(), "Order System Kategorien", "Ermöglicht das Freischalten von Order System Kategorien für diese Firma");
        }

        public override void onLoad() {
            Company.registerCompanyAdminElement(
                "ORDER_CATEGORIES",
                orderCategoriesGenerator,
                onOrderCategories
             );
        }

        private MenuElement orderCategoriesGenerator(IPlayer player) {
            var menu = new Menu("Order System Kategorien", "Wähle eine Kategorie aus um sie zu aktivieren");
            var categoryList = getAllOrderableCategories();

            foreach(var filter in OrderController.Filters) {
                var active = categoryList.Contains(filter.Key);
                menu.addMenuItem(new CheckBoxMenuItem(filter.Value, $"Aktiviere/Deaktiviere diese Kategorie für die Firma", active, "", MenuItemStyle.normal, false, false));
            }

            menu.addMenuItem(new MenuStatsMenuItem("Änderung speichern", "Speichere die Änderung", "SAVE_CHANGES", MenuItemStyle.green));

            return menu;
        }

        private void onOrderCategories(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var filters = OrderController.Filters.ToImmutableList();

            for(var i = 0; i < evt.elements.Length - 1; i++) {
                var checkInput = evt.elements[i].FromJson<CheckBoxMenuItemEvent>();
                var filter = filters[i];
               
                if(checkInput.check) {
                    company.setSetting(filter.Key, filter.Key, "ORDERABLE_CATEGORIES");
                } else {
                    company.deleteSetting(filter.Key, "ORDERABLE_CATEGORIES");
                }
            }

            player.sendNotification(Constants.NotifactionTypes.Success, "Änderungen zu den Kategorien wurden erfolgreich gespeichert!", "Kategorien geändert!");
        }

        public List<string> getAllOrderableCategories() {
            return Company.getSettings("ORDERABLE_CATEGORIES").Select(c => c.settingsValue).ToList();
        }

        public override void onRemove() {
            Company.unregisterCompanyElement("ORDER_CATEGORIES");
            Company.deleteSettings("ORDERABLE_CATEGORIES");
        }
    }
}
