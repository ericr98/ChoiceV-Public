using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem {
    public record MedicationDrugCheck(float currentLevel, int maxPainLevel);

    public class MedicationDrug : Drug {
        protected int MaxPainLevel;

        public MedicationDrug(string identifier, string name, float levelPerDose, float resitancePerDose, float dependencyPerDose, float levelDropoff, float resDropoff, float depDropoff, int maxPainLevel) : base(identifier, name, levelPerDose, resitancePerDose, dependencyPerDose, levelDropoff, resDropoff, depDropoff) {
            MaxPainLevel = maxPainLevel;
        }

        public override void checkForEffect(IPlayer player) { }

        public MedicationDrugCheck getPlayerLevel(IPlayer player) {
            if(player.getCharacterData().DrugLevels.ContainsKey(Identifier)) {
                var drug = player.getCharacterData().DrugLevels[Identifier];
                return new MedicationDrugCheck(drug.CurrentLevel, MaxPainLevel);
            } else {
                return null;
            }
        }
    }

    public class IbuprofenDrug : MedicationDrug {
        public IbuprofenDrug() : base("IBUPROFEN", "Ibuprofen",
            0.15f, //LevelPerDose
            0.1f, //ResistancePerDose 
            0f, //DependencyPerDose 
            0.1f, //LevelDropoff/h
            0.004f, //ResistanceDropoff/h
            0f, //DependencyDropoff/h
            2 //Max. usefull for level 3 
            ) {
        }

        public override void checkForEffect(IPlayer player) {
            //Ibu and Para dont give Effects
        }
    }

    public class ParacetamolDrug : MedicationDrug {
        public ParacetamolDrug() : base("PARACETAMOL", "Paracetamol",
            0.15f, //LevelPerDose
            0.1f, //ResistancePerDose 
            0f, //DependencyPerDose 
            0.1f, //LevelDropoff/h
            0.02f, //ResistanceDropoff/h
            0f, //DependencyDropoff/h
            2 //Max. usefull for level 3 
            ) {
        }

        public override void checkForEffect(IPlayer player) {
            //Ibu and Para dont give Effects
        }
    }


    public class TilidinDrug : MedicationDrug {
        public TilidinDrug() : base("TILIDIN", "Tilidin",
            0.3f, //LevelPerDose
            0.1f, //ResistancePerDose 
            0.1f, //DependencyPerDose 
            0.1f, //LevelDropoff/h
            0.02f, //ResistanceDropoff/h
            0.01f, //DependencyDropoff/h
            4 //Max. usefull for level 3 
            ) {
        }

        public override void checkForEffect(IPlayer player) {
            //TODO If Dependant and not enough level give effects!
        }
    }

    public class TramadolDrug : MedicationDrug {
        public TramadolDrug() : base("Tramadol", "TRAMADOL",
            0.3f, //LevelPerDose
            0.1f, //ResistancePerDose 
            0.1f, //DependencyPerDose 
            0.1f, //LevelDropoff/h
            0.02f, //ResistanceDropoff/h
            0.01f, //DependencyDropoff/h
            4 //Max. usefull for level 3 
            ) {
        }

        public override void checkForEffect(IPlayer player) {
            //TODO If Dependant and not enough level give effects!
        }
    }

    public class MorphinDrug : MedicationDrug {
        public MorphinDrug() : base("MORPHIN", "Morphin",
            0.6f, //LevelPerDose
            0.1f, //ResistancePerDose 
            0.15f, //DependencyPerDose 
            0.1f, //LevelDropoff/h
            0.02f, //ResistanceDropoff/h
            0.01f, //DependencyDropoff/h
            6 //Max. usefull for level 3 
            ) {
        }

        public override void checkForEffect(IPlayer player) {
            //TODO If Dependant and not enough level give effects!
            //If Level surpasses certain level, set screen effect

        }
    }

    public class FentanylDrug : MedicationDrug {
        public FentanylDrug() : base("FENTANYL", "Fentanyl",
            0.6f, //LevelPerDose
            0.1f, //ResistancePerDose 
            0.15f, //DependencyPerDose 
            0.1f, //LevelDropoff/h
            0.02f, //ResistanceDropoff/h
            0.01f, //DependencyDropoff/h
            6 //Max. usefull for level 3 
            ) {
        }

        public override void checkForEffect(IPlayer player) {
            //TODO If Dependant and not enough level give effects!
            //If Level surpasses certain level, set screen effect

        }
    }

    public class PainMedicationController : ChoiceVScript {

        public PainMedicationController() {
            DrugController.addDrug(new IbuprofenDrug());
            DrugController.addDrug(new ParacetamolDrug());
            DrugController.addDrug(new TilidinDrug());
            DrugController.addDrug(new TramadolDrug());
            DrugController.addDrug(new FentanylDrug());
            DrugController.addDrug(new MorphinDrug());
        }

        public static int getMedicatedPainLevel(List<MedicationDrug> allMedications, IPlayer player) {
            var runningCount = new float[6];

            foreach(var drug in allMedications) {
                drug.update(player);
                var ret = drug.getPlayerLevel(player);
                if(ret != null) {
                    for(var i = 0; i < ret.maxPainLevel; i++) {
                        var lvl = i + 1;
                        if(lvl % 2 != 0) {
                            lvl++;
                        }

                        runningCount[i] += ret.currentLevel * ((float)ret.maxPainLevel / lvl);
                    }
                }
            }

            var maxLevel = 0;
            for(var i = 0; i < runningCount.Length; i++) {
                if((i + 1) % 2 == 0) {
                    if(runningCount[i] >= 0.66) {
                        maxLevel = i + 1;
                    }
                } else {
                    if(runningCount[i] >= 0.33) {
                        maxLevel = i + 1;
                    }
                }
            }

            return maxLevel;
        }
    }

    //   Schmerzmittel:
    //   Sollten nur bei Verletzungen funktionieren, welche dem Level her passen(WHO-Stufenplan)


    //   Egal wieviel Ibuprofen eingeworfen wird, es kann niemals eine Verletzung Stufe > 2 gemildert werden.
    //   Aber gemilderte Verletzungen zählen nicht mehr zum "Effekt-Counter" dazu.D.h: Bei 2 leichten und einer mittleren Verletzung, können mit Ibu Effekte mögl.negiert werden.


    //   Schmerzlevel 1-2: Ibuprofen, Paracetamol, (WHO Stufe 1)

    //       Schmerzlevel 1: 100mg Ibuprofen, 1x Paracetamol 500mg
    //       Schmerzlevel 2: 200mg Ibuprofen, 2x Paracetamol 500mg
    //   Schmerzlevel 3-4: Tramadol, Tilidin(WHO Stufe 2)

    //       Schmerzlevel 3: 50mg Tramadol, 50mg Tilidin

    //       Schmerzlevel 4: 100mg Tramadol, 100mg Tilidin

    //   Schmerzlevel 5-6: Morphin, Fentanyl(WHO Stufe 3)

    //       Schmerzlevel 5: 15mg Morphin(Spritze), 0.1mg Fentanyl(Infusion, Pflaster oder Tablette?)

    //       Schmerzlevel 6: 2x 15mg Morphin(Spritze), 0.5mg Fentanyl(Infusion, Pflaster oder Tablette?)

    public class PainMedication : Item {
        private string DrugIdentifier;

        public PainMedication(item item) : base(item) {
            processAdditionalInfo(item.config.additionalInfo);

            UsingHasToBeConfirmed = true;
        }

        //Constructor for generic generation
        public PainMedication(configitem configItem, int amount, int quality) : base(configItem, quality, amount) {
            processAdditionalInfo(configItem.additionalInfo);

            UsingHasToBeConfirmed = true;
        }

        public override void use(IPlayer player) {
            base.use(player);

            Animation anim;
            if(DrugIdentifier == "MORPHIN") {
                anim = AnimationController.getAnimationByName("TAKE_SYRINGE");
            } else {
                anim = AnimationController.getAnimationByName("TAKE_PILL");
            }

            AnimationController.animationTask(player, anim, () => {
                var cfgDrug = DrugController.getDrugByIdentifier(DrugIdentifier);

                if(cfgDrug != null) {
                    cfgDrug.addDose(player);
                }

                player.sendNotification(Constants.NotifactionTypes.Success, "Schmerzmittel erfolgreich genommen. Es kann etwas dauern bis die Wirkung einsetzt", "Schmzermittel genommen", Constants.NotifactionImages.Bone);
            });
        }

        public override void processAdditionalInfo(string info) {
            DrugIdentifier = info;
        }
    }
}
