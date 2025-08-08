using System;
using System.Collections.Generic;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Controller.Minijobs.Model;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.Controller.Minijobs.Modules;

public class NPCMinijobModule : NPCModule {
    private readonly Minijob Minijob;

    public NPCMinijobModule(ChoiceVPed ped, Minijob minijob) : base(ped) {
        Minijob = minijob;
    }
    
    public override List<MenuItem> getMenuItems(IPlayer player) {
        if (Minijob.Ongoing && Minijob.isPlayerDoingMinijob(player)) {
            return null;
        } else {
            return [];
        }
    }

    public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
        return new StaticMenuItem("Minijob Modul", $"Das Ped gehört zum Minijob {Minijob.Name}.", "");
    }

    public override void onRemove() { }
}
