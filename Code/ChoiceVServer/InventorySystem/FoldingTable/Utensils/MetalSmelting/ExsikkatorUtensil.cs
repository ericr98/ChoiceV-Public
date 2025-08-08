using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.PlaceableObjects;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public class ExsikkatorUtensil : FoldTableUtensil {
        public Inventory Inventory { get => InventoryController.loadInventory((int)Data["InventoryId"]); set => Data["InventoryId"] = value.Id; }

        public ExsikkatorUtensil(item item) : base(item) { }

        public ExsikkatorUtensil(configitem configItem, int amount, int quality) : base(configItem, quality) {
            Inventory = InventoryController.createInventory(Id ?? -1, 1000, InventoryTypes.ExsikkatorUtensil);
        }

        public override void destroy(bool toDb = true) {
            InventoryController.destroyInventory(Inventory);
            base.destroy(toDb);
        }

        public override bool canBePulledOff(Controller.PlaceableObjects.FoldingTable table) {
            return Inventory.getCount() <= 0;
        }

        public override List<string> getContributedFunctionalities() {
            return new List<string>();
        }

        public override List<Item> getContributedItems() {
            return Inventory.getAllItems();
        }

        public override Menu getOnTableMenu(IPlayer player, Controller.PlaceableObjects.FoldingTable table) {
            var menu = new Menu("Exsikkator", "Was möchtest du tun?");

            var contentMenu = new Menu("Inhalt", "Wähle aus zum Herausnehmen");
            foreach(var item in Inventory.getItems<FoldingTableChemical>(i => i.Component != ChemicalType.DestilledWater)) {
                var data = new Dictionary<string, dynamic> {
                    { "Inventory", Inventory },
                    { "Item", item }
                };

                contentMenu.addMenuItem(new ClickMenuItem($"{ChemicalContainer.chemicalToName(item.Component)}", $"Es befinden sich {Math.Round(item.Amount, 2)}{ChemicalContainer.chemicalToUnit(item.Component)} {ChemicalContainer.chemicalToName(item.Component)} im Exsikkator. Es kann herausgenommen werden", $"{Math.Round(item.Amount, 2)} {ChemicalContainer.chemicalToUnit(item.Component)}", "PULL_STUFF_FROM_FOLD_TABLE_UTENSIL").withData(data));
            }
            menu.addMenuItem(new MenuMenuItem(contentMenu.Name, contentMenu));

            var currentComponent = Inventory.getItem<FoldingTableChemical>(i => !getAllResultChems().Contains(i.Component))?.Component ?? ChemicalType.None;
            var list = player.getInventory().getItems<ChemicalContainer>(i => i.Amount > 0 && (currentComponent == ChemicalType.None ? i.Component != ChemicalType.None : i.Component == currentComponent))
                        .Concat(table.getContributedItems(this).Where(i => (i is FoldingTableChemical chem && (currentComponent == ChemicalType.None ? chem.Component != ChemicalType.None : chem.Component == currentComponent))
                                                                    || (i is ChemicalContainer cont && cont.Amount > 0 && (currentComponent == ChemicalType.None ? cont.Component != ChemicalType.None : cont.Component == currentComponent))));

            var addMenu = new Menu("In Exsikkator legen", "Was möchtest du reinlegen?");
            foreach(var item in list) {
                var data = new Dictionary<string, dynamic> {
                    { "Inventory", Inventory },
                    { "Item", item }
                };

                if(item is ChemicalContainer cont) {
                    addMenu.addMenuItem(new InputMenuItem($"{ChemicalContainer.chemicalToName(cont.Component)} einfüllen", $"Fülle {ChemicalContainer.chemicalToName(cont.Component)} in den Exsikkator. Es sind {Math.Round(cont.Amount, 2)} {ChemicalContainer.chemicalToUnit(cont.Component)} verfügbar", $"{Math.Round(cont.Amount, 2)} {ChemicalContainer.chemicalToUnit(cont.Component)}", "PUT_STUFF_IN_FOLD_TABLE_UTENSIL").withData(data).needsConfirmation($"{item.Name} reinlegen?", "Item wirklich reinlegen?"));
                } else if(item is FoldingTableChemical chem) {
                    addMenu.addMenuItem(new InputMenuItem($"{ChemicalContainer.chemicalToName(chem.Component)} einfüllen", $"Fülle {ChemicalContainer.chemicalToName(chem.Component)} in den Exsikkator. Es sind {Math.Round(chem.Amount, 2)} {ChemicalContainer.chemicalToUnit(chem.Component)} verfügbar", $"{Math.Round(chem.Amount, 2)} {ChemicalContainer.chemicalToUnit(chem.Component)}", "PUT_STUFF_IN_FOLD_TABLE_UTENSIL").withData(data).needsConfirmation($"{item.Name} reinlegen?", "Item wirklich reinlegen?"));
                }
            }
            menu.addMenuItem(new MenuMenuItem(addMenu.Name, addMenu));

            if(table.hasTableFunctionality("BURNER") && Inventory.hasItem<FoldingTableChemical>(c => c.Amount > 0 && getAllDrybleChems().Contains(c.Component))) {
                menu.addMenuItem(new StaticMenuItem("Etwas passiert in der Vorrichtung!", "Es findet aktuell eine Trocknung in der Vorrichtung statt", "", MenuItemStyle.green));
            }
            return menu;
        }

        public ChemicalType chemicalToDryable(ChemicalType chemicalType) {
            return chemicalType switch {
                ChemicalType.ColloidalSilver => ChemicalType.SilverDust,
                _ => ChemicalType.None,
            };
        }

        private static List<ChemicalType> getAllDrybleChems() {
            return new List<ChemicalType> { ChemicalType.ColloidalSilver };
        }

        private static List<ChemicalType> getAllResultChems() {
            return new List<ChemicalType> { ChemicalType.SilverDust };
        }

        //2000ml per hour
        public override void onTick(Controller.PlaceableObjects.FoldingTable table, TimeSpan tickLength) {
            if(table.hasTableFunctionality("BURNER")) {
                var chem = Inventory.getItem<FoldingTableChemical>(c => getAllDrybleChems().Contains(c.Component));
                if(chem != null) {
                    var timeMult = (float)(tickLength / TimeSpan.FromHours(1));
                    timeMult = Math.Min(1, timeMult);

                    var mult = chem.Amount / (2000f * timeMult);
                    mult = Math.Min(1, mult);

                    chem.Amount -= 2000f * mult * timeMult;

                    if(chem.Amount <= 0) {
                        chem.destroy();
                    }

                    var already = Inventory.getItem<FoldingTableChemical>(c => c.Component == chemicalToDryable(chem.Component));
                    if(already != null) {
                        already.Amount += 2000f * mult * timeMult;
                    } else {
                        var cfg = InventoryController.getConfigItemForType<FoldingTableChemical>();
                        Inventory.addItem(new FoldingTableChemical(cfg, chemicalToDryable(chem.Component), 2000f * mult * timeMult, chem.Quality));
                    }
                }
            }
        }

        public override FoldTableVisualCategory getVisualCategory() {
            return FoldTableVisualCategory.None;
        }
    }
}
