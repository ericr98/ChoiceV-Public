using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Character;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.Crime_Stuff {
    public abstract class CrimeNetworkModule : NPCModule {
        public CrimeNetworkPillar Pillar;
        public string ReputationIdentifier;

        public string Name;

        public bool Active { get; private set; }

        public CrimeNetworkModule(ChoiceVPed ped, CrimeNetworkPillar pillar, string name, string reputationIdentifier) : base(ped) {
            Pillar = pillar;

            Name = name;
            ReputationIdentifier = reputationIdentifier;

            Active = true;
        }

        public override List<MenuItem> getMenuItems(IPlayer player) {
            if(player.getCharacterData().CharacterFlag != CharacterFlag.CrimeFlag) {
                return [];
            }

            if(!player.getCrimeReputation().isReputationHighEnough(Pillar, ReputationIdentifier)) {
                return [new StaticMenuItem("Reputation zu niedrig", "Die Person weigert sich mit dir zu reden, da deine kriminelle Reputation zu niedrig ist!", "", MenuItemStyle.yellow)];
            }

            return getCrimeMenuItems(player);
        }

        public void setActive(bool active) {
            Active = active;
        }

        public abstract List<MenuItem> getCrimeMenuItems(IPlayer player);
    }
}
