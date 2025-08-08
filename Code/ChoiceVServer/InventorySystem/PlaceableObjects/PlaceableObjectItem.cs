using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.PlaceableObjects;
using ChoiceVServer.Model.Database;
using System;

namespace ChoiceVServer.InventorySystem {
    public class PlaceableObjectItem : Item {
        private string CodeItem = null;
        private string Model;
        private float CollisionDimension;
        private float ZOffset;
        private float VecMultiplier;

        public PlaceableObjectItem(item item) : base(item) { }

        public PlaceableObjectItem(configitem configItem, int amount, int quality) : base(configItem) {
            processAdditionalInfo(configItem.additionalInfo);
        }

        public PlaceableObjectItem(configitem configItem) : base(configItem) {
            processAdditionalInfo(configItem.additionalInfo);
        }

        public override void use(IPlayer player) {
            if(player.isRestricted()) {
                player.sendBlockNotification("Du kannst hier keine Objekte platzieren!", "Objekt blockiert!");
                return;
            }

            var vec = player.getForwardVector();
            vec.X = vec.X * VecMultiplier;
            vec.Y = vec.Y * VecMultiplier;
            var rot = (Rotation)player.getRotationTowardsPosition(new Position(player.Position.X - vec.X, player.Position.Y - vec.Y, player.Position.Z));

            var anim = AnimationController.getAnimationByName(Constants.KNEEL_DOWN_ANIMATION);
            ObjectController.startObjectPlacerMode(player, Model, rot.Yaw, (p, pos, heading) => {
                AnimationController.animationTask(player, anim, () => {
                    if(CodeItem == null) {
                        //create SimplePlaceableObject
                        var placeable = new SimplePlaceable(player, pos, new DegreeRotation(0, 0, heading), Model, ConfigId, CollisionDimension);
                        placeable.initialize();

                    } else {
                        var type = System.Type.GetType("ChoiceVServer.Controller.PlaceableObjects." + CodeItem, false);
                        var placeable = Activator.CreateInstance(type, Model, this, player, pos, new Rotation(0, 0, heading * 3.14f / 180)) as PlaceableObject;
                        placeable.initialize();
                    }

                    base.use(player);
                });
            }, ZOffset);
        }

        public static string getModelForConfig(configitem configItem) {
            var infos = configItem.additionalInfo.Split('#');

            if(infos.Length > 2) {
                return infos[0];
            } else if(infos.Length == 2) {
                return infos[1];
            } else {
                return configItem.additionalInfo;
            }
        }

        public override void processAdditionalInfo(string info) {
            base.processAdditionalInfo(info);
            var infos = info.Split('#');

            if(infos.Length > 2) {
                Model = infos[0];
                CollisionDimension = float.Parse(infos[1]);
                VecMultiplier = float.Parse(infos[2]);
                ZOffset = float.Parse(infos[3]);
            } else if(infos.Length == 2) {
                CodeItem = infos[0];
                Model = infos[1];

                CollisionDimension = 2f;
                ZOffset = 0;
                VecMultiplier = 1;
            } else {
                Model = info;

                CollisionDimension = 2f;
                ZOffset = 0;
                VecMultiplier = 1;
            }
        }
    }
}