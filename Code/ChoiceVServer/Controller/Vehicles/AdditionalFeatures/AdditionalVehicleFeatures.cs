using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Numerics;
using static ChoiceVServer.Model.Menu.ListMenuItem;

namespace ChoiceVServer.Controller {
    public class AdditionalVehicleFeaturesController : ChoiceVScript {
        public AdditionalVehicleFeaturesController() {
            InteractionController.addVehicleInteractionElement(
                new ConditionalPlayerInteractionMenuElement(
                    () => new ClickMenuItem("Fahrzeug transportieren", "Befödere das Fahrzeug auf einem anderen Fahrzeug", "", "PUT_VEHICLE_ON_TRANSPORT"),
                    s => onTransportVehicleOnPredicateSender(s as IPlayer, false),
                    t => onTransportVehicleOnPredicateTarget(t as ChoiceVVehicle)
                )
            );

            InteractionController.addVehicleInteractionElement(
                new ConditionalPlayerInteractionMenuElement(
                    () => new ClickMenuItem("Fahrzeug bergen", "Berge das Fahrzeug. Nur benutzen, wenn andere Möglichkeiten ausgeschlossen! Wird geloggt, bitte Fotobeweis machen.", "", "PUT_VEHICLE_ON_TRANSPORT_SPECIAL", MenuItemStyle.yellow),
                    s => onTransportVehicleOnPredicateSender(s as IPlayer, true),
                    t => onTransportVehicleOnPredicateTarget(t as ChoiceVVehicle)
                )
            );

            InteractionController.addVehicleInteractionElement(
                new ConditionalGeneratedPlayerInteractionMenuElement(
                    transporterListItemGenerator,
                    s => true,
                    t => onTransportVehicleOffPredicateTarget(t as ChoiceVVehicle)
                )
            );

            InteractionController.addVehicleInteractionElement(
                new ConditionalPlayerInteractionMenuElement(
                    () => new ClickMenuItem("Fahrzeug umdrehen", "Drehe das Fahrzeug wieder um. Ein Wagenheber wird benötigt.", "", "USE_CAR_JACK_ITEM", MenuItemStyle.normal),
                    s => s is IPlayer pl && pl.getInventory().hasItem<ToolItem>(t => t.Flag == SpecialToolFlag.CarJack),
                    t => t is ChoiceVVehicle veh
                )
            );

            VehicleController.addSelfMenuElement(
                new ConditionalVehicleGeneratedSelfMenuElement(
                    "Fahrzeugtüren/hauben",
                    getVehicleDoorMenu,
                    vehicle => true,
                    player => player.Seat == 1
                )
            );


            EventController.addMenuEvent("PUT_VEHICLE_ON_TRANSPORT", onPutVehicleOnTransport);
            VehicleController.addVehicleSpawnDataSetCallback("IS_BEING_TRANSPORTED", onVehicleSpawnWithTransported);

            EventController.addMenuEvent("PULL_VEHICLE_OFF_TRANSPORT", onPullVehicleOffTransport);


            EventController.addMenuEvent("TOGGLE_VEHICLE_DOOR_CHOOSE", onToggleVehicleDoorChoose);

            EventController.addMenuEvent("TOGGLE_VEHICLE_WINDOW_CHOOSE", onToggleVehicleWindowChoose);

            EventController.addKeyEvent("SIREN", ConsoleKey.Q, "Sirenenton an/aus", onVehicleSirenSound);

            EventController.NetworkOwnerChangeDelegate += onNetworkOwnerChange;


            //Caddy 3 Stuff

            VehicleController.addTrunkOpenInject(onCheckForNearbyCaddy);
            InventoryController.addInventorySpotInjectInventory(onCheckForNearbyCaddy);

            //Custom Plane/Helicopter Sync

            InvokeController.AddTimedInvoke("Custom-Plane-Sync", updatePlaneHelicopterSync, TimeSpan.FromSeconds(1), true);

            //Emergency Vehicle Stuff
            VehicleController.addSelfMenuElement(
                new ConditionalVehicleSelfMenuElement(
                    () => new ClickMenuItem("Aktensystem öffnen", "Öffne das Aktensystem über das Fahrzeugpad", "", "ON_OPEN_FS"),
                    v => v is ChoiceVVehicle vehicle && vehicle.DbModel != null && VehicleController.hasVehicleSpecialFlag(vehicle, SpecialVehicleModelFlag.IsFileSystemOpenVehicle),
                    p => true
                )
            );
            EventController.addMenuEvent("ON_OPEN_FS", onOpenFs);

            //Use CarJack
            EventController.addMenuEvent("USE_CAR_JACK_ITEM", onPlayerUseCarJackItem);


            //Detach trailer
            EventController.addKeyEvent("DETACH_TRAILER", ConsoleKey.T, "Anhänger/Wagen abkoppeln", onDetachTrailer);

        }

        private bool onDetachTrailer(IPlayer player, ConsoleKey key, string eventName) {
            if(player.IsInVehicle) {
               if(player.Vehicle.Driver == player) {
                    player.emitClientEvent("DETACH_VEHICLE_TRAILER", player.Vehicle);
                } 
            }

            return false;
        }

        private bool onOpenFs(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            WebController.openFileSystem(player);
            return true;
        }

        #region Custom Plane/Helicopter Sync

        private void updatePlaneHelicopterSync(IInvoke obj) {
            return;
            foreach (var vehicle in ChoiceVAPI.GetVehicles(v => v.DbModel != null && (v.DbModel.classId == 15 || v.DbModel.classId == 16))) {
                vehicle.SetSyncedMetaData("p", vehicle.Position.Round());
                vehicle.SetSyncedMetaData("r", vehicle.Rotation.Round());
                vehicle.SetSyncedMetaData("i", (int)IslandController.getIslandFromDimension(vehicle.Dimension));
            }
        }

        #endregion

        #region SirenSound

        private bool onVehicleSirenSound(IPlayer player, ConsoleKey key, string eventName) {
            var veh = (ChoiceVVehicle)player.Vehicle;
            if (player.Seat == 1) {
                veh.SetStreamSyncedMetaData("VEHICLE_SIREN_SOUND", veh.SirenSound = !veh.SirenSound);
            }
            return true;
        }

        #endregion

        #region Put Vehicle on Transporter

        private MenuItem transporterListItemGenerator(IEntity sender, IEntity target) {
            var veh = target as ChoiceVVehicle;

            var list = new string[] { "Links", "Rechts" };
            if (veh.AttachedTo != null && veh.AttachedTo != null && ((ChoiceVVehicle)veh.AttachedTo).DbModel.ModelName == "Flatbed") {
                list = ["Links", "Rechts", "Hinten"];
            }

            return new ListMenuItem("Fahrzeug abladen", "Lade das Fahrzeug ab", list, "PULL_VEHICLE_OFF_TRANSPORT", MenuItemStyle.normal, true);

        }

        private bool onTransportVehicleOnPredicateSender(IPlayer player, bool special) {
            if (player != null) {
                if (!special) {
                    var transport = ChoiceVAPI.FindNearbyVehicle(player, 20, v => VehicleController.hasVehicleSpecialFlag(v, SpecialVehicleModelFlag.IsFlatBed));

                    if (transport != null && !transport.hasData("IS_FULL_WITH_VEHICLES")) {
                        return true;
                    }

                } else {
                    var transport = ChoiceVAPI.FindNearbyVehicle(player, 12.5f, v => VehicleController.hasVehicleSpecialFlag(v, SpecialVehicleModelFlag.IsFlatBed));

                    if (transport != null && !transport.hasData("IS_FULL_WITH_VEHICLES")) {
                        return false;
                    } else {
                        var transportSpecial = ChoiceVAPI.FindNearbyVehicle(player, 75, v => VehicleController.hasVehicleSpecialFlag(v, SpecialVehicleModelFlag.IsFlatBed));

                        if (transportSpecial != null && !transport.hasData("IS_FULL_WITH_VEHICLES")) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool onTransportVehicleOnPredicateTarget(ChoiceVVehicle vehicle) {
            if (vehicle != null) {
                if (!vehicle.hasData("IS_BEING_TRANSPORTED")) {
                    return true;
                }
            }

            return false;
        }

        private bool onPutVehicleOnTransport(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var target = (ChoiceVVehicle)data["InteractionTarget"];
            var transport = ChoiceVAPI.FindNearbyVehicle(player, 20, v => VehicleController.hasVehicleSpecialFlag(v, SpecialVehicleModelFlag.IsFlatBed));

            if(target == transport) {
                player.sendBlockNotification("Du hast nicht echt versucht gerade einen Flatbed auf einen Flatbed zu laden? Ernsthaft Harry?", "Du Idiot");
                return false;
            }

            if (target != null && transport != null) {
                if (!transport.hasData("IS_FULL_WITH_VEHICLES")) {
                    var startPoint = target.DbModel.StartPoint.FromJson();
                    var endPoint = target.DbModel.StartPoint.FromJson();
                    var length = Math.Abs(startPoint.Y) + Math.Abs(endPoint.Y);

                    if (!(transport.DbModel.ModelName == "Flatbed" && length > 8)) {
                        var anim = AnimationController.getAnimationByName("WORK_FRONT");
                        AnimationController.animationTask(player, anim, () => {
                            attachVehicleToOther(target, transport);
                        }, null, true, 1);
                    } else {
                        player.sendBlockNotification("Das Transportfahrzeug ist zu klein für das Fahrzeug!", "Transport zu klein!", Constants.NotifactionImages.Car);
                    }
                } else {
                    player.sendBlockNotification("Das Transportfahrzeug ist voll!", "Fahrzeug voll!", Constants.NotifactionImages.Car);
                }
            } else {
                player.sendBlockNotification("Es wurde keine passendes Fahrzeug zum beladen gefunden!", "Beladen unmöglich", Constants.NotifactionImages.Car);
            }

            return true;
        }

        private void onVehicleSpawnWithTransported(ChoiceVVehicle vehicle, vehiclesdatum data) {
            var targetId = int.Parse(data.value);

            var transport = ChoiceVAPI.FindVehicleById(targetId);

            if (transport != null) {
                attachVehicleToOther(vehicle, transport);
            } else {
                Logger.logWarning(LogCategory.Vehicle, LogActionType.Blocked, vehicle, "Vehicle could not find its transport. Looking again in 30secs");
                InvokeController.AddTimedInvoke("Vehicle-Attach-Invoke", (i) => {
                    var transport = ChoiceVAPI.FindVehicleById(targetId);

                    if (transport != null) {
                        onVehicleSpawnWithTransported(vehicle, data);
                    } else {
                        Logger.logError($"A vehicle could not found the transporting vehicle!, VehicleId: {vehicle.Id}, TransportId: {targetId}",
                            $"Fehler im Fahrzeug aufladen: Das Aufladefahrzeug konnte nicht gefunden werden: Fahrzeug-Id: {vehicle.Id}, Transport-Fahrzeug-Id: {targetId}");
                    }
                }, TimeSpan.FromSeconds(30), false);
            }
        }

        private void attachVehicleToOther(ChoiceVVehicle toAttach, ChoiceVVehicle transporter) {
            var startPoint = toAttach.DbModel.StartPoint.FromJson();

            var zOffset = Math.Abs(startPoint.Z);
            toAttach.SetNetworkOwner(transporter.NetworkOwner, false);

            if (transporter.DbModel.ModelName == "TrFlat") {
                var endPoint = toAttach.DbModel.StartPoint.FromJson();
                var length = Math.Abs(startPoint.Y) + Math.Abs(endPoint.Y);

                if (length >= 6.5 && !transporter.hasData("TRANSPORTED_VEHICLES_COUNT")) {
                    //toAttach.AttachToEntity(transporter, 0, 0, new Position(0, 0, 0.4f + zOffset), Rotation.Zero, true, false);

                    transporter.NetworkOwner?.emitClientEvent("ATTACH_VEHICLE_TO_VEHICLE", transporter, toAttach, 0, 0, 0, 0.4f + zOffset, 20);
                    transporter.setData("IS_FULL_WITH_VEHICLES", true);
                } else {
                    if (transporter.hasData("TRANSPORTED_VEHICLES_SPOTS")) {
                        var list = (ChoiceVVehicle[])transporter.getData("TRANSPORTED_VEHICLES_SPOTS");

                        if (list[0] == null) {
                            transporter.NetworkOwner?.emitClientEvent("ATTACH_VEHICLE_TO_VEHICLE", transporter, toAttach, 0, 0, -3, 0.4f + zOffset, 20);
                            //toAttach.AttachToEntity(transporter, 0, 0, new Position(0, -3, 0.4f + zOffset), Rotation.Zero, true, false);
                            list[0] = toAttach;
                        } else {
                            transporter.NetworkOwner?.emitClientEvent("ATTACH_VEHICLE_TO_VEHICLE", transporter, toAttach, 0, 0, 3, 0.4f + zOffset, 20);
                            //toAttach.AttachToEntity(transporter, 0, 0, new Position(0, 3, 0.4f + zOffset), Rotation.Zero, true, false);
                            list[1] = toAttach;
                        }

                        if (list[0] != null && list[1] != null) {
                            transporter.setData("IS_FULL_WITH_VEHICLES", true);
                        }

                        transporter.setData("TRANSPORTED_VEHICLES_SPOTS", list);
                    } else {
                        var list = new ChoiceVVehicle[] { toAttach, null };
                        //toAttach.AttachToEntity(transporter, 0, 0, new Position(0, -3, 0.4f + zOffset), Rotation.Zero, true, false);
                        transporter.NetworkOwner?.emitClientEvent("ATTACH_VEHICLE_TO_VEHICLE", transporter, toAttach, 0, 0, -3, 0.4f + zOffset, 20);

                        transporter.setData("TRANSPORTED_VEHICLES_SPOTS", list);
                    }

                }

                //TODO FreifhtTrailer can hold one big vehicle or one small one (maybe make player select?)
                //Big 0 2.1
            } else {
                //toAttach.AttachToEntity(transporter, 0, 0, new Position(0, -10, 2.1f + zOffset * 3), Rotation.Zero, true, false);
                //toAttach.AttachToEntity(transporter, 0, 0, new Position(0, -2.5f, 0.4f + zOffset), Rotation.Zero, true, false);
                
                transporter.NetworkOwner?.emitClientEvent("ATTACH_VEHICLE_TO_VEHICLE", transporter, toAttach, 0, 0, -2.5f, 0.4f + zOffset, 20);

                transporter.setData("IS_FULL_WITH_VEHICLES", true);
                transporter.setData("TRANSPORTED_VEHICLES_SPOTS", new ChoiceVVehicle[] { toAttach });
            }

            toAttach.setPermanentData("IS_BEING_TRANSPORTED", transporter.VehicleId.ToString());
        }

        #endregion

        #region Pull Vehicle Off Transporter

        private bool onTransportVehicleOffPredicateTarget(ChoiceVVehicle choiceVVehicle) {
            return choiceVVehicle.hasData("IS_BEING_TRANSPORTED");
        }

        private bool onPullVehicleOffTransport(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (ChoiceVVehicle)data["InteractionTarget"];
            var id = int.Parse((string)vehicle.getData("IS_BEING_TRANSPORTED"));
            var transporter = ChoiceVAPI.FindVehicleById(id);

            var evt = menuItemCefEvent as ListMenuItemEvent;

            if (transporter != null) {
                Vector2 rotVec;
                if (evt.currentElement != "Hinten") {
                    var mult = 1;
                    if (evt.currentElement == "Links") {
                        mult = -1;
                    }

                    rotVec = ChoiceVAPI.rotatePointInRect(4 * mult, 0, 0, 0, transporter.Rotation.Yaw);
                } else {
                    rotVec = ChoiceVAPI.rotatePointInRect(0, -7, 0, 0, transporter.Rotation.Yaw);
                }

                var newVehiclePos = new Position(vehicle.Position.X + rotVec.X, vehicle.Position.Y + rotVec.Y, vehicle.Position.Z - 0.5f);
                if (menuItemCefEvent.action != "enter") {
                    if (transporter != null) {
                        var marker = MarkerController.createMarker(27, new Position(newVehiclePos.X, newVehiclePos.Y, newVehiclePos.Z - 1), new Rgba(204, 138, 37, 255), 1.5f, 20, new List<IPlayer> { player });
                        InvokeController.AddTimedInvoke("Marker-Remover", (i) => {
                            MarkerController.removeMarker(marker);
                        }, TimeSpan.FromSeconds(2), false);
                    }
                } else {
                    if (vehicle != null) {
                        var anim = AnimationController.getAnimationByName("WORK_FRONT");
                        AnimationController.animationTask(player, anim, () => {
                            vehicle.Detach();
                            vehicle.NetworkOwner?.emitClientEvent("DETACH_VEHICLE", vehicle);
                            vehicle.Dimension = 9999999;
                            InvokeController.AddTimedInvoke("Detach-Vehicle", (i) => {
                                vehicle.Rotation = transporter.Rotation;
                                vehicle.Position = newVehiclePos;
                                vehicle.Dimension = player.Dimension;
                            }, TimeSpan.FromSeconds(1), false);

                            vehicle.resetPermantData("IS_BEING_TRANSPORTED");

                            if (transporter.DbModel.ModelName == "Flatbed") {
                                transporter.resetData("IS_FULL_WITH_VEHICLES");
                                transporter.resetData("TRANSPORTED_VEHICLES_SPOTS");
                            } else if (transporter.DbModel.ModelName == "TrFlat") {
                                var list = (ChoiceVVehicle[])transporter.getData("TRANSPORTED_VEHICLES_SPOTS");

                                if (list != null) {
                                    if (list[0] == vehicle) {
                                        list[0] = null;
                                    } else {
                                        list[1] = null;
                                    }

                                    if (list[0] == null && list[1] == null) {
                                        transporter.resetData("TRANSPORTED_VEHICLES_SPOTS");
                                    }

                                    transporter.setData("TRANSPORTED_VEHICLES_SPOTS", list);
                                }

                                transporter.resetData("IS_FULL_WITH_VEHICLES");
                            }
                        }, null, true, 1);
                        //TODO MAKE Direct Parking possible if stood in specififc CollisionSHape.
                    }
                }
            }
            return true;
        }

        #endregion

        //TODO REMOVE IF ALTV FIXES
        private void onNetworkOwnerChange(IEntity entity, IPlayer oldOwner, IPlayer newOwner) {
            if (entity is ChoiceVVehicle) {
                var vehicle = entity as ChoiceVVehicle;
                if (vehicle.hasData("TRANSPORTED_VEHICLES_SPOTS")) {
                    var list = (ChoiceVVehicle[])vehicle.getData("TRANSPORTED_VEHICLES_SPOTS");

                    vehicle.resetData("IS_FULL_WITH_VEHICLES");
                    vehicle.resetData("TRANSPORTED_VEHICLES_SPOTS");
                    foreach (var toAttach in list) {
                        if (toAttach != null) {
                            toAttach.SetNetworkOwner(newOwner, false);
                            toAttach.Detach();
                            oldOwner.emitClientEvent("DETACH_VEHICLE", toAttach);
                            toAttach.NetworkOwner?.emitClientEvent("DETACH_VEHICLE", toAttach);
                            InvokeController.AddTimedInvoke("Reattach", (i) => {
                                attachVehicleToOther(toAttach, vehicle);
                            }, TimeSpan.FromSeconds(1), false);
                        }
                    }
                }
            }
        }

        #region ToggleVehicleWindow

        private bool onToggleVehicleWindowChoose(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var targetId = (int)data["VEHICLE"];
            var windowId = (int)data["WINDOW_ID"];
            var vehicle = ChoiceVAPI.FindVehicleById(targetId);

            //if(vehicle == null) {
            //    return false;
            //}

            //switch(windowId) {
            //    case 0:
            //        vehicle.SetStreamSyncedMetaData("FIRST_WINDOW_STATE", vehicle.FirstWindowState = !vehicle.FirstWindowState);
            //        break;
            //    case 1:
            //        vehicle.SetStreamSyncedMetaData("SECOND_WINDOW_STATE", vehicle.SecondWindowState = !vehicle.SecondWindowState);
            //        break;
            //    case 2:
            //        vehicle.SetStreamSyncedMetaData("THIRD_WINDOW_STATE", vehicle.ThirdWindowState = !vehicle.ThirdWindowState);
            //        break;
            //    case 3:
            //        vehicle.SetStreamSyncedMetaData("FOURTH_WINDOW_STATE", vehicle.FourthWindowState = !vehicle.FourthWindowState);
            //        break;
            //    case 4:
            //        vehicle.SetStreamSyncedMetaData("FIRST_WINDOW_STATE", vehicle.FirstWindowState = !vehicle.FirstWindowState);
            //        vehicle.SetStreamSyncedMetaData("SECOND_WINDOW_STATE", vehicle.SecondWindowState = vehicle.FirstWindowState);
            //        vehicle.SetStreamSyncedMetaData("THIRD_WINDOW_STATE", vehicle.ThirdWindowState = vehicle.FirstWindowState);
            //        vehicle.SetStreamSyncedMetaData("FOURTH_WINDOW_STATE", vehicle.FourthWindowState = vehicle.FirstWindowState);
            //        break;
            //}

            return true;
        }

        private Menu getVehicleWindowMenu(ChoiceVVehicle vehicle, IPlayer player) {
            var menu = new Menu("Fahrzeugfenster", "Öffne/Schließe die Fenster des Fahrzeuges");

            if (player.Seat == 1) {
                if (vehicle.DbModel.doorCount >= 2) {
                    menu.addMenuItem(new ClickMenuItem("Alle", "Alle Fenster öffnen/schließen", "", "TOGGLE_VEHICLE_WINDOW_CHOOSE")
                        .withData(new Dictionary<string, dynamic> { { "WINDOW_ID", 4 }, { "VEHICLE", vehicle.VehicleId } }));
                    menu.addMenuItem(new ClickMenuItem("Fahrer", "Fahrerfenster öffnen/schließen", "", "TOGGLE_VEHICLE_WINDOW_CHOOSE")
                        .withData(new Dictionary<string, dynamic> { { "WINDOW_ID", 0 }, { "VEHICLE", vehicle.VehicleId } }));
                    menu.addMenuItem(new ClickMenuItem("Beifahrer", "Beifahrerfenster öffnen/schließen", "", "TOGGLE_VEHICLE_WINDOW_CHOOSE")
                        .withData(new Dictionary<string, dynamic> { { "WINDOW_ID", 1 }, { "VEHICLE", vehicle.VehicleId } }));
                } else if (vehicle.DbModel.doorCount >= 4) {
                    menu.addMenuItem(new ClickMenuItem("Hinten links", "Fenster hinten links öffnen/schließen", "", "TOGGLE_VEHICLE_WINDOW_CHOOSE")
                        .withData(new Dictionary<string, dynamic> { { "WINDOW_ID", 2 }, { "VEHICLE", vehicle.VehicleId } }));
                    menu.addMenuItem(new ClickMenuItem("Hinten rechts", "Fenster hinten rechts öffnen/schließen", "", "TOGGLE_VEHICLE_WINDOW_CHOOSE")
                        .withData(new Dictionary<string, dynamic> { { "WINDOW_ID", 3 }, { "VEHICLE", vehicle.VehicleId } }));
                }
            } else {
                menu.addMenuItem(new ClickMenuItem("Mein Fenster", "Mein Fenster öffnen/schließen", "", "TOGGLE_VEHICLE_WINDOW_CHOOSE")
                    .withData(new Dictionary<string, dynamic> { { "WINDOW_ID", player.Seat - 1 }, { "VEHICLE", vehicle.VehicleId } }));
            }

            return menu;
        }
        #endregion

        #region ToggleVehicleDoor

        private Menu getVehicleDoorMenuInteract(IEntity sender, IEntity target) {
            return getVehicleDoorMenu(target as ChoiceVVehicle, sender as IPlayer);
        }

        private Menu getVehicleDoorMenu(ChoiceVVehicle vehicle, IPlayer player) {
            var menu = new Menu("Fahrzeugtüren", "Öffne/Schließe die Türen des Fahrzeuges");

            if (vehicle is null) {
                return menu;
            }

            menu.addMenuItem(new ClickMenuItem("Motorhaube", "Motorhaube öffnen/schließen", "", "TOGGLE_VEHICLE_DOOR_CHOOSE", MenuItemStyle.normal, false, false)
                .withData(new Dictionary<string, dynamic> { { "DOOR_ID", 4 }, { "VEHICLE", vehicle.VehicleId } }));
            menu.addMenuItem(new ClickMenuItem("Kofferraum", "Kofferraum öffnen/schließen", "", "TOGGLE_VEHICLE_DOOR_CHOOSE", MenuItemStyle.normal, false, false)
                .withData(new Dictionary<string, dynamic> { { "DOOR_ID", 5 }, { "VEHICLE", vehicle.VehicleId } }));

            if (vehicle.DbModel.doorCount >= 2) {
                menu.addMenuItem(new ClickMenuItem("Fahrertür", "Fahrertür öffnen/schließen", "", "TOGGLE_VEHICLE_DOOR_CHOOSE", MenuItemStyle.normal, false, false)
                    .withData(new Dictionary<string, dynamic> { { "DOOR_ID", 0 }, { "VEHICLE", vehicle.VehicleId } }));
                menu.addMenuItem(new ClickMenuItem("Beifahrertür", "Beifahrertür öffnen/schließen", "", "TOGGLE_VEHICLE_DOOR_CHOOSE", MenuItemStyle.normal, false, false)
                    .withData(new Dictionary<string, dynamic> { { "DOOR_ID", 1 }, { "VEHICLE", vehicle.VehicleId } }));
            } else if (vehicle.DbModel.doorCount >= 4) {
                menu.addMenuItem(new ClickMenuItem("Hinten Links", "Hinten Links öffnen/schließen", "", "TOGGLE_VEHICLE_DOOR_CHOOSE", MenuItemStyle.normal, false, false)
                    .withData(new Dictionary<string, dynamic> { { "DOOR_ID", 2 }, { "VEHICLE", vehicle.VehicleId } }));
                menu.addMenuItem(new ClickMenuItem("Hinten Rechts", "Hinten Rechts öffnen/schließen", "", "TOGGLE_VEHICLE_DOOR_CHOOSE", MenuItemStyle.normal, false, false)
                    .withData(new Dictionary<string, dynamic> { { "DOOR_ID", 3 }, { "VEHICLE", vehicle.VehicleId } }));
            }

            return menu;
        }

        private bool onToggleVehicleDoorChoose(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var targetId = (int)data["VEHICLE"];
            var doorId = (byte)data["DOOR_ID"];
            var vehicle = ChoiceVAPI.FindVehicleById(targetId);

            if (vehicle == null) {
                return false;
            }

            if (doorId == 4) {
                vehicle.setHoodState(!vehicle.isHoodOpen());
            } else if (doorId == 5) {
                vehicle.setTrunkState(!vehicle.isTrunkOpen());
            } else {
                vehicle.setDoorState(doorId, !vehicle.isDoorOpen(doorId));
            }

            return true;
        }

        #endregion

        #region Caddy3 Stuff

        private Inventory onCheckForNearbyCaddy(IPlayer player) {
            var caddy = ChoiceVAPI.FindNearbyVehicle(player, 5, v => v.DbModel.ModelName == "Caddy3");

            if (caddy != null) {
                if (caddy.LockState == AltV.Net.Enums.VehicleLockState.Unlocked) {
                    return caddy.Inventory;
                } else {
                    return null;
                }
            } else {
                return null;
            }
        }

        #endregion

        #region Carjack

        private bool onPlayerUseCarJackItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (ChoiceVVehicle)data["InteractionTarget"];

            var itemAnim = AnimationController.getAnimationByName("USE_CARJACK_TOOL");
            AnimationController.animationTask(player, itemAnim, () => {
                var item = player.getInventory().getItem<ToolItem>(i => i.Flag == SpecialToolFlag.CarJack);
                item.use(player);

                vehicle.Position = new Position(vehicle.Position.X, vehicle.Position.Y, vehicle.Position.Z + 0.25f);
                vehicle.Rotation = new Rotation(vehicle.Rotation.Roll + 3.14f, vehicle.Rotation.Pitch, vehicle.Rotation.Yaw);
            });

            return true;
        }

        #endregion
    }
}