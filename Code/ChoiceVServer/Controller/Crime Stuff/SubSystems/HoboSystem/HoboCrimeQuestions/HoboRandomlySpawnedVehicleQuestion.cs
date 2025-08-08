using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Menu;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ChoiceVServer.Controller.Crime_Stuff;

public class HoboCrimeRandomVehicleQuestion : HoboCrimeQuestion {
    private int VehicleClassId;

    public HoboCrimeRandomVehicleQuestion () { }

    public HoboCrimeRandomVehicleQuestion(int id, CrimeNetworkPillar pillar, string name, List<string> labels, int requiredReputation, Dictionary<string, string> settings) : base(id, pillar, name, labels, requiredReputation) {
        VehicleClassId = int.Parse(settings["VehicleClassId"]);
    }

    public override Menu getQuestionMenu() {
        var menu = new Menu(Name, "Lasse dir ein Fahrzeug dieser Klasse anzeigen");

        menu.addMenuItem(new ClickMenuItem("Position anzeigen", $"Zeige die eine/einen {Name}, welcher aktuell aktiv ist, an", "", "ON_PLAYER_SHOW_RANDOM_VEHICLE_POSITION").withData(new Dictionary<string, dynamic> {
            { "VehicleClassId", VehicleClassId },
        }));

        return menu;
    }

    public override List<MenuItem> getSupportMenuInfo() {
        return [
            new StaticMenuItem("VehicleClassId", "", VehicleClassId.ToString()),
        ];
    }

    public override List<MenuItem> onSupportCreateMenuItems() {
        return [new InputMenuItem("Vehicleklassen ID", "Wähle die Vehicle Klassen ID aus", "", InputMenuItemTypes.number, "")];
    }

    public override Dictionary<string, string> onSupportCreateSettings(MenuStatsMenuItem.MenuStatsMenuItemEvent evt) {
        var classEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();

        return new Dictionary<string, string> {
            { "VehicleClassId", classEvt.input }
        };
    }
}