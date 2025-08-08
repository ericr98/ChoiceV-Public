using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Vehicles;
using ChoiceVServer.Model.Database;
using System;

namespace ChoiceVServer.InventorySystem;

public class JerryCan : ToolItem {
    public JerryCan(item item) : base(item) {
        Weight = updateWeight();
    }

    public JerryCan(configitem configItem, int amount, int quality) : base(configItem, amount, quality) {
        FuelCatagory = FuelType.None;
        FillStage = 0;
        Weight = updateWeight();

        updateDescription();
    }

    public float FillStage {
        get => (float)Data["Level"];
        set => Data["Level"] = value;
    }

    public FuelType FuelCatagory {
        get => (FuelType)Data["FuelCatagory"];
        set => Data["FuelCatagory"] = (int)value;
    }

    public float updateWeight() {
        return 1f + FillStage * 10f;
    }

    public override void updateDescription() {
        Description = FuelCatagory switch {
            FuelType.None => "Leer",
            FuelType.Petrol => $"Benzin: {Math.Round(FillStage * 10f, 1)} l",
            FuelType.Diesel => $"Diesel: {Math.Round(FillStage * 10f, 1)} l",
            _ => Description
        };
        Weight = updateWeight();

        base.updateDescription();
    }

    public override void use(IPlayer player) {
        var vehicle = ChoiceVAPI.FindNearbyVehicle(player, 4, v => v.FuelTankSize - v.Fuel * v.FuelTankSize > 0);
        if(vehicle == null) {
            AnimationController.playItemAnimation(player, "JERRYCAN", null, false);
        } else {
            if(FuelCatagory == FuelType.None) {
                AnimationController.playItemAnimation(player, "JERRYCAN", null, false);
                return;
            }

            if(vehicle.LockState == VehicleLockState.Unlocked) {
                player.stopAnimation();

                var anim = AnimationController.getAnimationByName("JERRYCANFILLING");
                AnimationController.animationTask(player, anim, () => {
                    var toFillStage = vehicle.FuelTankSize - vehicle.Fuel * vehicle.FuelTankSize;
                    if(toFillStage == 0) {
                        return;
                    }
                    if(toFillStage > FillStage * 10f) {
                        vehicle.Fuel = (vehicle.Fuel * vehicle.FuelTankSize + 10f) / vehicle.FuelTankSize;
                        FillStage = 0f;
                        FuelCatagory = FuelType.None;
                    } else {
                        FillStage = (FillStage * 10f - toFillStage) / 10f;
                        vehicle.Fuel = vehicle.FuelTankSize;
                    }
                    player.sendNotification(Constants.NotifactionTypes.Info, "Du hast den Tank aufgefüllt!", "Du hast den Tank aufgefüllt!", Constants.NotifactionImages.Car);
                    updateDescription();
                });

                if(vehicle.FuelType != FuelCatagory) {
                    InvokeController.AddTimedInvoke("WrongEngineFuel", i => {
                        VehicleMotorCompartmentController.applyMotorDamage(vehicle, 1);
                        if(player.Vehicle == vehicle) {
                            player.sendBlockNotification("Der Motor fängt an zu stottern und das Fahrzeug geht aus!", "Fahrzeug kaputt", Constants.NotifactionImages.Car);
                        }
                    }, TimeSpan.FromSeconds(25), false);
                }
            } else {
                player.sendNotification(Constants.NotifactionTypes.Info, "Fahrzeug muss aufgeschlossen sein!", "Fahrzeug ist zu!", Constants.NotifactionImages.Car);
            }
        }
    }
}