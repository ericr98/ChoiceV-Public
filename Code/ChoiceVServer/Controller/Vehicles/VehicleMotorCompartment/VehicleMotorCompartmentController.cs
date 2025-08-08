using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.Vehicles {
    public record VehicleCompartmentBlueprint(VehicleMotorCompartment Compartment, Predicate<ChoiceVVehicle> applyPredicate);

    public enum VehicleMotorCompartmentRepairType {
        None = -1,
        MotorRepair = 0,
        Lights = 1,
        Mechanics = 2,
        Battery = 3,
        Tube = 4,
        Motor = 5,
        FluidTank = 6,
        Electric = 7,
        Filter = 8,
        ElectricMotor = 9,
        HightVoltageBattery = 10,
    }

    public class VehicleMotorCompartmentController : ChoiceVScript {
        public static List<VehicleCompartmentBlueprint> AllCompartmentBlueprints;
        private static Dictionary<string, VehicleMotorCompartmentPart> AllParts;

        public VehicleMotorCompartmentController() {
            AllParts = new Dictionary<string, VehicleMotorCompartmentPart>();
            AllCompartmentBlueprints = new List<VehicleCompartmentBlueprint>();

            loadParts();
            loadBlueprints();

            EventController.VehicleDamageDelegate += onVehicleDamage;
        }

        //Teile die man reparieren kann können auch ausgetauscht werden müssen, wenn bestimmter Prozentwertwert überstiegen ist
        //Teile sind beim xten mal betrachten nicht identifiert
        private void loadParts() {
            //Lichter
            AllParts.Add("LCH1", new LightMotorCompartmentPart("LCH1", InventorySystem.SpecialToolFlag.Screwdriver));
            AllParts.Add("LCH2", new LightMotorCompartmentPart("LCH2", InventorySystem.SpecialToolFlag.Screwdriver));

            //Lenkermechanik
            AllParts.Add("LMC", new SteeringMotorCompartmentPart("LMC", InventorySystem.SpecialToolFlag.Ratchet, "Kastenteil", "box1x1"));

            //Batterie
            AllParts.Add("BTR", new ElectricalVehicleMotorCompartmentPart("BTR", InventorySystem.SpecialToolFlag.Ratchet, "Batterie", "battery1x2", VehicleMotorCompartmentRepairType.Battery));

            //Bremsleitung
            AllParts.Add("BRL", new BreakPartVehicleMotorCompartmentPart("BRL", InventorySystem.SpecialToolFlag.AutomotivePliers, "Schlauch", "tube1x2"));

            //Stoßfeder
            AllParts.Add("STF", new SuspensionMotorCompartmentPart("STF", InventorySystem.SpecialToolFlag.Ratchet, "Federung"));

            //Stoßdämpfer
            AllParts.Add("STD", new SuspensionMotorCompartmentPart("STD", InventorySystem.SpecialToolFlag.Ratchet, "Federung"));

            //Motor
            AllParts.Add("MTR", new EngineVehicleMotorCompartmentPart("MTR", InventorySystem.SpecialToolFlag.Ratchet, "motor2x2", VehicleMotorCompartmentRepairType.Motor, VehicleMotorCompartmentRepairType.MotorRepair));

            //Bremsflüssigkeitsbehälter
            AllParts.Add("BFB", new BreakPartVehicleMotorCompartmentPart("BFB", InventorySystem.SpecialToolFlag.Ratchet, "Flüssigkeitsbehälter", "fluidTank1x1"));

            //Steuergerät
            AllParts.Add("STG", new ElectricalVehicleMotorCompartmentPart("STG", InventorySystem.SpecialToolFlag.Screwdriver, "elektrisches Gerät", "electric1x1", VehicleMotorCompartmentRepairType.Electric));

            //Einspritzdüse (groß und klein)
            AllParts.Add("ESD", new CanDamageEngineVehicleCompartmentPart("ESD", InventorySystem.SpecialToolFlag.Ratchet, "Kastenteil", "box1x2", VehicleMotorCompartmentRepairType.Mechanics));

            //Kraftstofffilter
            AllParts.Add("KSF", new CanDamageEngineVehicleCompartmentPart("KSF", InventorySystem.SpecialToolFlag.Ratchet, "Kastenteil", "box1x2", VehicleMotorCompartmentRepairType.Filter));

            //Lenkgetriebe
            AllParts.Add("LGB", new SteeringMotorCompartmentPart("LGB", InventorySystem.SpecialToolFlag.Ratchet, "mechanisches Teil", "mech1x2"));

            //Elektromotor
            AllParts.Add("MTR1", new ElectricalEngineVehicleMotorCompartmentPart("MTR1", InventorySystem.SpecialToolFlag.Ratchet, "motor1x1", VehicleMotorCompartmentRepairType.ElectricMotor, VehicleMotorCompartmentRepairType.MotorRepair));
            AllParts.Add("MTR2", new ElectricalEngineVehicleMotorCompartmentPart("MTR2", InventorySystem.SpecialToolFlag.Ratchet, "motor1x1", VehicleMotorCompartmentRepairType.ElectricMotor, VehicleMotorCompartmentRepairType.MotorRepair));
            AllParts.Add("MTR3", new ElectricalEngineVehicleMotorCompartmentPart("MTR3", InventorySystem.SpecialToolFlag.Ratchet, "motor1x1", VehicleMotorCompartmentRepairType.ElectricMotor, VehicleMotorCompartmentRepairType.MotorRepair));
            AllParts.Add("MTR4", new ElectricalEngineVehicleMotorCompartmentPart("MTR4", InventorySystem.SpecialToolFlag.Ratchet, "motor1x1", VehicleMotorCompartmentRepairType.ElectricMotor, VehicleMotorCompartmentRepairType.MotorRepair));

            //Hochvoltbatterie
            AllParts.Add("HVB", new ElectricalVehicleMotorCompartmentPart("HVB", InventorySystem.SpecialToolFlag.Ratchet, "Batterie", "battery2x2", VehicleMotorCompartmentRepairType.HightVoltageBattery));

            //Leistungselektronik
            AllParts.Add("LTE", new AccelerationVehicleMotorCompartmentPart("LTE", InventorySystem.SpecialToolFlag.Screwdriver, "elektrisches Teil", "electric1x1", VehicleMotorCompartmentRepairType.Electric));



            //Kompressor
            AllParts.Add("KMP", new StaticVehicleMotorCompartmentPartWithName("KMP", InventorySystem.SpecialToolFlag.Ratchet, "Kastenteil", "box1x1"));
            //Auspuffrohr
            AllParts.Add("APR", new StaticVehicleMotorCompartmentPartWithName("APR", InventorySystem.SpecialToolFlag.Ratchet, "Auspuffrohr", "exhaust1x2"));
            //Abdeckung
            AllParts.Add("ABD", new StaticVehicleMotorCompartmentPartWithName("ABD", InventorySystem.SpecialToolFlag.Screwdriver, "Abdeckung", "cover2x2"));
            //Kühler
            AllParts.Add("KHL", new StaticVehicleMotorCompartmentPartWithName("KHL", InventorySystem.SpecialToolFlag.Ratchet, "Kastenteil", "box1x2"));
            //Luftfilter
            AllParts.Add("LFT", new StaticVehicleMotorCompartmentPartWithName("LFT", InventorySystem.SpecialToolFlag.Ratchet, "Kastenteil", "box1x2"));
            //Wischwasserbehälter
            AllParts.Add("WWB", new StaticVehicleMotorCompartmentPartWithName("WWB", InventorySystem.SpecialToolFlag.Ratchet, "Flüssigkeitsbehälter", "fluidTank1x1"));
            //Getriebe
            AllParts.Add("GTB1", new StaticVehicleMotorCompartmentPartWithName("GTB1", InventorySystem.SpecialToolFlag.Ratchet, "mechanisches Teil", "mech1x2"));
            AllParts.Add("GTB2", new StaticVehicleMotorCompartmentPartWithName("GTB2", InventorySystem.SpecialToolFlag.Ratchet, "mechanisches Teil", "mech1x2"));
            //Ölfilter
            AllParts.Add("OEF", new StaticVehicleMotorCompartmentPartWithName("OEF", InventorySystem.SpecialToolFlag.Ratchet, "Kastenteil", "box1x1"));
            //Kühlwasserpumpe
            AllParts.Add("KWP", new StaticVehicleMotorCompartmentPartWithName("KWP", InventorySystem.SpecialToolFlag.Ratchet, "Kastenteil", "box1x1"));
            //Schlauch
            AllParts.Add("SCL", new StaticVehicleMotorCompartmentPartWithName("SCL", InventorySystem.SpecialToolFlag.AutomotivePliers, "Schlauch", "tube1x2"));
            //Random Elektropart (Maybe give name?)
            AllParts.Add("REP1", new StaticVehicleMotorCompartmentPartWithName("REP1", InventorySystem.SpecialToolFlag.Screwdriver, "elektrisches Teil", "electric1x2"));
            //Random Elektropart (Maybe give name?)
            AllParts.Add("REP2", new StaticVehicleMotorCompartmentPartWithName("REP2", InventorySystem.SpecialToolFlag.Screwdriver, "elektrisches Teil", "electric1x1"));
            //Kabel
            AllParts.Add("KAB1", new StaticVehicleMotorCompartmentPartWithName("KAB1", InventorySystem.SpecialToolFlag.AutomotivePliers, "Kabel", "cable1x2"));
            AllParts.Add("KAB2", new StaticVehicleMotorCompartmentPartWithName("KAB2", InventorySystem.SpecialToolFlag.AutomotivePliers, "Kabel", "cable1x2"));
            AllParts.Add("KAB3", new StaticVehicleMotorCompartmentPartWithName("KAB3", InventorySystem.SpecialToolFlag.AutomotivePliers, "Kabel", "cable1x2"));
            AllParts.Add("KAB4", new StaticVehicleMotorCompartmentPartWithName("KAB4", InventorySystem.SpecialToolFlag.AutomotivePliers, "Kabel", "cable1x2"));
            //Bremsverstärker
            AllParts.Add("BVS", new StaticVehicleMotorCompartmentPartWithName("BVS", InventorySystem.SpecialToolFlag.Ratchet, "Kastenteil", "box1x1"));
            //Druckluftbehälter
            AllParts.Add("DLB", new StaticVehicleMotorCompartmentPartWithName("DLB", InventorySystem.SpecialToolFlag.Ratchet, "Behälter", "box1x1"));
        }

        private void loadBlueprints() {
            AllCompartmentBlueprints.Add(
                new VehicleCompartmentBlueprint(
                    new VehicleMotorCompartment(
                        "MOTOR_CYCLE",
                        [
                        "LCH1_L LMC  BTR  BTR    STF_L  STF_L  \n" +
                        "BRL_U  MTR  MTR  STD_L  KMP    LCH2_R \n" +
                        "BRL_U  MTR  MTR  STD_L  APR    APR_1"
                        ]
                    ),
                    (c) => c.DbModel.classId == 8
                 )
             );

            AllCompartmentBlueprints.Add(
                new VehicleCompartmentBlueprint(
                    new VehicleMotorCompartment(
                        "TRUCK",
                        [
                        "LCH1_UL  KHL    KHL    LCH2_UR \n" +
                        "WWB      ABD    ABD    BFB     \n" +
                        "STG      ABD    ABD    BTR     \n" +
                        "LFT      LFT    NON    BTR",

                        "KSF      ESD    SCL_D  SCL_D \n" +
                        "GTB1     GTB1   MTR    MTR   \n" +
                        "GTB2     GTB2   MTR    MTR   \n" +
                        "DLB      LGB_L  LGB_L  OEF"
                        ]
                    ),
                    (c) => c.DbModel.classId == 10 || c.DbModel.classId == 12 || c.DbModel.classId == 17 || c.DbModel.classId == 19 || c.DbModel.classId == 20
                 )
             );

            AllCompartmentBlueprints.Add(
                new VehicleCompartmentBlueprint(
                    new VehicleMotorCompartment(
                        "ELECTRO_CAR",
                        [
                        "LCH1_UL  WWB     NON     LCH2_UR \n" +
                        "REP1_U   KAB1_D  KAB1_D  NON     \n" +
                        "REP1_U   KAB2_U  KAB2_U  REP2    \n" +
                        "BTR      BTR     BFB     BVS",

                        "MTR1     STG    LTE    MTR2   \n" +
                        "KAB3_U   HVB    HVB    KAB4_D   \n" +
                        "KAB3_U   HVB    HVB    KAB4_D   \n" +
                        "MTR3     LGB_L  LGB_L  MTR4"
                        ]
                    ),
                    (c) => c.FuelType == FuelType.Electricity
                 )
             );

            AllCompartmentBlueprints.Add(
                new VehicleCompartmentBlueprint(
                    new VehicleMotorCompartment(
                        "NORMAL_CAR",
                        [
                        "LCH1_UL  KHL   KHL    LCH2_UR \n" +
                        "WWB      LFT   LFT    BTR     \n" +
                        "NON      ABD   ABD    BTR     \n" +
                        "BFB      ABD   ABD    STG",

                        "OEF      KWP   SCL_D  SCL_D   \n" +
                        "ESD      MTR   MTR    GTB1_L   \n" +
                        "ESD      MTR   MTR    GTB1_L   \n" +
                        "KSF      LGB_L LGB_L  NON"
                        ]
                    ),
                    (c) => c.DbModel.classId == 0 || c.DbModel.classId == 1 || c.DbModel.classId == 2 || c.DbModel.classId == 3 || c.DbModel.classId == 4 
                            || c.DbModel.classId == 5 || c.DbModel.classId == 6 || c.DbModel.classId == 7 || c.DbModel.classId == 9 || c.DbModel.classId == 12
                 )
             );
        }

        public static VehicleMotorCompartmentPart getCompartmentPart(string identifier) {
            if(AllParts.ContainsKey(identifier)) {
                return AllParts[identifier];
            } else {
                return null;
            }
        }

        public static List<string> getAllDamagablePartIdentifiers() {
            return AllParts.Where(p => p.Value is NonStaticVehicleMotorCompartmentPart).Select(p => p.Key).ToList();
        }

        public static string getSerialNumberForIdentifier(string identifier) {
            //TODO Make DB Access and return a random serial number

            return "TODO";
        }

        public static VehicleMotorCompartment getCompartmentForVehicle(ChoiceVVehicle vehicle) {
            VehicleCompartmentBlueprint blueprint;
            if(vehicle.hasData("COMPARTMENT_IDENTIFIER")) {
                var idf = (string)vehicle.getData("COMPARTMENT_IDENTIFIER");
                blueprint = AllCompartmentBlueprints.FirstOrDefault(b => b.Compartment.Identifier == idf);
            } else {
                blueprint = AllCompartmentBlueprints.FirstOrDefault(v => v.applyPredicate(vehicle));
            }

            if(blueprint != null) {
                return blueprint.Compartment;
            } else {
                return null;
            }
        }

        private void onVehicleDamage(IVehicle target, IEntity attacker, uint bodyHealthDamage, uint additionalBodyHealthDamage, uint engineHealthDamage, uint petrolTankDamage, uint weaponHash) {
            var vehicle = (ChoiceVVehicle)target;

            if(vehicle.IgnoreFirstDamage) {
                vehicle.IgnoreFirstDamage = false;
                return;
            }

            if(target.EngineHealth < 250) {
                target.EngineHealth = 999;
            }

            if(target.PetrolTankHealth < 250) {
                target.PetrolTankHealth = 999;
            }

            if(attacker == null && (bodyHealthDamage == 1000 || additionalBodyHealthDamage == 1000 || engineHealthDamage == 1000 || petrolTankDamage == 1000)) {
                return;
            }

            var externalAttacker = (attacker is IPlayer player) && player.Vehicle != target;
            var damageMultiplier = 1.5f * (externalAttacker ? 0.25f : 1f);
            getCompartmentForVehicle(vehicle)?
                .onVehicleDamage(vehicle, Math.Max((uint)(bodyHealthDamage * damageMultiplier), (uint)(additionalBodyHealthDamage * damageMultiplier)), (uint)(engineHealthDamage * damageMultiplier));
        }

        public static void onVehicleDrive(ChoiceVVehicle vehicle, float kilometers) {
            var compartment = getCompartmentForVehicle(vehicle);
            if(compartment != null) {
                compartment.onVehicleDrive(vehicle, kilometers);
            }
        }

        /// <summary>
        /// damage in range from 0 to 1
        /// </summary>
        public static void applyMotorDamage(ChoiceVVehicle vehicle, float damage) {
            var compartment = getCompartmentForVehicle(vehicle);
            if(compartment != null) {
                compartment.applyMotorDamage(vehicle, damage);
            }
        }

        public static void setPartDamageLevel(ChoiceVVehicle vehicle, string identifier, float damageLevel) {
            var compartment = getCompartmentForVehicle(vehicle);
            if(compartment != null) {
                compartment.setPartDamageLevel(vehicle, identifier, damageLevel);
            }
        }

        public static void applyCompartment(ChoiceVVehicle vehicle) {
            var compartment = getCompartmentForVehicle(vehicle);
            if(compartment != null) {
                compartment.applyCompartment(vehicle);
            }
        }

        public static void onSpawnVehicle(ChoiceVVehicle vehicle) {
            applyCompartment(vehicle);

            if(vehicle.hasData("MECHANICAL_GAME")) {
                vehicle.MotorCompartmentGame = new MechanicalGame((string)vehicle.getData("MECHANICAL_GAME"), (p, g, c, a) => { return true; });

                if(vehicle.DbModel.trunkInBack == 1) {
                    vehicle.setHoodState(true);
                } else {
                    vehicle.setTrunkState(true);
                }
            }
        }

        public static void destroyEverything(ChoiceVVehicle vehicle) {
            var compartment = getCompartmentForVehicle(vehicle);
            if(compartment != null) {
                compartment.destroyEverything(vehicle);
            }
        }

        public static void repairEverything(ChoiceVVehicle vehicle) {
            var compartment = getCompartmentForVehicle(vehicle);
            if(compartment != null) {
                compartment.repairEverything(vehicle);
            }
        }
    }
}
