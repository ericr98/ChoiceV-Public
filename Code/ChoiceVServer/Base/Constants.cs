using AltV.Net.Data;
using AltV.Net.Enums;
using AltV.Net.Shared;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Base {
    public static class Constants {
        #region ConnectionController

        public static string NO_ACCOUNT_KICK_MESSAGE = "Du hast keinen Account. Registriere dich vorher über den Discord!";
        public static string PLAYER_BANNED_MESSAGE = "Du wurdest für ChoiceV gesperrt. Der angegebene Grund ist: ";
        public static string WRONG_PASSWORD_MESSAGE = "Du hast dein Passwort zu oft falsch eingegeben! Melde dich im Support!";
        public static string TO_MANY_WRONG_LOGINS_MESSAGE = "Dein Account wurde aufgrund von zu vielen fehlgeschlagenen Anmeldeversuchen gesperrt! Melde dich im Support!";
        public static string SUCCESSFULL_LOGIN_MESSAGE = "Du hast dich bei ChoiceV angemeldet. Viel Spaß!";
        public static string NO_CHARACTER_MESSAGE = "Du hast keinen Character!";

        public static int AllowedWrongLogins = 5;

        #endregion

        #region BaseObjectData

        //Entity
        public static string DATA_ENTITY_CURRENT_CHECKPOINTS = "CURRENT_CHECKPOINTS";
        public static string DATA_ENTITY_CURRENT_COLLISIONSHAPE = "CURRENT_COLLISIONSHAPE";

        //Character
        public static string DATA_PLAYER_FULLY_LOADED = "FULLY_LOADED";
        public static string DATA_ACCOUNT_ID = "ACCOUNT_ID";
        public static string DATA_CHARACTER_ID = "CHARACTER_ID";
        public static string DATA_PLAYER_THIRST = "PLAYER_THIRST";
        public static string DATA_PLAYER_HUNGER = "PLAYER_HUNGER";
        public static string DATA_PLAYER_WEAPONS_EQUIPPED = "WEAPONS_EQUIPPED";
        public static string DATA_CHARACTER_MODEL = "CHARACTER_MODEL";
        public static string DATA_CHARACTER_MONEY = "CHARACTER_MONEY";
        public static string DATA_CHARATCER_INTERACTING = "CHARACTER_INTERACTING";
        public static string DATA_CHARACTER_STATE = "CHARACTER_STATE";
        public static string DATA_CHARACTER_PLAYING_ITEM_ANIMATION = "CHARACTER_PLAYING_ITEM_ANIM";

        //Vehicle
        public static string DATA_VEHICLE_ID = "VEHICLE_ID";
        public static string DATA_VEHICLE_INVENTORY = "VEHICLE_INVENTORY";
        public static string DATA_VEHICLE_OBJECT = "VEHICLE_OBJECT";
        public static string DATA_VEHICLE_DAMAGE = "VEHICLE_DAMAGE";
        public static string DATA_VEHICLE_COLORING = "VEHICLE_COLORING";
        public static string DATA_VEHICLE_TUNING = "VEHICLE_TUNING";
        public static string DATA_VEHICLE_MODEL = "VEHICLE_MODEL";
        public static string DATA_VEHICLE_CLASS = "VEHICLE_CLASS";

        //Clothing
        public static string DATA_CLOTHING_SAVE = "CLOTHING_SAVE";
        public static string DATA_CLOTHING_CURRENT = "CLOTHING_CURRENT";
        public static string DATA_CLOTHING_ARMOUR_TEMP = "CLOTHING_ARMOUR_TEMP";

        //Inventory
        public static string DATA_PLAYER_INVENTORY = "PLAYER_INVENTORY";

        //Checkpoint
        public static string ENTITIES_IN_CHECKPOINT = "INSIDE_ENTITY";

        #endregion

        #region ClientEvents

        //To Client

        public static string PlayerShowRegisterScreen = "SHOW_REGISTER_SCREEN";
        public static string PlayerShowLoginScreen = "SHOW_LOGIN_SCREEN";
        public static string PlayerLoginScreenDeactivate = "LOGIN_SCREEN_DEACTIVATE";
        public static string PlayerAnswerBankRequest = "ANSWER_CHOICEVNETBANKDATA";

        public static string PlayerSetClothes = "SET_CLOTHES";
        public static string PlayerSetAccessoire = "SET_ACCESSOIRE";
        public static string PlayerSetDecoration = "SET_PLAYER_DECORATION";
        public static string PlayerResetDecoration = "RESET_PLAYER_DECORATION";
        public static string PlayerSetStyle = "SET_CHARACTER_STYLE";

        public static string PlayerStartCharacterCreation = "START_CHAR_CREATE";

        public static string PlayerShowHud = "SHOW_HUD";
        public static string PlayerUpdateHud = "UPDATE_HUD";

        public static string PlayerCloseMenu = "CLOSE_MENU";
        public static string PlayerCreateMenu = "CREATE_MENU";

        public static string PlayerSetCharacterInfo = "SET_CHARACTER_INFO";
        public static string PlayerSetCharacterHealth = "SET_CHARACTER_HEALTH";

        public static string PlayerGiveWeapon = "GIVE_WEAPON";
        public static string PlayerRemoveWeapon = "REMOVE_WEAPON";
        public static string PlayerGiveWeaponComponent = "GIVE_WEAPON_COMPONENT";
        public static string PlayerRemoveWeaponComponent = "REMOVE_WEAPON_COMPONENT";
        public static string PlayerGiveAmmo = "GIVE_AMMO";
        public static string PlayerSetAmmo = "SET_AMMO";
        public static string PlayerSetArmour = "SET_ARMOUR";

        public static string PlayerInitializeWeaponUnequip = "INITIALIZE_WEAPON_UNEQUIP";
        public static string PlayerInitializeArmourUnequip = "INITIALIZE_ARMOUR_UNEQUIP";

        public static string PlayerCreateObject = "CREATE_OBJECT";
        public static string PlayerDeleteObject = "DELETE_OBJECT";
        public static string PlayerMoveObject = "MOVE_OBJECT";
        public static string PlayerAttachObjectToPlayer = "ATTACH_OBJECT_TO_PLAYER";
        public static string PlayerReattachObjectToPlayer = "REATTACH_OBJECT_TO_PLAYER";
        public static string PlayerRotateObject = "ROTATE_OBJECT";

        public static string PlayerPlayAnimation = "PLAY_ANIM";
        public static string PlayerStopAnimation = "STOP_ANIM";

        public static string PlayerWeatherTransition = "SET_WEATHER_TRANISTION";
        public static string PlayerWeatherMix = "SET_WEATHER_MIX";

        public static string PlayerStartScreenEffect = "START_SCREEN_EFFECT";
        public static string PlayerStopScreenEffect = "STOP_SCREEN_EFFECT";

        public static string PlayerSetHeading = "SET_PLAYER_HEADING";

        public static string PlayerSpawnStaticPed = "SPAWN_STATIC_PED";
        public static string PlayerDestroyStaticPed = "DESTROY_STATIC_PED";

        public static string PlayerCreatePointBlip = "CREATE_POINT_BLIP";
        public static string PlayerCreateRouteBlip = "CREATE_ROUTE_BLIP";
        public static string PlayerCreateAreaBlip = "CREATE_AREA_BLIP";
        public static string PlayerCreateRadiusBlip = "CREATE_RADIUS_BLIP";
        public static string PlayerCreateWaypointBlip = "CREATE_WAYPOINT_BLIP";
        public static string PlayerDestroyBlip = "DESTROY_BLIP";
        public static string PlayerDestroyBlipName = "DESTROY_BLIP_NAME";

        //From Client

        public static string OnPlayerSubmitRegistration = "SUBMIT_REGISTRATION";
        public static string OnPlayerSubmitLogin = "SUBMIT_LOGIN";

        public static string OnPlayerInteractionInventorySpot = "INVENTORY_SPOT";

        public static string OnPlayerSettingHudPosition = "UPDATE_HUD_POSITION";

        public static string OnPlayerMenuItemEvent = "MENU_ITEM_EVENT";

        public static string OnPlayerObjectCreated = "OBJECT_CREATED";
        public static string OnPlayerObjectMoved = "OBJECT_MOVED";

        public static string OnPlayerItemMove = "MOVE_ITEM";
        public static string OnPlayerItemUse = "USE_ITEM";
        public static string OnPlayerItemEquip = "EQUIP_ITEM";
        public static string OnPlayerItemDelete = "DELETE_ITEM";
        public static string OnPlayerCombinationLockAccessed = "COMBINATION_LOCK_ACCESSED";
        #endregion

        #region BankSystem

        //TODO Add Banks
        public enum BankCompanies : int {
            LibertyBank = 0,
            MazeBank = 1,
            FleecaBank = 2
        }

        public static Dictionary<BankCompanies, MenuItemStyle> BankCompaniesToMenuStyle = new Dictionary<BankCompanies, MenuItemStyle> {
            {BankCompanies.FleecaBank, MenuItemStyle.fleecaBank },
            {BankCompanies.LibertyBank, MenuItemStyle.libertyBank },
            {BankCompanies.MazeBank,  MenuItemStyle.mazeBank },
        };

        public static Dictionary<BankCompanies, string> BankCompaniesToName = new Dictionary<BankCompanies, string> {
            {BankCompanies.FleecaBank, "Fleeca Bank" },
            {BankCompanies.LibertyBank, "Liberty Bank" },
            {BankCompanies.MazeBank,  "Maze Bank" },
        };

        public static Dictionary<BankCompanies, string> BankCompaniesToPhoneString = new Dictionary<BankCompanies, string> {
            { BankCompanies.FleecaBank, "fleeca"},
            { BankCompanies.MazeBank, "maze"},
            { BankCompanies.LibertyBank, "liberty" },
        };

        public static Dictionary<BankAccountType, string> BankAccountTypeToName = new Dictionary<BankAccountType, string> {
            { BankAccountType.GiroKonto, "Girokonto"},
            { BankAccountType.DepositKonto, "Kreditkonto" },
            { BankAccountType.CompanyKonto, "Firmenkonto" },
        };

        public enum EmergencyCallType : int {
            Police = 0,
            Medic = 1,
            Fire = 2,
            Towing = 3,
            FBI = 4,
            Purge = 5,
            System = 6,
        }

        public static TimeSpan BANK_DEPOSIT_MIN_TIME = TimeSpan.FromDays(7);
        public static TimeSpan BANK_DEPOSIT_INTEREST_FREQUENCY = TimeSpan.FromDays(7);

        public static float BANK_DEPOSIT_INTEREST_PERCENT = 0.01f;
        public static decimal BANK_DEPOSIT_MAX_INTEREST = new decimal(10_000);

        public static TimeSpan DIFFERENT_BANK_TRANSACTION_TIME = TimeSpan.FromMinutes(30);
        public static decimal DIFFERENT_BANK_TRANSACTION_COST = new decimal(4.5);
        public static decimal DIFFERENT_BANK_TRANSCATION_PERCENT = new decimal(0.005);

        public static decimal ATM_DIFFERENT_BANK_WITHDRAW_COST = new decimal(2.5);
        public static decimal MAX_ATM_ROBBERY_AMOUNT = 10000;

        #endregion

        #region CompanySystem

        public enum JobLevelAllowances : int {
            VaultAccess = 0,
            BankAccess = 1,
            CarKeysAccess = 2,
        }

        #endregion

        #region TaxSystem

        public static TimeSpan COMPANY_TAX_PAYING_TIME = TimeSpan.FromDays(7);
        public static TimeSpan TAX_CHECKING_COMPANY = TimeSpan.FromMinutes(10);
        public static TimeSpan TAX_CHECKING_PLAYERS = TimeSpan.FromMinutes(30);

        public static string COMPANY_TRANSFER_TAX_MESSAGE = "Steuerüberweisung von ";

        public enum TaxIrragulations : int {
            None = 0,
            MoreExpensesThanProfit = 1,
            ProfitHasDoubled = 2,
            ExpensesHasDoubled = 3,
        }

        #endregion

        #region FarmSystem

        public static TimeSpan CHECK_FARM_REGROW = TimeSpan.FromMinutes(10);
        public static float NOT_FERTILIZER_FARM_REGROW = 0.3f;

        public static float MAX_FARM_REGROW_REDUCTION = 0.5f;

        public enum FertilizerTypes : int {
            Nitrogenous = 0,
            OrganicNitrogenous = 1,
            Phosphate = 2,
            Potassic = 3,
            Compound = 4,
            Complete = 5,
        }

        #endregion

        #region GarageController

        public enum GarageType : int {
            None = 0,
            GroundVehicle = 1,
            AirVehicle = 16,
            WaterVehicle = 32,
        }

        public enum GarageOwnerType : int {
            None = 0,
            Player = 1,
            Company = 2,
            Public = 3,
        }

        #endregion

        #region GasstationController

        public enum GasstationSpotType : int {
            CarPetrol = 0,
            CarDiesel = 1,
            CarElecticity = 2,
            Boat = 3,
            PlaneHelicopter = 4,
        }

        public static Dictionary<GasstationSpotType, FuelType> GasstationSpotTypesToFuelType = new Dictionary<GasstationSpotType, FuelType> {
            { GasstationSpotType.CarPetrol, FuelType.Petrol },
            { GasstationSpotType.CarDiesel, FuelType.Diesel },
            { GasstationSpotType.CarElecticity, FuelType.Electricity },
            { GasstationSpotType.Boat, FuelType.Diesel },
            { GasstationSpotType.PlaneHelicopter, FuelType.Kerosin },
        };

        //0 on 60 in 10 min
        public static float FillAmountPerSecond = 0.1f;

        #endregion

        #region WorkshopController

        public enum WorkshopType : int {
            None = 0,
            Car = 1,
            Motorcycle = 2,
            Truck = 4,
            Military = 8,
            Helicopter = 16,
            Boat = 32,
        }

        public enum WorkshopOwnerType : int {
            None = 0,
            Player = 1,
            Company = 2,
            State = 3,
        }

        public enum WorkshopSpotType : int {
            None = 0,
            Repair = 1,
            Coloring = 2,
            Tuning = 3,
        }

        #endregion

        #region HairSystem
        //11 41
        public static List<string> PlayerHairColors = new List<string> {
            "#1c1f21", "#272a2c", "#312e2c", "#35261c", "#4b321f", "#5c3b24", "#6d4c35", "#6b503b", "#765c45", "#7f684e", "#99815d", "#a79369", "#af9c70", "#bba063", "#d6b97b", "#dac38e",
            "#9f7f59", "#845039", "#682b1f", "#61120c", "#640f0a", "#7c140f", "#a02e19", "#b64b28", "#a2502f", "#aa4e2b", "#626262", "#808080", "#aaaaaa", "#c5c5c5", "#463955", "#5a3f6b",
            "#763c76", "#ed74e3", "#eb4b93", "#f299bc", "#04959e", "#025f86", "#023974", "#3fa16a", "#217c61", "#185c55", "#b6c034", "#70a90b", "#439d13", "#dcb857", "#e5b103", "#e69102",
            "#f28831", "#fb8057", "#e28b58", "#d1593c", "#ce3120", "#ad0903", "#880302", "#1f1814", "#291f19", "#2e221b", "#37291e", "#2e2218", "#231b15", "#020202", "#706c66", "#9d7a50", };

        #endregion

        #region VehicleSystem

        public static List<string> VehicleAllColors = new List<string> {
            "#0d1116", "#1c1d21", "#32383d", "#454b4f", "#999da0", "#c2c4c6", "#979a97", "#637380", "#63625c", "#3c3f47", "#444e54", "#1d2129", "#13181f", "#26282a", "#515554", "#151921",
            "#1e2429", "#333a3c", "#8c9095", "#39434d", "#506272", "#1e232f", "#363a3f", "#a0a199", "#d3d3d3", "#b7bfca", "#778794", "#c00e1a", "#da1918", "#b6111b", "#a51e23", "#7b1a22",
            "#8e1b1f", "#6f1818", "#49111d", "#b60f25", "#d44a17", "#c2944f", "#f78616", "#cf1f21", "#732021", "#f27d20", "#ffc91f", "#9c1016", "#de0f18", "#8f1e17", "#a94744", "#b16c51",
            "#371c25", "#132428", "#122e2b", "#12383c", "#31423f", "#155c2d", "#1b6770", "#66b81f", "#22383e", "#1d5a3f", "#2d423f", "#45594b", "#65867f", "#222e46", "#233155", "#304c7e",
            "#47578f", "#637ba7", "#394762", "#d6e7f1", "#76afbe", "#345e72", "#0b9cf1", "#2f2d52", "#282c4d", "#2354a1", "#6ea3c6", "#112552", "#1b203e", "#275190", "#608592", "#2446a8",
            "#4271e1", "#3b39e0", "#1f2852", "#253aa7", "#1c3551", "#4c5f81", "#58688e", "#74b5d8", "#ffcf20", "#fbe212", "#916532", "#e0e13d", "#98d223", "#9b8c78", "#503218", "#473f2b",
            "#221b19", "#653f23", "#775c3e", "#ac9975", "#6c6b4b", "#402e2b", "#a4965f", "#46231a", "#752b19", "#bfae7b", "#dfd5b2", "#f7edd5", "#3a2a1b", "#785f33", "#b5a079", "#fffff6",
            "#eaeaea", "#b0ab94", "#453831", "#2a282b", "#726c57", "#6a747c", "#354158", "#9ba0a8", "#5870a1", "#eae6de", "#dfddd0", "#f2ad2e", "#f9a458", "#83c566", "#f1cc40", "#4cc3da",
            "#4e6443", "#bcac8f", "#f8b658", "#fcf9f1", "#fffffb", "#81844c", "#ffffff", "#f21f99", "#fdd6cd", "#df5891", "#f6ae20", "#b0ee6e", "#08e9fa", "#0a0c17", "#0c0d18", "#0e0d14",
            "#9f9e8a", "#621276", "#0b1421", "#11141a", "#6b1f7b", "#1e1d22", "#bc1917", "#2d362a", "#696748", "#7a6c55", "#c3b492", "#5a6352", "#81827f", "#afd6e4", "#7a6440", "#7f6a48" };

        public static List<string> VehicleDefaultColors = new List<string> {
            "#151921", "#1e2429", "#333a3c", "#8c9095", "#39434d", "#506272", "#1e232f", "#363a3f", "#a0a199", "#d3d3d3", "#b7bfca", "#778794", "#9c1016", "#de0f18", "#8f1e17", "#a94744",
            "#b16c51", "#371c25", "#22383e", "#1d5a3f", "#2d423f", "#45594b", "#65867f", "#112552", "#1b203e", "#275190", "#608592", "#2446a8", "#4271e1", "#3b39e0", "#4c5f81", "#58688e",
            "#74b5d8", "#3a2a1b", "#785f33", "#b5a079", "#b0ab94", "#453831", "#2a282b", "#726c57", "#eae6de", "#dfddd0", "#f2ad2e", "#f9a458", "#f1cc40", "#4cc3da", "#f8b658", "#fffffb",
            "#81844c", "#ffffff", "#f21f99", "#fdd6cd", "#f6ae20", "#b0ee6e", "#08e9fa", "#9f9e8a", "#11141a", "#81827f", "#afd6e4" };

        public static List<string> VehicleMetallicColors = new List<string> {
            "#0d1116", "#1c1d21", "#32383d", "#454b4f", "#999da0", "#c2c4c6", "#979a97", "#637380", "#63625c", "#3c3f47", "#444e54", "#1d2129", "#c00e1a", "#da1918", "#b6111b", "#a51e23",
            "#7b1a22", "#8e1b1f", "#6f1818", "#49111d", "#b60f25", "#d44a17", "#c2944f", "#f78616", "#132428", "#122e2b", "#12383c", "#31423f", "#155c2d", "#1b6770", "#222e46", "#233155",
            "#304c7e", "#47578f", "#637ba7", "#394762", "#d6e7f1", "#76afbe", "#345e72", "#0b9cf1", "#2f2d52", "#282c4d", "#2354a1", "#6ea3c6", "#ffcf20", "#fbe212", "#916532", "#e0e13d",
            "#98d223", "#9b8c78", "#503218", "#473f2b", "#221b19", "#653f23", "#775c3e", "#ac9975", "#6c6b4b", "#402e2b", "#a4965f", "#46231a", "#752b19", "#bfae7b", "#dfd5b2", "#f7edd5",
            "#fffff6", "#eaeaea", "#83c566", "#df5891", "#0c0d18", "#0e0d14", "#621276", "#bc1917", "#0a0c17", "#0b1421", "#7a6440" };

        public static List<string> VehicleMatteColors = new List<string> {
            "#13181f", "#26282a", "#515554", "#cf1f21", "#732021", "#f27d20", "#ffc91f", "#66b81f", "#1f2852", "#253aa7", "#1c3551", "#4e6443", "#bcac8f", "#fcf9f1", "#6b1f7b", "#1e1d22",
            "#2d362a", "#696748", "#7a6c55", "#c3b492", "#5a6352" };

        public static List<string> VehicleMetalColors = new List<string> { "#6a747c", "#354158", "#9ba0a8", "#7f6a48" };

        public static List<string> VehicleChromeColors = new List<string> { "#5870a1" };

        public static List<int> VehicleTypeVehicle = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 9, 12, 18, 22 };
        public static List<int> VehicleTypeFlyable = new List<int> { 15, 16 };

        public static Dictionary<GasstationSpotType, string> GasstationSpotTypesToName = new Dictionary<GasstationSpotType, string> {
            { GasstationSpotType.CarPetrol, "Benzin" },
            { GasstationSpotType.CarDiesel, "Diesel" },
            { GasstationSpotType.CarElecticity, "Strom" },
            { GasstationSpotType.Boat, "Bootsdiesel" },
            { GasstationSpotType.PlaneHelicopter, "Kerosin" },
        };

        public enum SimplePartType : int {
            Window = 0,
            Light = 1,
            Door = 2,
        }

        public enum VehicleBumperState {
            On = 0,
            Bouncing = 1,
            Off = 2,
        }

        // Distance by driving around the shore in a circle: ca. 30k
        public static float ONE_FUEL_TANK_DISTANCE = 75000;
        public static float ONE_DIRT_AQUIRE_DISTANCE = 40000;

        public static float ONE_WHEEL_LIFE_TIME_DISTANCE = 500000; //16 mal um karte

        public static Dictionary<VehiclePart, string> VehiclePartsToName = new Dictionary<VehiclePart, string> {
            {VehiclePart.FrontLeft, "Vorne Links" },
            {VehiclePart.FrontRight, "Vorne Rechts" },
            {VehiclePart.MiddleLeft, "Mitte Links" },
            {VehiclePart.MiddleRight, "Mitte Rechts" },
            {VehiclePart.RearLeft, "Hinten Links" },
            {VehiclePart.RearRight, "Hinten Rechts" },
        };

        //public static Dictionary<int, string> VehicleDoorIdToName = new Dictionary<int, string> {
        //    {0, "Vorne Links" },
        //    {1, "Vorne Rechts" },
        //    {2, "Hinten Links" },
        //    {3, "Hinten Rechts" },
        //    {4, "Motorhaube" },
        //    {5, "Kofferraum" },
        //    {6, "Kofferraum 2" },
        //};

        public static Dictionary<int, string> VehicleSimplePartIdToName = new Dictionary<int, string> {
            {0, "Vorne Links" },
            {1, "Vorne Rechts" },
            {2, "Mitte Links" },
            {3, "Mitte Rechts" },
            {4, "Hinten Links" },
            {5, "Hinten Rechts" },
        };

        //public static Dictionary<int, string> VehicleWindowToName = new Dictionary<int, string> {
        //    {0, "Windschutzscheibe" },
        //    {1, "Heckscheibe" }, 
        //    {2, "Vorne links" },
        //    {3, "Vorne rechts" }, 
        //    {4, "Hinten Links" },
        //    {5, "Hinten Rechts" },
        //};

        public static Dictionary<int, string> VehicleBumperToName = new Dictionary<int, string> {
            {0, "Vorne" },
            {1, "Hinten" },
        };

        public static Dictionary<VehicleBumperState, string> VehicleBumperStateToName = new Dictionary<VehicleBumperState, string> {
            {VehicleBumperState.Off, "Abgefallen" },
            {VehicleBumperState.Bouncing, "Beschädigt" },
            {VehicleBumperState.On, "Ganz" },
        };

        public static Dictionary<VehicleBumperState, MenuItemStyle> VehicleBumperStateToStyle = new Dictionary<VehicleBumperState, MenuItemStyle> {
            {VehicleBumperState.Off, MenuItemStyle.red },
            {VehicleBumperState.Bouncing, MenuItemStyle.yellow },
            {VehicleBumperState.On,  MenuItemStyle.green },
        };

        //public static Dictionary<int, VehicleRepairItemType> BigPartIdVehicleRepairItemType = new Dictionary<int, VehicleRepairItemType> {
        //    {0, VehicleRepairItemType.FrontPart },
        //    {1, VehicleRepairItemType.FrontPart },
        //    {2, VehicleRepairItemType.MiddlePart },
        //    {3, VehicleRepairItemType.MiddlePart },
        //    {4, VehicleRepairItemType.BackPart },
        //    {5, VehicleRepairItemType.BackPart },
        //};

        public static Dictionary<VehicleModType, string> VehicleModTypeToName = new Dictionary<VehicleModType, string> {
            {VehicleModType.Armor, "Rüstung" },
            {VehicleModType.BackWheels, "Hinterräder" },
            {VehicleModType.Boost, "Boost" },
            {VehicleModType.Brakes, "Bremsen" },
            {VehicleModType.Color1, "Farbe 1" },
            {VehicleModType.Color2, "Farbe 2" },
            {VehicleModType.DashboardColor, "Dashboard Farbe" },
            {VehicleModType.DialDesign, "Zifferndesign" },
            {VehicleModType.Engine, "Motor" },
            {VehicleModType.Exhaust, "Auspuff" },
            {VehicleModType.Fender, "Kotflügel" },
            {VehicleModType.Frame, "Rahmen" },
            {VehicleModType.FrontBumper, "Frontstoßstange" },
            {VehicleModType.FrontWheels, "Vorderräder" },
            {VehicleModType.Grille, "Kühlergrill" },
            {VehicleModType.Hood, "Motorhaube" },
            {VehicleModType.Horns, "Hupe" },
            {VehicleModType.Hydraulics, "Hydraulik" },
            {VehicleModType.Livery, "Lackierung" },
            {VehicleModType.Ornaments, "Verzierungen" },
            {VehicleModType.Plaques, "Plakete" },
            {VehicleModType.Plate, "Nummernschild" },
            {VehicleModType.PlateHolders, "Nummernschildrand" },
            {VehicleModType.RearBumper, "Heckstoßstange" },
            {VehicleModType.RightFender, "Rechter Kotflügel" },
            {VehicleModType.Roof, "Dach" },
            {VehicleModType.ShiftLever, "Schalthebel" },
            {VehicleModType.SideSkirt, "Seitenschweller" },
            {VehicleModType.Spoilers, "Spoiler" },
            {VehicleModType.SteeringWheel, "Lenkrad" },
            {VehicleModType.Suspension, "Federung" },
            {VehicleModType.Transmission, "Energieübertragung" },
            {VehicleModType.TrimColor, "Verkleidungsfarbe" },
            {VehicleModType.TrimDesign, "Verkleidungsdesign" },
            {VehicleModType.Turbo, "Turbo" },
            {VehicleModType.WindowTint, "Fensterschattierung" },
            {VehicleModType.Xenon, "Xenon" },
        };

        #endregion

        #region WeaponSystem

        public static string EQUIP_ANIMATION = "EQUIP_ARMORWEST";

        public enum WeaponType {
            NotFound,
            Pistol,
            Rifle,
            Smg,
            Shotgun,
            Sniper,
            Knife,

            AntiqueMusket,
        }

        public enum WeaponPartType : int {
            //All
            Barrel,
            Magazine,
            Body,

            //Pistol, MP
            Grip,

            //Rifles, Shotgun
            Stock,
            Receiver,
            Sight,

            //Shotgun
            ForeArm,
        }

        //public static Dictionary<string, WeaponType> WeaponNameToTypes = new Dictionary<string, WeaponType> {
        //    { "WEAPON_CARBINERIFLE", WeaponType.Rifle},
        //    { "WEAPON_BULLPUPRIFLE", WeaponType.Rifle},
        //    { "WEAPON_SPECIALCARBINE", WeaponType.Rifle},
        //    { "WEAPON_ASSAULTRIFLE", WeaponType.Rifle },

        //    //Pistol
        //    { "WEAPON_PISTOL", WeaponType.Pistol},
        //    { "WEAPON_PISTOL50", WeaponType.Pistol}

        //    //Smgs

        //};

        //public static Dictionary<string, WeaponType> StringToWeaponType = new Dictionary<string, WeaponType> {
        //    { "", WeaponType.NotFound},
        //    { "Pistol", WeaponType.Pistol},
        //    { "Rifle", WeaponType.Rifle},
        //};

        //public static Dictionary<string, string> WeaponNameToEquipSlot = new Dictionary<string, string> {
        //    { "WEAPON_CARBINERIFLE", "longWeapon"},
        //    { "WEAPON_BULLPUPRIFLE", "longWeapon"},
        //    { "WEAPON_SPECIALCARBINE", "longWeapon"},
        //    { "WEAPON_ASSAULTRIFLE", "longWeapon" },
        //    { "WEAPON_PISTOL", "pistol"},
        //    { "WEAPON_PISTOL50", "pistol"},
        //};

        public static Dictionary<WeaponType, List<WeaponPartType>> WeaponTypeToWeaponParts = new Dictionary<WeaponType, List<WeaponPartType>>{
            {WeaponType.Rifle, new List<WeaponPartType>() { WeaponPartType.Magazine, WeaponPartType.Barrel, WeaponPartType.Body, WeaponPartType.Receiver, WeaponPartType.Sight, WeaponPartType.Stock } },
            {WeaponType.Pistol, new List<WeaponPartType>() { WeaponPartType.Barrel, WeaponPartType.Magazine, WeaponPartType.Body, WeaponPartType.Grip } },
            {WeaponType.Smg, new List<WeaponPartType>() { WeaponPartType.Barrel, WeaponPartType.Magazine, WeaponPartType.Body, WeaponPartType.Grip } },
            {WeaponType.Shotgun, new List<WeaponPartType>() { WeaponPartType.Magazine, WeaponPartType.Barrel, WeaponPartType.Body, WeaponPartType.Receiver, WeaponPartType.Sight, WeaponPartType.Stock, WeaponPartType.ForeArm } },
        };

        public static Dictionary<WeaponPartType, bool> IsWeaponPartSpecific = new Dictionary<WeaponPartType, bool>{
            { WeaponPartType.Body, true },
            { WeaponPartType.Grip, true },
            { WeaponPartType.Stock, true },

            { WeaponPartType.Barrel, false },
            { WeaponPartType.ForeArm, false },
            { WeaponPartType.Magazine, false },
            { WeaponPartType.Receiver, false },
            { WeaponPartType.Sight, false },
        };

        public static Dictionary<string, WeaponPartType> WeaponComponentToWeaponPartType = new Dictionary<string, WeaponPartType>{
            {"COMPONENT_BULLPUPRIFLE_CLIP_02", WeaponPartType.Magazine }
        };

        public static string[] WeaponNames = new string[] {
            "WEAPON_KNIFE", "WEAPON_NIGHTSTICK", "WEAPON_HAMMER", "WEAPON_BAT", "WEAPON_GOLFCLUB",
            "WEAPON_CROWBAR", "WEAPON_PISTOL", "WEAPON_PISTOL_MK2", "WEAPON_COMBATPISTOL", "WEAPON_APPISTOL", "WEAPON_PISTOL50",
            "WEAPON_MICROSMG", "WEAPON_SMG", "WEAPON_SMG_MK2", "WEAPON_ASSAULTSMG", "WEAPON_ASSAULTRIFLE",
            "WEAPON_CARBINERIFLE", "WEAPON_CARBINERIFLE_MK2", "WEAPON_ADVANCEDRIFLE", "WEAPON_MG", "WEAPON_COMBATMG", "WEAPON_COMBATMG_MK2", "WEAPON_PUMPSHOTGUN", "WEAPON_PUMPSHOTGUN_MK2",
            "WEAPON_SAWNOFFSHOTGUN", "WEAPON_ASSAULTSHOTGUN", "WEAPON_BULLPUPSHOTGUN", "WEAPON_STUNGUN", "WEAPON_SNIPERRIFLE",
            "WEAPON_HEAVYSNIPER", "WEAPON_HEAVYSNIPER_MK2", "WEAPON_GRENADELAUNCHER", "WEAPON_GRENADELAUNCHER_SMOKE", "WEAPON_RPG", "WEAPON_MINIGUN",
            "WEAPON_GRENADE", "WEAPON_STICKYBOMB", "WEAPON_SMOKEGRENADE", "WEAPON_BZGAS", "WEAPON_MOLOTOV",
            "WEAPON_FIREEXTINGUISHER", "WEAPON_PETROLCAN", "WEAPON_FLARE", "WEAPON_SNSPISTOL", "WEAPON_SNSPISTOLMK2", "WEAPON_SPECIALCARBINE",
            "WEAPON_HEAVYPISTOL", "WEAPON_BULLPUPRIFLE", "WEAPON_BULLPUPRIFLE_MK2", "WEAPON_HOMINGLAUNCHER", "WEAPON_PROXMINE", "WEAPON_SNOWBALL",
            "WEAPON_SPECIALCARBINE_MK2", "WEAPON_VINTAGEPISTOL", "WEAPON_DAGGER", "WEAPON_FIREWORK", "WEAPON_MUSKET", "WEAPON_MARKSMANRIFLE", "WEAPON_MARKSMANRIFLE_MK2",
            "WEAPON_HEAVYSHOTGUN", "WEAPON_GUSENBERG", "WEAPON_HATCHET", "WEAPON_RAILGUN", "WEAPON_COMBATPDW",
            "WEAPON_KNUCKLE", "WEAPON_MARKSMANPISTOL", "WEAPON_FLASHLIGHT", "WEAPON_MACHETE", "WEAPON_MACHINEPISTOL",
            "WEAPON_SWITCHBLADE", "WEAPON_REVOLVER", "WEAPON_REVOLVER_MK2", "WEAPON_COMPACTRIFLE", "WEAPON_DBSHOTGUN", "WEAPON_FLAREGUN",
            "WEAPON_AUTOSHOTGUN", "WEAPON_BATTLEAXE", "WEAPON_COMPACTLAUNCHER", "WEAPON_MINISMG", "WEAPON_PIPEBOMB",
            "WEAPON_POOLCUE", "WEAPON_SWEEPER", "WEAPON_WRENCH", "WEAPON_RAYPISTOl", "WEAPON_RAYCARBINE", "WEAPON_RAYMINIGUN", "gadget_parachute", "WEAPON_CERAMICPISTOL"
        };

        #endregion

        #region CharacterSystem

        public static TimeSpan PLAYER_UPDATE_INTERVALL = TimeSpan.FromSeconds(80);
        public static TimeSpan PLAYER_UPDATE_DATABASE_INTERVALL = TimeSpan.FromSeconds(25);

        #endregion

        #region PlaceableObjectsSystem

        public static TimeSpan PLACEABLE_OBJECTS_TICK_TIME = TimeSpan.FromMinutes(5);

        public static string MENU_PLACEABLE_OBJECT_HARVEST = "PLACEABLE_OBJECT_HARVEST";

        #endregion

        #region Inventory

        public static float PLAYER_INVENTORY_MAX_WEIGHT = 20;

        public static float BACKPACK_INVENTORY_MAX_WEIGHT = 10;

        #endregion

        #region SkillSystem

        public static double SKILLSYSTEM_EXPCHECK_PROBABILITY = 100.0;

        #endregion

        #region DamageController

        public static ScreenEffect DEATH_CONCUSION_SCREEN_EFFECT = new ScreenEffect("DeathFailOut", DEATH_CONCUSION_TIME, true);

        public static TimeSpan DEATH_CONCUSION_TIME = TimeSpan.FromMinutes(10);
        public static TimeSpan DEATH_INJURED_INFORM_TIME = TimeSpan.FromMinutes(7.5);

        public static Animation ANIMATION_CONCUSSION_ANIMATION = new Animation("combat@damage@writheidle_a", "writhe_idle_a", DEATH_CONCUSION_TIME, 1, 0);
        public static Animation ANIMATION_INJURED_ANIMATION = new Animation("missmic2leadinmic_2_intleadout", "ko_on_floor_idle", TimeSpan.FromMinutes(1000), 1, 0);

        public static float DEATH_REMAIN_HEALTH_PERCENT = 0.2f;

        public enum PainEffect {
            None = 0,
            SlowWalk = 1,
            NoUse = 2,
            BadSight = 3,
        }

        public enum DamageType {
            NoInjury = -1,
            NoInput = 0,
            Dull = 1,
            Sting = 2,
            Shot = 3,
            Inflammation = 4,
            Burning = 5,
        }

        public enum CharacterBodyPart {
            Head = 0,
            Torso = 1,
            LeftArm = 2,
            RightArm = 3,
            LeftLeg = 4,
            RightLeg = 5,
            None = 255,
        }

        public enum CharacterBodyPartCategories {
            All = 0,
            Arms = 1,
            Legs = 2,
            Limbs = 3,
        }

        public static List<CharacterBodyPartCategories> getCategoriesForBodyPart(CharacterBodyPart bodyPart) {
            switch(bodyPart) {
                case CharacterBodyPart.Head:
                case CharacterBodyPart.Torso:
                    return new List<CharacterBodyPartCategories> { CharacterBodyPartCategories.All };
                case CharacterBodyPart.LeftArm:
                case CharacterBodyPart.RightArm:
                    return new List<CharacterBodyPartCategories> { CharacterBodyPartCategories.All, CharacterBodyPartCategories.Arms, CharacterBodyPartCategories.Limbs };
                case CharacterBodyPart.LeftLeg:
                case CharacterBodyPart.RightLeg:
                    return new List<CharacterBodyPartCategories> { CharacterBodyPartCategories.All, CharacterBodyPartCategories.Legs, CharacterBodyPartCategories.Limbs };
                default:
                    return new List<CharacterBodyPartCategories> { CharacterBodyPartCategories.All };
            }
        }

        public static List<CharacterBodyPart> getBodyPartsForCategory(CharacterBodyPartCategories category) {
            switch(category) {
                case CharacterBodyPartCategories.All:
                    return new List<CharacterBodyPart> { CharacterBodyPart.Head, CharacterBodyPart.Torso, CharacterBodyPart.LeftArm, CharacterBodyPart.RightArm, CharacterBodyPart.LeftLeg, CharacterBodyPart.RightLeg };
                case CharacterBodyPartCategories.Arms:
                    return new List<CharacterBodyPart> { CharacterBodyPart.LeftArm, CharacterBodyPart.RightArm };
                case CharacterBodyPartCategories.Legs:
                    return new List<CharacterBodyPart> { CharacterBodyPart.LeftLeg, CharacterBodyPart.RightLeg };
                case CharacterBodyPartCategories.Limbs:
                    return new List<CharacterBodyPart> { CharacterBodyPart.LeftArm, CharacterBodyPart.RightArm, CharacterBodyPart.LeftLeg, CharacterBodyPart.RightLeg };
                default:
                    return new List<CharacterBodyPart> { CharacterBodyPart.Head, CharacterBodyPart.Torso, CharacterBodyPart.LeftArm, CharacterBodyPart.RightArm, CharacterBodyPart.LeftLeg, CharacterBodyPart.RightLeg };
            }
        }

        public static List<string> getCategoriesForBodyPartStrings(CharacterBodyPart bodyPart) {
            switch(bodyPart) {
                case CharacterBodyPart.Head:
                case CharacterBodyPart.Torso:
                    return new List<string> { CharacterBodyPartCategories.All.ToString() };
                case CharacterBodyPart.LeftArm:
                case CharacterBodyPart.RightArm:
                    return new List<string> { CharacterBodyPartCategories.All.ToString(), CharacterBodyPartCategories.Arms.ToString(), CharacterBodyPartCategories.Limbs.ToString() };
                case CharacterBodyPart.LeftLeg:
                case CharacterBodyPart.RightLeg:
                    return new List<string> { CharacterBodyPartCategories.All.ToString(), CharacterBodyPartCategories.Legs.ToString(), CharacterBodyPartCategories.Limbs.ToString() };
                default:
                    return new List<string> { CharacterBodyPartCategories.All.ToString() };
            }
        }

        public static Dictionary<uint, DamageType> WeaponIdToDamageType = new Dictionary<uint, DamageType> {
            //Drowning
            { AltShared.Hash("WEAPON_DROWNING"), DamageType.NoInjury },
            { AltShared.Hash("WEAPON_DROWNING_IN_VEHICLE"), DamageType.NoInjury },
            { AltShared.Hash("WEAPON_ELECTRIC_FENCE"), DamageType.NoInjury },
            { AltShared.Hash("WEAPON_EXHAUSTION"), DamageType.NoInjury },

            //Knifes
            { ChoiceVAPI.Hash("weapon_dagger"), DamageType.Sting },
            { ChoiceVAPI.Hash("weapon_bottle"), DamageType.Sting },
            { ChoiceVAPI.Hash("weapon_hatchet"), DamageType.Sting },
            { ChoiceVAPI.Hash("weapon_knife"), DamageType.Sting },
            { ChoiceVAPI.Hash("weapon_machete"), DamageType.Sting },
            { ChoiceVAPI.Hash("weapon_switchblade"), DamageType.Sting },
            { ChoiceVAPI.Hash("weapon_battleaxe"), DamageType.Sting },
            { ChoiceVAPI.Hash("weapon_stone_hatchet"), DamageType.Sting },

            //Pistols
            { ChoiceVAPI.Hash("weapon_pistol"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_pistol_mk2"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_combatpistol"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_appistol"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_pistol50"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_snspistol"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_snspistol_mk2"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_heavypistol"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_vintagepistol"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_marksmanpistol"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_revolver"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_revolver_mk2"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_doubleaction"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_ceramicpistol"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_navyrevolver"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_gadgetpistol"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_pistolxm3"), DamageType.Shot },

            //SMGs
            { ChoiceVAPI.Hash("weapon_microsmg"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_smg"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_smg_mk2"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_assaultsmg"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_combatpdw"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_machinepistol"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_minismg"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_tecpistol"), DamageType.Shot },

            //Shotguns
            { ChoiceVAPI.Hash("weapon_pumpshotgun"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_pumpshotgun_mk2"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_sawnoffshotgun"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_assaultshotgun"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_bullpupshotgun"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_heavyshotgun"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_dbshotgun"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_autoshotgun"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_combatshotgun"), DamageType.Shot },

            { ChoiceVAPI.Hash("weapon_musket"), DamageType.Shot },

            //Assault Rifles
            { ChoiceVAPI.Hash("weapon_assaultrifle"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_assaultrifle_mk2"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_carbinerifle"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_carbinerifle_mk2"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_advancedrifle"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_specialcarbine"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_specialcarbine_mk2"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_bullpuprifle"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_bullpuprifle_mk2"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_compactrifle"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_militaryrifle"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_heavyrifle"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_tacticalrifle"), DamageType.Shot },

            //LMGs
            { ChoiceVAPI.Hash("weapon_mg"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_combatmg"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_combatmg_mk2"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_gusenberg"), DamageType.Shot },

            //Snipers
            { ChoiceVAPI.Hash("weapon_sniperrifle"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_heavysniper"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_heavysniper_mk2"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_marksmanrifle"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_marksmanrifle_mk2"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_precisionrifle"), DamageType.Shot },

            //Heavy Weapons
            { ChoiceVAPI.Hash("weapon_minigun"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_railgun"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_railgunxm3"), DamageType.Shot },

            //Burning
            { ChoiceVAPI.Hash("weapon_molotov"), DamageType.Burning },
            { ChoiceVAPI.Hash("weapon_flaregun"), DamageType.Burning },

            { ChoiceVAPI.Hash("weapon_raycarbine"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_rayminigun"), DamageType.Shot },
            { ChoiceVAPI.Hash("weapon_petrolcan"), DamageType.Burning },
            { ChoiceVAPI.Hash("weapon_hazardcan"), DamageType.Burning },
            { ChoiceVAPI.Hash("weapon_fertilizercan"), DamageType.Burning },
            { ChoiceVAPI.Hash("WEAPON_FIRE"), DamageType.Burning },

        };

        public static Dictionary<CharacterBodyPart, string> CharacterBodyPartToString = new Dictionary<CharacterBodyPart, string> {
            { CharacterBodyPart.Head, "Kopf" },
            { CharacterBodyPart.Torso, "Torso" },
            { CharacterBodyPart.LeftArm, "linken Arm" },
            { CharacterBodyPart.RightArm, "rechten Arm" },
            { CharacterBodyPart.LeftLeg, "linken Bein" },
            { CharacterBodyPart.RightLeg, "rechten Bein" },
        };

        public static Dictionary<DamageType, string> DamageTypeToString = new Dictionary<DamageType, string> {
            { DamageType.NoInjury, "" },
            { DamageType.NoInput, "" },
            { DamageType.Dull, "stumpfe Schmerzen" },
            { DamageType.Sting, "stechende Schmerzen" },
            { DamageType.Shot, "Schusswunden" },
            { DamageType.Inflammation, "entzündete Schmerzen" },
            { DamageType.Burning, "brennende Schmerzen" },
        };

        public static Dictionary<CharacterBodyPart, string> CharacterBodyPartToCef = new Dictionary<CharacterBodyPart, string> {
            { CharacterBodyPart.Head, "head" },
            { CharacterBodyPart.Torso, "torso" },
            { CharacterBodyPart.LeftArm, "leftArm" },
            { CharacterBodyPart.RightArm, "rightArm" },
            { CharacterBodyPart.LeftLeg, "leftLeg" },
            { CharacterBodyPart.RightLeg, "rightLeg" },
        };

        public static Dictionary<int, CharacterBodyPart> BoneToCharacterBodyPart = new Dictionary<int, CharacterBodyPart> {
            //Brow
            {1356, CharacterBodyPart.Head },
            {37193, CharacterBodyPart.Head },
            {58331, CharacterBodyPart.Head },

            //Toes
            {2108, CharacterBodyPart.LeftLeg },
            {20781, CharacterBodyPart.RightLeg },

            //Elbow
            {2992, CharacterBodyPart.RightArm },
            {22711, CharacterBodyPart.LeftArm },

            //Finger
            {4089, CharacterBodyPart.LeftArm },
            {4090, CharacterBodyPart.LeftArm },
            {4137, CharacterBodyPart.LeftArm },
            {4138, CharacterBodyPart.LeftArm },
            {4153, CharacterBodyPart.LeftArm },
            {4154, CharacterBodyPart.LeftArm },
            {4169, CharacterBodyPart.LeftArm },
            {4170, CharacterBodyPart.LeftArm },
            {4185, CharacterBodyPart.LeftArm },
            {4186, CharacterBodyPart.LeftArm },
            {26610, CharacterBodyPart.LeftArm },
            {26611, CharacterBodyPart.LeftArm },
            {26612, CharacterBodyPart.LeftArm },
            {26613, CharacterBodyPart.LeftArm },
            {26614, CharacterBodyPart.LeftArm },

            {58866, CharacterBodyPart.RightArm },
            {58867, CharacterBodyPart.RightArm },
            {58868, CharacterBodyPart.RightArm },
            {58869, CharacterBodyPart.RightArm },
            {58870, CharacterBodyPart.RightArm },
            {64016, CharacterBodyPart.RightArm },
            {64017, CharacterBodyPart.RightArm },
            {64064, CharacterBodyPart.RightArm },
            {64065, CharacterBodyPart.RightArm },
            {64080, CharacterBodyPart.RightArm },
            {64081, CharacterBodyPart.RightArm },
            {64096, CharacterBodyPart.RightArm },
            {64097, CharacterBodyPart.RightArm },
            {64112, CharacterBodyPart.RightArm },
            {64113, CharacterBodyPart.RightArm },

            //Armroll
            {5232, CharacterBodyPart.LeftArm },
            {37119, CharacterBodyPart.RightArm },

            //Hand
            {6286, CharacterBodyPart.RightArm },

            //Thigh
            {6442, CharacterBodyPart.RightLeg },
            {23639, CharacterBodyPart.LeftLeg },
            {51826, CharacterBodyPart.RightLeg },
            {58271, CharacterBodyPart.LeftLeg },

            //Clavicle (Schlüsselbein)
            {10706, CharacterBodyPart.RightArm },
            {64729, CharacterBodyPart.LeftArm },

            //Lip
            {11174, CharacterBodyPart.Head },
            {17188, CharacterBodyPart.Head },
            {17719, CharacterBodyPart.Head },
            {20178, CharacterBodyPart.Head },
            {20279, CharacterBodyPart.Head },
            {20623, CharacterBodyPart.Head },
            {29868, CharacterBodyPart.Head },
            {47419, CharacterBodyPart.Head },
            {49979, CharacterBodyPart.Head },
            {61839, CharacterBodyPart.Head },

            //Toros
            {0, CharacterBodyPart.Torso },
            {11816, CharacterBodyPart.Torso },
            {56604, CharacterBodyPart.Torso },

            //Head
            {12844, CharacterBodyPart.Head },
            {31086, CharacterBodyPart.Head },
            {65068, CharacterBodyPart.Head },

            //Foot
            {14201, CharacterBodyPart.LeftLeg },
            {24806, CharacterBodyPart.LeftLeg },
            {35502, CharacterBodyPart.RightLeg },
            {52301, CharacterBodyPart.RightLeg },
            {57717, CharacterBodyPart.LeftLeg },
            {65245, CharacterBodyPart.LeftLeg },

            //Knee
            {16335, CharacterBodyPart.RightLeg },
            {46078, CharacterBodyPart.LeftArm },

            //Hand
            {18905, CharacterBodyPart.LeftArm },
            {28422, CharacterBodyPart.RightArm },
            {36029, CharacterBodyPart.LeftArm },
            {57005, CharacterBodyPart.RightArm },
            {60309, CharacterBodyPart.LeftArm },

            //Cheek
            {19336, CharacterBodyPart.Head },
            {21550, CharacterBodyPart.Head },

            //Spine
            {23553, CharacterBodyPart.Torso },
            {24816, CharacterBodyPart.Torso },
            {24817, CharacterBodyPart.Torso },
            {24818, CharacterBodyPart.Torso },
            {57597, CharacterBodyPart.Torso },

            //Eye
            {25260, CharacterBodyPart.Head },
            {27474, CharacterBodyPart.Head },

            //Forearm
            {28252, CharacterBodyPart.RightArm },
            {43810, CharacterBodyPart.RightArm },
            {61007, CharacterBodyPart.LeftArm },
            {61163, CharacterBodyPart.LeftArm },

            //Neck
            {35731, CharacterBodyPart.Head },
            {39317, CharacterBodyPart.Head },

            //Calf (Lower Leg)
            {36864, CharacterBodyPart.RightLeg },
            {63931, CharacterBodyPart.LeftLeg },
             
            //UpperArm
            {40269, CharacterBodyPart.RightArm },
            {45509, CharacterBodyPart.LeftArm },

            //Lid
            {43536, CharacterBodyPart.Head },
            {45750, CharacterBodyPart.Head },

            //Jaw
            {46240, CharacterBodyPart.Head },

            //Tongue
            {47495, CharacterBodyPart.Head },
        };

        public static TimeSpan DAMAGE_TREATED_SHOW_TIME = TimeSpan.FromMinutes(15);
        #endregion

        #region Notifications

        public enum NotifactionTypes {
            Info,
            Success,
            Danger,
            Warning
        }

        public enum NotifactionImages {
            System,
            Gold,
            MagnifyingGlass,
            Marihuana,
            Fertilizer,
            Thief,
            Car,
            Bone,
            Gun,
            Fire,
            Company,
            Police,
            MiniJob,
            Hotel,
            Fleeca,
            Maze,
            Liberty,
            ATM,
            Package,
            Printer,
            Gasstation,
            Garage,
            Radio,
            Island,
            Shop,
            Scissors,
            Tattoo,
            Prison,
            Lock,
            FoldingTable,
            Plane,
            Door,
            Music,
            Cigarette,
            Smartphone,
            Device,
            Newspaper,
            Crafting,
        }

        #endregion

        #region Interaction

        public static float MAX_OBJECT_INTERACT_RANGE = 3f;
        public static float MAX_ENTITY_INTERACTION_RANGE = 5f;

        public static float MAX_PLAYER_INTERACTION_RANGE = 3f;

        #endregion

        #region Prison

        public static float PASSIVE_TO_ACTIVE_SENTENCE_FACTOR = 0.1f;
        public static TimeSpan PRISON_UPDATE_TIME = TimeSpan.FromMinutes(1);
        public static float PRISON_SENTENCE_UNITS_PER_MINUTE = 1.0f;

        #endregion

        #region GoldController

        public static float FIND_GOLD_VEIN_CHANCE = 0.06f;
        public static float FIND_GOLD_FLAKE = 0.25f;
        public static float FIND_GOLD_NUGGET = 0.05f;

        public static TimeSpan VEIN_REMOVE_TIME = TimeSpan.FromMinutes(30);
        public static int VEIN_RADIUS = 7;

        public static string GOLD_MINE_ITEM_ANIMATION = "PICKAXE";
        public static string GOLD_WASH_ITEM_ANIMATION = "GOLD_WASH";

        public static string GOLD_SEARCH_BUFF_NAME = "GOLD_SEARCH_BUFF";

        #endregion

        #region WorldController

        public static int GridWidthHeight = 500;

        #endregion

        #region Evidence

        public static TimeSpan EVIDENCE_ANALYSE_TIME = TimeSpan.FromMinutes(0.001);

        public static Dictionary<EvidenceType, string> EvidenceTypeToDescription = new Dictionary<EvidenceType, string>() {
            { EvidenceType.Blood, "Ein Blutfleck. Aus diesem kann DNA extrahiert werden." },
            { EvidenceType.CarPaint, "Etwas abgesplitterter Lack. Gibt Infos über das Fahrzeug." },
            { EvidenceType.CartridgeCase, "Eine Patronenhülse. Gibt Infos über die Waffe." },
        };

        public static Dictionary<EvidenceType, string> EvidenceTypeToString = new Dictionary<EvidenceType, string>() {
            { EvidenceType.Blood, "Blut" },
            { EvidenceType.CarPaint, "Autolack" },
            { EvidenceType.CartridgeCase, "Patronenhülsen" },
        };

        public enum EvidenceType : int {
            Blood = 0,
            CartridgeCase = 1,
            CarPaint = 2,
        }

        public static Dictionary<EvidenceType, Rgba> EvidenceTypeToMarkerColor = new Dictionary<EvidenceType, Rgba> {
            { EvidenceType.Blood, new Rgba(153, 24, 24, 150) },
            { EvidenceType.CarPaint, new Rgba(50, 130, 16, 150) },
            { EvidenceType.CartridgeCase, new Rgba(165, 82, 14, 150) },
        };

        public static Dictionary<EvidenceType, TimeSpan> EvidenceTypeRemoveTime = new Dictionary<EvidenceType, TimeSpan> {
            { EvidenceType.Blood, TimeSpan.FromHours(2) },
            { EvidenceType.CarPaint, TimeSpan.FromDays(2) },
            { EvidenceType.CartridgeCase,  TimeSpan.FromDays(6) },
        };

        #endregion

        #region Trash

        public static float MAX_TRASH_DISTANCE = 1f;
        public static float MAX_TRASH_CAN_DISTANCE = 5f;

        public static TimeSpan STANDARD_TRASH_REMOVE_TIME = TimeSpan.FromHours(4);
        public static TimeSpan WEAPON_TRASH_REMOVE_TIME = TimeSpan.FromDays(6);

        #endregion

        #region Clothing

        public static Dictionary<int, ClothingComponent> StandartClothings = new Dictionary<int, ClothingComponent> {
            {1, new ClothingComponent(0, 0)},
            {3, new ClothingComponent(0, 0)},
            {4, new ClothingComponent(0, 0)},
            {6, new ClothingComponent(0, 0)},
            {7, new ClothingComponent(0, 0)},
            {8, new ClothingComponent(0, 0)},
            {11, new ClothingComponent(0, 0)}
        };

        public static Dictionary<int, ClothingComponent> StandartAccessoires = new Dictionary<int, ClothingComponent> {
            {0, new ClothingComponent(-1, -1)},
            {1, new ClothingComponent(-1, -1)},
            {2, new ClothingComponent(-1, -1)},
            {6, new ClothingComponent(-1, -1)},
            {7, new ClothingComponent(-1, -1)},
        };

        public static Dictionary<int, string> AccessoireSlotToEquipType = new Dictionary<int, string> {
            {0, "hat" },
            {1, "glasses" },
            {2, "ears" },
            {6, "watch" },
            {7, "bracelet" },
        };

        public static Dictionary<int, string> AccessoireSlotToName = new Dictionary<int, string> {
            {0, "Hut/Helm" },
            {1, "Brillen" },
            {2, "Ohraccessoire" },
            {6, "Uhren" },
            {7, "Armbänder" },
        };

        public static ClothingPlayer NakedMen = new ClothingPlayer {
            Mask = new ClothingComponent(0, 0),
            Torso = new ClothingComponent(15, 0),
            Shirt = new ClothingComponent(15, 0),
            Top = new ClothingComponent(15, 0),
            Legs = new ClothingComponent(14, 0),
            Feet = new ClothingComponent(34, 0),
            Accessories = new ClothingComponent(0, 0)
        };

        public static ClothingPlayer NakedFemale = new ClothingPlayer {
            Mask = new ClothingComponent(0, 0),
            Torso = new ClothingComponent(15, 0),
            Shirt = new ClothingComponent(6, 0),
            Top = new ClothingComponent(18, 0),
            Legs = new ClothingComponent(10, 0),
            Feet = new ClothingComponent(35, 0),
            Accessories = new ClothingComponent(0, 0)
        };


        public static List<int> MaleNoShirts = new List<int> {
            15
        };

        public static List<int> FemaleNoShirts = new List<int> {
            2, 3, 6, 7, 8, 9, 10, 14, 34,
        };

        #endregion

        #region DoorSystem

        #endregion

        #region Animation

        public static string KNEEL_DOWN_ANIMATION = "KNEEL_DOWN";

        #endregion

        #region Heists

        //ATM
        public static int ATM_MIN_TAKE_LOUD = 250;
        public static int ATM_MAX_TAKE_LOUD = 500;

        public static int ATM_LOUD_MIN_TAKES = 4;
        public static int ATM_LOUD_MAX_TAKES = 7;

        public static int ATM_MIN_TAKE_SILENT = 2500;
        public static int ATM_MAX_TAKE_SILENT = 5000;

        public static int ATM_SILENT_HACK_TIME = 45;

        #endregion

        #region ItemIds

        public static int GOLD_CORN_DB_ID = 5;
        public static int GOLD_FLAKE_DB_ID = 6;
        public static int GOLD_NUGGET_DB_ID = 7;

        #endregion

        #region DrugSystem
        public enum WeedItemTypes {
            Sappling,
            Stick,
            DriedStick,
            Weed,
            CrushedWeed,
        }
        public enum DrugTypes {
            Weed,
            Cocaine,
            Alcohol,
            Mescaline,
            Frog,
        }

        public enum WeedItems {
            Joint,
            Blunt,
            Bong,
        }

        public enum TripTypes {
            MexicanTrip,
            ClownTrip,
            AnimalTrip,
            HeatTrip,
        }
        #endregion

        #region DealerSystem
        public enum GangAreas {
            Northside,
            Eastside,
            Southside,
            Westside,
            SandyShores,
            PaletoBay,
        }
        #endregion

        #region Fire

        public static int FIRE_INTERVAL_SECONDS = 30;   // Tickrate for fire events
        public static int FIRE_GROW_SECONDS = 30;       // Growrate for fire events

        #endregion

        public enum PlayerStates : int {
            None = 0, //No State
            InHospital = 1,
            OnPhone = 11,
            Injured = 12,
            ChangingKey = 14,
            InTerminal = 15,
            Busy = 20,
            InAnimation = 21, //>= 20 Busy state
            InAnimationTask = 22, //>= 20 Busy state
            LayingDown = 22,
            BeingCarried = 23,
            OnBusyMenu = 24,
            OnTerminalFlight = 25,
            IsCarrying = 26,
            HandsUp = 27,

            Handcuffed = 30, //> 30 even busier state
            InCharCreator = 31,
            SupportTaskBlock = 32,
            
            InAnesthesia = 45, //> 45 Blocks speaking
            Dead = 50, //Dead
            PermaDeath = 60, //Koma state, in which player cannot be revived from
        }

        public static readonly bool TRY_THREAD_BASED = true;

        //public static Position DefaultSpawnLocation = new Position(-1067.5121f, -2788.9187f, 21.360474f);
        //public static Rotation DefaultSpawnRotation = new Rotation(0, 0, 1.6821126f);
        public static Position EmptyVector = new Position();
        public static Rotation EmptyRotation = new Rotation();
        public const int GlobalDimension = 0;
        public const int IslandDimension = -2147483648;

        public enum Islands {
            SanAndreas = 0,
            CayoPerico = 1,
        }
    }
}
