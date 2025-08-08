using AltV.Net.Elements.Entities;
using BenchmarkDotNet.Disassemblers;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Money;
using ChoiceVServer.Controller.Shopsystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.CheckBoxMenuItem;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Clothing {
    public enum ClothingShopTypes {
        None = -1,
        Shoes = 1,
        Legs = 2,
        Accessoires = 3,
        Tops = 4,
        Masks = 5,
    }

    public class ClothingShopController : ChoiceVScript {
        internal static bankaccount ShopBankaccount;

        public ClothingShopController() {
            EventController.addMenuEvent("SELECT_SINGLE_COMPONENT_CLOTHES", onPlayerSelectSingleComponentClothes);
            EventController.addMenuEvent("SELECT_SINGLE_COMPONENT_CLOTHES_LOAD_STEP", onSelectComponentClothesLoadStep);
            EventController.addMenuEvent("PUT_ON_NEW_CLOTHING", onPlayerPutOnNewClothing);

            EventController.addMenuEvent("RESET_PLAYER_CLOTHING", onResetPlayerClothing);

            #region Support

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    () => new ClickMenuItem("Top-Shirt-Torso Menü", "Füge die aktuelle Torso-Shirt-Top Kombination hinzu", "", "SUPPORT_ADD_TOP_COMBINATION_MENU"),
                    3,
                    SupportMenuCategories.Kleidung
                )
            );

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    () => new InputMenuItem("Top-Shirt-Torso Überprüfung", "Überprüfe die Kombinationen. Gib einen Startwert ein (es gibt ca. 10k Kombinationen)", "", "SUPPORT_START_TOP_COMBINATION_VERIFICATION"),
                    3,
                    SupportMenuCategories.Kleidung
                )
            );

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    () => new InputMenuItem("Namenlose Kleidung Überprüfung", "Überprüfe alle namenlose Kleidung auf ihre Benutzbarkeit", "ClothingId,StartIndex", "SUPPORT_NAMELESS_CLOTHES_VERIFICATION"),
                    3,
                    SupportMenuCategories.Kleidung
                )
            );

            EventController.addMenuEvent("SUPPORT_ADD_TOP_COMBINATION_MENU", onSupportAddTopCombinationMenu);
            EventController.addMenuEvent("SUPPORT_ON_SHOW_COMBO", onSupportShowCombination);
            EventController.addMenuEvent("SUPPORT_ADD_TOP_COMBINATION", onSupportAddTopCombination);

            EventController.addMenuEvent("SUPPORT_START_TOP_COMBINATION_VERIFICATION", onStartTopCombinationVerification);
            EventController.addMenuEvent("SUPPORT_DELETE_TOP_COMBINATION", onSupportDeleteTopCombination);
            EventController.addMenuEvent("SUPPORT_SELECT_NEXT_TOP_COMBINATION", onSupportSelectNextTopCombination);

            EventController.addMenuEvent("SUPPORT_NAMELESS_CLOTHES_VERIFICATION", onStartNamelessClothesVerification);
            EventController.addMenuEvent("SUPPORT_SELECT_NEXT_NAMELESS_CLOTHING", onSupportSelectNextNamelessClothing);
            EventController.addMenuEvent("SUPPORT_SET_NAMELESS_CLOTHING_NAME", onSupportSetNamelessClothingName);

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    allClothingMenuGenerator,
                    4,
                    SupportMenuCategories.Kleidung,
                    "Alle Kleidungsstücke"
                )
            );
            EventController.addMenuEvent("SUPPORT_SELECT_CLOTHING", onSupportSelectClothing);

            #endregion

            PedController.addNPCModuleGenerator("Kleidungsladenmodul", clothingShopGenerator, clothingGeneratorCallback);

            var accL = BankController.getControllerBankaccounts(typeof(ShopController));
            ShopBankaccount = accL is { Count: > 0 }
                ? accL.First()
                : BankController.createBankAccount(typeof(ShopController), "Kleidungsaccessoires-Konto", BankAccountType.CompanyKonto, 0, BankController.getBankByType(BankCompanies.LibertyBank), true);
        }

        private bool onResetPlayerClothing(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            resetPlayerClothing(player);

            return true;
        }

        public static void resetPlayerClothing(IPlayer player) {
            ClothingController.loadPlayerClothing(player, player.getClothing());
        }

        private static string getItemCodeIdentifier(int componentId) {
            switch(componentId) {
                case 1:
                    return "CLOTHING_MASKS";
                case 6:
                    return "CLOTHING_SHOES";
                case 4:
                    return "CLOTHING_PANTS";
                case 7:
                    return "CLOTHING_ACCESSOIRE";
                case 11:
                    return "CLOTHING_TOP";
                default:
                    return null;
            }
        }

        public static string getComponentName(int componentId) {
            switch(componentId) {
                case 1:
                    return "Masken";
                case 6:
                    return "Schuhe";
                case 4:
                    return "Hosen";
                case 7:
                    return "Accessoires";
                case 11:
                    return "Oberteile";
                default:
                    return "Unbekannt";
            }
        }

        public static Menu getSingleComponentClothShopMenu(IPlayer player, ClothingShopTypes shopType, ClothingType type, bool showPrices, Action<decimal, string, Action> buyAndExecuteCallback = null) {
            var menu = new Menu(getNameOfSingleComponentClothingShop(shopType), "Was möchtest du kaufen?", resetPlayerClothing, true);
            var componentId = getComponentOfClothingShop(shopType);
            var genderStr = player.getCharacterData().Gender.ToString();

            var adminMode = player.getCharacterData().AdminMode;

            var dict = new Dictionary<int, bool>();

            using(var db = new ChoiceVDb()) {
                var viableClothing = db.configclothings
                    .Include(c => c.configclothingvariations)
                    .Where(c => c.componentid == componentId
                        && (c.gender == "U" || c.gender == genderStr)
                        && (c.notBuyable == 0 || adminMode)
                        && ((c.clothingType == (int)type && c.configclothingvariations.Any(v => v.overrideClothingType == null && v.overrideNotBuyable != 1)) || c.configclothingvariations.Any(v => v.overrideClothingType == (int)type && v.overrideNotBuyable != 1)))
                        .GroupBy(c => c.name).Select(a => a.Select(a1 => new { Clothing = a1, ComboCount = a1.configclothingshirtstotoptops.Where(c => c.shirt.notBuyable != 1).Count() })).ToList();

                foreach(var sameNameclothings in viableClothing) {
                    var name = sameNameclothings.First().Clothing.name;
                    var virtualMenu = new VirtualMenu(name, () => {
                        var subMenu = new Menu(name, "Was möchtest du kaufen?");
                        using(var db = new ChoiceVDb()) {
                            foreach(var clothing in sameNameclothings) {
                                var variations = db.configclothingvariations
                                .Where(c => c.clothingId == clothing.Clothing.id
                                        && ((clothing.Clothing.clothingType == (int)type && c.overrideClothingType == null) || c.overrideClothingType == (int)type)
                                        && (c.overrideNotBuyable != 1 || adminMode)).ToList();

                                foreach(var variation in variations) {
                                    var price = clothing.Clothing.price;
                                    if(variation.overridePrice != null) {
                                        price = variation.overridePrice ?? -1;
                                    }
                                    var priceString = showPrices ? $"${price}" : "";

                                    var canFit = false;
                                    if(!dict.ContainsKey(clothing.Clothing.componentid)) {
                                        var cfg = InventoryController.getConfigItemByCodeIdentifier(getItemCodeIdentifier(clothing.Clothing.componentid));
                                        canFit = player.getInventory().canFitItem(cfg);
                                        dict.Add(clothing.Clothing.componentid, canFit);
                                    } else {
                                        canFit = dict[clothing.Clothing.componentid];
                                    }

                                    var variationName = variation.name;
                                    if(adminMode) {
                                        variationName = $"({clothing.Clothing.id}:{variation.variation}) {variation.name}";
                                    }

                                    //Has no Shirts
                                    if(clothing.ComboCount == 0) {
                                        var dic = new Dictionary<string, dynamic> {
                                                { "Clothing", clothing.Clothing },
                                                { "Variation", variation },
                                                { "BuyCallback", buyAndExecuteCallback } };

                                        if(clothing.Clothing.componentid == 11 && clothing.Clothing.torsoId == null) {
                                            player.sendBlockNotification($"FEHLER: Kleidungsstück {clothing.Clothing.id} aufschreiben und bitte im Support melden. Kein richtiger Torso gesetzt!", "Kein Torso gesetzt!");
                                            continue;
                                        } else if(clothing.Clothing.componentid == 11) {
                                            dic.Add("Torso", clothing.Clothing.torsoId);
                                        }

                                        var shownBecauseOfAdmin = adminMode && (variation.overrideNotBuyable == 1 || clothing.Clothing.notBuyable == 1);
                                        if(canFit) {
                                            subMenu.addMenuItem(new ClickMenuItem(variationName, $"Kaufe {variation.name} für den Preis von {priceString}", priceString, "SELECT_SINGLE_COMPONENT_CLOTHES", shownBecauseOfAdmin ? MenuItemStyle.red : MenuItemStyle.normal, true)
                                                .withData(dic));
                                        } else {
                                            subMenu.addMenuItem(new HoverMenuItem(variationName, $"Kaufe {variation.name} für den Preis von {priceString}", $"Kein Platz ({priceString})", "SELECT_SINGLE_COMPONENT_CLOTHES", shownBecauseOfAdmin ? MenuItemStyle.red : MenuItemStyle.yellow)
                                                  .withData(dic));
                                        }
                                    } else {
                                        var virtCompMenu = new VirtualMenu(variationName, () => {
                                            var topMenu = new Menu(variationName, "Welches Shirt initial beifügen?");
                                            var viableShirts = ClothingController.getCompatibleShirtsForTop(genderStr, clothing.Clothing.drawableid, clothing.Clothing.dlc);

                                            //If top goes without shirt also add this option
                                            if(clothing.Clothing.torsoId != null) {
                                                var nakedShirt = Constants.NakedMen.Shirt;
                                                if (genderStr == "F") {
                                                    nakedShirt = Constants.NakedFemale.Shirt;
                                                }

                                                topMenu.addMenuItem(new ClickMenuItem("Ohne Shirt", "Ziehe das Oberteil Ohne Shirt an", priceString, "SELECT_SINGLE_COMPONENT_CLOTHES", MenuItemStyle.normal, true)
                                                    .withData(new Dictionary<string, dynamic> {
                                                        { "Clothing", clothing.Clothing },
                                                        { "Variation", variation },
                                                        { "ShirtDrawable", nakedShirt.Drawable },
                                                        { "ShirtVariation", nakedShirt.Texture },
                                                        { "ShirtDlc", nakedShirt.Dlc },
                                                        { "ShirtName", "Oberkörpferfrei "},
                                                        { "Torso", clothing.Clothing.torsoId },
                                                        { "BuyCallback", buyAndExecuteCallback } 
                                                    }));
                                            }

                                            foreach (var shirts in viableShirts.Where(s => s.shirt.notBuyable != 1).GroupBy(s => s.shirt.name).Select(a => a.ToList()).ToList()) {
                                                var shirtVirtMenu = new VirtualMenu(shirts.First().shirt.name, () => {
                                                    var shirtVariationMenu = new Menu(shirts.First().shirt.name, "Welche Variation initial beifügen?");

                                                    foreach(var shirt in shirts) {
                                                        foreach(var shirtVariation in shirt.shirt.configclothingvariations.Where(v => v.overrideNotBuyable != 1)) {
                                                            var shirtName = shirtVariation.name;
                                                            if(adminMode) {
                                                                shirtName = $"({shirt.shirt.id}:{shirtVariation.variation}) {shirtVariation.name}";
                                                            }

                                                            var shownBecauseOfAdmin = adminMode && (variation.overrideNotBuyable == 1);
                                                            if(canFit) {
                                                                shirtVariationMenu.addMenuItem(new ClickMenuItem(shirtName, $"Kaufe {shirtVariation.name} für den Preis von {priceString}", priceString, "SELECT_SINGLE_COMPONENT_CLOTHES", shownBecauseOfAdmin ? MenuItemStyle.red : MenuItemStyle.normal, true)
                                                                      .withData(new Dictionary<string, dynamic> {
                                                                            { "Clothing", clothing.Clothing },
                                                                            { "Variation", variation },
                                                                            { "ShirtDrawable", shirt.shirt.drawableid },
                                                                            { "ShirtVariation", shirtVariation.variation },
                                                                            { "ShirtDlc", shirt.shirt.dlc },
                                                                            { "ShirtName", shirtVariation.name },
                                                                            { "Torso", shirt.TorsoId },
                                                                            { "BuyCallback", buyAndExecuteCallback } 
                                                                        }));
                                                            } else {
                                                                shirtVariationMenu.addMenuItem(new HoverMenuItem(shirtName, $"Kaufe {shirtVariation.name} für den Preis von {priceString}", $"Kein Platz ({priceString})", "SELECT_SINGLE_COMPONENT_CLOTHES", shownBecauseOfAdmin ? MenuItemStyle.red : MenuItemStyle.yellow)
                                                                      .withData(new Dictionary<string, dynamic> {
                                                                            { "Clothing", clothing.Clothing },
                                                                            { "Variation", variation },
                                                                            { "ShirtDrawable", shirt.shirt.drawableid },
                                                                            { "ShirtVariation", shirtVariation.variation },
                                                                            { "ShirtDlc", shirt.shirt.dlc },
                                                                            { "ShirtName", shirtVariation.name },
                                                                            { "Torso", shirt.TorsoId },
                                                                            { "BuyCallback", buyAndExecuteCallback } 
                                                                        }));
                                                            }
                                                        }
                                                    }

                                                    return shirtVariationMenu;
                                                });

                                                topMenu.addMenuItem(new MenuMenuItem(shirtVirtMenu.Name, shirtVirtMenu));
                                            }

                                            return topMenu;
                                        });

                                        if(clothing.Clothing.torsoId != null) {
                                            var nakedShirt = Constants.NakedMen.Shirt;
                                            if(genderStr == "F") {
                                                nakedShirt = Constants.NakedFemale.Shirt;
                                            }

                                            subMenu.addMenuItem(new MenuMenuItem(virtCompMenu.Name, virtCompMenu, MenuItemStyle.normal, "SELECT_SINGLE_COMPONENT_CLOTHES")
                                                .withData(new Dictionary<string, dynamic> {
                                                    { "Clothing", clothing.Clothing },
                                                    { "Variation", variation },
                                                    { "ShirtDrawable", nakedShirt.Drawable },
                                                    { "ShirtVariation", nakedShirt.Texture },
                                                    { "ShirtDlc", nakedShirt.Dlc },
                                                    { "ShirtName", "Ohne Shirt" },
                                                    { "Torso", clothing.Clothing.torsoId },
                                                    { "BuyCallback", buyAndExecuteCallback } }));
                                        } else {
                                            subMenu.addMenuItem(new MenuMenuItem(virtCompMenu.Name, virtCompMenu, MenuItemStyle.normal, "SELECT_SINGLE_COMPONENT_CLOTHES_LOAD_STEP")
                                                .withData(new Dictionary<string, dynamic> {
                                                    { "Clothing", clothing.Clothing },
                                                    { "Variation", variation } }));
                                        }
                                    }
                                }
                            }
                        }

                        return subMenu;
                    });

                    menu.addMenuItem(new MenuMenuItem(virtualMenu.Name, virtualMenu));
                }
            }

            return menu;
        }

        public static string getNameOfSingleComponentClothingShop(ClothingShopTypes shopType) {
            switch(shopType) {
                case ClothingShopTypes.Masks:
                    return "Maskenladen";
                case ClothingShopTypes.Shoes:
                    return "Schuhladen";
                case ClothingShopTypes.Legs:
                    return "Hosenladen";
                case ClothingShopTypes.Accessoires:
                    return "Accessoireladen";
                case ClothingShopTypes.Tops:
                    return "Oberteilladen";
                default:
                    return "Unbekannt";
            }
        }

        private static int getComponentOfClothingShop(ClothingShopTypes shopType) {
            switch(shopType) {
                case ClothingShopTypes.Masks:
                    return 1;
                case ClothingShopTypes.Shoes:
                    return 6;
                case ClothingShopTypes.Legs:
                    return 4;
                case ClothingShopTypes.Accessoires:
                    return 7;
                case ClothingShopTypes.Tops:
                    return 11;
                default:
                    return -1;
            }

        }


        public static string getPriceGroupName(ClothingType type) {
            return type switch {
                ClothingType.Cheap => "Günstig",
                ClothingType.Medium => "Mittelteuer",
                ClothingType.Expensive => "Teuer",
                ClothingType.Modded => "Modded",
                _ => "Fehler",
            };
        }

        private bool onPlayerSelectSingleComponentClothes(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var clothing = (configclothing)data["Clothing"];
            var variation = (configclothingvariation)data["Variation"];
            var shirt = data.ContainsKey("ShirtDrawable") ? (int)data["ShirtDrawable"] : -1;
            var shirtName = data.ContainsKey("ShirtName") ? (string)data["ShirtName"] : "";
            var shirtVariation = data.ContainsKey("ShirtVariation") ? (int)data["ShirtVariation"] : -1;
            var shirtDlc = data.ContainsKey("ShirtDlc") ? (string)data["ShirtDlc"] : "";
            var torso = data.ContainsKey("Torso") ? (int)data["Torso"] : -1;

            if(menuItemCefEvent.action == "changed") {
                showClothes(player, clothing, variation, shirt, shirtVariation, shirtDlc, torso);
            } else {
                var buyCallback = (Action<decimal, string, Action>)data["BuyCallback"];

                var price = clothing.price;
                if(variation.overridePrice is not null) {
                    price = variation.overridePrice ?? 1;
                }

                var cfg = InventoryController.getConfigItemByCodeIdentifier(getItemCodeIdentifier(clothing.componentid));
                ClothingItem item;
                if(clothing.componentid != 11) {
                    item = new ClothingItem(cfg, clothing.drawableid, variation.variation, $"({clothing.gender[0]}) {clothing.name}: {variation.name}", clothing.gender[0], clothing.dlc);
                } else {
                    if(shirt == -1) {
                        item = new TopClothingItem(cfg, clothing.drawableid, variation.variation, $"({clothing.gender[0]}) {clothing.name}: {variation.name}", clothing.gender[0], clothing.dlc, -1, -1, null, torso);
                    } else {
                        item = new TopClothingItem(cfg, clothing.drawableid, variation.variation, $"({clothing.gender[0]}) {clothing.name}: {variation.name}", clothing.gender[0], clothing.dlc, shirt, shirtVariation, shirtDlc, torso);
                        item.Description = $"{item.Description} mit Unterteil: {shirtName}";
                        item.updateDescription();
                    }
                }

                buyCallback.Invoke(price, variation.name, () => {
                    if(player.getInventory().addItem(item, true)) {
                       var menu = new Menu("Kleidung direkt anziehen?", "Das Kleidungsstück direkt anziehen?");
                        menu.addMenuItem(new ClickMenuItem("Nein", "Das Kleidungsstück nicht direkt anziehen", "", "RESET_PLAYER_CLOTHING", MenuItemStyle.red));
                        menu.addMenuItem(new ClickMenuItem("Ja", "Das Kleidungsstück direkt anziehen", "", "PUT_ON_NEW_CLOTHING", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Item", item } }));
                        player.showMenu(menu);
                    } else {
                        player.sendBlockNotification("Etwas ist beim Kleidungskauf fehlgeschlafen!", "Kein Geld!", NotifactionImages.Shop);
                    }
                });
            }

            return true;
        }

        private bool onSelectComponentClothesLoadStep(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var clothing = (configclothing)data["Clothing"];
            var variation = (configclothingvariation)data["Variation"];

            var viableShirts = ClothingController.getCompatibleShirtsForTop(player.getCharacterData().Gender.ToString(), clothing.drawableid, clothing.dlc);

            if(viableShirts.Count > 0) {
                var first = viableShirts.First();
                showClothes(player, clothing, variation, first.shirt.drawableid, first.shirt.configclothingvariations.First().variation, first.shirt.dlc, first.TorsoId);
            } else {
                player.sendBlockNotification("FEHLER: Kleidungsstück aufschreiben und bitte im Support melden. Keine kompatiblen Shirts gefunden!", "Keine Shirts gefunden!");
            }

            return true;
        }

        private static void showClothes(IPlayer player, configclothing clothing, configclothingvariation variation, int shirtDrawable, int shirtVariation, string shirtDlc, int torsoDrawable) {
            if(clothing.gender != "U" && player.getCharacterData().Gender.ToString() != clothing.gender) {
                return;
            }

            //Masks are universal, but after the drawable 189 woman have an id + 1
            var drawableId = clothing.drawableid;
            if(clothing.gender == "U") {
                drawableId = ClothingController.transformUniversalClothesDrawable(player.getCharacterData().Gender.ToString(), clothing.componentid, drawableId);
            }

            if(clothing.componentid == 11) {
                ChoiceVAPI.SetPlayerClothes(player, 3, torsoDrawable, 0);

                if(shirtDrawable != -1) {
                    ChoiceVAPI.SetPlayerClothes(player, 8, shirtDrawable, shirtVariation, shirtDlc);
                } else {
                    var nakedShirt = Constants.NakedMen.Shirt;
                    if(clothing.gender == "F") {
                        nakedShirt = Constants.NakedFemale.Shirt;
                    }
                    ChoiceVAPI.SetPlayerClothes(player, 8, nakedShirt.Drawable, nakedShirt.Texture);
                }
            }

            ChoiceVAPI.SetPlayerClothes(player, clothing.componentid, drawableId, variation.variation, clothing.dlc);
        }

        private bool onPlayerPutOnNewClothing(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (ClothingItem)data["Item"];
            item.equip(player);

            return true;
        }

        #region Support

        private Menu getCombinationMenu(IPlayer player) {
            var topPlace = player.hasData("SUPPORT_COMBINATION_TOP") ? ((int)player.getData("SUPPORT_COMBINATION_TOP")).ToString() : "";
            var shirtPlace = player.hasData("SUPPORT_COMBINATION_SHIRT") ? ((int)player.getData("SUPPORT_COMBINATION_SHIRT")).ToString() : "";
            var torsoPlace = player.hasData("SUPPORT_COMBINATION_TORSO") ? ((int)player.getData("SUPPORT_COMBINATION_TORSO")).ToString() : "";

            var menu = new Menu("Torso-Top-Menü", "Wähle deine Teile aus");

            menu.addMenuItem(new InputMenuItem("Oberteil (Db-Id)", "Setze das Oberteil", topPlace, "", MenuItemStyle.normal, true));
            menu.addMenuItem(new InputMenuItem("Unterteil (Db-Id)", "Setze das Unterteil", shirtPlace, "", MenuItemStyle.normal, true));
            menu.addMenuItem(new InputMenuItem("Torso (GTA-Id)", "Setze den Torso", torsoPlace, "", MenuItemStyle.normal, true));
            menu.addMenuItem(new MenuStatsMenuItem("Anzeigen (selekt.)", "Selektiere dieses Item zum Anzeigen", "SUPPORT_ON_SHOW_COMBO", MenuItemStyle.normal, true));
            menu.addMenuItem(new MenuStatsMenuItem("Hinzufügen", "Füge die Kombo hinzu", "SUPPORT_ADD_TOP_COMBINATION", MenuItemStyle.green).needsConfirmation("Wirklich hinzufügen?", "Combo wirklich hinzufügen?"));

            return menu;
        }

        private bool onSupportShowCombination(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var topEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var shirtEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var torsoEvt = evt.elements[2].FromJson<InputMenuItemEvent>();

            var topPlace = player.hasData("SUPPORT_COMBINATION_TOP") ? ((int)player.getData("SUPPORT_COMBINATION_TOP")) : 0;
            var shirtPlace = player.hasData("SUPPORT_COMBINATION_SHIRT") ? ((int)player.getData("SUPPORT_COMBINATION_SHIRT")) : 0;
            var torsoPlace = player.hasData("SUPPORT_COMBINATION_TORSO") ? ((int)player.getData("SUPPORT_COMBINATION_TORSO")) : 0;

            try {
                var top = topPlace;
                if(topEvt.input != null && topEvt.input != "") {
                    top = int.Parse(topEvt.input ?? "0");
                }

                var shirt = shirtPlace;
                if(shirtEvt.input != null && shirtEvt.input != "") {
                    shirt = int.Parse(shirtEvt.input ?? "0");
                }

                var torso = torsoPlace;
                if(torsoEvt.input != null && torsoEvt.input != "") {
                    torso = int.Parse(torsoEvt.input ?? "0");
                }

                var cloth = player.getClothing();

                var dbTop = ClothingController.getConfigClothing(top);
                var dbShirt = ClothingController.getConfigClothing(shirt);
                 
                cloth.UpdateClothSlot(11, dbTop.drawableid, 0, dbTop.dlc);
                cloth.UpdateClothSlot(8, dbShirt.drawableid, 0, dbShirt.dlc);
                cloth.UpdateClothSlot(3, torso, 0);

                ClothingController.setPlayerTempClothes(player, cloth);
            } catch(Exception) {
                player.sendBlockNotification("Etwas ist schiefgelaufen", "");
            }

            return true;
        }

        private bool onSupportAddTopCombinationMenu(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var menu = getCombinationMenu(player);
            player.showMenu(menu, false);

            return true;
        }

        private bool onSupportAddTopCombination(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = data["PreviousCefEvent"] as MenuStatsMenuItemEvent;
            var topEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var shirtEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var torsoEvt = evt.elements[2].FromJson<InputMenuItemEvent>();

            try {
                var topPlace = player.hasData("SUPPORT_COMBINATION_TOP") ? ((int)player.getData("SUPPORT_COMBINATION_TOP")) : 0;
                var shirtPlace = player.hasData("SUPPORT_COMBINATION_SHIRT") ? ((int)player.getData("SUPPORT_COMBINATION_SHIRT")) : 0;
                var torsoPlace = player.hasData("SUPPORT_COMBINATION_TORSO") ? ((int)player.getData("SUPPORT_COMBINATION_TORSO")) : 0;

                var top = topPlace;
                if(topEvt.input != null && topEvt.input != "") {
                    top = int.Parse(topEvt.input ?? "0");
                }

                var shirt = shirtPlace;
                if(shirtEvt.input != null && shirtEvt.input != "") {
                    shirt = int.Parse(shirtEvt.input ?? "0");
                }

                var torso = torsoPlace;
                if(torsoEvt.input != null && torsoEvt.input != "") {
                    torso = int.Parse(torsoEvt.input ?? "0");
                }

                player.setData("SUPPORT_COMBINATION_TOP", top);
                player.setData("SUPPORT_COMBINATION_SHIRT", shirt);
                player.setData("SUPPORT_COMBINATION_TORSO", torso);
                
                using(var db = new ChoiceVDb()) {
                    var gender = player.getCharacterData().Gender;
                    var dbTop = db.configclothings.FirstOrDefault(c => c.id == top);
                    if(shirt != -1) {
                        var dbShirt = db.configclothings.FirstOrDefault(c => c.id == shirt);
                        var already = db.configclothingshirtstotops.FirstOrDefault(c => c.topId == dbTop.id && c.shirtId == dbShirt.id);

                        if(already != null) {
                            player.sendNotification(NotifactionTypes.Warning, "Die Kombination gab es schon und der Torso wurde überschrieben", "");

                            already.otherTorso = torso;

                            db.SaveChanges();

                            var newMenu = getCombinationMenu(player);
                            player.showMenu(newMenu, false);
                            return true;
                        }

                        var newComb = new configclothingshirtstotop {
                            topId = dbTop.id,
                            shirtId = dbShirt.id,
                            otherTorso = torso
                        };

                        db.configclothingshirtstotops.Add(newComb);
                        db.SaveChanges();
                        player.sendNotification(NotifactionTypes.Success, "Die Kombination wurde erfolgreich hinzugefügt", "");
                    } else {
                        dbTop.torsoId = torso;
                        db.SaveChanges();
                        player.sendNotification(NotifactionTypes.Success, "Neuen Torso für Oberteil hinzugefügt", "");
                    }

                    if(player.hasData("TEMP_TORSO_COMBINATION")) {
                        var already = int.Parse((string)player.getData("TEMP_TORSO_COMBINATION"));
                        already++;
                        player.setPermanentData("TEMP_TORSO_COMBINATION", already.ToString());
                    } else {
                        player.setPermanentData("TEMP_TORSO_COMBINATION", "1");
                    }
                }
            } catch(Exception) {
                player.sendBlockNotification("Etwas ist schiefgelaufen!", "");
            }

            var menu = getCombinationMenu(player);
            player.showMenu(menu, false);
            return true;
        }

        private bool onStartTopCombinationVerification(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as InputMenuItemEvent;

            selectTopCombination(player, int.Parse(evt.input));

            return true;
        }

        private bool onSupportSelectNextTopCombination(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var index = (int)data["Index"];

            selectTopCombination(player, index);

            return true;
        }

        private static void selectTopCombination(IPlayer player, int index) {
            using(var db = new ChoiceVDb()) {
                var current = db.configclothingshirtstotops.Include(c => c.top).Include(c => c.shirt).ToList()[index];

                if(current.shirt.gender == "F") {
                    if(player.Model != ChoiceVAPI.Hash("mp_f_freemode_01")) {
                        player.Model = ChoiceVAPI.Hash("mp_f_freemode_01");
                    }
                } else {
                    if(player.Model != ChoiceVAPI.Hash("mp_m_freemode_01")) {
                        player.Model = ChoiceVAPI.Hash("mp_m_freemode_01");
                    }
                }

                player.sendNotification(NotifactionTypes.Info, $"Kombination: {index} wird angezeigt", $"Kombination: {index}");

                ChoiceVAPI.SetPlayerClothes(player, 3, current.otherTorso ?? -1, 0);
                ChoiceVAPI.SetPlayerClothes(player, 8, current.shirt.drawableid, 0);
                ChoiceVAPI.SetPlayerClothes(player, 11, current.top.drawableid, 0);

                if(current.shirt.gender == "M") {
                    ChoiceVAPI.SetPlayerClothes(player, 4, 44, 0);
                    ChoiceVAPI.SetPlayerClothes(player, 6, 33, 0);
                    ChoiceVAPI.SetPlayerClothes(player, 7, 0, 0);
                } else {
                    ChoiceVAPI.SetPlayerClothes(player, 4, 13, 0);
                    ChoiceVAPI.SetPlayerClothes(player, 6, 33, 0);
                    ChoiceVAPI.SetPlayerClothes(player, 7, 0, 0);
                }

                var menu = new Menu($"Aktuell: {index}", "Siehe die Daten", true);
                menu.addMenuItem(new ClickMenuItem("Nächste Kombination", "Gehe zur nächsten Kombination", "", "SUPPORT_SELECT_NEXT_TOP_COMBINATION", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Index", index + 1 } }));
                menu.addMenuItem(new StaticMenuItem("TopId", $"Die Datenbank TopId ist {current.topId}, das Oberteil heißt: {current.top.name} und hat Drawable {current.top.drawableid}", current.topId.ToString()));
                menu.addMenuItem(new StaticMenuItem("ShirtId", $"Die Datenbank ShirtId ist {current.shirtId}, das Unterteil heißt: {current.shirt.name} und hat Drawable {current.shirt.drawableid}", current.shirtId.ToString()));
                menu.addMenuItem(new StaticMenuItem("Torso", "", current.otherTorso.ToString()));
                menu.addMenuItem(new ClickMenuItem("Kombination löschen", "Lösche diese Kombination", "", "SUPPORT_DELETE_TOP_COMBINATION", MenuItemStyle.red).needsConfirmation("Kombination löschen?", "Kombination wirklich löschen?").withData(new Dictionary<string, dynamic> { { "Current", current }, { "Index", index } }));

                player.showMenu(menu, false, true);
            }

        }


        private bool onSupportDeleteTopCombination(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var index = (int)data["Index"];
            var current = (configclothingshirtstotop)data["Current"];

            using(var db = new ChoiceVDb()) {
                var dbCurrent = db.configclothingshirtstotops.FirstOrDefault(c => c.shirtId == current.shirtId && c.topId == current.topId);
                db.configclothingshirtstotops.Remove(dbCurrent);
                db.SaveChanges();
            }

            if(player.getCharacterData().Gender == 'M') {
                ChoiceVAPI.SetPlayerClothes(player, 3, 3, 0);
                ChoiceVAPI.SetPlayerClothes(player, 4, 44, 0);
                ChoiceVAPI.SetPlayerClothes(player, 6, 33, 0);
                ChoiceVAPI.SetPlayerClothes(player, 7, 0, 0);
                ChoiceVAPI.SetPlayerClothes(player, 8, 15, 0);
                ChoiceVAPI.SetPlayerClothes(player, 11, 15, 0);
            } else {
                ChoiceVAPI.SetPlayerClothes(player, 3, 8, 0);
                ChoiceVAPI.SetPlayerClothes(player, 4, 13, 0);
                ChoiceVAPI.SetPlayerClothes(player, 6, 12, 0);
                ChoiceVAPI.SetPlayerClothes(player, 7, 0, 0);
                ChoiceVAPI.SetPlayerClothes(player, 8, 6, 0);
                ChoiceVAPI.SetPlayerClothes(player, 11, 82, 0);
            }

            player.sendNotification(NotifactionTypes.Warning, $"Die Kombination mit TopId: {current.topId} ShirtId: {current.shirtId}, Torso: {current.otherTorso} wurde gelöscht!", "");
            selectTopCombination(player, index);

            return true;
        }

        private Menu allClothingMenuGenerator() {
            var menu = new Menu("Alle Kleidungsstücke", "Welche Kompente möchtest du anschauen?", resetPlayerClothing, true);

            foreach(var shop in System.Enum.GetValues<ClothingShopTypes>()) {
                var name = getNameOfSingleComponentClothingShop(shop);
                var componentId = getComponentOfClothingShop(shop);

                var virtualMenu = new VirtualMenu(name, () => {
                    var subMenu = new Menu(name, "Wähle ein Kleidungsstück aus");

                    using(var db = new ChoiceVDb()) {
                        var allGenderClothing = db.configclothings.Include(c => c.category).Include(c => c.configclothingvariations).Where(c => c.componentid == componentId).GroupBy(c => c.gender).Select(a => a.ToList()).ToList();

                        foreach(var genders in allGenderClothing) {
                            var genderName = "Männerkleidung";
                            var genderShort = "M";
                            if(genders.First().gender == "F") {
                                genderName = "Frauenkleidung";
                                genderShort = "F";
                            }

                            var virtGenderMenu = new VirtualMenu(genderName, () => {
                                var genderMenu = new Menu(genderName, "Wähle ein DLC");

                                var dlcList = genders.GroupBy(c => c.dlc).Select(a => a.ToList()).ToList();
                                foreach(var chachedClothings in dlcList) {
                                    var first = chachedClothings.First();
                                    var dlcName = "Standard-GTA";
                                    if(first.dlc != null) {
                                        dlcName = first.dlc;
                                    }

                                    chachedClothings.Sort((s1, s2) => s1.drawableid.CompareTo(s2.drawableid));

                                    var virtDlcMenu = new VirtualMenu(dlcName, () => {
                                        List<configclothing> clothings;
                                        using(var db = new ChoiceVDb()) {
                                            clothings = db.configclothings.Include(c => c.category).Include(c => c.configclothingvariations)
                                            .Where(c => c.componentid == componentId && c.gender == genderShort && c.dlc == first.dlc).ToList();
                                        }

                                        var dlcMenu = new Menu($"({genderShort}) {dlcName}", "Welches Kleidungsstück?");
                                        foreach(var cachedClothing in clothings) {
                                            var virtClothingMenu = new VirtualMenu($"{cachedClothing.id}: {cachedClothing.name}", () => {
                                                configclothing clothing;

                                                using(var db = new ChoiceVDb()) {
                                                    clothing = db.configclothings.Include(c => c.category).Include(c => c.configclothingvariations).FirstOrDefault(c => c.id == cachedClothing.id);
                                                }

                                                var clothMenu = new Menu($"{clothing.id}: {clothing.name}", "Siehe die Informationen");

                                                var virtVariationsMenu = new VirtualMenu("Variationen", () => {
                                                    var variationsMenu = new Menu("Variationen", "Variationen");
                                                    foreach(var variation in clothing.configclothingvariations) {
                                                        var variationMenu = new Menu($"{variation.variation} {variation.name}", "Siehe die Informationen");
                                                        variationMenu.addMenuItem(new StaticMenuItem("Per Hand editiert?", "", variation.handNamed == 1 ? "Ja" : "Nein"));
                                                        variationMenu.addMenuItem(new StaticMenuItem("GTA GTX", "", variation.gtaGTX));
                                                        variationMenu.addMenuItem(new StaticMenuItem("Überschreibender Preis", "", variation.overridePrice.ToString()));

                                                        variationsMenu.addMenuItem(new MenuMenuItem(variationMenu.Name, variationMenu, MenuItemStyle.normal, "SUPPORT_SELECT_CLOTHING").withData(new Dictionary<string, dynamic> { { "Clothing", clothing }, { "Variation", variation } }));
                                                    }

                                                    return variationsMenu;
                                                });

                                                clothMenu.addMenuItem(new MenuMenuItem(virtVariationsMenu.Name, virtVariationsMenu, MenuItemStyle.normal, null, true));

                                                clothMenu.addMenuItem(new StaticMenuItem("DrawableId", "", clothing.drawableid.ToString()));
                                                clothMenu.addMenuItem(new StaticMenuItem("Katergorie", "", clothing.category.Name));
                                                clothMenu.addMenuItem(new StaticMenuItem("Kleidungstyp", "0: Billig, 1: Mittel, 2: Teuer", clothing.clothingType.ToString()));
                                                clothMenu.addMenuItem(new StaticMenuItem("Preis", "", clothing.price.ToString()));
                                                clothMenu.addMenuItem(new StaticMenuItem("Nicht kaufbar", "", clothing.notBuyable == 0 ? "Kaufbar" : "Nicht kaufbar"));

                                                return clothMenu;
                                            });

                                            dlcMenu.addMenuItem(new MenuMenuItem(virtClothingMenu.Name, virtClothingMenu, MenuItemStyle.normal, "SUPPORT_SELECT_CLOTHING", true).withData(new Dictionary<string, dynamic> { { "Clothing", cachedClothing }, { "Variation", cachedClothing.configclothingvariations.First() } }));
                                        }

                                        return dlcMenu;
                                    });

                                    genderMenu.addMenuItem(new MenuMenuItem(virtDlcMenu.Name, virtDlcMenu, MenuItemStyle.normal, null, true));
                                }

                                return genderMenu;
                            });

                            subMenu.addMenuItem(new MenuMenuItem(virtGenderMenu.Name, virtGenderMenu));
                        }
                    }

                    return subMenu;
                });

                menu.addMenuItem(new MenuMenuItem(virtualMenu.Name, virtualMenu));
            }

            return menu;
        }

        private bool onSupportSelectClothing(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var clothing = (configclothing)data["Clothing"];
            var variation = (configclothingvariation)data["Variation"];

            if(player.getCharacterData().Gender == 'M') {
                ChoiceVAPI.SetPlayerClothes(player, 3, 3, 0);
                ChoiceVAPI.SetPlayerClothes(player, 4, 44, 0);
                ChoiceVAPI.SetPlayerClothes(player, 6, 33, 0);
                ChoiceVAPI.SetPlayerClothes(player, 7, 0, 0);
                ChoiceVAPI.SetPlayerClothes(player, 8, 15, 0);
                ChoiceVAPI.SetPlayerClothes(player, 11, 15, 0);
            } else {
                ChoiceVAPI.SetPlayerClothes(player, 3, 8, 0);
                ChoiceVAPI.SetPlayerClothes(player, 4, 13, 0);
                ChoiceVAPI.SetPlayerClothes(player, 6, 12, 0);
                ChoiceVAPI.SetPlayerClothes(player, 7, 0, 0);
                ChoiceVAPI.SetPlayerClothes(player, 8, 6, 0);
                ChoiceVAPI.SetPlayerClothes(player, 11, 82, 0);
            }

            ChoiceVAPI.SetPlayerClothes(player, clothing.componentid, clothing.drawableid, variation.variation, clothing.dlc);

            return true;
        }

        private bool onStartNamelessClothesVerification(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as InputMenuItemEvent;
            var input = evt.input;
            var split = input.Split(",");
            var componentId = int.Parse(split[0]);
            var idx = int.Parse(split[1]);

            using(var db = new ChoiceVDb()) {
                var variations = db.configclothingvariations
                    .Include(c => c.clothing)
                    .Where(v => v.clothing.componentid == componentId && v.name == "NULL");

                selectNamelessCloth(player, idx, variations.ToList());
            }

            return true;
        }

        private static void selectNamelessCloth(IPlayer player, int index, List<configclothingvariation> variations) {
            var variation = variations[index];

            if(variation.clothing.gender == "F") {
                if(player.Model != ChoiceVAPI.Hash("mp_f_freemode_01")) {
                    player.Model = ChoiceVAPI.Hash("mp_f_freemode_01");
                }
            } else {
                if(player.Model != ChoiceVAPI.Hash("mp_m_freemode_01")) {
                    player.Model = ChoiceVAPI.Hash("mp_m_freemode_01");
                }
            }

            if(player.getCharacterData().Gender == 'M') {
                ChoiceVAPI.SetPlayerClothes(player, 3, 3, 0);
                ChoiceVAPI.SetPlayerClothes(player, 4, 44, 0);
                ChoiceVAPI.SetPlayerClothes(player, 6, 33, 0);
                ChoiceVAPI.SetPlayerClothes(player, 7, 0, 0);
                ChoiceVAPI.SetPlayerClothes(player, 8, 15, 0);
                ChoiceVAPI.SetPlayerClothes(player, 11, 15, 0);
            } else {
                ChoiceVAPI.SetPlayerClothes(player, 3, 8, 0);
                ChoiceVAPI.SetPlayerClothes(player, 4, 13, 0);
                ChoiceVAPI.SetPlayerClothes(player, 6, 33, 0);
                ChoiceVAPI.SetPlayerClothes(player, 7, 0, 0);
                ChoiceVAPI.SetPlayerClothes(player, 8, 6, 0);
                ChoiceVAPI.SetPlayerClothes(player, 11, 15, 0);
            }

            ChoiceVAPI.SetPlayerClothes(player, variation.clothing.componentid, variation.clothing.drawableid, variation.variation, variation.clothing.dlc);

            var menu = new Menu($"{variation.clothing.drawableid}: {variation.variation}", "DrawableId: VariationId", true);
            menu.addMenuItem(new ClickMenuItem("Nächstes Kleidungsstück", "Gehe zum nächsten Kleidungsstück", $"{index}/{variations.Count}", "SUPPORT_SELECT_NEXT_NAMELESS_CLOTHING", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Index", index + 1 }, { "List", variations } }));
            menu.addMenuItem(new StaticMenuItem("Kleidungsstück", $"Der Name des Kleidungsstückes ist: {variation.clothing.name}", variation.clothing.name));
            menu.addMenuItem(new InputMenuItem("Namen setzen", "Setze einen Namen für das Kleidungsstück. Dies aktiviert automatisch auch das Kleidungsstück", "", "SUPPORT_SET_NAMELESS_CLOTHING_NAME").withData(new Dictionary<string, dynamic> { { "Variation", variation }, { "Index", index + 1 }, { "List", variations } }));

            player.showMenu(menu, true, true);
        }

        private bool onSupportSelectNextNamelessClothing(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var idx = (int)data["Index"];
            var list = (List<configclothingvariation>)data["List"];

            selectNamelessCloth(player, idx, list);

            return true;
        }



        private bool onSupportSetNamelessClothingName(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var variation = (configclothingvariation)data["Variation"];
            var idx = (int)data["Index"];
            var list = (List<configclothingvariation>)data["List"];

            var evt = menuItemCefEvent as InputMenuItemEvent;

            selectNamelessCloth(player, idx, list);

            using(var db = new ChoiceVDb()) {
                var dbVariation = db.configclothingvariations.FirstOrDefault(v => v.clothingId == variation.clothingId && v.variation == variation.variation);

                if(dbVariation != null) {
                    dbVariation.name = evt.input;

                    db.SaveChanges();
                }
            }

            player.sendNotification(NotifactionTypes.Info, $"Der Name des Kleidungsstückes wurde zu {evt.input} geändert. Das Kleidungsstück hatte ClothingId {variation.clothingId} und Variation {variation.variation}", $"{variation.clothingId}:{variation.variation}");

            return true;
        }

        #endregion

        #region NPCModule

        private List<MenuItem> clothingShopGenerator(ref System.Type codeType) {
            codeType = typeof(NPCClothingShopModule);

            var clothingTypeIds = String.Join(", ", Enum.GetValues<ClothingShopTypes>().Select(c => $"{c}: {(int)c}").ToList());
            return new List<MenuItem> { 
                new ListMenuItem("Preiskategorie", "Wähle welche Preiskategorie der NPC verkauft", System.Enum.GetValues<ClothingType>().Select(c => c.ToString()).ToArray(), ""),
                
                new InputMenuItem("Kleidungstyp", $"Welche Kleidung verkauft der NPC. Die Optionen bitte als Kommagetrennte Ids eingeben. Optionen sind: {clothingTypeIds}", "z.B: 1,4,5", "", MenuItemStyle.normal, true),
            };
        }

        private void clothingGeneratorCallback(IPlayer player, MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
            var categoryEvt = evt.elements[0].FromJson<ListMenuItemEvent>();
            var repertoireEvt = evt.elements[1].FromJson<InputMenuItemEvent>();

            var list = repertoireEvt.input.Split(",").Select(c => (ClothingShopTypes)int.Parse(c)).ToList();

            var type = System.Enum.Parse<ClothingType>(categoryEvt.currentElement);

            creationFinishedCallback.Invoke(new Dictionary<string, dynamic> { { "ClothingType", type }, { "Repertoire", list.ToJson() } });
        }

        #endregion
    }
}