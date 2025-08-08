using System;
using System.Collections.Generic;
using ChoiceVServer.Controller.SoundSystem;
using Mathos.Parser;
using static ChoiceVServer.Controller.SoundSystem.SoundController;

namespace ChoiceVServer.Controller.ProcessMachines.Model;

public class ProcessSound {
    public string Identifier;
    public Sounds Sound;
    public string FileExtension;
    public string ConditionFormula;

    public ProcessSound(string identifier, Sounds sound, string fileExtension, string conditionFormula) {
        Identifier = identifier;
        Sound = sound;
        FileExtension = fileExtension;
        ConditionFormula = conditionFormula;
    }

    public ProcessSound(string dbRepresentation) {
        var parts = dbRepresentation.Split("###");
        Identifier = parts[0];
        Sound = (Sounds)Enum.Parse(typeof(Sounds), parts[1]);
        FileExtension = parts[2];
        ConditionFormula = parts[3];
    }

    public bool isConditionMet(ref ProcessState processState, ref List<WorldProcessMachineError> errorMessages) {
        if(ConditionFormula == "") {
            return true;
        }

        var parser = new MathParser();

        foreach(var key in processState.getAllKeys()) {
            parser.LocalVariables[key] = processState.getVariable(key).getNumberValue();
        }

        try {
            return parser.Parse(ConditionFormula) == 1;
        } catch(Exception e) {
            errorMessages.Add(new WorldProcessMachineError($"Prozesssound {Identifier}", $"Prozesssound {Identifier} hatte einen Fehler in der Condition Formel {ConditionFormula}", e, DateTime.Now));
            return false;
        }
    }

    public void onInterval(WorldProcessMachine machine, ref ProcessState processState, ref List<WorldProcessMachineError> errorMessages) {
        if(isConditionMet(ref processState, ref errorMessages)) {
            playSound(machine);
        } else {
            stopSound(machine);
        }
    }

    public void playSound(WorldProcessMachine machine) {
        machine.playSound(Identifier, Sound, FileExtension);
    }

    public void stopSound(WorldProcessMachine machine) {
        machine.stopSound(Identifier);
    }

    public string getDbRepresentation() {
        return $"{GetType()}###{Identifier}###{Sound}###{FileExtension}###{ConditionFormula}";
    }
}
