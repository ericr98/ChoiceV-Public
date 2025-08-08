using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem {
    public class MapController : ChoiceVScript {
        public MapController() {
            EventController.addMenuEvent("ON_MAP_EQUIP", onMapEquip);
        }

        private bool onMapEquip(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            //call mapitem equip function: aka. mapitem.equip(player);
            throw new NotImplementedException();
        }
    }

    public class MapItem : EquipItem {
        //public string Variable { get => (string)Data["Variable"]; set { Data["Variable"] = value; } }

        public MapItem(item item) : base(item) { }

        //  /createItems 220 1 -1
        public MapItem(configitem configItem, int amount, int quality) : base(configItem, -1) { }

        public override void processAdditionalInfo(string info) {
            base.processAdditionalInfo(info);
        }

        public override void use(IPlayer player) {
            base.use(player);

            var blipName = "123";
            Data[$"BLIP_{blipName}"] = "POSITION/SPRITE";
            player.sendNotification(Constants.NotifactionTypes.Success, "Führe hier eine Aktion aus", "");

            //Generate Menu, where Map can be equipped
            var menu = new Menu("Test", "Test");

            menu.addMenuItem(new InputMenuItem("Klick", "Klick", "", "ON_MAP_EQUIP"));
            
            player.showMenu(menu);
        }

        public override void equip(IPlayer player) {
            base.equip(player);

            player.sendNotification(Constants.NotifactionTypes.Warning, "Führe hier eine andere Aktion aus", "");
        }

        public override void unequip(IPlayer player) {
            base.unequip(player);


            player.sendNotification(Constants.NotifactionTypes.Danger, "Führe hier eine Aktion aus", "");

            //Destroy Blips for player (iwas mit BlipController)
        }
    }
}
