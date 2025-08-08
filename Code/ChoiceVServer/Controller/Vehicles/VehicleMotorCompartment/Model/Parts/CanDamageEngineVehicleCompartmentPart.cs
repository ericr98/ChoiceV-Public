using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;

namespace ChoiceVServer.Controller.Vehicles {
    public class CanDamageEngineVehicleCompartmentPart : NonStaticVehicleMotorCompartmentPart {
        private static float CAN_DAMAGE_ENGINE_PART_DAMAGE_GAIN_MULTIPLIER = 1 / 750f;
        private static float CAN_DAMAGE_ENGINE_PART_KILOMETER_GAIN_MULTIPLIER = 0.00034f;

        private string Name;

        public CanDamageEngineVehicleCompartmentPart(string identifier, SpecialToolFlag loosenToolFlag, string name, string gameIcon, VehicleMotorCompartmentRepairType replaceType, VehicleMotorCompartmentRepairType repairType = VehicleMotorCompartmentRepairType.None) : base(identifier, loosenToolFlag, gameIcon, replaceType, repairType) {
            Name = name;
        }

        protected override void onApplyToVehicleStep(ChoiceVVehicle vehicle, float damage) { }

        public override bool onUpdate(ChoiceVVehicle vehicle, VehicleMotorCompartmentUpdateType type, float damage) {
            var alreadyDamage = vehicle.getCompartmentPartDamage(Identifier);
            if(alreadyDamage >= 0.65) {
                if(type == VehicleMotorCompartmentUpdateType.KilometerTick) {
                    VehicleMotorCompartmentController.applyMotorDamage(vehicle, damage * alreadyDamage);
                } else {
                    VehicleMotorCompartmentController.applyMotorDamage(vehicle, damage * alreadyDamage);
                }
            }

            if(type == VehicleMotorCompartmentUpdateType.Motordamage) {
                if(alreadyDamage >= 1) return false;
                vehicle.setCompartmentPartDamage(Identifier, alreadyDamage + damage * CAN_DAMAGE_ENGINE_PART_DAMAGE_GAIN_MULTIPLIER);
                return true;
            } else if(type == VehicleMotorCompartmentUpdateType.KilometerTick) {
                if(alreadyDamage >= 1) return false;
                vehicle.setCompartmentPartDamage(Identifier, alreadyDamage + CAN_DAMAGE_ENGINE_PART_KILOMETER_GAIN_MULTIPLIER * damage * VehicleMotorCompartment.DEVELOPMENT_SPEEDUP_MULTIPLIER);
                return true;
            }

            return false;
        }

        public override string getNameOrSerialNumber() {
            return Name;
        }
    }
}
