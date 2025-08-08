using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.ProcessMachines.Model {
    public class ProcessMachine {
        public int Id { get; }
        public string Name { get; }
        
        public Dictionary<string, StateValue> VariableStandardValues { get; }
        
        public List<ProcessDisplay> Displays { get; }
        public List<ProcessStep> ProcessSteps { get; }
        public List<ProcessConfigPoint> ConfigPoints { get; }
        public List<ProcessInventory> Inventories { get; }
        public List<ProcessTransformer> ProcessTransformers { get; }
        public List<ProcessSound> ProcessSounds { get; }
        
        public ProcessMachine(int id, string name, Dictionary<string, StateValue> variableStandardValues, List<ProcessDisplay> displays, List<ProcessStep> processSteps, List<ProcessConfigPoint> configPoints, List<ProcessInventory> inventories, List<ProcessTransformer> processTransformers, List<ProcessSound> processSounds) {
            Id = id;
            Name = name;
            
            VariableStandardValues = variableStandardValues;
            
            Displays = displays;
            ProcessSteps = processSteps;
            ConfigPoints = configPoints;
            Inventories = inventories;
            
            ProcessTransformers = processTransformers;
            ProcessSounds = processSounds;
        }
        
        public void setVariable(string name, StateValue value) {
            VariableStandardValues[name] = value;
        }
        
        public void removeVariable(string name) {
            VariableStandardValues.Remove(name);
        }

        public void setInventory(string identifier, ProcessInventory inventory) {
            if(Inventories.Exists(i => i.Identifier == identifier)) {
                Inventories.RemoveAll(i => i.Identifier == identifier);
            }

            Inventories.Add(inventory);
    }
        
        public void removeInventory(string identifier) {
            Inventories.RemoveAll(i => i.Identifier == identifier);
        }
        
        public void addDisplay(ProcessDisplay display) {
            Displays.Add(display);
        }
        
        public void removeDisplay(ProcessDisplay display) {
            Displays.Remove(display);
        }
        
        public void addConfigPoint(ProcessConfigPoint configPoint) {
            ConfigPoints.Add(configPoint);
        }
        
        public void removeConfigPoint(ProcessConfigPoint configPoint) {
            ConfigPoints.Remove(configPoint);
        }
        
        public void addProcessStep(ProcessStep processStep) {
            ProcessSteps.Add(processStep);
        }
        
        public void removeProcessStep(ProcessStep processStep) {
            ProcessSteps.Remove(processStep);
        }
        
        public void addTransformer(ProcessTransformer processTransformer) {
            ProcessTransformers.Add(processTransformer);
        }
        
        public void removeTransformer(ProcessTransformer processTransformer) {
            ProcessTransformers.Remove(processTransformer);
        }

        public void addSound(ProcessSound processSound) {
            ProcessSounds.Add(processSound);
        }

        public void removeSound(ProcessSound processSound) {
            ProcessSounds.Remove(processSound);
        }
        
        public void onInterval(WorldProcessMachine machine, ref Dictionary<Guid, DateTime> lastStepExecution, ref ProcessState virtualState, ref List<WorldProcessMachineError> errorMessages) {
            foreach (var processStep in ProcessSteps) {
                if(!lastStepExecution.ContainsKey(processStep.Id)) {
                    lastStepExecution[processStep.Id] = DateTime.MinValue;    
                }
                
                if(processStep.onInterval(lastStepExecution[processStep.Id], ref virtualState, ref errorMessages)) {
                    lastStepExecution[processStep.Id] = DateTime.Now;
                }
            }

            foreach(var sound in ProcessSounds) {
                if(machine.IsRunning) {
                    sound.onInterval(machine, ref virtualState, ref errorMessages);
                } else {
                    sound.stopSound(machine);
                }
            }
        }

        public Menu getDisplayMenu(IPlayer player, WorldProcessMachine worldProcessMachine, ProcessState virtualState) {
            var menu = new Menu(Name, "Was möchtest du tun?", (p) => worldProcessMachine.ErrorMessages.Clear());

            if(worldProcessMachine.ErrorMessages.Count > 0) {
                var errorMenu = new Menu("Fehler", "Bitte überprüfe die Fehlermeldungen.");
                foreach(var error in worldProcessMachine.ErrorMessages.OrderByDescending(e => e.time)) {
                    errorMenu.addMenuItem(new StaticMenuItem(error.origin, $"Um: {error.time}, {error.description}. Der Exception Text ist: {error.exception.Message}", ""));
                }
                
                menu.addMenuItem(new MenuMenuItem(errorMenu.Name, errorMenu, MenuItemStyle.red));
                return menu;
            }
            
            menu.addMenuItem(getStartStopButton(player, worldProcessMachine, virtualState));
            
            foreach(var display in Displays) {
                menu.addMenuItem(display.getMenuItem(ref virtualState, ref worldProcessMachine.ErrorMessages));                
            }

            var virtuConfigMenu = new VirtualMenu("Konfiguration", () => {
                var subMenu = new Menu("Konfiguration", "Was möchtest du konfigurieren?");
                foreach(var configPoint in ConfigPoints) {
                    subMenu.addMenuItem(configPoint.getMenuElement(player, worldProcessMachine, virtualState, ref worldProcessMachine.ErrorMessages));
                }

                return subMenu;
            });
            menu.addMenuItem(new MenuMenuItem(virtuConfigMenu.Name, virtuConfigMenu, MenuItemStyle.normal, null, true));

            var virtuInvMenu = new VirtualMenu("Ein/Ausgaben", () => {
                var subMenu = new Menu("Ein/Ausgaben", "Was möchtest du einsehen?");
                foreach(var inventory in Inventories) {
                    subMenu.addMenuItem(inventory.getMenuItem(player, worldProcessMachine, ref virtualState, ref worldProcessMachine.ErrorMessages));
                }

                return subMenu;
            });
            menu.addMenuItem(new MenuMenuItem(virtuInvMenu.Name, virtuInvMenu, MenuItemStyle.normal, null, true));
            
            return menu;
        }

        public ProcessDisplay getDisplayForVariable(ref ProcessState virtualState, string variableName) {
            foreach(var display in Displays) {
                if(display.showsVariable(variableName)) {
                    return display;
                }
            }
            
            return null;
        }
        
        public bool getIsInventoryBlocked(ref ProcessState virtualState, string identifier, ref List<WorldProcessMachineError> errorMessages) {
            foreach(var inventory in Inventories) {
                if(inventory.Identifier == identifier) {
                    return !inventory.getIsConditionMet(ref virtualState, ref errorMessages);
                }
            }
            
            return false;
        }
        
        public void onProcessStart(WorldProcessMachine worldProcessMachine, ref ProcessState virtualState) {
            foreach(var transformer in ProcessTransformers) {
                transformer.onProcessStart(worldProcessMachine, ref virtualState);
            }
        }
        
        public void onProcessStop(WorldProcessMachine worldProcessMachine, ref ProcessState virtualState) {
            foreach(var transformer in ProcessTransformers) {
                transformer.onProcessStop(worldProcessMachine, ref virtualState);
            }
        }
        
        private MenuItem getStartStopButton(IPlayer player, WorldProcessMachine machine, ProcessState virtualState) {
            var options = new string[] { "Aus", "An" };
            if(machine.IsRunning) {
                options = ["An", "Aus"];
            }  
            
            return new ListMenuItem("Maschinenstatus", "Startet oder stoppt die Machine", options, "PROCESS_START_STOP", MenuItemStyle.normal, true, true)
                .withData(new Dictionary<string, dynamic> {
                    { "Machine", machine },
                });
        }
    }
}
