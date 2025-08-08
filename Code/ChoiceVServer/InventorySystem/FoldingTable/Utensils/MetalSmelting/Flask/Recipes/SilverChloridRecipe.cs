using ChoiceVServer.Controller;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem.FoldingTable {
    public interface IContainsSilver {
        public float ContainsSilverAmountInG { get; }

        public void reduceSilverAmount(float amountInG);
    }

    public class SilverChloridRecipe : FlaskRecipe {
        public SilverChloridRecipe() : base() { }

        //500ml per hour equals 100g Silverchlroid per hour
        private static float REACTING_KINGWATER_PER_HOUR = 500;
        private static float REACTING_SILVER_PER_HOUR = 100;
        public override void onTick(TimeSpan tickLength, FlaskUtensil flask) {
            var timeMult = (float)(tickLength / TimeSpan.FromHours(1));
            timeMult = Math.Min(1, timeMult);

            var malus = 1f;
            if(flask.Temperature < 80 || flask.Temperature > 100) {
                var abs = Math.Abs(flask.Temperature - 90) - 10;

                malus = Math.Min(1 - (abs / 20), 0.1f);
            }

            var hAItem = flask.Inventory.getItem<FoldingTableChemical>(i => i.Component == ChemicalType.HydrochloricAcid);
            var nAItem = flask.Inventory.getItem<FoldingTableChemical>(i => i.Component == ChemicalType.NitricAcid);
            var silverItems = flask.Inventory.getItems<IContainsSilver>(i => i.ContainsSilverAmountInG > 0).ToList();

            var totalSilverAmount = 0f;
            silverItems.ForEach(i => totalSilverAmount += i.ContainsSilverAmountInG);

            var availableSilverMult = Math.Min(1, totalSilverAmount / REACTING_SILVER_PER_HOUR);
            var silverAmount = Math.Min(totalSilverAmount, timeMult * REACTING_SILVER_PER_HOUR);

            var preferedKingWaterAmount = silverAmount * REACTING_KINGWATER_PER_HOUR / REACTING_SILVER_PER_HOUR;

            var kingWaterAmount = 0f;
            if(preferedKingWaterAmount * 0.25 <= nAItem.Amount && preferedKingWaterAmount * 0.75 <= hAItem.Amount) {
                kingWaterAmount = preferedKingWaterAmount;
                hAItem.Amount -= preferedKingWaterAmount * 0.75f;
                nAItem.Amount -= preferedKingWaterAmount * 0.25f;

                if(nAItem.Amount <= 0) {
                    nAItem.destroy();
                }

                if(hAItem.Amount <= 0) {
                    hAItem.destroy();
                }
            } else {
                if(hAItem.Amount / 3 <= nAItem.Amount) {
                    kingWaterAmount = hAItem.Amount + hAItem.Amount / 3;
                    nAItem.Amount -= hAItem.Amount / 3;
                    if(nAItem.Amount <= 0) {
                        nAItem.destroy();
                    }
                    hAItem.destroy();
                } else {
                    kingWaterAmount = nAItem.Amount * 3 + nAItem.Amount;
                    hAItem.Amount -= nAItem.Amount * 3;
                    if(hAItem.Amount <= 0) {
                        hAItem.destroy();
                    }
                    nAItem.destroy();
                }
            }

            var tempSilverAmount = silverAmount * Math.Min(1, kingWaterAmount / preferedKingWaterAmount);
            foreach(var silverItem in silverItems) {
                if(silverItem.ContainsSilverAmountInG < tempSilverAmount) {
                    silverItem.reduceSilverAmount(silverItem.ContainsSilverAmountInG);
                    tempSilverAmount -= silverItem.ContainsSilverAmountInG;
                } else {
                    silverItem.reduceSilverAmount(tempSilverAmount);
                    break;
                }
            }

            var already = flask.Inventory.getItem<FoldingTableChemical>(i => i.Component == ChemicalType.SilverChlorid);
            if(already != null) {
                already.Amount += silverAmount * malus * Math.Min(1, kingWaterAmount / preferedKingWaterAmount);
            } else {
                var cfg = InventoryController.getConfigItemForType<FoldingTableChemical>();
                flask.Inventory.addItem(new FoldingTableChemical(cfg, ChemicalType.SilverChlorid, silverAmount * malus * Math.Min(1, kingWaterAmount / preferedKingWaterAmount), 1));
            }
        }

        protected override List<Predicate<IFoldingTableInFlaskItem>> getAllPredicates() {
            return new List<Predicate<IFoldingTableInFlaskItem>> {
                (i) => i is FoldingTableChemical ch && ch.Component == ChemicalType.HydrochloricAcid && ch.Amount > 0,
                (i) => i is FoldingTableChemical ch && ch.Component == ChemicalType.NitricAcid && ch.Amount > 0,
                (i) => i is IContainsSilver cs && cs.ContainsSilverAmountInG > 0,
            };
        }

        protected override Predicate<FlaskUtensil> getFlaskPredicate() {
            return (f) => f.Temperature > 50;
        }
    }
}
