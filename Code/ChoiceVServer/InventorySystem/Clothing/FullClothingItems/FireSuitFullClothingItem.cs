using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Controller.OrderController;

namespace ChoiceVServer.InventorySystem {
    public class FireSuitFullClothingItem : FullClothingItem {
        public FireSuitFullClothingItem(item item) : base(item) { }

        public FireSuitFullClothingItem(configitem configItem, int amount, int quality) : base(configItem, amount, quality) {

        }

        public override List<(int, ClothingComponent)> getComponents(string gender) {
            var naked = Constants.NakedMen;
            if(gender == "F") {
                naked = Constants.NakedFemale;
            }

            if(gender == "M") {
                return new List<(int, ClothingComponent)> {
                    (11, new ClothingComponent(314, 0)),
                    (8, new ClothingComponent(151, 1)),
                    (3, new ClothingComponent(165, 1)),
                    (4, new ClothingComponent(120, 0)),
                    (6, new ClothingComponent(71, 0)),

                    (7, new ClothingComponent(naked.Accessories.Drawable, naked.Accessories.Texture)),
                };
            } else {
                return new List<(int, ClothingComponent)> {
                    (11, new ClothingComponent(325, 0)),
                    (8, new ClothingComponent(187, 0)),
                    (3, new ClothingComponent(206, 1)),
                    (4, new ClothingComponent(126, 0)),
                    (6, new ClothingComponent(74, 0)),

                    (7, new ClothingComponent(naked.Accessories.Drawable, naked.Accessories.Texture)),
                };
            }
        }

        public override bool allowsMaks(string gender) {
            return false;
        }

        protected override void onEquipAdditional(IPlayer player) {
            player.toggleFireProof(true);
        }

        protected override void onUnequipAdditional(IPlayer player) {
            player.toggleFireProof(false);
        }
    }
}
