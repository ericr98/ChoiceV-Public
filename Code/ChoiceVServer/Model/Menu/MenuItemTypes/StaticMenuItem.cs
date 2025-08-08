using ChoiceVServer.Base;

namespace ChoiceVServer.Model.Menu {
    public class StaticMenuItem : MenuItem {
        public string RightInfo;
        public string UpdateIdentifier;
        
        /// <summary>
        /// Item that just displayes information
        /// </summary>
        public StaticMenuItem(string name, string description, string rightInfo, MenuItemStyle style = MenuItemStyle.normal) : base(name, description, style, false) {
            RightInfo = rightInfo;
        }
        
        public StaticMenuItem(string updateIdentifer, string name, string description, string rightInfo, MenuItemStyle style = MenuItemStyle.normal) : base(name, description, style, false) {
            UpdateIdentifier = updateIdentifer;
            RightInfo = rightInfo;
        }

        private class StaticMenuItemRepresentative : MenuItemCefRepresentative {
            public string right;
            public string updateIdentifier;
            
            public StaticMenuItemRepresentative(int id, string name, string description, string right, MenuItemStyle style, bool eventonselect = false, string updateIdentifier = "") : base(id, "static", name, description, style, eventonselect) {
                this.right = right;
                this.updateIdentifier = updateIdentifier;    
            }
        }

        public override string toCef() {
            return (new StaticMenuItemRepresentative(Id, Name, Description, RightInfo, Style, EventOnSelect, UpdateIdentifier)).ToJson();
        }
    }
}
