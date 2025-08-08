using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Controller.Money;
using ChoiceVServer.Controller.Shopsystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Clothing {
    public enum ClothingPropShops {
        NoShop = -1,
        HatsShop = 0,
        SunglassesShop = 1,
        Placeholder1 = 2,
        Placeholder2 = 3,
        Placeholder3 = 4,
        Placeholder4 = 5,
        Placeholder5 = 6,
        Placeholder6 = 7,
        Placeholder7 = 8,
        Placeholder8 = 9,
        Placeholder9 = 10,
        Placeholder10 = 11,
        Placeholder11 = 12,
        Placeholder12 = 13,
        Placeholder13 = 14,
        Placeholder14 = 15,
    }

    public class ClothingPropShopController : ChoiceVScript {
        private static bankaccount ShopBankaccount;

        public ClothingPropShopController() {

            EventController.addMenuEvent("ON_SELECT_CLOTHING_PROP", onPlayerSelectViewProp);

            InteractionController.addObjectInteractionCallback(
                "OPEN_SUNGLASSES_SHOP",
                "Sonnenbrillen Laden öffnen",
                onOpenSunglassesShop
            );

            InteractionController.addObjectInteractionCallback(
                "OPEN_HAT_SHOP",
                "Hut Laden öffnen",
                onOpenHatsShop
            );

            var accL = BankController.getControllerBankaccounts(typeof(ShopController));
            ShopBankaccount = accL is { Count: > 0 }
                ? accL.First()
                : BankController.createBankAccount(typeof(ShopController), "Kleidungsaccessoires-Konto", BankAccountType.CompanyKonto, 0, BankController.getBankByType(BankCompanies.LibertyBank), true);
        }

        private void onOpenSunglassesShop(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading, bool isBroken, ref Menu menu) {
            var shopMenu = generateClothingPropShop(player, ClothingPropShops.SunglassesShop, player.getCharacterData().Gender);

            player.showMenu(shopMenu, false);
        }

        private void onOpenHatsShop(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading, bool isBroken, ref Menu menu) {
            var shopMenu = generateClothingPropShop(player, ClothingPropShops.HatsShop, player.getCharacterData().Gender);

            player.showMenu(shopMenu, false);
        }

        private static void resetPlayerClothing(IPlayer player) {
            ClothingController.loadPlayerClothing(player, player.getClothing());
        }

        public static Menu generateClothingPropShop(IPlayer player, ClothingPropShops shopType, char gender) {
            var menu = new Menu(getShopNameByType(shopType), "Was möchtest du kaufen?", resetPlayerClothing);
            var cash = player.getCash();
            var adminMode = player.getCharacterData().AdminMode;
            var dict = new Dictionary<int, configitem>();

            using(var db = new ChoiceVDb()) {
                var props = db.configclothingprops
                    .Include(p => p.configclothingpropvariations)
                    .Where(p => (p.shopType == (int)shopType | p.configclothingpropvariations.Any(v => v.overrideShopType == (int)shopType))
                    && p.gender == gender.ToString() && (p.notBuyable != 1 || adminMode))
                    .GroupBy(c => c.name).Select(c => c.ToList()).ToList();

                foreach(var sameNameProps in props) {
                    var name = sameNameProps.First().name;
                    var virtualMenu = new VirtualMenu(name, () => {
                        var categoryMenu = new Menu(name, "Was möchtest du kaufen?", resetPlayerClothing);
                        var variations = sameNameProps.SelectMany(p => p.configclothingpropvariations)
                        .Where(v => ((v.overrideShopType == null && v.prop.shopType == (int)shopType) || v.overrideShopType == (int)shopType) && (adminMode || v.overrideNotBuyable != 1));
                        foreach(var variation in variations) {
                            var price = variation.prop.price;
                            if(variation.overridePrice != null) {
                                price = variation.overridePrice ?? -1;
                            }

                            configitem cfgItem;
                            if(!dict.ContainsKey(variation.prop.componentid)) {
                                cfgItem = InventoryController.getConfigItemByCodeIdentifier(getCodeIdentifierForComponentId(variation.prop.componentid));
                                dict.Add(variation.prop.componentid, cfgItem);
                            } else {
                                cfgItem = dict[variation.prop.componentid];
                            }

                            var variationName = variation.name;
                            if(adminMode) {
                                variationName = $"({variation.prop.id}:{variation.variation}) {variation.name}";
                            }

                            var shownBecauseOfAdmin = adminMode && (variation.overrideNotBuyable == 1 || variation.prop.notBuyable == 1);

                            if(player.getInventory().canFitItem(cfgItem)) {
                                categoryMenu.addMenuItem(new ClickMenuItem(variationName, $"Kaufe {variation.name} für den Preis von ${price}", $"${price}", "ON_SELECT_CLOTHING_PROP", shownBecauseOfAdmin ? MenuItemStyle.red : MenuItemStyle.normal, true)
                                    .withData(new Dictionary<string, dynamic> { { "Variation", variation }, { "Price", price } }));
                            } else {
                                var message = $"Kein Platz dafür! ({cfgItem.weight}kg)";
                                categoryMenu.addMenuItem(new HoverMenuItem(variationName, $"{variation.name} kaufbar für den Preis von ${price}", message, "ON_SELECT_CLOTHING_PROP", shownBecauseOfAdmin ? MenuItemStyle.red : MenuItemStyle.yellow)
                                    .withData(new Dictionary<string, dynamic> { { "Variation", variation } }));
                            }
                        }

                        return categoryMenu;
                    });

                    menu.addMenuItem(new MenuMenuItem(virtualMenu.Name, virtualMenu));
                }
            }

            return menu;
        }

        private static string getShopNameByType(ClothingPropShops shopType) {
            switch(shopType) {
                case ClothingPropShops.SunglassesShop:
                    return "Sonnenbrillenladen";
                case ClothingPropShops.HatsShop:
                    return "Laden für Kopfbedeckungen";
                default:
                    return "Unbekannt!";
            }
        }

        private static string getCodeIdentifierForComponentId(int componentId) {
            switch(componentId) {
                case 0:
                    return "CLOTHING_PROP_HAT";
                case 1:
                    return "CLOTHING_PROP_GLASSES";
                case 2:
                    return "CLOTHING_PROP_EAR";
                default:
                    return "unknown";
            }
        }

        private bool onPlayerSelectViewProp(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(menuItemCefEvent.action == "changed") {
                var cfg = (configclothingpropvariation)data["Variation"];

                ChoiceVAPI.SetPlayerClothingProp(player, cfg.prop.componentid, cfg.prop.drawableid, cfg.variation, cfg.prop.dlc);
            } else {
                var cfg = (configclothingpropvariation)data["Variation"];
                var price = (decimal)data["Price"];

                var menuItems = MoneyController.getPaymentMethodsMenu(player, price, $"{cfg.prop.name}: {cfg.name}", ShopBankaccount.id, (_, worked) => {
                    if(worked) {
                        var cfgItem = InventoryController.getConfigItemByCodeIdentifier(getCodeIdentifierForComponentId(cfg.prop.componentid));
                        var item = new ClothingProp(cfgItem, cfg);
                        
                        player.getInventory().addItem(item, true);
                    }
                });

                var menu = new Menu("Zahlungsmethode wählen", "Wähle deine Zahlungsmethode");
                foreach(var menuItem in menuItems) {
                    menu.addMenuItem(menuItem);
                }

                player.showMenu(menu, false);
            }

            return true;
        }
    }
}
