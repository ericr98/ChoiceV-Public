using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Menu;
using ChoiceVServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.InventorySystem;

namespace ChoiceVServer.Controller.PlaceableObjects {
    public class SoundPlaceable : PlaceableObject {
        protected int ConfigId { get => (int)Data["ConfigId"]; set { Data["ConfigId"] = value; } }
        protected string SoundSourceIdentifier { get => (string)Data["SoundSourceIdentifier"]; set { Data["SoundSourceIdentifier"] = value; } }
        protected string SoundSource { get => (string)Data["SoundSource"]; set { Data["SoundSource"] = value; } }
        protected string SoundMount { get => (string)Data["SoundMount"]; set { Data["SoundMount"] = value; } }
        protected float Volume { get => (float)Data["Volume"]; set { Data["Volume"] = value; } }
        protected float VolumeModifier { get => (float)Data["VolumeModifier"]; set { Data["VolumeModifier"] = value; } }
        protected float Distance { get => (float)Data["Distance"]; set { Data["Distance"] = value; } }
        protected bool Loop { get => (bool)Data["Loop"]; set { Data["Loop"] = value; } }

        protected bool Active { get => Data.hasKey("Active") ? (bool)Data["Active"]: false; set { Data["Active"] = value; } }

        protected int SoundEventId;

        public SoundPlaceable(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) { }

        public SoundPlaceable(string model, Item placeableItem, IPlayer player, Position playerPosition, Rotation playerRotation) : base(playerPosition, playerRotation, 2f, 2f, true, new Dictionary<string, dynamic>()) {
            ConfigId = placeableItem.ConfigId;
            Active = false;
        }

        public override void initialize(bool register = true) {
            base.initialize(register);

            if(Active) {
                SoundEventId = SoundController.createPositionSoundEvent(Position, SoundSourceIdentifier, SoundSource, SoundMount, Volume * VolumeModifier, Distance, Loop);
            }
        }

        protected void activate() {
            if(!Active) {
                Active = true;
                SoundEventId = SoundController.createPositionSoundEvent(Position, SoundSourceIdentifier, SoundSource, SoundMount, Volume, Distance, Loop);
            }
        }

        protected void deactivate() {
            if(Active) {
                Active = false;
                SoundController.removeSoundEvent(SoundEventId);
            }
        }

        public override bool onPickUp(IPlayer player, ref Constants.NotifactionImages img) {
            var configItem = InventoryController.getConfigById(ConfigId);
            return player.getInventory().addItem(new PlaceableObjectItem(configItem));
        }

        public override TimeSpan getAutomaticDeleteTimeSpan() {
            return TimeSpan.FromDays(4);
        }
    }
}
