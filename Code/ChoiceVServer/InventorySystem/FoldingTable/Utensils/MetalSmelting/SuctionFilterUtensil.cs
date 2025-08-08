using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.PlaceableObjects;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public class SuctionFilterUntensilController : ChoiceVScript {
        public SuctionFilterUntensilController() {
            EventController.addMenuEvent("SUCTION_FILTER_REMOVE_WATER", onSuctionFilterRemoveWater);
        }

        private bool onSuctionFilterRemoveWater(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var anim = AnimationController.getAnimationByName("WORK_FRONT");
            AnimationController.animationTask(player, anim, () => {
                var filter = (SuctionFilterUtensil)data["Utensil"];
                var item = (Item)data["Item"];

                filter.Inventory.removeItem(item);

                SoundController.playSoundAtCoords(player.Position, 5, ChemicalContainer.chemicalGetSoundForUnit("ml"), 1, "mp3");
                player.sendNotification(Constants.NotifactionTypes.Info, "Das Wasser wurde aus der Nutsche abgekippt!", "Wasser abgekippt!", Constants.NotifactionImages.FoldingTable);
            }, null, true, 1);
            return true;
        }
    }

    public class SuctionFilterUtensil : FoldTableUtensil {
        public Inventory Inventory { get => InventoryController.loadInventory((int)Data["InventoryId"]); set => Data["InventoryId"] = value.Id; }

        public SuctionFilterUtensil(item item) : base(item) { }

        public SuctionFilterUtensil(configitem configItem, int amount, int quality) : base(configItem, quality) {
            Inventory = InventoryController.createInventory(Id ?? -1, 1000, InventoryTypes.SuctionFilterUtensil);
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
            if(!Inventory.hasItem<FoldingTableChemical>(c => c.Component == ChemicalType.DestilledWater)) {
                return Inventory.getAllItems();
            } else {
                return new List<Item>();
            }
        }

        public override Menu getOnTableMenu(IPlayer player, Controller.PlaceableObjects.FoldingTable table) {
            var menu = new Menu("Vakuumfilter (Buechnertrichter)", "Was möchtest du tun?");

            var contentMenu = new Menu("Inhalt", "Wähle aus zum Herausnehmen");
            var waterItem = Inventory.getItem<FoldingTableChemical>(c => c.Component == ChemicalType.DestilledWater);
            if(waterItem != null) {
                contentMenu.addMenuItem(new ClickMenuItem("Wasser abkippen", "Kippe das übrige Wasser ab", "", "SUCTION_FILTER_REMOVE_WATER").withData(new Dictionary<string, dynamic> { { "Utensil", this }, { "Item", waterItem } }).needsConfirmation("Wasser abkippen?", $"{Math.Round(waterItem.Amount, 2)}ml Wasser wirklich abkippen?"));
                contentMenu.addMenuItem(new StaticMenuItem("dest. Wasser", $"Die Nutsche enthält {Math.Round(waterItem.Amount, 2)}ml dest. Wasser", $"{Math.Round(waterItem.Amount, 2)}ml"));
            }

            foreach(var item in Inventory.getItems<FoldingTableChemical>(i => i.Component != ChemicalType.DestilledWater)) {
                if(waterItem == null) {
                    var data = new Dictionary<string, dynamic> {
                        { "Inventory", Inventory },
                        { "Item", item }
                    };

                    contentMenu.addMenuItem(new ClickMenuItem($"{ChemicalContainer.chemicalToName(item.Component)}", $"Es befinden sich {Math.Round(item.Amount, 2)}{ChemicalContainer.chemicalToUnit(item.Component)} {ChemicalContainer.chemicalToName(item.Component)} in der Nutsche. Es kann herausgenommen werden", $"{Math.Round(item.Amount, 2)} {ChemicalContainer.chemicalToUnit(item.Component)}", "PULL_STUFF_FROM_FOLD_TABLE_UTENSIL").withData(data));
                } else {
                    contentMenu.addMenuItem(new StaticMenuItem($"{ChemicalContainer.chemicalToName(item.Component)}", $"Es befinden sich {Math.Round(item.Amount, 2)}{ChemicalContainer.chemicalToUnit(item.Component)} {ChemicalContainer.chemicalToName(item.Component)} in der Nutsche. Für das Herausnehmen muss das Wasser abgegossen werden!", $"{Math.Round(item.Amount, 2)} {ChemicalContainer.chemicalToUnit(item.Component)}"));
                }
            }
            menu.addMenuItem(new MenuMenuItem(contentMenu.Name, contentMenu));

            var currentComponent = Inventory.getItem<FoldingTableChemical>(i => i.Component != ChemicalType.DestilledWater && allCleanChemicals().Contains(i.Component))?.Component ?? ChemicalType.None;

            var addMenu = new Menu("In Vakuumfilter legen", "Wähle aus welche");
            var list = player.getInventory().getItems<ChemicalContainer>(i => i.Amount > 0 && i.Component == ChemicalType.DestilledWater || (currentComponent == ChemicalType.None ? i.Component != ChemicalType.None : i.Component == currentComponent))
                        .Concat(table.getContributedItems(this).Where(i => (i is FoldingTableChemical chem && (currentComponent == ChemicalType.None ? chem.Component != ChemicalType.None : chem.Component == currentComponent))
                                                                    || (i is ChemicalContainer cont && cont.Amount > 0 && (cont.Component == ChemicalType.DestilledWater || (currentComponent == ChemicalType.None ? cont.Component != ChemicalType.None : cont.Component == currentComponent)))));

            foreach(var item in list) {
                var data = new Dictionary<string, dynamic> {
                    { "Inventory", Inventory },
                    { "Item", item }
                };

                if(item is ChemicalContainer cont) {
                    addMenu.addMenuItem(new InputMenuItem($"{ChemicalContainer.chemicalToName(cont.Component)} einfüllen", $"Fülle {ChemicalContainer.chemicalToName(cont.Component)} in die Nutsche. Es sind {Math.Round(cont.Amount, 2)} {ChemicalContainer.chemicalToUnit(cont.Component)} verfügbar", $"{Math.Round(cont.Amount, 2)} {ChemicalContainer.chemicalToUnit(cont.Component)}", "PUT_STUFF_IN_FOLD_TABLE_UTENSIL").withData(data).needsConfirmation($"{item.Name} reinlegen?", "Item wirklich reinlegen?"));
                } else if(item is FoldingTableChemical chem) {
                    addMenu.addMenuItem(new InputMenuItem($"{ChemicalContainer.chemicalToName(chem.Component)} einfüllen", $"Fülle {ChemicalContainer.chemicalToName(chem.Component)} in die Nutsche. Es sind {Math.Round(chem.Amount, 2)} {ChemicalContainer.chemicalToUnit(chem.Component)} verfügbar", $"{Math.Round(chem.Amount, 2)} {ChemicalContainer.chemicalToUnit(chem.Component)}", "PUT_STUFF_IN_FOLD_TABLE_UTENSIL").withData(data).needsConfirmation($"{item.Name} reinlegen?", "Item wirklich reinlegen?"));
                }
            }
            menu.addMenuItem(new MenuMenuItem(addMenu.Name, addMenu));

            if(Inventory.hasItem<FoldingTableChemical>(c => c.Component == ChemicalType.DestilledWater) && Inventory.hasItem<FoldingTableChemical>(c => c.Component != ChemicalType.DestilledWater && allCleanableChemicals().Contains(c.Component))) {
                menu.addMenuItem(new StaticMenuItem("Etwas passiert in der Vorrichtung!", "Es findet aktuell eine Reinigung in der Vorrichtung statt", "", MenuItemStyle.green));
            }

            return menu;
        }

        //50ml Water for 1g
        //10g Chems per hour
        public override void onTick(Controller.PlaceableObjects.FoldingTable table, TimeSpan tickLength) {
            var water = Inventory.getItem<FoldingTableChemical>(c => c.Component == ChemicalType.DestilledWater);
            if(water != null) {
                var chem = Inventory.getItem<FoldingTableChemical>(c => c.Component != ChemicalType.DestilledWater && allCleanableChemicals().Contains(c.Component));
                if(chem != null) {
                    var timeMult = (float)(tickLength / TimeSpan.FromMinutes(60));
                    timeMult = Math.Min(timeMult, 1);
                    //Hat noch Bugs!
                    //Mal mit 59.87g Silverchlorid und 3000ml Wasser probieren!
                    //500ml per hour
                    var waterMult = water.Amount / (500f * timeMult);
                    waterMult = Math.Min(waterMult, 1);

                    //10g per hour
                    var chemMult = chem.Amount / (10 * timeMult);
                    chemMult = Math.Min(chemMult, 10);

                    var mult = Math.Min(waterMult, chemMult);

                    water.Amount -= 500f * mult * timeMult;
                    chem.Amount -= 10 * mult * timeMult;

                    if(water.Amount <= 0) {
                        water.destroy();
                    }

                    if(chem.Amount <= 0) {
                        chem.destroy();
                    }

                    var already = Inventory.getItem<FoldingTableChemical>(c => chemicalToCleanChemical(chem.Component) == c.Component);
                    if(already != null) {
                        already.Amount += 10f * mult * timeMult;
                    } else {
                        var cfg = InventoryController.getConfigItemForType<FoldingTableChemical>();
                        Inventory.addItem(new FoldingTableChemical(cfg, chemicalToCleanChemical(chem.Component), 10 * mult * timeMult, chem.Quality));
                    }
                }
            }
        }

        private static List<ChemicalType> allCleanableChemicals() {
            return new List<ChemicalType>() {
                ChemicalType.SilverChlorid,
            };
        }

        private static List<ChemicalType> allCleanChemicals() {
            return new List<ChemicalType>() {
                ChemicalType.CleanSilverChlorid,
            };
        }

        private static ChemicalType chemicalToCleanChemical(ChemicalType component) {
            return component switch {
                ChemicalType.SilverChlorid => ChemicalType.CleanSilverChlorid,
                _ => ChemicalType.None,
            };
        }

        public override FoldTableVisualCategory getVisualCategory() {
            return FoldTableVisualCategory.None;
        }
    }
}
