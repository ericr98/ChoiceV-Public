using System.Collections.Generic;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Controller;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.Controller;
public class NPCMenuItemsModule : NPCModule {
    private List<PlayerMenuItemGenerator> Generators;

    public NPCMenuItemsModule(ChoiceVPed ped, List<PlayerMenuItemGenerator> generators) : base(ped) {
        Generators = generators;
    }

    public override List<MenuItem> getMenuItems(IPlayer player) {
        var list = new List<MenuItem>();
        Generators.ForEach(g => list.Add(g.Invoke(player)));

        return list;
    }

    public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
        return new StaticMenuItem("Menü Element Modul", "Fügt ein oder mehrere Menü Elemente der Ped Interaktion hinzu", "");
    }

    public override void onRemove() {
        Generators.Clear();
    }
}