using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Collectables;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Controller.CraftingSystem;
using ChoiceVServer.Controller.Crime_Stuff;
using ChoiceVServer.Controller.Discord;
using ChoiceVServer.Controller.LockerSystem.Model;
using ChoiceVServer.Controller.Phone;
using ChoiceVServer.Controller.PlaceableObjects;
using ChoiceVServer.Controller.Shopsystem;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.Controller.Vehicles;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Color;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Farming;
using ChoiceVServer.Model.Fire;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Controller.CallbackController;
using static ChoiceVServer.Controller.GasRefuel;
using static ChoiceVServer.Controller.GasstationController;
using static ChoiceVServer.Controller.MiniJobController;
using static ChoiceVServer.Controller.Phone.PhoneCallController;
using static ChoiceVServer.Controller.Phone.PhoneController;
using static ChoiceVServer.Controller.SoundSystem.SoundController;
using static ChoiceVServer.InventorySystem.Evidence;
using System.Text.RegularExpressions;
using ChoiceVServer.Controller.DamageSystem;
using ChoiceVServer.Controller.DamageSystem.Model;
using ChoiceVServer.Controller.Garages;
using ChoiceVServer.Controller.ProcessMachines;
using ChoiceVServer.Controller.ProcessMachines.Model;
using ChoiceVServer.Controller.Voice.YacaModel;
using Mathos.Parser;
using File = ChoiceVServer.InventorySystem.File;

//using ChoiceVServer.Controller.Shops;

namespace ChoiceVServer.Admin.Commands;

public class CommandAttribute : Attribute {
    public string Command;
    public CommandAttribute(string command) {
        Command = command;
    }
}

internal class Commands : ChoiceVScript {
    private static readonly Dictionary<string, MethodInfo> AllCommands = new();

    private static bool toggleWeapon;

    private static readonly InventorySpotType InventorySpotType;
    private static readonly string AnimationIdentifier;
    private static readonly float MaxWeight;
    private static readonly string Name;

    //private static string MaterialName;
    //private static int MaterialCompanyId;

    //[CommandAttribute("createMaterialSpot")]
    //public static void createMaterialSpot(IPlayer player, string[] args) {
    //    try {
    //        MaterialName = args[0];
    //        MaterialCompanyId = int.Parse(args[1]);

    //        CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, onMaterialSpotCreator);
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //private static void onMaterialSpotCreator(Position position, float width, float height, float rotation) {
    //    //MaterialDistributionController.createMaterialSpot(MaterialName, MaterialCompanyId, position, width, height, rotation);
    //}

    private static string ShopName;
    private static ShopTypes ShopType;
    private static int? ShopInventoryId;

    //[CommandAttribute("TorsoMenu")]
    //public static void TorsoMenu(IPlayer player, string[] args) {
    //    try {
    //        var menu = new Menu("TorsoMenu", "");
    //        var torsoMenu = new Menu("TorsoMenu", "");
    //        var torsoList = new List<configclothing>();
    //        var shirtList = new List<configclothing>();
    //        var clothingList = new List<configclothing>();
    //        foreach (var clothes in ClothingShopController.clothingList) {
    //            if(clothes.componentid == 3 && clothes.gender == player.getCharacterData().Gender.ToString()) {
    //                torsoList.Add(clothes);
    //            }
    //            if(clothes.componentid == 8 && clothes.gender == player.getCharacterData().Gender.ToString()) {
    //                shirtList.Add(clothes);
    //            }
    //        }
    //        using(var db = new ChoiceVDb()) {
    //            foreach(var clothes in db.configclothing) {
    //                if(clothes.componentid == 11 && clothes.gender == player.getCharacterData().Gender.ToString()) {
    //                    menu.addMenuItem(new ClickMenuItem($"{clothes.name}", "", "", "Torso_Choose", MenuItemStyle.normal, true).withData(new Dictionary<string, dynamic> { { "Clothing", clothes }, { "List", torsoList }, {"ShirtList", shirtList } }));
    //                }
    //            }
    //        }

    //        player.showMenu(menu, false);
    //    } catch(Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("TestKeyMaster")]
    //public static void TestKeyMaster(IPlayer player, string[] args) {
    //    try {
    //        DoorController.doTest(player);
    //    }
    //    catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    private static string damg;
    private static string appear;
    private static string health;
    private static string script;

    private static string app;

    private static ChoiceVPed stestPed;

    public Commands() {
        EventController.addEvent("chatmessage", handleChatMessage);
        EventController.ConsoleCommandDelegate += onConsoleCommand;

        var methods = GetType().GetMethods();
        foreach(var method in methods.Where(ifo => ifo.CustomAttributes.Any(att => att.AttributeType == typeof(CommandAttribute)))) {
            var cmd = method.GetCustomAttribute<CommandAttribute>();

            AllCommands.Add(cmd.Command.ToLower(), method);
        }
    }

    private bool handleChatMessage(IPlayer player, string eventName, object[] args) {
        var input = args[0].ToString();

        if(input.StartsWith('/')) {
            var commandWithArgs = input.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            var key = commandWithArgs[0].Substring(1).ToLower();
            if(player.getAdminLevel() <= 0) {
                return false;
            }

            if(AllCommands.ContainsKey(key)) {
                AllCommands[key].Invoke(null, new object[] { player, commandWithArgs.Skip(1).ToArray() });
            } else {
                player.emitClientEvent("chatmessage", null, "{FF0000} Command not found!");
            }
        } else {
            player.emitClientEvent("chatmessage", null, "{FF0000} Thats not a command!");
        }

        return true;
    }

    private void onConsoleCommand(string text, string[] args) {
        switch(text) {
            case "testInventory":
                break;
            case "testNumbers":
                new Thread(() => {
                    var list = new List<string>();
                    for(int i = 0; i < 100_000; i++) {
                        var rand = ChoiceVAPI.randomString(6);

                        var count = 0;
                        for(int j = 0; j < 6; j++) {
                            var c = rand[j];
                            if((int)c >= 48 && (int)c <= 57) {
                                count++;
                            }
                        }
                    }
                    Logger.logFatal("RESULT: " + list.Count);
                }).Start();
                break;
        }
    }

    [Command("startLeaveTerminal")]
    public static void startLeaveTerminal(IPlayer player, string[] args) {
        TerminalController.startLeaveTerminal(player, true);
    }
    
    [Command("reloadCommands")]
    public void reloadCommands(IPlayer player, string[] args) {
        AllCommands.Clear();

        var methods = GetType().GetMethods();
        foreach(var method in methods.Where(ifo => ifo.CustomAttributes.Any(att => att.AttributeType == typeof(CommandAttribute)))) {
            var cmd = method.GetCustomAttribute<CommandAttribute>();

            AllCommands.Add(cmd.Command.ToLower(), method);
        }
    }

    #region Vehicle methods

    [Command("createVehicle")]
    public static void createVehicle(IPlayer player, string[] args) {
        try {
            var vehicle = VehicleController.createVehicle(ChoiceVAPI.Hash(args[0]), new Position(player.Position.X, player.Position.Y, player.Position.Z + 0.5f), player.Rotation, player.Dimension, 0, false, args[0]);

            if(args.Length > 1) {
                vehicle.DamageData = args[1];
            }

            InvokeController.AddTimedInvoke("", (i) => {
                player.SetIntoVehicle(vehicle, 1);
            }, TimeSpan.FromMilliseconds(500), false);

            if(!player.getCharacterData().AdminMode) {
                var cfg = InventoryController.getConfigItemForType<VehicleKey>();
                if(vehicle != null) {
                    player.getInventory().addItem(new VehicleKey(cfg, vehicle), true);
                } else {
                    InvokeController.AddTimedInvoke("", i => {
                        var veh = ChoiceVAPI.FindNearbyVehicle(player);
                        if(veh != null) {
                            player.getInventory().addItem(new VehicleKey(cfg, veh), true);
                        }
                    }, TimeSpan.FromSeconds(10), false);
                }
            } else {
                player.sendNotification(NotifactionTypes.Warning, "Da du im Admin Mode bist wurde kein Schlüssel erzeugt. Das Fahrzeug kann so geöffnet werden", "");
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    //private static Dictionary<int, ChoiceVVehicle> LimitVehicles = new Dictionary<int, ChoiceVVehicle>();

    //[CommandAttribute("createVehicleModel")]
    //public static void createVehicleModel(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 0 && args[0].Length > 1) {
    //            var veh = ChoiceVAPI.CreateVehicle(args[0], player.Position, player.Rotation);
    //            if (veh != null) {
    //                uint model = veh.Model;

    //                Alt.RemoveVehicle(veh);

    //                if (model > 0) {
    //                    var vehobj = new VehicleObject {
    //                        ModelId = model,
    //                        colPosition = player.Position,
    //                        colRotation = player.Rotation,
    //                        OwnerId = player.getCharacterId(),
    //                        OwnerName = player.getCharacterName(),
    //                        OwnerHistory = "",
    //                        GarageId = -1,
    //                    };

    //                    if (vehobj != null) {
    //                        vehobj.colPosition.X += 3;
    //                        vehobj.colPosition.Y += 3;
    //                        vehobj.colPosition.Z += 0.1f;

    //                        veh = VehicleController.createVehicle(player, vehobj);
    //                    }
    //                }
    //            }
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("createVehicle")]
    //public static void createVehicle(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 0 && args[0].Length > 1) {
    //            string arg = (args[0].First().ToString().ToUpper() + args[0].Substring(1));

    //            // if (LimitVehicles.ContainsKey(player.getCharacterId())) {
    //            //     VehicleController.deleteVehicle(player, LimitVehicles[player.getCharacterId()]);
    //            // }

    //            var veh = VehicleController.createVehicle(player, arg, -1);

    //            // LimitVehicles[player.getCharacterId()] = veh;
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("createVehiclePlayer")]
    //public static void createVehiclePlayer(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 1) {
    //            var altVName = args[0];
    //            var target = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.Name == altVName);

    //            if (target != null) {
    //                var modeluint = (uint)Enum.Parse(typeof(VehicleModel), args[1]);

    //                var vehobj = new VehicleObject {
    //                    ModelId = modeluint,
    //                    colPosition = target.Position,
    //                    colRotation = target.Rotation,
    //                    OwnerId = target.getCharacterId(),
    //                    OwnerName = target.getCharacterName(),
    //                    OwnerHistory = "",
    //                    GarageId = -1,
    //                };

    //                if (vehobj != null) {
    //                    vehobj.colPosition.X += 3;
    //                    vehobj.colPosition.Y += 3;
    //                    vehobj.colPosition.Z += 0.1f;

    //                    VehicleController.createVehicle(target, vehobj);
    //                }
    //            } else {
    //                player.sendBlockNotification("Der Name ist falsch.", "");
    //            }
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("destroyVehicle")]
    //public static void destroyVehicle(IPlayer player, string[] args) {
    //    try {
    //        var veh = ChoiceVAPI.FindNearbyVehicle(player);

    //        if (veh != null) {
    //            VehicleController.deleteVehicle(player, veh);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("lockVehicleModel")]
    //public static void lockVehicleModel(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 0 && args[0].Length > 1) {
    //            string arg = (args[0].First().ToString().ToUpper() + args[0].Substring(1));

    //            VehicleController.lockVehicleModel(player, arg);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("setFuel")]
    //public static void setFuel(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 0) {
    //            var veh = ChoiceVAPI.FindNearbyVehicle(player);

    //            if (veh != null) {
    //                VehicleController.setFuel(veh, float.Parse(args[0]));
    //            }
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("setVehicleOwner")]
    //public static void setVehicleOwner(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 0) {
    //            var veh = ChoiceVAPI.FindNearbyVehicle(player);

    //            if (veh != null) {
    //                int ownerid = int.Parse(args[0]);
    //                if (ownerid > 0) {
    //                    var newowner = ChoiceVAPI.FindPlayerByCharId(ownerid);
    //                    if (newowner != null) {
    //                        VehicleController.setOwner(veh, ownerid, VehicleOwnerType.Player, newowner.getCharacterName());
    //                    }
    //                }
    //            }
    //        } else {
    //            player.sendNotification(Constants.NotifactionTypes.Info, "Use: Owner ID.", "", Constants.NotifactionImages.Car);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("setVehicleOwnerCompany")]
    //public static void setVehicleOwnerCompany(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 0) {
    //            var veh = ChoiceVAPI.FindNearbyVehicle(player);

    //            if (veh != null) {
    //                int ownerid = int.Parse(args[0]);
    //                if (ownerid > 0) {
    //                    var company = CompanyController.getCompany(ownerid);
    //                    if (company != null) {
    //                        VehicleController.setOwner(veh, ownerid, VehicleOwnerType.Company, company.Name);
    //                    }
    //                }
    //            }
    //        } else {
    //            player.sendNotification(Constants.NotifactionTypes.Info, "Use: Owner ID.", "", Constants.NotifactionImages.Car);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("setVehicleOwnerState")]
    //public static void setVehicleOwnerState(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 0) {
    //            var veh = ChoiceVAPI.FindNearbyVehicle(player);

    //            if (veh != null) {
    //                int ownerid = int.Parse(args[0]);
    //                if (ownerid > 0) {
    //                    var company = CompanyController.getCompany(ownerid);
    //                    if (company != null) {
    //                        VehicleController.setOwner(veh, ownerid, VehicleOwnerType.State, company.Name);
    //                    }
    //                }
    //            }
    //        } else {
    //            player.sendNotification(Constants.NotifactionTypes.Info, "Use: Owner ID.", "", Constants.NotifactionImages.Car);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    #endregion

    #region Vehicle coloring methods

    //[CommandAttribute("colorMenu")]
    //public static void colorMenu(IPlayer player, string[] args) {
    //    try {
    //        var veh = ChoiceVAPI.FindNearbyVehicle(player);

    //        if (veh != null) {
    //            WorkshopController.coloringMenu(player, veh, null);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("resetColor")]
    //public static void resetColor(IPlayer player, string[] args) {
    //    try {
    //        var veh = ChoiceVAPI.FindNearbyVehicle(player);

    //        if (veh != null) {
    //            VehicleController.resetColoring(veh);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    [Command("setLivery")]
    public static void setLivery(IPlayer player, string[] args) {
        try {
            if(args.Length > 0) {
                var veh = ChoiceVAPI.FindNearbyVehicle(player);

                if(veh != null) {
                    var coloring = veh.VehicleColoring;
                    
                    coloring.setLivery(byte.Parse(args[0]));
                    veh.setVehicleColoring(coloring); 
                    VehicleController.saveVehicleColoring(veh);
                }
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    #endregion

    //[CommandAttribute("createItem")]
    //public static void createItem(IPlayer player, string[] args) {
    //    try {
    //        configitems configItem = InventoryController.AllConfigItems[int.Parse(args[0].ToString())];
    //        var item = new Item(configItem, -1);s
    //        InventoryController.registerItem(item, player.getInventory());
    //        player.getInventory().addItem(item);


    //    } catch(Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    [Command("setTattoo")]
    public static void setTattoo(IPlayer player, string[] args) {
        try {
            TattooController.setPlayerTattoo(player, args[0], args[1], bool.Parse(args[2]));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setTattooNoDatabase")]
    public static void setTattooNoDb(IPlayer player, string[] args) {
        try {
            player.emitClientEvent(PlayerSetDecoration, player, "tattoo", args[0], args[1]);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("loadChairs")]
    public static void loadChairs(IPlayer player, string[] args) {
        try {
            SittingController.load();
        } catch(Exception) {

        }
    }

    [Command("freeCam")]
    public static void freeCam(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("FREE_CAM_MODE");
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setPos")]
    public static void setPos(IPlayer player, string[] args) {
        try {
            player.Position = new Position(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getPos")]
    public static void getPos(IPlayer player, string[] args) {
        try {
            ChoiceVAPI.SendChatMessageToPlayer(player, $"Position: x: {player.Position.X}, y: {player.Position.Y}, z: {player.Position.Z}");
        } catch(Exception e) {
            Logger.logException(e);
        }
    }
    
    [Command("getRot")]
    public static void getRot(IPlayer player, string[] args) {
        try {
            ChoiceVAPI.SendChatMessageToPlayer(player, $"Rotation: roll: {player.Rotation.Roll}, pitch: {player.Rotation.Pitch}, yaw: {player.Rotation.Yaw}");
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("charCreator")]
    public static void charCreator(IPlayer player, string[] args) {
        try {
            ConnectionController.openCharCreator(player, true);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("toggleWeapons")]
    public static void toggleWeapons(IPlayer player, string[] args) {
        try {
            if(player.getAdminLevel() >= 3) {
                toggleWeapon = !toggleWeapon;
                if(toggleWeapon) {
                    foreach(var p in ChoiceVAPI.GetAllPlayers()) {
                        p.RemoveAllWeapons(true);
                        p.sendNotification(NotifactionTypes.Warning, "Waffen wurden deaktiviert", "");
                    }
                } else {
                    foreach(var p in ChoiceVAPI.GetAllPlayers()) {
                        p.sendNotification(NotifactionTypes.Info, "Waffen wurden aktiviert", "");
                    }
                }
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("giveWeapons")]
    public static void giveWeapons(IPlayer player, string[] args) {
        try {
            if(toggleWeapon && player.getAdminLevel() <= 3) {
                player.sendNotification(NotifactionTypes.Warning, "Waffen sind deaktiviert!", "");
                return;
            }

            foreach(var model in Enum.GetValues<WeaponModel>()) {
                player.GiveWeapon(model, 9999, false);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getHeading")]
    public static void getHeading(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("GET_HEADING");
            ChoiceVAPI.SendChatMessageToPlayer(player, "Look in F8");
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("carry")]
    public static void carry(IPlayer player, string[] args) {
        try {
            //0 0.2 -0.02
            var pl = ChoiceVAPI.FindPlayerByCharId(609);
            pl.Detach();
            pl.AttachToEntity(player, 91, 0, new Position(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2])), new DegreeRotation(0, 90, 180), false, false);
            player.playAnimation("missfinale_c2mcs_1", "fin_c2_mcs_1_camman", 1000000, 49);
            pl.playAnimation("nm", "firemans_carry", 1000000, 33);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    static Controller.Object pup;
    [Command("carryPup")]
    public static void carryPup(IPlayer player, string[] args) {
        try {
            if(pup != null) {
                ObjectController.deleteObject(pup);
            }
            //0 0.2 -0.02
            pup = ObjectController.createObject("skaar_dummy_worn_male_", player, new Position(0.3f, -0.175f, -0.45f), new DegreeRotation(5, -5, 10), 10706);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("pushPup")]
    public static void pushPup(IPlayer player, string[] args) {
        try {
            if(pup != null) {
                ObjectController.deleteObject(pup);
            }
            //0 0.2 -0.02
            player.playAnimation("veh@bike@sport@front@base", "lean_r_slow", 1000000, 49);
            pup = ObjectController.createObject("skaar_dummy_lying_stretcher_male", player, new Position(0.32f, 0.7f, -0.35f), new DegreeRotation(0, 0, 180), 36029);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [CommandAttribute("createVehicleKey")]
    public static void createVehicleKey(IPlayer player, string[] args) {
        try {
            var cf = InventoryController.getConfigItemForType<VehicleKey>();
            player.getInventory().addItem(new VehicleKey(cf, (ChoiceVVehicle)player.Vehicle), true);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setClothes")]
    public static void setPlayerClothes(IPlayer player, string[] args) {
        try {
            var clothing = (ClothingPlayer)player.getData(DATA_CLOTHING_SAVE);
            clothing.UpdateClothSlot(int.Parse(args[0]), int.Parse(args[1]), int.Parse(args[2]));
            ClothingController.loadPlayerClothing(player, clothing);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setAccessoire")]
    public static void setPlayerAccessoire(IPlayer player, string[] args) {
        try {
            var clothing = (ClothingPlayer)player.getData(DATA_CLOTHING_SAVE);
            clothing.UpdateAccessorySlot(int.Parse(args[0]), int.Parse(args[1]), int.Parse(args[2]));
            ClothingController.loadPlayerClothing(player, clothing);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

[Command("createObject")]
public static void createObject(IPlayer player, string[] args) {
    try {
        var x = float.Parse(args[1]);
        var y = float.Parse(args[2]);
        var z = float.Parse(args[3]);

        var obj = ObjectController.createObject(args[0], player.Position, new DegreeRotation(x, y, z));

        if(args.Length <= 4) {
            InvokeController.AddTimedInvoke("Object-Remover", (i) => {
                ObjectController.deleteObject(obj);
            }, TimeSpan.FromSeconds(30), false);
        }
    } catch(Exception e) {
        Logger.logException(e);
    }
}

    [Command("createObjectOnGround")]
    public static void createObjectOnGround(IPlayer player, string[] args) {
        try {
            var obj = ObjectController.createObjectPlacedOnGroundProperly(args[0], player.Position, new DegreeRotation(float.Parse(args[2]), float.Parse(args[3]), float.Parse(args[4])), 200, true, float.Parse(args[1]));

            if(args.Length <= 4) {
                InvokeController.AddTimedInvoke("Object-Remover", (i) => {
                    ObjectController.deleteObject(obj);
                }, TimeSpan.FromSeconds(30), false);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setCl")]
    public static void setCl(IPlayer player, string[] args) {
        try {
            player.emitClientEvent(PlayerSetClothes, int.Parse(args[0]), int.Parse(args[1]), int.Parse(args[2]), int.Parse(args[3]));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("triggerClientEvent")]
    public static void triggerClientEvent(IPlayer player, string[] args) {
        try {
            player.emitClientEvent(args[0]);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testJob")]
    public static void testJob(IPlayer player, string[] args) {
        try {
            MiniJobController.Minijobs.First(m => m.Id == int.Parse(args[0])).startMiniJob(player);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getCash")]
    public static void getCash(IPlayer player, string[] args) {
        try {
            player.addCash(decimal.Parse(args[0]));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setHealth")]
    public static void setHealth(IPlayer player, string[] args) {
        try {
            player.Health = ushort.Parse(args[0]);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("createDriversLicense")]
    public static void createDriversLicense(IPlayer player, string[] args) {
        try {
            var lic = new DriversLicense(InventoryController.getConfigById(65), player, DriverLicenseClasses.PKW);
            player.getInventory().addItem(lic);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("createBlip")]
    public static void createBlip(IPlayer player, string[] args) {
        try {
            BlipController.createPointBlipDb(String.Join(" ", args.Skip(2)), player.Position, int.Parse(args[0]), int.Parse(args[1]), 255, player.getIsland());
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testBlip")]
    public static void testBlip(IPlayer player, string[] args) {
        try {
            BlipController.createPointBlip(player, "Test", player.Position, 74, 475, 200, "TestBlip");
            InvokeController.AddTimedInvoke("Remove", i => {
                BlipController.destroyBlipByName(player, "TestBlip");
            }, TimeSpan.FromSeconds(5), false);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("itemAnimCreator")]
    public static void itemAnimCreator(IPlayer player, string[] args) {
        try {
            ItemAnimationCreator.CommandAnimationPlace(player, args[0], args[1], int.Parse(args[2]), args[3], args[4]);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    //USE /collectAreaCreator start TYPE MAXCOLLECABLES DELAY NAME

    [Command("collectAreaCreator")]
    public static void collectAreaCreator(IPlayer player, string[] args) {
        try {
            switch(args[0]) {
                case "start":
                    player.emitClientEvent("AREA_START", args[1], args[2], args[3], args[4]);
                    break;
                case "show":
                    var type = (CollectableAreaTypes)Enum.Parse(typeof(CollectableAreaTypes), args[1]);
                    foreach(var area in CollectableController.AllAreas.Values) {
                        if(area.Type == type) {
                            foreach(var point in area.Polygon) {
                                player.emitClientEvent("AREA_ADD", point.X, point.Y);
                            }
                        }
                    }
                    break;
                case "stop":
                    player.emitClientEvent("AREA_END");
                    break;

            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("playAnim")]
    public static void playAnim(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("PLAY_ANIM", args[0], args[1], int.Parse(args[2]), int.Parse(args[3]), -1, false, false, args.Length > 3 ? float.Parse(args[4]) : 0);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("saveOutfit")]
    public static void saveOutfit(IPlayer player, string[] args) {
        try {
            var name = args[0];
            var gender = args[1];
            var desc = args[2].Replace('_', ' ');

            var cloth = player.getClothing();
            using(var db = new ChoiceVDb()) {
                db.configoutfits.Add(new configoutfit {
                    name = name,
                    gender = gender,
                    description = desc,
                    torso_drawable = cloth.Torso.Drawable,
                    torso_texture = cloth.Torso.Texture,
                    top_drawable = cloth.Top.Drawable,
                    top_texture = cloth.Top.Texture,
                    shirt_drawable = cloth.Shirt.Drawable,
                    shirt_texture = cloth.Shirt.Texture,
                    accessoire_drawable = cloth.Accessories.Drawable,
                    accessoire_texture = cloth.Accessories.Texture,
                    legs_drawable = cloth.Legs.Drawable,
                    legs_texture = cloth.Legs.Texture,
                    feet_drawable = cloth.Feet.Drawable,
                    feet_texture = cloth.Feet.Texture
                });
                db.SaveChanges();
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("playAnimName")]
    public static void playAnimName(IPlayer player, string[] args) {
        try {
            var ident = args[0];
            using(var db = new ChoiceVDb()) {
                var row = db.configanimations.FirstOrDefault(ia => ia.identifier == ident);

                if(row == null) {
                    ChoiceVAPI.SendChatMessageToPlayer(player, "ItemAnimation not found!");
                    return;
                }

                var anim = new Animation(row.dict, row.name, TimeSpan.FromMilliseconds(row.duration), row.flag, row.startAtPercent);
                player.playAnimation(anim);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("playItemAnim")]
    public static void playItemAnim(IPlayer player, string[] args) {
        try {
            var ident = args[0];
            using(var db = new ChoiceVDb()) {
                var row = db.configitemanimations.FirstOrDefault(ia => ia.identifier == ident);

                if(row == null) {
                    ChoiceVAPI.SendChatMessageToPlayer(player, "ItemAnimation not found!");
                    return;
                }

                var anim = new ItemAnimation(row);
                player.playAnimation(anim, null, false);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("stopAnim")]
    public static void stopAnim(IPlayer player, string[] args) {
        try {
            player.stopAnimation();
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("requestIpl")]
    public static void requestIpl(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("REQUEST_IPL", args[0]);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("removeIpl")]
    public static void removeIpl(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("REMOVE_IPL", args[0]);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    ////importNewTasks {type} {jobId} {animationId} {info} {path}
    //[Command("importNewTasks")]
    //public static void importNewTasks(IPlayer player, string[] args) {
    //    try {
    //        var type = int.Parse(args[0]);
    //        var jobId = int.Parse(args[1]);
    //        var animationId = int.Parse(args[2]);
    //        var info = args[3];
    //        info = info.Replace('_', ' ');
    //        var path = args[4];

    //        var xmlDocument = new XmlDocument();
    //        xmlDocument.Load(path);
    //        var node = xmlDocument.DocumentElement.SelectSingleNode("/CMapData/entities");

    //        var newTasks = new List<configcollectjobtask>();

    //        foreach(XmlNode childNode in node.ChildNodes) {
    //            var posNode = childNode.SelectSingleNode("position");
    //            var xP = float.Parse(posNode.Attributes["x"].InnerText, CultureInfo.InvariantCulture);
    //            var yP = float.Parse(posNode.Attributes["y"].InnerText, CultureInfo.InvariantCulture);
    //            var zP = float.Parse(posNode.Attributes["z"].InnerText, CultureInfo.InvariantCulture);

    //            var rotNode = childNode.SelectSingleNode("rotation");
    //            var xR = float.Parse(rotNode.Attributes["x"].InnerText, CultureInfo.InvariantCulture);
    //            var yR = float.Parse(rotNode.Attributes["y"].InnerText, CultureInfo.InvariantCulture);
    //            var zR = float.Parse(rotNode.Attributes["z"].InnerText, CultureInfo.InvariantCulture);
    //            var zW = float.Parse(rotNode.Attributes["w"].InnerText, CultureInfo.InvariantCulture);
    //            var position = new Vector3(xP, yP, zP);
    //            Rotation rotation = new Quaternion(xR, yR, zR, zW).ToEulerAngles();

    //            var propNameNode = childNode.SelectSingleNode("archetypeName");

    //            var propName = propNameNode.InnerText;

    //            newTasks.Add(new configcollectjobtask {
    //                type = type,
    //                jobId = jobId,
    //                position = position.ToJson(),
    //                rotation = rotation.ToJson(),
    //                model = propName,
    //                animationId = animationId,
    //                infoString = info,
    //                colShapeHeight = 2f,
    //                colShapeWidth = 2f,
    //                colShapePosition = position.ToJson(),
    //                colShapeRotation = 0
    //            });
    //        }

    //        using(var db = new ChoiceVDb()) {
    //            foreach(var task in newTasks) {
    //                db.configcollectjobtasks.Add(task);
    //                db.SaveChanges();
    //            }
    //        }
    //    } catch(Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[Command("createDrivingTask")]
    //public static void createDrivingTask(IPlayer player, string[] args) {
    //    try {
    //        var cmd = args[0];
    //        if(cmd == "start") {
    //            type = int.Parse(args[1]);
    //            jobId = int.Parse(args[2]);
    //            info = args[3];
    //            info = info.Replace('_', ' ');
    //            from = player.Position;

    //            ChoiceVAPI.SendChatMessageToPlayer(player, "/createDrivingTask finish um 2. Position zu setzen");
    //        } else {
    //            using(var db = new ChoiceVDb()) {
    //                var table = db.configdrivingjobtasks;
    //                var newtask = new configdrivingjobtask {
    //                    from = from.ToJson(),
    //                    to = player.Position.ToJson(),
    //                    infoString = info,
    //                    type = type,
    //                    jobId = jobId
    //                };

    //                table.Add(newtask);
    //                db.SaveChanges();
    //                ChoiceVAPI.SendChatMessageToPlayer(player, $"Neuer Driving task erstellt: {newtask.ToJson()}");
    //            }
    //        }
    //    } catch(Exception) {

    //    }
    //}

    [Command("setTimeHour")]
    public static void setTimeHour(IPlayer player, string[] args) {
        try {
            foreach(var p in ChoiceVAPI.GetAllPlayers()) {
                p.emitClientEvent("SET_DATE_TIME_HOUR", args[0]);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("resetTimeHour")]
    public static void resetTimeHour(IPlayer player, string[] args) {
        try {
            foreach(var p in ChoiceVAPI.GetAllPlayers()) {
                p.emitClientEvent("SET_DATE_TIME_HOUR", -1);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("showGrid")]
    public static void showGrid(IPlayer player, string[] args) {
        try {
            var grid = WorldController.getWorldGrid(player.Position);
            grid.draw();
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("showGrids")]
    public static void showGrids(IPlayer player, string[] args) {
        try {
            foreach(var grid in WorldController.AllGrids) {
                grid.draw();
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getCurrentGridId")]
    public static void getCurrentGridId(IPlayer player, string[] args) {
        try {
            var grid = WorldController.getWorldGrid(player.Position);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("gridNeighbor")]
    public static void gridNeighbor(IPlayer player, string[] args) {
        try {
            var grid = WorldController.getWorldGrid(player.Position);
            var grids = WorldController.getNeighborGrids(grid);

            foreach(var g in grids) {
                g.draw();
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("addAnalyseSpot")]
    public static void addAnalyseSpot(IPlayer player, string[] args) {
        try {
            using(var db = new ChoiceVDb()) {
                var spot = new configanalysespot {
                    position = player.Position.ToJson(),
                    height = 2,
                    width = 2,
                    rotation = 0,
                    trackVehicles = 0,
                    codeItem = "DnaSpots"
                };

                db.configanalysespots.Add(spot);
                db.SaveChanges();
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("respawn")]
    public static void respawn(IPlayer player, string[] args) {
        try {
            DamageController.revivePlayer(player);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("penTestEvidence")]
    public static void penTestEvidence(IPlayer player, string[] args) {
        try {
            for(var i = 0; i < int.Parse(args[0]); i++) {
                var rand = new Random();
                var x = rand.Next(-4000, 4000);
                var y = rand.Next(-3000, 9000);
                var pos = new Position(x, y, WorldController.getGroundHeightAt(x, y) + 0.5f);
                var type = (EvidenceType)rand.Next(0, 3);

                EvidenceController.createEvidence(pos, type, new List<Evidence.EvidenceData> {
                    new EvidenceData("Waffentyp", $"Die Patrone wurde aus einer Waffe mit Typ gefeuert.", false),
                    new EvidenceData("Waffenname", $"Die Patrone wurde aus einer/einem abgefeuert.", true),
                    new EvidenceData("Ursprung", $"Diese Patrone wurde am {rand.Next(0, 199999).ToString()} um ca. {DateTime.Now.ToString("HH")} Uhr abgefeuert.", true)
                }, false);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("startFire")]
    public static void startFire(IPlayer player, string[] args) {
        try {
            var name = "";
            var size = 3f;
            var fires = 5;
            var spread = 5;

            if(args.Length > 0) {
                name = args[0].ToLower();
            }
            if(args.Length > 1) {
                size = float.Parse(args[1]);
            }
            if(args.Length > 2) {
                fires = int.Parse(args[2]);
            }
            if(args.Length > 3) {
                spread = int.Parse(args[3]);
            } else {
                ChoiceVAPI.SendChatMessageToPlayer(player, "Usage: [forest|farm|gas|oil|apartment|electric] [Size 1-5] [childfires 1-10] [spread 1-10]");
            }

            if(name == "forest") {
                var fire = new FireForest(size > 4f ? 4f : size, fires, 5, spread, "", "", "", 0, 0, DateTime.Now, 60, 60, 60, 0, 0, 0, player.Position, player.Rotation, 9f, 9f);
                fire.initialize();
            } else if(name == "oil") {
                var fire = new FireOil(size > 2f ? 2f : size, fires, 5, spread, "", "", "", 0, 0, DateTime.Now, 60, 60, 60, 0, 0, 0, player.Position, player.Rotation, 9f, 9f);
                fire.initialize();
            } else if(name == "gas") {
                var fire = new FireGas(size > 2f ? 2f : size, fires, 5, spread, "", "", "", 0, 0, DateTime.Now, 60, 60, 60, 0, 0, 0, player.Position, player.Rotation, 9f, 9f);
                fire.initialize();
            } else if(name == "farm") {
                var fire = new FireFarm(size > 4f ? 4f : size, fires, 5, spread, "", "", "", 0, 0, DateTime.Now, 60, 60, 60, 0, 0, 0, player.Position, player.Rotation, 9f, 9f);
                fire.initialize();
            } else if(name == "apartment") {
                var fire = new FireApartment(size > 2f ? 2f : size, fires, 5, spread, "", "", "", 0, 0, DateTime.Now, 60, 60, 60, 0, 0, 0, player.Position, player.Rotation, 9f, 9f);
                fire.initialize();
            } else if(name == "electric") {
                var fire = new FireElectric(size > 4f ? 4f : size, fires, 5, spread, "", "", "", 0, 0, DateTime.Now, 60, 60, 60, 0, 0, 0, player.Position, player.Rotation, 9f, 9f);
                fire.initialize();
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("showFire")]
    public static void showFire(IPlayer player, string[] args) {
        try {
            var shapesByNear = CollisionShape.AllShapes.Where(c => Vector3.Distance(c.Position, player.Position) < 100).Where(d => d.EventName == "FIRE_OBJECT_SPOT");
            foreach(var shape in shapesByNear) {
                player.emitClientEvent("SPOT_ADD", shape.Position.X, shape.Position.Y, shape.Width, shape.Height, 1, shape.Rotation);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("loadConfigWeapons")]
    public static void loadConfigWeapons(IPlayer player, string[] args) {
        try {
            WeaponController.loadConfigWeapons();
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getGenericItem")]
    public static void getGenericItem(IPlayer player, string[] args) {
        try {
            var item = InventoryController.createGenericItem(InventoryController.getConfigById(int.Parse(args[0])));
            player.getInventory().addItem(item);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getGenericStackableItem")]
    public static void getGenericStackableItem(IPlayer player, string[] args) {
        try {
            var item = InventoryController.createGenericStackableItem(InventoryController.getConfigById(int.Parse(args[0])), int.Parse(args[1]), int.Parse(args[2]));
            player.getInventory().addItem(item);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("startRecording")]
    public static void startRecroding(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("CLIP", true);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("stopRecording")]
    public static void stopRecroding(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("CLIP", false);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("startEditor")]
    public static void startEditor(IPlayer player, string[] args) {
        try {
            player.addState(PlayerStates.InCharCreator);
            player.emitClientEvent("EDITOR", true);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("stopEditor")]
    public static void stopEditor(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("EDITOR", false);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    #region HotelCommands

    //// ToDo:
    //// Funktionen:
    //// ======================
    ////
    //// Offen, aber für nach Version 1.0:
    //// ======================
    //// Mitarbeiter, die das Hotelsystem nutzen können. [MEDIUM]
    //// Implementierung einer Schlüsselweitergabe im RP (Buche auf Person X, Bekomme aber Schlüssel, Gebe Schlüssel weiter.) [MEDIUM]
    //// Adressermittlung des Hotels [LOW]

    //internal static IList<HotelRemoveTicket> HotelRemoveTickets = new List<HotelRemoveTicket>();
    //internal static IList<HotelRoomRemoveTicket> HotelRoomRemoveTickets = new List<HotelRoomRemoveTicket>();
    //internal static Dictionary<string, string> AllHotelCommands = new Dictionary<string, string>();

    //[CommandAttribute("ExitHotelRoom")]
    //private static void exitHotelRoom(IPlayer player, string[] args)
    //{
    //    //Suche die nächstgelegene Position und porte dort hin...
    //    Position position = player.getCharacterData().LastPosition;

    //    Hotel nearestHotel = null;
    //    float smallestDistance = float.MaxValue;

    //    foreach (Hotel hotel in HotelController.AllHotels.Values)
    //    {
    //        if (smallestDistance > position.Distance(hotel.DropoutPosition))
    //        {
    //            nearestHotel = hotel;
    //            smallestDistance = position.Distance(hotel.DropoutPosition);
    //        }
    //    }

    //    if (nearestHotel == null)
    //    {
    //        player.sendNotification(NotifactionTypes.Danger,
    //            "Wir können dich hier nicht weg bringen. Bitte wende dich an den Support.",
    //            "Teleport fehlgeschlagen.", NotifactionImages.Hotel);
    //        return;
    //    }

    //    player.SetPosition(nearestHotel.DropoutPosition.X, nearestHotel.DropoutPosition.Y,
    //        nearestHotel.DropoutPosition.Z);
    //    player.sendNotification(NotifactionTypes.Danger,
    //        $"Du wurdest zu {nearestHotel.Name} teleportiert. Dies wurde im Support vermerkt.",
    //        "Teleport erfolgreich.", NotifactionImages.Hotel);

    //    using (var db = new ChoiceVDb())
    //    {
    //        var ticket = new supporttickets
    //        {
    //            playerId = player.getAccountId(),
    //            playersocialClub = player.getData("SOCIALCLUB"),
    //            description =
    //                $"Information: Spieler teleportierte sich zur Dropout-Position von {nearestHotel.Name} durch Kommando /ExitHotelRoom",
    //            date = DateTime.Now,
    //            position = player.Position.ToJson()
    //        };
    //        db.supporttickets.Add(ticket);
    //        db.SaveChanges();
    //    }
    //}

    //private static void ensureHotelCommandHelpDict() {
    //    if (!AllHotelCommands.Any()) {
    //        var methods = typeof(Commands).GetMethods();

    //        //Liste der Hotel-Befehle extrahieren (Für Hilfe-Commando)
    //        foreach (var method in methods.Where(ifo => ifo.CustomAttributes.Any(att => att.AttributeType == typeof(HotelCommandAttribute)))) {
    //            var cmd = method.GetCustomAttribute<HotelCommandAttribute>();
    //            AllHotelCommands.Add(method.Name, cmd.Arguments);
    //        }
    //    }
    //}

    ////onMenuShowTeleportRoomMenu
    //[CommandAttribute("ShowTHotelMenu")]
    //public static void showTHotelMenu(IPlayer player, string[] args) {
    //    HotelController.onMenuShowTeleportRoomMenu(player, null, 0, null, null);
    //}

    [Command("GetDimension")]
    public static void getDimension(IPlayer player, string[] args) {
        player.sendNotification(NotifactionTypes.Info, $"Dimension {player.Dimension}", $"Dimension {player.Dimension}");
    }

    //[CommandAttribute("RegisterTeleportHotelRoom")]
    //[HotelCommandAttribute("[HotelId] [RoomType] [RoomClass] [RoomNumberFirst] [RoomNumberLast] [NumberFormat*] [RoomNumberHeader*]")]
    //public static void registerTeleportHotelRoom(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode && player.getAdminLevel() >= 2) {

    //            if (args.Length < 5) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. Args: [HotelId] [RoomType] [RoomClass] [RoomNumberFirst] [RoomNumberLast] [NumberFormat*] [RoomNumberHeader*]",
    //                    "Teleport-Hotelraum registrieren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                HotelController.sendNotificationToPlayer("Hinweis: Stelle dich GENAU dorthin, wo der Spieler den Raum betreten soll.",
    //                    "Teleport-Hotelraum registrieren.",
    //                    Constants.NotifactionTypes.Info,
    //                    player);
    //                return;
    //            }

    //            string hotelId = args[0].Replace('_', ' ');

    //            if (!HotelController.tryGetHotelByString(hotelId, out var hotelIdI)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [HotelId] falsch. Wert {args[0]} konnte nicht aufgelöst werden. Args: [HotelId] [RoomType] [RoomClass] [RoomNumberFirst] [RoomNumberLast] [NumberFormat*] [RoomNumberHeader*]",
    //                    "Hoteltür registrieren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            if (!int.TryParse(args[1], out int roomTypeI)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [RoomType] falsch. Wert {args[1]} konnte nicht aufgelöst werden. Args: [HotelId] [RoomType] [RoomClass] [RoomNumberFirst] [RoomNumberLast] [NumberFormat*] [RoomNumberHeader*]",
    //                    "Hoteltür registrieren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            if (!int.TryParse(args[2], out int roomClassI)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [RoomClass] falsch. Wert {args[2]} konnte nicht aufgelöst werden. Args: [HotelId] [RoomType] [RoomClass] [RoomNumberFirst] [RoomNumberLast] [NumberFormat*] [RoomNumberHeader*]",
    //                    "Hoteltür registrieren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }
    //            if (!int.TryParse(args[3], out int roomNumberFirst)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [RoomNumberFirst] falsch. Wert {args[3]} konnte nicht aufgelöst werden. Args: [HotelId] [RoomType] [RoomClass] [RoomNumberFirst] [RoomNumberLast] [NumberFormat*] [RoomNumberHeader*]",
    //                    "Hoteltür registrieren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }
    //            if (!int.TryParse(args[4], out int roomNumberLast)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [RoomNumberLast] falsch. Wert {args[4]} konnte nicht aufgelöst werden. Args: [HotelId] [RoomType] [RoomClass] [RoomNumberFirst] [RoomNumberLast] [NumberFormat*] [RoomNumberHeader*]",
    //                    "Hoteltür registrieren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            string roomNumberHeader = string.Empty;
    //            string roomNumberFormat = "0";
    //            if (args.Length > 5) {
    //                roomNumberFormat = args[5];
    //            }
    //            if (args.Length > 6) {
    //                roomNumberHeader = args[6];
    //            }

    //            string formatString = $"{roomNumberHeader}{{0:{roomNumberFormat}}}";

    //            Hotel hotel = HotelController.AllHotels[hotelIdI];

    //            Position position = player.getCharacterData().LastPosition;

    //            int maxDimension = 9999999;
    //            if (HotelController.AllHotelRooms.Values.Any(r => r.RoomTypeIngame == RoomTypeIngame.TeleportRoom))
    //            {
    //                maxDimension = HotelController.AllHotelRooms.Values.Where(r => r.RoomTypeIngame == RoomTypeIngame.TeleportRoom)
    //                    .Select(r => r.Dimension).Max();
    //            }

    //            int roomCounter = 0;
    //            using (var db = new ChoiceVDb()) {
    //                for (int counter = roomNumberFirst; counter <= roomNumberLast; counter++)
    //                {
    //                    maxDimension++;
    //                    string room = string.Format(formatString, counter);
    //                    string doorGroup = $"{hotel.Name}_{room}".Replace(" ", "_").ToUpper();

    //                    if (!db.configdoorgroups.Any(g => string.Equals(g.identifier, doorGroup))) {
    //                        var row = new configdoorgroups {
    //                            identifier = doorGroup,
    //                            description = $"Hoteltüren Zimmer {room} in {hotel.Name}",
    //                            crackAble = 1
    //                        };

    //                        db.configdoorgroups.Add(row);
    //                        db.SaveChanges();
    //                    }

    //                    DoorController.Door hotelRoomDoor = DoorController.registerDoor(new Position(maxDimension- 9999999, position.Y, -100), "Virtual-Room", doorGroup);
    //                    int hotelRoomId = HotelController.registerHotelRoom(room, (RoomTypes)roomTypeI, (RoomClasses)roomClassI, hotel.Id, player, doorGroup, hotelRoomDoor.Id, RoomTypeIngame.TeleportRoom, maxDimension);
    //                    if (hotelRoomId == -1)
    //                    {
    //                        HotelController.sendNotificationToPlayer("Da ist etwas schief gegangen bei Registrierung von {room}!", "Fehler!", NotifactionTypes.Danger, player);
    //                        return;
    //                    }

    //                    roomCounter++;
    //                }
    //            }
    //            HotelController.sendNotificationToPlayer($"{roomCounter} Teleport-Räume erfolgreich dem Hotel zugeordnet.", $"{roomCounter} Teleporträume erstellt.", NotifactionTypes.Success, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("SetHotelDropout")]
    //[HotelCommandAttribute("[HotelId]")]
    //public static void setHotelDropout(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode && player.getAdminLevel() >= 2) {

    //            if (args.Length < 1) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. Args: [HotelId]",
    //                    "Hoteltür registrieren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            string hotelId = args[0].Replace('_', ' ');

    //            if (!HotelController.tryGetHotelByString(hotelId, out var hotelIdI)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [HotelId] falsch. Wert {hotelId} konnte nicht aufgelöst werden. Args: [HotelId]",
    //                    "Hoteltür registrieren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            HotelController.setHotelDropout(player, hotelIdI);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("RegisterHotel")]
    //[HotelCommandAttribute("[HotelName, Replace Space with Underline!] [HotelType] [HotelStars] [BankAccountId] [CharID-Owner, Optional]")]
    //public static void registerHotel(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {

    //            if (args.Length < 4) {
    //                HotelController.sendNotificationToPlayer(
    //                    "Anzahl an Argumenten Falsch. Args: [HotelName, Replace Space with Underline!] [HotelType] [HotelStars] [BankAccountId] [CharID-Owner, Optional]",
    //                    "Hotel registrieren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            string hotelName = args[0].Replace('_', ' ');
    //            string hotelTypeT = args[1];
    //            string hotelStarsT = args[2];
    //            string bankAccountIdT = args[3];
    //            string hotelOwnerT = null;

    //            int? charOwnerId = null;

    //            //Command includes owner-information?
    //            if (args.Length > 4) {
    //                hotelOwnerT = args[4];
    //            }

    //            //Convert to HotelType
    //            if (!Enum.TryParse<HotelTypes>(hotelTypeT, out var hotelType)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [HotelType] falsch. Wert {hotelTypeT} ist unbekannt. Args: [HotelName, Replace Space with Underline!] [HotelType] [HotelStars] [BankAccountId] [CharID-Owner, Optional]",
    //                    "Hotel registrieren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            //Convert to HotelStars
    //            if (!int.TryParse(hotelStarsT, out var hotelStars) || hotelStars > 7) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [HotelStars] falsch. Wert {hotelStarsT} ist keine Zahl. Args: [HotelName, Replace Space with Underline!] [HotelType] [HotelStars] [BankAccountId] [CharID-Owner, Optional]",
    //                    "Hotel registrieren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            //Convert to Bankaccount
    //            if (long.TryParse(bankAccountIdT, out var bankAccountId)) {
    //                if (!BankController.accountIdExists(bankAccountId)) {
    //                    HotelController.sendNotificationToPlayer(
    //                        $"Argument [BankAccountId] falsch. Das angegebene Konto {bankAccountId} existiert nicht. Args: [HotelName, Replace Space with Underline!] [HotelType] [HotelStars] [BankAccountId] [CharID-Owner, Optional]",
    //                        "Hotel registrieren fehlgeschlagen.",
    //                        Constants.NotifactionTypes.Warning,
    //                        player);
    //                    return;
    //                }
    //            } else {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [BankAccountId] falsch. Wert {bankAccountIdT} ist keine Zahl. Args: [HotelName, Replace Space with Underline!] [HotelType] [HotelStars] [BankAccountId] [CharID-Owner, Optional]",
    //                    "Hotel registrieren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }


    //            //Convert to Character-ID
    //            if (!string.IsNullOrEmpty(hotelOwnerT)) {
    //                if (!int.TryParse(hotelOwnerT, out var charOwnerIdT)) {
    //                    HotelController.sendNotificationToPlayer(
    //                        $"Argument [CharID-Owner] falsch. Wert {hotelOwnerT} ist keine Zahl. Args: [HotelName, Replace Space with Underline!] [HotelType] [HotelStars] [BankAccountId] [CharID-Owner, Optional]",
    //                        "Hotel registrieren fehlgeschlagen.",
    //                        Constants.NotifactionTypes.Warning,
    //                        player);
    //                    return;
    //                } else {
    //                    charOwnerId = charOwnerIdT;
    //                }
    //            }

    //            int hotelId = HotelController.registerHotel(hotelName, hotelType, hotelStars, bankAccountId,
    //                charOwnerId, player);

    //            if (hotelId != -1) {
    //                registerHotelDoor(player, new[] { hotelId.ToString() });
    //            }
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("GetHotelId")]
    //[HotelCommandAttribute("None")]
    //public static void getHotelId(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            HotelController.getHotelId(player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }

    //}

    //[CommandAttribute("RegisterHotelDoor")]
    //[HotelCommandAttribute("[HotelId]")]
    //public static void registerHotelDoor(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 1) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. Args: [HotelId]",
    //                    "Hoteltür registrieren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            if (!player.hasData("REGISTER_HOTEL_DOOR_MODE")) {
    //                string hotelId = args[0].Replace('_', ' ');

    //                if (!HotelController.tryGetHotelByString(hotelId, out var hotelIdI)) {
    //                    HotelController.sendNotificationToPlayer(
    //                        $"Argument [HotelId] falsch. Wert {hotelId} konnte nicht aufgelöst werden. Args: [HotelId]",
    //                        "Hoteltür registrieren fehlgeschlagen.",
    //                        Constants.NotifactionTypes.Warning,
    //                        player);
    //                    return;
    //                }

    //                if (HotelController.AllHotels.ContainsKey(hotelIdI)) {
    //                    player.setData("REGISTER_HOTEL_DOOR_MODE", true);
    //                    player.emitClientEvent("REGISTER_HOTEL_DOOR_MODE", true, hotelIdI);
    //                    StringBuilder message = new StringBuilder();
    //                    message.AppendLine(
    //                        $"Hotel-Tür-Registrier-Modus aktiviert. Wähle Tür aus und drücke Rechtsklick! Die gewählte Hotel-ID ist: {hotelIdI} ({HotelController.AllHotels[hotelIdI].Name})");
    //                    if (HotelController.AllHotels[hotelIdI].DoorId == null &&
    //                        string.IsNullOrEmpty(HotelController.AllHotels[hotelIdI].DoorGroup)) {
    //                        message.AppendLine($"Hinweis: Es ist noch KEINE Tür gesetzt.");
    //                    } else {
    //                        message.AppendLine(
    //                            $"Hinweis: Es ist bereits eine Tür gesetzt! ({HotelController.AllHotels[hotelIdI].DoorId}{HotelController.AllHotels[hotelIdI].DoorGroup}).");
    //                    }

    //                    HotelController.sendNotificationToPlayer(message.ToString(),
    //                        "Hoteltür: Registriermodus gestartet", Constants.NotifactionTypes.Info, player);
    //                } else {
    //                    HotelController.sendNotificationToPlayer(
    //                        $"Fehler: Für die ID existiert kein Hotel! Die Unbekannte ID war: {hotelIdI}",
    //                        "Hoteltür registrieren fehlgeschlagen.",
    //                        Constants.NotifactionTypes.Warning, player);
    //                }

    //            } else {
    //                player.resetData("REGISTER_HOTEL_DOOR_MODE");
    //                player.emitClientEvent("REGISTER_HOTEL_DOOR_MODE", false, "");
    //                HotelController.sendNotificationToPlayer("Hotel-Tür-Registrier-Modus deaktiviert!",
    //                    "Hoteltür: Registriermodus beendet",
    //                    Constants.NotifactionTypes.Success, player);
    //            }
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }

    //}

    //[CommandAttribute("RegisterHotelRoomDoor")]
    //[HotelCommandAttribute("[HotelRoomID]")]
    //public static void registerHotelRoomDoor(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 1) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. Args: [HotelRoomID]",
    //                    "Hotelzimmertür registrieren fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            if (!player.hasData("REGISTER_HOTEL_ROOM_DOOR_MODE")) {
    //                string hotelRoomId = args[0];

    //                if (int.TryParse(hotelRoomId, out var hotelRoomIdI)) {
    //                    if (HotelController.AllHotelRooms.ContainsKey(hotelRoomIdI)) {
    //                        player.setData("REGISTER_HOTEL_ROOM_DOOR_MODE", true);
    //                        player.emitClientEvent("REGISTER_HOTEL_ROOM_DOOR_MODE", true, hotelRoomId);
    //                        StringBuilder message = new StringBuilder();
    //                        message.AppendLine(
    //                            $"Registrier-Modus für Hotel-Raum aktiviert. Wähle Tür aus und drücke Rechtsklick! Die gewählte Raum-ID ist: {hotelRoomId} ({HotelController.AllHotelRooms[hotelRoomIdI].RoomName})");
    //                        if (HotelController.AllHotelRooms[hotelRoomIdI].DoorId == null &&
    //                            string.IsNullOrEmpty(HotelController.AllHotelRooms[hotelRoomIdI].DoorGroup)) {
    //                            message.AppendLine($"Hinweis: Es ist noch KEINE Tür gesetzt.");
    //                        } else {
    //                            message.AppendLine(
    //                                $"Hinweis: Es ist bereits eine Tür gesetzt! ({HotelController.AllHotelRooms[hotelRoomIdI].DoorId}{HotelController.AllHotelRooms[hotelRoomIdI].DoorGroup}).");
    //                        }

    //                        HotelController.sendNotificationToPlayer(message.ToString(),
    //                            "Hotelzimmertür: Registriermodus gestartet.", Constants.NotifactionTypes.Info,
    //                            player);
    //                    } else {
    //                        HotelController.sendNotificationToPlayer(
    //                            $"Fehler: Für die ID existiert kein Hotel! Die Unbekannte ID war: {hotelRoomId}",
    //                            "Hotelzimmertür registrieren fehlgeschlagen.",
    //                            Constants.NotifactionTypes.Warning, player);
    //                    }
    //                }
    //            } else {
    //                player.resetData("REGISTER_HOTEL_ROOM_DOOR_MODE");
    //                player.emitClientEvent("REGISTER_HOTEL_ROOM_DOOR_MODE", false, "");
    //                HotelController.sendNotificationToPlayer("Registrier-Modus für Hotel-Raum deaktiviert!",
    //                    "Hotelzimmertür: Registriermodus beendet.",
    //                    Constants.NotifactionTypes.Success, player);
    //            }
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }

    //}

    //[CommandAttribute("RegisterHotelRoom")]
    //[HotelCommandAttribute("[RoomName, Replace Space with Underline!] [RoomType] [RoomClass] [HotelId]")]
    //public static void registerHotelRoom(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 4) {
    //                HotelController.sendNotificationToPlayer(
    //                    "Anzahl an Argumenten Falsch. Args: [RoomName, Replace Space with Underline!] [RoomType] [RoomClass] [HotelId]",
    //                    "Hotelzimmer registrieren fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            string roomName = args[0].Replace('_', ' ');

    //            string roomTypeT = args[1];
    //            string roomClassT = args[2];
    //            string hotelIdT = args[3];


    //            //Convert to RoomType
    //            if (!Enum.TryParse<RoomTypes>(roomTypeT, out var roomType)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [RoomType] falsch. Wert {roomTypeT} ist unbekannt. Args: [RoomName, Replace Space with Underline!] [RoomType] [RoomClass] [HotelId]",
    //                    "Hotelzimmer registrieren fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            //Convert to RoomClass
    //            if (!Enum.TryParse<RoomClasses>(roomClassT, out var roomClass)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [RoomClass] falsch. Wert {roomTypeT} ist unbekannt. Args: [RoomName, Replace Space with Underline!] [RoomType] [RoomClass] [HotelId]",
    //                    "Hotelzimmer registrieren fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            if (!HotelController.tryGetHotelByString(hotelIdT, out var hotelId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [HotelId] falsch. Wert {hotelIdT} konnte nicht aufgelöst werden. Args: [RoomName, Replace Space with Underline!] [RoomType] [RoomClass] [HotelId]",
    //                    "Hotelzimmer registrieren fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            if (!HotelController.AllHotels.ContainsKey(hotelId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [HotelId] falsch. Kein Hotel mit der ID {hotelId} vorhanden. Args: [RoomName, Replace Space with Underline!] [RoomType] [RoomClass] [HotelId]",
    //                    "Hotelzimmer registrieren fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            int roomId = HotelController.registerHotelRoom(roomName, roomType, roomClass, hotelId, player);
    //            if (roomId == -1) {
    //                HotelController.sendNotificationToPlayer("Anlage des Hotelraums fehler!",
    //                    "Hotelzimmer registrieren fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //            } else {
    //                registerHotelRoomDoor(player, new[] { roomId.ToString() });
    //            }
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("RemoveHotelRoom")]
    //[HotelCommandAttribute("[RoomId]")]
    //public static void removeHotelRoom(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            IList<HotelRoomRemoveTicket> cleanUpTickets = HotelRoomRemoveTickets
    //                .Where(t => DateTime.Now - t.TicketTime > TimeSpan.FromSeconds(30)).ToList();
    //            foreach (HotelRoomRemoveTicket hotelRoomRemoveTicket in cleanUpTickets) {
    //                HotelRoomRemoveTickets.Remove(hotelRoomRemoveTicket);
    //            }

    //            cleanUpTickets.Clear();

    //            if (args.Length < 1) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. Args: [RoomId]",
    //                    "Hotelzimmer entfernen fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            if (!int.TryParse(args[0], out var roomId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"RoomId \"${args[0]}\" ist keine Zahl. Args: [RoomId]",
    //                    "Hotelzimmer entfernen fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            if (!HotelController.AllHotelRooms.ContainsKey(roomId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"RoomId \"${roomId}\" ist unbekannt. Args: [RoomId]",
    //                    "Hotelzimmer entfernen fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            if (HotelRoomRemoveTickets.Count(t =>
    //                string.Equals(t.PlayerAuthToken, player.AuthToken, StringComparison.OrdinalIgnoreCase)) > 0) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Fehler: Löschticket bereits vorhanden. Bitte vorhandenes Löschticket bestätigen oder die 30 Sekunden abwarten um ein neues erstellen zu können.",
    //                    "Hotelzimmer entfernen fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            HotelRoomRemoveTickets.Add(new HotelRoomRemoveTicket() { RoomId = roomId, PlayerAuthToken = player.AuthToken, TicketTime = DateTime.Now });
    //            HotelController.sendNotificationToPlayer($"===== WARNUNG ===== " +
    //                                                     $"Hotel \"${HotelController.AllHotelRooms[roomId].RoomName} (#{roomId})\" wurde zur Löschung vorgemerkt! " +
    //                                                     $"Es werden ALLE Raumdaten gelöscht und der Raum wird sofort geräumt! " +
    //                                                     $"Gebe in den nächsten 30 Sekunden \"/DoRemoveHotelRoom\" zur Bestätigung ein. " +
    //                                                     $"===== WARNUNG =====",
    //                "Hotelzimmer zur Löschung vorgemerkt.", Constants.NotifactionTypes.Warning, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("DoRemoveHotelRoom")]
    //[HotelCommandAttribute("None")]
    //public static void doRemoveHotelRoom(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode && player.getAdminLevel() >= 2) {
    //            var target = ChoiceVAPI.GetAllPlayers()
    //                .FirstOrDefault(p => p.getCharacterId() != player.getCharacterId());
    //            ChoiceVAPI.emitClientEventToAll("STOP_PLAYER_DRAGGING", player, target);
    //            IList<HotelRoomRemoveTicket> cleanUpTickets = HotelRoomRemoveTickets
    //                .Where(t => DateTime.Now - t.TicketTime > TimeSpan.FromSeconds(30)).ToList();
    //            foreach (HotelRoomRemoveTicket hotelRoomRemoveTicket in cleanUpTickets) {
    //                HotelRoomRemoveTickets.Remove(hotelRoomRemoveTicket);
    //            }

    //            cleanUpTickets.Clear();

    //            HotelRoomRemoveTicket removeTicket = HotelRoomRemoveTickets.FirstOrDefault(t =>
    //                string.Equals(t.PlayerAuthToken, player.AuthToken, StringComparison.OrdinalIgnoreCase));

    //            if (removeTicket == null) {
    //                HotelController.sendNotificationToPlayer($"Fehler: Kein Löschticket vorhanden.",
    //                    "Hotelzimmer entfernen fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning, player);
    //            } else {
    //                HotelController.removeHotelRoom(removeTicket.RoomId, player);
    //                HotelRoomRemoveTickets.Remove(removeTicket);
    //            }
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("startRegisterHotelRooms")]
    //public static void startRegisterHotelRooms(IPlayer player, string[] args) {
    //    if (player.getCharacterData().AdminMode) {
    //        int hotelId = int.Parse(args[0]);
    //        int roomType = int.Parse(args[1]);
    //        int roomClass = int.Parse(args[2]);
    //        if (InteractionController.hotelRegPlayerToHotel.ContainsKey(player)) {
    //            InteractionController.hotelRegPlayerToHotel.Remove(player);
    //        }
    //        InteractionController.hotelRegPlayerToHotel.Add(player, new int[] { hotelId, roomType, roomClass });
    //        player.sendNotification(NotifactionTypes.Success, $"Hotel-Raum-Registrier-Modus gestartet für Hotel mit ID #{hotelId}.", "Hotel-Reg-Mode");
    //    }
    //}

    //[CommandAttribute("stopRegisterHotelRooms")]
    //public static void stopRegisterHotelRooms(IPlayer player, string[] args) {
    //    if (player.getCharacterData().AdminMode) {
    //        if (InteractionController.hotelRegPlayerToHotel.ContainsKey(player)) {
    //            InteractionController.hotelRegPlayerToHotel.Remove(player);
    //            player.sendNotification(NotifactionTypes.Success, $"Hotel-Raum-Registrier-Modus abgeschaltet.", "Hotel-Reg-Mode");
    //            return;
    //        }
    //        player.sendNotification(NotifactionTypes.Warning, $"Hotel-Registrier-Modus war nie gestartet.", "Hotel-Reg-Mode");
    //    }
    //}

    /*[CommandAttribute("RemoveHotel")]
    [HotelCommandAttribute("[HotelId]")]
    public static void removeHotel(IPlayer player, string[] args) {
        try {
            if (player.getCharacterData().AdminMode && player.getAdminLevel() >= 2) {
                var id = int.Parse(args[0]);
                var settings = new companysettings {
                    companyId = id,
                    settingsName = args[1],
                    settingsValue = args[2],
                };
                IList<HotelRemoveTicket> cleanUpTickets = HotelRemoveTickets
                    .Where(t => DateTime.Now - t.TicketTime > TimeSpan.FromSeconds(30)).ToList();
                foreach (HotelRemoveTicket hotelRemoveTicket in cleanUpTickets) {
                    HotelRemoveTickets.Remove(hotelRemoveTicket);
                }

                cleanUpTickets.Clear();

                using (var db = new ChoiceVDb()) {
                    db.companysettings.Add(settings);
                    if (args.Length < 1) {
                        HotelController.sendNotificationToPlayer(
                            "Anzahl an Argumenten Falsch. " + "Args: [HotelId]",
                            "Hotel entfernen fehlgeschlagen.", Constants.NotifactionTypes.Warning, player, true);
                        return;
                    }

                    db.SaveChanges();
                    if (!HotelController.tryGetHotelByString(args[0].Replace('_', ' '), out var hotelId)) {
                        HotelController.sendNotificationToPlayer(
                            $"HotelId \"${args[0]}\" konnte nicht aufgelöst werden. " + "Args: [HotelId]",
                            "Hotel entfernen fehlgeschlagen.", Constants.NotifactionTypes.Warning, player, true);
                        return;
                    }

                    if (!HotelController.AllHotels.ContainsKey(hotelId)) {
                        HotelController.sendNotificationToPlayer(
                            $"HotelId \"${hotelId}\" ist unbekannt. " + "Args: [HotelId]",
                            "Hotel entfernen fehlgeschlagen.", Constants.NotifactionTypes.Warning, player, true);
                        return;
                    }

                    if (HotelRemoveTickets.Count(t =>
                        string.Equals(t.PlayerAuthToken, player.AuthToken,
                            StringComparison.OrdinalIgnoreCase)) > 0) {
                        HotelController.sendNotificationToPlayer(
                            "Fehler: Löschticket bereits vorhanden. Bitte vorhandenes Löschticket bestätigen oder die 30 Sekunden abwarten um ein neues erstellen zu können.",
                            "Hotel entfernen fehlgeschlagen.", Constants.NotifactionTypes.Warning, player, true);
                        return;
                    }

                    HotelRemoveTickets.Add(new HotelRemoveTicket() { HotelId = hotelId, PlayerAuthToken = player.AuthToken, TicketTime = DateTime.Now });
                    HotelController.sendNotificationToPlayer(
                        $"===== WARNUNG ===== Hotel \"${HotelController.AllHotels[hotelId].Name}\" wurde zur Löschung vorgemerkt! Es werden ALLE Hoteldaten gelöscht, ALLE Hotelräume de-registriert, ALLE Hotelräume sofort geräumt! Gebe in den nächsten 30 Sekunden \"/DoRemoveHotel\" zur Bestätigung ein. ===== WARNUNG =====",
                        "Hotel zur Löschung vorgemerkt.", Constants.NotifactionTypes.Warning, player, true);
                }
            }
        } catch (Exception e) {
            Logger.logException(e);
        }

    } */

    //[CommandAttribute("DoRemoveHotel")]
    //[HotelCommandAttribute("None")]
    //public static void doRemoveHotel(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode && player.getAdminLevel() >= 2) {
    //            AnnouncementController.onCollisionShapeInteract(player);
    //            IList<HotelRemoveTicket> cleanUpTickets = HotelRemoveTickets
    //                .Where(t => DateTime.Now - t.TicketTime > TimeSpan.FromSeconds(30)).ToList();
    //            foreach (HotelRemoveTicket hotelRemoveTicket in cleanUpTickets) {
    //                HotelRemoveTickets.Remove(hotelRemoveTicket);
    //            }

    //            cleanUpTickets.Clear();

    //            HotelRemoveTicket removeTicket = HotelRemoveTickets.FirstOrDefault(t =>
    //                string.Equals(t.PlayerAuthToken, player.AuthToken, StringComparison.OrdinalIgnoreCase));

    //            if (removeTicket == null) {
    //                HotelController.sendNotificationToPlayer($"Fehler: Kein Löschticket vorhanden.",
    //                    "Hotel entfernen fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning, player);
    //            } else {
    //                HotelController.removeHotel(removeTicket.HotelId, player);
    //                HotelRemoveTickets.Remove(removeTicket);
    //            }
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("EvictHotelRoom")]
    //[HotelCommandAttribute("[RoomID] [CharId] [Reason, Replace Space with Underline!, Optional]")]
    //public static void evictHotelRoom(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 2) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. " +
    //                                                         "Args: [RoomID] [CharId] [Reason, Replace Space with Underline!, Optional]",
    //                    "Hotelzimmer räumen fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            string roomIdT = args[0];
    //            string characterIdT = args[1];
    //            string reason = null;

    //            if (args.Length > 2) {
    //                reason = args[2];
    //            }

    //            if (!int.TryParse(roomIdT, out var roomId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [RoomID] falsch. Wert {roomIdT} ist keine Zahl. " +
    //                    "Args: [RoomID] [CharId] [Reason, Replace Space with Underline!, Optional]",
    //                    "Hotelzimmer räumen fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            if (!int.TryParse(characterIdT, out var characterId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [CharId] falsch. Wert {characterIdT} ist keine Zahl. " +
    //                    "Args: [RoomID] [CharId] [Reason, Replace Space with Underline!, Optional]",
    //                    "Hotelzimmer räumen fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            HotelController.evictHotelRoom(roomId, characterId, reason, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }

    //}

    //[CommandAttribute("GetHotelIdByName")]
    //[HotelCommandAttribute("[HotelName, Replace Space with Underline!]")]
    //public static void getHotelIdByName(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 1) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. " +
    //                                                         "Args: [HotelName, Replace Space with Underline!]",
    //                    "Hotel-ID zu Name ermitteln fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            string hotelName = args[0].ToString().Replace('_', ' ');

    //            Hotel hotel = HotelController.AllHotels.FirstOrDefault(h => string.Equals(h.Value.Name, hotelName))
    //                .Value;

    //            if (hotel == null) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Das Hotel \"{hotelName}\" ist unbekannt. Prüfe bitte die Schreibweise. " +
    //                    "Args: [HotelName, Replace Space with Underline!]", "Hotel-Name unbekannt.",
    //                    Constants.NotifactionTypes.Warning, player);
    //            } else {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Das Hotel \"{hotel.Name}\" hat die Id \"{hotel.Id}\".", $"Hotel-ID ist {hotel.Id}",
    //                    Constants.NotifactionTypes.Info, player);
    //            }
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("GetRoomIDByName")]
    //[HotelCommandAttribute("[HotelRoomName, Replace Space with Underline!]")]
    //public static void getRoomIdByName(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 1) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. " +
    //                                                         "Args: [HotelRoomName, Replace Space with Underline!]",
    //                    "Raum-ID zu Name ermitteln fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            string hotelRoomName = args[0].Replace('_', ' ');

    //            var roomList = HotelController.AllHotelRooms
    //                .Where(h => string.Equals(h.Value.RoomName, hotelRoomName)).Select(h => h.Value).ToList();

    //            if (!roomList.Any()) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Der Raum \"{hotelRoomName}\" ist unbekannt. Prüfe bitte die Schreibweise. " +
    //                    "Args: [HotelRoomName, Replace Space with Underline!]",
    //                    "Raum-ID zu Name ermitteln fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //            } else {
    //                StringBuilder message = new StringBuilder();
    //                message.AppendLine(
    //                    $"Der Raum mit dem Namen \"{hotelRoomName}\" wurde {roomList.Count()}x gefunden.");
    //                foreach (var room in roomList) {
    //                    //types.FirstOrDefault(x => x.Value == "one").Key

    //                    if (HotelController.AllHotelAssignments.Any(ha => ha.Room == room.Id)) {
    //                        Assignment first = null;
    //                        foreach (var ha in HotelController.AllHotelAssignments) {
    //                            if (room != null && ha.Room == room.Id) {
    //                                first = ha;
    //                                break;
    //                            }
    //                        }

    //                        if (first != null) {
    //                            int hotelId = first.Hotel;

    //                            message.AppendLine(
    //                                $"Hotel: {HotelController.AllHotels[hotelId].Name} / #{hotelId}; RaumId: {room.Id}/");
    //                        }
    //                    } else {
    //                        message.AppendLine($"[Keinem Hotel zugeordneter Raum!]; RaumId: /");
    //                    }
    //                }

    //                HotelController.sendNotificationToPlayer(message.ToString(), "Raum wurde gefunden.",
    //                    Constants.NotifactionTypes.Info,
    //                    player);
    //            }
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("SetRoomRate")]
    //[HotelCommandAttribute("[HotelId] [RoomType] [RoomClass] [Rate] [RateTickInDays, Optional]")]
    //public static void setRoomRate(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 5) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. " +
    //                                                         "Args: [HotelId] [RoomType] [RoomClass] [Rate] [RateTickInDays, Optional]",
    //                    "Setzen von Zimmerpreis fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            string hotelIdT = args[0];
    //            string roomTypeT = args[1];
    //            string roomClassT = args[2];
    //            string roomRateT = args[3];
    //            string rateTickT = args[4];

    //            if (!HotelController.tryGetHotelByString(hotelIdT, out var hotelId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [HotelId] falsch. Wert {hotelIdT} konnte nicht aufgelöst werden. " +
    //                    "Args: [HotelId] [RoomType] [RoomClass] [Rate] [RateTickInDays, Optional]",
    //                    "Setzen von Zimmerpreis fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            if (!HotelController.AllHotels.ContainsKey(hotelId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [HotelId] falsch. Es ist kein Hotel mit der ID #{hotelId} vorhanden. " +
    //                    "Args: [HotelId] [RoomType] [RoomClass] [Rate] [RateTickInDays, Optional]",
    //                    "Setzen von Zimmerpreis fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            //Convert to RoomType
    //            if (!Enum.TryParse<RoomTypes>(roomTypeT, out var roomType)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [RoomType] falsch. Wert {roomTypeT} ist unbekannt. " +
    //                    "Args: [HotelId] [RoomType] [RoomClass] [Rate] [RateTickInDays, Optional]",
    //                    "Setzen von Zimmerpreis fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            //Convert to RoomClass
    //            if (!Enum.TryParse<RoomClasses>(roomClassT, out var roomClass)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [RoomClass] falsch. Wert {roomClassT} ist unbekannt. " +
    //                    "Args: [HotelId] [RoomType] [RoomClass] [Rate] [RateTickInDays, Optional]",
    //                    "Setzen von Zimmerpreis fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            if (!decimal.TryParse(roomRateT, out var roomRate)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [RoomRate] falsch. Wert {roomRateT} ist keine Zahl. " +
    //                    "Args: [HotelId] [RoomType] [RoomClass] [Rate] [RateTickInDays, Optional]",
    //                    "Setzen von Zimmerpreis fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            if (!int.TryParse(rateTickT, out var rateTick)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [RateTickInDays] falsch. Wert {rateTickT} ist keine Zahl. " +
    //                    "Args: [HotelId] [RoomType] [RoomClass] [Rate] [RateTickInDays, Optional]",
    //                    "Setzen von Zimmerpreis fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            HotelController.setRoomRate(hotelId, roomType, roomClass, roomRate, rateTick, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("AssignTerminalToHotel")]
    //[HotelCommandAttribute("[HotelId]")]
    //public static void assignTerminalToHotel(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 1) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. " +
    //                                                         "Args: [HotelId]",
    //                    "Zuordnung Terminal fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            string hotelId = args[0].Replace('_', ' ');

    //            if (!HotelController.tryGetHotelByString(hotelId, out var hotelIdI)) {
    //                HotelController.sendNotificationToPlayer($"Argument [HotelId] falsch. Wert {hotelId} konnte nicht aufgelöst werden. " +
    //                                                         "Args: [HotelId]", "Zuordnung Terminal fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            HotelController.assignTerminalToHotel(hotelIdI, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("SetHotelOwner")]
    //[HotelCommandAttribute("[HotelId] [CharId]")]
    //public static void setHotelOwner(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 2) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. " +
    //                                                         "Args: [HotelId] [CharId]",
    //                    "Zuordnung Hotelinhaber fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            string hotelId = args[0].Replace('_', ' ');

    //            if (!HotelController.tryGetHotelByString(hotelId, out var hotelIdI)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [HotelId] falsch. Wert {hotelId} konnte nicht aufgelöst werden. " +
    //                    "Args: [HotelId] [CharId]", "Zuordnung Hotelinhaber fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            string charId = args[1];

    //            if (!int.TryParse(charId, out var charIdI)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [CharId] falsch. Wert {charId} ist keine Zahl. " +
    //                    "Args: [HotelId] [CharId]", "Zuordnung Hotelinhaber fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            HotelController.setHotelOwner(hotelIdI, charIdI, player);
    //        }

    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }


    //}

    //[CommandAttribute("SetHotelBankAccount")]
    //[HotelCommandAttribute("[HotelId] [BankAccountId]")]
    //public static void setHotelBankAccount(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 2) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. " +
    //                                                         "Args: [HotelId] [BankAccountId]",
    //                    "Zuordnung des Bankkontos fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            string hotelId = args[0].Replace('_', ' ');

    //            if (!HotelController.tryGetHotelByString(hotelId, out var hotelIdI)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [HotelId] falsch. Wert {hotelId} konnte nicht aufgelöst werden. " +
    //                    "Args: [HotelId] [BankAccountId]", "Zuordnung des Bankkontos fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            string bankAccountIdT = args[1];

    //            if (!long.TryParse(bankAccountIdT, out var bankAccountId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [BankAccountId] falsch. Wert {bankAccountIdT} ist keine Zahl. " +
    //                    "Args: [HotelId] [BankAccountId]", "Zuordnung des Bankkontos fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            HotelController.setHotelBankAccountId(hotelIdI, bankAccountId, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("DumpHotelList")]
    //[HotelCommandAttribute("None")]
    //public static void dumpHotelList(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            HotelController.dumpHotelList(player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("DumpRoomListByHotel")]
    //[HotelCommandAttribute("[HotelId]")]
    //public static void dumpRoomListByHotel(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 1) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. " +
    //                                                         "Args: [HotelId]",
    //                    "Raumliste ausgeben fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }


    //            string hotelId = args[0].Replace('_', ' ');

    //            if (!HotelController.tryGetHotelByString(hotelId, out var hotelIdI)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [HotelId] falsch. Wert {hotelId} konnte nicht aufgelöst werden. " +
    //                    "Args: [HotelId]", "Raumliste ausgeben fehlgeschlagen.", Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            HotelController.dumpRoomListByHotel(hotelIdI, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("ClearHotelOwner")]
    //[HotelCommandAttribute("[HotelId]")]
    //public static void clearHotelOwner(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 1) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. " +
    //                                                         "Args: [HotelId]",
    //                    "Löschen des Hotelinhabers fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            string hotelId = args[0].Replace('_', ' ');

    //            if (!HotelController.tryGetHotelByString(hotelId, out var hotelIdI)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [HotelId] falsch. Wert {hotelId} konnte nicht aufgelöst werden. " +
    //                    "Args: [HotelId]", "Löschen des Hotelinhabers fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            HotelController.clearHotelOwner(hotelIdI, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("ListHotelTypes")]
    //[HotelCommandAttribute("None")]
    //public static void listHotelTypes(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            HotelController.dumpEnums(typeof(HotelTypes), player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("ListRoomTypes")]
    //[HotelCommandAttribute("None")]
    //public static void listRoomTypes(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            HotelController.dumpEnums(typeof(RoomTypes), player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("ListRoomClasses")]
    //[HotelCommandAttribute("None")]
    //public static void listRoomClasses(IPlayer player, string[] args) {
    //    try {
    //        HotelController.dumpEnums(typeof(RoomClasses), player);
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("GetHotelRoomGuestByRoomID")]
    //[HotelCommandAttribute("[RoomID]")]
    //public static void getHotelRoomGuestByRoomId(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 1) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. " +
    //                                                         "Args: [RoomID]", "Infoausgabe fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            string roomIdT = args[0];
    //            if (!int.TryParse(roomIdT, out var roomId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [RoomID] falsch. Wert {roomIdT} ist keine Zahl. " +
    //                    "Args: [RoomID]", "Infoausgabe fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            HotelController.getHotelRoomGuestByRoomId(roomId, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("GetHotelRoomListByCharID")]
    //[HotelCommandAttribute("None")]
    //public static void getHotelRoomListByCharId(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 1) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. " +
    //                                                         "Args: [CharId]", "Infoausgabe fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            string charIdT = args[0];
    //            if (!int.TryParse(charIdT, out var charId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [CharId] falsch. Wert {charIdT} ist keine Zahl. " +
    //                    "Args: [CharId]", "Infoausgabe fehlgeschlagen.", Constants.NotifactionTypes.Warning, player);
    //                return;
    //            }

    //            HotelController.getHotelRoomListByCharId(charId, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("ListHotelCommands")]
    //[HotelCommandAttribute("None")]
    //public static void listHotelCommands(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            ensureHotelCommandHelpDict();

    //            StringBuilder commandList = new StringBuilder();

    //            commandList.AppendLine("Liste der Hotelbefehle mit Attributen:");

    //            foreach (KeyValuePair<string, string> hotelCommand in AllHotelCommands) {
    //                commandList.AppendLine($"{hotelCommand.Key} {hotelCommand.Value}");
    //            }

    //            HotelController.sendNotificationToPlayer(commandList.ToString(), "Auflistung der Hotelbefehle.",
    //                Constants.NotifactionTypes.Info, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("GetRoomRates")]
    //[HotelCommandAttribute("[HotelId]")]
    //public static void getRoomRates(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 1) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. Args: [HotelId]",
    //                    "Ausgabe Preisliste fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            if (!HotelController.tryGetHotelByString(args[0].Replace('_', ' '), out var hotelId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [HotelId] falsch. Wert {args[0]} konnte nicht aufgelöst werden. Args: [HotelId]",
    //                    "Ausgabe Preisliste fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            HotelController.getRoomRates(hotelId, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("LockHotel")]
    //[HotelCommandAttribute("[HotelId] [Reason, Replace Space with Underline!]")]
    //public static void lockHotel(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 2) {
    //                HotelController.sendNotificationToPlayer(
    //                    "Anzahl an Argumenten Falsch. Args: [HotelId] [Reason]",
    //                    "Ausgabe Preisliste fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            if (!HotelController.tryGetHotelByString(args[0].Replace('_', ' '), out var hotelId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [HotelId] falsch. Wert {args[0]} konnte nicht aufgelöst werden. Args: [HotelId] [Reason]",
    //                    "Ausgabe Preisliste fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            string reason = args[1].Replace('_', ' ');

    //            HotelController.lockHotel(hotelId, reason, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("LockHotelRoom")]
    //[HotelCommandAttribute("[RoomId] [Reason, Replace Space with Underline!]")]
    //public static void lockHotelRoom(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 2) {
    //                HotelController.sendNotificationToPlayer(
    //                    "Anzahl an Argumenten Falsch. Args: [RoomId] [Reason]", "Hotel sperren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            if (!int.TryParse(args[0], out var roomId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [RoomId] falsch. Wert {args[0]} Ist keine Zahl. Args: [RoomId] [Reason]",
    //                    "Hotel sperren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            string reason = args[1].Replace('_', ' ');

    //            HotelController.lockHotelRoom(roomId, reason, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("UnlockHotel")]
    //[HotelCommandAttribute("[HotelId]")]
    //public static void unlockHotel(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 1) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. Args: [HotelId]",
    //                    "Hotel entsperren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            if (!HotelController.tryGetHotelByString(args[0].Replace('_', ' '), out var hotelId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [HotelId] falsch. Wert {args[0]} konnte nicht aufgelöst werden. Args: [HotelId]",
    //                    "Hotel entsperren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            HotelController.unlockHotel(hotelId, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("UnlockHotelRoom")]
    //[HotelCommandAttribute("[RoomId]")]
    //public static void unlockHotelRoom(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 2) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. Args: [RoomId]",
    //                    "Raum entsperren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            if (!int.TryParse(args[0], out var roomId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [RoomId] falsch. Wert {args[0]} Ist keine Zahl. Args: [RoomId]",
    //                    "Raum entsperren fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            HotelController.unlockHotelRoom(roomId, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("CheckHotelIntegrity")]
    //[HotelCommandAttribute("[HotelId]")]
    //public static void checkHotelIntegrity(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 1) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. Args: [HotelId]",
    //                    "Integritätsprüfung fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            if (!HotelController.tryGetHotelByString(args[0].Replace('_', ' '), out var hotelId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [HotelId] falsch. Wert {args[0]} konnte nicht aufgelöst werden. Args: [HotelId]",
    //                    "Integritätsprüfung fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            HotelController.checkHotelIntegrity(hotelId, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("ShowHotelBookingMenu")]
    //[HotelCommandAttribute("[HotelId]")]
    //public static void showHotelBookingMenu(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 1) {
    //                HotelController.sendNotificationToPlayer("Anzahl an Argumenten Falsch. Args: [HotelId]",
    //                    "Buchungsmenüanzeige fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            if (!HotelController.tryGetHotelByString(args[0].Replace('_', ' '), out var hotelId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [HotelId] falsch. Wert {args[0]} konnte nicht aufgelöst werden. Args: [HotelId]",
    //                    "Buchungsmenüanzeige fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            HotelController.showHotelBookingMenu(hotelId, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("CancelBooking")]
    //[HotelCommandAttribute("[CharId] [RoomId, Optional]")]
    //public static void cancelBooking(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            if (args.Length < 2) {
    //                HotelController.sendNotificationToPlayer(
    //                    "Anzahl an Argumenten Falsch. Args: [CharId] [RoomId]",
    //                    "Abbruch der Hotelbuchung fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            if (!int.TryParse(args[0], out var charId)) {
    //                HotelController.sendNotificationToPlayer(
    //                    $"Argument [CharId] falsch. Wert {args[0]} ist keine Zahl. Args: [CharId] [RoomId]",
    //                    "Abbruch der Hotelbuchung fehlgeschlagen.",
    //                    Constants.NotifactionTypes.Warning,
    //                    player);
    //                return;
    //            }

    //            int? roomId = null;
    //            if (args.Length >= 2) {
    //                if (!int.TryParse(args[0], out var roomIdTemp)) {
    //                    HotelController.sendNotificationToPlayer(
    //                        $"Argument [RoomId] falsch. Wert {args[1]} ist keine Zahl. Args: [CharId] [RoomId]",
    //                        "Abbruch der Hotelbuchung fehlgeschlagen.",
    //                        Constants.NotifactionTypes.Warning,
    //                        player);
    //                    return;
    //                }

    //                roomId = roomIdTemp;
    //            }

    //            HotelController.cancelBooking(charId, roomId, player);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    #endregion

    //#region BankCommands

    //[CommandAttribute("createBankAccount")]
    //public static void createBankAccount(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            int bankCompany;
    //            int bankAccountType;
    //            if (args.Length >= 2) {
    //                if (!int.TryParse(args[0], out bankCompany))
    //                    bankCompany = (int)Constants.BankCompanies.FleecaBank;
    //                if (!int.TryParse(args[1], out bankAccountType))
    //                    bankAccountType = (int)Constants.BankAccountType.Girokonto;
    //            } else {
    //                bankCompany = (int)Constants.BankCompanies.FleecaBank;
    //                bankAccountType = (int)Constants.BankAccountType.Girokonto;
    //            }

    //            EventController.onPlayerEventTrigger(player, "CHOICEVNET_CREATABANKACCOUNT",
    //                new string[] { bankCompany.ToString(), bankAccountType.ToString() });
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }

    //}

    //[CommandAttribute("giveMeMoney")]
    //public static void giveMeMoney(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode && player.getAdminLevel() >= 3) {
    //            decimal newbalance = Decimal.One;
    //            decimal amount;
    //            if (args.Any()) {
    //                if (!decimal.TryParse(args[0], out amount)) {
    //                    player.sendNotification(Constants.NotifactionTypes.Danger, "Konnte angabe nicht parsen.",
    //                        "Fehler", Constants.NotifactionImages.Gold);
    //                    return;
    //                }
    //            } else {
    //                amount = new decimal(1000.0);

    //            }

    //            using (var db = new ChoiceVDb()) {
    //                characters characters = db.characters.First(c => c.id == player.getCharacterId());

    //                if (!long.TryParse(characters.bankaccount, out var bankAccountOfPlayer) ||
    //                    bankAccountOfPlayer <= 0) {
    //                    player.sendNotification(Constants.NotifactionTypes.Danger,
    //                        "Du hast kein Bankkonto. Bitte erst eins erstellen.", "Fehler",
    //                        Constants.NotifactionImages.Gold);
    //                    return;
    //                }

    //                bankaccounts bankaccounts = db.bankaccounts.First(b => b.id == bankAccountOfPlayer);
    //                newbalance = bankaccounts.balance += amount;
    //                db.SaveChanges();
    //            }

    //            player.sendNotification(Constants.NotifactionTypes.Success,
    //                $"Du hast ${amount,0:0.00} bekommen. Du hast jetzt ${newbalance,0:0.00} auf dem Konto!",
    //                "Geld ercheatet!", Constants.NotifactionImages.Gold);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }

    //}

    //[CommandAttribute("updateBankingSettings")]
    //public static void updateBankingSettings(IPlayer player, string[] args) {
    //    try {
    //        if (player.getCharacterData().AdminMode) {
    //            BankingSettings.Singleton.updateBankingSettings();
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }

    //}

    //#endregion

    [Command("heal")]
    public static void heal(IPlayer player, string[] args) {
        try {
            var damg = player.getCharacterData().CharacterDamage;
            foreach(var inj in damg.AllInjuries) {
                damg.healInjury(player, inj);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("showCombinationLock")]
    public static void showCombinationLock(IPlayer player, string[] args) {
        try {
            CombinationLockController.requestPlayerCombination(player, string.Join("", new[] { 5, 5, 5, 5, 5 }), onTestCombination);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    private static void onTestCombination(IPlayer player, Dictionary<string, dynamic> data) {
        Logger.logError("FDKLSFJLSDK");
    }
    
    [Command("cleanPed")]
    public static void cleanPed(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("CLEAN_PED");
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("createShop")]
    public static void createShop(IPlayer player, string[] args) {
        try {
            ShopName = args[0];
            ShopType = (ShopTypes)Enum.Parse(typeof(ShopTypes), args[1]);
            if(args.Length >= 3) {
                ShopInventoryId = Convert.ToInt32(args[2]);
            }

            CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, onShopCreator);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    private static void onShopCreator(Position position, float width, float height, float rotation) {
        ShopController.createShop(ShopName, ShopType, position, width, height, rotation, ShopInventoryId);
    }

    [Command("addShopItem")]
    public static void addShopItem(IPlayer player, string[] args) {
        try {
            var itemId = int.Parse(args[0]);
            var type = (ShopTypes)Enum.Parse(typeof(ShopTypes), args[1]);
            var price = Convert.ToDecimal(double.Parse(args[2].Replace('.', ',')));

            ShopController.addShopItem(type, itemId, price);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("startDrag")]
    public static void startDrag(IPlayer player, string[] args) {
        try {
            var target = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.getCharacterId() != player.getCharacterId());
            ChoiceVAPI.emitClientEventToAll("START_PLAYER_DRAGGING", player, target);
        } catch(Exception e) {
            Logger.logException(e);
            Logger.logException(e);
        }
    }

    [Command("stopDrag")]
    public static void stopDrag(IPlayer player, string[] args) {
        try {

        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getSocialSecurityCard")]
    public static void getSocialSecurityCard(IPlayer player, string[] args) {
        try {
            var cf = InventoryController.getConfigItemForType<SocialSecurityCard>();
            player.getInventory().addItem(new SocialSecurityCard(cf, player));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("addCompanySettings")]
    public static void addCompanySettings(IPlayer player, string[] args) {
        try {
            var id = int.Parse(args[0]);
            var settings = new configcompanysetting {
                companyId = id,
                settingsName = args[1],
                settingsValue = args[2]
            };

            using(var db = new ChoiceVDb()) {
                db.configcompanysettings.Add(settings);

                db.SaveChanges();
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    //[CommandAttribute("SpawnDealer")]
    //public static void SpawnDealer(IPlayer player, string[] args) {
    //    try {
    //        DealerController.spawnDealer();
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("DespawnDealer")]
    //public static void DespawnDealer(IPlayer player, string[] args) {
    //    try {
    //        foreach (var pedPos in DealerController.pedList.ToArray()) {
    //            var ped = PedController.AllPeds.FirstOrDefault(x => x == pedPos);
    //            PedController.destroyPed(ped);
    //            DealerController.pedList.Remove(pedPos);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    [Command("WeedTest")]
    public static void WeedTest(IPlayer player, string[] args) {
        try {
            var configItem = InventoryController.getConfigById(75);
            var item = new PlaceableObjectItem(configItem, -1, -1);
            var cf = InventoryController.getConfigById(35);
            var split = cf.additionalInfo.Split("#");
            //var fertilizer = new Fertilizer(cf, float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
            player.getInventory().addItem(item);
            //player.getInventory().addItem(fertilizer); 
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("TestEffect")]
    public static void TestEffect(IPlayer player, string[] args) {
        try {
            player.setTimeCycle("TEST", args[0], float.Parse(args[1]));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    //[Command("getInvoice")]
    //public static void getInvoice(IPlayer player, string[] args) {
    //    try {
    //        var item = new InvoiceFile(InventoryController.getConfigItemForType<InvoiceFile>(), CompanyController.getCompanies(player).First());
    //        player.getInventory().addItem(item);
    //    } catch(Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    [Command("focus")]
    public static void focus(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("FOCUS_ON_CEF");
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setBackground")]
    public static void setBackground(IPlayer player, string[] args) {
        try {
            var item = player.getInventory().getItem<Smartphone>(s => s.Selected);
            item.BackgroundId = int.Parse(args[0]);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("sortTest")]
    public static void sortTest(IPlayer player, string[] args) {
        var list = new List<SortTest>();
        for(var i = 0; i < int.Parse(args[0]); i++) {
            for(var j = 0; j < 10; j++) {
                list.Add(new SortTest { Level = i, Room = j });
            }
        }
        list = list.Shuffle().ToList();

        var watch = Stopwatch.StartNew();
        var newList = list.OrderBy(l => l.Level).ThenBy(l => l.Room).ToList();
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        player.sendNotification(NotifactionTypes.Warning, $"It took: {elapsedMs}Ms", "");
    }


    //[CommandAttribute("RadioKit")]
    //lic static void getRadio(IPlayer player, string[] args) {
    //    var configItem = InventoryController.AllConfigItems.FirstOrDefault(x => x.codeItem == "Radio"); 
    //    var radio = new Radio(configItem);
    //    player.getInventory().addItem(radio);
    //    var configItem2 = InventoryController.AllConfigItems.FirstOrDefault(x => x.codeItem == "RadioHeadPhones"); //TODO: CONSTANTS
    //    var radioHeadPhones = new RadioHeadPhones(configItem2);
    //    player.getInventory().addItem(radioHeadPhones);
    //}

    //[CommandAttribute("testGen")]
    //public static void testGen(IPlayer player, string[] args) {
    //    var configItem = InventoryController.AllConfigItems[73];
    //    var item = InventoryController.createGenericItem(configItem);
    //    player.getInventory().addItem(item);
    //}

    //[CommandAttribute("StartMega")]
    //public static void StartMega(IPlayer player, string[] args) {
    //    player.Emit("MEGAPHONE_START", 50, 1);
    //}

    //[CommandAttribute("StopMega")]
    //public static void StopMega(IPlayer player, string[] args) {
    //    player.Emit("MEGAPHONE_STOP");
    //}

    //[CommandAttribute("CallMenu")]
    //public static void CallMenu(IPlayer player, string[] args) {
    //    VoiceChatController.CallMenu(player);
    //}

    [Command("GetHairHex")]
    public static void GetHairHex(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("GetHairRgb", int.Parse(args[0]));
        } catch {

        }
    }

    [Command("showHairMenu")]
    public static void showHairMenu(IPlayer player, string[] args) {
        try {

        } catch {

        }
    }


    //[CommandAttribute("registerATM")]
    //public static void registerATM(IPlayer player, string[] args) {
    //    if (player.getCharacterData().AdminMode) {
    //        try {
    //            var rot = player.Rotation;
    //            rot.Yaw = (180 - ChoiceVAPI.radiansToDegrees(rot.Yaw));
    //            int company = int.Parse(args[0]);
    //            configatms newAtm = new configatms() { company = company, deposit = 0, lastHeistDate = null, location = args[1].Replace("_", " "), position = player.getCharacterData().LastPosition.ToJson(), rotation = rot.Yaw };
    //            using (var db = new ChoiceVDb()) {
    //                db.Add(newAtm);
    //                db.SaveChanges();
    //            }
    //            var a = new ATM(newAtm.id, newAtm.location, newAtm.position.FromJson(), newAtm.rotation, (Constants.BankCompanies)newAtm.company, Convert.ToDecimal(newAtm.deposit), newAtm.lastHeistDate ?? DateTime.MinValue);
    //            ATMController.AllATMs.Add(a);
    //            player.sendNotification(NotifactionTypes.Success, $"ATM erfolgreich bei {newAtm.location} registriert.", "ATM registriert.", NotifactionImages.ATM);

    //        } catch (Exception e) {
    //            Logger.logException(e);
    //            player.sendNotification(NotifactionTypes.Danger, $"FEHLER! ATM wurde nicht registriert.", "Fehler", NotifactionImages.ATM);
    //        }
    //    }
    //}

    //[CommandAttribute("CreateMask")]
    //public static void CreateMask(IPlayer player, string[] args) {
    //    if (player.getCharacterData().AdminMode) {
    //        try {
    //            var configItem = InventoryController.AllConfigItems.FirstOrDefault(x => x.codeItem == "MaskItem");
    //            var radio = new MaskItem(configItem, int.Parse(args[0]), int.Parse(args[1]));
    //            player.getInventory().addItem(radio);
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //        }
    //    } 
    //}

    [Command("FillClothing")]
    public static void FillClothing(IPlayer player, string[] args) {
        using(var db = new ChoiceVDb()) {
            foreach(var clothing in db.configclothings) {
                if(clothing.componentid == int.Parse(args[0])) {
                    if(clothing.gender == player.getCharacterData().Gender.ToString()) {
                        player.emitClientEvent("Check_Clothing_Textures", int.Parse(args[0]), clothing.drawableid, int.Parse(args[1]), int.Parse(args[2]));
                    }
                }
            }
        }
    }

    //[CommandAttribute("NumberPlateText")]
    //public static void NumberPlateText(IPlayer player, string[] args) {
    //    var vehicle = player.Vehicle;
    //    var vehObj = vehicle.getObject();
    //    vehObj.NumberPlate = args[0];
    //    vehObj.updateVehicleObject();
    //}

    [Command("CreateStaticItem")]
    public static void CreateStaticItem(IPlayer player, string[] args) {
        try {
            var configItem = InventoryController.getConfigById(int.Parse(args[0]));
            var item = new StaticItem(configItem, 1, -1);
            player.getInventory().addItem(item);
            player.sendNotification(NotifactionTypes.Success, $"Item mit der ID {args[0]} erstellt {item.Name}", "");
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("CreateGenericItem")]
    public static void CreateGenericItem(IPlayer player, string[] args) {
        try {
            var configItem = InventoryController.getConfigById(int.Parse(args[0]));
            var item = InventoryController.createGenericItem(configItem);
            player.getInventory().addItem(item);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("CreateGenericStackItem")]
    public static void CreateGenericStackItem(IPlayer player, string[] args) {
        try {
            var configItem = InventoryController.getConfigById(int.Parse(args[0]));
            var item = InventoryController.createGenericStackableItem(configItem, int.Parse(args[1]), -1);
            player.getInventory().addItem(item);
            player.sendNotification(NotifactionTypes.Success, $"Item mit der ID {args[0]} erstellt {item.Name}", "");
        } catch(Exception e) {
            Logger.logException(e);
        }
    }


    [Command("CreatePlaceable")]
    public static void PlaceableNormal(IPlayer player, string[] args) {
        try {
            var configItem = InventoryController.getConfigById(int.Parse(args[0]));
            var item = new PlaceableObjectItem(configItem, -1, -1);
            player.getInventory().addItem(item);
            player.sendNotification(NotifactionTypes.Success, $"Item mit der ID {args[0]} erstellt {item.Name}", "");
        } catch(Exception e) {
            Logger.logException(e);
        }
    }


    [Command("TestGender")]
    public static void TestGender(IPlayer player, string[] args) {
        try {
            player.sendBlockNotification(player.getCharacterData().Gender.ToString(), "");
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("readDamage")]
    public static void readDamage(IPlayer player, string[] args) {
        try {
            damg = ((ChoiceVVehicle)player.Vehicle).DamageData;
            appear = ((ChoiceVVehicle)player.Vehicle).AppearanceData;
            health = ((ChoiceVVehicle)player.Vehicle).HealthData;
            script = ((ChoiceVVehicle)player.Vehicle).ScriptData;
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("writeDamage")]
    public static void writeDamage(IPlayer player, string[] args) {
        try {
            ((ChoiceVVehicle)player.Vehicle).DamageData = damg;
            ((ChoiceVVehicle)player.Vehicle).AppearanceData = appear;
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("copyVehicle")]
    public static void copyVehicle(IPlayer player, string[] args) {
        try {
            var vehicle = VehicleController.createVehicle(ChoiceVAPI.Hash(args[0]), new Position(player.Position.X + 2, player.Position.Y + 2, player.Position.Z + 0.5f), player.Rotation, player.Dimension);
            vehicle.DamageData = damg;
            vehicle.AppearanceData = appear;
            vehicle.HealthData = health;
            //vehicle.ScriptData = script;
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("readData")]
    public static void readData(IPlayer player, string[] args) {
        try {
            app = ((ChoiceVVehicle)player.Vehicle).ScriptData;
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("writeData")]
    public static void writeData(IPlayer player, string[] args) {
        try {
            ((ChoiceVVehicle)player.Vehicle).AppearanceData = app;
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("advanceMj")]
    public static void advanceMj(IPlayer player, string[] args) {
        var colShapes = player.getCurrentCollisionShapes();
        if(colShapes != null) {
            var colshape = player.getCurrentCollisionShapes().FirstOrDefault(c => c.Owner != null && c.Owner is IFertilizeObject);

            if(colshape != null) {
                var obj = colshape.Owner as Marihuana;
                obj.Data["Growth"] = 1;
                obj.onInterval(TimeSpan.Zero);
            }
        }
    }

    [Command("getShapes")]
    public static void getShapes(IPlayer player, string[] args) {
        var colShapes = player.getCurrentCollisionShapes();
    }

    [Command("fixBumper")]
    public static void fixBumper(IPlayer player, string[] args) {
        try {
            var tunings = InventoryController.getConfigItems(c => c.codeItem == typeof(VehicleTuningItem).Name);
            foreach(var cfg in tunings) {
                var item = new VehicleTuningItem(cfg, int.Parse(args[0]), null, null);
                player.getInventory().addItem(item, true);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("createPartItem")]
    public static void createPartItem(IPlayer player, string[] args) {
        try {
            var configItem = InventoryController.getConfigById(int.Parse(args[0]));
            var item = new VehicleRepairItem(configItem, int.Parse(args[1]));
            player.getInventory().addItem(item, true);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("createModKitItem")]
    public static void createModKitItem(IPlayer player, string[] args) {
        try {
            var configItem = InventoryController.getConfigById(96);
            var item = new ModKitItem(configItem, int.Parse(args[0]));
            player.getInventory().addItem(item);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("createTuningItem")]
    public static void createTuningItem(IPlayer player, string[] args) {
        try {
            var itemId = int.Parse(args[0]);
            var vehicleClass = int.Parse(args[1]);

            var configItem = InventoryController.getConfigById(itemId);
            var item = new VehicleTuningItem(configItem, vehicleClass, null, null);

            player.getInventory().addItem(item, true);
        } catch(Exception ex) {
            Logger.logException(ex);
        }
    }

    [Command("saveVehTunDb")]
    public static void saveVehTun(IPlayer player, string[] args) {
        try {
            var veh = (ChoiceVVehicle)player.Vehicle;
            using(var db = new ChoiceVDb()) {
                var dbVeh = db.vehicles
                    .Include(v => v.vehiclestuningbase)
                    .Include(v => v.vehiclestuningmods)
                    .FirstOrDefault(v => v.id == veh.VehicleId);

                veh.updateDbTuning(dbVeh);

                db.SaveChanges();
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("resetVehTun")]
    public static void resetVehTun(IPlayer player, string[] args) {
        try {
            var veh = (ChoiceVVehicle)player.Vehicle;
            veh.setVehicleTuning(new VehicleTuning());
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testGasRefill")]
    public static void testGasRefill(IPlayer player, string[] args) {
        try {
            GasRefuelController.showGasRefuel(player, new GasRefuel(
                GasStationType.GlobeOil,
                50,
                120,
                GasstationSpotType.CarPetrol,
                0.95f,
                true,
                true,
                false
            ), (p, act, acc, fA, r) => {
                p.sendBlockNotification("Oi", "oi");
            });
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testGasstation")]
    public static void testGasstation(IPlayer player, string[] args) {
        try {
            var spot = new GasstationSpot(0, new List<GasstationSpotType> { GasstationSpotType.CarDiesel, GasstationSpotType.CarPetrol }, player.Position, 3.5f, 3.5f, 0);
            var station = new Gasstation(0, "Test", 1, new List<GasstationSpot> { spot }, GasStationType.XeroGas, 3, 1.5f, 1.5f, 5f, new Position(player.Position.X + 6, player.Position.Y + 6, player.Position.Z), 2, 2, 0);
            AllGasstations.Add(station);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("createGasstation")]
    public static void createGasstation(IPlayer player, string[] args) {
        try {
            GasstationCreator.createGasstation(player, args[0].Replace("_", " "), (GasStationType)int.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]), float.Parse(args[4]), float.Parse(args[4]));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("createGasstationSpot")]
    public static void createGasstationSpot(IPlayer player, string[] args) {
        try {
            var list = new List<GasstationSpotType>();
            for(var i = 1; i < args.Length - 1; i++) {
                list.Add((GasstationSpotType)int.Parse(args[i]));
            }
            GasstationCreator.createGasstationSpot(player, int.Parse(args[0]), list);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testTyre")]
    public static void testTyre(IPlayer player, string[] args) {
        try {
            wheelCountRec((ChoiceVVehicle)player.Vehicle, 0, count => {
            });

        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testWindow")]
    public static void testWindow(IPlayer player, string[] args) {
        try {
            windowCountRec((ChoiceVVehicle)player.Vehicle, 0, count => {
            });
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    private static void wheelCountRec(ChoiceVVehicle veh, byte count, Action<int> callback) {
        var first = veh.IsWheelBurst(count);
        veh.SetWheelBurst(count, true);
        InvokeController.AddTimedInvoke("wheelCountRec", i => {
            var second = veh.IsWheelBurst(count);

            //Logger.logError("first: " + first + "  second: " + second);

            if(first == second) {
                callback.Invoke(count);
            } else {
                veh.SetWheelBurst(count, false);
                wheelCountRec(veh, (byte)(count + 1), callback);
            }
        }, TimeSpan.FromMilliseconds(200), false);
    }

    private static void windowCountRec(ChoiceVVehicle veh, byte count, Action<int> callback) {
        var first = veh.IsWindowDamaged(count);
        veh.SetWindowDamaged(count, true);
        InvokeController.AddTimedInvoke("windowCountRec", i => {
            var second = veh.IsWindowDamaged(count);

            //Logger.logError("first: " + first + "  second: " + second);

            if(first == second) {
                callback.Invoke(count);
            } else {
                veh.SetWindowDamaged(count, false);
                windowCountRec(veh, (byte)(count + 1), callback);
            }
        }, TimeSpan.FromMilliseconds(200), false);
    }

    [Command("testColorPicker")]
    public static void testColorPicker(IPlayer player, string[] args) {
        try {
            player.showColorPicker(new ColorPicker((ColorPickerType)int.Parse(args[0]), null, PlayerHairColors[0], PlayerHairColors.ToArray()).withData(null));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("openAllDoors")]
    public static void openAllDoors(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("OPEN_VEHICLE_DOORS");
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setEngineMult")]
    public static void setEngineMult(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("SET_ENGINE_MULT", float.Parse(args[0]));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setEngineAcc")]
    public static void setEngineAcc(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("SET_ENGINE_ACC", ((ChoiceVVehicle)player.Vehicle).DbModel.ModelName, float.Parse(args[0]));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("createAcc")]
    public static void createAcc(IPlayer player, string[] args) {
        try {
            var acc = BankController.createBankAccount(player, BankAccountType.GiroKonto, 1700, BankController.getBankByType(BankCompanies.FleecaBank), false);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("createDepAcc")]
    public static void createDepAcc(IPlayer player, string[] args) {
        try {
            var acc = BankController.createBankAccount(player, BankAccountType.DepositKonto, 20000, BankController.getBankByType(BankCompanies.FleecaBank), false);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("createCompAcc")]
    public static void createCompAcc(IPlayer player, string[] args) {
        try {
            var acc = BankController.createBankAccount(CompanyController.getCompanies(player).First(), 1700, BankController.getBankByType(BankCompanies.FleecaBank), false);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("createContAcc")]
    public static void createContAcc(IPlayer player, string[] args) {
        try {
            var acc = BankController.createBankAccount(typeof(Commands), "Commandskonto", BankAccountType.GiroKonto, 1700, BankController.getBankByType(BankCompanies.FleecaBank), false);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testPed")]
    public static void testPed(IPlayer player, string[] args) {
        try {
            using(var db = new ChoiceVDb()) {
                var ped = PedController.createPed(null, "mp_m_freemode_01", player.Position, 0);

                var style = db.characterstyles.FirstOrDefault(s => s.charId == player.getCharacterId());
                PedController.setPedStyle(ped, style);
                PedController.setPedPlayerClothing(ped, player.getClothing());
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testLogin")]
    public static void testLogin(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("CREATE_CHAR_SELECT_SCREEN");
            using(var db = new ChoiceVDb()) {
                if(stestPed != null) {
                    PedController.destroyPed(stestPed);
                }

                stestPed = PedController.createPed(null, "mp_m_freemode_01", new Position(-765.73f, 79.015f, 55.23f - 1f), ChoiceVAPI.radiansToDegrees(2.573f));

                var style = db.characterstyles.FirstOrDefault(s => s.charId == player.getCharacterId());
                PedController.setPedStyle(stestPed, style);
                PedController.setPedPlayerClothing(stestPed, player.getClothing());
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("cont")]
    public static void cont(IPlayer player, string[] args) {
        try {
            using(var db = new ChoiceVDb()) {
                if(stestPed != null) {
                    PedController.destroyPed(stestPed);
                }
                stestPed = PedController.createPed(null, "mp_m_freemode_01", new Position(-765.73f, 79.015f, 55.23f - 1f), ChoiceVAPI.radiansToDegrees(2.573f));
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("changeDim")]
    public static void changeDim(IPlayer player, string[] args) {
        try {
            player.changeDimension(int.Parse(args[0]));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getCustoms")]
    public static void getCustoms(IPlayer player, string[] args) {
        try {
            var cfg = InventoryController.getConfigById(128);
            player.getInventory().addItem(new USCustomsFile(cfg));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testFish")]
    public static void testFish(IPlayer player, string[] args) {
        try {

            player.emitClientEvent("START_FISHING", 2000, int.Parse(args[0]));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getStuff")]
    public static void getStuff(IPlayer player, string[] args) {
        try {

            var cfg = InventoryController.getConfigById(30);
            player.getInventory().addItem(new ToolItem(cfg, -1, -1));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testMat")]
    public static void testMat(IPlayer player, string[] args) {
        try {
            getPlayerMaterialStandOn(player, (p, m) => {
            });
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testWC")]
    public static void testWC(IPlayer player, string[] args) {
        WorldController.worldControllerTestMode(player);
    }

    [Command("ChangeTestWC")]
    public static void ChangeTestWC(IPlayer player, string[] args) {
        WorldController.changeTestModeRunning();
    }

    [Command("testMyRegion")]
    public static void testMyRegion(IPlayer player, string[] args) {
        try {
            getPlayerRegion(player, (p, m) => {
                //Logger.logDebug(m.ToString());
                //var region = m.ToString();
                var region = WorldController.getRegionDisplayName(m.ToString());
                player.sendNotification(NotifactionTypes.Info, $"Du befindest dich in {region}", "Deine Region");
            });
        } catch(Exception e) {
            Logger.logException(e);
        }
    }
    
    [Command("toggleTaxi")]
    public static void toggleTaxi(IPlayer player, string[] args) {
        try {
            ChoiceVVehicle vehicle = (ChoiceVVehicle)player.Vehicle;
            if (player.Vehicle.hasData("TAXI_IS_TAXI")) {
                if (Convert.ToBoolean(player.Vehicle.getData("TAXI_IS_TAXI"))){
                    TaxiController.removeTaxi(vehicle);
                } else {
                    TaxiController.newTaxi(vehicle);
                }
                
            } else {
                TaxiController.newTaxi(vehicle);
            }

        } catch (Exception e) {
            Logger.logException(e);
        }
    }

    [Command("toggleTaxiJob")]
    public static void toggleTaxiJob(IPlayer player, string[] args) {
        try {
            ChoiceVVehicle vehicle = (ChoiceVVehicle)player.Vehicle;
            if (player.Vehicle.hasData("TAXI_ACTIVTASK_PERMANENT")) {
                if (Convert.ToBoolean(player.Vehicle.getData("TAXI_ACTIVTASK_PERMANENT"))) {
                    TaxiController.stopTaxiTask(player, (ChoiceVVehicle)player.Vehicle);
                } else {
                    TaxiController.startTaxiTask(player, (ChoiceVVehicle)player.Vehicle, true);
                }

            }

        } catch (Exception e) {
            Logger.logException(e);
        }
    }

    [Command("openPropShop")]
    public static void openPropShop(IPlayer player, string[] args) {
        try {
            var menu = ClothingPropShopController.generateClothingPropShop(player, (ClothingPropShops)int.Parse(args[0]), player.getCharacterData().Gender);
            if(menu != null) {
                player.showMenu(menu, false);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testTeleport")]
    public static void testTeleport(IPlayer player, string[] args) {
        try {
            var vehicle = player.Vehicle;
            vehicle.Position = new Position(float.Parse(args[0]), float.Parse(args[1]), vehicle.Position.Z);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setModel")]
    public static void setModel(IPlayer player, string[] args) {
        try {
            player.Model = ChoiceVAPI.Hash(args[0]);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setCayoRoute")]
    public static void setCayoRoute(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("SET_WAYPOINT", IslandController.SanAndreasLeaveX, IslandController.SanAndreasLeaveY);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setSanAndreasRoute")]
    public static void setSanAndreasRoute(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("SET_WAYPOINT", 5000, IslandController.CayoLeaveY);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getNearbyObj")]
    public static void getNearbyObj(IPlayer player, string[] args) {
        getNearbyObject(player, player.Position, 10, new List<uint> { ChoiceVAPI.Hash("prop_dumpster_02a") }, (p, worked, hash, pos, heading) => {
            player.sendNotification(NotifactionTypes.Info, "Info" + hash, "");
        });
    }

    [Command("testShow")]
    public static void testShow(IPlayer player, string[] args) {
        try {
            var veh = ChoiceVAPI.FindNearbyVehicle(player, 10, v => v.LockState == VehicleLockState.Unlocked);
            var list = new List<Position>();
            var min = veh.DbModel.StartPoint.FromJson();
            var max = veh.DbModel.EndPoint.FromJson();
            foreach(var seat in veh.DbModel.configvehiclesseats.OrderBy(s => s.SeatNr)) {
                var seatPos = seat.SeatPos.FromJson();
                var vec2 = ChoiceVAPI.rotatePointInRect(seatPos.X + veh.Position.X, seatPos.Y + veh.Position.Y, veh.Position.X, veh.Position.Y, veh.Rotation.Yaw + float.Parse(args[0]));
                list.Add(new Position(vec2.X, vec2.Y, seatPos.Z + veh.Position.Z));
            }

            player.emitClientEvent("PLAYER_SHOW_SEATS", true, list);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setBeard")]
    public static void setBeard(IPlayer player, string[] args) {
        try {
            var style = player.getCharacterData().Style.ToJson().FromJson<characterstyle>();
            style.overlay_1 = int.Parse(args[0]);
            player.setStyle(style);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getHuntingRifle")]
    public static void getHuntingRifle(IPlayer player, string[] args) {
        try {
            player.GiveWeapon(WeaponModel.MarksmanRifle, 100, true);
            player.SetWeaponTintIndex(WeaponModel.MarksmanRifle, 1);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getAccMenu")]
    public static void getAccMenu(IPlayer player, string[] args) {
        try {
            var menu = ClothingPropShopController.generateClothingPropShop(player, ClothingPropShops.SunglassesShop, player.getCharacterData().Gender);
            player.showMenu(menu);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getAccMenu2")]
    public static void getAccMenu2(IPlayer player, string[] args) {
        try {
            var menu = ClothingPropShopController.generateClothingPropShop(player, ClothingPropShops.HatsShop, player.getCharacterData().Gender);
            player.showMenu(menu);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("fixClothingNameVariations")]
    public static void fixClothingNameVariations(IPlayer player, string[] args) {
        try {
            using(var db = new ChoiceVDb()) {
                foreach(var group in db.configclothings.ToList().GroupBy(c => c.name + c.gender + c.componentid).ToList()) {
                    var l = group.ToList();
                    if(l.Count <= 1) {
                        l.First().nameVariation = 0;
                    } else {
                        for(var i = 0; i < l.Count; i++) {
                            l[i].nameVariation = i + 1;
                        }
                    }
                }
                db.SaveChanges();
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getClothMenu")]
    public static void getClothMenu(IPlayer player, string[] args) {
        try {
            var menu = ClothingShopController.getSingleComponentClothShopMenu(player, (ClothingShopTypes)int.Parse(args[0]), (ClothingType)int.Parse(args[1]), true);
            player.showMenu(menu);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getTattooMenu")]
    public static void getTattooMenu(IPlayer player, string[] args) {
        try {
            var menu = TattooController.getTattooMenu(player, player, false);
            player.showMenu(menu);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getTattooMenuNpc")]
    public static void getTattooMenuNpc(IPlayer player, string[] args) {
        try {
            var menu = TattooController.getTattooMenu(player, player, true);
            player.showMenu(menu);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testFront")]
    public static void testFront(IPlayer player, string[] args) {
        try {
            getPositionInFront(player, float.Parse(args[0]), (p, pos) => {
                ObjectController.createObject("prop_food_cb_donuts", pos, new Rotation(0, 0, 0), 200, false);
            });
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testFront2")]
    public static void testFront2(IPlayer player, string[] args) {
        try {
            player.getCameraForwardVector((p, forwardVec) => {
                ObjectController.createObject("prop_food_cb_donuts", new Position(player.Position.X + forwardVec.X * 2, player.Position.Y + forwardVec.Y * 2, player.Position.Z), new Rotation(0, 0, 0), 200, false);
            });
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setColor")]
    public static void setColor(IPlayer player, string[] args) {
        var vehicle = ChoiceVAPI.FindNearbyVehicle(player);
        var coloring = vehicle.VehicleColoring;
        coloring.setColor(byte.Parse(args[0]), true);
        vehicle.setVehicleColoring(coloring);
        vehicle.refreshVehicle();
    }

    [Command("addGtaColor")]
    public static void addGtaColor(IPlayer player, string[] args) {
        try {
            var gtaId = int.Parse(args[0]);
            var type = args[1].Replace('_', ' ');
            var name = args[2].Replace('_', ' ');

            var newColor = new configvehiclecolor {
                gtaId = gtaId,
                name = name,
                type = type
            };

            using(var db = new ChoiceVDb()) {
                var already = db.configvehiclecolors.FirstOrDefault(c => c.gtaId == gtaId);
                if(already == null) {
                    db.configvehiclecolors.Add(newColor);
                    db.SaveChanges();
                    player.sendNotification(NotifactionTypes.Success, $"{type} {name} mit GtaId {gtaId} erfolgreich hinzugefügt", "");
                    VehicleColoringController.loadColors();
                } else {
                    player.sendNotification(NotifactionTypes.Warning, "Diese Farbe war schon registriert!", "");
                }
            }
        } catch(Exception) {
            player.sendBlockNotification("Etwas ist schiefgelaufen! Versuche es erneut", "");
        }
    }

    [Command("showRots")]
    public static void showRots(IPlayer player, string[] args) {
        getPlayerCameraHeading(player, (p, heading) => {
            //Logger.logError("Player: " + ChoiceVAPI.radiansToDegrees(player.Rotation.Yaw));
            //Logger.logError("Cam: " + heading);
        });
    }

    [Command("testVehicleInfo")]
    public static void testVehicleInfo(IPlayer player, string[] args) {
        try {
            getVehicleInfo(player, (ChoiceVVehicle)player.Vehicle, (p, ve, i, min, max, _, i2, seats, windows, doors, tyres, modelName) => {

            });
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("tpToWaypoint")]
    public static void tpToWaypoint(IPlayer player, string[] args) {
        EventController.addEvent("TP_TO_WAYPOINT", doTpToWaypoint);
        player.emitClientEvent("TP_TO_WAYPOINT", player);
    }
    private static bool doTpToWaypoint(IPlayer player, string eventName, object[] args) {
        player.Position = (Position)args[0];

        return true;
    }

    [Command("getVehicleMods")]
    public static void getVehicleMods(IPlayer player, string[] args) {
        try {
            var modelNames = args.Select(arg => arg.ToLower()).ToList();
            using(var db = new ChoiceVDb()) {
                var vehicles = modelNames.Count > 0
                    ? db.configvehiclesmodels.Where(vm => modelNames.Contains(vm.ModelName.ToLower())).ToList()
                    : db.configvehiclesmodels.Where(cvm => cvm.needsRecheck == 1).ToList();
                doGetVehicleMods(player, player.Position, 0, vehicles);
            }
        } catch(Exception ex) {
            Logger.logException(ex);
        }
    }

    private static void doGetVehicleMods(IPlayer player, Position position, int index, IReadOnlyList<configvehiclesmodel> vehicles) {
        try {
            //Will never be happening, but intellisense is happy with it because of possible NullRef.
            if(vehicles.Count == 0) {
                return;
            }

            //Get all ModTypes from DB.
            List<configvehiclemodtype> dbModTypes;
            using(var db = new ChoiceVDb()) {
                dbModTypes = db.configvehiclemodtypes.ToList();
            }

            //Filter DB ModTypes to only get the universal ones and cast them to byte.
            var universalModTypeIds = dbModTypes.Where(x => x.IsUniversal).Select(x => (byte)x.ModTypeIndex).ToList();

            //Get all ModTypes from AltV and exclude the universal ones.
            var modTypes = ((byte[])Enum.GetValues(typeof(VehicleModType))).Except(universalModTypeIds).ToList();

            //Create a new ChoiceV vehicle.
            var vehicle = vehicles[index];
            var veh = (ChoiceVVehicle)ChoiceVAPI.CreateVehicle((uint)vehicle.Model, position, Rotation.Zero);
            //var veh = new ChoiceVVehicle((uint)vehicle.Model, position, Rotation.Zero);

            //Since we're adding and removing vehicles a delay is needed so it can be synced properly.
            InvokeController.AddTimedInvoke("getVehicleMods", invoke => {
                var vehicleModKitList = new List<VehicleModKit>();

                //We start always with ModKit 1, since 0 means not moddable.
                for(byte modKitIndex = 1; modKitIndex <= veh.ModKitsCount; modKitIndex++) {
                    veh.ModKit = modKitIndex;

                    //We go through all non-universal ModTypes and get the amount of available mods for the given ModType index.
                    for(byte modTypeIndex = 0; modTypeIndex < modTypes.Count; modTypeIndex++) {
                        var modType = modTypes[modTypeIndex];

                        vehicleModKitList.Add(new VehicleModKit {
                            ModKit = modKitIndex,
                            ModTypeIndex = modType,
                            ModsCount = veh.GetModsCount(modType)
                        });
                    }
                }

                //To reduce the request and response data size we're removing everything that has no available mods.
                vehicleModKitList = vehicleModKitList.Where(v => v.ModsCount > 0).ToList();

                //Since the client is getting the needed data for us we need to work with callbacks, because of event emitting.
                CallbackController.getVehicleMods(player, veh, vehicleModKitList.ToJson(), (p, v, json) => {
                    var vehicleModKits = json.FromJson<VehicleModKit[]>().ToList();
                    //Index -1 is always the default variation for the current ModType.
                    foreach(var vehicleModKitMod in from vehicleModKit in vehicleModKits from vehicleModKitMod in vehicleModKit.Mods let displayName = vehicleModKitMod.ModDisplayName where string.IsNullOrWhiteSpace(displayName) || displayName.ToLower() == "null" select vehicleModKitMod) {
                        vehicleModKitMod.ModDisplayName = vehicleModKitMod.ModIndex == -1 ? "Serie" : null;
                    }

                    using(var db = new ChoiceVDb()) {
                        //Getting the VehicleModel for the current vehicle. Another null-check only for intellisense and it's useless NullRef warnings. 
                        var vehicleModel = db.configvehiclesmodels.Find(vehicle.id);
                        if(vehicleModel is null) {
                            veh.Destroy();

                            if(index < vehicles.Count - 1) {
                                doGetVehicleMods(player, position, index + 1, vehicles);
                            }

                            return;
                        }

                        foreach(var vehicleModKit in vehicleModKits) {
                            var genericNameCounter = 1;
                            foreach(var vehicleModKitMod in vehicleModKit.Mods) {
                                //Getting the vehicle ModType for the current data set.
                                var modType = db.configvehiclemodtypes.FirstOrDefault(modType => modType.ModTypeIndex == vehicleModKit.ModTypeIndex);
                                if(modType is null) {
                                    continue;
                                }

                                if(vehicleModKitMod.ModDisplayName == null) {
                                    vehicleModKitMod.ModDisplayName = $"Tuning-{modType.DisplayName} {genericNameCounter}";
                                    genericNameCounter++;
                                }

                                //Try to get an existing Mod from db by comparing VehicleModel, ModTypeIndex and ModIndex.
                                var existingMod = db.configvehiclemods.FirstOrDefault(vehicleMod =>
                                    vehicleMod.configvehiclemodels_id == vehicleModel.id
                                    && vehicleMod.configvehiclemodtypes_id == modType.id
                                    && vehicleMod.ModIndex == vehicleModKitMod.ModIndex);

                                //Depending on if there is an existing mod we're inserting a new one or we only update the DisplayName.
                                if(existingMod is null) {
                                    var configvehiclemod = new configvehiclemod {
                                        configvehiclemodels_id = vehicleModel.id,
                                        configvehiclemodtypes_id = modType.id,
                                        ModKit = vehicleModKit.ModKit,
                                        ModIndex = vehicleModKitMod.ModIndex,
                                        DisplayName = vehicleModKitMod.ModDisplayName
                                    };

                                    db.configvehiclemods.Add(configvehiclemod);
                                } else {
                                    existingMod.DisplayName = vehicleModKitMod.ModDisplayName;
                                }
                            }
                        }

                        //Marking the VehicleModel as "done".
                        vehicleModel.needsRecheck = 0;

                        db.SaveChanges();
                    }

                    veh.Destroy();

                    if(index < vehicles.Count - 1) {
                        doGetVehicleMods(player, position, index + 1, vehicles);
                    }
                });
            }, TimeSpan.FromSeconds(3), false);
        } catch(Exception ex) {
            Logger.logException(ex);

            if(index < vehicles.Count - 1) {
                doGetVehicleMods(player, position, index + 1, vehicles);
            }
        }
    }

    [Command("updateDbVehicles")]
    public static void updateDbVehicles(IPlayer player, string[] args) {
        try {
            using(var db = new ChoiceVDb()) {
                var vehicleList = db.configvehiclesmodels.Where(m => m.needsRecheck == 1).ToList(); 
                updateVehicleInfo(player, player.Position, 0, vehicleList);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    private static void updateVehicleInfo(IPlayer player, Position position, int index, List<configvehiclesmodel> list) {
        var el = list[index];
        try {
            var veh = (ChoiceVVehicle)ChoiceVAPI.CreateVehicle((uint)el.Model, position, Rotation.Zero);
            if(veh != null) {
                InvokeController.AddTimedInvoke("TEST", i => {
                    VehicleController.initModelCreation(player, veh);
                    InvokeController.AddTimedInvoke("TEST", i => {
                        veh.Destroy();

                        if(index != list.Count - 1) {
                            InvokeController.AddTimedInvoke("TEST", i => {
                                updateVehicleInfo(player, position, index + 1, list);
                            }, TimeSpan.FromSeconds(1), false);
                        }
                    }, TimeSpan.FromSeconds(2), false);
                }, TimeSpan.FromSeconds(1), false);
            } else {
                if(index != list.Count - 1) {
                    updateVehicleInfo(player, position, index + 1, list);
                }
            }
        } catch(Exception ex) {
            Logger.logException(ex);

            if(index != list.Count - 1) {
                updateVehicleInfo(player, position, index + 1, list);
            }
        }
    }

    [Command("setPartDamage")]
    public static void setPartDamage(IPlayer player, string[] args) {
        try {
            player.Vehicle.SetPartDamageLevel(byte.Parse(args[0]), byte.Parse(args[1]));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setPartDamage2")]
    public static void setPartDamage2(IPlayer player, string[] args) {
        try {
            player.Vehicle.SetPartDamageLevelExt((VehiclePart)int.Parse(args[0]), byte.Parse(args[1]));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("openInteractionTattooMenu")]
    public static void openInteractionTattooMenu(IPlayer player, string[] args) {
        TattooController.playerOpenTatooMenu(player, player);
    }

    [Command("addCharInjury")]
    public static void addCharInjury(IPlayer player, string[] args) {
        try {
            DamageController.addPlayerInjury(player, (DamageType)int.Parse(args[0]), (CharacterBodyPart)int.Parse(args[1]), int.Parse(args[2]));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setWeather")]
    public static void setWeather(IPlayer player, string[] args) {
        try {
            foreach(var p in ChoiceVAPI.GetAllPlayers()) {
                ChoiceVAPI.setWeatherMixForPlayer(p, args[0], args[1], float.Parse(args[2]));
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setWeatherTransition")]
    public static void setWeatherTransition(IPlayer player, string[] args) {
        try {
            ChoiceVAPI.setWeatherTransition(args[0], args[1], float.Parse(args[2]));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("openOtherCef")]
    public static void openOtherCef(IPlayer player, string[] args) {
        try {
            WebController.openFileSystem(player);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setOverlay")]
    public static void setOverlay(IPlayer player, string[] args) {
        try {
            player.resetOverlayType("hair_overlay");
            player.setOverlay("hair_overlay", args[0], args[1]);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("addOverlay")]
    public static void addOverlay(IPlayer player, string[] args) {
        try {
            using(var db = new ChoiceVDb()) {
                var newOv = new confighairoverlay {
                    collection = args[0],
                    hash = args[1],
                    displayName = args[2]
                };

                db.confighairoverlays.Add(newOv);
                db.SaveChanges();
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getVehCard")]
    public static void getVehCard(IPlayer player, string[] args) {
        var cfg = InventoryController.getConfigItemForType<VehicleRegistrationCard>();
        player.getInventory().addItem(new VehicleRegistrationCard(cfg, "", -1, ChoiceVAPI.FindNearbyVehicle(player), ""), true);
    }

    [Command("selfInteractMenu")]
    public static void selfInteractMenu(IPlayer player, string[] args) {
        InteractionController.onPlayerInteraction(player, player, 0, default, default, default);
    }

    [Command("createItems")]
    public static void createItems(IPlayer player, string[] args) {
        try {
            var cfg = InventoryController.getConfigById(int.Parse(args[0]));
            var items = InventoryController.createItems(cfg, int.Parse(args[1]), int.Parse(args[2]));
            foreach(var item in items) {
                player.getInventory().addItem(item, args.Length > 3);
            }
            player.sendNotification(NotifactionTypes.Success, $"Item mit der ID {args[0]} {args[1]}x erstellt {items.First().Name}", "");
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testIplSync")]
    public static void testIplSync(IPlayer player, string[] args) {
        try {
            InteriorController.toogleIplByIdentifier("CARGO_SHIP", args[0] == "1");

            var ipl = InteriorController.getMapObjectByIdentifer<YmapIPL>("CARGO_SHIP");
            SoundController.playSoundAtCoords(ipl.Position, 300, SoundController.Sounds.ShipHorn, 0.3f);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    private static string SoundIdentifier;
    [Command("playSound")]
    public static void playSound(IPlayer player, string[] args) {
        try {
            SoundIdentifier = SoundController.playSoundAtCoords(player.Position, int.Parse(args[1]), (SoundController.Sounds)Enum.Parse(typeof(SoundController.Sounds), args[0]), float.Parse(args[2]), args[3], bool.Parse(args[4]));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("stopSound")]
    public static void stopSound(IPlayer player, string[] args) {
        try {
            SoundController.stopSound(SoundIdentifier);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }
    
    //[CommandAttribute("testOrder")]
    //public static void testOrder(IPlayer player, string[] args) {
    //    try {
    //        OrderController.addOrderItem(new List<OrderComponent> { new OrderVehicle("Massacro", 318, 12, 3, 0) });
    //    } catch(Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    [Command("testCable")]
    public static void testCable(IPlayer player, string[] args) {

        try {
            var vehicle = VehicleController.createVehicle(ChoiceVAPI.Hash("cablecar"), player.Position, Rotation.Zero, 0);

            InvokeController.AddTimedInvoke("Tl", i => {
                vehicle.Position = new Position(vehicle.Position.X, vehicle.Position.Y, vehicle.Position.Z + 0.0001f);
            }, TimeSpan.FromMilliseconds(1), true);
        } catch(Exception e) {
            Logger.logException(e);

        }
    }

    [Command("attach")]
    public static void attachVehicles(IPlayer player, string[] args) {
        try {
            var nearests = ChoiceVAPI.FindNearbyVehicles(player, 10, v => v.DbModel.ModelName != "TrFlat" && !v.hasData("ATTACHED"));

            foreach(var nearest in nearests) {
                var startPoint = nearest.DbModel.StartPoint.FromJson();
                var endPoint = nearest.DbModel.EndPoint.FromJson();

                //var height = Math.Abs(endPoint.Z - startPoint.Z);
                var zOffset = Math.Abs(startPoint.Z);
                var flatBed = ChoiceVAPI.FindNearbyVehicle(player, 30, v => v.DbModel.ModelName == "TrFlat");

                nearest.setData("ATTACHED", true);
                nearest.AttachToEntity(flatBed, ushort.Parse(args[0]), ushort.Parse(args[1]), new Position(0, float.Parse(args[2]), float.Parse(args[3]) + zOffset * 3), new Rotation(0, 0, 0), true, false);
            }
        } catch(Exception e) {
            Logger.logException(e);

        }
    }

    [Command("detach")]
    public static void detach(IPlayer player, string[] args) {

        try {
            var nearest = ChoiceVAPI.FindNearbyVehicle(player, 15, v => v.DbModel.ModelName != "Flatbed");
            var flatBed = player.Vehicle;

            nearest.Detach();
        } catch(Exception e) {
            Logger.logException(e);

        }
    }

    [Command("moveVehicle")]
    public static void moveVehicle(IPlayer player, string[] args) {

        try {
            var nearest = ChoiceVAPI.FindNearbyVehicle(player, 1000);

            InvokeController.AddTimedInvoke("Tl", i => {
                nearest.Position = new Position(nearest.Position.X + 1, nearest.Position.Y, nearest.Position.Z + 1);
            }, TimeSpan.FromSeconds(1), true);
        } catch(Exception e) {
            Logger.logException(e);

        }
    }

    [Command("removeFuel")]
    public static void removeFuel(IPlayer player, string[] args) {

        try {
            var nearest = ChoiceVAPI.FindNearbyVehicle(player);
            nearest.reduceFuel(0.1f);
            player.sendNotification(NotifactionTypes.Info, "10% wurde entfernt!", "10% wurde entfernt!", NotifactionImages.Car);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("openIDOS")]
    public static void openIDOS(IPlayer player, string[] args) {
        try {
            OrderController.openBuyMenu(player, int.Parse(args[0]));
        } catch(Exception) {

        }
    }

    [Command("setEffect")]
    public static void setEffect(IPlayer player, string[] args) {
        try {
            EffectController.playScreenEffect(player, new ScreenEffect(args[0], TimeSpan.FromMilliseconds(int.Parse(args[1])), args[2] == "1"));
        } catch(Exception) {

        }
    }

    [Command("setWalkAnimation")]
    public static void setWalkAnimation(IPlayer player, string[] args) {
        try {
            player.setPedRagdoll();
        } catch(Exception) {

        }
    }

    [Command("createShowcaseVehicle")]
    public static void createShowcaseVehicle(IPlayer player, string[] args) {
        var vehicle = ChoiceVAPI.CreateVehicle(ChoiceVAPI.Hash(args[0]), player.Position, Rotation.Zero);

        vehicle.SetNetworkOwner(player, false);
        InvokeController.AddTimedInvoke("Test", i => {
            vehicle.LockState = VehicleLockState.Locked;
            player.emitClientEvent("MAKE_VEHICLE_DISPLAY", vehicle);
        }, TimeSpan.FromSeconds(2), false);
    }

    [Command("testDrawerFill")]
    public static void testDrawerFill(IPlayer player, string[] args) {
        try {
            var freeDrawer = LockerController.getFreeServerAccessedDrawer();
            if(freeDrawer != null) {
                var cfg = InventoryController.getConfigById(15);
                var items = InventoryController.createItems(cfg, 1);
                freeDrawer.setToGiveItems(player.getCharacterId(), items, "COMMANDS");
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    private static bool onServerAccessedDrawerTaskCompleted(ServerAccessedDrawer drawer) {
        if(!drawer.IsReceivingItems) {
            var player = ChoiceVAPI.GetAllPlayers().First();

            player.sendNotification(NotifactionTypes.Success, "Hat gefunkt!", "Hat gefunkt!");
        }

        return true;
    }

    [Command("testDrawerReceive")]
    public static void testDrawerReceive(IPlayer player, string[] args) {
        try {
            var freeDrawer = LockerController.getFreeServerAccessedDrawer();
            if(freeDrawer != null) {
                freeDrawer.setToReceiveItems(player.getCharacterId(), "COMMANDS");
            }

            LockerController.ServerAccessedDrawerAccessedDelegate += onServerAccessedItemsReceived;
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    private static bool onServerAccessedItemsReceived(ServerAccessedDrawer drawer) {
        if(drawer.Inventory.hasItem<ArmorWest>(i => true)) {

            var player = ChoiceVAPI.GetAllPlayers().First();
            player.sendNotification(NotifactionTypes.Success, "Hat gefunkt!", "Hat gefunkt!");
            return true;
        }

        return false;
    }

    [Command("showSpeed")]
    public static void showSpeed(IPlayer player, string[] args) {
        InvokeController.AddTimedInvoke("", i => {
            player.sendNotification(NotifactionTypes.Info, player.MoveSpeed.ToString(), "");
        }, TimeSpan.FromSeconds(1), true);
    }

    [Command("createInjury")]
    public static void createInjury(IPlayer player, string[] args) {
        var damg = player.getCharacterData().CharacterDamage;

        damg.addInjury(player, (DamageType)Enum.Parse(typeof(DamageType), args[0]), (CharacterBodyPart)Enum.Parse(typeof(CharacterBodyPart), args[1]), int.Parse(args[2]));
    }

    [Command("setSign")]
    public static void setSign(IPlayer player, string[] args) {
        try {
            var sign = new WetSign(player.Position, Rotation.Zero, 7.5f, 7.5f, false, new Dictionary<string, dynamic>());
            sign.initialize();
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testOil")]
    public static void testOil(IPlayer player, string[] args) {
        try {
            player.Vehicle.DriftMode = true;
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setRotation")]
    public static void setRotation(IPlayer player, string[] args) {
        player.Vehicle.Rotation = new DegreeRotation(int.Parse(args[0]), int.Parse(args[1]), int.Parse(args[2]));
    }

    private class MedicAnalyseCefEvent : IPlayerCefEvent {
        public int charId;
        public string[] injuries;
        public MedicAnalyseCefEvent(int charId, string[] injs) {
            Event = "MEDICAL_ANALYSE";
            injuries = injs;
            this.charId = charId;
        }
        public string Event { get; }
    }

    private class InjuryCefObject {
        public string bodyPart;
        public int injuryId;
        public int seed;
        public int severness;

        public InjuryCefObject(int injuryId, int severness, CharacterBodyPart bodyPart, int seed) {
            this.injuryId = injuryId;
            this.severness = severness;
            this.bodyPart = CharacterBodyPartToCef[bodyPart];
            this.seed = seed;
        }
    }

    //[CommandAttribute("getRadioList")]
    //public static void getRadioList(IPlayer player, string[] args) {
    //    try {
    //        foreach (var entry in RadioController.activeRadios) {
    //            player.sendBlockNotification($"ID: {entry.radioId}", "");
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    private class SortTest {
        public int Level;
        public int Room;
    }

    public class VehicleModKit {

        public byte ModKit { get; set; }
        public byte ModTypeIndex { get; set; }
        public byte ModsCount { get; set; }
        public VehicleModKitMod[] Mods { get; set; }
    }

    public class VehicleModKitMod {
        public int ModIndex { get; set; }
        public string ModDisplayName { get; set; }
    }

    private class OtherCefData {
        public int DeviceId;
        public long SocialSecurityNumber;

        public OtherCefData(int deviceId, long socialSecurityNumber) {
            DeviceId = deviceId;
            SocialSecurityNumber = socialSecurityNumber;
        }

        public string encrypt(string key) {
            return SecurityController.EncryptForCef(this.ToJson(), key);
        }
    }

    #region SupportCommands

    [Command("showSc")]
    public static void showSc(IPlayer player, string[] args) {
        var time = int.Parse(args[0]);
        foreach(var p in Alt.GetAllPlayers()) {
            if(p.Position.Distance(player.Position) < 100) {
                player.emitClientEvent("TEXT_LABEL_ON_PLAYER", p, (string)p.getData("SOCIALCLUB"), time * 1000);
            }
        }
    }

    [Command("ban")]
    public static void ban(IPlayer player, string[] args) {
        var target = ChoiceVAPI.GetAllPlayers().FirstOrDefault(t => t.getData("SOCIALCLUB") == args[0]);
        if(target != null) {
            target.ban(args[1]);
        }
    }

    [Command("kick")]
    public static void kick(IPlayer player, string[] args) {
        var target = ChoiceVAPI.GetAllPlayers().FirstOrDefault(t => t.getData("SOCIALCLUB") == args[0]);
        if(target != null) {
            target.Kick(args[1]);
        }
    }

    [Command("PlayerTp")]
    public static void PlayerTp(IPlayer player, string[] args) {
        var target = ChoiceVAPI.GetAllPlayers().FirstOrDefault(t => t.getData("SOCIALCLUB") == args[0]);
        if(target != null) {
            player.Position = target.Position;
        }
    }

    //[Command("SupportMenu")]
    //public static void SupportMenu(IPlayer player, string[] args) {
    //    SupportController.SupportMenu(player);
    //}

    // TODO - Remove - Only test
    [Command("Reset")]
    public static void reset(IPlayer player, string[] args) {
        try {
            ChoiceVAPI.emitClientEventToAll("SET_WEATHER_TRANISTION", "EXTRASUNNY", "EXTRASUNNY", 0f);
            ChoiceVAPI.emitClientEventToAll("SET_DATE_TIME_HOUR", "12");
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    // TODO - Remove - Only test
    [Command("giveCash")]
    public static void giveCash(IPlayer player, string[] args) {
        try {
            player.addCash(1000);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("resetSc")]
    public static void resetSc(IPlayer player, string[] args) {
        try {
            using(var db = new ChoiceVDb()) {
                var account = db.accounts.FirstOrDefault(a => a.socialclubName == args[0]);
                if(account != null) {
                    var dbChar = db.characters.FirstOrDefault(c => c.accountId == account.id);
                    if(dbChar != null) {
                        db.characters.Remove(dbChar);
                        account.state = 0;
                        db.SaveChanges();
                    } else {
                        player.sendBlockNotification("Manuell machen!", "");
                    }
                } else {
                    player.sendBlockNotification("Manuell machen!", "");
                }
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("resetScPos")]
    public static void resetScPos(IPlayer player, string[] args) {
        try {
            using(var db = new ChoiceVDb()) {
                var account = db.accounts.FirstOrDefault(a => a.socialclubName == args[0]);
                if(account != null) {
                    var dbChar = db.characters.FirstOrDefault(c => c.accountId == account.id);
                    if(dbChar != null) {
                        dbChar.position = new Position(0, 0, 72).ToJson();
                        db.SaveChanges();
                    } else {
                        player.sendBlockNotification("Manuell machen!", "");
                    }
                } else {
                    player.sendBlockNotification("Manuell machen!", "");
                }
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    #endregion

    #region Vehicle damage methods

    //[CommandAttribute("repairMenu")]
    //public static void repairMenu(IPlayer player, string[] args) {
    //    try {
    //        var veh = ChoiceVAPI.FindNearbyVehicle(player);

    //        if (veh != null) {
    //            WorkshopController.repairMenu(player, veh, null);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("resetDamage")]
    //public static void resetDamage(IPlayer player, string[] args) {
    //    try {
    //        var veh = ChoiceVAPI.FindNearbyVehicle(player);

    //        if (veh != null) {
    //            if (VehicleController.AllVehicleUsedSeats.Find(v => v.VehicleId == veh.getId()) == null) {
    //                VehicleController.resetDamage(veh);
    //            } else {
    //                player.sendNotification(Constants.NotifactionTypes.Info, "Alle Passagiere bitte aussteigen.", "Alle aussteigen", Constants.NotifactionImages.Car);
    //            }
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    #endregion

    #region Vehicle tuning methods

    //[CommandAttribute("tuningMenu")]
    //public static void tuningMenu(IPlayer player, string[] args) {
    //    try {
    //        var veh = ChoiceVAPI.FindNearbyVehicle(player);

    //        if (veh != null) {
    //            WorkshopController.tuningMenu(player, veh, null);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("resetTuning")]
    //public static void resetTuning(IPlayer player, string[] args) {
    //    try {
    //        var veh = ChoiceVAPI.FindNearbyVehicle(player);

    //        if (veh != null) {
    //            VehicleController.resetTuning(veh);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("setPower")]
    //public static void setPower(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 0) {
    //            var veh = ChoiceVAPI.FindNearbyVehicle(player);

    //            if (veh != null) {
    //                VehicleController.setPower(veh, float.Parse(args[0]));
    //            }
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("setSpeed")]
    //public static void setSpeed(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 0) {
    //            var veh = ChoiceVAPI.FindNearbyVehicle(player);

    //            if (veh != null) {
    //                VehicleController.setSpeed(veh, float.Parse(args[0]));
    //            }
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("setPowerModel")]
    //public static void setPowerModel(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 0) {
    //            var veh = ChoiceVAPI.FindNearbyVehicle(player);

    //            if (veh != null) {
    //                VehicleController.setPowerModel(veh, float.Parse(args[0]));
    //            }
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("setSpeedModel")]
    //public static void setSpeedModel(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 0) {
    //            var veh = ChoiceVAPI.FindNearbyVehicle(player);

    //            if (veh != null) {
    //                VehicleController.setSpeedModel(veh, float.Parse(args[0]));
    //            }
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    #endregion

    #region Garage methods

    [Command("createGarageSpot")]
    public static void createGarageSpot(IPlayer player, string[] args) {
        try {
            var garageId = int.Parse(args[0]);

            CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                GarageController.createGarageSpot(player, garageId, p, r, w, h);
            });


        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("deleteGarage")]
    public static void deleteGarage(IPlayer player, string[] args) {
        try {
            if(args.Length > 0) {
                var garageid = int.Parse(args[0]);

                GarageController.deleteGarage(garageid);

            } else {
                player.sendNotification(NotifactionTypes.Info, "Use: Garage ID.", "", NotifactionImages.Car);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("deleteGarageSpot")]
    public static void deleteGarageSpot(IPlayer player, string[] args) {
        try {
            if(args.Length > 1) {
                var garageid = int.Parse(args[0]);
                var spotid = int.Parse(args[1]);

                GarageController.deleteGarageSpot(garageid, spotid);

            } else {
                player.sendNotification(NotifactionTypes.Info, "Use: Garage ID, Spot ID.", "", NotifactionImages.Car);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("reloadGarages")]
    public static void reloadGarages(IPlayer player, string[] args) {
        try {
            CollisionShape.AllShapes.RemoveAll(c => Vector3.Distance(c.Position, player.Position) < 100 && c.EventName == "GARAGE_SPOT");
            player.emitClientEvent("SPOT_END");

            GarageController.reloadGarages();
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("showGarages")]
    public static void showGarage(IPlayer player, string[] args) {
        try {
            var shapesByNear = CollisionShape.AllShapes.Where(c => Vector3.Distance(c.Position, player.Position) < 100 && c.EventName == "GARAGE_SPOT");
            foreach(var shape in shapesByNear) {
                player.emitClientEvent("SPOT_ADD", shape.Position.X, shape.Position.Y, shape.Width, shape.Height, 1, shape.Rotation);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    #endregion

    #region Workshop methods

    //[CommandAttribute("createWorkshop")]
    //public static void createWorkshop(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 4) {
    //            var name = args[0].ToString();
    //            var type = int.Parse(args[1]);
    //            var ownerType = (Constants.WorkshopOwnerType)int.Parse(args[2]);
    //            var ownerId = int.Parse(args[3]);
    //            var ownerName = args[4].ToString();

    //            int id = WorkshopController.createWorkshop(player, name, type, ownerType, ownerId, ownerName);

    //            if (id > 0)
    //                player.sendNotification(Constants.NotifactionTypes.Success, $"Workshop {name} with ID {id} has been created.", "", Constants.NotifactionImages.Car);
    //            else
    //                player.sendNotification(Constants.NotifactionTypes.Warning, $"Workshop {name} has not been created.", "", Constants.NotifactionImages.Car);

    //        } else {
    //            player.sendNotification(Constants.NotifactionTypes.Info, "Use: Workshop Name, Workshop Type, Owner Type, Owner Id, Owner Name.", "", Constants.NotifactionImages.Car);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("createWorkshopSpot")]
    //public static void createWorkshopSpot(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 1) {
    //            var id = int.Parse(args[0]);
    //            var type = (Constants.WorkshopSpotType)int.Parse(args[1]);
    //            int spotid = -1;

    //            if (args.Length > 2)
    //                spotid = WorkshopController.createWorkshopSpot(id, type, player.Position, player.Rotation, int.Parse(args[2]), int.Parse(args[2]));
    //            else
    //                spotid = WorkshopController.createWorkshopSpot(id, type, player.Position, player.Rotation, 6, 6);

    //            if (spotid > 0)
    //                player.sendNotification(Constants.NotifactionTypes.Success, $"Workshop spot ID {spotid} for workshop ID {id} has been created.", "", Constants.NotifactionImages.Car);
    //            else
    //                player.sendNotification(Constants.NotifactionTypes.Warning, $"Workshop spot for workshop ID {id} has not been created.", "", Constants.NotifactionImages.Car);

    //        } else {
    //            player.sendNotification(Constants.NotifactionTypes.Info, "Use: Workshop Id, Spot Type.", "", Constants.NotifactionImages.Car);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("deleteWorkshop")]
    //public static void deleteWorkshop(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 0) {
    //            var shopid = int.Parse(args[0]);

    //            WorkshopController.deleteWorkshop(shopid);

    //        } else {
    //            player.sendNotification(Constants.NotifactionTypes.Info, "Use: Workshop ID.", "", Constants.NotifactionImages.Car);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("deleteWorkshopSpot")]
    //public static void deleteWorkshopSpot(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 1) {
    //            var shopid = int.Parse(args[0]);
    //            var spotid = int.Parse(args[1]);

    //            WorkshopController.deleteWorkshopSpot(shopid, spotid);

    //        } else {
    //            player.sendNotification(Constants.NotifactionTypes.Info, "Use: Workshop ID, Spot ID.", "", Constants.NotifactionImages.Car);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("reloadWorkshops")]
    //public static void reloadWorkshops(IPlayer player, string[] args) {
    //    try {
    //        CollisionShape.AllShapes.RemoveAll(c => Vector3.Distance(c.Position, player.Position) < 100 && c.EventName == "WORKSHOP_SPOT");
    //        player.emitClientEvent("SPOT_END");

    //        WorkshopController.reloadWorkshops();
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("showWorkshops")]
    //public static void showWorkshop(IPlayer player, string[] args) {
    //    try {
    //        var shapesByNear = CollisionShape.AllShapes.Where(c => Vector3.Distance(c.Position, player.Position) < 100 && c.EventName == "WORKSHOP_SPOT");
    //        foreach (var shape in shapesByNear) {
    //            player.emitClientEvent("SPOT_ADD", shape.Position.X, shape.Position.Y, shape.Width, shape.Height, 1, shape.Rotation);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    #endregion

    #region Gasstation methods

    //[CommandAttribute("createGasstation")]
    //public static void createGasstation(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 4) {
    //            var name = args[0].ToString();
    //            var type = int.Parse(args[1]);
    //            var ownerType = (Constants.GasstationOwnerType)int.Parse(args[2]);
    //            var ownerId = int.Parse(args[3]);
    //            var ownerName = args[4].ToString();

    //            int id = GasstationController.createGasstation(name, type, ownerType, ownerId, ownerName);

    //            if (id > 0)
    //                player.sendNotification(Constants.NotifactionTypes.Success, $"Gasstation {name} with ID {id} has been created.", "", Constants.NotifactionImages.Car);
    //            else
    //                player.sendNotification(Constants.NotifactionTypes.Warning, $"Gasstation {name} has not been created.", "", Constants.NotifactionImages.Car);

    //        } else {
    //            player.sendNotification(Constants.NotifactionTypes.Info, "Use: Gasstation Name, Gasstation Type, Owner Type, Owner Id, Owner Name.", "", Constants.NotifactionImages.Car);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("createGasstationSpot")]
    //public static void createGasstationSpot(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 1) {
    //            var id = int.Parse(args[0]);
    //            var type = int.Parse(args[1]);
    //            int spotid = -1;

    //            if (args.Length > 2)
    //                spotid = GasstationController.createGasstationSpot(id, type, player.Position, player.Rotation, int.Parse(args[2]), int.Parse(args[2]));
    //            else
    //                spotid = GasstationController.createGasstationSpot(id, type, player.Position, player.Rotation, 3, 6);

    //            if (spotid > 0)
    //                player.sendNotification(Constants.NotifactionTypes.Success, $"Gasstation spot ID {spotid} for gasstation ID {id} has been created.", "", Constants.NotifactionImages.Car);
    //            else
    //                player.sendNotification(Constants.NotifactionTypes.Warning, $"Gasstation spot for gasstation ID {id} has not been created.", "", Constants.NotifactionImages.Car);

    //        } else {
    //            player.sendNotification(Constants.NotifactionTypes.Info, "Use: Gasstation Id, Spot Type.", "", Constants.NotifactionImages.Car);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("deleteGasstation")]
    //public static void deleteGasstation(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 0) {
    //            var shopid = int.Parse(args[0]);

    //            GasstationController.deleteGasstation(shopid);

    //        } else {
    //            player.sendNotification(Constants.NotifactionTypes.Info, "Use: Gasstation ID.", "", Constants.NotifactionImages.Car);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("deleteGasstationSpot")]
    //public static void deleteGasstationSpot(IPlayer player, string[] args) {
    //    try {
    //        if (args.Length > 1) {
    //            var shopid = int.Parse(args[0]);
    //            var spotid = int.Parse(args[1]);

    //            GasstationController.deleteGasstationSpot(shopid, spotid);

    //        } else {
    //            player.sendNotification(Constants.NotifactionTypes.Info, "Use: Gasstation ID, Spot ID.", "", Constants.NotifactionImages.Car);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("reloadGasstations")]
    //public static void reloadGasstations(IPlayer player, string[] args) {
    //    try {
    //        CollisionShape.AllShapes.RemoveAll(c => Vector3.Distance(c.Position, player.Position) < 100 && c.EventName == "GASSTATION_SPOT");
    //        player.emitClientEvent("SPOT_END");

    //        GasstationController.reloadGasstations();
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("showGasstations")]
    //public static void showGasstation(IPlayer player, string[] args) {
    //    try {
    //        var shapesByNear = CollisionShape.AllShapes.Where(c => Vector3.Distance(c.Position, player.Position) < 100 && c.EventName == "GASSTATION_SPOT");
    //        foreach (var shape in shapesByNear) {
    //            player.emitClientEvent("SPOT_ADD", shape.Position.X, shape.Position.Y, shape.Width, shape.Height, 1, shape.Rotation);
    //        }
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    #endregion

    #region Company methods

    [Command("createCompany")]
    public static void createCompany(IPlayer player, string[] args) {
        try {
            if(args.Length > 6) {
                CompanyController.createNewCompany(player, args[0], args[1], args[2], args[3], (CompanyType)Enum.Parse(typeof(CompanyType), args[4]), args[5], player.Position, int.Parse(args[6]));
            } else {
                player.sendNotification(NotifactionTypes.Info, "Use: Name, Company Type, Code Company, Bankaccount.", "", NotifactionImages.Car);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("joinCompany")]
    public static void joinCompany(IPlayer player, string[] args) {
        try {
            var company = CompanyController.findCompanyById(int.Parse(args[0]));
            company.hireEmployee(player, player.getMainBankAccount(), player.getMainPhoneNumber(), 0, true);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    #endregion

    #region DispatchCommands

    //[CommandAttribute("sendDispatch")]
    //public static void setDispatch(IPlayer player, string[] args) {
    //    try {

    //        EmergencyCallType type = EmergencyCallType.Police;
    //        string message = null;

    //        if (args.Length >= 1) {
    //            type = Enum.Parse<EmergencyCallType>(args[0], true);
    //        }

    //        if (args.Length >= 2) {
    //            message = args[1];
    //        }

    //        EmergencyControlCentreController.addNewEmergencyCall(player, type, EmergencyCallSeverity.High, message);
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("attachBlip")]
    //public static void attachBlip(IPlayer player, string[] args) {
    //    try {
    //        var blip = Alt.CreateBlip(BlipType.Cop, player);
    //        InvokeController.AddTimedInvoke("TEST", (i) => {
    //            Alt.RemoveBlip(blip);
    //            blip.OnRemove();
    //            blip.Remove();
    //            blip.RemoveRef();
    //        }, TimeSpan.FromSeconds(5), false);
    //    } catch(Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("attachToDispatch")]
    //public static void attachToDispatch(IPlayer player, string[] args) {
    //    try {
    //        EmergencyControlCentreController.registerPlayerForBlips(player);
    //        EmergencyControlCentreController.registerPlayerForCallCentre(player);
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("detachFromDispatch")]
    //public static void detachFromDispatch(IPlayer player, string[] args) {
    //    try {
    //        EmergencyControlCentreController.deregisterPlayerForBlips(player);
    //        EmergencyControlCentreController.deregisterPlayerForCallCentre(player);
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("forceRemovePlayerFromDispatchCenter")]
    //public static void forceRemovePlayerFromDispatchCenter(IPlayer player, string[] args)
    //{
    //    try {
    //        // ToDo: player durch den eigentlichen Spiekler ersetzen, wenn Sozialversicherungsnummer vorhanden ist.
    //        EmergencyControlCentreController.deregisterPlayerForBlips(player);
    //        EmergencyControlCentreController.deregisterPlayerForCallCentre(player, true);
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }

    //}

    //[CommandAttribute("ShowCallCenterMenu")]
    //public static void showCallCenterMenu(IPlayer player, string[] args) {
    //    try {
    //        EmergencyControlCentreController.doShowCallCenterMenu(player);
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    //[CommandAttribute("ShowManagementMenu")]
    //public static void showManagementMenu(IPlayer player, string[] args) {
    //    try {
    //        EmergencyControlCentreController.doShowManagementMenu(player);
    //    } catch (Exception e) {
    //        Logger.logException(e);
    //    }
    //}

    #endregion

    #region PrisonCommands

    //[CommandAttribute("SetPrisonExitSpot")]
    //public static void setPrisonExitSpot(IPlayer player, string[] args) {
    //    try {
    //        PrisonController.doSetPrisonExitPosition(player);
    //    }
    //    catch {

    //    }
    //}

    //[CommandAttribute("SetPrisonEnterSpot")]
    //public static void setPrisonEnterSpot(IPlayer player, string[] args) {
    //    try {
    //        PrisonController.doSetPrisonEnterPosition(player);
    //    }
    //    catch {

    //    }
    //}

    #endregion

    [Command("forceAnim")]
    public static void forceAnim(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("PLAY_ANIM", args[0], args[1], int.Parse(args[2]), int.Parse(args[3]), -1, false, true, 0);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    //62, 44
    //50, 49, 48, 45
    [Command("testStuff")]
    public static void testStuff(IPlayer player, string[] args) {
        try {
            //CallbackController.getPlayerMaterialStandOn(player, (p, mat) => {
            //    var i = 1 + 1; //1
            //});

            //CallbackController.getPlayerRegion(player, (p, region) => {
            //    var i = 1 + 1; //1
            //});

            //CallbackController.getRegion(player, player.Position, (p, region) => {
            //    var i = 1 + 1; //1
            //});

            //CallbackController.getPlayerInInterior(player, "123", (p, found, milo) => {
            //    var i = 1 + 1; //1
            //});

            //CallbackController.getPlayerInInterior(player, new string[] { "123", "234" }, (p, found, milo) => {
            //    var i = 1 + 1; //1
            //});

            //CallbackController.getGroundZFromPos(player, player.Position, (p, z, inFrontOfWall, probablyInObject) => {
            //    var i = 1 + 1; //1
            //});

            //CallbackController.getNearbyObject(player, player.Position, 10, new List<uint> { ChoiceVAPI.Hash("prop_dumpster_02a") }, (p, worked, hash, pos, heading) => {
            //    var i = 1 + 1; //1
            //});

            //CallbackController.getPositionInFront(player, 1, (p, pos) => {
            //    var i = 1 + 1; //1
            //});

            //CallbackController.getPlayerCameraHeading(player, (p, heading) => {
            //    var i = 1 + 1;
            //});

            //CallbackController.getTextureVariations(player, 11, 0, true, (p, pos) => {
            //    var i = 1 + 1; //1
            //});

            //CallbackController.getGXTLocalizedName(player, "123", "123", (p, name, data) => {
            //    var i = 1 + 1; //1
            //});

            //ControlCenterController.createDispatch(DispatchType.NpcSilentDispatch, "Test Alarm!", "Dies ist ein Test Alarm!", player.Position, true);

            //var obj = Alt.CreateObject("prop_rub_trolley01a", player.Position + new Position(1, 1, 0), player.Rotation, 255, 0, 100, 2000);
            //obj.AttachToEntity(player, 6286, 0, new Position(0.12f, -0.1f, -0.01f), new Rotation(1.5708f, 1.5708f, -1.8326f), false, false);
            //player.AttachToEntity(obj, 0, 0, new Position(0, 0, 2), EmptyRotation, false, false);

            //var ped = Alt.CreatePed(PedModel.Deer, player.Position, player.Rotation);

            //player.Anim

            //foreach(var obj in Alt.GetAllNetworkObjects()) {
            //    obj.Destroy();
            //}

            //Alt.OnClientRequestObject += onRequestObject;
            //Alt.OnClientDeleteObject += onRequestDelete;

            Alt.OnRequestSyncScene += Alt_OnRequestSyncScene;
            Alt.OnStartSyncedScene += Alt_OnStartSyncedScene;
            Alt.OnStopSyncedScene += Alt_OnStopSyncedScene;
            Alt.OnUpdateSyncedScene += Alt_OnUpdateSyncedScene;
            Alt.OnGivePedScriptedTask += Alt_OnGivePedScriptedTask;
            Alt.OnPlayerRequestControl += Alt_OnPlayerRequestControl;
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    private static bool Alt_OnPlayerRequestControl(IEntity target, IPlayer player) {
        throw new NotImplementedException();
    }

    private static bool Alt_OnGivePedScriptedTask(IPlayer source, IPed target, uint taskType) {
        return true;
    }

    private static void Alt_OnUpdateSyncedScene(IPlayer source, float startRate, int sceneId) {
        throw new NotImplementedException();
    }

    private static void Alt_OnStopSyncedScene(IPlayer source, int sceneId) {
        throw new NotImplementedException();
    }

    private static void Alt_OnStartSyncedScene(IPlayer source, int sceneId, Position position, Rotation rotation, uint animDictHash, Dictionary<IEntity, uint> entityAndAnimHash) {
        throw new NotImplementedException();
    }

    private static bool Alt_OnRequestSyncScene(IPlayer source, int sceneId) {
        return true;
    }

    private static bool onRequestDelete(IPlayer target) {
        return true;
    }

    private static bool onRequestObject(IPlayer target, uint model, Position position) {
        return true;
    }


    [Command("testStuff2")]
    public static void testStuff2(IPlayer player, string[] args) {
        try {
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("kill")]
    public static void killPlayer(IPlayer player, string[] args) {
        try {
            DamageController.onPlayerDead(player, player, ChoiceVAPI.Hash(args[0]));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("anchor")]
    public static void anchor(IPlayer player, string[] args) {
        try {
            player.Vehicle.BoatAnchor = true;
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("removeKeys")]
    public static void removeKeys(IPlayer player, string[] args) {
        try {
            var keys = player.getInventory().getItems<VehicleKey>(i => true);
            foreach(var key in keys) {
                player.getInventory().removeItem(key);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("showJobs")]
    public static void showJobs(IPlayer player, string[] args) {
        try {
            MiniJobController.openMinijobMenu(player, (MiniJobTypes)int.Parse(args[0]));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("advanceMinijob")]
    public static void advanceMinijob(IPlayer player, string[] args) {
        try {
            var minijob = MiniJobController.Minijobs.FirstOrDefault(m => m.isPlayerDoingMinijob(player));
            var currentTask = minijob.AllTasks[minijob.getPlayerInstance(player).CurrentTask];
            currentTask.onFinish(minijob.getPlayerInstance(player));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    static MechanicalGame game;
    [Command("testMechGame")]
    public static void testMechGame(IPlayer player, string[] args) {
        try {
            if(game == null) {
                game = MechanicalGameController.startMechanicGame(player, 2, 3, new List<MechanicalGameComponent> {
                    new IdentifyMechanicalGameComponent(0, "A85SD8", SpecialToolFlag.Screwdriver, false, false, true, 0, "mech1x2", new List<GameComponentVector>{ new GameComponentVector(0, 0), new GameComponentVector(0, 1)}),
                    new IdentifyMechanicalGameComponent(1, "BN966S", SpecialToolFlag.Crowbar, true, true, true, 1, "mech1x1", new List<GameComponentVector>{ new GameComponentVector(0, 0) })
                }, (p, g, c, a) => { return true; });
            } else {
                game.showToPlayer(player);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("createTestMechgame")]
    public static void createTestMechgame(IPlayer player, string[] args) {
        game = new MechanicalGame("2~3~0#True#True#0#mech1x2#[{\"x\":0,\"y\":0},{\"x\":0,\"y\":1}]#I#A85SD8#False#3|1#False#False#1#mech1x1#[{\"x\":2,\"y\":0}]#I#BN966S#False#1", (p, g, c, a) => { return true; });
    }

    [Command("getMechGame")]
    public static void getMechGame(IPlayer player, string[] args) {
        var game = VehicleMotorCompartmentController.AllCompartmentBlueprints[0].Compartment.createAsMinigame(ChoiceVAPI.FindNearbyVehicle(player), (p, g, c, a) => { return true; });
        game.showToPlayer(player);
    }

    [Command("testAnimPed")]
    public static void testAnimPed(IPlayer player, string[] args) {
        try {
            var ped = PedController.createPed(null, "g_m_m_armgoon_01", player.Position, 0);
            ped.setScenario(args[0]);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testMissionModule")]
    public static void testMissionModule(IPlayer player, string[] args) {
        try {
            var ped = PedController.createPed(null, "g_m_m_armgoon_01", player.Position, 0);
            var module = new CrimeMissionModule(ped, CrimeNetworkController.CrimePillars[0]);
            module.setActive(true);
            ped.addModule(module);

            var module2 = new CrimeMissionModule(ped, CrimeNetworkController.CrimePillars[1]);
            module2.setActive(true);
            ped.addModule(module2);

            var module3 = new CrimeMissionModule(ped, CrimeNetworkController.CrimePillars[2]);
            module3.setActive(true);
            ped.addModule(module3);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getFenceJewelry")]
    public static void getFenceJewelry(IPlayer player, string[] args) {
        try {
            var cfg1 = InventoryController.getConfigById(329);
            var cfg2 = InventoryController.getConfigById(329);
            var cfg3 = InventoryController.getConfigById(329);

            var item1 = new CutFenceJewelery(new FenceJewelry(cfg1, 1, int.Parse(args[0])));
            var item2 = new CutFenceJewelery(new FenceJewelry(cfg1, 1, int.Parse(args[0])));
            var item3 = new CutFenceJewelery(new FenceJewelry(cfg1, 1, int.Parse(args[0])));

            item1.onUltrasoundScan();
            item2.onUltrasoundScan();
            item3.onUltrasoundScan();

            player.getInventory().addItem(item1);
            player.getInventory().addItem(item2);
            player.getInventory().addItem(item3);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("showWindowDamage")]
    public static void testBumperDamage(IPlayer player, string[] args) {
        try {
            var nearby = ChoiceVAPI.FindNearbyVehicle(player);

            for(byte i = 0; i < 30; i++) {
                if(nearby.IsWindowDamaged(i)) {
                    //Logger.logError("Window: " + i + ", State: " + nearby.IsWindowDamaged(i));
                }
            }
        } catch(Exception e) {
            Logger.logException(e);

        }
    }

    [Command("testSMS")]
    public static void testSMS(IPlayer player, string[] args) {
        try {
            PhoneController.sendSMSToNumber(1, player.getMainPhoneNumber(), String.Join(" ", args));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }
    
    [Command("testCall")]
    public static void testCall(IPlayer player, string[] args) {
        try {
            PhoneCallController.onSmartphoneStartCall(player, new PlayerWebSocketConnectionDataElement{
                Data = new PhoneStartCallCefEvent{
                    owner = player.getMainPhoneNumber(),
                    number = player.getMainPhoneNumber(),
                    hiddenNumber = false,
                }.ToJson()
            });
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getNewspaper")]
    public static void getNewspaper(IPlayer player, string[] args) {
        try {
            var cfg = InventoryController.getConfigItemForType<FlipBook>();
            player.getInventory().addItem(new FlipBook(cfg, args[0], args[0]), true);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("delTC")]
    public static void delTC(IPlayer player, string[] args) {
        try {
            var shirtCombo = new List<int>();
            foreach(var arg in args) {
                shirtCombo.Add(int.Parse(arg));
            }

            using(var db = new ChoiceVDb()) {
                var gender = player.getCharacterData().Gender.ToString();
                var drawId = player.getClothing().Top.Drawable;
                var top = db.configclothings.FirstOrDefault(c => c.gender == gender && c.componentid == 11 && c.drawableid == drawId);

                var combos = db.configclothingshirtstotops.Where(comb => comb.topId == top.id && shirtCombo.Contains(comb.shirtId));
                foreach(var combo in combos) {
                    db.configclothingshirtstotops.Remove(combo);
                }

                db.SaveChanges();
            }

            player.sendNotification(NotifactionTypes.Warning, $"Du hast erfolgreich die Combination gelöscht. Es waren: {shirtCombo.Count} Stück", "");
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("walkStyle")]
    public static void walkStyle(IPlayer player, string[] args) {
        try {
            if(args.Length == 1) {
                player.emitClientEvent("SET_WALKING_ANIMATION", "walk", args[0]);
                player.emitClientEvent("SET_WALKING_ANIMATION", "run", args[0]);
            } else {
                player.emitClientEvent("SET_WALKING_ANIMATION", args[0], args[1]);
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("setDlcClothes")]
    public static void setDlcClothes(IPlayer player, string[] args) {
        try {
            player.SetDlcClothes(byte.Parse(args[1]), byte.Parse(args[2]), byte.Parse(args[3]), 0, ChoiceVAPI.Hash(args[0]));
        } catch(Exception e) {
            Logger.logException(e);
        }
        
        player.Vehicle.Destroy();
    }

    [Command("setDlcProp")]
    public static void setDlcProp(IPlayer player, string[] args) {
        try {
            player.SetDlcProps(byte.Parse(args[1]), byte.Parse(args[2]), byte.Parse(args[3]), ChoiceVAPI.Hash(args[0]));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }


    [Command("updateDbClothing")]
    public static void updateDbClothing(IPlayer player, string[] args) {
        try {
            using(var db = new ChoiceVDb()) {
                //foreach(var clothing in db.configclothings
                //    .Include(c => c.configclothingshirtstotoptops)
                //    .ThenInclude(c => c.shirt)
                //    .Where(c => c.componentid == 11 && c.notBuyable == 0)) {

                //    var noShirtList = Constants.FemaleNoShirts;
                //    if(clothing.gender == "M") {
                //        noShirtList = Constants.MaleNoShirts;
                //    }

                //    if(clothing.configclothingshirtstotoptops.Any(c => noShirtList.Contains(c.shirt.drawableid))) {
                //        var combo = clothing.configclothingshirtstotoptops.FirstOrDefault(c => noShirtList.Contains(c.shirt.drawableid));

                //        clothing.torsoId = combo.otherTorso;
                //    } else {
                //        clothing.torsoId = null;
                //        //Some clothes cannot be equipped without a shirt (torso is glitching through)
                //    }
                //}

                foreach(var combo in db.configclothingshirtstotops.Include(c => c.shirt)) {
                    if(combo.shirt.gender == "M" && Constants.MaleNoShirts.Contains(combo.shirt.drawableid)) {
                        db.Remove(combo);
                    } else if (combo.shirt.gender == "F" && Constants.FemaleNoShirts.Contains(combo.shirt.drawableid)) {
                        db.Remove(combo);
                    }
                }

                db.SaveChanges();
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("updateItemAnimRotations")]
    public static void updateItemAnimRotations(IPlayer player, string[] args) {
        using(var db = new ChoiceVDb()) {
            foreach(var row in db.configitemanimations) {
                if(row.id >= 85) {
                    var rotation = row.rotation.FromJson<Vector3>();
                    var temp = rotation.X;

                    rotation.X = rotation.Z;
                    rotation.Z = temp;

                    row.rotation = rotation.ToJson();
                }
            }

            db.SaveChanges();
        }
    }

    [Command("quaternion")]
    public static void quaternion(IPlayer player, string[] args) {
        var quat = new Quaternion(0.01809016f, -0.176198f, -0.6462473f, -0.742288f);
        var euler = quat.ToEulerAngles();
        var degreeRot = new Vector3(euler.X * 180 / (float)Math.PI, euler.Y * 180 / (float)Math.PI, euler.Z * 180 / (float)Math.PI);
    }

    [Command("setMotorPartDamage")]
    public static void setMotorPartDamage(IPlayer player, string[] args) {
        var vehicle = ChoiceVAPI.FindNearbyVehicle(player, 100000000);

        VehicleMotorCompartmentController.setPartDamageLevel(vehicle, args[0], float.Parse(args[1]));
        VehicleMotorCompartmentController.applyCompartment(vehicle);
    }

    [Command("setPropPrices")]
    public static void setPropPrices(IPlayer player, string[] args) {
        try {
            var glassesPrices = new List<decimal> {
                29.90m,
                35.50m,
                33m,
                30.90m,
                27.90m,
                34m,
            };

            var hatPrices = new List<decimal> {
                39.90m,
                45.50m,
                43m,
                40.90m,
                37.90m,
                44m,
            };

            using(var db = new ChoiceVDb()) {
                var list = db.configclothingprops.Include(c => c.configclothingpropvariations).ToList();
                foreach(var prop in list) {
                   var alreadyPrice = db.configclothingprops.
                        Include(c => c.configclothingpropvariations)
                        .FirstOrDefault(c => c.price != 0 && c.gender != prop.gender && prop.componentid == c.componentid && c.configclothingpropvariations.Any(v => prop.configclothingpropvariations.Select(v => v.name).Contains(v.name)));

                    if(alreadyPrice != null) {
                        prop.price = alreadyPrice.price;
                    } else {
                        if(prop.componentid == 0) {
                            prop.price = hatPrices[prop.drawableid % hatPrices.Count];
                        } else {
                            prop.price = glassesPrices[prop.drawableid % glassesPrices.Count];
                        }
                    }
                    db.SaveChanges();
                }
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }


    private record VariationData(string GXT, string Name);
    [Command("loadVariationsToDb")]
    public static void loadVariationsToDb(IPlayer player, string[] args) {
        try {
            var componentIdx = int.Parse(args[0]);
            var gender = args[1];

            var dic = new Dictionary<string, VariationData>();

            // /loadVariationsToDb 6 M C:\ChoiceV\Server\libs\v-clothingnames-master\male_shoes.json

            // /loadVariationsToDb 7 M X:\Entwicklung\GTA\ChoiceV\libs\v-clothingnames-master\male_accessories.json
            // /loadVariationsToDb 6 F X:\Entwicklung\GTA\ChoiceV\libs\v-clothingnames-master\female_shoes.json

            // /loadVariationsToDb 0 M X:\Entwicklung\GTA\ChoiceV\libs\v-clothingnames-master\props_male_hats.json
            // /loadVariationsToDb 1 M X:\Entwicklung\GTA\ChoiceV\libs\v-clothingnames-master\props_male_glasses.json

            // /loadVariationsToDb 1 F X:\Entwicklung\GTA\ChoiceV\libs\v-clothingnames-master\props_female_glasses.json

            // /loadVariationsToDb 1 M X:\Entwicklung\GTA\ChoiceV\libs\v-clothingnames-master\masks_male.json

            using(StreamReader r = new StreamReader(args[2])) {
                string json = r.ReadToEnd();
                dynamic obj = JsonConvert.DeserializeObject(json);

                var counter = 0;
                while(obj[counter.ToString()] != null) {
                    var cloth = obj[counter.ToString()];

                    var counter2 = 0;
                    while(cloth[counter2.ToString()] != null) {
                        var variant = cloth[counter2.ToString()];

                        CallbackController.getGXTLocalizedName(player, (string)variant["GXT"], $"{counter}-{counter2}", (p, name, data) => {
                            dic.Add(data, new VariationData((string)variant["GXT"], name));
                            Logger.logDebug(LogCategory.System, LogActionType.Created, player, $"Found variation with gxt {(string)variant["GXT"]} for drawable {counter} with comonentId {componentIdx} for gender {gender}");
                        });

                        counter2++;
                    }

                    counter2 = 0;
                    counter++;
                }
            }

            while(CallbackController.PlayerCallbacks[player.getCharacterId()].Count > 0) {
                Thread.Sleep(1000);
                Logger.logDebug(LogCategory.System, LogActionType.Event, player, $"Still {CallbackController.PlayerCallbacks[player.getCharacterId()].Count} callbacks in Queue. Waiting for one second!");
            }

            //var list = new List<int>();
            //using(var db = new ChoiceVDb()) {
            //    foreach(var entry in dic) {
            //        var split = entry.Key.Split("-");
            //        var drawable = int.Parse(split[0]);
            //        var texture = int.Parse(split[1]);

            //        if(!list.Contains(drawable)) {
            //            list.Add(drawable);

            //            var alreadyEquivalent = db.configclothingpropvariations
            //                .Include(c => c.prop)
            //                .FirstOrDefault(c => c.prop.componentid == componentIdx && c.prop.gender != gender && c.name == entry.Value.Name);


            //            var newProp = new configclothingprop() {
            //                componentid = componentIdx,
            //                drawableid = drawable,
            //                gender = gender,
            //                name = alreadyEquivalent != null ? alreadyEquivalent.prop.name : entry.Value.Name,
            //                notBuyable = 0,
            //                shopType = -1,
            //                price = 0,
            //            };

            //            db.configclothingprops.Add(newProp);
            //        }
            //    }

            //    db.SaveChanges();
            //}

            //using(var db = new ChoiceVDb()) {
            //    configclothingprop dbProp = null;

            //    foreach(var entry in dic) {
            //        var split = entry.Key.Split("-");
            //        var drawable = int.Parse(split[0]);
            //        var texture = int.Parse(split[1]);

            //        if(dbProp == null || dbProp.drawableid != drawable) {
            //            dbProp = db.configclothingprops.Include(c => c.configclothingpropvariations).AsNoTracking().FirstOrDefault(c => c.componentid == componentIdx && c.drawableid == drawable && c.gender == gender);
            //        }

            //        if(dbProp != null) {
            //            var already = dbProp.configclothingpropvariations.FirstOrDefault(v => v.variation == texture);
            //            if(already != null) {
            //                if(already.handNamed != 1) {
            //                    already.name = entry.Value.Name;
            //                    already.gtaGTX = entry.Value.GXT;
            //                }
            //            } else {
            //                var variation = new configclothingpropvariation() {
            //                    propId = dbProp.id,
            //                    variation = texture,
            //                    name = entry.Value.Name,
            //                    gtaGTX = entry.Value.GXT,
            //                    overridePrice = null,
            //                };

            //                db.configclothingpropvariations.Add(variation);
            //            }

            //            Logger.logDebug(LogCategory.System, LogActionType.Created, player, $"Added/Updated variation {texture} for drawable {dbProp.drawableid} with comonentId {dbProp.componentid} for gender {gender}");
            //        }

            //        db.SaveChanges();

            //    }
            //}

            var list = new List<int>();
            using(var db = new ChoiceVDb()) {
                foreach(var entry in dic) {
                    var split = entry.Key.Split("-");
                    var drawable = int.Parse(split[0]);
                    var texture = int.Parse(split[1]);

                    if(!list.Contains(drawable)) {
                        list.Add(drawable);

                        var alreadyEquivalent = db.configclothingvariations
                            .Include(c => c.clothing)
                            .FirstOrDefault(c => c.clothing.componentid == componentIdx && c.clothing.gender != gender && c.name == entry.Value.Name);

                        var nameSplit = entry.Value.Name.Split(" ");
                        var name = entry.Value.Name;
                        if(nameSplit.Length > 1) {
                            name = nameSplit[1];
                        }

                        var newCloth = new configclothing() {
                            componentid = componentIdx,
                            drawableid = drawable,
                            gender = gender,
                            name = alreadyEquivalent != null ? alreadyEquivalent.clothing.name : name,
                            notBuyable = 0,
                            clothingType = 0,
                            price = 0,
                            categoryId = 5,
                        };

                        db.configclothings.Add(newCloth);
                    }
                }

                db.SaveChanges();
            }


            using(var db = new ChoiceVDb()) {
                configclothing dbCloth = null;

                foreach(var entry in dic) {
                    var split = entry.Key.Split("-");
                    var drawable = int.Parse(split[0]);
                    var texture = int.Parse(split[1]);

                    if(dbCloth == null || dbCloth.drawableid != drawable) {
                        dbCloth = db.configclothings.Include(c => c.configclothingvariations).FirstOrDefault(c => c.componentid == componentIdx && c.drawableid == drawable && c.gender == gender);
                    }

                    if(dbCloth != null) {
                        var already = dbCloth.configclothingvariations.FirstOrDefault(v => v.variation == texture);
                        if(already != null) {
                            if(already.handNamed != 1) {
                                already.name = entry.Value.Name;
                                already.gtaGTX = entry.Value.GXT;
                            }
                        } else {
                            var variation = new configclothingvariation() {
                                clothingId = dbCloth.id,
                                variation = texture,
                                name = entry.Value.Name,
                                gtaGTX = entry.Value.GXT,
                                overridePrice = null,
                            };

                            db.configclothingvariations.Add(variation);
                        }

                        Logger.logDebug(LogCategory.System, LogActionType.Created, player, $"Added/Updated variation {texture} for drawable {dbCloth.drawableid} with comonentId {dbCloth.componentid} for gender {gender}");
                    }

                    db.SaveChanges();

                }
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getCloth")]
    public static void getCloth(IPlayer player, string[] args) {
        try {
            // /getCloth 11 0 0 0
            // /getCloth 11 3 0 Oberteil
            // /getCloth 11 0 0 Sollte_passen
            // /getCloth 11 109 0 Sollte_nicht_passen
            // /getCloth 11 0 0 DLC_Oberteil mp_m_0_custom_pd_clothes

            var item = new ClothingItem(
                InventoryController.getConfigItemForType<ClothingItem>(i => i.additionalInfo == args[0]),
                int.Parse(args[1]),
                int.Parse(args[2]),
                args[3],
                player.getCharacterData().Gender,
                args.Length >= 5 ? args[4] : null);

            player.getInventory().addItem(item);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testDebugInfo")]
    public static void testDebugInfo(IPlayer player, string[] args) {
        try {
            var counter = 0;
            var t = InvokeController.AddTimedInvoke("Test-Debug-Info", (i) => {
                counter++;
                WebController.displayDebugInfo(player, "DEBUG_TEST", $"Test-Debug-Info: {counter}");

                if(counter >= 100) {
                    i.EndSchedule();
                    WebController.removeDebugInfo(player, "DEBUG_TEST");
                }

            }, TimeSpan.FromSeconds(0.5), true);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getDlcCloth")]
    public static void getDlcCloth(IPlayer player, string[] args) {
        try {
            player.sendBlockNotification(player.GetDlcClothes(byte.Parse(args[0])).ToJson(), "");
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("getColShapeStr")]
    public static void getColShapeStr(IPlayer player, string[] args) {
        try {
            CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                var col = CollisionShape.Create(p, w, h, r, true, false);
                var str = col.toShortSave();

                col.Dispose();

                Logger.logFatal(str);

                var dcId = player.getData("DISCORD_ID");
                DiscordController.sendMessageToUser(dcId, $"```{str}```");
            });
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("reloadItems")]
    public static void reloadItems(IPlayer player, string[] args) {
        try {
            InventoryController.loadConfigItems();
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("showLabel")]
    public static void showLabel(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("SHOW_TEXT_LABEL", args, player.Position, 30);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("hideLabels")]
    public static void hideLabels(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("REMOVE_ALL_TEXT_LABELS");
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("startObjectPlacer")] //startObjectPlacer prop_mp_cone_02 0
    public static void objectPlacerMode(IPlayer player, string[] args) {
        try {
            ObjectController.startObjectPlacerMode(player, args[0], float.Parse(args[1]), (p, pos, heading) => {
                ObjectController.createObject(args[0], pos, new DegreeRotation(0, 0, heading));
            });
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("mapPolygonCreator")]
    public static void mapPolygonCreator(IPlayer player, string[] args) {
        try {
            MapPolygonCreator.startPolygonCreationWithCallback(player, (points) => {

            });
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testAudio")]
    public static void testAudio(IPlayer player, string[] args) {
        //0.2 == 15
        //35 == 0.75
        try {
            SoundController.playSoundAtCoords(player.Position + new Position(0, 0, 10), 20, SoundController.Sounds.Airport1, float.Parse(args[0]), "mp3");
            InvokeController.AddTimedInvoke("", (i) => {
                SoundController.playSoundAtCoords(player.Position - new Position(0, 0, 10), 20, SoundController.Sounds.Airport2, float.Parse(args[1]), "mp3");
            }, TimeSpan.FromSeconds(1), false);
            //player.emitClientEvent("PLAY_SOUND_AT_POS",
            //    "http://choicev-cef.net/cef/sounds/ShipHorn.ogg",
            //    new Position(-123, -2404, 6), 20, 0.001);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("testZone")]
    public static void testZone(IPlayer player, string[] args) {
        try {
            var zone = new SoundEventZone("Test-Zone", CollisionShape.Create(player.Position, 3f, 2f, 0, true, false, true), player.Position);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("removeHunger")]
    public static void removeHunger(IPlayer player, string[] args) {
        try {
            player.getCharacterData().Hunger -= int.Parse(args[0]);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }
    
    [Command("newOutfits")]
    public static void newOutfits(IPlayer player, string[] args) {
        try {
            using(var db = new ChoiceVDb()) {
                foreach(var outfit in db.configoutfits.ToList()) {
                    var feetVar = db.configclothingvariations.Include(v => v.clothing)
                        .FirstOrDefault(v => v.clothing.componentid == 6 && v.clothing.drawableid == outfit.feet_drawable && v.variation == outfit.feet_texture);
                   
                    var legVar = db.configclothingvariations.Include(v => v.clothing)
                        .FirstOrDefault(v => v.clothing.componentid == 4 && v.clothing.drawableid == outfit.legs_drawable && v.variation == outfit.legs_texture);
                    
                    var accVar = db.configclothingvariations.Include(v => v.clothing)
                        .FirstOrDefault(v => v.clothing.componentid == 7 && v.clothing.drawableid == outfit.accessoire_drawable && v.variation == outfit.accessoire_texture);
                    
                    var shirtVar = db.configclothingvariations.Include(v => v.clothing)
                        .FirstOrDefault(v => v.clothing.componentid == 8 && v.clothing.drawableid == outfit.shirt_drawable && v.variation == outfit.shirt_texture);
                    
                    var topVar = db.configclothingvariations.Include(v => v.clothing)
                        .FirstOrDefault(v => v.clothing.componentid == 11 && v.clothing.drawableid == outfit.top_drawable && v.variation == outfit.top_texture);

                    var newClothingSet = new configclothingset {
                        name = outfit.name,
                        gender = outfit.gender,
                        feetId = feetVar?.clothingId,
                        feetVariation = feetVar?.variation,
                        legsId = legVar?.clothingId,
                        legsVariation = legVar?.variation,
                        accessoireId = accVar?.clothingId,
                        accessoireVariation = accVar?.variation,
                        shirtId = shirtVar?.clothingId,
                        shirtVariation = shirtVar?.variation,
                        topId = topVar?.clothingId,
                        topVariation = topVar?.variation,
                    };

                    db.configclothingsets.Add(newClothingSet);
                }
                db.SaveChanges();
            }
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("updateMinijobsColshapes")]
    public static void updateMinijobsColshapes(IPlayer player, string[] args) {
        using(var db = new ChoiceVDb()) {
            foreach(var task in db.configminijobstasks.Include(s => s.configminijobstaskssettings)) {
                try {
                    string pattern = @"(?<=ColShapeStr\"":\""{\\\""X\\\"":-?\d+.\d+,\\\""Y\\\"":-?\d+.\d+,\\\""Z\\\"":)(-?\d+.\d+)(?=\})";

                    var parse = task.configminijobstaskssettings.ToList().ToDictionary(s => s.name, s => s.value);
                    foreach(var entry in parse) {
                        MatchEvaluator evaluator = match =>
                        {
                            // Increment the Z index by 1
                            double zIndex = double.Parse(match.Value) + 1;
                            return zIndex.ToString();
                        };
                        string replaced = Regex.Replace(entry.Value, pattern, evaluator);

                        if(replaced != entry.Value) {
                            var find = task.configminijobstaskssettings.FirstOrDefault(s => s.name == entry.Key);
                            find.value = replaced;
                        }
                    }
                } catch(Exception _) { }
            }

            db.SaveChanges();
        }
    }

    #region

    private static List<string> vehicleNames = new List<string> {
        "fdlc2 ",
        "fdlcgranger ",
        "fdlchazmat ",
        "fdlcheavy ",
        "fdlcladder ",
        "fdlcrescue ",
        "fdlcsand ",
        "lcpdscout ",
        "fdlctruck ",
        "fdlctruck2 ",
        "lsfdcmd ",
        "lcpdyo ",
        "fdlcamb ",
        "fdlcamb2 ",
        "fdlcamb3 ",
        "fdlcambold ",
        "fdlcboxville ",
        "fdlcsteed ",
        "polnspeedo ",
        "polalamop2 ",
        "polalamop",
        "polbisonp ",
        "polbuffalop ",
        "polbuffalop2 ",
        "polcarap ",
        "polfugitivep ",
        "polscoutp ",
        "polstanierp ",
        "schlagenpol ",
        "bf400pol ",
        "gauntlet4pol ",
        "dubstafib ",
        "lcpd6 ",
        "lcpd7 ",
        "lcpdranch ",
        "lcpdmerit ",
        "lctrash ",
        "lctrash2 ",
        "boxville ",
        "phantom",
        "flatbed3 ",
        "utillitruck3b ",
        "oracxsle ",
        "towtruck ",
        "tiptruck2 ",
        "fdlcbastion ",
        "pressuv ",
        "akumac ",
        "nightblade2 ",
        "Polstalkerp ",
        "lcbus ",
        "lctaxi ",
        "lctaxi2 ",
        "lctaxi4 ",
        "cabby ",
        "lctaxiold ",
        "trubuffallo ",
        "trubuffallo2 ",
        "umkbuffalo ",
        "ambulance2 ",
        "nspeedo ",
        "boxville ",
        "phantom ",
        "flatbed3 ",
        "safeteam ",
        "utillitruck3b ",
        "Mule ",
        "bisonutil ",
        "Hauler3 ",
        "pony ",
        "Stockade ",
        "tvtrailer ",
        "trailer2 ",
        "trailer ",
        "benson2 ",
        "boxvilleretro ",
        "newsvan2 ",
        "trailer2a ",
        "trailer3 ",
        "taco ",
        "lcpdpredator ",
        "lcpdalamo ",
        "froggersl ",
        "lcpdspeedo ",
        "Kamachofib ",
        "sabregt2fib ",
        "Annihilator ",
        "valkyrie3 ",
        "sr650fly ",
        "zodiac ",
        "yacht2 ",
        "maverick2 ",
        "blimp2 ",
        "mammatus2 ",
        "fbic3 ",
        "fibw ",
        "fibt ",
        "fibs2 ",
        "fibo ",
        "fibo2 ",
        "fibj2 ",
        "fibg3 ",
        "fibf ",
        "fibd3 ",
        "fibdc2 ",
        "fibb2 ",
        "fibb ",
        "fibh ",
        "fibk ",
        "fibk2 ",
        "fibr2 ",
        "fibx2 ",
        "fibs3 ",
        "fibg ",
        "fibd ",
        "fibs ",
        "fibc ",
        "fibg2 ",
        "fibd2 ",
        "fibj ",
        "fibn ",
        "fibr ",
        "newsmav ",
        "elegy3",
        "froggersl",
        "uh1nasa",
        "bcfdbat",
        "caracarafib",
        "caracaramd",
        "caracarapol",
        "caracarapol2",
        "caracararanger",
        "fibc2",
        "fibc3",
        "fibc4",
        "fibd",
        "fibg",
        "fibt2",
        "fibw",
        "fibx",
        "polcoquettep",
        "polgauntletp",
        "polgresleyp",
        "policeb1",
        "policeb2",
        "polroamerp",
        "polspeedop",
        "poltorencep",
        "trubuffalo",
        "trubuffalo2",
        "airbus",
        "bus",
        "fbi2",
        "polnspeedo",
        "va_fdlcladder",
        "fdlc",
        "fdlcforklift",
        "lcsbus",
        "lcmav",
        "lcpd",
        "lcpd2",
        "lcpd5",
        "lcpd4",
        "lcpd3",
        "lcpdb",
        "lcpdbob",
        "lcpdboxville",
        "lcpdbtrailer",
        "lcpdesp",
        "lcpdmav",
        "lcpdfaggio",
        "lcpdold",
        "lcpdpanto",
        "lcpdpatriot",
        "lcpdpigeon",
        "lcpdprem",
        "lcpdriata",
        "lcpdsand",
        "lcpdsparrow",
        "lcpdshark",
        "lcpdspeedo",
        "lcpdstockade",
        "lcpdtru",
        "lcpdtruck",
        "lctaxibr",
        "lctaxiprem",
        "mrtasty",
        "napc",
        "nboxville",
        "tourmav",
        "subway",
        "nstockade",
        "trailers2",
        "trailers2a",
        "trailers3",
        "trailers4"
        };

    #endregion

    [Command("changeVehicleDbModelNames")]
    public static void changeVehicleDbModelNames(IPlayer player, string[] args) {
        using(var db = new ChoiceVDb()) {
            foreach(var name in vehicleNames) {
                var model = db.configvehiclesmodels.FirstOrDefault(v => v.ModelName == ChoiceVAPI.Hash(name.Trim()).ToString());

                if(model != null) {
                    model.ModelName = name.Trim();
                } else {
                    Logger.logError($"Could not find model with name {name.Trim()}", "");
                }
            }

            db.SaveChanges();

            var i = 1;
        }
    }


    [Command("showMessage")]
    public static void showMessage(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("SHOW_WASTED_SCREEN", args[0], string.Join(" ", args.Skip(1)));
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    [Command("stopMessage")]
    public static void stopMessage(IPlayer player, string[] args) {
        try {
            player.emitClientEvent("STOP_WASTED_SCREEN");
            var vc = Alt.CreateVoiceChannel(true, 10);
        } catch(Exception e) {
            Logger.logException(e);
        }
    }
    
    [Command("evaluateExpression")]
    public static void evaluateExpression(IPlayer player, string[] args) {
        try {
            var parser = new MathParser();
            
            var result = parser.Parse(string.Join(" ", args));
            player.sendNotification(NotifactionTypes.Success, $"Ergebnis: {result}", "");
        } catch(Exception e) {
            Logger.logException(e);
        }
    }

    public record TrashItem(string Name, Position Position, Position Rotation);
    [Command("importTrash")]
    public static void importTrash(IPlayer player, string[] args) {
        try {
            var json = System.IO.File.ReadAllText(args[0]);              
            var list = JsonConvert.DeserializeObject<List<TrashItem>>(json);

            using(var db = new ChoiceVDb()) {
                var counter = 0;
                foreach(var item in list) {
                    var rotation = new Rotation(item.Rotation.X, item.Rotation.Y, item.Rotation.Z);
                    db.configtrashcans.Add(new configtrashcan {
                        objectName = item.Name,
                        position = item.Position.ToJson(),
                        rotation = rotation.ToJson(),
                    });
                    counter++;
                    
                    if(counter > 100) {
                        db.SaveChanges();
                        counter = 0;
                        Logger.logDebug(LogCategory.System, LogActionType.Created, player, "Saved 100 trash items");
                    }
                }    
            }
        } catch(Exception e) { 
            Logger.logException(e);
        }
    }

    [Command("regionTrash")]
    public static void regionTrash(IPlayer player, string[] args) {
        var regionCounter = new Dictionary<string, int>();
        using(var db = new ChoiceVDb()) {
            var bigZone = db.configzonegroupings.Include(c => c.configzonegroupingszones).ToList();
            foreach(var can in db.configtrashcans) {
                var pos = can.position.FromJson<Position>();
                var region = WorldController.getRegionName(pos); 
                var bZone = bigZone.FirstOrDefault(b => b.configzonegroupingszones.Any(z => z.gtaName == region));
                if(regionCounter.ContainsKey(bZone.groupingName)) {
                    regionCounter[bZone.groupingName]++;
                } else {
                    regionCounter.Add(bZone.groupingName, 1);
                }
            }
        }
    }
    
    [Command("test")]
    public static void test(IPlayer player, string[] args) {
        var anim = AnimationController.getAnimationByName("FISHING");
    }
    
    [Command("setWeaponMult")]
    public static void setWeaponMult(IPlayer player, string[] args) {
        ChoiceVAPI.setPlayerWeaponDamageMult(player, args[0], float.Parse(args[1]));
    }


    [Command("lagTest")]
    public static void lagTest(IPlayer player, string[] args) {
        var objectCounter = int.Parse(args[0]);

        if (objectCounter > 0) {
            var oC = 0;
            var objectList = new List<Controller.Object>();
            InvokeController.AddTimedInvoke("Lag-Test", (i) => {
                if(oC >= objectCounter) {
                    i.EndSchedule();
                    return;
                }

                oC++;

                if(objectList.Count > 0) {
                    foreach(var o in objectList) {
                        ObjectController.deleteObject(o);
                    }
                    objectList.Clear();
                }
                var rand = new Random();
                var obj = ObjectController.createObject("prop_npc_phone", new Position(rand.Next(-5000, 5000), rand.Next(-5000, 5000), 70), new DegreeRotation(0, 0, 0));

                objectList.Add(obj);
            }, TimeSpan.FromMilliseconds(10), true);
        }
    }

    [Command("getHeapSnapshot")]
    public static void getHeapSnapshot(IPlayer player, string[] args) {
        SupportController.takeHeapSnapshot(player, int.Parse(args[0]));
    }
    
    [Command("getCompanyId")]
    public static void getCompanyId(IPlayer player, string[] args) {
        var cfg = InventoryController.getConfigItemForType<CompanyIdCard>(i => i.additionalInfo == args[0]);
        var company = CompanyController.getCompanyById(int.Parse(args[1]));
        var item = new CompanyIdCard(cfg, "lspd", player, company);
        
        player.getInventory().addItem(item, true);
    }
    
    [Command("createMarker")]
    public static void createMarker(IPlayer player, string[] args) {
        MarkerController.createMarker(1, player.Position, new Rgba(25, 36, 177, 255), 1, 1000, [player]);
    }

    [Command("openCraftingMenu")]
    public static void openCraftingMenu(IPlayer player, string[] args) {
        var menu = CraftingController.getCraftingMenu(player, new List<CraftingTransformations> {
            CraftingTransformations.Combining,
            CraftingTransformations.Cutting
        });
        
        player.showMenu(menu);
    }
    
    [Command("createCraftingSpot")]
    public static void createCraftingSpot(IPlayer player, string[] args) {
        CraftingController.createCraftingTimedSpot(0, "Support-Spot", 30, null, [CraftingTransformations.Combining, CraftingTransformations.Cutting]);
    }
    
    [Command("openCraftingSpot")]
    public static void openCraftingSpot(IPlayer player, string[] args) {
        CraftingController.openCraftingSpotMenu(player, int.Parse(args[0]));
    }
    
    [Command("removeCraftingSpot")]
    public static void removeCraftingSpot(IPlayer player, string[] args) {
        CraftingController.removeCraftingTimedSpot(int.Parse(args[0]));
    }

    private static string TestSound;
    [Command("testSound")]
    public static void testSound(IPlayer player, string[] args) {
        TestSound = SoundController.playSoundAtCoords(player.Position, 5, Sounds.Bell, 1, "ogg", true);
    }
    
    [Command("stoptestSound")]
    public static void stoptestSound(IPlayer player, string[] args) {
        SoundController.stopSound(TestSound);
    }
}