using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ChoiceVServer.Base;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.OrderSystem {
    internal class OrderContainer {
        public OrderContainer(int id, string name, string category, Position position, CollisionShape interactCollision, List<CollisionShape> allUnloadCollisions) {
            Id = id;
            Name = name;
            Category = category;
            Position = position;
            InteractCollision = interactCollision;
            InteractCollision.OnCollisionShapeInteraction += onInteract;

            AllUnloadCollisions = allUnloadCollisions;
            AllComponents = new List<OrderComponent>();
        }

        public int Id { get; }
        public string Name { get; }
        public string Category { get; }
        public Position Position { get; }
        public CollisionShape InteractCollision { get; }
        public List<CollisionShape> AllUnloadCollisions { get; }

        public List<OrderComponent> AllComponents { get; }

        public void addComponent(OrderComponent component) {
            AllComponents.Add(component);
        }

        public bool onInteract(IPlayer player) {
            var menu = new Menu($"{Name}", "Was willst du tun?");

            var caddy = AllUnloadCollisions.SelectMany(c => c.AllEntities).FirstOrDefault(v => v is ChoiceVVehicle && (v as ChoiceVVehicle).LockState == VehicleLockState.Unlocked && (v as ChoiceVVehicle).DbModel.ModelName == "Caddy3") as ChoiceVVehicle;
            var companyShippingNames = CompanyController.getCompaniesWithFunctionality<OrderSystemFunctionality>().Select(c => c.Name).ToArray();

            foreach(var company in CompanyController.getCompanies(player)) {
                var companyMenu = new Menu(company.Name, "Was möchtest du tun?");
                var orderItems = AllComponents.Where(c => c.OrderCompany == company.Id || c.ShippingCompany == company.Id);

                if(Category == "WEAPON" && !CompanyController.hasPlayerPermission(player, company, "ORDER_WEAPON_CONTAINER")) {
                    companyMenu.addMenuItem(new StaticMenuItem("Annahme verboten!", "Du darfst keine Container mit Waffen für diese Firma öffnen", ""));
                    continue;
                }

                if(Category == "CAR" && !CompanyController.hasPlayerPermission(player, company, "ORDER_VEHICLE_CONTAINER")) {
                    companyMenu.addMenuItem(new StaticMenuItem("Annahme verboten!", "Du darfst keine Container mit Fahrzeugen für diese Firma öffnen", ""));
                    continue;
                }

                if(Category != "CAR" && Category != "WEAPON" && !CompanyController.hasPlayerPermission(player, company, "ORDER_VEHICLE_CONTAINER")) {
                    companyMenu.addMenuItem(new StaticMenuItem("Annahme verboten!", "Du darfst keine Container mit Waren für diese Firma öffnen", ""));
                    continue;
                }

                if(!company.hasFunctionality<OrderSystemFunctionality>() && orderItems.Count() > 0) {
                    var data = new Dictionary<string, dynamic> {
                        { "Container", this },
                        { "Company", company }
                    };

                    companyMenu.addMenuItem(new ListMenuItem("Für Spedition freigeben", "Gib die Waren in diesem Container für die ausgewählte Spedition frei", companyShippingNames, "ORDER_CONTAINER_ADD_COMPANY_SHIPPING").withData(data).needsConfirmation("Container freigeben?", "Diesen Container wirklich freigeben?"));
                }


                foreach(var item in orderItems) {
                    var virtItemMenu = new VirtualMenu(item.Name, () => {
                        var data = new Dictionary<string, dynamic> {
                            { "Container", this },
                            { "OrderItem", item }
                        };

                        var itemMenu = item.containerMenu();

                        if(!(item is OrderVehicle)) {
                            data.Add("PlayerInventory", player.getInventory());
                            itemMenu.addMenuItem(new ClickMenuItem("Ins Inventar nehmen", "Nimm so viele Waren ins Inventar wie möglich", "", "TAKE_ORDER_ITEM_TO_PLAYER_INVENTORY", MenuItemStyle.green).withData(data).needsConfirmation("Ins Inventar nehmen?", "Wirklich ins Inventar nehmen?"));
                        } else {
                            var veh = item as OrderVehicle;
                            itemMenu.addMenuItem(new ClickMenuItem($"{veh.Name} ausparken", "Parke das Fahrzeug auf einen freien Parkplatz", "", "PARK_ORDER_VEHICLE", MenuItemStyle.green).withData(data).needsConfirmation($"{veh.Name} ausparken?", "Fahrzeug wirklich ausparken?"));
                        }

                        if(!(item is OrderVehicle)) {
                            if(caddy != null) {
                                data.Add("VehicleInventory", caddy.Inventory);
                                itemMenu.addMenuItem(new ClickMenuItem("Auf Caddy laden", "Packe so viele Waren wie möglich in den Caddy. Es können maximal 20kg auf einmal in den Caddy geladen werden.", "max. (20kg)", "TAKE_ORDER_ITEM_TO_VEHICLE_INVENTORY", MenuItemStyle.green).withData(data).needsConfirmation("In Caddy laden?", "Wirklich in Caddy laden?"));
                            }
                        }

                        return itemMenu;
                    });
                    

                    companyMenu.addMenuItem(new MenuMenuItem(virtItemMenu.Name, virtItemMenu));
                }

                if(companyMenu.getMenuItemCount() > 0) {
                    menu.addMenuItem(new MenuMenuItem(companyMenu.Name, companyMenu));
                }
            }


            player.showMenu(menu);

            return true;
        }
    }
}
