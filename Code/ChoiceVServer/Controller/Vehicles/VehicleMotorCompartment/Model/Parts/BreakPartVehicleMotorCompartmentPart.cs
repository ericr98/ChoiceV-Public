using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;

namespace ChoiceVServer.Controller.Vehicles {
    public class BreakPartVehicleMotorCompartmentPart : NonStaticVehicleMotorCompartmentPart {
        private string Name;
        private static float BREAK_PART_DAMAGE_GAIN_MULTIPLIER = 1 / 100f;

        public BreakPartVehicleMotorCompartmentPart(string identifier, SpecialToolFlag loosenToolFlag, string name, string gameImage) : base(identifier, loosenToolFlag, gameImage, VehicleMotorCompartmentRepairType.Mechanics, VehicleMotorCompartmentRepairType.None) {
            Name = name;
        }

        protected override void onApplyToVehicleStep(ChoiceVVehicle vehicle, float damage) {
            if(damage > 0.65) {
                if(damage >= 0.9) {
                    vehicle.addVehicleMechLightStatus(Identifier, ChoiceVVehicle.MechLightColors.Red);
                } else {
                    vehicle.addVehicleMechLightStatus(Identifier, ChoiceVVehicle.MechLightColors.Yellow);
                }

                vehicle.modifyHandlingMeta("brakeForce", damage.Map(0.65f, 1, 1, 0));
            } else {
                vehicle.removeVehicleMechLightStatus(Identifier);
                vehicle.modifyHandlingMeta("brakeForce", 1);
            }
        }

        public override bool onUpdate(ChoiceVVehicle vehicle, VehicleMotorCompartmentUpdateType type, float damage) {
            if(type == VehicleMotorCompartmentUpdateType.Motordamage) {
                var alreadyDamage = vehicle.getCompartmentPartDamage(Identifier);
                if(alreadyDamage >= 1) return false;
                vehicle.setCompartmentPartDamage(Identifier, alreadyDamage + damage * BREAK_PART_DAMAGE_GAIN_MULTIPLIER);
                return true;
            }

            return false;
        }

        public override string getNameOrSerialNumber() {
            return Name;
        }
    }
}
