using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller {
    public delegate void CombinationLockCallback(IPlayer player, Dictionary<string, dynamic> data);

    public class CombinationLockController : ChoiceVScript {
        private class CombinationLockCallbackElement {
            public CombinationLockCallback CombinationLockCallback;

            public string Combination;

            public Dictionary<string, dynamic> Data;

            public bool IsUnlocked = false;

            public CombinationLockCallbackElement(string combination, CombinationLockCallback combinationLockCallback, Dictionary<string, dynamic> data) {
                Combination = combination;
                CombinationLockCallback = combinationLockCallback;
                Data = data;
            }
        }

        private class CombinationLockCefEvent : IPlayerCefEvent {
            public string Event { get; set; }
            public int id;
            public int length;

            public CombinationLockCefEvent(int id, int length) {
                Event = "COMBINATION_LOCK_OPEN";
                this.id = id;
                this.length = length;
            }
        }

        private static Dictionary<int, CombinationLockCallbackElement> AllCallbacks = new Dictionary<int, CombinationLockCallbackElement>();

        public CombinationLockController() {
            EventController.addCefEvent("COMBINATION_LOCK_CHECK_COMBINATION", onCombinationLockCheckCombination);

            EventController.addCefEvent("COMBINATION_LOCK_ACCESSED", onCombinationLockEvent);
        }

        /// <summary>
        /// Opens a combinationLock for player
        /// </summary>
        /// <param name="combination">Has to be a array of numbers. e.g "01234" or "555"</param>
        public static void requestPlayerCombination(IPlayer player, int[] combination, CombinationLockCallback callback, Dictionary<string, dynamic> data = null) {
            requestPlayerCombination(player, String.Join("", combination), callback, data);
        }

        /// <summary>
        /// Opens a combinationLock for player
        /// </summary>
        /// <param name="combination">Has to be a string of numbers. e.g "01234" or "555"</param>
        public static void requestPlayerCombination(IPlayer player, string combination, CombinationLockCallback callback, Dictionary<string, dynamic> data = null) {
            var el = new CombinationLockCallbackElement(combination, callback, data);
            if(AllCallbacks.ContainsKey(player.getCharacterId())) {
                AllCallbacks[player.getCharacterId()] = el;
            } else {
                AllCallbacks.Add(player.getCharacterId(), el);
            }

            player.emitCefEventWithBlock(new CombinationLockCefEvent(player.getCharacterId(), combination.Length), "COMBINATION_LOCK");
        }

        private class CombinationLockAccessedEvent {
            public int? id;
            public string combination;
        }

        private void onCombinationLockCheckCombination(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var accEvt = new CombinationLockAccessedEvent();
            accEvt.PopulateJson(evt.Data);

            if(accEvt.id == null) {
                Logger.logWarning(LogCategory.System, LogActionType.Blocked, player, $"onCombinationLockCheckCombination: combinationLockId was null: {evt.ToJson()}");
                return;
            }
            if(AllCallbacks.ContainsKey(accEvt.id ?? -1)) {
                var el = AllCallbacks[accEvt.id ?? -1];
                if(el.Combination.Equals(accEvt.combination)) {
                    el.IsUnlocked = true;
                    player.emitCefEventNoBlock(new OnlyEventCefEvent("COMBINATION_LOCK_UNLOCKED"));
                }
            } else {
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onCombinationLockEvent: combinationLockId was not registered, charId: {player.getCharacterId()}");
            }
        }

        private void onCombinationLockEvent(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var accEvt = new CombinationLockAccessedEvent();
            accEvt.PopulateJson(evt.Data);
            
            if(accEvt.id == null) {
                Logger.logWarning(LogCategory.System, LogActionType.Blocked, player, $"onCombinationLockCheckCombination: combinationLockId was null: {evt.ToJson()}");
                return;
            }

            if(AllCallbacks.ContainsKey(accEvt.id ?? -1)) {
                var el = AllCallbacks[accEvt.id ?? -1];
                if(el.IsUnlocked) {
                    AllCallbacks.Remove(accEvt.id ?? -1);
                    el.CombinationLockCallback.Invoke(player, el.Data);
                } else {
                    player.ban("Hat versucht Combination Lock zu erhacken");
                }
            } else {
                Logger.logWarning(LogCategory.System, LogActionType.Blocked, player, $"onCombinationLockEvent: combinationLockId was not registered");
            }
        }
    }
}
