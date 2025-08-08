using ChoiceVServer.Base;
using Newtonsoft.Json;

namespace ChoiceVServer.Model.Menu {
    public class CheckBoxMenuItem : UseableMenuItem {
        public bool Checked;
        public bool CloseOnAction;

        /// <summary>
        /// Item that can be checked and unchecked with the spacebar
        /// </summary>
        public CheckBoxMenuItem(string name, string description, bool check, string evt, MenuItemStyle style = MenuItemStyle.normal, bool eventonselect = false, bool closeOnAction = true) : base(name, description, evt, style, eventonselect) {
            Checked = check;
            CloseOnAction = closeOnAction;
        }

        private class CheckBoxMenuItemCefRepresentative : MenuItemCefRepresentative {
            public string evt;
            public bool check;
            public bool closeOnAction;

            public CheckBoxMenuItemCefRepresentative(int id, string name, string description, string evt, bool check, MenuItemStyle style, bool closeOnAction, bool eventonselect) : base(id, "check", name, description, style, eventonselect) {
                this.evt = evt;
                this.check = check;
                this.closeOnAction = closeOnAction;
            }
        }

        public override string toCef() {
            return (new CheckBoxMenuItemCefRepresentative(Id, Name, Description, Event, Checked, Style, CloseOnAction, EventOnSelect)).ToJson();
        }

        public class CheckBoxMenuItemEvent : MenuItemCefEvent {
            public string name;
            public bool check;
        }

        public override MenuItemCefEvent getCefEvent(string data) {
            var obj = new CheckBoxMenuItemEvent();
            JsonConvert.PopulateObject(data, obj);
            return obj;
        }
    }
}
