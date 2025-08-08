using ChoiceVServer.Controller;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem.FoldingTable {
    internal class ColloidalSilverRecipe : FlaskRecipe {
        public ColloidalSilverRecipe() : base() { }

        //500 : 750 pro Stunde
        public override void onTick(TimeSpan tickLength, FlaskUtensil flask) {
            var silverChlorid = flask.Inventory.getItem<FoldingTableChemical>(i => i.Component == ChemicalType.SilverChlorid);
            var cleanSilverChlorid = flask.Inventory.getItem<FoldingTableChemical>(i => i.Component == ChemicalType.CleanSilverChlorid);

            var sodiumHydroxide = flask.Inventory.getItem<FoldingTableChemical>(i => i.Component == ChemicalType.SodiumHydroxide);


            var timeMult = (float)(tickLength / TimeSpan.FromHours(1));
            timeMult = Math.Min(timeMult, 1);

            var silverMult = (silverChlorid?.Amount ?? 0 + cleanSilverChlorid?.Amount ?? 0) / (500f * timeMult);
            silverMult = (float)Math.Min(silverMult, 1);

            var shMult = sodiumHydroxide.Amount / (750f * timeMult);
            shMult = (float)Math.Min(shMult, 1);

            var mult = Math.Min(shMult, silverMult);

            sodiumHydroxide.Amount -= 750f * timeMult * mult;

            if(sodiumHydroxide.Amount <= 0) {
                sodiumHydroxide.destroy();
            }

            var preferedSilverAmount = (500f * timeMult) * mult;
            var silverAmount = 0f;
            if(silverChlorid != null) {
                var uncleanSilverAmount = Math.Min(silverChlorid.Amount, preferedSilverAmount);
                silverChlorid.Amount -= uncleanSilverAmount;

                //Only 50 Percent yield if not cleaned!
                silverAmount += uncleanSilverAmount * 0.5f;

                if(silverChlorid.Amount <= 0) {
                    silverChlorid.destroy();
                }
            }

            if(silverAmount < preferedSilverAmount && cleanSilverChlorid != null) {
                var cleanSilverAmount = Math.Min(cleanSilverChlorid.Amount, preferedSilverAmount - silverAmount);

                cleanSilverChlorid.Amount -= cleanSilverAmount;
                silverAmount += cleanSilverAmount;

                if(cleanSilverChlorid.Amount <= 0) {
                    cleanSilverChlorid.destroy();
                }
            }

            var already = flask.Inventory.getItem<FoldingTableChemical>(i => i.Component == ChemicalType.ColloidalSilver);
            if(already != null) {
                already.Amount += silverAmount;
            } else {
                var cfg = InventoryController.getConfigItemForType<FoldingTableChemical>();
                flask.Inventory.addItem(new FoldingTableChemical(cfg, ChemicalType.ColloidalSilver, silverAmount, 0));
            }
        }

        protected override List<Predicate<IFoldingTableInFlaskItem>> getAllPredicates() {
            return new List<Predicate<IFoldingTableInFlaskItem>> {
                (i) => i is FoldingTableChemical ch && (ch.Component == ChemicalType.CleanSilverChlorid || ch.Component == ChemicalType.SilverChlorid) && ch.Amount > 0,
                (i) => i is FoldingTableChemical ch && ch.Component == ChemicalType.SodiumHydroxide && ch.Amount > 0,
            };
        }

        protected override Predicate<FlaskUtensil> getFlaskPredicate() {
            return new Predicate<FlaskUtensil>(f => true);
        }
    }
}
