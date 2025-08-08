using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.ListMenuItem;

namespace ChoiceVServer.Controller {
    class WeatherController : ChoiceVScript {
        public enum WeatherTypes {
            None = -1,
            Sun = 0,
            Clouds = 1,
            Fog = 2,
            Rain = 3,
            Storm = 4
        }

        public static float[][] MarkovChain = {
            [0.75f, 0.15f, 0.10f, 0.00f, 0.00f],
            [0.55f, 0.25f, 0.10f, 0.10f, 0.00f],
            [0.45f, 0.00f, 0.20f, 0.25f, 0.10f],
            [0.00f, 0.20f, 0.30f, 0.35f, 0.15f],
            [0.00f, 0.70f, 0.00f, 0.30f, 0.00f]
        };

        public static float MixPercent;

        public static WeatherTypes CurrentWeather;
        public static WeatherTypes PreviousWeather;
        public static string PreviousWeatherName;
        public static string CurrentWeatherName;

        public static DateTime NextChange;

        private static readonly TimeSpan WEATHER_CYLCE_TIME = TimeSpan.FromHours(4);

        //TODO check if alt.setWeatherSyncActive(false) on clientside fixes the blinking issue
        public WeatherController() {
            initiateCurrentWeather();

            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnected;

            InvokeController.AddTimedInvoke("WeatherChanger", (ivk) => calculateWeather(WeatherTypes.None), TimeSpan.FromMinutes(5), true);

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    () => new ListMenuItem("Wetter ändern", "Ändere das Wetter direkt zum gewählten. Wähle None um ein natürliches ändern hervorzurufen", Enum.GetValues<WeatherTypes>().Select(w => w.ToString()).ToArray(), "SUPPORT_ON_CHANGE_WEATHER"),
                    2,
                    SupportMenuCategories.Wetter
                )
            );
            EventController.addMenuEvent("SUPPORT_ON_CHANGE_WEATHER", onSupportChangeWeather);
        }

        private bool onSupportChangeWeather(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var listEvt = menuItemCefEvent as ListMenuItemEvent;
            var weatherType = Enum.Parse<WeatherTypes>(listEvt.currentElement);
            calculateWeather(weatherType);
            return true;
        }

        private static void initiateCurrentWeather() {
            using(var db = new ChoiceVDb()) {
                var info = db.weatherinformations.FirstOrDefault(w => w.current == 1);

                if(info == null) {
                    CurrentWeather = WeatherTypes.Sun;
                    PreviousWeather = WeatherTypes.Sun;
                    PreviousWeatherName = getWeatherByType(WeatherTypes.Sun);
                    CurrentWeatherName = getWeatherByType(WeatherTypes.Sun);

                    MixPercent = 1;

                    NextChange = DateTime.Now;
                    return;
                }

                CurrentWeather = (WeatherTypes)info.currentWeather;
                PreviousWeather = (WeatherTypes)info.previousWeather;
                PreviousWeatherName = info.previousWeatherName;
                CurrentWeatherName = info.currentWeatherName;

                MixPercent = info.mixPercent;

                NextChange = info.nextChange??DateTime.MinValue;
            }

            Logger.logInfo(LogCategory.System, LogActionType.Updated, $"WeatherController Initiated: Current: {CurrentWeatherName}, Previous {PreviousWeatherName}, MixPercent {MixPercent}, NextChange {NextChange}");
        }

        public static void calculateWeather(WeatherTypes nextweather) {
            if(nextweather != WeatherTypes.None || DateTime.Now >= NextChange) {
                PreviousWeather = CurrentWeather;
                PreviousWeatherName = CurrentWeatherName;

                var time = WEATHER_CYLCE_TIME;

                var column = (int)CurrentWeather;
                var probs = MarkovChain[column];

                var rand = new Random();
                if(nextweather == WeatherTypes.None) {
                    var prob = (float)rand.NextDouble();
                    var count = 0f;
                    for(int i = 0; i < probs.Length; i++) {
                        count += probs[i];
                        if(prob <= count) {
                            var newWeather = (WeatherTypes)i;
                            PreviousWeather = CurrentWeather;
                            PreviousWeatherName = CurrentWeatherName;

                            CurrentWeather = newWeather;
                            CurrentWeatherName = getWeatherByType(CurrentWeather);

                            if((newWeather != WeatherTypes.Storm) && (PreviousWeather != WeatherTypes.Storm) && CurrentWeather != PreviousWeather) {
                                MixPercent = (float)rand.NextDouble();
                            } else {
                                MixPercent = 1;
                            }

                            break;
                        }
                    }
                } else {
                    var newWeather = nextweather;
                    PreviousWeather = CurrentWeather;
                    PreviousWeatherName = CurrentWeatherName;

                    CurrentWeather = newWeather;
                    CurrentWeatherName = getWeatherByType(CurrentWeather);

                    if((newWeather != WeatherTypes.Storm) && (PreviousWeather != WeatherTypes.Storm) && CurrentWeather != PreviousWeather) {
                        MixPercent = (float)rand.NextDouble();
                    } else {
                        MixPercent = 1;
                    }
                }

                NextChange = DateTime.Now + TimeSpan.FromHours(5);

                Logger.logInfo(LogCategory.System, LogActionType.Updated, $"WeatherController: Weather changed from {PreviousWeather} to {CurrentWeather} with mix {MixPercent} for {time}h");

                ChoiceVAPI.setWeatherTransition(PreviousWeatherName, CurrentWeatherName, MixPercent);
                saveCurrentInterval();
            }
        }

        private static void saveCurrentInterval() {
            using(var db = new ChoiceVDb()) {
                var current = db.weatherinformations.FirstOrDefault(w => w.current == 1);
                if(current != null) {
                    current.current = 0;
                }

                var newC = new weatherinformation {
                    start = DateTime.Now,
                    current = 1,
                    currentWeather = (int)CurrentWeather,
                    previousWeather = (int)PreviousWeather,
                    previousWeatherName = PreviousWeatherName,
                    currentWeatherName = CurrentWeatherName,
                    mixPercent = MixPercent,

                    nextChange = NextChange,
                };

                db.weatherinformations.Add(newC);
                db.SaveChanges();
            }
        }

        private void onPlayerConnected(IPlayer player, character character) {
            ChoiceVAPI.setWeatherMixForPlayer(player, PreviousWeatherName, CurrentWeatherName, MixPercent);
        }

        private static string getWeatherByType(WeatherTypes type) {
            var r = new Random();
            switch(type) {
                case WeatherTypes.Sun:
                    var i = r.NextDouble();
                    if(i <= 0.5) return "EXTRASUNNY"; else return "CLEAR";
                case WeatherTypes.Clouds:
                    return "CLOUDS";
                case WeatherTypes.Fog:
                    return "FOGGY";
                case WeatherTypes.Rain:
                    return "RAIN";
                case WeatherTypes.Storm:
                    return "THUNDER";
            }

            Logger.logError($"getWeatherByType: Something went wrong! {type}",
                $"Fehler im Wettersystem. Ein Wettertyp konnte nicht zugeordnet werden! {type}");
            return "CLEAR";
        }

        public static uint getWeatherIdByType(string weatherName) {
            switch(weatherName) {
                case "EXTRASUNNY":
                    return 0;
                case "CLEAR":
                    return 1;
                case "CLOUDS":
                    return 2;
                case "FOGGY":
                    return 4;
                case "RAIN":
                    return 6;
                case "THUNDER":
                    return 7;
                default:
                    return 0;
            }
        }
    }
}
