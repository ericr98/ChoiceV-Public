using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Farming;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public class Fertilizer : Item {

        private float Level { get => (float)Data["Level"]; set { Data["Level"] = value; } }

        public Fertilizer(item item) : base(item) { }

        public Fertilizer(configitem configItem) : base(configItem) {
            Level = float.Parse(configItem.additionalInfo) / 100;
            setDescription();
        }

        public override void use(IPlayer player) {
            base.use(player);

            var colShapes = player.getCurrentCollisionShapes();
            if(colShapes != null) {
                var colshape = player.getCurrentCollisionShapes().FirstOrDefault(c => c.Owner != null && c.Owner is IFertilizeObject);

                if(colshape != null) {
                    var obj = colshape.Owner as IFertilizeObject;
                    var anim = AnimationController.getAnimationByName(Constants.KNEEL_DOWN_ANIMATION);
                    AnimationController.animationTask(player, anim, () => {
                        var desc = obj.onFertilize(Level);
                        player.sendNotification(Constants.NotifactionTypes.Success, desc, "Pflanze gedüngt", Constants.NotifactionImages.Fertilizer);
                    });
                    return;
                }
            }

            player.sendBlockNotification("Hier gibt es nichts zum Düngen!", "Nichts zu düngen!");
        }

        public override void processAdditionalInfo(string info) {
            Level = float.Parse(info) / 100;
            setDescription();
        }

        private void setDescription() {
            Description = $"Ist ein Dünger der Stufe {Level}.";
        }
    }
}
