using AltV.Net.Elements.Entities;
using ChoiceVServer.Controller.Crime_Stuff;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.InventorySystem {
    public class DrugItem : FoodItem {
        public DrugItem(item item) : base(item) { }
        
        public DrugItem(configitem configItem, int amount, int quality) : base(configItem, amount, quality) { }


        public override void use(IPlayer player) {
            base.use(player);
            //TODO Trigger Drug effect
        }
    }
}