using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.ProcessMachines.Model {
    public class ProcessRangeConfigPoint : ProcessConfigPoint {
        internal List<string> Values;
        
        public ProcessRangeConfigPoint(string targetVariableName, string name, string description, string unit, List<double> values, string blockCondition) : base(targetVariableName, name, description, unit, blockCondition) {
            Values = values.Select(v => $"{v} {unit}").ToList();
        }

        public ProcessRangeConfigPoint(string dbRepresentation) : base(dbRepresentation) {
            var split = dbRepresentation.Split("###");
            
            Values = split[5].FromJson<List<string>>();
        }

        public override MenuItem getMenuElement(IPlayer player, WorldProcessMachine worldProcessMachine, ProcessState virtualState, ref List<WorldProcessMachineError> errorMessages) {
            var currentVal = $"{virtualState.getVariable(TargetVariableName).getNumberValue()} {Unit}";

            if(getIfBlockConditionIsMet(virtualState, ref errorMessages)) {
                var shiftedValues = Values.ShiftLeft(Values.IndexOf(currentVal));
                var (evt, data) = getMenuElementInformation(worldProcessMachine);

                return new ListMenuItem(Name, Description, shiftedValues.ToArray(), evt, MenuItemStyle.normal, true, true).withData(data);
            } else {
                return new StaticMenuItem(Name, Description, currentVal);
            }
        }

        public override void onChangeConfig(IPlayer player, ref ProcessState virtualState, MenuItemCefEvent cefEvent, ref List<WorldProcessMachineError> errorMessages) {
            if(getIfBlockConditionIsMet(virtualState, ref errorMessages)) {
                var evt = cefEvent as ListMenuItem.ListMenuItemEvent;

                var newValueStr = evt.currentElement;
                if(Unit != "") {
                    newValueStr = newValueStr.Replace(Unit, "");
                }
                
                virtualState.setNumberVariable(TargetVariableName, double.Parse(newValueStr));
            }
        }
        protected override string getDbRepresentationStep() {
            return Values.ToJson();
        }
    }
}
