using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Menu;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static ChoiceVServer.Model.Menu.ListMenuItem;

namespace ChoiceVServer.Controller.DamageSystem {
    public class CarryController : ChoiceVScript {
        public enum CarryMedium {
            Carry,
            Stretcher,
            Wheelchair,
            None = 255,
        }

        public CarryController() {
            InteractionController.addPlayerInteractionElement(
                new ConditionalGeneratedPlayerInteractionMenuElement(
                    getPlayerCarryInteractMenuItem,
                    sender => sender is IPlayer,
                    target => target is IPlayer tar && (!tar.getBusy() || tar.hasState(Constants.PlayerStates.Dead))
                )
             );

            InteractionController.addVehicleInteractionElement(
                new ConditionalGeneratedPlayerInteractionMenuElement(
                    getPlayerCarryFromVehicleInteractMenu,
                    sender => sender is IPlayer,
                    target => target is ChoiceVVehicle targ && targ.LockState == VehicleLockState.Unlocked
                )
            );

            CharacterController.addSelfMenuElement(
                new ConditionalPlayerSelfMenuElement(
                    (p) => new ClickMenuItem("Mit Tragen aufhören", "Klappe die Trage wieder zusammen", "", "PUT_PLAYER_OFF_CARRY"),
                    player => isPersonCarrier(player)
                ), true
            );

            InteractionController.addVehicleInteractionElement(
                new ConditionalGeneratedPlayerInteractionMenuElement(
                    getPlayerCarryToVehicleInteractMenu,
                    sender => sender is IPlayer && isPersonCarrier(sender as IPlayer),
                    target => target is ChoiceVVehicle targ && targ.LockState == VehicleLockState.Unlocked,
                    true
                )
            );

            EventController.addMenuEvent("PUT_PLAYER_ON_CARRY", putPlayerOnCarry);
            EventController.addMenuEvent("PUT_PLAYER_OFF_CARRY", putPlayerOffCarry);
            EventController.addMenuEvent("PUT_PLAYER_IN_VEHICLE_FROM_CARRY", putPlayerInVehicleFromCarry);

            EventController.PlayerDisconnectedDelegate += onPlayerDisconnected;
            DamageController.BackendPlayerDeathDelegate += onPlayerDeath;
            DamageController.BackendPlayerReviveDelegate += onPlayerRevived;

            EventController.PlayerChangeDimensionDelegate += onPlayerChangeDimension;
        }

        private static string carryMediumToString(CarryMedium medium) {
            switch(medium) {
                case CarryMedium.Carry:
                    return "Tragen";
                case CarryMedium.Stretcher:
                    return "Klapp-Liege";
                case CarryMedium.Wheelchair:
                    return "Rollstuhl";
                default:
                    return "FEHLER";
            }
        }

        private static CarryMedium stringToCarryMedium(string medium) {
            switch(medium) {
                case "Tragen":
                    return CarryMedium.Carry;
                case "Klapp-Liege":
                    return CarryMedium.Stretcher;
                case "Rollstuhl":
                    return CarryMedium.Wheelchair;
                default:
                    return CarryMedium.Carry;
            }
        }

        private MenuItem getPlayerCarryInteractMenuItem(IEntity sender, IEntity target) {
            if(sender is not IPlayer senderPlayer) return null;

            var list = getMediumList(senderPlayer);

            return new ListMenuItem("Person tragen", "Trage eine Person mit einem Tragemedium", list.ToArray(), "PUT_PLAYER_ON_CARRY")
                .needsConfirmation("Person tragen?", "Person wirklich tragen?");
        }



        private MenuItem getPlayerCarryFromVehicleInteractMenu(IEntity sender, IEntity target) {
            var vehicle = target as ChoiceVVehicle;
            var player = sender as IPlayer;

            var passengerList = VehicleController.getVehiclePassengerList(vehicle, player, "Insasse");
            var mediumList = getMediumList(player);

            var menu = new Menu("Insasse tragen", "Welchen Insassen möchtest du tragen?");
            foreach(var passenger in passengerList) {
                menu.addMenuItem(new ListMenuItem($"{passenger} tragen", "Trage eine Person mit einem Tragemedium", mediumList.ToArray(), "PUT_PLAYER_ON_CARRY", MenuItemStyle.normal, true)
                    .withData(new Dictionary<string, dynamic> { { "Vehicle", vehicle }, { "PassengerName", passenger } })
                    .needsConfirmation("Person tragen?", "Person wirklich tragen?"));
            }

            return new MenuMenuItem(menu.Name, menu);
        }

        private List<string> getMediumList(IPlayer sender) {
            var list = new List<string>();
            foreach(var medium in Enum.GetValues<CarryMedium>()) {
                switch(medium) {
                    case CarryMedium.Carry:
                        list.Add(carryMediumToString(medium));
                        break;
                    case CarryMedium.Stretcher:
                        if(sender.getInventory().hasItem<Stretcher>(i => true)) {
                            list.Add(carryMediumToString(medium));
                        }
                        break;
                    case CarryMedium.Wheelchair:

                        break;
                }
            }

            return list;
        }

        private MenuItem getPlayerCarryToVehicleInteractMenu(IEntity sender, IEntity target) {
            var vehicle = target as ChoiceVVehicle;
            var player = sender as IPlayer;

            var passengerList = VehicleController.getVehicleEmptySeatsList(vehicle, "Platz");

            return new ListMenuItem("Person in Fahrzeug legen", "Lege die Person ins Fahrzeug", passengerList.ToArray(), "PUT_PLAYER_IN_VEHICLE_FROM_CARRY", MenuItemStyle.normal, true)
                .withData(new Dictionary<string, dynamic> { { "Vehicle", vehicle } });
        }

        private void onPlayerChangeDimension(IPlayer player, int oldDimension, int newDimension) {
            if(isPersonCarrier(player)) {
                var carryData = (CarryData)player.getData("CARRY_CARRIER");
                var pushed = ChoiceVAPI.FindPlayerByCharId(carryData.CarryId);

                pushed.changeDimension(newDimension);

                InvokeController.AddTimedInvoke("CARRYDimensionChange", (i) => {
                    if(pushed != null) {
                        putPlayerOnCarry(player, pushed, carryData.CarryMedium);
                    }
                }, TimeSpan.FromSeconds(0.5), false);
            }
        }

        private void onPlayerDeath(IPlayer player) {
            deactivateCarry(player);
        }

        private void onPlayerRevived(IPlayer player) {
            if(isOnCarry(player)) {
                var pusherId = (int)player.getData("CARRY_PERSON");
                var pusher = ChoiceVAPI.FindPlayerByCharId(pusherId);

                pusher.sendNotification(Constants.NotifactionTypes.Warning, $"Die Person auf der Trage kommt wieder zu Bewusstsein!", "Spieler wiederbelebt", Constants.NotifactionImages.Bone);

            }
        }

        private void onPlayerDisconnected(IPlayer player, string reason) {
            deactivateCarry(player);
        }

        private static void deactivateCarry(IPlayer player) {
            if(player.hasData("CARRY_CARRIER")) {
                var targetData = (CarryData)player.getData("CARRY_CARRIER");
                var targetId = targetData.CarryId;
                if (targetData.CarryType == "PLAYER") {
                    var target = ChoiceVAPI.FindPlayerByCharId(targetId);
                    if(target != null) {
                        removeCarryState(player, target, false);
                    }
                } else {
                    putObjectOffCarrier(player, targetData.callback);
                }

            } else if(player.hasData("CARRY_PERSON")) {
                var targetId = (int)player.getData("CARRY_PERSON");
                var target = ChoiceVAPI.FindPlayerByCharId(targetId);
                if(target != null) {
                    removeCarryState(target, player, false);
                }
            }
        }

        private static bool hasDeadPlayerInside(ChoiceVVehicle vehicle) {
            return ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.hasState(Constants.PlayerStates.Dead) && p.IsInVehicle && p.Vehicle == vehicle) != null;
        }

        public static bool isPersonCarrier(IPlayer player) {
            return player.hasData("CARRY_CARRIER");
        }

        public static IPlayer getCarriedPlayer(IPlayer player) {
            if(player.hasData("CARRY_CARRIER")) {
                var data = (CarryData)player.getData("CARRY_CARRIER");
                return ChoiceVAPI.FindPlayerByCharId(data.CarryId);
            }

            return null;
        }

        public static bool isOnCarry(IPlayer player) {
            return player.hasData("CARRY_PERSON");
        }

        public static CarryData getPersonCarryData(IPlayer player) {
            return (CarryData)player.getData("CARRY_CARRIER");
        }

        public static void reapplyAnimation(IPlayer player) {
            var stretchAnim = AnimationController.getAnimationByName("PUSH_CARRY");
            player.playAnimation(stretchAnim, null, false);
        }

        private bool putPlayerOnCarry(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as ListMenuItemEvent;

            int targetId;
            if(evt.action == "changed" && data.ContainsKey("Vehicle")) {
                var vehicle = data["Vehicle"];
                var passengerName = (string)data["PassengerName"];
                var seatId = int.Parse(Regex.Match(passengerName, "(\\d+)").Value) - 1;

                VehicleController.showPlayerPassengerPosition(player, vehicle, seatId);
                return true;
            }
            
            if(data.ContainsKey("Vehicle")) {
                var vehicle = (ChoiceVVehicle)data["Vehicle"];
                var passengerName = (string)data["PassengerName"];
                var seatId = int.Parse(Regex.Match(passengerName, "(\\d+)").Value) - 1;

                var passenger = vehicle.PassengerList[seatId];
                targetId = passenger.getCharacterId();
            } else {
                targetId = (int)data["InteractionTargetId"];
            }

            if(data.ContainsKey("CONFIRM_SENDER")) {
                player = (IPlayer)data["CONFIRM_SENDER"];
            }

            var target = ChoiceVAPI.FindPlayerByCharId(targetId);

            if(target != null && !target.hasState(Constants.PlayerStates.BeingCarried) && (target.hasState(Constants.PlayerStates.Dead)) || data.ContainsKey("CONFIRMATED")) {
                putPlayerOnCarry(player, target, stringToCarryMedium(evt.currentElement));
            } else {
                data["CONFIRMATED"] = true;
                data["CONFIRM_SENDER"] = player;

                var menu = MenuController.getConfirmationMenu("Auf Trage legen?", "Wirklich auf Trage legen?", "PUT_PLAYER_ON_CARRY", data);
                target.showMenu(menu);
            }

            return true;
        }

        private class AttachMetaData {
            public uint carrierId;
            public uint carriedId;

            public int status;

            public string bone;

            public float offsetX;
            public float offsetY;
            public float offsetZ;

            public float rotX;
            public float rotY;
            public float rotZ;

            public AttachMetaData(uint carrierId, uint carriedId, int status, string bone, float offsetX, float offsetY, float offsetZ, float rotX, float rotY, float rotZ) {
                this.carrierId = carrierId;
                this.carriedId = carriedId;

                this.status = status;

                this.bone = bone;
                this.offsetX = offsetX;
                this.offsetY = offsetY;
                this.offsetZ = offsetZ;
                this.rotX = rotX;
                this.rotY = rotY;
                this.rotZ = rotZ;
            }
        }

        public record CarryData(int CarryId, string CarryType, CarryMedium CarryMedium, Action<Action> callback);

        private static void putPlayerOnCarry(IPlayer player, IPlayer target, CarryMedium medium) {
            player.setData("CARRY_CARRIER", new CarryData(target.getCharacterId(), "PLAYER", medium, null));
            target.setData("CARRY_PERSON", player.getCharacterId());

            player.addState(Constants.PlayerStates.IsCarrying);
            target.addState(Constants.PlayerStates.BeingCarried);
            target.stopAnimation();
            target.emitClientEvent("WEAPON_PULLOUT_DISABLED", true);

            target.Detach();

            var wasSitting = false;
            if(SittingController.isPlayerSittingOnChair(target)) {
                SittingController.removeFromSitting(target);
                wasSitting = true;
            }

            if(target.IsInVehicle || wasSitting) {
                target.Position = target.Position;
                Thread.Sleep(500);
            }

            if(medium == CarryMedium.Stretcher) {
                var layAnim = AnimationController.getAnimationByName("LAY_STRAIGHT");
                target.forceAnimation(layAnim);

                var stretchAnim = AnimationController.getAnimationByName("PUSH_CARRY");
                player.playAnimation(stretchAnim, null, false);

                target.AttachToEntity(player, 91, 0, new Position(0, 1.5f, 1.0f), new DegreeRotation(0, 0, -5), false, false);
                target.emitClientEvent("PUT_ON_CARRY", player, target);

                player.toggleCannotAttack(true, "CARRY");

                SittingController.removeFromSitting(target);
            } else if (medium == CarryMedium.Carry) {
                var carrierAnim = new Animation("missfinale_c2mcs_1", "fin_c2_mcs_1_camman", TimeSpan.FromDays(1), 49, 0);
                var carriedAnim = new Animation("nm", "firemans_carry", TimeSpan.FromDays(1), 33, 0);

                player.forceAnimation(carrierAnim);
                target.forceAnimation(carriedAnim);

                target.AttachToEntity(player, "SKEL_ROOT", "SKEL_ROOT", new Position(0.27f, 0.05f, 0.5f), new DegreeRotation(0, 0, 0), true, false);
            }
        }

        public static void putObjectOnCarry(IPlayer player, Animation animation, string model, Position attachOffset, DegreeRotation rotationOffset, int bone, Action<Action> callback) {
            var obj = ObjectController.createObject(model, player, attachOffset, rotationOffset, bone);
            player.setData("CARRY_CARRIER", new CarryData(obj.Id, "OBJECT", CarryMedium.None, callback));

            player.addState(Constants.PlayerStates.IsCarrying);
            player.forceAnimation(animation);
        }

        private bool putPlayerOffCarry(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var targetData = (CarryData)player.getData("CARRY_CARRIER");
            var targetId = targetData.CarryId;

            if(targetData.CarryType == "PLAYER") {
                var target = ChoiceVAPI.FindPlayerByCharId(targetId);
                if(target != null) {
                    removeCarryState(player, target, false);
                }
            } else {
                putObjectOffCarrier(player, targetData.callback);
            }

            return true;
        }

        public static void removeCarryState(IPlayer player, IPlayer target = null, bool noAnim = false) {
            player.resetData("CARRY_CARRIER");
            target?.resetData("CARRY_PERSON");
            target?.emitClientEvent("WEAPON_PULLOUT_DISABLED", false);

            target?.removeState(Constants.PlayerStates.BeingCarried);
            player.removeState(Constants.PlayerStates.IsCarrying);

            player.toggleCannotAttack(false, "CARRY");

            player.stopAnimation();
            target?.emitClientEvent("PUT_OFF_CARRY", player, target);
            target?.Detach();

            if(!noAnim && target != null && target.hasState(Constants.PlayerStates.Dead)) {
                target?.forceAnimation("dead", "dead_b", -1, 1);
            } else {
                target?.stopAnimation();
            }
        }

        public static void putObjectOffCarrier(IPlayer player, Action<Action> callback) {
            callback?.Invoke(() => {
                var targetData = (CarryData)player.getData("CARRY_CARRIER");
                var targetId = targetData.CarryId;

                var obj = ObjectController.getObjectById(targetId);
                if(obj != null) {
                    ObjectController.deleteObject(obj);
                }

                removeCarryState(player);
            });
        }

        private bool putPlayerInVehicleFromCarry(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var targetVId = (int)data["InteractionTargetId"];
            var targetVeh = ChoiceVAPI.FindVehicleById(targetVId);

            var evt = menuItemCefEvent as ListMenuItemEvent;
            var seatId = byte.Parse(Regex.Match(evt.currentElement, "(\\d+)").Value) - 1;
            if(evt.action == "changed" && data.ContainsKey("Vehicle")) {
                var vehicle = data["Vehicle"];

                VehicleController.showPlayerSeatPosition(player, vehicle, seatId);
                return true;
            }

            if(targetVeh != null) {
                var targetData = (CarryData)player.getData("CARRY_CARRIER");
                var targetPId = targetData.CarryId;
                if(targetData.CarryType == "PLAYER") {
                    var targetPlayer = ChoiceVAPI.FindPlayerByCharId(targetPId);
                    if(targetPlayer != null) {

                        if(!setPlayerInCarryVehicle(player, targetPlayer, targetVeh, (byte)(seatId + 1))) {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
        
        public static bool setPlayerInCarryVehicle(IPlayer player, IPlayer targetPlayer, ChoiceVVehicle targetVeh, byte seatId) {
            removeCarryState(player, targetPlayer, true);

            if(!targetVeh.PassengerList.ContainsKey(seatId)) {
                targetPlayer.SetIntoVehicle(targetVeh, seatId);
            } else {
                player.sendBlockNotification("Der gewählte Platz ist belegt!", "Verfügbare Plätze belegt!", Constants.NotifactionImages.Bone);
                return false;
            }

            if(targetPlayer.hasState(Constants.PlayerStates.Dead)) {
                targetPlayer.playAnimation("veh@mower@base", "die", -1, 18, null, 1);
            }

            return true;
        }
    }
}
