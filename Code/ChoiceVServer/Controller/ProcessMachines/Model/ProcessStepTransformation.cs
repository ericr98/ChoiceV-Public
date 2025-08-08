using Mathos.Parser;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.ProcessMachines.Model {
    public class ProcessStepTransformation {
        public string TransformationFormula;
        public string ConditionFormula;
        public string OutputVariabe;
        public bool DoIfMachineOff;
        
        public ProcessStepTransformation(string transformationFormula, string conditionFormula, string outputVariabe, bool doIfMachineOff) {
            TransformationFormula = transformationFormula;
            ConditionFormula = conditionFormula;
            OutputVariabe = outputVariabe;
            DoIfMachineOff = doIfMachineOff;
        }

        public bool getIfConditionsIsMet(ProcessState virtualState, ref List<WorldProcessMachineError> errorMessages) {
            if(ConditionFormula == "") {
                return true;
            }
            
            var parser = new MathParser();
            foreach (var key in virtualState.getAllKeys()) {
                parser.LocalVariables[key] = virtualState.getVariable(key).getNumberValue();
            }
            
            try {
                return Math.Abs(parser.Parse(ConditionFormula) - 1) < 0.5;
            } catch(Exception e) {
                errorMessages.Add(new WorldProcessMachineError($"Transformation {OutputVariabe}", $"Fehler beim Überprüfen der Bedingung {ConditionFormula} für {OutputVariabe}", e, DateTime.Now));
                throw;
            }
        }
        
        public void executeTransformation(ref ProcessState virtualState, ref List<WorldProcessMachineError> errorMessages) {
            var parser = new MathParser();
            foreach (var key in virtualState.getAllKeys()) {
                parser.LocalVariables[key] = virtualState.getVariable(key).getNumberValue();
            }
            
            try {
                virtualState.setNumberVariable(OutputVariabe, parser.Parse(TransformationFormula));
            } catch(Exception e) {
                errorMessages.Add(new WorldProcessMachineError($"Transformation {OutputVariabe}", $"Fehler beim Ausführen der Transformation {TransformationFormula} für {OutputVariabe}", e, DateTime.Now));
                throw;
            }
        }
    }
}
