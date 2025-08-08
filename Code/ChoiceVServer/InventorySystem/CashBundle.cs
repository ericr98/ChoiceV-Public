using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.InventorySystem {
    public class CashBundle : Item {
        public const decimal CASH_BUNDLE_AMOUNT = 100;

        public CashBundle(item item) : base(item) { }

        public CashBundle(configitem cfgItem, int amount) : base(cfgItem, -1, amount) {
            updateDescription();
        }

        public override void use(IPlayer player) {
            var menu = new Menu("Bündel öffnen", "Bargeld in Geldbeutel legen");
            var cash = player.getCash();
            menu.addMenuItem(new StaticMenuItem("Bargeld", $"Du hast aktuell ${cash} Bargeld", $"${cash}"));

            decimal amount = (StackAmount ?? 1) * CASH_BUNDLE_AMOUNT;

            menu.addMenuItem(new StaticMenuItem("Geld in Bündeln", $"Du hast aktuell ${amount} in Bargeldbündeln", $"${amount}"));
            menu.addMenuItem(new InputMenuItem("Bündel zu Bargeld umwandeln", $"Wandle Bündel in Bargeld um. Der angegeben Betrag wird falls nötig gerundet", "", "PLAYER_CHANGE_BUNDLE_FOR_CASH"));

            player.showMenu(menu);
        }

        public override void updateDescription() {
            Description = $"Geldscheinbündel à ${CASH_BUNDLE_AMOUNT}";
            base.updateDescription();
        }
    }
}
