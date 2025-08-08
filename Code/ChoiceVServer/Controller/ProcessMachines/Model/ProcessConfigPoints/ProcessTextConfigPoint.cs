using AltV.Net.Elements.Entities;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.ProcessMachines.Model {
    public class ProcessTextConfigPoint : ProcessConfigPoint {
        public ProcessTextConfigPoint(string targetVariableName, string name, string description, string unit, string blockCondition) : base(targetVariableName, name, description, unit, blockCondition) { }
        
        public ProcessTextConfigPoint(string dbRepresentation) : base(dbRepresentation) { }
        
        public override MenuItem getMenuElement(IPlayer player, WorldProcessMachine worldProcessMachine, ProcessState virtualState, ref List<WorldProcessMachineError> errorMessages) {
            if(getIfBlockConditionIsMet(virtualState, ref errorMessages)) {
                var (evt, data) = getMenuElementInformation(worldProcessMachine);
                
                return new InputMenuItem(Name, Description, "", evt, MenuItemStyle.normal, true)
                    .withEventOnAnyUpdate()
                    .withStartValue(virtualState.getVariable(TargetVariableName).Value.ToString())
                    .withData(data);
            } else {
                return new StaticMenuItem(Name, Description, virtualState.getVariable(TargetVariableName).Value.ToString());
            }
        }

        public override void onChangeConfig(IPlayer player, ref ProcessState virtualState, MenuItemCefEvent cefEvent, ref List<WorldProcessMachineError> errorMessages) {
            if(getIfBlockConditionIsMet(virtualState, ref errorMessages)) {
                var evt = cefEvent as InputMenuItem.InputMenuItemEvent;

                var newValueStr = evt.input;
                
                virtualState.setStringVariable(TargetVariableName, newValueStr);
            }
        }

        protected override string getDbRepresentationStep() {
            return "";
        }
    }
}