using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Controller.DamageSystem;
using ChoiceVServer.Model.Database;
using System;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public class VentilationMask : MaskItem {
        private const int VENTILATION_MASK_DRAWABLEID = 36;

        public VentilationMask(item item) : base(item) { }

        //Constructor for generic generation
        public VentilationMask(configitem configItem, int amount, int quality) : base(configItem, ClothingController.getConfigClothing(1, VENTILATION_MASK_DRAWABLEID, "U", null), 0) {

        }

        public override void use(IPlayer player) { }

        public override void equip(IPlayer player) {
            base.equip(player);

            doEquip(player);
        }

        public override void fastEquip(IPlayer player) {
            base.fastEquip(player);

            doEquip(player);
        }

        private static void doEquip(IPlayer player) {
            if(!player.getBusy()) {
                player.setPedRagdoll();
            }

            player.addState(Constants.PlayerStates.InAnesthesia);

            VoiceController.globalMutePlayer(player, "VENITLATION_MASK");
            player.sendNotification(Constants.NotifactionTypes.Warning, "Du wurdest betäubt. Du kannst erst wieder aufstehen, wenn das Gerät abgenommen wird", "Betäubt worden", Constants.NotifactionImages.Bone);
        }

        public override void unequip(IPlayer player) {
            base.unequip(player);

            doUnequip(player);
        }

        public override void fastUnequip(IPlayer player) {
            base.fastUnequip(player);

            doUnequip(player);
        }

        private static void doUnequip(IPlayer player) {
            var rand = new Random();

            player.sendNotification(Constants.NotifactionTypes.Info, "Das Betäubungsgerät wurde dir abgenommen. Die Narkose lässt langsam nach", "Betäubungsgerät abgenommen", Constants.NotifactionImages.Bone);

            InvokeController.AddTimedInvoke("VentilationRemover", (Action<IInvoke>)((i) => {
                player.sendNotification(Constants.NotifactionTypes.Info, "Die Betäubung hat nachgelassen, du kannst dich wieder bewegen", "Betäubung nachgelassen worden", Constants.NotifactionImages.Bone);

                var comp = CompanyController.findCompaniesByType<MedicFunctionality>(c => c.isInHospital(player.Position)).FirstOrDefault();

                comp?.sendMessageToStaff("Ein Patient ist aus seiner Narkose erwacht!", "Patient aus Narkose");

                player.stopPedRagdoll();
                player.removeState(Constants.PlayerStates.InAnesthesia);
                VoiceController.globalUnmutePlayer(player, "VENTILATION_MASK");
            }), TimeSpan.FromMinutes(3 - rand.NextDouble() * 2), false);

        }

        public override void onConnectEquip(IPlayer player) {
            base.onConnectEquip(player);

            doEquip(player);
        }
    }
}
