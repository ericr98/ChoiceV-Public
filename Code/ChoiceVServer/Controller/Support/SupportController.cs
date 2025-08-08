using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller;

public delegate void OnAdminModeToggleDelegate(IPlayer player, bool toggle);

// ReSharper disable once ClassNeverInstantiated.Global 
public class SupportController : ChoiceVScript {
    private static readonly List<SupportMenuElement> AllMenuElements = new();

    private static readonly Dictionary<int, bool> AreSocialClubsShown = new();

    public static OnAdminModeToggleDelegate AdminModeToggleDelegate;

    public SupportController() {
        CharacterController.addSelfMenuElement(
            new ConditionalPlayerSelfMenuElement(
                "Support-Menü",
                onOpenSupportMenu,
                s => s.getCharacterData().AdminMode,
                MenuItemStyle.yellow
            )
        );

        loadBasicSupportMenu();
        EventController.addMenuEvent("SUPPORT_SPAWN_VEHICLE", supportSpawnVehicle);
        EventController.addMenuEvent("SUPPORT_SPAWN_VEHICLE_WITH_STUFF", supportSpawnVehicleWithStuff);
        EventController.addMenuEvent("SUPPORT_SHOW_SOCIALCLUBS", supportShowSocialclubs);
        EventController.addMenuEvent("SUPPORT_WHITELIST_SOCIALCLUB", supportWhitelistSocialclub);
        EventController.addMenuEvent("SUPPORT_TP_TO_WAYPOINT", supportTpToWaypoint);

        EventController.addKeyEvent("ADMIN_MODE", ConsoleKey.F5, "Admin Mode aktivieren", onActivateAdminMode, true);
        EventController.addKeyEvent("ADMIN_FASTACTION", ConsoleKey.F6, "Admin Mode Schnellaktion", onAdminModeFastAktion);

        var menu = new Menu("Notification senden", "Sende eine Notification an alle Spieler");

        var typesList = ((NotifactionTypes[])Enum.GetValues(typeof(NotifactionTypes))).Select(n => n.ToString()).ToArray();
        menu.addMenuItem(new ListMenuItem("Nachrichtentyp", "Typ der Nachricht (Vor allem Farbe)", typesList, ""));

        var categoryList = ((NotifactionImages[])Enum.GetValues(typeof(NotifactionImages))).Select(n => n.ToString()).ToArray();
        menu.addMenuItem(new ListMenuItem("Icon", "Wähle das Icon der Notification", categoryList, ""));
        menu.addMenuItem(new InputMenuItem("Nachricht", "Wähle die Nachricht der Notification", "", ""));
        menu.addMenuItem(new InputMenuItem("Kurznachricht", "Wähle die Kurznachricht (Angezeigt im Inventar)", "", ""));
        menu.addMenuItem(new MenuStatsMenuItem("Abschicken", "Schicke die Nachricht ab", "SUPPORT_SEND_NOTIFICATION", MenuItemStyle.green).needsConfirmation("Nachricht abschicken?",
            "Nachricht wirklich abschicken?"));

        addSupportMenuElement(
            new StaticSupportMenuElement(
                () => menu,
                1,
                SupportMenuCategories.Misc,
                "Notification senden"
            )
        );

        EventController.addMenuEvent("SUPPORT_SEND_NOTIFICATION", onSupportSendNotification);

        EventController.addEvent("EDITOR_OPEN", (p, _, a) => {
            p.emitClientEvent("EDITOR_OPENED", a[0]);
            return true;
        });


        EventController.addEvent("SENT_HEAP_SNAPSHOT_PART", sendHeapSnapshotPart);
        EventController.addEvent("CLIENT_LOG", onClientLog);
    }
    
    public static void takeHeapSnapshot(IPlayer player, int chunkSize) {
        player.emitClientEvent("TAKE_HEAP_SNAPSHOT", chunkSize);
    }

    private static Dictionary<int, string> HeapSnapshotParts = new();

    private bool sendHeapSnapshotPart(IPlayer player, string eventName, object[] args) {
        var partId = int.Parse(args[0].ToString());
        var part = (string)args[1];
        var isLast = (bool)args[2];

        if(partId == 0) {
            Logger.logInfo(LogCategory.Support, LogActionType.Event, player, "Heap Snapshot started");
        }

        HeapSnapshotParts[partId] = part;

        if(isLast) {
            tryFinishHeapSnapshot(partId);
        }

        return true;
    }

    private static void tryFinishHeapSnapshot(int lastPartId) {
        for (var i = 0; i <= lastPartId; i++) {
            if(!HeapSnapshotParts.ContainsKey(i)) {
                InvokeController.AddTimedInvoke("HEAP_SNAPSHOT_PARTS", (i) => tryFinishHeapSnapshot(lastPartId), TimeSpan.FromSeconds(5), false);
                return;
            }
        }

        var allParts = HeapSnapshotParts.OrderBy(p => p.Key).Select(p => p.Value).ToList();
        var full = string.Join("", allParts);
        //Write the dump to a file

        var file = new FileInfo($"heap_snapshot_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.heapsnapshot");
        System.IO.File.WriteAllText(file.FullName, full);
        HeapSnapshotParts.Clear();

        Logger.logInfo(LogCategory.Support, LogActionType.Event, $"Heap Snapshot written to {file.FullName}");
    }

    public static void addSupportMenuElement(SupportMenuElement element) {
        AllMenuElements.Add(element);
    }

    public static void removeSupportMenuElement(Predicate<SupportMenuElement> predicate) {
        AllMenuElements.RemoveAll(predicate);
    }

    public static void toggleClientSideLoggingForPlayer(IPlayer player, bool toggle) {
        player.emitClientEvent("TOGGLE_CLIENT_LOGGING", toggle);
    }

    private bool onClientLog(IPlayer player, string eventname, object[] args) {
        var message = args[0].ToString();
       
        Logger.logTrace(LogCategory.Support, LogActionType.Event, player, message);

        return true;
    }

    private static bool onSupportSendNotification(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        try {
            if(data["PreviousCefEvent"] is not MenuStatsMenuItemEvent evt) {
                throw new InvalidCastException();
            }

            if(evt.elements.Length < 4) {
                throw new IndexOutOfRangeException($"{nameof(evt.elements)} needs at least 4 entries, but only has {evt.elements.Length}!");
            }

            var typesEvt = evt.elements[0].FromJson<ListMenuItemEvent>();
            var iconEvt = evt.elements[1].FromJson<ListMenuItemEvent>();
            var messageEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
            var shortMessageEvt = evt.elements[3].FromJson<InputMenuItemEvent>();

            var type = (NotifactionTypes)Enum.Parse(typeof(NotifactionTypes), typesEvt.currentElement);
            var icon = (NotifactionImages)Enum.Parse(typeof(NotifactionImages), iconEvt.currentElement);

            foreach(var p in ChoiceVAPI.GetAllPlayers()) {
                p.sendNotification(type, messageEvt.input, shortMessageEvt.input, icon);
            }
        } catch (Exception) {
            player.sendBlockNotification("Nix funktionieren, bedanke dich bei Randy", "");
        }

        return true;
    }

    private static Menu onOpenSupportMenu(IPlayer player) {
        var menu = new Menu("Support-Menü", "Was möchtest du tun?", false);

        foreach(var cat in (SupportMenuCategories[])Enum.GetValues(typeof(SupportMenuCategories))) {
            var els = AllMenuElements.Where(e => e.Category == cat).ToList();
            if(els.Count <= 0) {
                continue;
            }

            var catMenu = new Menu(cat.ToString(), $"Alle Optionen im {cat}");
            foreach(var el in els.Select(menuElement => menuElement.getMenuElement(player))) {
                if(el is MenuItem subMenuElement) {
                    catMenu.addMenuItem(subMenuElement);
                } else {
                    var subMenu = el as VirtualMenu;
                    catMenu.addMenuItem(new MenuMenuItem(subMenu?.Name, subMenu));
                }
            }

            menu.addMenuItem(new MenuMenuItem(catMenu.Name, catMenu));
        }

        return menu;
    }

    private bool onAdminModeFastAktion(IPlayer player, ConsoleKey key, string eventName) {
        if(player.getCharacterData().AdminMode && player.hasData("SUPPORT_FAST_ACTION_DATA")) {
            var callback = (Action)player.getData("SUPPORT_FAST_ACTION_DATA");
            callback.Invoke();
        }

        return true;
    }

    public static void setCurrentSupportFastAction(IPlayer player, Action callback) {
        player.setData("SUPPORT_FAST_ACTION_DATA", callback);
        player.sendNotification(NotifactionTypes.Info, "Support-Schnellaktion (Standard F6) wurde gesetzt!", "");
    }

    private static void loadBasicSupportMenu() {
        addSupportMenuElement(
            new StaticSupportMenuElement(
                () => {
                    var messageMenu = new Menu("Serverweite Nachricht", "Sende eine Nachricht an alle Spieler");
                    messageMenu.addMenuItem(new InputMenuItem("Titel", "Der Titel der Nachricht", "", ""));
                    messageMenu.addMenuItem(new InputMenuItem("Nachricht", "Die Nachricht die gesendet werden soll", "", ""));
                    messageMenu.addMenuItem(
                        new MenuStatsMenuItem("Abschicken", "Sende die Nachricht an alle Spieler", "SUPPORT_SEND_MESSAGE", MenuItemStyle.green).needsConfirmation("Nachricht abschicken?",
                            "Nachricht wirklich abschicken?"));
                    return messageMenu;
                },
                1,
                SupportMenuCategories.Support_Aktionen,
                "Serverweite Nachricht"
            )
        );
        EventController.addMenuEvent("SUPPORT_SEND_MESSAGE", onSupportSendMessage);

        addSupportMenuElement(
            new StaticSupportMenuElement(
                () => new InputMenuItem("Fahrzeug spawnen", "Spawne ein Fahrzeug mit angegeben Namen", "Modelname", "SUPPORT_SPAWN_VEHICLE"),
                2,
                SupportMenuCategories.Support_Aktionen
            )
        );

        addSupportMenuElement(
            new StaticSupportMenuElement(
                () => new InputMenuItem("Fahrzeug spawnen", "Spawne ein Fahrzeug mit angegeben Namen", "Modelname", "SUPPORT_SPAWN_VEHICLE"),
                2,
                SupportMenuCategories.Support_Aktionen
            )
        );


        addSupportMenuElement(
            new GeneratedSupportMenuElement(
                2,
                SupportMenuCategories.Support_Aktionen,
                "Angemeldetes Fahrzeug spawnen",
                (p) => {
                    var regVehicleMenu = new Menu("Angemeldetes Fahrzeug spawnen", "Gib die Daten ein");
                    regVehicleMenu.addMenuItem(new InputMenuItem("Spawn Name", "Der GTA Name des Fahrzeugs", "", ""));
                    regVehicleMenu.addMenuItem(new InputMenuItem("Nummernschild", "Leerlassen für zufällig", "", ""));

                    var list = ChoiceVAPI.FindNearbyPlayers(p.Position).Select(c => $"{c.getCharacterId()}-Spieler: {c.getCharacterShortenedName()}")
                        .Concat(CompanyController.AllCompanies.Select(c => $"{c.Value.Id}-Firma: {c.Value.Name}")).ToArray();

                    regVehicleMenu.addMenuItem(new InputMenuItem("Auf wen anmelden", "Wähle auf wen das Fahrzeug angemeldet werden soll", "", "").withOptions(list));
                    regVehicleMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle das angegebene Fahrzeug", "", "SUPPORT_SPAWN_VEHICLE_WITH_STUFF"));

                    return regVehicleMenu;
                }
            )
        );

        addSupportMenuElement(
            new StaticSupportMenuElement(
                () => new ClickMenuItem("Socialclubs anzeigen/verstecken", "Lasse dir die Socialclubs und die AccountId der Spieler anzeigen/verstecke sie", "", "SUPPORT_SHOW_SOCIALCLUBS"),
                1,
                SupportMenuCategories.Support_Aktionen
            )
        );

        addSupportMenuElement(
            new StaticSupportMenuElement(
                () => new InputMenuItem("Socialclub whitelisten", "Whiteliste einen Socialclub Namen", "Socialclub", "SUPPORT_WHITELIST_SOCIALCLUB").needsConfirmation("Wirklich whitelisten?",
                    "Socialclub wirklich whitelisten?"),
                1,
                SupportMenuCategories.Support_Aktionen
            )
        );

        addSupportMenuElement(
            new StaticSupportMenuElement(
                () => new InputMenuItem("Spieler bannen", "Banne einen Spieler", "", "SUPPORT_BAN_PLAYER")
                    .withOptions(ChoiceVAPI.GetAllPlayers().Select(p => p.SocialClubName).ToArray()),
                1,
                SupportMenuCategories.Support_Aktionen
            )
        );
        EventController.addMenuEvent("SUPPORT_BAN_PLAYER", onSupportBanPlayer);
        EventController.addMenuEvent("SUPPORT_PLAYER_BAN_CONFIRM", onSupportBanPlayerConfirm);

        addSupportMenuElement(
           new StaticSupportMenuElement(
                () => new ClickMenuItem("Freecam aktivieren", "Die Freecam kann mit Y (zur aktuellen Position teleportieren) oder X (Zurück zum Charakter springen) beendet werden", "", "SUPPORT_START_FREECAM"),
                1,
                SupportMenuCategories.Support_Aktionen
            )
        );
        EventController.addMenuEvent("SUPPORT_START_FREECAM", onSupportStartFreecam);

        addSupportMenuElement(
            new StaticSupportMenuElement(
                () => new ClickMenuItem("Zu Wegpunkt porten", "Porte dich zum Wegpunkt", "", "SUPPORT_PORT_TO_WAYPOINT"),
                1,
                SupportMenuCategories.Support_Aktionen
            )
        );
        EventController.addMenuEvent("SUPPORT_PORT_TO_WAYPOINT", onSupportPortToWaypoint);


        addSupportMenuElement(
            new StaticSupportMenuElement(
                () => new InputMenuItem("Zu Spieler porten", "Porte dich zu einem Spieler nach Socialclub Namen", "", "SUPPORT_PORT_PLAYER")
                    .withOptions(ChoiceVAPI.GetAllPlayers().Select(p => p.SocialClubName).ToArray())
                    .withData(new Dictionary<string, dynamic> { { "To", true } }),
                1,
                SupportMenuCategories.Support_Aktionen
            )
        );

        addSupportMenuElement(
            new StaticSupportMenuElement(
                () => new InputMenuItem("Spieler zu dir porten", "Porte einen Spieler zu dir", "", "SUPPORT_PORT_PLAYER")
                    .withOptions(ChoiceVAPI.GetAllPlayers().Select(p => p.SocialClubName).ToArray())
                    .withData(new Dictionary<string, dynamic> { { "To", false } }),
                1,
                SupportMenuCategories.Support_Aktionen
            )
        );
        EventController.addMenuEvent("SUPPORT_PORT_PLAYER", onSupportPortPlayer);


        addSupportMenuElement(
            new StaticSupportMenuElement(
                () => new ClickMenuItem("Porte alle Spieler zu dir", "Porte einen Spieler zu dir", "", "SUPPORT_PORT_ALL_PLAYER")
                    .needsConfirmation("Alle Spieler porten?", "Wirklich alle Spieler porten"),
                3,
                SupportMenuCategories.Support_Aktionen
            )
        );
        EventController.addMenuEvent("SUPPORT_PORT_ALL_PLAYER", onSupportPortAllPlayer);

        addSupportMenuElement(
            new StaticSupportMenuElement(
                () => {
                    var itemMenu = new Menu("Item erstellen", "Erstelle Items");
                    itemMenu.addMenuItem(
                        new InputMenuItem("Item", "Wähle aus, welches Item erstellt werden soll", "", "").withOptions(InventoryController.getConfigItems(i => true).Select(i => $"{i.configItemId}: {i.name}")
                            .ToArray()));
                    itemMenu.addMenuItem(new InputMenuItem("Menge", "Wähle wieviele Items erstellt werden sollen", "Leer heißt: 1", InputMenuItemTypes.number, ""));
                    itemMenu.addMenuItem(new InputMenuItem("Qualität", "Wähle die Qualität des Items. Leerlassen wenn egal", "Leer heißt: Keine Qualität", InputMenuItemTypes.number, ""));
                    itemMenu.addMenuItem(new MenuStatsMenuItem("Items erstellen", "Erstelle die Items", "SUPPORT_ON_CREATE_ITEMS", MenuItemStyle.green));
        
                    return new MenuMenuItem("Item erstellen", itemMenu);
                },
                2,
                SupportMenuCategories.Support_Aktionen
            )
        );
        EventController.addMenuEvent("SUPPORT_ON_CREATE_ITEMS", onSupportCreateItems);
    }

    private static bool onSupportSendMessage(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
        var title = evt.elements[0].FromJson<InputMenuItemEvent>();
        var message = evt.elements[1].FromJson<InputMenuItemEvent>();

        ChoiceVAPI.sendServerWideMessage(title.input, message.input);

        return true;
    }

    private static bool onSupportBanPlayer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var evt = menuItemCefEvent as InputMenuItemEvent;
        var target = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.SocialClubName == evt.input);

        if(target != null) {
            var menu = MenuController.getConfirmationMenu($"{target.SocialClubName} bannen?", $"{target.SocialClubName} bannen?", "SUPPORT_PLAYER_BAN_CONFIRM",
                new Dictionary<string, dynamic> { { "Player", target } });
            player.showMenu(menu);
        } else {
            player.sendBlockNotification("Spieler nicht gefunden!", "");
        }

        return true;
    }

    private static bool onSupportBanPlayerConfirm(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var target = data["Player"] as IPlayer;

        target.ban("Gebannt!", true);
        return true;
    }

    private static bool onSupportCreateItems(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

        var itemEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
        var amountEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
        var qualityEvt = evt.elements[2].FromJson<InputMenuItemEvent>();

        var split = itemEvt.input.Split(": ");

        var amount = 1;
        var quality = -1;

        try {
            amount = int.Parse(amountEvt.input);
            quality = int.Parse(qualityEvt.input);
        } catch (Exception) { }

        var cfg = InventoryController.getConfigById(int.Parse(split[0]));
        var items = InventoryController.createItems(cfg, amount, quality);
        foreach(var item in items) {
            player.getInventory().addItem(item, true);
        }

        player.sendNotification(NotifactionTypes.Success, $"Item mit der ID {split[0]} {amount}x erstellt {items.First().Name}", "");
        Logger.logDebug(LogCategory.Support, LogActionType.Created, player, $"Player spawned items: {split[0]}: {items.First().Name} {amount}x");

        return true;
    }

    private static bool onSupportStartFreecam(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        player.emitClientEvent("FREE_CAM_MODE");
        Logger.logDebug(LogCategory.Support, LogActionType.Event, player, $"Player started Freecam");

        return true;
    }

    private bool supportTpToWaypoint(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        player.emitClientEvent("TP_TO_WAYPOINT");

        return true;
    }

    private static bool supportSpawnVehicle(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var inputEvt = menuItemCefEvent as InputMenuItemEvent;

        var vehicle = VehicleController.createVehicle(ChoiceVAPI.Hash(inputEvt?.input), new Position(player.Position.X, player.Position.Y, player.Position.Z + 0.1f), player.Rotation, player.Dimension,
            0, false, inputEvt?.input);

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

        return true;
    }

    private bool supportSpawnVehicleWithStuff(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

        var modelNameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
        var numberPlateEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
        var registerEvt = evt.elements[2].FromJson<InputMenuItemEvent>();

        player.Frozen = true;
        player.Collision = false;
        var vehicle = VehicleController.createVehicle(ChoiceVAPI.Hash(modelNameEvt.input), new Position(player.Position.X, player.Position.Y, player.Position.Z + 0.1f), player.Rotation,
            player.Dimension, 0, false, modelNameEvt.input);

        if(vehicle != null) {
            var cfg = InventoryController.getConfigItemForType<VehicleKey>();
            vehicle.Inventory.addItem(new VehicleKey(cfg, vehicle), true);
            vehicle.Inventory.addItem(new VehicleKey(cfg, vehicle), true);

            var cfg2 = InventoryController.getConfigItemForType<VehicleRegistrationCard>();
            var card = new VehicleRegistrationCard(cfg2, "", -1, vehicle, "");
            vehicle.Inventory.addItem(card, true);

            using(var db = new ChoiceVDb()) {
                var numberPlate = numberPlateEvt.input;
                if(string.IsNullOrEmpty(numberPlateEvt.input)) {
                    numberPlate = ChoiceVAPI.randomString(6);
                    while (db.vehiclesregistrations.FirstOrDefault(v => v.end == null && v.numberPlate == numberPlate) != null) {
                        numberPlate = ChoiceVAPI.randomString(6);
                    }
                }

                IPlayer registerPlayer = null;
                Company registerCompany = null;
                var id = int.Parse(registerEvt.input.Split("-")[0]);
                if(registerEvt.input.Contains("Spieler")) {
                    registerPlayer = ChoiceVAPI.FindPlayerByCharId(id);
                } else {
                    registerCompany = CompanyController.findCompanyById(id);
                }

                var newHis = new vehiclesregistration {
                    vehicleId = vehicle.VehicleId,
                    start = DateTime.Now,
                    end = null,
                    numberPlate = numberPlate,
                    ownerId = registerPlayer?.getCharacterId(),
                    companyOwnerId = registerCompany?.Id,
                };
                db.vehiclesregistrations.Add(newHis);

                db.SaveChanges();

                card.changeInfos(VehicleOwnerType.Player, registerPlayer != null ? registerPlayer.getCharacterId() : registerCompany.Id,
                    registerPlayer != null ? registerPlayer.getCharacterShortName() : registerCompany.ShortName, numberPlate, "Rigo Ross");

                InvokeController.AddTimedInvoke("SETTER", (i) => {
                    player.SetIntoVehicle(vehicle, 0);
                    player.Frozen = false;
                    player.Collision = true;
                }, TimeSpan.FromSeconds(4), false);

                VehicleController.setNumberPlateText(vehicle, numberPlate);
            }
        }


        return true;
    }

    private static bool onSupportPortAllPlayer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        foreach(var t in ChoiceVAPI.GetAllPlayers().Where(p => !p.hasState(PlayerStates.InTerminal) && !p.hasState(PlayerStates.OnTerminalFlight))) {
            t.Position = player.Position;
        }

        return true;
    }

    private static bool supportShowSocialclubs(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        if(AreSocialClubsShown.ContainsKey(player.getCharacterId()) && AreSocialClubsShown[player.getCharacterId()]) {
            AreSocialClubsShown[player.getCharacterId()] = false;
            player.emitClientEvent("STOP_TEXT_LABELS");
        } else {
            AreSocialClubsShown[player.getCharacterId()] = true;
            foreach(var p in ChoiceVAPI.GetAllPlayers().Where(p => p.Position.Distance(player.Position) < 200)) {
                player.emitClientEvent("TEXT_LABEL_ON_PLAYER", p, $"{(string)p.getData("SOCIALCLUB")} : Id {p.getAccountId()}", -1);
            }
        }

        return true;
    }

    private static bool supportWhitelistSocialclub(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var socialEvt = data["PreviousCefEvent"] as InputMenuItemEvent;

        using(var db = new ChoiceVDb()) {
            var already = db.accounts.FirstOrDefault(a => a.socialclubName == socialEvt.input);
            if(already == null) {
                var dbAcc = new account {
                    socialclubName = socialEvt?.input,
                    name = socialEvt?.input,
                    adminLevel = 3
                };

                db.accounts.Add(dbAcc);
                db.SaveChanges();
            } else {
                player.sendBlockNotification("Diese Person ist schon gewhitelisted!", "");
            }
        }

        player.sendNotification(NotifactionTypes.Success, $"Spieler mit Namen {socialEvt?.input} erfolgreich gewhitelisted", "");

        return true;
    }

    private static bool onSupportPortToWaypoint(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        CallbackController.getPlayerWaypoint(player, (p, pos) => {
            player.Position = pos;
            pos.Z = 1000;
            InvokeController.AddTimedInvoke("", (i) => { CallbackController.getGroundZFromPos(player, pos, (p, z, _, _) => { player.Position = new Position(pos.X, pos.Y, z + 1.0f); }); },
                TimeSpan.FromSeconds(1), false);
        });

        return true;
    }

    private static bool onSupportPortPlayer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var to = data["To"];
        var evt = menuItemCefEvent as InputMenuItemEvent;
        var target = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.SocialClubName == evt.input);

        if(target != null) {
            if(to) {
                player.Position = target.Position;
            } else {
                target.Position = player.Position;
            }
        } else {
            player.sendBlockNotification("Spieler nicht gefunden", "");
        }

        return true;
    }

    private static bool onActivateAdminMode(IPlayer player, ConsoleKey key, string eventName) {
        activateAdminMode(player);

        return true;
    }

    public static void activateAdminMode(IPlayer player) {
        if(player.getAdminLevel() <= 0) {
            return;
        }

        player.getCharacterData().AdminMode = !player.getCharacterData().AdminMode;

        if(player.getCharacterData().AdminMode) {
            player.sendNotification(NotifactionTypes.Success, "Adminmodus aktiviert", "Adminmodus aktiviert", NotifactionImages.System, "ADMIN_MODE");
            toggleClientSideLoggingForPlayer(player, true);
        } else {
            player.sendNotification(NotifactionTypes.Danger, "Adminmodus deaktiviert", "Adminmodus deaktiviert", NotifactionImages.System, "ADMIN_MODE");
            toggleClientSideLoggingForPlayer(player, false);
        }

        player.emitClientEvent("TOGGLE_CHAT");

        AdminModeToggleDelegate?.Invoke(player, player.getCharacterData().AdminMode);
    }


    public static void showPositions(List<Position> positions, IPlayer player) {
        player.emitClientEvent("SHOW_POSITIONS", positions);
    }

    public static void stopShowPositions(IPlayer player) {
        player.emitClientEvent("STOP_SHOW_POSITIONS");
    }
}