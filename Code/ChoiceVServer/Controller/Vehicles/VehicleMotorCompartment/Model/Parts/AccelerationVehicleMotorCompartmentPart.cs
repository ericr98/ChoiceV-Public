using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;

namespace ChoiceVServer.Controller.Vehicles {
    public class AccelerationVehicleMotorCompartmentPart : NonStaticVehicleMotorCompartmentPart {
        private static float ACCELERATION_PART_DAMAGE_GAIN_MULTIPLIER = 1 / 95f;
        private static float ACCELERATION_PART_KILOMETER_DAMAGE_GAIN = 0.000525f;

        private string Name;

        public AccelerationVehicleMotorCompartmentPart(string identifier, SpecialToolFlag loosenToolFlag, string name, string gameIcon, VehicleMotorCompartmentRepairType replaceType, VehicleMotorCompartmentRepairType repairType = VehicleMotorCompartmentRepairType.None) : base(identifier, loosenToolFlag, gameIcon, replaceType, repairType) {
            Name = name;
        }

        protected override void onApplyToVehicleStep(ChoiceVVehicle vehicle, float damage) {
            if(damage >= 0.65) {
                vehicle.addVehicleMechLightStatus(Identifier, ChoiceVVehicle.MechLightColors.Yellow);
                vehicle.modifyHandlingMeta("acceleration", damage.Map(0.65f, 1, 1, 0.5f));
            } else {
                vehicle.removeVehicleMechLightStatus(Identifier);
                vehicle.modifyHandlingMeta("acceleration", 1);
            }
        }

        public override bool onUpdate(ChoiceVVehicle vehicle, VehicleMotorCompartmentUpdateType type, float damage) {
            if(type == VehicleMotorCompartmentUpdateType.Motordamage) {
                var alreadyDamage = vehicle.getCompartmentPartDamage(Identifier);
                if(alreadyDamage >= 1) return false;
                vehicle.setCompartmentPartDamage(Identifier, alreadyDamage + damage * ACCELERATION_PART_DAMAGE_GAIN_MULTIPLIER);
                return true;
            } else if(type == VehicleMotorCompartmentUpdateType.KilometerTick) {
                var alreadyDamage = vehicle.getCompartmentPartDamage(Identifier);
                if(alreadyDamage >= 1) return false;
                vehicle.setCompartmentPartDamage(Identifier, alreadyDamage + ACCELERATION_PART_KILOMETER_DAMAGE_GAIN * damage);
                return true;
            }

            return false;
        }

        public override string getNameOrSerialNumber() {
            return Name;
        }
    }
}
