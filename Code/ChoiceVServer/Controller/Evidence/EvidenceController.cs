using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Controller.MarkerController;
using static ChoiceVServer.InventorySystem.Evidence;

namespace ChoiceVServer.Controller {
    public class EvidenceController : ChoiceVScript {
        public class WorldEvidence {
            public int DbId;
            public WorldGrid WorldGrid;
            public Position Position;
            public ChoiceVMarker Marker;
            public List<int> ShownPlayers = new List<int>();
            public DateTime CreateTime;

            public EvidenceType Type;

            public List<EvidenceData> Data;

            private CollisionShape CollisionShape;

            public WorldEvidence(WorldGrid worldGrid, Position position, EvidenceType type, List<EvidenceData> data) {
                Position = position;
                WorldGrid = worldGrid;
                Type = type;
                Data = data;

                Marker = createMarker(27, position, EvidenceTypeToMarkerColor[type], 0.6f, 1.75f, null);
                CreateTime = DateTime.Now;
            }

            public void remove() {
                removeMarker(Marker);

                CollisionShape?.Dispose();
                CollisionShape = null;
            }

            public bool showForPlayer(IPlayer player) {
                if(ShownPlayers.Contains(player.getCharacterId())) {
                    return false;
                }

                Marker.showToPlayer(player);
                ShownPlayers.Add(player.getCharacterId());

                if(CollisionShape == null) {
                    CollisionShape = CollisionShape.Create(Position, 1.5f, 1.5f, 0, true, false, true);
                    CollisionShape.OnCollisionShapeInteraction += (p) => onCollectEvidence(p, this);
                }

                return true;
            }

            public void noLongerShowForPlayer(IPlayer player) {
                if(ShownPlayers.Contains(player.getCharacterId())) {
                    Marker.noLongerShowToPlayer(player);
                    ShownPlayers.Remove(player.getCharacterId());

                    if(ShownPlayers.Count == 0) {
                        CollisionShape.Dispose();
                        CollisionShape = null;
                    }
                }
            }
        }

        public static Dictionary<int, List<WorldEvidence>> AllEvidence = new Dictionary<int, List<WorldEvidence>>();
        private static List<int> AllDetectives = new List<int>();

        public EvidenceController() {
            //Evidence Box Stuff
            EventController.addMenuEvent("COLLECT_ALL_EVIDENCE", onCollectAllEvidence);
            EventController.addMenuEvent("RETRIEVE_EVIDENCE", onRetrieveEvidence);
            EventController.addMenuEvent("OPEN_EVIDENCE_BOX", onOpenEvidenceBox);

            InvokeController.AddTimedInvoke("Evidence Remover", removeOldEvidence, TimeSpan.FromMinutes(5), true);

            EventController.PlayerChangeWorldGridDelegate += onPlayerChangeWorldGrid;

            EventController.MainReadyDelegate += onMainReady;

            EventController.VehicleHealthChangeDelegate += onVehicleHealthChange;
        }

        private void onMainReady() {
            loadEvidence();
        }

        private void onPlayerChangeWorldGrid(object sender, IPlayer player, WorldGrid previousGrid, WorldGrid currentGrid) {
            if(AllDetectives.Contains(player.getCharacterId())) {
                if(previousGrid == null || currentGrid == null) {
                    previousGrid = WorldController.getWorldGrid(player.Position);
                    currentGrid = previousGrid;
                }

                List<WorldEvidence> oldEvidence = new List<WorldEvidence>();
                List<WorldEvidence> newEvidence = new List<WorldEvidence>();

                if(AllEvidence.ContainsKey(previousGrid.Id)) {
                    oldEvidence = AllEvidence[previousGrid.Id];
                }

                if(AllEvidence.ContainsKey(currentGrid.Id)) {
                    newEvidence = AllEvidence[currentGrid.Id];
                }

                foreach(var ev in oldEvidence) {
                    ev.noLongerShowForPlayer(player);
                }

                foreach(var ev in newEvidence) {
                    ev.showForPlayer(player);
                }
            }
        }

        private void loadEvidence() {
            var count = 0;
            using(var db = new ChoiceVDb()) {
                foreach(var evidence in db.evidences) {
                    count++;
                    var ev = createEvidence(evidence.position.FromJson(), (EvidenceType)evidence.type, evidence.data.FromJson<List<EvidenceData>>(), false);
                    ev.DbId = evidence.id;
                }
            }

            Logger.logDebug(LogCategory.ServerStartup, LogActionType.Created, $"loadEvidence: {count} WorldEvidence created");
        }

        private void removeOldEvidence(IInvoke ivk) {
            var removeList = new List<WorldEvidence>();
            foreach(var grid in AllEvidence.Keys) {
                foreach(var evidence in AllEvidence[grid]) {
                    if(evidence.CreateTime + EvidenceTypeRemoveTime[evidence.Type] < DateTime.Now) {
                        removeList.Add(evidence);
                    }
                }
            }

            foreach(var item in removeList) {
                removeEvidence(item);
            }
        }

        private bool onCollectAllEvidence(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            EvidenceBox box = data["EvidenceBox"];
            var count = 0;

            var items = player.getInventory().getItems<Evidence>(i => true);
            foreach(Evidence item in items) {
                count++;
                var workd = player.getInventory().moveItems(box.BoxContent, item);
            }

            box.updateDescription();
            box.setWeight();
            player.sendNotification(NotifactionTypes.Info, count + " Beweis(e) wurden in die Kiste gelegt", "Beweise sortiert", NotifactionImages.MagnifyingGlass);

            return true;
        }

        private bool onRetrieveEvidence(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var ev = (Evidence)data["Evidence"];
            var box = (EvidenceBox)data["EvidenceBox"];

            box.BoxContent.moveItem(player.getInventory(), ev);

            box.updateDescription();
            player.sendNotification(NotifactionTypes.Info, "Beweis wurden aus der Kiste genommen", "Beweise genommen", NotifactionImages.MagnifyingGlass);

            return false;
        }

        private bool onOpenEvidenceBox(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var box = (EvidenceBox)data["EvidenceBox"];

            InventoryController.showMoveInventory(player, player.getInventory(), box.BoxContent, (p, f, t, i, a) => {
                box.setWeight();
                box.updateDescription();
                return true;
            }, null, "Beweis-Kiste", true);
            return true;
        }


        private static bool onCollectEvidence(IPlayer player, WorldEvidence evidence) {
            if(!AllDetectives.Contains(player.getCharacterId())) {
                return false;
            }
            
            AnimationController.animationTask(player, AnimationController.getAnimationByName(KNEEL_DOWN_ANIMATION), () => {
                if(AllEvidence.TryGetValue(evidence.WorldGrid.Id, out var list)) {
                    if(!list.Contains(evidence)) {
                        player.sendNotification(NotifactionTypes.Danger, "Jemand anderes hat den Beweis aufgehoben!", "Beweis blockiert!", NotifactionImages.MagnifyingGlass);
                        return;
                    }
                }

                if(evidence.ShownPlayers.Contains(player.getCharacterId())) {
                    //configitems item = InventoryController.AllConfigItems[Constants.EVIDENCE_ITEM_ID];
                    if(player.getInventory().addItem(new Evidence(InventoryController.getConfigItemForType<Evidence>(), evidence.Type, evidence.Data))) {
                        player.sendNotification(NotifactionTypes.Info, "Du hast einen Beweis gefunden!", "Beweis gefunden", NotifactionImages.MagnifyingGlass);

                        removeEvidence(evidence);
                    } else {
                        player.sendNotification(NotifactionTypes.Danger, "Dein Inventar ist voll!", "Inventar voll!", NotifactionImages.MagnifyingGlass);
                    }
                }
            });

            return true;
        }

        public static WorldEvidence createEvidence(Position position, EvidenceType type, List<EvidenceData> data, bool withDb = true) {
            var grid = WorldController.getWorldGrid(position);
            var evidence = new WorldEvidence(grid, position, type, data);

            if(AllEvidence.ContainsKey(grid.Id)) {
                if(AllEvidence[grid.Id] == null) {
                    var list = new List<WorldEvidence>();
                    list.Add(evidence);
                    AllEvidence[grid.Id] = list;
                } else {
                    AllEvidence[grid.Id].Add(evidence);
                }
            } else {
                var list = new List<WorldEvidence>();
                list.Add(evidence);
                AllEvidence.Add(grid.Id, list);
            }

            foreach(var detective in AllDetectives) {
                var player = ChoiceVAPI.FindPlayerByCharId(detective);
                if(player != null && player.Exists()) {
                    var playerGrid = WorldController.getWorldGrid(player.Position);

                    if(grid.Id == playerGrid.Id) {
                        evidence.showForPlayer(player);
                    }
                }
            }

            if(withDb) {
                using(var db = new ChoiceVDb()) {
                    var ev = new evidence {
                        type = (int)evidence.Type,
                        position = evidence.Position.ToJson(),
                        dateTime = evidence.CreateTime,
                        data = data.ToJson(),
                    };

                    db.evidences.Add(ev);
                    db.SaveChanges();

                    evidence.DbId = ev.id;
                }
            }

            Logger.logDebug(LogCategory.System, LogActionType.Created, $"createEvidence: evidence has been created: {evidence.Type}, at: {position.ToString()}");



            return evidence;
        }

        public static void removeEvidence(WorldEvidence evidence) {
            if(evidence != null) {
                evidence.remove();
                AllEvidence[evidence.WorldGrid.Id].Remove(evidence);
                if(AllEvidence[evidence.WorldGrid.Id].Count == 0) {
                    AllEvidence.Remove(evidence.WorldGrid.Id);
                }

                using(var db = new ChoiceVDb()) {
                    var ev = db.evidences.FirstOrDefault(e => e.id == evidence.DbId);
                    if(ev != null) {
                        db.evidences.Remove(ev);
                    }

                    db.SaveChanges();
                }

                Logger.logDebug(LogCategory.ServerStartup, LogActionType.Removed, $"removeEvidence: evidence has been removed: {evidence.Type}, at: {evidence.Position}");
            } else {
                Logger.logError($"removeEvidence: Tried to remove Evidence which was not found, position: {evidence.Position.ToString()}",
                    $"Fehler bei auotmaischenBeweisentfernung: Beweis konnte nicht in der Datenbank gefunden werden. {evidence.Position}");
            }
        }

        public static bool activateDetectiveMode(IPlayer player) {
            if(AllDetectives.Contains(player.getCharacterId())) {
                return false;
            }

            AllDetectives.Add(player.getCharacterId());

            var grid = WorldController.getWorldGrid(player.Position);
            List<WorldEvidence> newEvidence = new List<WorldEvidence>();

            if(AllEvidence.ContainsKey(grid.Id)) {
                newEvidence = AllEvidence[grid.Id];
            }

            foreach(var ev in newEvidence) {
                ev.showForPlayer(player);
            }

            return true;
        }

        public static bool deactivateDetectiveMode(IPlayer player) {
            if(AllDetectives.Remove(player.getCharacterId())) {

                var grid = WorldController.getWorldGrid(player.Position);
                List<WorldEvidence> oldEvidence = new List<WorldEvidence>();

                if(AllEvidence.ContainsKey(grid.Id)) {
                    oldEvidence = AllEvidence[grid.Id];
                }

                foreach(var ev in oldEvidence) {
                    ev.noLongerShowForPlayer(player);
                }

                return true;
            } else {
                return false;
            }
        }

        public static void createBloodEvidence(IPlayer player) {
            if(!player.hasData("LAST_BLOOD_EVIDENCE")) {
                player.setData("LAST_BLOOD_EVIDENCE", DateTime.MinValue);
            }

            var lastDrop = (DateTime)player.getData("LAST_BLOOD_EVIDENCE");
            player.setData("LAST_BLOOD_EVIDENCE", DateTime.Now);

            if(lastDrop + TimeSpan.FromSeconds(20) <= DateTime.Now) {
                var data = new List<EvidenceData> {
                    new EvidenceData("charID", player.getCharacterId().ToString(), false, true),
                         new EvidenceData("Ursprung", $"Dieser Beweis wurde {DateTime.Now.ToString("dd/MM/yyyy")} um ca. {DateTime.Now.ToString("HH:mm")} Uhr erzeugt.", true)
                };
                //data.Add(new EvidenceData("Person", "Name : " + player.getCharacterName() + "\nSozial Vers.: " + player.getSocialSecurityNumber(), false, true));

                CallbackController.getGroundZFromPos(player, player.Position, (p, z, b1, b2) => {
                    createEvidence(new Position(player.Position.X, player.Position.Y, z + 0.05f), EvidenceType.Blood, data);
                });
            }
        }

        public static void createWeaponEvidence(IPlayer player, Weapon weapon) {
            if(!player.hasData("LAST_WEAPON_EVIDENCE")) {
                player.setData("LAST_WEAPON_EVIDENCE", DateTime.MinValue);
            }

            var lastDrop = (DateTime)player.getData("LAST_WEAPON_EVIDENCE");
            player.setData("LAST_WEAPON_EVIDENCE", DateTime.Now);

            if(lastDrop + TimeSpan.FromSeconds(10) <= DateTime.Now) {
                var configWeap = WeaponController.getConfigWeapon(weapon.WeaponName);
                var data = new List<EvidenceData> {
                    new EvidenceData("itemId", weapon.Id.ToString(), false, true),
                    new EvidenceData("Waffentyp", $"Die Patrone wurde aus einer Waffe mit Typ {configWeap.displayType} abgefeuert.", false),
                    new EvidenceData("Waffenname", $"Die Patrone wurde aus einer/einem {weapon.Name} abgefeuert.", true),
                    new EvidenceData("Ursprung", $"Dieser Beweis wurde am {DateTime.Now.ToString("dd/MM/yyyy")} um ca. {DateTime.Now.ToString("HH:mm")} Uhr abgefeuert.", true)
                };

                CallbackController.getGroundZFromPos(player, player.Position, (p, z, b1, b2) => {
                    createEvidence(new Position(player.Position.X, player.Position.Y, player.Position.Z - 0.95f), EvidenceType.CartridgeCase, data);
                });
            }
        }

        private void onVehicleHealthChange(ChoiceVVehicle vehicle, uint oldHealth, uint newHealth) {
            if(oldHealth > newHealth) {
                createCarEvidence(vehicle);
            }
        }

        public static void createCarEvidence(ChoiceVVehicle vehicle) {
            if(!vehicle.hasData("LAST_CAR_EVIDENCE")) {
                vehicle.setData("LAST_CAR_EVIDENCE", DateTime.MinValue);
            }

            var lastDrop = (DateTime)vehicle.getData("LAST_CAR_EVIDENCE");
            vehicle.setData("LAST_CAR_EVIDENCE", DateTime.Now);

            if(lastDrop + TimeSpan.FromSeconds(20) <= DateTime.Now) {
                var data = new List<EvidenceData> {
                    new EvidenceData("vehicleId", vehicle.VehicleId.ToString(), false, true)
                };

                //TODO Unsinn! Muss mit VehicleColor geregelt werden
                var cP = vehicle.PrimaryColorRgb;
                var cS = vehicle.SecondaryColorRgb;
                data.Add(new EvidenceData("Primärfarbe", $"Die primäre Farbe des Autos war {Color.FromArgb(cP.R, cP.G, cP.B).getColorName()}", false));
                data.Add(new EvidenceData("Sekundärfarbe", $"Die sekundäre Farbe des Autos war {Color.FromArgb(cS.R, cS.G, cS.B).getColorName()}", false));
                var model = (VehicleModel)vehicle.Model;
                data.Add(new EvidenceData("Modell", $"Das Auto war vom Typ: {model.ToString()}", true));
                data.Add(new EvidenceData("Ursprung", $"Dieser Beweis wurde am {DateTime.Now.ToString("dd/MM/yyyy")} um ca. {DateTime.Now.ToString("HH:mm")} Uhr erzeugt.", true));



                var player = vehicle.Driver;
                if(player == null) {
                    player = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.Position.Distance(vehicle.Position) < 50);
                    if(player == null) {
                        return;
                    }
                }

                CallbackController.getGroundZFromPos(player, vehicle.Position, (p, z, b1, b2) => {
                    createEvidence(new Position(vehicle.Position.X, vehicle.Position.Y, z + 0.05f), EvidenceType.CarPaint, data);
                });

                if(vehicle.Driver != null) {
                    CamController.checkIfCamSawAction(vehicle.Driver, $"Fahrzeug beschädigt", $"Der Fahrer hat das Fahrzeug ({vehicle.DbModel.ModelName}:{vehicle.NumberplateText}) beschädigt.");
                }
            }
        }
    }
}
