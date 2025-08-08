using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller.Collectables {
    public class CollectableFrog : Collectable {
        private CollisionShape DetectRange;

        private DateTime NextQuack = DateTime.MinValue;

        private static readonly int FROG_RANGE = 20;
        private static readonly int FROG_FLEE_RANGE = 10;

        public CollectableFrog(CollectableAreaTypes areaType, Position position) : base(areaType, position, 1.5f, 1.5f) {
            DetectRange = CollisionShape.Create(position, FROG_RANGE, FROG_RANGE, 0, true, false, true);
            DetectRange.OnPlayerMoveInShape += onPlayerMoveInShape;
        }

        private void onPlayerMoveInShape(CollisionShape shape, IEntity entity) {
            if (entity is IPlayer player) {
                if (player.MoveSpeed >= 1.5 || !player.getCharacterData().IsCrouching) {
                    if (player.Position.Distance(Position) <= FROG_FLEE_RANGE) {
                        SoundController.playSoundAtCoords(Position, FROG_FLEE_RANGE + 3, SoundController.Sounds.WaterSplash, 1, "mp3");
                        destroy();
                        return;
                    }
                }

                if (NextQuack == DateTime.MinValue || DateTime.Now > NextQuack) {
                    var r = new Random();
                    NextQuack = DateTime.Now + TimeSpan.FromSeconds(r.Next(15, 30));

                    if (r.Next(0, 1000) == 1) {
                        SoundController.playSoundAtCoords(Position, FROG_RANGE + 3, SoundController.Sounds.QuackRare, 1, "mp3");
                        player.sendNotification(NotifactionTypes.Info, "Achievment freigeschaltet: \"Was war das denn für ein Frosch?\"", "Achievment freigeschaltet", NotifactionImages.System);
                    } else {
                        switch (r.Next(3)) {
                            case 0:
                                SoundController.playSoundAtCoords(Position, FROG_RANGE + 3, SoundController.Sounds.Quack1, 1, "mp3");
                                break;
                            case 1:
                                SoundController.playSoundAtCoords(Position, FROG_RANGE + 3, SoundController.Sounds.Quack2, 1, "mp3");
                                break;
                            case 2:
                                SoundController.playSoundAtCoords(Position, FROG_RANGE + 3, SoundController.Sounds.Quack3, 1, "mp3");
                                break;

                        }
                    }
                }
            }
        }

        protected override void loadModels() {
            Models = new List<string> {
                "frog_brown", "frog_green", "frog_yellow"
            };
        }

        protected override float getZOffsetForModel(string model) {
            return 0;
        }

        protected override Rotation getRotationForModel(string model) {
            return new DegreeRotation(0, 0, new Random().Next(0, 360));
        }

        protected override Animation getAnimationForModel(string model) {
            return new Animation("weapons@projectile@sticky_bomb", "plant_floor", TimeSpan.FromMilliseconds(500), 40, 0.37f);
        }

        public override void onCollectStep(IPlayer player) {
            var cfg = InventoryController.getConfigItem(i => i.additionalInfo == Object.ModelName);
            var item = InventoryController.createItem(cfg, 1);

            if (player.getInventory().addItem(item)) {
                player.sendNotification(NotifactionTypes.Success, $"Du hast ein/eine {item.Name} gefangen!", $"{item.Name} erhalten");
                base.onCollectStep(player);
            } else {
                player.sendNotification(NotifactionTypes.Danger, "Dein Inventar ist voll!", "Inventar voll");
            }
        }

        protected override string getObjetModel() {
            switch (AreaType) {
                case CollectableAreaTypes.AllFrogs:
                    return Models[new Random().Next(0, Models.Count)];
                default:
                    return null;
            }
        }

        public override void destroy() {
            DetectRange.Dispose();
            base.destroy();
        }
    }
}
