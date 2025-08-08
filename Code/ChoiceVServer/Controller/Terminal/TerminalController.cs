using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.FsDatabase;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller {
    public class TerminalController : ChoiceVScript {
        public CollisionShape Terminal;

        public static Position TerminalFlightArrivePosition;
        public static Rotation TerminalFlightArriveRotation;
        public static Position TerminalPortPosition;
        public static float XDiv = 5;

        public static terminalflight CurrentFlight;
        private static bool Sent10minWarning = false;
        private static bool Sent5minWarning = false;
        private static bool Sent2minWarning = false;

        public static int TerminalDimension = 999999;

        private static List<SoundController.Sounds> AllAirportSounds = new List<SoundController.Sounds> {
            SoundController.Sounds.Airport1, SoundController.Sounds.Airport2, SoundController.Sounds.Airport3,
            SoundController.Sounds.Airport4, SoundController.Sounds.Airport5, SoundController.Sounds.Airport6,
            SoundController.Sounds.Airport7, SoundController.Sounds.Airport8, SoundController.Sounds.Airport9,
            SoundController.Sounds.Airport10,
        };

        private static List<SoundController.Sounds> PlayedSounds = new List<SoundController.Sounds>();

        public TerminalController() {
            TerminalFlightArrivePosition = new Position(-1067.2748f, -2788.6682f, 22.5f);
            TerminalFlightArriveRotation = new Rotation(0f,0f, -0.5f);

            TerminalPortPosition = new Position(0, -27.8f, 1.1f);

            InteractionController.addPlayerInteractionElement(
                new ConditionalGeneratedPlayerInteractionMenuElement(
                    (s, t) => new ClickMenuItem("Spieler aus Terminal porten", "Porte den Spieler aus dem Terminal. Die Markenrewards werden trotzdem ausgeführt", "", "SUPPORT_TERMINAL_PORT_PLAYER_OUT", MenuItemStyle.yellow).needsConfirmation("Aus Terminal porten?", "Wirklich aus dem Terminal porten?"),
                    s => s is IPlayer sender && sender.getCharacterData().AdminMode,
                    t => t is IPlayer target && target.hasState(Constants.PlayerStates.InTerminal)
                )
            );
            EventController.addMenuEvent("SUPPORT_TERMINAL_PORT_PLAYER_OUT", onSupportTerminalPortPlayerOut);

            Terminal = CollisionShape.Create(new Position(17.301098f, -0.50109863f, 1f), 62, 70, 0, true, false);
            Terminal.ZDiv = 10;
            Terminal.withRestrictedActions();
            Terminal.OnEntityExitShape += onPlayerExitShape;

            InvokeController.AddTimedInvoke("Check-Flights", checkFlights, TimeSpan.FromMinutes(0.97), true);

            EventController.addMenuEvent("REGISTER_TO_FLIGHT", onRegisterToFlight);
            EventController.addEvent("PLAYER_INTRO_CUTSCENE_FINISHED", onIntroCutsceneFinished);

            selectNextFlight();

            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnect;

            #region Support

            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    1,
                    SupportMenuCategories.Einreise,
                    "Einreiseflüge einstellen",
                    supportFlightGenerator
                )
            );
            EventController.addMenuEvent("SUPPORT_CREATE_TERMINAL_FLIGHT", onSupportCreateTerminalFlight);
            
            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    1,
                    SupportMenuCategories.Einreise,
                    (p) => new ClickMenuItem("In/Aus Terminal porten", "Porte dich in oder aus dem Terminal", "", "SUPPORT_TERMINAL_PORT_PLAYER_IN_OUT")
                        .needsConfirmation("In/Aus Terminal porten?", "Wirklich in oder aus dem Terminal porten?")
                )
            );

            #endregion
        }

        private void onPlayerConnect(IPlayer player, character character) {
            if(player.hasData("NOT_YET_LEFT_TERMINAL")) {
                player.changeDimension(TerminalDimension);
                TerminalShopController.refreshPlayerTerminalDisplay(player);
                player.addState(Constants.PlayerStates.InTerminal);
                player.Position = TerminalPortPosition;
            }
        }

        public static bool isPlayerInTerminal(IPlayer player) {
            return player.hasState(Constants.PlayerStates.InTerminal);
        }

        private static void selectNextFlight() {
            using(var db = new ChoiceVDb()) {
                foreach(var flight in db.terminalflights.Where(f => f.hasFlown == 0)) {
                    if(CurrentFlight == null || CurrentFlight.date < flight.date) {
                        CurrentFlight = flight;
                    }
                }
            }

            Sent10minWarning = false;
            Sent5minWarning = false;
            Sent2minWarning = false;
        }

        private void onPlayerExitShape(CollisionShape shape, IEntity entity) {
            var player = entity as Player;

            if(player.hasData("NOT_YET_LEFT_TERMINAL")) {
                InvokeController.AddTimedInvoke("Test-If-Player-Left-Terminal", (i) => {
                    if(!Terminal.IsInShape(player.Position) && player.getAdminLevel() <= 0) {
                        player.ban("Flugterminal vor Abflug verlassen! Melde dich im Support!");
                    }
                }, TimeSpan.FromSeconds(5), false);
            }

            TerminalShopController.refreshPlayerTerminalDisplay(player);
        }

        private void checkFlights(IInvoke obj) {
            var noticeSent = false;
            if(CurrentFlight != null) {
                if(DateTime.Now > CurrentFlight.date) {
                    var players = Terminal.AllEntities.Where(e => e is IPlayer player && player.hasData("TERMINAL_REGISTERED_FOR_FLIGHT")).OrderBy(p => ((string)p.getData("TERMINAL_REGISTERED_FOR_FLIGHT")).FromJson<DateTime>()).Cast<IPlayer>().ToList();

                    var count = 0;
                    foreach(var player in players) {
                        if(count < CurrentFlight.capacity) {
                            if(player.hasState(Constants.PlayerStates.InCharCreator)) {
                                continue;
                            }
                            count++;
                            startLeaveTerminal(player, true);
                        } else {
                            player.sendBlockNotification("Der Flug war leider überbucht, du konntest nicht mitfliegen! Plätze werden der Reihe nach (Flugbereitschafts-Datum) vergeben", "Flug überrbucht");
                        }
                    }

                    using(var db = new ChoiceVDb()) {
                        var find = db.terminalflights.Find(CurrentFlight.id);

                        if(find != null) {
                            find.hasFlown = 1;
                            db.SaveChanges();
                        }

                        db.SaveChanges();
                    }

                    CurrentFlight = null;
                    selectNextFlight();
                }

                if(CurrentFlight != null) {
                    var diff = CurrentFlight.date - DateTime.Now;
                    if(!Sent10minWarning && diff < TimeSpan.FromMinutes(1) && diff > TimeSpan.FromMinutes(8)) {
                        Sent10minWarning = true;
                        noticeSent = true;
                        SoundController.playSoundAtCoords(new Position(8, 1, 7), 60, SoundController.Sounds.Terminal10min, 1.5f, "mp3", false, p => p.getCharacterFullyLoaded() && !p.hasState(Constants.PlayerStates.InCharCreator));
                    }

                    if(!Sent5minWarning && diff < TimeSpan.FromMinutes(6) && diff > TimeSpan.FromMinutes(4)) {
                        Sent5minWarning = true;
                        noticeSent = true;
                        SoundController.playSoundAtCoords(new Position(8, 1, 7), 60, SoundController.Sounds.Terminal5min, 1.5f, "mp3", false, p => p.getCharacterFullyLoaded() && !p.hasState(Constants.PlayerStates.InCharCreator));
                    }

                    if(!Sent2minWarning && diff < TimeSpan.FromMinutes(2)) {
                        Sent2minWarning = true;
                        noticeSent = true;
                        SoundController.playSoundAtCoords(new Position(8, 1, 7), 60, SoundController.Sounds.Terminal2min, 1.5f, "mp3", false, p => p.getCharacterFullyLoaded() && !p.hasState(Constants.PlayerStates.InCharCreator));
                    }
                }
            }

            if(!noticeSent && !Config.IsDevServer) {
                var rand = new Random();
                if(rand.NextDouble() < 0.3) {
                    if(PlayedSounds.Count == 0) {
                        PlayedSounds = AllAirportSounds.Shuffle().ToList();
                    }

                    var selected = PlayedSounds.First();
                    SoundController.playSoundAtCoords(new Position(0, 0, 10), 60, selected, 1.5f, "mp3", false, p => p.getCharacterFullyLoaded() && !p.hasState(Constants.PlayerStates.InCharCreator));
                    PlayedSounds.Remove(selected);
                }
            }
        }

        public static void startLeaveTerminal(IPlayer player, bool startCutscene) {
            if(player.getCharacterData().AdminMode && startCutscene) {
                SupportController.activateAdminMode(player);
            }
            
            AnimationController.stopAllAnimationForPlayer(player, true);
            player.removeState(Constants.PlayerStates.InTerminal);
            player.addState(Constants.PlayerStates.OnTerminalFlight);
            player.closeMenu();
            WebController.closePlayerCef(player);
            player.resetPermantData("NOT_YET_LEFT_TERMINAL");
            player.resetPermantData("TERMINAL_TOKENS");
            TerminalShopController.refreshPlayerTerminalDisplay(player);
            player.resetPermantData("TERMINAL_REGISTERED_FOR_FLIGHT");
            
            if(!startCutscene) return;
            player.emitClientEvent("PLAYER_INTRO_CUTSCENE", player.getCharacterData().Gender.ToString());
        }

        private static void stopLeaveTerminal(IPlayer player) {
            player.changeDimension(Constants.GlobalDimension);
            player.removeState(Constants.PlayerStates.Busy);
            player.removeState(Constants.PlayerStates.OnTerminalFlight);

            if(player.getCash() < 10000) { 
                player.addCash(10000);
            }

            //Add Character to FS
            using(var fsDb = new ChoiceVFsDb()) {
                var newCit = new executive_person_file {
                    charId = player.getCharacterId(),
                    name = player.getCharacterName(),
                    birthdate = player.getCharacterData().DateOfBirth.ToString("yyyy-MM-dd"),
                    driversLicense = 0,
                    createDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    gender = player.getCharacterData().Gender == 'F' ? "Weiblich" : "Männlich",
                    socialSecurityNumber = player.getCharacterData().SocialSecurityNumber,
                    affiliation = "",
                    info = "",
                    wantedState = "",
                };

                fsDb.executive_person_files.Add(newCit);
                fsDb.SaveChanges();

                TerminalShopController.onPlayerLeaveAirport(player, ref newCit);
            }

            var socialCf = InventoryController.getConfigItemForType<SocialSecurityCard>();
            player.getInventory().addItem(new SocialSecurityCard(socialCf, player));
        }

        private bool onIntroCutsceneFinished(IPlayer player, string eventName, object[] args) {
            player.Frozen = true;
            player.fadeScreen(true, 0);
            
            stopLeaveTerminal(player);

            player.Spawn(TerminalFlightArrivePosition);
            InvokeController.AddTimedInvoke("Unfreeze Player", (i) => {
                player.Rotation = TerminalFlightArriveRotation;
                player.Frozen = false;
                player.fadeScreen(false, 2000);
                player.Position = TerminalFlightArrivePosition;
            }, TimeSpan.FromSeconds(15), false);
            return true;
        }

        private bool onSupportTerminalPortPlayerOut(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var target = (IPlayer)data["InteractionTarget"];

            startLeaveTerminal(target, false);
            stopLeaveTerminal(target);

            target.Spawn(TerminalFlightArrivePosition);

            return true;
        }

        private bool onRegisterToFlight(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(player.hasData("TERMINAL_REGISTERED_FOR_FLIGHT")) {
                player.resetPermantData("TERMINAL_REGISTERED_FOR_FLIGHT");
                player.sendNotification(Constants.NotifactionTypes.Warning, "Du hast dich erfolgreich vom Flug abgemeldet! Du wirst nicht beim nächstmöglichen Flug mitfiegen", "Flugbereitschaft entfernt", Constants.NotifactionImages.Plane);
            } else {
                player.setPermanentData("TERMINAL_REGISTERED_FOR_FLIGHT", DateTime.Now.ToJson());
                player.sendNotification(Constants.NotifactionTypes.Success, "Du hast dich flugbereit gesetzt! Du wirst beim nächstmöglichen Flug mitfliegen, falls du online bist!", "Flugbereitschaft", Constants.NotifactionImages.Plane);
            }

            return true;
        }

        public static void portToTerminal(IPlayer player, bool giveTokens, string outfit = null, bool setOutfit = true) {
            player.setPermanentData("NOT_YET_LEFT_TERMINAL", "1");
            var rand = new Random();
            player.Spawn(new Position(Convert.ToSingle(TerminalPortPosition.X + (rand.NextDouble() - 0.5) * 2 * XDiv), TerminalPortPosition.Y, TerminalPortPosition.Z));
            InvokeController.AddTimedInvoke("Screen-Out-Fader", (i) => player.fadeScreen(false, 1000), TimeSpan.FromSeconds(3), false);
            player.changeDimension(TerminalDimension);

            setPlayerStartInventory(player);

            if(setOutfit) {
                if(outfit != null) {
                    if(!Config.IsStressTestActive) {
                        setPlayerStartOutfit(player, outfit);
                    } else {
                        setPlayerStartOutfit(player, "CASUAL");
                    }
                } else {
                    setPlayerStartOutfit(player, "CASUAL");
                }
            } else {
                var clothingItems = player.getInventory().getItems<ClothingItem>(i => i.IsEquipped);
                foreach(var item in clothingItems) {
                    item.fastEquip(player);
                }
            }

            if(giveTokens) {
                TerminalShopController.addOrRemovePlayerTokens(player, 20);
            }

            TerminalShopController.refreshPlayerTerminalDisplay(player);
        }

        private static Dictionary<string, int> OUTFIT_VARIATIONS = new Dictionary<string, int> {
            { "CASUAL", 4 },
            { "SMART_CASUAL", 2 },
            { "BUSINESS", 2 },
        };

        private static void setPlayerStartOutfit(IPlayer player, string outfit) { 
            var amount = OUTFIT_VARIATIONS[outfit];
            var rand = new Random().Next(1, amount + 1);
            
            //TODO Change to work differently
            //var outfitItems = ClothingController.getConfigOutfit($"AIRPORT_{outfit}_{rand}", player);

            //foreach(var item in outfitItems) {
            //    player.getInventory().addItem(item, true);
            //    item.fastEquip(player);
            //}
        }

        public static void setPlayerStartInventory(IPlayer player) {
            var inv = player.getInventory();

            var noteCf = InventoryController.getConfigItemForType<Note>();
            var noteItem = InventoryController.createItem(noteCf, 1);
            inv.addItem(noteItem);

            //Remove Cef
            //var flipBookCf = InventoryController.getConfigItemForType<FlipBook>();
            //var flipBook = new FlipBook(flipBookCf, "AIRPORT_FLYER", "Flyer vom Flughafen Terminal");
            //inv.addItem(flipBook);
        }

        public static Position getPortTerminalPosition() {
            var rand = new Random();
            return new Position(Convert.ToSingle(TerminalPortPosition.X + (rand.NextDouble() - 0.5) * 2 * XDiv), TerminalPortPosition.Y, TerminalPortPosition.Z);
        }

        #region Support

        private Menu supportFlightGenerator(IPlayer player) {
            var menu = new Menu("Flüge einstellen", "Stelle neue Flüge ein");

            var createMenu = new Menu("Flug erstellen", "Gib die Daten ein");
            createMenu.addMenuItem(new InputMenuItem("Datum", "Gib das Datum der Fluges ein. Es sollte im Format Tag.Monat.Jahr sein. z.B. 19.4.2019", "DD.MM.YYYY", ""));
            createMenu.addMenuItem(new InputMenuItem("Uhrzeit", "Gib die Uhrzeit der Fluges ein. Sie sollte im Format Stunde:Minute sein. z.B. 20:15", "HH:MM", ""));
            createMenu.addMenuItem(new InputMenuItem("Flugnummer", "Gib die Flugnummer. Sie dient zur Identifizierung regelmäßiger Flüge. z.B. HE8123", "z.B. HE8123", ""));
            createMenu.addMenuItem(new InputMenuItem("Kapazität", "Gib die Kapazität des Fluges an. Diese gibt an wieviele Spieler maximal geportet werden. Kann höhrer sein, je mehr Einreisebeamte anwesend sind", "z.B. 20", ""));
            createMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle den Flug wie angegeben", "SUPPORT_CREATE_TERMINAL_FLIGHT", MenuItemStyle.green));

            menu.addMenuItem(new MenuMenuItem(createMenu.Name, createMenu, MenuItemStyle.green));

            var listMenu = new Menu("Kommende Flüge", "Was möchtest du tun?");
            using(var db = new ChoiceVDb()) {
                foreach(var flight in db.terminalflights.Where(t => t.hasFlown == 0).OrderBy(f => f.date).ToList()) {
                    var flightMenu = new Menu($"{flight.number}: {flight.date.ToString("dd.MM HH:mm")}", "Siehe alle Daten");
                    flightMenu.addMenuItem(new StaticMenuItem("Abflugdatum", $"Der Flug fliegt am {flight.date.ToString("dd.MM")} um {flight.date.ToString("HH:mm")} ab", flight.date.ToString("dd.MM HH:mm")));
                    flightMenu.addMenuItem(new StaticMenuItem("Kapazität", $"Der Flug hat eine Kapazität von {flight.capacity}", flight.capacity.ToString()));
                    flightMenu.addMenuItem(new ClickMenuItem("Flug löschen", "Lösche den angegebenen Flug", "", "SUPPORT_DELETE_FLIGHT", MenuItemStyle.red).needsConfirmation("Flug löschen?", "Flug wirklich löschen?").withData(new Dictionary<string, dynamic> { { "Flight", flight } }));

                    listMenu.addMenuItem(new MenuMenuItem(flightMenu.Name, flightMenu));
                }
            }
            menu.addMenuItem(new MenuMenuItem(listMenu.Name, listMenu));
            return menu;
        }

        private bool onSupportCreateTerminalFlight(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var dateEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var timeEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var numberEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
            var capacityEvt = evt.elements[3].FromJson<InputMenuItemEvent>();

            DateTime time = DateTime.MaxValue;
            string flightNumber = numberEvt.input;
            int capacity = -1;

            try {
                var dateStrSplit = dateEvt.input.Split(".");
                var timeStrSplit = timeEvt.input.Split(":");

                time = new DateTime(int.Parse(dateStrSplit[2]), int.Parse(dateStrSplit[1]), int.Parse(dateStrSplit[0]), int.Parse(timeStrSplit[0]), int.Parse(timeStrSplit[1]), 0);


                capacity = int.Parse(capacityEvt.input);
            } catch(Exception) {
                player.sendBlockNotification("Eingabe falsch!", "");
            }

            using(var db = new ChoiceVDb()) {
                var flight = new terminalflight {
                    number = flightNumber,
                    date = time,
                    capacity = capacity,
                    hasFlown = 0,
                };

                db.terminalflights.Add(flight);
                db.SaveChanges();

                if(CurrentFlight == null || CurrentFlight.date > time) {
                    CurrentFlight = flight;
                }
            }
            player.sendNotification(Constants.NotifactionTypes.Success, $"Flug {flightNumber} am {dateEvt.input} um {timeEvt.input} wurde erfolgreich erstellt", "Flug erstellt", Constants.NotifactionImages.Plane);

            return true;
        }

        #endregion
    }

    public class NPCTerminalLeaveModule : NPCModule {
        public NPCTerminalLeaveModule(ChoiceVPed ped) : base(ped) { }
        public NPCTerminalLeaveModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, settings) { }

        public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
            return new StaticMenuItem("Flughafen Terminal Modul", "Ermöglicht den Spielern hier das Terminal zu verlassen.", "");
        }

        public override List<MenuItem> getMenuItems(IPlayer player) {
            var flights = new List<terminalflight>();
            using(var db = new ChoiceVDb()) {
                flights = db.terminalflights.Where(f => f.date > DateTime.Now).OrderBy(f => f.date).ToList();
            }

            var list = new List<MenuItem>();

            if(flights.Count > 0) {
                list.Add(new StaticMenuItem("Nächste Flugzeit", $"Der nächste Flug geht am {TerminalController.CurrentFlight.date.ToString("d.M.yyyy H:m")}", $"{TerminalController.CurrentFlight.date.ToString("d.M.yyyy H:m")}"));
                list.Add(new StaticMenuItem("Nächste Flugkapazität", $"Der nächste Flug hat eine Kapazität von: {TerminalController.CurrentFlight.capacity}", $"{TerminalController.CurrentFlight.capacity}"));

                var subMenu = new Menu("Abflugzeiten", "Siehe dir die versch. Flüge an");
                foreach(var flight in flights) {
                    subMenu.addMenuItem(new StaticMenuItem($"{flight.number}", $"Der Flug {flight.number} geht am {flight.date.ToString("d.M.yyyy H:m")} und hat eine Kapazität von {flight.capacity}", $"{flight.date.ToString("d.M.yyyy H:m")}"));
                }
                list.Add(new MenuMenuItem(subMenu.Name, subMenu));
            } else {
                list.Add(new StaticMenuItem("Aktuell kein Flug", $"Es gibt aktuell keine Flüge! Melde dich im Support für weitere Fragen", "", MenuItemStyle.red));
            }

            if(player.hasData("TERMINAL_REGISTERED_FOR_FLIGHT")) {
                list.Add(new ClickMenuItem("Flugbereitschaft entfernen", "Entferne deine Flugbereitschaft. Dadurch wirst du nicht beim nächstmöglichen Flug mitfliegen.", "", "REGISTER_TO_FLIGHT", MenuItemStyle.yellow).needsConfirmation("Wirklich abmelden?", "Wirklich vom Flug abmelden?"));
            } else {
                list.Add(new ClickMenuItem("Flugbereitschaft hinzufügen", "Setze dich als flugbereit. Solltest du online sein, wenn ein Flug geht wirst du an ihm teilnehmen.", "", "REGISTER_TO_FLIGHT", MenuItemStyle.green).needsConfirmation("Wirklich flugbereit?", "Wirklich zum Flug anmelden?"));
            }

            return list;
        }

        public override void onRemove() { }
    }


}
