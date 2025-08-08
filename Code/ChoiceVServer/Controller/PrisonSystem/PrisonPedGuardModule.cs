using AltV.Net.Elements.Entities;
using ChoiceVServer.Controller.PrisonSystem.Model;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.PrisonSystem {
    public class NPCPrisonGuardModule : NPCModule {
        private Prison Prison;

        public NPCPrisonGuardModule(ChoiceVPed ped) : base(ped) { }
        
        public NPCPrisonGuardModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, settings) {
            Prison = PrisonController.getPrisonById((int)settings["PrisonId"]);
        }

        public override List<MenuItem> getMenuItems(IPlayer player) {
            if(Prison != null && Prison.isPlayerInPrison(player)) {
                var inmate = Prison.getInmateForPlayer(player);
                var menu = new Menu("Insasseninformation", "Siehe die Daten");
                
                if(inmate.FreeToGo) {
                    menu.addMenuItem(new StaticMenuItem("Du bist frei!", "Du bist frei! Du kannst das Gefängnis nun verlassen. Melde dich bei einem Wärter oder verlasse eigenständig das Gefängnis. Um das Gefängnis eigenständig zu verlassen gib dem Wächter (dieser Person hier) Bescheid und nähere dich den Türen zum Ausgang. Du wirst nach ein paar Sekunden \"durchgelassen\". Vergiss beim Ausgang deine Sachen nicht!", "Siehe Beschreibung"));
                    if(inmate.IsReleased) {
                        menu.addMenuItem(new StaticMenuItem("Du bist bereits frei", "Du bist bereits freigelassen worden. Du kannst das Gefängnis verlassen. Was machst du noch hier?", ""));
                    } else {
                        menu.addMenuItem(new ClickMenuItem("Freilassung beginnen", "Beginne deine eigene Freilassung", "", "PRISON_START_RELEASE", MenuItemStyle.green)
                            .withData(new Dictionary<string, dynamic> { { "Inmate", inmate } }));
                    }
                } else {
                    var imprisonUnits = PrisonController.getImprisonUnitsFromTimeSpan(inmate.TimeLeftOnline, inmate.TimeLeftOffline);
                    menu.addMenuItem(new StaticMenuItem("Haftzeit verbleibend: ", $"{imprisonUnits} Hafteinheiten verbleibend.", $"{imprisonUnits} HE"));
                }


                return [new MenuMenuItem(menu.Name, menu)];
            } else {
                return [];
            }
        }

        public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
            return new StaticMenuItem("Gefängniswärter", $"Zeigt Insassesn wichtige Informationen an für das Gefängnis: {Prison.Name}", Prison.Name);
        }

        public override void onRemove() {
            Prison = null;
        }
    }
}
