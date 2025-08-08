using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;

namespace ChoiceVServer.Controller.Vehicles {
    public enum VehicleMotorCompartmentUpdateType {
        Motordamage,
        KilometerTick, //Every Kilometer
        Hulldamage,
    }

    public abstract class VehicleMotorCompartmentPart {
        public string Identifier { get; private set; }
        public SpecialToolFlag LoosenToolFlag;

        public string GameImage;

        public VehicleMotorCompartmentRepairType ReplaceType;
        public VehicleMotorCompartmentRepairType RepairType = VehicleMotorCompartmentRepairType.None;

        public VehicleMotorCompartmentPart(string identifier, SpecialToolFlag loosenToolFlag, string gameImage, VehicleMotorCompartmentRepairType replaceType, VehicleMotorCompartmentRepairType repairType = VehicleMotorCompartmentRepairType.None) {
            Identifier = identifier;
            LoosenToolFlag = loosenToolFlag;
            GameImage = gameImage;

            ReplaceType = replaceType;
            RepairType = repairType;
        }

        /// <returns>return true if something changed</returns>
        public abstract bool onUpdate(ChoiceVVehicle vehicle, VehicleMotorCompartmentUpdateType type, float damage);

        public virtual void setDamageLevel(ChoiceVVehicle vehicle, float damage) { }

        public abstract void onApplyToVehicle(ChoiceVVehicle vehicle);

        public abstract string getNameOrSerialNumber();

        public virtual void onRepair(ChoiceVVehicle vehicle) { }

        public virtual bool isDamageAble() { return false; }

        public virtual float getCurrentDamage(ChoiceVVehicle vehicle) { return 0; }

        public MechanicalGameComponent getComponent(ChoiceVVehicle vehicle, VehicleMotorCompartmentMapping mapping) {
            var component = new VehicleMechanicalGameComponent(mapping.Id, mapping, false, false);
            component.setVehicle(vehicle);
            return component;
        }

        public void setDamageToVehicle(ChoiceVVehicle vehicle, float damage) {
            vehicle.setCompartmentPartDamage(Identifier, damage);
        }

        public float getDamageFromVehicle(ChoiceVVehicle vehicle) {
            return vehicle.getCompartmentPartDamage(Identifier);
        }
    }

    public abstract class StaticVehicleMotorCompartmentPart : VehicleMotorCompartmentPart {
        public StaticVehicleMotorCompartmentPart(string identifier, SpecialToolFlag loosenToolFlag, string gameImage) : base(identifier, loosenToolFlag, gameImage, VehicleMotorCompartmentRepairType.None, VehicleMotorCompartmentRepairType.None) { }

        public override void onApplyToVehicle(ChoiceVVehicle vehicle) { }

        public override bool onUpdate(ChoiceVVehicle vehicle, VehicleMotorCompartmentUpdateType type, float damage) { return false; }
    }

    public class StaticVehicleMotorCompartmentPartWithName : StaticVehicleMotorCompartmentPart {
        private string Name;

        public StaticVehicleMotorCompartmentPartWithName(string identifier, SpecialToolFlag loosenToolFlag, string name, string gameImage) : base(identifier, loosenToolFlag, gameImage) {
            Name = name;
        }

        public override string getNameOrSerialNumber() {
            return Name;
        }
    }

    public class StaticVehicleMotorCompartmentPartWithSerialNumber : StaticVehicleMotorCompartmentPart {
        private string SerialNumberGroup;
        public StaticVehicleMotorCompartmentPartWithSerialNumber(string identifier, string serialNumberGroup, SpecialToolFlag loosenToolFlag, string gameImage) : base(identifier, loosenToolFlag, gameImage) {
            SerialNumberGroup = serialNumberGroup;
        }

        public override string getNameOrSerialNumber() {
            return VehicleMotorCompartmentController.getSerialNumberForIdentifier(SerialNumberGroup);
        }
    }

    public abstract class NonStaticVehicleMotorCompartmentPart : VehicleMotorCompartmentPart {
        public NonStaticVehicleMotorCompartmentPart(string identifier, SpecialToolFlag loosenToolFlag, string gameImage, VehicleMotorCompartmentRepairType replaceType, VehicleMotorCompartmentRepairType repairType = VehicleMotorCompartmentRepairType.None) : base(identifier, loosenToolFlag, gameImage, replaceType, repairType) {
            LoosenToolFlag = loosenToolFlag;
        }

        public override void onApplyToVehicle(ChoiceVVehicle vehicle) {
            onApplyToVehicleStep(vehicle, vehicle.getCompartmentPartDamage(Identifier));
        }

        public override void setDamageLevel(ChoiceVVehicle vehicle, float damage) {
            vehicle.setCompartmentPartDamage(Identifier, damage);
        }

        protected abstract void onApplyToVehicleStep(ChoiceVVehicle vehicle, float damage);

        public override void onRepair(ChoiceVVehicle vehicle) {
            vehicle.setCompartmentPartDamage(Identifier, 0);
            onRepairStep(vehicle);
            onApplyToVehicle(vehicle);
        }

        public virtual void onRepairStep(ChoiceVVehicle vehicle) { }

        public override bool isDamageAble() {
            return true;
        }

        public override float getCurrentDamage(ChoiceVVehicle vehicle) {
            return vehicle.getCompartmentPartDamage(Identifier);
        }
    }
}
