using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Model.Database;
using System;

namespace ChoiceVServer.InventorySystem;

public class CleaningRag : ToolItem {
    public CleaningRag(item item) : base(item) { }

    public CleaningRag(configitem configItem, int amount, int quality) : base(configItem, amount, quality) { }

    public override void use(IPlayer player) {
        var vehicle = ChoiceVAPI.FindNearbyVehicle(player, 4, v => v.DirtLevel > 0);

        if(vehicle != null) {
            var anim = AnimationController.getAnimationByName("CAR_CLOTH");
            AnimationController.animationTask(player, anim, () => {
                if(vehicle.DirtLevel <= 3) {
                    vehicle.DbDirtLevel = 0;
                } else {
                    vehicle.DbDirtLevel -= 3;
                }

                vehicle.DirtLevel = Convert.ToByte(Math.Round(vehicle.DbDirtLevel));

                player.sendNotification(Constants.NotifactionTypes.Success, vehicle.DirtLevel > 0
                    ? "Fahrzeug etwas gesäubert"
                    : "Fahrzeug komplett gesäubert", "Fahrzeug gesäubert!", Constants.NotifactionImages.Car);

                base.use(player);
            }, null, true, null, TimeSpan.FromSeconds(5));
        } else {
            player.sendBlockNotification("Kein dreckiges Fahrzeug in der Nähe gefunden!", "Kein Fahrzeug gefunden!", Constants.NotifactionImages.Car);
            base.use(player);
        }
    }
}