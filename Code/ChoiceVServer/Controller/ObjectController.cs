using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller {
    public class ObjectController : ChoiceVScript {
        public static int ObjectsId = 0;
        public static Dictionary<int, Object> AllObjects = new Dictionary<int, Object>();
        public static Dictionary<int, Object> AllAttachedObjects = new Dictionary<int, Object>();

        public ObjectController() {
            EventController.addEvent(Constants.OnPlayerObjectMoved, onObjectMoved);

            EventController.PlayerPastSuccessfullConnectionDelegate += onPlayerSuccessfullyConnected;
            EventController.PlayerDisconnectedDelegate += onPlayerDisconnect;

            EventController.PlayerChangeDimensionDelegate += onPlayerChangeDimension;

            CallbackController.registerCallback(CallbackController.PlayerCallbackTypes.ObjectPlacerMode, "STOP_OBJECT_PLACE_MODE");

            EventController.addMenuEvent("FINISH_OBJECT_PLACER", onPlacerFinishPlaceObject);

            InvokeController.AddTimedInvoke("ObjectUpdater", (i) => {
               updatedObjects();
            }, TimeSpan.FromSeconds(2.5), true);
        }

        private void updatedObjects() {
            foreach(var obj in AllAttachedObjects.Values.Reverse()) {
                if(obj.AttachedPlayer.Position.Distance(obj.Position) > 20) {
                    obj.Position = obj.AttachedPlayer.Position;
                    ChoiceVAPI.ForAllPlayers((p) => p.emitClientEvent("UPDATE_OBJECT_POSITION", obj.Id, obj.Position.X, obj.Position.Y, obj.Position.Z));
                }
            }
        }

        private void onPlayerChangeDimension(IPlayer player, int oldDimension, int newDimension) {
            var charId = player.getCharacterId();

            var attacheds = AllObjects.Values.Where(o => o.attachedToCharacter == charId).ToList();
            if(attacheds.Count > 0) {
                foreach(var p in ChoiceVAPI.GetAllPlayers()) {
                    if(p != player) {
                        foreach(var attached in attacheds) {
                            if(player.Dimension != p.Dimension) {
                                p.emitClientEvent("CHANGE_OBJECT_SAME_DIMENSION", attached.Id, false);
                            } else {
                                p.emitClientEvent("CHANGE_OBJECT_SAME_DIMENSION", attached.Id, true);
                            }
                        }
                    }
                }
            }
        }

        public static Object getObjectById(int id) {
            return AllObjects.ContainsKey(id) ? AllObjects[id] : null;
        }

        public static Object createObject(string modelName, IPlayer attachTo, Position offset, DegreeRotation rotation, int bone, int lodDistance = 150, bool collision = false, int attachVertexOrder = 2) {
            var obj = createObject(modelName, attachTo.Position - new Position(0, 0, 10), Rotation.Zero, lodDistance, collision);

            obj.attachedToCharacter = attachTo.getCharacterId();
            obj.Position = offset;
            obj.Rotation = rotation;
            obj.Bone = bone;
            obj.AttachedPlayer = attachTo;
            obj.AttachVertexOrder = attachVertexOrder;

            AllAttachedObjects.Add(obj.Id, obj);

            foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                player.emitClientEvent(Constants.PlayerAttachObjectToPlayer, obj.Id, obj.AttachedPlayer, obj.Bone, obj.Position.X, obj.Position.Y, obj.Position.Z, obj.Rotation.Roll, obj.Rotation.Pitch, obj.Rotation.Yaw, attachVertexOrder);
            }

            return obj;
        }

        public static Object createObject(string modelName, Position position, DegreeRotation rotation, int lodDistance = 200, bool collision = true, int dimension = Constants.GlobalDimension) {
            lock(AllObjects) {
                ObjectsId++;

                var obj = new Object { Id = ObjectsId, ModelName = modelName, Position = position };

                obj.LodDistance = lodDistance;
                obj.Collision = collision;
                obj.Rotation = rotation;
                obj.Dimension = dimension;

                AllObjects.Add(obj.Id, obj);

                foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                    if(player.getCharacterFullyLoaded()) {
                        player.emitClientEvent(Constants.PlayerCreateObject, obj.Id, obj.ModelName, obj.Position.X, obj.Position.Y, obj.Position.Z, lodDistance, collision, rotation.Pitch, rotation.Roll, rotation.Yaw, obj.PlacedOnGroundProperly, obj.PlacedOnGroundProperlyZOffset);
                    }
                }

                return obj;
            }
        }

        public static Object createObjectPlacedOnGroundProperly(string modelName, Position position, DegreeRotation rotation, int lodDistance = 200, bool collision = true, float zOffset = 0, int dimension = Constants.GlobalDimension) {
            lock(AllObjects) {
                ObjectsId++;

                var obj = new Object { Id = ObjectsId, ModelName = modelName, Position = position };

                obj.LodDistance = lodDistance;
                obj.Collision = collision;
                obj.Rotation = rotation;
                obj.PlacedOnGroundProperly = true;
                obj.PlacedOnGroundProperlyZOffset = zOffset;
                obj.Dimension = dimension;

                AllObjects.Add(obj.Id, obj);

                foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                    if(player.getCharacterFullyLoaded()) {
                        player.emitClientEvent(Constants.PlayerCreateObject, obj.Id, obj.ModelName, obj.Position.X, obj.Position.Y, obj.Position.Z, lodDistance, collision, rotation.Pitch, rotation.Roll, rotation.Yaw, obj.PlacedOnGroundProperly, obj.PlacedOnGroundProperlyZOffset);
                    }
                }

                return obj;
            }
        }

        public static void setObjectRotation(Object obj, Rotation rotation) {
            obj.Rotation = rotation;

            foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                player.emitClientEvent(Constants.PlayerRotateObject, obj.Id, rotation.Pitch, rotation.Roll, rotation.Yaw);
            }
        }

        public static void setObjectHeading(Object obj, float heading) {
            foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                player.emitClientEvent(Constants.PlayerRotateObject, obj.Id, heading);
            }
        }

        public static void deleteObject(Object obj) {
            lock(AllObjects) {
                if(AllObjects.Remove(obj.Id)) {
                    obj.Deleted = true;

                    foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                        if(player.getCharacterFullyLoaded()) {
                            player.emitClientEvent(Constants.PlayerDeleteObject, obj.Id);
                        }
                    }
                }

                if(AllAttachedObjects.ContainsKey(obj.Id)) {
                    AllAttachedObjects.Remove(obj.Id);
                }
            }
        }

        public static void moveObject(Object obj, Position newPosition, bool deleteOnFinish, float speedX = 1, float speedY = 1, float speedZ = 1, bool collision = false) {
            obj.Position = newPosition;
            obj.DeleteAfterAction = deleteOnFinish;

            foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                player.emitClientEvent(Constants.PlayerMoveObject, obj.Id, newPosition.X, newPosition.Y, newPosition.Z, speedX, speedY, speedZ, collision);
            }
        }

        public static void reattachObject(Object obj, Position offset, DegreeRotation rot, int bone) {
            obj.Position = offset;
            obj.Rotation = rot;
            obj.Bone = bone;

            var player = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.getCharacterId() == obj.attachedToCharacter);
            if(player == null) {
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"Player for reattach not found! Id: {obj.attachedToCharacter}, Object: {obj.ToJson()}");
                return;
            }

            player.emitClientEvent(Constants.PlayerReattachObjectToPlayer, obj.Id, player, bone, offset.X, offset.Y, offset.Z, rot.Roll, rot.Pitch, rot.Yaw);
        }

        private static bool onObjectMoved(IPlayer player, string eventName, object[] args) {
            var id = int.Parse(args[0].ToString());
            var handle = int.Parse(args[1].ToString());

            if(!AllObjects.ContainsKey(id)) {
                return false;
            }

            var obj = AllObjects[id];

            if(obj.DeleteAfterAction) {
                deleteObject(obj);
            }

            return true;
        }

        private void onPlayerSuccessfullyConnected(IPlayer player, character character) {
            try {
                foreach(var obj in AllObjects.Values) {
                    var rot = (DegreeRotation)obj.Rotation;
                    player.emitClientEvent(Constants.PlayerCreateObject, obj.Id, obj.ModelName, obj.Position.X, obj.Position.Y, obj.Position.Z, obj.LodDistance, obj.Collision, rot.Pitch, rot.Roll, rot.Yaw, obj.PlacedOnGroundProperly, obj.PlacedOnGroundProperlyZOffset);

                    if(obj.attachedToCharacter != -1) {
                        var target = ChoiceVAPI.FindPlayerByCharId(obj.attachedToCharacter);

                        player.emitClientEvent(Constants.PlayerAttachObjectToPlayer, obj.Id, target, obj.Bone, obj.Position.X, obj.Position.Y, obj.Position.Z, obj.Rotation.Roll, obj.Rotation.Pitch, obj.Rotation.Yaw, obj.AttachVertexOrder);

                        if(target.Dimension != player.Dimension) {
                            player.emitClientEvent("CHANGE_OBJECT_SAME_DIMENSION", obj.Id, false);
                        }
                    }


                    Logger.logTrace(LogCategory.Player, LogActionType.Created, player, $"Spawned object for player: object: {obj.Id}, {obj.ModelName}, {obj.Position.X}, {obj.Position.Y}, {obj.Position.Z}");
                }
            } catch(Exception) {
                ChoiceVAPI.KickPlayer(player, "TableLeg", "Versuche den Login erneut, bei weiterem Fehler melde dich im Support!", "Ein Fehler beim Spawnen der Objekte ist aufgetreten");
            }
        }

        private static void onPlayerDisconnect(IPlayer player, string reason) {
            var charId = player.getCharacterId();
            if(charId != -1) {
                var attachedToChar = AllObjects.Values.Where(o => o.attachedToCharacter == player.getCharacterId());

                foreach(var atObj in attachedToChar) {
                    deleteObject(atObj);
                }
            }
        }

        public delegate void ObjectPlacerCallback(IPlayer player, Position position, float heading);
        public static void startObjectPlacerMode(IPlayer player, string model, float headingOffset, ObjectPlacerCallback callback, float zOffset = 0) {
            player.emitClientEvent("START_OBJECT_PLACE_MODE", model, headingOffset, zOffset);

            var menu = new Menu("Objekt-Platzierer", "Platziere das Objekt", (p) => {
                player.emitClientEvent("STOP_OBJECT_PLACE_MODE", -1);
            });
            menu.addMenuItem(new StaticMenuItem("Benutzung", "Das Objekt kann hiermit platziert werden. Es wird mithilfe der Kameraausrichtung positioniert. Scrollen mit dem Mausrad passt die Rotation an", "Kamera + Mausrad"));
            menu.addMenuItem(new ClickMenuItem("Bestätigen", " Platziere das Objekt wie ausgerichtet", "", "FINISH_OBJECT_PLACER", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Cancel", false }, { "Callback", callback } }));
            menu.addMenuItem(new ClickMenuItem("Abbrechen", "Brich das Platzieren ab", "", "FINISH_OBJECT_PLACER", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "Cancel", true }, { "Callback", callback } }));

            player.showMenu(menu, false);
        }

        private bool onPlacerFinishPlaceObject(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var cancel = (bool)data["Cancel"];
            var callback = (ObjectPlacerCallback)data["Callback"];

            if(cancel) {
                player.emitClientEvent("STOP_OBJECT_PLACE_MODE", -1);
            } else {
                CallbackController.onPlayerCallback(CallbackController.PlayerCallbackTypes.ObjectPlacerMode, player, (p, args) => {
                    var pos = (Position)args[0];
                    var heading = Convert.ToSingle(args[1]);

                    callback.Invoke(player, pos, heading);
                });
            }

            return true;
        }

    }

    public class Object {
        public int Id;
        public string ModelName;

        [JsonIgnore]
        public Position Position;
        [JsonIgnore]
        public DegreeRotation Rotation = DegreeRotation.Zero;

        public int LodDistance;
        public bool Collision;

        public int attachedToCharacter = -1;
        public int Bone = -1;

        [JsonIgnore]
        public IPlayer AttachedPlayer;
        public bool DeleteAfterAction;

        public bool Deleted = false;

        public int AttachVertexOrder = 2;

        public bool PlacedOnGroundProperly = false;
        public float PlacedOnGroundProperlyZOffset = 0;

        public int Dimension = Constants.GlobalDimension;
    }
}
