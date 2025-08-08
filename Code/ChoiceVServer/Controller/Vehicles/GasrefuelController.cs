using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using Newtonsoft.Json;
using NLog.Targets;
using System;
using System.Collections.Generic;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller {
    public class GasRefuelController : ChoiceVScript {
        public static Dictionary<int, GasRefuel> CurrentlyDisplayedGasRefuel = new Dictionary<int, GasRefuel>();

        public GasRefuelController() {
            EventController.addCefEvent("GASSTATION_EVENT", onGasRefuelEvent);

            EventController.PlayerDisconnectedDelegate += onPlayerDisconnect;
        }

        private void onPlayerDisconnect(IPlayer player, string reason) {
            if(CurrentlyDisplayedGasRefuel.ContainsKey(player.getCharacterId())) {
                CurrentlyDisplayedGasRefuel.Remove(player.getCharacterId());
            }
        }

        public static void showGasRefuel(IPlayer player, GasRefuel station, Action<IPlayer, string, string, float, GasRefuel> callBack) {
            station.withCallback(callBack);

            if(CurrentlyDisplayedGasRefuel.ContainsKey(player.getCharacterId())) {
                CurrentlyDisplayedGasRefuel[player.getCharacterId()] = station;
            } else {
                CurrentlyDisplayedGasRefuel.Add(player.getCharacterId(), station);
            }

            player.emitCefEventWithBlock(station.toCef(), "GAS_REFUEL");
        }

        public static void closeGasRefuel(IPlayer player) {
            if(CurrentlyDisplayedGasRefuel.ContainsKey(player.getCharacterId())) {
                CurrentlyDisplayedGasRefuel.Remove(player.getCharacterId());
            }

            player.emitCefEventNoBlock(new OnlyEventCefEvent("CLOSE_GASSTATION"));
        }

        private class DefaultGasRefuelWebSocketEvent {
            public string Action = "";
            public string Account = "";
            public float FuelPrice = 0f;
            public float FuelAmmount = 0f;
            public string FuelType = "";
        }

        private void onGasRefuelEvent(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            if(CurrentlyDisplayedGasRefuel.ContainsKey(player.getCharacterId())) {
                var refuel = CurrentlyDisplayedGasRefuel[player.getCharacterId()];
                if(refuel != null) {
                    var cefData = new DefaultGasRefuelWebSocketEvent();
                    JsonConvert.PopulateObject(evt.Data, cefData);

                    refuel.Callback.Invoke(player, cefData.Action, cefData.Account, cefData.FuelAmmount, refuel);
                } else {
                    Logger.logError($"onGasRefuelEvent: Player triggered event of gasrefuel that was not opened!",
                            $"Fehler Tankstellensystem: Spieler hat Tankstellen Event ausgeführt obwohl er kein Tankstellenfenster offen hatte", player);
                }
            } else {
                Logger.logError($"onGasRefuelEvent: Player triggered event though he hasnt opened a gasrefuel yet.",
                            $"Fehler Tankstellensystem: Spieler hat Tankstellen Event ausgeführt obwohl er kein Tankstellenfenster offen hatte", player);
            }
        }
    }

    public class GasRefuel {
        public GasStationType StationType = GasStationType.None;
        public float Fuel;
        public float FuelMax;
        public string FuelName { get => GasstationSpotTypesToName[FuelType]; }
        public float FuelPrice;
        public bool ShowCash = false;
        public bool ShowBank = false;
        public bool ShowComp = false;

        [JsonIgnore]
        public GasstationSpotType FuelType;

        [JsonIgnore]
        public Dictionary<string, dynamic> Data = new Dictionary<string, dynamic>();

        [JsonIgnore]
        public Action<IPlayer, string, string, float, GasRefuel> Callback;

        public GasRefuel(GasStationType type, float fuel, float fuelmax, GasstationSpotType fuelType, float fuelprice, bool showcash, bool showbank, bool showcomp) {
            StationType = type;
            Fuel = fuel;
            FuelMax = fuelmax;
            FuelType = fuelType;
            FuelPrice = fuelprice;
            ShowCash = showcash;
            ShowBank = showbank;
            ShowComp = showcomp;
        }

        public GasRefuel withData(Dictionary<string, dynamic> data) {
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

        public GasRefuel withCallback(Action<IPlayer, string, string, float, GasRefuel> callback) {
            Callback = callback;
            return this;
        }

        public enum GasStationType : int {
            None = 0,
            Ltd = 1,
            Ron = 2,
            GlobeOil = 3,
            XeroGas = 4,
            ElectricCharging = 5,
        }

        public class GasstationCefRepresentative : IPlayerCefEvent {
            public string Event { get; }
            public GasStationType StationType = GasStationType.None;
            public float Fuel;
            public float FuelMax;
            public string FuelName;
            public float FuelPrice;
            public bool ShowCash = false;
            public bool ShowBank = false;
            public bool ShowComp = false;

            public GasstationCefRepresentative(GasStationType type, float fuel, float fuelmax, string fuelname, float fuelprice, bool showcash, bool showbank, bool showcomp) {
                Event = "CREATE_GASSTATION";
                StationType = type;
                Fuel = fuel;
                FuelMax = fuelmax;
                FuelName = fuelname;
                FuelPrice = fuelprice;
                ShowCash = showcash;
                ShowBank = showbank;
                ShowComp = showcomp;
            }
        }
        public GasstationCefRepresentative toCef() {
            return new GasstationCefRepresentative(StationType, Fuel, FuelMax, FuelName, FuelPrice, ShowCash, ShowBank, ShowComp);
        }
    }
}
