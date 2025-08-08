using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.Model.Database;
using System;

namespace ChoiceVServer.InventorySystem {
    public enum ChemicalType {
        None = -1,
        HydrochloricAcid = 0, //Salzssäure
        NitricAcid = 1, //Salpetersäure
        OxalicAcid = 2, //Oxalsäure
        SodiumHydroxide = 3, //Natriumhydroxid aka Ätznatron
        SulfurDioxide = 4, //Schwefeldioxid

        SilverChlorid = 5, //Silberchlorid
        CleanSilverChlorid = 6, //gereinigtes Silberchlorid
        DestilledWater = 7, //Destilliertes Wasser
        ColloidalSilver = 8, // Kolloidales Silber (nass)
        SilverDust = 9, // Silberstaub
    }

    public class ChemicalContainer : Item, IFoldingTableFlaskPuttable {
        public ChemicalType Component { get => (ChemicalType)Data["Component"]; set { Data["Component"] = value; } }
        public float Amount { get => (int)Data["Amount"]; set { Data["Amount"] = value; calculateWeight(); updateDescription(); } }
        public float MaxAmount { get => Data.hasKey("MaxAmount") ? (int)Data["MaxAmount"] : 400; set { Data["MaxAmount"] = value; } }

        public string FlaskName => Name;
        public string FlaskInfo => Description;
        public bool OnlyFullInFlask => false;
        public float FlaskAvailableAmount => Amount;
        public string FlaskUnit => chemicalToUnit(Component);

        public ChemicalContainer(item item) : base(item) {
            calculateWeight();
        }

        public ChemicalContainer(configitem configItem, int amount, int quality) : base(configItem, quality) {
            if(configItem.additionalInfo != null) {
                var split = configItem.additionalInfo.Split('#');
                Component = (ChemicalType)int.Parse(split[0]);
                Amount = int.Parse(split[1]);
                MaxAmount = int.Parse(split[1]);
            } else {
                Component = ChemicalType.None;
                Amount = 0;
            }
        }

        public void calculateWeight() {
            Weight = (Amount / 1000f) + 0.1f;
        }

        public override void processAdditionalInfo(string info) {
            if(info != null) {
                var split = info.Split('#');
                Component = (ChemicalType)int.Parse(split[0]);
            } else {
                if(!Data.hasKey("Component")) {
                    Component = ChemicalType.None;
                }
            }
            updateDescription();
        }

        public override void updateDescription() {
            if(Component == ChemicalType.None) {
                Description = $"Ist leer und enthält keine Chemikalie";
            } else {
                Description = $"Enthält {Amount} {chemicalToUnit(Component)} {chemicalToName(Component)}";
            }

            base.updateDescription();
        }

        public static string chemicalToName(ChemicalType component) {
            return component switch {
                ChemicalType.HydrochloricAcid => "konz. Salzsäure",
                ChemicalType.NitricAcid => "konz. Salpetersäure",
                ChemicalType.OxalicAcid => "Oxalsäure",
                ChemicalType.SodiumHydroxide => "Natriumhydroxid",
                ChemicalType.SulfurDioxide => "Schwefeldioxid",

                ChemicalType.SilverChlorid => "Silberchlorid",
                ChemicalType.CleanSilverChlorid => "gereingtes Silberchlorid",
                ChemicalType.ColloidalSilver => "kolloidales Silber",
                ChemicalType.SilverDust => "Silberstaub",

                ChemicalType.DestilledWater => "dest. Wasser",
                _ => "Fehler",
            };
        }

        public static string chemicalToUnit(ChemicalType component) {
            return component switch {
                ChemicalType.HydrochloricAcid => "ml",
                ChemicalType.NitricAcid => "ml",
                ChemicalType.OxalicAcid => "g",
                ChemicalType.SodiumHydroxide => "g",
                ChemicalType.SulfurDioxide => "g",

                ChemicalType.SilverChlorid => "g",
                ChemicalType.CleanSilverChlorid => "g",
                ChemicalType.ColloidalSilver => "ml",
                ChemicalType.SilverDust => "g",

                ChemicalType.DestilledWater => "ml",
                _ => "Fehler",
            };
        }

        public static SoundController.Sounds chemicalGetSoundForUnit(string unit) {
            return unit switch {
                "kg" => SoundController.Sounds.SolidPour,
                "g" => SoundController.Sounds.SolidPour,
                "ml" => SoundController.Sounds.LiquidPour,
                _ => SoundController.Sounds.None,
            };
        }

        public static bool chemicalCanBePulled(ChemicalType component) {
            return component switch {
                ChemicalType.HydrochloricAcid => false,
                ChemicalType.NitricAcid => false,
                ChemicalType.OxalicAcid => false,
                ChemicalType.SodiumHydroxide => false,
                ChemicalType.SulfurDioxide => false,

                ChemicalType.SilverChlorid => true,
                ChemicalType.CleanSilverChlorid => true,
                ChemicalType.ColloidalSilver => true,
                ChemicalType.SilverDust => true,

                ChemicalType.DestilledWater => false,
                _ => false,
            };
        }


        public void onPutInFlask(IPlayer player, FlaskUtensil flask, float amountPre) {
            var amount = Math.Min(Amount, amountPre);

            Amount -= amount;

            var already = flask.Inventory.getItem<FoldingTableChemical>(c => c.Component == Component);
            if(already != null) {
                already.Amount += amount;
            } else {
                var cfg = InventoryController.getConfigItemForType<FoldingTableChemical>();
                flask.Inventory.addItem(new FoldingTableChemical(cfg, Component, amount, Quality));
            }

            updateDescription();

            player.sendNotification(Constants.NotifactionTypes.Success, $"Erfolgreich {amount} {chemicalToUnit(Component)} {chemicalToName(Component)} hinzugefügt!", "Chemikalie hinzugefügt", Constants.NotifactionImages.FoldingTable);
        }
    }
}
