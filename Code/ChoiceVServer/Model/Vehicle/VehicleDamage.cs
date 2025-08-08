using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;
using System.Collections.Generic;

namespace ChoiceVServer.Model {
    public enum VehicleDamagePartType {
        Wheels = 0,
        Doors = 1,
        Bumper = 2,
        Window = 3,
        Light = 4,
        Part = 5,
    }

    public class VehicleDamage {
        public string AppearanceData;
        public string DamageData;
        public string HealthData;

        public VehicleDamage() { }

        public void apply(ChoiceVVehicle vehicle) {
            if(AppearanceData != null) {
                vehicle.AppearanceData = AppearanceData;
            }

            if(DamageData != null) {
                vehicle.DamageData = DamageData;
            }

            if(HealthData != null) {
                vehicle.HealthData = HealthData;
            }
        }

        public void read(ChoiceVVehicle vehicle) {
            AppearanceData = vehicle.AppearanceData;
            DamageData = vehicle.DamageData;
            HealthData = vehicle.HealthData;
        }

        public void fillFromDb(vehicle dbVehicle) {
            if(dbVehicle.vehiclesdamagebasis != null) {
                AppearanceData = dbVehicle.vehiclesdamagebasis.appearanceData;
                DamageData = dbVehicle.vehiclesdamagebasis.damageData;
                HealthData = dbVehicle.vehiclesdamagebasis.healthData;
            }
        }

        public void saveToDb(ref vehicle dbDamage) {
            if(dbDamage.vehiclesdamagebasis == null) {
                dbDamage.vehiclesdamagebasis = new vehiclesdamagebasis {
                    vehicleId = dbDamage.id,
                };
            }

            dbDamage.vehiclesdamagebasis.appearanceData = AppearanceData;
            dbDamage.vehiclesdamagebasis.damageData = DamageData;
            dbDamage.vehiclesdamagebasis.healthData = HealthData;
        }

        public void destroyEverything(ChoiceVVehicle vehicle) {
            for(byte i = 0; i < vehicle.DbModel.tyreCount; i++) {
                vehicle.SetWheelBurst(i, true);
                vehicle.SetWheelDetached(i, true);
                vehicle.SetWheelHasTire(i, false);
            }

            for(byte i = 0; i < vehicle.DbModel.windowCount; i++) {
                vehicle.SetWindowDamaged(i, true);
            }

            for(byte i = 0; i < vehicle.DbModel.doorCount; i++) {
                vehicle.SetDoorState(i, 255);
            }

            for(byte i = 0; i < 6; i++) {
                vehicle.SetLightDamaged(i, true);
            }

            for(byte i = 0; i < 6; i++) {
                vehicle.SetPartDamageLevel(i, 3);
            }

            //if(vehicle.NetworkOwner != null) {
            //    vehicle.NetworkOwner.emitClientEvent("VEHICLE_DESTROYED", vehicle);
            //}
        }

        public void repairEverything(ChoiceVVehicle vehicle) {
            for(byte i = 0; i < vehicle.DbModel.tyreCount; i++) {
                vehicle.SetWheelFixed(i);
            }

            for(byte i = 0; i < vehicle.DbModel.windowCount; i++) {
                vehicle.SetWindowDamaged(i, false);
            }

            for(byte i = 0; i < 10; i++) {
                vehicle.SetLightDamaged(i, false);
            }

            for(byte i = 0; i < 6; i++) {
                vehicle.SetPartDamageLevel(i, 0);
                vehicle.SetPartBulletHoles(i, 0);
            }

            if(vehicle.NetworkOwner != null) {
                vehicle.NetworkOwner.emitClientEvent("VEHICLE_REPAIRED", vehicle);
            }
        }

        public record VehicleDamageElement(VehicleDamagePartType Type, int Amount);

        public List<VehicleDamageElement> getDamageElements(ChoiceVVehicle vehicle) {
            return new List<VehicleDamageElement> {
                new VehicleDamageElement(VehicleDamagePartType.Wheels, getDamagedPartAmount(vehicle, VehicleDamagePartType.Wheels)),
                new VehicleDamageElement(VehicleDamagePartType.Doors, getDamagedPartAmount(vehicle, VehicleDamagePartType.Doors)),
                new VehicleDamageElement(VehicleDamagePartType.Bumper, getDamagedPartAmount(vehicle, VehicleDamagePartType.Bumper)),
                new VehicleDamageElement(VehicleDamagePartType.Window, getDamagedPartAmount(vehicle, VehicleDamagePartType.Window)),
                new VehicleDamageElement(VehicleDamagePartType.Light, getDamagedPartAmount(vehicle, VehicleDamagePartType.Light)),
                new VehicleDamageElement(VehicleDamagePartType.Part, getDamagedPartAmount(vehicle, VehicleDamagePartType.Part)),
            };
        }

        public static string getDamageElementName(VehicleDamagePartType type) {
            switch(type) {
                case VehicleDamagePartType.Wheels:
                    return "Reifen";
                case VehicleDamagePartType.Doors:
                    return "Türen";
                case VehicleDamagePartType.Bumper:
                    return "Stoßstangen";
                case VehicleDamagePartType.Window:
                    return "Fenster";
                case VehicleDamagePartType.Light:
                    return "Lichter";
                case VehicleDamagePartType.Part:
                    return "Verkleidung";
                default:
                    return "Unbekannt";
            }
        }

        public int getDamagedPartAmount(ChoiceVVehicle vehicle, VehicleDamagePartType partType) {
            var count = 0;
            switch(partType) {
                case VehicleDamagePartType.Wheels:
                    for(byte i = 0; i < vehicle.WheelsCount; i++) {
                        if(vehicle.IsWheelBurst(i)) {
                            count++;
                        }
                    }
                    return count;
                case VehicleDamagePartType.Window:
                    for(byte i = 0; i < 6; i++) {
                        if(vehicle.IsWindowDamaged(i)) {
                            count++;
                        }
                    }
                    return count;
                case VehicleDamagePartType.Doors:
                    for(byte i = 0; i < 6; i++) {
                        if(vehicle.GetDoorState(i) == 255) {
                            count++;
                        }
                    }
                    return count;
                case VehicleDamagePartType.Bumper:
                    for(byte i = 0; i < 2; i++) {
                        if(vehicle.GetBumperDamageLevel(i) > 0) {
                            count++;
                        }
                    }
                    return count;
                case VehicleDamagePartType.Light:
                    for(byte i = 0; i < 6; i++) {
                        if(vehicle.IsLightDamaged(i)) {
                            count++;
                        }
                    }
                    return count;
                case VehicleDamagePartType.Part:
                    for(byte i = 0; i < 6; i++) {
                        if(vehicle.GetPartDamageLevel(i) > 1 || vehicle.GetPartBulletHoles(i) > 1) {
                            count++;
                        }
                    }
                    return count;
                default:
                    return -1;

            }
        }

        public int getDamageLevel(ChoiceVVehicle vehicle) {
            var elements = getDamageElements(vehicle);

            var level = 0;
            foreach(var element in elements) {
                if(element.Amount > 0) {
                    switch(element.Type) {
                        case VehicleDamagePartType.Wheels:
                            return 2;
                        case VehicleDamagePartType.Doors:
                            return 2;
                        case VehicleDamagePartType.Bumper:
                            level = vehicle.GetBumperDamageLevel(0) + vehicle.GetBumperDamageLevel(1);
                            if(level >= 2) {
                                return 2;
                            }
                            break;
                        case VehicleDamagePartType.Window:
                            level = 1;
                            break;
                        case VehicleDamagePartType.Light:
                            level = 1;
                            break;
                        case VehicleDamagePartType.Part:
                            for(byte i = 0; i < 6; i++) {
                                var part = vehicle.GetPartDamageLevel(i);
                                if(part >= 3) {
                                    return 2;
                                } else if(part >= 2) {
                                    level = 1;
                                }
                            }
                            break;
                    }
                }
            }

            return level;
        }
    }
}