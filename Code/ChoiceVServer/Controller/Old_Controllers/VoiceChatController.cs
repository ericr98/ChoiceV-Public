//using AltV.Net;
//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Model.Menu;
//using System;
//using System.Collections.Generic;
//using System.Linq;
////
//namespace ChoiceVServer.Controller {
//    class VoiceChatController : ChoiceVScript {
//        private static int counter = 0;
//        public static List<VoiceRadio> radioList = new List<VoiceRadio>();
//        public static List<Phone> phoneList = new List<Phone>();
//        public static List<RangePlayer> rangeList = new List<RangePlayer>();
//        public static List<Speaker> activatedSpeaker = new List<Speaker>();
//        public static List<IPlayer> activeSend = new List<IPlayer>();
//        public static List<IPlayer> mutedPlayer = new List<IPlayer>();
//        public static List<IPlayer> connectedPlayer = new List<IPlayer>();

//        public VoiceChatController() {
//            EventController.PlayerSuccessfullConnectionDelegate += OnPlayerConnected;
//            EventController.PlayerDisconnectedDelegate += OnPlayerDisconnect;

//            EventController.PlayerDeadDelegate += onPlayerDead;


//            EventController.addKeyEvent(ConsoleKey.Add, ChangeVoiceRange);

//            EventController.addEvent("VOICE_RADIO_SEND_START_PRIMARY", OnRadioSend);
//            EventController.addEvent("VOICE_RADIO_SEND_START_SECONDARY", OnRadioSend);

//            EventController.addEvent("VOICE_RADIO_SEND_STOP_SECONDARY", RadioSendStop);
//            EventController.addEvent("VOICE_RADIO_SEND_STOP_PRIMARY", RadioSendStop);

//            EventController.addEvent("VOICE_NOT_CONNECTED", OnVoiceNotConnected);

//            EventController.addEvent("VOICE_MIC_MUTED", onMicMuted);
//            EventController.addEvent("VOICE_SOUND_MUTED", onSoundMuted);

//            EventController.addMenuEvent("RADIO_JOIN", JoinRadio);
//            EventController.addMenuEvent("RADIO_LEAVE", OnLeaveRadio);
//            EventController.addMenuEvent("RADIO_VOL", VolumeRadio);
//            EventController.addMenuEvent("RADIO_SPEAKER", RadioMode);

//            EventController.addMenuEvent("START_CALL", StartCall);
//            EventController.addMenuEvent("STOP_CALL", StopCall);
//            EventController.addMenuEvent("CALL_VOLUME", CallVolume);
//            EventController.addMenuEvent("CALL_SPEAKER", CallSpeaker);

//            onStartUp();
//        }

//        private void onPlayerDead(IPlayer player, IEntity killer, uint weapon) {
//            ChoiceVAPI.emitClientEventToAll("SaltyChat_PlayerDied", player);
//        }

//        #region events

//        private bool OnVoiceNotConnected(IPlayer player, string eventName, object[] args) {
//            player.setTimeCycle("NeutralColorCode", 50f);
//            player.sendBlockNotification("Bitte überprüfe dein Saltychat und reconnecte!", "SaltyChat");
//            return true;
//        }

//        public static void onStartUp() {
//            if (Config.EnableSaltyChat == 1) {
//                InvokeController.AddTimedInvoke("VoiceConnectedCheck", (ivk) => {
//                    foreach (var player in connectedPlayer) {
//                        player.emitClientEvent("Voice_Connect_Check");
//                    }
//                }, TimeSpan.FromMinutes(3), true);
//            }
//        }
//        private bool onSoundMuted(IPlayer player, string eventName, object[] args) {
//            var check = (bool)args[0];
//            if (check) {
//                player.sendNotification(Constants.NotifactionTypes.Warning, "Bitte aktiviere deinen Sound oder du wirst gekickt!", "");
//            }

//            return true;
//        }

//        private bool onMicMuted(IPlayer player, string eventName, object[] args) {
//            var check = (bool)args[0];
//            if (check) {
//                voiceRange(player, 0);
//                var muteCheck = mutedPlayer.FirstOrDefault(x => x == player);
//                if (muteCheck == null) {
//                    mutedPlayer.Add(player);
//                }
//            } else {
//                var currentRange = rangeList.FirstOrDefault(x => x.player == player);
//                var muteCheck = mutedPlayer.FirstOrDefault(x => x == player);
//                if (muteCheck != null) {
//                    mutedPlayer.Remove(muteCheck);
//                    if (currentRange != null) {
//                        voiceRange(player, currentRange.range);
//                    } else {
//                        voiceRange(player, 2);
//                    }
//                }

//            }
//            return true;
//        }

//        private bool OnRadioSend(IPlayer player, string eventName, object[] args) {
//            if (eventName == "VOICE_RADIO_SEND_START_SECONDARY") {
//                if (getRadioChannel(player, false) == "") {
//                    return true;
//                } else {
//                    startSendingOnRadio(player, false);
//                }

//            }
//            if (eventName == "VOICE_RADIO_SEND_START_PRIMARY") {
//                if (getRadioChannel(player, true) == "") {
//                    return true;
//                } else {
//                    startSendingOnRadio(player, true);
//                }

//            }
//            return true;
//        }


//        #endregion

//        #region RemoteMethods

//        #region Megaphone

//        public static void enableMegaphoneEffect(IPlayer player, int range, int volume) {
//            SaltyChatServer.VoiceManager.OnStartMegaphone(player, range, volume);
//        }

//        public static void disableMegaphoneEffect(IPlayer player) {
//            SaltyChatServer.VoiceManager.OnStopMegaphone(player);
//        }

//        #endregion

//        #region Phone

//        public static void CallManager(IPlayer player, IPlayer target) { //Player ist Anrufer; Target ist angerufener
//            var callId = 0;
//            if (GetCallStatus(player) == true && GetCallStatus(target) == false) { //WENN ANRUFER IM TELEFONAT
//                StartCall(player, target);
//                StartCall(target, player);
//                var check = phoneList.FirstOrDefault(x => x.member.getCharacterId() == player.getCharacterId());
//                callId = check.ID;
//                var phone = new Phone {
//                    ID = callId,
//                    member = target,
//                };
//                phoneList.Add(phone);
//                foreach (var clients in phoneList) {
//                    if (clients.ID == callId) {
//                        if (clients.member.getCharacterId() == player.getCharacterId()) {
//                            continue;
//                        } else {

//                            StartCall(clients.member, target);
//                            StartCall(target, clients.member);
//                        }
//                    }
//                }
//                return;
//            }

//            if (GetCallStatus(target) == true && GetCallStatus(player) == false) { //WENN ANGERUFENER IM TELEFONAT
//                StartCall(player, target);

//                StartCall(target, player);

//                var check = phoneList.FirstOrDefault(x => x.member.getCharacterId() == target.getCharacterId());
//                callId = check.ID;
//                var phone = new Phone {
//                    ID = callId,
//                    member = player,
//                };
//                phoneList.Add(phone);
//                foreach (var clients in phoneList) {
//                    if (clients.ID == callId) {
//                        if (clients.member.getCharacterId() == target.getCharacterId()) {
//                            continue;
//                        } else {

//                            StartCall(clients.member, player);

//                            StartCall(player, clients.member);

//                        }
//                    }
//                }
//                return;
//            }
//            if (GetCallStatus(player) == false && GetCallStatus(target) == false) { //Wenn Keiner im Telefonat
//                //Start Call
//                StartCall(player, target);

//                StartCall(target, player);

//                var phone = new Phone {
//                    ID = counter,
//                    member = player,
//                };
//                var phone2 = new Phone {
//                    ID = counter,
//                    member = target,
//                };
//                counter++;
//                phoneList.Add(phone);
//                phoneList.Add(phone2);
//                return;
//            }

//            if (GetCallStatus(player) && GetCallStatus(target)) {
//                player.sendNotification(Constants.NotifactionTypes.Info, "Die Person befindet sich in einer anderen Konferenz!", "Andere Konferenz");
//                return;
//            }

//        }
//        public static void stopCall(IPlayer player) {
//            var callId = 0;
//            var counter = 0;
//            var callCheck = phoneList.FirstOrDefault(x => x.member.getCharacterId() == player.getCharacterId());
//            if (callCheck != null) {
//                phoneList.Remove(callCheck);
//                callId = callCheck.ID;
//                foreach (var call in phoneList) {
//                    if (call.ID == callId) {
//                        call.member.sendNotification(Constants.NotifactionTypes.Info, $"Jemand hat die Sitzung verlassen", "");
//                        player.emitClientEvent("SaltyChat_EndCall", call.member.Id);
//                        counter++;
//                    }
//                }
//                if (counter == 1) {
//                    phoneList.RemoveAll(x => x.ID == callId);
//                }
//            }
//            Alt.EmitAllClients("SaltyChat_EndCall", player.Id);
//        }

//        public static void SetPhoneSpeaker(IPlayer player, bool toggle) {
//            var callId = 0;
//            if (toggle) {
//                var speakercheck = activatedSpeaker.FirstOrDefault(x => x.player.getCharacterId() == player.getCharacterId());
//                if (speakercheck == null) {
//                    var speaker = new Speaker {
//                        player = player,
//                        speaker = toggle
//                    };
//                    activatedSpeaker.Add(speaker);
//                    var client = phoneList.FirstOrDefault(x => x.member.getCharacterId() == player.getCharacterId());
//                    if (client != null) {
//                        callId = client.ID;
//                        foreach (var clients in phoneList) {
//                            if (clients.member.getCharacterId() == player.getCharacterId()) {
//                                continue;
//                            } else if (clients.ID == callId) {
//                                StartCall(player, clients.member);
//                                StartCall(clients.member, player);
//                            }
//                        }
//                    }
//                } else {
//                    player.sendNotification(Constants.NotifactionTypes.Info, "Deine Lautsprecher sind bereits an!", "");
//                }
//            } else {
//                var speakercheck = activatedSpeaker.FirstOrDefault(x => x.player.getCharacterId() == player.getCharacterId());
//                if (speakercheck != null) {
//                    activatedSpeaker.Remove(speakercheck);
//                    foreach (var clients in phoneList) {
//                        if (clients.member.getCharacterId() == player.getCharacterId()) {
//                            continue;
//                        } else if (clients.ID == callId) {
//                            Alt.EmitAllClients("SaltyChat_EndCall", clients.member.Id);
//                            StartCall(player, clients.member);
//                            StartCall(clients.member, player);
//                        }
//                    }
//                }
//            }
//        }

//        public static void setPhoneVolume(IPlayer player, float volume) {
//            player.Emit("Call_Volume", volume);
//            var callId = 0;
//            var check = phoneList.FirstOrDefault(x => x.member.getCharacterId() == player.getCharacterId());
//            if (check != null) { //TODO FIX
//                callId = check.ID;
//            }
//            foreach (var clients in phoneList) {
//                if (clients.member.getCharacterId() == player.getCharacterId()) {
//                    continue;
//                } else if (clients.ID == callId) {
//                    if (clients.member.getCharacterId() == player.getCharacterId()) {
//                        continue;
//                    } else if (clients.ID == callId) {
//                        StartCall(player, clients.member);
//                    }
//                }
//            }
//            player.sendNotification(Constants.NotifactionTypes.Info, "Deine Telefonlautstärke wurde auf " + volume.ToString() + "% gestellt!", "Telefonlautstärke");
//        }
//        #endregion

//        #region radio


//        public static void JoinRadio(IPlayer player, string channelName, bool primaryRadio, bool showNotification) {
//            var obj = radioList.FirstOrDefault(x => x.CharID == player.getCharacterId());
//            if (obj != null) {
//                if (primaryRadio) {
//                    if (obj.radio == "") {
//                        if (showNotification) {
//                            player.sendNotification(Constants.NotifactionTypes.Info, $"Dein Primärer Channel ist jetzt {channelName}", "Funk Wechsel");
//                        }
//                        obj.radio = channelName;
//                    } else {
//                        if (showNotification) {
//                            player.sendNotification(Constants.NotifactionTypes.Info, $"Dein Primärer Channel war {obj.radio} und ist jetzt: {channelName}", "Funk Wechsel");
//                        }
//                        obj.radio = channelName;
//                    }
//                    LeaveRadio(player, true);
//                    //player.Emit("JOIN_RADIO", channelName);
//                    SaltyChatServer.VoiceManager.OnJoinRadioChannel(player, channelName);

//                } else {
//                    if (obj.radio2 == "") {
//                        if (showNotification)
//                            player.sendNotification(Constants.NotifactionTypes.Info, $"Dein Sekundärer Channel ist jetzt {channelName}", "Funk Wechsel");
//                        obj.radio2 = channelName;
//                    } else {
//                        if (showNotification)
//                            player.sendNotification(Constants.NotifactionTypes.Info, $"Dein Sekundärer Channel war {obj.radio2} und ist jetzt: {channelName}", "Funk Wechsel");
//                        obj.radio2 = channelName;
//                    }
//                    LeaveRadio(player, false);
//                    //player.Emit("JOIN_RADIO", channelName);
//                    SaltyChatServer.VoiceManager.OnJoinRadioChannel(player, channelName);
//                }

//            } else {
//                var voiceradio = new VoiceRadio {
//                    CharID = player.getCharacterId(),
//                    radio = "",
//                    radio2 = "",
//                    type = "Longrange",
//                };
//                if (primaryRadio) {
//                    voiceradio.radio = channelName;
//                    if (showNotification)
//                        player.sendNotification(Constants.NotifactionTypes.Info, $"Dein Primärer Channel ist jetzt {channelName}", "Funk Wechsel");
//                } else {
//                    voiceradio.radio2 = channelName;
//                    if (showNotification)
//                        player.sendNotification(Constants.NotifactionTypes.Info, $"Dein Sekundärer Channel ist jetzt {channelName}", "Funk Wechsel");
//                }
//                radioList.Add(voiceradio);
//                //player.emitClientEvent("JOIN_RADIO", channelName);
//                SaltyChatServer.VoiceManager.OnJoinRadioChannel(player, channelName);
//            }
//        }

//        public static void LeaveRadio(IPlayer player, bool primaryRadio) {
//            var obj = radioList.FirstOrDefault(x => x.CharID == player.getCharacterId());
//            if (obj != null) {
//                if (primaryRadio) {
//                    //player.Emit("LEAVE_RADIO", obj.radio);
//                    SaltyChatServer.VoiceManager.OnLeaveRadioChannel(player, obj.radio);
//                } else {
//                    //player.Emit("LEAVE_RADIO", obj.radio2);
//                    SaltyChatServer.VoiceManager.OnLeaveRadioChannel(player, obj.radio2);
//                }
//                return;
//            } else {
//                return;
//            }
//        }


//        public static void setRadioVolume(IPlayer player, float volume) {
//            var check = radioList.FirstOrDefault(x => x.radio != "");
//            if (check != null) { //TODO FIX
//                var radio = check.radio;
//            }

//            player.sendNotification(Constants.NotifactionTypes.Info, "Lautstärke wurde auf " + volume + " % gestellt!", "Funklautstärle auf " + volume);
//            player.emitClientEvent("CLIENT_VOLUME", volume);
//        }

//        public static void setRadioSpeaker(IPlayer player, bool toggle) {
//            if (toggle) {
//                SaltyChatServer.VoiceManager.OnSetRadioSpeaker(player, "true");
//            } else {
//                SaltyChatServer.VoiceManager.OnSetRadioSpeaker(player, "false");
//            }
//        }

//        #endregion

//        #endregion

//        #region Helper

//        public static void startSendingOnRadio(IPlayer player, bool primary) {
//            var primaryChannel = getRadioChannel(player, true);
//            var secondaryChannel = getRadioChannel(player, false);
//            player.emitClientEvent("PLAY_ANIM", "random@arrests", "generic_radio_chatter", 99999999, 49, -1);
//            if (primary) {
//                //player.Emit("SEND_RADIO_STATUS_START", primaryChannel);
//                SaltyChatServer.VoiceManager.OnSendingOnRadio(player, primaryChannel, true);
//                LeaveRadio(player, false);
//                activeSend.Add(player);
//                foreach (var target in activeSend) {
//                    if (target.Id == player.Id) {
//                        continue;
//                    } else {
//                        player.Emit("SaltyChat_IsSending", target, false);
//                    }
//                }
//            } else {
//                //player.Emit("SEND_RADIO_STATUS_START", secondaryChannel);
//                SaltyChatServer.VoiceManager.OnSendingOnRadio(player, secondaryChannel, true);
//                LeaveRadio(player, true);
//                activeSend.Add(player);
//                foreach (var target in activeSend) {
//                    if (target.Id == player.Id) {
//                        continue;
//                    } else {
//                        player.Emit("SaltyChat_IsSending", target, false);
//                    }
//                }
//            }
//        }
//        public static void stopSendOnRadio(IPlayer player, bool primary) {
//            var primaryChannel = getRadioChannel(player, true);
//            var secondaryChannel = getRadioChannel(player, false);
//            if (primary) {
//                JoinRadio(player, secondaryChannel, false, false);
//                //player.Emit("SEND_RADIO_STATUS_STOP", primaryChannel);
//                SaltyChatServer.VoiceManager.OnSendingOnRadio(player, primaryChannel, false);
//            } else {
//                JoinRadio(player, primaryChannel, true, false);
//                //player.Emit("SEND_RADIO_STATUS_STOP", secondaryChannel);
//                SaltyChatServer.VoiceManager.OnSendingOnRadio(player, secondaryChannel, false);
//            }
//        }

//        private bool OnLeaveRadio(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            LeaveRadio(player, true);
//            return true;
//        }
//        private bool JoinRadio(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            string id = data["RADIO_ID"];
//            JoinRadio(player, id, true, true);
//            return true;
//        }
//        public static bool GetSpeakerStatus(IPlayer player) {

//            var speakercheck = activatedSpeaker.FirstOrDefault(x => x.player.getCharacterId() == player.getCharacterId());
//            if (speakercheck != null && speakercheck.speaker == true) {
//                return true;
//            }
//            return false;
//        }

//        public static void StartCall(IPlayer player, IPlayer target) {
//            if (GetSpeakerStatus(player) == true) {
//                player.emitClientEvent("SaltyChat_EndCall", target.Id);
//                //Alt.Emit("CALL_RELAYED", player, target);
//                SaltyChatServer.VoiceManager.CallRelayed(player, target);
//            } else {
//                player.emitClientEvent("SaltyChat_EndCall", target.Id);
//                //Alt.Emit("CALL_NORMAL", player, target);
//                SaltyChatServer.VoiceManager.CallNormal(player, target);
//            }
//        }

//        private bool CallSpeaker(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var check = data["CHECK"];
//            SetPhoneSpeaker(player, check);
//            return true;
//        }

//        private void OnPlayerDisconnect(IPlayer player, string reason) {
//            if (GetCallStatus(player) == true) {
//                var check = phoneList.FirstOrDefault(x => x.member.getCharacterId() == player.getCharacterId());
//                if (check != null) {
//                    phoneList.Remove(check);
//                }
//            }
//            var phoneListCheck = phoneList.FirstOrDefault(x => x.member == player);
//            var rangeListCheck = rangeList.FirstOrDefault(x => x.player == player);
//            var activatedSpeakerCheck = activatedSpeaker.FirstOrDefault(x => x.player == player);
//            var activeSendCheck = activeSend.FirstOrDefault(x => x == player);
//            var muteCheck = mutedPlayer.FirstOrDefault(x => x == player);
//            var playerCheck = connectedPlayer.FirstOrDefault(x => x == player);

//            if (phoneListCheck != null) {
//                phoneList.Remove(phoneListCheck);
//            }
//            if (rangeListCheck != null) {
//                rangeList.Remove(rangeListCheck);
//            }
//            if (activatedSpeakerCheck != null) {
//                activatedSpeaker.Remove(activatedSpeakerCheck);
//            }
//            if (activeSendCheck != null) {
//                activeSend.Remove(activeSendCheck);
//            }
//            if (muteCheck != null) {
//                mutedPlayer.Remove(muteCheck);
//            }
//            if (playerCheck != null) {
//                connectedPlayer.Remove(player);
//            }
//        }

//        private bool CallVolume(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            float volume;
//            ListMenuItem.ListMenuItemEvent listItem = menuItemCefEvent as ListMenuItem.ListMenuItemEvent;
//            var volumeString = listItem.currentElement;
//            volume = float.Parse(volumeString);
//            setPhoneVolume(player, volume);
//            return true;
//        }

//        internal static void CallMenu(IPlayer player) {
//            var menu = new Menu("TEFLON", "Wähle deinen partner!");
//            var playerlist = ChoiceVAPI.GetAllPlayers();
//            foreach (var target in playerlist) {
//                if (target == player) {
//                    continue;
//                }
//                if (target.Position.Distance(player.Position) <= 10) {
//                    menu.addMenuItem(new ClickMenuItem(target.Name + " anrufen", "", "", "START_CALL").withData(new Dictionary<string, dynamic> { { "CALL", target }, { "START", true } }));
//                }
//            }
//            menu.addMenuItem(new ClickMenuItem("Auflegen", "", "", "STOP_CALL"));
//            menu.addMenuItem(new ListMenuItem("Lautstärke ändern", "", new string[] { "10", "20", "30", "40", "50", "60", "70", "80", "90", "100", }, "CALL_VOLUME"));
//            menu.addMenuItem(new ClickMenuItem("Lautsprecher an", "", "", "CALL_SPEAKER").withData(new Dictionary<string, dynamic> { { "CHECK", true } }));
//            menu.addMenuItem(new ClickMenuItem("Lautsprecher aus", "", "", "CALL_SPEAKER").withData(new Dictionary<string, dynamic> { { "CHECK", false } }));

//            player.showMenu(menu);
//        }
//        private bool StartCall(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var target = data["CALL"];
//            var check = data["START"];
//            CallManager(player, target);
//            return true;
//        }
//        private bool StopCall(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            stopCall(player);
//            return true;
//        }

//        public static int getCallAmount(IPlayer player) {
//            var check = phoneList.FirstOrDefault(x => x.member.Id == player.Id);
//            var amount = 0;
//            if (check != null) {
//                foreach (var count in phoneList) {
//                    if (count.ID == check.ID) {
//                        amount++;
//                    }
//                }
//            }
//            return amount;
//        }



//        private static bool GetCallStatus(IPlayer player) {
//            var check = phoneList.FirstOrDefault(x => x.member.getCharacterId() == player.getCharacterId());
//            if (check != null) {
//                return true;
//            } else {
//                return false;
//            }
//        }

//        private bool RadioMode(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var check = data["CHECK"];
//            if (check == true) {
//                //player.Emit("SPEAKER_CASE", "true");
//                SaltyChatServer.VoiceManager.OnSetRadioSpeaker(player, "true");
//            } else {
//                //player.Emit("SPEAKER_CASE", "false");
//                SaltyChatServer.VoiceManager.OnSetRadioSpeaker(player, "false");
//            }
//            return true;
//        }

//        private bool VolumeRadio(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            ListMenuItem.ListMenuItemEvent listItem = menuItemCefEvent as ListMenuItem.ListMenuItemEvent;
//            var volumeString = listItem.currentElement;
//            float volume = float.Parse(volumeString);
//            setRadioVolume(player, volume);
//            return true;


//        }

//        //TODO Julius Make private static int Counter
//        //TODO More than one person in one call

//        public static void RadioMenu(IPlayer player) {
//            var radioMenu = new Menu("Long Range Funkmenü", "wähle deinen Funkkanal");
//            radioMenu.addMenuItem(new ClickMenuItem("Funk 1", "", "", "RADIO_JOIN").withData(new Dictionary<string, dynamic> { { "RADIO_ID", "Funk1" } }));
//            radioMenu.addMenuItem(new ClickMenuItem("Funk 2", "", "", "RADIO_JOIN").withData(new Dictionary<string, dynamic> { { "RADIO_ID", "Funk2" } }));
//            radioMenu.addMenuItem(new ClickMenuItem("Funk 3", "", "", "RADIO_JOIN").withData(new Dictionary<string, dynamic> { { "RADIO_ID", "Funk3" } }));
//            radioMenu.addMenuItem(new ClickMenuItem("Funk verlassen", "", "", "RADIO_LEAVE"));
//            radioMenu.addMenuItem(new ListMenuItem("Funk Lautstärke", "Zwischen 1 und 100 %", new string[] { "10", "20", "30", "40", "50", "60", "70", "80", "90", "100", }, "RADIO_VOL")); ;
//            radioMenu.addMenuItem(new ClickMenuItem("Lautsprecher an", "", "", "RADIO_SPEAKER").withData(new Dictionary<string, dynamic> { { "CHECK", true } }));
//            radioMenu.addMenuItem(new ClickMenuItem("Lautsprecher aus", "", "", "RADIO_SPEAKER").withData(new Dictionary<string, dynamic> { { "CHECK", false } }));


//            player.showMenu(radioMenu);
//        }
//        private bool RadioSendStop(IPlayer player, string eventName, object[] args) {
//            player.stopAnimation();
//            if (eventName == "VOICE_RADIO_SEND_STOP_PRIMARY") {
//                if (getRadioChannel(player, true) == "") {
//                    return true;
//                }
//                stopSendOnRadio(player, true);
//            }
//            if (eventName == "VOICE_RADIO_SEND_STOP_SECONDARY") {
//                if (getRadioChannel(player, false) == "") {
//                    return true;
//                }
//                stopSendOnRadio(player, false);
//            }
//            return true;
//        }

//        private bool ChangeVoiceRange(IPlayer player, ConsoleKey key, string eventName) {
//            var range = 0;
//            var check = rangeList.FirstOrDefault(x => x.player.getCharacterId() == player.getCharacterId());
//            if (check != null) {
//                range = check.range;
//                if (range == 2 || range == 4 || range == 8 || range == 16) {
//                    if (range >= 16) {
//                        range = 2;
//                        check.range = 2;
//                    } else {
//                        range = range * 2;
//                        check.range = range;
//                    }
//                }
//                voiceRange(player, range);
//                //player.Emit("Voice_Range", range);
//                SaltyChatServer.VoiceManager.OnSetVoiceRange(player, range);
//            }

//            return true;

//        }

//        public static string getRadioChannel(IPlayer player, bool primary) {
//            var channelCheck = radioList.FirstOrDefault(x => x.CharID == player.getCharacterId());
//            var channel = "";
//            if (channelCheck != null) {
//                if (primary) {
//                    channel = channelCheck.radio;
//                } else {
//                    channel = channelCheck.radio2;
//                }
//            } else {
//            }
//            return channel;
//        }

//        private static void OnPlayerConnected(IPlayer player, characters character) {
//            SaltyChatServer.VoiceManager.OnPlayerConnected(player, player.Name);
//            var range = new RangePlayer {
//                player = player,
//                range = 2,
//            };
//            voiceRange(player, 2);
//            rangeList.Add(range);
//            if (!(player.hasData("Radio_Headphones"))) {
//                setRadioSpeaker(player, true);
//            }
//            connectedPlayer.Add(player);

//        }

//        internal static void CallCheck(IPlayer player) {
//            foreach (var check in phoneList) {
//                player.sendNotification(Constants.NotifactionTypes.Info, "ID: " + check.ID + "NAME: " + check.member.Name, "");
//            }
//        }

//        public static void voiceRange(IPlayer player, int voiceRange) {
//            var send = 1;
//            if (voiceRange == 0) { send = 0; }
//            if (voiceRange == 2) { send = 1; }
//            if (voiceRange == 4) { send = 2; }
//            if (voiceRange == 8) { send = 3; }
//            if (voiceRange == 16) { send = 4; }
//            if (!(mutedPlayer.Contains(player))) {
//                player.emitCefEvent(new VoiceRange(send), false);
//            }

//        }

//        #endregion

//        //113.0-168.9 mHz
//    }

//    public class VoiceRange : IPlayerCefEvent {
//        public string Event { get; set; }
//        public int Range;
//        public VoiceRange(int sendInt) {
//            Event = "CREATE_VOICERANGE";
//            Range = sendInt;
//        }
//    }

//    internal class VoiceRadio {
//        public int CharID { get; set; }
//        public string radio { get; set; }
//        public string radio2 { get; set; }
//        public string type { get; set; }

//    }

//    internal class Phone {
//        public int ID { get; set; }
//        public IPlayer member { get; set; }
//        public bool relayed { get; set; }
//    }

//    internal class RangePlayer {
//        public int range { get; set; }
//        public IPlayer player { get; set; }

//    }

//    internal class Speaker {
//        public IPlayer player { get; set; }
//        public bool speaker { get; set; }
//    }

//}
