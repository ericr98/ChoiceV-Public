using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.PrisonSystem.Model {
    public class Prison {
        public int Id { get; }
        public string Name { get; }

        private List<PrisonInmate> Inmates;

        public List<CollisionShape> Outline { get; }
        public List<CollisionShape> RegisterSpots { get; private set; }
        public List<CollisionShape> BelongingsSpots { get; private set; }
        public List<PrisonGetOutPoint> GetOutPoints { get; }

        public List<PrisonCell> Cells { get; }

        public Prison(int id, string name) {
                Id = id;
                Name = name;

                Inmates = [];

                Outline = [];
                RegisterSpots = [];
                BelongingsSpots = [];
                GetOutPoints = [];

                Cells = [];
        }

        #region CollisionShapes

        public void addOutline(CollisionShape shape) {
            shape.HasNoHeight = true;
            shape.OnEntityExitShape += onPlayerExitOutline;
            Outline.Add(shape);
        }

        public void removeOutline(CollisionShape shape) {
            Outline.Remove(shape);
            shape.Dispose();
        }

        public string getOutlineString() {
            return string.Join("---", Outline.Select(c => c.toShortSave()));
        }

        public void addRegisterSpot(CollisionShape spot) {
            spot.Owner = this;
            RegisterSpots.Add(spot);
        }

        public void removeRegisterSpot(CollisionShape spot) {
            RegisterSpots.Remove(spot);
            spot.Dispose();
        }

        public string getRegisterSpotsString() {
            return string.Join("---", RegisterSpots.Select(c => c.toShortSave()));
        }

        public void addBelongingSpot(CollisionShape spot) {
            spot.OnCollisionShapeInteraction += onBelongingsSpotInteraction;
            BelongingsSpots.Add(spot);
        }

        public void removeBelongingSpot(CollisionShape spot) {
            BelongingsSpots.Remove(spot);
            spot.Dispose();
        }

        public string getBelongingsSpotsString() {
            return string.Join("---", BelongingsSpots.Select(c => c.toShortSave()));
        }

        #endregion

        public void addGetOutPoint(CollisionShape fromShape, Position toPos, string message, bool isFinalPoint) {
            GetOutPoints.Add(new PrisonGetOutPoint(this, fromShape, toPos, message, isFinalPoint));
        }

        public void removeGetOutPoint(PrisonGetOutPoint point) {
            GetOutPoints.Remove(point);
        }

        public string getGetOutPointsString() {
            return string.Join("---", GetOutPoints.Select(g => g.toShortSave()));
        }
        
        public void addCell(PrisonCell cell) {
            cell.Prison = this;
            Cells.Add(cell);
        }
        
        public void removeCell(PrisonCell cell) {
            Cells.Remove(cell);
        }
        
        public string getCellsString() {
            return string.Join("---", Cells.Select(c => c.toShortSave()));
        }

        public List<PrisonInmate> getInmates() {
            return Inmates;
        }

        public void addInmate(PrisonInmate inmate) {
            inmate.Prison = this;
            Inmates.Add(inmate);
        }

        public PrisonInmate getInmateForPlayer(IPlayer player) {
            return Inmates.FirstOrDefault(i => i.CharId == player.getCharacterId());
        }

        public bool isPlayerInPrison(IPlayer player) {
            return Inmates.Any(i => i.CharId == player.getCharacterId());
        }

        public bool isPositionInPrison(Position position) {
            return Outline.Any(o => o.IsInShape(position));
        }

        internal void imprisonPlayer(IPlayer player, string name, TimeSpan imprisonDuration) {
            var inventoryContainer = InventoryController.createInventory(player.getCharacterId(), Constants.PLAYER_INVENTORY_MAX_WEIGHT * 2, InventoryTypes.PrisonBox);

            using(var db = new ChoiceVDb()) {
                var dbInmate = new prisoninmate {
                    charId = player.getCharacterId(),
                    name = name,
                    prisonId = Id,
                    inventoryId = inventoryContainer.Id,
                    timeLeftOnline = (int)(imprisonDuration.TotalMinutes * PrisonController.TIMELEFT_ACTIVE_PERCENTAGE),
                    timeLeftOffline = (int)(imprisonDuration.TotalMinutes * (1 - PrisonController.TIMELEFT_ACTIVE_PERCENTAGE)),
                    clearedForExit = false,
                    escaped = false,
                    releasedDate = null,
                    createdDate = DateTime.Now
                };

                db.prisoninmates.Add(dbInmate);
                db.SaveChanges();

                var inmate = new PrisonInmate(dbInmate.id, name, player, (int)imprisonDuration.TotalMinutes, inventoryContainer.Id, false, false, null, dbInmate.createdDate);
                addInmate(inmate);
            }

            player.sendNotification(Constants.NotifactionTypes.Warning, $"Du wurdest in das {Name} eingesperrt. Alle deine Sachen werden nun von den Wachen eingesammelt.", "Eingesperrt worden", Constants.NotifactionImages.Prison);
        }

        internal void releaseInmate(PrisonInmate inmate) {
            using(var db = new ChoiceVDb()) {
                var dbInmate = db.prisoninmates.Find(inmate.Id);
                if(dbInmate != null) {
                    dbInmate.releasedDate = DateTime.Now;
                }
                
                db.SaveChanges();
            }
        }

        internal (bool worked, string message) tryDeleteInmate(PrisonInmate inmate) {
            var res = inmate.canBeDeleted();
            if(res.worked) {
                using(var db = new ChoiceVDb()) {
                    var dbInmate = db.prisoninmates.Find(inmate.Id);
                    db.prisoninmates.Remove(dbInmate);
                    db.SaveChanges();
                }

                Inmates.Remove(inmate);

                return res;
            }

            return res;
        }

        public void update(TimeSpan updateTime) {
            using(var db = new ChoiceVDb()) {
                foreach(var inmate in Inmates) {
                    if(inmate.Escaped) {
                        continue;
                    }

                    var row = db.prisoninmates.Find(inmate.Id);
                    var player = ChoiceVAPI.FindPlayerByCharId(inmate.CharId);

                    if(player != null) {
                        if(inmate.TimeLeftOnline > 0) {
                            inmate.TimeLeftOnline -= (int)updateTime.TotalMinutes;
                        } else {
                            inmate.TimeLeftOffline -= (int)(updateTime.TotalMinutes * PrisonController.PASSIV_TIME_MULTIPLIER);
                        }
                    } else {
                        inmate.TimeLeftOffline -= (int)updateTime.TotalMinutes;
                    }

                    if(inmate.TimeLeftOnline <= 0) {
                        inmate.TimeLeftOnline = 0;
                    }

                    if(inmate.TimeLeftOffline <= 0) {
                        inmate.TimeLeftOffline = 0;
                    }

                    row.timeLeftOffline = inmate.TimeLeftOffline;
                    row.timeLeftOnline = inmate.TimeLeftOnline;

                    if(!inmate.Escaped && !inmate.IsReleased && inmate.TimeLeftOffline <= 0 && inmate.TimeLeftOnline <= 0) {
                        player?.sendNotification(Constants.NotifactionTypes.Success, "Du bist nun bereit das Gefängnis zu verlassen. Gehe zum Ausgangspunkt oder sprich mit dem Wärter für mehr Informationen!", "Entlassung bereit", Constants.NotifactionImages.Prison, "PRISON_MESSAGE");
                    }
                }

                db.SaveChanges();
            }
        }

        private bool onBelongingsSpotInteraction(IPlayer player) {
            if(isPlayerInPrison(player)) {
                var inmate = getInmateForPlayer(player);
                if(inmate != null) {
                    inmate.openBelongings(player);
                } else {
                    player.sendBlockNotification("Es ist ein Fehler aufgetreten. Melde dich im Support. Code: PRISONBEAR 1", "Kein Insasse");
                }
            } else if(PrisonController.hasPlayerAccessToPrison(player, this)) {
                var menu = new Menu("Gefangenenbesitztümer", "Wessen Besitztümer möchtest du öffnen?");

                foreach(var inmate in Inmates.OrderBy(i => i.Name)) {
                    menu.addMenuItem(new ClickMenuItem(inmate.Name, $"Öffne die Besitztümer von {inmate.Name}", "", "OPEN_INMATE_BELONGINGS")
                        .withData(new Dictionary<string, dynamic> { { "Inmate", inmate } }));
                }

                player.showMenu(menu);
            } else {
                player.sendBlockNotification("Du hast keinen Zugriff auf dieses Inventar.", "Kein Zugriff");
            }

            return true;
        }


        private void onPlayerExitOutline(CollisionShape shape, IEntity entity) {
            var player = entity as IPlayer;

            if(!Outline.Any(o => o.IsInShape(player.Position))) {
                var inmate = getInmateForPlayer(player);


                if(inmate != null && !inmate.ClearedForExit) {
                    inmate.onEscapePrisonOutline(player, player.Position);
                }
            }
        }

        public void onDelete() {
            RegisterSpots?.ForEach(c => c.Dispose());
            RegisterSpots = null;

            BelongingsSpots?.ForEach(c => c.Dispose());
            BelongingsSpots = null;
            
            Cells.ForEach(c => c.onDelete());
        }
    }
}
