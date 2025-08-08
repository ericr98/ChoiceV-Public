//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.Model.Database;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using static ChoiceVServer.Base.Constants;

//namespace ChoiceVServer.InventorySystem {
//    public class Weed : Item {
//        public string WeedType { get => (string)Data["WeedType"]; set { Data["WeedType"] = value; } }

//        public bool WeedDried { get => (bool)Data["WeedDried"]; set { Data["WeedDried"] = value; } }

//        public Weed(items item) : base(item) { }

//        public Weed(configitems configItem, int quality, int stackAmount, bool dried, string type = "") : base(configItem, quality, stackAmount) {
//            Data = new ExtendedDictionary<string, dynamic>(new Dictionary<string, dynamic>());
//            WeedType = type;
//            WeedDried = dried;
//            //var dic = new Dictionary<string, dynamic>();
//            //Data = new ExtendedDictionary<string, dynamic>(dic);
//            //Data.AddItem("TYPE", type);

//            if (!dried) {
//                Description = $"Feuchtes Marihuana";
//            } else {
//                Description = $"Getrocknetes Marihuana";
//            }
//        }

//        public override void use(IPlayer player) {

//        }
//    }
//}
