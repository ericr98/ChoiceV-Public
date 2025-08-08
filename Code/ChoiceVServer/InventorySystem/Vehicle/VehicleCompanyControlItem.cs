using AltV.Net.CApi;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;
using System.Linq;
using ClickMenuItem = ChoiceVServer.Model.Menu.ClickMenuItem;

namespace ChoiceVServer.InventorySystem;

public class VehicleCompanyControlItemController : ChoiceVScript {
    public VehicleCompanyControlItemController() {
        VehicleController.addSelfMenuElement(
            new ConditionalVehicleGeneratedSelfMenuElement(
                (v, p) => new ListMenuItem("Firmenfahrzeugkontrolle einbauen", "Bei die Möglichkeit ein das Fahrzeug mit der Firmenkarte zu steuern", CompanyController.getCompaniesWithPermission(p, "COMPANY_VEHICLE_CONTROL").Select(c => c.ShortName).ToArray(), "COMPANY_CAR_CONTROL_SET")
                    .needsConfirmation("Firmenfahrzeugkontrolle einbauen?", "Gerät wirklich einbauen?"),
                v => v.RegisteredCompany == null,
                p => p.getInventory().hasItem<VehicleCompanyControlItem>(_ => true)
            )
        );
        EventController.addMenuEvent("COMPANY_CAR_CONTROL_SET", onCompanyCarControlSet);
        
        VehicleController.addSelfMenuElement(
            new ConditionalVehicleGeneratedSelfMenuElement(
                "Firmenfahrzeugkontrolle",
                generateCompanyVehicleControlMenu,
                v => v.RegisteredCompany != null,
                p => true
            )
        );
        EventController.addMenuEvent("COMPANY_CAR_CONTROL_REMOVE", onCompanyCarControlRemove);
    }
    
    private bool onCompanyCarControlSet(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
        var item = player.getInventory().getItem<ToolItem>(i => i.Flag == SpecialToolFlag.Screwdriver);
        if(item == null) {
            player.sendBlockNotification("Du benötigst einen Schraubenzieher um das Firmenfahrzeug zu steuern.", "Firmenfahrzeugkontrolle", Constants.NotifactionImages.Device);
            return false;
        }
        
        var evt = menuitemcefevent as ListMenuItem.ListMenuItemEvent;
        var vehicle = (ChoiceVVehicle)player.Vehicle;
        var company = CompanyController.findCompany(c => c.ShortName == evt.currentElement);

        if(vehicle.hasPlayerAccess(player) && vehicle.LockState == VehicleLockState.Unlocked) {
            player.sendBlockNotification("Du musst Zugriff auf das Fahrzeug haben und es muss entsperrt sein um das Gerät einzubauen.", "Firmenfahrzeugkontrolle", Constants.NotifactionImages.Device);
            return false;
        }

        if(company != null) {
            var controlItem = player.getInventory().getItem<VehicleCompanyControlItem>(_ => true);
            if(!player.getInventory().removeItem(controlItem)) {
                player.sendBlockNotification("Du benötigst ein Firmenfahrzeugkontrollgerät um das Fahrzeug zu steuern.", "Firmenfahrzeugkontrolle", Constants.NotifactionImages.Device);
                return true;
            }
            
            vehicle.RegisteredCompany = company;
            VehicleController.saveVehicle(vehicle);

            item.use(player);
            player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast das Fahrzeug erfolgreich auf {company.Name} registriert.", "Firmenfahrzeugkontrolle", Constants.NotifactionImages.Device);
        } else {
            Logger.logError($"onCompanyCarControlSet: Company {evt.currentElement} not found");
        }
        
        return true;
    }
    
    private Menu generateCompanyVehicleControlMenu(ChoiceVVehicle vehicle, IPlayer player) {
        var menu = new Menu("Firmenfahrzeugkontrolle", "was möchtest du tun?");
        
        menu.addMenuItem(new StaticMenuItem("Firmenfahrzeugkontrolle", $"Das Fahrzeug ist auf {vehicle.RegisteredCompany.Name} registriert.", vehicle.RegisteredCompany.Name, MenuItemStyle.normal));
        
        if(CompanyController.hasPlayerPermission(player, vehicle.RegisteredCompany, "COMPANY_VEHICLE_CONTROL")) {
            if(player.getInventory().hasItem<ToolItem>(i => i.Flag == SpecialToolFlag.Screwdriver)) {
                menu.addMenuItem(new ClickMenuItem("Gerät ausbauen", "Baue das Firmenfahrzeugkontrollgerät aus", "", "COMPANY_CAR_CONTROL_REMOVE", MenuItemStyle.yellow)
                    .withData(new Dictionary<string, dynamic> { { "Vehicle", vehicle } })
                    .needsConfirmation("Gerät ausbauen?", "Gerät wirklich ausbauen?"));
            } else {
                menu.addMenuItem(new StaticMenuItem("Gerät ausbauen nicht möglich", "Du benötigst einen Schraubenzieher um das Gerät auszubauen.", "", MenuItemStyle.yellow));
            }
        } else {
            menu.addMenuItem(new StaticMenuItem("Keine Berechtigung", "Du hast keine Berechtigung das Gerät zu bedienen.", "", MenuItemStyle.red));
        }

        return menu;
    }
    
    
    private bool onCompanyCarControlRemove(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
        var item = player.getInventory().getItem<ToolItem>(i => i.Flag == SpecialToolFlag.Screwdriver);
        
        if(item == null) {
            player.sendBlockNotification("Du benötigst einen Schraubenzieher um das Firmenfahrzeug zu steuern.", "Firmenfahrzeugkontrolle", Constants.NotifactionImages.Device);
            return false;
        }
        
        if(!player.getInventory().addItem(InventoryController.createItem(InventoryController.getConfigItemForType<VehicleCompanyControlItem>(), 1))) {
            player.sendBlockNotification("Du hast keinen Platz um das Firmenfahrzeugkontrollgerät auszubauen.", "Firmenfahrzeugkontrolle", Constants.NotifactionImages.Device);
            return false;
        }
        
        var vehicle = data["Vehicle"] as ChoiceVVehicle;
        vehicle.RegisteredCompany = null;
        VehicleController.saveVehicle(vehicle);
        
        item.use(player);
        
        player.sendNotification(Constants.NotifactionTypes.Info, "Du hast das Firmenfahrzeugkontrollgerät erfolgreich ausgebaut.", "Firmenfahrzeugkontrolle", Constants.NotifactionImages.Device);
        
        return true;
    }
}

public class VehicleCompanyControlItem : Item {

    public VehicleCompanyControlItem(item item) : base(item) { }

    public VehicleCompanyControlItem(configitem configItem, int amount, int quality) : base(configItem) { }
}