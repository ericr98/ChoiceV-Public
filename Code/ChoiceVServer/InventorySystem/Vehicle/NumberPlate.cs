using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem;

internal class NumberPlate : Item {
    public NumberPlate(item item) : base(item) { }

    public NumberPlate(configitem configItem, string plateText) : base(configItem) {
        Description = $"Kennzeichen: {plateText}";
        NumberPlateContent = plateText;
    }

    public string NumberPlateContent {
        get => (string)Data["NumberPlateContent"];
        set => Data["NumberPlateContent"] = value;
    }
}

public class NumberPlateController : ChoiceVScript {
    public NumberPlateController() {
        InteractionController.addVehicleInteractionElement(
            new ConditionalGeneratedPlayerInteractionMenuElement(
                "Nummernschild anbringen",
                numberPlateGenerator,
                p => p is IPlayer player && player.getInventory().hasItem<NumberPlate>(i => true),
                t => t is IVehicle { NumberplateText: " " } vehicle && (vehicle as ChoiceVVehicle).DbModel.hasNumberplate == 1
            )
        );

        EventController.addMenuEvent("NUMBERPLATE_PUT", onNumberplatePut);

        InteractionController.addVehicleInteractionElement(
            new ConditionalGeneratedPlayerInteractionMenuElement(
                getNumberPlateRemoveGenerator,
                p => p is IPlayer player && player.getInventory().hasItem<ToolItem>(i => i.Flag == SpecialToolFlag.Screwdriver),
                t => t is IVehicle vehicle && vehicle.NumberplateText != " " && (vehicle as ChoiceVVehicle).DbModel.hasNumberplate == 1
            )
        );

        EventController.addMenuEvent("NUMBERPLATE_REMOVE", onNumberplateRemove);
    }

    private static bool numberPlateSpot(IPlayer player, ChoiceVVehicle vehicle) {
        var length = Math.Abs(-vehicle.DbModel.StartPoint.FromJson().Y + vehicle.DbModel.EndPoint.FromJson().Y);
        var backPos = ChoiceVAPI.getPositionInFront(vehicle.Position, vehicle.Rotation, -(length / 2), true);

        return player.Position.Distance(backPos) < 1.5f;
    }

    private static Menu numberPlateGenerator(IEntity sender, IEntity target) {
        var player = sender as IPlayer;


        var menu = new Menu("Nummernschild anbringen", "Welches möchtest du anbringen?");
 
        if(target is ChoiceVVehicle vehicle && !vehicle.IsRandomlySpawned && !vehicle.hasPlayerAccess(player)) {
            menu.addMenuItem(new StaticMenuItem("Keinen Zugriff", "Du hast keinen Zugriff auf dieses Fahrzeug", ""));
            return menu;
        }
        
        var plates = player.getInventory().getItems<NumberPlate>(i => true);
        foreach(var plate in plates) {
            menu.addMenuItem(new ClickMenuItem(plate.NumberPlateContent, $"Das Nummernschild mit Nummer {plate.NumberPlateContent} anbringen", "", "NUMBERPLATE_PUT").withData(new Dictionary<string, dynamic> { { "Plate", plate }, { "Vehicle", target } }).needsConfirmation($"{plate.NumberPlateContent} anschrauben?", "Nummernschild wirklich anschrauben?"));
        }

        return menu;
    }

    private MenuItem getNumberPlateRemoveGenerator(IEntity sender, IEntity target) {
        if(target is ChoiceVVehicle vehicle && sender is IPlayer player && !vehicle.IsRandomlySpawned && !vehicle.hasPlayerAccess(player)) {
            return new StaticMenuItem("Keinen Zugriff", "Du hast keinen Zugriff auf dieses Fahrzeug", ""); 
        } else {
            return new ClickMenuItem("Nummernschild abschrauben", "Schraube das Nummernschild des Autos ab", "", "NUMBERPLATE_REMOVE").needsConfirmation("Nummernschild abschrauben?",
                "Nummernschild wirklich abschrauben?");
        }
    }

    private static bool onNumberplateRemove(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var targetId = (int)data["InteractionTargetId"];
        var vehicle = ChoiceVAPI.FindVehicleById(targetId);

        if(vehicle == null) {
            return true;
        }

        if(numberPlateSpot(player, vehicle)) {
            var cfg = InventoryController.getConfigItemForType<NumberPlate>();
            var nbPlt = new NumberPlate(cfg, vehicle.NumberplateText);
            if(player.getInventory().addItem(nbPlt)) {
                var anim = AnimationController.getAnimationByName("KNEEL_DOWN");
                AnimationController.animationTask(player, anim, () => {
                    VehicleController.setNumberPlateText(vehicle, " ");
                    player.sendNotification(Constants.NotifactionTypes.Success, "Du hast das Nummernschild erfolgreich angebracht!", "Nummernschild abgeschraubt");
                }, player.getRotationTowardsPosition(vehicle.Position));
            } else {
                player.sendBlockNotification("Du hast nicht genug Platz im Inventar!", "Nicht genug Platz");
            }
        } else {
            player.sendBlockNotification("Gehe hinten am das Fahrzeug um das Nummernschild abzuschrauben!", "Falsche Position");
        }

        return true;
    }

    private static bool onNumberplatePut(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var vehicle = (ChoiceVVehicle)data["Vehicle"];
        var plate = (NumberPlate)data["Plate"];

        var text = plate.NumberPlateContent;
        if(vehicle == null) {
            return true;
        }
        if(numberPlateSpot(player, vehicle)) {
            if(player.getInventory().removeItem(plate)) {
                var anim = AnimationController.getAnimationByName("KNEEL_DOWN");
                AnimationController.animationTask(player, anim, () => {
                    VehicleController.setNumberPlateText(vehicle, text);
                    player.sendNotification(Constants.NotifactionTypes.Success, "Du hast das Nummernschild erfolgreich angeschraubt!", "Nummernschild angeschraubt");
                }, player.getRotationTowardsPosition(vehicle.Position));
            } else {
                player.sendBlockNotification("Du hast das Nummernschild nicht mehr im Inventar!", "Nummernschild weg");
            }
        } else {
            player.sendBlockNotification("Gehe hinten am das Fahrzeug um das Nummernschild abzuschrauben!", "Falsche Position");
        }

        return true;
    }

    //private bool onNumberPlateRemove(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
    //    var camera = player.Rotation.Yaw;
    //    var vehicle = (ChoiceVVehicle)data["Vehicle"];
    //    var vehObj = vehicle.getObject();
    //    var currentPlate = vehObj.NumberPlate;
    //    var configItem = InventoryController.AllConfigItems.FirstOrDefault(x => x.codeItem == "NumberPlate");
    //    var anim = AnimationController.getAnimationByName("KNEEL_DOWN");
    //    AnimationController.animationTask(player, anim, () => {
    //        var plateItem = new NumberPlate(configItem, currentPlate);
    //        player.getInventory().addItem(plateItem);
    //        vehObj.NumberPlate = "";
    //        vehObj.updateVehicleObject();
    //        player.sendNotification(Constants.NotifactionTypes.Success, "Kennzeichen wurde abgeschraubt", "");
    //    });
    //    return true;
    //}

    //private bool onNumberPlateAdd(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
    //    var vehicle = (ChoiceVVehicle)data["Vehicle"];
    //    var vehObj = vehicle.getObject();
    //    var allPlates = player.getInventory().getAllItems();
    //    var plateMenu = new Menu("Kennzeichenauswahl", "Welches Kennzeichen möchtest du nutzen?");
    //    foreach(var item in allPlates) {
    //        if((item is NumberPlate)) {
    //            plateMenu.addMenuItem(new ClickMenuItem($"{item.Description}", "Schraubt das Kennzeichen fest", "", "NUMBERPLATE_ATACH_TO_VEHICLE").withData(new Dictionary<string, dynamic> { { "Vehicle", vehicle }, { "Item", item } }));
    //        }

    //    }
    //    player.showMenu(plateMenu);
    //    return true;
    //}
    //private bool onNumberPlateAtach(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
    //    try {
    //        var vehicle = (ChoiceVVehicle)data["Vehicle"];
    //        var vehObj = vehicle.getObject();
    //        if(data.ContainsKey("Item")) {
    //            var item = (NumberPlate)data["Item"];
    //            if(item != null) {
    //                var anim = AnimationController.getAnimationByName("KNEEL_DOWN");
    //                AnimationController.animationTask(player, anim, () => {
    //                    var numberPlate = item.NumberPlateContent;
    //                    vehObj.NumberPlate = numberPlate;
    //                    player.getInventory().removeItem(item);
    //                    vehObj.updateVehicleObject();
    //                    player.sendNotification(Constants.NotifactionTypes.Success, $"Kennzeichen {numberPlate} erfolgreich angeschraubt", "Kennzeichen");
    //                });
    //            }
    //        }
    //    } catch(Exception e) {
    //        Logger.logException(e);
    //    }
    //    return true;
    //}
}