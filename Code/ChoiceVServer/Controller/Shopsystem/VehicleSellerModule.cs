using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.Controller;

public class NPCVehicleSellerModule : NPCModule {
    internal List<(int vehicleConfigId, decimal price)> AvailableVehicles = [];

    public NPCVehicleSellerModule(ChoiceVPed ped) : base(ped) {

    }

    public NPCVehicleSellerModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, settings) {
        AvailableVehicles = ((string)settings["AvailableVehicles"]).FromJson<List<(int, decimal)>>();
    }

    public override List<MenuItem> getMenuItems(IPlayer player) {
        var menu = new Menu("Verfügbare Fahrzeuge", "Was möchtest du tun?");

        var buyMenu = new Menu("Kaufen", "Fahrzeuge kaufen");
        foreach(var (vehicleConfigId, price) in AvailableVehicles) {
            var model = VehicleController.getVehicleModelById(vehicleConfigId);
            buyMenu.addMenuItem(new ClickMenuItem(model.DisplayName, $"Kaufe ein/eine {model.DisplayName} für ${price}", $"${price}", "ON_PLAYER_BUY_BIKE")
                .withData(new Dictionary<string, dynamic> {{"BikeModel", model}, {"Price", price}}));
        }

        menu.addMenuItem(new MenuMenuItem(buyMenu.Name, buyMenu));

        var sellMenu = new Menu("Verkaufen", "Fahrzeuge verkaufen");
        var vehicle = ChoiceVAPI.FindNearbyVehicle(player, 10, v => v.VehicleClass == VehicleClassesDbIds.Cycles 
                                                        && (v.hasPlayerAccess(player) || player.isCop()) 
                                                        && AvailableVehicles.Any(b => b.vehicleConfigId == v.DbModel.id));
        if(vehicle != null) {
            var price = AvailableVehicles.FirstOrDefault(b => b.vehicleConfigId == vehicle.DbModel.id);
            if(price != default) {
                var p = Math.Round(price.price * 0.66m, 2);
                sellMenu.addMenuItem(new ClickMenuItem(vehicle.DbModel.DisplayName, $"Verkaufe das Fahrrad für ${p}", $"${p}", "ON_PLAYER_SELL_BIKE")
                    .withData(new Dictionary<string, dynamic> {{"Vehicle", vehicle}, {"Price", p}}));

            } else {
                sellMenu.addMenuItem(new StaticMenuItem($"{vehicle.DbModel.DisplayName} wird nicht gekauft", $"An diesem Laden werden keine Fahrräder vom Typ {vehicle.DbModel.DisplayName} angekauft", ""));
            }
        } else {
            sellMenu.addMenuItem(new StaticMenuItem("Gebraucht-Fahrzeugankauf verfügbar", "Bringe alte Fahrzeutge hierhin zurück, um diese zu verkaufen. Es werden nur Fahrzeuge angekauft, die auch hier verkauft werden.", ""));
        }

       menu.addMenuItem(new MenuMenuItem(sellMenu.Name, sellMenu));

        if(player.getAdminLevel() > 2 && player.getCharacterData().AdminMode) {
            var supportMenu = new Menu("Support-Einträge erstellen", "");

            var supCreateMenu = new Menu("Erstellen", "Gib die Daten ein");
            supCreateMenu.addMenuItem(new InputMenuItem("Model", "Das Fahrzeugmodell. Den GTA Namen", "", ""));
            supCreateMenu.addMenuItem(new InputMenuItem("Preis", "Der Preis zum kaufen. 75% davon ist der Verkaufspreis", "", InputMenuItemTypes.number, ""));
            supCreateMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle das Ding", "SUPPORT_CREATE_VEHICLE_SELLER_ENTRY", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> {{"Module", this}}));

            supportMenu.addMenuItem(new MenuMenuItem(supCreateMenu.Name, supCreateMenu));


            foreach(var entry in AvailableVehicles) {
                var vehicleName = VehicleController.getVehicleModelById(entry.vehicleConfigId)?.ModelName;
                supportMenu.addMenuItem(new ClickMenuItem(vehicleName, "Lösche den Eintrag", $"${entry.price}", "SUPPORT_DELETE_VEHICLE_SELLER_ENTRY")
                    .withData(new Dictionary<string, dynamic> {{"Module", this}, {"Entry", entry}}));
            }

            menu.addMenuItem(new MenuMenuItem(supportMenu.Name, supportMenu, MenuItemStyle.yellow));
        }

        return [new MenuMenuItem(menu.Name, menu)];
    }

    public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
        return new StaticMenuItem("Fahrradladen Modul", $"Ein Modul in welchem Fahrräder gekauft und auch gebraucht wieder verkauft werden können", "");
    }

    public override void onRemove() { }
}