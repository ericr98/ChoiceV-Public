using AltV.Net.Elements.Entities;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Model.Menu {
    public class UseableMenuItem : MenuItem {
        public string Event;

        public UseableMenuItem(string name, string description, string evt, MenuItemStyle style, bool eventonselect = false) : base(name, description, style, eventonselect) {
            Event = evt;
        }

        /// <summary>
        /// Adds Data to the MenuElement. can be uses like: menu.addMenuItem(new ....).withData(data));
        /// </summary>
        public UseableMenuItem withData(Dictionary<string, dynamic> data) {
            if(data == null) {
                return this;
            }

            if(Data == null) {
                Data = data;
            } else {
                foreach(var pair in data) {
                    Data.Add(pair.Key, pair.Value);
                }
            }
            return this;
        }

        /// <summary>
        /// A Confirm Menu Will be opened to confirm
        /// The Event of the confirmed menuItem is in data["PreviousCefEvent"]
        /// </summary>
        public UseableMenuItem needsConfirmation(string name, string subtitle, string onSelectNoEvent = null) {
            if(Data == null) {
                Data = new Dictionary<string, dynamic>();
            }

            Data.Add("ConfirmationName", name);
            Data.Add("ConfirmationSubtitle", subtitle);
            Data.Add("ConfirmationEvent", Event);
            if(onSelectNoEvent != null) {
                Data.Add("OnSelectNoEvent", onSelectNoEvent);
            }

            Event = "OPEN_MENU_CONFIRMATION";
            BlockMenuCloseCallback = true;
            return this;
        }
    }
}
