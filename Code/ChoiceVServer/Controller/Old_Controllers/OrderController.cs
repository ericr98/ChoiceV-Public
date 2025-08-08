//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.InventorySystem;
//using ChoiceVServer.Model;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Model.Menu;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.RegularExpressions;
//using AltV.Net.Enums;
//using ChoiceVServer.Companies;
//using ChoiceVServer.Model.Company;

//namespace ChoiceVServer.Controller {

//    #region Internal Order Classes
//    internal class OrderableElement {
//        private string _alterDisplayName;

//        public int ElementId { get; set; }
//        public string DisplayName {
//            get => _alterDisplayName;
//            set {
//                if(string.IsNullOrEmpty(_alterDisplayName)) {
//                    _alterDisplayName = value;
//                }
//            }
//        }
//        public string ElementClass { get; set; }
//    }

//    internal class OrderableItemElement : OrderableElement {
//        public Item Item { get; set; }
//    }

//    internal class OrderableVehicleElement : OrderableElement {
//        public VehicleModel VehicleModel { get; set; }
//    }

//    internal class MarketGarage {
//        public int MarketCompanyId { get; set; }
//        public int GarageId { get; set; }
//        public string VehicleClass { get; set; }

//        public MarketGarage(configordermarketgarages configOrderMarketGarage) {
//            MarketCompanyId = configOrderMarketGarage.companyId;
//            GarageId = configOrderMarketGarage.garageId;
//            VehicleClass = configOrderMarketGarage.vehicleClass;
//        }
//    }

//    internal class OrderMarketElement {
//        public int MarketElementId { get; set; }
//        public int MarketCompanyId { get; set; }
//        public OrderableElement OrderableElement { get; set; }
//        public int BulkCount { get; set; }
//        public decimal PricePerUnit { get; set; }
//        public TimeSpan DeliveryTimeSpan { get; set; }
//        public DayOfWeek? DeliveryStartDayOfWeek { get; set; }
//        public DateTime EstimatedArrivalNow {
//            get {
//                if(DeliveryStartDayOfWeek.HasValue) {
//                    return DateTime.Now + (DeliveryTimeSpan +
//                                           TimeSpan.FromDays(((int)DeliveryStartDayOfWeek -
//                                                              (int)DateTime.Now.DayOfWeek +
//                                                              7) % 7));
//                }

//                return DateTime.Now + DeliveryTimeSpan;

//            }
//        }

//        public decimal TotalPrice => decimal.Multiply(PricePerUnit, BulkCount);

//        public string ElementImageUrl { get; set; }
//        public string ElementWebsiteUrl { get; set; }

//    }

//    internal class OrderMarket {
//        public Company Company { get; set; }
//        public List<Company> AllowedCompanies { get; set; }
//        public List<OrderMarketElement> OrderableElements { get; }

//        public OrderMarket() {
//            OrderableElements = new List<OrderMarketElement>();
//        }
//    }

//    internal class Order {
//        public OrderableElement OrderableElement { get; set; }
//        public int OrderId { get; set; }
//        public int Amount { get; set; }
//        public int OrderedBy { get; set; }
//        public DateTime OrderDate { get; set; }
//        public DateTime DeliveryDate { get; set; }
//        public bool ArrivalMessageSent { get; set; }
//        public bool PickedUp { get; set; }
//        public int OrderedAtCompanyId { get; set; }
//        public long PhoneNumber { get; set; }
//    }
//    #endregion

//    public class OrderController : ChoiceVScript {

//        private static readonly List<Order> OrderCache = new List<Order>();
//        private static readonly List<OrderableElement> OrderableElementsCache = new List<OrderableElement>();
//        private static readonly List<OrderMarket> OrderMarketsCache = new List<OrderMarket>();
//        private static readonly List<MarketGarage> MarketGarageCache = new List<MarketGarage>();

//        private static readonly object LockObject = new object();

//        public OrderController() {
//            try {

//                // Timers
//                InvokeController.AddTimedInvoke("OrderSystemUpdateingOrders", updateOrders,
//                    TimeSpan.FromSeconds(30), true);

//                InvokeController.AddTimedInvoke("OrderSystemUpdateCache", updateCache,
//                    TimeSpan.FromMinutes(10), true);

//                // Events mit Collision-Shapes
//                EventController.addCollisionShapeEvent("USE_ORDERTERMINAL", onUsingOrderTerminal);

//                // Menü-Events
//                EventController.addMenuEvent("ORDER_NEWORDER_2", onMenuEventCreateNewOrder2);
//                EventController.addMenuEvent("ORDER_PICKUPITEMS", onPickupItems);
//                EventController.addMenuEvent("ORDER_GETKEYS", onPickupKeys);

//            } catch(Exception e) {
//                Logger.logException(e);
//            }
//        }

//        private bool onMenuEventCreateNewOrder2(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            lock(LockObject) {
//                try {
//                    if(!data.ContainsKey("Element") || !data.ContainsKey("Company")) {
//                        sendNotificationToPlayer("Bestellung kann nicht durchgeführt werden. Es fehlen Daten.",
//                            "Bestellung fehlgeschlagen.", Constants.NotifactionTypes.Danger, player);
//                        return false;
//                    }

//                    int elementId = (int)data["Element"];
//                    int companyId = (int)data["Company"];

//                    OrderMarket orderMarket = OrderMarketsCache.First(m =>
//                        m.OrderableElements.Any(e => e.MarketElementId == elementId));
//                    OrderMarketElement orderMarketElement =
//                        orderMarket.OrderableElements.First(e => e.MarketElementId == elementId);
//                    List<Company> companiesOfPlayer = getPlayerCompanies(player);
//                    Company company = CompanyController.getCompany(companyId);

//                    if(!companiesOfPlayer.Contains(company)) {
//                        sendNotificationToPlayer(
//                            "Bestellung kann nicht durchgeführt werden. Du darfst nicht für die Firma bestellen (Bankberechtigung benötigt.)",
//                            "Bestellung fehlgeschlagen.", Constants.NotifactionTypes.Danger, player);
//                        return false;
//                    }

//                    if(orderMarket.AllowedCompanies.All(c => c != company)) {
//                        sendNotificationToPlayer(
//                            "Bestellung kann nicht durchgeführt werden. Firma darf hier nicht bestellen.",
//                            "Bestellung fehlgeschlagen.", Constants.NotifactionTypes.Danger, player);
//                        return false;
//                    }

//                    if(!BankController.accountIdExists(company.CompanyBankAccount)) {
//                        sendNotificationToPlayer(
//                            $"Bestellung kann nicht durchgeführt werden. Es stimmt etwas mit dem Bankkonto deines Unternehmens {company.Name} nicht. Ist eins angelegt worden?",
//                            "Bestellung fehlgeschlagen.", Constants.NotifactionTypes.Danger, player);
//                        return false;
//                    }

//                    if(!(BankController.getAccountBalance(player, company.CompanyBankAccount) > orderMarketElement.TotalPrice)) {
//                        sendNotificationToPlayer(
//                            $"Bestellung kann nicht durchgeführt werden. Das Bankkonto von {company.Name} ist nicht ausreichend gedeckt. Ist eins angelegt worden?",
//                            "Bestellung fehlgeschlagen.", Constants.NotifactionTypes.Danger, player);
//                        return false;
//                    }

//                    using(var db = new ChoiceVDb()) {

//                        orders order = new orders {
//                            pickedUp = 0,
//                            amount = orderMarketElement.BulkCount,
//                            arrivalMessageSent = 0,
//                            deliveryDateTime = orderMarketElement.EstimatedArrivalNow,
//                            elementId = orderMarketElement.MarketElementId,
//                            orderBy = companyId,
//                            orderedAtCompanyId = orderMarket.Company.Id,
//                            orderDateTime = DateTime.Now,
//                            phoneNumber = -1
//                        };

//                        db.Add(order);
//                        db.SaveChanges();

//                        if(!BankController.transferMoney(company.CompanyBankAccount, orderMarket.Company.CompanyBankAccount,
//                            orderMarketElement.TotalPrice,
//                            $"Bestellung #{order.orderId,0:000000} bei {orderMarket.Company.Name} für {orderMarketElement.BulkCount}x {orderMarketElement.OrderableElement.DisplayName}.",
//                            player)) {
//                            db.Remove(order);
//                            db.SaveChanges();
//                            sendNotificationToPlayer("Bestellung nicht möglich.", "Bestellung nicht möglich.",
//                                Constants.NotifactionTypes.Warning, player);
//                            return false;
//                        }

//                        OrderCache.Add(new Order() {
//                            Amount = order.amount,
//                            OrderedBy = order.orderBy,
//                            ArrivalMessageSent = order.arrivalMessageSent != 0,
//                            DeliveryDate = order.deliveryDateTime,
//                            OrderDate = order.orderDateTime,
//                            OrderId = order.orderId,
//                            OrderableElement = OrderableElementsCache.First(o => o.ElementId == order.elementId),
//                            PickedUp = order.pickedUp != 0,
//                            OrderedAtCompanyId = order.orderedAtCompanyId,
//                            PhoneNumber = order.phoneNumber
//                        });

//                        sendNotificationToPlayer(
//                            $"Bestellung von {orderMarketElement.BulkCount}x {orderMarketElement.OrderableElement.DisplayName}. bei {orderMarket.Company.Name} für ${orderMarketElement.TotalPrice,0:0,00} abgeschlossen.",
//                            "Bestellung erfolgreich.", Constants.NotifactionTypes.Success, player);
//                        return true;
//                    }

//                } catch(Exception e) {
//                    Logger.logException(e);
//                }
//            }

//            return false;
//        }

//        public static void updateCache(IInvoke obj = null) {
//            lock(LockObject) {
//                try {
//                    Logger.logDebug("OrderSystem: Rebuilding Cache Task Started");
//                    List<orders> openOrdersList;
//                    List<configorderableelement> configOrderableElements;
//                    List<configorderableelementtoitem> configOrderableElementToItems;
//                    List<configorderableelementtovehicle> configOrderableElementToVehicles;
//                    List<configordermarketelements> configOrderMarketElements;
//                    List<configordermarketusablecompanies> configOrderMarketUsableCompanies;
//                    List<configordermarketgarages> configOrderMarketGarages;
//                    List<items> itemList = new List<items>();

//                    // Get all data-sets
//                    using(var db = new ChoiceVDb()) {
//                        openOrdersList = db.orders.Where(o => o.pickedUp == 0).ToList();
//                        configOrderableElements = db.configorderableelement.ToList();
//                        configOrderableElementToItems = db.configorderableelementtoitem.ToList();
//                        configOrderableElementToVehicles = db.configorderableelementtovehicle.ToList();
//                        configOrderMarketElements = db.configordermarketelements.ToList();
//                        configOrderMarketUsableCompanies = db.configordermarketusablecompanies.ToList();
//                        configOrderMarketGarages = db.configordermarketgarages.ToList();

//                        foreach(configorderableelementtoitem o in configOrderableElementToItems) {
//                            itemList.Add(db.items.FirstOrDefault(i => i.id == o.itemId));
//                        }

//                        itemList.RemoveAll(i => i == null);
//                    }

//                    Logger.logDebug("OrderSystem: Rebuilding Orderable Elements");
//                    OrderableElementsCache.Clear();
//                    foreach(configorderableelement orderableElement in configOrderableElements) {
//                        configorderableelementtoitem itemElement =
//                            configOrderableElementToItems.FirstOrDefault(i =>
//                                i.elementId == orderableElement.elementId);
//                        configorderableelementtovehicle itemVehicle =
//                            configOrderableElementToVehicles.FirstOrDefault(v =>
//                                v.elementId == orderableElement.elementId);

//                        if(itemElement != null) {
//                            OrderableItemElement orderableItemElement = new OrderableItemElement {
//                                ElementId = orderableElement.elementId,
//                                DisplayName = orderableElement.alterDisplayName,
//                                ElementClass = orderableElement.elementClass,
//                                Item = new Item(itemList.First(i => i.id == itemElement.itemId))
//                            };
//                            OrderableElementsCache.Add(orderableItemElement);

//                        } else if(itemVehicle != null) {
//                            OrderableVehicleElement orderableVehicleElement = new OrderableVehicleElement {
//                                ElementId = orderableElement.elementId,
//                                DisplayName = orderableElement.alterDisplayName,
//                                ElementClass = orderableElement.elementClass,
//                                VehicleModel = (VehicleModel)itemVehicle.vehicleModelId
//                            };
//                            OrderableElementsCache.Add(orderableVehicleElement);
//                        }
//                    }

//                    Logger.logDebug("OrderSystem: Rebuilding Order-Markets");
//                    OrderMarketsCache.Clear();
//                    List<int> marketCompanyIds = new List<int>();
//                    marketCompanyIds.AddRange(configOrderMarketElements.Select(m => m.companyId).ToArray());
//                    marketCompanyIds.AddRange(configOrderMarketUsableCompanies.Select(m => m.MarketCompanyId).ToArray());
//                    int[] cleanedMarketCompanyIds = marketCompanyIds.Distinct().ToArray();

//                    List<Company> marketCompanies = CompanyController.AllCompanies.Where(c => cleanedMarketCompanyIds.Contains(c.Id)).ToList();

//                    foreach(Company company in marketCompanies) {
//                        int[] includeList = configOrderMarketUsableCompanies
//                            .Where(c => c.MarketCompanyId == company.Id && c.IsInclude == 0).Select(c => c.CompanyId)
//                            .ToArray();

//                        int[] excludeList = configOrderMarketUsableCompanies
//                            .Where(c => c.MarketCompanyId == company.Id && c.IsInclude != 0).Select(c => c.CompanyId)
//                            .ToArray();

//                        OrderMarket orderMarket = new OrderMarket { Company = company };

//                        if(includeList.Any()) {
//                            orderMarket.AllowedCompanies = CompanyController.AllCompanies
//                                .Where(c => includeList.Contains(c.Id)).ToList();
//                        } else {
//                            orderMarket.AllowedCompanies = CompanyController.AllCompanies
//                                .Where(c => !excludeList.Contains(c.Id)).ToList();
//                        }

//                        Logger.logDebug($"OrderSystem: Rebuilding MarketItems for Company {company.Name}");

//                        configordermarketelements[] marketElements = configOrderMarketElements.Where(e => e.companyId == company.Id).ToArray();

//                        foreach(configordermarketelements marketElement in marketElements) {
//                            OrderMarketElement orderMarketElement = new OrderMarketElement() { BulkCount = marketElement.bulkCount, DeliveryTimeSpan = TimeSpan.FromDays(marketElement.deliveryTimespan), ElementImageUrl = marketElement.elementImage, ElementWebsiteUrl = marketElement.elementWebsite, MarketCompanyId = marketElement.companyId, OrderableElement = OrderableElementsCache.First(o => o.ElementId == marketElement.elementId), PricePerUnit = marketElement.pricePerUnit, MarketElementId = marketElement.marketElementId };
//                            if(marketElement.deliveryStartDayOfWeek > 0 && marketElement.deliveryStartDayOfWeek < 7) {
//                                orderMarketElement.DeliveryStartDayOfWeek =
//                                    (DayOfWeek)marketElement.deliveryStartDayOfWeek;
//                            }

//                            if(marketElement.deliveryStartDayOfWeek == 7) {
//                                orderMarketElement.DeliveryStartDayOfWeek = DayOfWeek.Sunday;
//                            }
//                            orderMarket.OrderableElements.Add(orderMarketElement);
//                        }
//                        OrderMarketsCache.Add(orderMarket);
//                    }

//                    Logger.logDebug("OrderSystem: Rebuilding Orders");
//                    OrderCache.Clear();

//                    foreach(orders order in openOrdersList) {
//                        OrderCache.Add(new Order() { Amount = order.amount, OrderedBy = order.orderBy, ArrivalMessageSent = order.arrivalMessageSent != 0, DeliveryDate = order.deliveryDateTime, OrderDate = order.orderDateTime, OrderId = order.orderId, OrderableElement = OrderableElementsCache.First(o => o.ElementId == order.elementId), PickedUp = order.pickedUp != 0, OrderedAtCompanyId = order.orderedAtCompanyId });
//                    }

//                    MarketGarageCache.Clear();
//                    foreach(configordermarketgarages configOrderMarketGarage in configOrderMarketGarages) {
//                        MarketGarageCache.Add(new MarketGarage(configOrderMarketGarage));
//                    }

//                } catch(Exception e) {
//                    Logger.logException(e);
//                }
//            }

//            updateOrders(obj);
//        }

//        private bool onUsingOrderTerminal(IPlayer player, CollisionShape collisionShape, Dictionary<string, object> data) {
//            lock(LockObject) {
//                try {

//                    if(!data.Any()) {
//                        return false;
//                    }

//                    OrderMarket orderMarket = null;

//                    if(int.TryParse(data["MarketId"].ToString(), out var companyId)) {
//                        orderMarket = OrderMarketsCache.FirstOrDefault(o => o.Company.Id == companyId);
//                    }

//                    if(orderMarket == null) {
//                        orderMarket = OrderMarketsCache.FirstOrDefault(o =>
//                            string.Equals(o.Company.Name, data["MarketId"].ToString(),
//                                StringComparison.OrdinalIgnoreCase));
//                    }

//                    if(orderMarket == null) {
//                        sendNotificationToPlayer("Hier wird nichts verkauft.", "Kein Shop",
//                            Constants.NotifactionTypes.Warning, player);
//                        return false;
//                    }

//                    List<Company> companiesOfPlayer = getPlayerCompanies(player);

//                    Menu playerMenu = new Menu(orderMarket.Company.Name, "Warenbestellsystem V. 1.0");


//                    List<Company> usableCompanies = orderMarket.AllowedCompanies.Where(c => companiesOfPlayer.Contains(c)).ToList();

//                    if(!usableCompanies.Any()) {
//                        sendNotificationToPlayer("Du kannst hier nichts bestellen.", "Bestellen nicht möglich.", Constants.NotifactionTypes.Warning, player);
//                        return false;
//                    }

//                    Menu orderMenu = new Menu("Bestellen", "Geben Sie hier Ihre Bestellungen auf.");
//                    foreach(Company usableCompany in usableCompanies) {
//                        Menu companyMenu = new Menu($"... für {usableCompany.Name}", $"Waren für {usableCompany.Name} bestellen.");
//                        addOrderSubMenu(companyMenu, orderMarket, usableCompany, player);
//                        orderMenu.addMenuItem(new MenuMenuItem("... für {usableCompany.Name}", companyMenu, MenuItemStyle.green));
//                    }
//                    playerMenu.addMenuItem(new MenuMenuItem("Bestellen", orderMenu, MenuItemStyle.green));
//                    Menu receiveMenu = new Menu("Abholen", "Bestellungen für das Unternehmen abholen.");

//                    foreach(Company company in companiesOfPlayer) {
//                        int itemsToPickup = OrderCache.Count(o => o.OrderedBy == company.Id && !o.PickedUp && o.DeliveryDate > DateTime.Now &&
//                                                                  o.OrderableElement is OrderableItemElement && o.OrderedAtCompanyId == orderMarket.Company.Id);

//                        int vehiclesToPickup = OrderCache.Count(o => o.OrderedBy == company.Id && !o.PickedUp && o.DeliveryDate > DateTime.Now &&
//                                                                     o.OrderableElement is OrderableVehicleElement && o.OrderedAtCompanyId == orderMarket.Company.Id);

//                        if(itemsToPickup == 0 && vehiclesToPickup == 0) {
//                            continue;
//                        }

//                        Menu companyMenu = new Menu($"{company.Name}", $"Bestellungen für {company.Name} abholen.");

//                        companyMenu.addMenuItem(new ClickMenuItem("Ware abholen", "Dir wird soviel Ware übergeben wie sie noch tragen können.", $"{itemsToPickup}x", "ORDER_PICKUPITEMS", MenuItemStyle.green).withData(new Dictionary<string, dynamic>() { { "Company", company.Id } }));

//                        if(vehiclesToPickup > 0) {
//                            if(vehiclesToPickup == 1) {
//                                companyMenu.addMenuItem(new ClickMenuItem("Schlüssel abholen",
//                                    "Wir übergeben dir den Schlüssel für das Fahrzeug. Dieses findest du in der Garage.",
//                                    $"{vehiclesToPickup}x",
//                                    "ORDER_GETKEYS",
//                                    MenuItemStyle.green).withData(new Dictionary<string, object>() { { "MarketId", orderMarket.Company.Id }, { "Company", company.Id } }));
//                            } else {
//                                companyMenu.addMenuItem(new ClickMenuItem("Schlüssel abholen", "Wir übergeben dir die Schlüssel für die Fahrzeuge. Diese findest du in der Garage.",
//                                    $"{vehiclesToPickup}x",
//                                    "ORDER_GETKEYS",
//                                    MenuItemStyle.green).withData(new Dictionary<string, object>() { { "MarketId", orderMarket.Company.Id }, { "Company", company.Id } }));
//                            }
//                        }
//                    }

//                    playerMenu.addMenuItem(new MenuMenuItem("Abholen", receiveMenu, MenuItemStyle.green));
//                    player.showMenu(playerMenu);

//                    return true;
//                } catch(Exception e) {
//                    Logger.logException(e);
//                }
//            }
//            return false;
//        }

//        private List<Company> getPlayerCompanies(IPlayer player) {
//            companypermissions bankingPermission = CompanyController.AllCompanyPermissions["BANK_MANAGEMENT"];
//            int characterId = player.getCharacterId();
//            return CompanyController.getCompanies(player).Where(c => c.findEmployee(characterId) != null && (c.findEmployee(characterId).JobLevel.IsCEO || c.findEmployee(characterId).JobLevel.hasPermission(bankingPermission))).ToList();
//        }

//        private void addOrderSubMenu(Menu companyMenu, OrderMarket orderMarket, Company usableCompany, IPlayer player) {
//            try {
//                List<string> list = orderMarket.OrderableElements.Select(o => o.OrderableElement.ElementClass).Distinct().ToList();
//                foreach(string elementClass in list) {
//                    Menu elementClassMenu = new Menu($"{elementClass}", $"Waren aus der Gruppe {elementClass}.");
//                    addElementsToClassMenu2(elementClassMenu, elementClass, orderMarket, usableCompany, player);
//                    elementClassMenu.addMenuItem(new MenuMenuItem($"{elementClass}", elementClassMenu, MenuItemStyle.green));
//                }
//            } catch(Exception e) {
//                Logger.logException(e);
//            }
//        }

//        private void addElementsToClassMenu2(Menu elementClassMenu, string elementClass, OrderMarket orderMarket, Company usableCompany, IPlayer player) {
//            try {
//                List<OrderMarketElement> orderMarketElements = orderMarket.OrderableElements.Where(e => string.Equals(e.OrderableElement.ElementClass, elementClass)).ToList();
//                foreach(OrderMarketElement orderMarketElement in orderMarketElements) {
//                    elementClassMenu.addMenuItem(
//                        new ClickMenuItem($"{orderMarketElement.OrderableElement.DisplayName}",
//                            $"{orderMarketElement.BulkCount}x {orderMarketElement.OrderableElement.DisplayName} für ${orderMarketElement.TotalPrice:0.00} bestellen.",
//                            $"({orderMarketElement.BulkCount}x)",
//                            "ORDER_NEWORDER_2",
//                            MenuItemStyle.green)
//                            .withData(new Dictionary<string, dynamic>() { { "Element", orderMarketElement.MarketElementId }, { "Company", usableCompany.Id } }));
//                }
//            } catch(Exception e) {
//                Logger.logException(e);
//            }
//        }

//        private class WarehousedPhoneMessageInfo {
//            public List<long> PhoneNumbers { get; set; }
//            public bool EverythingWasWarehoused { get; set; }
//            public int ItemsFilledCount { get; set; }
//        }

//        private static void updateOrders(IInvoke obj) {
//            lock(LockObject) {
//                try {
//                    companypermissions bankingPermission = CompanyController.AllCompanyPermissions["BANK_MANAGEMENT"];
//                    List<long> phonesToMessage = new List<long>();
//                    List<WarehousedPhoneMessageInfo> warehousedPhoneMessageInfos = new List<WarehousedPhoneMessageInfo>();

//                    Dictionary<int, List<Order>> ordersOfCompany = OrderCache.Where(o => !o.ArrivalMessageSent && o.DeliveryDate < DateTime.Now).GroupBy(o => o.OrderedAtCompanyId).ToDictionary(o => o.Key, o => o.ToList());

//                    List<Order> totalOrders = new List<Order>();
//                    foreach(int marketId in ordersOfCompany.Keys) {
//                        Company marketCompany = CompanyController.AllCompanies.First(c => c.Id == marketId);
//                        var orders = ordersOfCompany[marketId];

//                        Company[] companies = orders.Select(o => o.OrderedBy).Distinct().Select(CompanyController.getCompany).ToArray();

//                        foreach(Company company in companies) {
//                            var companyEmployees = company.Employees.Where(e =>
//                                e.JobLevel.IsCEO || e.JobLevel.hasPermission(bankingPermission)).ToArray();

//                            if(company is CompanyWorkshop) {
//                                int inventoryId = CompanyWorkshopController.getWorkshopInventoryId(company.Id);
//                                Inventory inventory = InventoryController.AllInventories[inventoryId];
//                                int itemFilledCount = fillInventoryWithOrders(inventory, company.Id, marketId, out bool ordersLeft);
//                                warehousedPhoneMessageInfos.Add(new WarehousedPhoneMessageInfo() { EverythingWasWarehoused = !ordersLeft, PhoneNumbers = companyEmployees.Select(companyEmployee => companyEmployee.PhoneNumber).ToList(), ItemsFilledCount = itemFilledCount });
//                            } else {
//                                phonesToMessage.AddRange(companyEmployees.Select(companyEmployee => companyEmployee.PhoneNumber));
//                            }
//                        }

//                        foreach(long phoneNumber in phonesToMessage) {
//                            sendTextMessageToPlayer(phoneNumber, marketCompany,
//                                $"Ihre Bestellung steht ab sofort im Lager zur Abholung bereit. Es warten insgesamt {orders.Count(o => o.PhoneNumber == phoneNumber && !o.ArrivalMessageSent && !o.PickedUp)} Liefereinheiten zur Abholung auf Sie.");
//                        }

//                        foreach(WarehousedPhoneMessageInfo warehousedPhoneMessageInfo in warehousedPhoneMessageInfos) {
//                            foreach(long phoneNumber in warehousedPhoneMessageInfo.PhoneNumbers) {
//                                if(warehousedPhoneMessageInfo.EverythingWasWarehoused) {
//                                    sendTextMessageToPlayer(phoneNumber, marketCompany,
//                                        $"Ihre Bestellung ist angekommen! Es wurden {warehousedPhoneMessageInfo.ItemsFilledCount} Einheiten ihrem Lageristen übergeben.");
//                                } else {
//                                    sendTextMessageToPlayer(phoneNumber, marketCompany,
//                                        $"Ihre Bestellung ist angekommen! Es wurden {warehousedPhoneMessageInfo.ItemsFilledCount} Einheiten ihrem Lageristen übergeben. Leider konnten nicht alle Waren eingeliefert werden. Wir versuchen es später noch einmal.");
//                                }
//                            }
//                        }

//                        totalOrders.AddRange(orders);
//                    }

//                    using(var db = new ChoiceVDb()) {
//                        foreach(Order order in totalOrders) {
//                            orders orderDb = db.orders.FirstOrDefault(o => o.orderId == order.OrderId);
//                            if(orderDb != null) {
//                                orderDb.arrivalMessageSent = 1;
//                            }

//                            order.ArrivalMessageSent = true;
//                        }

//                        db.SaveChanges();
//                    }

//                } catch(Exception e) {
//                    Logger.logException(e);
//                }
//            }
//        }

//        #region Helpers
//        /// <summary>
//        /// Send Text-Message to Player
//        /// </summary>
//        private static void sendTextMessageToPlayer(long phoneNumber, Company company, string text, IDictionary<string, string> keyValueReplacement = null) {
//            if(phoneNumber <= 0) {
//                return;
//            }

//            if(keyValueReplacement != null) {
//                foreach(KeyValuePair<string, string> keyValuePair in keyValueReplacement) {
//                    text.Replace($"{{{keyValuePair.Key}}}", keyValuePair.Value, StringComparison.OrdinalIgnoreCase);
//                }
//            }

//            // ToDo: SMS-System animplementieren!
//        }

//        internal static void sendNotificationToPlayer(string text, string shortText, Constants.NotifactionTypes notificationType, IPlayer player, bool isAdminNotification = false) {
//            try {
//                Logger.logDebug($"{shortText}\t{notificationType} {text}");
//                if(player == null) {
//                    return;
//                }
//                if(isAdminNotification) {
//                    string pretext = string.Empty;
//                    if(notificationType == Constants.NotifactionTypes.Info) pretext = "[i]";
//                    else if(notificationType == Constants.NotifactionTypes.Warning) pretext = "[!]";
//                    else if(notificationType == Constants.NotifactionTypes.Danger) pretext = "[!!!]";
//                    else if(notificationType == Constants.NotifactionTypes.Success) pretext = "[S]";
//                    ChoiceVAPI.SendChatMessageToPlayer(player, $"{pretext} {shortText}");
//                    var result = Regex.Split(text, " |\r|\n");
//                    foreach(string s in result) {
//                        ChoiceVAPI.SendChatMessageToPlayer(player, s);
//                    }
//                    return;
//                }
//                //TODO_ERK
//                player.sendNotification(notificationType, text, shortText, Constants.NotifactionImages.Hotel);

//            } catch(Exception e) {
//                Logger.logException(e);
//            }
//        }

//        private bool onPickupKeys(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
//            lock(LockObject) {
//                try {
//                    int marketId = (int)data["MarketId"];

//                    int companyId = (int)data["Company"];
//                    List<Company> companiesOfPlayer = getPlayerCompanies(player);
//                    Company company = CompanyController.getCompany(companyId);
//                    if(!companiesOfPlayer.Contains(company)) {
//                        return false;
//                    }

//                    List<Order> orders = OrderCache.Where(o => o.OrderedBy == companyId && !o.PickedUp && o.DeliveryDate > DateTime.Now &&
//                                                               o.OrderableElement is OrderableVehicleElement && o.OrderedAtCompanyId == marketId).ToList();

//                    int carCount = 0;
//                    using(var db = new ChoiceVDb()) {
//                        foreach(Order order in orders) {
//                            try {
//                                OrderableVehicleElement orderableVehicleElement = (OrderableVehicleElement)order.OrderableElement;
//                                MarketGarage garage = MarketGarageCache.FirstOrDefault(g =>
//                                    g.MarketCompanyId == order.OrderedAtCompanyId &&
//                                    g.VehicleClass == orderableVehicleElement.ElementClass);

//                                //Specific Garage-Class not found. Fallback to default.
//                                if(garage == null) {
//                                    garage = MarketGarageCache.FirstOrDefault(g =>
//                                        g.MarketCompanyId == order.OrderedAtCompanyId &&
//                                        string.IsNullOrEmpty(g.VehicleClass));
//                                }

//                                if(garage == null) {
//                                    sendNotificationToPlayer("Der Händler kann das Fahrzeug nicht ausladen. Bitte beschwere dich umgehend bei der Marktleitung!", "Fehler bei Ausgabe.", Constants.NotifactionTypes.Danger, player);
//                                    return false;
//                                }

//                                //ToDo: NoErrX mit Dieter prüfen!
//                                uint vehicleModel = (uint)orderableVehicleElement.VehicleModel;
//                                ChoiceVVehicle vehicle = VehicleController.createVehicle(player, vehicleModel, garage.GarageId);
//                                orders first = db.orders.First(o => o.orderId == order.OrderId);
//                                first.pickedUp = 1;
//                                carCount++;
//                            } catch(Exception e) {
//                                Logger.logException(e);
//                            }
//                        }

//                        db.SaveChanges();
//                    }

//                    if(carCount == 1) {
//                        sendNotificationToPlayer("Dir wurde der Schlüssel für ein Fahrzeug übergeben. Du findest es in der Garage beim Händler.", "Schlüssel übergeben.", Constants.NotifactionTypes.Success, player);
//                    } else if(carCount > 1) {
//                        sendNotificationToPlayer($"Dir wurden die Schlüssel für {carCount} Fahrzeuge übergeben. Du findest sie in der Garage beim Händler.", $"{carCount} Schlüssel übergeben.", Constants.NotifactionTypes.Success, player);
//                    } else {
//                        sendNotificationToPlayer("Aktuell gibt es hier nichts für dich zum abholen.", "Kein Schlüssel übergeben.", Constants.NotifactionTypes.Warning, player);
//                    }

//                    return true;

//                } catch(Exception e) {
//                    Logger.logException(e);
//                }
//            }

//            return false;
//        }

//        private bool onPickupItems(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
//            lock(LockObject) {
//                try {

//                    int marketId = (int)data["MarketId"];
//                    Inventory inventory = player.getInventory();

//                    int companyId = (int)data["Company"];
//                    List<Company> companiesOfPlayer = getPlayerCompanies(player);
//                    Company company = CompanyController.getCompany(companyId);

//                    if(!companiesOfPlayer.Contains(company)) {
//                        return false;
//                    }


//                    int itemCount = fillInventoryWithOrders(inventory, companyId, marketId, out bool ordersLeft);
//                    if(itemCount == 1) {
//                        sendNotificationToPlayer($"Dir wurde {itemCount} Gegenstand übergeben.", $"{itemCount} Gegenstand übergeben.", Constants.NotifactionTypes.Success, player);
//                    } else if(itemCount > 1) {
//                        sendNotificationToPlayer($"Dir wurden {itemCount} Gegenstände übergeben.", $"{itemCount} Gegenstände übergeben.", Constants.NotifactionTypes.Success, player);
//                    } else if(itemCount == 0 && ordersLeft) {
//                        sendNotificationToPlayer("Du kannst nicht mehr tragen.", "Kein Gegenstand übergeben.", Constants.NotifactionTypes.Warning, player);
//                    } else {
//                        sendNotificationToPlayer("Aktuell gibt es hier nichts für dich zum abholen.", "Kein Gegenstand übergeben.", Constants.NotifactionTypes.Warning, player);
//                    }
//                    return true;
//                } catch(Exception e) {
//                    sendNotificationToPlayer("Fehler bei der Übergabe. Versuche es noch einmal. Wenn diese Meldung wiederholt auftritt wende dich bitte an die Marktaufsicht.", "Handelsfehler.", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(e);
//                }
//            }
//            return false;
//        }

//        private static int fillInventoryWithOrders(Inventory inventory, int companyId, int marketId, out bool ordersLeft) {
//            int itemCount = 0;
//            float availableWeight = inventory.MaxWeight - inventory.CurrentWeight;

//            List<Order> orders = OrderCache.Where(o => o.OrderedBy == companyId && !o.PickedUp &&
//                                                       o.DeliveryDate > DateTime.Now &&
//                                                       o.OrderableElement is OrderableItemElement &&
//                                                       o.OrderedAtCompanyId == marketId).ToList();

//            orders.Sort((a, b) =>
//                (((OrderableItemElement)b.OrderableElement).Item.Weight.CompareTo((((OrderableItemElement)a.OrderableElement)
//                    .Item.Weight))));
//            float spaceLeft = availableWeight;

//            List<Order> orderChanged = new List<Order>();

//            try {
//                foreach(Order order in orders) {
//                    var itemWeight = ((OrderableItemElement)order.OrderableElement).Item.Weight;
//                    bool doContinue = true;
//                    if(spaceLeft > itemWeight) {
//                        orderChanged.Add(order);
//                        while(spaceLeft > itemWeight && order.Amount > 0 && doContinue) {
//                            doContinue = inventory.addItem(((OrderableItemElement)order.OrderableElement).Item);
//                            order.Amount--;
//                            itemCount++;
//                        }
//                    }
//                }
//            } catch(Exception e) {
//                Logger.logException(e);
//            }

//            using(var db = new ChoiceVDb()) {
//                foreach(Order order in orderChanged) {
//                    try {
//                        orders first = db.orders.First(o => o.orderId == order.OrderId);
//                        if(order.Amount > 0) {
//                            first.amount = order.Amount;
//                        } else {
//                            order.PickedUp = true;
//                            first.pickedUp = 1;
//                        }
//                    } catch(Exception e) {
//                        Logger.logException(e);
//                    }
//                }

//                db.SaveChanges();
//            }

//            ordersLeft = orders.Any();

//            return itemCount;
//        }

//        private Dictionary<string, object> appendData(OrderMarketElement orderMarketElement) {
//            return new Dictionary<string, object>() { { "MarketElementId", orderMarketElement.MarketElementId } };
//        }

//        #endregion

//    }
//}
