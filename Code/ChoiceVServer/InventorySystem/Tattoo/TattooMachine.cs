using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem {
    public class TattooMachine : Item {
        public bool FreshNeedle { get => (bool)Data["FreshNeedle"]; set { Data["FreshNeedle"] = value; } }
        public bool FreshColorSet { get => (bool)Data["FreshColorSet"]; set { Data["FreshColorSet"] = value; } }

        public TattooMachine(item item) : base(item) { }

        //Constructor for generic generation
        public TattooMachine(configitem configItem, int amount, int quality) : base(configItem) {
            FreshNeedle = true;
            FreshColorSet = true;
        }

        public override void use(IPlayer player) {
            var menu = new Menu("Tattoo Maschine", "Bereite die Tattoo Maschine vor");

            menu.addMenuItem(new ClickMenuItem("Machine inspizieren", "Inspiziere die Maschine", "", "TATTOO_MACHINE_INSPECT").withData(new Dictionary<string, dynamic> { { "Item", this } }));
            var needleItem = player.getInventory().getItem<StaticItem>(n => n.AdditionalInfo == "TattooNeedle");
            if(needleItem != null) {
                var data = new Dictionary<string, dynamic> { { "Type", "Needle" }, { "Item", this } };
                menu.addMenuItem(new ClickMenuItem("Tattoonadel wechseln", "Wechlse die Tattoonadel", "", "TATTOO_MACHINE_EDIT").needsConfirmation("Nadel wechseln?", "Nadel wirklich wechseln?").withData(data));
            }

            var colorItem = player.getInventory().getItem<StaticItem>(n => n.AdditionalInfo == "TattooColor");
            if(colorItem != null) {
                var data = new Dictionary<string, dynamic> { { "Type", "Color" }, { "Item", this } };
                menu.addMenuItem(new ClickMenuItem("Tattoofarbset wechseln", "Wechlse das Tattoofarbset", "", "TATTOO_MACHINE_EDIT").needsConfirmation("Farbe wechseln?", "Farbe wirklich wechseln?").withData(data));
            }

            player.showMenu(menu);
        }
    }
}
