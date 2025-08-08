using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.OrderSystem {
    public class VehicleSpecificOrderItem : OrderItem {
        public readonly int VehicleClass;

        public VehicleSpecificOrderItem(string name, int configId, int shipId, int? containerId, int orderCompany, int? shippingCompany, int vehicleClass, int amount, string category) : base(name, configId, shipId, containerId, orderCompany, shippingCompany, amount, category) {
            OrderType = OrderType.VehicleSpecificItem;
            VehicleClass = vehicleClass;
            Data = $"{configId}#{vehicleClass}#{category}";
        }

        public VehicleSpecificOrderItem(orderitem orderItem) : base(orderItem) {
            VehicleClass = int.Parse(orderItem.data.Split("#")[1]);
        }

        public override Menu harborMasterMenu(OrderShip ship) {
            var itemMenu = new Menu(Name, "Was möchtest du tun?");

            itemMenu.addMenuItem(new StaticMenuItem("Anzahl:", $"{Amount} {Name} befinden sich in der Lieferung", $"{Amount}"));
            var cL = VehicleController.getVehicleClassById(VehicleClass);
            itemMenu.addMenuItem(new StaticMenuItem("Typ:", $"Diese Bestellung ist spezifisch für {cL.ClassName}", "Fahrzeugspezifisch"));

            if(ship != null && ship.StartTime > DateTime.Now) {
                var data = new Dictionary<string, dynamic> {
                    { "Ship", ship },
                    { "OrderItem", this }
                };

                itemMenu.addMenuItem(new InputMenuItem("Stornieren:", "Storniere eine gewisse Anzahl der Bestellungen", $"{Amount}", "ORDER_CANCEL_ORDER", MenuItemStyle.red).withData(data).needsConfirmation("Bestellung stornieren?", "Bestellung wirklich stornieren?"));
            } else {
                itemMenu.addMenuItem(new StaticMenuItem("Nicht stornierbar", "Die Bestellung ist schon losgefahren, sie ist nicht mehr stornierbar", ""));
            }

            return itemMenu;
        }

        public override Menu containerMenu() {
            var cfg = VehicleController.getVehicleClassById(VehicleClass);

            var menu = new Menu($"{Name}", "Was möchtest du tun?");

            var company = CompanyController.findCompanyById(OrderCompany);
            menu.addMenuItem(new StaticMenuItem("Bestellfirma:", $"Die Waren wurde von der Firma: {company.Name} bestellt", $"{company.Name}"));

            menu.addMenuItem(new StaticMenuItem("Anzahl:", $"Es befinden sich {Amount} {Name} in dem Container", $"{Amount}"));
            menu.addMenuItem(new StaticMenuItem("Fahrzeugklasse:", $"Das Item ist spezifisch für {cfg.ClassName}", $"{cfg.ClassName}"));

            return menu;
        }
    }
}
