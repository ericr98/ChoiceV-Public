using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Bogus;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.PrisonSystem.Model {
    public class PrisonInmate {
        public int Id { get; private set; }
        public Prison Prison { get; internal set; }
        public int CharId { get; private set; }
        public string Name { get; set; }
        public int TimeLeftOffline { get; set; }
        public int TimeLeftOnline { get; set; }
        public bool FreeToGo { get => TimeLeftOffline <= 0 && TimeLeftOnline <= 0; }
        public bool GotFood { get; private set; }
        public int InventoryId { get; private set; }
        public bool ClearedForExit { get; set; }
        public bool Escaped { get; private set; }

        public DateTime? ReleasedDate { get; set; }
        public DateTime CreatedDate { get; private set; }
        public bool IsReleased { get => ReleasedDate.HasValue; }

        public PrisonInmate(int id, string name, IPlayer player, int timeleft, int inventoryId, bool clearedForExit, bool escaped, DateTime? releasedDate, DateTime createdDate) {
            Id = id;
            CharId = player.getCharacterId();
            Name = name;
            TimeLeftOffline = (int)(timeleft * (1 - PrisonController.TIMELEFT_ACTIVE_PERCENTAGE));
            TimeLeftOnline = (int)(timeleft * PrisonController.TIMELEFT_ACTIVE_PERCENTAGE);
            GotFood = false;

            InventoryId = inventoryId;
            ClearedForExit = clearedForExit;
            Escaped = escaped;
            ReleasedDate = releasedDate;
            CreatedDate = createdDate;
        }

        public PrisonInmate(int id, int charId, string charName, int timeLeftOnline, int timeLeftOffline, int inventoryId, bool clearedForExit, bool escaped, DateTime? releasedDate, DateTime createdDate) {
            Id = id;
            CharId = charId;
            Name = charName;
            TimeLeftOffline = timeLeftOffline;
            TimeLeftOnline = timeLeftOnline;
            GotFood = false;

            InventoryId = inventoryId;
            ClearedForExit = clearedForExit;
            Escaped = escaped;
            ReleasedDate = releasedDate;
            CreatedDate = createdDate;
        }

        public void openBelongings(IPlayer player) {
            var inventory = InventoryController.loadInventory(InventoryId);
            if(inventory != null) {
                InventoryController.showMoveInventory(player, player.getInventory(), inventory, null, null, $"Besitztümer: {Name}");
            } else {
                player.sendBlockNotification("Ein Fehler ist aufgetreten. Melde dich im Support. Code: PRISONBEAR 2.", "Kein Insasse");
            }
        }

        private IInvoke EscapeInvoke;
        public void onEscapePrisonOutline(IPlayer player, Position escapePosition) {
            EscapeInvoke?.EndSchedule();

            player.sendNotification(Constants.NotifactionTypes.Warning, "Du hast gerade das Gefängnis verlassen. Nicht mehr lange bis die automatische Videokontrolle dein Fehlen erkennt! Wenn dies ein Fehler ist gehe einfach ins Gefängnis zurück.", "Ausbruch", Constants.NotifactionImages.Prison);
            EscapeInvoke = InvokeController.AddTimedInvoke($"Prisonbreak-{CharId}", (i) => {
                if(Prison.isPositionInPrison(player.Position)) {
                    return;
                }

                playerEscapedPrison(escapePosition);
            }, TimeSpan.FromMinutes(10), false);
        }

        public void playerEscapedPrison(Position position) {
            if(ClearedForExit) {
                return;
            }

            ControlCenterController.createDispatch(DispatchType.AutomatedDispatch, $"Gefangener ist ausgebrochen", $"Insasse {Name} aus {Prison.Name} geflohen!", position, true, true);

            using(var db = new ChoiceVDb()) {
                var inmate = db.prisoninmates.Find(Id);
                inmate.escaped = true;
                db.SaveChanges();
            }
            Escaped = true;
        }

        public (bool worked, string message) canBeDeleted() {
            var inventory = InventoryController.loadInventory(InventoryId);
            if(inventory != null && inventory.getCount() > 0) {
                return (false, "Der Insasse hat noch Besitztümer im Gefängnis!");
            }

            return (true, "");
        }

        public override string ToString() {
            return $"prison: {Prison.Name}, charId: {CharId}, timeLeftOffline: {TimeLeftOffline}, timeLeftOnlne: {TimeLeftOnline}";
        }
    }
}
