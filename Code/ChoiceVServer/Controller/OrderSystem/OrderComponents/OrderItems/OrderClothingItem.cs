using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.OrderSystem.OrderComponents.OrderItems {
    internal class OrderClothingItem : OrderItem {
        public int ClothingVariation { get; }

        public OrderClothingItem(string name, int configClothingId, int clothingVariation, int shipId, int? containerId, int orderCompany, int? shippingCompany, int amount, string category) : base(name, configClothingId, shipId, containerId, orderCompany, shippingCompany, amount, category) {
            ClothingVariation = clothingVariation;
        }

        public OrderClothingItem(orderitem orderItem) : base(orderItem) {
            var split = orderItem.data.Split('#');
            ConfigElementId = int.Parse(split[0]);
            ClothingVariation = int.Parse(split[1]);
        }

        public override Menu harborMasterMenu(OrderShip ship) {
            var itemMenu = new Menu(Name, "Was möchtest du tun?");

            itemMenu.addMenuItem(new StaticMenuItem("Anzahl:", $"{Amount} {Name} befinden sich in der Lieferung", $"{Amount}"));

            if (ship != null && ship.StartTime > DateTime.Now) {
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

            menu.addMenuItem(new StaticMenuItem("Anzahl:", $"Es befinden sich {Amount}x {Name} in dem Container", $"{Amount}"));

            return menu;
        }

        public override configitem getConfigItem() {
            var clothing = ClothingController.getClothingVariation(ConfigElementId, ClothingVariation);

            if(clothing.clothing.componentid == 11) {
                return InventoryController.getConfigItemForType<TopClothingItem>(c => c.additionalInfo == clothing.clothing.componentid.ToString());
            } else {
                return InventoryController.getConfigItemForType<ClothingItem>(c => c.additionalInfo == clothing.clothing.componentid.ToString());
            }
        }
    }
}
