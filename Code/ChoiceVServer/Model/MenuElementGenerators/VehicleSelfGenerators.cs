using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Character;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller {
    public delegate MenuItem GeneratedVehicleSelfMenuItemDelegate(ChoiceVVehicle vehicle, IPlayer player);
    public delegate Menu GeneratedVehicleSelfMenuDelegate(ChoiceVVehicle vehicle, IPlayer player);

    public interface VehicleSelfMenuElement {
        public List<CharacterType> ShowForTypes { get; set; }

        
        public bool checkShow(ChoiceVVehicle vehicle, IPlayer player);

        public MenuElement getMenuElement(ChoiceVVehicle vehicle, IPlayer player);
    }

    public class UnconditionalVehicleSelfMenuElement : VehicleSelfMenuElement {
        public List<CharacterType> ShowForTypes { get; set; }

        
        private GenericMenuItemGenerator MenuElementGenerator = null;
        private GenericMenuGenerator MenuGenerator = null;

        private string Name;

        public UnconditionalVehicleSelfMenuElement(GenericMenuItemGenerator menuElementGenerator) {
            MenuElementGenerator = menuElementGenerator;
        }

        public UnconditionalVehicleSelfMenuElement(string name, GenericMenuGenerator menuGenerator) {
            MenuGenerator = menuGenerator;
            Name = name;
        }

        public bool checkShow(ChoiceVVehicle vehicle, IPlayer player) {
            return true;
        }

        public MenuElement getMenuElement(ChoiceVVehicle vehicle, IPlayer player) {
            if(MenuElementGenerator != null) {
                return MenuElementGenerator.Invoke();
            } else {
                return new VirtualMenu(Name, MenuGenerator);
            }
        }
    }

    public class UnconditionalVehicleGeneratedSelfMenuElement : VehicleSelfMenuElement {
        public List<CharacterType> ShowForTypes { get; set; }

        
        private GeneratedVehicleSelfMenuItemDelegate MenuElementGenerator;
        private GeneratedVehicleSelfMenuDelegate MenuGenerator;

        private string Name;

        public UnconditionalVehicleGeneratedSelfMenuElement(GeneratedVehicleSelfMenuItemDelegate menuElementGenerator) {
            MenuElementGenerator = menuElementGenerator;
        }

        public UnconditionalVehicleGeneratedSelfMenuElement(string name, GeneratedVehicleSelfMenuDelegate menuGenerator) {
            MenuGenerator = menuGenerator;
            Name = name;
        }


        public bool checkShow(ChoiceVVehicle vehicle, IPlayer player) {
            return true;
        }

        public MenuElement getMenuElement(ChoiceVVehicle vehicle, IPlayer player) {
            if(MenuElementGenerator != null) {
                return MenuElementGenerator.Invoke(vehicle, player);
            } else {
                return new VirtualMenu(Name, () => MenuGenerator.Invoke(vehicle, player));
            }
        }
    }

    public class ConditionalVehicleSelfMenuElement : VehicleSelfMenuElement {
        public List<CharacterType> ShowForTypes { get; set; }
        
        private GenericMenuItemGenerator MenuElementGenerator = null;
        private GenericMenuGenerator MenuGenerator = null;

        private Predicate<ChoiceVVehicle> VehiclePredicate { get; set; }
        private Predicate<IPlayer> PlayerPredicate { get; set; }

        private string Name;

        /// <summary>
        /// A InteractionElement which can be specified to show, only when a conditon is met
        /// If MenuElement in MenuItem, then a custom Data Tag with the target will be added to Data!
        /// BaseType = data["InteractionTargetBaseType"], TargetId = data["InteractionTargetId"]
        /// </summary>
        /// <param name="senderPredicate">The Predicate for the sender. If only target predicate is required set to p => true</param>
        /// <param name="targetPredicate">The Predicate for the target. If only sender predicate is required set to p => true</param>
        public ConditionalVehicleSelfMenuElement(GenericMenuItemGenerator menuElementGenerator, Predicate<ChoiceVVehicle> vehiclePredicate, Predicate<IPlayer> playerPredicate) {
            MenuElementGenerator = menuElementGenerator;
            VehiclePredicate = vehiclePredicate;
            PlayerPredicate = playerPredicate;
        }

        /// <summary>
        /// A InteractionElement which can be specified to show, only when a conditon is met
        /// If MenuElement in MenuItem, then a custom Data Tag with the target will be added to Data!
        /// BaseType = data["InteractionTargetBaseType"], TargetId = data["InteractionTargetId"]
        /// </summary>
        /// <param name="senderPredicate">The Predicate for the sender. If only target predicate is required set to p => true</param>
        /// <param name="targetPredicate">The Predicate for the target. If only sender predicate is required set to p => true</param>
        public ConditionalVehicleSelfMenuElement(string name, GenericMenuGenerator menuGenerator, Predicate<ChoiceVVehicle> vehiclePredicate, Predicate<IPlayer> playerPredicate) {
            MenuGenerator = menuGenerator;
            VehiclePredicate = vehiclePredicate;
            PlayerPredicate = playerPredicate;
            Name = name;
        }

        public bool checkShow(ChoiceVVehicle vehicle, IPlayer player) {
            return VehiclePredicate(vehicle) && PlayerPredicate(player);
        }

        public MenuElement getMenuElement(ChoiceVVehicle vehicle, IPlayer player) {
            if(MenuElementGenerator != null) {
                return MenuElementGenerator.Invoke();
            } else {
                return new VirtualMenu(Name, MenuGenerator);
            }
        }
    }

    public class ConditionalVehicleGeneratedSelfMenuElement : VehicleSelfMenuElement {
        public List<CharacterType> ShowForTypes { get; set; }
        
        private GeneratedVehicleSelfMenuItemDelegate MenuElementGenerator;
        private GeneratedVehicleSelfMenuDelegate MenuGenerator;

        private Predicate<ChoiceVVehicle> VehiclePredicate { get; set; }
        private Predicate<IPlayer> PlayerPredicate { get; set; }

        private string Name;

        /// <summary>
        /// A InteractionElement which can be specified to show, only when a conditon is met
        /// If MenuElement in MenuItem, then a custom Data Tag with the target will be added to Data!
        /// BaseType = data["InteractionTargetBaseType"], TargetId = data["InteractionTargetId"]
        /// </summary>
        /// <param name="senderPredicate">The Predicate for the sender. If only target predicate is required set to p => true</param>
        /// <param name="targetPredicate">The Predicate for the target. If only sender predicate is required set to p => true</param>
        public ConditionalVehicleGeneratedSelfMenuElement(GeneratedVehicleSelfMenuItemDelegate menuElementGenerator, Predicate<ChoiceVVehicle> vehiclePredicate, Predicate<IPlayer> playerPredicate) {
            MenuElementGenerator = menuElementGenerator;
            VehiclePredicate = vehiclePredicate;
            PlayerPredicate = playerPredicate;
        }

        /// <summary>
        /// A InteractionElement which can be specified to show, only when a conditon is met
        /// If MenuElement in MenuItem, then a custom Data Tag with the target will be added to Data!
        /// BaseType = data["InteractionTargetBaseType"], TargetId = data["InteractionTargetId"]
        /// </summary>
        /// <param name="senderPredicate">The Predicate for the sender. If only target predicate is required set to p => true</param>
        /// <param name="targetPredicate">The Predicate for the target. If only sender predicate is required set to p => true</param>
        public ConditionalVehicleGeneratedSelfMenuElement(string name, GeneratedVehicleSelfMenuDelegate menuGenerator, Predicate<ChoiceVVehicle> vehiclePredicate, Predicate<IPlayer> playerPredicate) {
            MenuGenerator = menuGenerator;
            VehiclePredicate = vehiclePredicate;
            PlayerPredicate = playerPredicate;
            Name = name;
        }

        public bool checkShow(ChoiceVVehicle vehicle, IPlayer player) {
            return VehiclePredicate(vehicle) && PlayerPredicate(player);
        }

        public MenuElement getMenuElement(ChoiceVVehicle vehicle, IPlayer player) {
            if(MenuElementGenerator != null) {
                return MenuElementGenerator.Invoke(vehicle, player);
            } else {
                return new VirtualMenu(Name, () => MenuGenerator.Invoke(vehicle, player));
            }
        }
    }
}
