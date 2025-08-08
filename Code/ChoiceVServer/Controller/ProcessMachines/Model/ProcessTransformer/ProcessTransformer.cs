using System.Collections.Generic;

namespace ChoiceVServer.Controller.ProcessMachines.Model {
    public abstract class ProcessTransformer {
        internal string VariableIdentifier;
        internal bool UpdateOnInterval;
        
        public ProcessTransformer(string variableIdentifier, bool updateOnInterval) {
            VariableIdentifier = variableIdentifier;
            UpdateOnInterval = updateOnInterval;
        }
        
        public ProcessTransformer(string dbRepresentation) {
            var split = dbRepresentation.Split("###");
            
            VariableIdentifier = split[0];
        }
        
        public abstract void onProcessStart(WorldProcessMachine machine, ref ProcessState virtualState);
        public abstract void onProcessStop(WorldProcessMachine machine, ref ProcessState virtualState);
        
        public string getDbRepresentation() {
            return $"{GetType()}###{VariableIdentifier}###{getDbSaveRepresentationStep()}";
        }
        
        protected abstract string getDbSaveRepresentationStep();
    }
}
