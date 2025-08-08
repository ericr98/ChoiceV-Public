using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem {
    public class IdCardController : ChoiceVScript {
        public IdCardController() {
            InteractionController.addPlayerInteractionElement(
                new ConditionalGeneratedPlayerInteractionMenuElement(
                    "Ausweiskarten zeigen",
                    generateIdCardMenu,
                    sender => sender is IPlayer && (sender as IPlayer).getInventory().hasItem<IdCardItem>(i => true),
                    target => true
                )
             );

            EventController.addMenuEvent("SHOW_ID_CARD_ITEM", onShowIdCard);
        }

        private Menu generateIdCardMenu(IEntity sender, IEntity target) {
            var menu = new Menu("Ausweiskarten zeigen", "Welche Karte möchtest du zeigen?");
            var items = (sender as IPlayer).getInventory().getItems<IdCardItem>(i => true);

            foreach(var item in items) {
                var data = new Dictionary<string, dynamic> {
                    { "Item", item },
                    { "Target", target }
                };

                menu.addMenuItem(new ClickMenuItem(item.Name, $"Zeige der Person {item.Description}", "", "SHOW_ID_CARD_ITEM").withData(data));
            }

            return menu;
        }

        private bool onShowIdCard(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var target = (IPlayer)data["Target"];
            var item = (IdCardItem)data["Item"];

            if(target != null && target.Exists() && item != null) {
                target.emitCefEventWithBlock(item.getCefEvent(), "ID_CARD");
            }

            return true;
        }
    }

    public class IdCardItemCefEvent : IPlayerCefEvent {
        public string Event { get; private set; }
        public IdCardItemElement[] data;

        public IdCardItemCefEvent(List<IdCardItemElement> data, string eventName) {
            Event = eventName;
            this.data = data.ToArray();
        }
    }

    public class IdCardItemElement {

        [JsonProperty(PropertyName = "name")]
        public string Name;

        [JsonProperty(PropertyName = "data")]
        public string Data;

        public IdCardItemElement(string name, string data) {
            Name = name;
            Data = data;
        }
    }

    public abstract class IdCardItem : Item {
        protected string EventName;

        public IdCardItem(item item) : base(item) { }

        public IdCardItem(FileItem item) : base(InventoryController.getConfigById(item.ConfigId)) { }

        public IdCardItem(configitem cfg) : base(cfg) { }

        public void setData(List<IdCardItemElement> data) {
            Data["Inputs"] = data.ToJson();
        }

        public List<IdCardItemElement> getData() {
            var list = new List<IdCardItemElement>();
            var dataL = ((string)Data["Inputs"]).FromJson<List<IdCardItemElement>>();

            foreach(var el in dataL) {
                list.Add(new IdCardItemElement(el.Name, el.Data));
            }

            return list;
        }

        public string getData(string name) {
            var dataL = ((string)Data["Inputs"]).FromJson<List<IdCardItemElement>>();

            foreach(var el in dataL) {
                if(el.Name == name) {
                    return el.Data;
                }
            }

            return null;
        }

        public IdCardItemCefEvent getCefEvent() {
            return new IdCardItemCefEvent(getData(), EventName);
        }

        public override void use(IPlayer player) {
            base.use(player);

            player.emitCefEventWithBlock(getCefEvent(), "ID_CARD");
        }
    }
}
