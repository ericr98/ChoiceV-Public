using AltV.Net.Elements.Entities;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;


namespace ChoiceVServer.Controller.CraftingSystem {
    public enum CraftingTransformations {
        Combining = 0,
        Cutting = 1, //Schneiden
        Frying = 2, //Braten
        Boiling = 3, //Kochen
        Baking = 4, //Backen
        DeepFrying = 5, //Frittieren
        Placeholder1 = 6,
        Placeholder2 = 7,
        Placeholder3 = 8,
        Placeholder4 = 9,
        Placeholder5 = 10,
        Placeholder6 = 11,
        Placeholder7 = 12,
    }

    public class CraftingController : ChoiceVScript {
        private static Dictionary<int, CraftingTimedSpot> CraftingTimedSpots = [];
        private static List<PlacedCraftingTimedSpot> PlacedCraftingTimedSpots = [];
        
        private static readonly Dictionary<CraftingTransformations, string> TRANSFORMATION_NAMES = new() {
            { CraftingTransformations.Combining, "Verarbeiten" },
            { CraftingTransformations.Cutting, "Schneiden" },
            { CraftingTransformations.Frying, "Braten" },
            { CraftingTransformations.Boiling, "Kochen" },
            { CraftingTransformations.Baking, "Backen" },
            { CraftingTransformations.DeepFrying, "Frittieren" },
            
            { CraftingTransformations.Placeholder1, "Platzhalter1" },
            { CraftingTransformations.Placeholder2, "Platzhalter2" },
            { CraftingTransformations.Placeholder3, "Platzhalter3" },
            { CraftingTransformations.Placeholder4, "Platzhalter4" },
            { CraftingTransformations.Placeholder5, "Platzhalter5" },
            { CraftingTransformations.Placeholder6, "Platzhalter6" },
            { CraftingTransformations.Placeholder7, "Platzhalter7" },
        };
        
        private static readonly Dictionary<CraftingTransformations, SoundController.Sounds> TRANSFORMATION_SOUNDS = new() {
            { CraftingTransformations.Cutting, SoundController.Sounds.KnifeChopping },
        };
        
        private static readonly List<CraftingTransformations> PLAYER_AVAILABLE_TRANSFORMATION = [
            CraftingTransformations.Combining,
            CraftingTransformations.Cutting,
        ];

        public CraftingController() {
            EventController.addMenuEvent("ON_PLAYER_COMBINE_ITEMS", onPlayerCombineItems);
            EventController.addMenuEvent("CRAFTING_SPOT_OPEN", onCraftingSpotOpen);
            InvokeController.AddTimedInvoke("CRAFTING_TIMED_SPOT", onTick, TimeSpan.FromSeconds(10), true);

            using(var db = new ChoiceVDb()) {
                var dbCraftingTimedSpots = db.craftingtimedspots
                    .Include(s => s.inventory)
                    .Include(s => s.configcraftingplacedtimedspots)
                    .Include(s => s.mains).ToList();
                
                foreach(var dbCraftingTimedSpot in dbCraftingTimedSpots) {
                    var transformations = dbCraftingTimedSpot.transformationsList.FromJson<List<CraftingTransformations>>();
                    var spot = new CraftingTimedSpot(dbCraftingTimedSpot.id, dbCraftingTimedSpot.name, transformations, InventoryController.loadInventory(dbCraftingTimedSpot.inventoryId), dbCraftingTimedSpot.soundIdentifier);
                    CraftingTimedSpots.Add(dbCraftingTimedSpot.id, spot);

                    foreach(var cfgSpot in dbCraftingTimedSpot.configcraftingplacedtimedspots.Concat(dbCraftingTimedSpot.mains)) {
                        var already = PlacedCraftingTimedSpots.FirstOrDefault(s => s.Id == cfgSpot.id);
                        if(already != null) {
                            already.CraftingTimedSpots.Add(spot);
                            spot.setPlacedCraftingTimedSpot(already);
                        } else {
                            var col = CollisionShape.Create(cfgSpot.collisionShape);
                            var placed = new PlacedCraftingTimedSpot(cfgSpot.id, col, [spot]);
                            PlacedCraftingTimedSpots.Add(placed);
                            spot.setPlacedCraftingTimedSpot(placed);
                        }
                    }
                }
            }

            CharacterController.addSelfMenuElement(
                new ConditionalPlayerSelfMenuElement(
                    "Crafting",
                    p => getCraftingMenu(p, PLAYER_AVAILABLE_TRANSFORMATION),
                    _ => true
                )
            );

            #region SupportStuff
            
            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    2, 
                    SupportMenuCategories.Registrieren, 
                    "Craftingspots",
                    supportCraftingSpotGenerator
                )
            );
            
            EventController.addMenuEvent("SUPPORT_CREATE_CRAFTING_SPOT", onCreateCraftingSpot); 
            EventController.addMenuEvent("SUPPORT_REMOVE_CRAFTING_SPOTS", onRemoveCraftingSpot);
            
            #endregion
        }

        private static void onTick(IInvoke obj) {
            foreach(var spot in CraftingTimedSpots) {
                spot.Value.onTick();
            }
        }

        public static Menu getCraftingMenu(IPlayer player, List<CraftingTransformations> transformations) {
            var menu = new Menu("Verarbeiten", "Was möchtest du tun?");

            using(var db = new ChoiceVDb()) {
                var configItemIds = player.getInventory().getAllItems().Select(i => i.ConfigId).ToList();

                foreach(var transformation in transformations) {
                    var (meetsRequirement, _, failMessage) = areTransformationRequirementsMet(transformation, player.getInventory());
                    if(!meetsRequirement) {
                        menu.addMenuItem(new StaticMenuItem(TRANSFORMATION_NAMES[transformation], failMessage, "", MenuItemStyle.yellow));
                        continue;
                    }
                    
                    var combMenu = new Menu(TRANSFORMATION_NAMES[transformation], "Was möchtest du verarbeiten?");
                    foreach(var combination in db.configcraftingtransformations
                                .Include(t => t.configcraftingtransformationsitems)
                                .ThenInclude(t => t.item)
                                .Where(c => c.transformationType == (int)transformation && c.configcraftingtransformationsitems.Any(i => i.isInput && configItemIds.Contains(i.itemId)))) {
                        
                        var outputItems = combination.configcraftingtransformationsitems.Where(i => !i.isInput).ToList();
                        var inputItems = combination.configcraftingtransformationsitems.Where(i => i.isInput).ToList();
                        
                        var outputStr = $"{outputItems.First().amount}x {outputItems.First().item.name}";
                        if(outputItems.Count > 1) {
                            outputStr = $"{outputItems.Count} Ausgaben";
                        }

                        var inputStr = $"{inputItems.First().amount}x {inputItems.First().item.name}";
                        foreach(var input in inputItems.Skip(1)) {
                            inputStr += $", {input.amount}x {input.item.name}";
                        }

                        var outputCombineItem = outputItems.FirstOrDefault(i => i.item.codeItem == nameof(FoodCombinedItem)); 
                        if(outputCombineItem == null) {
                                 var allItemsAvailable = false;
                            foreach(var input in inputItems) {
                                if(player.getInventory().hasItems<Item>(i => i.ConfigId == input.itemId, input.amount)) {
                                    allItemsAvailable = true;
                                } else {
                                    allItemsAvailable = false;
                                    break;
                                }
                            }

                            if(allItemsAvailable) {
                                combMenu.addMenuItem(new ClickMenuItem($"Verarbeite zu {outputStr}", $"Verarbeite {inputStr} zu {outputStr}", "", "ON_PLAYER_COMBINE_ITEMS")
                                    .withData(new Dictionary<string, dynamic> {
                                        { "CombinationId", combination.id },
                                    }));
                            } else if(combination.showIfNotAllIngredients) {
                                combMenu.addMenuItem(new StaticMenuItem($"Verarbeite zu {outputStr}",
                                    $"Verarbeite {inputStr} zu {outputStr} aktuell nicht möglich. Du benötigst nicht alle benötigen Waren!", "", MenuItemStyle.yellow));
                            }     
                        } else {
                            var inputOptions = player.getInventory().getItems<FoodItem>(i => i.SpecialFoodType.HasFlag(SpecialFoodType.CombineableItem)).ToList(); 
                           
                            var combSubmenu = new Menu($"Verarbeite zu {outputStr}", "Was möchtest du hinzufügen?"); 
                            if(inputOptions.Count == 0) {
                                combSubmenu.addMenuItem(new StaticMenuItem("Keine Zutaten dabei", "Du hast keine weiteren Zutaten dabei", "", MenuItemStyle.yellow));
                            } else {
                                combSubmenu.addMenuItem(new StaticMenuItem("Zutaten zum Hinzufügen auswählen", $"Wähle die Zutaten aus die du zu deinem/deiner {outputCombineItem.item.name} hinzufügen möchtest.", ""));
                                foreach(var inputOption in inputOptions.OrderBy(i => i.Name)) {
                                    combSubmenu.addMenuItem(new CheckBoxMenuItem(inputOption.Name, $"Füge {inputOption.Name} hinzu", false, ""));
                                }
                            }

                            combSubmenu.addMenuItem(new MenuStatsMenuItem($"Verarbeite zu {outputCombineItem.item.name}", "Kombiniere die ausgewählten Items", "", "ON_PLAYER_COMBINE_ITEMS", MenuItemStyle.green)
                                .withData(new Dictionary<string, dynamic> {
                                    { "CombinationId", combination.id },
                                }));
                            
                            combMenu.addMenuItem(new MenuMenuItem(combSubmenu.Name, combSubmenu, $"Verarbeite {inputStr} zu {outputStr}. Du kannst noch weitere Zutaten hinzufügen!", ">"));
                        }
                    }

                    menu.addMenuItem(new MenuMenuItem(combMenu.Name, combMenu));
                }
            }

            return menu;
        }
        
        public static int createCraftingTimedSpot(int ownerId, string name, float inventorySize, string soundIdentifier, List<CraftingTransformations> transformations) {
            var inventory = InventoryController.createInventory(ownerId, inventorySize, InventoryTypes.CraftingSpot);
            using(var db = new ChoiceVDb()) {
                var craftingSpot = new craftingtimedspot {
                    inventoryId = inventory.Id,
                    name = name,
                    soundIdentifier = soundIdentifier,
                    transformationsList = transformations.ToJson(),
                };
                db.craftingtimedspots.Add(craftingSpot);
                db.SaveChanges();
                
                CraftingTimedSpots.Add(craftingSpot.id, new CraftingTimedSpot(craftingSpot.id, name, transformations, inventory, soundIdentifier));
                
                return craftingSpot.id;
            }
        }
        
        public static bool removeCraftingTimedSpot(int spotId) {
            if(CraftingTimedSpots.TryGetValue(spotId, out var spot)) {
                using(var db = new ChoiceVDb()) {
                    var dbSpot = db.craftingtimedspots.First(s => s.id == spotId);
                    db.craftingtimedspots.Remove(dbSpot);
                    db.SaveChanges();
                }
                
                InventoryController.destroyInventory(spot.Inventory);
                
                CraftingTimedSpots.Remove(spotId);
                
                return true;
            } else {
                return false;
            }
        }

        public static void openCraftingSpotMenu(IPlayer player, int spotId) {
            if(CraftingTimedSpots.TryGetValue(spotId, out var spot)) {
                var menu = new Menu(spot.Name, "Was möchtest du tun?");

                menu.addMenuItem(new ClickMenuItem("Öffnen", $"Öffne den/das {spot.Name}", spot.getSpotTransformationProgressString(), "CRAFTING_SPOT_OPEN")
                    .withUpdateIdentifier($"CRAFTING_SPOT_OPEN_{spot.Id}")
                    .withData(new Dictionary<string, dynamic> {
                        { "Spot", spot },
                    })
                );
                
                var spotTransformations = spot.Transformations.Select(t => (int)t).ToList();

                var combinationsMenu = new Menu("Mögliche Verarbeitungen", "Siehe dir mögliche Verarbeitungen an");
                addPossibleTransformationsToMenu(combinationsMenu, player.getInventory(), spotTransformations);
                menu.addMenuItem(new MenuMenuItem(combinationsMenu.Name, combinationsMenu));
                
                player.showUpdatingMenu(menu, "CRAFTING_SPOT_MENU");
            } 
        }
        public static void openCraftingSpotsMenu(IPlayer player, List<int> spotIds) {
            if(spotIds.All(s => CraftingTimedSpots.ContainsKey(s))) {
                var spots = spotIds.Select(s => CraftingTimedSpots[s]).ToList();
                
                var menu = new Menu(spots.First().Name, "Was möchtest du tun?");
                foreach(var spot in spots) {
                    menu.addMenuItem(new ClickMenuItem(spot.Name, $"Öffne den/das {spot.Name}", spot.getSpotTransformationProgressString(), "CRAFTING_SPOT_OPEN")
                        .withUpdateIdentifier($"CRAFTING_SPOT_OPEN_{spot.Id}")
                        .withData(new Dictionary<string, dynamic> {
                            { "Spot", spot },
                        })
                    );
                }
                var combinationsMenu = new Menu("Mögliche Verarbeitungen", "Siehe dir mögliche Verarbeitungen an");
                addPossibleTransformationsToMenu(combinationsMenu, player.getInventory(), spots.SelectMany(s => s.Transformations).Distinct().ToList());
                menu.addMenuItem(new MenuMenuItem(combinationsMenu.Name, combinationsMenu));  
                
                player.showUpdatingMenu(menu, "CRAFTING_SPOT_MENU");
            } 
        }

        private static void addPossibleTransformationsToMenu(Menu menu, Inventory inventory, List<int> transformations) { 
            var configItemIds = inventory.getAllItems().Select(i => i.ConfigId).ToList();
            
            using(var db = new ChoiceVDb()) {
                foreach(var combination in db.configcraftingtransformations
                            .Include(t => t.configcraftingtransformationsitems)
                            .ThenInclude(t => t.item)
                            .Where(c => transformations.Contains(c.transformationType) && c.configcraftingtransformationsitems.Any(i => i.isInput && configItemIds.Contains(i.itemId)))) {
                    var transformationName = TRANSFORMATION_NAMES[(CraftingTransformations)combination.transformationType];
                    var outputItems = combination.configcraftingtransformationsitems.Where(i => !i.isInput).ToList();
                    var inputItems = combination.configcraftingtransformationsitems.Where(i => i.isInput).ToList();

                    var outputStr = $"{outputItems.First().amount}x {outputItems.First().item.name}";
                    if(outputItems.Count > 1) {
                        outputStr = $"{outputItems.Count} Ausgaben";
                    }

                    var inputStr = $"{inputItems.First().amount}x {inputItems.First().item.name}";
                    foreach(var input in inputItems.Skip(1)) {
                        inputStr += $", {input.amount}x {input.item.name}";
                    }

                    var timeStr = "";
                    if(combination.craftingProcessTime != null) {
                        if(combination.craftingProcessTime > 60) {
                            timeStr = $"{Math.Round((float)combination.craftingProcessTime / 60f, 1)} min";
                        } else {
                            timeStr = $"{combination.craftingProcessTime} sek";
                        }
                    }

                    menu.addMenuItem(new StaticMenuItem($"{transformationName} zu {outputStr} ({timeStr})", $"Verarbeite {inputStr} zu {outputStr}. Dies dauert ca. {timeStr}", ""));
                }
            }
        }

        internal static (bool meetsRequirement, Item potentialUseItem, string failMessage) areTransformationRequirementsMet(CraftingTransformations transformation, Inventory inventory, IPlayer player = null) {
            if(transformation == CraftingTransformations.Combining) {
                return (true, null, null);
            } 
            
            if(transformation == CraftingTransformations.Cutting) {
                var requiredTool = inventory.getItem<ToolItem>(i => i.Flag == SpecialToolFlag.Knife);

                if(requiredTool == null) {
                    return (false, null, "Du benötigst ein Messer um zu schneiden!");
                }

                return (true, requiredTool, null);
            }
            
            return (true, null, "Diese Transformation ist aktuell nicht implementiert!");
        }

        private static bool onPlayerCombineItems(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {  
            var combinationId = (int)data["CombinationId"];

            var list = new List<Item>();
            if(menuitemcefevent is MenuStatsMenuItem.MenuStatsMenuItemEvent evt) {
                var inputItems = evt.elements.Skip(1).Select(e => e.FromJson<CheckBoxMenuItem.CheckBoxMenuItemEvent>()).ToList();
                foreach(var option in inputItems.SkipLast(1)) {
                    if(!option.check) continue;
                    
                    var item = player.getInventory().getItem<Item>(i => i.Name == option.name);
                    
                    if(item != null) {
                        list.Add(item);
                    }
                }
            }
            
            using(var db = new ChoiceVDb()) {
                var combination = db.configcraftingtransformations
                    .Include(t => t.configcraftingtransformationsitems)
                    .ThenInclude(t => t.item)
                    .First(c => c.id == combinationId);

                var anim = AnimationController.getAnimationByName("WORK_FRONT");
                if(combination.animationIdentifier != null) {
                    anim = AnimationController.getAnimationByName(combination.animationIdentifier);
                }
                
                if(TRANSFORMATION_SOUNDS.ContainsKey((CraftingTransformations)combination.transformationType)) {
                    SoundController.playSoundAtCoords(player.Position, 7.5f, TRANSFORMATION_SOUNDS[(CraftingTransformations)combination.transformationType], 1f, "mp3");
                }
                
                AnimationController.animationTask(player, anim, () => {
                    var (meetsRequirement, potentialUseItem, failMessage) = areTransformationRequirementsMet((CraftingTransformations)combination.transformationType, player.getInventory(), player);
                    if(!meetsRequirement) {
                        player.sendBlockNotification(failMessage, $"Du erfüllst die Bedingung nicht: {failMessage}", Constants.NotifactionImages.Crafting);
                        return;
                    }
                                    
                    potentialUseItem?.use(player);
                    
                    if(processCraftingTransformation(combination, player.getInventory(), list)) {
                        player.sendNotification(Constants.NotifactionTypes.Success,
                            $"Du hast die Verarbeitung erfolgreich durchgeführt!",
                            "Erfolgreich verarbeitet", Constants.NotifactionImages.Crafting);
                    } else {
                        player.sendBlockNotification(
                            $"Du hast nicht alle benötigten Items um diese Transformation durchzuführen!", "Fehler beim Verarbeiten", Constants.NotifactionImages.Crafting);
                    }
                });
            }

            return true;
        }

        public static bool processCraftingTransformation(configcraftingtransformation transformation, Inventory inventory, List<Item> selectedCombineItems = null) {
            var outputItems = transformation.configcraftingtransformationsitems.Where(i => !i.isInput).ToList();
            var inputItems = transformation.configcraftingtransformationsitems.Where(i => i.isInput).ToList();

            var allItemsAvailable = false;
            foreach(var input in inputItems) {
                if(inventory.hasItems<Item>(i => i.ConfigId == input.itemId, input.amount)) {
                    allItemsAvailable = true;
                } else {
                    allItemsAvailable = false;
                    break;
                }
            }

            if(allItemsAvailable) {
                foreach(var combineItem in selectedCombineItems ?? []) {
                    if(!inventory.removeSimelarItems(combineItem, 1)) {
                        return false;
                    }
                }
                
                foreach(var input in inputItems) {
                    var item = inventory.getItem(i => i.ConfigId == input.itemId);
                    if(!inventory.removeSimelarItems(item, input.amount)) {
                        return false;
                    }
                }

                foreach(var output in outputItems) {
                    var newItems = InventoryController.createItems(output.item, output.amount, -1, new Dictionary<string, dynamic> {
                        { "CombineItems", selectedCombineItems },
                    });
                    inventory.addItems(newItems, true);
                }
            }

            return true;
        }

        private static bool onCraftingSpotOpen(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var spot = (CraftingTimedSpot)data["Spot"];
            InventoryController.showMoveInventory(player, player.getInventory(), spot.Inventory, null, null, spot.Name, true);
            return true;
        }

        #region SupportStuff
        
        private bool onRemoveCraftingSpot(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var spots = (List<CraftingTimedSpot>)data["SpotId"];
            foreach(var spot in spots) {
                if(removeCraftingTimedSpot(spot.Id)) {
                    var placedSpots = PlacedCraftingTimedSpots.Where(s => s.CraftingTimedSpots.Any(s => s.Id == spot.Id)).ToList();
                    if(placedSpots.Count == 0) continue;
                    
                    foreach(var placedSpot in placedSpots) {
                        placedSpot.CollisionShape.Dispose();
                        PlacedCraftingTimedSpots.Remove(placedSpot);
                    } 
                    player.sendNotification(Constants.NotifactionTypes.Info, "Craftingspot entfernt!", "Craftingspot entfernt", Constants.NotifactionImages.Crafting);
                } else {
                    player.sendNotification(Constants.NotifactionTypes.Danger, "Craftingspot nicht gefunden!", "Craftingspot nicht gefunden", Constants.NotifactionImages.Crafting);
                }    
            }

            return true;
        }

        private Menu supportCraftingSpotGenerator(IPlayer player) {
            var menu = new Menu("Craftingspots", "Was möchtest du tun?");

            var addSpotMenu = new Menu("Craftingspot hinzufügen", "Was möchtest du tun?");
            addSpotMenu.addMenuItem(new InputMenuItem("Name", "Wie soll der Craftingspot heißen?", "", ""));
            addSpotMenu.addMenuItem(new InputMenuItem("Inventargröße", "Wie groß soll das Inventar sein?", "", InputMenuItemTypes.number, ""));
            addSpotMenu.addMenuItem(new InputMenuItem("Sound", "Welcher Sound soll abgespielt werden?", "", "")
                .withOptions(SoundController.getAllSoundNames().ToArray()));
            
            var transformationsMenu = new Menu("Transformationen", "Welche Transformationen soll der Spot haben?");
            foreach(var transformation in Enum.GetValues(typeof(CraftingTransformations))) {
                transformationsMenu.addMenuItem(new CheckBoxMenuItem(TRANSFORMATION_NAMES[(CraftingTransformations)transformation], "", false, ""));
            }
            addSpotMenu.addMenuItem(new MenuMenuItem(transformationsMenu.Name, transformationsMenu));
            
            addSpotMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle den Craftingspot", "SUPPORT_CREATE_CRAFTING_SPOT", MenuItemStyle.green));
            menu.addMenuItem(new MenuMenuItem(addSpotMenu.Name, addSpotMenu, MenuItemStyle.green));
            
            foreach(var spot in PlacedCraftingTimedSpots) {
                var spotMenu = new Menu(spot.CraftingTimedSpots.First().Name, "Was möchtest du tun?");
                #region AdditionalSpots
                var alreadyAddSpots = new Menu("Craftingspots", "Was möchtest du tun?");
                foreach(var alreadySpot in spot.CraftingTimedSpots) {
                    var alreadySpotMenu = new Menu(alreadySpot.Name, "Was möchtest du tun?");
                    alreadySpotMenu.addMenuItem(new StaticMenuItem("Position", spot.CollisionShape.Position.ToString(), ""));
                    alreadySpotMenu.addMenuItem(new StaticMenuItem("Sound", alreadySpot.SoundIdentifier, ""));
                    alreadySpotMenu.addMenuItem(new StaticMenuItem("Transformationen", string.Join(",", alreadySpot.Transformations.Select(t => TRANSFORMATION_NAMES[(CraftingTransformations)t]).ToList()), ""));
                    alreadyAddSpots.addMenuItem(new MenuMenuItem(alreadySpotMenu.Name, alreadySpotMenu));
                }
                spotMenu.addMenuItem(new MenuMenuItem(alreadyAddSpots.Name, alreadyAddSpots));
                
                var additionalSpotMenu = new Menu("Craftingspot hinzufügen", "Was möchtest du tun?");
                additionalSpotMenu.addMenuItem(new InputMenuItem("Name", "Wie soll der Craftingspot heißen?", "", ""));
                additionalSpotMenu.addMenuItem(new InputMenuItem("Inventargröße", "Wie groß soll das Inventar sein?", "", InputMenuItemTypes.number, ""));
                additionalSpotMenu.addMenuItem(new InputMenuItem("Sound", "Welcher Sound soll abgespielt werden?", "", "")
                    .withOptions(SoundController.getAllSoundNames().ToArray()));
            
                var addTransformationsMenu = new Menu("Transformationen", "Welche Transformationen soll der Spot haben?");
                foreach(var transformation in Enum.GetValues(typeof(CraftingTransformations))) {
                    addTransformationsMenu .addMenuItem(new CheckBoxMenuItem(TRANSFORMATION_NAMES[(CraftingTransformations)transformation], "", false, ""));
                }
                additionalSpotMenu.addMenuItem(new MenuMenuItem(addTransformationsMenu.Name, addTransformationsMenu));
                additionalSpotMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle den Craftingspot", "SUPPORT_CREATE_CRAFTING_SPOT", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> {
                        { "PlacedSpot", spot },
                    })
                );
                alreadyAddSpots.addMenuItem(new MenuMenuItem(additionalSpotMenu.Name, additionalSpotMenu, MenuItemStyle.green));
                
                #endregion
                
                spotMenu.addMenuItem(new ClickMenuItem("Öffnen", $"Öffne den/das Spot", "", "SUPPORT_CRAFTING_SPOT_OPEN") );
                
                spotMenu.addMenuItem(new ClickMenuItem("Entfernen", $"Entferne den/das {spot.CraftingTimedSpots.First().Name}", "", "SUPPORT_REMOVE_CRAFTING_SPOTS", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> {
                        { "SpotId", spot.CraftingTimedSpots },
                    })
                    .needsConfirmation("Craftingspot entfernen", "Spot wirklich entfernen?")
                );
                menu.addMenuItem(new MenuMenuItem(spotMenu.Name, spotMenu));
            }
            
            return menu;
        }

        private bool onCreateCraftingSpot(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;
            var nameEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            var sizeEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();
            var soundEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();
            
            var transformations = new List<CraftingTransformations>();
            var transformationOptions = Enum.GetValues(typeof(CraftingTransformations));
            for (var i = 3; i < evt.elements.Length - 1; i++) {
                var transformationEvt = evt.elements[i].FromJson<CheckBoxMenuItem.CheckBoxMenuItemEvent>();
                if(transformationEvt.check) {
                    transformations.Add((CraftingTransformations)transformationOptions.GetValue(i - 3));
                }
            }

            var sound = SoundController.getSoundIdentifier(soundEvt.input);               
            if(data.TryGetValue("PlacedSpot", out var value)) {
                var placedSpot = (PlacedCraftingTimedSpot)value;
                var spotId = createCraftingTimedSpot(-1, nameEvt.input, float.Parse(sizeEvt.input), sound, transformations);
                using(var db = new ChoiceVDb()) {
                    var already = db.configcraftingplacedtimedspots.Find(placedSpot.Id);
                    var alreadyCreated = db.craftingtimedspots.Find(spotId);
                    
                    already.spots.Add(alreadyCreated);
                    db.SaveChanges();
                }
                
                var spot = CraftingTimedSpots[spotId];
                placedSpot.CraftingTimedSpots.Add(spot);
                spot.setPlacedCraftingTimedSpot(placedSpot);
                player.sendNotification(Constants.NotifactionTypes.Success, "Craftingspot hinzugefügt!", "Craftingspot hinzugefügt", Constants.NotifactionImages.Crafting);
            } else {
                CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                    var spotId = createCraftingTimedSpot(-1, nameEvt.input, float.Parse(sizeEvt.input), sound, transformations);
                    using(var db = new ChoiceVDb()) {
                        var coll = CollisionShape.Create(p, w, h, r, true, false, true);
                        var dbSpot = db.craftingtimedspots.Find(spotId);
                        var placedSpot = new configcraftingplacedtimedspot {
                            collisionShape = coll.toShortSave(),
                            spotId = spotId,
                        };
                        dbSpot.configcraftingplacedtimedspots.Add(placedSpot);
                        db.SaveChanges();

                        var spot = CraftingTimedSpots[spotId];
                        var placed = new PlacedCraftingTimedSpot(placedSpot.id, coll, [spot]);
                        PlacedCraftingTimedSpots.Add(placed);
                        spot.setPlacedCraftingTimedSpot(placed);
                    }

                    player.sendNotification(Constants.NotifactionTypes.Success, "Craftingspot erstellt!", "Craftingspot erstellt", Constants.NotifactionImages.Crafting);
                });
            }

            return true; 
        }

        #endregion
    }
}