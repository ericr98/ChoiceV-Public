using ChoiceVServer.Base;
using Newtonsoft.Json;

namespace ChoiceVServer.Model.Menu {
    public class HoverMenuItem : UseableMenuItem {
        public string RightInfo;

        /// <summary>
        /// Item that simply checks for a a enter by the player
        /// </summary>
        public HoverMenuItem(string name, string description, string rightInfo, string evt, MenuItemStyle style = MenuItemStyle.normal) : base(name, description, evt, style) {
            RightInfo = rightInfo;
        }

        private class SelectMenuItemRepresentative : MenuItemCefRepresentative {
            public string right;
            public string evt;

            public SelectMenuItemRepresentative(int id, string name, string description, string right, string evt, MenuItemStyle style) : base(id, "hover", name, description, style) {
                this.right = right;
                this.evt = evt;
            }
        }

        public override string toCef() {
            return (new SelectMenuItemRepresentative(Id, Name, Description, RightInfo, Event, Style)).ToJson();
        }

        public override MenuItemCefEvent getCefEvent(string data) {
            var obj = new MenuItemCefEvent();
            JsonConvert.PopulateObject(data, obj);
            return obj;
        }
    }
}
