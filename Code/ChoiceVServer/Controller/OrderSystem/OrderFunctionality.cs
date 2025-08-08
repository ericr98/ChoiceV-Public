using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Controller.Companies.CompanyFunctionality;

namespace ChoiceVServer.Controller.OrderSystem {
    public class OrderSystemFunctionality : CompanyFunctionality {
        public OrderSystemFunctionality() : base() { }

        public OrderSystemFunctionality(Company company) : base(company) {
            Company = company;
        }

        public override string getIdentifier() {
            return "ORDER_FUNCTIONALITY";
        }

        public override SelectionInfo getSelectionInfo() {
            return new SelectionInfo(getIdentifier(), "Order System Anbindung", "Ermöglicht es das Ordersystem von überall aus aufzurufen");
        }

        public override void onLoad() {
            Company.registerCompanySelfElement(
                "SHIPPING_OPEN_HARBORMASTER",
                shippingIDOSGenerator,
                onShippingIDOS,
                "ORDER_BUY"
            );
        }

        public override void onRemove() {
            Company.unregisterCompanyElement("SHIPPING_OPEN_HARBORMASTER");
        }

        private MenuElement shippingIDOSGenerator(Company company, IPlayer player) {
            return new ClickMenuItem("Güterkontrolle öffnen", "Öffne die Güterkontroller und das IDOS System", "", "OPEN_HARBOR_SYSTEM");
        }

        private void onShippingIDOS(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(subEvent == "OPEN_HARBOR_SYSTEM") {
                var menu = OrderController.getHarborMasterMenu(player);
                player.showMenu(menu);
            }
        }
    }
}
