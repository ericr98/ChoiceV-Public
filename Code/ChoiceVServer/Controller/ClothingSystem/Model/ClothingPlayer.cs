using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.Clothing {

    public class ClothingPlayer {
        public static int[] ClothingSlots = { 1, 3, 4, 5, 6, 7, 8, 9, 11 };
        public static int[] AccessoireSlots = { 0, 1, 2, 6, 7 };

        public static readonly ClothingPlayer Empty = new ClothingPlayer();
        public static readonly ClothingComponent NoChange = new ClothingComponent(-99, 0);

        public ClothingComponent Mask { get; set; } = new ClothingComponent(0, 0);
        public ClothingComponent Torso { get; set; } = new ClothingComponent(0, 0);
        public ClothingComponent Top { get; set; } = new ClothingComponent(0, 0);
        public ClothingComponent Shirt { get; set; } = new ClothingComponent(0, 0);
        public ClothingComponent Legs { get; set; } = new ClothingComponent(0, 0);
        public ClothingComponent Feet { get; set; } = new ClothingComponent(0, 0);
        public ClothingComponent Bag { get; set; } = new ClothingComponent(0, 0);
        public ClothingComponent Accessories { get; set; } = new ClothingComponent(0, 0);
        public ClothingComponent Armor { get; set; } = new ClothingComponent(0, 0);
        public ClothingComponent Hat { get; set; } = new ClothingComponent(-1, -1);
        public ClothingComponent Glasses { get; set; } = new ClothingComponent(-1, -1);
        public ClothingComponent Ears { get; set; } = new ClothingComponent(-1, -1);
        public ClothingComponent Watches { get; set; } = new ClothingComponent(-1, -1);
        public ClothingComponent Bracelets { get; set; } = new ClothingComponent(-1, -1);

        public ClothingComponent GetSlot(int slotId, bool isAccessory) {
            if (!isAccessory) {
                switch (slotId) {
                    case 1: return Mask;
                    case 3: return Torso;
                    case 8: return Shirt;
                    case 11: return Top;
                    case 4: return Legs;
                    case 5: return Bag;
                    case 6: return Feet;
                    case 7: return Accessories;
                    case 9: return Armor;
                    default:
                        break;
                }
            } else {
                switch (slotId) {
                    case 0: return Hat;
                    case 1: return Glasses;
                    case 2: return Ears;
                    case 6: return Watches;
                    case 7: return Bracelets;
                    default:
                        break;
                }
            }
            return new ClothingComponent(0, 0);
        }

        public void UpdateClothSlot(int slotId, int variant, int texture, string dlc = null) {
            switch (slotId) {
                case 1: Mask = new ClothingComponent(variant, texture, dlc); break;
                case 3: Torso = new ClothingComponent(variant, texture, dlc); break;
                case 8: Shirt = new ClothingComponent(variant, texture, dlc); break;
                case 11: Top = new ClothingComponent(variant, texture, dlc); break;
                case 4: Legs = new ClothingComponent(variant, texture, dlc); break;
                case 6: Feet = new ClothingComponent(variant, texture, dlc); break;
                case 7: Accessories = new ClothingComponent(variant, texture, dlc); break;
                case 5: Bag = new ClothingComponent(variant, texture, dlc); break;
                case 9: Armor = new ClothingComponent(variant, texture, dlc); break;
            }
        }


        public void UpdateAccessorySlot(int slotId, int variant, int texture, string dlc = null) {
            switch (slotId) {
                case 0: Hat = new ClothingComponent(variant, texture, dlc); break;
                case 1: Glasses = new ClothingComponent(variant, texture, dlc); break;
                case 2: Ears = new ClothingComponent(variant, texture, dlc); break;
                case 6: Watches = new ClothingComponent(variant, texture, dlc); break;
                case 7: Bracelets = new ClothingComponent(variant, texture, dlc); break;
            }
        }

        public List<(bool isProp, int slotId, ClothingComponent component)> getComponentList() {
            return new List<(bool isProp, int slotId, ClothingComponent component)> {
                (false, 1, Mask),
                (false, 3, Torso),
                (false, 8, Shirt),
                (false, 11, Top),
                (false, 4, Legs),
                (false, 6, Feet),
                (false, 7, Accessories),
                (false, 5, Bag),
                (false, 9, Armor),
                (true, 0, Hat),
                (true, 1, Glasses),
                (true, 2, Ears),
                (true, 6, Watches),
                (true, 7, Bracelets) 
            };
        }

        public static ClothingPlayer NoChangeClothing() {
            return new ClothingPlayer();
        }

        public ClothingPlayer() {

        }

        public ClothingPlayer(characteroutfit dbOutfit) {
            Torso = new ClothingComponent(dbOutfit.torso_drawable, dbOutfit.torso_texture);
            Top = new ClothingComponent(dbOutfit.top_drawable, dbOutfit.top_texture);
            Shirt = new ClothingComponent(dbOutfit.shirt_drawable, dbOutfit.shirt_texture);
            Accessories = new ClothingComponent(dbOutfit.accessoire_drawable, dbOutfit.accessoire_texture);
            Legs = new ClothingComponent(dbOutfit.legs_drawable, dbOutfit.legs_texture);
            Feet = new ClothingComponent(dbOutfit.feet_drawable, dbOutfit.feet_texture);
        }

        public static ClothingPlayer FromJson(string playerClothingsString) {
            ClothingPlayer playerCothingNew = new ClothingPlayer();

            JsonConvert.PopulateObject(playerClothingsString, playerCothingNew);

            return playerCothingNew;
        }

        public ClothingPlayer Clone() {
            return FromJson(this.ToJson());
        }

        public bool equalsOtherSet(ClothingPlayer set) {
            return Torso.Equals(set.Torso) && Shirt.Equals(set.Shirt) && Top.Equals(set.Top) && Legs.Equals(set.Legs) && Feet.Equals(set.Feet) && Accessories.Equals(set.Accessories);
        }
    }

    public class ClothingPlayerTop {
        public ClothingComponent Torso { get; set; } = new ClothingComponent(0, 0);
        public ClothingComponent Top { get; set; } = new ClothingComponent(0, 0);
        public ClothingComponent Shirt { get; set; } = new ClothingComponent(0, 0);

        public ClothingPlayerTop() { }

        public static ClothingPlayerTop FromJson(string playerClothingsString) {
            ClothingPlayerTop playerClothingNew = new ClothingPlayerTop();

            JsonConvert.PopulateObject(playerClothingsString, playerClothingNew);

            return playerClothingNew;
        }
    }
}
