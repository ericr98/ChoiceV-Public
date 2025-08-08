using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.InventorySystem.Clothing;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.ClothingSystem {
    public class ClothingItemController : ChoiceVScript {
        public ClothingItemController() {
            ClothingController.addOnConnectClothesCheck(1, checkForClothingItem);
        }

        private record CheckElement(int componentId, bool isProp);
        private void checkForClothingItem(IPlayer player, ref ClothingPlayer cloth) {
            ClothingPlayer naked;
            if(player.getCharacterData().Gender == 'F') {
                naked = Constants.NakedFemale;
            } else {
                naked = Constants.NakedMen;
            }

            var gender = player.getCharacterData().Gender.ToString();

            var fullClothesItems = player.getInventory().getItems<FullClothingItem>(i => true);

            foreach(var item in fullClothesItems) {
                var worked = true;
                foreach(var (drawableId, component) in item.getComponents(player.getCharacterData().Gender.ToString())) {
                    if(!cloth.GetSlot(drawableId, false).Equals(component)) {
                        worked = false;
                        break;
                    }
                }

                if(worked) {
                    item.onConnectEquip(player);
                    return;
                }
            }

            var componentIds = new List<CheckElement>() {
                new(11, false),
                new(9, false),
                new(4, false),
                new(6, false),
                new(7, false),

                new(0, true),
                new(1, true),
                new(2, true),
                new(6, true),
                new(7, true),
            };

            foreach(var id in componentIds) {
                var clSlot = cloth.GetSlot(id.componentId, id.isProp);
                if(clSlot != naked.GetSlot(id.componentId, id.isProp)) {
                    var item = player.getInventory().getItem<ClothingItem>(c => c.Gender == gender && c.ComponentId == id.componentId && c.Drawable == clSlot.Drawable && c.Texture == clSlot.Texture && c.Dlc == clSlot.Dlc);

                    if(item != null) {
                        item.onConnectEquip(player);
                    } else {
                        if(id.isProp) {
                            cloth.UpdateAccessorySlot(id.componentId, naked.GetSlot(id.componentId, id.isProp).Drawable, naked.GetSlot(id.componentId, id.isProp).Texture);
                        } else {
                            cloth.UpdateClothSlot(id.componentId, naked.GetSlot(id.componentId, id.isProp).Drawable, naked.GetSlot(id.componentId, id.isProp).Texture);

                            if(id.componentId == 11) {
                                cloth.UpdateClothSlot(3, naked.Torso.Drawable, naked.Torso.Texture);
                                cloth.UpdateClothSlot(8, naked.Shirt.Drawable, naked.Shirt.Texture);
                            }
                        }
                    }
                }
            }
        }

        public static List<configoutfit> getOutfits(List<string> names) {
            using(var db = new ChoiceVDb()) {
                return db.configoutfits.Where(c => names.Contains(c.name)).ToList();
            }
        }
    }
}
