using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.ProcessMachines.Model {
    public delegate void VariableUpdate(string variableName, object newValue);

    public enum ProcessStateValueType {
        Number = 0,
        String = 1,
    }
    
    public class StateValue {
        public ProcessStateValueType Type { get; set; }
        public object Value { get; set; }
        
        public StateValue(ProcessStateValueType type, object value) {
            Type = type;
            Value = value;
        }
        
        
        public double getNumberValue() {
            if(Value is double value) {
                return value;
            } else {
                return double.MinValue;
            }
        }
    }
    
    public class ProcessState {
        private Dictionary<string, StateValue> VirtualState;
        internal VariableUpdate VariableUpdate;

        public ProcessState(Dictionary<string, StateValue> virtualState) {
            VirtualState = virtualState;
        }
        
        public StateValue getVariable(string variableName) {
            if(VirtualState.ContainsKey(variableName)) {
                return VirtualState[variableName];
            } else {
                return null;
            }
        }
        
        public bool hasVariable(string variableName) {
            return VirtualState.ContainsKey(variableName);
        }
        
        public List<string> getAllKeys() {
            return [..VirtualState.Keys];
        }
        
        public void setNumberVariable(string variableName, double value) {
            double oldValue = double.MinValue;
            if(VirtualState.ContainsKey(variableName)) {
                var stateValue = VirtualState[variableName];
                oldValue = stateValue.getNumberValue();
                stateValue.Type = ProcessStateValueType.Number;
                stateValue.Value = value;
            } else {
                VirtualState[variableName] = new StateValue(ProcessStateValueType.Number, value);
            }
            
            if(Math.Abs(VirtualState[variableName].getNumberValue() - oldValue) > 0.00001) {
                VariableUpdate?.Invoke(variableName, value);
            }
        }
        
        public void setStringVariable(string variableName, string value) {
            string oldValue = "";
            if(VirtualState.ContainsKey(variableName)) {
                var stateValue = VirtualState[variableName];
                oldValue = stateValue.Value.ToString();
                stateValue.Type = ProcessStateValueType.Number;
                stateValue.Value = value;
            } else {
                VirtualState[variableName] = new StateValue(ProcessStateValueType.Number, value);
            }
            
            if(VirtualState[variableName].Value.ToString() != oldValue) {
                VariableUpdate?.Invoke(variableName, value);
            }
            
        }
    }
}
