using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Controller.OrderSystem;
using ChoiceVServer.Controller.OrderSystem.OrderComponents.OrderItems;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static ChoiceVServer.Base.Constants;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace ChoiceVServer.InventorySystem {
    public class TopClothingController : ChoiceVScript {
        public TopClothingController() {
            EventController.addMenuEvent("EQUIP_TOP_CLOTHING_ITEM", onEquipTopClothingItem);
            EventController.addMenuEvent("SELECT_CLOTHING_TOP_SHIRT", onSelectClothingTopShirt);
        }

        private bool onSelectClothingTopShirt(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (TopClothingItem)data["Item"];
            var topDrawable = (int)data["TopDrawable"];
            var topTexture = (int)data["TopTexture"];
            var topDlc = (string)data["TopDlc"];
            var shirtDrawable = (int)data["ShirtDrawable"];
            var shirtTexture = (int)data["ShirtTexture"];
            var shirtDlc = (string)data["ShirtDlc"];
            var shirtName = (string)data["ShirtName"];
            var torso = (int)data["Torso"];

            if(menuItemCefEvent.action == "changed") {
                ChoiceVAPI.SetPlayerClothes(player, 11, topDrawable, topTexture, topDlc);
                ChoiceVAPI.SetPlayerClothes(player, 8, shirtDrawable, shirtTexture, shirtDlc);
                ChoiceVAPI.SetPlayerClothes(player, 3, torso, 0, null);
            } else {
                item.setNewShirt(shirtDrawable, shirtTexture, shirtDlc, shirtName, torso);
                player.sendNotification(Constants.NotifactionTypes.Info, $"Du hast das Unterteil {shirtName} ausgewählt!", "Unterteil ausgewählt", NotifactionImages.Shop);
            }

            return true;
        }

        private bool onEquipTopClothingItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (TopClothingItem)data["Item"];

            item.equip(player);

            return true;
        }
    }

    public class TopClothingItem : ClothingItem, NoKeepEquippedItem {
        public int ShirtDrawable { get => ((int)Data["ShirtDrawable"]); private set { Data["ShirtDrawable"] = value; } }
        public int ShirtTexture { get => ((int)Data["ShirtTexture"]); private set { Data["ShirtTexture"] = value; } }
        public string ShirtDlc { get => ((string)Data["ShirtDlc"]); private set { Data["ShirtDlc"] = value; } }
        public int TorsoId { get => ((int)Data["TorsoId"]); private set { Data["TorsoId"] = value; } }

        public TopClothingItem(item item) : base(item) { }
        
        //Generic Constructor for order system generation
        public TopClothingItem(configitem configItem, int amount, int quality) : base(configItem, amount, quality) { }

        public TopClothingItem(configitem configItem, int drawableId, int textureId, string name, char gender, string dlc, int shirtDrawable, int shirtVariaton, string shirtDlc, int torsoId) : base(configItem, drawableId, textureId, name, gender, dlc) {
            if(shirtDrawable != -1) {
                ShirtDrawable = shirtDrawable;
                ShirtTexture = shirtVariaton;
                ShirtDlc = shirtDlc;
            }

            TorsoId = torsoId;
        }

        public override void use(IPlayer player) {
            if(changeNotAllowed(player)) {
                player.sendBlockNotification("Du kannst aktuell deine Kleidung nicht ändern!", "Kleidung nicht änderbar", Constants.NotifactionImages.System);
                return;
            }

            if(IsEquipped) {
                unequip(player);
            } else {
                if(!Data.hasKey("ShirtDrawable")) {
                    equip(player);
                } else {
                    var menu = new Menu("Unterteil anpassen?", "Die Anzeige des Shirt bei jedem sichtbar!", (p) => {
                        ClothingShopController.resetPlayerClothing(p);
                    }, true);

                    menu.addMenuItem(new ClickMenuItem("Anziehen", "Ziehe das Unterteil an", "", "EQUIP_TOP_CLOTHING_ITEM", MenuItemStyle.green)
                        .withData(new Dictionary<string, dynamic> { { "Item", this } }));

                    var configClothing = ClothingController.getConfigClothing(ComponentId, Drawable, Gender, Dlc);

                    if(configClothing == null) {
                        player.sendBlockNotification("Es ist ein Fehler aufgetreten! Melde dich im Support Code: ClothMoth", "Code: ClothMoth");
                        return;
                    }

                    if(configClothing.torsoId != null) {
                        var nakedShirt = Constants.NakedMen.Shirt;
                        if (Gender == "F") {
                            nakedShirt = Constants.NakedFemale.Shirt;
                        }

                        menu.addMenuItem(new ClickMenuItem("Ohne Shirt", $"Wähle das Unterteil Ohne Shirt", "", "SELECT_CLOTHING_TOP_SHIRT", MenuItemStyle.normal, true)
                            .withData(new Dictionary<string, dynamic> {
                                    { "Item", this },
                                    { "TopDrawable", Drawable },
                                    { "TopTexture", Texture },
                                    { "TopDlc", Dlc },
                                    { "ShirtDrawable", nakedShirt.Drawable },
                                    { "ShirtTexture", nakedShirt.Texture },
                                    { "ShirtDlc", nakedShirt.Dlc },
                                    { "ShirtName", "Ohne Shirt" },
                                    { "Torso", configClothing.torsoId } 
                                }).needsConfirmation($"Ohne Shirt auswählen?", "Ohne Shirt auswählen?"));
                    }

                    var viableShirts = ClothingController.getCompatibleShirtsForTop(Gender, Drawable, Dlc);
                    foreach(var shirts in viableShirts.Where(s => s.shirt.notBuyable != 1).GroupBy(s => s.shirt.name).Select(a => a.ToList()).ToList()) {
                        var shirtVirtMenu = new VirtualMenu(shirts.First().shirt.name, () => {
                            var shirtVariationMenu = new Menu(shirts.First().shirt.name, "Welche Variation beifügen?");

                            foreach(var shirt in shirts) {
                                foreach(var shirtVariation in shirt.shirt.configclothingvariations.Where(v => v.overrideNotBuyable != 1)) {
                                    shirtVariationMenu.addMenuItem(new ClickMenuItem(shirtVariation.name, $"Wähle das Unterteil {shirtVariation.name}", "", "SELECT_CLOTHING_TOP_SHIRT", MenuItemStyle.normal, true)
                                    .withData(new Dictionary<string, dynamic> {
                                        { "Item", this },
                                        { "TopDrawable", Drawable },
                                        { "TopTexture", Texture },
                                        { "TopDlc", Dlc },
                                        { "ShirtDrawable", shirt.shirt.drawableid },
                                        { "ShirtTexture", shirtVariation.variation },
                                        { "ShirtDlc", shirt.shirt.dlc },
                                        { "ShirtName", shirtVariation.name },
                                        { "Torso", shirt.TorsoId } }
                                    ).needsConfirmation($"Unterteil auswählen?", "Dieses Unterteil auswählen?"));
                                }
                            }

                            return shirtVariationMenu;
                        });

                        menu.addMenuItem(new MenuMenuItem(shirtVirtMenu.Name, shirtVirtMenu));
                    }

                    player.showMenu(menu);
                }
            }
        }

        public override void equipStep(IPlayer player, bool alsoLoad = true) {
            var cl = ClothingController.getPlayerClothing(player);
            cl.UpdateClothSlot(3, TorsoId, 0);

            if(Data.hasKey("ShirtDrawable")) {
                cl.UpdateClothSlot(8, ShirtDrawable, ShirtTexture, ShirtDlc);
            } else {
                var naked = Constants.NakedMen;
                if(player.getCharacterData().Gender == 'F') {
                    naked = Constants.NakedFemale;
                }

                cl.UpdateClothSlot(8, naked.Shirt.Drawable, naked.Shirt.Texture);
            }

            base.equipStep(player);
        }

        protected override void unequipStep(IPlayer player) {
            var naked = Constants.NakedMen;
            if(player.getCharacterData().Gender == 'F') {
                naked = Constants.NakedFemale;
            }

            var cl = ClothingController.getPlayerClothing(player);
            cl.UpdateClothSlot(3, naked.Torso.Drawable, naked.Torso.Texture);
            cl.UpdateClothSlot(8, naked.Shirt.Drawable, naked.Shirt.Texture);
            base.unequipStep(player);
        }

        internal void setNewShirt(int shirtDrawable, int shirtTexture, string shirtDlc, string shirtName, int torsoId) {
            ShirtDrawable = shirtDrawable;
            ShirtTexture = shirtTexture;
            ShirtDlc = shirtDlc;
            TorsoId = torsoId;

            var search = "mit Unterteil: ";
            Description = $"{Description.Substring(0, Description.IndexOf(search) + search.Length)} {shirtName}";
        }
        
        public override void setOrderData(OrderItem orderItem) {
            base.setOrderData(orderItem);
            
            var clothingOrderItem = orderItem as OrderClothingItem;
            var configVariation = ClothingController.getClothingVariation(clothingOrderItem.ConfigElementId, clothingOrderItem.ClothingVariation);

            ComponentId = configVariation.clothing.componentid;

            if(configVariation.clothing.torsoId != null) {
                var naked = Constants.NakedMen;
                if (configVariation.clothing.gender == "F") {
                    naked = Constants.NakedFemale;
                }

                ShirtDrawable = naked.Shirt.Drawable;
                ShirtTexture = naked.Shirt.Texture;
                ShirtDlc = naked.Shirt.Dlc;
                TorsoId = configVariation.clothing.torsoId ?? 0;
            } else {
                var viableShirts = ClothingController.getCompatibleShirtsForTop(configVariation.clothing.gender, configVariation.clothing.drawableid, configVariation.clothing.dlc);

                if(viableShirts.Count > 0) {
                    var first = viableShirts.First();

                    ShirtDrawable = first.shirt.drawableid;
                    ShirtTexture = first.shirt.textureAmount;
                    ShirtDlc = first.shirt.dlc;
                    TorsoId = first.shirt.torsoId ?? 0;
                }
            }

            Description = $"({Gender}) {configVariation.clothing.name}: {configVariation.name}";

            updateDescription();
        }
    }
}
