using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.Controller.Crime_Stuff;

public class PlayerCrimeReputation {
    private List<float> Reputation;
    private List<float> Favors;

    public PlayerCrimeReputation(List<float> reputation, List<float> favors) {
        Reputation = reputation;
        Favors = favors;
    }

    /// <summary>
    /// Returns the reputation of the player for the given pillar
    /// </summary>
    /// <param name="pillar">If null, the max reputation will be returned</param>
    /// <returns></returns>
    public float getPillarReputation(CrimeNetworkPillar pillar) {
        return pillar == null ? Reputation.Max() : Reputation[pillar.Id];
    }

    public bool isReputationHighEnough(CrimeNetworkPillar pillar, string identifier) {
        if (pillar == null) return true;

        return getPillarReputation(pillar) >= pillar.getNeededReputation(identifier);
    }

    public float getPillarFavors(CrimeNetworkPillar pillar) {
        return Favors[pillar.Id];
    }

    public void giveReputation(CrimeNetworkPillar pillar, float amount) {
        Reputation[pillar.Id] += amount;
    }

    public bool removeReputation(CrimeNetworkPillar pillar, float amount) {
        if (pillar == null) {
            for(int i = 0; i < Reputation.Count; i++) {
                if (Reputation[i] > amount) {
                    Reputation[i] -= amount;
                    return true;
                }
            }
        } else {
            if (Reputation[pillar.Id] > amount) {
                Reputation[pillar.Id] -= amount;
                return true;
            }
        }

        return false;
    }

    public void giveFavors(CrimeNetworkPillar pillar, float amount) {
        Favors[pillar.Id] += amount;
    }

    public bool removeFavors(CrimeNetworkPillar pillar, float amount) {
        if (Favors[pillar.Id] > amount) {
            Favors[pillar.Id] -= amount;
            return true;
        }

        return false;
    }

    public void updateDb(IPlayer player, CrimeNetworkPillar pillar) {
        updateDb(player.getCharacterId(), pillar);
    }

    public void updateDb(int charId, CrimeNetworkPillar pillar) {
            using(var db = new ChoiceVDb()) {
                var dbRep = db.charactercrimereputations.Find(charId, pillar.Id);
                if(dbRep != null) {
                    dbRep.reputation = Reputation[pillar.Id];
                    dbRep.favors = Favors[pillar.Id];
                } else {
                    var newRp = new charactercrimereputation {
                        charId = charId,
                        pillarId = pillar.Id,
                        reputation = Reputation[pillar.Id],
                        favors = Favors[pillar.Id],
                    };
                    db.charactercrimereputations.Add(newRp);
                }

                db.SaveChanges();
            }
        }
    }
