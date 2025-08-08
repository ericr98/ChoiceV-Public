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
    public class ScubaSuitFullClothingItem : FullClothingItem {
        public ScubaSuitFullClothingItem(item item) : base(item) { }

        public ScubaSuitFullClothingItem(configitem configItem, int amount, int quality) : base(configItem, amount, quality) {

        }

        public override List<(int, ClothingComponent)> getComponents(string gender) {
            var naked = Constants.NakedMen;
            if(gender == "F") {
                naked = Constants.NakedFemale;
            }

            if(gender == "M") {
                return new List<(int, ClothingComponent)> {
                    (11, new ClothingComponent(243, 0)),
                    (8, new ClothingComponent(151, 1)),
                    (3, new ClothingComponent(17, 1)),
                    (4, new ClothingComponent(94, 0)),
                    (6, new ClothingComponent(67, 0)),

                    (7, new ClothingComponent(naked.Accessories.Drawable, naked.Accessories.Texture)),
                };
            } else {
                return new List<(int, ClothingComponent)> {
                    (11, new ClothingComponent(251, 0)),
                    (8, new ClothingComponent(187, 0)),
                    (3, new ClothingComponent(18, 1)),
                    (4, new ClothingComponent(97, 0)),
                    (6, new ClothingComponent(70, 0)),

                    (7, new ClothingComponent(naked.Accessories.Drawable, naked.Accessories.Texture)),
                };
            }
        }

        public override bool allowsMaks(string gender) {
            return true;
        }

        protected override void onEquipAdditional(IPlayer player) {
            player.toggleInfiniteAir(true);
            player.emitClientEvent("TOGGLE_SCUBA_MODE", true);
        }

        protected override void onUnequipAdditional(IPlayer player) {
            player.toggleInfiniteAir(false);
            player.emitClientEvent("TOGGLE_SCUBA_MODE", false);
        }
    }
}
