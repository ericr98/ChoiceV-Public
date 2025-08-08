using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem.FoldingTable {
    public abstract class FlaskRecipe {
        public FlaskRecipe() { }

        protected abstract List<Predicate<IFoldingTableInFlaskItem>> getAllPredicates();
        protected abstract Predicate<FlaskUtensil> getFlaskPredicate();

        public bool isValidRecipe(FlaskUtensil flask) {
            return getFlaskPredicate()(flask) && !getAllPredicates().Any(rp => !flask.Inventory.hasItem<IFoldingTableInFlaskItem>(i => rp(i)));
        }

        public abstract void onTick(TimeSpan tickLength, FlaskUtensil flask);
    }
}
