using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.FsDatabase;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller {
    public abstract class TerminalClothingShop : TerminalShop {
        public const int CLOTHES_TOKENS_PRICE = 2;
        public const int CLOTHES_TOKENS_AMOUNT = 5;

        public ClothingType ClothingType;

        public TerminalClothingShop(ClothingType type, string name, string identifier) : base(identifier, name) {
            ClothingType = type;
        }

        public override Menu getBuyableOptions(IPlayer player) {
            var menu = new Menu($"Kleidungsladen: {ClothingShopController.getPriceGroupName(ClothingType)}", "Kaufe Kleidung", ClothingShopController.resetPlayerClothing);
            menu.addMenuItem(new StaticMenuItem("Information", "Es befindet sich eine Garderobe im hinteren Teil des Ladens. (Bei den Kleiderbügeln)", "siehe Beschreibung"));

            var tokens = TerminalShopController.getPlayerTokens(player);

            menu.addMenuItem(new StaticMenuItem("Noch freie Kleidungsstücke", $"Du kannst dir noch {getCurrentClothesTokens(player)} Kleidungsstücke aussuchen!", $"{getCurrentClothesTokens(player)}"));

            if(tokens >= CLOTHES_TOKENS_PRICE) {
                menu.addMenuItem(new ClickMenuItem($"{CLOTHES_TOKENS_AMOUNT} Kleidungstücke kaufen", $"Kaufe Tokens für {CLOTHES_TOKENS_AMOUNT} Kleidungsstücke deiner Wahl", $"{CLOTHES_TOKENS_PRICE} Marken", "TERMINAL_SHOP_BUY_CLOTHES_TOKEN", MenuItemStyle.green).needsConfirmation("Wirklich kaufen?", $"Für {CLOTHES_TOKENS_PRICE} Marken kaufen?"));
            } else {
                menu.addMenuItem(new StaticMenuItem($"{CLOTHES_TOKENS_AMOUNT} Kleidungstücke kaufen", $"Kaufe Tokens für {CLOTHES_TOKENS_AMOUNT} Kleidungsstücke deiner Wahl. Aktuell zu teuer!", $"zu teuer ({CLOTHES_TOKENS_PRICE}M)", MenuItemStyle.yellow));
            }

            if(player.hasData("TERMINAL_CLOTHES_TOKENS")) {
                var clothesBuyMenu = new Menu("Kleidung kaufen", "Kaufe Kleidung");

                foreach(var type in Enum.GetValues<ClothingShopTypes>()) {
                    if(type == ClothingShopTypes.None) { continue; }
                    var virtMenu = new VirtualMenu(ClothingShopController.getNameOfSingleComponentClothingShop(type), () => {
                        return ClothingShopController.getSingleComponentClothShopMenu(player, type, ClothingType, false, (price, productName, afterBuyAction) => {
                            //Check if player has a clothes token left
                            if(player.hasData("TERMINAL_CLOTHES_TOKENS")) {
                                var amount = int.Parse((string)player.getData("TERMINAL_CLOTHES_TOKENS"));
                                if(amount > 0) {
                                    player.setPermanentData("TERMINAL_CLOTHES_TOKENS", (amount - 1).ToString());
                                    afterBuyAction.Invoke();
                                }
                            }

                        });
                    });
                    clothesBuyMenu.addMenuItem(new MenuMenuItem(virtMenu.Name, virtMenu));
                }
                menu.addMenuItem(new MenuMenuItem(clothesBuyMenu.Name, clothesBuyMenu));
            } else {
                menu.addMenuItem(new StaticMenuItem("Keine freien Kleidungsstücke übrig!", "Kaufe Tokens um hier Kleidungsstücke zu erwerben", "", MenuItemStyle.yellow));
            }

            return menu;
        }

        public override Menu getAlreadyBoughtListing(IPlayer player) {
            var menu = new Menu("Bereits Gekauftes zurückgeben", "Welches Kleidungsstück zurückgeben?");

            if(getCurrentClothesTokens(player) >= CLOTHES_TOKENS_AMOUNT) {
                menu.addMenuItem(new ClickMenuItem("5 Kleidungstoken zurückgeben", "Erhalte eine Marke indem zu Kleidsstücke zurückgibst", "", "TERMINAL_RETURN_CLOTHES_TOKENS", MenuItemStyle.normal).needsConfirmation("Kleidsstücke zurückgeben?", "Wirklich zurückgeben"));
            } else {
                menu.addMenuItem(new StaticMenuItem($"Nicht genug Kleidungsstücke zurückgabe", "Gib genug Kleidungsstücke zurück um einen Token eintauschen zu können", "", MenuItemStyle.yellow));
            }

            foreach(var cloth in player.getInventory().getItems<ClothingItem>(i => true)) {
                var data = new Dictionary<string, dynamic> {
                    { "Cloth", cloth }
                };

                menu.addMenuItem(new ClickMenuItem($"{cloth.Description}", $"Gib {cloth.Description} zurück und erhalte 1 Kleidungsstück zurück", "", "TERMINAL_RETURN_CLOTHING", MenuItemStyle.red).withData(data).needsConfirmation($"{cloth.Description} zurückgeben?", "Wirklich zurückgeben"));
            }

            return menu;
        }

        public static int getCurrentClothesTokens(IPlayer player) {
            if(player.hasData("TERMINAL_CLOTHES_TOKENS")) {
                return int.Parse(player.getData("TERMINAL_CLOTHES_TOKENS"));
            } else {
                return 0;
            }
        }

        public static bool changeClothesTokens(IPlayer player, int change) {
            if(player.hasData("TERMINAL_CLOTHES_TOKENS")) {
                var current = int.Parse((string)player.getData("TERMINAL_CLOTHES_TOKENS"));

                current += change;

                if(current >= 0) {
                    if(current > 0) {
                        player.setPermanentData("TERMINAL_CLOTHES_TOKENS", current.ToString());
                    } else {
                        player.resetPermantData("TERMINAL_CLOTHES_TOKENS");
                    }
                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }

        public override bool hasPlayerBoughtSomething(IPlayer player) {
            return true;
        }

        public override void onPlayerLand(IPlayer player, ref executive_person_file file) {
            if(player.hasData("TERMINAL_CLOTHES_TOKENS")) {
                player.resetPermantData("TERMINAL_CLOTHES_TOKENS");
            }
        }
    }

    public class CheapClothingShop : TerminalClothingShop {
        public CheapClothingShop() : base(ClothingType.Cheap, "Billiger Kleidungsladen", "CHEAP_CLOTHES_STORE") { }
    }

    public class NormalClothingShop : TerminalClothingShop {
        public NormalClothingShop() : base(ClothingType.Medium, "Mittelteurer Kleidungsladen", "NORMAL_CLOTHES_STORE") { }
    }

    public class ExpensiveClothingShop : TerminalClothingShop {
        public ExpensiveClothingShop() : base(ClothingType.Expensive, "Teurer Kleidungsladen", "EXPENSIVE_CLOTHES_STORE") { }
    }

    public class TerminalClothesShopController : ChoiceVScript {
        public TerminalClothesShopController() {
            EventController.addMenuEvent("TERMINAL_SHOP_BUY_CLOTHES_TOKEN", onTerminalShopBuyClothesToken);

            TerminalShopController.addTerminalShop(new CheapClothingShop());
            TerminalShopController.addTerminalShop(new NormalClothingShop());
            TerminalShopController.addTerminalShop(new ExpensiveClothingShop());

            EventController.addMenuEvent("TERMINAL_RETURN_CLOTHES_TOKENS", onTerminalReturnClothesToken);
            EventController.addMenuEvent("TERMINAL_RETURN_CLOTHING", onTerminalReturnClothing);
        }

        private bool onTerminalShopBuyClothesToken(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(TerminalShopController.addOrRemovePlayerTokens(player, -TerminalClothingShop.CLOTHES_TOKENS_PRICE)) {
                var already = 0;
                if(player.hasData((string)"TERMINAL_CLOTHES_TOKENS")) {
                    already = int.Parse(player.getData("TERMINAL_CLOTHES_TOKENS"));
                }
                already += TerminalClothingShop.CLOTHES_TOKENS_AMOUNT;
                player.setPermanentData("TERMINAL_CLOTHES_TOKENS", already.ToString());

                player.sendNotification(Constants.NotifactionTypes.Info, $"Du hast erfolgreich {TerminalClothingShop.CLOTHES_TOKENS_AMOUNT} Kleidungstoken erworben!", "Kleidungstoken erworben", Constants.NotifactionImages.Plane);
            }

            return true;
        }

        private bool onTerminalReturnClothesToken(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(TerminalClothingShop.changeClothesTokens(player, -TerminalClothingShop.CLOTHES_TOKENS_AMOUNT)) {
                TerminalShopController.addOrRemovePlayerTokens(player, TerminalClothingShop.CLOTHES_TOKENS_PRICE);
                player.sendNotification(Constants.NotifactionTypes.Warning, $"Du hast erfolgreich {TerminalClothingShop.CLOTHES_TOKENS_AMOUNT} Kleidungstoken zurückgegen und {TerminalClothingShop.CLOTHES_TOKENS_PRICE} Marken erhalten!", "Kleidungstoken zurückgegeben", Constants.NotifactionImages.Plane);
            }

            return true;
        }

        private bool onTerminalReturnClothing(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var ownedClothing = (ClothingItem)data["Cloth"];

            if(ownedClothing != null) {
                TerminalClothingShop.changeClothesTokens(player, 1);

                if(ownedClothing.IsEquipped) {
                    ownedClothing.fastUnequip(player);
                }

                player.getInventory().removeItem(ownedClothing);

                player.sendNotification(Constants.NotifactionTypes.Warning, $"Du hast erfolgreich {ownedClothing.Description} zurückgeben, und hast nun wieder {TerminalClothingShop.getCurrentClothesTokens(player)} Kleidungsstücke frei", "Kleidungsstück zurückgegeben", Constants.NotifactionImages.Plane);
            }

            return true;
        }
    }

}
