using ChoiceVServer.Base;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.InventorySystem {
    public class ProtectiveSuitFullClothingItem : FullClothingItem {
        public ProtectiveSuitFullClothingItem(item item) : base(item) { }

        public ProtectiveSuitFullClothingItem(configitem configItem, int amount, int quality) : base(configItem, amount, quality) {

        }

        public override List<(int, ClothingComponent)> getComponents(string gender) {
            if(AdditionalInfo.StartsWith("HAZMAT_NORMAL")) {
                var split = AdditionalInfo.Split("#");
                var color = int.Parse(split[1]);

                var naked = Constants.NakedMen;
                if(gender == "F") {
                    naked = Constants.NakedFemale;
                }

                if(gender == "M") {
                    return new List<(int, ClothingComponent)> {
                        (11, new ClothingComponent(67, color)),
                        (8, new ClothingComponent(62, color)),
                        (3, new ClothingComponent(38, 0)),
                        (4, new ClothingComponent(40, color)),
                        (6, new ClothingComponent(24, 0)),

                        (7, new ClothingComponent(naked.Accessories.Drawable, naked.Accessories.Texture)),

                        (1, new ClothingComponent(46, 0))
                    };
                } else {
                    return new List<(int, ClothingComponent)> {
                        (11, new ClothingComponent(61, color)),
                        (8, new ClothingComponent(43, color)),
                        (3, new ClothingComponent(101, 0)),
                        (4, new ClothingComponent(40, color)),
                        (6, new ClothingComponent(25, 0)),

                        (7, new ClothingComponent(naked.Accessories.Drawable, naked.Accessories.Texture)),


                        (1, new ClothingComponent(46, 0))
                    };
                }
            }

            return new List<(int, ClothingComponent)>();
        }

        public override bool allowsMaks(string gender) {
            if(AdditionalInfo.StartsWith("HAZMAT_NORMAL")) {
                return false;
            }

            return false;
        }
    }
}
