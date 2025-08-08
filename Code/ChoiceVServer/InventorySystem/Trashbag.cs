using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.Model.Database;
using System;
using System.Text;

namespace ChoiceVServer.InventorySystem {
    public enum TrashBagSize {
        Small,
        Medium,
        Large,
    }
    
    public class TrashBag: Item {
        public TrashBagSize Size;
        
        public TrashBag(item item) : base(item) { }

        public TrashBag(configitem configItem, int amount, int quality) : base(configItem, quality, amount) { }

        public override void use(IPlayer player) {
            SoundController.playSoundAtCoords(player.Position, 3, SoundController.Sounds.BagRustle, 0.3f);
            var anim = AnimationController.getAnimationByName("WORK_FRONT");
            AnimationController.animationTask(player, anim, () =>{
                var items = TrashController.createTrashItemsFromTrashBag(this);

                foreach(var item in items) {
                    player.getInventory().addItem(item, true);
                }

                var str = new StringBuilder();
                str.Append("Müllsack geöffnet es waren: ");
                foreach(var item in items) {
                    str.Append($"{item.StackAmount}x {item.ConfigItem.name}, ");
                }
                str.Remove(str.Length - 2, 2);
                str.Append(" drin.");
                player.sendNotification(Constants.NotifactionTypes.Success, str.ToString(), "Müllsack geöffnet", Constants.NotifactionImages.Package);
            
                base.use(player);
            }, null, true, null, TimeSpan.FromSeconds(3));
        }

        public override void processAdditionalInfo(string info) {
            Size = Enum.Parse<TrashBagSize>(info);
        }
    }
}
