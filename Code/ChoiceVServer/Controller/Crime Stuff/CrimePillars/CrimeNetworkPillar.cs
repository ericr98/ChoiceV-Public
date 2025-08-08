using AltV.Net.Elements.Entities;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.Crime_Stuff {
    public abstract class CrimeNetworkPillar {
        public int Id;
        public string Name;

        public CrimeNetworkPillar(int id, string name) {
            Id = id;
            Name = name;
        }

        public void onInit() {

            //Todo get all Peds with Module that has this crime Pillar
            //AllConnectedPeds = PedController.AllPeds.Where(p => p)
        }

        public abstract void onCrimeAction(IPlayer player, CrimeAction action, float amount, Dictionary<string, dynamic> data);

        public abstract void onTick();

        public abstract float getNeededReputation(string identifier);

        public abstract float getCrimeNetworkAllowedActionReputation(string identifier);

        public abstract List<CrimeAction> allConnectedCrimeActions();
    }
}
