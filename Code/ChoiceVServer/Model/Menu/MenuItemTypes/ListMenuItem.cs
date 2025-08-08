using ChoiceVServer.Base;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Model.Menu {
    public class ListMenuItem : UseableMenuItem {
        public readonly string[] Elements;
        public readonly Dictionary<string, dynamic> MappedElements;
        public readonly bool DisableEnter;

        public bool NoLoopOver;
        public string NoLoopOverStart;

        /// <summary>
        /// Item that holds a list the player can chose from
        /// </summary>
        public ListMenuItem(string name, string description, string[] elements, string evt, MenuItemStyle style = MenuItemStyle.normal, bool eventonselect = false, bool disableEnterAction = false) : base(name, description, evt, style, eventonselect) {
            Elements = elements;
            DisableEnter = disableEnterAction;
        }

        /// <summary>
        /// Item that holds a list the player can chose from including a mapped value
        /// </summary>
        public ListMenuItem(string name, string description, Dictionary<string, dynamic> elements, string evt, MenuItemStyle style = MenuItemStyle.normal, bool eventonselect = false, bool disableEnterAction = false) : base(name, description, evt, style, eventonselect) {
            Elements = elements.Keys.ToArray();
            MappedElements = elements;
            DisableEnter = disableEnterAction;
        }

        public ListMenuItem withNoLoopOver(string leftStart) {
            NoLoopOver = true;
            NoLoopOverStart = leftStart;

            return this;
        }

        private class ListMenuItemRepresentative : MenuItemCefRepresentative {
            public string[] elements;
            public bool disableEnter;
            public string evt;

            public bool noLoopOver;
            public string noLoopOverStart;

            public ListMenuItemRepresentative(int id, string name, string description, string[] elements, bool disableEnter, string evt, MenuItemStyle style, bool eventonselect, bool noLoopOver, string noLoopOverStart) : base(id, "list", name, description, style, eventonselect) {
                this.elements = elements;
                this.disableEnter = disableEnter;
                this.evt = evt;
                this.noLoopOver = noLoopOver;
                this.noLoopOverStart = noLoopOverStart;
            }
        }

        public override string toCef() {
            return (new ListMenuItemRepresentative(Id, Name, Description, Elements, DisableEnter, Event, Style, EventOnSelect, NoLoopOver, NoLoopOverStart)).ToJson();
        }

        public class ListMenuItemEvent : MenuItemCefEvent {
            public string currentElement;
            public dynamic currentValue;
            public int currentIndex;
        }

        public override MenuItemCefEvent getCefEvent(string data) {
            var obj = new ListMenuItemEvent();
            JsonConvert.PopulateObject(data, obj);

            if(MappedElements is not null) {
                obj.currentValue = MappedElements[obj.currentElement];
            }

            return obj;
        }
    }
}
