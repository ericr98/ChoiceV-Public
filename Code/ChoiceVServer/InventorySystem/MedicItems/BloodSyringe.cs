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
using static ChoiceVServer.Model.Menu.InputMenuItem;

namespace ChoiceVServer.InventorySystem {
    public class BloodSyringeController : ChoiceVScript {
        public BloodSyringeController() {
            InteractionController.addPlayerInteractionElement(new ConditionalPlayerInteractionMenuElement(
                    () => new ClickMenuItem("Blut entnehmen", "Entnimm dem Spieler eine Blutprobe", "", "BLOOD_SYRINGE_INTERACTION"),
                    sender => sender is IPlayer && hasViableBloodSyringe(sender as IPlayer),
                    target => target is IPlayer && HandCuffController.hasHandsUp(target as IPlayer)
            ));

            EventController.addMenuEvent("BLOOD_SYRINGE_INTERACTION", onBloodSyringeInteraction);
            EventController.addMenuEvent("BLOOD_SYRINGE_NAME", onBloodSyringeName);
        }

        private bool hasViableBloodSyringe(IPlayer player) {
            var syringes = player.getInventory().getItems<BloodSyringe>(i => true);

            if(syringes != null && syringes.Count() > 0) {
                foreach(var syringe in syringes) {
                    if(!syringe.Locked) {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool onBloodSyringeInteraction(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var type = (BaseObjectType)data["InteractionTargetBaseType"];
            var targetId = (int)data["InteractionTargetId"];

            if(type == BaseObjectType.Player) {
                var target = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.getCharacterId() == targetId);
                if(target != null) {
                    var item = player.getInventory().getItem<BloodSyringe>(i => !i.Locked);
                    if(item != null) {
                        var anim = AnimationController.getAnimationByName("WORK_FRONT");
                        AnimationController.animationTask(player, anim, () => {
                            if(target != null && player.Position.Distance(target.Position) < 3) {
                                item.CharacterId = target.getCharacterId();
                                item.Locked = true;
                                item.ExpirationDate = DateTime.Now + TimeSpan.FromDays(14);

                                item.Description = $"Verfällt: {item.ExpirationDate.ToString("d.M.yyyy")} | {item.Description}";
                                item.updateDescription();

                                player.sendNotification(Constants.NotifactionTypes.Success, "Du hast den Spieler erfolgreich Blut abgenommen!", "Blut abgenommen", Constants.NotifactionImages.Bone);
                            } else {
                                player.sendBlockNotification("Der Spieler hat sich zu weit entfernt!", "Spieler weg", Constants.NotifactionImages.Bone);
                            }
                        });
                    } else {
                        player.sendBlockNotification("Du hast kein Blutabnahmegerät mehr im Inventar!", "Item fehlt", Constants.NotifactionImages.Bone);
                    }
                } else {
                    player.sendBlockNotification("Dein Ziel wurde nicht gefunden!", "Ziel fehlt", Constants.NotifactionImages.Bone);
                }
            }

            return true;
        }

        private bool onBloodSyringeName(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as InputMenuItemEvent;
            var item = (BloodSyringe)data["Item"];

            if(item != null) {
                item.Description = evt.input;
                item.updateDescription();

                player.sendNotification(Constants.NotifactionTypes.Info, $"Du hast die Blutprobe mit {evt.input} beschriftet", "Blutprobe beschriftet", Constants.NotifactionImages.Bone);
            }

            return true;
        }

    }

    public class BloodSyringe : Item {
        public int CharacterId { get => ((int)Data["CharacterId"]); set { Data["CharacterId"] = value; } }
        public bool Locked { get => ((bool)Data["Locked"]); set { Data["Locked"] = value; } }
        public DateTime ExpirationDate { get => ((DateTime)Data["ExpirationDate"]); set { Data["ExpirationDate"] = value; } }

        public BloodSyringe(item item) : base(item) { }

        //Constructor for generic generation
        public BloodSyringe(configitem configItem, int amount, int quality) : base(configItem) {
            Locked = false;
        }

        public override void use(IPlayer player) {
            if(!isEvidence()) {
                if(Locked) {
                    var menu = new Menu("Blutabnahmegerät", "Was möchtest du tun?");
                    menu.addMenuItem(new InputMenuItem("Entnahme beschriften", "Beschrifte die Probe", "", "BLOOD_SYRINGE_NAME").withData(new Dictionary<string, dynamic> { { "Item", this } }));

                    player.showMenu(menu);
                } else {
                    player.sendBlockNotification("Das Blutabnahmegerät ist noch leer!", "Item unbenutzbar", Constants.NotifactionImages.Bone);
                }
            } else {
                player.sendBlockNotification("Das Blutabnahmegerät kann nicht mehr beschriftet werden!", "Item unbenutzbar", Constants.NotifactionImages.Bone);
            }
        }
    }
}
