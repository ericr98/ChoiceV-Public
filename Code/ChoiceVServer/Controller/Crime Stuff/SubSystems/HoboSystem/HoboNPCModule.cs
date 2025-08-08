using System.Collections.Generic;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.Controller.Crime_Stuff;

public class HoboNPCModule : NPCModule {
    public HoboNPCModule(ChoiceVPed ped) : base(ped) { }

    public HoboNPCModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, settings) { }

    public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
        return new StaticMenuItem("Bettlermodule", "Ermöglicht das Fragen von Bettlern", "");
    }

    public override List<MenuItem> getMenuItems(IPlayer player) {
        if(player.hasCrimeFlag()) {
            var list = new List<MenuItem> {
                new InputMenuItem("Frage stellen", "Stelle eine Frage an den Bettler. Gib ein Stichwort ein. Der Bettler wird dir dann eine Antwort geben, wenn er zu dem Thema etwas sagen kann.", "", "ON_PLAYER_ASK_HOBO")
                .needsConfirmation($"Für ${HoboController.HOBO_QUESTION_COST} Bettler fragen?", "Wirklich den Bettler fragen?")
            };

            return list;
        } else {
            return [
                new StaticMenuItem("Bettler schaut dich schräg an", "Der Bettler schaut dich an und sagt nichts.", "")
            ];
        }
    }

    public override void onRemove() { }
}