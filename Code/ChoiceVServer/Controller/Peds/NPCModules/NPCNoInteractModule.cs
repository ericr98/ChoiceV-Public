using System.Collections.Generic;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.Controller;
public class NPCNoInteractModule : NPCModule {
    private string OriginalCollsionShapeStr;

    public NPCNoInteractModule(ChoiceVPed ped) : base(ped) { }
    public NPCNoInteractModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, settings) {
        Ped = ped;

        if (Ped.CollisionShape != null) {
            OriginalCollsionShapeStr = Ped.CollisionShape.toShortSave();
            Ped.CollisionShape.Dispose();
            Ped.CollisionShape = null;
        }
    }

    public override List<MenuItem> getMenuItems(IPlayer player) {
        return null;
    }

    public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
        return new StaticMenuItem("Interaktions Block Modul", "Entfernt jegliche Interaktionsmöglichkeit mit dem Ped", "");
    }

    public override void onRemove() {
        Ped.CollisionShape = CollisionShape.Create(OriginalCollsionShapeStr);
        Ped.CollisionShape.OnCollisionShapeInteraction += Ped.onInteract;
        Ped.CollisionShape.OnEntityEnterShape += Ped.onEnterShape;
    }
}