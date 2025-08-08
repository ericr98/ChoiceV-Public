using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Model.Database;
using System;

namespace ChoiceVServer.InventorySystem {
    public class RebreatherMask : MaskItem {
        private static int AIR_SECONDS = 300;

        public int RemainingSeconds { get => ((int)Data["RemainingSeconds"]); set { Data["RemainingSeconds"] = value; } }
        private bool RemainingSecondsSet { get => Data.hasKey("RemainingSeconds"); }

        public DateTime LastEquipTime { get => ((string)Data["LastEquipTime"]).FromJson<DateTime>(); set { Data["LastEquipTime"] = value.ToJson(); } }
        private bool LastEquipTimeSet { get => Data.hasKey("LastEquipTime"); }

        private IInvoke Invoke;

        public RebreatherMask(item item) : base(item) { }

        public RebreatherMask(configitem cfgItem, int amount, int quality) : base(cfgItem, ClothingController.getConfigClothing(1, 166, "U", null), 0) {
            RemainingSeconds = AIR_SECONDS;
        }

        public override void use(IPlayer player) {
            base.use(player);
        }

        public override void equip(IPlayer player) {
            base.equip(player);

            doEquip(player);
        }

        public override void fastEquip(IPlayer player) {
            base.fastEquip(player);

            doEquip(player);
        }

        private void doEquip(IPlayer player) {
            if(RemainingSeconds > 0) {
                if(Invoke != null) {
                    Invoke.EndSchedule();
                }

                LastEquipTime = DateTime.Now;
                Invoke = InvokeController.AddTimedInvoke($"Rebreather-{Id}", (i) => removeAir(player), TimeSpan.FromSeconds(RemainingSeconds), false);
                player.toggleInfiniteAir(true, true);
            } else {
                player.sendBlockNotification("Der Rebreather ist aufgebraucht! Er wird unter Wasser nichts nutzen!", "Rebreather aufgebraucht");
            }
        }

        public override void unequip(IPlayer player) {
            base.unequip(player);

            doUnequip(player);
        }

        public override void fastUnequip(IPlayer player) {
            base.fastUnequip(player);

            doUnequip(player);
        }

        private void doUnequip(IPlayer player) {
            player.toggleInfiniteAir(false, true);

            if(Invoke != null) {
                Invoke.EndSchedule();
            }

            RemainingSeconds -= (int)(DateTime.Now - LastEquipTime).TotalSeconds;

            if(RemainingSeconds <= 0) {
                RemainingSeconds = 0;
            }

            updateDescription();
        }

        public override void onConnectEquip(IPlayer player) {
            base.onConnectEquip(player);

            if(RemainingSeconds > 0) {
                LastEquipTime = DateTime.Now;
                Invoke = InvokeController.AddTimedInvoke($"Rebreather-{Id}", (i) => removeAir(player), TimeSpan.FromSeconds(RemainingSeconds), false);
                player.toggleInfiniteAir(true, true);
            } else {
                player.sendBlockNotification("Der Rebreather ist aufgebraucht! Er wird unter Wasser nichts nutzen!", "Rebreather aufgebraucht");
            }
        }

        private void removeAir(IPlayer player) {
            if(this != null && IsEquipped && player != null && player.Exists()) {
                player.toggleInfiniteAir(false, true);
                player.sendBlockNotification("Der Rebreather ist aufgebraucht! Er wird unter Wasser nichts nutzen!", "Rebreather aufgebraucht");

            }
        }

        public override void onUnloaded() {
            if(Invoke != null) {
                Invoke.EndSchedule();
            }

            if(LastEquipTimeSet) {
                RemainingSeconds -= (int)(DateTime.Now - LastEquipTime).TotalSeconds;

                if(RemainingSeconds <= 0) {
                    RemainingSeconds = 0;
                }

                updateDescription();
            }
        }

        public override void updateDescription() {
            if(RemainingSecondsSet) {
                Description = $"Hat noch Luft für {RemainingSeconds} Sekunden";
            } else {
                Description = $"Hat noch Luft für {AIR_SECONDS} Sekunden";
            }

            base.updateDescription();
        }
    }
}
