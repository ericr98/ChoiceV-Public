using System.Collections.Generic;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.Controller;
public class NPCRelocateCollisionModule : NPCModule {

    private string OriginalCollsionShapeStr;

    public NPCRelocateCollisionModule(ChoiceVPed ped) : base(ped) { }
    public NPCRelocateCollisionModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, settings) {
        Ped = ped;

        if (Ped.CollisionShape != null) {
            OriginalCollsionShapeStr = Ped.CollisionShape.toShortSave();
            Ped.CollisionShape.Dispose();

            Ped.CollisionShape = CollisionShape.Create((string)settings["ColshapeStr"]);
            Ped.CollisionShape.OnCollisionShapeInteraction += Ped.onInteract;

            Ped.CollisionShape.OnEntityEnterShape += Ped.onEnterShape;
        }
    }

    public override List<MenuItem> getMenuItems(IPlayer player) {
        return new List<MenuItem>();
    }

    public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
        return new StaticMenuItem("CollisionShaoe", "Das Ped kann nicht bedroht werden", "");
    }

    public override void onRemove() {
        if (Ped.CollisionShape != null) {
            Ped.CollisionShape.Dispose();
        }

        Ped.CollisionShape = CollisionShape.Create(OriginalCollsionShapeStr);
        Ped.CollisionShape.OnCollisionShapeInteraction += Ped.onInteract;
        Ped.CollisionShape.OnEntityEnterShape += Ped.onEnterShape;
    }
}