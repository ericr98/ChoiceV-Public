using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Shopsystem.Model;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.Shopsystem {
    public class ShopVendingMachineController : ChoiceVScript {
        public ShopVendingMachineController() {
            InteractionController.addObjectInteractionCallback(
                "OPEN_SNACK_SHOP",
                "Snack Shop öffnen",
                onInteractWithSnackShop
            );

            InteractionController.addObjectInteractionCallback(
                "OPEN_DRINK_SHOP",
                "Getränke Shop öffnen",
                onDrinkAutomatInteraction
            );

            InteractionController.addObjectInteractionCallback(
                "OPEN_WATER_SHOP",
                "Wasser Shop öffnen",
                onWaterAutomatInteraction
            );

            InteractionController.addObjectInteractionCallback(
                "OPEN_COFFEE_SHOP",
                "Kaffee Shop öffnen",
                onCoffeeAutomatInteraction
            );

            InteractionController.addObjectInteractionCallback(
                "OPEN_CIGARETTE_SHOP",
                "Zigaretten Shop öffnen",
                onCigaretteShopOpen
            );

        }

        private void onInteractWithSnackShop(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading, bool isBroken, ref Menu menu) {
            var shopMenu = ShopController.getShopMenuByType(player, ShopTypes.SnackVendingMachine);
            menu.addMenuItem(new MenuMenuItem(shopMenu.Name, shopMenu));
        }

        private void onDrinkAutomatInteraction(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading, bool isBroken, ref Menu menu) {
            var shopMenu = ShopController.getShopMenuByType(player, ShopTypes.DrinkVendingMachine);
            menu.addMenuItem(new MenuMenuItem(shopMenu.Name, shopMenu));
        }

        private void onWaterAutomatInteraction(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading, bool isBroken, ref Menu menu) {
            var shopMenu = ShopController.getShopMenuByType(player, ShopTypes.WaterVendingMachine);
            menu.addMenuItem(new MenuMenuItem(shopMenu.Name, shopMenu));
        }

        private void onCoffeeAutomatInteraction(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading, bool isBroken, ref Menu menu) {
            var shopMenu = ShopController.getShopMenuByType(player, ShopTypes.CoffeeVendingMachine);
            menu.addMenuItem(new MenuMenuItem(shopMenu.Name, shopMenu));
        }

        private void onCigaretteShopOpen(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading, bool isBroken, ref Menu menu) {
            var shopMenu = ShopController.getShopMenuByType(player, ShopTypes.CigaretteVendingMachine);
            menu.addMenuItem(new MenuMenuItem(shopMenu.Name, shopMenu));
        }
    }
}
