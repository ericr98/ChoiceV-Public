//using ChoiceVServer.Model.Database;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using ChoiceVServer.Controller;
//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Model.Menu;
//using ChoiceVServer.Base;

//namespace ChoiceVServer.InventorySystem {
//    public class WeaponCase : Item {

//        public Inventory CaseContent { get => InventoryController.loadInventory((int)Data["InventoryId"]); set => Data["InventoryId"] = value.Id; }

//        public WeaponCase(items item) : base(item) {
//            var statement = new InventoryAddBlockStatement(this, i => !(i is WeaponPartItem));
//            CaseContent.BlockStatements.Add(statement);
//        }

//        public WeaponCase(configitems configItem) : base(configItem) {
//            CaseContent = InventoryController.createInventory(0, 10, InventoryTypes.WeaponCase);
//            var statement = new InventoryAddBlockStatement(this, i => !(i is WeaponPartItem));
//            CaseContent.BlockStatements.Add(statement);
//        }

//        public override void use(IPlayer player) {
//            base.use(player);
//            var inv = CaseContent;
//            InventoryController.showMoveInventory(player, player.getInventory(), inv);
//        }
//    }
//}
