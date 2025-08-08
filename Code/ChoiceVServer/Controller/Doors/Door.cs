using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ChoiceVServer.Controller {
    public class DoorDimension {
        public int Dimension;
        public bool Locked;
        public int LockIndex;

        public DoorDimension(int dimension, int lockIndex, bool locked) {
            Dimension = dimension;
            LockIndex = lockIndex;
            Locked = locked;
        }
    }

    public delegate void DoorAdditonalDimensionChange(Door door, DoorDimension dimension, bool add);

    public class Door {
        [JsonIgnore]
        public static DoorAdditonalDimensionChange DoorAdditonalDimensionChange;

        public int Id;
        [JsonIgnore]
        public Position Position;
        public bool LockedInBaseDimension;
        public string ModelHash;
        public string GroupName;
        public int LockIndex;

        [JsonIgnore]
        public Dictionary<int, DoorDimension> AdditionalDimensions { get; private set; }

        private List<Door> ComboDoors;

        public List<DoorCompanyFunctionality> ConnectedCompanyFunctionalities { get; private set; }

        public Door(int id, Position position, bool locked, string modelHash, string groupName, int lockIndex) {
            Id = id;
            Position = position;
            LockedInBaseDimension = locked;
            ModelHash = modelHash;
            GroupName = groupName;

            LockIndex = lockIndex;

            ComboDoors = null;

            AdditionalDimensions = [];

            EventController.PlayerChangeDimensionDelegate += onPlayerChangeDimension;
        }

        public bool isLocked(int dimension) {
            if(dimension == Constants.GlobalDimension) {
                return LockedInBaseDimension;
            } else {
                if(AdditionalDimensions.ContainsKey(dimension)) {
                    return AdditionalDimensions[dimension].Locked;
                } else {
                    return true;
                }
            }
        }

        public void addAdditionalDimension(DoorDimension newDimension, bool saveToDb = true) {
            if(!AdditionalDimensions.ContainsKey(newDimension.Dimension)) {
                AdditionalDimensions.Add(newDimension.Dimension, newDimension);

                if(saveToDb) {
                    DoorAdditonalDimensionChange?.Invoke(this, newDimension, true);
                }
            }
        }

        public void removeAdditionalDimension(DoorDimension newDimension, bool saveToDb = true) {
            if(AdditionalDimensions.Remove(newDimension.Dimension)) {
                if(saveToDb) {
                    DoorAdditonalDimensionChange?.Invoke(this, newDimension, false);
                }
            }
        }

        public void lockDoor() {
            lockDoorStep(true);
        }

        public void lockDoorStep(bool initial) {
            LockedInBaseDimension = true;
            foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                player.emitClientEvent("CHANGE_DOOR_STATE", Position.X, Position.Y, Position.Z, true);
            }

            using(var db = new ChoiceVDb()) {
                var row = db.configdoors.Find(Id);
                if(row != null) {
                    row.locked = 1;

                    db.SaveChanges();
                }
            }

            if(initial && ComboDoors != null) {
                ComboDoors.ForEach(c => c.lockDoorStep(false));
            }
        }

        public void unlockDoor() {
            unlockDoorStep(true);
        }

        private void unlockDoorStep(bool initial) {
            LockedInBaseDimension = false;
            foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                player.emitClientEvent("CHANGE_DOOR_STATE", Position.X, Position.Y, Position.Z, false);
            }

            using(var db = new ChoiceVDb()) {
                var row = db.configdoors.Find(Id);
                if(row != null) {
                    row.locked = 0;

                    db.SaveChanges();
                }
            }

            if(initial && ComboDoors != null) {
                ComboDoors.ForEach(c => c.unlockDoorStep(false));
            }
        }


        public void addComboDoor(Door door) {
            if(ComboDoors == null) {
                ComboDoors = new List<Door>();
            }

            ComboDoors.Add(door);
        }

        public int getComboDoorAmount() {
            if(ComboDoors == null) {
                return 0;
            } else {
                return ComboDoors.Count;
            }
        }

        public void removeComboDoor(Door door) {
            if(door == this) {
                if(ComboDoors != null) {
                    ComboDoors.ForEach(d => d.removeComboDoor(door));
                    ComboDoors = null;
                }
            } else {
                ComboDoors.Remove(door);

                if(ComboDoors.Count == 0) {
                    ComboDoors = null;
                }
            }
        }

        public List<Door> getComboDoors() {
            return ComboDoors;
        }

        public void setConnectedCompanyFunctionality(DoorCompanyFunctionality functionality) {
            if(ConnectedCompanyFunctionalities == null) {
                ConnectedCompanyFunctionalities = new();
            }
            ConnectedCompanyFunctionalities.Add(functionality);
        }

        public void removeConnectedCompanyFunctionality(DoorCompanyFunctionality functionality) {
            ConnectedCompanyFunctionalities.Remove(functionality);

            if(ConnectedCompanyFunctionalities.Count <= 0) {
                ConnectedCompanyFunctionalities = null;
            }
        }

        /// <summary>
        /// This locks the door in a specific Dimension. This is not saved after restart!
        /// </summary>
        public void lockForDimension(int dimension) {
            if(dimension == Constants.GlobalDimension) {
                lockDoor();
                return;
            }

            if(AdditionalDimensions.ContainsKey(dimension)) {
                var addDim = AdditionalDimensions[dimension];
                addDim.Locked = true;

                using(var db = new ChoiceVDb()) {
                    var dbDim = db.configdoorsadditonaldimensions.Find(Id, dimension);
                    if(dbDim != null) {
                        dbDim.locked = 1;

                        db.SaveChanges();
                    }
                }

                foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                    if(player.Dimension == dimension) {
                        player.emitClientEvent("CHANGE_DOOR_STATE", Position.X, Position.Y, Position.Z, true);
                    }
                }
            }
        }

        /// <summary>
        /// This unlocks the door in a specific Dimension. This is not saved after restart!
        /// </summary>
        public void unlockForDimension(int dimension) {
            if(dimension == Constants.GlobalDimension) {
                unlockDoor();
                return;
            } 

            if(AdditionalDimensions.ContainsKey(dimension)) {
                var addDim = AdditionalDimensions[dimension];
                addDim.Locked = false;

                using(var db = new ChoiceVDb()) {
                    var dbDim = db.configdoorsadditonaldimensions.Find(Id, dimension);
                    if(dbDim != null) {
                        dbDim.locked = 0;

                        db.SaveChanges();
                    }
                }

                foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                    if(player.Dimension == dimension) {
                        player.emitClientEvent("CHANGE_DOOR_STATE", Position.X, Position.Y, Position.Z, false);
                    }
                }
            }
        }

        public void onPlayerChangeDimension(IPlayer player, int oldDimension, int newDimension) {
            upateForPlayer(player);
        }

        public void upateForPlayer(IPlayer player) {
            if(player.Dimension == Constants.GlobalDimension) {
                player.emitClientEvent("CHANGE_DOOR_STATE", Position.X, Position.Y, Position.Z, LockedInBaseDimension);
            } else {
                if(AdditionalDimensions.ContainsKey(player.Dimension)) {
                    var addDim = AdditionalDimensions[player.Dimension];
                    player.emitClientEvent("CHANGE_DOOR_STATE", Position.X, Position.Y, Position.Z, addDim.Locked);
                } else {
                    player.emitClientEvent("CHANGE_DOOR_STATE", Position.X, Position.Y, Position.Z, true);
                }
            }
        }

        public void changeLock() {
            using(var db = new ChoiceVDb()) {
                var row = db.configdoors.Find(Id);
                if(row != null) {
                    row.lockIndex++;
                    db.SaveChanges();
                    LockIndex = row.lockIndex;
                }
            }
        }

        public void changeLock(int newIdx) {
            using(var db = new ChoiceVDb()) {
                var row = db.configdoors.Find(Id);
                if(row != null) {
                    row.lockIndex = newIdx;
                    db.SaveChanges();
                    LockIndex = row.lockIndex;
                }
            }
        }

        public void changeLockInDimension(int dimension) {
            if(AdditionalDimensions.ContainsKey(dimension)) {
                var addDim = AdditionalDimensions[dimension];
                addDim.LockIndex++;
                using(var db = new ChoiceVDb()) {
                    var row = db.configdoorsadditonaldimensions.Find(Id, dimension);
                    if(row != null) {
                        row.lockIndex = addDim.LockIndex;
                        db.SaveChanges();
                    }
                }
            }
        }

        public int getLockIndexInDimension(int dimension) {
            if(AdditionalDimensions.ContainsKey(dimension)) {
                var addDim = AdditionalDimensions[dimension];
                return addDim.LockIndex;
            } else {
                return 0;
            }
        }

        public bool fitsForKey(int triggerDimension, DoorKey key) {
            if(triggerDimension != key.Dimension) {
                return false;
            }

            if(key.Dimension == Constants.GlobalDimension) {
                if(key.LockIndex == LockIndex && (key.DoorId == Id || key.DoorGroup == GroupName)) {
                    return true;
                } else {
                    return false;
                }
            } else {
                if(AdditionalDimensions.ContainsKey(key.Dimension)) {
                    var addDim = AdditionalDimensions[key.Dimension];

                    if(addDim.LockIndex == key.LockIndex && (key.DoorId == Id || key.DoorGroup == GroupName)) {
                        return true;
                    } else {
                        return false;
                    }
                } else {
                    return false;
                }
            }
        }

        public bool canPlayerOpen(IPlayer player) {
            var keys = player.getInventory().getItems<DoorKey>(d => true);

            if(keys.Any(k => fitsForKey(player.Dimension, k))
                || (ConnectedCompanyFunctionalities != null && ConnectedCompanyFunctionalities.Any(f => f.canPlayerOpenDoor(player, this)))) {
                return true;
            } else {
                return false;
            }
        }

        public bool canPlayerOpen(IPlayer player, IEnumerable<DoorKey> keys) {
            if(player.getCharacterData().AdminMode
                || keys.Any(k => fitsForKey(player.Dimension, k))
                || (ConnectedCompanyFunctionalities != null && ConnectedCompanyFunctionalities.Any(f => f.canPlayerOpenDoor(player, this)))) {
                return true;
            } else {
                return false;
            }
        }

        public int getLockIndexForDimension(int dimension) {
            if(AdditionalDimensions.ContainsKey(dimension)) {
                var addDim = AdditionalDimensions[dimension];

                return addDim.LockIndex;
            } else {
                return LockIndex;
            }
        }
    }
}
