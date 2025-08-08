using AltV.Net.Elements.Entities;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.PlaceableObjects;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem {

    public class ScaleUtensil : FoldTableUtensil {
        public ScaleUtensil(item item) : base(item) { }

        public ScaleUtensil(configitem configItem, int amount, int quality) : base(configItem, quality) { }

        public override bool canBePulledOff(Controller.PlaceableObjects.FoldingTable table) {
            return true;
        }

        public override List<string> getContributedFunctionalities() {
            return new List<string> { "SCALE" };
        }

        public override List<Item> getContributedItems() {
            return new List<Item>();
        }

        public override Menu getOnTableMenu(IPlayer player, Controller.PlaceableObjects.FoldingTable table) {
            var menu = new Menu("Laborwaage", "Was möchtest du tun?");

            return menu;
        }

        public override void onTick(Controller.PlaceableObjects.FoldingTable table, TimeSpan tickLenght) { }

        public override FoldTableVisualCategory getVisualCategory() {
            return FoldTableVisualCategory.None;
        }
    }
}
