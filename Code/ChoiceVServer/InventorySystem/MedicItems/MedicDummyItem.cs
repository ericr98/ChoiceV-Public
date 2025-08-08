using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using BenchmarkDotNet.Attributes;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.DamageSystem;
using ChoiceVServer.Controller.PlaceableObjects;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Model.Menu.ListMenuItem;

namespace ChoiceVServer.InventorySystem {
    public class MedicDummyItem : Item {
        public MedicDummyItem(item dbItem) : base(dbItem) { }

        public MedicDummyItem(configitem configItem, int amount, int quality) : base(configItem) {
            
        }

        public override void use(IPlayer player) {
            var menu = new Menu("Medizinische Puppe", "Was möchtest du tun?");

            var options = new List<string> { "Aufgestellt", "Hingelegt", "Auf Schulter nehmen" };

            if (player.getInventory().hasItem<Stretcher>(i => true)) {
                options.Add("Auf Trage legen");
            }

            foreach(var option in options) {
                var (mode, model, offsetPos, offsetRot, bone, animationName) = getInfoForOption(option);

                menu.addMenuItem(new ClickMenuItem(option, "Was möchtest du mit der Puppe machen?", "", "USE_MEDIC_DUMMY")
                    .withData(new Dictionary<string, dynamic> { { "Dummy", this }, { "Mode", mode }, { "Model", model }, { "OffsetPos", offsetPos}, { "OffsetRot", offsetRot }, { "Bone", bone }, { "AnimationName", animationName } }));
            }

            player.showMenu(menu);
        }

        public static (string, string, Position, DegreeRotation, int, string) getInfoForIdentifier(string identifier) {
            switch(identifier) {
                case "SHOULDER_CARRY":
                    return getInfoForOption("Auf Schulter nehmen");
                case "STRETCHER_CARRY":
                    return getInfoForOption("Auf Trage legen");
                default:
                    return (null, null, Position.Zero, Rotation.Zero, -1, null);
            }
        }

        private static (string, string, Position, DegreeRotation, int, string) getInfoForOption(string option) {
            switch(option) {
                case "Aufgestellt":
                    return ("OBJECT", "skaar_dummy_standing_male", Position.Zero, Rotation.Zero, -1, null);

                case "Hingelegt":
                    return ("OBJECT", "skaar_dummy_lying_male", Position.Zero, Rotation.Zero, -1, null);

                case "Auf Schulter nehmen":
                    return ("CARRY", "skaar_dummy_worn_male_", new Position(0.3f, -0.175f, -0.45f), new DegreeRotation(5, -5, 10), 10706, "CARRY_OBJECT");

                case "Auf Trage legen":
                    return ("CARRY", "skaar_dummy_lying_stretcher_male", new Position(0.32f, 0.7f, -0.35f), new DegreeRotation(0, 0, 180), 36029, "PUSH_DUMMY_STRETCHER");

                default:
                    return (null, null, Position.Zero, Rotation.Zero, -1, null);

            }
        }

        public static string getDefaultPlaceModel() {
            return "skaar_dummy_lying_male";
        }
    }
}
