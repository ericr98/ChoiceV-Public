using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChoiceVServer.Model.Menu {

    public enum MenuItemStyle {
        normal,
        lightlyGreen,
        green,
        yellow,
        red,
        fleecaBank,
        mazeBank,
        libertyBank
    }

    public class MenuItemCefRepresentative {
        public int id;
        public string type;
        public string name;
        public string description;
        public string className;
        public bool eventOnSelect;

        public MenuItemCefRepresentative(int id, string type, string name, string description, MenuItemStyle className, bool eventonselect = false) {
            this.id = id;
            this.type = type;
            this.name = name;
            this.description = description;
            this.className = className.ToString();
            this.eventOnSelect = eventonselect;
        }
    }

    public class MenuItemCefEvent {
        public int id;
        public string evt;
        public string action;
    }

    public class MenuItem : MenuElement {
        public int Id;
        public string Name;
        public string Description;
        public MenuItemStyle Style;
        public bool NeedsConfirmation = false;
        public bool EventOnSelect;
        public bool BlockMenuCloseCallback = false;


        /// <summary>
        /// Contains CurrentlySelected and CurrentlySelectedName for Listmenu Items: ["index"], ["name"]
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, dynamic> Data = new Dictionary<string, dynamic>();

        public MenuItem(string name, string description, MenuItemStyle style, bool eventonselect = false) {
            Name = name;
            Description = description;
            Style = style;
            Data = new Dictionary<string, dynamic>();
            EventOnSelect = eventonselect;
        }

        /// <summary>
        /// Transforms the MenuItem into a representative object the client cef can read
        /// </summary>
        public virtual string toCef() {
            return "";
        }

        /// <summary>
        /// Parses the cef event string to an cef event object
        /// </summary>
        public virtual MenuItemCefEvent getCefEvent(string data) {
            return null;
        }
        

        public virtual bool doesMenuItemAutomaticallyBlockMovement() {
            return false;
        }
    }
}
