using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.Vehicles;

public class VehicleKeyController : ChoiceVScript {
    public VehicleKeyController() {
        CharacterController.addSelfMenuElement(
            new ConditionalPlayerSelfMenuElement(
                "Fahrzeug lokalisieren",
                getVehicleKeyMenu,
                p => p.getInventory().hasItem(i => i is VehicleKey)
            )
        );

        EventController.addMenuEvent("VEHICLE_LOCATE", onVehicleLocate);
    }
    private bool onVehicleLocate(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
        var vehicleKey = (VehicleKey)data["VEHICLE_KEY"];
        if(vehicleKey is null) {
            return false;
        }

        vehicleKey.use(player);

        return true;
    }

    private static Menu getVehicleKeyMenu(IPlayer player) {
        var menu = new Menu("Fahrzeug lokalisieren", "Lokalisiere das Fahrzeug auf der Karte");

        var vehicleKeys = player.getInventory().getItems<VehicleKey>(_ => true).DistinctBy(k => k.VehicleId);
        foreach(var vehicleKey in vehicleKeys) {
            string modelDisplayName;
            string numberPlate;

            using(var db = new ChoiceVDb()) {
                var dbVehicle = db.vehicles
                    .Include(v => v.model)
                    .FirstOrDefault(v => v.id == vehicleKey.VehicleId);
                if(dbVehicle is null) {
                    continue;
                }

                modelDisplayName = string.IsNullOrWhiteSpace(dbVehicle.model.DisplayName)
                    ? dbVehicle.model.ModelName
                    : dbVehicle.model.DisplayName;
                numberPlate = dbVehicle.numberPlate;
            }

            menu.addMenuItem(new ClickMenuItem(modelDisplayName, $"{modelDisplayName} ({numberPlate})", string.Empty, "VEHICLE_LOCATE")
                .withData(new Dictionary<string, dynamic> { { "VEHICLE_KEY", vehicleKey } }));
        }

        return menu;
    }
}