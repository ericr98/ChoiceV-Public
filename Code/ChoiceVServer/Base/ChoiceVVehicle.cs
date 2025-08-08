using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.Vehicles;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Base {
    public class ChoiceVVehicleFactory : IEntityFactory<IVehicle> {
        public IVehicle Create(ICore server, IntPtr vehiclePointer, uint id) {
            return new ChoiceVVehicle(server, vehiclePointer, id);
        }
    }

    public enum FuelType {
        None = 0,
        Petrol = 1,
        Diesel = 2,
        Electricity = 3,
        Kerosin = 4,
    }

    public enum PartDamageLevel {
        None = 0,
        Stutter = 1,
        Broken = 2,
    }

    public class ChoiceVVehicle : Vehicle {
        public int VehicleId { get; private set; }
        public int ChassisNumber { get; private set; }

        public int VehicleClassId { get; private set; }
        public VehicleClassesDbIds VehicleClass {get => (VehicleClassesDbIds)VehicleClassId; }

        public configvehiclesmodel DbModel;

        public int ScriptDimension { get; private set; }

        public Inventory Inventory { get; private set; }
        public VehicleDamage VehicleDamage { get; private set; }
        public VehicleTuning VehicleTuning { get; private set; }

        public VehicleColoring VehicleColoring { get; private set; }

        public bool SkipNextSave { get; set; }

        public Position LastPosition;

        public bool SirenSound = true;

        public float FuelTankSize;
        public float Fuel;
        public FuelType FuelType;
        public float DrivenDistance;
        public float LastLongTickDrivenDistance;

        public int KeyLockVersion;

        public float DbDirtLevel;

        public PartDamageLevel BatteryDamageLevel { get; set; }
        public PartDamageLevel LightDamageLevel { get; set; }
        public PartDamageLevel EngineDamageLevel { get; set; }

        public bool IgnoreFirstDamage = true;

        public bool CanDrive {
            get => Fuel != 0 &&
                !this.hasData("ELECTRO_CHARGING_START") &&
                MotorCompartmentGame == null &&
                BatteryDamageLevel != PartDamageLevel.Broken &&
                EngineDamageLevel != PartDamageLevel.Broken;
        }

        //Player and Seat
        public Dictionary<int, IPlayer> PassengerList = new Dictionary<int, IPlayer>();

        public bool CurrentlyTravelingToIsland = false;

        public bool IsRandomlySpawned { get => RandomlySpawnedDate != null && RandomlySpawnedDate != DateTime.MaxValue; }

        public DateTime? RandomlySpawnedDate = DateTime.MaxValue;

        public Company RegisteredCompany;

        public DateTime LastMoved;
        
        public ChoiceVVehicle(ICore server, IntPtr nativePointer, uint id) : base(server, nativePointer, id) {
            VehicleId = -1;
            Inventory = null;
        }

        public void init(int id, int chassisNumber, configvehiclesmodel dbModel, int vehicleClass, string numberPlate, int dimension, float fuel, float drivenDistance, int keyLockVersion, float dirtLevel, DateTime? randomlySpawnedDate, DateTime lastMoved, Company registeredCompany) {
            VehicleId = id;
            ChassisNumber = chassisNumber;
            DbModel = dbModel;
            VehicleClassId = vehicleClass;
            ScriptDimension = dimension;
            Dimension = dimension;

            Fuel = fuel;
            FuelTankSize = dbModel.FuelMax != -1 ? dbModel.FuelMax : dbModel._class.FuelMax;
            FuelType = dbModel.FuelType != -1 ? (FuelType)dbModel.FuelType : (FuelType)dbModel._class.FuelType;
            DrivenDistance = drivenDistance;
            KeyLockVersion = keyLockVersion;

            LastMoved = lastMoved;
            RegisteredCompany = registeredCompany;
            
            if(Inventory != null) {
                if(Inventory.hasItem<VehicleKey>(i => i.VehicleId == id)) {
                    LockState = VehicleLockState.Unlocked;
                } else {
                    LockState = VehicleLockState.Locked;
                }

                //Caddy 3 can show filled Inventory with its Extras
                if(dbModel.ModelName == "Caddy3") {
                    Inventory.ThisInventoryAddItemDelegate += onAddItem;
                    Inventory.ThisInventoryItemsMovedIntoThisInventoryDelegate += onMoveItem;
                    Inventory.ThisInventoryEmptyAfterMoveDelegate += onEmptyAfterMove;

                    ToggleExtra(2, false);
                }
            } else {
                LockState = VehicleLockState.Locked;
            }

            NumberplateText = numberPlate;

            DbDirtLevel = dirtLevel;
            DirtLevel = Convert.ToByte(Math.Round(dirtLevel));

            if(dbModel.PowerMultiplier != 1 && dbModel.PowerMultiplier != 0) {
                SetStreamSyncedMetaData("ENGINE_MULT", dbModel.PowerMultiplier);
            }

            RandomlySpawnedDate = randomlySpawnedDate;
        }

        public ChoiceVVehicle setInventory(Inventory inventory) {
            Inventory = inventory;

            return this;
        }

        #region Caddy 3 Inventory Show

        private void onAddItem(Inventory inventory, Item item, int? amount, bool saveToDb) {
            if(Inventory.getAllItems().Count > 0) {
                showExtraWithSave(2);
            }
        }

        private void onMoveItem(Inventory oldInventory, Item item, int amount) {
            if(Inventory.getAllItems().Count > 0) {
                showExtraWithSave(2);
            }

        }

        private bool onEmptyAfterMove(Inventory inventory) {
            hideExtraWithSave(2);
            return true;
        }

        #endregion

        public void showExtraWithSave(byte extraId) {
            if(this.hasData("ACTIVATED_EXTRAS")) {
                var list = ((string)this.getData("ACTIVATED_EXTRAS")).FromJson<List<string>>();

                if(!list.Contains(extraId.ToString())) {
                    list.Add(extraId.ToString());
                    this.setPermanentData("ACTIVATED_EXTRAS", list.ToJson());

                    ToggleExtra(extraId, true);
                }
            } else {
                var newList = new List<string> { extraId.ToString() };
                this.setPermanentData("ACTIVATED_EXTRAS", newList.ToJson());

                ToggleExtra(extraId, true);
            }
        }

        public void hideExtraWithSave(byte extraId) {
            if(this.hasData("ACTIVATED_EXTRAS")) {
                var list = ((string)this.getData("ACTIVATED_EXTRAS")).FromJson<List<string>>();

                if(list.Contains(extraId.ToString())) {
                    list.Remove(extraId.ToString());
                    this.setPermanentData("ACTIVATED_EXTRAS", list.ToJson());

                    ToggleExtra(extraId, false);
                }
            }
        }

        public void setPermanentData(string name, string value) {
            this.setData(name, value);

            using(var db = new ChoiceVDb()) {
                var already = db.vehiclesdata.Find(VehicleId, name);
                if(already != null) {
                    already.value = value;
                } else {
                    var newData = new vehiclesdatum {
                        vehicleId = VehicleId,
                        name = name,
                        value = value,
                    };

                    db.vehiclesdata.Add(newData);
                }

                db.SaveChanges();
            }
        }

        public void resetPermantData(string name) {
            this.resetData(name);

            using(var db = new ChoiceVDb()) {
                var data = db.vehiclesdata.Find(VehicleId, name);
                if(data != null) {
                    db.vehiclesdata.Remove(data);
                } else {
                    Logger.logWarning(LogCategory.System, LogActionType.Blocked, $"resetPermantData: Tried to remove data {name} which was not found!");
                }

                db.SaveChanges();
            }
        }

        /// <summary>
        /// Returns if a player has access to the vehicle
        /// </summary>
        public bool hasPlayerAccess(IPlayer player, bool ignoreInventoryKeys = false) {
            return (!ignoreInventoryKeys && Inventory != null && Inventory.hasItem<VehicleKey>(v => v.worksForCar(this))) || player.getInventory().hasItem<VehicleKey>(vk => vk.worksForCar(this)) || (RegisteredCompany != null && CompanyController.hasPlayerPermission(player, RegisteredCompany, "COMPANY_VEHICLE_ACCESS"));
        }

        public void refreshVehicle(Action afterRefresh = null) {
            Dimension = 99;
            InvokeController.AddTimedInvoke("VehicleChanger", (i) => {
                Dimension = ScriptDimension;

                InvokeController.AddTimedInvoke("VehicleChanger2", (i) => {
                    afterRefresh?.Invoke();
                }, TimeSpan.FromMilliseconds(250), false);
            }, TimeSpan.FromMilliseconds(250), false);
        }

        public void changeDimension(int newDimension) {
            ScriptDimension = newDimension;
            Dimension = newDimension;
            using(var db = new ChoiceVDb()) {
                var dbVeh = db.vehicles.Find(VehicleId);

                if(dbVeh != null) {
                    dbVeh.dimension = newDimension;
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Sets VehicleDamage for Vehicle and applies it
        /// </summary>
        public void setVehicleDamage(VehicleDamage damage, bool setNoApply = false) {
            VehicleDamage = damage;
            if(!setNoApply) {
                applyVehicleDamage(damage);
            }
        }

        public void reduceFuel(float reduce) {
            if(FuelType != FuelType.None) {
                if(Fuel > reduce) {
                    Fuel = Fuel - reduce;
                } else {
                    Fuel = 0;
                    EngineOn = false;
                    if(Driver != null) {
                        Driver.sendBlockNotification("Der Tank des Fahrzeuges ist leer und der Motor geht aus!", "Tank leer", NotifactionImages.Car, "CAR_FUEL_EMPTY");
                    }
                }
            }
        }

        /// <summary>
        /// Applies Vehicle Damage so it is visible for everybody
        /// </summary>
        public void applyVehicleDamage(VehicleDamage damage, bool dbOverride = false) {
            damage.apply(this);


            ////fixVehicleForNearby();
            //SkipNextSave = true;

            //InvokeController.AddTimedInvoke("applyVehicleDamage", (i) => {
            //    BodyHealth = (uint)damage.BodyHealth;
            //    BodyAdditionalHealth = (uint)damage.BodyAdditionalHealth;
            //    PetrolTankHealth = damage.PetrolTankHealth;
            //    EngineHealth = damage.EngineHealth;

            //    foreach(var part in damage.AllParts) {
            //        if(part.DamageLevel == 0) {
            //            SetPartDamageLevel(part.PartId.ToByte(), part.DamageLevel.ToByte());
            //            SetPartBulletHoles(part.PartId.ToByte(), part.BulletHoles.ToByte());
            //        }
            //    }

            //    foreach(var tyre in damage.AllTyres) {
            //        SetWheelHealth(tyre.TyreId.ToByte(), tyre.Health);
            //        if(tyre.Destroyed) {
            //            SetWheelHasTire(tyre.TyreId.ToByte(), false);
            //            SetWheelBurst(tyre.TyreId.ToByte(), true);
            //        } else {
            //            SetWheelFixed(tyre.TyreId.ToByte());
            //        }
            //    }

            //    //foreach(var window in damage.AllWindows) {
            //    //    SetWindowDamaged(window.SimplePartId.ToByte(), window.Destroyed);
            //    //}

            //    foreach(var light in damage.AllLights) {
            //        SetLightDamaged(light.SimplePartId.ToByte(), false);
            //    }

            //    //foreach(var door in damage.AllDoors) {
            //    //    if(door.Destroyed) {
            //    //        SetDoorState(door.SimplePartId.ToByte(), 255);
            //    //    } else {
            //    //        SetDoorState(door.SimplePartId.ToByte(), 0);
            //    //    }
            //    //}

            //    foreach(var bumper in damage.AllBumpers) {
            //        SetBumperDamageLevel(bumper.BumperId.ToByte(), ((int)bumper.State).ToByte());
            //    }

            //    SetSyncedMetaData("VEHICLE_DAMAGE", damage.ToJson());

            //    if(dbOverride) {
            //        VehicleController.startDbOverride(this);
            //    }

            //    //refreshVehicle();
            //}, TimeSpan.FromMilliseconds(400), false);
        }

        public void fixVehicleForNearby() {
            if(HasStreamSyncedMetaData("SET_VEHICLE_FIXED")) {
                int count = 0;
                GetStreamSyncedMetaData("SET_VEHICLE_FIXED", out count);
                SetStreamSyncedMetaData("SET_VEHICLE_FIXED", count++);
            } else {
                SetStreamSyncedMetaData("SET_VEHICLE_FIXED", 1);
            }
        }

        /// <summary>
        /// Sets setVehicleTuning for Vehicle and applies it
        /// </summary>
        public void setVehicleTuning(VehicleTuning tuning) {
            VehicleTuning = tuning;
            applyVehicleTuning(tuning);
        }

        /// <summary>
        /// Applies applyVehicleTuning so it is visible for everybody
        /// </summary>
        public void applyVehicleTuning(VehicleTuning tuning) {
            ModKit = tuning.ModKit.ToByte();

            foreach(var modType in (VehicleModType[])Enum.GetValues(typeof(VehicleModType))) {
                var tuningMod = tuning.AllUpgradedMods.FirstOrDefault(t => t.Type == modType);
                if(tuningMod != null) {
                    this.SetMod((byte)modType, (tuningMod.Level + 1).ToByte());
                } else {
                    this.SetMod((byte)modType, 0);
                }
            }

            refreshVehicle();
        }

        /// <summary>
        /// Sets setVehicleTuning for Vehicle and applies it
        /// </summary>
        public void setVehicleColoring(VehicleColoring coloring) {
            VehicleColoring = coloring;
            applyVehicleColoring(coloring);
        }

        public void applyVehicleColoring(VehicleColoring coloring) {
            Livery = coloring.Livery;
            
            if(coloring.UsingPrimaryRGB) {
                PrimaryColorRgb = coloring.PrimaryColorRGB;
            } else {
                PrimaryColor = coloring.PrimaryColor;
            }

            if(coloring.UsingSecondaryRGB) {
                SecondaryColorRgb = coloring.SecondaryColorRGB;
            } else {
                SecondaryColor = coloring.SecondaryColor;
            }

            PearlColor = coloring.PearlColor;

            refreshVehicle();
        }

        public void testHorn() {
            ChoiceVAPI.emitClientEventToAll("TEST_HORN", this);
        }

        public void updateDbTuning(vehicle dbVeh) {
            VehicleTuning.updateDb(dbVeh.vehiclestuningbase, dbVeh.vehiclestuningmods, dbVeh.id);
        }

        public void updateDbDamage(ref vehicle dbVeh) {
            if(!this.Exists()) {
                return;
            }

            VehicleDamage?.read(this);
            VehicleDamage?.saveToDb(ref dbVeh);
        }

        public void updateDbColoring(vehicle dbVeh) {
            VehicleColoring.updateDb(dbVeh.vehiclescoloring);
        }

        public void updateDbVehicle(vehicle dbVeh) {
            dbVeh.fuel = Fuel;
            dbVeh.drivenDistance = (int)DrivenDistance;
            dbVeh.dirtLevel = DbDirtLevel;
            dbVeh.keyLockVersion = KeyLockVersion;

            dbVeh.position = Position.ToJson();
            dbVeh.rotation = Rotation.ToJson();
            dbVeh.lastMoved = LastMoved;
            
            if(RegisteredCompany != null) {
                dbVeh.registeredCompanyId = RegisteredCompany.Id;
            } else {
                dbVeh.registeredCompanyId = null;
            }
        }

        public class VehicleIconUpdateCefEvent : IPlayerCefEvent {
            public string Event { get; }
            public string name;
            public int value;

            public VehicleIconUpdateCefEvent(string name, int value) {
                Event = "UPDATE_CAR_ICON";
                this.name = name;
                this.value = value;
            }
        }

        public class VehicleHudUpdateCefEvent : IPlayerCefEvent {
            public string Event { get; }
            public float milage;
            public float fuelMax;
            public float fuel;
            public int fuelType;

            public VehicleHudUpdateCefEvent(float milage, float fuel, bool hasFuel, int fuelType) {
                Event = "UPDATE_CAR_HUD";
                this.milage = milage / 1000;
                this.fuelMax = hasFuel ? 100 : -1;
                this.fuel = fuel * 100;
                this.fuelType = fuelType;
            }
        }

        public void verylongTickUpdate() {
            if(DbModel == null || !this.Exists()) {
                return;
            }

            var dist = 0f;

            if(LastLongTickDrivenDistance > 0) {
                dist = (DrivenDistance - LastLongTickDrivenDistance);
            }

            if(dist > 1) {
                LastMoved = DateTime.Now;
            }

            //DriverIcons
            foreach(var passenger in PassengerList.Values.Reverse<IPlayer>()) {
                if(passenger != null && passenger.Exists()) {
                    var damageLevel = VehicleDamage?.getDamageLevel(this) ?? 0;
                    var compartmentDamage = 0;

                    if(MechLightStatuses.Count > 0) {
                        compartmentDamage = (int)MechLightStatuses.Values.Max();
                    }

                    passenger.emitCefEventNoBlock(new VehicleIconUpdateCefEvent("mech", Math.Max(damageLevel, compartmentDamage)));

                    passenger.emitCefEventNoBlock(new VehicleHudUpdateCefEvent(DrivenDistance, Fuel, FuelType != FuelType.None, (int)FuelType));
                } else {
                    var seat = PassengerList.FirstOrDefault(p => p.Value == passenger);
                    //if(!seat.Equals(default(KeyValuePair<int, IPlayer>))) {
                    PassengerList.Remove(seat.Key);
                    //}
                }
            }

            //Dirt Calculation

            DbDirtLevel += dist / Constants.ONE_DIRT_AQUIRE_DISTANCE;

            //Update Dirt
            if(DirtLevel > DbDirtLevel) {
                DbDirtLevel = DirtLevel;
            }

            DirtLevel = Convert.ToByte(Math.Round(DbDirtLevel));
            
            if(dist > 5) {
                VehicleMotorCompartmentController.onVehicleDrive(this, dist / 1000);
            }

            if(EngineOn) {
                if((BatteryDamageLevel == PartDamageLevel.Stutter || EngineDamageLevel == PartDamageLevel.Stutter)) {
                    var random = new Random();
                    if(random.NextDouble() <= 0.05) {
                        EngineOn = false;

                        InvokeController.AddTimedInvoke("Battery-Stutter-On", (i) => {
                            EngineOn = true;
                        }, TimeSpan.FromSeconds(random.Next(2, 5)), false);
                    }
                }

                if(LightDamageLevel == PartDamageLevel.Stutter) {
                    var random = new Random();
                    if(random.NextDouble() <= 0.25) {
                        LightState = LightState == 1 ? (byte)2 : (byte)1;

                        InvokeController.AddTimedInvoke("Light-Stutter-On", (i) => {
                            LightState = LightState == 1 ? (byte)2 : (byte)1;
                        }, TimeSpan.FromSeconds(random.Next(1, 3)), false);
                    }
                }
            }

            if(LightDamageLevel == PartDamageLevel.Broken || BatteryDamageLevel == PartDamageLevel.Broken) {
                LightState = 1;
            }


            LastLongTickDrivenDistance = DrivenDistance;

            //Save DamageToDb
            foreach(var value in CompPartDamageChanges.Reverse()) {
                setPermanentData(value.Key, value.Value.ToString());
            }

            CompPartDamageChanges = new();

            if(MotorCompartmentGame != null && MotorCompartmentGame.UpdateFlag) {
                MotorCompartmentGame.UpdateFlag = false;
                setPermanentData("MECHANICAL_GAME", MotorCompartmentGame.getStringRepresentation());
            } else if(MotorCompartmentGame == null && this.hasData("MECHANICAL_GAME")) {
                resetPermantData("MECHANICAL_GAME");
            }
        }

        public void tryToggleLight() {
            if(LightDamageLevel != PartDamageLevel.Broken && BatteryDamageLevel != PartDamageLevel.Broken) {
                LightState = LightState == 1 ? (byte)2 : (byte)1;
            }
        }

        public float getSize() {
            if(DbModel != null) {
                var pos1 = DbModel.StartPoint.FromJson();
                var pos2 = DbModel.EndPoint.FromJson();

                var width = pos2.X - pos1.X;
                var length = pos2.Y - pos1.Y;

                return Math.Max(width, length);
            } else {
                return 10;
            }
        }

        public MechanicalGame MotorCompartmentGame { get; set; }

        private Dictionary<string, float> CompPartDamage = new();
        private Dictionary<string, float> CompPartDamageChanges = new();

        public float getCompartmentPartDamage(string identifier) {
            if(CompPartDamage.ContainsKey(identifier)) {
                return CompPartDamage[identifier];
            } else {
                return 0;
            }

            //if(this.hasData($"COMP_PART_{identifier}")) {
            //    return float.Parse(this.getData($"COMP_PART_{identifier}"));
            //} else {
            //    return 0;
            //}
        }

        public void setCompartmentPartDamage(string identifier, float damage, bool saveChange = true) {
            if(damage > 1) {
                damage = 1;
            }

            //TODO CHANGE SO IT FLUSH SAVES TO DB ON E.G. VEHICLE SAVE
            //Logger.logError($"Set Compartment Damage: {identifier} {damage}");

            CompPartDamage[identifier] = damage;
            if(saveChange) {
                CompPartDamageChanges[identifier] = damage;
            }

            //this.setPermanentData($"COMP_PART_{identifier}", damage.ToString());
        }

        public void modifyHandlingMeta(string property, float multiplierFromOriginal) {
            SetStreamSyncedMetaData($"HANDLING_{property}", multiplierFromOriginal);
        }

        public enum MechLightColors {
            Yellow = 1,
            Red = 2,
        }

        private Dictionary<string, MechLightColors> MechLightStatuses = new();
        public void addVehicleMechLightStatus(string origin, MechLightColors color) {
            MechLightStatuses[origin] = color;
        }

        public void removeVehicleMechLightStatus(string origin) {
            MechLightStatuses.Remove(origin);
        }


        Dictionary<byte, bool> DoorStates = new();
        public void setHoodState(bool open) {
            setDoorState(4, open);
        }

        public bool isHoodOpen() {
            return isDoorOpen(4);
        }

        public void setTrunkState(bool open) {
            setDoorState(5, open);
        }

        public bool isTrunkOpen() {
            return isDoorOpen(5);
        }

        public void setDoorState(byte doorId, bool open) {
            SetDoorState(doorId, open ? (byte)6 : (byte)0);

            if(open) {
                DoorStates[doorId] = open;
            } else {
                DoorStates.Remove(doorId);
            }
        }

        public bool isDoorOpen(byte doorId) {
            return DoorStates.ContainsKey(doorId);
        }

        internal void repairAllDamages() {
            Repair();

            VehicleDamage.repairEverything(this);


            var elements = VehicleDamage.getDamageElements(this);

            foreach(var element in elements) {
                resetPermantData($"REPAIR_TYPE_{element.Type}");
            }

            refreshVehicle();
        }
    }
}
