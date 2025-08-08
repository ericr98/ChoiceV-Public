using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ChoiceVServer.Admin.Tools {
    class CollisionShapeCreator : ChoiceVScript {
        public CollisionShapeCreator() {
            EventController.addEvent("SPOT_SAVE", OnFSSave);

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    () => new ClickMenuItem("Kollisionen anzeigen/verstecken", "Lasse dir die Kollisionen anzeigen/oder mache sie unsichtbar", "", "SUPPORT_SHOW_COLLISIONSHAPES").withNotCloseOnAction(),
                    1,
                    SupportMenuCategories.Infos
                )
            );

            EventController.addMenuEvent("SUPPORT_SHOW_COLLISIONSHAPES", onSupportShowCollisonShapes);
        }

        private static Dictionary<int, bool> AreCollisionsShown = new Dictionary<int, bool>();

        private bool onSupportShowCollisonShapes(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            SupportController.setCurrentSupportFastAction(player, () => onSupportShowCollisonShapes(player, itemEvent, menuItemId, data, menuItemCefEvent));

            if(!AreCollisionsShown.ContainsKey(player.getCharacterId())) {
                AreCollisionsShown.Add(player.getCharacterId(), true);

                var shapesByNear = CollisionShape.AllShapes.Where(c => Vector3.Distance(c.Position, player.Position) < 300);
                foreach(var shape in shapesByNear) {
                    Logger.logTrace(LogCategory.Support, LogActionType.Viewed, player, $"CollisionShape shown at position: x: {shape.Position.X}, y: {shape.Position.Y}, z: {shape.Position.Z}");
                    player.emitClientEvent("SPOT_ADD", shape.Position.X, shape.Position.Y, shape.Position.Z, shape.ZDiv, shape.Width, shape.Height, 1, shape.Rotation);
                }

                player.sendNotification(Constants.NotifactionTypes.Success, "Kollisionen werden nun angezeigt", "Kollisionen angezeigt");
            } else {
                if(AreCollisionsShown[player.getCharacterId()]) {
                    AreCollisionsShown[player.getCharacterId()] = false;

                    player.emitClientEvent("SPOT_END");
                    player.sendNotification(Constants.NotifactionTypes.Warning, "Kollisionen sind nun wieder unsichtbar", "Kollisionen unsichtbar");
                } else {
                    AreCollisionsShown[player.getCharacterId()] = true;

                    var shapesByNear = CollisionShape.AllShapes.Where(c => Vector3.Distance(c.Position, player.Position) < 300);
                    foreach(var shape in shapesByNear) {
                        Logger.logTrace(LogCategory.Support, LogActionType.Viewed, player, $"CollisionShape shown at position: x: {shape.Position.X}, y: {shape.Position.Y}, z: {shape.Position.Z}");
                        player.emitClientEvent("SPOT_ADD", shape.Position.X, shape.Position.Y, shape.Position.Z, shape.ZDiv, shape.Width, shape.Height, 1, shape.Rotation);
                    }

                    player.sendNotification(Constants.NotifactionTypes.Success, "Kollisionen werden nun angezeigt", "Kollisionen angezeigt");
                }
            }
            return true;
        }

        public delegate void CollisionShapeCreatorDelegate(Position position, float width, float height, float rotation);

        private static Dictionary<int, CollisionShapeCreatorDelegate> Callbacks = new Dictionary<int, CollisionShapeCreatorDelegate>();

        public static void startCollisionShapeCreationWithCallback(IPlayer player, CollisionShapeCreatorDelegate callback, float startWidth = 5, float startHeight = 5, Position? startPos = null, bool noZHeight = false) {
            Callbacks[player.getCharacterId()] = callback;
            var pos = startPos != null && startPos != Position.Zero ? (Position)startPos : player.Position;

            player.setData("COLLISION_SPOT_POS", pos);

            player.emitClientEvent("SPOT_START", pos.X, pos.Y, pos.Z, noZHeight ? 10000 : CollisionShape.StandardZDiv, startWidth, startHeight, 0, 0, "SPOT_", "DONTCARE", "DONTCARE");
        }

        public bool OnFSSave(IPlayer player, string eventName, object[] args) {
            var oId = int.Parse(args[5].ToString());
            var name = args[6].ToString();
            var eve = args[7].ToString();


            var position = new Vector3(Convert.ToSingle(args[0]), Convert.ToSingle(args[1]), player.Position.Z);
            var width = Convert.ToSingle(args[2]);
            var height = Convert.ToSingle(args[3]);
            var rotation = Convert.ToSingle(args[4]);

            if(Callbacks.ContainsKey(player.getCharacterId())) {
                var callback = Callbacks[player.getCharacterId()];
                player.emitClientEvent("SPOT_END");
                Callbacks.Remove(player.getCharacterId());
                callback.Invoke(position, width, height, rotation);
                return true;
            } else {
                player.sendBlockNotification("Irgendetwas ist schiefgelaufen!", "Fehler!");
                return false;
            }
        }
    }
}
