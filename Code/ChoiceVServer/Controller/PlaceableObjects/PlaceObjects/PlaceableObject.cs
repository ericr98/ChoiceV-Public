using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.PlaceableObjects {
    public delegate void PlaceableObjectModelChange(PlaceableObject obj, string newModel);
    public delegate void PlaceableObjectResetRemoveTimer(PlaceableObject obj, TimeSpan removeTimer);

    public class PlaceableObject {

        public static PlaceableObjectModelChange PlaceableObjectModelChange;
        public static PlaceableObjectResetRemoveTimer PlaceableObjectResetRemoveTimer;


        public int Id;
        public Object Object;
        public CollisionShape CollisionShape;
        public Position Position;
        public Rotation Rotation;

        public DateTime CreateTime;
        public DateTime AutomaticDeleteDate;
        public DateTime LastAutoDeleteReset;

        public ExtendedDictionary<string, dynamic> Data;

        public string ModelName;

        public bool IntervalPlaceable = false;
        public bool AutomaticallyDeletedPlacable = true;

        public Animation PickUpAnimation;

        public PlaceableObject(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data, Animation pickupAnim = null) {
            Position = position;
            Rotation = rotation;

            CollisionShape = CollisionShape.Create(position, width, height, rotation.Yaw, true, trackVehicles, true);
            CollisionShape.OnCollisionShapeInteraction += onInteraction;

            CollisionShape.OnEntityEnterShape += onEntityEnterShape;
            CollisionShape.OnEntityExitShape += onEntityExitShape;

            CollisionShape.Owner = this;
            CreateTime = DateTime.Now;

            if(data != null) {
                Data = new ExtendedDictionary<string, dynamic>(data);
                Data.OnValueChanged += (key, value) => {
                    PlaceableObjectsController.updateData(this);
                };
            }

            if(PickUpAnimation == null) {
                PickUpAnimation = AnimationController.getAnimationByName(Constants.KNEEL_DOWN_ANIMATION);
            }
        }

        public void setAutomaticDeleteDate(DateTime? automaticDeleteDate) {
            if(automaticDeleteDate != null) {
                AutomaticallyDeletedPlacable = true;
                AutomaticDeleteDate = automaticDeleteDate ?? DateTime.MinValue;
                LastAutoDeleteReset = DateTime.MinValue;
            } else {
                AutomaticallyDeletedPlacable = false;
            }
        }


        /// <summary>
        /// IMPORTANT! Call the base.initialize() after you created your Object! Otherwise the db will return an error
        /// </summary>
        /// <param name="register"></param>
        public virtual void initialize(bool register = true) {
            if(Config.IsDevServer) {
                AutomaticallyDeletedPlacable = false;
            }

            if(register) {
                PlaceableObjectsController.registerPlaceable(this);


                if(AutomaticallyDeletedPlacable) {
                    setAutomaticDeleteDate(DateTime.Now + getAutomaticDeleteTimeSpan());
                }
            }
        }

        public virtual void onRemove() {
            PlaceableObjectsController.unregisterPlaceable(this);

            if(Object != null) {
                ObjectController.deleteObject(Object);
                Object = null;
            }

            if(CollisionShape != null) {
                CollisionShape.Dispose();
                CollisionShape = null;
            }
        }

        public virtual void onEntityExitShape(CollisionShape shape, IEntity entity) { if(AutomaticallyDeletedPlacable) PlaceableObjectResetRemoveTimer.Invoke(this, getAutomaticDeleteTimeSpan()); }

        public virtual void onEntityEnterShape(CollisionShape shape, IEntity entity) { }

        public bool onInteraction(IPlayer player) { 
            var menu = onInteractionMenu(player);
            if(menu != null) {
                player.showMenu(menu);

                return true;
            } else {
                return false;
            }
        }

        public virtual Menu onInteractionMenu(IPlayer player) { return null; }

        public virtual TimeSpan getAutomaticDeleteTimeSpan() { return TimeSpan.FromDays(14); }

        public virtual void onInterval(TimeSpan tickLength) { }

        public virtual bool onPickUp(IPlayer player, ref Constants.NotifactionImages img) { return true; }
    }
}
