using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Controller.Money;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller {
    public class NPCClothingShopModule : NPCModule {
        public ClothingType PriceGroup;
        public List<ClothingShopTypes> Repertoire;

        public NPCClothingShopModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped) {
            PriceGroup = (ClothingType)settings["ClothingType"];
            if(settings.ContainsKey("Repertoire")) { 
                Repertoire = ((string)settings["Repertoire"]).FromJson<List<ClothingShopTypes>>();
            } else {
                Repertoire = Enum.GetValues<ClothingShopTypes>().ToList();
            }
        }

        public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
            return new StaticMenuItem("Kleidungsladenshop", $"Ein Kleidungsladenmodul für die Preisklasse: {PriceGroup}", $"{PriceGroup}");
        }

        public override List<MenuItem> getMenuItems(IPlayer player) {
            var clothingMenu = new Menu($"Kleidungsladen {ClothingShopController.getPriceGroupName(PriceGroup)}", "Was möchtest du kaufen?", true);

            //clothingMenu.addMenuItem(new ListMenuItem("Freie Umsicht", "Aktiviere die freie Umsicht", new string[] { "Aus", "An" }, "ACTIVATE_FREE_CAMERA", MenuItemStyle.normal, true, true));

            foreach(var type in Repertoire) {
                var virtMenu = new VirtualMenu(ClothingShopController.getNameOfSingleComponentClothingShop(type), () => {
                    return ClothingShopController.getSingleComponentClothShopMenu(player, type, PriceGroup, true, (price, productName, afterBuyAction) => {
                        var menuItems = MoneyController.getPaymentMethodsMenu(player, price, productName, ClothingShopController.ShopBankaccount.id, (p, worked) => {
                            if(worked) {
                                afterBuyAction.Invoke();
                            }
                        });

                        var menu = new Menu("Zahlungsmethode wählen", "Wähle deine Zahlungsmethode", ClothingShopController.resetPlayerClothing);
                        foreach(var menuItem in menuItems) {
                            menu.addMenuItem(menuItem);
                        }
                        player.showMenu(menu);
                    });
                });

                clothingMenu.addMenuItem(new MenuMenuItem(virtMenu.Name, virtMenu));
            }

            return new List<MenuItem> { new MenuMenuItem(clothingMenu.Name, clothingMenu) };
        }

        public override void onRemove() { }
    }
}
