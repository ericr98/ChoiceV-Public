using System;
using System.Collections.Generic;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Menu;
using Microsoft.VisualBasic;

namespace ChoiceVServer.Controller;

public class VehicleInspectFunctionality : CompanyFunctionality {
    private CollisionShape OpenSpot;

    public VehicleInspectFunctionality() : base() { }
    public VehicleInspectFunctionality(Company company) : base(company) { }

    public override string getIdentifier() {
        return "VEHICLE_INSPECT";
    }

    public override List<string> getSinglePermissionsGranted() {
        return [ "VEHICLE_UNLOCK_IN_SPOT", "ACCESS_VEHICLE_IMPOUND" ];
    }

    public override SelectionInfo getSelectionInfo() {
        return new SelectionInfo(getIdentifier(), "Fahrzeuginspektion", "Öffne Fahrzeuge in Spezialspot und Zugriff auf Impoundgarage");
    }

    public override void onLoad() {
        if(Company.hasSetting("VEHICLE_OPEN_SPOT")) {
            OpenSpot = CollisionShape.Create(Company.getSetting("VEHICLE_OPEN_SPOT"));
            OpenSpot.Owner = this;
        }

        Company.registerCompanyAdminElement(
            "VEHICLE_OPEN_SPOT",
            getVehicleOpenSpotMenu,
            onVehicleOpenSpotCallback
        );

        Company.registerCompanyVehicleInteractElement(
            "VEHICLE_UNLOCK",
            getVehicleUnlockMenu,
            onVehicleUnlockCallback,
            "VEHICLE_UNLOCK_IN_SPOT"
        );
    }

    private MenuElement getVehicleUnlockMenu(IPlayer player, ChoiceVVehicle target) {
        if(!target.isInSpecificCollisionShape(c => c.Owner == this)) {
            return new StaticMenuItem("Spezialequipment fehlt!", "Bringe das Fahrzeug zur Override-Maschine. Dort kann das Fahrzeug dann extern gesteuert werden!", "", MenuItemStyle.yellow);
        }

        var menu = new Menu("Fahrzeug-Override Steuerung", "Entsperre das Fahrzeug, um es zu benutzen!");

        var lockOptions = new List<string> { "Entriegeln", "Verriegeln" };
        if(target.LockState == VehicleLockState.Locked) {
            lockOptions = ["Verriegeln", "Entriegeln"];
        }

        menu.addMenuItem(new ListMenuItem("Fahrzeugverriegelung", "Wähle aus, ob du das Fahrzeug entriegeln oder verriegeln möchtest!", lockOptions.ToArray(), "VEHICLE_UNLOCK", MenuItemStyle.normal, true)
        .withData(new Dictionary<string, dynamic> { { "Vehicle", target } }));
        
        var engineOptions = new List<string> { "Aus", "An" };
        if(target.EngineOn) {
            engineOptions = ["An", "Aus"];
        }

        menu.addMenuItem(new ListMenuItem("Motor", "Wähle aus, ob du den Motor an oder aus machen möchtest!", engineOptions.ToArray(), "VEHICLE_MOTOR", MenuItemStyle.normal, true)
        .withData(new Dictionary<string, dynamic> { { "Vehicle", target } }));

        return menu;
    }

    private void onVehicleUnlockCallback(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        if(subEvent == "VEHICLE_UNLOCK") {
            var vehicle = data["Vehicle"] as ChoiceVVehicle;
            var evt = menuItemCefEvent as ListMenuItem.ListMenuItemEvent;
            if(evt.currentElement == "Verriegeln") {
                if(vehicle.LockState == VehicleLockState.Locked) {
                    return;
                }

                VehicleController.lockVehicle(vehicle, player);
            } else {
                if(vehicle.LockState == VehicleLockState.Unlocked) {
                    return;
                }

                VehicleController.unlockVehicle(vehicle, player);
            }
        } else {
            var vehicle = data["Vehicle"] as ChoiceVVehicle;
            var evt = menuItemCefEvent as ListMenuItem.ListMenuItemEvent;
            if(evt.currentElement == "An") {
                if(vehicle.EngineOn) {
                    return;
                }

                VehicleController.startVehicleEngine(vehicle, player);
            } else {
                if(!vehicle.EngineOn) {
                    return;
                }

                VehicleController.stopVehicleEngine(vehicle, player);
            }
        }
    }

    private MenuElement getVehicleOpenSpotMenu(IPlayer player) {
        return new ClickMenuItem("Fahrzeugöffnerspot setzen", "Setz damit den Spot Skaar. Ja, ich weiß dass du den setzt!", "", "VEHICLE_SPOT");
    }

    private void onVehicleOpenSpotCallback(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        if(subEvent == "VEHICLE_SPOT") {
            player.sendNotification(Base.Constants.NotifactionTypes.Info, "Wähle den Spot aus, an dem du Fahrzeuge öffnen möchtest!", "Spot setzen", Base.Constants.NotifactionImages.Car);
            CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                var spot = CollisionShape.Create(p, w, h, r, false, true, false);
                spot.Owner = this;

                if(OpenSpot != null) {
                    OpenSpot.Dispose();
                    OpenSpot = null;
                }

                company.setSetting("VEHICLE_OPEN_SPOT", spot.toShortSave());
                OpenSpot = spot;

                player.sendNotification(Base.Constants.NotifactionTypes.Success, "Fahrzeugöffnerspot gesetzt!", "Spot gesetzt", Base.Constants.NotifactionImages.Car);
            });
        }
    }

    public override void onRemove() {
        Company.deleteSetting("VEHICLE_OPEN_SPOT");
    }
}