using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.OrderSystem {
    public class OrderItem : OrderComponent {
        public string OrderOption;

        public OrderItem(string name, int configId, int shipId, int? containerId, int orderCompany, int? shippingCompany, int amount, string category) {
            RelativeId = -1;
            ShipId = shipId;
            Name = name;
            ContainerId = containerId;
            Category = category;
            OrderCompany = orderCompany;
            ShippingCompany = shippingCompany;
            OrderType = OrderType.Item;
            Amount = amount;

            ConfigElementId = configId;

            Data = $"{configId}";
        }

        public OrderItem(orderitem orderItem) {
            RelativeId = orderItem.shipRelativeId;
            ShipId = orderItem.shipId;
            Name = orderItem.name;
            ContainerId = orderItem.containerId;
            Category = orderItem.category;
            Amount = orderItem.amount;
            OrderCompany = orderItem.orderCompany;
            ShippingCompany = orderItem.shippingCompany;
            OrderType = (OrderType)orderItem.orderType;
            Data = orderItem.data;

            var split = orderItem.data.Split('#');
            ConfigElementId = int.Parse(split[0]);
            if(split.Length > 1) {
                OrderOption = split[1];
            }
        }

        //Not necessarily and item id, can also be another id of something
        public int ConfigElementId { get; protected set; }

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
            var menu = new Menu($"{Name}", "Was möchtest du tun?");

            var company = CompanyController.findCompanyById(OrderCompany);
            menu.addMenuItem(new StaticMenuItem("Bestellfirma:", $"Die Waren wurde von der Firma: {company.Name} bestellt", $"{company.Name}"));
            menu.addMenuItem(new StaticMenuItem("Anzahl:", $"Es befinden sich {Amount} {Name} in dem Container", $"{Amount}"));

            return menu;
        }

        public virtual configitem getConfigItem() {
            return InventoryController.getConfigById(ConfigElementId);
        }
    }
}
