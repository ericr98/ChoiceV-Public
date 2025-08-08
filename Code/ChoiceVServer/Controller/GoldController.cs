using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller {
    public class GoldController : ChoiceVScript {
        private class GoldVein {
            public Position Position;
            public DateTime RemoveTime;
            public int GoldLeft;

            public GoldVein(Position pos, TimeSpan livingTime) {

                Position = pos;
                var rnd = new Random();
                GoldLeft = rnd.Next(3, 7);

                RemoveTime = DateTime.Now + livingTime;

                Logger.logTrace(LogCategory.System, LogActionType.Created, $"Goldvein created: pos: {pos.ToString()}, goldLeft: {GoldLeft}, removeTime: {RemoveTime}");
            }

            public static bool veinFound(IPlayer player, ref GoldVein vein) {
                var rnd = new Random();
                var per = rnd.NextDouble();

                var found = false;
                foreach(var v in GoldRadiusList) {
                    if(v.Position.Distance(player.Position) <= VEIN_RADIUS) {
                        found = true;
                        vein = v;
                    }
                }

                //Check if found Gold Vein is overdue
                if(found) {
                    if(vein.RemoveTime <= DateTime.Now) {
                        GoldRadiusList.Remove(vein);
                        found = false;

                        Logger.logTrace(LogCategory.System, LogActionType.Removed, $"Goldvein removed: pos: {vein.Position.ToString()}, goldLeft: {vein.GoldLeft}");
                    } else {
                        return true;
                    }
                }

                //Maybe generate New GoldVein
                if(!found && per <= FIND_GOLD_VEIN_CHANCE) {
                    vein = new GoldVein(player.Position, VEIN_REMOVE_TIME);
                    player.sendNotification(NotifactionTypes.Success, "Du hast eine Ader gefunden!", "Ader gefunden!", NotifactionImages.Gold);

                    GoldRadiusList.Add(vein);
                    return true;
                } else {
                    return false;
                }
            }

            public static string getGoldFound() {
                var rnd = new Random();
                var per = rnd.NextDouble();

                if(per > FIND_GOLD_FLAKE) {
                    return "GOLD_SMALL_PIECE";
                } else if(per > FIND_GOLD_NUGGET) {
                    return "GOLD_FLAKE";
                } else {
                    return "GOLD_NUGGET";
                }
            }
        }

        private static List<GoldVein> GoldRadiusList = new List<GoldVein>();

        public GoldController() {   
            //EventController.addCollisionShapeEvent("GOLD", onGoldRiverInteraction);
            EventController.addInteriorInteractionEvent("CS6_08_Mine_INT", onGoldMineInteraction);

            //EventController.addMenuEvent("SELL_ALL_GOLD", onSellAllGold);
        }

        //private bool onGoldMenu(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> data) {
        //    var menu = new Menu("Randy Gold McNuggets", "Das beste Equipment und die besten Preise!");
        //    menu.addMenuItem(new ClickMenuItem("Equipment einkaufen", "Kaufe Equipment und Lizenzen für das Goldschürfen", "", "OPEN_GOLD_SHOP"));

        //    var sellGold = new ClickMenuItem("Dein Gold verkaufen", "Verkaufe alles Gold, was du dabei hast!", "", "SELL_ALL_GOLD").needsConfirmation("Alles Gold verkaufen?", "Du verkaufst all dein getragenes Gold!");
        //    menu.addMenuItem(sellGold);


        //    //TODO MAKE RED WHEN NOT ENOUGH GOLD OR MONEY!
        //    var melt = new ClickMenuItem("Gold zum Barren schmelzen", "Schmilz dein Gold zu einem Barren! Kosten: 70$", "", "MELT_GOLD_TO_INGOT").needsConfirmation("Alles Gold verkaufen?", "Du verkaufst all dein getragenes Gold!");
        //    menu.addMenuItem(melt);

        //    player.showMenu(menu);

        //    return true;
        //}

        ////TODO FIX
        //private bool onSellAllGold(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        //    var inv = player.getInventory();
        //    var corns = inv.getItem(GOLD_CORN_DB_ID, i => true);
        //    var flake = inv.getItem(GOLD_FLAKE_DB_ID, i => true);
        //    var nugget = inv.getItem(GOLD_NUGGET_DB_ID, i => true);

        //    decimal amount = decimal.Zero;
        //    if(corns != null && inv.removeItem(corns, corns.StackAmount ?? 1)) {
        //        amount += Convert.ToDecimal(corns.StackAmount * 5);
        //    }

        //    if(flake != null && inv.removeItem(flake, flake.StackAmount ?? 1)) {
        //        amount += Convert.ToDecimal(flake.StackAmount * 35);
        //    }

        //    if(nugget != null && inv.removeItem(nugget, nugget.StackAmount ?? 1)) {
        //        amount += Convert.ToDecimal(nugget.StackAmount * 500);
        //    }

        //    if(amount > 0) {
        //        player.getCharacterData().Cash += amount;
        //        player.sendNotification(NotifactionTypes.Success, $"Du hast all dein Gold für ${amount} verkauft!", $"${amount} durch Gold", NotifactionImages.Gold);
        //    } else {
        //        player.sendBlockNotification("Du hast kein Gold!", "Verkauf abgebrochen", NotifactionImages.Gold);
        //    }

        //    return true;
        //}

        //private bool onGoldRiverInteraction(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> data) {
        //    if(player.getBusy()) {
        //        return false;
        //    }
        //    var anim = AnimationController.getAnimationByName(GOLD_WASH_ITEM_ANIMATION);
        //    AnimationController.animationTask(player, anim, () => {
        //        handleGoldInteraction(player);
        //    });
        //    return true;
        //}

        private bool onGoldMineInteraction(IPlayer player, string milo) {
            //Check if player is on good height:
            if(player.Position.Z > 145 || player.getBusy()) {
                return false;
            }

            var pickaxe = player.getInventory().getItem<ToolItem>(i => i.Flag == SpecialToolFlag.Pickaxe);
            if(pickaxe == null) {
                player.sendNotification(NotifactionTypes.Danger, "Du hast nicht das nötige Werkzeug.", "Werkzeug fehlt");
                return true;
            }

            pickaxe.evaluateUse(player);
            var anim = AnimationController.getAnimationByName(GOLD_MINE_ITEM_ANIMATION);
            AnimationController.animationTask(player, anim, () => {
                handleGoldInteraction(player);
            });

            return true;
        }

        private void handleGoldInteraction(IPlayer player) {
            GoldVein vein = null;
            if(GoldVein.veinFound(player, ref vein)) {
                if(vein.GoldLeft > 0) {
                    vein.GoldLeft--;
                    generateAndAddItem(player, GoldVein.getGoldFound(), -1);
                    player.sendNotification(NotifactionTypes.Success, "Du siehst da etwas funkeln!", "Gold gefunden!", NotifactionImages.Gold);
                } else {
                    player.sendNotification(NotifactionTypes.Info, "Du siehst hier kein Gold mehr!", "Gold gefunden!", NotifactionImages.Gold);
                }
            } else {
                player.sendNotification(NotifactionTypes.Info, "Nur Dreck und Steine!", "Gold gefunden!", NotifactionImages.Gold);
            }
        }

        public void generateAndAddItem(IPlayer player, string goldLevel, int quality) {
            var config = InventoryController.getConfigItemByCodeIdentifier(goldLevel);
            var item = new StaticItem(config, quality, 1);

            item.Description = "Sieht ungefähr nach 18 Karat aus. Schmilz es ein oder verkauf es.";

            player.getInventory().addItem(item);
        }
    }
}
