using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.InventorySystem {
    public enum SpecialToolFlag : byte {
        None = 0,
        Crowbar = 1,
        Pickaxe = 2,
        Screwdriver = 3,
        CarJack = 4,
        CarDentPuller = 5,
        AutomotivePliers = 6, //Zange (Auto)
        Ratchet = 7, //Knarre/Ratsche (Auto)
        ImpactWrench = 8, //Schlagschrauber (Auto)
        VehicleKeyKit = 9,
        Knife = 10,
    }

    public delegate void ItemWearChangeDelegate(ToolItem item, int newWear);

    public class ToolItem : Item {
        public static ItemWearChangeDelegate ItemWearChangeDelegate;

        public SpecialToolFlag Flag { get; private set; }
        public int MaxWear { get; private set; }
        public int Wear { get; private set; }

        public ToolItem(item item) : base(item) {
            MaxWear = item.config.wear;
            Wear = item.wear ?? 1;
        }

        //Constructor for generic generation
        public ToolItem(configitem configItem, int amount, int quality) : base(configItem) {
            MaxWear = configItem.wear;
            Wear = configItem.wear;
        }

        public virtual bool showMessage() { return false; }

        public override void use(IPlayer player) {
            base.use(player);

            Wear--;
            if(Wear <= 0) {
                destroy();
                if(MaxWear > 1) {    
                    player?.sendNotification(Constants.NotifactionTypes.Info, "Das Werkzeug ist nach der Benutzung aufgebraucht. Es wurde jedoch erfolgreich verwendet.", "Werkzeug kaputt gegangen", Constants.NotifactionImages.System);
                }
            } else {
                updateToolDescription();
                ItemWearChangeDelegate.Invoke(this, Wear);
            }
        }

        public override void externalUse() {
            base.externalUse();

            Wear--;
            if(Wear <= 0) {
                destroy();
            } else {
                updateToolDescription();
                ItemWearChangeDelegate.Invoke(this, Wear);
            }
        }

        private void updateToolDescription() {
            var per = (float)Wear / (float)MaxWear;
            if(per > 0.75) {
                Description = "So gut wie unbenutzt";
            } else if(per > 0.5) {
                Description = "Noch einige Benutzungen über";
            } else if(per > 0.25) {
                Description = "Noch ein paar Benutzungen über";
            } else {
                Description = "Nicht mehr viele Benutzungen über!";
            }
        }

        public override void updateDescription() {
            updateToolDescription();

            base.updateDescription();
        }

        public override object Clone() {
            return this.MemberwiseClone();
        }

        public override void processAdditionalInfo(string info) {
            base.processAdditionalInfo(info);

            if(info == null || info == "") {
                Flag = SpecialToolFlag.None;
            } else {
                Flag = (SpecialToolFlag)byte.Parse(info);
            }
        }
    }
}
