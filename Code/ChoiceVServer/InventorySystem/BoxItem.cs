using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using Newtonsoft.Json;
using System.Collections.Generic;
using ChoiceVServer.Model.Menu;
using System;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using Bogus.DataSets;

namespace ChoiceVServer.InventorySystem {
    public class BoxItemController : ChoiceVScript {
        public BoxItemController() {
            EventController.addMenuEvent("ON_OPEN_ITEM_BOX", onNoteOpenFileBox);
            EventController.addMenuEvent("ON_RENAME_ITEM_BOX", onNoteRenameFileBox);
        }

        private bool onNoteOpenFileBox(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (BoxItem)data["Item"];

            InventoryController.showMoveInventory(player, player.getInventory(), item.BoxContent, null, (p) => {
                item.updateWeight();
            }, item.Name, true);

            return true; 
        }

        private bool onNoteRenameFileBox(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (BoxItem)data["Item"];

            var evt = menuItemCefEvent as InputMenuItemEvent;

            if(evt.input.Length > 65) {
                player.sendBlockNotification("Die Beschriftung ist zu lang! Maximal 65 Zeichen!", "Maximal 65 Zeichen");
                return true;
            }

            item.Label = evt.input;

            item.updateDescription();

            player.sendNotification(Constants.NotifactionTypes.Info, $"Box erfolgreich mit \"{evt.input}\" beschriftet!", "Box beschriftet", Constants.NotifactionImages.Package);

            return true;
        }
    }

    public class BoxItem : Item {
        private readonly float InitialWeight;
        public string Label {get => Data.hasKey("Label") ? (string)Data["Label"] : ""; set => Data["Label"] = value; }
        public Inventory BoxContent { get => InventoryController.loadInventory((int)Data["InventoryId"]); set => Data["InventoryId"] = value.Id; }

        public BoxItem(item item) : base(item) { 
            InitialWeight = item.config.weight;
        }

        public BoxItem(configitem cfg, int amount, int quality) : base(cfg) { 
            InitialWeight = cfg.weight;
        }

        public override void processAdditionalInfo(string info) { 
            if(!Data.hasKey("InventoryId")) {
                BoxContent = InventoryController.createInventory(Id ?? -1, 20, InventoryTypes.BoxItem);
            }

            switch(info) {
                case "FILE":
                    BoxContent.addBlockStatement(new InventoryAddBlockStatement(this, i => i is not File));
                    break;
            }
        }

        public override void use(IPlayer player) {
            base.use(player);

            var menu = new Menu(Name, "Was möchtest du tun?");
            var data = new Dictionary<string, dynamic> {
                { "Item", this }
            };

            menu.addMenuItem(new ClickMenuItem("Box öffnen", "Öffne die Box", "", "ON_OPEN_ITEM_BOX")
                .withData(data));

            menu.addMenuItem(new InputMenuItem("Box beschriften", "Beschrifte die Box um", "", "ON_RENAME_ITEM_BOX")
                .withStartValue(Label)
                .withData(data)
                .needsConfirmation("Box umbenennen?", "Wirklich umbenennen?"));

            player.showMenu(menu);
        }

        public void updateWeight() {
            Weight = InitialWeight + BoxContent.getCombinedWeight();
        }

        public override void updateDescription() {
            Description = Name;
            base.updateDescription();
        }
    }
}

