using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem {
    public class Backpack : EquipItem {
        public Inventory BackpackContent { get => InventoryController.loadInventory((int)Data["BackpackContent"]); set => Data["BackpackContent"] = value.Id; }

        public Backpack(item item) : base(item) {
            if(BackpackContent == null) {
                destroy();
                return;
            }

            Weight = BackpackContent.getCombinedWeight();
            updateDescription();
        }

        public Backpack(configitem configItem, int amount, int quality) : base(configItem, null) {
            BackpackContent = InventoryController.createInventory(Id ?? 0, 8, InventoryTypes.Backpack);
        }

        public Backpack(configitem configItem, Inventory inventory) : base(configItem) {
            if(inventory != null) {
                BackpackContent = inventory;
            } else {
                BackpackContent = InventoryController.createInventory(Id ?? 0, 8, InventoryTypes.Backpack);
            }
        }

        public override void use(IPlayer player) {
            var menu = new Menu("Rucksack", "Was möchtest du tun?");
            var data = new Dictionary<string, dynamic> { { "Backpack", this } };

            if(IsEquipped) {
                menu.addMenuItem(new ClickMenuItem("Rucksack öffnen", "Öffne den Rucksack. Lege Sachen hinein oder hebe sie auf", "", "OPEN_BACKPACK").withData(data));
                menu.addMenuItem(new ClickMenuItem("Absetzen", "Setze den Rucksack ab. Packt den Rucksack ins Inventar", "", "PUT_OFF_BACKPACK").withData(data));
                menu.addMenuItem(new ClickMenuItem("Auf Boden legen", "Lege den Rucksack auf den Boden. Jemand anderes kann ihn aufnehmen. Der Rucksack despawnt 14 Tage nachdem niemand mehr mit ihm interagiert hat!", "", "PUT_BACKPACK_ON_GROUND").withData(data));
            } else {
                menu.addMenuItem(new ClickMenuItem("Rucksack ansehen", "Schaue in den Rucksack hinein", "", "SHOW_BACKPACK").withData(data));
                menu.addMenuItem(new ClickMenuItem("Anziehen", "Setze den Rucksack auf. Verteilt sein Gewicht auf dem Rücken", "", "PUT_ON_BACKPACK").withData(data));
                menu.addMenuItem(new ClickMenuItem("Auf Boden legen", "Lege den Rucksack auf den Boden. Jemand anderes kann ihn aufnehmen. Der Rucksack despawnt 14 Tage nachdem niemand mehr mit ihm interagiert hat!", "", "PUT_BACKPACK_ON_GROUND").withData(data));
            }

            player.showMenu(menu);
        }

        public override void updateDescription() {
            var desc = "Enthält u.a:";
            foreach(var item in BackpackContent.getAllItems()) {
                if(desc.Length < 65) {
                    if(!desc.Contains(item.Name)) {
                        desc += " " + item.getInfo() + ",";
                    }
                } else {
                    desc += " ...";
                    break;
                }
            }

            if(desc[desc.Length - 1] == ',') {
                desc = desc.Remove(desc.Length - 1);
            }

            Description = desc;
        }

        public override void destroy(bool toDb = true) {
            InventoryController.destroyInventory(BackpackContent);
            base.destroy(toDb);
        }
    }
}
