using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.Controller.Crime_Stuff {
    public class CrimeMissionTokens {
        public float SelectToken;
        public float RandomToken;
        public float CrimeSpreeToken;

        public CrimeMissionTokens(float selectToken, float randomToken, float crimeSpreeToken) {
            SelectToken = selectToken;
            RandomToken = randomToken;
            CrimeSpreeToken = crimeSpreeToken;
        }

        public void updateDb(IPlayer player) {
            using(var db = new ChoiceVDb()) {
                var charToken = db.charactercrimemissiontokens.Find(player.getCharacterId());

                if(charToken != null) {
                    charToken.selectToken = SelectToken;
                    charToken.randomToken = RandomToken;
                    charToken.crimeSpreeToken = CrimeSpreeToken;
                } else {
                    var newToken = new charactercrimemissiontoken {
                        charId = player.getCharacterId(),
                        selectToken = SelectToken,
                        randomToken = RandomToken,
                        crimeSpreeToken = CrimeSpreeToken,
                    };

                    db.charactercrimemissiontokens.Add(newToken);
                }

                db.SaveChanges();
            }
        }
    }
}