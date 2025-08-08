using System;
using System.Linq;
using System.Collections.Generic;
using ChoiceVServer.Controller.Minijobs.Model;
using ChoiceVServer.Model.Menu;
using AltV.Net.Elements.Entities;

namespace ChoiceVServer.Controller.Minijobs.Modules;

public class NPCOnlyVisibleInMinijobModule : NPCModule {
        private Minijob Minijob;

        public NPCOnlyVisibleInMinijobModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, settings) {
            Minijob = MiniJobController.Minijobs.FirstOrDefault(m => m.Id == settings["MinijobId"]);

            if(Minijob != null) {
                Minijob.AllOnlyOngoingVisible.Add(ped);

                if(!Minijob.Ongoing) {
                    Ped.toggleVisible(false);
                }
            } else {
                PedController.deletePedModule(ped, this);
            }
        }

        public void onMinijobStart() {
            Ped.toggleVisible(true);
        }

        public void onMinijobStop() {
            Ped.toggleVisible(false);
        }

        public override List<MenuItem> getMenuItems(IPlayer player) {
            return [];
        }

        public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
            return new StaticMenuItem("Minijob-Only-Module", $"Der NPC ist nur sichtbar, wenn der Minijob: {Minijob?.Name} gespielt wird", "");
        }

        public override void onRemove() { }
    }