using System;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Model.Database;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsWPF;

namespace ChoiceVServer.InventorySystem {
    public class ArmorWest : ClothingItem {
        public float Percent { get => (int)Data["Durability"]; set { Data["Durability"] = value; updateDescription(); } }

        public ArmorWest(item item) : base(item) { }

        //Constructor for generic generation
        public ArmorWest(configitem configItem, int amount, int quality) : base(configItem, amount, quality) {
            Percent = 100;
            updateDescription();
        }

        public ArmorWest(configitem configItem, int percent) : base(configItem, 1, -1) {
            Percent = percent;
            updateDescription();
        }

        public (uint newDamage, bool wentThroughWest) absorbDamage(uint damage) {
            var rand = new Random();

            //One West can hold for up to 100 damage
            float reduction = rand.Next(0, Math.Min((int)Percent / 2, 40));
            Percent -= reduction;

            //If the reduction is more the chance of the west blocking more is better
            var wentThroughWest = reduction < 0.3 && rand.NextDouble() > Percent / 100;

            if(!wentThroughWest) {
                reduction *= 0.4f;
            }

            return (Math.Max(0, Convert.ToUInt32(damage * (1 - reduction))), wentThroughWest);
        }

        public override void processAdditionalInfo(string info) {
            var split = info.Split("#");
            var configClothing = ClothingController.getClothingVariation(int.Parse(split[0]), int.Parse(split[1]));

            ComponentId = configClothing.clothing.componentid;
            Drawable = configClothing.clothing.drawableid;
            Texture = configClothing.variation;
            Gender = configClothing.clothing.gender;

            base.processAdditionalInfo(info);
        }

        public override void updateDescription() {
            double perc = Percent;

            Description = "Schutz: " + perc + "%";
        }
    }
}
