using AltV.Net.Data;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;


public class VehicleColoring {
    public byte Livery { get; private set; }
    public byte PrimaryColor { get; private set; }
    public byte SecondaryColor { get; private set; }

    public Rgba PrimaryColorRGB { get; private set; }
    public Rgba SecondaryColorRGB { get; private set; }

    public byte PearlColor { get; private set; }

    public bool UsingPrimaryRGB { get => PrimaryColorRGB != Rgba.Zero; }
    public bool UsingSecondaryRGB { get => SecondaryColorRGB != Rgba.Zero; }

    public bool HasPearlColoring { get => PearlColor != 0; }

    public VehicleColoring(byte startColor) {
        Livery = 0;
        PrimaryColor = startColor;
        SecondaryColor = 0;
        PearlColor = 0;

        PrimaryColorRGB = Rgba.Zero;
        SecondaryColorRGB = Rgba.Zero;
    }

    public static VehicleColoring fromDb(vehiclescoloring dbColoring) {
        var coloring = new VehicleColoring(0);
        
        coloring.Livery = (byte)(dbColoring.livery);

        coloring.PrimaryColor = (byte)dbColoring.primaryColor;
        coloring.SecondaryColor = (byte)dbColoring.secondaryColor;

        coloring.PrimaryColorRGB = dbColoring.primaryColorRGB.FromJson<Rgba>();
        coloring.SecondaryColorRGB = dbColoring.secondaryColorRGB.FromJson<Rgba>();

        coloring.PearlColor = (byte)dbColoring.pearlColor;

        return coloring;
    }

    public void updateDb(vehiclescoloring dbColoring) {
        dbColoring.primaryColor = PrimaryColor;
        dbColoring.secondaryColor = SecondaryColor;
        dbColoring.primaryColorRGB = PrimaryColorRGB.ToJson();
        dbColoring.secondaryColorRGB = SecondaryColorRGB.ToJson();
        dbColoring.pearlColor = PearlColor;
        dbColoring.livery = Livery;
    }

    public void setRGBColor(Rgba color, bool primary) {
        if(primary) {
            PrimaryColor = 0;
            PrimaryColorRGB = color;
        } else {
            SecondaryColor = 0;
            SecondaryColorRGB = color;
        }
    }

    public void setColor(byte color, bool primary) {
        if(primary) {
            PrimaryColorRGB = Rgba.Zero;
            PrimaryColor = color;
        } else {
            SecondaryColorRGB = Rgba.Zero;
            SecondaryColor = color;
        }
    }

    public void setPearlColor(byte color) {
        PearlColor = color;
    }
    
    public void setLivery(byte livery) {
        Livery = livery;
    }
}











































//using System;
//using System.Linq;
//using AltV.Net.Data;
//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.Model.Database;
//using static ChoiceVServer.Base.Constants;
//using ChoiceVServer.Controller;

//namespace ChoiceVServer.Model.Vehicle {

//    public class ModColor {
//        public PaintType type = PaintType.Normal;
//        public byte color = 0;
//    }

//    public class VehicleColoring {
//        public int VehicleId;

//        public string AppearanceData = "";

//        public bool IsPrimaryColorRGBA = false;
//        public Rgba PrimaryColorRGBA = Rgba.Zero;

//        public bool IsSecondaryColorRGBA = false;
//        public Rgba SecondaryColorRGBA = Rgba.Zero;

//        public byte PrimaryColor = 0;
//        public byte SecondaryColor = 0;

//        public byte PearlColor = 0;
//        public byte WheelColor = 0;

//        public byte Livery = 1;
//        public byte RoofLivery = 1;

//        public PaintType ModColor1Type = PaintType.Normal;
//        public int ModColor1 = -1;
//        public PaintType ModColor2Type = PaintType.Normal;
//        public int ModColor2 = -1;

//        public bool Sanded1 = false;
//        public bool Sanded2 = false;
//        public bool Sanded3 = false;

//        public bool Sanding = false;
//        public bool Initialized = false;
//        public bool DataChanged = false;
//        public bool NoSave = false;

//        public VehicleColoring() { }

//        public VehicleColoring Clone() {
//            return (VehicleColoring)this.MemberwiseClone();
//        }

//        public bool createVehicleColoring() {
//            try {
//                if (!NoSave) {
//                    using (var db = new ChoiceVDb()) {
//                        var row = new vehiclescoloring {
//                            vehicleId = VehicleId,

//                            appearanceData = AppearanceData.ToJson(),

//                            IsPrimaryColorRGBA = IsPrimaryColorRGBA ? 1 : 0,
//                            PrimaryColorRGBA = PrimaryColorRGBA.ToJson(),

//                            IsSecondaryColorRGBA = IsSecondaryColorRGBA ? 1 : 0,
//                            SecondaryColorRGBA = SecondaryColorRGBA.ToJson(),

//                            PrimaryColor = PrimaryColor,
//                            SecondaryColor = SecondaryColor,

//                            PearlColor = PearlColor,
//                            WheelColor = WheelColor,

//                            Livery = Livery,
//                            RoofLivery = RoofLivery,

//                            ModColor1Type = (byte)ModColor1Type,
//                            ModColor1 = ModColor1,
//                            ModColor2Type = (byte)ModColor2Type,
//                            ModColor2 = ModColor2,

//                            Sanded1 = Sanded1 ? 1 : 0,
//                            Sanded2 = Sanded2 ? 1 : 0,
//                            Sanded3 = Sanded3 ? 1 : 0,
//                        };

//                        var already = db.vehiclescoloring.Find(VehicleId);
//                        if(already == null) {
//                            db.vehiclescoloring.Add(row);
//                        } else {
//                            already = row;
//                        }
//                        db.SaveChanges();
//                        db.Dispose();

//                        DataChanged = false;

//                        // Logger.logInfo($"Vehicle coloring for ID {VehicleId} has been created.");

//                        return true;
//                    }
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }

//            return false;
//        }

//        public void updateVehicleColoring() {
//            try {
//                if (!NoSave) {
//                    using (var db = new ChoiceVDb()) {
//                        var row = db.vehiclescoloring.FirstOrDefault(v => v.vehicleId == VehicleId);
//                        if (row != null) {
//                            row.appearanceData = AppearanceData.ToJson();

//                            row.IsPrimaryColorRGBA = IsPrimaryColorRGBA ? 1 : 0;
//                            row.PrimaryColorRGBA = PrimaryColorRGBA.ToJson();

//                            row.IsSecondaryColorRGBA = IsSecondaryColorRGBA ? 1 : 0;
//                            row.SecondaryColorRGBA = SecondaryColorRGBA.ToJson();

//                            row.PrimaryColor = PrimaryColor;
//                            row.SecondaryColor = SecondaryColor;

//                            row.PearlColor = PearlColor;
//                            row.WheelColor = WheelColor;

//                            row.Livery = Livery;
//                            row.RoofLivery = RoofLivery;

//                            row.ModColor1Type = (byte)ModColor1Type;
//                            row.ModColor1 = ModColor1;
//                            row.ModColor2Type = (byte)ModColor2Type;
//                            row.ModColor2 = ModColor2;

//                            row.Sanded1 = Sanded1 ? 1 : 0;
//                            row.Sanded2 = Sanded2 ? 1 : 0;
//                            row.Sanded3 = Sanded3 ? 1 : 0;

//                            db.SaveChanges();

//                            DataChanged = false;

//                            // Logger.logInfo($"Vehicle coloring for ID {VehicleId} has been saved.");
//                        }

//                        db.Dispose();
//                    }
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public void deleteVehicleColoring() {
//            try {
//                if (!NoSave) {
//                    using (var db = new ChoiceVDb()) {
//                        var row = db.vehiclescoloring.FirstOrDefault(v => v.vehicleId == VehicleId);
//                        if (row != null) {
//                            db.vehiclescoloring.Remove(row);
//                            db.SaveChanges();

//                            DataChanged = false;

//                            // Logger.logDebug($"Vehicle coloring for ID {VehicleId} has been deleted.");
//                        }

//                        db.Dispose();
//                    }
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public bool fetchVehicleColoring(ChoiceVVehicle vehicle) {
//            if (Initialized) {
//                AppearanceData = vehicle.AppearanceData;

//                IsPrimaryColorRGBA = vehicle.IsPrimaryColorRgb;
//                PrimaryColorRGBA = vehicle.PrimaryColorRgb;

//                IsSecondaryColorRGBA = vehicle.IsSecondaryColorRgb;
//                SecondaryColorRGBA = vehicle.SecondaryColorRgb;

//                PrimaryColor = vehicle.PrimaryColor;
//                SecondaryColor = vehicle.SecondaryColor;

//                PearlColor = vehicle.PearlColor;
//                WheelColor = vehicle.WheelColor;

//                Livery = vehicle.Livery;
//                RoofLivery = vehicle.RoofLivery;

//                DataChanged = true;

//                // Logger.logWarning($"fetchVehicleColoring: {VehicleId}.");
//            }

//            return true;
//        }

//        public bool applyVehicleColoring(ChoiceVVehicle vehicle) {
//            if (AppearanceData.Length > 0)
//                vehicle.AppearanceData = AppearanceData;
//            else if (vehicle.AppearanceData.Length > 0)
//                AppearanceData = vehicle.AppearanceData;

//            if (IsPrimaryColorRGBA)
//                vehicle.PrimaryColorRgb = PrimaryColorRGBA;
//            else
//                vehicle.PrimaryColor = PrimaryColor;

//            if (IsSecondaryColorRGBA)
//                vehicle.SecondaryColorRgb = SecondaryColorRGBA;
//            else
//                vehicle.SecondaryColor = SecondaryColor;

//            vehicle.PearlColor = PearlColor;
//            vehicle.WheelColor = WheelColor;

//            vehicle.Livery = Livery;
//            vehicle.RoofLivery = RoofLivery;

//            vehicle.SetSyncedMetaData("VehicleColorLivery", (int)Livery);
//            vehicle.SetSyncedMetaData("VehicleColorRoofLivery", (int)RoofLivery);

//            if (Initialized)
//                DataChanged = false;
//            else
//                DataChanged = true;

//            Initialized = true;

//            // Logger.logWarning($"applyVehicleColoring: {VehicleId}.");

//            return true;
//        }

//        public bool resetColoring(ChoiceVVehicle vehicle) {
//            AppearanceData = "AAAAAA";

//            IsPrimaryColorRGBA = false;
//            PrimaryColorRGBA = Rgba.Zero;

//            IsSecondaryColorRGBA = false;
//            SecondaryColorRGBA = Rgba.Zero;

//            PrimaryColor = 0;
//            SecondaryColor = 0;

//            PearlColor = 0;
//            WheelColor = 0;

//            Livery = 1;
//            RoofLivery = 1;

//            ModColor1Type = PaintType.Normal;
//            ModColor1 = -1;
//            ModColor2Type = PaintType.Normal;
//            ModColor2 = -1;

//            Sanding = false;
//            Sanded1 = false;
//            Sanded2 = false;
//            Sanded3 = false;

//            Initialized = false;
//            DataChanged = true;

//            return true;
//        }

//        public bool setPrimaryColor(ChoiceVVehicle vehicle, byte color) {
//            vehicle.PrimaryColor = color;

//            IsPrimaryColorRGBA = false;
//            PrimaryColor = color;

//            ModColor1Type = PaintType.Normal;
//            ModColor1 = -1;

//            DataChanged = true;
//            return true;
//        }

//        public bool setPrimaryColor(ChoiceVVehicle vehicle, Rgba color) {
//            vehicle.PrimaryColorRgb = color;

//            IsPrimaryColorRGBA = true;
//            PrimaryColorRGBA = color;

//            ModColor1Type = PaintType.Normal;
//            ModColor1 = -1;

//            DataChanged = true;
//            return true;
//        }

//        public bool setSecondaryColor(ChoiceVVehicle vehicle, byte color) {
//            vehicle.SecondaryColor = color;

//            IsSecondaryColorRGBA = false;
//            SecondaryColor = color;

//            ModColor2Type = PaintType.Normal;
//            ModColor2 = -1;

//            DataChanged = true;
//            return true;
//        }

//        public bool setSecondaryColor(ChoiceVVehicle vehicle, Rgba color) {
//            vehicle.SecondaryColorRgb = color;

//            IsSecondaryColorRGBA = true;
//            SecondaryColorRGBA = color;

//            ModColor2Type = PaintType.Normal;
//            ModColor2 = -1;

//            DataChanged = true;
//            return true;
//        }

//        public bool setPearlColor(ChoiceVVehicle vehicle, byte color) {
//            vehicle.PearlColor = color;
//            PearlColor = color;

//            DataChanged = true;
//            return true;
//        }

//        public bool setWheelColor(ChoiceVVehicle vehicle, byte color) {
//            vehicle.WheelColor = color;
//            WheelColor = color;

//            DataChanged = true;
//            return true;
//        }

//        public bool setLivery(ChoiceVVehicle vehicle, byte livery) {
//            vehicle.Livery = livery;
//            Livery = livery;

//            vehicle.SetSyncedMetaData("VehicleColorLivery", (int)Livery);

//            DataChanged = true;
//            return true;
//        }

//        public bool setRoofLivery(ChoiceVVehicle vehicle, byte livery) {
//            vehicle.RoofLivery = livery;
//            RoofLivery = livery;

//            vehicle.SetSyncedMetaData("VehicleColorRoofLivery", (int)RoofLivery);

//            DataChanged = true;
//            return true;
//        }
//    }
//}
