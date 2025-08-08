//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.InventorySystem;
//using ChoiceVServer.InventorySystem.Clothing;
//using ChoiceVServer.Model;
//using ChoiceVServer.Model.Clothing;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Model.Menu;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;

//namespace ChoiceVServer.Controller.Shops {
//    class MaskShopController : ChoiceVScript {
//        public static List<configclothing> clothingList = new List<configclothing>();
//        public static List<ChoosedMask> choosedMasks = new List<ChoosedMask>();
//        public MaskShopController() {
//            using (var db = new ChoiceVDb()) {
//                clothingList = db.configclothing.ToList();
//            }

//            EventController.addCollisionShapeEvent("MASK_INTERACT", onMaskshopInteract);

//            EventController.addEvent("Clothing_Texture", onAddClothingTextures);

//            EventController.addMenuEvent("MASKSHOP_CHOOSE_MASK", onMaskChoose);
//            EventController.addMenuEvent("MASKSHOP_CHOOSE_TEXTURE", onMaskTextureChange);
//            EventController.addMenuEvent("MASKSHOP_BUY_MASK", onMaskBuy);
//            EventController.addMenuEvent("MASKSHOP_ABORT", onMaskAbort);

//        }

//        private bool onMaskBuy(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var mask = (configclothing)data["Mask"];
//            if (player.getCash() < (decimal)mask.price) {
//                player.sendNotification(Constants.NotifactionTypes.Warning, "Du hast nicht so viel Geld!", "Kein Geld");
//                return true;
//            }
//            var clothing = (ClothingPlayer)player.getData(Constants.DATA_CLOTHING_SAVE);
//            clothing.UpdateClothSlot(1, 0, 0);
//            ClothingController.loadPlayerClothing(player, clothing);
//            var configItem = InventoryController.AllConfigItems.FirstOrDefault(x => x.codeItem == "MaskItem");
//            if (configItem != null) {
//                var texture = 0;
//                if (player.hasData("MaskTexture")) {
//                    texture = player.getData("MaskTexture");
//                }
//                var maskItem = new MaskItem(configItem, mask.drawableid, texture);
//                var itemCheck = player.getInventory().addItem(maskItem);
//                if (itemCheck) {
//                    player.removeCash(mask.price);
//                    player.sendNotification(Constants.NotifactionTypes.Success, $"Maske wurde erfolgreicht für {mask.price}$ gekauft!", "Maske gekauft");
//                } else {
//                    player.sendBlockNotification("Dein Inventar ist voll!", "Volles Inventar");
//                }
//            } 
//            return true; 
//        }

//        private bool onMaskAbort(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var clothing = (ClothingPlayer)player.getData(Constants.DATA_CLOTHING_SAVE);
//            clothing.UpdateClothSlot(1, 0, 0);
//            ClothingController.loadPlayerClothing(player, clothing);
//            return true;
//        }

//        private bool onAddClothingTextures(IPlayer player, string eventName, object[] args) {
//            Int64 componentId = (long)args[0];
//            Int64 drawableId = (long)args[1];
//            Int64 textures = (long)args[2];
//            using (var db = new ChoiceVDb()) {
//                var mask = db.configclothing.FirstOrDefault(x => x.componentid == componentId && x.drawableid == drawableId);
//                if (mask != null) {
//                    mask.textureAmount = (int)textures;
//                    db.Update(mask);
//                    db.SaveChanges();
//                }
//            }
//            return true;
//        }

//        private bool onMaskTextureChange(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var mask = (configclothing)data["Mask"];
//            ListMenuItem.ListMenuItemEvent listItem = menuItemCefEvent as ListMenuItem.ListMenuItemEvent;
//            var texture = listItem.currentIndex;
//            var clothing = (ClothingPlayer)player.getData(Constants.DATA_CLOTHING_SAVE);
//            clothing.UpdateClothSlot(mask.componentid, mask.drawableid, texture);
//            ClothingController.loadPlayerClothing(player, clothing);
//            player.setData("MaskTexture", texture);
//            return true;
//        }

//        #region Shop
//        private bool onMaskChoose(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var mask = (configclothing)data["Mask"];
//            var clothing = (ClothingPlayer)player.getData(Constants.DATA_CLOTHING_SAVE);
//            clothing.UpdateClothSlot(mask.componentid, mask.drawableid, 0);
//            ClothingController.loadPlayerClothing(player, clothing);
//            if (menuItemCefEvent.action == "enter") {
//                var menu = new Menu($"{mask.name}", "Wähle deine Farbe");
//                var textures = new string[mask.textureAmount];
//                var counter = 1;
//                while (counter <= mask.textureAmount) {
//                    textures[counter - 1] = $"Farbe {counter}";
//                    counter++;
//                }
//                menu.addMenuItem(new ListMenuItem("", "", textures, "MASKSHOP_CHOOSE_TEXTURE", MenuItemStyle.normal, true).withData(new Dictionary<string, dynamic> { { "Mask", mask } }));
//                menu.addMenuItem(new ClickMenuItem("Maske kaufen", "", "", "MASKSHOP_BUY_MASK", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Mask", mask } }));
//                menu.addMenuItem(new ClickMenuItem("Kauf abbrechen", "", "", "MASKSHOP_ABORT", MenuItemStyle.red));
//                player.showMenu(menu);
//            }
//            return true;
//        }

//        private bool onMaskshopInteract(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> data) {
//            var maskShopMenu = new Menu("Maskenhändler", "Wähle aus unserem Angebot");
//            foreach (var clothing in clothingList) {
//                if (clothing.componentid == 1 && clothing.gender == "u") {
//                    maskShopMenu.addMenuItem(new ClickMenuItem($"{clothing.name}", "", $"{clothing.price}$", "MASKSHOP_CHOOSE_MASK", MenuItemStyle.normal, true).withData(new Dictionary<string, dynamic> { {"Mask", clothing } }));
//                } 
//            }
//            collisionShape.OnEntityExitShape += onEntityExit;
//            player.showMenu(maskShopMenu);
//            return true;
//        }

//        private void onEntityExit(CollisionShape shape, IEntity entity) {
//            if (entity.Type == BaseObjectType.Player) {
//                var player = (IPlayer)entity;
//                var clothing = (ClothingPlayer)player.getData(Constants.DATA_CLOTHING_SAVE);
//                clothing.UpdateClothSlot(1, 0, 0);
//                ClothingController.loadPlayerClothing(player, clothing);
//            }
//        }

//        #endregion

//        #region Helper
//        public static Menu getMaskMenu(IPlayer player, configclothing mask) {
//            var clothing = (ClothingPlayer)player.getData(Constants.DATA_CLOTHING_SAVE);
//            clothing.UpdateClothSlot(mask.componentid, mask.drawableid, 0);
//            ClothingController.loadPlayerClothing(player, clothing);
//            var maskMenu = new Menu($"{mask.name}", "Wähle das passende Muster");
//            maskMenu.addMenuItem(new ListMenuItem($"{mask.name}", "", new string[]{"Farbe 1", "Farbe 2" }, "MASKSHOP_CHOOSE_MASKTEXTURE").withData(new Dictionary<string, dynamic> { {"Mask", mask } }));
//            maskMenu.addMenuItem(new ClickMenuItem("Maske kaufen", "Kauft die Maske", $"{mask.price}", "MASKSHOP_BUY_MASK", MenuItemStyle.green));
//            maskMenu.addMenuItem(new ClickMenuItem("Andere Masken anschauen", "Zeigt das Maskensortiment", "", "MASKSHOP_SHOW_MENU", MenuItemStyle.red));
//            return maskMenu;
//        }
//        #endregion
//    }
//    public class ChoosedMask {
//        public IPlayer player { get; set; }
//        public configclothing clothing { get; set; }
//        public int textureId { get; set; }
//    }
//}
