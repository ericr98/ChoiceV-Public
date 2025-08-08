using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.Collectables {
    public abstract class Collectable {
        protected CollectableAreaTypes AreaType;

        public Position Position;
        protected float ZOffset;
        protected float Height;
        protected float Width;

        public Object Object;
        public CollisionShape CollisionShape;

        public List<string> Models;

        public CollectableRemoveDelegate CollectableRemoveDelegate;

        protected abstract void loadModels();

        protected abstract float getZOffsetForModel(string model);

        protected virtual Rotation getRotationForModel(string model) {
            return Rotation.Zero;
        }

        private bool onCollect(IPlayer player) {
            if(Object != null) {
                onCollectStep(player);
                return true;
            } else {
                return false;
            }
        }

        public virtual void onCollectStep(IPlayer player) {
            destroy();
        }

        public virtual void destroy() {
            ObjectController.deleteObject(Object);
            CollisionShape.Dispose();

            if(CollectableRemoveDelegate != null) {
                CollectableRemoveDelegate.Invoke(this);
            }
        }

        public Collectable(CollectableAreaTypes areaType, Position position, float height, float width) {
            loadModels();

            AreaType = areaType;

            Position = position;

            Height = height;
            Width = width;
        }

        public void spawn() {
            var r = new Random();
            var model = getObjetModel();

            var spawnPos = new Position(Position.X, Position.Y, WorldController.getGroundHeightAt(Position.X, Position.Y));
            Object = ObjectController.createObjectPlacedOnGroundProperly(model, spawnPos, getRotationForModel(model), 100, false, getZOffsetForModel(model));

            CollisionShape = CollisionShape.Create(spawnPos, Width, Height, 0, true, false, true);
            CollisionShape.OnCollisionShapeInteraction += onCollect;

            var anim = getAnimationForModel(model);
            if(anim != null) {
                CollisionShape.Animation = anim;
            }
        }

        protected virtual string getObjetModel() {
            var r = new Random();
            return Models[r.Next(0, Models.Count)];
        }

        protected virtual Animation getAnimationForModel(string model) {
            return null;
        }
    }
}
