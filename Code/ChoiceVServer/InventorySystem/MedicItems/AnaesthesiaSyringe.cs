using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;
using ChoiceVServer.Controller.DamageSystem;

namespace ChoiceVServer.InventorySystem {
    public class AnaesthesiaSyringeController : ChoiceVScript {
        public AnaesthesiaSyringeController() {
            InteractionController.addPlayerInteractionElement(
                new ConditionalPlayerInteractionMenuElement(
                    () => new ClickMenuItem("Person betäuben", "Betäube die Person", "", "SHOOT_PLAYER_ANAESTHESIA", MenuItemStyle.red),
                    sender => sender is IPlayer && hasAnaesthesiaSyringe(sender as IPlayer),
                    target => target is IPlayer && !(target as IPlayer).getBusy()
                )
            );

            EventController.addMenuEvent("SHOOT_PLAYER_ANAESTHESIA", onPlayerShootAnaesthesia);
        }

        private static bool hasAnaesthesiaSyringe(IPlayer player) {
            return player.getInventory().getItem<AnaesthesiaSyringe>(i => true) != null;
        }

        private bool onPlayerShootAnaesthesia(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var targetId = (int)data["InteractionTargetId"];
            var target = ChoiceVAPI.FindPlayerByCharId(targetId);
            if(target != null) {
                var anim = AnimationController.getAnimationByName("WORK_FRONT");
                AnimationController.animationTask(player, anim, () => {
                    if(target.Exists() && target.Position.Distance(player.Position) < 3) {
                        var item = player.getInventory().getItem<AnaesthesiaSyringe>(i => true);
                        item.use(player);

                        DamageController.killPlayer(target, false, "Du spürst wie die Anaestesiespritze zu wirken beginnt", "Du wurdest betäubt");
                        target.sendNotification(Constants.NotifactionTypes.Info, "Du spürst wie die Anaestesiespritze zu wirken beginnt", "Betäubt worden", Constants.NotifactionImages.Bone);
                        player.sendNotification(Constants.NotifactionTypes.Success, "Die Person wurde erfolgreich betäubt", "Person betäubt", Constants.NotifactionImages.Bone);
                    }
                });
            }

            return true;
        }
    }

    public class AnaesthesiaSyringe : Item {

        public AnaesthesiaSyringe(item item) : base(item) { }

        //Constructor for generic generation
        public AnaesthesiaSyringe(configitem configItem, int amount, int quality) : base(configItem) { }
    }
}
