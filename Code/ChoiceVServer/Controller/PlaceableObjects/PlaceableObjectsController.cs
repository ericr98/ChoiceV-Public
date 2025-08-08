using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Controller.CallbackController;

namespace ChoiceVServer.Controller.PlaceableObjects {
    public class PlaceableObjectsController : ChoiceVScript {
        private static Dictionary<WorldGrid, List<PlaceableObject>> AllPlaceableObjects = new();
        private static Dictionary<string, string> ModelHashToPlaceableName = new();


        private static readonly TimeSpan PLACEABLE_TICK_LENGTH = TimeSpan.FromMinutes(6);
        public PlaceableObjectsController() {
            EventController.addMenuEvent("PICK_UP_PLACABLE", onPickupPlaceable);
            EventController.addMenuEvent("DESTROY_PLACABLE", onDestroyPlaceable);
            
            PlaceableObject.PlaceableObjectModelChange += onObjectChangeModel;
            PlaceableObject.PlaceableObjectResetRemoveTimer += onObjectResetRemoveTimer;

            var list = new List<Materials>() { Materials.SandCompact, Materials.SandLoose, Materials.SandWet, Materials.SandDryDeep, Materials.SandWetDeep };
            CharacterController.addSelfMenuElement(
                new ConditionalPlayerSelfMenuElement(
                    (p) => new ClickMenuItem("Sandburg bauen", "Baue eine wunderschöne Sandburg", "", "ON_PLAYER_BUILD_SANDCASTLE"),
                    p => !p.IsInVehicle && list.Contains(p.getCharacterData().Material)
                )
            );
            EventController.addMenuEvent("ON_PLAYER_BUILD_SANDCASTLE", onPlayerBuildSandcastle);

            EventController.MainReadyDelegate += onMainReady;

            InteractionController.addObjectInteractionCallback("PLACEABLE_INTERACTION", null, onPlaceableInteraction);
        }

        private void onMainReady() {            
            loadPlaceables();
            InvokeController.AddTimedInvoke("Placeable-Interval", _ => onPlaceableInterval(), PLACEABLE_TICK_LENGTH, true);
            
            var list = new List<string>();
            foreach(var cfg in InventoryController.getConfigItems(c => c.codeItem == nameof(PlaceableObjectItem))) {
                var modelForCfg = PlaceableObjectItem.getModelForConfig(cfg);
                list.Add(ChoiceVAPI.Hash(modelForCfg).ToString());

                ModelHashToPlaceableName.Add(ChoiceVAPI.Hash(modelForCfg).ToString(), cfg.name);
            }

            InteractionController.addInteractableObjects(list, "PLACEABLE_INTERACTION");
        }

        public static string getPlaceableObjectMenuName(PlaceableObject obj) {
            var hash = ChoiceVAPI.Hash(obj.ModelName).ToString();
            if(ModelHashToPlaceableName.TryGetValue(hash, out var name)) {
                return name;
            } else {
                return "Objekt";
            }
        }

        private void onPlaceableInteraction(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading, bool isBroken, ref Menu menu) {
            var worldGrid = WorldController.getWorldGrid(objectPosition);

            if(AllPlaceableObjects.ContainsKey(worldGrid)) {
                var find = AllPlaceableObjects[worldGrid].FirstOrDefault(p => ChoiceVAPI.Hash(p.ModelName).ToString() == modelName && p.Position.Distance(objectPosition) < 0.15);

                if(find != null) {
                    var findMenu = find.onInteractionMenu(player);

                    if(findMenu != null) {
                        menu.addMenuItem(new MenuMenuItem(findMenu.Name, findMenu));
                    }
                }
            }
        }

        private bool onPlayerBuildSandcastle(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var model = SandcastlePlaceable.getRandomModel();
            ObjectController.startObjectPlacerMode(player, model, 0, (player, pos, rot) => {
                //var facingRot = player.getRotationTowardsPosition(pos);
                AnimationController.animationTask(player, "amb@world_human_gardener_plant@male@idle_a", "idle_a", TimeSpan.FromSeconds(60), 1, () => {
                    var sandcastle = new SandcastlePlaceable(player, pos, new DegreeRotation(0, 0, rot), model);
                    sandcastle.initialize();

                }, Position.Zero, Position.Zero, null, 0, false);
            });

            return true;
        }

        private static void addPlaceableToList(PlaceableObject placeable) {
            var worldGrid = WorldController.getWorldGrid(placeable.Position);

            if(AllPlaceableObjects.TryGetValue(worldGrid, out var list)) {
                list.Add(placeable);
            } else {
                var newList = new List<PlaceableObject> {
                    placeable
                };
                AllPlaceableObjects.Add(worldGrid, newList);
            }
        }

        private static void removePlaceableFromList(PlaceableObject placeable) {
            var worldGrid = WorldController.getWorldGrid(placeable.Position);

            if(AllPlaceableObjects.ContainsKey(worldGrid)) {
                var list = AllPlaceableObjects[worldGrid];
                list.Remove(placeable);

                if(list.Count == 0) {
                    AllPlaceableObjects.Remove(worldGrid);
                }
            }
        }

        private static void onPlaceableInterval() {
            foreach(var placeableList in AllPlaceableObjects.Values) {
                foreach(var placeable in placeableList) {
                    if(placeable.AutomaticallyDeletedPlacable && placeable.AutomaticDeleteDate < DateTime.Now) {
                        placeable.onRemove();
                    }

                    if(placeable.IntervalPlaceable) {
                        placeable.onInterval(PLACEABLE_TICK_LENGTH);
                    }
                }
            }
        }

        private void loadPlaceables() {
            using(var db = new ChoiceVDb()) {
                foreach(var row in db.placeableobjects) {

                    var type = Type.GetType("ChoiceVServer.Controller.PlaceableObjects." + row.codeItem, false);

                    var placeable = Activator.CreateInstance(type, row.position.FromJson(), row.rotation.FromJson<Rotation>(), row.colWidth, row.colHeight, row.trackVehicles == 1, row.data.FromJson<Dictionary<string, dynamic>>()) as PlaceableObject;
                    placeable.Id = row.id;
                    placeable.ModelName = row.objectModel;
                    placeable.setAutomaticDeleteDate(row.automaticDeleteDate);
                    placeable.initialize(false);

                    Logger.logDebug(LogCategory.ServerStartup, LogActionType.Created, $"loadPlaceables. placeable created: {placeable.Id}, type: {placeable.GetType().Name}");
                    addPlaceableToList(placeable);
                }
            }
        }

        public static void registerPlaceable(PlaceableObject placeable) {
            try {
                addPlaceableToList(placeable);

                using(var db = new ChoiceVDb()) {
                    var row = new placeableobject {
                        objectModel = placeable.Object.ModelName,
                        position = placeable.Position.ToJson(),
                        rotation = placeable.Rotation.ToJson(),
                        colHeight = placeable.CollisionShape.Height,
                        colWidth = placeable.CollisionShape.Width,
                        createDate = placeable.CreateTime,
                        codeItem = placeable.GetType().Name,
                        data = placeable.Data.ToJson(),
                        trackVehicles = placeable.CollisionShape.TrackVehicles ? 1 : 0,
                        automaticDeleteDate = placeable.AutomaticallyDeletedPlacable ? placeable.AutomaticDeleteDate : null,
                    };

                    db.placeableobjects.Add(row);
                    db.SaveChanges();

                    placeable.Id = row.id;
                }
            } catch(Exception e) {
                Logger.logException(e, "registerPlaceable: Something went wrong");
            }
        }

        private void onObjectChangeModel(PlaceableObject obj, string newModel) {
            using(var db = new ChoiceVDb()) {
                var row = db.placeableobjects.FirstOrDefault(r => r.id == obj.Id);

                if(row != null) {
                    row.objectModel = newModel;
                    db.SaveChanges();
                }
            }
        }

        private void onObjectResetRemoveTimer(PlaceableObject obj, TimeSpan removeTimer) {
            if(DateTime.Now - obj.LastAutoDeleteReset > TimeSpan.FromMinutes(15)) {
                obj.LastAutoDeleteReset = DateTime.Now;
                obj.AutomaticDeleteDate = DateTime.Now + removeTimer;

                using(var db = new ChoiceVDb()) {
                    var row = db.placeableobjects.Find(obj.Id);

                    if(row != null) {
                        row.automaticDeleteDate = obj.AutomaticDeleteDate;
                        db.SaveChanges();
                    }
                }
            }
        }

        public static void unregisterPlaceable(PlaceableObject placeable) {
            removePlaceableFromList(placeable);

            using(var db = new ChoiceVDb()) {
                var row = db.placeableobjects.FirstOrDefault(r => r.id == placeable.Id);

                if(row != null) {
                    db.placeableobjects.Remove(row);
                    db.SaveChanges();
                }
            }

            Logger.logDebug(LogCategory.ServerStartup, LogActionType.Removed, $"placeable removed: {placeable.Id} type: {placeable.GetType().Name}");
        }

        public static void updateData(PlaceableObject placeable) {
            using(var db = new ChoiceVDb()) {
                var row = db.placeableobjects.FirstOrDefault(r => r.id == placeable.Id);

                if(row != null) {
                    row.data = placeable.Data.ToJson();
                    db.SaveChanges();
                }
            }
        }

        private bool onPickupPlaceable(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(data.ContainsKey("placeable")) {
                var plac = (PlaceableObject)data["placeable"];
                var img = Constants.NotifactionImages.System;

                if(plac.PickUpAnimation != null) {
                    AnimationController.animationTask(player, plac.PickUpAnimation, () => {
                        if(plac.onPickUp(player, ref img)) {
                            plac.onRemove();
                            WebController.closePlayerCef(player);
                        } else {
                            player.sendBlockNotification("Du konntest das Objekt nicht aufnehmen. Dein Inventar ist voll!", "Inventar voll!", img);
                        }
                    });

                    return true;
                } else {
                    if(plac.onPickUp(player, ref img)) {
                        plac.onRemove();
                        return true;
                    } else {
                        player.sendBlockNotification("Du konntest das Objekt nicht aufnehmen. Dein Inventar ist voll!", "Inventar voll!", img);
                    }
                }
            } else {
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onPickupPlaceable: Tried to execute Pickup Event without any placable set!");
            }

            return false;
        }

        private bool onDestroyPlaceable(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(data.ContainsKey("placeable")) {
                var plac = (PlaceableObject)data["placeable"];
                if(plac.PickUpAnimation != null) {
                    AnimationController.animationTask(player, plac.PickUpAnimation, () => {
                        plac.onRemove();
                        player.sendNotification(Constants.NotifactionTypes.Success, "Das Objekt wurde erfolgreich zerstört", "Objekt zerstört!");
                    });
                } else {
                    plac.onRemove();
                    player.sendNotification(Constants.NotifactionTypes.Success, "Das Objekt wurde erfolgreich zerstört", "Objekt zerstört!");
                }
                return true;
            } else {
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onDestroyPlaceable: Tried to execute Destroy Event without any placable set!");
            }

            return false;
        }
    }
}
