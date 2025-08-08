using System.Collections.Generic;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Controller;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.Controller;
public class NPCWillNotCallPoliceModule : NPCModule {
    public NPCWillNotCallPoliceModule(ChoiceVPed ped) : base(ped) { }
    public NPCWillNotCallPoliceModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, settings) { }

    public override List<MenuItem> getMenuItems(IPlayer player) {
        return new List<MenuItem>();
    }

    public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
        return new StaticMenuItem("Ruft keine Polizei-Modul", "Ein Ped mit diesem Modul ruft keine Polizei, wenn es bedroht wird", "");
    }

    public override void onRemove() { }
}