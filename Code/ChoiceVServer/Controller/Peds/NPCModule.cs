using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Bogus;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.Controller;
public abstract class NPCModule {
    public int? DbId;

    protected ChoiceVPed Ped;

    public NPCModule(ChoiceVPed ped) {
        Ped = ped;
    }

    //For dynamic db generation
    public NPCModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) {
        Ped = ped;
    }

    public abstract List<MenuItem> getMenuItems(IPlayer player);
    public abstract void onRemove();

    public virtual bool overridesOtherModulesOnInteraction(IPlayer player) { return false; }


    public virtual void onInteraction(IPlayer player) { }

    public virtual void onEntityEnterPedShape(IEntity entity) { }

    public virtual void onTick(TimeSpan TickTimer) { }
    public abstract StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player);

    public virtual Dictionary<string, dynamic> getSettings() {
        return [];
    }
}