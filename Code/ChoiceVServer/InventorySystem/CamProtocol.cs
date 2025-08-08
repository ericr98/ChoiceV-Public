using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public class CamProtocol : Item {
        public int CamId { get => (int)Data["CamId"]; set { Data["CamId"] = value; } }
        public DateTime StartTime { get => ((string)Data["StartTime"]).FromJson<DateTime>(); set { Data["StartTime"] = value.ToJson(); } }

        public CamProtocol(item item) : base(item) { }

        public CamProtocol(int camId, DateTime startTime) : base(InventoryController.getConfigItemForType<CamProtocol>()) {
            CamId = camId;
            StartTime = startTime;
        }

        public override void use(IPlayer player) {
            base.use(player);

            using(var db = new ChoiceVDb()) {
                var endTime = StartTime + TimeSpan.FromMinutes(30);
                var logs = db.camlogs.Where(c => c.camId == CamId && c.createTime > StartTime && c.createTime < endTime).ToList();

                if(logs.Count <= 0) {
                    player.sendBlockNotification("Die Aufzeichnung ist zu alt, und enthält keine lesbaren Informationen mehr. Sie kann nur noch entsorgt werden", "Protokoll ungülitg");
                } else {
                    var menu = new Menu("Kameraufzeichnung", "Was möchtest du tun?");

                    var logMenu = CamController.getCameraProtocolMenu(player, logs);
                    menu.addMenuItem(new MenuMenuItem(logMenu.Name, logMenu));

                    menu.addMenuItem(new InputMenuItem("Beschriften", "Beschrifte die Aufzeichnung", "", "CAM_PROTOCOL_NAME_ITEM", MenuItemStyle.normal, true).withData(new Dictionary<string, dynamic> { { "Item", this } }).needsConfirmation("Aufzeichnung beschriften?", "Wirklich beschriften?"));

                    player.showMenu(menu, false);
                }
            }
        }
    }
}
