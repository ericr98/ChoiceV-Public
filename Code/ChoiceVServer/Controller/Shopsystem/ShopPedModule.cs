using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Controller.Crime_Stuff;
using ChoiceVServer.Controller.Shopsystem.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller.Shopsystem {
    public class NPCShopModule : NPCModule {
        public class PlayerStealInteraction {
            public int PlayerId;
            public DateTime Date;
            public int Amount;

            public bool HasBeenDispatched;

            public PlayerStealInteraction(int playerId, DateTime date, int amount) {
                PlayerId = playerId;
                Date = date;
                Amount = amount;

                HasBeenDispatched = false;
            }
        }

        private ShopModel Shop;

        private List<PlayerStealInteraction> StealInteractions;

        public NPCShopModule(ChoiceVPed ped, ShopModel shop) : base(ped) {
            Shop = shop;

            if(ped.Data.Items.ContainsKey("StealInteractions")) {
                StealInteractions = ((string)ped.Data["StealInteractions"]).FromJson<List<PlayerStealInteraction>>();

                var newL = new List<PlayerStealInteraction>();
                foreach(var interaction in StealInteractions) {
                    if(interaction.Date + TimeSpan.FromDays(2) >= DateTime.Now) {
                        newL.Add(interaction);
                    }
                }

                StealInteractions = newL;
                ped.Data["StealInteractions"] = newL.ToJson();
            } else {
                StealInteractions = new List<PlayerStealInteraction>();
            }
        }

        public override List<MenuItem> getMenuItems(IPlayer player) {
            var list = new List<MenuItem>();

            if(Ped.IsBeingThreatend) {
                list.Add(new ClickMenuItem("Kasse ausrauben", "Zwinge den Ladenbesitzer dir den Inhalt der Kasse zu geben. Dies ist eine illegale Aktion, und kann rechtliche Konsequenzen haben!", "", "SHOP_STEAL_SHOP_MONEY", MenuItemStyle.yellow).withData(new Dictionary<string, dynamic> { { "Shop", Shop } }).needsConfirmation("Laden ausrauben?", "Wirklich ausrauben?"));
            } else {
                if(player.getClothing().Mask.Drawable == 0) {
                    var menu = Shop.getShopMenu(player, onMenuClose);

                    player.setData("INTERACTING_SHOPKEEPER", true);

                    list.Add(new MenuMenuItem(menu.Name, menu));
                } else {
                    list.Add(new StaticMenuItem("Maske absetzen", "Der Laden kann nicht geöffnet werden, solange du eine Maske trägst!", "", MenuItemStyle.red));
                }
            }

            return list;
        }

        private void onMenuClose(IPlayer player) {
            player.resetData("INTERACTING_SHOPKEEPER");
        }

        public bool isPedBusy() {
            return Shop.CollisionShape.getAllEntitiesList().Cast<IPlayer>().Any(p => p.hasData("INTERACTING_SHOPKEEPER"));
        }

        public void playerEnterShop(IPlayer player) {
            var old = StealInteractions.FirstOrDefault(i => i.PlayerId == player.getCharacterId() && i.Date + TimeSpan.FromMinutes(20) < DateTime.Now);

            if(!Shop.ShopKeeper.IsBeingThreatend) {
                if(player.getClothing().Mask.Drawable == 0) {
                    var rand = new Random();
                    if(old != null && rand.NextDouble() < 0.75) {
                        ControlCenterController.createDispatch(DispatchType.NpcSilentDispatch, "Dieb wiedererkannt", $"Der Inhaber des {Shop.Name} hat eine Person wiedererkannt die in der Vergangenheit gestohlen hat", Ped.Position);
                    }
                } else {
                    maskDetectedByShopKeeper(player);
                }
            }
        }

        private void maskDetectedByShopKeeper(IPlayer player) {
            player.sendBlockNotification($"Du hast 10sek die Maske abzusetzen oder den Laden zu verlassen, bevor der Ladeninhaber die Polizei ruft!", "Ladenbesitzer ruft", NotifactionImages.Shop);

            InvokeController.AddTimedInvoke("Mask-Shop-Invoke", (i) => {
                if(!Shop.ShopKeeper.IsBeingThreatend && Shop.CollisionShape.IsInShape(player.Position) && player.getClothing().Mask.Drawable != 0) {
                    ControlCenterController.createDispatch(DispatchType.NpcSilentDispatch, "Maskierte Person", $"Eine Person hat im {Shop.Name} die Maske trotz Aufforderung nicht abgezogen", Ped.Position);
                }
            }, TimeSpan.FromSeconds(10), false);
        }

        public bool playerMakesStealInteraction(IPlayer player, configitem item) {
            var all = StealInteractions.Where(i => i.PlayerId == player.getCharacterId()).ToList();
            var old = all.FirstOrDefault(i => i.Date + TimeSpan.FromMinutes(5) < DateTime.Now && i.Date + TimeSpan.FromDays(1) > DateTime.Now);
            var current = all.FirstOrDefault(i => i.Date + TimeSpan.FromMinutes(5) > DateTime.Now);

            var stealCounter = 0;
            all.Where(i => i.Date + TimeSpan.FromDays(1) > DateTime.Now).ForEach(i => stealCounter += i.Amount);
            if(stealCounter > 10) {
                return false;
            }

            if(old != null) {
                if(!Shop.ShopKeeper.IsBeingThreatend) {
                    ControlCenterController.createDispatch(DispatchType.NpcSilentDispatch, "Diebstahl erkannt", $"Ein ein Diebstahl im {Shop.Name} wurde vom Inhaber bemerkt", Ped.Position);
                }
            } else {
                if(current == null) {
                    var newC = new PlayerStealInteraction(player.getCharacterId(), DateTime.Now, 0);
                    StealInteractions.Add(newC);
                    current = newC;
                }
                current.Amount += 1;
                Ped.Data["StealInteractions"] = StealInteractions.ToJson();

                var rand = new Random();
                if(isPedBusy()) {
                    if(current.Amount > 4) {
                        var check = rand.NextDouble();

                        if(!Shop.ShopKeeper.IsBeingThreatend && check < 0.2 * (current.Amount - 4) && (!player.hasData("LAST_SEEN_STEALING") || (DateTime)player.getData("LAST_SEEN_STEALING") + TimeSpan.FromMinutes(5) < DateTime.Now)) {
                            ControlCenterController.createDispatch(DispatchType.NpcSilentDispatch, "Diebstahl erkannt", $"Ein ein Diebstahl im {Shop.Name} wurde vom Inhaber bemerkt", Ped.Position);
                            current.HasBeenDispatched = true;
                            Ped.Data["StealInteractions"] = StealInteractions.ToJson();
                            player.setData("LAST_SEEN_STEALING", DateTime.Now);
                        }
                    }
                } else {
                    var check = rand.NextDouble();

                    if(!Shop.ShopKeeper.IsBeingThreatend && check < 0.2 * current.Amount && (!player.hasData("LAST_SEEN_STEALING") || (DateTime)player.getData("LAST_SEEN_STEALING") + TimeSpan.FromMinutes(5) < DateTime.Now)) {
                        ControlCenterController.createDispatch(DispatchType.NpcSilentDispatch, "Diebstahl erkannt", $"Ein Diebstahl im {Shop.Name} wurde vom Inhaber bemerkt", Ped.Position);
                        current.HasBeenDispatched = true;
                        Ped.Data["StealInteractions"] = StealInteractions.ToJson();
                        player.setData("LAST_SEEN_STEALING", DateTime.Now);
                    }
                }

            }

            CamController.checkIfCamSawAction(player, $"Produkt gestohlen", $"Die Person hat ein/eine {item.name} gestohlen");
            return true;
        }

        public override void onTick(TimeSpan TickTimer) {
            var timeTillFull = TimeSpan.FromHours(5);
            Shop.RobValue += (decimal)Math.Round((float)Shop.MaxRobValue / (PedController.MODULE_TICK_TIME.TotalMilliseconds / timeTillFull.TotalMilliseconds), 2);
            Shop.RobValue = Math.Min(Shop.RobValue, Shop.MaxRobValue);

            using(var db = new ChoiceVDb()) {
                var dbShop = db.configshops.Find(Shop.Id);
                if(dbShop != null) {
                    dbShop.robValue = Shop.RobValue;
                }
                db.SaveChanges();
            }

            foreach(var interact in StealInteractions) {
                if(!interact.HasBeenDispatched && interact.Date + TimeSpan.FromMinutes(15) < DateTime.Now) {
                    ControlCenterController.createDispatch(DispatchType.NpcCallDispatch, "Diebstahl im Nachhinein bemerkt", $"Der Ladenbesitzer von {Shop.Name} hat einen Ladendiebstahl bemerkt. Er ereignete sich vor ca. 15min", Ped.Position);
                    interact.HasBeenDispatched = true;

                    Ped.Data["StealInteractions"] = StealInteractions.ToJson();
                }
            }
        }

        private class RobMoneyNotCollected {
            public int CharId;
            public decimal Amount;
            public DateTime LastDropValue;

            public RobMoneyNotCollected(int charId, decimal amount) {
                CharId = charId;
                Amount = amount;
                LastDropValue = DateTime.Now;
            }
        }

        private IInvoke RobingInvoke;
        private int TickCounter = 0;
        public void startRobbing(IPlayer player) {
            Shop.ShopKeeper.playAnimation("mp_am_hold_up", "holdup_victim_20s", 1, 15_000, 0.15f, 2);
            Shop.ShopKeeper.PerformingThreatendIgnoreAction = true;
            TickCounter = 0;

            var returnToNormalCounter = 0;

            var maxTicks = 12f;
            var payOutTicks = 2;
            var segments = 3;
            var segmentValue = new float[] { 0.10f, 0.25f, 0.65f };
            var totalRuns = maxTicks / payOutTicks;
            var runsPerSegment = totalRuns / segments;

            ControlCenterController.createDispatch(DispatchType.NpcSilentDispatch, "Kasse wird ausgeraubt", $"Die Kasse des Ladens {Shop.Name} wird ausgeraubt", Shop.ShopKeeper.Position);

            RobingInvoke = InvokeController.AddTimedInvoke("Robing-Invoke", (i) => {
                if(!Shop.ShopKeeper.IsBeingThreatend) {
                    returnToNormalCounter++;

                    if(returnToNormalCounter >= 2) {
                        Shop.ShopKeeper.returnToStandardAnimation();
                        Shop.ShopKeeper.PerformingThreatendIgnoreAction = false;
                        RobingInvoke.EndSchedule();
                        return;
                    }
                }

                Shop.ShopKeeper.playAnimation("mp_am_hold_up", "holdup_victim_20s", 1, 16_000, 0.15f, 2);
                TickCounter++;

                if(TickCounter % payOutTicks == 0) {
                    var segment = (int)Math.Ceiling(TickCounter / (maxTicks / segments));
                    var value = segmentValue[segment - 1];

                    var getValue = (decimal)Math.Round((float)Shop.MaxRobValue * (value / runsPerSegment), 2);

                    if(getValue > Shop.RobValue) {
                        Shop.RobValue = 0;
                    } else {
                        Shop.RobValue -= getValue;
                    }

                    if(player.Position.Distance(Shop.ShopKeeper.Position) < 4) {
                        player.addCash(getValue);
                        CrimeNetworkController.OnPlayerCrimeActionDelegate.Invoke(player, CrimeAction.StoreRobbingNpc, (float)getValue, null);
                        player.sendNotification(NotifactionTypes.Success, $"Du hast ${getValue} vom Ladenbesitzer erhalten!", $"${getValue} erhalten", NotifactionImages.Thief);
                    } else {
                        if(Shop.ShopKeeper.NonPersistentData.ContainsKey("ROBBED_MONEY")) {
                            var money = (RobMoneyNotCollected)Shop.ShopKeeper.NonPersistentData["ROBBED_MONEY"];
                            money.Amount += getValue;
                        } else {
                            Shop.ShopKeeper.NonPersistentData["ROBBED_MONEY"] = new RobMoneyNotCollected(player.getCharacterId(), getValue);
                        }
                        player.sendNotification(NotifactionTypes.Warning, $"Der Ladenbesitzer konnte dir das Geld nicht geben und hat es auf den Tresen gelegt. Geh zu ihm um es einzusammeln.", "Raubgeld einzusammeln", NotifactionImages.Thief);
                    }

                    using(var db = new ChoiceVDb()) {
                        var dbShop = db.configshops.Find(Shop.Id);
                        if(dbShop != null) {
                            dbShop.robValue = Shop.RobValue;
                        }
                        db.SaveChanges();
                    }
                }

                if(TickCounter > maxTicks || Shop.RobValue == 0) {
                    player.sendNotification(NotifactionTypes.Success, $"Die Kasse des Ladenbesitzers ist leer!", "Kass leer", NotifactionImages.Thief);
                    Shop.ShopKeeper.returnToStandardAnimation();
                    Shop.ShopKeeper.PerformingThreatendIgnoreAction = false;
                    RobingInvoke.EndSchedule();
                } else {
                    Shop.ShopKeeper.playAnimation("mp_am_hold_up", "holdup_victim_20s", 1, 16_000, 0.15f);
                }
            }, TimeSpan.FromSeconds(15), true);
        }

        public override void onEntityEnterPedShape(IEntity entity) {
            var player = entity as IPlayer;

            if(Shop.ShopKeeper.NonPersistentData.ContainsKey("ROBBED_MONEY")) {
                var money = (RobMoneyNotCollected)Shop.ShopKeeper.NonPersistentData["ROBBED_MONEY"];
                if(money.CharId == player.getCharacterId() && money.LastDropValue + TimeSpan.FromMinutes(5) > DateTime.Now) {
                    player.addCash(money.Amount);
                    CrimeNetworkController.OnPlayerCrimeActionDelegate.Invoke(player, CrimeAction.StoreRobbingNpc, (float)money.Amount, null);
                    player.sendNotification(NotifactionTypes.Info, $"Du hast dir das Geld hinter dem Tresen genommen. Es waren ${money.Amount}", $"${money.Amount} erhalten", NotifactionImages.Thief);
                    Shop.ShopKeeper.NonPersistentData.Remove("ROBBED_MONEY");
                }
            }
        }

        public void onPlayerPutOnClothing(IPlayer player, ClothingPlayer newClothing) {
            if(newClothing.Mask.Drawable != 0) {
                maskDetectedByShopKeeper(player);
            }
        }

        public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
            return new StaticMenuItem("Shop Modul", $"Ein Modul welches das Öffnen des Shops: {Shop.Name} ermöglicht", "");
        }

        public override void onRemove() { }
    }
}
