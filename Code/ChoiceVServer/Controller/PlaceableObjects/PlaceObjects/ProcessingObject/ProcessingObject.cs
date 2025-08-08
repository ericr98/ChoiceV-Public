using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.PlaceableObjects {
    public class ProcessingObjectController : ChoiceVScript {
        public static List<ProcessingRecipe> AllRecipes { get; private set; }
        public ProcessingObjectController() {
            EventController.addMenuEvent("OPEN_PROCESSING_OBJECT_INVENTORY", onOpenProcessingObjectInventory);
            EventController.addMenuEvent("START_PROCESSING_OBJECT_PROCESS", onStartProcessingObjectProcess);
            loadRecipes();
        }

        public void loadRecipes() {
            AllRecipes = new List<ProcessingRecipe>();
            using(var db = new ChoiceVDb()) {
                foreach(var row in db.processingecipes
                    .Include(p => p.processingrecipeparts)
                        .ThenInclude(part => part.configItem)) {
                    var input = row.processingrecipeparts.Where(p => p.isOutput != 1).Select(part => new ProcessingRecipePart(part.configItem, part.amount, part.maxTolerance)).ToList();
                    var output = row.processingrecipeparts.Where(p => p.isOutput == 1 && p.isFailedOutput != 1).Select(part => new ProcessingRecipePart(part.configItem, part.amount, part.maxTolerance)).ToList();
                    var failedOutput = row.processingrecipeparts.Where(p => p.isOutput == 1 && p.isFailedOutput == 1).Select(part => new ProcessingRecipePart(part.configItem, part.amount, part.maxTolerance)).ToList();
                    AllRecipes.Add(new ProcessingRecipe(row.id, row.name, TimeSpan.FromSeconds(row.cookTimeSeconds), (ProcessingRecipePartTypes)row.recipeType, input, output, failedOutput, row.processingObjectModel));
                }
            }
        }

        private bool onOpenProcessingObjectInventory(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var obj = (ProcessingObject)data["ProcessingObject"];
            var isRunning = (bool)data["IsRunning"];
            if(isRunning) {
                InventoryController.showInventory(player, obj.Inventory, true);
            } else {
                InventoryController.showMoveInventory(player, player.getInventory(), obj.Inventory, null, null, obj.Name, true);
            }
            return true;
        }

        private bool onStartProcessingObjectProcess(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var obj = (ProcessingObject)data["ProcessingObject"];
            obj.onProcessStart();
            return true;
        }

        public static ProcessingRecipe getRecipeById(int id) {
            return AllRecipes.FirstOrDefault(r => r.Id == id);
        }
    }

    public class ProcessingObject : PlaceableObject {
        public string Name { get => (string)Data["Name"]; set { Data["Name"] = value; } }
        public Inventory Inventory { get => InventoryController.loadInventory((int)Data["Inventory"]); set { Data["Inventory"] = value.Id; } }
        private DateTime FinishDate { get => (DateTime)Data["FinishDate"]; set { Data["FinishDate"] = value; } }
        public int OutputMultiplier { get => (int)Data["OutputMultiplier"]; set { Data["OutputMultiplier"] = value; } }
        public bool Finished { get => (bool)Data["Finished"]; set { Data["Finished"] = value; } }
        public int CurrentQuality { get => (int)Data["CurrentQuality"]; set { Data["CurrentQuality"] = value; } }
        public bool HasFailed { get => (bool)Data["HasFailed"]; set { Data["HasFailed"] = value; } }
        public ProcessingRecipe SelectedRecipe { get => ProcessingObjectController.getRecipeById((int)Data["SelectedRecipe"]); set { Data["SelectedRecipe"] = value.Id; } }
        public bool IsRunning { get => (bool)Data["IsRunning"]; set { Data["IsRunning"] = value; } }

        private List<ProcessingRecipe> AllViableRecipes;
        public ProcessingRecipePartTypes ProcessingType;

        public ProcessingObject(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) {
            IntervalPlaceable = true;
        }

        public ProcessingObject(string model, Item placeableItem, IPlayer player, Position playerPosition, Rotation playerRotation) : base(playerPosition, playerRotation, 4f, 4f, true, new Dictionary<string, dynamic>()) {
            IntervalPlaceable = true;
            Name = placeableItem.Name;
            FinishDate = DateTime.MaxValue;
            IsRunning = false;
            Finished = false;
        }

        public override void onInterval(TimeSpan tickLength) {
            if(FinishDate <= DateTime.Now && !Finished) {
                Finished = true;
                IsRunning = false;
                SelectedRecipe.onFinishWithRecipe(Inventory, OutputMultiplier, CurrentQuality, HasFailed);
                resetModel();
            }
        }

        public override void initialize(bool register = true) {
            AllViableRecipes = ProcessingObjectController.AllRecipes.Where(r => r.Type == ProcessingType).ToList();
            Inventory.addBlockStatement(new InventoryAddBlockStatement(this, i => i is EquipItem));
            base.initialize(register);
        }

        public override Menu onInteractionMenu(IPlayer player) {
            return getLayerMenu();
        }

        public virtual void onProcessStart() {
            IsRunning = true;
            foreach(var viableRecipe in AllViableRecipes) {
                var mult = viableRecipe.checkForRecipe(Inventory);
                if(mult != -1) {
                    HasFailed = false;
                    FinishDate = DateTime.Now + viableRecipe.CookTime;
                    SelectedRecipe = viableRecipe;
                    OutputMultiplier = mult;
                    CurrentQuality = 1;
                    changeModel(viableRecipe);
                    return;
                }
            }

            //TODO WHAT HAPPENS TO SelectedRecipe?!
            HasFailed = true;
            OutputMultiplier = 1;
            CurrentQuality = 1;
            FinishDate = DateTime.Now + TimeSpan.FromMinutes(15);
        }

        public virtual void changeModel(ProcessingRecipe recipe) {
            ObjectController.deleteObject(Object);
            Object = ObjectController.createObject(recipe.ProcessingObjectModel, Position, Rotation, 200, true);
            PlaceableObjectModelChange?.Invoke(this, recipe.ProcessingObjectModel);
        }

        public virtual void resetModel() {
            ObjectController.deleteObject(Object);
        }

        private string getRestTimeString() {
            var num = Math.Round((FinishDate - DateTime.Now).TotalMinutes);
            if(num > 0) {
                return num.ToString() + "min";
            } else {
                return "Gleich fertig!";
            }
        }

        public virtual Menu getLayerMenu() {
            var menu = new Menu(Name, "Was möchtest du tun?");
            if(IsRunning) {
                menu.addMenuItem(new StaticMenuItem("Prozessdauer", $"Der Prozess wird noch ca. {getRestTimeString()} dauern", $"{getRestTimeString()}", MenuItemStyle.green));
            }
            menu.addMenuItem(new ClickMenuItem("Inhalt überprüfen", "Überprüfe den Inhalt und lege ggf. Sachen hinein", "", "OPEN_PROCESSING_OBJECT_INVENTORY").withData(new Dictionary<string, dynamic> { { "ProcessingObject", this }, { "IsRunning", IsRunning } }));
            if(!IsRunning && Inventory.getAllItems().Count != 0) {
                menu.addMenuItem(new ClickMenuItem("Prozess starten", "Starte den Prozess!", "", "START_PROCESSING_OBJECT_PROCESS").withData(new Dictionary<string, dynamic> { { "ProcessingObject", this }, { "IsRunning", IsRunning } }));
            }

            if(Inventory.getAllItems().Count != 0) {
                menu.addMenuItem(new StaticMenuItem("Zerstören", $"Es befinden sich noch Sachen drinnen!", $"Nicht möglich!", MenuItemStyle.red));
            } else {
                menu.addMenuItem(new ClickMenuItem("Zerstören", "Zerstöre das Objekt", "", "DESTROY_PLACABLE", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "placeable", this } }));
            }

            return menu;
        }

        public override void onRemove() {
            InventoryController.destroyInventory(Inventory);
            base.onRemove();
        }
    }
}
