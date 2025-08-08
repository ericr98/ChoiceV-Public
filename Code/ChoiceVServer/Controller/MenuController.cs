using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Menu;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuMenuItem;

namespace ChoiceVServer.Controller {
    public delegate void MenuCloseEventDelegate(IPlayer player);

    public delegate MenuItem GenericMenuItemGenerator();
    public delegate Menu GenericMenuGenerator();

    public delegate MenuItem PlayerMenuItemGenerator(IPlayer player);

    public class MenuController : ChoiceVScript {
        //int is CharacterId
        private static ConcurrentDictionary<int, Menu> CurrentlyDisplayedMenus = new();

        private static ConcurrentDictionary<string, List<IPlayer>> CurrentlyDisplayedUpdateMenu = new();
        public MenuController() {
            EventController.addCefEvent("MENU_EVENT", onMenuEvent);
            EventController.addMenuEvent("OPEN_MENU_CONFIRMATION", onConfirmationNeeded);
            EventController.addMenuEvent("DO_NOTHING", (p, b, c, d, e) => {
                closeMenu(p);
                return true;
            });

            EventController.PlayerDisconnectedDelegate += onPlayerDisconnect;
            EventController.OnCefCloseDelegate += onCefClose;
        }

        public class MenuCefEvent : IPlayerCefEvent {
            public string Event { get; }
            public string name;
            public string subtitle;
            public string[] elements;

            public MenuCefEvent(string name, string subtitle, string[] elements) {
                this.name = name;
                this.subtitle = subtitle;
                this.elements = elements;
            }
        }

        public class MenuCefUpdateEvent : IPlayerCefEvent {
            public string Event => "UPDATE_MENU";
            public string identifier;
            public string value;

            public MenuCefUpdateEvent(string identifier, string value) {
                this.identifier = identifier;
                this.value = value;
            }
        }
        
        public static void updateMenu(string menuUpdatingIdentifier, string menuItemIdentifier, string value) {
            if(CurrentlyDisplayedUpdateMenu.ContainsKey(menuUpdatingIdentifier)) {
                foreach(var player in CurrentlyDisplayedUpdateMenu[menuUpdatingIdentifier]) {
                    player.emitCefEventNoBlock(new MenuCefUpdateEvent(menuItemIdentifier, value));
                }
            }
        }

        public static void showMenu(IPlayer player, Menu menu, bool blockmovement = true, bool playerBusy = true, string updatingIdentifier = null) {
            if(CurrentlyDisplayedMenus.ContainsKey(player.getAccountId())) {
                CurrentlyDisplayedMenus[player.getAccountId()] = menu;
            } else {
                CurrentlyDisplayedMenus.Add(player.getAccountId(), menu);
            }
            
            if(updatingIdentifier != null) {
                if(CurrentlyDisplayedUpdateMenu.ContainsKey(updatingIdentifier)) {
                    CurrentlyDisplayedUpdateMenu[updatingIdentifier].Add(player);
                } else {
                    CurrentlyDisplayedUpdateMenu.Add(updatingIdentifier, new List<IPlayer> {player});
                }
            }

            if(playerBusy) {
                player.addState(Constants.PlayerStates.OnBusyMenu);
            }

            menu.AllowMovement = !blockmovement;
            if(blockmovement) {
                player.emitCefEventWithBlock(menu.toCef(), "MENU");
            } else {
                player.emitCefEventNoBlock(menu.toCef());
            }
        }

        public static void closeMenu(IPlayer player) {
            if(CurrentlyDisplayedMenus.ContainsKey(player.getAccountId())) {
                var menu = CurrentlyDisplayedMenus[player.getAccountId()];
                CurrentlyDisplayedMenus.Remove(player.getAccountId());

                player.emitCefEventNoBlock(new OnlyEventCefEvent("CLOSE_MENU"));
                player.removeState(Constants.PlayerStates.OnBusyMenu);

                if(menu.MenuCloseCallbacks != null) {
                    foreach(var callback in menu.MenuCloseCallbacks) {
                        callback.Invoke(player);
                    }
                }
                
                WebController.setMovementBlockForCef(player, "MENU", false);
            }

            foreach(var key in CurrentlyDisplayedUpdateMenu.Keys) {
                if(CurrentlyDisplayedUpdateMenu[key].Contains(player)) {
                    CurrentlyDisplayedUpdateMenu[key].Remove(player);
                }
            }
        }
        
        private void onCefClose(IPlayer player) {
            closeMenu(player);
        }

        private bool onConfirmationNeeded(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var name = (string)data["ConfirmationName"];
            var subtitle = (string)data["ConfirmationSubtitle"];
            var evt = (string)data["ConfirmationEvent"];

            string onSelectNoEvent = null;
            if(data.ContainsKey("OnSelectNoEvent")) {
                onSelectNoEvent = (string)data["OnSelectNoEvent"];
            }

            if(menuItemCefEvent.action != "changed") {
                data["PreviousCefEvent"] = menuItemCefEvent;

                //Maybe add posibility to add data for confirmation menu, with .withConformation()
                var closeEvents = new List<MenuCloseEventDelegate>();
                if(CurrentlyDisplayedMenus.ContainsKey(player.getAccountId())) {
                    var current = CurrentlyDisplayedMenus[player.getAccountId()];
                    closeEvents = current.MenuCloseCallbacks;
                }

                var menu = getConfirmationMenuMultiCallback(name, subtitle, evt, data, closeEvents, onSelectNoEvent);
                player.showMenu(menu);
            } else {
                EventController.triggerMenuEvent(player, evt, menuItemId, data, menuItemCefEvent);
            }
            return true;
        }

         public static Menu getConfirmationMenu(string name, string subtitle, string evt, Dictionary<string, dynamic> data = null, MenuCloseEventDelegate closeCallback = null, string onSelectNoEvent = null) {
             var menu = new Menu(name, subtitle, closeCallback);
 
             menu.addMenuItem(new ClickMenuItem("Nein", "Abbrechen?", "", onSelectNoEvent != null ? onSelectNoEvent : "DO_NOTHING", MenuItemStyle.red).withData(data));
             menu.addMenuItem(new ClickMenuItem("Ja", "Wirklich bestätigen?", "", evt, MenuItemStyle.green).withData(data));
 
             return menu;
         }

        public static Menu getConfirmationMenuMultiCallback(string name, string subtitle, string evt, Dictionary<string, dynamic> data = null, List<MenuCloseEventDelegate> closeCallback = null, string onSelectNoEvent = null) {
            var menu = new Menu(name, subtitle, closeCallback);

            menu.addMenuItem(new ClickMenuItem("Nein", "Abbrechen?", "", onSelectNoEvent != null ? onSelectNoEvent : "DO_NOTHING", MenuItemStyle.red).withData(data));
            menu.addMenuItem(new ClickMenuItem("Ja", "Wirklich bestätigen?", "", evt, MenuItemStyle.green).withData(data));

            return menu;
        }

        public static void fakeClickMenuEvent(IPlayer player, ClickMenuItem menuItem) {
            var data = new MenuItemCefEvent {
                evt = menuItem.Event,
                id = menuItem.Id,
                action = "click"
            };
            EventController.triggerMenuEvent(player, data.evt, menuItem.Id, menuItem.Data, data);
        }

        private class DefaultMenuWebSocketEvent {
            public int id;
            public string subEvt;
            public bool closeMenu;
        }

        private void onMenuEvent(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            if(CurrentlyDisplayedMenus.ContainsKey(player.getAccountId())) {
                var menu = CurrentlyDisplayedMenus[player.getAccountId()];
                //TODO: Check if there are any bugs. Seems not necessary?
                //if(menu.AllowMovement == true) {
                //    WebController.setMovementBlockForCef(player, "MENU", false);
                //} else {
                //    WebController.setMovementBlockForCef(player, "MENU", true);
                //}

                var cefData = new DefaultMenuWebSocketEvent();
                JsonConvert.PopulateObject(evt.Data, cefData);

                if(cefData.subEvt == "SUB_MENU_OPEN") {
                    if(cefData.id == -1) {
                        //Can be null!
                        if(menu.AllowMovement == true) {
                            WebController.setMovementBlockForCef(player, "MENU", false);
                        } else {
                            WebController.setMovementBlockForCef(player, "MENU", true);
                        }

                        return;
                    }
                }

                var item = getMenuItemById(menu, cefData.id);

                if(cefData.subEvt == "MENU_REQUEST_SUB_MENU") {
                    if(item is MenuMenuItem menuMenuItem) {
                        SubMenuAddDataCefEvent subMenudata = menuMenuItem.getSubMenuAddDataCefEvent();
                        if(subMenudata != null && subMenudata.elements != null && menuMenuItem.SubMenu != null) {
                            player.emitCefEventNoBlock(menuMenuItem.getSubMenuAddDataCefEvent());
                            menu.MenuCloseCallbacks = menu.MenuCloseCallbacks.Concat(menuMenuItem.SubMenu.MenuCloseCallbacks).Distinct().ToList();
                        }
                        return;
                    }
                } else if(cefData.subEvt == "SUBMENU_CLOSE") {
                    if(item is MenuMenuItem menuMenuItem) {
                        menuMenuItem.subMenuClosed(player);
                        return;
                    } else {
                        Logger.logError($"onMenuEvent: subEvent was SUBMENU_CLOSE, but menuItem was no MenuMenuItem: : charId: {player.getAccountId()}, menuName: {menu.Name}",
                            $"Fehler bei Menüs: Es wurde ein Submenü geschlossen aber das Menü-Item war kein Untermenü {menu.Name}", player);
                    }
                } else if(cefData.subEvt == "SUB_MENU_OPEN") {
                    if(item is MenuMenuItem menuMenuItem) {
                        if((menuMenuItem.SubMenu.AllowMovement == null && menu.AllowMovement == true) || menuMenuItem.SubMenu.AllowMovement == true) {
                            WebController.setMovementBlockForCef(player, "MENU", false);
                        } else {
                            WebController.setMovementBlockForCef(player, "MENU", true);
                        }

                        return;
                    }
                }

                if(item != null) {
                    if(cefData.closeMenu && !item.BlockMenuCloseCallback) {
                        closeMenu(player);
                    }

                    var data = item.getCefEvent(evt.Data);
                    EventController.triggerMenuEvent(player, data.evt, item.Id, item.Data, data);
                } else {
                    Logger.logError($"onMenuEvent: Player triggered menuEvent of item which was not found in menu: charId: {player.getAccountId()}, menuName: {menu.Name}",
                        $"Fehler bei Menüs: Es wurde mit einem Menü-Item interagiert, welches nicht gefunden wurde {menu.Name}", player);
                }
            } else {
                Logger.logError($"onMenuEvent: Player triggered menuEvent though he hasnt opened a menu yet. charId: {player.getAccountId()}",
                    $"Fehler bei Menüs: Es wurde mit einem Menü interagiert, welches nicht offen war", player);
            }
        }

        private MenuItem getMenuItemById(Menu menu, int id) {
            foreach(var menuItem in menu.getMenuItems()) {
                if(menuItem.Id == id) {
                    return menuItem;
                }

                if(menuItem is MenuMenuItem && (menuItem as MenuMenuItem).SubMenu != null) {
                    var item = getMenuItemById((menuItem as MenuMenuItem).SubMenu, id);
                    if(item != null) {
                        return item;
                    }
                } else {
                    if(menuItem.Id == id) {
                        return menuItem;
                    }
                }
            }

            return null;
        }

        private void onPlayerDisconnect(IPlayer player, string reason) {
            closeMenu(player);
        }
    }
}
