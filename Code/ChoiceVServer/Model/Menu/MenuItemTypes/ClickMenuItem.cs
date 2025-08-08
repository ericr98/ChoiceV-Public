using ChoiceVServer.Base;
using Newtonsoft.Json;

namespace ChoiceVServer.Model.Menu {
    public class ClickMenuItem : UseableMenuItem {
        public string RightInfo;
        public bool CloseOnAction;

        public string UpdateIdentifier;
        
        /// <summary>
        /// Item that simply checks for a a enter by the player
        /// </summary>
        public ClickMenuItem(string name, string description, string rightInfo, string evt, MenuItemStyle style = MenuItemStyle.normal, bool eventonselect = false, bool closeOnAction = true) : base(name, description, evt, style, eventonselect) {
            RightInfo = rightInfo;
            CloseOnAction = closeOnAction;
        }
        
        public ClickMenuItem withUpdateIdentifier(string updateIdentifier) {
            UpdateIdentifier = updateIdentifier;
            return this;
        }

        private class ClickMenuItemRepresentative : MenuItemCefRepresentative {
            public string right;
            public string evt;

            public bool closeOnAction;
            
            public string updateIdentifier;

            public ClickMenuItemRepresentative(int id, string name, string description, string right, string evt, MenuItemStyle style, bool eventonselect = false, bool closeOnAction = false, string updateIdentifier = "") : base(id, "click", name, description, style, eventonselect) {
                this.right = right;
                this.evt = evt;
                this.closeOnAction = closeOnAction;
                
                this.updateIdentifier = updateIdentifier;
            }
        }

        public ClickMenuItem withNotCloseOnAction() {
            CloseOnAction = false;
            return this;
        }

        public override string toCef() {
            return (new ClickMenuItemRepresentative(Id, Name, Description, RightInfo, Event, Style, EventOnSelect, CloseOnAction, UpdateIdentifier)).ToJson();
        }

        public override MenuItemCefEvent getCefEvent(string data) {
            var obj = new MenuItemCefEvent();
            JsonConvert.PopulateObject(data, obj);
            return obj;
        }
    }
}
