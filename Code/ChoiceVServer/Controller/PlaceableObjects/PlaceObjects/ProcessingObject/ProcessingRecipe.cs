using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.PlaceableObjects {
    public enum ProcessingRecipePartTypes {
        RainBarrel = 0,
        DryingRack = 1,
        CampFire = 2,
    }

    public class ProcessingRecipe {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public TimeSpan CookTime { get; private set; }
        public ProcessingRecipePartTypes Type { get; private set; }
        public List<ProcessingRecipePart> AllInputParts { get; private set; }
        public List<ProcessingRecipePart> AllOutputParts { get; private set; }
        public List<ProcessingRecipePart> AllFailedOutputParts { get; private set; }
        public string ProcessingObjectModel { get; private set; }

        public ProcessingRecipe(int id, string name, TimeSpan cookTime, ProcessingRecipePartTypes type, List<ProcessingRecipePart> allInputs, List<ProcessingRecipePart> allOutputs, List<ProcessingRecipePart> allFailedOutputs, string objectModel) {
            Id = id;
            Name = name;
            CookTime = cookTime;
            Type = type;
            AllInputParts = allInputs;
            AllOutputParts = allOutputs;
            AllFailedOutputParts = allFailedOutputs;
            ProcessingObjectModel = objectModel;
        }

        /// <summary>
        /// Checks if a recipe works for a given Inventory. If true returns the ratio for the recipe. If false returns -1
        /// </summary>
        public int checkForRecipe(Inventory inventory) {
            var currentAmountRatio = 0f;
            foreach(var input in AllInputParts) {
                var items = inventory.getItems(i => i.ConfigId == input.Item.configItemId);
                if(items == null) {
                    return -1;
                } else {
                    var amount = 0;
                    items.ForEach(i => amount += i.StackAmount ?? 1);
                    if(currentAmountRatio == 0) {
                        currentAmountRatio = amount / input.Amount;
                    } else {
                        var itemRatio = amount / input.Amount;
                        if(currentAmountRatio - input.MaxTolerance > itemRatio || currentAmountRatio + input.MaxTolerance < itemRatio) {
                            return -1;
                        }
                    }
                }
            }

            return (int)Math.Round(currentAmountRatio);
        }

        public void onFinishWithRecipe(Inventory inventory, int multiplier, int currentQuality, bool failed) {
            inventory.clearAllItems();
            var itList = AllOutputParts;
            if(failed) {
                itList = AllFailedOutputParts;
            }

            foreach(var output in itList) {
                if(Item.isGenericStackable(output.Item)) {
                    var item = InventoryController.createGenericStackableItem(output.Item, output.Amount * multiplier, currentQuality);
                    inventory.addItem(item);
                } else {
                    for(int i = 0; i < multiplier * output.Amount; i++) {
                        var item = InventoryController.createGenericItem(output.Item);
                        inventory.addItem(item);
                    }
                }
            }
        }
    }

    public class ProcessingRecipePart {
        public configitem Item { get; private set; }
        public int Amount { get; private set; }
        public float MaxTolerance { get; private set; }

        public ProcessingRecipePart(configitem item, int amount, float maxTolerance) {
            Item = item;
            Amount = amount;
            MaxTolerance = maxTolerance;
        }
    }
}
