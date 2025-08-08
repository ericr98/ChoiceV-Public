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
using static ChoiceVServer.Model.Menu.InputMenuItem;

namespace ChoiceVServer.InventorySystem {
    public class CrucibleUtensilController : ChoiceVScript {
        public CrucibleUtensilController() {
            EventController.addMenuEvent("CRUCIBLE_SMELT_SILVER_COINS", onCrucibleSmeltSilverCoins);
        }

        private bool onCrucibleSmeltSilverCoins(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var table = (Controller.PlaceableObjects.FoldingTable)data["Table"];
            var component = (ChemicalType)data["Component"];

            var evt = menuItemCefEvent as InputMenuItemEvent;

            var amount = 0;
            try {
                amount = Math.Abs(int.Parse(evt.input));
            } catch(Exception) {
                player.sendBlockNotification("Falsche Eingabe!", "Falsche Eingabe!");
                return false;
            }

            crucibleSmeltStep(player, component, table, amount);

            return true;
        }

        private void crucibleSmeltStep(IPlayer player, ChemicalType component, Controller.PlaceableObjects.FoldingTable table, int remaining) {
            var anim = AnimationController.getAnimationByName("WORK_FRONT");
            if(player.getInventory().MaxWeight - player.getInventory().CurrentWeight < 0.03 && table.ItemInventory.MaxWeight - table.ItemInventory.CurrentWeight < 0.03) {
                player.sendBlockNotification($"Es ist kein Platz für eine weitere Münze!!", $"Nicht genug Platz!", Constants.NotifactionImages.FoldingTable);
                return;
            }

            AnimationController.animationTask(player, anim, () => {
                var neededAmount = 30f;

                var onTableChem = table.getContributedItems().Where(i => i is FoldingTableChemical chem && chem.Component == component).Cast<FoldingTableChemical>();
                var onTableCont = table.getContributedItems().Where(i => i is ChemicalContainer chem && chem.Component == component).Cast<ChemicalContainer>();
                var playerCont = player.getInventory().getItems<ChemicalContainer>(i => i.Component == component);

                foreach(var item in onTableChem) {
                    var available = Math.Min(item.Amount, neededAmount);

                    item.Amount -= available;
                    neededAmount -= available;
                    if(item.Amount <= 0) {
                        item.destroy();
                    }

                    if(neededAmount <= 0) {
                        continue;
                    }
                }

                if(neededAmount > 0) {
                    foreach(var item in onTableCont.Concat(playerCont)) {
                        var available = Math.Min(item.Amount, neededAmount);

                        item.Amount -= available;
                        neededAmount -= available;

                        if(neededAmount <= 0) {
                            continue;
                        }
                    }
                }

                if(neededAmount <= 0) {
                    remaining--;
                    var cfg = InventoryController.getConfigItem(c => c.additionalInfo == CrucibleUtensil.chemicalToCoinKeyWord(component));
                    var item = new StaticItem(cfg, 1, 0);

                    if(!table.ItemInventory.addItem(item)) {
                        player.getInventory().addItem(item);
                    }

                    SoundController.playSoundAtCoords(player.Position, 10, SoundController.Sounds.FizzlingWater, 0.9f, "mp3");
                    player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast eine {item.Name} gegossen! Sie wurde auf die Ablage oder ins Inventar gelegt.", $"{item.Name} gegossen", Constants.NotifactionImages.FoldingTable);
                } else {
                    player.sendBlockNotification($"Es war nicht mehr genügend {ChemicalContainer.chemicalToName(component)} übrig! Der Rest des Staubes ist verloren gegangen!", $"Nicht genug {ChemicalContainer.chemicalToName(component)}", Constants.NotifactionImages.FoldingTable);
                    return;
                }

                if(remaining > 0) {
                    crucibleSmeltStep(player, component, table, remaining);
                }
            }, null, true, 1);
        }
    }

    public class CrucibleUtensil : FoldTableUtensil {

        public CrucibleUtensil(item item) : base(item) { }

        public CrucibleUtensil(configitem configItem, int amount, int quality) : base(configItem, quality) { }

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
            var menu = new Menu("Schmelztiegel", "Was möchtest du tun?");

            var items = player.getInventory().getItems(i => i is ChemicalContainer cont && chemicalToCoinKeyWord(cont.Component) != null)
                        .Concat(table.getContributedItems().Where(i => (i is ChemicalContainer cont && chemicalToCoinKeyWord(cont.Component) != null)
                                                               || (i is FoldingTableChemical chem && chemicalToCoinKeyWord(chem.Component) != null)));
            var dic = new Dictionary<ChemicalType, float>();
            foreach(var item in items) {
                var comp = ChemicalType.None;
                var amount = 0f;

                if(item is ChemicalContainer cont) {
                    comp = cont.Component;
                    amount = cont.Amount;
                } else if(item is FoldingTableChemical chem) {
                    comp = chem.Component;
                    amount = chem.Amount;
                }

                if(dic.ContainsKey(comp)) {
                    dic[comp] += amount;
                } else {
                    dic[comp] = amount;
                }
            }

            var smeltMenu = new Menu("Schmelzen", "Schmelze Edelmetalle zu Münzen");
            foreach(var el in dic) {
                var data = new Dictionary<string, dynamic> {
                    { "Table", table },
                    { "Component", el.Key }
                };
                var coinAmount = Math.Floor(el.Value / 30);
                smeltMenu.addMenuItem(new InputMenuItem($"{chemicalToResultName(el.Key)}-Münze", $"Schmelze eine Silbermünze à 30g. Es ist {el.Value}{ChemicalContainer.chemicalToUnit(el.Key)} {ChemicalContainer.chemicalToName(el.Key)} verfügbar. Es können {coinAmount} Münzen geschmolzen werden!", $"verfügbar: {coinAmount}", "CRUCIBLE_SMELT_SILVER_COINS").withData(data).needsConfirmation("Münzen schmelzen?", "Münzen wirklich schmelzen?"));
            }
            menu.addMenuItem(new MenuMenuItem(smeltMenu.Name, smeltMenu));

            return menu;
        }

        public static string chemicalToCoinKeyWord(ChemicalType chemicalType) {
            return chemicalType switch {
                ChemicalType.SilverDust => "SILVER_COIN",
                _ => null,
            };
        }

        public string chemicalToResultName(ChemicalType chemicalType) {
            return chemicalType switch {
                ChemicalType.SilverDust => "Silber",
                _ => null,
            };
        }

        public override void onTick(Controller.PlaceableObjects.FoldingTable table, TimeSpan tickLenght) { }

        public override FoldTableVisualCategory getVisualCategory() {
            return FoldTableVisualCategory.None;
        }
    }
}
