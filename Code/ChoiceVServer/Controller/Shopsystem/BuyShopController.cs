using AltV.Net.Elements.Entities;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Crime_Stuff;
using ChoiceVServer.Controller.Money;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Renci.SshNet.Messages.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.Shopsystem {
    public class BuyShopController : ChoiceVScript {
        private readonly bankaccount ShopBankaccount;
        
        public BuyShopController() {
            EventController.addMenuEvent("SHOP_PED_SELL_ITEM", onShopPedSellItem);
            
            var accL = BankController.getControllerBankaccounts(typeof(ShopController));
            ShopBankaccount = accL is { Count: > 0 }
                ? accL.First()
                : BankController.createBankAccount(typeof(BuyShopController), "Ankauf-Shop-Konto", BankAccountType.CompanyKonto, 0, BankController.getBankByType(Constants.BankCompanies.LibertyBank), true);
            
            #region Support
            
            PedController.addNPCModuleGenerator("Item-Ankauf-Modul", buyPedModuleGenerator, buyPedModuleGeneratorCallback);
            EventController.addMenuEvent("SUPPORT_SHOP_PED_CREATE_ITEM", onSupportShopPedCreateItem);
            EventController.addMenuEvent("SUPPORT_SHOP_PED_CHANGE_ITEM", onSupportShopPedChangeItem);
            EventController.addMenuEvent("SUPPORT_SHOP_PED_DELETE_ITEM", onSupportShopPedDeleteItem);
            
            #endregion
        }

        private bool onShopPedSellItem(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var evt = menuitemcefevent as InputMenuItem.InputMenuItemEvent;
            var perUnit = (bool)data["PerUnit"];
            var inventory = (Inventory)data["Inventory"];

            if(perUnit) {
                if(!int.TryParse(evt.input, out var amount) || amount <= 0) {
                    player.sendBlockNotification("Fehler bei der Eingabe! Gib eine gültige Menge an", "Ungültige Menge");
                    return false;
                }

                var item = (ShopPedBuyItem)data["Item"];

                var fittingItems = inventory.getItems(i => i.ConfigId == item.ConfigItemId);

                if(amount > fittingItems.Sum(i => i.StackAmount ?? 1)) {
                    player.sendBlockNotification("Du hast nicht genügend Items", "Nicht genügend Items");
                    return false;
                }

                var price = item.Price * amount;
                var configItem = InventoryController.getConfigById(item.ConfigItemId);
                
                var itemName = configItem.name;
                if(configItem.suspicious == 1) {
                    itemName = $"\"{CrimeBaseTraderController.getCodedNameForItem(configItem.configItemId)}\"";
                }

                var menuItems = MoneyController.getReceivingPaymentsMethodsMenu(player, price, $"{amount}x {itemName}", ShopBankaccount.id, (_, willWork, failMessage, action) => {
                    if(willWork) {
                        if(!inventory.removeSimelarItems(fittingItems.First(), amount)) {
                            player.sendBlockNotification("Fehler beim Verkauf: Items konnten nicht entfernt werden", "Fehler");
                        } else {
                            action.Invoke();

                            if(configItem.suspicious == 1) {
                                CrimeNetworkController.OnPlayerCrimeActionDelegate.Invoke(player, CrimeAction.IllegalItemSell, amount, new Dictionary<string, dynamic> {
                                    { "SellNotBuy", true },
                                    { "ConfigItem", fittingItems.First().ConfigItem },
                                    { "Position", player.Position },
                                });
                            }
                        }    
                    } else {
                        player.sendBlockNotification($"Fehler beim Verkauf: {failMessage}", "Fehler");
                    }     
                });

                var showMenu = new Menu("Zahlungsmethoden", "Wie möchtest du bezahlt werden?");
                foreach(var menuItem in menuItems) {
                   showMenu.addMenuItem(menuItem);
                }
                player.showMenu(showMenu);
            } else {
               //TODO 
            }

            return true;
        }

        #region Support

        private List<MenuItem> buyPedModuleGenerator(ref Type codetype) {
            codetype = typeof(ShopBuyPedModule);

            return new List<MenuItem> {
                new InputMenuItem("Name", "Name des Shop", "", ""),
                new CheckBoxMenuItem("Ladezone erstellen", "Soll eine Ladezone erstellt werden?", false, "")
            };
        }

        private void buyPedModuleGeneratorCallback(IPlayer player, MenuStatsMenuItem.MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationfinishedcallback) {
            var nameEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            var createLoadingZoneEvt = evt.elements[1].FromJson<CheckBoxMenuItem.CheckBoxMenuItemEvent>();

            if(createLoadingZoneEvt.check) {
                player.sendNotification(Constants.NotifactionTypes.Info, "Erstelle nun die Fahrzeugladezone des Shops. Gehe an die Position und drücke E um die Ladezone zu erstellen.", "");
                CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (position, width, height, rotation) => {
                    var collisionShape = CollisionShape.Create(position, width, height, rotation, false, true, false);
                    var colShapeStr = collisionShape.toShortSave();
                    collisionShape.Dispose();

                    creationfinishedcallback.Invoke(new Dictionary<string, dynamic> {
                        { "Name", nameEvt.input },
                        { "LoadingZone", colShapeStr },
                    });
                });
            } else {
                creationfinishedcallback.Invoke(new Dictionary<string, dynamic> {
                    { "Name", nameEvt.input },
                    { "LoadingZone", "" },
                });
            }
        }
        
        private bool onSupportShopPedCreateItem(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var shop = (ShopBuyPedModule)data["Shop"];

            var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;
            var configEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            var priceEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();
            var pricePerUnitEvt = evt.elements[2].FromJson<CheckBoxMenuItem.CheckBoxMenuItemEvent>();
            
            var configItem = InventoryController.getConfigById(int.Parse(configEvt.input.Split(":")[0]));
            var price = decimal.Parse(priceEvt.input);
            var pricePerUnit = pricePerUnitEvt.check;
            
            shop.BuyItems.Add(new ShopPedBuyItem(configItem.configItemId, price, pricePerUnit));
            PedController.updatePedModuleSettings(shop, "BuyItems", shop.BuyItems.ToJson());
            
            player.sendNotification(Constants.NotifactionTypes.Success, "Item hinzugefügt", "");

            return true;
        }

        private bool onSupportShopPedChangeItem(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var shop = (ShopBuyPedModule)data["Shop"];
            var item = (ShopPedBuyItem)data["Item"];

            var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;
            var configEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            var priceEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();
            var pricePerUnitEvt = evt.elements[2].FromJson<CheckBoxMenuItem.CheckBoxMenuItemEvent>();

            var configItem = InventoryController.getConfigById(int.Parse(configEvt.input.Split(":")[0]));
            var price = decimal.Parse(priceEvt.input);
            var pricePerUnit = pricePerUnitEvt.check;

            var newItem = new ShopPedBuyItem(configItem.configItemId, price, pricePerUnit);
            shop.BuyItems[shop.BuyItems.IndexOf(item)] = newItem;
            PedController.updatePedModuleSettings(shop, "BuyItems", shop.BuyItems.ToJson());
            
            player.sendNotification(Constants.NotifactionTypes.Success, "Item geändert", "");

            return true;
        }

        private bool onSupportShopPedDeleteItem(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var shop = (ShopBuyPedModule)data["Shop"];
            var item = (ShopPedBuyItem)data["Item"];

            shop.BuyItems.Remove(item);
            PedController.updatePedModuleSettings(shop, "BuyItems", shop.BuyItems.ToJson());
            
            player.sendNotification(Constants.NotifactionTypes.Success, "Item gelöscht", "");

            return true;
        }
        
        #endregion
    }
}