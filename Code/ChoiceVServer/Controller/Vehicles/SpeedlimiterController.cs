using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using static ChoiceVServer.Model.Menu.ListMenuItem;

namespace ChoiceVServer.Controller.Vehicles {
    public class SpeedlimiterController : ChoiceVScript {
        public SpeedlimiterController() {
            EventController.addEvent("NOTIFICATION_SPEEDLIMITER", onNotificationSpeedlimiter);

            var speedList = new string[] { "Aus", "60", "80", "100", "120", "140", "160" };

            VehicleController.addSelfMenuElement(
                new ConditionalVehicleSelfMenuElement(
                    () => new ListMenuItem("Speedlimiter", "Stelle deine Geschwindigkeit ein.", speedList, "SET_SPEEDLIMTER", MenuItemStyle.normal),
                    v => v.VehicleClass != VehicleClassesDbIds.Cycles && v.VehicleClass != VehicleClassesDbIds.Boats && v.VehicleClass != VehicleClassesDbIds.Helicopters && v.VehicleClass != VehicleClassesDbIds.Planes && v.VehicleClass != VehicleClassesDbIds.Cycles,
                    p => p.Seat == 1
                )
            );

            EventController.addMenuEvent("SET_SPEEDLIMTER", onSetSpeedlimiter);
            EventController.addKeyEvent("SPEED_LIMITER", ConsoleKey.J, "Speedlimiter", onToggleSpeedLimiter);
        }

        private bool onToggleSpeedLimiter(IPlayer player, ConsoleKey key, string eventName) {
            if(player.Vehicle != null) {
                player.emitClientEvent("SET_TOGGLE_SPEEDLIMITER", player.Vehicle);
            }
            return true;
        }

        private bool onSetSpeedlimiter(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as ListMenuItemEvent;

            switch(evt.currentElement) {
                case "Aus":
                    player.emitClientEvent("SET_SPEEDLIMTER", player.Vehicle, 500);
                    break;
                case "60":
                    player.emitClientEvent("SET_SPEEDLIMTER", player.Vehicle, 60);
                    break;
                case "80":
                    player.emitClientEvent("SET_SPEEDLIMTER", player.Vehicle, 80);
                    break;
                case "100":
                    player.emitClientEvent("SET_SPEEDLIMTER", player.Vehicle, 100);
                    break;
                case "120":
                    player.emitClientEvent("SET_SPEEDLIMTER", player.Vehicle, 120);
                    break;
                case "140":
                    player.emitClientEvent("SET_SPEEDLIMTER", player.Vehicle, 140);
                    break;
                case "160":
                    player.emitClientEvent("SET_SPEEDLIMTER", player.Vehicle, 160);
                    break;
            }
            return true;
        }

        private bool onNotificationSpeedlimiter(IPlayer player, string eventName, object[] args) {
            player.sendNotification(Constants.NotifactionTypes.Success, args[0].ToString(), args[0].ToString(), Constants.NotifactionImages.Car);
            return true;
        }
    }
}
