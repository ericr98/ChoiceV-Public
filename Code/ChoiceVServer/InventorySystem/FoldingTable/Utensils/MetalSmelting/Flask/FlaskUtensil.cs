using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.PlaceableObjects;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem.FoldingTable;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.InputMenuItem;

namespace ChoiceVServer.InventorySystem {
    public interface IFoldingTableFlaskPuttable {
        public string FlaskName { get; }
        public string FlaskInfo { get; }
        public bool OnlyFullInFlask { get; }
        public float FlaskAvailableAmount { get; }
        public string FlaskUnit { get; }

        public void onPutInFlask(IPlayer player, FlaskUtensil flask, float amount);
    }

    public interface IFoldingTableInFlaskItem {
        public string InFlaskName { get; }
        public string InFlaskDescription { get; }
        public float InFlaskAmount { get; }
        public string InFlaskUnit { get; }

        public bool CanBePulledFromFlask { get; }

        public void getPulledItem(IPlayer player, FlaskUtensil utensil);
    }

    public class FlaskUtensilController : ChoiceVScript {
        public static List<FlaskRecipe> AllRecipes;

        public FlaskUtensilController() {
            EventController.addMenuEvent("PUT_STUFF_IN_FLASK", onPutStuffinFlask);
            EventController.addMenuEvent("FLASK_REMOVE_NON_EXTRACTABLES", onFlaskRemoveNonExtractables);
            EventController.addMenuEvent("FLASK_REGULATE_TEMPERATURE", onFlaskRegulateTemperature);
            EventController.addMenuEvent("ON_FLASK_PULL_EXTRACTABLE", onFlaskPullExtractable);

            loadRecipes();
        }


        private void loadRecipes() {
            AllRecipes = new List<FlaskRecipe>();
            AllRecipes.Add(new SilverChloridRecipe());
            AllRecipes.Add(new ColloidalSilverRecipe());
        }

        private bool onPutStuffinFlask(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var flask = (FlaskUtensil)data["Flask"];
            var item = (IFoldingTableFlaskPuttable)data["Item"];

            var amount = item.FlaskAvailableAmount;

            if(menuItemCefEvent is InputMenuItemEvent evt) {
                try {
                    amount = float.Parse(evt.input);
                } catch(Exception) {
                    player.sendBlockNotification("Eingabe falsch!", "Eingabe falsch!", Constants.NotifactionImages.FoldingTable);
                    return true;
                }
            }
            amount = Math.Abs(amount);

            var anim = AnimationController.getAnimationByName("WORK_FRONT");
            AnimationController.animationTask(player, anim, () => {
                SoundController.playSoundAtCoords(player.Position, 5, ChemicalContainer.chemicalGetSoundForUnit(item.FlaskUnit), 0.75f, "mp3");
                item.onPutInFlask(player, flask, Math.Min(item.FlaskAvailableAmount, amount));
            }, null, true, 1, TimeSpan.FromSeconds(1));
            return true;
        }

        private bool onFlaskRemoveNonExtractables(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var flask = (FlaskUtensil)data["Flask"];

            var anim = AnimationController.getAnimationByName("WORK_FRONT");
            AnimationController.animationTask(player, anim, () => {
                var nonExtracts = flask.Inventory.getItems<IFoldingTableInFlaskItem>(i => !i.CanBePulledFromFlask);

                foreach(var item in nonExtracts) {
                    flask.Inventory.removeItem(item as Item);
                }

                SoundController.playSoundAtCoords(player.Position, 5, ChemicalContainer.chemicalGetSoundForUnit("ml"), 1, "mp3");
                player.sendNotification(Constants.NotifactionTypes.Info, "Du hast alle nicht extrahierbaren Inhalte aus der Vorrichtung gekippt", "Inhalte entfernt", Constants.NotifactionImages.FoldingTable);

            }, null, true, 1, TimeSpan.FromSeconds(1));
            return true;
        }

        private bool onFlaskRegulateTemperature(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var flask = (FlaskUtensil)data["Flask"];

            var evt = menuItemCefEvent as InputMenuItemEvent;

            try {
                var temp = float.Parse(evt.input);
                temp = Math.Abs(temp);

                var anim = AnimationController.getAnimationByName("WORK_FRONT");
                AnimationController.animationTask(player, anim, () => {
                    flask.Temperature = temp;
                    SoundController.playSoundAtCoords(player.Position, 5, SoundController.Sounds.BurnerStarting, 1, "mp3");
                }, null, true, 1);
            } catch(Exception) {
                player.sendBlockNotification("Eingabe falsch!", "Etwas schiefgelaufen");
            }

            return true;
        }
        private bool onFlaskPullExtractable(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var flask = (FlaskUtensil)data["Flask"];
            var item = (IFoldingTableInFlaskItem)data["Item"];

            SoundController.playSoundAtCoords(player.Position, 5, ChemicalContainer.chemicalGetSoundForUnit(item.InFlaskUnit), 1, "mp3");
            item.getPulledItem(player, flask);
            return true;
        }
    }

    public class FlaskUtensil : FoldTableUtensil {
        public Inventory Inventory { get => InventoryController.loadInventory((int)Data["InventoryId"]); set => Data["InventoryId"] = value.Id; }
        public float Temperature { get => (float)Data["Temperature"]; set => Data["Temperature"] = value; }

        public FlaskUtensil(item item) : base(item) {
            if(!Data.hasKey("Temperature")) {
                Temperature = 0;
            }
        }

        public FlaskUtensil(configitem configItem, int amount, int quality) : base(configItem, quality) {
            Inventory = InventoryController.createInventory(Id ?? -1, 1000, InventoryTypes.FlaskUtensil);
            Temperature = 0;
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
            if(!Inventory.hasItem<IFoldingTableInFlaskItem>(i => !i.CanBePulledFromFlask)) {
                return Inventory.getItems<IFoldingTableInFlaskItem>(i => i.CanBePulledFromFlask).Cast<Item>().ToList();
            } else {
                return new List<Item>();
            }
        }

        public override Menu getOnTableMenu(IPlayer player, Controller.PlaceableObjects.FoldingTable table) {
            var menu = new Menu("Glaskolbenvorrichtung", "Was möchtest du tun?");

            if(Inventory.getAllItems().Count > 0) {
                var takeMenu = new Menu("Inhalt", "Klicke um auf ins aufzunehmen");
                var anyNonRemovable = Inventory.hasItem<IFoldingTableInFlaskItem>(i => !i.CanBePulledFromFlask);
                if(anyNonRemovable) {
                    takeMenu.addMenuItem(new ClickMenuItem("Nicht-Extrahierbare wegkippen", "Kippe alle nicht extrahierbaren Inhalte weg.", "", "FLASK_REMOVE_NON_EXTRACTABLES").withData(new Dictionary<string, dynamic> { { "Flask", this } }).needsConfirmation("Inhalte wegkippen?", "Nicht extrahierbare Inhalte wegkippen?"));
                }

                foreach(var item in Inventory.getItems<IFoldingTableInFlaskItem>(i => true)) {
                    var data = new Dictionary<string, dynamic> {
                        { "Flask", this },
                        { "Item", item }
                    };

                    var desc = "";
                    if(item.InFlaskDescription != null && item.InFlaskDescription != "") {
                        desc = $"Mit Beschreibung: {item.InFlaskDescription}.";
                    }

                    if(!anyNonRemovable && item.CanBePulledFromFlask) {
                        takeMenu.addMenuItem(new ClickMenuItem($"{item.InFlaskName}", $"Es befinden sich: {Math.Round(item.InFlaskAmount, 2)} {item.InFlaskUnit} {item.InFlaskName} in der Vorrichtung. {desc} Es kann herausgenommen werden.", $"{Math.Round(item.InFlaskAmount, 2)} {item.InFlaskUnit}", "ON_FLASK_PULL_EXTRACTABLE").withData(data));
                    } else {
                        if(item.CanBePulledFromFlask) {
                            takeMenu.addMenuItem(new StaticMenuItem($"{item.InFlaskName}", $"Es befinden sich: {Math.Round(item.InFlaskAmount, 2)} {item.InFlaskUnit} {item.InFlaskName} in der Vorrichtung. {desc} Zum Herausnehmen nicht Extrahierbare wegkippen!", $"{Math.Round(item.InFlaskAmount, 2)} {item.InFlaskUnit}"));
                        } else {
                            takeMenu.addMenuItem(new StaticMenuItem($"{item.InFlaskName}", $"Es befinden sich: {Math.Round(item.InFlaskAmount, 2)} {item.InFlaskUnit} {item.InFlaskName} in der Vorrichtung. {desc} Kann nicht herausgenommen werden!", $"{Math.Round(item.InFlaskAmount, 2)} {item.InFlaskUnit}"));
                        }
                    }
                }
                menu.addMenuItem(new MenuMenuItem(takeMenu.Name, takeMenu));
            }

            var addMenu = new Menu("In Glaskolben legen", "Lege Items in den Kolben");
            foreach(var item in player.getInventory().getItems<IFoldingTableFlaskPuttable>(i => i.FlaskAvailableAmount > 0).Concat(table.getContributedItems(this).Where(i => i is IFoldingTableFlaskPuttable ifp && ifp.FlaskAvailableAmount > 0).Cast<IFoldingTableFlaskPuttable>())) {
                var data = new Dictionary<string, dynamic> {
                    { "Flask", this },
                    { "Item", item }
                };

                if(item.OnlyFullInFlask) {
                    addMenu.addMenuItem(new ClickMenuItem($"{item.FlaskName}", $"Fülle/Packe {item.FlaskName} mit Beschreibung: {item.FlaskInfo} in die Vorrichtung", $"{item.FlaskAvailableAmount} {item.FlaskUnit}", "PUT_STUFF_IN_FLASK").withData(data).needsConfirmation($"{item.FlaskName} in Vorrichtung geben?", "Wirklich in Vorrichtung geben?"));
                } else {
                    addMenu.addMenuItem(new InputMenuItem($"{item.FlaskName}", $"Fülle/Packe {item.FlaskName} mit Beschreibung: {item.FlaskInfo} in die Vorrichtung. Es sind {item.FlaskAvailableAmount} {item.FlaskUnit} verfügbar.", $"{item.FlaskAvailableAmount} {item.FlaskUnit} verfügbar", "PUT_STUFF_IN_FLASK").withData(data).needsConfirmation($"{item.FlaskName} in Vorrichtung geben?", "Wirklich in Vorrichtung geben?"));

                }
            }
            menu.addMenuItem(new MenuMenuItem(addMenu.Name, addMenu));

            if(table.hasTableFunctionality("BURNER")) {
                menu.addMenuItem(new InputMenuItem("Temperatur einstellen", "Stelle die Temperatur des Prozesses ein", $"{Math.Round(Temperature, 1)}° Grad", "FLASK_REGULATE_TEMPERATURE").withData(new Dictionary<string, dynamic> { { "Flask", this } }));
            }

            if(FlaskUtensilController.AllRecipes.Any(r => r.isValidRecipe(this))) {
                menu.addMenuItem(new StaticMenuItem("Etwas reagiert in der Vorrichtung!", "Es findet aktuell eine Reaktion in der Vorrichtung statt", "", MenuItemStyle.yellow));
            }

            return menu;
        }

        public override void onTick(Controller.PlaceableObjects.FoldingTable table, TimeSpan tickLength) {
            if(table.hasTableFunctionality("BURNER")) {
                //Just the number, the stuff was tuned to
                var tickMult = (float)(tickLength / TimeSpan.FromMinutes(6));
                if(Temperature != 0) {
                    Temperature += ((float)new Random().NextDouble()).Map(0, 1, -3 * tickMult, 2 * tickMult);
                }

                if(Temperature <= 0) {
                    Temperature = 0;
                }
            } else {
                Temperature = 0;
            }

            var recipe = FlaskUtensilController.AllRecipes.Find(r => r.isValidRecipe(this));

            if(recipe != null) {
                recipe.onTick(tickLength, this);
            }
        }

        public override FoldTableVisualCategory getVisualCategory() {
            return FoldTableVisualCategory.Lab;
        }
    }
}
