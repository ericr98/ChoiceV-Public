//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.Controller;
//using ChoiceVServer.Model.Clothing;
//using ChoiceVServer.Model.Database;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace ChoiceVServer.InventorySystem {

//    public class ClothingOutfitController : ChoiceVScript {
//        public ClothingOutfitController() {
//            ClothingController.addOnConnectClothesCheck(checkForClothingOutfit);
//        }

//        private void checkForClothingOutfit(IPlayer player, ref ClothingPlayer cloth) {
//            return;

//            ClothingPlayer naked;
//            if(player.getCharacterData().Gender == 'F') {
//                naked = Constants.NakedFemale;
//            } else {
//                naked = Constants.NakedMen;
//            }

//            var cl = cloth.Clone();

//            //Clothing Sets
//            if(!cloth.equalsOtherSet(naked)) {
//                var item = player.getInventory().getItem<ClothingOutfit>(i => i.equalsOtherSet(cl, player)) as ClothingOutfit;

//                if(item != null) {
//                    item.onConnectEquip(player);
//                } else {
//                    cloth.Torso = naked.Torso;
//                    cloth.Shirt = naked.Shirt;
//                    cloth.Top = naked.Top;
//                    cloth.Legs = naked.Legs;
//                    cloth.Feet = naked.Feet;
//                    cloth.Accessories = naked.Accessories;
//                }
//            }
//        }

//        public static List<configoutfit> getOutfits(List<string> names) {
//            using(var db = new ChoiceVDb()) {
//                return db.configoutfits.Where(c => names.Contains(c.name)).ToList();
//            }
//        }
//    }

//    public class ClothingOutfit : EquipItem {
//        public string ConfigOutfit { get => ((string)Data["ConfigOutfit"]); set { Data["ConfigOutfit"] = value; } }
//        private bool ConfigOutfitSet { get => Data.hasKey("ConfigOutfit"); }

//        public ClothingComponent Top { get => ((string)Data["Top"]).FromJson<ClothingComponent>(); set { Data["Top"] = value.ToJson(); } }
//        public ClothingComponent Shirt { get => ((string)Data["Shirt"]).FromJson<ClothingComponent>(); set { Data["Shirt"] = value.ToJson(); } }
//        public ClothingComponent Torso { get => ((string)Data["Torso"]).FromJson<ClothingComponent>(); set { Data["Torso"] = value.ToJson(); } }
//        public ClothingComponent Legs { get => ((string)Data["Legs"]).FromJson<ClothingComponent>(); set { Data["Legs"] = value.ToJson(); } }
//        public ClothingComponent Feet { get => ((string)Data["Shoes"]).FromJson<ClothingComponent>(); set { Data["Shoes"] = value.ToJson(); } }
//        public ClothingComponent Accessoires { get => ((string)Data["Accessoire"]).FromJson<ClothingComponent>(); set { Data["Accessoire"] = value.ToJson(); } }

//        public string Gender { get => ((string)Data["Gender"]); set { Data["Gender"] = value; } }
//        private bool GenderSet { get => Data.hasKey("Gender"); }

//        public string Info { get => ((string)Data["Info"]); set { Data["Info"] = value; } }

//        public ClothingOutfit(item item) : base(item) {
//            EquipType = "clothes";
//        }

//        public ClothingOutfit(configitem configItem, ClothingPlayer clothing, string description, string info, char gender) : base(configItem) {
//            Top = clothing.Top;
//            Shirt = clothing.Shirt;
//            Torso = clothing.Torso;
//            Legs = clothing.Legs;
//            Feet = clothing.Feet;
//            Accessoires = clothing.Accessories;

//            Description = description;
//            Info = info;

//            EquipType = "clothes";

//            Gender = gender.ToString();
//        }

//        public ClothingOutfit(configitem configItem, characteroutfit characterClothing, char gender) : base(configItem) {
//            Top = new ClothingComponent(characterClothing.top_drawable, characterClothing.top_texture);
//            Shirt = new ClothingComponent(characterClothing.shirt_drawable, characterClothing.shirt_texture);
//            Torso = new ClothingComponent(characterClothing.torso_drawable, characterClothing.torso_texture);
//            Legs = new ClothingComponent(characterClothing.legs_drawable, characterClothing.legs_texture);
//            Feet = new ClothingComponent(characterClothing.feet_drawable, characterClothing.feet_texture);
//            Accessoires = new ClothingComponent(characterClothing.accessoire_drawable, characterClothing.accessoire_texture);

//            Description = characterClothing.name;
//            Info = characterClothing.name;

//            EquipType = "clothes";

//            Gender = gender.ToString();
//        }

//        public ClothingOutfit(configitem configItem, string configOutfit) : base(configItem, 1, -1) {
//            ConfigOutfit = configOutfit;

//            EquipType = "clothes";
//        }

//        public override void use(IPlayer player) {
//            if(GenderSet && player.getCharacterData().Gender.ToString() != Gender) {
//                player.sendBlockNotification("Das Kleidungsstück ist für ein anderes Geschlecht!", "Kleidung passt nicht", Constants.NotifactionImages.System);
//                return;
//            }

//            base.use(player);
//            var currentOutfit = player.getInventory().getItem<ClothingOutfit>(i => i.IsEquipped);
//            var dressAnim = AnimationController.getAnimationByName("DRESS_SET");
//            var undressAnim = AnimationController.getAnimationByName("UNDRESS_SET");

//            if(currentOutfit != null) {
//                CamController.checkIfCamSawAction(player, "Kleidung ausgezogen", $"Person hat aktuelle Kleidung ausgezogen");

//                AnimationController.animationTask(player, undressAnim, () => {
//                    AnimationController.animationTask(player, dressAnim, () => {

//                        var cl = ClothingController.getPlayerClothing(player);
//                        if(!ConfigOutfitSet) {
//                            cl.Top = Top; cl.Shirt = Shirt; cl.Torso = Torso; cl.Legs = Legs; cl.Feet = Feet; cl.Accessories = Accessoires;
//                        } else {
//                            var outfit = getConfigOutfit(ConfigOutfit, player.getCharacterData().Gender);
//                            cl.Top = new ClothingComponent(outfit.top_drawable, outfit.top_texture); cl.Shirt = new ClothingComponent(outfit.shirt_drawable, outfit.shirt_texture); cl.Torso = new ClothingComponent(outfit.torso_drawable, outfit.torso_texture); cl.Legs = new ClothingComponent(outfit.legs_drawable, outfit.legs_texture); cl.Feet = new ClothingComponent(outfit.feet_drawable, outfit.feet_texture); cl.Accessories = new ClothingComponent(outfit.accessoire_drawable, outfit.accessoire_texture);
//                        }

//                        ClothingController.loadPlayerClothing(player, cl);

//                        currentOutfit.unequipDeactivateFunctions(player);
//                        fastEquip(player);

//                        CamController.checkIfCamSawAction(player, "Kleidung angezogen", $"Person hat neue Kleidung angezogen");
//                    });
//                });
//            } else {
//                equip(player);
//            }
//        }

//        public override void equip(IPlayer player) {
//            if(GenderSet && player.getCharacterData().Gender.ToString() != Gender) {
//                player.sendBlockNotification("Das Kleidungsstück ist für ein anderes Geschlecht!", "Kleidung passt nicht", Constants.NotifactionImages.System);
//                return;
//            }

//            var currentOutfit = player.getInventory().getItem<ClothingOutfit>(i => i.IsEquipped);
//            var dressAnim = AnimationController.getAnimationByName("DRESS_SET");
//            var undressAnim = AnimationController.getAnimationByName("UNDRESS_SET");

//            if(currentOutfit == null) {
//                AnimationController.animationTask(player, dressAnim, () => {
//                    var cl = ClothingController.getPlayerClothing(player);

//                    if(!ConfigOutfitSet) {
//                        cl.Top = Top; cl.Shirt = Shirt; cl.Torso = Torso; cl.Legs = Legs; cl.Feet = Feet; cl.Accessories = Accessoires;
//                    } else {
//                        var outfit = getConfigOutfit(ConfigOutfit, player.getCharacterData().Gender);
//                        cl.Top = new ClothingComponent(outfit.top_drawable, outfit.top_texture); cl.Shirt = new ClothingComponent(outfit.shirt_drawable, outfit.shirt_texture); cl.Torso = new ClothingComponent(outfit.torso_drawable, outfit.torso_texture); cl.Legs = new ClothingComponent(outfit.legs_drawable, outfit.legs_texture); cl.Feet = new ClothingComponent(outfit.feet_drawable, outfit.feet_texture); cl.Accessories = new ClothingComponent(outfit.accessoire_drawable, outfit.accessoire_texture);
//                    }

//                    ClothingController.loadPlayerClothing(player, cl);
//                    CamController.checkIfCamSawAction(player, "Kleidung angezogen", $"Person hat neue Kleidung angezogen");
//                });
//            }

//            base.equip(player);
//        }

//        public override void unequip(IPlayer player) {
//            var currentOutfit = player.getInventory().getItem<ClothingOutfit>(i => i.IsEquipped);
//            var dressAnim = AnimationController.getAnimationByName("DRESS_SET");
//            var undressAnim = AnimationController.getAnimationByName("UNDRESS_SET");

//            ClothingPlayer naked;
//            if(player.getCharacterData().Gender == 'F') {
//                naked = Constants.NakedFemale;
//            } else {
//                naked = Constants.NakedMen;
//            }

//            CamController.checkIfCamSawAction(player, "Kleidung ausgezogen", $"Person hat aktuelle Kleidung ausgezogen");
//            AnimationController.animationTask(player, undressAnim, () => {
//                var cl = ClothingController.getPlayerClothing(player);
//                cl.Top = naked.Top; cl.Shirt = naked.Shirt; cl.Torso = naked.Torso; cl.Legs = naked.Legs; cl.Feet = naked.Feet; cl.Accessories = naked.Accessories;
//                ClothingController.loadPlayerClothing(player, cl);
//            });

//            base.unequip(player);
//        }

//        public override void fastUnequip(IPlayer player) {
//            var currentOutfit = player.getInventory().getItem<ClothingOutfit>(i => i.IsEquipped);
//            var dressAnim = AnimationController.getAnimationByName("DRESS_SET");
//            var undressAnim = AnimationController.getAnimationByName("UNDRESS_SET");

//            ClothingPlayer naked;
//            if(player.getCharacterData().Gender == 'F') {
//                naked = Constants.NakedFemale;
//            } else {
//                naked = Constants.NakedMen;
//            }
//            var cl = ClothingController.getPlayerClothing(player);
//            cl.Top = naked.Top; cl.Shirt = naked.Shirt; cl.Torso = naked.Torso; cl.Legs = naked.Legs; cl.Feet = naked.Feet; cl.Accessories = naked.Accessories;
//            ClothingController.loadPlayerClothing(player, cl);

//            base.fastUnequip(player);
//        }

//        //public override void fastEquip(IPlayer player) {
//        //    var currentOutfit = player.getInventory().getItem<ClothingOutfit>(i => i.IsEquipped);
//        //    var dressAnim = AnimationController.getAnimationByName("DRESS_SET");
//        //    var undressAnim = AnimationController.getAnimationByName("UNDRESS_SET");

//        //    if(currentOutfit == null) {
//        //        var cl = ClothingController.getPlayerClothing(player);

//        //        if(!ConfigOutfitSet) {
//        //            cl.Top = Top; cl.Shirt = Shirt; cl.Torso = Torso; cl.Legs = Legs; cl.Feet = Feet; cl.Accessories = Accessoires;
//        //        } else {
//        //            var outfit = getConfigOutfit(ConfigOutfit, player.getCharacterData().Gender);
//        //            cl.Top = new ClothingComponent(outfit.top_drawable, outfit.top_texture); cl.Shirt = new ClothingComponent(outfit.shirt_drawable, outfit.shirt_texture); cl.Torso = new ClothingComponent(outfit.torso_drawable, outfit.torso_texture); cl.Legs = new ClothingComponent(outfit.legs_drawable, outfit.legs_texture); cl.Feet = new ClothingComponent(outfit.feet_drawable, outfit.feet_texture); cl.Accessories = new ClothingComponent(outfit.accessoire_drawable, outfit.accessoire_texture);
//        //        }

//        //        ClothingController.loadPlayerClothing(player, cl);
//        //    }

//        //    base.fastEquip(player);
//        //}

//        public virtual void unequipDeactivateFunctions(IPlayer player) {
//            IsEquipped = false;
//        }

//        public bool equalsOtherSet(ClothingPlayer set, IPlayer player) {
//            if(ConfigOutfitSet) {
//                var outfit = getConfigOutfit(ConfigOutfit, player.getCharacterData().Gender);

//                return set.Top.Drawable == outfit.top_drawable && set.Top.Texture == outfit.top_texture
//                     && set.Shirt.Drawable == outfit.shirt_drawable && set.Shirt.Texture == outfit.shirt_texture
//                     && set.Legs.Drawable == outfit.legs_drawable && set.Legs.Texture == outfit.legs_texture
//                     && set.Feet.Drawable == outfit.feet_drawable && set.Feet.Texture == outfit.feet_texture
//                     && set.Torso.Drawable == outfit.torso_drawable && set.Torso.Texture == outfit.torso_texture
//                     && set.Accessories.Drawable == outfit.accessoire_drawable && set.Accessories.Texture == outfit.accessoire_texture;
//            } else {
//                return Torso.Equals(set.Torso) && Shirt.Equals(set.Shirt) && Top.Equals(set.Top) && Legs.Equals(set.Legs) && Feet.Equals(set.Feet) && Accessoires.Equals(set.Accessories);
//            }
//        }

//        public static ClothingOutfit getConfigOutfit(string name, IPlayer player) {
//            var item = InventoryController.getConfigItemForType(typeof(ClothingOutfit));
//            using(var db = new ChoiceVDb()) {
//                var gender = player.getCharacterData().Gender.ToString();
//                var outfit = db.configoutfits.FirstOrDefault(o => o.gender == gender && o.name == name);

//                if(outfit != null) {
//                    var cloth = new ClothingPlayer();
//                    cloth.Torso = new ClothingComponent(outfit.torso_drawable, outfit.torso_texture);
//                    cloth.Top = new ClothingComponent(outfit.top_drawable, outfit.top_texture);
//                    cloth.Shirt = new ClothingComponent(outfit.shirt_drawable, outfit.shirt_texture);
//                    cloth.Accessories = new ClothingComponent(outfit.accessoire_drawable, outfit.accessoire_texture);
//                    cloth.Legs = new ClothingComponent(outfit.legs_drawable, outfit.legs_texture);
//                    cloth.Feet = new ClothingComponent(outfit.feet_drawable, outfit.feet_texture);

//                    return new ClothingOutfit(item, cloth, outfit.description, outfit.info, outfit.gender.ToCharArray()[0]);
//                } else {
//                    return new ClothingOutfit(item, new ClothingPlayer(), "Standard Outfit", "Standard Outfit", gender.ToCharArray()[0]);
//                }

//            }
//        }

//        public static configoutfit getConfigOutfit(string name, char gender) {
//            var item = InventoryController.getConfigItemForType(typeof(ClothingOutfit));
//            using(var db = new ChoiceVDb()) {
//                var outfit = db.configoutfits.FirstOrDefault(o => o.gender == gender.ToString() && o.name == name);

//                return outfit;
//            }
//        }

//        public void updateSimpleSlot(int slotId, int drawableId, int textureId) {
//            switch(slotId) {
//                case 4:
//                    Legs = new ClothingComponent(drawableId, textureId);
//                    return;
//                case 6:
//                    Feet = new ClothingComponent(drawableId, textureId);
//                    return;
//                case 7:
//                    Accessoires = new ClothingComponent(drawableId, textureId);
//                    return;
//                case 11:
//                    Top = new ClothingComponent(drawableId, textureId);
//                    return;
//                case 8:
//                    Shirt = new ClothingComponent(drawableId, textureId);
//                    return;
//                case 3:
//                    Torso = new ClothingComponent(drawableId, textureId);
//                    return;
//            }
//        }

//        public ClothingComponent getSimpleSlot(int slotId) {
//            switch(slotId) {
//                case 4:
//                    return Legs;
//                case 6:
//                    return Feet;
//                case 7:
//                    return Accessoires;
//                case 11:
//                    return Top;
//                case 8:
//                    return Shirt;
//                case 3:
//                    return Torso;
//                default:
//                    return null;
//            }
//        }

//        public void updateTopSlot(int torsoId, int torsoTexture, int topId, int topTexture, int underShirtId, int underShirtTexture) {
//            Torso = new ClothingComponent(torsoId, torsoTexture);
//            Top = new ClothingComponent(topId, topTexture);
//            Shirt = new ClothingComponent(underShirtId, underShirtTexture);
//        }

//        public virtual void onConnectEquip(IPlayer player) {
//            IsEquipped = true;
//        }

//        public override string getInfo() {
//            return Info;
//        }
//    }
//}
