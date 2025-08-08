using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.InventorySystem {
    public class StaticFlower : StaticItem {
        public string FlowerModel;

        public StaticFlower(item item) : base(item) { }

        public StaticFlower(configitem configItem, int amount, int quality) : base(configItem, amount, quality) { }

        public override void processAdditionalInfo(string info) {
            FlowerModel = info;
        }
    }
}
