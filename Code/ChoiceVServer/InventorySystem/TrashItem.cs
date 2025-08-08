using ChoiceVServer.Controller;
using ChoiceVServer.Model.Database;
using System;

namespace ChoiceVServer.InventorySystem {
    public class TrashItem : Item {
        public TrashController.TrashTypes TrashType;
        
        public TrashItem(item item) : base(item) { }

        public TrashItem(configitem configItem, int amount, int quality) : base(configItem, quality, amount) { }


        public override void processAdditionalInfo(string info) {
            TrashType = Enum.Parse<TrashController.TrashTypes>(info);
        }
    }
}