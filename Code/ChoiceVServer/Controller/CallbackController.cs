using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ChoiceVServer.Controller;

public class CallbackController : ChoiceVScript {
    public enum Materials : uint {
        Blood = 5236042,
        Temp30 = 13626292,
        MetalHollowSmall = 15972667,
        Spine3 = 32752644,
        Temp11 = 47470226,
        Temp10 = 63305994,
        HandLeft = 113101985,
        Cloth = 122789469,
        MetalChainlinkLarge = 125958708,
        WoodSolidPolished = 126470059,
        RockNoinst = 127813971,
        CarpetSolidDusty = 158576196,
        Marsh = 223086562,
        CardboardSheet = 236511221,
        GlassBulletproof = 244521486,
        Tarmac = 282940568,
        AnimalDefault = 286224918,
        Linoleum = 289630530,
        MudPothole = 312396330,
        VfxMetalFlame = 332778253,
        ConcretePothole = 359120722,
        Water = 435688960,
        Temp23 = 465002639,
        Paper = 474149820,
        Buttocks = 483400232,
        SandDryDeep = 509508168,
        SandCompact = 510490462,
        CarGlassOpaque = 513061559,
        WoodSolidMedium = 555004797,
        ClaySoft = 560985072,
        Cobblestone = 576169331,
        Bushes = 581794674,
        SandstoneSolid = 592446772,
        CarGlassMedium = 602884284,
        PhysPooltableSurface = 605776921,
        VfxMetalWaterTower = 611561919,
        PlasticHollow = 627123000,
        ShinLeft = 652772852,
        CarpetSolid = 669292054,
        SlattedBlinds = 673696729,
        Temp12 = 702596674,
        WoodOldCreaky = 722686013,
        Temp01 = 746881105,
        MetalSolidLarge = 752131025,
        MetalChainlinkSmall = 762193613,
        Stone = 765206029,
        PlasticHollowClear = 772722531,
        Foam = 808719444,
        WoodSolidLarge = 815762359,
        MetalCorrugatedIron = 834144982,
        FreshMeat = 868733839,
        Temp27 = 889255498,
        SandWet = 909950165,
        GlassShootThrough = 937503243,
        GravelSmall = 951832588,
        PhysPooltableCushion = 972939963,
        VfxWoodBeerBarrel = 998201806,
        Puddle = 999829011,
        Temp17 = 1011960114,
        Temp09 = 1026054937,
        LowerArmLeft = 1045062756,
        EmissivePlastic = 1059629996,
        Temp06 = 1061250033,
        CarGlassStrong = 1070994698,
        Temp29 = 1078418101,
        MudDeep = 1109728704,
        ClayHard = 1144315879,
        WoodChipboard = 1176309403,
        Concrete = 1187676648,
        CarGlassWeak = 1247281098,
        SandWetDeep = 1288448767,
        Grass = 1333033863,
        FeatherPillow = 1341866303,
        Temp08 = 1343679702,
        PhysCarVoid = 1345867677,
        Fibreglass = 1354180827,
        Temp18 = 1354993138,
        Tvscreen = 1429989756,
        BushesNoinst = 1441114862,
        Spine2 = 1457572381,
        GlassOpaque = 1500272081,
        EmissiveGlass = 1501078253,
        UpperArmRight = 1501153539,
        SnowTarmac = 1550304810,
        MarshDeep = 1584636462,
        SnowDeep = 1619704960,
        MudSoft = 1635937914,
        Brick = 1639053622,
        PhysNoFriction = 1666473731,
        Neck = 1718294164,
        RoofTile = 1755188853,
        MetalDuct = 1761524221,
        LowerArmRight = 1777921590,
        Laminate = 1845676458,
        MetalHollowMedium = 1849540536,
        TarmacPothole = 1886546517,
        PavingSlab = 1907048430,
        Temp03 = 1911121241,
        SandstoneBrittle = 1913209870,
        Temp04 = 1923995104,
        GravelTrainTrack = 1925605558,
        FootLeft = 1926285543,
        Marble = 1945073303,
        Temp25 = 1952288305,
        Temp24 = 1963820161,
        WoodHollowSmall = 1993976879,
        HandRight = 2000961972,
        WoodLattice = 2011204130,
        ConcretePavement = 2015599386,
        PhysCasterRusty = 2016463089,
        MetalRailing = 2100727187,
        GravelLarge = 2128369009,
        CarSofttopClear = 2130571536,
        CarPlastic = 2137197282,
        WoodHighFriction = 2154880249,
        StuntRampSurface = 2206792300,
        Plastic = 2221655295,
        Temp20 = 2242086891,
        PhysDynamicCoverBound = 2247498441,
        Leaves = 2253637325,
        PhysElectricMetal = 2281206151,
        Temp02 = 2316997185,
        MudHard = 2352068586,
        SnowLoose = 2357397706,
        IceTarmac = 2363942873,
        Spine0 = 2372680412,
        CarEngine = 2378027672,
        TreeBark = 2379541433,
        SandTrack = 2387446527,
        DirtTrack = 2409420175,
        PlasticClear = 2435246283,
        Hay = 2461440131,
        Default = 2519482235,
        Temp07 = 2529443614,
        Polystyrene = 2538039965,
        WoodHighDensity = 2552123904,
        CarGlassBulletproof = 2573051366,
        PhysGolfBall = 2601153738,
        Temp13 = 2657481383,
        Petrol = 2660782956,
        PlasticHighDensity = 2668971817,
        Perspex = 2675173228,
        SandLoose = 2699818980,
        Temp15 = 2710969365,
        ClavicleRight = 2737678298,
        PhysBarbedWire = 2751643840,
        Temp16 = 2782232023,
        ClavicleLeft = 2825350831,
        MetalSolidSmall = 2847687191,
        DriedMeat = 2849806867,
        RoofFelt = 2877802565,
        CardboardBox = 2885912856,
        CarpetFloorboard = 2898482353,
        Temp05 = 2901304848,
        FootRight = 2925830612,
        PlasticHighDensityClear = 2956494126,
        TarmacPainted = 2993614768,
        GrassShort = 3008270349,
        Ceramic = 3108646581,
        Temp28 = 3115293198,
        PhysElectricFence = 3124923563,
        BrickPavement = 3147605720,
        Spine1 = 3154854427,
        SandUnderwater = 3158909604,
        Temp26 = 3178714198,
        ConcreteDusty = 3210327185,
        Temp21 = 3257211236,
        CarSofttop = 3315319434,
        BreezeBlock = 3340854742,
        WoodHollowLarge = 3369548007,
        Twigs = 3381615457,
        SnowCompact = 3416406407,
        Rock = 3454750755,
        Temp19 = 3493162850,
        Ice = 3508906581,
        RubberHollow = 3511032624,
        FibreglassHollow = 3528912198,
        MetalManhole = 3539969597,
        WoodFloorDusty = 3545514974,
        PhysPooltableBall = 3546625734,
        Head = 3559574543,
        MetalSolidRoadSurface = 3565854962,
        Soil = 3594309083,
        VfxMetalSteam = 3603690002,
        Temp14 = 3649011722,
        Tarpaulin = 3652308448,
        Oil = 3660485991,
        Temp22 = 3674578943,
        MetalHollowLarge = 3711753465,
        PlasterSolid = 3720844863,
        Leather = 3724496396,
        UpperArmLeft = 3784624938,
        GrassLong = 3833216577,
        ThighLeft = 3834431425,
        ShinRight = 3848931141,
        MetalGrille = 3868849285,
        WoodSolidSmall = 3895095068,
        MetalSolidMedium = 3929336056,
        WoodHollowMedium = 3929491133,
        GravelDeep = 3938260814,
        VfxMetalElectrified = 3985833031,
        Woodchips = 3985845843,
        PhysPedCapsule = 4003336261,
        MudUnderwater = 4021477129,
        PhysTennisBall = 4038262533,
        PlasterBrittle = 4043078398,
        RumbleStrip = 4044799021,
        ThighRight = 4057986041,
        PhysCaster = 4059664613,
        MetalGarageDoor = 4063706601,
        Rubber = 4149231379,
        RockMossy = 4170197704,
        CarMetal = 4201905313,
    }

    public enum PlayerCallbackTypes {
        MiloResponse = 0,
        MaterialPlayerIsStandingOn = 1,
        PlayerRegion = 2,
        GroundZ = 3,
        PlayerLastDamagedBone = 4,
        GetNearbyObjects = 5,
        PositionInFront = 6,
        PlayerCameraHeading = 7,
        GetTextureVariations = 8,
        GetLocalizedName = 9,
        GetWaterInfo = 10,
        GetWaypointPosition = 11,
        GetObjectsInFront = 12,

        VehicleGetMods = 101,
        VehicleGetInfo = 102,
        VehicleGetDamage = 103,

        ObjectPlacerMode = 204,
    }

    public delegate void PlayerCallbackAnswerDelegate(IPlayer player, object[] clientArgs);
    public class PlayerCallbackPlan {
        public PlayerCallbackTypes Type { get; private set; }
        public string RequestEvent { get; private set; }

        public PlayerCallbackPlan(PlayerCallbackTypes type, string requestEvent) {
            Type = type;
            RequestEvent = requestEvent;
        }
    }

    public record PlayerCallbackEntry(int Id, IPlayer Player, PlayerCallbackAnswerDelegate Callback);

    private static Dictionary<PlayerCallbackTypes, PlayerCallbackPlan> PlayerCallbackPlans = new();

    private static object CallbackMutex = new();
    private static int CallbackId = 0;
    public static Dictionary<int, List<PlayerCallbackEntry>> PlayerCallbacks = new();

    public CallbackController() {
        EventController.PlayerDisconnectedDelegate += onPlayerDisconnect;

        EventController.addEvent("ON_ANSWER_CALLBACK", onCallbackAnswered);

        registerCallback(PlayerCallbackTypes.MiloResponse, "TEST_FOR_MILO_NAMES");
        registerCallback(PlayerCallbackTypes.MaterialPlayerIsStandingOn, "TEST_FOR_GROUND_MATERIAL");
        registerCallback(PlayerCallbackTypes.PlayerRegion, "TEST_FOR_REGION");
        registerCallback(PlayerCallbackTypes.GroundZ, "TEST_FOR_GROUND_Z");
        registerCallback(PlayerCallbackTypes.PlayerLastDamagedBone, "TEST_DAMAGED_BONE");
        registerCallback(PlayerCallbackTypes.GetNearbyObjects, "TEST_FOR_NEARBY_OBJECTS");
        registerCallback(PlayerCallbackTypes.PositionInFront, "TEST_FOR_POSITION_IN_FRONT");
        registerCallback(PlayerCallbackTypes.PlayerCameraHeading, "TEST_FOR_CAMERA_HEADING");
        registerCallback(PlayerCallbackTypes.GetTextureVariations, "TEST_TEXTURE_VARIATION");
        registerCallback(PlayerCallbackTypes.GetLocalizedName, "TEST_LOCALIZED_NAME");
        registerCallback(PlayerCallbackTypes.GetWaterInfo, "TEST_FOR_WATER");
        registerCallback(PlayerCallbackTypes.GetWaypointPosition, "TEST_FOR_WAYPOINT");
        registerCallback(PlayerCallbackTypes.GetObjectsInFront, "TEST_FOR_OBJECTS_IN_FRONT");
        
        registerCallback(PlayerCallbackTypes.VehicleGetMods, "GET_VEHICLE_MODS_REQUEST");
        registerCallback(PlayerCallbackTypes.VehicleGetInfo, "TEST_VEHICLE_INFO");
        registerCallback(PlayerCallbackTypes.VehicleGetDamage, "TEST_VEHICLES_DAMAGE");

    }

    #region General Setup

    public static void registerCallback(PlayerCallbackTypes type, string requestEvent) {
        PlayerCallbackPlans.Add(type, new PlayerCallbackPlan(type, requestEvent));
    }

    private static bool onCallbackAnswered(IPlayer player, string eventName, object[] args) {
        var id = Convert.ToInt32(args[0].ToString());

        var callbacks = PlayerCallbacks[player.getCharacterId()];
        var callback = callbacks.FirstOrDefault(c => c.Id == id);

        callbacks.Remove(callback);
        callback.Callback.Invoke(player, args.Skip(1).ToArray());

        return true;
    }

    public static void onPlayerCallback(PlayerCallbackTypes type, IPlayer player, PlayerCallbackAnswerDelegate callback, params dynamic[] clientArgs) {
        lock(CallbackMutex) {
            var plan = PlayerCallbackPlans[type];
            var entry = new PlayerCallbackEntry(CallbackId++, player, callback);

            if(!PlayerCallbacks.ContainsKey(player.getCharacterId())) {
                PlayerCallbacks.Add(player.getCharacterId(), new List<PlayerCallbackEntry> { entry });
            } else {
                var list = PlayerCallbacks[player.getCharacterId()];
                list.Add(entry);
            }

            var newClientArgs = new object[clientArgs.Length + 1];
            newClientArgs[0] = entry.Id;
            Array.Copy(clientArgs, 0, newClientArgs, 1, clientArgs.Length);

            player.emitClientEvent(plan.RequestEvent, newClientArgs);
        }
    }

    private static void onPlayerDisconnect(IPlayer player, string reason) {
        PlayerCallbacks.Remove(player.getCharacterId());
    }

    #endregion

    #region Player

    public static void getPlayerMaterialStandOn(IPlayer player, Action<IPlayer, Materials> callback) {
        onPlayerCallback(PlayerCallbackTypes.MaterialPlayerIsStandingOn, player, (p, args) => {
            var mat = (Materials)long.Parse(args[4].ToString());

            callback.Invoke(player, mat);
        }, player.Position.X, player.Position.Y, player.Position.Z);
    }

    /// <summary>
    /// Returns the region(system name) a player is in
    /// </summary>
    public static void getPlayerRegion(IPlayer player, Action<IPlayer, string> callback) {
        onPlayerCallback(PlayerCallbackTypes.PlayerRegion, player, (p, args) => {
            var region = args[0].ToString();

            callback.Invoke(player, region);
        }, player.Position.X, player.Position.Y, player.Position.Z);
    }

    /// <summary>
    /// Returns the region(system name) via player by coords
    /// </summary>
    /// //ToDo merge getPlayerRegion and GetRegion
    public static void getRegion(IPlayer player, Position pos, Action<IPlayer, string> callback) {
        onPlayerCallback(PlayerCallbackTypes.PlayerRegion, player, (p, args) => {
            var region = args[0].ToString();

            callback.Invoke(player, region);
        }, pos.X, pos.Y, pos.Z);
    }

    public static void getPlayerInInterior(IPlayer player, string miloName, Action<IPlayer, bool, string> callback) {
        onPlayerCallback(PlayerCallbackTypes.MiloResponse, player, (p, args) => {
            var found = args[0].ToString() == "1";
            var milo = args[1].ToString();

            callback.Invoke(player, found, milo);
        }, player.Position.X, player.Position.Y, player.Position.Z, new string[] { miloName });
    }

    public static void getPlayerInInterior(IPlayer player, string[] miloNames, Action<IPlayer, bool, string> callback) {
        onPlayerCallback(PlayerCallbackTypes.MiloResponse, player, (p, args) => {
            var found = args[0].ToString() == "1";
            var milo = args[1].ToString();

            callback.Invoke(player, found, milo);
        }, player.Position.X, player.Position.Y, player.Position.Z, miloNames);
    }

    /// <summary>
    /// Returns the z value where something is on the ground properly. (Considering Buildings etc.!)
    /// </summary>
    public static void getGroundZFromPos(IPlayer player, Position position, Action<IPlayer, float, bool, bool> callback) {
        onPlayerCallback(PlayerCallbackTypes.GroundZ, player, (p, args) => {
            var z = float.Parse(args[0].ToString());
            var inFrontOfWall = args[1].ToString() == "1";
            var probablyInObject = args[2].ToString() == "1";

            callback.Invoke(player, z, inFrontOfWall, probablyInObject);
        }, position.X, position.Y, position.Z);
    }

    /// <summary>
    /// Returns the last damaged bone of a player
    /// </summary>
    public static void getPlayerLastDamagedBone(IPlayer player, Action<IPlayer, int> callback) {
        onPlayerCallback(PlayerCallbackTypes.PlayerLastDamagedBone, player, (p, args) => {
            var bone = int.Parse(args[0].ToString());

            callback.Invoke(player, bone);
        });
    }

    /// <summary>
    /// Returns obj was found?, modelHash, modelPosition, modelHeading
    /// </summary>
    public static void getNearbyObject(IPlayer player, Position pos, float radius, List<uint> objHashes, Action<IPlayer, bool, uint, Position, float> callback) {
        onPlayerCallback(PlayerCallbackTypes.GetNearbyObjects, player, (p, args) => {
            if(args[0] != null) {
                var hash = uint.Parse(args[0].ToString());
                var pos = args[1].ToString().FromJson<Position>();
                var heading = float.Parse(args[2].ToString());

                callback.Invoke(player, true, hash, pos, heading);
            } else {
                callback.Invoke(player, false, 0, Position.Zero, 0);
            }
        }, pos.X, pos.Y, pos.X, radius, objHashes);
    }

    /// <summary>
    /// Gets Position in Front ON OBJECTS (Tables, Rocks, etc.)
    /// </summary>
    public static void getPositionInFront(IPlayer player, float multiplier, Action<IPlayer, Position> callback) {
        onPlayerCallback(PlayerCallbackTypes.PositionInFront, player, (p, args) => {
            var pos = args[0].ToString().FromJson();

            callback.Invoke(player, pos);
        }, multiplier);
    }

    /// <summary>
    /// Gets Rotation of the player camera
    /// </summary>
    public static void getPlayerCameraHeading(IPlayer player, Action<IPlayer, float> callback) {
        onPlayerCallback(PlayerCallbackTypes.PlayerCameraHeading, player, (p, args) => {
            var heading = float.Parse(args[0].ToString());

            callback.Invoke(player, heading);
        });
    }

    /// <summary>
    /// Get Texture Variation of a drawable
    /// </summary>
    public static void getTextureVariations(IPlayer player, int componentId, int drawableId, bool isClothingNotAccessoire, Action<IPlayer, int> callback) {
        onPlayerCallback(PlayerCallbackTypes.GetTextureVariations, player, (p, args) => {
            var textures = int.Parse(args[0].ToString());

            callback.Invoke(player, textures);
        }, componentId, drawableId, isClothingNotAccessoire);
    }

    /// <summary>
    /// Get Texture Variation of a drawable
    /// </summary>
    public static void getGXTLocalizedName(IPlayer player, string gxt, string data, Action<IPlayer, string, string> callback) {
        onPlayerCallback(PlayerCallbackTypes.GetLocalizedName, player, (p, args) => {
            var name = args[0].ToString();
            var data = args[1].ToString();

            callback.Invoke(player, name, data);
        }, gxt, data);
    }

    /// <summary>
    /// Get Water Info FromPosition
    /// </summary>
    public static void getWaterInfo(IPlayer player, Position position, Action<IPlayer, bool, double?, double?> callback) {
        onPlayerCallback(PlayerCallbackTypes.GetWaterInfo, player, (p, args) => {
            var isWater = bool.Parse(args[0].ToString());
            var waterHeight = (double?)args[1];
            var groundHeight = (double?)args[2];

            callback.Invoke(player, isWater, waterHeight, groundHeight);
        }, position);
    }


    /// <summary>
    /// Get Waypoint Position
    /// </summary>
    /// <param name="player"></param>
    public static void getPlayerWaypoint(IPlayer player, Action<IPlayer, Position> callback) {
        onPlayerCallback(PlayerCallbackTypes.GetWaypointPosition, player, (p, args) => {
            var pos = (Position)args[0];

            callback.Invoke(player, pos);
        }, player.Position.X, player.Position.Y, player.Position.Z);
    }
    
    
    public record ObjectInFront(uint modelHash, Position position, Position offset, float heading, bool isBroken);
    public static void getObjectsInFront(IPlayer player, Action<IPlayer, List<ObjectInFront>> callback) {
        onPlayerCallback(PlayerCallbackTypes.GetObjectsInFront, player, (p, data) => {
            var objects = (string)data[0];
            
            callback.Invoke(p, objects.FromJson<List<ObjectInFront>>());
        });
    }

    #endregion Player

    #region Vehicle

    public static void getVehicleMods(IPlayer player, ChoiceVVehicle vehicle, string vehicleModKitsJson, Action<IPlayer, ChoiceVVehicle, string> callback) {
        onPlayerCallback(PlayerCallbackTypes.VehicleGetMods, player, (p, args) => {
            var vehicle = (ChoiceVVehicle)args[0];
            var json = (string)args[1];

            callback.Invoke(player, vehicle, json);
        }, vehicle, vehicleModKitsJson);
    }

    /// <summary>
    /// Return Vehicle Information containing: classId, DimensionCorner1, DimensionCorner2, Extras, SeatAmount, RelativeSeat List, windowCount, doorCount, tyreCount
    /// </summary>
    public static void getVehicleInfo(IPlayer player, ChoiceVVehicle vehicle, Action<IPlayer, ChoiceVVehicle, int, Position, Position, Dictionary<string, bool>, int, Vector2[], int, int, int, string> callback) {
        onPlayerCallback(PlayerCallbackTypes.VehicleGetInfo, player, (p, args) => {
            var cl = int.Parse(args[0].ToString());
            var pos1 = args[1].ToString().FromJson<Position>();
            var pos2 = args[2].ToString().FromJson<Position>();
            var extras = (args[3].ToString().FromJson<int[]>()).Select(o => int.Parse(o.ToString())).ToDictionary(i => i.ToString(), i => true);
            var seatNumber = int.Parse(args[4].ToString());
            var seatList = args[5].ToString().FromJson<Vector2[]>();
            var windowCount = int.Parse(args[6].ToString());
            var doorCount = int.Parse(args[7].ToString());
            var tyreCount = int.Parse(args[8].ToString());
            var displayName = args[9].ToString();
            var veh = (ChoiceVVehicle)args[10];

            callback.Invoke(player, veh, cl, pos1, pos2, extras, seatNumber, seatList, windowCount, doorCount, tyreCount, displayName);

        }, vehicle);
    }

    /// <summary>
    /// Get Vehicle Damage paramters from client. 
    /// first List is the indexes of destroyed vehicle windowsz
    /// </summary>
    public static void getVehicleDamage(IPlayer player, ChoiceVVehicle vehicle, Action<IPlayer, ChoiceVVehicle, List<bool>> callback) {
        if(player != null) {
            onPlayerCallback(PlayerCallbackTypes.VehicleGetDamage, player, (p, args) => {
                var vehicle = (ChoiceVVehicle)args[1];
                var windowList = args[0].ToString().FromJson<List<bool>>();

                callback.Invoke(player, vehicle, windowList);
            }, vehicle, vehicle.DbModel.windowCount);
        } else {
            var list = new List<bool> { };
            for(var i = 0; i < vehicle.DbModel.windowCount; i++) {
                list.Add(false);
            }
            callback.Invoke(player, vehicle, list);
        }
    }

    #endregion Vehicle
}