using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.InventorySystem.FoldingTable;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem {
    //
    //Edelmetallgehalt kann mit Ultraschallgerät heruasgefunden werden

    //https://www.entdecke-schmuck.eu/94464514nx1490/galvanik-edelmetallscheidung-aetzen-f16/scheiden-von-alt-silber-zu-999er-silber-t753.html
    //Für Silber:
    // Mit Königswasser reagieren lassen (entwender in kleinen Mengen, dafür längere Zeit und immer mal wieder nachfüllen, oder viel drauf, kürzere Zeit mehr Kosten)
    //Dann Reinigen (Dauert Zeit x) => Immer nach Zeit x mit destilliert. Wasser auswaschen um zu gucken ob es rein ist
    //Dann Silber mit Natriumhydroxid kombinieren (Wieder richtige Menge)
    //Dann muss trocknen (Zeit x)
    //Dann kann es zu Barren geschmolzen werden

    //https://www.entdecke-schmuck.eu/94464514nx1490/galvanik-edelmetallscheidung-aetzen-f16/scheiden-von-zahn-gold-im-grossen-massstab-t747.html
    //Für Gold und Silber
    // Mit Königswasser reagieren lassen (entwender in kleinen Mengen, dafür längere Zeit und immer mal wieder nachfüllen, oder viel drauf, kürzere Zeit mehr Kosten)
    // Man erhält Silberchlorid und Metallklumpen, Silberchlorid muss abgefiltert werden (hoch zu Silberverarbeitung)
    // Metallklumpen müssen auf 80 Grad erhitzt werden, danach Schwefeldioxid rein, Muss gerührt werden (Wenn zu hohe Temperatur, dann Gold teilweise weg)
    // Das Gemisch filtern und Waschen
    // Oxalsäure (richitge Menge + Wasser) mit Gold mischen und Erhitzen (auf richtige Temperatur) (Wenn zu hohe Temperatur, dann Gold teilweise weg)
    // Dann muss trocknen (Zeit x)
    // Dann kann es zu Barren geschmolzen werden

    //Benötigt
    // Klapptisch (wenn interagiert und unten gelistete Items => können drauf gepackt werden)


    // Schmelztiegel (Zum schmelzen zu flüssigem Edelmetall)
    // Exsikkator (zu Trocknen)
    // Büchnertrichter (Nutsche)
    // Glaskolbenvorrichtung
    // Ultraschallgerät

    //Hilftmittel:
    // Bunsenbrenner
    // Waage

    //Items (nicht auf dem Tisch) (ggf. auf Ablage)
    // Wasser (Für Nutsche und zum schnellen Abkühlen der Barren)
    // konz. Salpetersäure
    // konz. Salzsäure
    // Oxalsäure
    // Natriumhydroxid
    // Schwefeldioxid


    //Klapptisch aufstellen, alle obigen Utelnsilien daraufstellen
    //1. Schmuck und Chemikalien auf Ablage legen
    //2. Durch Ultraschallgerät bestimmen wieviel Edelmetall da drinnen ist (Beschreibung wird geändert, dass dran steht wie viel drinnen ist)
    //3. Glaskolbenvorrichtung auswählen:
    //   Schmuck reinlegen
    //   Salpetersäure und Salzsäure (in richtigen Mengen einfüllen aka. Königwasser)
    //   Es muss 80 - 100 Grad haben die ganze Zeit
    //   Prozess starten (Jeden Tick wird x Königswasser mit y Silber in Schmuck reagieren, x hängt von der Menge an Königswasser ab!)
    //   Option 1: Genügend Königswasser eingefüllt => Przess ist nach Zeit X ohne Einwirken zuende
    //   Option 2: Geringe Menge Königswasser drinnen => Es muss immer mal wieder nachgefüllt werden
    //4. Im Glaskolben sind Silberchlorid und ein Metallklumpen (mit restlichen Metallen) (Kann wieder unter Ultraschall gehalten werden)
    //5. Metallklumpen wird rausgenommen.

    //6. Nutsche auswählen und Silberchlorid von Kolben tranferieren und mind. Menge Wasser hinzugeben (Nach Menge Silberchlorid)
    //7. Reinigungsprozess starten => Dauert Zeit x. Wenn Wasser alle muss nachgefüllt werden
    //8. Wieder Kolben auswählen und Silberchlorid von Nutsche transferieren. Richtige Menge Natriumhydroxid beigeben, die Silberchlorid in elementares Silber umwandelt
    //   Wenn nicht gereinigt => Viel Silber geht verloren
    //9. Dann Exsikkator auswählen und elementares Silber reinpacken. Prozess starten => Dauert Zeit x, dann kommt Silberstaub raus
    //10. Schmelztiegel auswählen, Silberstaub rein, und zu Barren (dauert wieder kurze Zeit)


    // 5.1 Metallklumpen in Glaskolben geben
    // 6.1 Schwefeldioxid in Glaskolben und auf 80 Grad erhitzen
    // 7.1 Nutsche wie bei Silber => kristallines Gold
    // 8.1 Oxalsäure mit Zeug in Glaskolben kombinieren => elementares Gold
    // 9.1 Trocknen wie Silber => Goldstaub
    // 10.1 Schmelztiegel wie Silber

    public class FenceJewelry : FenceGood, IUltrasoundScannable {
        private List<FenceJewelryMetal> Composition { get => ((string)Data["Composition"]).FromJson<List<FenceJewelryMetal>>(); set => Data["Composition"] = value.ToJson(); }
        private List<FenceJewelryJewel> Jewels { get => ((string)Data["Jewels"]).FromJson<List<FenceJewelryJewel>>(); set => Data["Jewels"] = value.ToJson(); }

        public string UltrasoundScanName => Name;
        public string UltrasoundScanDescription => Description;

        public FenceJewelry(item item) : base(item) { }

        public FenceJewelry(configitem configItem, int amount, int quality) : base(configItem, quality) {
            setComposition(quality);
        }

        private void setComposition(int quality) {
            var rand = new Random();
            var alreadyPercent = 0;
            if(quality == 0) {
                var comp = new List<FenceJewelryMetal>();

                var silverPer = rand.Next(1, 20);
                alreadyPercent += silverPer;
                comp.Add(new FenceJewelryMetal(JeweleryMetal.Silver, ((float)silverPer) / 100f));

                var leadPercent = rand.Next(30, 60);
                alreadyPercent += leadPercent;
                comp.Add(new FenceJewelryMetal(JeweleryMetal.Lead, ((float)leadPercent) / 100f));

                var tinPercent = 100 - alreadyPercent;
                comp.Add(new FenceJewelryMetal(JeweleryMetal.Tin, ((float)tinPercent) / 100f));

                Composition = comp;
            } else if(quality == 1) {
                var comp = new List<FenceJewelryMetal>();

                var goldPer = rand.Next(1, 20);
                alreadyPercent += goldPer;
                comp.Add(new FenceJewelryMetal(JeweleryMetal.Gold, ((float)goldPer) / 100f));

                var silverPer = rand.Next(10, 25);
                alreadyPercent += silverPer;
                comp.Add(new FenceJewelryMetal(JeweleryMetal.Silver, ((float)silverPer) / 100f));

                var leadPercent = rand.Next(30, 55);
                alreadyPercent += leadPercent;
                comp.Add(new FenceJewelryMetal(JeweleryMetal.Lead, ((float)leadPercent) / 100f));

                var tinPercent = 100 - alreadyPercent;
                comp.Add(new FenceJewelryMetal(JeweleryMetal.Tin, ((float)tinPercent) / 100f));

                Composition = comp;
            } else if(quality == 2) {
                var comp = new List<FenceJewelryMetal>();

                var goldPer = rand.Next(15, 30);
                alreadyPercent += goldPer;
                comp.Add(new FenceJewelryMetal(JeweleryMetal.Gold, ((float)goldPer) / 100f));

                var silverPer = rand.Next(20, 40);
                alreadyPercent += silverPer;
                comp.Add(new FenceJewelryMetal(JeweleryMetal.Silver, ((float)silverPer) / 100f));

                var leadPercent = 100 - alreadyPercent;
                comp.Add(new FenceJewelryMetal(JeweleryMetal.Lead, ((float)leadPercent) / 100f));

                Composition = comp;

                var jewelList = new List<FenceJewelryJewel>();

                if(rand.NextDouble() > 0.75) {
                    jewelList.Add(new FenceJewelryJewel(JewleryJewel.Pearl, rand.Next(1, 3)));
                    jewelList.Add(new FenceJewelryJewel(JewleryJewel.Zircon, rand.Next(3, 5)));
                } else {
                    jewelList.Add(new FenceJewelryJewel(JewleryJewel.Zircon, rand.Next(4, 6)));
                }

                Jewels = jewelList;
            }
        }

        public List<FenceJewelryMetal> getComposition() {
            return Composition;
        }

        public void onUltrasoundScan() {
            Description = "";

            if(Jewels != null && Jewels.Count > 0) {
                Description = "Edelsteine: ";
                foreach(var jewel in Jewels) {
                    Description += $"{jewel}, ";
                }
                Description = Description.Substring(0, Description.Length - 2);
                Description += " | ";
            }

            if(Composition.Count > 0) {
                Description += "Metalle: ";
                foreach(var metal in Composition) {
                    Description += $"{metal}, ";
                }
                Description = Description.Substring(0, Description.Length - 2);
            }

            updateDescription();
        }
    }

    public class CutFenceJewelery : Item, IContainsSilver, IFoldingTableFlaskPuttable, IFoldingTableInFlaskItem, IUltrasoundScannable {
        private List<FenceJewelryMetal> Composition { get => ((string)Data["Composition"]).FromJson<List<FenceJewelryMetal>>(); set => Data["Composition"] = value.ToJson(); }
        private float JewelryWeight { get => (float)Data["JewelryWeight"]; set { Data["JewelryWeight"] = value; Weight = value; } }

        public float ContainsSilverAmountInG => Composition.Find(c => c.Metal == JeweleryMetal.Silver)?.Percentage * JewelryWeight * 1000 ?? 0;

        public string FlaskName => Name;
        public string FlaskInfo => Description;
        public bool OnlyFullInFlask => true;
        public float FlaskAvailableAmount => JewelryWeight;
        public string FlaskUnit => "kg";

        public string InFlaskName => Name;
        public string InFlaskDescription => Description;
        public float InFlaskAmount => JewelryWeight;
        public string InFlaskUnit => "kg";
        public bool CanBePulledFromFlask => true;

        public string UltrasoundScanName => Name;
        public string UltrasoundScanDescription => Description;


        public CutFenceJewelery(item item) : base(item) { Weight = JewelryWeight; }

        public CutFenceJewelery(FenceJewelry previous) : base(InventoryController.getConfigItemForType<CutFenceJewelery>(), previous.Quality) {
            JewelryWeight = previous.Weight;
            Weight = previous.Weight;

            Composition = previous.getComposition();

            Description = $"Ehemalig {previous.Name} | {previous.Description}";
            updateDescription();
        }

        public void onPutInFlask(IPlayer player, FlaskUtensil flask, float amount) {
            ResidingInventory.moveItem(flask.Inventory, this);
        }

        public void getPulledItem(IPlayer player, FlaskUtensil utensil) {
            var anim = AnimationController.getAnimationByName("WORK_FRONT");
            AnimationController.animationTask(player, anim, () => {
                if(utensil.Inventory.moveItem(player.getInventory(), this)) {
                    player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast erfolgreich {Name} aus der Vorrichtung genommen", $"{Name} genommen", Constants.NotifactionImages.FoldingTable);
                } else {
                    player.sendBlockNotification("Nicht genügend Platz im Inventar!", "Nicht genug Platz", Constants.NotifactionImages.FoldingTable);
                }
            }, null, true, 1, TimeSpan.FromSeconds(1));
        }

        public void reduceSilverAmount(float amountInG) {
            var comp = Composition;
            var silverCom = comp.Find(c => c.Metal == JeweleryMetal.Silver);

            if(silverCom != null) {
                var newSilverAmount = (silverCom.Percentage * JewelryWeight * 1000 - amountInG) / 1000;

                JewelryWeight -= amountInG / 1000;
                silverCom.Percentage = newSilverAmount / JewelryWeight;
            }

            Composition = comp;
        }

        public void onUltrasoundScan() {
            Description = "";

            if(Composition.Count > 0) {
                Description = "Metalle: ";
                foreach(var metal in Composition) {
                    Description += $"{metal}, ";
                }
                Description = Description.Substring(0, Description.Length - 2);
            }

            updateDescription();
        }
    }

    public enum JeweleryMetal : int {
        Tin = 0,
        Lead = 1,
        Silver = 2,
        Gold = 3,
        Platinum = 4,
    }

    public class FenceJewelryMetal {
        public JeweleryMetal Metal;
        public float Percentage;

        public FenceJewelryMetal(JeweleryMetal metal, float percentage) {
            Metal = metal;
            Percentage = percentage;
        }

        private string getName() {
            switch(Metal) {
                case JeweleryMetal.Tin:
                    return "Zinn";
                case JeweleryMetal.Lead:
                    return "Blei";
                case JeweleryMetal.Silver:
                    return "Silver";
                case JeweleryMetal.Gold:
                    return "Gold";
                case JeweleryMetal.Platinum:
                    return "Platin";
                default:
                    return "Fehler";
            }
        }

        public override string ToString() {
            return $"{Math.Round(Percentage * 100, 2)}% {getName()}";
        }
    }

    public enum JewleryJewel : int {
        Zircon = 0,
        Pearl = 1,
        Diamond = 2,
    }

    public class FenceJewelryJewel {
        public JewleryJewel Jewel;
        public int Amount;

        public FenceJewelryJewel(JewleryJewel jewel, int amount) {
            Jewel = jewel;
            Amount = amount;
        }

        private string getName() {
            switch(Jewel) {
                case JewleryJewel.Zircon:
                    if(Amount <= 1) {
                        return "Zirkon";
                    } else {
                        return "Zirkone";
                    }
                case JewleryJewel.Pearl:
                    if(Amount <= 1) {
                        return "Perle";
                    } else {
                        return "Perlen";
                    }
                case JewleryJewel.Diamond:
                    if(Amount <= 1) {
                        return "Diamant";
                    } else {
                        return "Diamanten";
                    }
                default:
                    return "Fehler";
            }
        }

        public override string ToString() {
            return $"{Amount} {getName()}";
        }
    }
}
