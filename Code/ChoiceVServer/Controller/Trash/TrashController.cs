using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Model;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ChoiceVServer.Controller {
    internal record Trash(int DbTrashCanId, InventorySpot Spot);
    public class TrashController : ChoiceVScript {
        public enum TrashTypes {
            Plastic,
            Metal,
            Paper,
            Glass,
            Organic,
            ResidualWaste,
            HazardousWaste,
        }
        
        private static List<TrashZone> TrashZones = new List<TrashZone>();
        private static List<Trash> AllTrash = new List<Trash>();
        private static List<uint> GarbageCansHashes = new List<uint>();

        private const int MAX_TRASH_ITEM_AMOUNT = 10;
        private const int MAX_BAGS_AMOUNT = 6;

        private const int MIN_TRASHCAN_FOR_ZONE = 100;
        private const float MIN_TRASH_GAIN_PER_HOUR = 1 / 168f;
        private const float MAX_TRASH_GAIN_PER_HOUR = 1 / 72f;
        
        private static TimeSpan TRASH_REMOVE_TIME = TimeSpan.FromMinutes(6);
        public TrashController() {
            EventController.MainReadyDelegate += onMainReady;

            Inventory.InventoryEmptyAfterMoveDelegate += onInventoryEmpty;

            InvokeController.AddTimedInvoke("Trash-Remover", updateTrash, TRASH_REMOVE_TIME, true);

            InteractionController.addObjectInteractionCallback(
                "OPEN_TRASHCAN",
                "Mülleimer öffnen",
                onOpenTrashCan
             );

            InteractionController.addObjectInteractionCallback(
                "OPEN_TRASH_BAG",
                "Müllsack öffnen",
                onOpenTrashBag
            );
            
            EventController.addMenuEvent("OPEN_TRASH_CAN", openTrashCan);
            EventController.addMenuEvent("EMPTY_TRASH_CAN", emptyTrashCan);
        }

        private void updateTrash(IInvoke ivk) {
            var toRemove = new List<Trash>();
            foreach(var trash in AllTrash) {
                var spot = trash.Spot;
                spot.loadInventory();

                if(spot.Inventory != null) {
                    var weapon = spot.Inventory.Items.Distinct().FirstOrDefault(i => i.Type == ItemTypes.Weapon);
                    if(spot.Inventory.getCount() <= 0) {
                        toRemove.Add(trash);
                    } else {
                        if(weapon != null) {
                            if(spot.CreateDate + Constants.WEAPON_TRASH_REMOVE_TIME < DateTime.Now) {
                                toRemove.Add(trash);
                            }
                        } else {
                            if(spot.CreateDate + Constants.STANDARD_TRASH_REMOVE_TIME < DateTime.Now) {
                                toRemove.Add(trash);
                            }
                        }
                    }
                }
            }

            using(var db = new ChoiceVDb()) {
                foreach(var zone in TrashZones) {
                    var random = new Random();
                    var change = (float)random.NextDouble() * (zone.MaxGain * MAX_TRASH_GAIN_PER_HOUR - zone.MinGain * MIN_TRASH_GAIN_PER_HOUR) + zone.MinGain * MIN_TRASH_GAIN_PER_HOUR;
                    zone.FillLevel += change * (float)TRASH_REMOVE_TIME.TotalHours;
                    zone.FillLevel = Math.Clamp(zone.FillLevel, 0, 1);
                    
                    var dbZone = db.configtrashzones.FirstOrDefault(t => t.zoneIdentifier == zone.ZoneCollectionIdentifier);
                    if(dbZone != null) {
                        dbZone.fillLevel = zone.FillLevel;
                    }
                }     
                
                db.SaveChanges();
            }
            
            foreach(var spot in toRemove) {
                removeSpot(spot);
            }
        }

        private void onMainReady() {
            AllTrash = new();
            using(var db = new ChoiceVDb()) {
                foreach(var trashSpot in InventorySpot.getListByPredicate(s => s.Type == InventorySpotType.Trash)) {
                    var dbTrash = db.configtrashcans.FirstOrDefault(t => t.position == trashSpot.CollisionShape.Position.ToJson());

                    if(dbTrash != null) {
                        AllTrash.Add(new Trash(dbTrash.id, trashSpot));
                    } else {
                        AllTrash.Add(new Trash(-1, trashSpot));
                    }
                }

                var trashCanPerZone = new Dictionary<string, float>();
                foreach(var trashZone in db.configtrashzones.Include(t => t.zoneIdentifierNavigation)) {
                    TrashZones.Add(new TrashZone(trashZone.zoneIdentifier, trashZone.zoneIdentifierNavigation.groupingName, trashZone.minGain, trashZone.maxGain, trashZone.fillLevel));
                }

                var hashSet = new HashSet<string>();
                foreach(var dbCan in db.configtrashcans) {
                    if(!hashSet.Contains(ChoiceVAPI.Hash(dbCan.objectName).ToString())) {
                        hashSet.Add(ChoiceVAPI.Hash(dbCan.objectName).ToString());
                    }

                    var bigRegion = WorldController.getBigRegionName(dbCan.position.FromJson());
                    
                    if(bigRegion == "OCEANA") {
                        Logger.logError("ERROR IN ZONE FILE!");
                        throw new Exception("Zone file is not correct!");
                    }
                       
                    if(trashCanPerZone.ContainsKey(bigRegion)) {
                        trashCanPerZone[bigRegion]++;
                    } else {
                        trashCanPerZone.Add(bigRegion, 1);
                    }
                }
                InteractionController.addInteractableObjects(hashSet.ToList(), "OPEN_TRASHCAN");

                var maxZone = trashCanPerZone.Max(t => t.Value);
                var minZone = trashCanPerZone.Where(e => e.Value > MIN_TRASHCAN_FOR_ZONE).Min(t => t.Value);
                foreach(var zone in trashCanPerZone) {
                    if(zone.Key == "NO_REGION" ||
                        zone.Value < MIN_TRASHCAN_FOR_ZONE) {
                        continue;
                    }
                   
                    var dbZone = db.configtrashzones.FirstOrDefault(t => t.zoneIdentifier == zone.Key);
                    var minGain = minZone / zone.Value + 0.5f;
                    var maxGain = (zone.Value / maxZone + 0.4f) * 1.2f;
                    if(dbZone != null) {
                        dbZone.minGain = minGain;
                        dbZone.maxGain = maxGain;
                    } else {
                        db.configtrashzones.Add(new configtrashzone {
                            zoneIdentifier = zone.Key,
                            minGain = minGain,
                            maxGain = maxGain,
                            fillLevel = 0f,
                        });
                    } 
                }

                db.SaveChanges();
            }
            
            GarbageCansHashes = InteractionController.AllConfigObjects.Values.Where(o => o.codeFunctionOrIdentifier == "OPEN_TRASHCAN").Select(o => o.modelName != null ? ChoiceVAPI.Hash(o.modelName) : uint.Parse(o.modelHash)).ToList();
        }

        public static void playerThrowItemAway(IPlayer player, Item item, int amount) {
            var vec = player.getForwardVector();

            if(!player.isRestricted() && !player.getBusy() && WorldController.getWorldGrid(player.Position) != null && (player.Dimension == Constants.GlobalDimension || player.Dimension == Constants.IslandDimension)) {
                CallbackController.getNearbyObject(player, player.Position, Constants.MAX_TRASH_CAN_DISTANCE, GarbageCansHashes, (player, found, hash, pos, heading) => {
                    if(found) {
                        createNewTrash(player, false, pos, item, amount, heading);
                    } else {
                        CallbackController.getGroundZFromPos(player, new Position(player.Position.X + (vec.X * 1.5f), player.Position.Y + (vec.Y * 1.5f), player.Position.Z), (p, z, inWall, inObject) => {
                            if(inWall || inObject) {
                                player.sendBlockNotification("Du kannst hier keinen Müll hinlegen!", "Nicht ablegbar!");
                                return;
                            }

                            createNewTrash(player, true, new Position(player.Position.X + vec.X, player.Position.Y + vec.Y, z - 0.05f), item, amount);
                        });
                    }
                });
            } else {
                player.sendBlockNotification("Du darfst hier nichts wegwerfen!", "Wegwerfen verboten");
            }
        }

        private static void createNewTrash(IPlayer player, bool withObj, Position position, Item item, int amount, float heading = 0) {
            foreach(var already in AllTrash) {
                var alreadySpot = already.Spot;
                if(alreadySpot.CollisionShape == null) {
                    continue;
                }

                if(Vector3.Distance(alreadySpot.CollisionShape.Position, player.Position) < Constants.MAX_TRASH_DISTANCE || Vector3.Distance(alreadySpot.CollisionShape.Position, position) < Constants.MAX_TRASH_DISTANCE) {
                    alreadySpot.loadInventory();
                    player.getInventory().moveItems(alreadySpot.Inventory, item, amount);
                    return;
                }
            }

            InventorySpot spot;
            if(withObj) {
                var obj = ObjectController.createObject("ng_proc_binbag_01a", position, Rotation.Zero, 200, false);
                spot = InventorySpot.createWithObject(obj, InventorySpotType.Trash, "Müllsack", position, 1.5f, 1.5f, heading, 100, null);
            } else {
                spot = InventorySpot.create(InventorySpotType.Trash, "Mülleimer", position, 1.5f, 1.5f, heading, 200, null);
            }

            AllTrash.Add(new Trash(-1, spot));

            SoundController.playSoundAtCoords(position, 2, SoundController.Sounds.BagRustle, 0.1f);
            var anim = AnimationController.getAnimationByName("THROW_AWAY");
            player.playAnimation(anim);
            if(!player.getInventory().moveItems(spot.Inventory, item, amount)) {
                spot.remove();
                return;
            }

            var itemStr = "Unbekanntes Objekt";
            if(item.Weight > 1) {
                itemStr = item.Name;
            }

            CamController.checkIfCamSawAction(player, "Objekt weggeworfen", $"Die Person hat {amount}x {itemStr} weggeworfen.");
        }

        private void onOpenTrashCan(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading, bool isBroken, ref Menu menu) {
            if(!isBroken) {
                using(var db = new ChoiceVDb()) {
                    var dbTrashCan = db.configtrashcans.FirstOrDefault(t => t.position == objectPosition.ToJson());
                    if(dbTrashCan != null) {
                        var already = AllTrash.FirstOrDefault(s => s.Spot.Object == null && objectPosition.Distance(s.Spot.CollisionShape.Position) < Constants.MAX_TRASH_DISTANCE);
                        if(already != null) {
                            if(player.getInventory().hasItem<StaticItem>(i => i.ConfigItem.codeIdentifier == "TRASHBAG") && already.Spot.Inventory.getItems<TrashItem>(i => true).Sum(i => i.StackAmount ?? 1) >= 3) {
                                menu.addMenuItem(new ClickMenuItem("Mülleimer öffnen", "Öffne den Müllsack", "", "OPEN_TRASH_CAN")
                                    .withData(new Dictionary<string, dynamic> { { "TrashCan", already } }));
                                menu.addMenuItem(new ClickMenuItem("Mülleimer in Sack leeren", "Leere den Mülleimer in einen Müllsack", "", "EMPTY_TRASH_CAN")
                                    .withData(new Dictionary<string, dynamic> { { "TrashCan", already } }));
                            } else {
                                already.Spot.showInventorySpotOpen(player, player.getInventory(), (p, fI, tI, i, a) => onMoveItemFromTrashCan(p, already.Spot, fI, tI, i, a));
                            }
                        } else {
                            generateTrashCanInventory(player, objectPosition, objectHeading, dbTrashCan);
                        }
                    } else {
                        player.sendBlockNotification("Dieser Mülleimer wurde nicht gefunden! Wurde er verschoben oder ist er umgekippt? Benutze bitte eine anderen.", "Mülleimer nicht gefunden");
                    }
                }
            } else {
                player.sendBlockNotification("Der Mülleimer ist kaputt. Du kannst ihn nicht benutzen!", "Objekt kaputt");
            }
        }
        
        private bool openTrashCan(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var trash = data["TrashCan"] as Trash;
            if(trash != null) {
                trash.Spot.showInventorySpotOpen(player, player.getInventory(), (p, fI, tI, i, a) => onMoveItemFromTrashCan(p, trash.Spot, fI, tI, i, a));
            }

            return true;
        }
        
        private bool emptyTrashCan(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var trash = data["TrashCan"] as Trash;
            var bagItem = player.getInventory().getItem<StaticItem>(i => i.ConfigItem.codeIdentifier == "TRASHBAG");
            var freeSpace = player.getInventory().FreeWeight;
            var trashBags = InventoryController.getConfigItemsForType<TrashBag>(i => true);
            
            if(freeSpace < trashBags.Max(t => t.weight)) {
                player.sendBlockNotification("Du hast nicht genug Platz für einen gefüllten Müllsack!", "Nicht genug Platz");
                return true;
            }
           
            var anim = AnimationController.getAnimationByName("WORK_FRONT");
            var rotation = player.getRotationTowardsPosition(trash.Spot.CollisionShape.Position);
            AnimationController.animationTask(player, anim, () => {
                if(bagItem != null) {
                    var trashItemAmount = trash.Spot.Inventory.getItems(i => i is TrashItem).Sum(i => i.StackAmount);

                    if(trashItemAmount < 2) {
                        player.sendBlockNotification("Es ist nicht genug Müll im Mülleimer um einen Müllsack zu füllen!", "Nicht genug Müll");
                        return;
                    }

                    var counter = 0;
                    for (var i = 0; i <= 9 - counter; i++) {
                        var item = trash.Spot.Inventory.getItem<TrashItem>(i => true);
                        if(item != null) {
                            var maxAmount = Math.Min(9 - counter, item.StackAmount ?? 1);
                            if(trash.Spot.Inventory.removeItem(item, maxAmount)) {
                                counter += maxAmount;
                            }
                        }
                    }

                    bagItem.use(player);
                    if(counter <= 4) {
                        player.getInventory().addItem(new TrashBag(InventoryController.getConfigItemByCodeIdentifier("SMALL_TRASHBAG"), 1, -1));
                        player.sendNotification(Constants.NotifactionTypes.Success, "Du hast einen kleinen Müllsack erhalten!", "Müllsack erhalten", Constants.NotifactionImages.Package);
                        changeBagFillFromZone(player, getTrashZoneForPosition(player.Position), TrashBagSize.Small, true);
                    } else if(counter <= 7) {
                        player.getInventory().addItem(new TrashBag(InventoryController.getConfigItemByCodeIdentifier("MEDIUM_TRASHBAG"), 1, -1));
                        player.sendNotification(Constants.NotifactionTypes.Success, "Du hast einen mittleren Müllsack erhalten!", "Müllsack erhalten", Constants.NotifactionImages.Package);
                        changeBagFillFromZone(player, getTrashZoneForPosition(player.Position), TrashBagSize.Medium, true);
                    } else {
                        player.getInventory().addItem(new TrashBag(InventoryController.getConfigItemByCodeIdentifier("BIG_TRASHBAG"), 1, -1));
                        player.sendNotification(Constants.NotifactionTypes.Success, "Du hast einen großen Müllsack erhalten!", "Müllsack", Constants.NotifactionImages.Package);
                        changeBagFillFromZone(player, getTrashZoneForPosition(player.Position), TrashBagSize.Large, true);
                    }
                }
            }, rotation, true, 1);
            
            return true;
        }

        private TrashZone getTrashZoneForPosition(Position position) {
            var zone = WorldController.getBigRegionName(position);
            return TrashZones.FirstOrDefault(t => t.ZoneCollectionIdentifier == zone);
        }

        private void generateTrashCanInventory(IPlayer player, Position objectPosition, float objectHeading, configtrashcan dbTrashCan) {
            var spot = InventorySpot.create(InventorySpotType.Trash, "Mülleimer", objectPosition, 1.5f, 1.5f, objectHeading, 200, null);
            AllTrash.Add(new Trash(dbTrashCan.id, spot));
           
            var trashZone = getTrashZoneForPosition(objectPosition);
            if(trashZone == null) {
                return;
            }
            
            var random = new Random();
            var itemsAmount = random.Next(0, (int)(MAX_TRASH_ITEM_AMOUNT * trashZone.FillLevel) + 2); 
            var bagsAmount = random.Next(0, (int)(MAX_BAGS_AMOUNT * trashZone.FillLevel) + 2);

            if(itemsAmount > 0) {
                var trashItems = InventoryController.getConfigItemsForType<TrashItem>();
                
                for(var i = 0; i < itemsAmount; i++) {
                    var type = generateTrashType(random);
                    var cfg = trashItems.FirstOrDefault(t => t.additionalInfo == type.ToString());
                    if(cfg != null) {
                        if(!spot.Inventory.addItem(new TrashItem(cfg, 1, -1))) {
                            //If the item could not be added, we stop the loop
                            return;
                        }
                    }
                }
            }
            
            if(bagsAmount > 0) {
                var smallBags = 0;
                var mediumBags = 0;
                var largeBags = 0;
                                    
                for(var i = 0; i < bagsAmount; i++) {
                    //Generate multiple trashbags and on tendency larger trashbags if the zone is more filled
                    var number = random.NextDouble();
                    if(number < 0.5 * trashZone.FillLevel) {
                        largeBags += 1;
                    } else if (number < 0.8 * trashZone.FillLevel) {
                        mediumBags += 1;
                    } else {
                        smallBags += 1;
                    } 
                }
                
                if(largeBags > 0) {
                    spot.Inventory.addItem(InventoryController.createItem(InventoryController.getConfigItemByCodeIdentifier("BIG_TRASHBAG"), largeBags, -1));
                }
                
                if(mediumBags > 0) {
                    spot.Inventory.addItem(InventoryController.createItem(InventoryController.getConfigItemByCodeIdentifier("MEDIUM_TRASHBAG"), mediumBags, -1));
                }

                if(smallBags > 0) {
                    spot.Inventory.addItem(InventoryController.createItem(InventoryController.getConfigItemByCodeIdentifier("SMALL_TRASHBAG"), smallBags, -1));
                }
            }
            
            spot.showInventorySpotOpen(player, player.getInventory(), (p, fI, tI, i, a) => onMoveItemFromTrashCan(p, spot, fI, tI, i, a));
        }

        private bool onMoveItemFromTrashCan(IPlayer player, InventorySpot trashSpot, Inventory frominventory, Inventory toinventory, Item item, int amount) {
            if(item is not TrashBag bag) {
                return true;
            }
 
            var trashZone = getTrashZoneForPosition(trashSpot.CollisionShape.Position);
            if(trashZone == null) {
                return true;
            }
            
            changeBagFillFromZone(player, trashZone, bag.Size, trashSpot.Inventory == frominventory);
            
            return true;
        }

        private static void changeBagFillFromZone(IPlayer player, TrashZone zone, TrashBagSize size, bool remove) {
            var change = 0.0025f;
            if(size == TrashBagSize.Medium) {
                change = 0.004f;
            } else if(size == TrashBagSize.Large) {
                change = 0.007f;
            }
                       
            if(remove) {
                zone.FillLevel -= change;
                player.sendNotification(Constants.NotifactionTypes.Info, $"Diese Aktion hat den Füllstand des Gebiets: {zone.Name} verringert.", "Füllstand geändert");
            } else {
                zone.FillLevel += change;
            }
            
        }

        private void onOpenTrashBag(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading, bool isBroken, ref Menu menu) {
            var find = AllTrash.FirstOrDefault(t => player.Position.Distance(t.Spot.CollisionShape.Position) < Constants.MAX_TRASH_DISTANCE);

            if(find != null) {
                find.Spot.showInventorySpotOpen(player, player.getInventory());
            }
        }

        private static bool onInventoryEmpty(Inventory inventory) {
            var spot = AllTrash.FirstOrDefault(s => s.Spot.Inventory != null && s.Spot.Inventory.Id == inventory.Id);

            if(spot != null && spot.DbTrashCanId == -1) {
                removeSpot(spot);
            }

            return true;
        }

        private static void removeSpot(Trash trash) {
            AllTrash.Remove(trash);

            if(trash.Spot.Object != null) {
                SoundController.playSoundAtCoords(trash.Spot.CollisionShape.Position, 2, SoundController.Sounds.BagRustle, 0.1f);
            }

            trash.Spot?.remove();
        }
        
        
        #region TrashCreation
        
        public static List<TrashZone> getTrashZones() {
            return TrashZones;
        }
        
        public static List<TrashItem> createTrashItemsFromTrashBag(TrashBag bag) {
            var items = new List<TrashItem>();
            var random = new Random();

            var itemAmount = bag.Size switch {
                TrashBagSize.Small => random.Next(1, 3),
                TrashBagSize.Medium => random.Next(2, 5),
                TrashBagSize.Large => random.Next(4, 8),
                _ => 0,
            };

            var list = new List<(TrashTypes, int)>();
            for (var i = 0; i < itemAmount; i++) {
               var trashType = generateTrashType(random); 
               
               var valueTuple = list.FirstOrDefault(t => t.Item1 == trashType);
               if(valueTuple != default) {
                   valueTuple.Item2++;
               } else {
                   list.Add((trashType, 1));
               }
            }

            foreach(var trash in list) {
                items.Add(new TrashItem(InventoryController.getConfigItemByCodeIdentifier(getItemIdentifierFromTrashType(trash.Item1)), trash.Item2, -1));
            }
            
            return items;
        }

        private static string getItemIdentifierFromTrashType(TrashTypes type) {
            return type switch {
                TrashTypes.Plastic => "PLASTIC_TRASH",
                TrashTypes.Metal => "METAL_TRASH",
                TrashTypes.Paper => "PAPER_TRASH",
                TrashTypes.Glass => "GLASS_TRASH",
                TrashTypes.Organic => "ORGANIC_TRASH",
                TrashTypes.ResidualWaste => "RESIDUAL_TRASH",
                TrashTypes.HazardousWaste => "HAZARDOUS_TRASH",
                _ => "UNKNOWN_TRASH",
            };
        }

        private static TrashTypes generateTrashType(Random random) {
            var number = random.NextDouble();
        
            if(number < 0.2f) {
                return TrashTypes.Plastic;
            } else if (number < 0.35f) {
                return TrashTypes.Metal;
            } else if (number < 0.5f) {
                return TrashTypes.Paper;
            } else if (number < 0.7f) {
                return TrashTypes.Glass;
            } else if (number < 0.8f) {
                return TrashTypes.Organic;
            } else if (number < 0.95f) {
                return TrashTypes.ResidualWaste;
            } else {
                return TrashTypes.HazardousWaste;
            }
        }
        #endregion
    }
}
