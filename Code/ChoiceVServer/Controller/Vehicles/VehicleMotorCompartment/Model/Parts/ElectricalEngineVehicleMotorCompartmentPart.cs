using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using System.Linq;

namespace ChoiceVServer.Controller.Vehicles {
    public class ElectricalEngineVehicleMotorCompartmentPart : EngineVehicleMotorCompartmentPart {
        public ElectricalEngineVehicleMotorCompartmentPart(string identifier, SpecialToolFlag loosenToolFlag, string gameIcon, VehicleMotorCompartmentRepairType replaceType, VehicleMotorCompartmentRepairType repairType = VehicleMotorCompartmentRepairType.None) : base(identifier, loosenToolFlag, gameIcon, replaceType, replaceType) {

        }

        protected override void onApplyToVehicleStep(ChoiceVVehicle vehicle, float damage) {
            var compartment = VehicleMotorCompartmentController.getCompartmentForVehicle(vehicle);
            var otherMotors = compartment.getCompartmentParts(p => p is EngineVehicleMotorCompartmentPart && p != this);
            var anyWithHigherDamage = otherMotors.Any(m => m.getDamageFromVehicle(vehicle) > damage);

            if(!anyWithHigherDamage) {
                if(damage >= 1) {
                    vehicle.EngineDamageLevel = PartDamageLevel.Broken;
                    vehicle.addVehicleMechLightStatus(Identifier, ChoiceVVehicle.MechLightColors.Red);
                } else if(damage >= 0.65) {
                    vehicle.EngineDamageLevel = PartDamageLevel.Stutter;
                    vehicle.addVehicleMechLightStatus(Identifier, ChoiceVVehicle.MechLightColors.Yellow);
                } else {
                    vehicle.EngineDamageLevel = PartDamageLevel.None;
                    vehicle.removeVehicleMechLightStatus(Identifier);
                }
            }
        }

        public override string getNameOrSerialNumber() {
            return "Elektro-Motor";
        }
    }
}
