using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller {
    public delegate void PlayerIslandChangeDelegate(IPlayer player, Islands previousIsland, Islands newIsland);

    //TODO Position nicht resetten, wenn Spiel abstürzt
    public class IslandController : ChoiceVScript {
        public static PlayerIslandChangeDelegate PlayerIslandChangeDelegate;

        public const float CayoEntryX = 6000;
        public const float CayoEntryY = 5800;

        public const float CayoLeaveX = 4000;
        public const float CayoLeaveY = 6000;

        public const float SanAndreasEntryX = 2500;
        public const float SanAndreasEntryY = -5000;

        public const float SanAndreasLeaveX = 3000;
        public const float SanAndreasLeaveY = -5300;

        public IslandController() {
            EventController.VehicleMovedDelegate += onVehicleMoved;

            EventController.PlayerPreSuccessfullConnectionDelegate += onPlayerPreConnect;

            VehicleController.addSelfMenuElement(
                new ConditionalVehicleSelfMenuElement(
                    () => new ClickMenuItem("Route nach SA", "Setze die Flugroute nach San Andreas", "", "SET_FLIGHT_ROUTE").withData(new Dictionary<string, dynamic> { { "Island", Islands.SanAndreas } }),
                    v => v.Dimension == Constants.IslandDimension && (v.VehicleClass == VehicleClassesDbIds.Helicopters || v.VehicleClass == VehicleClassesDbIds.Planes || v.DbModel.ModelName == "Marquis" || v.DbModel.ModelName == "Tug"),
                    p => p.getIsland() != Islands.SanAndreas
                )
            );

            VehicleController.addSelfMenuElement(
                new ConditionalVehicleSelfMenuElement(
                    () => new ClickMenuItem("Route nach CP", "Setze die Flugroute nach Cayo Perico", "", "SET_FLIGHT_ROUTE").withData(new Dictionary<string, dynamic> { { "Island", Islands.CayoPerico } }),
                    v => v.Dimension == Constants.GlobalDimension && (v.VehicleClass == VehicleClassesDbIds.Helicopters || v.VehicleClass == VehicleClassesDbIds.Planes || v.DbModel.ModelName == "Marquis" || v.DbModel.ModelName == "Tug"),
                    p => p.getIsland() != Islands.CayoPerico
                )
            );
            EventController.addMenuEvent("SET_FLIGHT_ROUTE", onSetFlightRoute);

            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    1,
                   SupportMenuCategories.Misc,
                   generatePortMenu
                )
            );

            EventController.addMenuEvent("SUPPORT_SWITCH_ISLAND", onSupportSwitchIsland);
        }

        public static Islands getIslandFromDimension(int dimension) {
            switch(dimension) {
                case Constants.GlobalDimension:
                    return Islands.SanAndreas;

                case Constants.IslandDimension:
                    return Islands.CayoPerico;

                default:
                    return Islands.SanAndreas;
            }
        }

        private MenuItem generatePortMenu(IPlayer player) {
            switch(player.getIsland()) {
                case Islands.CayoPerico:
                    return new ClickMenuItem("Nach Los Santos porten", "Teleportiere dich nach Los Santos", "", "SUPPORT_SWITCH_ISLAND").withData(new Dictionary<string, dynamic> { { "ToIsland", Islands.SanAndreas } });
                case Islands.SanAndreas:
                    return new ClickMenuItem("Nach Cayo Perico porten", "Teleportiere dich nach Cayo Perico", "", "SUPPORT_SWITCH_ISLAND").withData(new Dictionary<string, dynamic> { { "ToIsland", Islands.CayoPerico } });
                default:
                    return null;
            }
        }

        private bool onSupportSwitchIsland(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var toIsland = (Islands)data["ToIsland"];

            switch(toIsland) {
                case Islands.SanAndreas:
                    player.emitClientEvent("LEAVE_CAYO_PERICO", false);
                    player.Position = new Position(0, 0, 72);
                    player.changeDimension(Constants.GlobalDimension);
                    PlayerIslandChangeDelegate.Invoke(player, player.getIsland(), Islands.SanAndreas);
                    break;
                case Islands.CayoPerico:
                    player.emitClientEvent("ARRIVE_AT_CAYO_PERICO", false);
                    player.Position = new Position(4000, -4700, 5);
                    player.changeDimension(Constants.IslandDimension);
                    PlayerIslandChangeDelegate.Invoke(player, player.getIsland(), Islands.CayoPerico);
                    break;
            }

            return true;
        }

        private bool onSetFlightRoute(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var island = (Islands)data["Island"];

            switch(island) {
                case Islands.CayoPerico:
                    BlipController.setWaypoint(player, SanAndreasLeaveX, SanAndreasLeaveY);
                    break;
                case Islands.SanAndreas:
                    BlipController.setWaypoint(player, 5000, CayoLeaveY);
                    break;
            }

            return true;
        }

        private void onPlayerPreConnect(IPlayer player, character character) {
            if(character.island == (int)Islands.CayoPerico) {
                player.emitClientEvent("ARRIVE_AT_CAYO_PERICO", false);
            }
        }

        private void onVehicleMoved(object sender, ChoiceVVehicle vehicle, Position moveFromPos, Position moveToPosition, float distance) {
            if(!vehicle.Exists() || vehicle.DbModel == null) {
                return;
            }

            //Check if its planes or right boat
            if(!(vehicle.VehicleClass == VehicleClassesDbIds.Helicopters || vehicle.VehicleClass == VehicleClassesDbIds.Planes || vehicle.DbModel.ModelName == "Marquis" || vehicle.DbModel.ModelName == "Tug")) {
                return;
            }

            //Calculate if plane flies to Cayo Perico
            if(!vehicle.CurrentlyTravelingToIsland && vehicle.ScriptDimension == Constants.GlobalDimension && moveToPosition.X >= SanAndreasLeaveX && moveToPosition.Y <= SanAndreasLeaveY) {
                vehicle.CurrentlyTravelingToIsland = true;
                var pList = vehicle.PassengerList.Values.ToList();
                var nearbyPlayers = ChoiceVAPI.GetAllPlayers().Where(p => p.Position.Distance(vehicle.Position) < 20 && !p.IsInVehicle).ToList();

                vehicle.changeDimension(Constants.IslandDimension);
                foreach(var passenger in pList.Concat(nearbyPlayers)) {
                    passenger.sendNotification(Constants.NotifactionTypes.Info, "Du reist nun nach Cayo Perico!", "Flug nach Cayo Perico!", Constants.NotifactionImages.Island);
                    passenger.changeDimension(Constants.IslandDimension);
                    passenger.fadeScreen(true, 500);
                }

                var vehiclePrePos = vehicle.Position.ToJson().FromJson();
                var vehiclePreRot = vehicle.Rotation.ToJson().FromJson<DegreeRotation>();
                InvokeController.AddTimedInvoke("IslandPorter", (i) => {
                    vehicle.Position = new Position(CayoEntryX, CayoEntryY, vehicle.Position.Z);
                    vehicle.Rotation = new DegreeRotation(0, 0, 180);
                    foreach(var passenger in pList.Concat(nearbyPlayers)) {
                        passenger.sendNotification(Constants.NotifactionTypes.Info, "Du bist nun nach Cayo Perico gereist! Der Wegpunkt wurde auf die Insel gesetzt.", "Flug nach Cayo Perico!", Constants.NotifactionImages.Island);
                        passenger.emitClientEvent("ARRIVE_AT_CAYO_PERICO", true);
                    }

                    if(vehicle.DbModel.ModelName == "Marquis" || vehicle.DbModel.ModelName == "Tug") {
                        foreach(var nearbyPlayer in nearbyPlayers) {
                            var relPos = nearbyPlayer.Position - vehiclePrePos;
                            var rotationChange = vehiclePreRot.Yaw - 180;

                            var newPos = ChoiceVAPI.rotatePointInRect(relPos.X, relPos.Y, 0, 0, rotationChange);

                            nearbyPlayer.Position = new Position(CayoEntryX + newPos.X, CayoEntryY + newPos.Y, nearbyPlayer.Position.Z + 0.5f);
                            nearbyPlayer.Rotation = new DegreeRotation(0, 0, 180);

                            nearbyPlayer.sendNotification(Constants.NotifactionTypes.Info, "Du bist nun nach Cayo Perico gereist! Der Wegpunkt wurde auf die Insel gesetzt.", "Flug nach Cayo Perico!", Constants.NotifactionImages.Island);
                            nearbyPlayer.emitClientEvent("ARRIVE_AT_CAYO_PERICO", true);
                        }
                    }

                    InvokeController.AddTimedInvoke("IslandPorter", (i) => {
                        foreach(var passenger in pList.Concat(nearbyPlayers)) {
                            vehicle.CurrentlyTravelingToIsland = false;
                            PlayerIslandChangeDelegate.Invoke(passenger, passenger.getIsland(), Islands.CayoPerico);
                            passenger.fadeScreen(false, 1000);
                        }
                    }, TimeSpan.FromSeconds(0.75), false);
                }, TimeSpan.FromSeconds(0.75), false);
            }

            if(!vehicle.CurrentlyTravelingToIsland && vehicle.Driver != null && vehicle.ScriptDimension == Constants.IslandDimension && vehicle.Position.X < 2500) {
                if(vehicle.hasData("NEXT_OUT_OF_BOUNDS_NOTFICITATION")) {
                    if((DateTime)vehicle.getData("NEXT_OUT_OF_BOUNDS_NOTFICITATION") < DateTime.Now) {
                        vehicle.setData("NEXT_OUT_OF_BOUNDS_NOTFICITATION", DateTime.Now + TimeSpan.FromSeconds(3));
                        if(vehicle.Driver != null) {
                            vehicle.Driver.sendBlockNotification("Du wendest dich in gefährliche Gewässer ab! Begib dich zurück Richtung Cayo Perico", "Kurs korrigieren!", Constants.NotifactionImages.Island);
                        }
                    }
                } else {
                    vehicle.setData("NEXT_OUT_OF_BOUNDS_NOTFICITATION", DateTime.Now + TimeSpan.FromSeconds(3));
                    if(vehicle.Driver != null) {
                        vehicle.Driver.sendBlockNotification("Du wendest dich in gefährliche Gewässer ab! Begib dich zurück Richtung Cayo Perico", "Kurs korrigieren!", Constants.NotifactionImages.Island);
                    }
                }
            }

            //Flying back to Los Santos
            if(!vehicle.CurrentlyTravelingToIsland && vehicle.ScriptDimension == Constants.IslandDimension && moveToPosition.X > CayoLeaveX && moveToPosition.Y > CayoLeaveY) {
                vehicle.CurrentlyTravelingToIsland = true;
                var pList = vehicle.PassengerList.Values.ToList();

                var nearbyPlayers = ChoiceVAPI.GetAllPlayers().Where(p => p.Position.Distance(vehicle.Position) < 20 && !p.IsInVehicle).ToList();

                vehicle.changeDimension(Constants.GlobalDimension);
                foreach(var passenger in pList.Concat(nearbyPlayers)) {
                    passenger.sendNotification(Constants.NotifactionTypes.Info, "Du reist nun nach San Andreas!", "Flug nach San Andreas!", Constants.NotifactionImages.Island);
                    passenger.changeDimension(Constants.GlobalDimension);
                    passenger.fadeScreen(true, 500);
                }

                var vehiclePrePos = vehicle.Position.ToJson().FromJson();
                var vehiclePreRot = vehicle.Rotation.ToJson().FromJson<DegreeRotation>();

                InvokeController.AddTimedInvoke("IslandPorter", (i) => {
                    vehicle.Position = new Position(SanAndreasEntryX, SanAndreasEntryY, vehicle.Position.Z);
                    vehicle.Rotation = new DegreeRotation(0, 0, 245);
                    foreach(var passenger in pList) {
                        passenger.sendNotification(Constants.NotifactionTypes.Info, "Du bist nun nach San Andreas gereist! Der Wegpunkt wurde auf die Insel gesetzt.", "Flug nach San Andreas!", Constants.NotifactionImages.Island);
                        passenger.emitClientEvent("LEAVE_CAYO_PERICO", true);
                    }

                    if(vehicle.DbModel.ModelName == "Marquis" || vehicle.DbModel.ModelName == "Tug") {
                        foreach(var nearbyPlayer in nearbyPlayers) {
                            var relPos = nearbyPlayer.Position - vehiclePrePos;
                            var rotationChange = vehiclePreRot.Yaw - 180;

                            var newPos = ChoiceVAPI.rotatePointInRect(relPos.X, relPos.Y, 0, 0, rotationChange);

                            nearbyPlayer.Position = new Position(SanAndreasEntryX + newPos.X, SanAndreasEntryY + newPos.Y, nearbyPlayer.Position.Z + 0.5f);
                            nearbyPlayer.Rotation = new DegreeRotation(0, 0, 180);

                            nearbyPlayer.sendNotification(Constants.NotifactionTypes.Info, "Du bist nun nach Cayo Perico gereist! Der Wegpunkt wurde auf die Insel gesetzt.", "Flug nach Cayo Perico!", Constants.NotifactionImages.Island);
                            nearbyPlayer.emitClientEvent("LEAVE_CAYO_PERICO", true);
                        }
                    }

                    InvokeController.AddTimedInvoke("IslandPorter", (i) => {
                        foreach(var passenger in pList.Concat(nearbyPlayers)) {
                            vehicle.CurrentlyTravelingToIsland = false;
                            PlayerIslandChangeDelegate.Invoke(passenger, passenger.getIsland(), Islands.SanAndreas);
                            passenger.fadeScreen(false, 1000);
                        }
                    }, TimeSpan.FromSeconds(0.75), false);
                }, TimeSpan.FromSeconds(0.75), false);
            }
        }
    }
}
