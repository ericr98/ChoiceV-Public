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
    public class EvidenceBox : Item {
        public int BoxId { get => (int)Data["Id"]; set => Data["Id"] = value; }
        public Inventory BoxContent { get => InventoryController.loadInventory((int)Data["InventoryId"]); set => Data["InventoryId"] = value.Id; }

        public EvidenceBox(item item) : base(item) {
            updateDescription();

            BoxContent.addBlockStatement(new InventoryAddBlockStatement(this, (i) => i is EvidenceBox || i is not Evidence || !i.Data.hasKey("EVIDENCE_ID")));

            setWeight();
        }

        public EvidenceBox(configitem configItem, int amount, int quality) : base(configItem) {
            BoxId = getNextEvidenceBoxId();
            BoxContent = InventoryController.createInventory(BoxId, 100, InventoryTypes.EvidenceBox);

            BoxContent.addBlockStatement(new InventoryAddBlockStatement(this, (i) => i is EvidenceBox || i is not Evidence || !i.Data.hasKey("EVIDENCE_ID")));

            updateDescription();

            setWeight();
        }

        public override void use(IPlayer player) {
            base.use(player);

            //Evidence Box Menu Stuff is in EvidenceController

            var menu = new Menu("Beweis Kiste", "Beweiskiste: " + BoxId);
            menu.addMenuItem(new ClickMenuItem("Beweise sammeln", "Lege alle Beweise in deinem Inventar in die Kiste", "", "COLLECT_ALL_EVIDENCE").withData(new Dictionary<string, dynamic> { { "EvidenceBox", this } }));
            var evidenceMenu = new Menu("Beweise Details", "Alle Beweise in der Kiste");
            foreach(var item in BoxContent.getItems<Evidence>(i => true)) {
                var itemMenu = item.createEvidenceMenu(true, this);
                evidenceMenu.addMenuItem(new MenuMenuItem(itemMenu.Name, itemMenu));
            }

            menu.addMenuItem(new MenuMenuItem(evidenceMenu.Name, evidenceMenu));

            menu.addMenuItem(new ClickMenuItem("Beweis-Kiste öffnen", "Öffne die Beweiskiste. Es können nur Beweise oder als Beweis markierte Gegenstände hineingelegt werden!", "", "OPEN_EVIDENCE_BOX").withData(new Dictionary<string, dynamic> { { "EvidenceBox", this } }));

            player.showMenu(menu);
        }

        public static int getNextEvidenceBoxId() {
            using(var db = new ChoiceVDb()) {
                var dItem = db.configitems.FirstOrDefault(i => i.codeItem == typeof(EvidenceBox).Name);
                var cItem = InventoryController.getConfigItemForType<EvidenceBox>();

                var value = int.Parse(cItem.additionalInfo);
                value += 1;

                cItem.additionalInfo = value.ToString();
                dItem.additionalInfo = value.ToString();

                db.SaveChanges();

                return value;
            }
        }

        public override void updateDescription() {
            var count = BoxContent.getCount();

            Description = $"Beweiskiste: {BoxId}, Es befinden sich {count} Beweis(e) in der Kiste";
        }

        public override void destroy(bool toDb = true) {
            InventoryController.destroyInventory(BoxContent);
            base.destroy(toDb);
        }

        public void setWeight() {
            Weight = Math.Min(8, 0.5f + BoxContent.CurrentWeight);
        }
    }
}
