using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.Shopsystem.Model {
    public class ShopShelf {
        public int Id;
        public string Name;
        public string ModelName;
        public List<CollisionShape> ColShapes;
        public List<configitem> AvailableItems;

        public ShopShelf(int id, string name, string modelName, List<configitem> items, List<CollisionShape> colShapes) {
            Id = id;
            Name = name;
            ModelName = modelName;
            AvailableItems = items;
            ColShapes = colShapes;

            foreach(var colShape in ColShapes) {
                colShape.OnCollisionShapeInteraction += onInteract;
            }
        }
        public bool onInteract(IPlayer player) {
            if(player.hasData("CURRENT_SHOP")) {
                openShelf(player, (ShopModel)player.getData("CURRENT_SHOP"));
                
                return true;
            }

            return false;
        }

        public void openShelf(IPlayer player, ShopModel shop) {
            if(player.getCharacterData().AdminMode) { shop.makeShelfCheck(player, this); }
            
            var menu = new Menu(Name, "Was möchtest du tun?");

            var productMenu = new Menu("Produkte kaufen", "Lege die Produkte in deinen Warenkorb");
            var stealMenu = new Menu("Produkte stehlen", "Welches Produkt möchtest du stehlen?");
            
            foreach(var shopItem in shop.AllItemsInShop.Join(AvailableItems, e1 => e1.ConfigItem.configItemId, e2 => e2.configItemId, (e1, e2) => e1)) {
                var data = new Dictionary<string, dynamic> {
                    { "Shop", shop },
                    { "ShopItem", shopItem },
                    { "Shelf", this },
                };

                productMenu.addMenuItem(new ClickMenuItem(shopItem.getName(), $"Lege {shopItem.ConfigItem.name} in den Warenkorb. Der Laden hat genug auf Lager", $"{shopItem.Price}", "SHOP_SHOPPING_CART_ITEM_SELECT").withData(data));
                stealMenu.addMenuItem(new ClickMenuItem(shopItem.getName(), $"Stehle ein/eine {shopItem.getName()}. Dies ist eine illegale Aktion, und kann rechtliche Konsequenzen haben!", "", "SHOP_STEAL_ITEM").needsConfirmation($"{shopItem.getName()} stehlen?", "Wirklich stehlen?").withData(data));
            }

            menu.addMenuItem(new MenuMenuItem(productMenu.Name, productMenu));
            
            if(player.hasCrimeFlag()) {
                menu.addMenuItem(new MenuMenuItem(stealMenu.Name, stealMenu, MenuItemStyle.yellow));
            }

            player.showMenu(menu);
        }

        public void onDelete() {
            ColShapes.ForEach(c => c.Dispose());
            ColShapes = null;
        }
    }
}
