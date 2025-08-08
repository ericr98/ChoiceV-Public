using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Money;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller.Shopsystem.Model {
    public class ShoppingCartItem {
        public ShopItem Item;
        public int Amount;

        public ShoppingCartItem(ShopItem item, int amount) {
            Item = item;
            Amount = amount;
        }

        public float getRealWeight() {
            var weight = Item.ConfigItem.weight;
            if(Item.ConfigItem.configitemsitemcontainerinfoconfigItems.Count > 0) {
                weight += Item.ConfigItem.configitemsitemcontainerinfoconfigItems.Sum(i => i.subConfigItem.weight * i.subItemAmount);
            }

            return weight * Amount;
        }
    }

    public class ShoppingCart {
        public ShopModel Shop;
        public List<ShoppingCartItem> Items;

        public ShoppingCart(ShopModel shop) {
            Shop = shop;

            Items = new List<ShoppingCartItem>();
        }

        public void addItem(ShopItem item, int amount) {
            var already = Items.FirstOrDefault(i => i.Item.ConfigItem.configItemId == item.ConfigItem.configItemId);
            if(already != null) {
                already.Amount += amount;
            } else {
                Items.Add(new ShoppingCartItem(item, amount));
            }
        }

        public Menu getMenu(IPlayer player, string shopName) {
            var menu = new Menu("Warenkorb", "Was möchtest du tun?");

            var combinedWeight = 0f;
            Items.ForEach(i => combinedWeight += i.getRealWeight());

            var price = 0m;
            Items.ForEach(i => price += i.Item.Price * i.Amount);

            var invLeft = player.getInventory().MaxWeight - player.getInventory().CurrentWeight;
            if(combinedWeight <= invLeft) {
                var paymentMenu = new Menu("Zahlungsmethoden", "Wie möchtest du zahlen?");
                var menuItems = MoneyController.getPaymentMethodsMenu(player, price, "Warenkorb", ShopController.getShopBankAccountId(), (p, successfull) => {
                    if(successfull) {
                        ShopController.onPlayerBuyShoppingCart(p, this);
                    }
                });

                foreach(var item in menuItems) {
                    paymentMenu.addMenuItem(item);
                }
                menu.addMenuItem(new MenuMenuItem("Warenkorb kaufen", paymentMenu));
            } else {
                menu.addMenuItem(new StaticMenuItem("Warenkorb zu schwer!", $"Der Warenkorb ist {Math.Round(combinedWeight - invLeft, 2)}kg zu schwer. Mache Platz in deinem Inventar!", $"Es fehlen {Math.Round(combinedWeight - invLeft, 2)}kg", MenuItemStyle.red));
            }

            menu.addMenuItem(new ClickMenuItem("Warenkorb leeren", "Leere deinen Warenkorb. Diese Aktion löscht alle Items", "", "SHOP_EMPTY_SHOPPING_CART", MenuItemStyle.yellow).needsConfirmation("Warenkorb leeren?", "Warenkorb wirklich leeren?").withData(new Dictionary<string, dynamic> { { "Cart", this } }));

            var listMenu = new Menu("Produkte", "Was möchstest du tun?");
            foreach(var item in Items) {
                listMenu.addMenuItem(new ClickMenuItem(item.Item.getName(), "Klicke um das Item aus dem Warenkorb zu entfernen.", $"{item.Amount}x ≙ ${item.Item.Price * item.Amount}", "SHOP_REMOVE_ITEM_FROM_CART").needsConfirmation($"{item.Item.getName()} entfernen?", "Produkt wirklich entfernen?").withData(new Dictionary<string, dynamic> { { "Cart", this }, { "Item", item } }));
            }

            menu.addMenuItem(new MenuMenuItem(listMenu.Name, listMenu));

            return menu;
        }
    }
}
