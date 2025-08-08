using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;

namespace ChoiceVServer.Controller.Vehicles {
    public class SteeringMotorCompartmentPart : NonStaticVehicleMotorCompartmentPart {
        private string Name;
        private static float STEERING_PART_DAMAGE_GAIN_MULTIPLIER = 1 / 100f;
        private static float STEERING_PART_KILOMETER_DAMAGE_GAIN = 0.00044f;

        public SteeringMotorCompartmentPart(string identifier, SpecialToolFlag loosenToolFlag, string name, string gameIcon) : base(identifier, loosenToolFlag, gameIcon, VehicleMotorCompartmentRepairType.Mechanics, VehicleMotorCompartmentRepairType.MotorRepair) {
            Name = name;
        }

        protected override void onApplyToVehicleStep(ChoiceVVehicle vehicle, float damage) {
            if(damage > 0.65) {
                if(damage >= 0.9) {
                    vehicle.addVehicleMechLightStatus(Identifier, ChoiceVVehicle.MechLightColors.Red);
                } else {
                    vehicle.addVehicleMechLightStatus(Identifier, ChoiceVVehicle.MechLightColors.Yellow);
                }

                vehicle.modifyHandlingMeta("steeringLock", damage.Map(0.65f, 1, 1, 0));
            } else {
                vehicle.removeVehicleMechLightStatus(Identifier);
                vehicle.modifyHandlingMeta("steeringLock", 1);
            }
        }

        public override bool onUpdate(ChoiceVVehicle vehicle, VehicleMotorCompartmentUpdateType type, float damage) {
            if(type == VehicleMotorCompartmentUpdateType.Motordamage) {
                var alreadyDamage = vehicle.getCompartmentPartDamage(Identifier);
                if(alreadyDamage >= 1) return false;
                vehicle.setCompartmentPartDamage(Identifier, alreadyDamage + damage * STEERING_PART_DAMAGE_GAIN_MULTIPLIER);
                return true;
            } else if(type == VehicleMotorCompartmentUpdateType.KilometerTick) {
                var alreadyDamage = vehicle.getCompartmentPartDamage(Identifier);
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
