using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Garages;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using System.Linq;

namespace ChoiceVServer.InventorySystem;

public class VehicleKey : Item {
    public int VehicleId { get => (int)Data["VehicleId"]; set => Data["VehicleId"] = value; }
    public int LockVersion { get => (int)Data["LockVersion"]; set => Data["LockVersion"] = value; }

    public VehicleKey(item item) : base(item) { }

    public VehicleKey(configitem configItem, ChoiceVVehicle vehicle) : base(configItem) {
        VehicleId = vehicle.VehicleId;
        LockVersion = vehicle.KeyLockVersion;

        Description = $"Gehört zu einem {vehicle.DbModel.ModelName.ToUpper()} mit Chassis {vehicle.ChassisNumber} und Schlossversion: {LockVersion}";
    }

    public override void use(IPlayer player) {
        var position = getVehiclePosition();
        if(position == Position.Zero) {
            player.sendBlockNotification("Das Fahrzeug ist nicht auffindbar. Dieser Schlüssel funktioniert nicht mehr!", "Schlüssel funktioniert nicht", Constants.NotifactionImages.Car);
            return;
        }

        BlipController.createWaypointBlip(player, position);
        player.sendNotification(Constants.NotifactionTypes.Info, "Die Position des Fahrzeugs wurde auf deiner Karte markiert!", "Position markiert");
    }

    public bool worksForCar(ChoiceVVehicle vehicle) {
        return VehicleId == vehicle.VehicleId && vehicle.KeyLockVersion == LockVersion;
    }
    
    public bool worksForCar(vehicle vehicle) {
        return VehicleId == vehicle.id && vehicle.keyLockVersion == LockVersion;
    }

    private Position getVehiclePosition() {
        Position vehiclePosition;
        int? garageId;

        using(var db = new ChoiceVDb()) {
            var dbVehicle = db.vehicles.FirstOrDefault(v => v.id == VehicleId && v.keyLockVersion == LockVersion);
            if(dbVehicle is null) {
                return Position.Zero;
            }

            vehiclePosition = dbVehicle.position.FromJson();
            garageId = dbVehicle.garageId;
        }

        return garageId.HasValue
            ? GarageController.getGaragePosition(garageId ?? -1)
            : vehiclePosition;
    }
}