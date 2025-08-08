using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Crime_Stuff;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.Crime_Stuff {

    //Schritt 1: Kontakt mit den Netzwerken
    //0. (Optional) Finde einen Obdachlosen und frage ihnen nach der Position eines NPCs
    //1. Finde den NPC eines Netzwerkes

    //Schritt 2: Der erste Auftrag
    //	0. Du erhälst eine kleine Einführung in das Aufgabengebiet des jeweiligen Netzwerks
    //	1. Du erhälst einen Einfhührungsauftrag. (Ein kleiner Auftrag, welcher Level 0.1 des jeweiligen Netzwerkes beinhaltet)
    //	2. Danach hast du Zugriff auf die AUftreäge des Netzwerkes

    //Schritt 3: Der Aufstieg
    //	1. Mache mehrere Aufträge, versch.Art, um Netzwerklevel zu erhalten.
    //	2. Nach genügend Reputation schaltest du neue Variationen des Auftrags frei (z.B.zeitgebrenzte, ort-spezifische, etc.)
    //	3. Mit noch mehr Reputation schaltet sich ein neuer Auftragstyp frei.
    //	4. Wie beim ersten des Levels schalten sich nach einiger Zeit die Variationen der Aufträge frei.

    //Schritt 4: Level 1 der Netzwerke
    //	0. Du erhälst Zugriff auf den Shop des Netzwerks. Erste Items werden freigeschaltet und können für Geld gekauft werden.
    //	1. Der erste neue Auftragstyp wird freigeschaltet und der Progress ist wie auf Level 0
    //	2. Ab sofort wird Währung Gefallen freigeschaltet. Mit dieser kann man sich bei Informationsbeschaffern, der jeweiligen Netzewerke, mit Gefallen Informationen kaufen.	

    //ENDE FÜR GO-LIVE

    //Funktionen:
    // NPC-Funktionen:
    //	Obdachlosen-Modul: Kann, wenn Crime Flag gesetzt, gefragt werden, wo sich NPCs der jeweiligen Netzwerke befinden.

    //	Netzwerk-Modul: 
    //		NPCs reden nicht mit Leuten ohne Crime-Flag über Crime-spezifische Sachen.
    //		NPC kann ausgewählt werden bestimmte Rollen im Netzwerk zu übernehmen (Auftraggeber (A), Shop/Ausrüstungsbeschaffer (S), Informationsbeschaffer (I))
    //		NPC bekommt Geoflag(Norden, Mitte, Süden). Die NPCs sollten gleichmäßig auf der Karte verfügbar sein. (D.h.z.B.A und S in Los Santos(ggf.auch mehrere in LS), S und I in Sandy Shores und A und I in Paleto Bay)

    //		Bei jedem NPC-Typ kann man gewisse FAQ Fragen stellen.

    //		Auftraggeber:
    //			Wenn angesprochen besteht die Möglichkeit Aufträge anzunehmen:
    //			z.B.mache x mal eine bestimmte illegale Aktion.
    //			Ab einem bestimmten Punkt gibt es die Aufträge mit Zustätzen.
    //			z.B.innerhalb von Zeit x o. mache die Aktion an einem bestimmten Ort o. die Aktion muss auf eine bestimmte Art gemacht werden (z.B.laut o. leise)

    //		Shop:
    //			Bietet die Möglichkeit Items, welche illegale Aktionen ermöglichen/vereinfachen, zu kaufen.
    //			Einige Items sind für Geld kaufbar, der Rest benötigt Gefallen um gekauft werden zu können.
    //			Jedes Item wird mit der Zeit freigeschaltet.


    //		Informationsbeschaffer:
    //			Bieten Informationen im Tausch gegen Geld bzw.Gefallen an

    //			2 Arten von Informationen:
    //				Permanente Informationen: Wie z.B.Bau oder Ablaufpläne (von z.B.Snackautomaten)
    //				Direkte Informationen: 
    //						Wie z.B.aktueller Standorte o. aktuelle Bestände (von z.B.Schließfächern)
    //						Auch Möglichkeiten Sachen zu verkaufen.Gibt dann Auskunft über aktuelle "Ankäufer". Bestimmt, welche "Sonderaufträge" es gibt.


    //	Ankäufer-Modul:
    //		2 Varianten: Käufer von gestohlenen Waren und Käufer von bestimmten (z.B.reingewaschenen) Waren (z.B.temporärer Ankäufer von Kokain, oder Juwelier, der Goldbarren kauft)

    //	Social-Media Modul:
    //		Möglichkeit Aufträge auszuschreiben(TODO)

    //	Aufträge:
    //		Da jede Crime Aktion theoretisch von jedem zu jeder Zeit getätigt werden kann, beziehen sich Aufträge auf diese.
    //		Jede Crime Aktion triggert ein bestimmtest Event, sollte sich ein Spieler gerade in einem Auftrag befinden, welcher diesen Trigger abfragt, wird ein Teil des Auftrags abgeschlossen.
    //		Aufträge können auch beinhalten ein bestimmtes Item abzugegeben.z.B.ein geklautes oder hergestelltes Objekt
    //		Aufträge werden manuelle erzeugt (stets versch. Varianten) und können auch versch.Auftragstypen mischen.z.B.knacke x Parkuhren und raube einen bestimmten Shop aus.

    //	Für jede Crime Aktion eisntellbare Trigger, so wie bei den Minijobs

    //Bspl: Welche Items geklaut, etc.


    //Missionen:
    //Simple Trigger Missionen: Bestehen einfach aus einem Trigger, der x mal gemacht werden muss(z.B. 5mal Parkuhr knacken, 7x Cola su dem Laden klauen)
    //Crime Spree Missionen: Erstmal nur random, immer mit Zeitlimit kombiniert aus einzelmissionen - 30% (Idee macht die zusammen), sind 2-3 aneinandergehängte TriggerMissionen. (Idee schwierig mehrere Crime Sachen hintereinander zu machen)
    //	Können zusammen gemacht werden!

    //Missionen können beim Auftragshändler angefragt werden:
    //Dies benötigt "Token":
    //Es gibt 3 Arten:
    //	Selektiertoken: Ermöglichen einen Auftrag mit spezifizierten Crime Action anzufordern(Es werden 3 Aufträge mit dieser Aktion ausgeben) (Tracken wie oft welche Mission genommen wird)
    //	Randomtoken: Man erhält einen Random Auftrag
    //	Crime-Spree-Token: Sind ziemlich selten.Ermöglichen z.B.alle 3 Tage eine Crime Spree Mission mit anderen Leuten zu machen


    //Token sammelt man indem man aktiv online spielt.
    //Jeder Random-Token braucht 100 Token Punkte
    //Jeder Selektier-Token braucht 300 Token Punkte
    //Jeder Crime-Spree Token braucht 800 Punkte

    //Onlinezeit							0 - 1  | 1 - 2  | 2 - 3 | 3 - 4 | >=4 - >=5 |
    //Tokenpunkte pro Stunde (Alle 10min):	100    | 100    | 50    | 50    | 25        |


    //Progress
    //Level 0:
    //  Einbrecher: 
    //	Mission: Parkuhren
    //	Auftragsverteiler

    //   Hehler:
    //	Mission: Illegales Zeug verteilen
    //	Auftragsverteiler
    //	Shop:
    //	   Gestohlene Kleidung, Schmuck, Elektroartikel
    //	Ankäufer freischalten: (Je höhere Hehlerlevel, desto besser Preise)
    //	   Kaufen: Kleidung, Schmuck, Elektroartikel

    //   Dealer:
    //	Mission: Drogen kaufen und verkaufen
    //		Manchmal kann gesetzer Dealer auch mit Hehlerware bezahlt worden sein
    //	Auftragsverteiler
    //	Shop:
    //	   Verschiedene Joints und Päckchen von Drogen

    //Level 1:
    //   Einbrecher:
    //	Missionsvariationen: Parkuhren
    //	Mission: Ladendiebstahl

    //   Hehler:
    //	Missionsvariationen: Illegals Zeug verteilen
    //	Mission: Auto Tuningteile klauen

    //   Dealer:
    //	Missionsvariationen: Drogen kaufen und verkaufen
    //	Mission: Drogenprozess verfolgen(zum Verarbeiter bringen)
    //	Tauscher(Tauschen Roh/Zwischenstoffe gegen nächste Stufe?)
    //	Shop:
    //	   Verschiedene Joints und Päckchen von Drogen


    //   In einer Säule dieses Level erreicht:
    //	Crime "Looking for Group" freigeschaltet

    //Level 2:
    //   Einbrecher: 
    //	Missionsvariationen: Ladendiebstahl
    //	Mission: Kassenraub
    //		Manchmal kann auch Hehlergut in selbem Wert rausgegeben werden!
    //	Informationsbeschaffer
    //	Shop


    //   Hehler:
    //	Missionsvariationen: Auto Tuningteile klauen
    //	Mission: Geklaute Autos verkaufen(Je mehr Level 2 - 3, desto mehr Geld)
    //	Informationsbeschaffer

    //   Dealer:
    //	Missionsvariationen: Drogenprozess verfolgen
    //	Mission: Möglichkeit Drogen-Rohstoffe zu verkaufen
    //	Informationsbeschaffer

    //   Besonderheit:
    //	Ab diesem Level verliert man mit jedem abgeschlossenen Auftrag einer Säule Reputation bei einer anderen(Level 2 kann nicht wieder verloren werden)

    //Level 3:
    //   Einbrecher: 
    //	Missionsvariationen: Kassenraub
    //	Mission: Snackautomaten snacken
    //	Ankäufer:
    //		Kauft Teile von Snackmaschinen können aber auch an Hehler verkauft werden

    //   Hehler:
    //	Missionsvariationen: Geklaute Autos verkaufen
    //	Mission: Zeugs verarbeiten was nicht ganz legal ist
    //			Kleidung(Zertifikat raustrennen und gefälschtes einsetzen)
    //			Schmuck(Schmelzen und Trennne)
    //			Elektroartikel(wie Snackautomaten, auseinanderbauen und richtige Teile umprogrammieren)
    //	Shop:

    //		Werkzeug für Kleidungsstückfälschung, Labels, Nadel, passender Faden, etc.
    //		Chemikalien für Schmuckauflösung, Brenner, Werkzeug wat auch immer
    //		Elektroersatzteile(die aus Snackautomaten kommen können)

    //   Dealer:
    //	Missionsvariationen: Möglichkeit Drogen-Rohstoffe zu verkaufen
    //	Mission: Drogenprozess durchführen, aber mit gekauften Rohstoffen

    public enum CrimeAction {
        None = -1,
        ParkingClockBreaking = 0,
        StoreTheft = 1,
        StoreRobbingNpc = 2,
        IllegalItemSell = 3,
        RobNPC = 4,
        StealVehiclePart = 5,
    }

    
    public delegate void OnPlayerCrimeActionDelegate(IPlayer player, CrimeAction action, float amount, Dictionary<string, dynamic> data);
    public delegate void OnPlayerIdCrimeActionDelegate(int charId, CrimeAction action, float amount, Dictionary<string, dynamic> data);


    public class CrimeNetworkController : ChoiceVScript {
        public static OnPlayerCrimeActionDelegate OnPlayerCrimeActionDelegate;
        public static OnPlayerIdCrimeActionDelegate OnPlayerIdCrimeActionDelegate;

        public static Dictionary<int, CrimeNetworkPillar> CrimePillars;

        public static TimeSpan SHIFT_CHANGE_TIME = TimeSpan.FromHours(10);

        public CrimeNetworkController() {
            CrimePillars = new Dictionary<int, CrimeNetworkPillar> {
                { 0, new BurglarNetworkPillar(0) },
                { 1, new DealerNetworkPillar(1) },
                { 2, new FenceNetworkPillar(2) },
            };

            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnect;

            OnPlayerCrimeActionDelegate += onPlayerCrimeAction;
            OnPlayerIdCrimeActionDelegate += onPlayerIdCrimeAction;
        }

        private void onPlayerConnect(IPlayer player, character character) {
            if(!player.hasCrimeFlag()) return;

            var repList = new float[CrimePillars.Count];
            var favList = new float[CrimePillars.Count];

            foreach(var rep in character.charactercrimereputations) {
                repList[rep.pillarId] = rep.reputation;
                favList[rep.pillarId] = rep.favors;
            }

            player.setData("CRIME_REPUTATION", new PlayerCrimeReputation(repList.ToList(), favList.ToList()));

            if(character.charactercrimemissiontoken != null) {
                player.setData("CRIME_MISSION_TOKENS", new CrimeMissionTokens(character.charactercrimemissiontoken.selectToken, character.charactercrimemissiontoken.randomToken, character.charactercrimemissiontoken.crimeSpreeToken));
            } else {
                player.setData("CRIME_MISSION_TOKENS", new CrimeMissionTokens(0, 0, 0));
            }

            if(character.charactercrimemissiontrigger != null) {
                player.setData("CRIME_MISSIONS", new PlayerCrimeMissions(character.charactercrimemissiontrigger));
            } else {
                player.setData("CRIME_MISSIONS", new PlayerCrimeMissions());
            }

            using(var db = new ChoiceVDb()) {
                var triggers = db.charactercrimeofflinetriggers.Where(t => t.charId == player.getCharacterId()).ToList();
                foreach(var trigger in triggers) {
                    onPlayerIdCrimeAction(trigger.charId, (CrimeAction)trigger.action, trigger.amount, trigger.data.FromJson<Dictionary<string, dynamic>>());
                    db.charactercrimeofflinetriggers.Remove(trigger);
                }

                db.SaveChanges();
            }
        }

        private void onPlayerCrimeAction(IPlayer player, CrimeAction action, float amount, Dictionary<string, dynamic> data) {
            CrimeMissionsController.onPlayerCrimeAction(player, action, amount, data);
        }

        private void onPlayerIdCrimeAction(int charId, CrimeAction action, float amount, Dictionary<string, dynamic> data) {
            var player = ChoiceVAPI.FindPlayerByCharId(charId);

            if(player != null) {
                CrimeMissionsController.onPlayerCrimeAction(player, action, amount, data);
            } else {
                using(var db = new ChoiceVDb()) {
                    var newTrigger = new charactercrimeofflinetrigger {
                        charId = player.getCharacterId(),
                        action = (int)action,
                        amount = amount,
                        data = data.ToJson(),
                    };

                    db.charactercrimeofflinetriggers.Add(newTrigger);
                    db.SaveChanges();
                }
            }
        }

        public static void playerRemoveReputation(int charId, float amount, CrimeNetworkPillar pillar) {
            var onlinePlayer = ChoiceVAPI.FindPlayerByCharId(charId);

            if(onlinePlayer != null) {
                playerRemoveReputation(onlinePlayer, pillar, amount);
            } else {
                using (var db = new ChoiceVDb()) {
                    if (pillar == null) {
                        foreach (var rep in db.charactercrimereputations.Where(r => r.charId == charId)) {
                            rep.reputation -= amount;
                        }
                    } else {
                        var rep = db.charactercrimereputations.FirstOrDefault(r => r.charId == charId && r.pillarId == pillar.Id);
                        if (rep != null) {
                            rep.reputation -= amount;
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        public static void playerRemoveReputation(IPlayer player, CrimeNetworkPillar pillar, float amount) {
            var reputation = player.getData("CRIME_REPUTATION") as PlayerCrimeReputation;
            reputation.removeReputation(pillar, amount);
        }

        public static CrimeNetworkPillar getPillarById(int id) {
            if(CrimePillars.ContainsKey(id)) {
                return CrimePillars[id];
            } else {
                return null;
            }
        }

        public static CrimeNetworkPillar getPillarByType<T>() {
            return CrimePillars.Values.FirstOrDefault(p => p is T);
        }

        public static CrimeNetworkPillar getPillarByPredicate(Predicate<CrimeNetworkPillar> predicate) {
            return CrimePillars.Values.FirstOrDefault(p => predicate(p));
        }

        public static List<CrimeNetworkPillar> getAllPillars() {
            return CrimePillars.Values.ToList();
        }

        public static string getNameForCrimeAction(CrimeAction action) {
            return action switch {
                CrimeAction.ParkingClockBreaking => "Parkuhren aufbrechen",
                CrimeAction.StoreTheft => "Waren stehlen",
                CrimeAction.StoreRobbingNpc => "Kassenraub",
                CrimeAction.IllegalItemSell => "Illegale Waren (ver)kaufen",
                CrimeAction.RobNPC => "Personen ausrauben",
                CrimeAction.StealVehiclePart => "Fahrzeugteile stehlen",
                _ => "Fehler",
            };
        }

        public static CrimeAction getCrimeActionFromName(string name) {
            return name switch {
                "Parkuhren aufbrechen" => CrimeAction.ParkingClockBreaking,
                "Waren stehlen" => CrimeAction.StoreTheft,
                "Kassenraub" => CrimeAction.StoreRobbingNpc,
                "Illegale Waren (ver)kaufen" => CrimeAction.IllegalItemSell,
                "Personen ausrauben" => CrimeAction.RobNPC,
                "Fahrzeugteile stehlen" => CrimeAction.StealVehiclePart,
                _ => CrimeAction.None,
            };
        }
    }
}
