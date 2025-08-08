using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.PlaceableObjects;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem {
    public class JewelryStationController : ChoiceVScript {
        public JewelryStationController() {
            EventController.addMenuEvent("JEWELRY_STATION_CUT_ITEM", onJewelryStationCutItem);
        }

        private bool onJewelryStationCutItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (FenceJewelry)data["Item"];

            var anim = AnimationController.getAnimationByName("WORK_FRONT");
            AnimationController.animationTask(player, anim, () => {
                if(player.getInventory().removeItem(item)) {
                    player.getInventory().addItem(new CutFenceJewelery(item), true);

                    SoundController.playSoundAtCoords(player.Position, 5, SoundController.Sounds.CuttingMetal, 1, "mp3");
                    player.sendNotification(Constants.NotifactionTypes.Info, $"Du hast die/den {item.Name} erfolgreich zerkleinert!", "Schmuck zerkleinert!", Constants.NotifactionImages.FoldingTable);
                } else {
                    player.sendBlockNotification("Etwas ist schiefgelaufen!", "Etwas schiefgelaufen");
                }
            }, null, true, 1);

            return true;
        }
    }

    public class JewelryStationUtensil : FoldTableUtensil {
        public JewelryStationUtensil(item item) : base(item) { }

        public JewelryStationUtensil(configitem configItem, int amount, int quality) : base(configItem, quality) { }

        public override bool canBePulledOff(Controller.PlaceableObjects.FoldingTable table) {
            return true;
        }

        public override List<string> getContributedFunctionalities() {
            return new List<string>();
        }

        public override List<Item> getContributedItems() {
            return new List<Item>();
        }

        public override Menu getOnTableMenu(IPlayer player, Controller.PlaceableObjects.FoldingTable table) {
            var menu = new Menu("Schmuckwerkbank", "Was möchtest du tun?");

            //TODO Juwelen rausnehmen

            foreach(var item in player.getInventory().getItems<FenceJewelry>(i => true)) {
                menu.addMenuItem(new ClickMenuItem($"{item.Name} zerkleinern", $"Zerkleinere die/den {item.Name}. {item.Description}", "", "JEWELRY_STATION_CUT_ITEM").withData(new Dictionary<string, dynamic> { { "Item", item } }).needsConfirmation($"{item.Name} zerkleinern", "Schmuck wirklich zerkleinern?"));
            }

            return menu;
        }

        public override void onTick(Controller.PlaceableObjects.FoldingTable table, TimeSpan tickLenght) { }

        public override FoldTableVisualCategory getVisualCategory() {
            return FoldTableVisualCategory.None;
        }
    }
}
