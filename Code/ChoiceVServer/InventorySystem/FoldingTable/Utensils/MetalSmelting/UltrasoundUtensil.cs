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
    public class UltrasoundUtensilController : ChoiceVScript {
        public UltrasoundUtensilController() {
            EventController.addMenuEvent("ULTRA_SOUND_SCAN_ITEM", onUltraSoundScanItem);
        }

        private bool onUltraSoundScanItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (IUltrasoundScannable)data["Item"];

            var anim = AnimationController.getAnimationByName("WORK_FRONT");
            AnimationController.animationTask(player, anim, () => {
                item.onUltrasoundScan();

                SoundController.playSoundAtCoords(player.Position, 5, SoundController.Sounds.ScannerSound, 1, "mp3");
                player.sendNotification(Constants.NotifactionTypes.Success, $"{item.UltrasoundScanName} erfolgreich gescannt. Es enthält: {item.UltrasoundScanDescription}", $"{item.UltrasoundScanName} gescannt");
            }, null, true, 1);

            return true;
        }
    }

    public interface IUltrasoundScannable {
        public string UltrasoundScanName { get; }
        public string UltrasoundScanDescription { get; }

        public void onUltrasoundScan();

    }

    public class UltrasoundUtensil : FoldTableUtensil {
        public UltrasoundUtensil(item item) : base(item) { }

        public UltrasoundUtensil(configitem configItem, int amount, int quality) : base(configItem, quality) { }

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
            var menu = new Menu("Ultraschall", "Was möchtest du tun?");

            foreach(var item in player.getInventory().getItems<IUltrasoundScannable>(i => true)) {
                menu.addMenuItem(new ClickMenuItem($"{item.UltrasoundScanName} scannen", $"Scanne die/den {item.UltrasoundScanName} auf die Bestandteile. {item.UltrasoundScanDescription}", "", "ULTRA_SOUND_SCAN_ITEM").withData(new Dictionary<string, dynamic> { { "Item", item } }).needsConfirmation($"{item.UltrasoundScanName} scannen", "Ware wirklich scannen?"));
            }

            return menu;
        }

        public override void onTick(Controller.PlaceableObjects.FoldingTable table, TimeSpan tickLenght) { }

        public override FoldTableVisualCategory getVisualCategory() {
            return FoldTableVisualCategory.Lab;
        }
    }
}
