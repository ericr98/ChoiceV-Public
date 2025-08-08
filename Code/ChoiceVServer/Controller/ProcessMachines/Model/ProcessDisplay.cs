using ChoiceVServer.Model.Menu;
using Mathos.Parser;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ChoiceVServer.Controller.ProcessMachines.Model {
    public class ProcessDisplay {
        public string Name { get; internal set; }
        internal string Description;
        internal string Unit;
        internal string RepresentationFormula;
        internal bool ShowsOnlyVariable;
        
        public ProcessDisplay(string name, string description, string unit, string representationFormula) {
            Name = name;
            Description = description;
            Unit = unit;
            RepresentationFormula = representationFormula;
            
            updateShowOnlyVariable();
        }
        
        public ProcessDisplay(string dbRepresentation) {
            var split = dbRepresentation.Split("###");
            
            Name = split[0];
            Description = split[1];
            Unit = split[2];
            RepresentationFormula = split[3];

            updateShowOnlyVariable();
        }
        
        public void updateShowOnlyVariable() {
            ShowsOnlyVariable = Regex.Match(RepresentationFormula, @"^[a-zA-z]+$").Success;
        }

        public StaticMenuItem getMenuItem(ref ProcessState virtualState, ref List<WorldProcessMachineError> errorMessages) {
            return new StaticMenuItem(Name, Name, Description, getDisplayRepresentation(ref virtualState, ref errorMessages));
        }
        
        public bool showsVariable(string variableName) {
            return RepresentationFormula.Contains(variableName);
        }

        public string getDisplayRepresentation(ref ProcessState virtualState, ref List<WorldProcessMachineError> errorMessages) {
            if(ShowsOnlyVariable) {
                try {
                    return $"{virtualState.getVariable(RepresentationFormula).Value} {Unit}";
                } catch (Exception e) {
                    errorMessages.Add(new WorldProcessMachineError($"Anzeige {Name}", $"Fehler beim Berechnen der Anzeige {Name}. Anzeige wollte nur Variable anzeigen dies war aber nicht möglich", e, DateTime.Now));
                    throw;
                }
            }
            
            var parser = new MathParser();
            
            foreach (var variableName in virtualState.getAllKeys()) {
                parser.LocalVariables[variableName] = virtualState.getVariable(variableName).getNumberValue();
            }
            
            try {
                return $"{parser.Parse(RepresentationFormula)} {Unit}";
            } catch (Exception e) {
                errorMessages.Add(new WorldProcessMachineError($"Anzeige {Name}", $"Fehler beim Berechnen der Anzeige {Name}", e, DateTime.Now));
                throw;
            }
        }
        
        public string getDbRepresentation() {
            return $"{GetType()}###{Name}###{Description}###{Unit}###{RepresentationFormula}";
        }
    }
}
