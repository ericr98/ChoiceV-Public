using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using ChoiceVServer.Controller.DamageSystem;

namespace ChoiceVServer.InventorySystem {
    public class AdrenalinSyringeController : ChoiceVScript {
        public AdrenalinSyringeController() {
            InteractionController.addPlayerInteractionElement(
                new ConditionalPlayerInteractionMenuElement(
                    () => new ClickMenuItem("Person wiederbeleben", "Spritze der Person Adrenalin", "", "SHOOT_PLAYER_ADRENALIN", MenuItemStyle.green),
                    sender => sender is IPlayer && hasAdrenalinSyringe(sender as IPlayer),
                    target => target is IPlayer && (target as IPlayer).hasState(Constants.PlayerStates.Dead)
                )
            );

            EventController.addMenuEvent("SHOOT_PLAYER_ADRENALIN", onPlayerShootAdrenalin);
        }

        private static bool hasAdrenalinSyringe(IPlayer player) {
            return player.getInventory().getItem<AdrenalinSyringe>(i => true) != null;
        }

        private bool onPlayerShootAdrenalin(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var targetId = (int)data["InteractionTargetId"];
            var target = ChoiceVAPI.FindPlayerByCharId(targetId);
            if(target != null) {
                var anim = AnimationController.getAnimationByName("WORK_FRONT");
                AnimationController.animationTask(player, anim, () => {
                    if(target.Exists()) {
                        var item = player.getInventory().getItem<AdrenalinSyringe>(i => true);
                        item.use(player);

                        DamageController.revivePlayer(target);
                        target.setData("DEATH_TIMER", DateTime.Now + TimeSpan.FromMinutes(15));
                        target.sendNotification(Constants.NotifactionTypes.Info, "Du spürst wie Adrenalin durch deine Adern fließt!", "Wiederbelebt worden", Constants.NotifactionImages.Bone);
                        player.sendNotification(Constants.NotifactionTypes.Success, "Die Person wurde erfolgreich wiederbelebt", "Person wiederbelebt", Constants.NotifactionImages.Bone);
                    }
                });
            }

            return true;
        }
    }

    public class AdrenalinSyringe : Item {

        public AdrenalinSyringe(item item) : base(item) { }

        //Constructor for generic generation
        public AdrenalinSyringe(configitem configItem, int amount, int quality) : base(configItem) { }
    }
}
