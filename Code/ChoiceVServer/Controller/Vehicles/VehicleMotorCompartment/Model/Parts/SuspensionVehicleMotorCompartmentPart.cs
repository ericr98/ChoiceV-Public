using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;

namespace ChoiceVServer.Controller.Vehicles {
    public class SuspensionMotorCompartmentPart : NonStaticVehicleMotorCompartmentPart {
        private string Name;
        private static float STEERING_PART_DAMAGE_GAIN_MULTIPLIER = 1 / 150f;
        private static float STEERING_PART_KILOMETER_DAMAGE_GAIN = 0.00036f;

        public SuspensionMotorCompartmentPart(string identifier, SpecialToolFlag loosenToolFlag, string name) : base(identifier, loosenToolFlag, "mech1x2", VehicleMotorCompartmentRepairType.Mechanics, VehicleMotorCompartmentRepairType.MotorRepair) {
            Name = name;
        }

        protected override void onApplyToVehicleStep(ChoiceVVehicle vehicle, float damage) {
            if(damage > 0.65) {
                if(damage >= 0.9) {
                    vehicle.addVehicleMechLightStatus(Identifier, ChoiceVVehicle.MechLightColors.Red);
                } else {
                    vehicle.addVehicleMechLightStatus(Identifier, ChoiceVVehicle.MechLightColors.Yellow);
                }
                vehicle.modifyHandlingMeta("suspensionForce", damage.Map(0.65f, 1, 1, 6));
            } else {
                vehicle.modifyHandlingMeta("suspensionForce", 1);
                vehicle.removeVehicleMechLightStatus(Identifier);
            }
        }

        public override bool onUpdate(ChoiceVVehicle vehicle, VehicleMotorCompartmentUpdateType type, float damage) {
            var alreadyDamage = vehicle.getCompartmentPartDamage(Identifier);
            if(alreadyDamage >= 0.65) {
                if(type == VehicleMotorCompartmentUpdateType.KilometerTick) {
                    VehicleMotorCompartmentController.applyMotorDamage(vehicle, damage * alreadyDamage * STEERING_PART_KILOMETER_DAMAGE_GAIN);
                } else {
                    VehicleMotorCompartmentController.applyMotorDamage(vehicle, damage * alreadyDamage * STEERING_PART_DAMAGE_GAIN_MULTIPLIER);
                }
            }

            if(type == VehicleMotorCompartmentUpdateType.Motordamage) {
                if(alreadyDamage >= 1) return false;
                vehicle.setCompartmentPartDamage(Identifier, alreadyDamage + damage * STEERING_PART_DAMAGE_GAIN_MULTIPLIER);
                return true;
            } else if(type == VehicleMotorCompartmentUpdateType.KilometerTick) {
                if(alreadyDamage >= 1) return false;
                vehicle.setCompartmentPartDamage(Identifier, alreadyDamage + STEERING_PART_KILOMETER_DAMAGE_GAIN * damage * VehicleMotorCompartment.DEVELOPMENT_SPEEDUP_MULTIPLIER);
                return true;
            }

            return false;
        }

        public override string getNameOrSerialNumber() {
            return Name;
        }
    }
}
