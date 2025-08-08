//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Model.Database;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ChoiceVServer.InventorySystem {
//    class WeedStick : Item {
//        public string WeedType { get => (string)Data["WeedType"]; set { Data["WeedType"] = value; } }

//        public bool WeedDried { get => (bool)Data["WeedDried"]; set { Data["WeedDried"] = value; } }

//        public WeedStick(items item) : base(item) { }

//        public WeedStick(configitems configItem, int quality, int stackAmount, bool dried, string type = "") : base(configItem, quality, stackAmount) {
//            Data = new ExtendedDictionary<string, dynamic>(new Dictionary<string, dynamic>());
//            WeedType = type;
//            WeedDried = dried;

//            if (!dried) {
//                Description = $"Feuchter Ast mit Knospen";
//            } else {
//                Description = $"Getrockneter Ast mit Knospen";
//            }
//        }

//        public override void use(IPlayer player) {

//        }
//    }
//}
