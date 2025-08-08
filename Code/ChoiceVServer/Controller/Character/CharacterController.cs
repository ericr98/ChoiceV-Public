using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Character;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;
using ChoiceVServer.Controller.DamageSystem.Model;

namespace ChoiceVServer.Controller {
    public delegate void PlayerConnectDataSetDelegate(IPlayer player, character character, characterdatum data);
    public delegate void PlayerCharacterCreatedDelegate(IPlayer player, character character);

    public class CharacterController : ChoiceVScript {
        public static PlayerCharacterCreatedDelegate PlayerCharacterCreatedDelegate;


        public static List<SelfMenuElement> AllSelfMenuElements = new List<SelfMenuElement>();

        public static Dictionary<string, PlayerConnectDataSetDelegate> AllOnConnectDataInits = new Dictionary<string, PlayerConnectDataSetDelegate>();

        private SmallFeatures SmallFeatures;

        public CharacterController() {
            EventController.MainReadyDelegate += onServerReady;
            EventController.MainShutdownDelegate += onServerShutdown;

            EventController.PlayerPreSuccessfullConnectionDelegate += loadCharacter;
            EventController.PlayerSuccessfullConnectionDelegate += onPlayerSuccessfullConnect;

            EventController.PlayerDisconnectedDelegate += onPlayerDisconnect;

            IslandController.PlayerIslandChangeDelegate += onPlayerChangeIsland;

            EventController.addEvent("FINISH_PED_CREATION", onSaveChangeCharacter);
            EventController.addEvent("CANCEL_PED_CREATION", onCancelPedCreation);

            EventController.addEvent("SET_MODEL", onSetModel);

            //Player main Menu, where registered actions are listed
            EventController.addKeyEvent("MAIN_MENU", ConsoleKey.M, "Haupt-Menü", onSelfMenu, true);

            EventController.addEvent("UPDATE_GROUND_MATERIAL", onUpdateGroundMaterial);

            //Activate small features
            SmallFeatures = new SmallFeatures();
        }

        private bool onUpdateGroundMaterial(IPlayer player, string eventName, object[] args) {
            if(player.getCharacterFullyLoaded()) {
                player.getCharacterData().Material = (CallbackController.Materials)uint.Parse(args[0].ToString());
            }

            return true;
        }

        private void onPlayerSuccessfullConnect(IPlayer player, character character) {
            //Deactivate Attacks
            if(player.getCharFlagSetting("START_WITHOUT_WEAPONS")) {
                player.setData("TOOGLE_CAN_ATTACK", true);
                player.emitClientEvent("TOOGLE_CAN_ATTACK");
                player.sendNotification(NotifactionTypes.Info, "Angriffe deaktiviert! Aktiviere sie mit der Taste.", "Angriffe deaktiviert!");
            }
        }

        private void onPlayerChangeIsland(IPlayer player, Islands previousIsland, Islands newIsland) {
            player.getCharacterData().Island = newIsland;
            using(var db = new ChoiceVDb()) {
                var dbChar = db.characters.Find(player.getCharacterId());
                if(dbChar != null) {
                    dbChar.island = (int)newIsland;
                    db.SaveChanges();
                }
            }
        }

        private bool onSetModel(IPlayer player, string eventName, object[] args) {
            var model = args[0].ToString();
            player.Model = ChoiceVAPI.Hash(model);
            return true;
        }

        private void onServerReady() {
            InvokeController.AddTimedInvoke("PlayerUpdater", updatePlayers, Constants.PLAYER_UPDATE_INTERVALL, true);
            InvokeController.AddTimedInvoke("PlayerDatabaseUpdater", updatePlayersDatabase, Constants.PLAYER_UPDATE_DATABASE_INTERVALL, true);

            EventController.TickDelegate += onTick;
        }

        private void onServerShutdown() {
            foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                updatePlayerDatabase(player);
            }
        }

        private void onTick() {
            var timer = DateTime.Now;

            foreach(var player in ChoiceVAPI.GetAllPlayers().Reverse<IPlayer>()) {
                if(player.getCharacterFullyLoaded()) {
                    // Get character data
                    var data = player.getCharacterData();

                    // Get current player position
                    var aktPos = player.Position;

                    // On player move process create events and check for nearby vehicles
                    if(data != null && aktPos.Distance(data.LastPosition) > 0.15f) {
                        data.Age += 100;

                        var grid = WorldController.getWorldGrid(aktPos);

                        // Create player moved event
                        EventController.onPlayerMoved(this, player, data.LastPosition, data.LastGrid, aktPos, WorldController.getGridBlock(aktPos));

                        data.LastPosition = aktPos;

                        // Create event for worldgrid changed
                        if(data.LastGrid == null || data.LastGrid != grid) {
                            EventController.onPlayerChangeWorldGrid(this, player, data.LastGrid, grid);
                            data.LastGrid = grid;
                        }
                    }
                }
            }

            var span = DateTime.Now.Subtract(timer).Milliseconds;

            if(span > 50) Logger.logWarning(LogCategory.System, LogActionType.Blocked, "Player tick runtime: " + span + "ms");
        }

        private bool onSelfMenu(IPlayer player, ConsoleKey key, string eventName) {
            var menu = new Menu("Spieler Interaktion", "Was möchtest du tun?");

            if(player.IsInVehicle) {
                var virtVehMenu = new VirtualMenu("Fahrzeug-Menü", () => {
                    return VehicleController.getVehicleIndoorMenu(player);
                });

                menu.addMenuItem(new MenuMenuItem("Fahrzeug-Menü", virtVehMenu));
            }

            var data = player.getCharacterData();
            foreach(var element in AllSelfMenuElements) {
                if((element.ShowOnBusy || !player.getBusy()) && element.ShowForTypes.Contains(data.CharacterType) && element.checkShow(player)) {
                    var menuEl = element.getMenuElement(player);
                    if(menuEl != null) {
                        if(menuEl is MenuItem) {
                            var item = menuEl as MenuItem;
                            menu.addMenuItem(item);
                        } else {
                            var virtualMenu = menuEl as VirtualMenu;

                            //Make MenuItem only using generator, to reduce cpu power
                            menu.addMenuItem(new MenuMenuItem(virtualMenu.Name, virtualMenu, virtualMenu.Style));
                        }
                    }
                }
            }
            player.showMenu(menu, false);
            return true;
        }

        /// <summary>
        /// Adds a MenuElement to the player "self" interaction menu
        /// </summary>
        public static void addSelfMenuElement(SelfMenuElement element, bool showOnBusy = false, List<CharacterType> showForTypes = null) {
            element.ShowOnBusy = showOnBusy;
            element.ShowForTypes = showForTypes ?? [CharacterType.Player];
            
            AllSelfMenuElements.Add(element);
        }

        /// <summary>
        /// Adds a callback for a permanent data, triggered when the player connects
        /// </summary>
        /// <param name="key">Key of Permanent data</param>
        public static void addPlayerConnectDataSetCallback(string key, PlayerConnectDataSetDelegate callback) {
            if(!AllOnConnectDataInits.ContainsKey(key)) {
                AllOnConnectDataInits.Add(key, callback);
            } else {
                Logger.logWarning(LogCategory.System, LogActionType.Blocked, $"addPlayerConnectDataSetCallback: Tried to register dataSetCallback which was already registered!");
            }
        }

        /// <summary>
        /// Loads a specific Character. Setting Clothes, initialising all kinds of models, etc.
        /// </summary>
        private void loadCharacter(IPlayer player, character character) {
            try {
                if(!string.IsNullOrEmpty(character.model)) {
                    player.Model = ChoiceVAPI.Hash(character.model);
                } else if(character.characterstyle.gender == "F") {
                    player.Model = 0x9C9EFFD8;
                } else {
                    player.Model = 0x705E61F2;
                }

                player.setStyle(character.characterstyle);
                using(var db = new ChoiceVDb()) {
                    var acc = db.accounts.FirstOrDefault(acc => acc.id == character.accountId);
                    player.setCharacterData(new CharacterData {
                        CharacterType = (CharacterType)character.characterType,
                        PermadeathActivated = character.dead == 1,

                        Age = character.age,
                        Hunger = character.hunger,
                        Thirst = character.thirst,
                        Energy = character.energy,

                        CharacterFlag = (CharacterFlag)character.flag,

                        DateOfBirth = character.birthdate,

                        Health = character.health,

                        Title = character.title,
                        FirstName = character.firstname,
                        MiddleNames = character.middleNames,
                        LastName = character.lastname,
                        Gender = character.characterstyle.gender.ToCharArray()[0],

                        SocialSecurityNumber = character.socialSecurityNumber,

                        Cash = character.cash,

                        CurrentPlayTime = character.currentPlayTimeDate.Date == DateTime.Now.Date ? character.currentPlayTime : 0,
                        CurrentPlayTimeDate = DateTime.Now,

                        StatusUpdateDate = character.stateUpdateDate ?? DateTime.MinValue,

                        Style = character.characterstyle,

                        Island = (Islands)character.island,
                    });

                    var inv = InventoryController.loadInventory(player);
                    if(inv == null) {
                        inv = InventoryController.createInventory(player.getCharacterId(), Constants.PLAYER_INVENTORY_MAX_WEIGHT, InventoryTypes.Player);
                        player.setInventory(inv);
                    } else {
                        player.setInventory(inv);
                        var equippedOnReconnectItems = inv.getItems<EquipItem>(i => i.Data.hasKey("KeepEquipped"));
                        foreach (var item in equippedOnReconnectItems) {
                            item.fastEquip(player);
                            item.Data.remove("KeepEquipped");
                        }
                    }

                    Rotation rot = character.rotation.FromJson<Rotation>();
                    Position pos = character.position.FromJson();

                    player.Rotation = rot;
                    player.Spawn(pos);
                    player.changeDimension(character.dimension);
                    player.Frozen = true;

                    InvokeController.AddTimedInvoke("Remove-Freeze", (i) => {
                        player.Frozen = false;
                    }, TimeSpan.FromSeconds(1), false);

                    // Initialize position
                    player.getCharacterData().LastPosition = pos;

                    // Apply CharacterData
                    player.ignoreNextDamage();
                    player.setHealth(character.health, true);
                    List<characterinjury> damageData = new List<characterinjury>();

                    if(character.characterinjuries != null) {
                        damageData = character.characterinjuries.ToList();
                    }

                    player.getCharacterData().CharacterDamage = new CharacterDamage(damageData);

                    EventController.onPlayerMoved(this, player, Position.Zero, null, pos, WorldController.getGridBlock(pos));
                }

                //Run this twice to first set all data and then run the connect inits
                foreach(var data in character.characterdata) {
                    player.setData(data.name, data.value);
                } 
                
                foreach(var data in character.characterdata) {
                    if(AllOnConnectDataInits.ContainsKey(data.name)) {
                        AllOnConnectDataInits[data.name].Invoke(player, character, data);
                    }
                }
                

                player.updateHud();

                Logger.logInfo(LogCategory.Player, LogActionType.Created, player, $"Character was successfully loaded");
            } catch(Exception e) {
                Logger.logException(player, e);
                ChoiceVAPI.KickPlayer(player, "Loadway", "Versuche den Login erneut, bei weiterem Fehler melde dich im Support!", "Beim Laden deines Charakters ist etwas schiefgelaufen");
            }
        }

        private void onPlayerDisconnect(IPlayer player, string reason) {
            if(player.getCharacterFullyLoaded()) {
                updatePlayerDatabase(player);
            }
        }

        private bool onCancelPedCreation(IPlayer player, string eventName, object[] args) {
            player.removeState(PlayerStates.InCharCreator);

            if(player.hasData("NOT_YET_LEFT_TERMINAL")) {
                TerminalController.portToTerminal(player, false, null, false);
            } else {
                player.changeDimension(Constants.GlobalDimension);
                player.Position = new Position(0, 0, 72);
                InvokeController.AddTimedInvoke("Screen-Out-Fader", (i) => player.fadeScreen(false, 1000), TimeSpan.FromSeconds(3), false);
            }

            return true;
        }

        private bool onSaveChangeCharacter(IPlayer player, string eventName, object[] args) {
            player.removeState(PlayerStates.InCharCreator);
            try {
                using(var db = new ChoiceVDb()) {
                    if(db.characters.Any(c => c.id == player.getCharacterId())) {
                        overrideExistingCharacter(player, Convert.ToString(args[0]), args[2].ToString());
                        return true;
                    }

                    var sc = (string)args[3];
                    var account = db.accounts.FirstOrDefault(p => p.socialclubName == sc);
                    if(account == null) {
                        Logger.logError($"onSaveChangeCharacter: account not found! socialclub: {sc}",
                                $"Fehler beim Speichern des Charakters! Der Account wurde nicht gefunden", player);
                        return false;
                    }

                    account.state = 1;
                    db.SaveChanges();

                    var playerChar = new character {
                        accountId = account.id,
                        characterclothing = new characterclothing(),

                        title = "",
                        middleNames = "",
                        originState = "",

                        position = TerminalController.getPortTerminalPosition().ToJson(),
                        rotation = new Rotation().ToJson(),

                        health = 100,
                    };
                    db.characters.Add(playerChar);

                    db.SaveChanges();

                    player.setData(Constants.DATA_CHARACTER_ID, playerChar.id);
                    player.SetStreamSyncedMetaData(Constants.DATA_CHARACTER_ID, playerChar.id);

                    string data = Convert.ToString(args[0]);
                    var gender = args[2].ToString();
                    //var playerChar = db.characters.Include(c => c.characterstyles).FirstOrDefault(c => c.id == player.getCharacterId());
                    var playerStyle = db.characterstyles.FirstOrDefault(c => c.charId == playerChar.id);

                    if(playerStyle == null) {
                        playerStyle = new characterstyle() { charId = playerChar.id };
                        db.characterstyles.Add(playerStyle);
                    }

                    JsonConvert.PopulateObject(data, playerStyle);
                    playerStyle.gender = gender;
                    playerChar.gender = gender;

                    player.setStyle(playerStyle);
                    dynamic dataModel = null;
                    try {
                        dataModel = JObject.Parse(args[0].ToString());

                        playerChar.title = dataModel.title ?? "";
                        playerChar.firstname = dataModel.firstName ?? "Joe";
                        playerChar.lastname = dataModel.lastName ?? "Doe";
                        playerChar.middleNames = dataModel.middleNames ?? "";
                        playerChar.originState = dataModel.originState;
                        playerChar.socialSecurityNumber = dataModel.sscPrefix;

                        try {
                            playerChar.birthdate = dataModel.charBirth;
                        } catch(Exception) {
                            playerChar.birthdate = new DateOnly(2000, 1, 1);
                        }

                        if(!dataModel.hairOverlayCollection.ToString().Equals("")) {
                            FaceFeatureController.savePlayerHairOverlay(player, FaceFeatureController.AllHairOverlays.FirstOrDefault(o => o.collection == dataModel.hairOverlayCollection.ToString() && o.hash == dataModel.hairOverlayHash.ToString()));
                        }

                        var clothing = new ClothingPlayer();

                        ClothingController.loadPlayerClothing(player, clothing);
                    } catch(Exception e) {
                        Logger.logException(e);
                    }

                    db.SaveChanges();

                    PlayerCharacterCreatedDelegate?.Invoke(player, playerChar);

                    EventController.onPlayerSuccessfullConnection(player);

                    TerminalController.portToTerminal(player, true, (string)dataModel.outfit);
                }
            } catch(Exception ex) {
                Logger.logException(ex);
            }

            return true;
        }

        private static void overrideExistingCharacter(IPlayer player, string data, string gender) {
            dynamic dataModel = JObject.Parse(data);

            player.removeState(PlayerStates.InCharCreator);
            using(var db = new ChoiceVDb()) {
                var already = db.characters.Include(c => c.characterstyle).FirstOrDefault(c => c.id == player.getCharacterId());

                if(already.characterstyle == null) {
                    already.characterstyle = new characterstyle() { charId = already.id };
                }

                JsonConvert.PopulateObject(data, already.characterstyle);
                already.characterstyle.gender = gender;
                already.gender = gender;

                already.title = dataModel.title ?? "";
                already.firstname = dataModel.firstName ?? "Joe";
                already.lastname = dataModel.lastName ?? "Doe";
                already.middleNames = dataModel.middleNames ?? "";
                already.originState = dataModel.originState;
                already.socialSecurityNumber = "-1";

                try {
                    var date = dataModel.charBirth.ToString().Split('.');
                    already.birthdate = dataModel.charBirth;
                } catch(Exception) {
                    already.birthdate = new DateOnly(2000, 1, 1);
                }

                db.SaveChanges();

                player.setStyle(already.characterstyle);

                player.getCharacterData().Title = dataModel.title ?? "";
                player.getCharacterData().FirstName = dataModel.firstName ?? "Joe";
                player.getCharacterData().LastName = dataModel.lastName ?? "Doe";
                player.getCharacterData().MiddleNames = dataModel.middleNames ?? "";
                player.getCharacterData().SocialSecurityNumber = "-1";

                player.getCharacterData().Style = already.characterstyle;
            }

            player.resetOverlayType("hair_overlay");
            if(!dataModel.hairOverlayCollection.ToString().Equals("")) {
                FaceFeatureController.savePlayerHairOverlay(player, FaceFeatureController.AllHairOverlays.FirstOrDefault(o => o.collection == dataModel.hairOverlayCollection.ToString() && o.hash == dataModel.hairOverlayHash.ToString()));
            } else {
                player.resetOverlayType("hair_overlay");
            }

            if(player.hasData("NOT_YET_LEFT_TERMINAL")) {
                TerminalController.portToTerminal(player, false, null, false);
            } else {
                player.changeDimension(Constants.GlobalDimension);
                player.Position = new Position(0, 0, 72);
                InvokeController.AddTimedInvoke("Screen-Out-Fader", (i) => player.fadeScreen(false, 1000), TimeSpan.FromSeconds(3), false);
            }

        }

        private static void updatePlayers(IInvoke ivk) {
            foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                //Only try Update if everything is loaded
                try {
                    if(player.getCharacterFullyLoaded()) {
                        updatePlayer(player);
                    }
                } catch(Exception ex) {
                    Logger.logException(player, ex);
                }
            }
        }

        private static void updatePlayer(IPlayer player) {
            var data = player.getCharacterData();
            if(data != null) {
                if(!player.isKnockedOut() && !player.hasState(PlayerStates.InTerminal) && Config.IsEnableHunger) {
                    if(data.Hunger - 1.1 > 0) {
                        data.Hunger -= 1.1;
                    } else {
                        data.Hunger = 0;
                        player.sendNotification(NotifactionTypes.Warning, "Du hast großen Hunger und beginnst dich schwach zu fühlen! Du solltest etwas essen!", "Hunger leer");
                    }

                    if(data.Thirst - 1.4 > 0) {
                        data.Thirst -= 1.4;
                    } else {
                        data.Thirst = 0;
                        player.sendNotification(NotifactionTypes.Warning, "Du hast großen Durst und beginnst dich schwach zu fühlen! Du solltest etwas trinken!", "Durst leer");
                    }

                    if(data.Energy - 0.75 > 0) {
                        data.Energy -= 0.75;
                    } else {
                        data.Energy = 0;
                        player.sendNotification(NotifactionTypes.Warning, "Du hast keine Energie mehr und beginnst dich schwach zu fühlen! Du solltest etwas zu dir nehmen!", "Energie leer");
                    }

                    if(data.Hunger <= 0 || data.Thirst <= 0 || data.Energy <= 0) {
                        player.emitClientEvent("TOGGLE_HUNGER_MODE", true);
                        player.setTimeCycle("HUNGER", "drug_drive_blend01", 1.15f);
                    } else {
                        player.emitClientEvent("TOGGLE_HUNGER_MODE", false);
                        player.stopTimeCycle("HUNGER");
                    }
                }

                data.Age += (int)Constants.PLAYER_UPDATE_INTERVALL.TotalMilliseconds;
                data.Health = player.Health;

                if(data.CurrentPlayTimeDate.Date == DateTime.Now.Date) {
                    data.CurrentPlayTime += (int)Constants.PLAYER_UPDATE_INTERVALL.TotalMilliseconds;
                } else {
                    data.CurrentPlayTimeDate = DateTime.Now;
                    data.CurrentPlayTime = 0;
                }

                //Update the Hud
                player.updateHud();
            }
        }

        private static void updatePlayersDatabase(IInvoke ivk) {
            foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                if(player.getCharacterFullyLoaded()) {
                    updatePlayerDatabase(player);
                }
            }
        }

        private static void updatePlayerDatabase(IPlayer player) {
            try {
                int Id = player.getCharacterId();

                if(Id > 0) {
                    using(var db = new ChoiceVDb()) {
                        var row = db.characters.FirstOrDefault(c => c.id == Id);
                        if(row != null) {
                            var data = player.getCharacterData();

                            if(data != null) {
                                row.hunger = data.Hunger;
                                row.thirst = data.Thirst;
                                row.energy = data.Energy;

                                row.health = Math.Min((int)player.Health, 200);
                                row.age = data.Age;

                                row.position = player.Position.ToJson();
                                row.rotation = player.Rotation.ToJson();

                                row.state = (int)data.State;
                                row.stateUpdateDate = data.StatusUpdateDate;

                                row.cash = data.Cash;

                                row.currentPlayTime = data.CurrentPlayTime;
                                row.currentPlayTimeDate = data.CurrentPlayTimeDate;

                                db.SaveChanges();
                            }
                        }
                    }
                }
            } catch(Exception e) {
                Logger.logException(e, "updatePlayerDatabase");
            }
        }

        public static void resetPlayerPermanentData(int charId, string dataName) {
            var online = ChoiceVAPI.FindPlayerByCharId(charId);

            if(online != null) {
                online.resetPermantData(dataName);
            } else {
                using(var db = new ChoiceVDb()) {
                    var data = db.characterdata.Find(charId, dataName);
                    if(data != null) {
                        db.characterdata.Remove(data);
                    } else {
                        Logger.logWarning(LogCategory.System, LogActionType.Blocked, $"resetPermantData: Tried to remove data {dataName} which was not found!");
                    }

                    db.SaveChanges();
                }
            }
        }
    }
}
