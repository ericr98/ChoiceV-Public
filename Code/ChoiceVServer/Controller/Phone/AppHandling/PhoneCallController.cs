using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.DamageSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Controller.Phone.PhoneController;

namespace ChoiceVServer.Controller.Phone {
    public class PhoneCallMember {
        [JsonIgnore]
        public IPlayer Player;

        [JsonProperty("number")]
        public long Number;

        [JsonProperty("hidden")]
        public bool HiddenNumber;

        public PhoneCallMember(IPlayer player, long number, bool hiddenNumber) {
            Player = player;
            Number = number;
            HiddenNumber = hiddenNumber;
        }
    }

    public class PhoneCall : IDisposable {
        private static int CallIdCounter = 0;

        public int Id { get; private set; }
        public PhoneCallMember Owner;
        public List<PhoneCallMember> Members { get; private set; }

        public PhoneCall(PhoneCallMember owner) {
            Id = CallIdCounter + 1;
            CallIdCounter++;
            Owner = owner;
            Members = [];
        }

        public bool containsNumber(long number) {
            if(Owner.Number == number) {
                return true;
            } else {
                return Members.FirstOrDefault(m => m.Number == number) != null;
            }
        }

        public bool containsChar(IPlayer player) {
            if(Owner.Player == player) {
                return true;
            } else {
                return Members.FirstOrDefault(m => m.Player == player) != null;
            }
        }

        public List<IPlayer> getAllPlayers() {
            var list = new List<IPlayer>();
            if(Owner.Player != null) {
                list.Add(Owner.Player);
            }

            foreach(var member in Members) {
                if(Owner.Player != null) {
                    list.Add(member.Player);
                }
            }

            return list;
        }

        public void removeMember(PhoneCallMember member) {
            Members.Remove(member);

            foreach(var mem in getAllPlayers()) {
                VoiceController.stopCall(mem, member.Player);
            }
        }

        public void addMember(PhoneCallMember member) {
            foreach(var mem in getAllPlayers()) {
                VoiceController.startCall(mem, member.Player);
            }

            Members.Add(member);
        }

        public void Dispose() {
            foreach(var mem1 in getAllPlayers()) {
                foreach(var mem2 in getAllPlayers()) {
                    if(mem1 != mem2) {
                        VoiceController.stopCall(mem1, mem2);
                    }
                }

                if(mem1.hasState(PlayerStates.OnPhone)) {
                    mem1.playAnimation(AnimationController.getAnimationByName("PHONE_OPEN"), null, false);
                } else {
                    mem1.resetData("ANIMATION");
                    mem1.stopAnimation();
                }
            }
            Members.Clear();

            PhoneCallController.AllCalls.Remove(this);
        }
    }

    internal class PhoneCallController : ChoiceVScript {
        internal static List<PhoneCall> AllCalls = new List<PhoneCall>();

        public PhoneCallController() {
            EventController.addCefEvent("PHONE_START_CALL", onSmartphoneStartCall);
            EventController.addCefEvent("PHONE_ACCEPT_CALL", onSmartphoneAcceptCall);
            EventController.addCefEvent("PHONE_SEND_GPS", onSmartphoneGPS);
            EventController.addCefEvent("PHONE_END_CALL", onSmartphoneEndCall);
            EventController.addCefEvent("PHONE_REQUEST_CALLLIST", onSmartphoneRequestCalllist);
            EventController.addCefEvent("PHONE_CHECK_CALLLIST", onSmartphoneCheckedCalllist);


            DamageController.BackendPlayerDeathDelegate += onPlayerBackendDead;
            EventController.PlayerDisconnectedDelegate += onPlayerDisconnect;
        }

        internal static IPlayer findPlayerWithNumber(long number) {
            foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                var smartphone = player.getInventory().getItem<Smartphone>(i => i.Selected);
                if(smartphone != null && 
                    (smartphone.hasNumber(number) || CompanyPhoneModule.hasPlayerAccessToNumber(player, number)) && 
                    AllCalls.FirstOrDefault(c => c.containsChar(player)) == null) {
                    return player;
                }
            }

            return null;
        }

        internal static void removePlayerFromCall(PhoneCall call, IPlayer player) {
            if(call.Owner.Player == player) {
                foreach(var target in call.getAllPlayers()) {
                    target.emitCefEventNoBlock(new PhoneAnswerEvent("END_CALL"));
                }
                call.Dispose();
            } else {
                var member = call.Members.FirstOrDefault(m => m.Player == player);
                call.removeMember(member);

                if(call.Members.Count <= 0) {
                    foreach(var target in call.getAllPlayers()) {
                        target.emitCefEventNoBlock(new PhoneAnswerEvent("END_CALL"));
                    }
                    call.Dispose();
                }
            }
        }

        private void onPlayerBackendDead(IPlayer player) {
            foreach(var call in AllCalls) {
                if(call.containsChar(player)) {
                    removePlayerFromCall(call, player);
                    return;
                }
            }
        }

        private void onPlayerDisconnect(IPlayer player, string reason) {
            foreach(var call in AllCalls) {
                if(call.containsChar(player)) {
                    removePlayerFromCall(call, player);
                    return;
                }
            }
        }

        #region RequestCalllist

        private class CallListElement {
            public int id;
            public string icon;
            public long number;
            public string dateTime;
            public bool missed;
            public bool check;

            public CallListElement(phonecalllist dbCall, long showNumber) {
                id = dbCall.id;
                if(dbCall.from == showNumber) {
                    icon = "list_out";
                    number = dbCall.to;
                    check = true;
                    missed = false;
                } else if(dbCall.to == showNumber && dbCall.missed == 1) {
                    icon = "list_miss";
                    number = dbCall.from;
                    check = dbCall._checked == 1;
                    missed = dbCall.missed == 1;
                } else {
                    icon = "list_in";
                    number = dbCall.from;
                    check = dbCall._checked == 1;
                    missed = dbCall.missed == 1;
                }
                dateTime = dbCall.date.ToString("dd.MM hh:mm tt", new CultureInfo("en-US"));
            }
        }

        private class PhoneAnswerCallistEvent : PhoneAnswerEvent {
            public string[] callList;

            public PhoneAnswerCallistEvent(List<phonecalllist> callList, long showNumber) : base("PHONE_ANSWER_CALLLIST") {
                this.callList = callList.Select(c => new CallListElement(c, showNumber).ToJson()).ToArray();
                this.callList = this.callList.Reverse().ToArray();
            }
        }

        private class PhoneRequestCalllistEvent {
            public int itemId;
            public long number;
            public int[] noLongerMissed;
        }

        private void onSmartphoneRequestCalllist(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = new PhoneRequestCalllistEvent();
            cefData.PopulateJson(evt.Data);

            sendCallListToPlayer(player, cefData.number);
        }

        public static void sendCallListToPlayer(IPlayer player, long number) {
            using(var db = new ChoiceVDb()) {
                var phonecalls = db.phonecalllists.Where(c => c.to == number || c.from == number).ToList();

                //player.emitCefEvent(new PhoneAnswerCallistEvent(phonecalls.Where(p => p.date > DateTime.Now - TimeSpan.FromDays(30)).ToList(), number), false);
                player.emitCefEventNoBlock(new PhoneAnswerCallistEvent(phonecalls.Take(50).ToList(), number));
            }
        }

        private class PhoneCheckedCalllistEvent {
            public int itemId;
            public long number;

            public List<int> noLongerMissed;
            public DateTime lastUpdate;
        }

        private void onSmartphoneCheckedCalllist(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = new PhoneCheckedCalllistEvent();
            cefData.PopulateJson(evt.Data);

            using(var db = new ChoiceVDb()) {
                var phonecalls = db.phonecalllists.Where(c => cefData.noLongerMissed.Contains(c.id)).ToList();
                phonecalls.ForEach((p) => { p._checked = 1; });
                db.SaveChanges();

                if(cefData.lastUpdate + TimeSpan.FromMinutes(0.5) < DateTime.Now) {
                    var newCalls = db.phonecalllists.Where(c => c.date > cefData.lastUpdate && (c.to == cefData.number || c.from == cefData.number));
                    player.emitCefEventNoBlock(new PhoneAnswerCallistEvent(newCalls.ToList(), cefData.number));
                }
            }
        }

        #endregion

        #region StartCall

        public class PhoneStartCallCefEvent {
            public long owner;
            public bool hiddenNumber;
            public long number;
        }

        private class PhoneCallChallengeCefEvent : PhoneAnswerEvent {
            public int callId;
            public int targetId;
            public string owner;
            public string[] members;
            public double cost;

            public PhoneCallChallengeCefEvent(int callId, int targetId, PhoneCallMember owner, List<PhoneCallMember> members, double cost) : base("INCOMING_CALL") {
                this.callId = callId;
                this.targetId = targetId;
                this.owner = owner.ToJson();
                this.members = members.Select(m => m.ToJson()).ToArray();
                this.cost = cost;
            }
        }

        public static void onSmartphoneStartCall(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = new PhoneStartCallCefEvent();
            try {
                cefData.PopulateJson(evt.Data);
            } catch (Exception e) {
                return;
            }

            var already = AllCalls.FirstOrDefault(c => c.containsNumber(cefData.owner));
            player.playAnimation(AnimationController.getAnimationByName("PHONE"), null, false);
            if(already == null) {
                var target = findPlayerWithNumber(cefData.number);
                if(target != null && target != player) {
                    var owner = new PhoneCallMember(player, cefData.owner, cefData.hiddenNumber);
                    var call = new PhoneCall(owner);
                    AllCalls.Add(call);

                    var callEvt = new PhoneCallChallengeCefEvent(call.Id, target.getCharacterId(), call.Owner, call.Members, 1.5);
                    target.emitCefEventNoBlock(callEvt);
                } else {
                    using(var db = new ChoiceVDb()) {
                        var newCall = new phonecalllist {
                            from = cefData.owner,
                            to = cefData.number,
                            date = DateTime.Now,
                            missed = 1,
                        };

                        db.phonecalllists.Add(newCall);
                        db.SaveChanges();
                    }
                }
            } else {
                //TODO in Future: Send back event that triggers "Already in call" sound
            }
        }

        #endregion

        #region AcceptCall

        private class PhoneAcceptCallCefEvent {
            public int itemId;
            public int callId;
            public int playerId;
            public long number;
        }

        private static void onSmartphoneAcceptCall(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = new PhoneAcceptCallCefEvent();
            cefData.PopulateJson(evt.Data);

            var call = AllCalls.FirstOrDefault(c => c.Id == cefData.callId);
            if(call != null) {
                var target = ChoiceVAPI.FindPlayerByCharId(cefData.playerId);
                if(target != null) {
                    if(call.Owner.Player != null) {
                        call.addMember(new PhoneCallMember(target, cefData.number, false));
                        call.Owner.Player.emitCefEventNoBlock(new PhoneAnswerEvent("STOP_CALL_SOUND"));

                        player.playAnimation(AnimationController.getAnimationByName("PHONE"), null, false);
                    }
                } else {
                    player.emitCefEventNoBlock(new PhoneAnswerEvent("END_CALL"));

                    if(player.hasState(PlayerStates.OnPhone)) {
                        player.playAnimation(AnimationController.getAnimationByName("PHONE_OPEN"), null, false);
                    } else {
                        player.stopAnimation();
                    }
                }

                using(var db = new ChoiceVDb()) {
                    var newCall = new phonecalllist {
                        from = call.Owner.Number,
                        to = cefData.number,
                        date = DateTime.Now,
                        missed = 0,
                    };

                    db.phonecalllists.Add(newCall);
                    db.SaveChanges();
                }
            } else {
                player.emitCefEventNoBlock(new PhoneAnswerEvent("END_CALL"));

                if(player.hasState(PlayerStates.OnPhone)) {
                    player.playAnimation(AnimationController.getAnimationByName("PHONE_OPEN"), null, false);
                } else {
                    player.stopAnimation();
                }
            }
        }

        #endregion

        #region GPS

        private class PhoneGPSCefEvent {
            public int itemId;
            public long number;
        }

        private void onSmartphoneGPS(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = new PhoneGPSCefEvent();
            cefData.PopulateJson(evt.Data);

            var call = AllCalls.FirstOrDefault(c => c.containsChar(player));
            if(call != null) {
                var members = call.Members.ToList();
                foreach(var member in members) {
                    BlipController.createPointBlip(member.Player, $"GPS von {player.getMainPhoneNumber()}", player.Position, 28, 8, 162, $"Phone-Call-{call.Id}");
                    InvokeController.AddTimedInvoke($"Phone-Call-{call.Id}", (i) => {
                        BlipController.destroyBlipByName(member.Player, $"Phone-Call-{call.Id}");
                    }, TimeSpan.FromMinutes(10), false);
                }
            }
        }

        #endregion

        #region EndCall

        private class PhoneEndCallCefEvent {
            public int itemId;
            public long number;
        }

        private static void onSmartphoneEndCall(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = new PhoneEndCallCefEvent();
            cefData.PopulateJson(evt.Data);

            var call = AllCalls.FirstOrDefault(c => c.containsChar(player));
            if(call != null) {
                removePlayerFromCall(call, player);
            } else {
                player.emitCefEventNoBlock(new PhoneAnswerEvent("END_CALL"));
            }

            if(player.hasState(PlayerStates.OnPhone)) {
                player.playAnimation(AnimationController.getAnimationByName("PHONE_OPEN"), null, false);
            } else {
                player.stopAnimation();
            }
        }

        #endregion
    }
}
