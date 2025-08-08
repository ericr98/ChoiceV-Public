using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Vehicles;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.ListMenuItem;

namespace ChoiceVServer.Controller {
    public class VehicleRepairController : ChoiceVScript {
        public VehicleRepairController() {
            InteractionController.addVehicleInteractionElement(
                new ConditionalGeneratedPlayerInteractionMenuElement(
                    "Fahrzeugschaden",
                    openVehicleDamageMenu,
                    sender => sender is IPlayer player,
                    target => target is ChoiceVVehicle vehicle && vehicle.LockState == VehicleLockState.Unlocked

                )
            );

            InteractionController.addVehicleInteractionElement(
                new ConditionalGeneratedPlayerInteractionMenuElement(
                    "Schlüssel tauschen",
                    openVehicleKeyKitMenu,
                    sender => sender is IPlayer player && player.getInventory().hasItem<ToolItem>(t => t.Flag == SpecialToolFlag.VehicleKeyKit),
                    target => target is ChoiceVVehicle vehicle && vehicle.LockState == VehicleLockState.Unlocked

                )
            );


            EventController.addMenuEvent("VEHICLE_REPAIR_DAMAGE", onVehicleRepairDamage);

            EventController.addMenuEvent("VEHICLE_MOTOR_COMPARTMENT_OPEN", onVehicleMotorCompartmentOpen);
            EventController.addMenuEvent("VEHICLE_MOTOR_COMPARTMENT_CLOSE", onVehicleMotorCompartmentClose);

            EventController.addMenuEvent("CREATE_NEW_VEHICLE_KEY", onCreateNewVehicleKey);
            EventController.addMenuEvent("CHANGE_VEHICLE_KEY_LOCK", onCreateVehicleKeyLock);

            EventController.addMenuEvent("ON_RESPAWN_VEHICLE", onVehicleRespawnVehicle);
        }

        private Menu openVehicleDamageMenu(IEntity sender, IEntity target) {
            var menu = new Menu("Fahrzeugschaden", "Siehe und repariere Beschädigungen");

            if(target is not ChoiceVVehicle vehicle) {
                return menu;
            }

            var player = sender as IPlayer;

            menu.addMenuItem(new ClickMenuItem("Motorraum öffnen/anschauen", "Öffne den Motorraum des Fahrzeugs, oder schaue ihn dir an, wenn er schon offen ist", "", "VEHICLE_MOTOR_COMPARTMENT_OPEN").withData(new Dictionary<string, dynamic> { { "Vehicle", vehicle } }).needsConfirmation("Motorraum öffnen?", "Muss geschlossen werden zum fahren!"));
            if(vehicle.MotorCompartmentGame != null && !vehicle.MotorCompartmentGame.anyStashedComponents()) {
                menu.addMenuItem(new ClickMenuItem("Motorraum schließen", "Schließe den Motorraum. Nur möglich, wenn alle Teile eingebaut sind", "", "VEHICLE_MOTOR_COMPARTMENT_CLOSE").withData(new Dictionary<string, dynamic> { { "Vehicle", vehicle } }));
            }
            vehicle.VehicleDamage.read(vehicle);

            if(vehicle.IsDestroyed) {
                menu.addMenuItem(new ClickMenuItem("Fahrzeug neu spawnen", "Falls sich das Fahrzeug im komplett zerstörten Zustand befindet kannst du es hiermit neu spawnen. Nach dem Zerstören sollte das Fahrzeug automatisch neu spawnen!", "", "ON_RESPAWN_VEHICLE", MenuItemStyle.yellow).needsConfirmation("Fahrzeug neu spawnen?", "Fahrzeug wirklich neu spawnen?").withData(new Dictionary<string, dynamic> { { "Vehicle", vehicle } }));
            }

            var subMenu = new Menu("Äußere Schäden", "Was möchtest du tun?");

            var elements = vehicle.VehicleDamage.getDamageElements(vehicle);
            var items = player.getInventory().getItems<VehicleRepairItem>(i => true);
            if(items.Any() && elements.Any(e => e.Amount > 0)) {
                var tool = player.getInventory().getItem<ToolItem>(t => t.Flag == SpecialToolFlag.Screwdriver);

                if(tool != null) {
                    subMenu.addMenuItem(new ListMenuItem("Teil einbauen", "Baue das Teil ein", items.Select(i => i.Name).Distinct().ToArray(), "VEHICLE_REPAIR_DAMAGE").needsConfirmation("Teil einbauen?", "Teil wirklich einbauen?").withData(new Dictionary<string, dynamic> { { "Vehicle", vehicle } }));
                } else {
                    subMenu.addMenuItem(new StaticMenuItem("Kein Werkzeug dabei", "Du hast einbaubare Teile dabei, aber nicht das passende Werkzeug!", "", MenuItemStyle.yellow));
                }
            }

            foreach(var element in elements) {
                if(element.Amount > 0) {
                    var name = VehicleDamage.getDamageElementName(element.Type);

                    if(vehicle.hasData($"REPAIR_TYPE_{element.Type}")) {
                        var amount = int.Parse(vehicle.getData($"REPAIR_TYPE_{element.Type}"));

                        if(amount >= element.Amount) {
                            subMenu.addMenuItem(new StaticMenuItem(name, $"An dem Fahrzeug befinden sich {element.Amount} beschädigte {name}. Es wurden bereits {amount} Ersatzteile eingebaut.", $"Eingebaut: {amount}/{element.Amount}", MenuItemStyle.green));
                        } else {
                            subMenu.addMenuItem(new StaticMenuItem(name, $"An dem Fahrzeug befinden sich {element.Amount} beschädigte {name}. Es wurden bereits {amount} Ersatzteile eingebaut.", $"Eingebaut: {amount}/{element.Amount}", MenuItemStyle.yellow));
                        }
                    } else {
                        subMenu.addMenuItem(new StaticMenuItem(name, $"An dem Fahrzeug befinden sich {element.Amount} beschädigte{name}.", $"Beschädigt: {element.Amount}", MenuItemStyle.red));
                    }
                }
            }

            if(subMenu.getMenuItemCount() == 0) {
                menu.addMenuItem(new StaticMenuItem("Keine äußeren Schäden", "An dem Fahrzeug befinden sich keine äußeren Schäden.", "", MenuItemStyle.green));
            } else {
                menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
            }


            return menu;
        }

        private Menu openVehicleKeyKitMenu(IEntity sender, IEntity target) {
            var vehicle = target as ChoiceVVehicle;
            var player = sender as IPlayer;

            var hasCard = player.getInventory().hasItem<VehicleRegistrationCard>(c => c.VehicleId == vehicle.VehicleId);

            var menu = new Menu("Fahrzeugschlüssel tauschen", "Was möchtest du tun?");

            menu.addMenuItem(new StaticMenuItem("Schlüsselversion", $"Die Schlüsselversion des Fahrzeugs ist {vehicle.KeyLockVersion}.", $"{vehicle.KeyLockVersion}"));

            if(hasCard) {
                menu.addMenuItem(new ClickMenuItem("Neuen Schlüssel anfertigen", "Fertige einen neuen Schlüssel an.", "", "CREATE_NEW_VEHICLE_KEY").needsConfirmation("Neuen Schlüssel fertigen?", "Wirklich neuen Schlüssel fertigen?").withData(new Dictionary<string, dynamic> { { "Vehicle", vehicle } }));
            } else {
                menu.addMenuItem(new StaticMenuItem("Schlüssel anfertigen nicht möglich", "Dir fehlt die Fahrzeuginhaberkarte des Fahrzeugs!", "", MenuItemStyle.red));
            }

            if(hasCard) {
                menu.addMenuItem(new ClickMenuItem("Schlösser austauschen", "Ändere alle Schlösser des Fahrzeuges und erstelle einen neuen Schlüssel.", "", "CHANGE_VEHICLE_KEY_LOCK").needsConfirmation("Alle Schlösser tauschen?", "Wirklich alle Schlösser tauschen?").withData(new Dictionary<string, dynamic> { { "Vehicle", vehicle } }));
            } else {
                menu.addMenuItem(new StaticMenuItem("Schlösser austauschen nicht möglich", "Dir fehlt die Fahrzeuginhaberkarte des Fahrzeugs!", "", MenuItemStyle.red));
            }

            return menu;
        }

        /// <summary>
        /// Method that is called when player uses a vehicle repair item to repair vehicle damage.
        /// </summary>
        /// <param name="player">The player who used the item.</param>
        /// <param name="itemEvent">Event name.</param>
        /// <param name="menuItemId">The id of the menu item.</param>
        /// <param name="data">Dictionary containing additional data.</param>
        /// <param name="menuItemCefEvent">Event object containing selected menu item data.</param>
        /// <returns>Returns true if vehicle damage repair is successful, otherwise returns false.</returns>
        private bool onVehicleRepairDamage(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as ListMenuItemEvent;
            var vehicle = (ChoiceVVehicle)data["Vehicle"];
            var item = player.getInventory().getItem<VehicleRepairItem>(i => i.Name == evt.currentElement);

            if(item != null) {
                var anim = AnimationController.getAnimationByName("SCREWING_STANDING");
                AnimationController.animationTask(player, anim, () => {
                    if(player.getInventory().removeItem(item)) {
                        var tool = player.getInventory().getItem<ToolItem>(t => t.Flag == SpecialToolFlag.Screwdriver);

                        if(tool != null) {
                            tool.use(player);

                            var already = 0;
                            if(vehicle.hasData($"REPAIR_TYPE_{item.RepairType}")) {
                                already = int.Parse(vehicle.getData($"REPAIR_TYPE_{item.RepairType}"));
                            }

                            vehicle.setPermanentData($"REPAIR_TYPE_{item.RepairType}", (already + 1).ToString());
                            player.sendNotification(NotifactionTypes.Success, $"Du hast erfolgreich ein {item.Name} eingebaut.", $"{item.Name} eingebaut", NotifactionImages.Car);

                            Logger.logDebug(LogCategory.Player, LogActionType.Updated, player, $"Player repaired vehicle {vehicle.VehicleId} with a {item.Name} item.");

                            if(checkIfVehicleIsFullyRepaired(vehicle)) {
                                vehicle.repairAllDamages();

                                player.sendNotification(NotifactionTypes.Success, $"Du hast das Fahrzeug komplett repariert.", $"Fahrzeug komplett repariert", NotifactionImages.Car);
                            }
                        }
                    }
                }, null, true, 1, TimeSpan.FromSeconds(10));
            }

            return true;
        }

        private bool checkIfVehicleIsFullyRepaired(ChoiceVVehicle vehicle) {
            var elements = vehicle.VehicleDamage.getDamageElements(vehicle);

            foreach(var element in elements) {
                if(vehicle.hasData($"REPAIR_TYPE_{element.Type}")) {
                    var amount = int.Parse(vehicle.getData($"REPAIR_TYPE_{element.Type}"));

                    if(amount < element.Amount) {
                        return false;
                    }
                } else {
                    if(element.Amount > 0) {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool onVehicleMotorCompartmentOpen(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (ChoiceVVehicle)data["Vehicle"];

            if(vehicle.MotorCompartmentGame == null) {
                var compartment = VehicleMotorCompartmentController.getCompartmentForVehicle(vehicle);
                if(compartment == null) {
                    player.sendBlockNotification("Für dieses Fahrzeug ist kein Motorraum verfügbar.", "Kein Motorraum", NotifactionImages.Car);
                    return false;
                }

                var game = compartment.createAsMinigame(vehicle, (_, _, _, _) => {
                    return true;
                });

                vehicle.MotorCompartmentGame = game;

                game.showToPlayer(player);
            } else {
                vehicle.MotorCompartmentGame.showToPlayer(player);
            }

            if(vehicle.DbModel.trunkInBack == 1) {
                vehicle.setHoodState(true);
            } else {
                vehicle.setTrunkState(true);
            }

            return true;
        }

        private bool onVehicleMotorCompartmentClose(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (ChoiceVVehicle)data["Vehicle"];

            vehicle.MotorCompartmentGame = null;

            if(vehicle.DbModel.trunkInBack == 1) {
                vehicle.setHoodState(false);
            } else {
                vehicle.setTrunkState(false);
            }

            return true;
        }

        private bool onCreateNewVehicleKey(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (ChoiceVVehicle)data["Vehicle"];
            var item = player.getInventory().getItem<ToolItem>(t => t.Flag == SpecialToolFlag.VehicleKeyKit);

            if(player.getInventory().hasItem<VehicleRegistrationCard>(c => c.VehicleId == vehicle.VehicleId) && item != null) {
                var key = new VehicleKey(InventoryController.getConfigItemForType<VehicleKey>(), vehicle);

                if(player.getInventory().addItem(key)) {
                    item.use(player);

                    Logger.logDebug(LogCategory.Player, LogActionType.Created, player, $"Player created vehicle key for {vehicle.VehicleId} with a {item.Name} item.");
                    player.sendNotification(NotifactionTypes.Success, $"Du hast einen neuen Schlüssel für das Fahrzeug angefertigt.", $"Neuen Schlüssel angefertigt", NotifactionImages.Car);
                } else {
                    player.sendNotification(NotifactionTypes.Danger, $"Du hast keinen Platz im Inventar!", $"Kein Platz", NotifactionImages.Car);
                }
            }


            return true;
        }

        private bool onCreateVehicleKeyLock(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (ChoiceVVehicle)data["Vehicle"];
            var item = player.getInventory().getItem<ToolItem>(t => t.Flag == SpecialToolFlag.VehicleKeyKit);

            if(player.getInventory().hasItem<VehicleRegistrationCard>(c => c.VehicleId == vehicle.VehicleId) && item != null) {
                vehicle.KeyLockVersion++;

                var key = new VehicleKey(InventoryController.getConfigItemForType<VehicleKey>(), vehicle);
                player.getInventory().addItem(key, true);

                Logger.logDebug(LogCategory.Player, LogActionType.Updated, player, $"Player swapped locks for {vehicle.VehicleId} with a {item.Name} item.");
                player.sendNotification(NotifactionTypes.Success, $"Du hast die Schlösser für das Fahrzeug gewechselt.", $"Neuen Schlüssel angefertigt", NotifactionImages.Car);
            }

            return true;
        }

        private bool onVehicleRespawnVehicle(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (ChoiceVVehicle)data["Vehicle"];
            VehicleController.onVehicleDestroyed(vehicle);

            return true;
        }
    }
}
