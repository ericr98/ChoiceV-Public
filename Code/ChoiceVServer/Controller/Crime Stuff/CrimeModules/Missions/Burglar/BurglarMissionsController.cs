using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Crime_Stuff;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.Crime_Stuff {
     public class BurglarMissionsController : ChoiceVScript {
        public BurglarMissionsController() {
            CrimeMissionsController.addJobTaskCreator("Parkuhr knacken", onCreateParkingTrigger);
            CrimeMissionsController.addJobTaskCreator("Waren stehlen", onCreateStoreTheftTrigger);
        }

        private List<MenuItem> onCreateParkingTrigger(IPlayer player, ref CrimeAction action, ref string codeItem) {
            action = CrimeAction.ParkingClockBreaking;
            codeItem = nameof(ParkingClockMissionTrigger);

            return [];
        }

        private List<MenuItem> onCreateStoreTheftTrigger(IPlayer player, ref CrimeAction action, ref string codeItem) {
            action = CrimeAction.StoreTheft;
            codeItem = nameof(StoreTheftMissionTrigger);

            var list = new List<MenuItem> {
                new InputMenuItem("Akzeptierte Items", "Gib die Ids der Items ein, welche beim Diebstahl akzeptiert werden. Leerlassen um keine Items zu spezifizieren. Die Items müssen in einer Liste getrennt durch Kommas angegeben werden. z.B. [78,65,123,1], ohne die []", "z.B 56,78,1", ""),
            };

            return list;
        }
    }
}