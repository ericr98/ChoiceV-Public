using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.ListMenuItem;


namespace ChoiceVServer.Controller.Vehicles {
    public class VehicleTuningController : ChoiceVScript {
        public VehicleTuningController() {
            InteractionController.addVehicleInteractionElement(
                new ConditionalGeneratedPlayerInteractionMenuElement(
                    "Fahrzeugtuning",
                    openVehicleTuningMenu,
                    sender => sender is IPlayer player
                        && (CompanyController.getPlayerInCompanyWithFunctionality<VehicleTuningFunctionality>(player)
                        || player.getInventory().hasItem(i => i is ModKitItem || i is VehicleTuningItem)),
                    target => target is ChoiceVVehicle vehicle && vehicle.LockState == VehicleLockState.Unlocked && vehicle.VehicleDamage?.getDamageLevel(vehicle) <= 1
                )
            );

            InteractionController.addVehicleInteractionElement(
                new ConditionalPlayerInteractionMenuElement(
                    () => new StaticMenuItem("Fahrzeugtuning", "Das Fahrzeug ist beschädigt und kann nicht verändert werden", string.Empty, MenuItemStyle.yellow),
                    sender => sender is IPlayer player,
                    target => target is ChoiceVVehicle vehicle && vehicle.VehicleDamage?.getDamageLevel(vehicle) >= 2
                )
            );

            EventController.addMenuEvent("VEHICLE_TUNING_SELECT_MODKIT", onVehicleAddModKit);
            EventController.addMenuEvent("VEHICLE_TUNING_MODKIT_BUILD_IN", onVehicleBuildInModKit);

            EventController.addMenuEvent("VEHICLE_TUNING_PREVIEW", onVehiclePreviewTuning);

            EventController.addMenuEvent("VEHICLE_TUNING_SELECT_TYPE", onVehicleSelectType);
            EventController.addMenuEvent("VEHICLE_TUNING_SELECT_MOD", onVehicleSelectMod);
            EventController.addMenuEvent("VEHICLE_TUNING_BUILD_IN", onVehicleBuildIn);
        }

        private static Menu openVehicleTuningMenu(IEntity sender, IEntity target) {
            var player = sender as IPlayer;

            if(target is not ChoiceVVehicle vehicle) {
                return null;
            }

            var menu = new Menu("Fahrzeugtuning", "Was möchtest du tun?", true);

            menu.addMenuItem(new ClickMenuItem("Tuning: Modkit", $"Es ist {(vehicle.ModKit == 0 ? "kein" : "ein")} Modkit {vehicle.ModKit} verbaut", $"{(vehicle.ModKit == 0 ? "Fehlt" : "Verbaut")}", "VEHICLE_TUNING_SELECT_MODKIT")
                .withData(new Dictionary<string, dynamic> { { "Vehicle", target } }));


            var tuningPreviewMenu = new Menu("Tuning: Vorschau", "Zeige eine Tuning-Vorschau", p => resetVehicleTuning(player, vehicle));
            var tuningMenu = new Menu("Tuning: Einbau", "Einbau von Tuning-Teilen");


            using(var db = new ChoiceVDb()) {
                var dbVehicle = db.vehicles.Find(vehicle.VehicleId);
                if(dbVehicle is not null) {
                    var dbVehicleModel = db.configvehiclesmodels.Find(dbVehicle.modelId);
                    if(dbVehicleModel is not null) {
                        var universalMods = db.configvehicleuniversalmods.ToList();
                        var mods = db.configvehiclemods.Where(x => x.configvehiclemodels_id == dbVehicleModel.id).ToList();

                        var modTypes = db.configvehiclemodtypes.ToList();
                        foreach(var modType in modTypes) {
                            var relevantUniversalMods = universalMods.Where(x => x.configvehiclemodtypes_id == modType.id && modType.IsUniversal).ToList();
                            var relevantMods = mods.Where(x => x.configvehiclemodtypes_id == modType.id).ToList();

                            if(relevantUniversalMods.Count == 0 && relevantMods.Count == 0) {
                                continue;
                            }

                            var modIdx = vehicle.VehicleTuning.getMod(modType.ModTypeIndex);

                            //Tuningvorschau
                            var displayNames = new List<string>();
                            displayNames.AddRange(relevantUniversalMods.Select(x => x.DisplayName));
                            displayNames.AddRange(relevantMods.Select(x => x.DisplayName));

                            displayNames = displayNames.ShiftLeft(modIdx + 1);

                            if(displayNames.Count > 0) {
                                tuningPreviewMenu.addMenuItem(new ListMenuItem(modType.DisplayName, $"Siehe dir die Vorschau für {modType.DisplayName} an", displayNames.ToArray(), "VEHICLE_TUNING_PREVIEW", MenuItemStyle.normal, true, true)
                                    .withData(new Dictionary<string, dynamic> {
                                        { "Vehicle", target },
                                        { "ModTypeIndex", modType.ModTypeIndex },
                                        { "MaxModsCount", displayNames.Count }
                                    }));
                            }
                            

                            //Tuning Einbau
                            //Because altV uses an unsigned byte and GTA a signed byte, we have to subtract 1 to get the DB ModIndex.
                            //Was removed, because we always trust the model, and not altv! BUT altv ids are still +1 to gta ids, it is only added to one at the apply stage!
                            var modDisplayName = "Unbekannt";

                            var universalMod = relevantUniversalMods.FirstOrDefault(x => x.ModIndex == modIdx);
                            if(universalMod is not null) {
                                modDisplayName = universalMod.DisplayName;
                            } else {
                                var mod = relevantMods.FirstOrDefault(x => x.ModIndex == modIdx);
                                if(mod is not null) {
                                    modDisplayName = mod.DisplayName;
                                }
                            }

                            var virtualMenu = new VirtualMenu($"Tuning-Einbau: {modType.DisplayName}", () => {
                                var menu = new Menu($"Tuning-Einbau: {modType.DisplayName}", "Auswahl des Tuning-Teil");
                                var tuningItems = player.getInventory().getItems<VehicleTuningItem>(i => (byte)i.ModType == modType.ModTypeIndex && (i.Model is null || i.Model == vehicle.DbModel.id));

                                if(vehicle is { Exists: true }) {
                                    foreach(var tuningItem in tuningItems) {
                                        var menuItem = new ClickMenuItem(tuningItem.Name, tuningItem.Description, VehicleController.getVehicleClassName(tuningItem.VehicleClass), "VEHICLE_TUNING_SELECT_MOD")
                                            .withData(new Dictionary<string, dynamic> {
                                                { "Vehicle", vehicle },
                                                { "CurrentModIndex", modIdx },
                                                { "CurrentModName", modDisplayName },
                                                { "ModTypeIndex", modType.ModTypeIndex },
                                                { "ModTypeName", modType.DisplayName },
                                                { "TuningItem", tuningItem }
                                                                    });
                                        menu.addMenuItem(menuItem);
                                    }
                                }

                                if(menu.getMenuItemCount() == 0) {
                                    menu.addMenuItem(new StaticMenuItem("Keine Tuning-Teile verfügbar", "Es sind keine Tuning-Teile verfügbar", "", MenuItemStyle.yellow));
                                }

                                return menu;
                            });

                            tuningMenu.addMenuItem(new MenuMenuItem(virtualMenu.Name, virtualMenu, $"Aktuell Verbaut: {modDisplayName}", $"{modDisplayName}"));

                            //tuningMenu.addMenuItem(new ClickMenuItem(modType.DisplayName, $"Aktuell Verbaut: {modDisplayName}", $"{modDisplayName}", "VEHICLE_TUNING_SELECT_TYPE")
                            //    .withData(new Dictionary<string, dynamic> {
                            //    { "Vehicle", target },
                            //    { "CurrentModIndex", modIdx },
                            //    { "CurrentModName", modDisplayName },
                            //    { "ModTypeIndex", modType.ModTypeIndex },
                            //    { "ModTypeName", modType.DisplayName }
                            //    }));

                        }

                        menu.addMenuItem(new MenuMenuItem(tuningPreviewMenu.Name, tuningPreviewMenu));

                        menu.addMenuItem(new MenuMenuItem(tuningMenu.Name, tuningMenu));
                    }
                }
            }

            return menu;
        }

        private static bool onVehicleAddModKit(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var modKitItems = player.getInventory().getItems<ModKitItem>(i => true);
            var vehicle = (ChoiceVVehicle)data["Vehicle"];

            var menu = new Menu("Mod-Kit einbauen", "Baue das ausgewählte Mod-Kit ein");

            if(vehicle is { Exists: true }) {
                foreach(var item in modKitItems) {
                    var menuItem = new ClickMenuItem($"Mod-Kit für {VehicleController.getVehicleClassName(item.VehicleClass)}", item.Description, $"Level: {item.Level}", "VEHICLE_TUNING_MODKIT_BUILD_IN")
                        .withData(new Dictionary<string, dynamic> {
                        { "Item", item },
                        { "Vehicle", vehicle }
                        })
                        .needsConfirmation("Wirklich verbauen?", $"{item.Name} wirklich verbauen?");
                    menu.addMenuItem(menuItem);
                }
            }

            player.showMenu(menu);

            return true;
        }

        private static bool onVehicleBuildInModKit(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (ChoiceVVehicle)data["Vehicle"];
            var item = (ModKitItem)data["Item"];

            if(vehicle is not { Exists: true }) {
                return false;
            }

            if(vehicle.VehicleClassId == item.VehicleClass && vehicle.ModKitsCount >= item.Level) {
                vehicle.VehicleTuning.ModKit = item.Level;
                vehicle.applyVehicleTuning(vehicle.VehicleTuning);
                VehicleController.saveVehicleTuning(vehicle);

                Logger.logDebug(LogCategory.Player, LogActionType.Updated, player, $"Player build in modkit into {vehicle.VehicleId} with a {item.Name} item.");
                player.sendNotification(NotifactionTypes.Success, "Das Mod-Kit passt perfekt!", "Tuning geglückt", NotifactionImages.Car);
            } else {
                player.sendBlockNotification("Das Mod-Kit sieht irgendwie fehlplatziert aus?", "Tuning fehlgeschlagen", NotifactionImages.Car);
            }

            item.use(player);

            return true;
        }

        private static bool onVehiclePreviewTuning(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var cefEvent = (ListMenuItemEvent)menuItemCefEvent;
            if(cefEvent.action != "changed") {
                return false;
            }

            var vehicle = (ChoiceVVehicle)data["Vehicle"];
            if(vehicle == null) {
                return false;
            }

            var modTypeIndex = (byte)data["ModTypeIndex"];
            var maxModsCount = (int)data["MaxModsCount"];

            var modIndex = (cefEvent.currentIndex + vehicle.VehicleTuning.getMod(modTypeIndex) + 1) % maxModsCount; // (cefEvent.currentIndex + vehicle.VehicleTuning.getMod(modTypeIndex) + 1 % (maxModsCount - 1)) - 1;

            vehicle.SetMod(modTypeIndex, (byte)modIndex);
            if(modTypeIndex == (byte)VehicleModType.Horns) {
                vehicle.testHorn();
            }

            Logger.logTrace(LogCategory.Player, LogActionType.Viewed, player, $"player previewed mod: {modTypeIndex}, {(byte)modIndex} in {vehicle.VehicleId}");

            return true;
        }

        private static bool onVehicleSelectType(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (ChoiceVVehicle)data["Vehicle"];
            var currentModIndex = (int)data["CurrentModIndex"];
            var currentModName = (string)data["CurrentModName"];
            var modTypeIndex = (byte)data["ModTypeIndex"];
            var modTypeName = (string)data["ModTypeName"];

            var tuningItems = player.getInventory().getItems<VehicleTuningItem>(i => (byte)i.ModType == modTypeIndex && (i.Model is null || i.Model == vehicle.DbModel.id));

            var menu = new Menu($"Tuning-Einbau: {modTypeName}", "Auswahl des Tuning-Teil");

            if(vehicle is { Exists: true }) {
                foreach(var tuningItem in tuningItems) {
                    var menuItem = new ClickMenuItem(tuningItem.Name, tuningItem.Description, VehicleController.getVehicleClassName(tuningItem.VehicleClass), "VEHICLE_TUNING_SELECT_MOD")
                        .withData(new Dictionary<string, dynamic> {
                        { "Vehicle", vehicle },
                        { "CurrentModIndex", currentModIndex },
                        { "CurrentModName", currentModName },
                        { "ModTypeIndex", modTypeIndex },
                        { "ModTypeName", modTypeName },
                        { "TuningItem", tuningItem }
                        });
                    menu.addMenuItem(menuItem);
                }
            }

            player.showMenu(menu);

            return true;
        }

        private static bool onVehicleSelectMod(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (ChoiceVVehicle)data["Vehicle"];
            var currentModIndex = (int)data["CurrentModIndex"];
            var currentModName = (string)data["CurrentModName"];
            var modTypeIndex = (byte)data["ModTypeIndex"];
            var modTypeName = (string)data["ModTypeName"];
            var tuningItem = (VehicleTuningItem)data["TuningItem"];

            var menu = new Menu($"Tuning-Einbau: {modTypeName}", $"Auswahl der Variation für {modTypeName}");

            if(vehicle is { Exists: true }) {
                using(var db = new ChoiceVDb()) {
                    var dbVehicle = db.vehicles.Find(vehicle.VehicleId);
                    if(dbVehicle is not null) {
                        var dbVehicleModel = db.configvehiclesmodels.Find(dbVehicle.modelId);
                        if(dbVehicleModel is not null) {
                            var modType = db.configvehiclemodtypes.First(x => x.ModTypeIndex == modTypeIndex);

                            if(tuningItem.ModIndex != null) {
                                vehicleBuildInItem(player, vehicle, currentModIndex, currentModName, modTypeIndex, tuningItem.ModIndex ?? -1, tuningItem.ModName, tuningItem);
                                return true;
                            } else {
                                var universalMods = db.configvehicleuniversalmods.Where(x => x.configvehiclemodtypes_id == modType.id).ToList();
                                if(universalMods.Count > 0) {
                                    foreach(var universalMod in universalMods) {
                                        var menuItem = new ClickMenuItem($"{universalMod.DisplayName}", $"{modTypeName}: {universalMod.DisplayName}", string.Empty, "VEHICLE_TUNING_BUILD_IN")
                                            .withData(new Dictionary<string, dynamic> {
                                                { "Vehicle", vehicle },
                                                { "CurrentModIndex", currentModIndex },
                                                { "CurrentModName", currentModName },
                                                { "ModTypeIndex", modTypeIndex },
                                                { "ModTypeName", modTypeName },
                                                { "ModName", universalMod.DisplayName },
                                                { "ModIndex", universalMod.ModIndex },
                                                { "TuningItem", tuningItem }
                                            }).needsConfirmation("Wirklich verbauen?", $"{tuningItem.Description} verbauen?");
                                        menu.addMenuItem(menuItem);
                                    }
                                } else {
                                    var mods = db.configvehiclemods.Where(x => x.configvehiclemodels_id == dbVehicleModel.id && x.configvehiclemodtypes_id == modType.id).ToList();
                                    foreach(var mod in mods) {
                                        var menuItem = new ClickMenuItem($"{mod.DisplayName}", $"{modTypeName}: {mod.DisplayName}", string.Empty, "VEHICLE_TUNING_BUILD_IN")
                                            .withData(new Dictionary<string, dynamic> {
                                                { "Vehicle", vehicle },
                                                { "CurrentModIndex", currentModIndex },
                                                { "CurrentModName", currentModName },
                                                { "ModTypeIndex", modTypeIndex },
                                                { "ModTypeName", modTypeName },
                                                { "ModIndex", mod.ModIndex },
                                                { "ModName", mod.DisplayName },
                                                { "TuningItem", tuningItem }
                                            }).needsConfirmation("Wirklich verbauen?", $"{tuningItem.Description} verbauen?");
                                        menu.addMenuItem(menuItem);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            player.showMenu(menu);

            return true;
        }

        private static bool onVehicleBuildIn(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (ChoiceVVehicle)data["Vehicle"];
            var currentModIndex = (int)data["CurrentModIndex"];
            var currentModName = (string)data["CurrentModName"];
            var modTypeIndex = (byte)data["ModTypeIndex"];
            var modIndex = (int)data["ModIndex"];
            var modName = (string)data["ModName"];
            var tuningItem = (VehicleTuningItem)data["TuningItem"];

            vehicleBuildInItem(player, vehicle, currentModIndex, currentModName, modTypeIndex, modIndex, modName, tuningItem);

            return true;
        }

        private static void vehicleBuildInItem(IPlayer player, ChoiceVVehicle vehicle, int currentModIndex, string currentModName, byte modTypeIndex, int modIndex, string modName, VehicleTuningItem tuningItem) {
            if(vehicle is not { Exists: true }) {
                return;
            }

            if(vehicle.VehicleClassId == tuningItem.VehicleClass && modTypeIndex == (byte)tuningItem.ModType && (tuningItem.ModIndex is null || tuningItem.ModIndex == modIndex)) {
                var anim = AnimationController.getAnimationByName("WORK_FRONT");
                AnimationController.animationTask(player, anim, () => {
                    var toolItem = player.getInventory().getItem<ToolItem>(t => t.Flag == tuningItem.BuildInToolFlag);

                    if(toolItem == null) {
                        player.sendBlockNotification("Du hast probiert das Teil einzubauen, aber dir fehlt das richtige Werkzeug", "Werkzeug fehlt", NotifactionImages.Car);
                        return;
                    }

                    vehicle.VehicleTuning.setMod(tuningItem.ModType, modIndex);
                    vehicle.applyVehicleTuning(vehicle.VehicleTuning);
                    VehicleController.saveVehicleTuning(vehicle);

                    var oldTuningItemConfig = InventoryController.getConfigItem(x => x.codeItem == typeof(VehicleTuningItem).Name && int.Parse(x.additionalInfo.Split("#")[0]) == modTypeIndex);
                    var oldTuningItem = new VehicleTuningItem(oldTuningItemConfig, vehicle.VehicleClassId, currentModName, vehicle.DbModel.id) {
                        ModIndex = currentModIndex,
                    };

                    tuningItem.use(player);
                    toolItem.use(player);

                    player.getInventory().addItem(oldTuningItem);

                    Logger.logDebug(LogCategory.Player, LogActionType.Updated, player, $"Player build in mod: {modTypeIndex}, {(byte)modIndex} in {vehicle.VehicleId} with {tuningItem.Name}");

                    player.sendNotification(NotifactionTypes.Success, "Das Teil passt perfekt!", "Tuning geglückt", NotifactionImages.Car);
                });
            } else {
                player.sendBlockNotification("Das Teil sieht irgendwie fehlplatziert und unpassend aus!", "Tuning fehlgeschlagen", NotifactionImages.Car);
            }
        }

        private static void resetVehicleTuning(IPlayer player, ChoiceVVehicle vehicle) {
            vehicle.applyVehicleTuning(vehicle.VehicleTuning);
            player.sendNotification(NotifactionTypes.Warning, "Die Tuningvorschau für das Fahrzeug wurde beendet!", "Tuning-Vorschau beendet!");
            Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"Player reset tuning for {vehicle.VehicleId}");
        }
    }

    public class VehicleTuningFunctionality : CompanyFunctionality {
        public VehicleTuningFunctionality() : base() { }

        public VehicleTuningFunctionality(Company company) : base(company) {
            Company = company;
        }

        public override string getIdentifier() {
            return "VEHICLE_TUNING_FUNCTIONALITY";
        }

        public override SelectionInfo getSelectionInfo() {
            return new SelectionInfo(getIdentifier(), "Fahrzeug Tuning", "Ermöglicht erweiterte Funktionen des Fahrzeugtunings. Wie z.B. die Tuning Vorschau");
        }

        public override void onLoad() { }

        public override void onRemove() { }
    }
}