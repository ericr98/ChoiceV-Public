using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.CApi.ClientEvents;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Crime_Stuff.CrimeModules.Shops;
using ChoiceVServer.Controller.LockerSystem.Model;
using ChoiceVServer.Controller.Phone;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.AnalyseSpots;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace ChoiceVServer.Controller.Crime_Stuff;

public record CrimeTraderItem(int Id, configitem Item, decimal Price, int MaxPerRequest, bool PriceIsFavors, int ReputationRequirement, bool SellNotBuy);

public class CrimeBaseTraderController : ChoiceVScript {
    private static Dictionary<string, List<CrimeTraderItem>> BaseTraderShops = [];

    private const int NPC_SELL_ITEM_MIN_MINUTES = 15;
    private const int NPC_SELL_ITEM_MAX_MINUTES = 40;

    private const int NPC_BUY_ITEM_MIN_MINUTES = 10;
    private const int NPC_BUY_ITEM_MAX_MINUTES = 30;
    
    private const int MAX_WAITING_HOURS = 24; 

    private const int FULLFILLED_REQUEST_PROCESSING_MINUTES = 5; 

    private static long InformPhoneNumber = -1;

    private static List<string> ItemSynonyms = [
        "Gummibären", "Traubendrops", "Kaugyumis", "Ps&Qs", "Zebra-Bars", "Schokostreifen", "Captain's Logs", "Softeis", "Schokotafeln",
        "Morgenwunder", "EgoChasers", "EarthQuakes", "Releases", "Birnenjoghurt", "Üder Milken"
    ];

    public CrimeBaseTraderController() {
        loadTraderShops();
        InvokeController.AddTimedInvoke("CrimeTrader", onUpdate, TimeSpan.FromMinutes(2), true);

        EventController.addMenuEvent("ON_PLAYER_BUY_SELL_CRIME_TRADER_ITEM", onPlayerBuySellCrimeTraderItem);
        EventController.addMenuEvent("ON_PLAYER_COLLECT_CRIME_TRADER_REQUEST", onPlayerCollectCrimeTraderRequest);

        InformPhoneNumber = PhoneController.findPhoneNumberByComment("CRIME_TRADER")?.number ?? PhoneController.createNewPhoneNumber("CRIME_TRADER").number; 

        LockerController.ServerAccessedDrawerAccessedDelegate += onServerAccessedDrawerReceivedItems;

        #region SupportStuff

        SupportController.addSupportMenuElement(new GeneratedSupportMenuElement(4, SupportMenuCategories.Crime, "Crime-Ausüster", generateSupportMenu));
        EventController.addMenuEvent("ON_SUPPORT_CREATE_CRIME_TRADER_ITEM", onSupportCreateItem);
        EventController.addMenuEvent("ON_SUPPORT_DELETE_CRIME_TRADER_ITEM", onSupportDeleteItem);

        PedController.addNPCModuleGenerator("Crime-Ausrüster", moduleGenerator, menuCallback);

        #endregion
    }

    private void loadTraderShops() {
        BaseTraderShops.Clear();

        using(var db = new ChoiceVDb()) {
            foreach(var item in db.configcrimetradershopitems.Include(s => s.configItem)) {
                if(!BaseTraderShops.ContainsKey(item.shop)) {
                    BaseTraderShops.Add(item.shop, []);
                }
                
                BaseTraderShops[item.shop].Add(new CrimeTraderItem(item.id, item.configItem, item.price, item.maxPerRequest, item.priceIsFavors, item.reputationRequirement, item.sellNotBuy));
            }
        }
    }

    public static string getCodedNameForItem(int configItemId) {
        return ItemSynonyms[configItemId % ItemSynonyms.Count];
    }

    private void onUpdate(IInvoke invoke) {
        using(var db = new ChoiceVDb()) {
            var removeList = new List<crimetraderrequest>();
            foreach(var request in db.crimetraderrequests
                .Include(r => r.lockerDrawer)
                .Include(r => r.configCrimeTraderItem)) {
                if(request.arriveDate < DateTime.Now && request.lockerDrawerId == null) {
                    var drawer = LockerController.getFreeServerAccessedDrawer();

                    if(drawer == null) continue;

                    var combination = "";
                    if(request.buyNotSell) {
                        var cfg = InventoryController.getConfigById(request.configCrimeTraderItem.configItemId);
                        var createItems = InventoryController.createItems(cfg, request.amount);
                        combination = drawer.setToGiveItems(request.requesterCharId, createItems, "CRIME_TRADER");
                    } else {
                        combination = drawer.setToReceiveItems(request.requesterCharId, "CRIME_TRADER");
                    }

                    request.lockerDrawerId = drawer.Id;
                    if(request.informPhoneNumber != null) {
                        var namingOption = getCodedNameForItem(request.configCrimeTraderItem.configItemId);
                        var whatStr = request.buyNotSell ? "Annahme" : "Lieferung";
                        var informStr = $"Deine {whatStr} von {request.amount}x \"{namingOption}\" ist nun im Schließfach {drawer.DisplayNumber} bei {drawer.Parent.Name} möglich! Die Kombination für das Schließfach ist: {combination}";

                        PhoneController.sendSMSToNumber(InformPhoneNumber, request.informPhoneNumber ?? -1, informStr);
                    }
                }

                if(request.isFullfilled && request.isFullfilledDate + TimeSpan.FromMinutes(FULLFILLED_REQUEST_PROCESSING_MINUTES) < DateTime.Now && !request.canBeCollected) {
                    var lockerInventory = LockerController.getLockerDrawerInventory(request.lockerDrawer.lockerId, request.lockerDrawer.id);

                    LockerController.freeServerAccessedDrawer(request.lockerDrawer.lockerId, request.lockerDrawer.id);
                    if(!request.buyNotSell) {
                        request.canBeCollected = true;
                    } else {
                        removeList.Add(request);
                    }

                    if(request.informPhoneNumber != null) {
                        var namingOption = getCodedNameForItem(request.configCrimeTraderItem.configItemId);
                        var whatStr = request.buyNotSell ? "Annahme" : "Lieferung";
                        var addStr = "";
                        if(!request.buyNotSell) {
                            addStr = " der \"Finderlohn\" für die Lieferung kann bei \"deinem Freund\" abgeholt werden!";
                        }
                        var informStr = $"Deine {whatStr} von {request.amount}x \"{namingOption}\" wurde als fertig annerkannt!{addStr}";

                        PhoneController.sendSMSToNumber(InformPhoneNumber, request.informPhoneNumber ?? -1, informStr);
                    }

                    continue;
                }

                if(request.stopDate < DateTime.Now) {
                    LockerController.freeServerAccessedDrawer(request.lockerDrawer.lockerId, request.lockerDrawer.id);

                    if(!request.buyNotSell) {
                        CrimeNetworkController.playerRemoveReputation(request.requesterCharId, 20, null);
                    }

                    removeList.Add(request);
                }
            }

            db.crimetraderrequests.RemoveRange(removeList);
            db.SaveChanges();
        }
    }

    private bool onServerAccessedDrawerReceivedItems(ServerAccessedDrawer drawer) {
        using(var db = new ChoiceVDb()) {
            var request = db.crimetraderrequests.Include(r => r.configCrimeTraderItem).ThenInclude(r => r.configItem).FirstOrDefault(r => r.lockerDrawerId == drawer.Id);

            if(request != null) {
                if(!request.buyNotSell) {
                    var items = drawer.Inventory?.getItems(i => i.ConfigId == request.configCrimeTraderItem.configItemId);

                    var amount = items?.Sum(i => i.StackAmount ?? 1);

                    if(amount != null && amount == request.amount) {
                        request.isFullfilled = true;
                        request.isFullfilledDate = DateTime.Now;
                    } else {
                        request.isFullfilled = false;
                        request.isFullfilledDate = null;
                    }
                } else {
                    if(drawer.Inventory?.getAllItems().Count == 0) {
                        request.isFullfilled = true;
                        request.isFullfilledDate = DateTime.Now;
                        
                        CrimeNetworkController.OnPlayerIdCrimeActionDelegate.Invoke(request.requesterCharId, CrimeAction.IllegalItemSell, request.amount, new Dictionary<string, dynamic> {
                            { "SellNotBuy", false },
                            { "ConfigItem", request.configCrimeTraderItem.configItem },
                            { "Position", Position.Zero },
                        });
                    } else {
                        request.isFullfilled = false;
                        request.isFullfilledDate = null;
                    }
                }

                db.SaveChanges();
            }
        }

        return true;
    }

    public static List<CrimeTraderItem> getItemsForShop(string shopName) {
        if(BaseTraderShops.ContainsKey(shopName)) {
            return BaseTraderShops[shopName];
        } else {
            return [];
        }
    }

    public static List<crimetraderrequest> getCrimeTradeRequestsForPlayer(IPlayer player) {
        using(var db = new ChoiceVDb()) {
            var charId = player.getCharacterId();
            return db.crimetraderrequests
            .Include(c => c.lockerDrawer)
                .ThenInclude(l => l.locker)
            .Include(c => c.configCrimeTraderItem)
                .ThenInclude(i => i.configItem)
            .Where(r => r.requesterCharId == charId).ToList();
        }
    }

    private bool onPlayerBuySellCrimeTraderItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var item = data["Item"] as CrimeTraderItem;
        var pillar = data["Pillar"] as CrimeNetworkPillar;

        var evt = menuItemCefEvent as MenuStatsMenuItem.MenuStatsMenuItemEvent;
        var amountEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();
        var givePhoneNumberEvt = evt.elements[3].FromJson<CheckBoxMenuItem.CheckBoxMenuItemEvent>();

        if(!int.TryParse(amountEvt.input, out var amount)) {
            player.sendBlockNotification("Falsche Eingabe!", "Falsche Eingabe!");
            return false;
        }

        if(amount <= 0) {
            player.sendBlockNotification("Mindestens 1!", "Mindestens 1");
            return false;
        }

        if(amount > item.MaxPerRequest) {
            player.sendBlockNotification($"Du kannst nur maximal {item.MaxPerRequest} Einheiten auf einmal Anfragen!", "");
            return false;
        }

        using(var db = new ChoiceVDb()) {
            var charId = player.getCharacterId();
            if(db.crimetraderrequests.Any(r => r.configCrimeTraderItemId == item.Id && r.requesterCharId == charId)) {
                player.sendBlockNotification("Du hast bereits eine Anfrage für diesen Warentyp laufen!", "Bereits Anfrage!", Base.Constants.NotifactionImages.Thief);
                return true;
            }
        }

        var price = item.Price * amount;
        if(item.PriceIsFavors) {
            var reputation = player.getCrimeReputation();
            if(!reputation.removeFavors(pillar, Convert.ToSingle(price))) {
                player.sendBlockNotification($"Du hast nicht genügend Gefallen! Du benötigst {price}!", "Nicht genügend Gefallen", Base.Constants.NotifactionImages.Thief);
                return false;
            }
        } else {
            if (!player.removeCash(price)) {
                player.sendBlockNotification($"Du hast nicht genügend Bargeld dabei. Du benötigst ${price}!", "Nicht genügend Bargeld", Base.Constants.NotifactionImages.Thief);
                return false;
            }
        }
        
        long? phoneNumber = null;
        if(givePhoneNumberEvt.check) {
            phoneNumber = player.getMainPhoneNumber();
        }

        if(phoneNumber == -1) {
            player.sendBlockNotification("Du hast aktuell kein Smartphone ausgerüstet!", "Kein Smartphone dabei");
            return false;
        }

        using(var db = new ChoiceVDb()) {
            var rand = new Random();

            var timeSpanMinutes = 0;
            if(item.SellNotBuy) {
                timeSpanMinutes = rand.Next(NPC_SELL_ITEM_MIN_MINUTES, NPC_SELL_ITEM_MAX_MINUTES);
            } else {
                timeSpanMinutes = rand.Next(NPC_BUY_ITEM_MIN_MINUTES, NPC_BUY_ITEM_MAX_MINUTES);
            }

            var entry = new crimetraderrequest {
                configCrimeTraderItemId = item.Id,
                amount = amount,
                arriveDate = DateTime.Now + TimeSpan.FromMinutes(timeSpanMinutes),
                stopDate = DateTime.Now + TimeSpan.FromMinutes(timeSpanMinutes) + TimeSpan.FromHours(MAX_WAITING_HOURS),
                informPhoneNumber = phoneNumber,
                buyNotSell = item.SellNotBuy,
                requesterCharId = player.getCharacterId(),
            };

            db.crimetraderrequests.Add(entry);
            db.SaveChanges();

            player.sendNotification(Base.Constants.NotifactionTypes.Success, "Der Antrag wurde gestellt! Du musst nun etwas warten bis ein Käufer/Verkäufer gefunden ist. Checke dein Handy oder bei Ausrüstern um den Status deiner Anfrage einzusehen", "Anfrage gestellt!", Base.Constants.NotifactionImages.Thief);
        }

        return true;
    }

    private bool onPlayerCollectCrimeTraderRequest(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var request = data["Request"] as crimetraderrequest;
        var pillar = data["Pillar"] as CrimeNetworkPillar;


        using (var db = new ChoiceVDb()) {
            var dbRequ = db.crimetraderrequests.FirstOrDefault(r => r.id == request.id);
            if(dbRequ == null) return false;

            var amount = request.configCrimeTraderItem.price * request.amount;

            if (request.configCrimeTraderItem.priceIsFavors) {
                var reputation = player.getCrimeReputation();
                reputation.giveFavors(pillar, Convert.ToSingle(amount));
                reputation.updateDb(player, pillar);

                player.sendNotification(Base.Constants.NotifactionTypes.Success, $"Du hast {amount} Gefallen erhalten. Sie wurden dir im Netzwerk angerechnet!", "Gefallen erhalten", Base.Constants.NotifactionImages.Thief);
            } else {
                player.addCash(amount);

                player.sendNotification(Base.Constants.NotifactionTypes.Success, $"Du hast den Erlös von ${amount} erhalten!", "Geld erhalten", Base.Constants.NotifactionImages.Thief);
            }

            db.crimetraderrequests.Remove(dbRequ);

            db.SaveChanges();
            CrimeNetworkController.OnPlayerIdCrimeActionDelegate.Invoke(request.requesterCharId, CrimeAction.IllegalItemSell, request.amount, new Dictionary<string, dynamic> {
                { "SellNotBuy", true },
                { "ConfigItem", request.configCrimeTraderItem.configItem },
                { "Position", Position.Zero },
            });
        }

        return true;
    }

    #region Support Stuff

    private Menu generateSupportMenu(IPlayer player) {
        var menu = new Menu("Crime-Trader", "Was möchtest du tun?");

        var createMenu = new Menu("Item erstellen", "Erstelle ein Item");
        createMenu.addMenuItem(new InputMenuItem("ShopName", "Ein konsistenter Name für den Shop.", "", ""));
        createMenu.addMenuItem(new CheckBoxMenuItem("Verkaufen-nicht-kaufen", "Wenn die Checkbox gesetzt ist kann der spieler das Item KAUFEN, wenn sie nicht gesetzt ist kann er sie verkaufen!", true, ""));
        createMenu.addMenuItem(InventoryController.getConfigItemSelectMenuItem("Item", ""));
        createMenu.addMenuItem(new CheckBoxMenuItem("Preis-ist-Gefallen", "Gib an ob der Preis den du angegeben hast Gefallen sind", false, ""));
        createMenu.addMenuItem(new InputMenuItem("Preis", "Der Preis des Items", "", InputMenuItemTypes.number, ""));
        createMenu.addMenuItem(new InputMenuItem("Max. Anzahl pro Anfrage", "Max Anzahl pro Anfrage", "", InputMenuItemTypes.number, ""));
        createMenu.addMenuItem(new InputMenuItem("Reputationsnotwendigkeit", "Wieviel Reputation nötig ist um dieses Item zu handeln", "", InputMenuItemTypes.text, ""));
        createMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle das Item", "ON_SUPPORT_CREATE_CRIME_TRADER_ITEM", MenuItemStyle.green));

        menu.addMenuItem(new MenuMenuItem(createMenu.Name, createMenu));

        foreach(var shop in BaseTraderShops) {
            var shopMenu = new Menu(shop.Key, "Was möchtest du tun?");

            foreach(var item in shop.Value) {
                var itemMenu = new Menu(item.Item.name, "Was möchtest du tun?");
                itemMenu.addMenuItem(new StaticMenuItem("Item von Shop verkauft", "", item.SellNotBuy.ToString()));
                itemMenu.addMenuItem(new StaticMenuItem("Item", "", $"{item.Item.configItemId}: {item.Item.name}"));
                itemMenu.addMenuItem(new StaticMenuItem("Preis-ist-Gefallen", "", item.PriceIsFavors.ToString()));
                itemMenu.addMenuItem(new StaticMenuItem("Preis", "", item.Price.ToString()));
                itemMenu.addMenuItem(new StaticMenuItem("Max Anzahl pro Anfrage", "", item.MaxPerRequest.ToString()));
                itemMenu.addMenuItem(new StaticMenuItem("Reputation", "", item.ReputationRequirement.ToString()));
                itemMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche das Item", "", "ON_SUPPORT_DELETE_CRIME_TRADER_ITEM", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> {{"Item", item}})
                    .needsConfirmation("Item löschen?", "Wirklich löschen?"));

                shopMenu.addMenuItem(new MenuMenuItem(itemMenu.Name, itemMenu));
            }

            menu.addMenuItem(new MenuMenuItem(shopMenu.Name, shopMenu));
        }

        return menu;
    }

    private bool onSupportCreateItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var evt = menuItemCefEvent as MenuStatsMenuItem.MenuStatsMenuItemEvent;

        var shopNameEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
        var sellNotBuyEvt = evt.elements[1].FromJson<CheckBoxMenuItem.CheckBoxMenuItemEvent>();
        var itemEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();
        var priceIsFavors = evt.elements[3].FromJson<CheckBoxMenuItem.CheckBoxMenuItemEvent>();
        var price = evt.elements[4].FromJson<InputMenuItem.InputMenuItemEvent>();
        var maxPerRequest = evt.elements[5].FromJson<InputMenuItem.InputMenuItemEvent>();
        var reputation = evt.elements[6].FromJson<InputMenuItem.InputMenuItemEvent>();

        using(var db = new ChoiceVDb()) {
            var newEntry = new configcrimetradershopitem {
                shop = shopNameEvt.input,
                configItemId = InventoryController.getConfigItemFromSelectMenuItemInput(itemEvt.input).configItemId,
                sellNotBuy = sellNotBuyEvt.check,
                price = decimal.Parse(price.input),  
                maxPerRequest = int.Parse(maxPerRequest.input),
                priceIsFavors = priceIsFavors.check,
                reputationRequirement = int.Parse(reputation.input),
            };

            db.configcrimetradershopitems.Add(newEntry);

            db.SaveChanges();

            player.sendNotification(Base.Constants.NotifactionTypes.Info, "Item erfolgreich erstellt", "Item erstellt");
            Logger.logDebug(LogCategory.Support, LogActionType.Created, player, $"Crime Trader Item created: {newEntry.id}, ItemId: {newEntry.configItemId}, sellNotBuy: {newEntry.sellNotBuy}, price: {newEntry.price}");
        }

        loadTraderShops();

        SupportController.setCurrentSupportFastAction(player, () => player.showMenu(generateSupportMenu(player)));

        return true;
    } 

    private bool onSupportDeleteItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var item = data["Item"] as CrimeTraderItem;

        using(var db = new ChoiceVDb()) {
            var entry = db.configcrimetradershopitems.Find(item.Id);

            db.configcrimetradershopitems.Remove(entry);

            db.SaveChanges();

            player.sendBlockNotification("Item erfolgreich gelöscht!", "");
            Logger.logDebug(LogCategory.Support, LogActionType.Removed, player, $"Crime Trader Item deleted: {item.Id}");
        }

        SupportController.setCurrentSupportFastAction(player, () => player.showMenu(generateSupportMenu(player)));
        return true;
    }

    private List<MenuItem> moduleGenerator(ref Type codeType) {
        codeType = typeof(BaseTraderModule);

        return [
            new InputMenuItem("ShopName", "Der Name des Shops", "", "").withOptions(BaseTraderShops.Keys.ToArray()),
            new InputMenuItem("Pillar", "Die Säule", "", "").withOptions(new string[] { "Alle" }.Concat(CrimeNetworkController.getAllPillars().Select(c => c.Name)).ToArray()),
        ];
    }

    private void menuCallback(IPlayer player, MenuStatsMenuItem.MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
        var shopNameEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
        var pillarEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();

        creationFinishedCallback.Invoke(
            new Dictionary<string, dynamic>{
                {"BASE_TRADER_SHOPNAME", shopNameEvt.input},
                {"BaseShop_CrimePillar", CrimeNetworkController.getPillarByPredicate(p => p.Name == pillarEvt.input)?.Id}
            });
    }

    #endregion
}