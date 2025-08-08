using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.OrderSystem {
    internal class OrderVehicle : OrderComponent {
        public OrderVehicle(string name, string category, int shipId, int? containerId, int configModelId, int orderCompany, int? shippingCompany, int amount, int color) {
            RelativeId = -1;
            ShipId = shipId;
            Name = name;
            Category = category;
            ContainerId = containerId;
            ShippingCompany = shippingCompany;
            OrderCompany = orderCompany;
            OrderType = OrderType.Vehicle;
            Amount = amount;

            ConfigId = configModelId;
            Color = color;
            Data = $"{configModelId}#{color}";
        }

        public OrderVehicle(orderitem orderItem) {
            RelativeId = orderItem.shipRelativeId;
            ShipId = orderItem.shipId;
            ContainerId = orderItem.containerId;
            OrderCompany = orderItem.orderCompany;
            ShippingCompany = orderItem.shippingCompany;
            OrderType = (OrderType)orderItem.orderType;
            Name = orderItem.name;
            Category = orderItem.category;
            Amount = orderItem.amount;

            ConfigId = int.Parse(orderItem.data.Split("#")[0]);
            Color = int.Parse(orderItem.data.Split("#")[1]);
            Data = orderItem.data;
        }

        public int ConfigId { get; }
        public int Color { get; }

        public override Menu harborMasterMenu(OrderShip ship) {
            var itemMenu = new Menu(Name, "Was möchtest du tun?");

            itemMenu.addMenuItem(new StaticMenuItem("Anzahl:", $"{Amount} {Name} befinden sich in der Lieferung", $"{Amount}"));

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
            var split = Data.Split("#");

            var menu = new Menu($"{Name}", "Was möchtest du tun?");

            var company = CompanyController.findCompanyById(OrderCompany);
            menu.addMenuItem(new StaticMenuItem("Bestellfirma:", $"Die Waren wurde von der Firma: {company.Name} bestellt", $"{company.Name}"));

            menu.addMenuItem(new StaticMenuItem("Anzahl:", $"Es befinden sich {Amount}x {Name} in dem Container", $"{Amount}"));

            var cfg = VehicleController.getVehicleModelById(int.Parse(split[0]));
            menu.addMenuItem(new StaticMenuItem("Modell:", $"Das Fahrzeug ist vom Modell: {cfg.DisplayName}", $"{cfg.DisplayName}"));

            var color = VehicleColoringController.getVehicleColorById(int.Parse(split[1]));
            menu.addMenuItem(new StaticMenuItem("Farbe:", $"Das Fahrzeug hat die Farbe {color.name}", $"{color.name}"));

            return menu;
        }
    }
}
