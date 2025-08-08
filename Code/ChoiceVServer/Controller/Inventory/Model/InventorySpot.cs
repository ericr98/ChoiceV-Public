using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public enum InventorySpotType : int {
        None = 1,
        Trash = 2,
        Company = 3,
    }

    //Additionally access is possible if person has key
    public class InventorySpot {
        //public static List<InventorySpot> AllSpots = new List<InventorySpot>();
        private static Dictionary<int, InventorySpot> AllSpots = new();

        public int Id;
        public InventorySpotType Type;
        public int InventoryId;
        public string Name;
        public Inventory Inventory => _Inventory ?? InventoryController.loadInventory(InventoryId);
        private Inventory _Inventory;
        public CollisionShape CollisionShape;
        public int[] Combination;

        public Animation Animation;

        public Controller.Object Object;
        public DateTime CreateDate;

        public Predicate<IPlayer> OpenPredicate;
        public string OpenPredicateFailMessage;

        public string DoorGroup;

        public InventorySpot(int id, InventorySpotType type, string name, CollisionShape collisionShape, Inventory inventory, int[] combination, DateTime createDate, Animation animation, Controller.Object obj = null, string doorGroup = null) {
            Id = id;
            Type = type;
            Name = name;
            CollisionShape = collisionShape;
            _Inventory = inventory;
            Combination = combination;

            Animation = animation;

            var data = new Dictionary<string, dynamic> { { "spot", this } };
            CollisionShape.InteractData = data;

            Object = obj;

            CreateDate = createDate;
            DoorGroup = doorGroup;
        }

        public void setOpenPredicate(Predicate<IPlayer> predicate, string failMessage) {
            OpenPredicate = predicate;
            OpenPredicateFailMessage = failMessage;
        }

        public bool addItem(Item item, bool ignoreWeight = false) {
            if(_Inventory == null) {
                _Inventory = InventoryController.loadInventory(InventoryId);
            }

            return _Inventory.addItem(item, ignoreWeight);
        }

        public static InventorySpot getById(int id) {
            if(AllSpots.ContainsKey(id)) {
                return AllSpots[id];
            } else {
                return null;
            }
        }

        public static InventorySpot getByPredicate(Predicate<InventorySpot> predicate) {
            return AllSpots.Values.FirstOrDefault(i => predicate(i));
        }

        public static List<InventorySpot> getListByPredicate(Predicate<InventorySpot> predicate) {
            return AllSpots.Values.Where(i => predicate(i)).ToList();
        }

        public static InventorySpot create(InventorySpotType type, string name, Position position, float width, float height, float rotation, float maxWeight, int[] combination, string animationIdentifier = null, string doorGroup = null) {
            inventoryspot newSpot;
            var colShape = CollisionShape.Create(position, width, height, rotation, true, false, true, Constants.OnPlayerInteractionInventorySpot);
            var inv = InventoryController.createInventory(-1, maxWeight, InventoryTypes.InteractSpot);

            if(combination == null) {
                combination = [-1, -1, -1, -1, -1];
            }

            if(doorGroup == "") {
                doorGroup = null;
            }

            using(var db = new ChoiceVDb()) {
                newSpot = new inventoryspot {
                    type = (int)type,
                    inventoryId = inv.Id,
                    name = name,
                    combination = combination.ToJson(),
                    position = colShape.Position.ToJson(),
                    height = colShape.Height,
                    width = colShape.Width,
                    rotation = colShape.Rotation,

                    createDate = DateTime.Now,

                    connectedDoorGroup = doorGroup,
                };

                db.inventoryspots.Add(newSpot);
                db.SaveChanges();
            }

            inv.OwnerId = newSpot.id;

            var animation = AnimationController.getAnimationByName(animationIdentifier);

            var spot = new InventorySpot(newSpot.id, type, name, colShape, inv, combination, DateTime.Now, animation, null, doorGroup);
            AllSpots.Add(spot.Id, spot);

            return spot;
        }

        public static InventorySpot createWithObject(Controller.Object obj, InventorySpotType type, string name, Position position, float width, float height, float rotation, float maxWeight, int[] combination, string animationIdentifier = "NONE") {
            inventoryspot newSpot;
            var colShape = CollisionShape.Create(position, width, height, rotation, true, false, true, Constants.OnPlayerInteractionInventorySpot);
            var inv = InventoryController.createInventory(-1, maxWeight, InventoryTypes.InteractSpot);

            if(combination == null) {
                combination = new int[] { -1, -1, -1, -1, -1 };
            }

            using(var db = new ChoiceVDb()) {
                newSpot = new inventoryspot {
                    type = (int)type,
                    inventoryId = inv.Id,
                    name = name,
                    combination = combination.ToJson(),
                    position = colShape.Position.ToJson(),
                    height = colShape.Height,
                    width = colShape.Width,
                    rotation = colShape.Rotation,

                    objectModel = obj.ModelName,
                    objectPosition = obj.Position.ToJson(),
                    objectRotation = obj.Rotation.ToJson(),
                    createDate = DateTime.Now,
                };

                db.inventoryspots.Add(newSpot);
                db.SaveChanges();
            }

            inv.OwnerId = newSpot.id;

            var animation = AnimationController.getAnimationByName(animationIdentifier);

            var spot = new InventorySpot(newSpot.id, type, name, colShape, inv, combination, DateTime.Now, animation, obj);
            AllSpots.Add(spot.Id, spot);

            return spot;
        }

        public static InventorySpot load(inventoryspot spot) {
            var colShape = CollisionShape.Create(spot.position.FromJson(), spot.width, spot.height, spot.rotation, true, false, true, Constants.OnPlayerInteractionInventorySpot);

            //Animation anim = AnimationController.getAnimationByName(spot.animationIdentifier ?? "");
            Animation anim = null;
            //Not setting Inventory because of dynamic loading
            var newSpot = new InventorySpot(spot.id, (InventorySpotType)spot.type, spot.name, colShape, null, spot.combination.FromJson<int[]>(), spot.createDate, anim, null, spot.connectedDoorGroup);
            newSpot.InventoryId = spot.inventoryId;

            if(spot.objectModel != null) {
                newSpot.Object = ObjectController.createObject(spot.objectModel, spot.objectPosition.FromJson(), spot.objectRotation.FromJson<Rotation>(), 200, false);
            }

            AllSpots.Add(newSpot.Id, newSpot);
            return newSpot;
        }

        public void loadInventory() {
            if(_Inventory == null) {
                _Inventory = InventoryController.loadInventory(InventoryId);
            }
        }

        public void unloadInventory() {
            if(_Inventory != null) {
                InventoryController.unloadInventory(_Inventory);
            }
        }

        public void remove() {
            using(var db = new ChoiceVDb()) {
                var spot = db.inventoryspots.FirstOrDefault(i => i.id == Id);
                if(spot != null) {
                    db.Remove(spot);

                    db.SaveChanges();
                }
            }

            if(CollisionShape != null) {
                CollisionShape.Dispose();
                CollisionShape = null;
            }

            InventoryController.destroyInventory(_Inventory);

            if(Object != null) {
                ObjectController.deleteObject(Object);
            }

            AllSpots.Remove(Id);
        }

        public void showInventorySpotOpen(IPlayer player, Inventory inventory, OnInventoryMoveCallback onMoveAdditionalCallback = null) {
            if(player.getCharacterData().AdminMode) {
                player.sendNotification(Constants.NotifactionTypes.Info, $"Die Id des Spots ist {Id}", "");
            }

            if(OpenPredicate != null && !OpenPredicate(player)) {
                player.sendBlockNotification(OpenPredicateFailMessage, "Öffnen fehlgeschlagen", Constants.NotifactionImages.Package);
                return;
            }

            var doors = DoorController.getDoorsByGroups(new List<string> { DoorGroup });
            if(doors.Count > 0 && !doors.Any(d => !d.isLocked(player.Dimension)) && !doors.Any(d => d.canPlayerOpen(player))) {
                player.sendBlockNotification("Öffnen fehlgeschlagen. (Öffne verbundene Türen)", "Öffnen fehlgeschlagen", Constants.NotifactionImages.Package);
                return;
            }

            if(_Inventory == null) {
                _Inventory = InventoryController.loadInventory(InventoryId);
            }

            if(Combination.Length > 0 && Combination.ToJson() != new int[] { -1, -1, -1, -1, -1 }.ToJson()) {
                CombinationLockController.requestPlayerCombination(player, Combination, onAccessedSpot);
            } else {
                InventoryController.showMoveInventory(player, inventory, _Inventory, (p, fI, tI, i, a) => {
                    if(Animation != null) {
                        player.playAnimation(Animation);
                    }

                    var itemStr = "Unbekanntes Objekt";
                    if(i.Weight > 1) {
                        itemStr = i.Name;
                    }

                    if(fI.Id == player.getInventory().Id) {
                        CamController.checkIfCamSawAction(player, "Objekt weggelegt", $"Die Person hat {a}x {itemStr} in {Name} gelegt.");
                    } else if(tI.Id == player.getInventory().Id) {
                        CamController.checkIfCamSawAction(player, "Objekt genommen", $"Die Person hat {a}x {itemStr} aus {Name} genommen.");
                    }

                    onMoveAdditionalCallback?.Invoke(p, fI, tI, i, a);

                    return true;
                }, null, Name, true);
            }
        }

        private void onAccessedSpot(IPlayer player, Dictionary<string, dynamic> data) {
            InventoryController.showMoveInventory(player, player.getInventory(), _Inventory, (p, fI, tI, i, a) => {
                if(Animation != null) {
                    player.playAnimation(Animation);
                }

                return true;
            }, null, Name, true);
        }
    }
}
