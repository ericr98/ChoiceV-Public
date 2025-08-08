using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.DamageSystem.Model {
    public class DamageDummy {
        public delegate void DummyUpdateDelegate(DamageDummy dummy);

        public DummyUpdateDelegate OnDummyUpdateDelegate;

        public int CountingInjuryId = 1;
        public int Id { get; private set; }
        public List<Injury> InjuryList { get; private set; }

        public DamageDummy(int id) {
            Id = id;
            InjuryList = new List<Injury>();
        }

        public Menu getInteractionMenu(IPlayer player) {
            DamageTreatmentController.showMedicScreen(player, Id, InjuryList, (injuryId) => onSelectInjury(player, injuryId));
            var menu = new Menu("Verletzungs-Dummy", "Siehe dir die Konfiguration an");

            var createMenu = new Menu("Verletzung hinzufügen", "Welche Verletzung möchtest du hinzufügen?");
            using(var db = new ChoiceVDb()) {
                foreach(var injury in db.configinjuries.Include(i => i.treatmentCategoryNavigation).ThenInclude(t => t.configinjurytreatmentssteps)) {
                    createMenu.addMenuItem(new ClickMenuItem(injury.name, $"Füge eine {injury.name} hinzu.", "", "DUMMY_ADD_INJURY")
                        .withData(new Dictionary<string, dynamic> { { "Injury", injury }, { "Dummy", this } })
                        .needsConfirmation($"{injury.name} hinzufügen?", "Verletzung wirklich hinzufügen?"));
                }
            }
            menu.addMenuItem(new MenuMenuItem(createMenu.Name, createMenu, MenuItemStyle.green));

            foreach(var injury in InjuryList) {
                var name = injury.DiagnosedInjury != null ? injury.DiagnosedInjury.name : "Unbekannte Verletzung";
                var treated = injury.IsHealing ? " (Behandelt)" : "(N/A)";
                var style = injury.IsHealing ? MenuItemStyle.green : MenuItemStyle.normal;

                menu.addMenuItem(new StaticMenuItem(name, $"Es handelt sich um eine {name}.", treated, style));
            }

            menu.addMenuItem(new ClickMenuItem("Verletzungen löschen", "Lösche alle Verletzungen", "", "DUMMY_DELETE_INJURIES", MenuItemStyle.red)
                .withData(new Dictionary<string, dynamic> { { "Dummy", this } })
                .needsConfirmation("Verletzungen löschen?", "Verletzungen wirklich löschen?"));

            return menu;
        }

        public void onSelectInjury(IPlayer player, int injuryId) {
            var injury = InjuryList.FirstOrDefault(i => i.Id == injuryId);
            var name = injury.DiagnosedInjury != null ? injury.DiagnosedInjury.name : "Unbekannte Verletzung";

            var menu = new Menu(name, "Welches Werkzeug möchtest du benutzen?");
            var medItems = player.getInventory().getItems<MedicItem>(i => true);

            foreach(var item in medItems) {
                var menuItem = item.getMedicalAnalyseMenuItem("DUMMY_HEAL_INJURY").withData(new Dictionary<string, dynamic> { { "Item", item }, { "Dummy", this }, { "Injury", injury } });
                menu.addMenuItem(menuItem);
            }

            player.showMenu(menu);
        }

        public void addInjury(Injury injury) {
            InjuryList.Add(injury);
        }

        public void addInjuries(List<Injury> injuries) {
            InjuryList.AddRange(injuries);
        }

        public string getShortSave() {
            var sb = new StringBuilder();

            foreach(var injury in InjuryList) {
                sb.Append(injury.DiagnosedInjury.id);
                sb.Append(';');
                sb.Append(injury.Seed);
                sb.Append(';');
                sb.Append(injury.BodyPart.ToString());
                sb.Append(';');
                sb.Append(injury.OperatedOrder.ToJson());
                sb.Append('#');
            }

            return sb.ToString();
        }
    }
}
