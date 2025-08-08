using AltV.Net.Elements.Entities;
using Bogus.DataSets;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.CraftingSystem {
    public delegate void InventoryChangeDelegate(CraftingTimedSpot spot, bool isEmpty);
    
    public class CraftingTimedSpot {
        public int Id;
        private PlacedCraftingTimedSpot? PlacedCraftingTimedSpot;
        public readonly string Name;
        public readonly List<int> Transformations;
        public readonly Inventory Inventory;
        private configcraftingtransformation Transformation;
        private DateTime? TransformationStartDate;
        
        public string SoundIdentifier;
        private string SoundName;
        
        public CraftingTimedSpot(int id, string name, List<CraftingTransformations> transformations, Inventory inventory, string soundIdentifier) {
            Id = id;
            Transformations = transformations.Select(t => (int)t).ToList();
            Name = name;
            Inventory = inventory;
            
            SoundIdentifier = soundIdentifier;
            
            Inventory.ThisInventoryEmptyAfterMoveDelegate += onInventoryEmpty; 
            Inventory.ThisInventoryAddItemDelegate += onInventoryAddItem;
        }
        
        public void setPlacedCraftingTimedSpot(PlacedCraftingTimedSpot placedCraftingTimedSpot) {
            PlacedCraftingTimedSpot = placedCraftingTimedSpot;
        }

        private bool onInventoryEmpty(Inventory inventory) {
            onInventoryChange(true);
            return true;
        }

        private void onInventoryAddItem(Inventory inventory, Item item, int? amount, bool savetodb) {
            onInventoryChange(false);

            if(Transformation == null) {
                onTick();
            }
        }
        
        private void onInventoryChange(bool isEmpty) {
            if(string.IsNullOrEmpty(SoundIdentifier) || PlacedCraftingTimedSpot == null) return;
                     
            if(!isEmpty) {
                if(SoundName == null) {
                    SoundName = SoundController.playSoundAtCoords(PlacedCraftingTimedSpot.CollisionShape.Position, 7.5f, SoundIdentifier, true);
                }
            } else {
                if(SoundName != null) {
                    SoundController.stopSound(SoundName);
                    SoundName = null;
                }
            }
        }

        public string getSpotTransformationProgressString() {
            if(Transformation == null) return "";

            var progress = Math.Min((DateTime.Now - TransformationStartDate ?? TimeSpan.Zero).TotalSeconds / (Transformation.craftingProcessTime ?? 1), 1);

            return $"Fortschritt: {Math.Round(progress * 100)}%";
        }

        public void onTick() {
            if(Inventory.getCount() == 0) {
                if(Transformation != null) {
                    MenuController.updateMenu("CRAFTING_SPOT_MENU", $"CRAFTING_SPOT_OPEN_{Id}", getSpotTransformationProgressString());
                    Transformation = null;
                    TransformationStartDate = null;
                }
                return;
            }
            
            if(Transformation == null) {
                using(var db = new ChoiceVDb()) {
                    var inventoryConfigIds = Inventory.getAllItems().Select(i => i.ConfigId).ToList();
                    var viableTransformations = db.configcraftingtransformations
                        .Include(t => t.configcraftingtransformationsitems)
                        .ThenInclude(t => t.item)
                        .Where(c => Transformations.Contains(c.transformationType) && c.configcraftingtransformationsitems.All(i => !i.isInput || inventoryConfigIds.Contains(i.itemId))).ToList();
                    
                    Transformation = viableTransformations.FirstOrDefault(c => CraftingController.areTransformationRequirementsMet((CraftingTransformations)c.transformationType, Inventory, null).meetsRequirement);
                    TransformationStartDate = DateTime.Now;
                }
            } else {
                MenuController.updateMenu("CRAFTING_SPOT_MENU", $"CRAFTING_SPOT_OPEN_{Id}", getSpotTransformationProgressString());
                
                var worked = true;
                foreach(var item in Transformation.configcraftingtransformationsitems) {
                    if(item.isInput) {
                        if(!Inventory.hasItem(i => i.ConfigId == item.itemId)) {
                            worked = false;
                            break;
                        }
                    }
                }
                
                if(!worked) {
                    Transformation = null;
                    TransformationStartDate = null;
                    MenuController.updateMenu("CRAFTING_SPOT_MENU", $"CRAFTING_SPOT_OPEN_{Id}", getSpotTransformationProgressString());
                } else if(TransformationStartDate != null && DateTime.Now - TransformationStartDate > TimeSpan.FromSeconds(Transformation.craftingProcessTime ?? 1)) {
                    var (requirementsMet, toolItem, _) = CraftingController.areTransformationRequirementsMet((CraftingTransformations)Transformation.transformationType, Inventory, null);
                    if(requirementsMet) {
                        if(CraftingController.processCraftingTransformation(Transformation, Inventory)) {
                            toolItem?.use(null);
                        }
                    }
                }
            }
        }
    }
}