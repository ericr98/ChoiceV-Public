using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Menu;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.ProcessMachines.Model {
    public class ProcessMapConfigPoint : ProcessConfigPoint {
        internal List<(string from, double to)> Values;
        
        public ProcessMapConfigPoint(string targetVariableName, string name, string description, string unit, List<(string from, double to)> values, string blockCondition) : base(targetVariableName, name, description, unit, blockCondition) {
            Values = values;
        }
        
        public ProcessMapConfigPoint(string dbRepresentation) : base(dbRepresentation) {
            var split = dbRepresentation.Split("###");
            
            Values = split[5].FromJson<List<(string from, double to)>>();
        }

        public override MenuItem getMenuElement(IPlayer player, WorldProcessMachine worldProcessMachine, ProcessState virtualState, ref List<WorldProcessMachineError> errorMessages) {
            var currentVal = Values.First(v => Math.Abs(v.to - virtualState.getVariable(TargetVariableName).getNumberValue()) < 0.0001);
            
            if(getIfBlockConditionIsMet(virtualState, ref errorMessages)) {
                var shiftedValues = Values.ShiftLeft(Values.IndexOf(currentVal));
                var (evt, data) = getMenuElementInformation(worldProcessMachine);

                return new ListMenuItem(Name, Description, shiftedValues.Select(v => v.from).ToArray(), evt, MenuItemStyle.normal, true, true).withData(data);
            } else {
                return new StaticMenuItem(Name, Description, currentVal.from);
            }
        }

        public override void onChangeConfig(IPlayer player, ref ProcessState virtualState, MenuItemCefEvent cefEvent, ref List<WorldProcessMachineError> errorMessages) {
            if(getIfBlockConditionIsMet(virtualState, ref errorMessages)) {
                var evt = cefEvent as ListMenuItem.ListMenuItemEvent;

                var newValue = Values.First(v => v.from == evt.currentElement).to;
                virtualState.setNumberVariable(TargetVariableName, newValue);
            }
        }
        
        protected override string getDbRepresentationStep() {
            return Values.ToJson();
        }
    }
}
