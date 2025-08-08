using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using NLog.Targets;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem {
    public class MaskController : ChoiceVScript {
        public MaskController() {
            InteractionController.addPlayerInteractionElement(
                new ConditionalPlayerInteractionMenuElement(
                    () => new ClickMenuItem("Maske abziehen", "Ziehe der Person die Maske ab!", "", "REMOVE_PLAYER_MASK"),
                    p => p is IPlayer,
                    t => t is IPlayer target && (target.hasState(Constants.PlayerStates.InAnesthesia) || target.hasState(Constants.PlayerStates.Dead) || target.hasState(Constants.PlayerStates.LayingDown) || target.hasState(Constants.PlayerStates.HandsUp)) && target.getInventory().hasItem<MaskItem>(i => i.IsEquipped)
                )
            );
            EventController.addMenuEvent("REMOVE_PLAYER_MASK", onPlayerRemoveMask);

            InteractionController.addPlayerInteractionElement(
                new ConditionalGeneratedPlayerInteractionMenuElement(
                    "Maske aufsetzen",
                    putOnMaskMenuGenerator,
                    p => p is IPlayer player && player.getInventory().hasItem<MaskItem>(i => !i.IsEquipped),
                    t => t is IPlayer target && (target.hasState(Constants.PlayerStates.Dead) || target.hasState(Constants.PlayerStates.LayingDown)) && !target.getInventory().hasItem<MaskItem>(i => i.IsEquipped)
                )
            );
            EventController.addMenuEvent("PUT_MASK_ON_PLAYER", onPutMaskOnPlayer);

            ClothingController.addOnConnectClothesCheck(2, onCheckForMask);
        }

        private bool onPlayerRemoveMask(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var target = (IPlayer)data["InteractionTarget"];

            if(target.Exists()) {
                var mask = target.getInventory().getItem<MaskItem>(i => i.IsEquipped);
                if(mask != null) {
                    var anim = AnimationController.getAnimationByName("WORK_FRONT");
                    AnimationController.animationTask(player, anim, () => {
                        mask.fastUnequip(target);
                        target.getInventory().moveItem(player.getInventory(), mask, 1, true);

                        player.sendNotification(Constants.NotifactionTypes.Success, "Die Maske der anderen Person wurde von dir abgenommen", "Maske abgenommen");
                        target.sendNotification(Constants.NotifactionTypes.Warning, $"Deine Maske wurde dir von einer anderen Person abgenommen!", "Maske abgenommen");
                    });
                }
            }
            return true;
        }

        private Menu putOnMaskMenuGenerator(IEntity sender, IEntity target) {
            var masks = (sender as IPlayer).getInventory().getItems<MaskItem>(i => !i.IsEquipped);

            var menu = new Menu("Maske aufsetzen", "Setze der Person eine Maske auf");

            foreach(var mask in masks) {
                menu.addMenuItem(new ClickMenuItem(mask.Name, mask.Description, "", "PUT_MASK_ON_PLAYER").withData(new Dictionary<string, dynamic> { { "Mask", mask }, { "InteractionTarget", target } }).needsConfirmation("Maske aufsetzen?", "Person geben und aufsetzen?"));
            }

            return menu;
        }

        private bool onPutMaskOnPlayer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var mask = (MaskItem)data["Mask"];
            var target = (IPlayer)data["InteractionTarget"];

            if(target.Exists()) {
                var anim = AnimationController.getAnimationByName("WORK_FRONT");
                AnimationController.animationTask(player, anim, () => {
                    player.getInventory().moveItem(target.getInventory(), mask, 1, true);

                    mask.fastEquip(target);
                    player.sendNotification(Constants.NotifactionTypes.Success, "Die Maske wurde der anderen Person aufgesetzt", "Maske aufgesetzt");
                    target.sendNotification(Constants.NotifactionTypes.Warning, $"Dir wurde ein/e {mask.Name} von einer anderen Person aufgesetzt", "Maske gesetzt");
                });
            }

            return true;
        }

        private void onCheckForMask(IPlayer player, ref ClothingPlayer cloth) {
            var gender = player.getCharacterData().Gender.ToString();
            if(player.getInventory().hasItem<FullClothingItem>(i => i.IsEquipped && !i.allowsMaks(gender))) {
                return;
            }

            var comp = cloth.GetSlot(1, false);
            var stand = Constants.StandartClothings[1];
            var maskItem = player.getInventory().getItem<MaskItem>(i => i.DrawableId == comp.Drawable && i.TextureId == comp.Texture);

            // TODO STRESSTEST FIX
            if(comp.Drawable != stand.Drawable && maskItem != null) {
                maskItem.onConnectEquip(player);
            } else {
                cloth.UpdateClothSlot(1, stand.Drawable, stand.Texture);
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"Player had mask equipped, which was not in his inventory! component: {comp.ToJson()}");
            }
        }
    }

    public class MaskItem : EquipItem {
        public int DrawableId { get => (int)Data["DrawableId"]; set { Data["DrawableId"] = value; } }
        public int TextureId { get => (int)Data["textureId"]; set { Data["textureId"] = value; } }

        public MaskItem(item item) : base(item) {
            EquipType = "mask";
        }

        public MaskItem(configitem configItem, configclothing clothing, int textureId) : base(configItem) {
            DrawableId = clothing.drawableid;
            TextureId = textureId;

            Description = $"Eine Maske vom Typ: {clothing.name} in Variation {textureId}";
            updateDescription();

            EquipType = "mask";
        }

        public override void use(IPlayer player) {
            if(changeNotAllowed(player)) {
                player.sendBlockNotification("Du kannst aktuell deine Kleidung nicht ändern!", "Kleidung nicht änderbar", Constants.NotifactionImages.System);
                return;
            }

            if(IsEquipped) {
                unequip(player);
            } else {
                var already = player.getInventory().getItem<MaskItem>(m => m.IsEquipped);

                if(already != null) {
                    already.fastUnequip(player);
                }

                equip(player);
            }
        }

        public override void equip(IPlayer player) {
            if(changeNotAllowed(player)) {
                player.sendBlockNotification("Du kannst aktuell deine Kleidung nicht ändern!", "Kleidung nicht änderbar", Constants.NotifactionImages.System);
                return;
            }

            base.equip(player);

            var clothing = player.getClothing();

            var anim = AnimationController.getAnimationByName("EQUIP_HAT");
            AnimationController.animationTask(player, anim, () => {
                clothing.UpdateClothSlot(1, DrawableId, TextureId);
                ClothingController.loadPlayerClothing(player, clothing);
            });
        }

        public override void fastEquip(IPlayer player) {
            base.fastEquip(player);

            var clothing = player.getClothing();
            clothing.UpdateClothSlot(1, DrawableId, TextureId);
            ClothingController.loadPlayerClothing(player, clothing);
        }

        public override void unequip(IPlayer player) {
            if(changeNotAllowed(player)) {
                player.sendBlockNotification("Du kannst aktuell deine Kleidung nicht ändern!", "Kleidung nicht änderbar", Constants.NotifactionImages.System);
                return;
            }

            base.unequip(player);

            var clothing = player.getClothing();

            var anim = AnimationController.getAnimationByName("UNEQUIP_HAT");
            AnimationController.animationTask(player, anim, () => {
                clothing.UpdateClothSlot(1, 0, 0);
                ClothingController.loadPlayerClothing(player, clothing);
            });
        }

        public override void fastUnequip(IPlayer player) {
            base.fastUnequip(player);

            var clothing = player.getClothing();
            clothing.UpdateClothSlot(1, 0, 0);
            ClothingController.loadPlayerClothing(player, clothing);
        }

        public virtual void onConnectEquip(IPlayer player) {
            IsEquipped = true;
        }

        public bool changeNotAllowed(IPlayer player) {
            var gender = player.getCharacterData().Gender.ToString();
            return player.getInventory().hasItem<FullClothingItem>(i => i.IsEquipped && !i.allowsMaks(gender));
        }
    }
}
