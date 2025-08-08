using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.ProcessMachines.Model;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Controller.SoundSystem.SoundController;

namespace ChoiceVServer.Controller.ProcessMachines {
    public enum WorldProcessMachineType {
        Temporary = 0,
        Static = 1,
    }

    public record WorldProcessMachineError(string origin, string description, Exception exception, DateTime time);
    public class WorldProcessMachine : IDisposable {
        public int Id;
        public string Name => ProcessMachine.Name;
        public WorldProcessMachineType Type;
        public bool IsRunning => VirtualState.hasVariable("MACHINEPOWERSTATUS") && VirtualState.getVariable("MACHINEPOWERSTATUS").getNumberValue().Equals(1);
        
        private ProcessMachine ProcessMachine;
        internal ProcessState VirtualState;
        private Dictionary<Guid, DateTime> LastStepExecutions;
        private Dictionary<string, string> ProcessSounds; 
        private Dictionary<string, Inventory> Inventories;

        private CollisionShape ColShape;
        private CollisionShape LoadingZone;
        public Position Position{ get => ColShape.Position; }

        internal List<WorldProcessMachineError> ErrorMessages = new List<WorldProcessMachineError>(); 

        public WorldProcessMachine(int id, WorldProcessMachineType type, ProcessMachine processMachine, CollisionShape shape, CollisionShape loadingZone = null) {
            Id = id;
            Type = type;
            ProcessMachine = processMachine;
            VirtualState = new ProcessState(new Dictionary<string, StateValue>(processMachine.VariableStandardValues));
            VirtualState.VariableUpdate += onProcessStateUpdate;

            shape.OnCollisionShapeInteraction += onInteraction;
            ColShape = shape;
            LoadingZone = loadingZone;
            
            Inventories = ProcessMachine.Inventories.ToDictionary(i => i.Identifier, i => InventoryController.createInventory(Id, i.StandardSize, InventoryTypes.WorldProcessMachine));
            
            LastStepExecutions = new Dictionary<Guid, DateTime>();
            ProcessSounds = new Dictionary<string, string>();
            
            VirtualState.setNumberVariable("MACHINEPOWERSTATUS", 0);
        }

        public WorldProcessMachine(worldprocessmachine dbMachine) {
            Id = dbMachine.id;
            Type = (WorldProcessMachineType) dbMachine.type;
            ProcessMachine = ProcessMachineController.getProcessMachine(dbMachine.processMachineId);
            VirtualState = new ProcessState(new Dictionary<string, StateValue>(ProcessMachine.VariableStandardValues));
            foreach(var data in dbMachine.worldprocessmachinesdata) {
                if((ProcessStateValueType)data.type == ProcessStateValueType.String) {
                    VirtualState.setStringVariable(data.key, data.value);
                } else if((ProcessStateValueType)data.type == ProcessStateValueType.Number) {
                    VirtualState.setNumberVariable(data.key, double.Parse(data.value));
                }
            }

            VirtualState.VariableUpdate += onProcessStateUpdate;

            ColShape = CollisionShape.Create(dbMachine.colShape);
            ColShape.OnCollisionShapeInteraction += onInteraction;
           
            if(dbMachine.loadingZone != null) {
                LoadingZone = CollisionShape.Create(dbMachine.loadingZone);
            }
            
            Inventories = [];
            foreach(var inventory in dbMachine.worldprocessmachinesinventories) {
                Inventories[inventory.identifier] = InventoryController.loadInventory(inventory.inventoryId);
            }

            LastStepExecutions = [];
            ProcessSounds = [];
        }

        public worldprocessmachine getDbRepresentation() {
            return new worldprocessmachine() {
                type = (int) Type,
                processMachineId = ProcessMachine.Id,
                colShape = ColShape.toShortSave(),
                loadingZone = LoadingZone?.toShortSave(),
                worldprocessmachinesdata = VirtualState.getAllKeys().Select(k => new worldprocessmachinesdatum() {
                    key = k,
                    type = (int)VirtualState.getVariable(k).Type,
                    value = VirtualState.getVariable(k).Value.ToString(),
                }).ToList(),

                worldprocessmachinesinventories = Inventories.Select(i => new worldprocessmachinesinventory() {
                    identifier = i.Key,
                    inventoryId = i.Value.Id,
                }).ToList(),
            };
        }

        public void start(IPlayer player) {
            if(IsRunning) {
                return;
            }
            
            VirtualState.setNumberVariable("MACHINEPOWERSTATUS", 1);
            ProcessMachine.onProcessStart(this, ref VirtualState);
            
            Logger.logDebug(LogCategory.Player, LogActionType.Event, player, $"WorldProcessMachine {Id} started");
        }
        
        public void stop(IPlayer player) {
            if(!IsRunning) {
                return;
            }
            
            VirtualState.setNumberVariable("MACHINEPOWERSTATUS", 0);
            ProcessMachine.onProcessStop(this, ref VirtualState);

            Logger.logDebug(LogCategory.Player, LogActionType.Event, player, $"WorldProcessMachine {Id} stopped");
        }
        
        public void openInventory(IPlayer player, string identifier) {
            if(!ProcessMachine.getIsInventoryBlocked(ref VirtualState, identifier, ref ErrorMessages)) {
                var vehicle = LoadingZone?.getAllEntities().FirstOrDefault(e => e is ChoiceVVehicle veh && veh.LockState == VehicleLockState.Unlocked && veh.hasPlayerAccess(player)) as ChoiceVVehicle;

                var inventory = Inventories[identifier];
                if(vehicle != null) {
                    InventoryController.showMoveInventory(player, vehicle.Inventory, inventory);
                    player.sendNotification(Constants.NotifactionTypes.Info, "Ein zugängliches Fahrzeug befand sich in der Ladezone. Das Inventar des Fahrzeugs wurde geöffnet.", "Fahrzeug-Inventar geöffnet", Constants.NotifactionImages.Package);
                } else {
                    InventoryController.showMoveInventory(player, player.getInventory(), inventory);
                }
            }
        }
        
        public void onInterval() {
            ProcessMachine.onInterval(this, ref LastStepExecutions, ref VirtualState, ref ErrorMessages);
        }
        
        private Menu getDisplayMenu(IPlayer player) {
            return ProcessMachine.getDisplayMenu(player, this, VirtualState);
        }
        
        internal bool onInteraction(IPlayer player) {
            Logger.logDebug(LogCategory.Player, LogActionType.Event, player, $"WorldProcessMachine {Id} interaction");
            player.showUpdatingMenu(getDisplayMenu(player), $"WORLD_PROCESS_{Id}");
            
            return true;
        }
        
        public double getItemAmountInInventory(string identifier, int configId) {
            if(Inventories.ContainsKey(identifier)) {
                return Inventories[identifier].getItems(i => i.ConfigId == configId).Select(i => i.StackAmount ?? 1).Sum();
            } else {
                return 0;
            }
        }
        
        public int changeInventoryAmount(string identifier, configitem cfgItem, int amount, List<string> passVariablesIdentifiers) {
            Logger.logDebug(LogCategory.System, LogActionType.Event, $"WorldProcessMachine: {Id}, Change inventory amount for {identifier} with {cfgItem.configItemId} by {amount}");
            
            if(Inventories.ContainsKey(identifier)) {
                var items = Inventories[identifier].getItems(i => i.ConfigId == cfgItem.configItemId);
                
                var changeAmount = amount - items.Select(i => i.StackAmount ?? 1).Sum();
                if(changeAmount > 0) { 
                    var createInformationDictionary = new Dictionary<string, dynamic>();
                    foreach(var key in VirtualState.getAllKeys().Where(passVariablesIdentifiers.Contains)) {
                        createInformationDictionary[key] = VirtualState.getVariable(key).Value; 
                    }
                    var newItems = InventoryController.createItems(cfgItem, changeAmount, -1, createInformationDictionary);

                    if(!Inventories[identifier].addItems(newItems)) {
                        return Inventories[identifier].getItems(i => i.ConfigId == cfgItem.configItemId).Select(i => i.StackAmount ?? 1).Sum();
                    } else {
                        return amount;
                    }
                } else {
                    var changeTemp = Math.Abs(changeAmount);
                    foreach(var item in items) {
                        if(changeTemp == 0) {
                            break;
                        }
                        
                        if(!item.CanBeStacked) {
                            if(Inventories[identifier].removeItem(item)) {
                                changeTemp--;
                            }
                        } else {
                            if(item.StackAmount >= changeTemp) {
                                if(Inventories[identifier].removeItem(item, changeTemp)) {
                                    changeTemp = 0;
                                }
                            } else {
                                if(Inventories[identifier].removeItem(item, item.StackAmount ?? 1)) {
                                    changeTemp -= item.StackAmount ?? 1;
                                }
                            }
                        }
                    }

                    return amount - changeTemp;
                }
            } else {
                return 0;
            }
        }
        
        private void onProcessStateUpdate(string variableName, object newValue) {
            var display = ProcessMachine.getDisplayForVariable(ref VirtualState, variableName);

            if(display != null) {
                MenuController.updateMenu($"WORLD_PROCESS_{Id}", display.Name, display.getDisplayRepresentation(ref VirtualState, ref ErrorMessages));
            }

            ProcessMachineController.updateWorldProcessMachineDbValue(this, variableName, newValue);
        }

        public void playSound(string processSoundIdentifier, Sounds sound, string extension) {
            if(ProcessSounds.ContainsKey(processSoundIdentifier)) {
                return;
            }
            
            var soundIdentifier = SoundController.playSoundAtCoords(Position, 10, sound, 1, extension, true);

            ProcessSounds[processSoundIdentifier] = soundIdentifier;
        }

        public void stopSound(string processSoundIdentifier) {
            if(ProcessSounds.TryGetValue(processSoundIdentifier, out string value)) {
                SoundController.stopSound(value);
                ProcessSounds.Remove(processSoundIdentifier);
            }
        }
        
        public void Dispose() {
            VirtualState.VariableUpdate -= onProcessStateUpdate;
            ProcessMachine = null;
            VirtualState = null;
            LastStepExecutions = null;
            Inventories.ForEach(i => InventoryController.destroyInventory(i.Value));
            ColShape.Dispose();
        }
    }
}
