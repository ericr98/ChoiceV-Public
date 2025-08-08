using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.Phone;
using ChoiceVServer.Model.Database;
using Newtonsoft.Json;

namespace ChoiceVServer.InventorySystem {
    public class SIMCardSettings {
        public int volume;
        public bool hiddenNumber;
        public bool silent;
        public bool flyMode;

        public bool socialMediaHorizontalStart;

        public SIMCardSettings() {
            volume = 5;
            hiddenNumber = false;
            silent = false;
            flyMode = false;
            socialMediaHorizontalStart = false;
        }
    }

    public class SIMCard : Item {
        public long Number { get => (long)Data["Number"]; set => Data["Number"] = value; }
        public decimal Balance { get => (decimal)Data["Balance"]; set => Data["Balance"] = value; }

        public SIMCardSettings Settings { get => JsonConvert.DeserializeObject<SIMCardSettings>(Data["SIMCardSettings"]); set => Data["SIMCardSettings"] = value.ToJson(); }

        public SIMCard(item item) : base(item) {

        }

        public SIMCard(configitem configItem, long number) : base(configItem) {
            Number = PhoneController.createNewPhoneNumber(number, $"SIM-Card: {Id}").number;

            Settings = new SIMCardSettings();
        }

        //Constructor for generic generation
        public SIMCard(configitem configItem, int amount = -1, int quality = -1) : base(configItem) {
            Number = PhoneController.createNewPhoneNumber($"SIM-Card: {Id}").number;

            updateDescription();


            Settings = new SIMCardSettings();
        }

        public override void updateDescription() {
            Description = $"Nummer: {PhoneController.formatPhoneNumber(Number)}";
            base.updateDescription();
        }
    }
}
