using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.CheckBoxMenuItem;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Clothing {
    public delegate void OnConnectClothingItemCheck(IPlayer player, ref ClothingPlayer cloth);
    public delegate void OnPlayerPutOnClohtesDelegate(IPlayer player, ClothingPlayer newClothing);

    public enum ClothingType {
        Cheap = 0,
        Medium = 1,
        Expensive = 2,
        Modded = 3,
        Placeholder1 = 4,
        Placeholder2 = 5,
        Placeholder3 = 6,
        Placeholder4 = 7,
        Placeholder5 = 8,
        Placeholder6 = 9,
        Placeholder7 = 10,
        Placeholder8 = 11,
        Placeholder9 = 12,
        Placeholder10 = 13,
        Placeholder11 = 14,
        Placeholder12 = 15,
        Placeholder13 = 16,
        Placeholder14 = 17,
        Placeholder15 = 18,
        Placeholder16 = 19,
        Placeholder17 = 20,
        Placeholder18 = 21,
       Placeholder19 = 22,
    }

    public class ClothingController : ChoiceVScript {
        public static List<(int, OnConnectClothingItemCheck)> AllOnConnectClothesCheckings = new List<(int, OnConnectClothingItemCheck)>();

        private static Dictionary<int, List<configclothingcategory>> AllCategories;

        public static OnPlayerPutOnClohtesDelegate OnPlayerPutOnClothesDelegate;

        public ClothingController() {
            EventController.PlayerSuccessfullConnectionDelegate += initPlayerClothing;

            var clothingCreateMenu = new Menu("Kleidungserstellungsmenü", "Wähle den Kleidungstyp");
            clothingCreateMenu.addMenuItem(new InputMenuItem("Masken", "Masken auswählen", "StartId eingeben", "SUPPORT_CLOTH_MENU").withData(new Dictionary<string, dynamic> { { "ComponentId", 1 } }));
            clothingCreateMenu.addMenuItem(new InputMenuItem("Schuhe", "Schuhe auswählen", "StartId eingeben", "SUPPORT_CLOTH_MENU").withData(new Dictionary<string, dynamic> { { "ComponentId", 6 } }));
            clothingCreateMenu.addMenuItem(new InputMenuItem("Hosen", "Hosen auswählen", "StartId eingeben", "SUPPORT_CLOTH_MENU").withData(new Dictionary<string, dynamic> { { "ComponentId", 4 } }));
            clothingCreateMenu.addMenuItem(new InputMenuItem("Accessoires", "Accessoires auswählen", "StartId eingeben", "SUPPORT_CLOTH_MENU").withData(new Dictionary<string, dynamic> { { "ComponentId", 7 } }));
            clothingCreateMenu.addMenuItem(new InputMenuItem("Untershirts", "Untershirts auswählen", "StartId eingeben", "SUPPORT_CLOTH_MENU").withData(new Dictionary<string, dynamic> { { "ComponentId", 8 } }));
            clothingCreateMenu.addMenuItem(new InputMenuItem("Oberteile", "Oberteile auswählen", "StartId eingeben", "SUPPORT_CLOTH_MENU").withData(new Dictionary<string, dynamic> { { "ComponentId", 11 } }));

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    () => clothingCreateMenu,
                    3,
                    SupportMenuCategories.PlayerStyle,
                    "Kleidungserstellung"
                )
            );
            
            var list = new List<string> {
                "M-11", "F-11",
                "M-4", "F-4",
                "M-6", "F-6",
                "M-7", "F-7",
                "M-9", "F-9",
            };

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    () => new ListMenuItem("Kleidungsstück erstellen", "Erstelle ein Kleidungsstück", list.ToArray(), "SUPPORT_CLOTHES_SPAWN"),
                    2,
                    SupportMenuCategories.Kleidung
                )
            );
            EventController.addMenuEvent("SUPPORT_CLOTHES_SPAWN", onSupportClothesSpawn);
            EventController.addMenuEvent("SUPPORT_CREATE_CLOTHING_SUBMIT", onSupportCreateClothingSubmit);

            EventController.addMenuEvent("SUPPORT_CLOTH_MENU", onSupportClothMenu);
            EventController.addMenuEvent("SUPPORT_CLOTH_FINISH_CREATION", onSupportFinishClothCreation);
            EventController.addMenuEvent("SUPPORT_CLOTH_CHANGE_VARIATION", onSupportChangeClothVariation);

            using(var db = new ChoiceVDb()) {
                AllCategories = new Dictionary<int, List<configclothingcategory>>();
                var cats = db.configclothingcategories.ToList().GroupBy(c => c.slotId).Select(a => a.ToList()).ToList();

                foreach(var cat in cats) {
                    AllCategories.Add(cat.First().slotId, cat);
                }
            }

            EventController.addEvent("SET_NAKED", onPlayerSetNaked);
        }

        private bool onSupportClothesSpawn(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as ListMenuItemEvent;

            var split = evt.currentElement.Split("-");
            var gender = split[0];
            var componentId = int.Parse(split[1]);

            var menu = new Menu("Kleidungsstück erstellen", "");
            using(var db = new ChoiceVDb()) {
                var variations = db.configclothingvariations.Include(r => r.clothing).Where(r => r.clothing.gender == gender && r.clothing.componentid == componentId);

                menu.addMenuItem(new InputMenuItem("Variation wählen", "Wähle die Variation", "", "")
                    .withOptions(variations.Select(v => $"{v.clothing.name}: {v.name}").ToArray()));
                menu.addMenuItem(new InputMenuItem("Anzahl", "Gib die Anzahl ein", "", InputMenuItemTypes.number, "")
                    .withStartValue("1"));
                menu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle das Kleidungstück", "", "SUPPORT_CREATE_CLOTHING_SUBMIT", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic>{ {"ComponentId", componentId }, {"Gender", gender}})); 
            }

            player.showMenu(menu);

            return true;
        }

        private bool onSupportCreateClothingSubmit(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var kindEvent = evt.elements[0].FromJson<InputMenuItemEvent>();
            var amountEvent = evt.elements[1].FromJson<InputMenuItemEvent>();

            var amount = 1;
            try {
                amount = int.Parse(amountEvent.input);
            } catch(Exception) { }

            var componentId = (int)data["ComponentId"];
            var gender = (string)data["Gender"];

            var split = kindEvent.input.Split(":");
            var clothName = split[0].Trim();
            var variationName = split[1].Trim();

            using(var db = new ChoiceVDb()) {
                var variation = db.configclothingvariations.Include(r => r.clothing).FirstOrDefault(r => r.clothing.gender == gender && r.clothing.componentid == componentId && r.clothing.name == clothName && r.name == variationName);

                if(variation != null) {
                    while(amount > 0) {
                        amount--;
                        ClothingItem clothingItem;
                        if (componentId != 11) {
                            var clothCfg = InventoryController.getConfigItemForType<ClothingItem>(c => c.additionalInfo == variation.clothing.componentid.ToString());
                            clothingItem = new ClothingItem(clothCfg, variation.clothing.drawableid, variation.variation, $"({variation.clothing.gender[0]}) {variation.clothing.name}: {variation.name}", variation.clothing.gender.ToCharArray()[0], variation.clothing.dlc);
                        } else {
                            var clothCfg = InventoryController.getConfigItemForType<TopClothingItem>(i => true);
                            var shirts = ClothingController.getCompatibleShirtsForTop(gender, variation.clothing.drawableid, variation.clothing.dlc);

                            if (shirts.Count > 0) {
                                var shirt = shirts.First();
                                clothingItem = new TopClothingItem(clothCfg, variation.clothing.drawableid, variation.variation,  $"({variation.clothing.gender[0]}) {variation.clothing.name}: {variation.name}", variation.clothing.gender.ToCharArray()[0], variation.clothing.dlc,
                                shirt.shirt.drawableid, shirt.shirt.configclothingvariations.First().variation, shirt.shirt.dlc, shirt.TorsoId);

                                clothingItem.Description = $"{clothingItem.Description} mit Unterteil: {shirt.shirt.name}: {shirt.shirt.configclothingvariations.First().name}";
                                clothingItem.updateDescription();
                            } else {
                                var naked = Constants.NakedMen;
                                if (gender == "F") naked = Constants.NakedFemale;

                                clothingItem = new TopClothingItem(clothCfg, variation.clothing.drawableid, variation.variation, $"({variation.clothing.gender[0]}) {variation.clothing.name}: {variation.name}", variation.clothing.gender.ToCharArray()[0], variation.clothing.dlc,
                                naked.Shirt.Drawable, naked.Shirt.Texture, naked.Shirt.Dlc, variation.clothing.torsoId ?? 1);
                            }

                        }
                        
                        player.getInventory().addItem(clothingItem, true);
                    }
                    player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast erfolgreich {amount}x {variation.clothing.name}: {variation.name} erstellt!", "");
                }
            }

            return true;
        }

        private bool onPlayerSetNaked(IPlayer player, string eventName, object[] args) {
            var gender = args[0].ToString();
            setPlayerTempClothes(player, gender == "F" ? Constants.NakedFemale : Constants.NakedMen);

            return true;
        }

        #region Support Stuff

        private bool onSupportClothMenu(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var inputEvent = menuItemCefEvent as InputMenuItemEvent;
            var componentId = (int)data["ComponentId"];

            var startId = 0;
            try {
                startId = int.Parse(inputEvent.input);
            } catch(Exception) { }


            configclothing already;
            var namePlaceholder = "";
            var price = "";
            var pricingCategory = new string[] { "Billig", "Mittelklasse", "Teuer" };
            var cats = AllCategories[componentId];
            var buyable = true;
            using(var db = new ChoiceVDb()) {
                var gender = player.getCharacterData().Gender.ToString();
                already = db.configclothings.Include(c => c.category).FirstOrDefault(c => c.componentid == componentId && c.drawableid == startId && c.gender == gender);

                if(already != null) {
                    namePlaceholder = already.name;
                    price = already.price.ToString();
                    pricingCategory = pricingCategory.ToList().ShiftLeft(already.clothingType).ToArray();
                    while(cats.First().Name != already.category.Name) {
                        cats = cats.ShiftLeft(1);
                    }

                    buyable = already.notBuyable == 0;
                }
            }

            var newData = new Dictionary<string, dynamic> { { "ComponentId", componentId }, { "DrawableId", startId } };
            ChoiceVAPI.SetPlayerClothes(player, componentId, startId, 0);
            var menu = new Menu($"Kleidungsmenü Id: {startId}", "Stelle das Kleidungsstück ein");
            
            var variationList = new string[] { "0", "1", "2", "3" };
            menu.addMenuItem(new ListMenuItem("Variation ändern", "Zeige dir andere Variationen des Kleidungsstückes an", variationList, "SUPPORT_CLOTH_CHANGE_VARIATION", MenuItemStyle.normal, true, true).withData(newData));
            menu.addMenuItem(new InputMenuItem("Name", "Gib den Namen des Kleidungsstückes ein", namePlaceholder, ""));
            menu.addMenuItem(new InputMenuItem("Preis eingeben", "Gib den Preis des Kleidungsstückes ein", price, InputMenuItemTypes.number, ""));

            menu.addMenuItem(new ListMenuItem("Preiskategorie wählen", "Wähle die Preiskategorie", pricingCategory, ""));

            menu.addMenuItem(new ListMenuItem("Kategorie", "Wähle die generelle Kategorie", cats.Select(c => c.Name).ToArray(), ""));

            menu.addMenuItem(new CheckBoxMenuItem("Kaufbar", "Gib an ob das Kleidungsstück gekauft werden kann", buyable, ""));
            menu.addMenuItem(new MenuStatsMenuItem("Abschließen", "Schließe die Erstellung ab", "SUPPORT_CLOTH_FINISH_CREATION", MenuItemStyle.green).withData(newData));
            player.showMenu(menu, true, true);
            return true;
        }

        private bool onSupportFinishClothCreation(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var componentId = (int)data["ComponentId"];
            var drawableId = (int)data["DrawableId"];
            var gender = player.getCharacterData().Gender;
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            try {
                var nameEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
                var priceEvt = evt.elements[3].FromJson<InputMenuItemEvent>();
                var categoryEvt = evt.elements[4].FromJson<ListMenuItemEvent>();
                var catEvt = evt.elements[5].FromJson<ListMenuItemEvent>();
                var buyableEvt = evt.elements[6].FromJson<CheckBoxMenuItemEvent>();

                var name = nameEvt.input;
                var price = -1m;
                if(priceEvt.input != null) {
                    price = decimal.Parse(priceEvt.input);
                }
                var buyable = buyableEvt.check;

                var configCategory = AllCategories[componentId].FirstOrDefault(c => c.Name == catEvt.currentElement);

                var pricingCategory = new List<string> { "Billig", "Mittelklasse", "Teuer" };

                CallbackController.getTextureVariations(player, componentId, drawableId, true, (p, textures) => {
                    using(var db = new ChoiceVDb()) {
                        var gender = player.getCharacterData().Gender.ToString();
                        var already = db.configclothings.FirstOrDefault(c => c.gender == gender.ToString() && c.componentid == componentId && c.drawableid == drawableId && c.gender == gender);

                        if(already != null) {
                            if(name != null) {
                                already.name = name;
                            }

                            if(price != -1) {
                                already.price = price;
                            }
                            already.notBuyable = buyable ? 0 : 1;
                            already.textureAmount = textures;


                            already.clothingType = pricingCategory.IndexOf(categoryEvt.currentElement);
                            already.categoryId = configCategory.id;
                        } else {
                            var alreadyNames = db.configclothings.Where(c => c.name == name && c.gender == gender);

                            var dbCloth = new configclothing {
                                gender = gender.ToString(),
                                componentid = componentId,
                                drawableid = drawableId,
                                name = $"{name}",
                                nameVariation = alreadyNames.Count(),
                                price = price,
                                notBuyable = buyable ? 0 : 1,
                                textureAmount = textures,
                                clothingType = categoryEvt.currentIndex,
                                categoryId = configCategory.id,
                            };

                            db.configclothings.Add(dbCloth);
                        }

                        db.SaveChanges();
                    }

                    configclothing already2;
                    var namePlaceholder = "";
                    var priceM = "";
                    var cats = AllCategories[componentId];
                    var buyableM = true;
                    string[] pricingCategoryM = { };
                    using(var db = new ChoiceVDb()) {
                        var gender = player.getCharacterData().Gender.ToString();
                        already2 = db.configclothings.Include(c => c.category).FirstOrDefault(c => c.componentid == componentId && c.drawableid == drawableId + 1 && c.gender == gender);

                        if(already2 != null) {
                            namePlaceholder = already2.name;
                            priceM = already2.price.ToString();
                            pricingCategoryM = pricingCategory.ShiftLeft(already2.clothingType).ToArray();
                            while(cats.First().Name != already2.category.Name) {
                                cats = cats.ShiftLeft(1);
                            }

                            buyableM = already2.notBuyable == 0;
                        } else {
                            pricingCategoryM = pricingCategory.ToArray();
                        }
                    }

                    var newData = new Dictionary<string, dynamic> { { "ComponentId", componentId }, { "DrawableId", drawableId + 1 } };
                    ChoiceVAPI.SetPlayerClothes(player, componentId, drawableId + 1, 0);
                    var menu = new Menu($"Kleidungsmenü Id: {drawableId + 1}", "Stelle das Kleidungsstück ein");
                    
                    var variationList = new string[] { "0", "1", "2", "3" };
                    menu.addMenuItem(new ListMenuItem("Variation ändern", "Zeige dir andere Variationen des Kleidungsstückes an", variationList, "SUPPORT_CLOTH_CHANGE_VARIATION", MenuItemStyle.normal, true, true).withData(newData));
                    menu.addMenuItem(new InputMenuItem("Name", "Gib den Namen des Kleidungsstückes ein", namePlaceholder, ""));
                    menu.addMenuItem(new InputMenuItem("Preis eingeben", "Gib den Preis des Kleidungsstückes ein", priceM, ""));

                    menu.addMenuItem(new ListMenuItem("Preiskategorie wählen", "Wähle die Preiskategorie", pricingCategoryM, ""));

                    menu.addMenuItem(new ListMenuItem("Kategorie", "Wähle die generelle Kategorie", cats.Select(c => c.Name).ToArray(), ""));

                    menu.addMenuItem(new CheckBoxMenuItem("Kaufbar", "Gib an ob das Kleidungsstück gekauft werden kann", buyableM, ""));
                    menu.addMenuItem(new MenuStatsMenuItem("Abschließen", "Schließe die Erstellung ab", "SUPPORT_CLOTH_FINISH_CREATION", MenuItemStyle.green).withData(newData));
                    player.showMenu(menu, true, true);
                });
            } catch(Exception) {
                player.sendBlockNotification("Etwas ist schiefgelaufen", "");
            }
            return true;
        }

        private bool onSupportChangeClothVariation(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var componentId = (int)data["ComponentId"];
            var drawableId = (int)data["DrawableId"];
            var evt = menuItemCefEvent as ListMenuItemEvent;

            var variation = evt.currentIndex;

            ChoiceVAPI.SetPlayerClothes(player, componentId, drawableId, variation);
            return true;
        }

        #endregion 

        /// <summary>
        /// Backups the current playerClothing. Restore with restorePlayerClothing
        /// </summary>
        public static ClothingPlayer getPlayerClothing(IPlayer player) {
            return player.getData(Constants.DATA_CLOTHING_SAVE);
        }

        /// <summary>
        /// Load a complete Clothing for a player. ONLY FOR TEMP USE
        /// </summary>
        public static void setPlayerTempClothes(IPlayer player, ClothingPlayer clothing, bool withSave = false) {
            foreach(var slot in ClothingPlayer.ClothingSlots) {
                var component = clothing.GetSlot(slot, false);
                ChoiceVAPI.SetPlayerClothes(player, slot, component.Drawable, component.Texture, component.Dlc);
            }

            //Setting accesooire for all slots
            foreach(var slot in ClothingPlayer.AccessoireSlots) {
                var component = clothing.GetSlot(slot, true);
                ChoiceVAPI.SetPlayerClothingProp(player, slot, component.Drawable, component.Texture, component.Dlc);
            }

            if(withSave) {
                player.setData(Constants.DATA_CLOTHING_SAVE, clothing);
            }
        }

        /// <summary>
        /// Load a complete Clothing for a player. IF only a single Component shall be replaced use setPlayerClothing of setPlayerProp method
        /// </summary>
        public static void loadPlayerClothing(IPlayer player, ClothingPlayer clothing) {
            dbSavePlayerClothing(player, clothing);
            //TODO FIND OUT HOW GTA ONLINE DOES!
            //Send edited character Styles to Player, so that e.g. Masks do not clip
            //var style = player.getCharacterData().Style;

            //if (clothing.Mask.Drawable != 0) {

            //    var newStyle = style.Clone<characterstyles>();

            //    newStyle.faceFeature_10 = 1; //Wangenbreite inside
            //    newStyle.faceFeature_8 = 1; //Wangenknochnebreite inside
            //    newStyle.faceMother = 0; //Mutter auf 0 Setzen
            //    newStyle.faceFeature_13 = -1; //Kiefer nach innen
            //    newStyle.faceFeature_4 = -1;
            //    newStyle.faceFeature_2 = 1; //Nose inside

            //    player.emitClientEvent(Constants.PlayerSetStyle, newStyle.ToJson());
            //} else {
            //    player.emitClientEvent(Constants.PlayerSetStyle, style.ToJson());
            //}

            //Setting clothing for all slots

            foreach(var slot in ClothingPlayer.ClothingSlots) {
                var component = clothing.GetSlot(slot, false);
                ChoiceVAPI.SetPlayerClothes(player, slot, component.Drawable, component.Texture, component.Dlc);
            }

            //Setting accesooire for all slots
            foreach(var slot in ClothingPlayer.AccessoireSlots) {
                var component = clothing.GetSlot(slot, true);
                ChoiceVAPI.SetPlayerClothingProp(player, slot, component.Drawable, component.Texture, component.Dlc);
            }

            player.setData(Constants.DATA_CLOTHING_SAVE, clothing);

            OnPlayerPutOnClothesDelegate?.Invoke(player, clothing);
        }

        /// <summary>
        /// Load a complete Clothing for a player from Database!
        /// </summary>
        private static void initPlayerClothing(IPlayer player, character character) {
            var cloth = getClothingFromDb(character.characterclothing);

            if(player.getInventory() != null) {
                checkClothingForItems(player, ref cloth);
            }

            loadPlayerClothing(player, cloth);

            Logger.logTrace(LogCategory.Player, LogActionType.Removed, player, "Loaded player clothing");
        }

        public static ClothingPlayer getClothingFromDb(characterclothing dbcloth) {
            var cloth = new ClothingPlayer();

            #region dbLoading Clothing

            cloth.Mask = new ClothingComponent(dbcloth.mask_drawable, dbcloth.mask_texture, dbcloth.maskDlc);
            cloth.Torso = new ClothingComponent(dbcloth.torso_drawable, dbcloth.torso_texture, dbcloth.torsoDlc);
            cloth.Top = new ClothingComponent(dbcloth.top_drawable, dbcloth.top_texture, dbcloth.topDlc);
            cloth.Shirt = new ClothingComponent(dbcloth.shirt_drawable, dbcloth.shirt_texture, dbcloth.shirtDlc);
            cloth.Legs = new ClothingComponent(dbcloth.legs_drawable, dbcloth.legs_texture, dbcloth.legsDlc);
            cloth.Feet = new ClothingComponent(dbcloth.feet_drawable, dbcloth.feet_texture, dbcloth.feetDlc);
            cloth.Bag = new ClothingComponent(dbcloth.bag_drawable, dbcloth.bag_texture, dbcloth.bagDlc);
            cloth.Accessories = new ClothingComponent(dbcloth.accessoire_drawable, dbcloth.accessoire_texture, dbcloth.accessoireDlc);
            cloth.Armor = new ClothingComponent(dbcloth.armor_drawable, dbcloth.armor_texture, dbcloth.armorDlc);
            cloth.Hat = new ClothingComponent(dbcloth.hat_drawable, dbcloth.hat_texture, dbcloth.hatDlc);
            cloth.Glasses = new ClothingComponent(dbcloth.glasses_drawable, dbcloth.glasses_texture, dbcloth.glassesDlc);
            cloth.Ears = new ClothingComponent(dbcloth.ears_drawable, dbcloth.ears_texture, dbcloth.earsDlc);
            cloth.Watches = new ClothingComponent(dbcloth.watches_drawable, dbcloth.watches_texture, dbcloth.watchesDlc);
            cloth.Bracelets = new ClothingComponent(dbcloth.bracelets_drawable, dbcloth.bracelets_texture, dbcloth.braceletsDlc);

            #endregion

            return cloth;
        }

        /// <summary>
        /// If an items need to check with the clothing if it should be equipped, add one with this funtion! Look in eg. Accessoire or ClothingOutfit!
        /// </summary>
        public static void addOnConnectClothesCheck(int zOrder, OnConnectClothingItemCheck callback) {
            AllOnConnectClothesCheckings.Add((zOrder, callback));
            AllOnConnectClothesCheckings = AllOnConnectClothesCheckings.OrderBy(c => c.Item1).ToList();
        }

        private static void checkClothingForItems(IPlayer player, ref ClothingPlayer cloth) {
            if(cloth == null) {
                return;
            }

            foreach(var (_, check) in AllOnConnectClothesCheckings) {
                check.Invoke(player, ref cloth);
            }
        }

        /// <summary>
        /// Saves a PlayerClothing to the Database !
        /// </summary>
        private static void dbSavePlayerClothing(IPlayer player, ClothingPlayer clothing) {
            using(var db = new ChoiceVDb()) {
                var dbcloth = db.characterclothings.FirstOrDefault(c => c.characterid == player.getCharacterId());

                #region dbSaving Clothing

                dbcloth.mask_drawable = clothing.Mask.Drawable;
                dbcloth.mask_texture = clothing.Mask.Texture;
                dbcloth.maskDlc = clothing.Mask.Dlc;

                dbcloth.torso_drawable = clothing.Torso.Drawable;
                dbcloth.torso_texture = clothing.Torso.Texture;
                dbcloth.torsoDlc = clothing.Torso.Dlc;

                dbcloth.top_drawable = clothing.Top.Drawable;
                dbcloth.top_texture = clothing.Top.Texture;
                dbcloth.topDlc = clothing.Top.Dlc;

                dbcloth.shirt_drawable = clothing.Shirt.Drawable;
                dbcloth.shirt_texture = clothing.Shirt.Texture;
                dbcloth.shirtDlc = clothing.Shirt.Dlc;

                dbcloth.legs_drawable = clothing.Legs.Drawable;
                dbcloth.legs_texture = clothing.Legs.Texture;
                dbcloth.legsDlc = clothing.Legs.Dlc;

                dbcloth.feet_drawable = clothing.Feet.Drawable;
                dbcloth.feet_texture = clothing.Feet.Texture;
                dbcloth.feetDlc = clothing.Feet.Dlc;

                dbcloth.bag_drawable = clothing.Bag.Drawable;
                dbcloth.bag_texture = clothing.Bag.Texture;
                dbcloth.bagDlc = clothing.Bag.Dlc;

                dbcloth.accessoire_drawable = clothing.Accessories.Drawable;
                dbcloth.accessoire_texture = clothing.Accessories.Texture;
                dbcloth.accessoireDlc = clothing.Accessories.Dlc;

                dbcloth.armor_drawable = clothing.Armor.Drawable;
                dbcloth.armor_texture = clothing.Armor.Texture;
                dbcloth.armorDlc = clothing.Armor.Dlc;

                dbcloth.hat_drawable = clothing.Hat.Drawable;
                dbcloth.hat_texture = clothing.Hat.Texture;
                dbcloth.hatDlc = clothing.Hat.Dlc;

                dbcloth.glasses_drawable = clothing.Glasses.Drawable;
                dbcloth.glasses_texture = clothing.Glasses.Texture;
                dbcloth.glassesDlc = clothing.Glasses.Dlc;

                dbcloth.ears_drawable = clothing.Ears.Drawable;
                dbcloth.ears_texture = clothing.Ears.Texture;
                dbcloth.earsDlc = clothing.Ears.Dlc;

                dbcloth.watches_drawable = clothing.Watches.Drawable;
                dbcloth.watches_texture = clothing.Watches.Texture;
                dbcloth.watchesDlc = clothing.Watches.Dlc;

                dbcloth.bracelets_drawable = clothing.Bracelets.Drawable;
                dbcloth.bracelets_texture = clothing.Bracelets.Texture;
                dbcloth.braceletsDlc = clothing.Bracelets.Dlc;

                #endregion

                db.SaveChanges();
            }
        }

        public static void resetPlayerTopData(IPlayer player) {
            player.resetData("CLOTHING_MENU_TOP");
        }

        public static configclothingvariation getClothingVariation(int clothingId, int variation) {
            configclothingvariation configClothing;
            using(var db = new ChoiceVDb()) {
                configClothing = db.configclothingvariations.Include(c => c.clothing).FirstOrDefault(v => v.clothingId == clothingId && v.variation == variation);
            }

            return configClothing;
        }

        public static configclothing getConfigClothing(int id) {
            configclothing configClothing;
            using(var db = new ChoiceVDb()) {
                configClothing = db.configclothings.Find(id);
            }

            return configClothing;
        }

        public static configclothing getConfigClothing(int slot, int drawableId, string gender, string dlc) {
            configclothing configClothing;
            using(var db = new ChoiceVDb()) {
                configClothing = db.configclothings.FirstOrDefault(c => c.componentid == slot && c.drawableid == drawableId && c.gender == gender && c.dlc == dlc);
            }

            return configClothing;
        }

        public static configclothingvariation getConfigClothingVariation(int slot, int drawableId, int variation, string gender, string dlc) {
            configclothingvariation configClothingVariation;
            using(var db = new ChoiceVDb()) {
                configClothingVariation = db.configclothingvariations.Include(c => c.clothing).FirstOrDefault(c => c.clothing.componentid == slot && c.clothing.drawableid == drawableId && c.clothing.gender == gender && c.variation == variation && c.clothing.dlc == dlc);
            }

            return configClothingVariation;
        }

        public static configclothingpropvariation getConfigPropVariation(int propId, int variation) {
            configclothingpropvariation configClothing;
            using(var db = new ChoiceVDb()) {
                configClothing = db.configclothingpropvariations.Include(c => c.prop).FirstOrDefault(v => v.propId == propId && v.variation == variation);
            }

            return configClothing;
        }


        public static ClothingPlayer getOutfitFromCharacterOutfit(characteroutfit outift) {
            var outfit = new ClothingPlayer();

            outfit.Top = new ClothingComponent(outift.top_drawable, outift.top_texture);
            outfit.Shirt = new ClothingComponent(outift.shirt_drawable, outift.shirt_texture);
            outfit.Torso = new ClothingComponent(outift.torso_drawable, outift.torso_texture);
            outfit.Legs = new ClothingComponent(outift.legs_drawable, outift.legs_texture);
            outfit.Feet = new ClothingComponent(outift.feet_drawable, outift.feet_texture);
            outfit.Accessories = new ClothingComponent(outift.accessoire_drawable, outift.accessoire_texture);

            return outfit;
        }

        public record UnderShirtCompatableReturn(bool IsCompatible, int TorsoId);
        public static UnderShirtCompatableReturn getUndershirtCompatibleWithTop(string gender, int topDrawable, string topDlc, int underShirtDrawable, string underShirtDlc) {
            using(var db = new ChoiceVDb()) {
                var shirt = db.configclothings
                    .Include(c => c.configclothingshirtstotopshirts)
                    .ThenInclude(c => c.top)
                    .FirstOrDefault(c => c.componentid == 8 && c.drawableid == underShirtDrawable && c.dlc == underShirtDlc && c.gender == gender);

                if(shirt != null) {
                    var find = shirt.configclothingshirtstotopshirts.FirstOrDefault(t => t.top.drawableid == topDrawable && t.top.dlc == topDlc && t.top.gender == gender);
                    if(find != null) {
                        return new UnderShirtCompatableReturn(true, find.otherTorso ?? -1);
                    } else {
                        return new UnderShirtCompatableReturn(false, -1);
                    }
                }
            }

            return new UnderShirtCompatableReturn(false, -1);
        }

        public record CompatibleShirtReturn(configclothing shirt, int TorsoId);
        public static List<CompatibleShirtReturn> getCompatibleShirtsForTop(string gender, int topDrawable, string topDlc) {
            using(var db = new ChoiceVDb()) {
                var top = db.configclothings
                    .Include(c => c.configclothingshirtstotoptops)
                    .ThenInclude(c => c.shirt)
                    .ThenInclude(c => c.configclothingvariations)
                    .FirstOrDefault(c => c.componentid == 11 && c.drawableid == topDrawable && c.dlc == topDlc && c.gender == gender);

                if(top != null) {
                    return top.configclothingshirtstotoptops.Where(s => s.shirt.gender == gender).Select(t => new CompatibleShirtReturn(t.shirt, t.otherTorso ?? -1)).ToList();
                } else {
                    return new List<CompatibleShirtReturn>();
                }
            }
        }

        public static int? getCompatibleTorsoForShirtsTopKombo(string gender, int topDrawable, string topDlc, int shirtDrawable, string shirtDlc) {
            using(var db = new ChoiceVDb()) {
                var top = db.configclothings
                    .Include(c => c.configclothingshirtstotoptops)
                    .ThenInclude(c => c.shirt)
                    .FirstOrDefault(c => c.componentid == 11 && c.drawableid == topDrawable && c.dlc == topDlc && c.gender == gender);

                if(top != null) {
                    var torso = top.configclothingshirtstotoptops.FirstOrDefault(s => s.shirt.drawableid == shirtDrawable && s.shirt.dlc == shirtDlc && s.shirt.gender == gender);
                    return torso?.otherTorso;
                } else {
                    return null;
                }
            }
        }

        public static string getNameForClothingVariation(int componentId, int drawableId, int variation, string dlc = null) {
            using(var db = new ChoiceVDb()) {
                var config = db.configclothingvariations.Include(c => c.clothing).FirstOrDefault(c => c.clothing.componentid == componentId && c.clothing.drawableid == drawableId && c.variation == variation && c.clothing.dlc == dlc);
                return config?.name;
            }
        }

        public static int transformUniversalClothesDrawable(string charGender, int componentId, int drawableId) {
            if(charGender == "F" && componentId == 1 && drawableId > 189) {
                return drawableId + 1;
            }

            return drawableId;
        }


       //public static List<ClothingItem> getConfigOutfit(string name, IPlayer player) {
       //   var list = new List<ClothingItem>();

       //   using(var db = new ChoiceVDb()) {
       //       var gender = player.getCharacterData().Gender.ToString();
       //       var outfit = db.configclothingsets
       //           .Include(s => s.feet)
       //           .ThenInclude(a => a.clothing)
       //           .Include(s => s.legs)
       //           .ThenInclude(a => a.clothing)
       //           .Include(s => s.accessoire)
       //           .ThenInclude(a => a.clothing)
       //           .Include(s => s.shirt)
       //           .ThenInclude(a => a.clothing)
       //           .Include(s => s.top)
       //           .ThenInclude(a => a.clothing)
       //           .FirstOrDefault(o => o.gender == gender && o.name == name);

       //       if(outfit.feetId != null) {
       //           var cfg = InventoryController.getConfigItemByCodeIdentifier("CLOTHING_SHOES");
       //           list.Add(new ClothingItem(cfg, outfit.feet.clothing.drawableid, outfit.feet.variation, outfit.feet.name, gender.ToCharArray()[0], outfit.feet.clothing.dlc));
       //       }

       //       if(outfit.legsId != null) {
       //           var cfg = InventoryController.getConfigItemByCodeIdentifier("CLOTHING_PANTS");
       //           list.Add(new ClothingItem(cfg, outfit.legs.clothing.drawableid, outfit.legs.variation, outfit.legs.name, gender.ToCharArray()[0], outfit.legs.clothing.dlc));
       //       }

       //       if(outfit.accessoire != null) {
       //           var cfg = InventoryController.getConfigItemByCodeIdentifier("CLOTHING_ACCESSOIRE");
       //           list.Add(new ClothingItem(cfg, outfit.accessoire.clothing.drawableid, outfit.accessoire.variation, outfit.accessoire.name, gender.ToCharArray()[0], outfit.accessoire.clothing.dlc));
       //       }

       //       if(outfit.top != null) {
       //           var shirtDrawable = -1;
       //           var shirtTexture = -1;
       //           string shirtDlc = null;
       //           var torsoId = outfit.top.clothing.torsoId;

       //           if(outfit.shirt != null) {
       //               shirtDrawable = outfit.shirt.clothing.drawableid;
       //               shirtTexture = outfit.shirt.variation;
       //               shirtDlc = outfit.shirt.clothing.dlc;

       //               var combo = db.configclothingshirtstotops.FirstOrDefault(c => c.shirtId == outfit.shirtId && c.topId == outfit.topId);
       //               if(combo != null) {
       //                   torsoId = combo.otherTorso;
       //               }
       //           } else {
       //               var naked = Constants.NakedMen;
       //               if(gender == "F") {
       //                   naked = Constants.NakedFemale;
       //               }

       //               shirtDrawable = naked.Shirt.Drawable;
       //               shirtTexture = naked.Shirt.Texture;
       //           }

       //           var cfg = InventoryController.getConfigItemByCodeIdentifier("CLOTHING_TOP");
       //           var topItem = new TopClothingItem(cfg, outfit.top.clothing.drawableid, outfit.top.variation, outfit.top.name, gender.ToCharArray()[0], outfit.top.clothing.dlc,
       //               shirtDrawable, shirtTexture, shirtDlc,
       //               torsoId ?? 0);

       //           if(outfit.shirt != null) {
       //               topItem.Description = $"{topItem.Description} mit Unterteil: {outfit.shirt.name}";
       //               topItem.updateDescription();
       //           }

       //           list.Add(topItem);
       //       }


       //       //if(outfit != null) {
       //       //    var feetCfg = InventoryController.getConfigItemByCodeIdentifier("CLOTHING_SHOES");
       //       //    list.Add(new ClothingItem(feetCfg, outfit.feet_drawable, outfit.feet_texture, ));
       //       //    cloth.Torso = new ClothingComponent(outfit.torso_drawable, outfit.torso_texture);
       //       //    cloth.Top = new ClothingComponent(outfit.top_drawable, outfit.top_texture);
       //       //    cloth.Shirt = new ClothingComponent(outfit.shirt_drawable, outfit.shirt_texture);
       //       //    cloth.Accessories = new ClothingComponent(outfit.accessoire_drawable, outfit.accessoire_texture);
       //       //    cloth.Legs = new ClothingComponent(outfit.legs_drawable, outfit.legs_texture);
       //       //    cloth.Feet = new ClothingComponent(outfit.feet_drawable, outfit.feet_texture);
       //       //}
       //   }

       //   return list;
       //}
    }
}