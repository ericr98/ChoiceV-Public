using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;

namespace ChoiceVServer.Controller.Vehicles {
    public class LightMotorCompartmentPart : NonStaticVehicleMotorCompartmentPart {
        private static float LIGHT_PART_MULTIPLIER = 1 / 90f;

        public LightMotorCompartmentPart(string identifier, SpecialToolFlag loosenToolFlag) : base(identifier, loosenToolFlag, "light1x1", VehicleMotorCompartmentRepairType.Lights) { }

        protected override void onApplyToVehicleStep(ChoiceVVehicle vehicle, float damage) {
            if(damage >= 1) {
                vehicle.LightDamageLevel = PartDamageLevel.Broken;
                //vehicle.addVehicleMechLightStatus(Identifier, ChoiceVVehicle.MechLightColors.Red);
            } else if(damage >= 0.65) {
                vehicle.LightDamageLevel = PartDamageLevel.Stutter;
                //vehicle.addVehicleMechLightStatus(Identifier, ChoiceVVehicle.MechLightColors.Yellow);
            } else {
                vehicle.LightDamageLevel = PartDamageLevel.None;
                // vehicle.removeVehicleMechLightStatus(Identifier);
            }

        }

        public override bool onUpdate(ChoiceVVehicle vehicle, VehicleMotorCompartmentUpdateType type, float damage) {
            if(type == VehicleMotorCompartmentUpdateType.Hulldamage || type == VehicleMotorCompartmentUpdateType.Motordamage) {
                var alreadyDamage = vehicle.getCompartmentPartDamage(Identifier);
                if(alreadyDamage >= 1) return false;
                vehicle.setCompartmentPartDamage(Identifier, alreadyDamage + damage * LIGHT_PART_MULTIPLIER);
                return true;
            }

            return false;
        }

        public override string getNameOrSerialNumber() {
            return "Scheinwerfer";
        }
    }
}
