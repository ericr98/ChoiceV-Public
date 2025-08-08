using ChoiceVServer.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.ProcessMachines.Model {
    public class ProcessStep {
        public Guid Id { get; private set; }
        internal List<ProcessStepTransformation> Transformations;
        internal int DurationSeconds;

        public ProcessStep(Guid id, List<ProcessStepTransformation> transformations, int durationSeconds) {
            Id = id;
            Transformations = transformations;
            DurationSeconds = durationSeconds;
        }

        public ProcessStep(string dbRepresentation) {
            var split = dbRepresentation.Split("###");

            Id = Guid.Parse(split[0]);
            DurationSeconds = int.Parse(split[1]);
            Transformations = split[2].FromJson<List<ProcessStepTransformation>>();
        }

        public bool onInterval(DateTime lastExecution, ref ProcessState virtualState, ref List<WorldProcessMachineError> errorMessages) {
            if(lastExecution.AddSeconds(DurationSeconds) > DateTime.Now) {
                return false;
            }

            foreach(var transformation in Transformations) {
                if(transformation.DoIfMachineOff || virtualState.getVariable("MACHINEPOWERSTATUS").getNumberValue().Equals(1)) {
                    if(transformation.getIfConditionsIsMet(virtualState, ref errorMessages)) {
                        transformation.executeTransformation(ref virtualState, ref errorMessages);
                    }
                }
            }

            return true;
        }

        public void addTransformation(ProcessStepTransformation transformation) {
            Transformations.Add(transformation);
        }

        public void removeTransformation(ProcessStepTransformation transformation) {
            Transformations.Remove(transformation);
        }

        public string getDbRepresentation() {
            return $"{GetType()}###{Id}###{DurationSeconds}###{Transformations.ToJson()}";
        }
    }
}
