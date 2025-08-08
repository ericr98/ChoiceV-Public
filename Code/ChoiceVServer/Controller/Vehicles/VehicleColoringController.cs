using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.InventorySystem.VehicleColoringSet;

namespace ChoiceVServer.Controller {
    public enum GtaColorTypes {
        Primary,
        Secondary,
        Pearl,
    }

    public class VehicleColoringController : ChoiceVScript {
        private static Dictionary<int, configvehiclecolor> AllColors;

        public VehicleColoringController() {
            loadColors();

            InteractionController.addVehicleInteractionElement(
                 new ConditionalGeneratedPlayerInteractionMenuElement(
                     "Fahrzeuglackierung",
                     getVehicleColoringMenu,
                     sender => sender is IPlayer player,
                     target => target is ChoiceVVehicle vehicle && vehicle.LockState == VehicleLockState.Unlocked
                 )
             );

            EventController.addMenuEvent("SHOW_GTA_COLOR", onPlayerShowGtaColor);
            EventController.addMenuEvent("COLOR_VEHICLE_WITH_SET", onPlayerColorVehicleWithSet);

            EventController.addMenuEvent("EDIT_COLOR", onPlayerEditColor);
        }

        public static void loadColors() {
            AllColors = new Dictionary<int, configvehiclecolor>();
            using(var db = new ChoiceVDb()) {
                AllColors = db.configvehiclecolors.OrderBy(c => c.gtaId).ToDictionary(c => c.gtaId, c => c);
            }
        }

        public static configvehiclecolor getVehicleColorById(int gtaId) {
            return AllColors[gtaId];
        }

        public static List<configvehiclecolor> getVehicleColorsByType(VehicleColorType colorType) {
            var typeStr = colorType.ToString();
            return AllColors.Values.Where(c => c.type == typeStr).ToList();
        }

        private void resetVehicleColoring(ChoiceVVehicle vehicle) {
            vehicle.applyVehicleColoring(vehicle.VehicleColoring);
            Logger.logTrace(LogCategory.Vehicle, LogActionType.Updated, "vehicle reset coloring");
        }

        private Menu getVehicleColoringMenu(IEntity sender, IEntity target) {
            var player = sender as IPlayer;
            var vehicle = target as ChoiceVVehicle;

            if(vehicle != null) {
                var coloring = vehicle.VehicleColoring;

                var menu = new Menu("Lackiermenü", "Was möchtest du tun?", true);

                var colorMenu = new Menu("Fahrzeug lackieren", "Lackiere das Fahrzeug mit einem Lackierset");

                foreach(var type in Enum.GetValues<GtaColorTypes>()) {
                    var virtMen = getColorSetMenu(vehicle, player, $"{type} Lack färben", type);
                    colorMenu.addMenuItem(new MenuMenuItem(virtMen.Name, virtMen));
                }

                menu.addMenuItem(new MenuMenuItem(colorMenu.Name, colorMenu));

                if (CompanyController.hasPlayerCompanyWithPredicate(player, c => c.hasFunctionality<VehicleColoringFunctionality>())) {
                    var showMenu = new Menu("Lackiervorschau", "Lasse dir Lackierungen anzeigen", (p) => { resetVehicleColoring(vehicle); });
                    var primaryMenu = new Menu("Primärlackierung anpassen", "Passe die Primärlackierung an");
                    var primaryGtaMenu = getGtaColorMenu(vehicle, null, "Standard-Primärlackierung", GtaColorTypes.Primary);

                    primaryMenu.addMenuItem(new MenuMenuItem(primaryGtaMenu.Name, primaryGtaMenu));
                    showMenu.addMenuItem(new MenuMenuItem(primaryMenu.Name, primaryMenu));

                    var secondaryMenu = new Menu("Sekundärlackierung anpassen", "Passe die Sekundärlackierung an");
                    var secondaryGtaMenu = getGtaColorMenu(vehicle, null, "Standard-Sekundärlackierung", GtaColorTypes.Secondary);

                    secondaryMenu.addMenuItem(new MenuMenuItem(secondaryGtaMenu.Name, secondaryGtaMenu));
                    showMenu.addMenuItem(new MenuMenuItem(secondaryMenu.Name, secondaryMenu));

                    var pearlMenu = getGtaColorMenu(vehicle, AllColors.Values.FirstOrDefault(c => c.gtaId == coloring.PearlColor), "Pearllackierung", GtaColorTypes.Pearl);

                    showMenu.addMenuItem(new MenuMenuItem(pearlMenu.Name, pearlMenu));

                    menu.addMenuItem(new MenuMenuItem(showMenu.Name, showMenu));
                }

                return menu;
            } else {
                return null;
            }
        }

        private VirtualMenu getGtaColorMenu(ChoiceVVehicle vehicle, configvehiclecolor current, string name, GtaColorTypes colorType) {
            return new VirtualMenu(name, () => {
                var menu = new Menu(name, "Wähle eine Lackierung aus");
                if(current != null) {
                    menu.addMenuItem(new StaticMenuItem($"Aktuelle {name}", $"Die aktuelle {name} ist {current.type} {current.name}", $"{current.type} {current.name}"));
                }

                menu.addMenuItem(new HoverMenuItem("Keine Lackierung", "Lackerierung entfernen", "", "SHOW_GTA_COLOR").withData(new Dictionary<string, dynamic> { { "Color", AllColors.First().Value }, { "Vehicle", vehicle }, { "Type", colorType } }));
                var colorTypes = AllColors.Values.Skip(1).GroupBy(c => c.type);
                foreach(var type in colorTypes) {
                    var typeMenu = new Menu(type.First().type, "Wähle eine Lackierung");

                    foreach(var color in type) {
                        var data = new Dictionary<string, dynamic> { { "Color", color }, { "Vehicle", vehicle }, { "Type", colorType } };
                        typeMenu.addMenuItem(new HoverMenuItem($"{color.name}", $"Lasse die Lackierung {color.name} anzeigen", "", "SHOW_GTA_COLOR").withData(data));
                    }

                    menu.addMenuItem(new MenuMenuItem(typeMenu.Name, typeMenu));
                }

                return menu;
            });
        }

        private VirtualMenu getColorSetMenu(ChoiceVVehicle vehicle, IPlayer player, string name, GtaColorTypes type) {
            return new VirtualMenu(name, () => {
                var colorMenu = new Menu(name, "Lackiere das Fahrzeug mit einem Lackierset");

                var items = player.getInventory().getItems<VehicleColoringSet>(i => i.GTAColor != -1).Distinct((i1, i2) => i1.GTAColor.Equals(i2.GTAColor));
                foreach(var item in items) {
                    var data = new Dictionary<string, dynamic> {
                    { "Vehicle", vehicle },
                    { "Item", item },
                    { "Type", type }
                };

                    var color = getVehicleColorById(item.GTAColor);
                    var name = $"{item.ColorType} {color.name}";
                    colorMenu.addMenuItem(new ClickMenuItem(name, $"Färbe die {type} Farbe des Fahrzeuges in {name}", "", "COLOR_VEHICLE_WITH_SET").withData(data).needsConfirmation("Fahrzeug färben?", $"Fahrzeug {name} färben?"));
                }

                return colorMenu;
            });
        }

        private bool onPlayerShowGtaColor(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (ChoiceVVehicle)data["Vehicle"];
            var type = (GtaColorTypes)data["Type"];
            var color = (configvehiclecolor)data["Color"];

            if(vehicle.Exists()) {
                switch(type) {
                    case GtaColorTypes.Primary:
                        vehicle.PrimaryColor = (byte)color.gtaId;
                        break;
                    case GtaColorTypes.Secondary:
                        vehicle.SecondaryColor = (byte)color.gtaId;
                        break;
                    case GtaColorTypes.Pearl:
                        vehicle.PearlColor = (byte)color.gtaId;
                        break;
                }

                Logger.logTrace(LogCategory.Player, LogActionType.Viewed, $"Player previewed color {color} of type {type} on vehicle {vehicle.VehicleId}");
            }

            return true;
        }

        private bool onPlayerEditColor(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var color = (configvehiclecolor)data["Color"];
            var type = Enum.Parse<VehicleColorType>(color.type);

            var item = player.getInventory().getItem<VehicleColoringSet>(c => c.ColorType == type && c.GTAColor == -1);

            if(item != null) {
                var anim = AnimationController.getAnimationByName("WORK_FRONT");
                AnimationController.animationTask(player, anim, () => {
                    item.GTAColor = color.gtaId;
                    item.updateDescription();

                    Logger.logDebug(LogCategory.Player, LogActionType.Updated, player, $"Player set color {color} of type {type} on item {item.Id}");
                    player.sendNotification(NotifactionTypes.Success, $"Ein Lackset in die Farbe: {color.type} {color.name} gefärbt", "Lackset gefärbt", NotifactionImages.Car);
                });
            }

            return true;
        }



        private bool onPlayerColorVehicleWithSet(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (ChoiceVVehicle)data["Vehicle"];
            var type = (GtaColorTypes)data["Type"];
            var item = (VehicleColoringSet)data["Item"];

            var anim = AnimationController.getAnimationByName("SPRAY_BOTTLE");
            AnimationController.animationTask(player, anim, () => {
                if(vehicle.Exists()) {
                    switch(type) {
                        case GtaColorTypes.Primary:
                            vehicle.VehicleColoring.setColor((byte)item.GTAColor, true);
                            break;
                        case GtaColorTypes.Secondary:
                            vehicle.VehicleColoring.setColor((byte)item.GTAColor, false);
                            break;
                        case GtaColorTypes.Pearl:
                            vehicle.VehicleColoring.setPearlColor((byte)item.GTAColor);
                            break;
                    }
                    player.getInventory().removeItem(item);
                    VehicleController.saveVehicleColoring(vehicle);
                    vehicle.setVehicleColoring(vehicle.VehicleColoring);

                    Logger.logDebug(LogCategory.Player, LogActionType.Updated, player, $"Player colored vehicle {vehicle.Id} with color {item.GTAColor} of type {type} on item {item.Id}");
                }
            });
            return true;
        }
    }

    public class VehicleColoringFunctionality : CompanyFunctionality {
        public CollisionShape ColorEditingArea { get; private set; }

        public VehicleColoringFunctionality() : base() { }

        public VehicleColoringFunctionality(Company company) : base(company) {
            Company = company;
        }

        public override string getIdentifier() {
            return "VEHICLE_COLORING_FUNCTIONALITY";
        }

        public override SelectionInfo getSelectionInfo() {
            return new SelectionInfo(getIdentifier(), "Fahrzeuge färben", "Ermöglicht der Firma die Funktionen des Fahrzeugsfärbens");
        }

        public override void onLoad() {
            var coloringEditingArea = Company.getSetting("COLORING_EDIT_AREA");
            if(coloringEditingArea != default) {
                ColorEditingArea = CollisionShape.Create(coloringEditingArea);
                ColorEditingArea.Owner = this;
                ColorEditingArea.TrackPlayers = true;
                ColorEditingArea.OnCollisionShapeInteraction += onInteraction;
            }

            Company.registerCompanyAdminElement(
                "COLORING_AREA_CREATION",
                getColoringAreaCreator,
                onColoringAreaCreator
            );
        }

        public override void onRemove() {
            Company.unregisterCompanyElement("COLORING_AREA_CREATION");
            Company.deleteSetting("COLORING_EDIT_AREA");
        }

        private MenuElement getColoringAreaCreator(IPlayer player) {
            //Tuning/Reperaturspots für bestimmte Klassen beschränken
            var menu = new Menu("Kollisionen setzen", "Setze die verschiedenen Kollisionen");
            menu.addMenuItem(new ClickMenuItem("Lackeriungs-Area setzen", "Setze die Outline der Lackeriungswerkstatt", "", "COLORING_AREA"));
            menu.addMenuItem(new ClickMenuItem("Lackiereditierung setzen", "Setze die Outline den Spot um Lackfarben zu editieren", "", "COLORING_EDIT_AREA"));

            return menu;
        }

        private void onColoringAreaCreator(Company company, IPlayer player, string subEvent, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
            switch(subEvent) {
                case "COLORING_EDIT_AREA":
                    player.sendNotification(NotifactionTypes.Info, "Setze nun die Kollision der Lackeriungswerkstatt.", "");
                    CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                        ColorEditingArea = CollisionShape.Create(p, w, h, r, true, false, true);
                        ColorEditingArea.Owner = this;
                        ColorEditingArea.TrackPlayers = true;
                        ColorEditingArea.OnCollisionShapeInteraction += onInteraction;

                        Company.setSetting("COLORING_EDIT_AREA", ColorEditingArea.toShortSave());
                        player.sendNotification(NotifactionTypes.Success, "Lackeriungswerkstatt-Area gesetzt.", "");
                    });
                    break;
            }
        }

        private bool onInteraction(IPlayer player) {
            var menu = new Menu("Lackmischtisch", "Was möchtest du tun?");

            var setItemsTypes = player.getInventory().getItems<VehicleColoringSet>(i => i.GTAColor == -1).GroupBy(i => i.ColorType);

            if(setItemsTypes.Any()) {
                var coloringMenu = new Menu("Lackierungsset", "Was möchtest du tun?");

                foreach(var typeList in setItemsTypes) {
                    var first = typeList.First();
                    var colors = VehicleColoringController.getVehicleColorsByType(first.ColorType);

                    var subMenu = new Menu($"{first.ColorType} färben", "Welche Farbe ist gewünscht?");

                    foreach(var color in colors) {
                        var data = new Dictionary<string, dynamic> { { "Color", color } };
                        subMenu.addMenuItem(new ClickMenuItem(color.name, $"Färbe ein Lackierungsset zu {first.ColorType} {color.name}", "", "EDIT_COLOR").withData(data).needsConfirmation("Set färben?", $"Wicklich {color.name} färben?"));
                    }

                    coloringMenu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
                }

                menu.addMenuItem(new MenuMenuItem(coloringMenu.Name, coloringMenu));
            }

            player.showMenu(menu);

            return true;
        }
    }
}
