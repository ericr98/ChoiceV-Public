using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Crime_Stuff;
using ChoiceVServer.Model.Menu;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.Crime_Stuff {
     public class FenceMissionsController : ChoiceVScript {
        public FenceMissionsController() {
            CrimeMissionsController.addJobTaskCreator("Fahrzeugteile stehlen", onVehiclePartStealTrigger);
        }

        private List<MenuItem> onVehiclePartStealTrigger(IPlayer player, ref CrimeAction action, ref string codeItem) {
            action = CrimeAction.StealVehiclePart;
            codeItem = nameof(StealVehiclePartMissionTrigger);

            return [
                new InputMenuItem("Fahrzeugklassen-Ids", "Die Fahrzeugklassen-Ids, die gestohlen werden sollen, getrennt mit einem Komma", "", "")
            ];
        }
    }
}