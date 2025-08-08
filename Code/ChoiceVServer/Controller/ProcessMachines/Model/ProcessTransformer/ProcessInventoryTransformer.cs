using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.ProcessMachines.Model {
    public class ProcessInventoryTransformer : ProcessTransformer {
        internal configitem Item;
        internal string InventoryIdentifier;
        internal List<string> PassVariablesIdentifiers;
        
        public ProcessInventoryTransformer(string variableIdentifier, configitem item, string inventoryIdentifier, List<string> passVariablesIdentifiers) : base(variableIdentifier, false) {
            Item = item;
            InventoryIdentifier = inventoryIdentifier;
            PassVariablesIdentifiers = passVariablesIdentifiers; 
        }
        
        public ProcessInventoryTransformer(string dbRepresentation) : base(dbRepresentation) {
            var split = dbRepresentation.Split("###");
            
            Item = InventoryController.getConfigById(int.Parse(split[1]));
            InventoryIdentifier = split[2];
            if(split.Length > 3) {
                PassVariablesIdentifiers = split[3].FromJson<List<string>>();
            } else {
                PassVariablesIdentifiers = new List<string>();
            }
        }
        
        public override void onProcessStart(WorldProcessMachine machine, ref ProcessState virtualState) {
            virtualState.setNumberVariable(VariableIdentifier, machine.getItemAmountInInventory(InventoryIdentifier, Item.configItemId));
        }
        
        public override void onProcessStop(WorldProcessMachine machine, ref ProcessState virtualState) {
            var flooredAmount = (int)Math.Floor(virtualState.getVariable(VariableIdentifier).getNumberValue());
            var outputAmount = machine.changeInventoryAmount(InventoryIdentifier, Item, flooredAmount, PassVariablesIdentifiers);
            virtualState.setNumberVariable(VariableIdentifier, outputAmount);
        }
        
        protected override string getDbSaveRepresentationStep() {
            return $"{Item.configItemId}###{InventoryIdentifier}###{PassVariablesIdentifiers.ToJson()}";
        }
    }
}
