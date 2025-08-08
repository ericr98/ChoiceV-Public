using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace ChoiceVServer.Controller.Crime_Stuff {
    public enum DrugType {
        Organic = 0,
        Cocaine = 1,
        Synthetics = 2,
    }

    public class DealerNPCController : ChoiceVScript {
        internal const decimal DEALER_HIRE_AMOUNT = 55;
        internal const int DEALER_HIRE_HOURS = 2;
        
        private const decimal DEALER_ROB_AMOUNT = 35;
        private const int DEALER_ROB_REFILL_IN_HOURS = 24;
        private const decimal DEALER_ROB_AMOUNT_FLUCTUATION = 0.25m;
        

        private static readonly List<(string name, int torso)> DEALER_PED_NAMES = [
            ("g_m_y_mexgoon_01", 0),
            ("g_m_y_famfor_01", 1), 
            ("a_m_m_salton_03", 0),
            ("s_m_y_dealer_01", 0),
            ("g_f_y_ballas_01", 0),
            ("g_f_y_families_01", 0),
            ("csb_grove_str_dlr", 0),
            ("ig_lamardaviscs_guadalope", 0),
            ("a_m_m_salton_03", 1), 
            ("a_m_m_soucent_01", 0),
            ("a_m_y_soucent_02", 0),
            ("g_m_y_strpunk_01", 0),
            ("a_m_m_mexcntry_01", 0),
            ("g_m_y_famfor_01", 0),
            ("g_m_y_famca_01", 1), 
            ("a_m_y_breakdance_01", 0),
            ("g_m_y_ballasout_01", 1),
            ("g_m_y_ballaorig_01", 1),
            ("ig_ortega", 0)
        ];

        private static readonly TimeSpan DEALER_SPAWN_TIME = TimeSpan.FromMinutes(10);
        private static Dictionary<string, List<ChoiceVPed>> DealerNPCs = [];
        private static Dictionary<int, (DrugType type, decimal npcDealerPrice, decimal playerDealerPrice)> DrugPrices;
        
        private static Dictionary<string, float> MeanOptiomalDistanceOfDealersPerZone = [];

        public DealerNPCController() {
            EventController.MainAfterReadyDelegate += onMainAfterReady;
            InvokeController.AddTimedInvoke("DealerSpawner", onUpdateDealer, DEALER_SPAWN_TIME, true);

            EventController.addMenuEvent("ON_DEALER_BUY_ITEM", onDealerBuyItem);
            EventController.addMenuEvent("COP_REMOVE_DEALER", onCopRemoveDealer);

            EventController.addMenuEvent("HIRE_DEALER", onHireDealer);
            EventController.addMenuEvent("OPEN_DEALER_INVENTORY", onOpenDealerInventory);

            calculateMeanOptimalDistancePerZone();
            
            #region Support Stuff

            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                   3,
                   SupportMenuCategories.Crime,
                    "Dealer",
                    getDealerSupportMenu
                )
            );
            EventController.addMenuEvent("SET_DEALER_POSITION", onSetDealerPosition);
            EventController.addMenuEvent("SHOW_DEALER_POSITIONS", onShowDealerPosition);
            EventController.addMenuEvent("REMOVE_DEALER_POSITION", onRemoveDealerPosition);
            
            EventController.addMenuEvent("SUPPORT_TRIGGER_DEALER_UPDATE", (player, itemEvent, menuItemId, data, menuItemCefEvent) => {
                onUpdateDealer(null);
                return true;
            });

            #endregion
        }

        private void calculateMeanOptimalDistancePerZone() {
            using(var db = new ChoiceVDb()) {
                var zones = db.configcrimedealerpositions
                        .Include(p => p.zoneGroupNavigation)
                        .ThenInclude(g => g.configcrimedealerzone)
                        .GroupBy(p => p.zoneGroup).ToList();

                foreach(var zone in zones) {
                    var points = zone.Select(z => new Vector2(z.x, z.y)).ToList();

                    var dbZone = zone.First().zoneGroupNavigation.configcrimedealerzone;
                    if(dbZone != null) {
                        var dist = calculateMeanOptimalDistance(points.ToList(), dbZone.supportedDealerAmount);
                        MeanOptiomalDistanceOfDealersPerZone.Add(dbZone.zoneGroup, dist);
                    }
                }
            }
        }

        private float calculateMeanOptimalDistance(List<Vector2> points, int centersCount) {
            if(points.Count < centersCount) {
                return -1;
            }
            
            var n = points.Count;
            var selectedIndices = new List<int> { 0 }; // Start with the first point
            var distances = new double[n];
            Array.Fill(distances, double.MaxValue); // Initialize distances to infinity

            for (var iteration = 1; iteration < centersCount; iteration++) {
                // Update the minimum distance to any selected center for all points
                for (var i = 0; i < n; i++) {
                    if (!selectedIndices.Contains(i)) {
                        var minDistance = double.MaxValue;
                        foreach (var selectedIndex in selectedIndices)
                        {
                            double distance = Vector2.Distance(points[i], points[selectedIndex]);
                            if (distance < minDistance)
                                minDistance = distance;
                        }
                        distances[i] = Math.Min(distances[i], minDistance);
                    } else {
                        distances[i] = 0;
                    }
                }

                // Select the point that is farthest from its nearest center
                var farthestPointIndex = Array.IndexOf(distances, distances.Max());
                selectedIndices.Add(farthestPointIndex);
            }
            
            
            // Calculate the mean shortest distance between each pair of selected centers
            var sum = 0.0;
            for(var i = 0; i < selectedIndices.Count; i++) {
                var minDistance = double.MaxValue;
                for(var j = 0; j < selectedIndices.Count; j++) {
                    if(i == j) {
                        continue;
                    }
                    
                    var distance = Vector2.Distance(points[selectedIndices[i]], points[selectedIndices[j]]);
                    if(distance < minDistance) {
                        minDistance = distance;
                    }
                }
                sum += minDistance;
            }

            //For plotting with eCharts
            //var posStr = points.Select(p => new float[] { p.X, p.Y }).ToArray().ToJson();
            //var highlightPoints = selectedIndices.Select(i => new float[] { points[i].X, points[i].Y }).ToArray().ToJson();
            
            return (float)(sum / selectedIndices.Count);
        }

        private static bool onOpenDealerInventory(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var inventory = (Inventory)data["Inventory"];

            InventoryController.showMoveInventory(player, player.getInventory(), inventory);

            return true;
        }

        private static bool onHireDealer(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var module = (DealerNPCModule)data["Module"];

            if(module.IsHired) {
                player.sendNotification(Constants.NotifactionTypes.Danger, "Dieser Dealer ist bereits angestellt.", "");
                return false;
            }

            if(player.removeCash(DEALER_HIRE_AMOUNT)) {
                module.setHireCharacter(player);
                module.clearBelongings();
                
                player.sendNotification(Constants.NotifactionTypes.Success, "Dealer erfolgreich angestellt.", "");

                Logger.logTrace(LogCategory.Player, LogActionType.Created, player, $"Player hired Dealer NPC");
            } else {
                player.sendBlockNotification("Du hast nicht genug Bargeld.", "Nicht genug Geld");
            }

            return true;
        }

        private static bool onDealerBuyItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (Item)data["Item"];
            var evt = menuItemCefEvent as InputMenuItem.InputMenuItemEvent;

            var price = getPriceForItem(item, true);
            if(price == null) {
                player.sendNotification(Constants.NotifactionTypes.Danger, $"Dieses Item kann nicht gekauft werden.", "");
                return false;
            }

            if(!int.TryParse(evt.input, out var amount) || amount <= 0) {
                player.sendNotification(Constants.NotifactionTypes.Danger, $"Ungültige Menge.", "");
                return false;
            }

            if (player.removeCash(price ?? decimal.MaxValue)) {
                if (item.moveToInventory(player.getInventory(), int.Parse(evt.input))) {
                    CrimeNetworkController.OnPlayerCrimeActionDelegate(player, CrimeAction.IllegalItemSell, amount, new Dictionary<string, dynamic> {
                        { "SellNotBuy", false },
                        { "ConfigItem", item.ConfigItem },
                        { "Position", player.Position },
                    });

                    player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast {item.Name} für {price} $ gekauft.", "");
                } else {
                    player.sendBlockNotification($"Du hast nicht genug Platz in deinem Inventar.", "");
                    player.addCash(price ?? 0);
                }
            }

            return true;
        }

        private static bool onCopRemoveDealer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var ped = (ChoiceVPed)data["Dealer"];

            ped.playAnimation("anim@mp_player_intselfiethe_bird", "idle_a", 49, 3000, 0);
            InvokeController.AddTimedInvoke("REMOVE_DEALER", (i) => removeDealer(ped), TimeSpan.FromSeconds(3), false);

            player.sendNotification(Constants.NotifactionTypes.Info, "Die Person wurde vertrieben!", "Person vertrieben");

            Logger.logTrace(LogCategory.Player, LogActionType.Removed, player, $"Cop removed Dealer NPC");

            return true;
        }

        private static void removeDealer(ChoiceVPed ped) {
            DealerNPCs[WorldController.getBigRegionName(ped.Position)].Remove(ped);
            PedController.destroyPed(ped);

            Logger.logDebug(LogCategory.System, LogActionType.Removed, $"Removed Dealer NPC");
        }

        private static void onMainAfterReady() {
            var dealers = PedController.findPeds(p => p.hasModule<DealerNPCModule>(i => true));

            foreach(var dealer in dealers) {
                var zone = WorldController.getBigRegionName(dealer.Position);

                if(!DealerNPCs.ContainsKey(zone)) {
                    DealerNPCs.Add(zone, [dealer]);
                } else {
                    DealerNPCs[zone].Add(dealer);
                }
            }

            Logger.logInfo(LogCategory.ServerStartup, LogActionType.Created, $"Loaded {dealers.Count} Dealer NPCs");

            DrugPrices = [];
            using(var db = new ChoiceVDb()) {
                foreach(var item in db.configcrimedealerdrugitems) {
                    DrugPrices.Add(item.configItemId, ((DrugType)item.drugType, item.npcDealerPrice, item.playerDealerPrice));
                }
            }
        }

        private static void onUpdateDealer(IInvoke invoke) {
            using(var db = new ChoiceVDb()) {
                var zones = db.configcrimedealerzones.ToList();
                foreach(var zone in zones) {
                    if(!DealerNPCs.ContainsKey(zone.zoneGroup)) {
                        DealerNPCs.Add(zone.zoneGroup, []);
                    }
                    
                    var drugDemands = getDrugDemandsForZone(zone.zoneGroup);
                    var dealerSellingDrugs = new Dictionary<DrugType, int>();
                    
                    foreach(var dealer in DealerNPCs[zone.zoneGroup]) {
                        var dealerModule = dealer.getNPCModulesByType<DealerNPCModule>().First();
                        var availableDealerDrugs = dealerModule.getAvailableDrugs();
                        
                        foreach(var (type, amount) in availableDealerDrugs) {
                            if(!dealerSellingDrugs.TryAdd(type, 1)) {
                                dealerSellingDrugs[type] += 1;
                            }
                        }
                    }

                    var soldDrugs = new Dictionary<DrugType, int>();
                    foreach(var dealer in DealerNPCs[zone.zoneGroup]) {
                        var dist = 0d;
                        var dealerPos = dealer.Position;
                        dealerPos.Z = 0;
                        var closestOtherDealer = DealerNPCs[zone.zoneGroup].Where(d => d != dealer).MinBy(d => new Position(d.Position.X, d.Position.Y, 0).Distance(dealerPos)); 
                        if(closestOtherDealer != null) {
                            dist = dealer.Position.Distance(closestOtherDealer.Position);
                        }
                        
                        var optimalDist = MeanOptiomalDistanceOfDealersPerZone[zone.zoneGroup] * 0.85;

                        var drugSellList = dealer.getNPCModulesByType<DealerNPCModule>().First().sellProduct(optimalDist, dist, zone.supportedDealerAmount, drugDemands, dealerSellingDrugs);
                        
                        foreach(var (type, amount) in drugSellList) {
                            if(!soldDrugs.TryAdd(type, amount)) {
                                soldDrugs[type] += amount;
                            }
                        }
                    }
        
                    var dealers = DealerNPCs[zone.zoneGroup];
                    if(dealers.Count >= zone.maxNpcDealerAmount) {
                        //Remove Dealer
                        var dealer = dealers.FirstOrDefault(d => d.getNPCModulesByType<DealerNPCModule>().First()?.canBeAutomaticallyRemoved() ?? false);
                        PedController.destroyPed(dealer);

                        dealers.Remove(dealer);

                        Logger.logDebug(LogCategory.System, LogActionType.Removed, $"Removed Dealer NPC in {zone.zoneGroup}");
                    }

                    var soldStr = "";
                    foreach(var (type, amount) in soldDrugs) {
                        soldStr += $"{type}: {amount}, ";
                    }
                    
                    Logger.logInfo(LogCategory.System, LogActionType.Event, $"Dealer Update in {zone.zoneGroup} - Sold Drugs: {soldStr}");

                    configcrimedealerposition random = null;
                    var counter = 0;
                    while(random == null || DealerNPCs[zone.zoneGroup].Any(p => p.Position.Distance(new Position(random.x, random.y, random.z)) < 10)) {
                        counter++;
                        if(counter > 10) {
                            return;
                        }
                        
                        random = db.configcrimedealerpositions
                                       .Where(p => p.zoneGroup == zone.zoneGroup)
                                       .OrderBy(p => EF.Functions.Random())
                                       .FirstOrDefault();
                    }

                    var (name, torso) = DEALER_PED_NAMES[new Random().Next(0, DEALER_PED_NAMES.Count)];
                    var ped = PedController.createPedDb(null, name, new Position(random.x, random.y, random.z), ChoiceVAPI.radiansToDegrees(random.yaw), 0, torso);
                    PedController.addPedModuleDb(ped, new DealerNPCModule(ped, zone, random));
                    PedController.addPedModuleDb(ped, new NPCWillNotCallPoliceModule(ped));
                    PedController.addPedModuleDb(ped, new NPCRobbingModule(ped, Convert.ToDecimal(new Random().NextDouble() * 2 - 1) * DEALER_ROB_AMOUNT_FLUCTUATION * DEALER_ROB_AMOUNT + DEALER_ROB_AMOUNT, DEALER_ROB_REFILL_IN_HOURS));

                    dealers.Add(ped);

                    Logger.logDebug(LogCategory.System, LogActionType.Created, $"Spawned Dealer NPC in {zone.zoneGroup}");
                }
            }
            
            foreach(var dealers in DealerNPCs.Values) {
                foreach(var dealer in dealers) {
                    var module = dealer.getNPCModulesByType<DealerNPCModule>().First();
                    module.update();
                }
            }
        }
        
        internal static void onHiredDealerTimeUp(DealerNPCModule module) {
            var player = ChoiceVAPI.GetAllPlayers().Find(p => p.getCharacterId() == module.HireCharacterId);

            player?.sendNotification(Constants.NotifactionTypes.Info, "Dein Dealer hat Feierabend.", "Dealer Feierabend");

            module.setHireCharacter(null);
        }

        public static List<(DrugType type, int demand)> getDrugDemandsForZone(string zone) {
            using(var db = new ChoiceVDb()) {
                var zoneData = db.configcrimedealerzones.FirstOrDefault(z => z.zoneGroup == zone);
                if(zoneData == null) {
                    return [];
                }

                return [
                    (DrugType.Organic, zoneData.organicDemand),
                    (DrugType.Cocaine, zoneData.cocaineDemand),
                    (DrugType.Synthetics, zoneData.syntheticsDemand),
                ];
            }
        }

        public static decimal? getPriceForItem(Item item, bool npcDealer) {
            if(DrugPrices.TryGetValue(item.ConfigId, out var value)) {
                if(npcDealer) {
                    return value.npcDealerPrice;
                } else {
                    return value.playerDealerPrice;
                }
            } else {
                return null;
            }
        }
        
        public static DrugType? getDrugTypeForItem(Item item) {
            if(DrugPrices.TryGetValue(item.ConfigId, out var value)) {
                return value.type;
            } else {
                return null;
            }
        }

        #region Support Stuff

        private Menu getDealerSupportMenu(IPlayer player) {
            var menu = new Menu("Dealer", "Was möchtest du tun?");
            menu.addMenuItem(new ClickMenuItem("Dealerposition setzen", "Setze die Position des Dealers auf deine aktuelle Position", "", "SET_DEALER_POSITION"));
            menu.addMenuItem(new ClickMenuItem("Dealerpositionen anzeigen/verstecken", "Zeige alle Dealerpositionen an", "", "SHOW_DEALER_POSITIONS"));
            menu.addMenuItem(new ClickMenuItem("Dealerposition löschen", "Lösche die nächste Dealerposition", "", "REMOVE_DEALER_POSITION"));
            
            menu.addMenuItem(new ClickMenuItem("Dealerupdate triggern", "Triggere ein Dealerupdate", "", "SUPPORT_TRIGGER_DEALER_UPDATE"));

            return menu;
        }

        private bool onSetDealerPosition(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var pos = player.Position;
            var rot = player.Rotation;

            using(var db = new ChoiceVDb()) {
                var zone = WorldController.getBigRegionName(pos);
                var newPos = new configcrimedealerposition {
                    zoneGroup = zone,
                    x = pos.X,
                    y = pos.Y,
                    z = pos.Z - 1,
                    roll = rot.Roll,
                    pitch = rot.Pitch,
                    yaw = rot.Yaw,
                };
                db.configcrimedealerpositions.Add(newPos);
                db.SaveChanges();

                player.sendNotification(Constants.NotifactionTypes.Success, "Dealerposition erfolgreich gesetzt.", "");
                SupportController.setCurrentSupportFastAction(player, () => onSetDealerPosition(player, null, -1, null, null));

                if(player.hasData("SUPPORT_SHOW_DEALER_POS")) {
                    player.emitClientEvent("SHOW_POSITIONS", new List<Position> { new(newPos.x, newPos.y, newPos.z) });
                }
            }

            return true;
        }

        private bool onShowDealerPosition(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(player.hasData("SUPPORT_SHOW_DEALER_POS")) {
               player.emitClientEvent("STOP_SHOW_POSITIONS"); 
               player.resetData("SUPPORT_SHOW_DEALER_POS");
               
               player.sendNotification(Constants.NotifactionTypes.Success, "Dealerpositionen versteckt.", "");
            } else {
                var positions = new List<Position>();
                using (var db = new ChoiceVDb()) {
                    positions = db.configcrimedealerpositions.Select(c => new Position(c.x, c.y, c.z)).ToList();
                }
                
                var dealers = DealerNPCs.Values.SelectMany(d => d).Select(d => new Position(d.Position.X, d.Position.Y, d.Position.Z)).ToList();

                player.emitClientEvent("SHOW_POSITIONS", positions);
                player.emitClientEvent("SHOW_POSITIONS", dealers, 124, 252, 0);
                player.setData("SUPPORT_SHOW_DEALER_POS", true);

                player.sendNotification(Constants.NotifactionTypes.Success, "Dealerpositionen angezeigt.", "");
            }

            return true;
        }

        private bool onRemoveDealerPosition(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            using(var db = new ChoiceVDb()) {
                var pos = db.configcrimedealerpositions.ToList().MinBy(p => player.Position.Distance(new Position(p.x, p.y, p.z)));

                if(pos == null) {
                    player.sendBlockNotification("Keine Dealerposition gefunden!", "");
                    return false;
                }
                
                if(player.Position.Distance(new Position(pos.x, pos.y, pos.z)) > 10) {
                    player.sendBlockNotification("Du bist zu weit entfernt!", "");
                    return false;
                }

                db.configcrimedealerpositions.Remove(pos);

                if(player.hasData("SUPPORT_SHOW_DEALER_POS")) {
                    player.emitClientEvent("REMOVE_SHOW_POSITION", new Position(pos.x, pos.y, pos.z) );
                }

                db.SaveChanges();
                player.sendNotification(Constants.NotifactionTypes.Warning, "Dealerposition gelöscht.", "");

                SupportController.setCurrentSupportFastAction(player, () => onRemoveDealerPosition(player, null, -1, null, null));
            } 

            return true;
        }

        #endregion
    }
}