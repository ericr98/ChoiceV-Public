using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Crime_Stuff;
using ChoiceVServer.Model.Menu;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.Crime_Stuff {
     public class DealerMissionsController : ChoiceVScript {
        public DealerMissionsController() {
            CrimeMissionsController.addJobTaskCreator("Personen ausrauben", onRobNpcTrigger);
        }

        private List<MenuItem> onRobNpcTrigger(IPlayer player, ref CrimeAction action, ref string codeItem) {
            action = CrimeAction.RobNPC;
            codeItem = nameof(NPCRobbingMissionTrigger);

            return [
                WorldController.getBigRegionSelectMenuItem("Zone", "", true)
            ];
        }
    }
}