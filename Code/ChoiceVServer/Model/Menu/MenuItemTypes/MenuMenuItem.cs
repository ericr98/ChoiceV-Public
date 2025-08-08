using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Model.Menu {
    public class MenuMenuItem : MenuItem {
        public Menu SubMenu = null;
        public VirtualMenu VirtualMenu = null;
        public string RightInfo;
        public string Event;

        public bool AlwaysCreateNew = false;

        /// <summary>
        /// Item that holds a another SubMenu. Instantly uses a created menu
        /// </summary>
        public MenuMenuItem(string name, Menu subMenu, MenuItemStyle style = MenuItemStyle.normal, string eventOnSelect = null) : base(name, "Untermenü", style, eventOnSelect != null) {
            SubMenu = subMenu;
            Event = eventOnSelect;
        }

        /// <summary>
        /// Item that holds a another SubMenu. Waits with creation of submenu until opening of the menu
        /// </summary>
        public MenuMenuItem(string name, VirtualMenu virtualMenu, MenuItemStyle style = MenuItemStyle.normal, string eventOnSelect = null, bool alwaysCreateNew = false) : base(name, "Untermenü", style, eventOnSelect != null) {
            VirtualMenu = virtualMenu;
            Event = eventOnSelect;

            AlwaysCreateNew = alwaysCreateNew;
        }

        /// <summary>
        /// Item that holds a another SubMenu. Instantly uses a created menu
        /// </summary>
        public MenuMenuItem(string name, Menu subMenu, string description, string rightInfo, MenuItemStyle style = MenuItemStyle.normal, string eventOnSelect = null) : base(name, description, style, eventOnSelect != null) {
            SubMenu = subMenu;
            RightInfo = rightInfo;
            Event = eventOnSelect;
        }

        /// <summary>
        /// Item that holds a another SubMenu. Waits with creation of submenu until opening of the menu
        /// </summary>
        public MenuMenuItem(string name, VirtualMenu virtualMenu, string description, string rightInfo, MenuItemStyle style = MenuItemStyle.normal, string eventOnSelect = null, bool alwaysCreateNew = false) : base(name, description, style, eventOnSelect != null) {
            VirtualMenu = virtualMenu;
            RightInfo = rightInfo;
            Event = eventOnSelect;

            AlwaysCreateNew = alwaysCreateNew;
        }

        private class SubMenuItemRepresentative : MenuItemCefRepresentative {
            public MenuCefRepresentative menuData;
            public string evt;
            public string right;
            public bool alwaysCreateNew;

            public SubMenuItemRepresentative(int id, string name, string description, string rightInfo, MenuCefRepresentative menuData, MenuItemStyle style, bool eventOnSelect, bool alwaysCreateNew, string evt) : base(id, "menu", name, description, style, eventOnSelect) {
                this.menuData = menuData;
                this.right = rightInfo;
                this.evt = evt;
                this.alwaysCreateNew = alwaysCreateNew;
            }
        }

        /// <summary>
        /// Adds Data to the MenuElement. can be uses like: menu.addMenuItem(new ....).withData(data));
        /// </summary>
        public MenuMenuItem withData(Dictionary<string, dynamic> data) {
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

        public override string toCef() {
            return new SubMenuItemRepresentative(Id, Name, Description, RightInfo, null, Style, EventOnSelect, AlwaysCreateNew, Event).ToJson();

            ////Is recursive!
            //var menuRep = SubMenu.toCef();
            //var rep = new SubMenuItemRepresentative(Id, Name, menuRep, Style);

            //return rep.ToJson();
        }

        public class SubMenuAddDataCefEvent : IPlayerCefEvent {
            public string Event { get; }
            public int menuMenuItemId;
            public string name;
            public string subtitle;
            public string[] elements;

            public SubMenuAddDataCefEvent(int menuMenuItemId, MenuCefRepresentative menu) {
                Event = "ADD_MENU_DATA";
                this.menuMenuItemId = menuMenuItemId;
                this.name = menu.name;
                this.subtitle = menu.subtitle;
                this.elements = menu.elements;
            }
        }


        public SubMenuAddDataCefEvent getSubMenuAddDataCefEvent() {
            //Is recursive!

            MenuCefRepresentative menuRep;
            if(SubMenu != null && !AlwaysCreateNew) {
                menuRep = SubMenu.toCef();
            } else {
                var menu = VirtualMenu.getMenu();
                SubMenu = menu;
                if(menu != null) {
                    menuRep = menu.toCef();
                } else {
                    menuRep = new MenuCefRepresentative("Leeres Menü", "Dieses Menü ist leer", new string[] { });
                }
            }

            return new SubMenuAddDataCefEvent(Id, menuRep);
        }

        public void subMenuClosed(IPlayer player) {
            if(SubMenu != null && SubMenu.MenuCloseCallbacks.Count > 0) {
                foreach(var callback in SubMenu.MenuCloseCallbacks) {
                    callback.Invoke(player);
                }
            }
        }

        public override MenuItemCefEvent getCefEvent(string data) {
            var obj = new MenuItemCefEvent();
            JsonConvert.PopulateObject(data, obj);
            return obj;
        }

        public Menu getMenu() {
            return SubMenu;
        }
    }
}