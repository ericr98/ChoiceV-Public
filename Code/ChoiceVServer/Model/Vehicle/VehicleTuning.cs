using AltV.Net.Enums;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;

public class VehicleTuning {
    public int ModKit;

    //All Mods that arnt -1
    public List<VehicleMods> AllUpgradedMods;


    // TODO! Works with vehicle.SetWheels(TYPE, VARIATION)
    // Type is 0 - 7, being like 0: SPORT, 7: HIGH_END, being the different wheel types
    // Variation is just the rim in another form!
    // Color setable with vehicle.WheelColor
    public int WheelType;
    public int WheelVariation;
    public int WheelColor;
    public VehicleTuning() {
        AllUpgradedMods = new List<VehicleMods>();
        ModKit = 1;
    }

    public static bool checkIfModIsConstructable(ChoiceVVehicle vehicle, VehicleModType modType, int level) {
        return vehicle.GetModsCount((byte)modType) <= level;
    }

    public void setMod(VehicleModType type, int level) {
        var mod = AllUpgradedMods.FirstOrDefault(m => m.Type == type);

        if(level != -1) {
            if(mod != null) {
                mod.Level = level;
            } else {
                AllUpgradedMods.Add(new VehicleMods((int)type, level));
            }
        } else {
            if(mod != null) {
                AllUpgradedMods.Remove(mod);
            } else {
                Logger.logWarning(LogCategory.System, LogActionType.Blocked, $"setMod: this shouldnt happen!");
            }
        }
    }

    public bool anyStealableParts() {
        return AllUpgradedMods.Any(m => m.Type == VehicleModType.Spoilers || m.Type == VehicleModType.FrontBumper || m.Type == VehicleModType.RearBumper || m.Type == VehicleModType.SideSkirt || m.Type == VehicleModType.Fender);
    }

    public List<VehicleMods> getStealableParts() {
        return AllUpgradedMods.Where(m => m.Type == VehicleModType.Spoilers || m.Type == VehicleModType.FrontBumper || m.Type == VehicleModType.RearBumper || m.Type == VehicleModType.SideSkirt || m.Type == VehicleModType.Fender).ToList();
    }

    public bool anyModsInstalled() {
        return AllUpgradedMods.Any();
    }

    public int getMod(int modTypeId) {
        return AllUpgradedMods.FirstOrDefault(m => m.ModId == modTypeId)?.Level ?? -1;
    }

    public static VehicleTuning fromDb(vehiclestuningbase baseTuning, List<vehiclestuningmod> mods) {
        var tuning = new VehicleTuning();

        tuning.ModKit = baseTuning.modKit;

        foreach(var mod in mods) {
            tuning.AllUpgradedMods.Add(new VehicleMods(mod.modType, mod.level));
        }

        return tuning;
    }

    public void updateDb(vehiclestuningbase baseTuning, ICollection<vehiclestuningmod> mods, int vehicleId) {
        //Base
        baseTuning.modKit = ModKit;

        foreach(var modType in (VehicleModType[])Enum.GetValues(typeof(VehicleModType))) {
            var tuningMod = AllUpgradedMods.FirstOrDefault(t => t.Type == modType);
            var dbMod = mods.FirstOrDefault(m => m.modType == (int)modType);

            if(tuningMod != null && dbMod != null) {
                dbMod.level = tuningMod.Level;
            } else if(tuningMod == null && dbMod != null) {
                mods.Remove(dbMod);
            } else if(tuningMod != null && dbMod == null) {
                mods.Add(new vehiclestuningmod {
                    vehicleId = vehicleId,
                    modType = tuningMod.ModId,
                    level = tuningMod.Level
                });
            }
        }
    }
}

public class VehicleMods {
    public int ModId;
    public VehicleModType Type { get => (VehicleModType)ModId; }

    public int Level;

    public VehicleMods(int modId, int level) {
        ModId = modId;
        Level = level;
    }
}