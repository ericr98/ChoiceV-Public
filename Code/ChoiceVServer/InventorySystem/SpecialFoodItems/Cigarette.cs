using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.InventorySystem {
    public class CigaretteController : ChoiceVScript {
        public CigaretteController() {
            EventController.addMenuEvent("ON_SELECT_CIGARETTE_ANIMATION", onSelectCigaretteAnimation);
        }

        private bool onSelectCigaretteAnimation(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var anim = data["ANIM"];
            var item = (Cigarette)data["ITEM"];

            item.smokeCigarette(player, anim);

            return true;
        }
    }

    public class Cigarette : FoodItem {
        public Cigarette(item item) : base(item) {}

        public Cigarette(configitem configItem, int amount, int quality) : base(configItem, amount, quality) { }

        public override void use(IPlayer player) {
            var menu = new Menu("Zigarette", "Wie möchtest du rauchen?");

            menu.addMenuItem(new ClickMenuItem("Normal ziehen", "Ziehe normal an der Zigarette", "", "ON_SELECT_CIGARETTE_ANIMATION")
                .withData(new Dictionary<string, dynamic> { { "ANIM", "SMOKE_CIGARETTE" }, { "ITEM", this } }));

            menu.addMenuItem(new ClickMenuItem("Lässig rauchen", "Ziehe übel lässig an der Zigarette", "", "ON_SELECT_CIGARETTE_ANIMATION")
                .withData(new Dictionary<string, dynamic> { { "ANIM", "SMOKE_CIGARETTE_2" }, { "ITEM", this } }));

            menu.addMenuItem(new ClickMenuItem("Mit links rauchen", "Für Linkshänder geeignet!", "", "ON_SELECT_CIGARETTE_ANIMATION")
                .withData(new Dictionary<string, dynamic> { { "ANIM", "SMOKE_CIGARETTE_3" }, { "ITEM", this } }));

            menu.addMenuItem(new ClickMenuItem("Genussvoll ziehen", "Du fühlst es richtig!", "", "ON_SELECT_CIGARETTE_ANIMATION")
                .withData(new Dictionary<string, dynamic> { { "ANIM", "SMOKE_CIGARETTE_4" }, { "ITEM", this } }));

            menu.addMenuItem(new ClickMenuItem("Chillig ziehen", "Du bist entspannt!", "", "ON_SELECT_CIGARETTE_ANIMATION")
               .withData(new Dictionary<string, dynamic> { { "ANIM", "SMOKE_CIGARETTE_5" }, { "ITEM", this } }));

            player.showMenu(menu);
        }

        public void smokeCigarette(IPlayer player, string anim) {
            player.playAnimation(AnimationController.getAnimationByName(anim), null, false);
            player.sendNotification(Constants.NotifactionTypes.Info, "Die Animation läuft endlos. Du kannst sie jederzeit abbrechen!", "Animation abbrechbar", Constants.NotifactionImages.Cigarette);
            base.use(player);
        }

        public override object Clone() {
            return this.MemberwiseClone();
        }
    }
}
