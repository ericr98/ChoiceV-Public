using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Model.Menu;
using Fleck;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.EventSystem {
    public interface IPlayerCefEvent {
        public string Event { get; }
    }

    public class OnlyEventCefEvent : IPlayerCefEvent {
        public string Event { get; }

        public OnlyEventCefEvent(string evt) {
            Event = evt;
        }
    }

    public class WebController : ChoiceVScript {
        private class PlayerWebSocketConnection {
            public IWebSocketConnection WebSocket;
            public IPlayer Player;

            public PlayerWebSocketConnection(IWebSocketConnection webSocket, IPlayer player) {
                WebSocket = webSocket;
                Player = player;
            }
        }

        private static WebSocketServer Server;

        private static Thread ServerThread;
        private static Dictionary<string, PlayerWebSocketConnection> PlayerWebSockets = new Dictionary<string, PlayerWebSocketConnection>();

        private static string Url = Config.WebSocketIp;
        private static string CefHostUrl = Config.CefIp; //http://www.choicev-cef.net/cef/

        public WebController() {
            ServerThread = new Thread(new ThreadStart(runServer));
            ServerThread.Start();

            //EventController.PlayerSuccessfullConnectionDelegate += initPlayerWebSocketConnection;
            EventController.PlayerDisconnectedDelegate += onPlayerDisconnect;

            //Add move keys for Cef Close here
            EventController.addKeyEvent("CLOSE_HUD", ConsoleKey.Delete, "HUD Element schließen", onCefClose, true, true);
            EventController.addKeyEvent("HIDE_HUD", ConsoleKey.F7, "HUD anzeigen/verstecken", onToggleHud, true);
            EventController.addEvent("OTHER_CEF_CLOSED", onOtherCefClosed);
            
            InvokeController.AddTimedInvoke("PlayerPinger", (i) => {
                foreach(var p in ChoiceVAPI.GetAllPlayers()) {
                    if(p.hasData(DATA_ACCOUNT_ID) && !p.hasState(PlayerStates.InCharCreator)) {
                        p.emitCefEventNoBlock(new OnlyEventCefEvent("PING_CEF"));
                    }
                }
            }, TimeSpan.FromSeconds(7.5), true);

            InteractionController.addObjectInteractionCallback(
                "MONITOR_INTERACT",
                "Aktensystem öffnen",
                onMonitorInteract
            );
        }

        private bool onCefClose(IPlayer player, ConsoleKey key, string eventName) {
            sendClosePlayerCefElements(player, new OnlyEventCefEvent("CLOSE_CEF"));
            EventController.OnCefCloseDelegate?.Invoke(player);
            return true;
        }

        public static bool closePlayerCef(IPlayer player) {
            sendClosePlayerCefElements(player, new OnlyEventCefEvent("CLOSE_CEF"));
            return true;
        }

        private bool onToggleHud(IPlayer player, ConsoleKey key, string eventName) {
            player.emitCefEventNoBlock(new OnlyEventCefEvent("TOGGLE_HUD"));
            return true;
        }

        public static void sendClosePlayerCefElements(IPlayer player, IPlayerCefEvent toSendEvent) {
            //If player has socket and closeEvent is triggered, check if player can move again
            if(PlayerWebSockets.ContainsKey(player.getAccountToken()) && player.getCharacterFullyLoaded()) {
                removeMovementBlockForCef(player);
            }

            //MenuController.CurrentlyDisplayedMenus.Remove(player.getAccountId());
            player.emitCefEventNoBlock(toSendEvent);

        }

        public static void initPlayerWebSocketConnection(IPlayer player) {
            //Try to init the player websocket connection
            player.emitClientEvent("INIT_WEBSOCKET", player.getAccountId(), Url, CefHostUrl, -1, player.getAccountToken());
        }

        private void onPlayerDisconnect(IPlayer player, string reason) {
            var accountId = player.getAccountToken();
            //Remove websocket on player disconnect
            if(accountId!= null && PlayerWebSockets.ContainsKey(accountId)) {
                var socket = PlayerWebSockets[player.getAccountToken()];
                socket.WebSocket.Send(new OnlyEventCefEvent("CLOSE_CONNECTION").ToJson());
                socket.WebSocket.Close();
                PlayerWebSockets.Remove(player.getAccountToken());
            } else {
                Logger.logWarning(LogCategory.Player, LogActionType.Event, player, "Player disconnected without having a WebSocket Connection! accountId: " + player.getAccountId());
            }
        }

        public static void runServer() {
            //Run the server. Is in a different Thread, just in case ;)
            Server = new WebSocketServer(Url);
            Server.ListenerSocket.NoDelay = true;
            Server.RestartAfterListenError = true;
            Server.Start(socket => {
                socket.OnOpen = () => Logger.logDebug(LogCategory.System, LogActionType.Created, "New WebSocket opened!");
                socket.OnClose = () => Console.WriteLine("A WebSocket was closed!"); //TODO Remove From PlayerWebSockets
                socket.OnMessage = message => handleSocketMessage(socket, message);
                socket.OnError = exception => handleSocketException(socket, exception);
            });
        }
        
        private static void handleSocketMessage(IWebSocketConnection socket, string message) {
            try {
                //Serialize data for usage
                var data = JsonConvert.DeserializeObject<PlayerWebSocketConnectionDataElement>(message);

                //if this is the first handshake add player socket to list
                if(!PlayerWebSockets.ContainsKey(data.LoginToken)) {
                    var player = ChoiceVAPI.GetAllPlayersIncludeNotFullyLoaded().FirstOrDefault(p => p.getAccountToken() == data.LoginToken);
                    if(player != null) {
                        PlayerWebSockets.Add(data.LoginToken, new PlayerWebSocketConnection(socket, player));
                    } else {
                        return;
                    }
                }

                //Initial Handshake Event
                if(data.Event == "INIT_WEBSOCKET") {
                    if(PlayerWebSockets.ContainsKey(data.LoginToken)) {
                        var socketO = PlayerWebSockets[data.LoginToken];
                        if(MissedEventQueue.ContainsKey(data.Id)) {
                            var list = MissedEventQueue[data.Id].Clone();
                            MissedEventQueue[data.Id].Clear();
                            foreach(var item in list) {
                                if(item.BlockMovement) {
                                    socketO.Player.emitCefEventWithBlock(item.Event, item.BlockMovementIdentifier);
                                } else {
                                    socketO.Player.emitCefEventNoBlock(item.Event);
                                }
                                
                            }
                        }
                    }

                    return;
                }

                //check if movement is allowed again
                var socketObj = PlayerWebSockets[data.LoginToken];
                if(data.ReleaseMovement) {
                    setMovementBlockForCef(socketObj.Player, data.MovementBlockedIdentifier, false);
                }

                //trigger the resulting event
                var playerLock = EventController.PlayerLocks[socketObj.Player.NativePointer];
                lock(playerLock) {
                    var thread = new Thread(() => {
                        try {
                            EventController.triggerCefEvent(socketObj.Player, data);
                        } catch(Exception e) {
                            Logger.logException(e, "handleSocketMessage");
                        }
                    });

                    thread.Start();
                }
            } catch(Exception e) {
                Logger.logException(e, "handleSocketMessage");
            }
        }

        private class EventQueueElement : ICloneable {
            public IPlayerCefEvent Event;
            public bool BlockMovement;
            public string BlockMovementIdentifier;

            public EventQueueElement(IPlayerCefEvent evt, bool blockMovement, string blockMovementIdentifier) {
                Event = evt;
                BlockMovement = blockMovement;
                BlockMovementIdentifier = blockMovementIdentifier;
            }

            public object Clone() {
                return new EventQueueElement(Event, BlockMovement, BlockMovementIdentifier);
            }
        }

        private static Dictionary<int, List<EventQueueElement>> MissedEventQueue = new Dictionary<int, List<EventQueueElement>>();

        //function to send a cef event to the player
        public static bool emitCefEvent(IPlayer player, IPlayerCefEvent data, bool blockMovement, string blockMovementIdentifier = "NONE") {
            var accountId = player.getAccountToken();
            if(accountId != null && PlayerWebSockets.ContainsKey(accountId)) {
                lock(PlayerWebSockets[player.getAccountToken()]) {
                    var message = data.ToJson();
                    var socket = PlayerWebSockets[player.getAccountToken()];
                    socket.WebSocket.Send(message);

                    // block player movement if necessary
                    if(blockMovement) {
                        setMovementBlockForCef(player, blockMovementIdentifier, true);
                    }
                }
                return true;
            } else {
                player.emitClientEvent("INIT_WEBSOCKET", player.getAccountId(), Url, CefHostUrl, 7500, player.getAccountToken());
                Logger.logTrace(LogCategory.Player, LogActionType.Event, player, "Player had no WebSocket! accId: " + player.getAccountId());

                var accId = player.getAccountId();
                var elem = new EventQueueElement(data, blockMovement, blockMovementIdentifier);

                if(MissedEventQueue.ContainsKey(accId)) {
                    var list = MissedEventQueue[accId];
                    list.Add(elem);
                } else {
                    var newList = new List<EventQueueElement> {
                        elem
                    };
                    MissedEventQueue.Add(accId, newList);
                }
                return false;
            }
        }

        //tries to reconnect to player on socket error!
        private static void handleSocketException(IWebSocketConnection socket, Exception exception) {
            try {
                var playerSocket = PlayerWebSockets.FirstOrDefault(pw => pw.Value.WebSocket == socket);
                var player = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.getAccountToken() == playerSocket.Key);

                if(player != null) {
                    if(exception is System.IO.IOException) {
                        Logger.logDebug(LogCategory.Player, LogActionType.Event, player, "Player WebSocket was disconnected! Trying to reconnect!");
                    } else {
                        Logger.logDebug(LogCategory.Player, LogActionType.Event, player, "Player WebSocket was disconnected! Waiting 30sec for reconnection!");
                    }

                    PlayerWebSockets.Remove(player.getAccountToken());

                    // Send new SocketConnection to Player
                    player.emitClientEvent("INIT_WEBSOCKET", player.getAccountId(), Url, CefHostUrl, 3500, player.getAccountToken());

                    InvokeController.AddTimedInvoke("Player-WebSocket-Reconnection-Trial", (ii) => {
                        tryPlayerWebSocketReconnect(player);
                    }, TimeSpan.FromSeconds(15), false);
                }
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        //checks if reconnect worked
        private static void tryPlayerWebSocketReconnect(IPlayer player) {
            if(player.Exists()) {
                if(player != null && PlayerWebSockets.ContainsKey(player.getAccountToken())) {
                    Logger.logInfo(LogCategory.Player, LogActionType.Event, player, "Player WebSocket reconnect worked.");
                } else {
                    ChoiceVAPI.KickPlayer(player, "Colorfriend", "Die CEF Verbindung wurde zu lange unterbrochen. Melde dich beim Entwicklungsteam!", "Die CEF Verbindung wurde zu lange unterbrochen");
                }
            }
        }

        private void onMonitorInteract(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading, bool isBroken, ref Menu menu) {
            openFileSystem(player);
        }

        public static void openFileSystem(IPlayer player) {
            var key = player.getAccountToken();
            //var encryptData = mockObject.encrypt(key);
            if(CompanyController.getCompanies(player).Count > 0) {
                player.emitClientEvent("OTHER_CEF", Config.FsIp + $"?userId={player.getCharacterId()}&token={key}");
                setMovementBlockForCef(player, "FILE_SYSTEM", true);
            }
        }
        
        private bool onOtherCefClosed(IPlayer player, string eventname, object[] args) {
            setMovementBlockForCef(player, "FILE_SYSTEM", false);
            player.emitClientEvent("FOCUS_ON_CEF");
            
            return true;
        }
        
        public class DebugInfoCefEvent : IPlayerCefEvent {
            public string Event { get; }
            public string key { get; }
            public string value { get; }

            public DebugInfoCefEvent(string evt, string key, string value) {
                Event = evt;
                this.key = key;
                this.value = value;
            }
        }

        public static void displayDebugInfo(IPlayer player, string key, string info) {
            player.emitCefEventNoBlock(new DebugInfoCefEvent("SET_DEBUG_INFO", key, info));
        }

        public static void removeDebugInfo(IPlayer player, string key) {
            player.emitCefEventNoBlock(new DebugInfoCefEvent("REMOVE_DEBUG_INFO", key, ""));
        }

        public static void setMovementBlockForCef(IPlayer player, string identifier, bool block) {
            player.emitClientEvent("SET_CEF_BLOCK_MOVEMENT", identifier, block);
        }
        
        public static void removeMovementBlockForCef(IPlayer player) {
            player.emitClientEvent("RESET_CEF_BLOCK_MOVEMENT");
        }
    }
}
