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
using static ChoiceVServer.Model.Menu.InputMenuItem;

namespace ChoiceVServer.InventorySystem {
    public class FoldTableUtensilController : ChoiceVScript {
        public FoldTableUtensilController() {
            EventController.addMenuEvent("PUT_STUFF_IN_FOLD_TABLE_UTENSIL", onPutStuffInFoldTableUtensil);

            EventController.addMenuEvent("PULL_STUFF_FROM_FOLD_TABLE_UTENSIL", onPullStuffFromFoldTableUtensil);
        }

        private bool onPutStuffInFoldTableUtensil(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var anim = AnimationController.getAnimationByName("WORK_FRONT");
            AnimationController.animationTask(player, anim, () => {
                var inv = (Inventory)data["Inventory"];
                var item = (Item)data["Item"];

                var amount = 0f;

                if(menuItemCefEvent is InputMenuItemEvent evt) {
                    try {
                        amount = float.Parse(evt.input);
                    } catch(Exception) {
                        player.sendBlockNotification("Eingabe falsch!", "Eingabe falsch!", Constants.NotifactionImages.FoldingTable);
                        return;
                    }
                }
                amount = Math.Abs(amount);

                if(amount > 0) {
                    if(item is FoldingTableChemical chem) {
                        amount = Math.Min(chem.Amount, amount);
                        var already = inv.getItem<FoldingTableChemical>(c => c.Component == chem.Component);
                        if(already != null) {
                            chem.Amount -= amount;
                            already.Amount += amount;
                        } else {
                            if(chem.Amount <= amount) {
                                chem.moveToInventory(inv);
                            } else {
                                chem.Amount -= amount;
                                var cfg = InventoryController.getConfigItemForType<FoldingTableChemical>();
                                inv.addItem(new FoldingTableChemical(cfg, chem.Component, amount, chem.Quality));
                            }
                        }

                        SoundController.playSoundAtCoords(player.Position, 5, ChemicalContainer.chemicalGetSoundForUnit(ChemicalContainer.chemicalToUnit(chem.Component)), 1, "mp3");
                        player.sendNotification(Constants.NotifactionTypes.Success, $"Erfolgreich {amount}{ChemicalContainer.chemicalToUnit(chem.Component)} {ChemicalContainer.chemicalToName(chem.Component)} in das Utensil gelegt", "Chemikalie in Utensil gelegt!", Constants.NotifactionImages.FoldingTable);
                    } else if(item is ChemicalContainer cont) {
                        amount = Math.Min(cont.Amount, amount);

                        cont.Amount -= amount;
                        var already = inv.getItem<FoldingTableChemical>(c => c.Component == cont.Component);
                        if(already != null) {
                            already.Amount += amount;
                        } else {
                            var cfg = InventoryController.getConfigItemForType<FoldingTableChemical>();
                            inv.addItem(new FoldingTableChemical(cfg, cont.Component, amount, cont.Quality));
                        }

                        SoundController.playSoundAtCoords(player.Position, 5, ChemicalContainer.chemicalGetSoundForUnit(ChemicalContainer.chemicalToUnit(cont.Component)), 1, "mp3");
                        player.sendNotification(Constants.NotifactionTypes.Success, $"Erfolgreich {amount}{ChemicalContainer.chemicalToUnit(cont.Component)} {ChemicalContainer.chemicalToName(cont.Component)} in das Utensil gelegt", "Chemikalie in Utensil gelegt!", Constants.NotifactionImages.FoldingTable);
                    }
                } else {
                    player.sendBlockNotification("Eingabe falsch!", "Eingabe falsch!", Constants.NotifactionImages.FoldingTable);
                }
            }, null, true, 1);

            return true;
        }

        private bool onPullStuffFromFoldTableUtensil(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var inv = (Inventory)data["Inventory"];
            var item = (FoldingTableChemical)data["Item"];

            SoundController.playSoundAtCoords(player.Position, 5, ChemicalContainer.chemicalGetSoundForUnit(ChemicalContainer.chemicalToUnit(item.Component)), 1, "mp3");
            item.showPulledFromUtensilMenu(player, inv);
            return true;
        }
    }

    public abstract class FoldTableUtensil : Item {
        public FoldTableUtensil(item item) : base(item) { }

        public FoldTableUtensil(configitem configItem, int quality) : base(configItem, quality) {

        }

        public abstract Menu getOnTableMenu(IPlayer player, Controller.PlaceableObjects.FoldingTable table);
        public abstract List<Item> getContributedItems();
        public abstract List<string> getContributedFunctionalities();
        public abstract FoldTableVisualCategory getVisualCategory();
        public abstract bool canBePulledOff(Controller.PlaceableObjects.FoldingTable table);
        public abstract void onTick(Controller.PlaceableObjects.FoldingTable table, TimeSpan tickLength);
    }
}
