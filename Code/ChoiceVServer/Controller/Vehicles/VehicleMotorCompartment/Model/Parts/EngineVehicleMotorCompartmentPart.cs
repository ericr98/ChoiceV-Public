using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;

namespace ChoiceVServer.Controller.Vehicles {
    public class EngineVehicleMotorCompartmentPart : NonStaticVehicleMotorCompartmentPart {
        private static float ENGINE_PART_DAMAGE_GAIN_MULTIPLIER = 1 / 120f;
        private static float ENGINE_PART_KILOMETER_DAMAGE_GAIN = 0.00045f;

        public EngineVehicleMotorCompartmentPart(string identifier, SpecialToolFlag loosenToolFlag, string gameIcon, VehicleMotorCompartmentRepairType replaceType, VehicleMotorCompartmentRepairType repairType = VehicleMotorCompartmentRepairType.None) : base(identifier, loosenToolFlag, gameIcon, replaceType, replaceType) {

        }

        protected override void onApplyToVehicleStep(ChoiceVVehicle vehicle, float damage) {
            if(damage >= 1) {
                vehicle.EngineDamageLevel = PartDamageLevel.Broken;
                vehicle.addVehicleMechLightStatus(Identifier, ChoiceVVehicle.MechLightColors.Red);
            } else if(damage >= 0.65) {
                vehicle.EngineDamageLevel = PartDamageLevel.Stutter;
                vehicle.addVehicleMechLightStatus(Identifier, ChoiceVVehicle.MechLightColors.Yellow);
                vehicle.modifyHandlingMeta("acceleration", damage.Map(0.65f, 1, 1, 0.5f));
            } else {
                vehicle.EngineDamageLevel = PartDamageLevel.None;
                vehicle.removeVehicleMechLightStatus(Identifier);
                vehicle.modifyHandlingMeta("acceleration", 1);
            }
        }

        public override bool onUpdate(ChoiceVVehicle vehicle, VehicleMotorCompartmentUpdateType type, float damage) {
            if(type == VehicleMotorCompartmentUpdateType.Motordamage) {
                var alreadyDamage = vehicle.getCompartmentPartDamage(Identifier);
                if(alreadyDamage >= 1) return false;
                vehicle.setCompartmentPartDamage(Identifier, alreadyDamage + damage * ENGINE_PART_DAMAGE_GAIN_MULTIPLIER);
                return true;
            } else if(type == VehicleMotorCompartmentUpdateType.KilometerTick) {
                var alreadyDamage = vehicle.getCompartmentPartDamage(Identifier);
                if(alreadyDamage >= 1) return false;
                vehicle.setCompartmentPartDamage(Identifier, alreadyDamage + ENGINE_PART_KILOMETER_DAMAGE_GAIN * damage * VehicleMotorCompartment.DEVELOPMENT_SPEEDUP_MULTIPLIER);
                return true;
            }

            return false;
        }

        public override string getNameOrSerialNumber() {
            return "Motor";
        }
    }
}
