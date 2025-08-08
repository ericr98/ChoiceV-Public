using AltV.Net.Elements.Entities;
using ChoiceVServer.Model.Menu;
using Mathos.Parser;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.ProcessMachines.Model {
    public class ProcessInventory {
        public string Name { get; internal set; }
        public string Description { get; internal set; }
        public string Identifier { get; internal set; }
        public int StandardSize { get; internal set;}
        public string ConditionFormula { get; internal set;}
        
        public ProcessInventory(string name, string description, int standardSize, string identifier, string conditionFormula) {
            Name = name;
            Description = description;
            StandardSize = standardSize;
            Identifier = identifier;
            ConditionFormula = conditionFormula;
        }
        
        public ProcessInventory(string dbRepresentation) {
            var split = dbRepresentation.Split("###");
            
            Name = split[0];
            Description = split[1];
            Identifier = split[2];
            ConditionFormula = split[3];
            StandardSize = int.Parse(split[4]);
        }

        public bool getIsConditionMet(ref ProcessState virtualState, ref List<WorldProcessMachineError> errorMessages) {
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
                errorMessages.Add(new WorldProcessMachineError($"Prozessinventar {Identifier}", $"Fehler beim Erstellen des Menüpunkts für {Identifier} aufgrund der Condition {ConditionFormula}", e, DateTime.Now));
                throw;
            }   
        }

        public MenuItem getMenuItem(IPlayer player, WorldProcessMachine worldProcessMachine, ref ProcessState virtualState, ref List<WorldProcessMachineError> errorMessages) {
            if(!worldProcessMachine.IsRunning && (ConditionFormula == "" || getIsConditionMet(ref virtualState, ref errorMessages))) {
                return new ClickMenuItem(Name, Description, "Öffnen", "OPEN_PROCESS_INVENTORY")
                    .withData(new Dictionary<string, dynamic> {
                        { "Identifier", Identifier },
                        { "ProcessMachine", worldProcessMachine },
                    });
            } else {
                return new StaticMenuItem(Name, Description, "Aktuell blockiert");
            }  
        }

        public string getDbRepresentation() {
            return $"{GetType()}###{Name}###{Description}###{Identifier}###{ConditionFormula}###{StandardSize}";
        }
    }
}
