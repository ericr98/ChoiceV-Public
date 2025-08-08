using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public class HandCuffController : ChoiceVScript {
        public HandCuffController() {
            InteractionController.addPlayerInteractionElement(new ConditionalPlayerInteractionMenuElement(
                () => new ClickMenuItem("Spieler fesseln", "Benutze einen Kabelbinder um den Spieler zu fesseln", "", "ON_HANDCUFF_PLAYER"),
                sender => sender is IPlayer && hasCableTies(sender as IPlayer),
                target => target is IPlayer && hasHandsUp(target as IPlayer)
            ));

            InteractionController.addPlayerInteractionElement(new ConditionalPlayerInteractionMenuElement(
                () => new ClickMenuItem("Spieler entfesseln", "Befreie den Spieler von seinen Fesseln", "", "ON_REMOVE_HANDCUFF_PLAYER"),
                sender => sender is IPlayer,
                target => target is IPlayer && isHandCuffed(target as IPlayer)
            ));

            InteractionController.addPlayerInteractionElement(new ConditionalPlayerInteractionMenuElement(
                () => new ClickMenuItem("Spieler durchsuchen", "Durchsuche das Inventar des Spielers", "", "ON_SEARCH_PLAYER"),
                sender => sender is IPlayer,
                target => target is IPlayer && (hasHandsUp(target as IPlayer) || isHandCuffed(target as IPlayer))
            ));

            InteractionController.addPlayerInteractionElement(new ConditionalPlayerInteractionMenuElement(
                () => new ClickMenuItem("Spieler hinknien/aufsetzen", "Zwinge den Spieler sich hinzuknien/aufzustehen", "", "ON_KNEEL_PLAYER"),
                sender => sender is IPlayer,
                target => target is IPlayer && isHandCuffed(target as IPlayer)
            ));

            CharacterController.addPlayerConnectDataSetCallback("HANDCUFFED", onConnectDataHandcuff);


            EventController.addKeyEvent("HANDS_UP", ConsoleKey.H, "Ergeben", onPlayerHandsUp, true);

            EventController.addMenuEvent("ON_HANDCUFF_PLAYER", onHandCuffPlayer);
            EventController.addMenuEvent("ON_REMOVE_HANDCUFF_PLAYER", onRemoveHandCuffPlayer);
            EventController.addMenuEvent("ON_SEARCH_PLAYER", onSearchPlayer);
            EventController.addMenuEvent("ON_KNEEL_PLAYER", onKneelPlayer);
        }


        private bool onHandCuffPlayer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var type = (BaseObjectType)data["InteractionTargetBaseType"];
            var targetId = (int)data["InteractionTargetId"];

            if(type == BaseObjectType.Player) {
                var target = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.getCharacterId() == targetId);
                if(target != null) {
                    var handCuff = player.getInventory().getItem<HandCuff>(i => true);
                    if(handCuff != null && hasHandsUp(target)) {
                        var anim = AnimationController.getAnimationByName("WORK_FRONT");
                        AnimationController.animationTask(player, anim, () => {
                            if(target.Position.Distance(player.Position) < 3) {
                                handcuffPlayer(target);
                                handCuff.use(player);
                            } else {
                                player.sendBlockNotification("Der Spieler hat sich zu weit entfernt!", "Spieler weg", Constants.NotifactionImages.System);
                            }
                        });
                    }
                }
            }

            return true;
        }

        private bool onRemoveHandCuffPlayer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var type = (BaseObjectType)data["InteractionTargetBaseType"];
            var targetId = (int)data["InteractionTargetId"];

            if(type == BaseObjectType.Player) {
                var target = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.getCharacterId() == targetId);
                if(target != null) {
                    var anim = AnimationController.getAnimationByName("WORK_FRONT");
                    AnimationController.animationTask(player, anim, () => {
                        if(target.Position.Distance(player.Position) < 3) {
                            freePlayerFromHandcuff(target);
                        } else {
                            player.sendBlockNotification("Der Spieler hat sich zu weit entfernt!", "Spieler weg", Constants.NotifactionImages.System);
                        }
                    });
                }
            }

            return true;
        }

        private bool onPlayerHandsUp(IPlayer player, ConsoleKey key, string eventName) {
            if((player.getBusy() && !player.hasState(Constants.PlayerStates.HandsUp)) || player.IsInVehicle) {
                return false;
            }

            if(player.hasState(Constants.PlayerStates.HandsUp)) {
                player.removeState(Constants.PlayerStates.HandsUp);
                player.stopAnimation();
                player.emitClientEvent("WEAPON_PULLOUT_DISABLED", false);
            } else {
                var anim = AnimationController.getAnimationByName("HANDS_UP");
                player.addState(Constants.PlayerStates.HandsUp);
                player.playAnimation(anim);
                player.emitClientEvent("WEAPON_PULLOUT_DISABLED", true);
            }

            return true;
        }

        private bool onSearchPlayer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var type = (BaseObjectType)data["InteractionTargetBaseType"];
            var targetId = (int)data["InteractionTargetId"];

            if(type == BaseObjectType.Player) {
                var target = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.getCharacterId() == targetId);
                if(target != null) {
                    if(target.Position.Distance(player.Position) > 1.5) {
                        player.sendBlockNotification("Du bist zu weit weg von dem Spieler!", "Zu weit weg", Constants.NotifactionImages.System);
                        return false;
                    }

                    InventoryController.showMoveInventory(player, player.getInventory(), target.getInventory(), (p, fI, tI, i, a) => {
                        if(!target.Exists() && (target.Position.Distance(player.Position) > 1.5 || (!hasHandsUp(target) && !isHandCuffed(target)))) {
                            player.sendBlockNotification("Der Spieler hat die Hände nicht mehr oben oder ist zu weit weg!", "Aktion abgebrochen");
                            return false;
                        } else {
                            return true;
                        }
                    });
                }
            }

            return true;
        }


        private bool onKneelPlayer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {

            var type = (BaseObjectType)data["InteractionTargetBaseType"];
            var targetId = (int)data["InteractionTargetId"];

            if(type == BaseObjectType.Player) {
                var target = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.getCharacterId() == targetId);
                if(target != null) {
                    if(target.hasData("IS_KNEELED")) {
                        target.resetData("IS_KNEELED");
                        target.stopAnimation();
                        var anim = AnimationController.getAnimationByName("HANDCUFFED");
                        target.playAnimation(anim);
                    } else {
                        target.setData("IS_KNEELED", true);
                        var anim = AnimationController.getAnimationByName("CUFFED_KNEEL_DOWN");
                        target.playAnimation(anim);
                        var anim2 = AnimationController.getAnimationByName("HANDCUFFED");
                        target.playAnimation(anim2);
                    }
                }
            }
            return true;
        }

        private bool hasCableTies(IPlayer player) {
            return player.getInventory().getItem<HandCuff>(i => true) != null;
        }

        public static bool hasHandsUp(IPlayer player) {
            return player.hasState(Constants.PlayerStates.HandsUp);
        }

        private bool isHandCuffed(IPlayer player) {
            return player.hasData("HANDCUFFED");
        }

        private void handcuffPlayer(IPlayer player) {
            player.addState(Constants.PlayerStates.Handcuffed);

            player.removeState(Constants.PlayerStates.HandsUp);
            player.setPermanentData("HANDCUFFED", true.ToString());

            var anim = AnimationController.getAnimationByName("HANDCUFFED");
            player.forceAnimation(anim);
            player.emitClientEvent("WEAPON_PULLOUT_DISABLED", true);
        }

        private void onConnectDataHandcuff(IPlayer player, character character, characterdatum data) {
            if(bool.Parse(data.value)) {
                player.addState(Constants.PlayerStates.Handcuffed);

                player.removeState(Constants.PlayerStates.HandsUp);

                var anim = AnimationController.getAnimationByName("HANDCUFFED");
                player.forceAnimation(anim);
                player.emitClientEvent("WEAPON_PULLOUT_DISABLED", true);
            } else {
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onConnectDataHandcuff: data was false, which is not even possible!");
            }
        }

        private void freePlayerFromHandcuff(IPlayer player) {
            player.removeState(Constants.PlayerStates.Handcuffed);

            player.resetPermantData("HANDCUFFED");

            player.stopAnimation();
            player.emitClientEvent("WEAPON_PULLOUT_DISABLED", true);
        }
    }

    public class HandCuff : Item {
        public HandCuff(item item) : base(item) { }

        public HandCuff(configitem configItem, int amount, int quality) : base(configItem, quality, amount) { }
    }
}
