using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem {
    public abstract class FenceGood : Item {
        protected string SerialNumber { get => (string)Data["SerialNumber"]; set => Data["SerialNumber"] = value; }
        protected bool SerialNumberRevealed { get => (bool)Data["SerialNumberRevealed"]; set => Data["SerialNumberRevealed"] = value; }
        protected bool IsWashed { get => (bool)Data["IsWashed"]; set => Data["IsWashed"] = value; }

        private static readonly object SerialNumberLock = new();

        public FenceGood(item item) : base(item) { }

        public FenceGood(configitem configItem, int quality) : base(configItem, quality, null) {
            SerialNumber = getNextSerialNumber();

            SerialNumberRevealed = false;
        }

        private string getNextSerialNumber() {
            lock(SerialNumberLock) {
                string serial = null;
                using(var db = new ChoiceVDb()) {
                    var dbItem = db.configitems.Find(ConfigId);
                    if(dbItem != null) {
                        var split = dbItem.additionalInfo.Split('#');
                        var preFix = split[0];
                        var alreadyId = int.Parse(split[1]);
                        var nextId = alreadyId + new Random().Next(1, 1000);

                        serial = preFix + nextId.ToString("X");

                        dbItem.additionalInfo = $"{preFix}#{nextId}";

                        db.SaveChanges();
                    }
                }

                return serial;
            }
        }

        public override void use(IPlayer player) {
            var menu = new Menu(Name, "Was möchtest du tun?");

            if(SerialNumberRevealed) {
                menu.addMenuItem(new StaticMenuItem("Seriennummer", $"Die Seriennummer des Objektes ist: {SerialNumber}", $"{SerialNumber}"));
                menu.addMenuItem(new ClickMenuItem("Seriennummer verdecken", "Bringe das Objekt in einen Zustand, in dem man die Seriennummer nicht direkt erkennt", "", "ON_PLAYER_REVEAL_FENCE_GOOD", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Item", this }, { "Type", "HIDE" } }));
            } else {
                menu.addMenuItem(new ClickMenuItem("Seriennummer anzeigen", "Bringe das Objekt in einen Zustand, in dem man die Seriennummer direkt erkennbar ist", "", "ON_PLAYER_REVEAL_FENCE_GOOD", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Item", this }, { "Type", "REVEAL" } }));
            }

            player.showMenu(menu);
        }

        public override void updateDescription() {
            if(SerialNumberRevealed) {
                Description = $"Seriennummer: {SerialNumber} {Description}";
            }

            base.updateDescription();
        }
    }
}
