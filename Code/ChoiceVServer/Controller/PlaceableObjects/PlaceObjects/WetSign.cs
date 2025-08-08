using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.PlaceableObjects {
    public class WetSign : PlaceableObject {
        private int ConfigId { get => (int)Data["ConfigId"]; set { Data["ConfigId"] = value; } }
        public WetSign(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) { }

        public WetSign(string model, Item placeableItem, IPlayer player, Position playerPosition, Rotation playerRotation) : base(playerPosition, playerRotation, 7.5f, 7.5f, true, new Dictionary<string, dynamic>()) {
            ConfigId = placeableItem.ConfigId;
        }

        public override void initialize(bool register = true) {
            var d = (DegreeRotation)Rotation;
            Object = ObjectController.createObject("v_serv_wetfloorsn", Position, d, 200, true);

            base.initialize(register);
        }

        public override void onEntityEnterShape(CollisionShape shape, IEntity entity) {
            base.onEntityEnterShape(shape, entity);

            checkPlayerFalls(entity);
        }

        public override void onEntityExitShape(CollisionShape shape, IEntity entity) {
            base.onEntityExitShape(shape, entity);

            checkPlayerFalls(entity);
        }

        private void checkPlayerFalls(IEntity entity) {
            if(entity.Type == BaseObjectType.Player && (entity as IPlayer).MoveSpeed >= 1.5) {
                var rand = new Random();

                if(rand.NextDouble() < 0.25) {
                    var player = entity as IPlayer;

                    player.sendNotification(Constants.NotifactionTypes.Warning, "Du bist auf dem rutschigen Boden ausgerutscht! Hast du das Schild nicht gesehen?", "Ausgerutscht", Constants.NotifactionImages.System);
                    var anim = AnimationController.getAnimationByName("RUTSCH_OUT");
                    player.playAnimation(anim);
                }
            }
        }

        public override Menu onInteractionMenu(IPlayer player) {
            var menu = new Menu("\"Nasser Boden\" Schild", "Was möchtest du tun?");
            var data = new Dictionary<string, dynamic> {
                {"placeable", this }
            };

            menu.addMenuItem(new ClickMenuItem("Aufheben", "Lege das Schild wieder in dein Inventar", "", "PICK_UP_PLACABLE", MenuItemStyle.green).withData(data));

            return menu;
        }

        public override bool onPickUp(IPlayer player, ref Constants.NotifactionImages img) {
            var configItem = InventoryController.getConfigById(ConfigId);
            return player.getInventory().addItem(new PlaceableObjectItem(configItem));
        }

        public override TimeSpan getAutomaticDeleteTimeSpan() {
            return TimeSpan.FromDays(5);
        }
    }
}
