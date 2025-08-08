using ChoiceVServer.Controller;
using ChoiceVServer.EventSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Model.Menu {
    public interface MenuElement { }

    public class MenuCefRepresentative : IPlayerCefEvent {
        public string Event { get; }
        public string name;
        public string subtitle;
        public string[] elements;

        public MenuCefRepresentative(string name, string subtitle, string[] elements) {
            Event = "CREATE_MENU";
            this.name = name;
            this.subtitle = subtitle;
            this.elements = elements;
        }
    }

    public class VirtualMenu : MenuElement {
        public GenericMenuGenerator Generator;

        public List<Action<Menu>> OnGenerateAction;
        public string Name { get; private set; }
        public MenuItemStyle Style { get; private set; }

        public VirtualMenu(string name, GenericMenuGenerator generator, MenuItemStyle style = MenuItemStyle.normal) {
            Generator = generator;
            Name = name;
            Style = style;
        }

        public Menu getMenu() {
            var menu =  Generator.Invoke();

            foreach(var action in OnGenerateAction ?? Enumerable.Empty<Action<Menu>>()) {
                action.Invoke(menu);
            }

            return menu;
        }

        public void addOnGenerateAction(Action<Menu> action) {
            OnGenerateAction ??= new ();

            OnGenerateAction.Add(action);
        }
    }
    
    public class Menu : MenuElement {
        public string Name;
        public string Subtitle;
        public List<MenuCloseEventDelegate> MenuCloseCallbacks = [];
        private List<MenuItem> MenuItems;

        public object ConsecItemMutex = new object();
        public static int ConsecItemId = 0;

        public bool? AllowMovement = null;

        public string UpdatingIdentifier = null;
        
        public Menu(string name, string subtitle) {
            Name = name;
            Subtitle = subtitle;
            MenuItems = new List<MenuItem>();
        }

        public Menu(string name, string subtitle, bool allowMovement) {
            Name = name;
            Subtitle = subtitle;
            MenuItems = new List<MenuItem>();

            AllowMovement = allowMovement;
        }


        /// <param name="callback">Most upper Menu ALWAYS has to clean up all Submenu changes!</param>
        public Menu(string name, string subtitle, MenuCloseEventDelegate callback) {
            Name = name;
            Subtitle = subtitle;
            MenuItems = new List<MenuItem>();
            if(callback != null) {
                MenuCloseCallbacks = [callback];
            }
        }
        
        public Menu(string name, string subtitle, List<MenuCloseEventDelegate> callbacks) {
            Name = name;
            Subtitle = subtitle;
            MenuItems = new List<MenuItem>();
            MenuCloseCallbacks = callbacks;
        }

        /// <param name="callback">Most upper Menu ALWAYS has to clean up all Submenu changes!</param>
        public Menu(string name, string subtitle, MenuCloseEventDelegate callback, bool allowMovement) {
            Name = name;
            Subtitle = subtitle;
            MenuItems = new List<MenuItem>();
            
            if(callback != null) {
                MenuCloseCallbacks = [callback];
            }

            AllowMovement = allowMovement;
        }

        /// <summary>
        /// Adds a menuItem to the menu. Generates a continues id for every menuItem. It is never the same in one run!
        /// </summary>
        public void addMenuItem(MenuItem menuItem) {
            if(menuItem == null) {
                return;
            }

            lock(ConsecItemMutex) {
                menuItem.Id = ConsecItemId;
                ConsecItemId++;
                MenuItems.Add(menuItem);
                
                if(menuItem.doesMenuItemAutomaticallyBlockMovement()) {
                    AllowMovement = false;
                }
            }
        }
        
        /// <summary>
        /// Appends a menuItem to the menu. Generates a continues id for every menuItem. It is never the same in one run!
        /// </summary>
        public void insertMenuItem(MenuItem menuItem, int index = 0) {
            lock(ConsecItemMutex) {
                menuItem.Id = ConsecItemId;
                ConsecItemId++;
                MenuItems.Insert(index, menuItem);
                
                if(menuItem.doesMenuItemAutomaticallyBlockMovement()) {
                    AllowMovement = false;
                }
            }
        }
        
        public void sortMenuItems(Comparison<MenuItem> comparison) {
            MenuItems.Sort(comparison);
        }

        public int getMenuItemCount() {
            return MenuItems.Count;
        }

        public MenuItem getMenuItemByIndex(int index) {
            return MenuItems[index];
        }

        public List<MenuItem> getMenuItems() {
            return MenuItems;
        }

        /// <summary>
        /// Transforms the Menu into a representative object the client cef can read
        /// </summary>
        public MenuCefRepresentative toCef() {
            return new MenuCefRepresentative(Name, Subtitle, MenuItems.Select(i => i.toCef()).ToArray());
        }
    }
}
