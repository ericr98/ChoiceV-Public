using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.CApi.ClientEvents;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Money;
using ChoiceVServer.Controller.Shopsystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.VisualBasic;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller;

public class VehicleSellerController : ChoiceVScript {
    private bankaccount VehicleSellerBankAccount;

    public VehicleSellerController() {
        EventController.addMenuEvent("ON_PLAYER_BUY_BIKE", onPlayerBuyBike);
        EventController.addMenuEvent("ON_PLAYER_SELL_BIKE", onPlayerSellBike);

        var accL = BankController.getControllerBankaccounts(typeof(ShopController));
        VehicleSellerBankAccount = accL is { Count: > 0 }
            ? accL.First()
            : BankController.createBankAccount(typeof(ShopController), "Fahrzeughändler", BankAccountType.CompanyKonto, 0, BankController.getBankByType(BankCompanies.LibertyBank), true);


        #region Support Stuff

        EventController.addMenuEvent("SUPPORT_CREATE_VEHICLE_SELLER_ENTRY", onSupportCreateVehicleSellerEntry);
        EventController.addMenuEvent("SUPPORT_DELETE_VEHICLE_SELLER_ENTRY", onSupportDeleteVehicleSellerEntry);

        PedController.addNPCModuleGenerator("NPC-Fahrzeughandel", npcVehicleTraderGenerator, npcVehicleTraderCallback);

        #endregion
    }    
    
    private bool onPlayerBuyBike(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var model = (configvehiclesmodel)data["BikeModel"];
        var price = (decimal)data["Price"];

        var randomColors = new List<byte> {0, 28, 55, 73, 91, 123};
        var shopMenuItems = MoneyController.getPaymentMethodsMenu(player, price, model.DisplayName, VehicleSellerBankAccount.id, (p, successFull) => {
            if(successFull) {
                var vehicle = VehicleController.createVehicle(ChoiceVAPI.Hash(model.ModelName), player.Position, player.Rotation, player.Dimension, randomColors[new Random().Next(0, randomColors.Count)]);
                var cfg = InventoryController.getConfigItemForType<VehicleKey>();
                var vehicleKey = new VehicleKey(cfg, vehicle);
                var vehicleKey2 = new VehicleKey(cfg, vehicle);

                player.getInventory().addItem(vehicleKey, true);
                player.getInventory().addItem(vehicleKey2, true);

                player.sendNotification(NotifactionTypes.Success, $"Du hast das/den {model.DisplayName} erfolgreich erworben!", "Fahrzeug erworben", NotifactionImages.Shop);

                InvokeController.AddTimedInvoke("Vehicle-Buyer-Setter", (i) => {
                    player.SetIntoVehicle(vehicle, 0);
                }, TimeSpan.FromSeconds(4), false);
            }
        });

        var menu = new Menu($"{model.DisplayName} kaufen?", $"Wirklich für ${price} kaufen");
        foreach(var item in shopMenuItems) menu.addMenuItem(item);
        player.showMenu(menu);
        return true;
    }

    private bool onPlayerSellBike(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var vehicle = (ChoiceVVehicle)data["Vehicle"];
        var price = (decimal)data["Price"];

        var shopMenuItems = MoneyController.getReceivingPaymentsMethodsMenu(player, price, vehicle.DbModel.DisplayName, VehicleSellerBankAccount.id, (p, willWork, failMessage, sendMoneyCallback) => {
            if(willWork) {
                var keys = player.getInventory().getItems<VehicleKey>(v => v.worksForCar(vehicle));
                keys.ForEach(k => player.getInventory().removeItem(k));
                VehicleController.removeVehicle(vehicle);
                sendMoneyCallback.Invoke();
            }
        });
        
        var menu = new Menu($"{vehicle.DbModel.DisplayName} verkaufen?", $"Wirklich für ${price} verkaufen?");
        foreach(var item in shopMenuItems) menu.addMenuItem(item);
        player.showMenu(menu);
        return true;
    }

    #region SupportStuff

    private bool onSupportCreateVehicleSellerEntry(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var module = (NPCVehicleSellerModule)data["Module"];

        var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

        var modelNameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
        var priceEvt = evt.elements[1].FromJson<InputMenuItemEvent>();

        var model = VehicleController.getVehicleModelByName(modelNameEvt.input);
        module.AvailableVehicles.Add((model.id, decimal.Parse(priceEvt.input)));

        PedController.updatePedModuleSettings(module, "AvailableVehicles", module.AvailableVehicles.ToJson());

        player.sendNotification(NotifactionTypes.Success, $"Eintrag für {model.DisplayName} für ${priceEvt.input} erstellt", "Eintrag erstellt");

        return true;
    }

    private bool onSupportDeleteVehicleSellerEntry(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var module = (NPCVehicleSellerModule)data["Module"];
        var entry = ((int, decimal))data["Entry"];

        module.AvailableVehicles.RemoveAll(r => r.vehicleConfigId == entry.Item1);

        PedController.updatePedModuleSettings(module, "AvailableVehicles", module.AvailableVehicles.ToJson());

        player.sendNotification(NotifactionTypes.Warning, $"Eintrag für {entry.Item1} gelöscht", "Eintrag gelöscht");

        return true;
    }

    private List<MenuItem> npcVehicleTraderGenerator(ref Type codeType) {
        codeType = typeof(NPCVehicleSellerModule);

        return [];
    }

    private void npcVehicleTraderCallback(IPlayer player, MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
        creationFinishedCallback.Invoke(new Dictionary<string, dynamic> {{"AvailableVehicles", new List<(int, decimal)> {}.ToJson()}});
    }

    #endregion
}