using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public class ControlCenterTransmitter : Item, InventoryAuraItem {
        public string DisplayName { get => (string)Data["DisplayName"]; set { Data["DisplayName"] = value; } }
        public TransmitterColor Color { get => (TransmitterColor)Data["Color"]; set { Data["Color"] = value; } }
        public bool Activated { get => (bool)Data["Activated"]; set { Data["Activated"] = value; } }

        public ControlCenterTransmitter(item item) : base(item) {
           if(Data.hasKey("Activated")) {
               Activated = true;
           } 
        }

        public ControlCenterTransmitter(configitem configItem, int amount, int quality) : base(configItem) { }

        public override void use(IPlayer player) {
            base.use(player);

            var menu = new Menu("Leitstellen-Transmitter einstellen", "Was möchtest du tun?");
            if(!Data.hasKey("DisplayName") || !Data.hasKey("Color")) {
                menu.addMenuItem(new StaticMenuItem("Transmitter aktivieren nicht möglich", "Es müssen noch initial Daten eingegeben werden!", ""));
            } else {
                if(Data.hasKey("Activated") && Activated) {
                    menu.addMenuItem(new ClickMenuItem("Transmitter deaktivieren", "Deaktiviere den Transmitter, er sendet dann keine Position mehr", "", "CONTROL_CENTER_TRANSMITTER_TOOGLE", MenuItemStyle.yellow).withData(new Dictionary<string, dynamic> { { "Item", this } }));
                } else {
                    menu.addMenuItem(new ClickMenuItem("Transmitter aktivieren", "Deaktiviere den Transmitter, er sendet dann keine Position mehr", "", "CONTROL_CENTER_TRANSMITTER_TOOGLE", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Item", this } }));
                }
            }

            var allowedTypes = new List<CompanyType> { CompanyType.Police, CompanyType.Fbi, CompanyType.Sheriff, CompanyType.Fire };
            if(Config.IsStressTestActive || CompanyController.hasPlayerCompanyWithPredicate(player, c => allowedTypes.Contains(c.CompanyType))) {
                var dataMenu = new Menu("Daten einstellen", "Stelle die Daten ein");
                if(Data.hasKey("DisplayName")) {
                    dataMenu.addMenuItem(new InputMenuItem("Anzeigename", "Gib den Anzeigenamen des Transmitters ein", "", "").withStartValue(DisplayName));
                } else {
                    dataMenu.addMenuItem(new InputMenuItem("Anzeigename", "Gib den Anzeigenamen des Transmitters ein", "Bisher keiner gesetzt", ""));
                }

                var list = Enum.GetValues<TransmitterColor>().Select(c => ControlCenterController.getNameForTransmitterColor(c)).ToList();

                if(Data.hasKey("Color")) {
                    var pos = list.FindIndex(a => a == ControlCenterController.getNameForTransmitterColor(Color));
                    list = list.ShiftLeft(pos);
                }

                dataMenu.addMenuItem(new ListMenuItem("Anzeigefarbe", "Wähle die Anzeigefarbe des Transmitters", list.ToArray(), ""));
                dataMenu.addMenuItem(new MenuStatsMenuItem("Daten setzen", "Setze die gewählten Daten", "CONTROL_CENTER_TRANSMITTER_SET_DATA", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Item", this } }));
                menu.addMenuItem(new MenuMenuItem(dataMenu.Name, dataMenu));
            } else {
                menu.addMenuItem(new StaticMenuItem("Keine Berechtigung Daten zu ändern", "Deine Authentfizierung erlaubt es nicht Daten zu ändern", "", MenuItemStyle.yellow));
            }

            player.showMenu(menu);
        }

        public void onEnterInventory(Inventory inventory) {
            if(Data.hasKey("Activated") && Activated) {
                if(inventory.InventoryType == InventoryTypes.Player) {
                    var player = ChoiceVAPI.FindPlayerByCharId(inventory.OwnerId, true);
                    if(player != null) {
                        ControlCenterController.addMapMarkerTransmittor(Id ?? -1, DisplayName, player, Color);
                    }
                } else if(inventory.InventoryType == InventoryTypes.Vehicle) {
                    var vehicle = ChoiceVAPI.FindVehicleById(inventory.OwnerId);
                    if(vehicle != null) {
                        ControlCenterController.addMapMarkerTransmittor(Id ?? -1, DisplayName, vehicle, Color);
                    }
                }
            }
        }

        public void onExitInventory(Inventory inventory, bool becauseOfUnload) {
            if(Data.hasKey("Activated") && Activated) {
                ControlCenterController.removeMapMarkerTransmittor(m => m.ItemId == Id);
            }
        }
    }
}
