using ChoiceVServer.Model.Database;
using System;

namespace ChoiceVServer.Controller.ProcessMachines.Model {
    public class ProcessResetVariableTransformer : ProcessTransformer {
        internal double ResetValue;

        public ProcessResetVariableTransformer(string variableIdentifier, double value) : base(variableIdentifier, false) {
            ResetValue = value;
        }

        public ProcessResetVariableTransformer(string dbRepresentation) : base(dbRepresentation) {
            var split = dbRepresentation.Split("###");
            
            ResetValue = double.Parse(split[1]);
        }
        
        public override void onProcessStart(WorldProcessMachine machine, ref ProcessState virtualState) {
            
        }
        
        public override void onProcessStop(WorldProcessMachine machine, ref ProcessState virtualState) {
            virtualState.setNumberVariable(VariableIdentifier, ResetValue);
        }
        
        protected override string getDbSaveRepresentationStep() {
            return $"{ResetValue}";
        }
    }
}
