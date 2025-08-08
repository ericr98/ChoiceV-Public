using AltV.Net.Elements.Entities;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.PlaceableObjects;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem {
    public class BurnerUtensil : FoldTableUtensil {
        public BurnerUtensil(item item) : base(item) { }

        public BurnerUtensil(configitem configItem, int amount, int quality) : base(configItem, quality) { }

        public override bool canBePulledOff(Controller.PlaceableObjects.FoldingTable table) {
            return true;
        }

        public override List<string> getContributedFunctionalities() {
            return new List<string> { "BURNER" };
        }

        public override List<Item> getContributedItems() {
            return new List<Item>();
        }

        public override Menu getOnTableMenu(IPlayer player, Controller.PlaceableObjects.FoldingTable table) {
            var menu = new Menu("Bunsenbrenner", "Was möchtest du tun?");

            return menu;
        }

        public override void onTick(Controller.PlaceableObjects.FoldingTable table, TimeSpan tickLenght) { }

        public override FoldTableVisualCategory getVisualCategory() {
            return FoldTableVisualCategory.Lab;
        }
    }
}
