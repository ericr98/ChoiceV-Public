using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Vehicles;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.ListMenuItem;

public class VehicleMechanicalGameComponent : MechanicalGameComponent {
    public ChoiceVVehicle Vehicle { get; private set; }
    public VehicleMotorCompartmentPart Part;
    public bool Moveable { get; set; }
    private string ImageOrientation;

    public VehicleMechanicalGameComponent(int id, VehicleMotorCompartmentMapping mapping, bool inStash, bool moveable) : base(id, true, inStash, mapping.Depth, mapping.Part.GameImage + mapping.ImageOrientation, mapping.Positions) {
        Part = mapping.Part;
        Moveable = moveable;
        ImageOrientation = mapping.ImageOrientation;
    }

    public VehicleMechanicalGameComponent(int id, VehicleMotorCompartmentPart part, ChoiceVVehicle vehicle, string imageOrientation, bool inStash, bool moveable, List<GameComponentVector> positions, int depth) : base(id, true, inStash, depth, part.GameImage + imageOrientation, positions) {
        Part = part;
        Vehicle = vehicle;
        Moveable = moveable;
        ImageOrientation = imageOrientation;
    }

    public void setVehicle(ChoiceVVehicle vehicle) {
        Vehicle = vehicle;
    }

    public override Menu getSelectMenu(IPlayer player, bool blockedByOtherComponent) {
        Menu menu;

        menu = new Menu(Part.getNameOrSerialNumber(), "Was möchtest du tun?");

        if(!MovedToStash) {
            if(Moveable) {
                var items = player.getInventory().getItems<VehicleMotorCompartmentItem>(i => true);
                if(items.Count() > 0) {
                    menu.addMenuItem(new ListMenuItem("Teil reparieren", "Wähle ein Teil aus mit dem du reparieren willst", items.Select(i => i.Name).ToArray(), "MECHANICAL_GAME_REPAIR_PART").withData(new Dictionary<string, dynamic> { { "Component", this }, { "StillInCar", true } }).needsConfirmation("Teil reparieren?", "Teil wirklich reparieren?"));
                } else {
                    menu.addMenuItem(new StaticMenuItem("Teile reparieren (keine Teile)", "Du hast keine Teile zum reparieren dabei", "", MenuItemStyle.yellow));
                }

                menu.addMenuItem(new ClickMenuItem("Teil beiseite legen", "Lege das Teil in die Ablage. Es kann von dort noch eingesteckt werden", "", "MECHANICAL_GAME_MOVE_TO_STASH").withData(new Dictionary<string, dynamic> { { "Component", this } }).needsConfirmation("Teil beiseite legen?", "Teil wirklich beiseite legen?"));
            } else {
                if(!blockedByOtherComponent) {
                    var tools = player.getInventory().getItems<ToolItem>(t => true);
                    if(tools.Count() == 0) {
                        menu.addMenuItem(new StaticMenuItem("Keine Werkzeuge dabei", "Du hast keine Werkzeuge dabei um das Teil zu lösen", "", MenuItemStyle.yellow));
                    } else {
                        menu.addMenuItem(new ListMenuItem("Teil lösen", "Löse das Teil aus der Maschine", tools.Select(t => t.Name).ToArray(), "MECHANICAL_GAME_VEHICLE_LOSEN").withData(new Dictionary<string, dynamic> { { "Component", this } }).needsConfirmation("Teil lösen?", "Teil mit ausgewähltem Werkzeug lösen?"));
                    }

                } else {
                    menu.addMenuItem(new StaticMenuItem("Teil ist blockiert!", "Dieses Teil ist blockiert durch ein anderes Teil!", "", MenuItemStyle.yellow));
                }
            }

            //menu.addMenuItem(new StaticMenuItem("Schaden", "", $"{Vehicle.getCompartmentPartDamage(Part.Identifier) * 100}%"));

        } else {
            var items = player.getInventory().getItems<VehicleMotorCompartmentItem>(i => true);
            if(items.Count() > 0) {
                menu.addMenuItem(new ListMenuItem("Teil reparieren", "Wähle ein Teil aus mit dem du reparieren willst", items.Select(i => i.Name).ToArray(), "MECHANICAL_GAME_REPAIR_PART").withData(new Dictionary<string, dynamic> { { "Component", this }, { "StillInCar", false } }).needsConfirmation("Teil reparieren?", "Teil wirklich reparieren?"));
            } else {
                menu.addMenuItem(new StaticMenuItem("Teile reparieren (keine Teile)", "Du hast keine Teile zum reparieren dabei", "", MenuItemStyle.yellow));
            }

            var tools = player.getInventory().getItems<ToolItem>(t => true);
            if(tools.Count() == 0) {
                menu.addMenuItem(new StaticMenuItem("Keine Werkzeuge dabei", "Du hast keine Werkzeuge dabei um das Teil wieder einzubauen", "", MenuItemStyle.yellow));
            } else {
                menu.addMenuItem(new ListMenuItem("Teil einbauen", "Baue das Teil von der Ablage wieder ein. Dies ist nur möglich, wenn kein anderes Teil dieses blockiert!", tools.Select(t => t.Name).ToArray(), "MECHANICAL_GAME_VEHICLE_BUILD_IN").withData(new Dictionary<string, dynamic> { { "Component", this } }).needsConfirmation("Teil wieder einbauen?", "Mit ausgewähltem Werkzeug einbauen?"));
            }
            menu.addMenuItem(new ClickMenuItem("Teil analysieren", "Analysiere das Teil auf mögliche Schäden", "", "MECHANICAL_GAME_ANALYSE_PART").withData(new Dictionary<string, dynamic> { { "Component", this } }).needsConfirmation("Teil analysieren?", "Teil wirklich analysieren?"));
        }

        return menu;
    }

    public override string getStringRepresentationStep() {
        return $"M#{Part.Identifier}#{Moveable}#{ImageOrientation}#{Vehicle.VehicleId}";
    }

    public static VehicleMechanicalGameComponent createFromRepresentation(int id, bool identified, bool movedToStash, int depth, string image, List<GameComponentVector> positions, string[] data) {
        var partIdentifier = data[1];
        var moveable = bool.Parse(data[2]);
        var imageOrientation = data[3];
        var vehicleId = int.Parse(data[4]);

        var part = VehicleMotorCompartmentController.getCompartmentPart(partIdentifier);
        var vehicle = ChoiceVAPI.FindVehicleById(vehicleId);

        return new VehicleMechanicalGameComponent(id, part, vehicle, imageOrientation, movedToStash, moveable, positions, depth);

    }
}

public class VehicleMechanicalGameController : ChoiceVScript {
    public VehicleMechanicalGameController() {
        EventController.addMenuEvent("MECHANICAL_GAME_VEHICLE_LOSEN", onMechanicalGameSerialLosen);
        EventController.addMenuEvent("MECHANICAL_GAME_VEHICLE_BUILD_IN", onMechanicalGameBuildIn);

        EventController.addMenuEvent("MECHANICAL_GAME_ANALYSE_PART", onMechanicalGameAnalysePart);

        EventController.addMenuEvent("MECHANICAL_GAME_REPAIR_PART", onMechanicalGameRepairPart);
    }

    private bool onMechanicalGameSerialLosen(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var component = (VehicleMechanicalGameComponent)data["Component"];
        var evt = menuItemCefEvent as ListMenuItemEvent;

        makeWorkAndToolCheck(player, component, evt.currentElement, (checkPassed, tool) => {
            var game = (MechanicalGame)player.getData("MECHANICAL_GAME");

            if(checkPassed) {
                tool.use(player);
                component.Moveable = true;
                MechanicalGameController.refreshMenu(player, component);

                player.sendNotification(Constants.NotifactionTypes.Success, "Das Teil hat sich mit dem Werkzeug einfach lösen lassen. Es kann nun herausgenommen werden", "Teil gelöst");
            } else {
                MechanicalGameController.refreshMenu(player, component);
                game.invokeAction(player, component, "WRONGLY_LOSEND");
                player.sendNotification(Constants.NotifactionTypes.Danger, "Das Teil konnte nicht gelöst werden, weil das Werkzeug nicht passt!", "Teil nicht gelöst");
            }

            game.UpdateFlag = true;
        });

        return true;
    }

    private bool onMechanicalGameBuildIn(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var component = (VehicleMechanicalGameComponent)data["Component"];
        var evt = menuItemCefEvent as ListMenuItemEvent;

        makeWorkAndToolCheck(player, component, evt.currentElement, (checkPassed, tool) => {
            if(checkPassed) {
                tool.use(player);
                if(MechanicalGameController.getPositionIsAvailable(player, component.Depth, component.Positions)) {
                    var game = (MechanicalGame)player.getData("MECHANICAL_GAME");
                    MechanicalGameController.putMechanicalGameBackIn(player, component);
                    component.Moveable = false;

                    MechanicalGameController.refreshMenu(player, component);

                    player.sendNotification(Constants.NotifactionTypes.Success, "Das Teil hat sich mit dem Werkzeug einfach einbauen lassen. Es ist jetzt wieder fest verbaut", "Teil eingebaut");

                    game.UpdateFlag = true;
                } else {
                    player.sendNotification(Constants.NotifactionTypes.Danger, "Das Teil konnte nicht eingebaut werden, weil es durch ein anderes Teil blockiert ist!", "Teil blockiert!");
                }
            } else {
                var charId = player.getCharacterId();
                if(player.hasData("MECHANICAL_GAME")) {
                    var game = (MechanicalGame)player.getData("MECHANICAL_GAME");
                    game.invokeAction(player, component, "WRONGLY_PUT_IN");

                    game.UpdateFlag = true;
                    player.sendNotification(Constants.NotifactionTypes.Danger, "Das Teil konnte nicht ausgebaut werden, weil das Werkzeug nicht passt!", "Teil nicht gelöst");
                }
            }
        });


        return true;
    }

    private static bool onMechanicalGameAnalysePart(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var component = (VehicleMechanicalGameComponent)data["Component"];

        var anim = AnimationController.getAnimationByName("WORK_FRONT_LONG");
        AnimationController.animationTask(player, anim, () => {
            var damage = component.Part.getCurrentDamage(component.Vehicle);

            if(damage >= 0.75) {
                player.sendNotification(Constants.NotifactionTypes.Danger, "Das Teil ist stark beschädigt!", "Teil stark beschädigt", Constants.NotifactionImages.Car);
            } else if(damage >= 0.5) {
                player.sendNotification(Constants.NotifactionTypes.Warning, "Das Teil hat starke Gebrauchsspuren!", "Teil starke Gebrauchsspuren", Constants.NotifactionImages.Car);
            } else if(damage >= 0.25) {
                player.sendNotification(Constants.NotifactionTypes.Info, "Das Teil hat leichte Gebrauchsspuren!", "Teil leichte Gebrauchsspuren", Constants.NotifactionImages.Car);
            } else {
                player.sendNotification(Constants.NotifactionTypes.Success, "Das Teil ist fast nicht beschädigt!", "Teil nicht beschädigt", Constants.NotifactionImages.Car);
            }

        }, null, true, 1, TimeSpan.FromSeconds(10));

        return true;
    }

    private void makeWorkAndToolCheck(IPlayer player, VehicleMechanicalGameComponent component, string toolName, Action<bool, ToolItem> callback) {
        var anim = AnimationController.getAnimationByName("WORK_FRONT_LONG");
        AnimationController.animationTask(player, anim, () => {
            var tool = player.getInventory().getItem<ToolItem>(t => t.Name == toolName);
            callback.Invoke(tool != null && tool.Flag == component.Part.LoosenToolFlag, tool);
        }, null, true, 1, TimeSpan.FromSeconds(10));
    }

    private bool onMechanicalGameRepairPart(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var component = (VehicleMechanicalGameComponent)data["Component"];
        var evt = menuItemCefEvent as ListMenuItemEvent;

        var stillInCar = (bool)data["StillInCar"];

        var item = player.getInventory().getItem<VehicleMotorCompartmentItem>(t => t.Name == evt.currentElement);

        if(item != null) {
            var anim = AnimationController.getAnimationByName("WORK_FRONT_LONG");
            AnimationController.animationTask(player, anim, () => {
                if(stillInCar) {
                    if(item.RepairType == component.Part.RepairType && component.Part.getCurrentDamage(component.Vehicle) <= 0.75f) {
                        var game = (MechanicalGame)player.getData("MECHANICAL_GAME");
                        component.Part.onRepair(component.Vehicle);
                        player.sendNotification(Constants.NotifactionTypes.Info, "Teil wurde repariert!", "Teil repariert!", Constants.NotifactionImages.Car);
                        item.use(player);

                        game.UpdateFlag = true;
                    } else {
                        player.sendBlockNotification("Was immer du da versuchst scheint nicht zu funktionieren!", "Teil reparieren fehlgeschlagen", Constants.NotifactionImages.Car);
                    }
                } else {
                    if((item.RepairType == component.Part.RepairType && component.Part.getCurrentDamage(component.Vehicle) <= 0.75f) || item.RepairType == component.Part.ReplaceType) {
                        var game = (MechanicalGame)player.getData("MECHANICAL_GAME");
                        component.Part.onRepair(component.Vehicle);
                        player.sendNotification(Constants.NotifactionTypes.Info, "Teil wurde repariert!", "Teil repariert!", Constants.NotifactionImages.Car);
                        item.use(player);

                        game.UpdateFlag = true;
                    } else {
                        player.sendBlockNotification("Was immer du da versuchst scheint nicht zu funktionieren!", "Teil reparieren fehlgeschlagen", Constants.NotifactionImages.Car);
                    }
                }
            }, null, true, 1, TimeSpan.FromSeconds(10));
        }

        return true;
    }
}