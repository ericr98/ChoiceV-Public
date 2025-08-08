using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.InputMenuItem;

namespace ChoiceVServer.InventorySystem {
    public class FoldingTableChemicalController : ChoiceVScript {
        public FoldingTableChemicalController() {
            EventController.addMenuEvent("ON_FILL_CHEMICAL_CONTAINER", onFillChemicalContainer);
        }

        private bool onFillChemicalContainer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (FoldingTableChemical)data["Item"];
            var inv = (Inventory)data["Inventory"];
            var container = (ChemicalContainer)data["Container"];

            var evt = menuItemCefEvent as InputMenuItemEvent;

            var amount = 0f;
            try {
                amount = Math.Min(Math.Abs(float.Parse(evt.input)), item.Amount);
            } catch(Exception) {
                player.sendBlockNotification("Falsche Eingabe!", "Falsche Eingabe!");
                return false;
            }


            var anim = AnimationController.getAnimationByName("WORK_FRONT");
            AnimationController.animationTask(player, anim, () => {
                amount = Math.Min(container.MaxAmount - container.Amount, amount);

                container.Component = item.Component;
                container.Amount += amount;

                if(amount == item.Amount) {
                    inv.removeItem(item);
                } else {
                    item.Amount -= amount;
                }

                container.updateDescription();
                player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast erfolgreich {amount} {ChemicalContainer.chemicalToUnit(item.Component)} in den Behälter gefüllt", "Chemikalien in Behälter gefüllt", Constants.NotifactionImages.FoldingTable);
            }, null, true, 1, TimeSpan.FromSeconds(1));
            return true;
        }
    }

    public class FoldingTableChemical : Item, IFoldingTableInFlaskItem, IFoldingTableFlaskPuttable {
        public ChemicalType Component { get => (ChemicalType)Data["Component"]; set { Data["Component"] = value; } }
        public float Amount { get => (float)Data["Amount"]; set { Data["Amount"] = value; } }

        public string InFlaskName => ChemicalContainer.chemicalToName(Component);
        public string InFlaskDescription => Description;
        public float InFlaskAmount => Amount;
        public string InFlaskUnit => ChemicalContainer.chemicalToUnit(Component);

        public bool CanBePulledFromFlask => ChemicalContainer.chemicalCanBePulled(Component);

        public string FlaskName => ChemicalContainer.chemicalToName(Component);
        public string FlaskInfo => Description;
        public bool OnlyFullInFlask => false;
        public float FlaskAvailableAmount => Amount;
        public string FlaskUnit => ChemicalContainer.chemicalToUnit(Component);

        public FoldingTableChemical(item item) : base(item) { }

        public FoldingTableChemical(configitem configItem, ChemicalType component, float amount, int quality) : base(configItem, quality) {
            Component = component;
            Amount = amount;
        }

        public MenuItem getInFlaskMenuItem(string eventName) {
            return new ClickMenuItem(ChemicalContainer.chemicalToName(Component), $"{Amount}{ChemicalContainer.chemicalToUnit(Component)} {ChemicalContainer.chemicalToName(Component)}. Klicke um auf die Ablage zu legen.", $"{Amount}{ChemicalContainer.chemicalToUnit(Component)}", eventName);
        }

        public void getPulledItem(IPlayer player, FlaskUtensil flask) {
            showPulledFromUtensilMenu(player, flask.Inventory);
        }

        public void showPulledFromUtensilMenu(IPlayer player, Inventory inventory) {
            var menu = new Menu("Behälter auswählen", $"Vefügbar: {Amount} {ChemicalContainer.chemicalToUnit(Component)}");

            var conts = player.getInventory().getItems<ChemicalContainer>(c => c.Component == ChemicalType.None || c.Component == Component);

            if(conts.Count() > 0) {
                foreach(var cont in conts) {
                    var nameStr = "Leerer Chemikalienbehälter";
                    var containsStr = $"{cont.Amount}/{cont.MaxAmount}";
                    if(cont.Component != ChemicalType.None) {
                        nameStr = $"{ChemicalContainer.chemicalToName(cont.Component)}-Behälter";

                        if(cont.Amount == 0) {
                            containsStr = $"{cont.Amount}/{cont.MaxAmount} (kontaminiert)";
                        } else {
                            containsStr = $"{cont.Amount}/{cont.MaxAmount}{ChemicalContainer.chemicalToUnit(Component)}";
                        }
                    }
                    menu.addMenuItem(new InputMenuItem($"{nameStr}", $"Der Behälter enthält bereits {containsStr}", $"{containsStr}", "ON_FILL_CHEMICAL_CONTAINER").withData(new Dictionary<string, dynamic> { { "Inventory", inventory }, { "Item", this }, { "Container", cont } }));
                }

                player.showMenu(menu);
            } else {
                player.sendBlockNotification("Du hast keine passenden Behälter um das zu tun!", "Keine Behälter", Constants.NotifactionImages.FoldingTable);
            }
        }

        public void onPutInFlask(IPlayer player, FlaskUtensil flask, float amount) {
            var already = flask.Inventory.getItem<FoldingTableChemical>(c => c.Component == Component);

            if(amount >= Amount && already == null) {
                ResidingInventory.moveItem(flask.Inventory, this);
                return;
            }

            Amount -= amount;

            if(already != null) {
                already.Amount += amount;
            } else {
                var cfg = InventoryController.getConfigItemForType<FoldingTableChemical>();
                flask.Inventory.addItem(new FoldingTableChemical(cfg, Component, amount, Quality));
            }

            if(Amount <= 0) {
                destroy();
            }
            player.sendNotification(Constants.NotifactionTypes.Success, $"Erfolgreich {amount} {ChemicalContainer.chemicalToUnit(Component)} {ChemicalContainer.chemicalToName(Component)} hinzugefügt!", "Chemikalie hinzugefügt", Constants.NotifactionImages.FoldingTable);
        }
    }
}
