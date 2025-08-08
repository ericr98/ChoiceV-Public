using System.Collections.Generic;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.Controller;
public class NPCSimpleTextModule : NPCModule {
        public string Text;

        public NPCSimpleTextModule(ChoiceVPed ped) : base(ped) { }
        public NPCSimpleTextModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, settings) {
            Ped = ped;
            Text = settings["Text"];
        }

        public override List<MenuItem> getMenuItems(IPlayer player) {
            return new List<MenuItem> { new StaticMenuItem("Die Person sagt:", Text, "") };
        }

        public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
            return new StaticMenuItem("Statisches Text Modul", "Zeigt einen simplen Text an, wenn der NPC ausgewählt wird.", "");
        }

        public override void onRemove() { }
    }