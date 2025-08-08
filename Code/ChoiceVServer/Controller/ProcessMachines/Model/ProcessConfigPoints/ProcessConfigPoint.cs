using AltV.Net.Elements.Entities;
using ChoiceVServer.Model.Menu;
using Mathos.Parser;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.ProcessMachines.Model {
    public abstract class ProcessConfigPoint {
        internal string TargetVariableName;
        internal string Name;
        internal string Description;
        internal string Unit;
        internal string BlockCondition;
        
        public ProcessConfigPoint(string targetVariableName, string name, string description, string unit, string blockCondition) {
            TargetVariableName = targetVariableName;
            Name = name;
            Description = description;
            Unit = unit;
            BlockCondition = blockCondition;
        }

        public ProcessConfigPoint(string dbRepresentation) {
            var split = dbRepresentation.Split("###");
            
            TargetVariableName = split[0];
            Name = split[1];
            Description = split[2];
            Unit = split[3];
            BlockCondition = split[4];
        }
        
        protected bool getIfBlockConditionIsMet(ProcessState virtualState, ref List<WorldProcessMachineError> errorMessages) {
            if(BlockCondition == "") {
                return true;
            }
            
            var parser = new MathParser();
            foreach (var key in virtualState.getAllKeys()) {
                parser.LocalVariables[key] = virtualState.getVariable(key).getNumberValue();
            }
            
            try {
                return Math.Abs(parser.Parse(BlockCondition) - 1) < 0.5;
            } catch(Exception e) {
                errorMessages.Add(new WorldProcessMachineError($"Konfigurationspunkt {Name}", $"Fehler beim Überprüfen der Blockierungsbedingung {BlockCondition} für {Name}", e, DateTime.Now));
                throw;
            }
        }

        protected (string evt, Dictionary<string, dynamic> dictionary) getMenuElementInformation(WorldProcessMachine worldProcessMachine) {
            return ("PROCESS_MACHINE_CONFIG_CHANGE", new Dictionary<string, dynamic> {
                { "CONFIG_POINT", this },
                { "WORLD_PROCESS_MACHINE", worldProcessMachine },
            });
        }

        public abstract MenuItem getMenuElement(IPlayer player, WorldProcessMachine worldProcessMachine, ProcessState virtualState, ref List<WorldProcessMachineError> errorMessages);
        
        public abstract void onChangeConfig(IPlayer player, ref ProcessState virtualState, MenuItemCefEvent cefEvent, ref List<WorldProcessMachineError> errorMessages);

        public string getDbRepresentation() {
            return $"{GetType()}###{TargetVariableName}###{Name}###{Description}###{Unit}###{BlockCondition}###{getDbRepresentationStep()}";
        }

        protected abstract string getDbRepresentationStep();
    }
}
