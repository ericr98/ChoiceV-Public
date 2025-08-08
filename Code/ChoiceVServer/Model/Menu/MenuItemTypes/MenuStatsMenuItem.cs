using ChoiceVServer.Base;
using Newtonsoft.Json;

namespace ChoiceVServer.Model.Menu {
    public class StatsMenuItemDataElement {
        public int id;
        public string evt;
    }

    public class MenuStatsMenuItem : UseableMenuItem {
        private string RightInfo;

        /// <summary>
        /// Item that gives you the stats/state of the whole Submenu
        /// </summary>
        public MenuStatsMenuItem(string name, string description, string evt, MenuItemStyle style = MenuItemStyle.normal, bool eventonselect = false) : base(name, description, evt, style, eventonselect) {
            RightInfo = "";
        }

        public MenuStatsMenuItem(string name, string description, string rightInfo, string evt, MenuItemStyle style = MenuItemStyle.normal, bool eventonselect = false) : base(name, description, evt, style, eventonselect) {
            RightInfo = rightInfo;
        }

        private class MenuStateMenuItemCefRepresentative : MenuItemCefRepresentative {
            public string evt;
            public string rightInfo;

            public MenuStateMenuItemCefRepresentative(int id, string name, string description, string evt, MenuItemStyle style, bool eventonselect = false, string rightInfo = null) : base(id, "stats", name, description, style, eventonselect) {
                this.evt = evt;
                this.rightInfo = rightInfo;
            }
        }

        public override string toCef() {
            return (new MenuStateMenuItemCefRepresentative(Id, Name, Description, Event, Style, EventOnSelect, RightInfo)).ToJson();
        }

        public class MenuStatsMenuItemEvent : MenuItemCefEvent {
            //User needs to parse themselfs
            public string[] elements;
        }

        public override MenuItemCefEvent getCefEvent(string data) {
            var obj = new MenuStatsMenuItemEvent();
            JsonConvert.PopulateObject(data, obj);
            return obj;
        }
    }
}
