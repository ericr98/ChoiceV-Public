using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;

namespace ChoiceVServer.Controller.Vehicles {
    public class ElectricalVehicleMotorCompartmentPart : NonStaticVehicleMotorCompartmentPart {
        private static float ELECTRICAL_PART_DAMAGE_GAIN_MULTIPLIER = 1 / 150f;
        private static float ELECTRICAL_PART_KILOMETER_DAMAGE_GAIN = 0.00072f;

        private string Name;

        public ElectricalVehicleMotorCompartmentPart(string identifier, SpecialToolFlag loosenToolFlag, string name, string gameIcon, VehicleMotorCompartmentRepairType replaceType, VehicleMotorCompartmentRepairType repairType = VehicleMotorCompartmentRepairType.None) : base(identifier, loosenToolFlag, gameIcon, replaceType, repairType) {
            Name = name;
        }

        protected override void onApplyToVehicleStep(ChoiceVVehicle vehicle, float damage) {
            if(damage >= 1) {
                vehicle.BatteryDamageLevel = PartDamageLevel.Broken;
                vehicle.addVehicleMechLightStatus(Identifier, ChoiceVVehicle.MechLightColors.Red);
            } else if(damage >= 0.65) {
                vehicle.BatteryDamageLevel = PartDamageLevel.Stutter;
                vehicle.addVehicleMechLightStatus(Identifier, ChoiceVVehicle.MechLightColors.Yellow);
            } else {
                vehicle.BatteryDamageLevel = PartDamageLevel.None;
                vehicle.removeVehicleMechLightStatus(Identifier);
            }
        }

        public override bool onUpdate(ChoiceVVehicle vehicle, VehicleMotorCompartmentUpdateType type, float damage) {
            if(type == VehicleMotorCompartmentUpdateType.Motordamage) {
                var alreadyDamage = vehicle.getCompartmentPartDamage(Identifier);
                if(alreadyDamage >= 1) return false;
                vehicle.setCompartmentPartDamage(Identifier, alreadyDamage + damage * ELECTRICAL_PART_DAMAGE_GAIN_MULTIPLIER);
                return true;
            } else if(type == VehicleMotorCompartmentUpdateType.KilometerTick) {
                var alreadyDamage = vehicle.getCompartmentPartDamage(Identifier);
                if(alreadyDamage >= 1) return false;
                vehicle.setCompartmentPartDamage(Identifier, alreadyDamage + ELECTRICAL_PART_KILOMETER_DAMAGE_GAIN * damage);
                return true;
            }

            return false;
        }

        public override string getNameOrSerialNumber() {
            return Name;
        }
    }
}
