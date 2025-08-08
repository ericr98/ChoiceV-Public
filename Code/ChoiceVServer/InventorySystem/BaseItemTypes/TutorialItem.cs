using AltV.Net.Elements.Entities;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.InventorySystem {
    public class TutorialItem : Item {
        //Variables are automatically saved into db on value change
        //Try to use primitve data types
        public string Variable { get => (string)Data["Variable"]; set { Data["Variable"] = value; } }

        public TutorialItem(item item) : base(item) {
            //Creation from database on inventory load
        }

        public TutorialItem(configitem configItem, int amount, int quality) : base(configItem, -1, null) {
            //First dynamic creation (e.g. from shop)
        }

        //Will be called when item is loaded (aka: created or loaded from database)
        public override void processAdditionalInfo(string info) {
            base.processAdditionalInfo(info);

            //Differentiate between different items of same codeItem
        }

        public override void use(IPlayer player) {
            base.use(player);

            //Execute Action when player uses item
        }
    }
}
