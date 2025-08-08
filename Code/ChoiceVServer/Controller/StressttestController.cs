using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller {
    public class StressttestController : ChoiceVScript {
        public StressttestController() {
            if(Config.IsStressTestActive) {
                EventController.PlayerDisconnectedDelegate += onPlayerDisconnected;

                CharacterController.addSelfMenuElement(
                    new ConditionalPlayerSelfMenuElement(
                        "Stresstest-Menü",
                        generateStresstestMenu,
                        s => s.getCharacterData().AdminMode && s.getAdminLevel() > 1,
                        MenuItemStyle.green
                    ), true
                );

                EventController.addMenuEvent("STRESSTEST_CREATE_VEHICLE", onStresstestCreateVehicle);
                EventController.addMenuEvent("STRESSTEST_GET_CASH", onStresstestGetCash);
                EventController.addMenuEvent("STRESSTEST_CREATE_OUTER_VEHICLE_REPAIR_ITEM", onStresstestCreateOuterVehicleRepairItem);
                EventController.addMenuEvent("STRESSTEST_SPAWN_INNER_VEHICLE_REPAIR_ITEM", onStresstestSpawnInnerVehicleRepairItem);
                EventController.addMenuEvent("STRESSTEST_CREATE_OUTER_VEHICLE_TUNING_ITEM", onStressTestCreateVehicleTuningItem);
                EventController.addMenuEvent("STRESSTEST_SPAWN_MEDICITEMS", onStresstestSpawnMedicItems);
                EventController.addMenuEvent("STRESSTEST_SPAWN_ITEM", onStressTestCreateItem);
                EventController.addMenuEvent("STRESSTEST_HEAL", onStresstestHeal);

                EventController.addMenuEvent("STRESSTEST_GET_TRANSPONDER_ITEM", onStresstestGetTransponderItem);

                InteractionController.addVehicleInteractionElement(
                    new UnconditionalPlayerInteractionMenuElement(
                        (s, v) => new StaticMenuItem("Fahrzeugklasse", "Das Fahrzeug hat die angezeigte Klasse. Diese Info ist z.B. für äußere Schäden notwendig.", VehicleController.getVehicleClassName((v as ChoiceVVehicle).VehicleClassId))
                    )
                );
            }
        }

        private void onPlayerDisconnected(IPlayer player, string reason) {
            if(player.hasData("StresstestVehicle")) {
                var vehicle = player.getData("StresstestVehicle") as ChoiceVVehicle;
                VehicleController.removeVehicle(vehicle);
            }
        }

        private List<string> BannedVehicles = new() {
#region List

            "avis", "dinghy", "kosatka", "patrolboat", "submersible", "submersible2", "cerberus", "cerberus2", "cerberus3", "akul", "annihilato", "annihilator", "buzzhar", "hauler", "bulldozer", "cutter", "dump", "handler", "apc", "BARRACKS", "BARRACKS2", "BARRACKS3", "barrage", "chernobog", "CRUSADER", "halftrack", "khanjali", "minitank", "RHINO", "scarab", "scarab2", "scarab3", "thruster", "trailersmall2", "veti", "phantom", "hunte", "savag", "valkyri", "valkyrie", "stockade", "skylif", "cablecar", "freight", "freightcar", "freightcar2", "freightcont1", "freightcont2", "freightgrain", "metrotrain", "tankercar", "deathbike", "dominator4", "dominator", "bruiser", "Bruiser ", "Bruiser ",
            "dominator", "dukes", "oppresso", "oppressor", "formula2", "formula", "openwheel", "openwheel", "caracara", "alkonos", "alphaz", "avenge", "avenger", "besr", "BLIM", "BLIMP", "BLIMP", "bombushk", "cargoplan", "cargoplane", "cuba80", "dod", "duste", "howar", "hydr", "je", "Laze", "luxo", "luxor", "mammatu", "microligh", "Milje", "mogu", "moloto", "nimbu", "nokot", "pyr", "rogu", "seabreez", "Shama", "starlin", "strikeforc", "stun", "tita", "tul", "velu", "velum", "vestr", "volato", "tampa3", "cog552", "dune", "dune", "dune", "dune", "cognoscenti2", "limo2", "insurgent", "insurgent", "schafter5", "schafter6", "marshal", "menacer", "everon2", "kuruma2", "paragon2", "monste",
            "monster", "monster", "monster", "nightshark", "rancherxl2", "rcbandito", "technical", "technical", "technical", "veto", "veto", "trophytruck", "zr380", "trophytruck", "zr3802", "zr3803", "armytanker", "Airtug", "armytrailer", "armytrailer2", "baletrailer", "boattrailer", "caddy", "Caddy2", "caddy3", "docktrailer", "docktug", "FORKLIFT", "freighttrailer", "graintrailer", "Mower", "proptrailer", "raketrailer", "tanker", "zhaba", "tanker2", "tr2", "tr3", "tr4", "trailerlarge", "trailerlogs", "trailers", "trailers2", "trailers3", "trailers4", "trailersmall", "trflat", "tvtrailer", "boxville5", "stromberg", "toreador", "scramjet", "vigilante", "voltic2", "baller5", "baller6",
            "mesa2", "xls2", "lcpdpredator", "lcpdalamo", "froggersl", "lcpdspeedo", "Kamachofib", "sabregt2fib", "Annihilator", "valkyrie3", "sr650fly", "zodiac", "yacht2", "maverick2", "blimp2", "mammatus2", "fbic3", "fibw", "fibt", "fibs2", "fibo", "fibo2", "fibj2", "fibg3", "fibf", "fibd3", "fibdc2", "fibb2", "fibb", "fibh", "fibk", "fibk2", "fibr2", "fibx2", "fibs3", "fibg", "fibd", "fibs", "fibc", "fibg2", "fibd2", "fibj", "fibn", "fibr", "newsmav", "elegy3", "froggersl", "uh1nasa", "bcfdbat", "caracarafib", "caracaramd", "caracarapol", "caracarapol2", "caracararanger", "fibc2", "fibc3", "fibc4", "fibd", "fibg", "fibt2", "fibw", "fibx", "polcoquettep", "polgauntletp",
            "polgresleyp", "policeb1", "policeb2", "polroamerp", "polspeedop", "poltorencep", "trubuffalo", "trubuffalo2", "airbus", "bus", "fbi2", "polnspeedo", "va_fdlcladder", "fdlc", "fdlcforklift", "lcsbus", "lcmav", "lcpd", "lcpd2", "lcpd5", "lcpd4", "lcpd3", "lcpdb", "lcpdbob", "lcpdboxville", "lcpdbtrailer", "lcpdesp", "lcpdmav", "lcpdfaggio", "lcpdold", "lcpdpanto", "lcpdpatriot", "lcpdpigeon", "lcpdprem", "lcpdriata", "lcpdsand", "lcpdsparrow", "lcpdshark", "lcpdspeedo", "lcpdstockade", "lcpdtru", "lcpdtruck", "lctaxibr", "lctaxiprem", "mrtasty", "napc", "nboxville", "tourmav", "subway", "nstockade", "trailers2", "trailers2a", "trailers3", "trailers4"

#endregion
        };

        private Menu generateStresstestMenu(IPlayer player) {
            var vehicleClassNames = VehicleController.getAllVehicleClasses();

            var menu = new Menu("Stresstest-Menü", "Was möchtest du tun?");

            //TODO
            // 4. Add self to Stresstest Company

            var vehicleModels = VehicleController.getAllVehicleModels().Where(m => !BannedVehicles.Contains(m.ModelName.ToLower()) && (!uint.TryParse(m.ModelName, out var _) || (uint)m.Model != uint.Parse(m.ModelName)));
            var vehicleSpawn = new Menu("Fahrzeug erstellen", "Erstelle ein Fahrzeug", false);
            vehicleSpawn.addMenuItem(new InputMenuItem("Fahrzeugmodel", "Wähle das Fahrzeugmodel aus", "", "STRESSTEST_CREATE_VEHICLE", MenuItemStyle.green).withOptions(vehicleModels.Select(m => m.ModelName).ToArray()));
            menu.addMenuItem(new MenuMenuItem(vehicleSpawn.Name, vehicleSpawn));

            var spawnItemsMenu = new Menu("Spezialitems erhalten", "Erhalte SpezialItems für versch. Funktionen");

            var vehicleRepairItems = new Menu("Fahrzeugreperatur", "Wähle das Item");
            #region VehicleRepairItems

            var outerVehicleRepairItems = new Menu("Äußere Schäden", "Spawne dir Items für äußere Schäden");

            var vehicleRepairItemsList = InventoryController.getConfigItemsForType<VehicleRepairItem>();

            outerVehicleRepairItems.addMenuItem(new ListMenuItem("Fahrzeugklasse", "Bestimmte die Fahrzeugklasse, für welche das Reperaturteil passen soll. Wähle das Fahrzeug aus um diese Info zu erhalten!", vehicleClassNames.Select(c => c.ClassName).ToArray(), ""));
            outerVehicleRepairItems.addMenuItem(new ListMenuItem("Reperaturitem", "Wähle die Art des Items aus", vehicleRepairItemsList.Select(i => i.name).ToArray(), ""));
            outerVehicleRepairItems.addMenuItem(new InputMenuItem("Anzahl", "Anzahl des Items", "", InputMenuItemTypes.number, ""));
            outerVehicleRepairItems.addMenuItem(new MenuStatsMenuItem("Item erstellen", "Erstelle das von dir angegeben Item und füge es deinem Inventar hinzu", "STRESSTEST_CREATE_OUTER_VEHICLE_REPAIR_ITEM", MenuItemStyle.green));

            vehicleRepairItems.addMenuItem(new MenuMenuItem(outerVehicleRepairItems.Name, outerVehicleRepairItems));
            spawnItemsMenu.addMenuItem(new MenuMenuItem(vehicleRepairItems.Name, vehicleRepairItems));

            var innerVehicleRepairItemsList = InventoryController.getConfigItemsForType<VehicleMotorCompartmentItem>();
            var innerVehicleRepairItems = new Menu("Innere Schäden", "Spawne dir Items für innere Schäden");
            foreach(var repairItem in innerVehicleRepairItemsList) {
                innerVehicleRepairItems.addMenuItem(new ClickMenuItem(repairItem.name, $"Spawne dir ein {repairItem.name}", "", "STRESSTEST_SPAWN_INNER_VEHICLE_REPAIR_ITEM").withData(new Dictionary<string, dynamic> { { "Item", repairItem } }));
            }
            vehicleRepairItems.addMenuItem(new MenuMenuItem(innerVehicleRepairItems.Name, innerVehicleRepairItems));

            #endregion

            var vehicleTuningItems = new Menu("Fahrzeugtuning", "Wähle das Item");

            #region VehicleTuningItems

            var tuningItemsList = InventoryController.getConfigItemsForType<VehicleTuningItem>();
            vehicleTuningItems.addMenuItem(new ListMenuItem("Fahrzeugklasse", "Bestimmte die Fahrzeugklasse, für welche das Reperaturteil passen soll. Wähle das Fahrzeug aus um diese Info zu erhalten!", vehicleClassNames.Select(c => c.ClassName).ToArray(), ""));
            vehicleTuningItems.addMenuItem(new ListMenuItem("TuningItem", "Wähle die Art des Items aus", tuningItemsList.Select(i => i.name).ToArray(), ""));
            vehicleTuningItems.addMenuItem(new MenuStatsMenuItem("Item erstellen", "Erstelle das von dir angegeben Item und füge es deinem Inventar hinzu", "STRESSTEST_CREATE_OUTER_VEHICLE_TUNING_ITEM", MenuItemStyle.green));

            spawnItemsMenu.addMenuItem(new MenuMenuItem(vehicleTuningItems.Name, vehicleTuningItems));

            #endregion

            spawnItemsMenu.addMenuItem(new ClickMenuItem("Alle Medic-Items erhalten", $"Erhalte alle Medicitems in Inventar", "", "STRESSTEST_SPAWN_MEDICITEMS"));

            var idList = new List<int> { 22, 23, 24, 91, 171, 495, 497, 498 };
            foreach(var id in idList) {
                var cfg = InventoryController.getConfigById(id);
                if(cfg != null) {
                    spawnItemsMenu.addMenuItem(new ClickMenuItem(cfg.name, $"Erhalte ein {cfg.name}", "", "STRESSTEST_SPAWN_ITEM").withData(new Dictionary<string, dynamic> { { "Item", cfg } }));
                }
            }
            
            spawnItemsMenu.addMenuItem(new ClickMenuItem("Transmitter", "Erhalte einen Leitstellentransponder mit dem du deine Position auf der Leitstellenkarte aktivieren kannst", "", "STRESSTEST_GET_TRANSPONDER_ITEM"));

            menu.addMenuItem(new MenuMenuItem(spawnItemsMenu.Name, spawnItemsMenu));

            menu.addMenuItem(new ClickMenuItem("$1000 Bargeld erhalten", "Gebe dir selbst $1000 Bargeld", "", "STRESSTEST_GET_CASH"));

            menu.addMenuItem(new ClickMenuItem("Verletzungen heilen", "Heile deine Verletzungen. Reparo und so..", "", "STRESSTEST_HEAL"));

            return menu;
        }

        private bool onStresstestCreateVehicle(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as InputMenuItemEvent;

            if(evt.input != "" && evt.input != null && !BannedVehicles.Contains(evt.input.ToLower())) {
                if(player.hasData("StresstestVehicle")) {
                    var oldVehicle = player.getData("StresstestVehicle") as ChoiceVVehicle;
                    if(oldVehicle != null) {
                        VehicleController.removeVehicle(oldVehicle);
                    }
                }

                var vehicle = VehicleController.createVehicle(ChoiceVAPI.Hash(evt.input), player.Position, player.Rotation, player.Dimension);
                if(vehicle == null) {
                    player.sendBlockNotification("Das gewählte Fahrzeug ist nicht verfügbar", "Fahrzeug nicht verfügbar");
                    return true;
                }

                player.setData("StresstestVehicle", vehicle);

                var cfg = InventoryController.getConfigItemForType<VehicleKey>();
                if(vehicle != null) {
                    player.getInventory().addItem(new VehicleKey(cfg, vehicle), true);
                }
                player.SetIntoVehicle(vehicle, 1);
                vehicle.LockState = VehicleLockState.Unlocked;

                player.sendNotification(Constants.NotifactionTypes.Success, "Fahrzeug erstellt und den Schlüssel erhalten!", "Fahrzeug erstellt!");
            } else {
                player.sendBlockNotification("Das gewählte Fahrzeug ist nicht verfügbar", "Fahrzeug nicht verfügbar");
            }

            return true;
        }

        private bool onStresstestGetCash(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            player.addCash(1000);
            player.sendNotification(Constants.NotifactionTypes.Success, "Du hast dir $1000 Cash geben lassen", "$1000 Cash erhalten");

            return true;
        }

        private bool onStresstestCreateOuterVehicleRepairItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var classEvt = evt.elements[0].FromJson<ListMenuItemEvent>();
            var itemEvt = evt.elements[1].FromJson<ListMenuItemEvent>();
            var amountEvt = evt.elements[2].FromJson<InputMenuItemEvent>();

            var amount = 1;
            try {
                amount = int.Parse(amountEvt.input);
            } catch(Exception) {
                amount = 1;
            }
            var vehicleClass = VehicleController.getVehicleClass(c => c.ClassName == classEvt.currentElement);
            var item = InventoryController.getConfigItem(c => c.name == itemEvt.currentElement);

            for(var i = 0; i < amount; i++) {
                player.getInventory().addItem(new VehicleRepairItem(item, vehicleClass.classId), true);
            }
            player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast dir {amount}x {item.name} für die Fahrzeugklasse {vehicleClass.ClassName} erstellt", $"{item.name} erhalten");

            return true;
        }

        private bool onStresstestSpawnInnerVehicleRepairItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (configitem)data["Item"];

            player.getInventory().addItem(new VehicleMotorCompartmentItem(item, 1, -1), true);
            player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast dir ein {item.name} erstellt", $"{item.name} erhalten");

            return true;
        }

        private bool onStressTestCreateVehicleTuningItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var classEvt = evt.elements[0].FromJson<ListMenuItemEvent>();
            var itemEvt = evt.elements[1].FromJson<ListMenuItemEvent>();

            var vehicleClass = VehicleController.getVehicleClass(c => c.ClassName == classEvt.currentElement);
            var item = InventoryController.getConfigItem(c => c.name == itemEvt.currentElement);

            player.getInventory().addItem(new VehicleTuningItem(item, vehicleClass.classId, null, null), true);
            player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast dir ein {item.name} für die Fahrzeugklasse {vehicleClass.ClassName} erstellt", $"{item.name} erhalten");

            return true;
        }

        private bool onStresstestSpawnMedicItems(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var cfgs = InventoryController.getConfigItemsForType<MedicItem>();

            foreach(var cfg in cfgs) {
                player.getInventory().addItem(new MedicItem(cfg, 1, -1), true);
            }
            player.sendNotification(Constants.NotifactionTypes.Info, "Du hast dir alle Medicitems erstellt", "Medicitems erhalten");

            return true;
        }

        private bool onStressTestCreateItem(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var cfg = (configitem)data["Item"];

            var item = InventoryController.createItem(cfg, 1);

            player.getInventory().addItem(item, true);

            player.sendNotification(Constants.NotifactionTypes.Info, $"Du hast dir ein {cfg.name} erstellt", $"Item erhalten");

            return true;
        }

        private bool onStresstestGetTransponderItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = InventoryController.getConfigItemForType<ControlCenterTransmitter>();

            player.getInventory().addItem(new ControlCenterTransmitter(item, 1, -1), true);
            player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast dir ein {item.name} erstellt", $"{item.name} erhalten");

            return true;
        }

        private bool onStresstestHeal(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var damg = player.getCharacterData().CharacterDamage;
            foreach(var inj in damg.AllInjuries) {
                damg.healInjury(player, inj);
            }

            return true;
        }
    }
}
