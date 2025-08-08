//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Controller;
//using ChoiceVServer.Model.Database;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ChoiceVServer.InventorySystem {
//    class CrushedWeed : Item {
//        public string WeedType { get => (string)Data["WeedType"]; set { Data["WeedType"] = value; } }

//        public CrushedWeed(items item) : base(item) { }

//        public CrushedWeed(configitems configItem, int quality, int stackAmount, string type = "") : base(configItem, quality, stackAmount) {
//            Data = new ExtendedDictionary<string, dynamic>(new Dictionary<string, dynamic>());
//            WeedType = type;
//            Description = $"Zerkleinertes {WeedType}";

//        }

//        public override void use(IPlayer player) {
//            WeedController.showWeedMenu(player);
//        }
//    }
//}
