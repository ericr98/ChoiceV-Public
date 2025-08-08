using ChoiceVServer.Base;
using ChoiceVServer.Model.Menu;
using Newtonsoft.Json;

public class FileMenuItem : UseableMenuItem {
    public string RightInfo;

    /// <summary>
    /// Item that simply checks for a a enter by the player
    /// </summary>
    public FileMenuItem(string name, string description, string rightInfo, string evt, MenuItemStyle style = MenuItemStyle.normal) : base(name, description, evt, style) {
        RightInfo = rightInfo;
    }

    private class FileMenuItemRepresentative : MenuItemCefRepresentative {
        public string right;
        public string evt;

        public FileMenuItemRepresentative (int id, string name, string description, string right, string evt, MenuItemStyle style) : base(id, "file", name, description, style) {
            this.right = right;
            this.evt = evt;
        }
    }

    public override string toCef() {
        return (new FileMenuItemRepresentative(Id, Name, Description, RightInfo, Event, Style)).ToJson();
    }
    
    public class FileMenuItemEvent : MenuItemCefEvent {
        public string fileData;
    }

    public override MenuItemCefEvent getCefEvent(string data) {
        var obj = new FileMenuItemEvent();
        JsonConvert.PopulateObject(data, obj);
        return obj;
    }
}           
