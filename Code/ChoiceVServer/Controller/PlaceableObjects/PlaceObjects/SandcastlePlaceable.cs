using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Controller.SoundSystem;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.PlaceableObjects {
    public class SandcastlePlaceable : PlaceableObject {
        public SandcastlePlaceable(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) { }

        private static List<string> ModelOptions = new List<string> { "prop_beach_sandcas_03", "prop_beach_sandcas_04", "prop_beach_sandcas_05" };
        private string Model;

        public SandcastlePlaceable(IPlayer player, Position playerPosition, Rotation playerRotation, string model) : base(playerPosition, playerRotation, 1.2f, 1.2f, false, new Dictionary<string, dynamic>()) {
            SoundController.playSoundAtCoords(player.Position, 5f, SoundController.Sounds.SandStep, 1, "mp3");
            IntervalPlaceable = true;
            Model = model;
        }

        public override void initialize(bool register = true) {
            var d = (DegreeRotation)Rotation;
            CollisionShape.HasNoHeight = true;
            Object = ObjectController.createObject(Model, Position, d, 100, false);

            base.initialize(register);
        }

        public static string getRandomModel() {
            return ModelOptions[new Random().Next(0, ModelOptions.Count)];
        }

        public override TimeSpan getAutomaticDeleteTimeSpan() {
            return TimeSpan.FromHours(1);
        }
    }
}
