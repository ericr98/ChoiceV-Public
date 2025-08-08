using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.DamageSystem.Model;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public class MedicItem : ToolItem {
        public MedicItem(item item) : base(item) { }

        //Constructor for generic generation
        public MedicItem(configitem configItem, int amount, int quality) : base(configItem, quality, amount) { }

        public UseableMenuItem getMedicalAnalyseMenuItem(string evt) {
            return new ClickMenuItem(Name + " benutzen", "Benutze " + Name + " um die ausgewählte Wunde zu behandlen", "", evt).needsConfirmation($"{Name} benutzen?", $"{Name} wirklich benutzen?");
        }

        public bool treatInjury(Injury injury) {
            var worked = false;
            if(injury.DiagnosedInjury == null) {
                injury.generateInjury();
            }

            var map = injury.DiagnosedInjury.treatmentCategoryNavigation.configinjurytreatmentssteps.FirstOrDefault(d => d.itemId == ConfigId);
            if(map != null) {
                if(injury.OperatedOrder != null) {
                    injury.OperatedOrder.Add(map.itemId);
                    worked = injury.checkForRightTreatment();
                }
            }

            injury.IsTreated = true;

            InvokeController.AddTimedInvoke($"{Id}-isTreatedRemover", (i) => {
                if(injury != null) {
                    injury.IsTreated = false;
                }
            }, Constants.DAMAGE_TREATED_SHOW_TIME, false);

            return worked;
        }

        public override void updateDescription() {
            if(MaxWear != 1) {
                base.updateDescription();
            }
        }
    }
}
