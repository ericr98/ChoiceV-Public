using System.Collections.Generic;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Menu;


namespace ChoiceVServer.Controller;
public class NPCCannotBeThreatendModule : NPCModule {
    public NPCCannotBeThreatendModule(ChoiceVPed ped) : base(ped) { }
    public NPCCannotBeThreatendModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, settings) {
        Ped = ped;
    }

    public override List<MenuItem> getMenuItems(IPlayer player) {
        return [];
    }

    public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
        return new StaticMenuItem("Nicht bedrohbar Modul", "Das Ped kann nicht bedroht werden", "");
    }

    public override void onRemove() { }
}