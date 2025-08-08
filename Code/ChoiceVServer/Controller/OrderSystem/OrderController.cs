using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Controller.OrderSystem;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller;

public enum OrderType {
    Item = 0,
    VehicleSpecificItem = 1,
    Vehicle = 2,
    Clothing = 3,
    ClothingProp = 4,
}

public enum ShipState {
    NotYetStarted = 0,
    OnItsWay = 1,
    InHarbor = 2,
    HasLeft = 3
}

public class OrderController : ChoiceVScript {
    public static readonly Dictionary<string, string> Filters = new() {
        { "FOOD", "Essbares" },
        { "DRINKS", "Getränke" },
        { "TOOLS", "Werkzeug/Geräte" },
        { "CHEMIC", "Medizinisch/Chemisch" },
        { "CAR", "Fahrzeuge" },
        { "CAR_PARTS", "Fahrzeugsteile" },
        { "WEAPONS", "Waffen" },
        { "CLOTHES", "Kleidung" },
        { "INTERIOR", "Möbel/Aufbaubares" },
        { "SECURED", "Sicherheitsware" },
        { "MISC", "Sonstiges" },
    };

    private static readonly List<string> NoTruckZoneCollisionsStrings = new List<string> {
        "{\"X\":25.367123,\"Y\":-2472.8354,\"Z\":4.993408}#78.04257#107.92362#55.549255#false#true#false", //Front left
        "{\"X\":-91.12789,\"Y\":-2476.2942,\"Z\":5.010254}#37.798138#43.264496#54.663574#false#true#false", //Middle Middle
        "{\"X\":-108.50344,\"Y\":-2530.9077,\"Z\":4.993408}#37.798138#26.865425#56.485687#false#true#false", //Middle Top
        "{\"X\":-127.5552,\"Y\":-2458.6426,\"Z\":5.010254}#74.24052#19.57695#0.0#false#true#false", //Middle Diagonale
    };

    private static List<CollisionShape> NoTruckZoneCollisions = new List<CollisionShape>();
    private static readonly decimal NO_TRUCK_ZONE_PENALTY = 150;

    private static List<OrderShip> CurrentShips;

    private static readonly TimeSpan SHIP_TRAVEL_TIMESPAN = TimeSpan.FromDays(2);
    private static readonly TimeSpan SHIP_MAX_WAIT_TIME = TimeSpan.FromDays(1);

    private static Dictionary<string, OrderContainer> AllContainers;
    private static List<OrderableItem> OrderableItems;
    private static bool ShipInHarbor;

    private static List<OrderableOptionSet> OptionSets;

    private static bankaccount IDOSBankaccount;

    public readonly ChoiceVPed HarborMasterPed;

    public OrderController() {
        init();

        #region NoTruckZone

        InvokeController.AddTimedInvoke("NoTruckZoneDipatcher", onDispatchPenalties, TimeSpan.FromMinutes(10), true);

        foreach (var collStr in NoTruckZoneCollisionsStrings) {
            var noTruckZoneCollision = CollisionShape.Create(collStr);
            noTruckZoneCollision.OnEntityEnterShape += onEntityEnterNoTruckZone;
            NoTruckZoneCollisions.Add(noTruckZoneCollision);
        }

        EventController.addMenuEvent("PAY_NO_TRUCK_ZONE_PENALTY", onPayNoTruckZonePenalty);
        EventController.addMenuEvent("NO_TRUCK_ZONE_COP_REMOVE_PENALTY", onTruckZoneCopRemovePenalty);

        #endregion

        HarborMasterPed = PedController.createPed("Hafenmeister Steve", "s_m_y_airworker", new Position(-53f, -2523f, 6.4f), 52);
        HarborMasterPed.addModule(new NPCMenuItemsModule(HarborMasterPed, new List<PlayerMenuItemGenerator> { onHarborMasterInteraction }));

        InvokeController.AddTimedInvoke("Order-Ship-Invoke", onUpdateOrderShips, TimeSpan.FromMinutes(10), true);

        EventController.addMenuEvent("ORDER_OPEN_IDOS", onOrderOpenIDOS);

        EventController.addMenuEvent("ORDER_SHOW_CONTAINER_POSITION", onOrderShowContainerPosition);

        EventController.addCefEvent("ORDER_SHOP_BUY", onOrderIDOSBuy);
        EventController.addMenuEvent("IDOS_ORDER_CONFIRM", onIDOSOrderConfirm);

        EventController.addMenuEvent("ORDER_CANCEL_ORDER", onIDOSCancelOrder);

        EventController.addMenuEvent("TAKE_ORDER_ITEM_TO_PLAYER_INVENTORY", onTakeItemsToPlayerInventory);
        EventController.addMenuEvent("TAKE_ORDER_ITEM_TO_VEHICLE_INVENTORY", onTakeItemsToVehicleInventory);
        EventController.addMenuEvent("PARK_ORDER_VEHICLE", onParkOrderVehicle);

        EventController.addMenuEvent("ORDER_CONTAINER_ADD_COMPANY_SHIPPING", onOrderContainerAddShippingCompany);

        #region Admin Stuff

        SupportController.addSupportMenuElement(
            new GeneratedSupportMenuElement(
                3,
                SupportMenuCategories.ItemSystem,
                "Order System Menü",
                orderSystemMenuGenerator
            )
        );

        EventController.addMenuEvent("ADMIN_ADD_NEW_CONTAINER", onAdminAddNewContainer);
        EventController.addMenuEvent("ADMIN_ADD_CONTAINER_SPOT", onAdminAddContainerSpot);
        EventController.addMenuEvent("ADMIN_UPDATE_ORDER_VEHICLES", onAdminUpdateOrderVehicles);
        EventController.addMenuEvent("ADMIN_DELETE_ORDER_ITEM", onAdminAddOrderItem);
        EventController.addMenuEvent("ADMIN_CREATE_NEW_ORDER_ITEM", onAdminCreateNewOrderItem);
        EventController.addMenuEvent("SUPPORT_BRING_ORDER_SHIP_NEXT_PHASE", onSupportBringShipNextPhase); 

        var accL = BankController.getControllerBankaccounts(typeof(OrderController));
        IDOSBankaccount = accL is { Count: > 0 }
            ? accL.First()
            : BankController.createBankAccount(typeof(OrderController), "IDOS-Konto", BankAccountType.CompanyKonto, 0, BankController.getBankByType(BankCompanies.LibertyBank), true);

        #endregion
    }

    #region NoTruckZone

    private void onEntityEnterNoTruckZone(CollisionShape shape, IEntity entity) {
        if (entity is not ChoiceVVehicle vehicle) {
            return;
        }

        //Industrial, Service, Commercials, not allowed!
        if (vehicle.VehicleClass == VehicleClassesDbIds.Industrial || vehicle.VehicleClass == VehicleClassesDbIds.Service || vehicle.VehicleClass == VehicleClassesDbIds.Commercial) {
            if (vehicle.Driver != null) {
                var driver = vehicle.Driver;
                driver.sendNotification(NotifactionTypes.Warning, "Du befindest dich in einem für LKW gesperrten Bereich! Entferne dich in 10sek oder erhalte eine Strafe!", "LKW-Sperrzone", NotifactionImages.Package);

                InvokeController.AddTimedInvoke($"LKW-Zone-Penalty-{vehicle.Driver.getCharacterId()}", (i) => {
                    if (NoTruckZoneCollisions.Any(c => c.IsInShape(vehicle.Position))) {
                        if (driver != null && driver.Exists()) {
                            driver.sendNotification(NotifactionTypes.Danger, $"Das Fahrzeug befand sich auch nach der Frist in der Sperrzone. Deine Strafe sind ${NO_TRUCK_ZONE_PENALTY}. Du hast 48h sie zu begleichen", "LKW-Sperrzone", NotifactionImages.Package);

                            using (var db = new ChoiceVDb()) {
                                var already = db.ordernotruckzonepenalties.Find(driver.getCharacterId());

                                if (already != null) {
                                    already.amount += NO_TRUCK_ZONE_PENALTY;
                                } else {
                                    db.ordernotruckzonepenalties.Add(new ordernotruckzonepenalty {
                                        charId = driver.getCharacterId(),
                                        amount = NO_TRUCK_ZONE_PENALTY,
                                        dueDate = DateTime.Now + TimeSpan.FromDays(2)
                                    });
                                }

                                db.SaveChanges();
                            }
                        }
                    }
                }, TimeSpan.FromSeconds(10), false);
            }
        }
    }

    private void onDispatchPenalties(IInvoke invoke) {
        var dispatches = new List<string>();

        using (var db = new ChoiceVDb()) {
            var penalties = db.ordernotruckzonepenalties.Include(o => o._char);
            foreach (var penalty in penalties) {
                if (penalty.dueDate < DateTime.Now && (penalty.lastDispatchDate == null || penalty.lastDispatchDate + TimeSpan.FromDays(1) < DateTime.Now)) {
                    penalty.lastDispatchDate = DateTime.Now;

                    dispatches.Add($"Eine Person mit Namen {penalty._char.firstname} {penalty._char.lastname} hat ihre/seine Hafenstrafe nicht bezahlt!");
                }
            }
            db.SaveChanges();
        }

        foreach (var dispatch in dispatches) {
            ControlCenterController.createDispatch(DispatchType.AutomatedDispatch, "LKW-Sperrzonenstrafe nicht bezahlt", dispatch, HarborMasterPed.Position, true, true);
        }
    }


    private bool onPayNoTruckZonePenalty(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var amount = (decimal)data["Amount"];

        if (player.removeCash(amount)) {
            using (var db = new ChoiceVDb()) {
                var find = db.ordernotruckzonepenalties.Find(player.getCharacterId());

                if (find != null) {
                    db.ordernotruckzonepenalties.Remove(find);

                    db.SaveChanges();

                    player.sendNotification(NotifactionTypes.Success, "Deine Strafe wurde erfolgreich bezahlt!", "Strafe bezahlt", NotifactionImages.Package);
                }
            }
        }

        return true;
    }

    private bool onTruckZoneCopRemovePenalty(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var charId = (int)data["CharId"];

        using (var db = new ChoiceVDb()) {
            var find = db.ordernotruckzonepenalties.Find(charId);

            if (find != null) {
                db.ordernotruckzonepenalties.Remove(find);
                db.SaveChanges();

                player.sendNotification(NotifactionTypes.Info, "Die Strafe von wurde erfolgreich entfernt!", "Strafe Strafe", NotifactionImages.Package);
            }
        }

        return true;
    }


    #endregion

    private static void init() {
        CurrentShips = new List<OrderShip>();
        using(var db = new ChoiceVDb()) {
            var relevantShips = db.orderships.Include(s => s.orderitems).Where(s => s.leaveTime > DateTime.Now).ToList();
            if(relevantShips.Count > 0 || relevantShips.Any(s => s.startTime > DateTime.Now)) {
                foreach(var dbShip in relevantShips) {
                    var ship = new OrderShip(dbShip.id, dbShip.itemsId, (ShipState)dbShip.currentState, dbShip.createDate, dbShip.startTime, dbShip.arriveTime, dbShip.leaveTime);
                    if(ship.ShipState == ShipState.InHarbor) {
                        ShipInHarbor = true;
                    }

                    CurrentShips.Add(ship);

                    if(ship.ShipState == ShipState.InHarbor) {
                        var ipl = InteriorController.getMapObjectByIdentifer<YmapIPL>("CARGO_SHIP");
                        if(ipl != null) {
                            ipl.setIplLoaded(true);
                        }
                    }

                    foreach(var item in dbShip.orderitems) {
                        if(item.containerId == null) {
                            ship.addComponent(OrderComponent.createOrderComponent(item));
                        }
                    }
                }
            } else {
                createNewShip();
            }

            AllContainers = new Dictionary<string, OrderContainer>();
            foreach(var dbCont in db.configordercontainers.Include(c => c.orderitems)) {
                if(dbCont.category == "NONE") {
                    continue;
                }

                var list = dbCont.unloadCollisions.Split("*").Select(CollisionShape.Create).ToList();
                list.ForEach(c => c.HasNoHeight = true);
                var container = new OrderContainer(dbCont.id, dbCont.name, dbCont.category, dbCont.position.FromJson(), CollisionShape.Create(dbCont.interactCollision), list);

                foreach(var item in dbCont.orderitems) {
                    container.addComponent(OrderComponent.createOrderComponent(item));
                }

                AllContainers.Add(container.Category, container);
            }

            OptionSets = new List<OrderableOptionSet>();
            foreach(var optionSet in db.configorderableoptionsets.Include(os => os.configorderableoptions)) {
                var options = optionSet.configorderableoptions.Select(i => new OrderableOption(i.name, i.extraPrice, i.data)).ToList();

                OptionSets.Add(new OrderableOptionSet(optionSet.id, optionSet.description, options, optionSet.type));
            }

            OrderableItems = new List<OrderableItem>();
            foreach(var oItem in db.configorderorderableitems
                .Include(o => o.configItem)
                .ThenInclude(c => c.configitemsitemcontainerinfoconfigItems)
                .ThenInclude(c => c.subConfigItem)) {

                var addString = "";
                var weight = oItem.configItem.weight;
                foreach(var sub in oItem.configItem.configitemsitemcontainerinfoconfigItems) {
                    weight += sub.subConfigItem.weight * sub.subItemAmount;

                    addString += $"{sub.subConfigItem.name[..3]} {sub.subItemAmount}x, ";
                }

                if(addString != "") {
                    addString = $" ({addString[0..^2]})";
                }

                OrderableItems.Add(new OrderableItem((OrderType)oItem.itemType, oItem.configItemId, "", oItem.configItem.name + addString, weight.ToString(), oItem.price, oItem.orderCategory, "∞", oItem.optionSet ?? -1));
            }

            foreach(var oVehicle in db.configorderorderablevehicles.Include(o => o.configModel)) {
                OrderableItems.Add(new OrderableItem(OrderType.Vehicle, oVehicle.configModelId, "", oVehicle.configModel.DisplayName, "N/A", oVehicle.price, "CAR", "∞", oVehicle.optionsSet ?? -1));
            }

            foreach(var oClothes in db.configorderorderableclothes.Include(o => o.configclothingvariation).ThenInclude(v => v.clothing)) {
                OrderableItems.Add(new OrderableItem(OrderType.Clothing, oClothes.configClothingId, oClothes.variation.ToString(), $"({oClothes.configclothingvariation.clothing.gender}) {oClothes.configclothingvariation.clothing.name}: {oClothes.configclothingvariation.name}", "N/A", oClothes.price, oClothes.orderCategory, "∞", -1));
            }

            foreach(var oProp in db.configorderorderableprops.Include(p => p.configclothingpropvariation).ThenInclude(p => p.prop)) {
                OrderableItems.Add(new OrderableItem(OrderType.ClothingProp, oProp.configPropId, oProp.variation.ToString(), $"({oProp.configclothingpropvariation.prop.gender}) {oProp.configclothingpropvariation.prop.name}: {oProp.configclothingpropvariation.name}","N/A",  oProp.price, oProp.orderCategory, "∞", -1));
            }
        }
    }

    private static void createNewShip() {
        using (var db = new ChoiceVDb()) {
            var newShip = new ordership {
                itemsId = 0,
                createDate = DateTime.Now,
                startTime = DateTime.Now + SHIP_TRAVEL_TIMESPAN,
                arriveTime = DateTime.Now + 2 * SHIP_TRAVEL_TIMESPAN,
                leaveTime = DateTime.Now + 2 * SHIP_TRAVEL_TIMESPAN + SHIP_MAX_WAIT_TIME
            };

            db.orderships.Add(newShip);
            db.SaveChanges();

            CurrentShips.Add(new OrderShip(newShip.id, 0, ShipState.NotYetStarted, newShip.createDate, newShip.startTime, newShip.arriveTime, newShip.leaveTime));
        }
    }

    private static void onUpdateOrderShips(IInvoke obj) {
        if (!CurrentShips.Any(s => s.StartTime > DateTime.Now)) {
            createNewShip();
        }

        using (var db = new ChoiceVDb()) {
            foreach (var ship in CurrentShips.Reverse<OrderShip>()) {
                if (ship.LeaveTime < DateTime.Now && ship.ShipState < ShipState.HasLeft) {
                    ShipInHarbor = false;

                    var ipl = InteriorController.getMapObjectByIdentifer<YmapIPL>("CARGO_SHIP");
                    ipl.setIplLoaded(false);
                    SoundController.playSoundAtCoords(ipl.Position, 500, SoundController.Sounds.ShipHorn, 0.3f);
                    CurrentShips.Remove(ship);

                    var dbShip = db.orderships.Find(ship.Id);
                    dbShip.currentState = (int)ShipState.HasLeft;
                    ship.setState(ShipState.HasLeft);
                } else if (ship.ArriveTime < DateTime.Now && ship.ShipState < ShipState.InHarbor) {
                    ShipInHarbor = true;

                    var ipl = InteriorController.getMapObjectByIdentifer<YmapIPL>("CARGO_SHIP");
                    if (ipl != null) {
                        ipl.setIplLoaded(true);
                        SoundController.playSoundAtCoords(ipl.Position, 500, SoundController.Sounds.ShipHorn, 0.3f);
                    }

                    foreach (var item in ship.Components) {
                        if (AllContainers.ContainsKey(item.Category)) {
                            var container = AllContainers[item.Category];
                            item.setContainerId(container.Id);

                            container.AllComponents.Add(item);
                        }
                    }

                    var dbShip = db.orderships.Include(s => s.orderitems).FirstOrDefault(s => s.id == ship.Id);
                    foreach (var dbItem in dbShip.orderitems) {
                        var item = ship.Components.FirstOrDefault(c => c.RelativeId == dbItem.shipRelativeId);
                        if (item != null) {
                            dbItem.containerId = item.ContainerId;
                        }
                    }

                    dbShip.currentState = (int)ShipState.InHarbor;
                    ship.setState(ShipState.InHarbor);
                    ship.Components = new List<OrderComponent>();
                } else if (ship.StartTime < DateTime.Now && ship.ShipState < ShipState.OnItsWay) {
                    var dbShip = db.orderships.Find(ship.Id);
                    dbShip.currentState = (int)ShipState.OnItsWay;
                    ship.setState(ShipState.OnItsWay);
                }
            }

            db.SaveChanges();
        }
    }

    public static decimal? getPriceOfVehicle(int modelId) {
        return OrderableItems.FirstOrDefault(o => o.type == OrderType.Vehicle && o.configId == modelId)?.price ?? null;
    }

    public static void openBuyMenu(IPlayer player, int companyId) {
        string[] orderableItems;
        List<OrderableItem> orderableItemsSave;

        var company = CompanyController.findCompanyById(companyId);

        string[] optionsSets;
        if (company.hasFunctionality<OrderSystemFunctionality>()) {
            orderableItems = OrderableItems.Select(o => o.ToJson()).ToArray();
            orderableItemsSave = OrderableItems;
            optionsSets = OptionSets.Select(os => os.ToJson()).ToArray();
        } else {
            var functionality = company.getFunctionality<OrderSystemCategoriesFunctionality>();
            if (functionality != null && functionality.getAllOrderableCategories().Count >= 0) {
                var categoryList = functionality.getAllOrderableCategories();

                var oItems = OrderableItems.Where(o => categoryList.Contains(o.category)).ToList();
                orderableItems = oItems.Select(o => o.ToJson()).ToArray();
                orderableItemsSave = oItems.ToList();

                var optionsIdList = oItems.GroupBy(oi => oi.optionsSet).Select(g => g.First().optionsSet);
                optionsSets = OptionSets.Where(os => optionsIdList.Contains(os.id)).Select(os => os.ToJson()).ToArray();
            } else {
                player.sendBlockNotification("Deine Firma darf keine Waren im IDOS kaufen!", "Kein kaufen von Waren möglich!", NotifactionImages.Package);
                return;
            }
        }

        player.setData("ORDERABLE_ITEMS", orderableItemsSave);
        player.emitCefEventWithBlock(new OrderShopCefEvent(companyId, 2, orderableItems, optionsSets), "ORDER_SHOP");
    }

    public static Menu getHarborMasterMenu(IPlayer player) {
        var menu = new Menu("Güterkontrolle", "Was möchtest du tun?");

        var companiesList = CompanyController.getCompanies(player).Where(c => CompanyController.hasPlayerPermission(player, c, "ORDER_BUY")).Select(c => c.Name).ToArray();
        if (companiesList.Length > 0) {
            menu.addMenuItem(new ListMenuItem("IDOS öffnen", "Öffne das International Delivery and Order System", companiesList, "ORDER_OPEN_IDOS"));
        }

        var containerAllMenu = new Menu("Container-Liste", "Zeige Informationen über die Container an");
        foreach (var container in AllContainers.Values) {
            var containerMenu = new Menu($"{container.Name}", "Was möchtest du tun?");
            var data = new Dictionary<string, dynamic> {
                { "Container", container }
            };

            containerMenu.addMenuItem(new ClickMenuItem("Position anzeigen", "Lasse dir die Position des Containers anzeigen", "", "ORDER_SHOW_CONTAINER_POSITION").withData(data));

            foreach (var company in CompanyController.getCompanies(player)) {
                var companyMenu = new Menu(company.Name, "Was möchtest du tun?");
                var orderItems = container.AllComponents.Where(c => c.OrderCompany == company.Id).ToList();

                foreach (var itemMenu in orderItems.Select(item => item.harborMasterMenu(null))) {
                    companyMenu.addMenuItem(new MenuMenuItem(itemMenu.Name, itemMenu));
                }

                if (companyMenu.getMenuItemCount() > 0) {
                    containerMenu.addMenuItem(new MenuMenuItem(companyMenu.Name, companyMenu));
                }
            }

            containerAllMenu.addMenuItem(new MenuMenuItem(containerMenu.Name, containerMenu));
        }

        menu.addMenuItem(new MenuMenuItem(containerAllMenu.Name, containerAllMenu));

        var shipAllMenu = new Menu("Schiff-Liste", "Zeige Informationen über die Schiffe an");
        foreach (var ship in CurrentShips) {
            var shipMenu = new Menu($"Ankunft: {ship.ArriveTime:dd.MM HH:mm}", "Was möchtest du tun?");
            shipMenu.addMenuItem(new StaticMenuItem($"Abfahrt: {ship.StartTime:dd.MM HH:mm}", $"Das Schiff fährt am {ship.StartTime:dd.MM HH: mm} ab", ""));

            foreach (var company in CompanyController.getCompanies(player)) {
                if (!CompanyController.hasPlayerPermission(player, company, "ORDER_BUY")) {
                    continue;
                }

                var companyMenu = new Menu(company.Name, "Was möchtest du tun?");

                var orderItems = ship.Components.Where(c => c.OrderCompany == company.Id).ToList();
                foreach (var itemMenu in orderItems.Select(item => item.harborMasterMenu(ship))) {
                    companyMenu.addMenuItem(new MenuMenuItem(itemMenu.Name, itemMenu));
                }

                if (companyMenu.getMenuItemCount() > 0) {
                    shipMenu.addMenuItem(new MenuMenuItem(companyMenu.Name, companyMenu));
                }
            }

            shipAllMenu.addMenuItem(new MenuMenuItem(shipMenu.Name, shipMenu));
        }

        menu.addMenuItem(new MenuMenuItem(shipAllMenu.Name, shipAllMenu));

        #region NoTruckZone Stuff

        using (var db = new ChoiceVDb()) {
            var penalty = db.ordernotruckzonepenalties.Find(player.getCharacterId());

            if (penalty != null) {
                var penaltyMenu = new Menu("Hafenstrafen begleichen", "Begleiche deine Strafen");
                penaltyMenu.addMenuItem(new StaticMenuItem("Strafe", $"Deine Strafe beträgt ${penalty.amount}", $"${penalty.amount}"));
                penaltyMenu.addMenuItem(new StaticMenuItem("Fälligkeitsdatum", $"Deine Strafe ist fällig bis {penalty.dueDate:G}", $"{penalty.dueDate:G}"));

                var infoString = "Es wurde bisher keine Info an die Polizei gesendet";
                var rightString = "Nein";
                if (penalty.lastDispatchDate != null) {
                    infoString = $"Es wurde bisher eine Info an die Polizei gesendet am {penalty.lastDispatchDate:G}";
                    rightString = "Ja";
                }

                penaltyMenu.addMenuItem(new StaticMenuItem("Info an Polizei gesendet", infoString, rightString));

                if (player.getCash() > penalty.amount) {
                    penaltyMenu.addMenuItem(new ClickMenuItem("Strafe bar bezahlen", "Bezahle die Strafe bar, der Hafenmeister nimmt keine Kartenzahlung!", "", "PAY_NO_TRUCK_ZONE_PENALTY", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Amount", penalty.amount } }));
                } else {
                    penaltyMenu.addMenuItem(new StaticMenuItem("Strafe bar bezahlen", "Du hast nicht genug Geld um die Strafe zu bezahlen", "", MenuItemStyle.yellow));
                }

                menu.addMenuItem(new MenuMenuItem(penaltyMenu.Name, penaltyMenu));
            }

            if (CompanyController.hasPlayerCompanyWithPredicate(player,
                c => c.CompanyType == CompanyType.Sheriff || c.CompanyType == CompanyType.Police || c.CompanyType == CompanyType.Fbi)) {

                var penaltiesMenu = new Menu("Alle Strafen anzeigen", "Lasse dir alle aktuellen Strafen anzeigen");

                foreach (var pen in db.ordernotruckzonepenalties.Include(p => p._char)) {
                    var name = $"{pen._char.firstname} {pen._char.lastname}";
                    if (name.Length > 20) {
                        name = $"{pen._char.firstname} {pen._char.lastname.Take(5)}..";
                    }

                    var penMenu = new Menu(name, "Siehe die Daten");
                    penMenu.addMenuItem(new StaticMenuItem("Sozialversicherungsnummer", $"Die Sozialversicherungsnummer der Person ist: {pen._char.socialSecurityNumber}", $"{pen._char.socialSecurityNumber}"));
                    penMenu.addMenuItem(new StaticMenuItem("Strafe", $"Die Strafe beträgt ${pen.amount}", $"${pen.amount}"));
                    penMenu.addMenuItem(new StaticMenuItem("Fälligkeitsdatum", $"Die Strafe ist fällig bis {pen.dueDate:G}", $"{pen.dueDate:G}"));

                    var infoString = "Es wurde bisher keine Info an die Polizei gesendet";
                    var rightString = "Nein";
                    if (pen.lastDispatchDate != null) {
                        infoString = $"Es wurde bisher eine Info an die Polizei gesendet am {pen.lastDispatchDate:G}";
                        rightString = "Ja";
                    }
                    penMenu.addMenuItem(new StaticMenuItem("Dispatch gesendet", infoString, rightString));

                    penMenu.addMenuItem(new ClickMenuItem("Strafe entfernen", "Entferne die Strafe. Genutzt entweder zum Erlassen der Strafe oder um sie in die Akten aufzunehmen (manuell) und keine Dispatches mehr zu erhalten", "", "NO_TRUCK_ZONE_COP_REMOVE_PENALTY", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "CharId", pen.charId } }));

                    penaltiesMenu.addMenuItem(new MenuMenuItem(penMenu.Name, penMenu));
                }

                menu.addMenuItem(new MenuMenuItem(penaltiesMenu.Name, penaltiesMenu));
            }
        }

        #endregion

        return menu;
    }

    private static MenuItem onHarborMasterInteraction(IPlayer player) {
        var menu = getHarborMasterMenu(player);
        return new MenuMenuItem(menu.Name, menu);
    }

    private static bool onOrderOpenIDOS(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var listEvt = menuItemCefEvent as ListMenuItemEvent;
        var companies = CompanyController.getCompanies(player).FirstOrDefault(c => c.Name == listEvt.currentElement);

        openBuyMenu(player, companies.Id);

        return true;
    }

    private static void onOrderIDOSBuy(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
        var cefData = new IDOSBuyWebSocketEvent();
        JsonConvert.PopulateObject(evt.Data, cefData);

        if (cefData.items.Length <= 0) {
            return;
        }

        var menu = new Menu("IDOS Bestellung", "Möchtest du die Bestellung ausführen?");

        var list = new List<IDOSBoughtItem>();
        foreach (var item in cefData.items) {
            var obj = item.FromJson<IDOSBoughtItem>();
            list.Add(obj);
            var optionSubstring = "";
            if (obj.option != null) {
                optionSubstring = $"({obj.option[..3]})";
            }

            menu.addMenuItem(new StaticMenuItem($"{obj.name} {optionSubstring}", $"Bestelle {obj.name} in {obj.option ?? ""}. Die Bestellmenge beträgt {obj.amount}", $"{obj.amount}"));
        }

        var data = new Dictionary<string, dynamic> {
            { "CompanyId", cefData.companyId },
            { "Items", list },
        };

        menu.addMenuItem(new StaticMenuItem("Gesamtpreis", $"Alle Waren kosten zusammen ${cefData.price}", $"${cefData.price}", MenuItemStyle.yellow));
        menu.addMenuItem(new ClickMenuItem("Bestellung aufgeben", "Gib die oben aufgeführte Bestellung auf", "", "IDOS_ORDER_CONFIRM", MenuItemStyle.green).withData(data).needsConfirmation("Bestellung aufgeben?", "Wirklich bestellen?"));
        player.showMenu(menu);
    }

    private static bool onIDOSOrderConfirm(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var list = (List<IDOSBoughtItem>)data["Items"];
        var companyId = (int)data["CompanyId"];

        var orderableItems = (List<OrderableItem>)player.getData("ORDERABLE_ITEMS");

        decimal price = 0;
        foreach (var item in list) {
            var oI = orderableItems.FirstOrDefault(oI => (int)oI.type == item.type && oI.configId == item.configId);
            if (oI == null) {
                player.ban("Hat beim Order System Waren bestellt ohne Zugriff");
                return true;
            }
            price += Math.Abs(item.amount) * oI.price;
        }

        if(AllContainers.Values.Any(c => c.AllComponents.Any(co => co.OrderCompany == companyId))) {
            player.sendBlockNotification($"Es gibt noch Container mit Gütern! Leere sie erst bevor du neue Waren bestellst! (Kosten nicht abgebucht)", "Container voll", NotifactionImages.Package);
            return false;
        }
        
        if (CompanyController.hasPlayerPermission(player, "ORDER_BUY", companyId)) {
            var comp = CompanyController.findCompanyById(companyId);

            if (!BankController.transferMoney(comp.CompanyBankAccount, IDOSBankaccount.id, price, "Überweisung IDOS Bestellung", out var mess, false)) {
                player.sendBlockNotification($"Bezahlung fehlgeschlagen mit Grund: {mess}", "Bezahlung fehlgeschlagen", NotifactionImages.Package);
                return false;
            }
        } else {
            return false;
        }

        OrderShip ship = null;

        foreach (var orderShip in CurrentShips.Where(orderShip => orderShip.StartTime > DateTime.Now)) {
            if (ship == null) {
                ship = orderShip;
            } else if (orderShip.StartTime < ship.StartTime) {
                ship = orderShip;
            }
        }

        using (var db = new ChoiceVDb()) {
            foreach (var item in list) {
                string dbData;

                if (item.optionSet != -1) {
                    var optionSet = OptionSets[item.optionSet - 1];
                    var option = optionSet.options.FirstOrDefault(o => o.name == item.option);

                    dbData = OrderComponent.getDataFromCef(item, option);
                } else {
                    dbData = OrderComponent.getDataFromCef(item, null);
                }

                var name = item.name;

                if (item.option != null) {
                    name = $"{item.name} ({item.option.Substring(0, 3)})";
                }

                var already = db.orderitems.FirstOrDefault(o => o.shipId == ship.Id && o.orderCompany == companyId && o.orderType == item.type && o.data == dbData);
                if (already != null) {
                    already.amount += item.amount;

                    var alreadyO = ship.Components.FirstOrDefault(c => c.RelativeId == already.shipRelativeId);
                    alreadyO?.addAmount(item.amount);

                    db.SaveChanges();
                } else {
                    var orderItem = new orderitem {
                        shipId = ship.Id,
                        shipRelativeId = ship.ItemRelativeId++,
                        name = name,
                        category = item.category,
                        amount = Math.Abs(item.amount),
                        orderCompany = companyId,
                        orderType = item.type,
                        data = dbData
                    };

                    db.orderitems.Add(orderItem);

                    var dbShip = db.orderships.FirstOrDefault(os => os.id == ship.Id);
                    dbShip.itemsId = ship.ItemRelativeId;

                    db.SaveChanges();

                    ship.addComponent(OrderComponent.createOrderComponent(orderItem));
                }
            }

            player.sendNotification(NotifactionTypes.Success, "Bestellung erfolgreich aufgegeben", "Bestellung aufgegeben", NotifactionImages.Package);
        }

        return true;
    }

    private static bool onIDOSCancelOrder(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var ship = (OrderShip)data["Ship"];
        var orderItem = (OrderItem)data["OrderItem"];
        var configOrderItem = OrderableItems.Find(oi => oi.type == orderItem.OrderType && oi.configId == orderItem.ConfigElementId);

        var company = CompanyController.findCompanyById(orderItem.OrderCompany);


        var evt = menuItemCefEvent as InputMenuItemEvent;

        var amount = int.Parse(evt.input);
        amount = Math.Abs(amount);

        //Doesnt give extra price back
        if (!BankController.transferMoney(IDOSBankaccount.id, company.CompanyBankAccount, amount * configOrderItem.price * 0.99m, $"Erstattung IDOS-Stornierung: {amount}x {orderItem.Name}", out var message, false)) {
            player.sendNotification(NotifactionTypes.Info, $"Bei der Überweisung ist etwas fehlgeschlagen: {message}", "Stornierung fehlgeschlagen", NotifactionImages.Package);
            return true;
        }

        if (amount >= orderItem.Amount) {
            ship.removeComponent(orderItem);

            using (var db = new ChoiceVDb()) {
                var dbItem = db.orderitems.Find(orderItem.RelativeId, ship.Id);

                if (dbItem != null) {
                    db.orderitems.Remove(dbItem);

                    db.SaveChanges();
                } else {
                    Logger.logError("Db OrderItem not found!", 
                        $"Fehler im Order-System: Db Orderitem wurde noch nicht gefunden: {orderItem.Name}", player);
                }
            }
        } else {
            orderItem.addAmount(-amount);

            using (var db = new ChoiceVDb()) {
                var dbItem = db.orderitems.Find(orderItem.RelativeId, ship.Id);

                if (dbItem != null) {
                    dbItem.amount -= amount;

                    db.SaveChanges();
                } else {
                    Logger.logError("Db OrderItem not found!",
                        $"Fehler im Order-System: Db Orderitem wurde noch nicht gefunden: {orderItem.Name}", player);
                }
            }
        }

        player.sendNotification(NotifactionTypes.Info, "Waren erfolgreich storniert. Dir wurden 99% des Kaufpreises erstattet. (Zusatzpreise wurden nicht erstattet)", "Waren storniert", NotifactionImages.Package);

        return true;
    }

    private static bool onOrderShowContainerPosition(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var container = (OrderContainer)data["Container"];
        var id = BlipController.createPointBlip(player, container.Name, container.Position, 4, 478, 255, true);

        InvokeController.AddTimedInvoke("Blip-Remover", i => {
            BlipController.destroyBlipByName(player, id);
        }, TimeSpan.FromMinutes(2.5), false);

        return true;
    }

    private static bool onOrderContainerAddShippingCompany(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var evt = (ListMenuItemEvent)data["PreviousCefEvent"];
        var container = (OrderContainer)data["Container"];
        var company = (Company)data["Company"];

        var shippingCompany = CompanyController.getCompaniesWithFunctionality<OrderSystemFunctionality>().FirstOrDefault(c => c.Name == evt.currentElement);

        if (shippingCompany == null) {
            return true;
        }

        using (var db = new ChoiceVDb()) {
            foreach (var oI in container.AllComponents.Where(c => c.OrderCompany == company.Id)) {
                oI.setShippingCompany(shippingCompany.Id);
            }

            var dbItems = db.orderitems.Where(oi => oi.orderCompany == company.Id && oi.containerId == container.Id);

            dbItems.ForEach(oi => oi.shippingCompany = shippingCompany.Id);

            db.SaveChanges();
        }

        player.sendNotification(NotifactionTypes.Info, $"Für alle Waren im Container wurde der Firma: {evt.currentElement} Zugriff erteilt.", "Zugriff erteilt", NotifactionImages.Package);

        return true;
    }

    internal record OrderableOptionSet(
        int id,
        string description,
        List<OrderableOption> options,
        string type
    );

    internal record OrderableOption(
        string name,
        decimal extraPrice,
        string data
    );

    private record OrderableItem(
        OrderType type,
        int configId,
        string additionalInfo,
        string name,
        string weight,
        decimal price,
        string category,
        string maxAmount,
        int optionsSet
    );


    private class OrderShopCefEvent : IPlayerCefEvent {
        public string bannerColor;
        public int companyId;
        public string[] items;
        public string[] optionSets;
        public float quantDiscConst;
        public string title;

        public OrderShopCefEvent(int companyId, float quantDiscConst, string[] items, string[] optionSets) {
            Event = "OPEN_SHOP";
            this.companyId = companyId;
            title = "<strong>I</strong>nternational<b> D</b>elivery and <b>O</b>rder <b>S</b>ystem";
            bannerColor = "#307CB8";
            this.quantDiscConst = quantDiscConst;
            this.items = items;
            this.optionSets = optionSets;
        }
        public string Event { get; }
    }

    internal class IDOSBoughtItem {
        public int amount;
        public string category;
        public int configId;
        public string additionalInfo;
        public string name;
        public string option;
        public int optionSet;
        public int type;
    }

    private class IDOSBuyWebSocketEvent {
        public int companyId;
        public string[] items;
        public decimal price;
    }

    #region Container Menu Events

    private static bool onTakeItemsToPlayerInventory(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        createOrderItemsToInventory(player, data["Container"], data["OrderItem"], data["PlayerInventory"], "dein Inventar");
        return true;
    }

    private static bool onTakeItemsToVehicleInventory(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        createOrderItemsToInventory(player, data["Container"], data["OrderItem"], data["VehicleInventory"], "den Caddy");
        return true;
    }

    private static void createOrderItemsToInventory(IPlayer player, OrderContainer container, OrderItem orderItem, Inventory inv, string notiTarget) {
        var weightLeft = inv.MaxWeight - inv.CurrentWeight;

        var itemCount = 0;
        var cfg = orderItem.getConfigItem();
        if (cfg == null) {
            player.sendBlockNotification("Ein Fehler ist aufgetreten! Melde dich im Support. Code: Bullauge", "Code: Bullage");
            return;
        }

        if (cfg.weight == 0) {
            itemCount = orderItem.Amount;
        } else {
            while (itemCount * cfg.weight + cfg.weight < weightLeft && itemCount < orderItem.Amount && itemCount * cfg.weight + cfg.weight <= 20) {
                itemCount++;
            }
        }

        if (itemCount == 0) {
            player.sendBlockNotification("Du hast nicht genug Platz im Inventar", "Nicht genug Platz", NotifactionImages.Package);
            return;
        }

        var anim = AnimationController.getAnimationByName("TAKE_STUFF_LONG");
        AnimationController.animationTask(player, anim, () => {
            var items = InventoryController.createItems(cfg, itemCount, cfg.hasQuality == 1 ? 1 : -1);

            foreach (var item in items) {
                item.setOrderData(orderItem);
                inv.addItem(item, true);
            }

            if (itemCount == orderItem.Amount) {
                container.AllComponents.Remove(orderItem);

                using (var db = new ChoiceVDb()) {
                    var dbItem = db.orderitems.Find(orderItem.RelativeId, orderItem.ShipId);

                    if (dbItem != null) {
                        db.orderitems.Remove(dbItem);
                    }

                    //TODO Maybe Log for Lookup
                    db.SaveChanges();
                }
            } else {
                orderItem.addAmount(-itemCount);

                using (var db = new ChoiceVDb()) {
                    var dbItem = db.orderitems.Find(orderItem.RelativeId, orderItem.ShipId);

                    if (dbItem != null) {
                        dbItem.amount = orderItem.Amount;
                    }

                    db.SaveChanges();
                }
            }

            player.sendNotification(NotifactionTypes.Success, $"Es wurden erfolgreich {itemCount}x {orderItem.Name} in {notiTarget} gelegt", "Waren genommen", NotifactionImages.Package);
        });
    }

    private static bool onParkOrderVehicle(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var container = (OrderContainer)data["Container"];
        var orderVehicle = (OrderVehicle)data["OrderItem"];

        var anim = AnimationController.getAnimationByName("TAKE_STUFF_LONG");
        AnimationController.animationTask(player, anim, () => {

            var cfgModel = VehicleController.getVehicleModelById(orderVehicle.ConfigId);

            ChoiceVVehicle veh = null;

            foreach (var spot in container.AllUnloadCollisions) {
                if (spot.AllEntities.Count > 0) {
                    continue;
                }

                var rot = Rotation.Zero;
                rot.Yaw = spot.Rotation + (float)(Math.PI / 2);

                veh = VehicleController.createVehicle(ChoiceVAPI.Hash(cfgModel.ModelName), spot.Position, rot, GlobalDimension, (byte)orderVehicle.Color);

                if (veh != null) {
                    var cfg = InventoryController.getConfigItemForType<VehicleKey>();
                    veh.Inventory.addItem(new VehicleKey(cfg, veh), true);
                    veh.Inventory.addItem(new VehicleKey(cfg, veh), true);

                    var cfg2 = InventoryController.getConfigItemForType<VehicleRegistrationCard>();
                    veh.Inventory.addItem(new VehicleRegistrationCard(cfg2, "", -1, veh, ""), true);

                    veh.LockState = VehicleLockState.Unlocked;
                }

                break;
            }

            if (veh != null) {
                if (orderVehicle.Amount == 1) {
                    container.AllComponents.Remove(orderVehicle);

                    using (var db = new ChoiceVDb()) {
                        var dbItem = db.orderitems.Find(orderVehicle.RelativeId, orderVehicle.ShipId);

                        if (dbItem != null) {
                            db.orderitems.Remove(dbItem);
                        }

                        //TODO Maybe Log for Lookup
                        db.SaveChanges();
                    }
                } else {
                    orderVehicle.addAmount(-1);

                    using (var db = new ChoiceVDb()) {
                        var dbItem = db.orderitems.Find(orderVehicle.RelativeId, orderVehicle.ShipId);

                        if (dbItem != null) {
                            dbItem.amount = orderVehicle.Amount;
                        }

                        db.SaveChanges();
                    }
                }
                player.sendNotification(NotifactionTypes.Success, "Fahrzeug erfolgreich ausgeparkt!", "Fahrzeug ausgeparkt", NotifactionImages.Package);
            } else {
                player.sendBlockNotification("Es wurde keine freier Parkplatz gefunden!", "Kein freier Platz", NotifactionImages.Package);
            }
        });

        return true;
    }

    #endregion

    #region Admin Stuff

    private static Menu orderSystemMenuGenerator(IPlayer player) {
        var menu = new Menu("Order System Menü", "Was möchstest du tun?");

        var containerMenu = new Menu("Container einstellen", "Was möchtest du tun");

        var newContainerMenu = new Menu("Neuen Container erstellen", "Trage die Informationen ein");
        newContainerMenu.addMenuItem(new InputMenuItem("Name", "Gib den Namen des Containers ein", "", ""));
        newContainerMenu.addMenuItem(new ListMenuItem("Kategorie", "Die Kategorie des Containers", Filters.Values.ToArray(), ""));

        newContainerMenu.addMenuItem(new MenuStatsMenuItem("Erstellung beginnen", "Erstelle einen neuen Container", "ADMIN_ADD_NEW_CONTAINER", MenuItemStyle.green));

        containerMenu.addMenuItem(new MenuMenuItem(newContainerMenu.Name, newContainerMenu, MenuItemStyle.green));

        var allContainerMenu = new Menu("Alle Container", "Was möchtest du tun?");
        foreach (var container in AllContainers.Values) {
            var data = new Dictionary<string, dynamic> {
                { "Container", container }
            };

            var subContainerMenu = new Menu(container.Name, "Was möchtest du tun?");
            subContainerMenu.addMenuItem(new StaticMenuItem("Kategorie", $"In den Container werden Items mit der Kategorie {container.Category} geladen", $"{container.Category}"));

            subContainerMenu.addMenuItem(new ClickMenuItem("Neuen Ausladespot erstellen", "Erstelle einen neuen Spot zum ausladen", $"Aktuell: {container.AllUnloadCollisions.Count}", "ADMIN_ADD_CONTAINER_SPOT", MenuItemStyle.green).withData(data));
            subContainerMenu.addMenuItem(new ClickMenuItem(container.Name, "", container.Category, "ADMIN_DELETE_CONTAINER", MenuItemStyle.red).withData(data).needsConfirmation("Container löschen", "Wirklich löschen?"));

            allContainerMenu.addMenuItem(new MenuMenuItem(subContainerMenu.Name, subContainerMenu));
        }

        containerMenu.addMenuItem(new MenuMenuItem(allContainerMenu.Name, allContainerMenu));

        menu.addMenuItem(new MenuMenuItem(containerMenu.Name, containerMenu));


        var orderableMenu = new Menu("Bestellbare Items bearbeiten", "");
        orderableMenu.addMenuItem(new ClickMenuItem("Bestellbare Fahrzeuge updaten", "Zieht sich ein Update für die bestellbaren Fahrzeuge aus der vehicle Tabelle", "", "ADMIN_UPDATE_ORDER_VEHICLES", MenuItemStyle.green));

        var newItemMenu = new Menu("Neues Bestellitem hinzufügen", "Füge die Daten ein");
        var typeList = ((OrderType[])Enum.GetValues(typeof(OrderType))).Select(t => t.ToString()).ToArray();
        newItemMenu.addMenuItem(new InputMenuItem("Config Item Id", "Setze die Id des Config Items", "", ""));
        newItemMenu.addMenuItem(new ListMenuItem("Type", "Wähle den Typen des Items aus", typeList, ""));
        newItemMenu.addMenuItem(new ListMenuItem("Kategorie", "Wähle die Kategorie des Items aus", Filters.Values.ToArray(), ""));
        newItemMenu.addMenuItem(new InputMenuItem("Preis", "Wähle den Preis aus", "", ""));
        newItemMenu.addMenuItem(new ListMenuItem("Optionsset", "Wähle welche Auswahloptionen für das Item gibt es", new string[] { "Keins" }.Concat(OptionSets.Select(o => o.description)).ToArray(), ""));
        newItemMenu.addMenuItem(new MenuStatsMenuItem("Abschließen", "Schließe die Erstellung ab", "ADMIN_CREATE_NEW_ORDER_ITEM", MenuItemStyle.green));

        orderableMenu.addMenuItem(new MenuMenuItem(newItemMenu.Name, newItemMenu));

        using (var db = new ChoiceVDb()) {
            var oItemMenu = new Menu("Items Liste", "Wähle das Item aus");
            foreach (var row in db.configorderorderableitems.Include(i => i.configItem)) {
                var data = new Dictionary<string, dynamic> {
                    { "Row", row }
                };

                var oItemSubMenu = new Menu(row.configItem.name, "Was möchtest du tun?");
                oItemSubMenu.addMenuItem(new StaticMenuItem("Config Item", $"Das Item die ConfigId: {row.configItemId}", $"{row.configItemId}"));
                oItemSubMenu.addMenuItem(new StaticMenuItem("Type", $"Das Item hat den Type: {(OrderType)row.itemType}", $"{(OrderType)row.itemType}"));
                oItemSubMenu.addMenuItem(new StaticMenuItem("Kategorie", $"Das Item hat die Kategorie: {Filters[row.orderCategory]}", Filters[row.orderCategory]));
                oItemSubMenu.addMenuItem(new StaticMenuItem("Preis", $"Das Item hat einen Preis von: ${row.price}", $"${row.price}"));
                var optionSetStr = "Keins";
                if (row.optionSet != null) {
                    optionSetStr = OptionSets[row.optionSet - 1 ?? -1].description;
                }
                oItemSubMenu.addMenuItem(new StaticMenuItem("Option-Set", "Welche Optionen für das Item zur Verfügung stehen", optionSetStr));
                oItemSubMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche das Item aus der Liste", "", "ADMIN_DELETE_ORDER_ITEM", MenuItemStyle.red).needsConfirmation($"{row.configItem.name} löschen?", "Item wirklich löschen?").withData(data));

                oItemMenu.addMenuItem(new MenuMenuItem(oItemSubMenu.Name, oItemSubMenu));
            }

            orderableMenu.addMenuItem(new MenuMenuItem(oItemMenu.Name, oItemMenu));
        }

        menu.addMenuItem(new MenuMenuItem(orderableMenu.Name, orderableMenu));

        menu.addMenuItem(new ClickMenuItem("Schiff in nächste Phase bringen", "Bringe das aktuelle Schiff in die nächste Phase", "", "SUPPORT_BRING_ORDER_SHIP_NEXT_PHASE").needsConfirmation("Schiff voran bringen?", "Schiff wirklich voranbringen?"));

        return menu;
    }

    private static bool onAdminAddNewContainer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

        var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
        var categoryEvt = evt.elements[1].FromJson<ListMenuItemEvent>();

        player.sendNotification(NotifactionTypes.Info, "Setze nun die Kollision für die Interaktion. Sie muss alle Container die dazugehören enthalten", "", NotifactionImages.Package);
        var pos = player.Position.ToJson().FromJson();
        CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
            var interactPosition = CollisionShape.Create(p, w, h, r, true, false, true);

            player.sendNotification(NotifactionTypes.Info, "Setze nun die Kollision für die das Abladen. Sie sollte den sinnvollen Ausladepunkt enthalten", "", NotifactionImages.Package);
            CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p2, w2, h2, r2) => {
                var unloadPosition = CollisionShape.Create(p2, w2, h2, r2, false, true);

                using (var db = new ChoiceVDb()) {
                    var cat = Filters.FirstOrDefault(f => f.Value == categoryEvt.currentElement);
                    var dbCont = new configordercontainer {
                        name = nameEvt.input,
                        category = cat.Key,
                        interactCollision = interactPosition.toShortSave(),
                        unloadCollisions = unloadPosition.toShortSave(),
                        position = pos.ToJson()
                    };

                    db.configordercontainers.Add(dbCont);

                    db.SaveChanges();
                    var unloadColl = CollisionShape.Create(dbCont.unloadCollisions);
                    unloadColl.HasNoHeight = true;
                    AllContainers.Add(dbCont.category, new OrderContainer(dbCont.id, dbCont.name, dbCont.category, dbCont.position.FromJson(), CollisionShape.Create(dbCont.interactCollision), new List<CollisionShape> { unloadColl }));
                    player.sendNotification(NotifactionTypes.Info, $"Container {nameEvt.input} erfolgreich erstellt", "", NotifactionImages.Package);
                }
            });
        });

        return true;
    }

    private static bool onAdminAddContainerSpot(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var container = (OrderContainer)data["Container"];

        player.sendNotification(NotifactionTypes.Info, "Setze nun die Kollision für die das Abladen. Sie sollte den sinnvollen Ausladepunkt enthalten", "", NotifactionImages.Package);
        CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p2, w2, h2, r2) => {
            var unloadPosition = CollisionShape.Create(p2, w2, h2, r2, false, true);

            container.AllUnloadCollisions.Add(unloadPosition);

            using (var db = new ChoiceVDb()) {
                var dbCont = db.configordercontainers.Find(container.Id);

                var colString = container.AllUnloadCollisions.First().toShortSave();
                colString = container.AllUnloadCollisions.Skip(1).Aggregate(colString, (current, col) => current + $"*{col.toShortSave()}");

                dbCont.unloadCollisions = colString;

                db.SaveChanges();

                player.sendNotification(NotifactionTypes.Info, "Abladepunkt erfolgreich erstellt", "", NotifactionImages.Package);
            }
        });

        return true;
    }

    private static bool onAdminUpdateOrderVehicles(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        using (var db = new ChoiceVDb()) {
            foreach (var row in db.configorderorderablevehicles.ToList()) {
                db.configorderorderablevehicles.Remove(row);
            }

            db.SaveChanges();

            foreach (var row in db.configvehiclesmodels.Where(v => v.useable == 1)) {
                db.configorderorderablevehicles.Add(new configorderorderablevehicle {
                    configModelId = row.id,
                    optionsSet = 1,
                    price = row.price
                });
            }

            db.SaveChanges();
        }

        return true;
    }

    private static bool onAdminAddOrderItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var row = (configorderorderableitem)data["Row"];

        using (var db = new ChoiceVDb()) {
            var dbI = db.configorderorderableitems.Find(row.configItemId);

            db.configorderorderableitems.Remove(dbI);

            var oi = OrderableItems.FirstOrDefault(oi => oi.type != OrderType.Vehicle && oi.configId == row.configItemId);

            OrderableItems.Remove(oi);

            db.SaveChanges();
        }

        return true;
    }

    private static bool onAdminCreateNewOrderItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

        var configEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
        var typeEvt = evt.elements[1].FromJson<ListMenuItemEvent>();
        var categoryEvt = evt.elements[2].FromJson<ListMenuItemEvent>();
        var priceEvt = evt.elements[3].FromJson<InputMenuItemEvent>();
        var optionSetEvt = evt.elements[4].FromJson<ListMenuItemEvent>();

        var type = (OrderType)typeEvt.currentIndex;

        using (var db = new ChoiceVDb()) {
            var osS = OptionSets.FirstOrDefault(o => o.description == optionSetEvt.currentElement);
            int? optionSet = osS != null ? osS.id : null;

            var oItem = new configorderorderableitem {
                configItemId = int.Parse(configEvt.input),
                itemType = typeEvt.currentIndex,
                orderCategory = Filters.FirstOrDefault(f => f.Value == categoryEvt.currentElement).Key,
                price = decimal.Parse(priceEvt.input),
                optionSet = optionSet
            };

            db.configorderorderableitems.Add(oItem);

            db.SaveChanges();

            var cfgI = db.configitems.FirstOrDefault(c => c.configItemId == oItem.configItemId);

            OrderableItems.Add(new OrderableItem((OrderType)oItem.itemType, oItem.configItemId, "", cfgI.name, cfgI.weight.ToString(), oItem.price, oItem.orderCategory, "∞", oItem.optionSet ?? -1));
        }

        return true;
    }

    private bool onSupportBringShipNextPhase(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        foreach (var currentShip in CurrentShips) {
            if (currentShip.ShipState == ShipState.NotYetStarted) {
                currentShip.StartTime = DateTime.Now - TimeSpan.FromMinutes(1);
            } else if (currentShip.ShipState == ShipState.OnItsWay) {
                currentShip.ArriveTime = DateTime.Now - TimeSpan.FromMinutes(1);
            } else if (currentShip.ShipState == ShipState.InHarbor) {
                currentShip.LeaveTime = DateTime.Now - TimeSpan.FromDays(1);
            }

            onUpdateOrderShips(null);

            SupportController.setCurrentSupportFastAction(player, () => onSupportBringShipNextPhase(player, null, new int(), null, null));

            player.sendNotification(NotifactionTypes.Info, $"Schiff mit Id {currentShip.Id} ist nun in State: {currentShip.ShipState}", "", NotifactionImages.Package);
            return true;
        }

        player.sendBlockNotification("Etwas ist schiefgelaufen!", "", NotifactionImages.Package);
        return false;
    }

    #endregion
}