//using AltV.Net.Data;
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
//using System.Text;
//using static ChoiceVServer.Base.Constants;

//namespace ChoiceVServer.Controller {
//    class DealerController : ChoiceVScript {
//        public static bool dealerSpawned = false;
//        public static Inventory pepeShapeInventory;
//        public static List<DealerLevel> levelList = new List<DealerLevel>();
//        public static List<DealerShop> shopList = new List<DealerShop>();
//        public static List<DealerPositions> positionList = new List<DealerPositions>();
//        public static List<DealerSpawns> spawnList = new List<DealerSpawns>();
//        public static List<Ped> pedList = new List<Ped>();
//        public static List<DealerInventory> inventoryList = new List<DealerInventory>();
//        public static List<DealerShopList> dealerShopList = new List<DealerShopList>();
//        public static List<DealerTakeover> takeoverList = new List<DealerTakeover>();
//        public static List<DealerNumber> dealerNumberList = new List<DealerNumber>();
//        public static List<GangNumbers> gangNumberList = new List<GangNumbers>();

//        public DealerController() {
//            spawnPepe();
//            onStartUp();
//            EventController.addCollisionShapeEvent("PEPE_TALK", onPepeTalk);
//            EventController.addCollisionShapeEvent("DEALER_TALK", onDealerTalk);

//            EventController.PlayerDisconnectedDelegate += onDisconnect;

//            EventController.addMenuEvent("SET_SIDE", onGangSetSide);

//            EventController.addMenuEvent("DEALER_POLICE_CHECK", onPoliceSearch);
//            EventController.addMenuEvent("DEALER_POLICE_ASK", onPoliceAsk);
//            EventController.addMenuEvent("DEALER_POLICE_ARREST", onPoliceArrest);

//            EventController.addMenuEvent("DEALER_ORDER", onOrder);
//            EventController.addMenuEvent("DEALER_SHOW_INVENTORY", onDealerShowInventory);
//            EventController.addMenuEvent("DEALER_SELL_DRUGS", onDealerSellDrugs);
//            EventController.addMenuEvent("DEALER_TAKEOVER", onDealerTakeover);
//            EventController.addMenuEvent("DEALER_THREAT", onDealerThreat);
//            EventController.addMenuEvent("DEALER_ITEM_TAKE", onDealerItemTake);

//            EventController.addMenuEvent("PEPE_ASK_LOCATION", onAskPepe);
//            EventController.addMenuEvent("PEPE_SHAPE_TAKE", onPepeShapeTake);

//            EventController.addMenuEvent("Dealer_setPhoneNumber", onSetPhoneNumber);

//            EventController.addCollisionShapeEvent("PEPE_SHAPE_INTERACT", onPepeShapeInteract);
//            EventController.LongTickDelegate += takeoverCheck;
//        }



//        //TODO: GETORDER
//        #region PoliceMethods
//        private bool onPoliceSearch(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var dealerId = data["DealerId"];
//            var inventory = (Inventory)getDealerInventory(dealerId);
//            InventoryController.showMoveInventory(player, player.getInventory(), inventory);
//            var itemList = inventory.getAllItems();
//            if (itemList.Count >= 1) {
//                var inventoryCheck = inventoryList.FirstOrDefault(x => x.dealerInventory.Id == inventory.Id);
//                if (inventoryCheck != null) {
//                    inventoryCheck.illegalContentCheck = true;
//                }
//            }
//            return true;
//        }

//        private bool onPoliceAsk(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var dealerId = data["DealerId"];
//            var random = new Random();
//            var dealerFlag = getDealerFlag(dealerId);
//            var randomInt = random.Next(1, 100);
//            if (randomInt <= 33 && dealerFlag >= 1) {
//                var companyList = CompanyController.AllCompanies;
//                var gangCheck = companyList.FirstOrDefault(x => x.Id == dealerFlag);
//                if (gangCheck != null) {
//                    player.sendNotification(NotifactionTypes.Info, $"Scheiße okay Man! Momentan ticke ich für die {gangCheck.Name}", "Dealer Aussage");
//                }
//            } else {
//                player.sendNotification(NotifactionTypes.Info, "Ich weiß von garnichts man!", "Dealer Aussage");
//            }
//            return true;
//        }

//        private bool onPoliceArrest(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var dealerId = data["DealerId"];
//            var dealerPedModel = getDealerModel(dealerId);
//            var dealerPed = pedList.FirstOrDefault(x => x.Model == dealerPedModel);
//            if (dealerPed != null) {
//                PedController.destroyPed(dealerPed);
//                pedList.Remove(dealerPed);
//            } else {
//                player.sendBlockNotification("Du kannst das Ped grade nicht entfernen!", "Fehler");
//                Logger.logError("DealerController.OnPoliceArrest; Ped Konnte nicht entfernt werden!");
//            }
//            return true;
//        }
//        #endregion

//        #region OrderMethods
//        private bool onOrder(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            using (var db = new ChoiceVDb()) {
//                if (data.ContainsKey("Item") && data.ContainsKey("ItemListEntry") && data.ContainsKey("Pepe") && data.ContainsKey("MaxAmount")) {
//                    var item = (Item)data["Item"];
//                    var listItem = (DealerShop)data["ItemListEntry"];
//                    var orderAmount = 1;
//                    var pepe = (int)data["Pepe"];
//                    var dealerId = 0;
//                    if (pepe == 0 && data.ContainsKey("DealerId")) {
//                        dealerId = (int)data["DealerId"];
//                    }
//                    InputMenuItem.InputMenuItemEvent inputItem = menuItemCefEvent as InputMenuItem.InputMenuItemEvent;
//                    if (inputItem != null) {
//                        var tryParse = int.TryParse(inputItem.input, out var inputAmount);
//                        if ((tryParse)) {
//                            if (inputAmount > data["MaxAmount"]) {
//                                player.sendNotification(NotifactionTypes.Warning, "So viel hab ich nicht!", "falscher Input"); //PEPE Bild
//                                return true;
//                            }
//                            if (inputAmount <= 0) {
//                                player.sendNotification(NotifactionTypes.Warning, "Der Input muss mindestens 1 betragen", "falscher Input");
//                                return true;
//                            } else {
//                                orderAmount = inputAmount;
//                            }
//                        } else {
//                            player.sendNotification(NotifactionTypes.Warning, "Der Input muss eine Zahl sein!", "falscher Input");
//                            return true;
//                        }
//                    }
//                    if (pepe == 1) {
//                        if (player.getCash() < ((decimal)listItem.price * orderAmount)) {
//                            player.sendNotification(NotifactionTypes.Warning, "So viel Geld hast du nicht!", "Zu wenig Geld");
//                            return true;
//                        } else {
//                            player.removeCash((decimal)(listItem.price * orderAmount));
//                        }
//                    } else {
//                        var currentBalance = getGangDealerBalance(getGangId(player), dealerId);
//                        if (currentBalance < ((int)listItem.price * orderAmount)){
//                            player.sendNotification(NotifactionTypes.Warning, "So viel hast du bei mir nicht offen!", "Zu wenig Geld");
//                            return true;
//                        } else {
//                            removeGangDealerBalance(getGangId(player), dealerId, (int)(listItem.price * orderAmount));
//                        }
//                    }
//                    var deliveryTime = DateTime.Now;
//                    var newDeliveryTime = deliveryTime.AddDays(listItem.deliveryTime);
//                    var orderCheck = db.gangorders.FirstOrDefault(x => x.itemid == item.ConfigId && x.deliverdate.Date == DateTime.Today.AddDays(listItem.deliveryTime));
//                    if (orderCheck != null) {
//                        orderCheck.amount += orderAmount;
//                        db.gangorders.Update(orderCheck);
//                    } else {
//                    var newOrder = new gangorders {
//                        dealerid = dealerId,
//                        pepe = pepe,
//                        deliverdate = newDeliveryTime,
//                        gangid = getGangId(player),
//                        itemid = item.ConfigId,
//                        amount = orderAmount,
//                    };
//                    db.gangorders.Add(newOrder);
//                    }
//                    player.sendNotification(NotifactionTypes.Info, "Jo, ich schreib dir wenn die Lieferung da ist.", "Pepe");
//                    db.SaveChanges();
//                } else {
//                    Logger.logError("Error at DealerController.onOrder. DataKey was not found");
//                }
//                return true;
//            }
//        }
//        #endregion

//        #region PepeMethods

//        private bool onPepeShapeTake(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            using (var db = new ChoiceVDb()) {
//                var order = (gangorders)data["Order"];
//                var playerinv = player.getInventory();
//                var amount = 1;
//                InputMenuItem.InputMenuItemEvent inputItem = menuItemCefEvent as InputMenuItem.InputMenuItemEvent;
//                if (inputItem != null) {
//                    var tryparse = int.TryParse(inputItem.input, out var newAmount);
//                    if (!(tryparse)) {
//                        player.sendNotification(NotifactionTypes.Warning, "Der Input muss eine Zahl sein", "falscher Input");
//                        return true;
//                    }
//                    if (newAmount <= 0) {
//                        player.sendNotification(NotifactionTypes.Warning, "Der Input muss größer 0 sein", "falscher Input");
//                        return true;
//                    } else {
//                        amount = newAmount;
//                    }
//                }
//                if (amount <= 0) { amount = 1; }
//                var configItem = InventoryController.AllConfigItems[order.itemid];
//                var item = InventoryController.createGenericStackableItem(configItem, amount, -1);
//                var itemCheck = pepeShapeInventory.addItem(item, true);
//                InventoryController.showMoveInventory(player, playerinv, pepeShapeInventory);
//                order.amount -= amount;
//                if (order.amount <= 0) {
//                    db.gangorders.Remove(order);
//                } else {
//                    db.gangorders.Update(order);
//                }
//                db.SaveChanges();
//                return true;
//            }
//        }
//        private bool onPepeShapeInteract(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> data) {
//            using (var db = new ChoiceVDb()) {
//                var gangId = getGangId(player);
//                if (gangId != 0) {
//                    var menu = new Menu("Pepes Versteck", "Ist was für dich dabei?");
//                    var counter = 0;
//                    foreach (var order in db.gangorders) {
//                        if (DateTime.Now > order.deliverdate && gangId == order.gangid) {
//                            counter++;
//                            var configItem = InventoryController.AllConfigItems[order.itemid];
//                            if (order.amount == 1) {
//                                menu.addMenuItem(new ClickMenuItem($"{configItem.name}", "", "Anzahl: 1", "PEPE_SHAPE_TAKE").withData(new Dictionary<string, dynamic> { { "Order", order } }));
//                            } else {
//                                menu.addMenuItem(new InputMenuItem($"{configItem.name}", "", $"Anzahl: {order.amount}", "PEPE_SHAPE_TAKE").withData(new Dictionary<string, dynamic> { { "Order", order } }));
//                            }

//                        }
//                    }
//                    if (counter == 0) {
//                        menu.addMenuItem(new StaticMenuItem("Da ist nichts für dich", "", ""));
//                    }
//                    player.showMenu(menu);
//                }
//            }
//            return true;
//        }

//        private bool onAskPepe(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            player.sendNotification(NotifactionTypes.Info, "Am Hafen in der Mitte der Elysian Island gibt es eine Firma namens: Bugstars Pest Control. In deren Lagerhalle stehen in der mitte ein paar Kisten, dort wird die Lieferung versteckt sein.", "");
//            return true;
//        }
//        private void spawnPepe() {
//            var position = new Position(-53, -1216, 28 - 0.27f);
//            PedController.createPed("g_m_y_mexgoon_01", position, 96f, "PEPE_TALK", "");
//        }


//        private bool onGangSetSide(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            using (var db = new ChoiceVDb()) {
//                var sideString = (string)data["GANG_SIDE"];
//                var gangId = getGangId(player);
//                var sideEntry = db.ganglocationprogress.FirstOrDefault(x => x.gangid == gangId);
//                var newInput = false;
//                if (sideEntry == null) {
//                    registerNewGang(player);
//                    newInput = true;
//                    sideEntry = new ganglocationprogress {
//                        gangid = gangId,
//                        northside = -1,
//                        eastside = -1,
//                        southside = -1,
//                        westside = -1,
//                        sandyshores = -1,
//                        paletobay = -1,
//                    };
//                }
//                if (sideString == "North") {
//                    sideEntry.northside = 1;
//                    player.sendBlockNotification(sideString, "");
//                }
//                if (sideString == "East") {
//                    sideEntry.eastside = 1;
//                    player.sendBlockNotification(sideString, "");
//                }
//                if (sideString == "South") {
//                    sideEntry.southside = 1;
//                    player.sendBlockNotification(sideString, "");
//                }
//                if (sideString == "West") {
//                    sideEntry.westside = 1;
//                    player.sendBlockNotification(sideString, "");
//                }
//                if (sideString == "Sandy") {
//                    sideEntry.sandyshores = 1;
//                    player.sendBlockNotification(sideString, "");
//                }
//                if (sideString == "Paleto") {
//                    sideEntry.paletobay = 1;
//                    player.sendBlockNotification(sideString, "");
//                }
//                if (newInput) {
//                    db.ganglocationprogress.Add(sideEntry);
//                } else {
//                    db.ganglocationprogress.Update(sideEntry);
//                }
//                db.SaveChanges();
//                return true;
//            }
//        }

//        private void registerNewGang(IPlayer player) {
//            using (var db = new ChoiceVDb()) {
//                var newGang = new gangprogress {
//                    gangid = getGangId(player),
//                    weedprogress = 0,
//                    cocaineprogress = 0,
//                    xpprogress = 0,
//                    ganglevel = 1,
//                };
//                db.gangprogress.Add(newGang);
//                var phonenumber = new gangphonenumbers {
//                    gangId = getGangId(player),
//                    phonenumber1 = 0,
//                    phonenumber2 = 0,
//                    phonenumber3 = 0,
//                    phonenumber4 = 0,
//                    phonenumber5 = 0,
//                };
//                db.gangphonenumbers.Add(phonenumber);
//                db.SaveChanges();
//            }
//        }

//        private bool onPepeTalk(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> data) {
//            var menu = new Menu("Pepe", "Sprich mit Pepe");
//            var companies = CompanyController.getCompanies(player);
//            var policeCheck = false;
//            var gangCheck = false;
//            var gangId = 0; //TODO Replace with 0
//            foreach (var company in companies) {
//                if (company.Type == Model.Company.CompanyType.Police) {
//                    policeCheck = true;
//                }
//                if (company.Type == Model.Company.CompanyType.Gang) {
//                    gangCheck = true;
//                    gangId = company.Id;
//                }
//            }
//            if (policeCheck) {
//                menu.addMenuItem(new StaticMenuItem("Yo alles cool", "Ich hab dir nix zu sagen", "", MenuItemStyle.red));
//            }
//            if (!(policeCheck || gangCheck)) {
//                menu.addMenuItem(new StaticMenuItem("Verpiss dich", "Ich hab dir nix zu sagen", "", MenuItemStyle.red));
//            }
//            if (gangCheck) {
//                using (var db = new ChoiceVDb()) {
//                    var gangIdCheck = db.ganglocationprogress.FirstOrDefault(x => x.gangid == gangId);
//                    if (gangIdCheck != null) {
//                        menu.addMenuItem(new MenuMenuItem("Was hast du für mich?", getPepeShopMenu(player)));
//                        menu.addMenuItem(new ClickMenuItem("Wo lieferst du hin?", "Frage nach dem Lieferstandort", "", "PEPE_ASK_LOCATION"));
//                        menu.addMenuItem(new MenuMenuItem("Telefonnummer geben", getPhoneMenu(player)));
//                        //Level Check
//                    } else {
//                        var sideMenu = new Menu("Wohngebiet", "Wo kommst du her?");
//                        menu.addMenuItem(new MenuMenuItem("Nach Dealern fragen", sideMenu));
//                        sideMenu.addMenuItem(new ClickMenuItem("Northside", "Sagt dem Dealer du wohnst in der Northside", "", "SET_SIDE").needsConfirmation("Bist du dir sicher?", "Gibt den Einflussbereich deiner Gruppe an").withData(new Dictionary<string, dynamic> { { "GANG_SIDE", "North" } }));
//                        sideMenu.addMenuItem(new ClickMenuItem("Eastside", "Sagt dem Dealer du wohnst in der Eastside", "", "SET_SIDE").needsConfirmation("Bist du dir sicher?", "Gibt den Einflussbereich deiner Gruppe an").withData(new Dictionary<string, dynamic> { { "GANG_SIDE", "East" } }));
//                        sideMenu.addMenuItem(new ClickMenuItem("Southside", "Sagt dem Dealer du wohnst in der Southside", "", "SET_SIDE").needsConfirmation("Bist du dir sicher?", "Gibt den Einflussbereich deiner Gruppe an").withData(new Dictionary<string, dynamic> { { "GANG_SIDE", "South" } }));
//                        sideMenu.addMenuItem(new ClickMenuItem("Westside", "Sagt dem Dealer du wohnst in der Westside", "", "SET_SIDE").needsConfirmation("Bist du dir sicher?", "Gibt den Einflussbereich deiner Gruppe an").withData(new Dictionary<string, dynamic> { { "GANG_SIDE", "West" } }));
//                        sideMenu.addMenuItem(new ClickMenuItem("Sandy Shores", "Sagt dem Dealer du wohnst in Sandy Shores", "", "SET_SIDE").needsConfirmation("Bist du dir sicher?", "Gibt den Einflussbereich deiner Gruppe an").withData(new Dictionary<string, dynamic> { { "GANG_SIDE", "Sandy" } }));
//                        sideMenu.addMenuItem(new ClickMenuItem("Paleto Bay", "Sagt dem Dealer du wohnst in der Paleto Bay", "", "SET_SIDE").needsConfirmation("Bist du dir sicher?", "Gibt den Einflussbereich deiner Gruppe an").withData(new Dictionary<string, dynamic> { { "GANG_SIDE", "Paleto" } }));
//                    }
//                }
//            }
//            player.showMenu(menu);
//            return true;
//        }
//        #endregion

//        #region DealerMethods
//        private bool onDealerItemTake(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            using (var db = new ChoiceVDb()) {
//                var order = (gangorders)data["Order"];
//                var dealerId = (int)data["DealerId"];
//                var playerinv = player.getInventory();
//                var amount = 1;
//                InputMenuItem.InputMenuItemEvent inputItem = menuItemCefEvent as InputMenuItem.InputMenuItemEvent;
//                if (inputItem != null) {
//                    var tryparse = int.TryParse(inputItem.input, out var newAmount);
//                    if (!(tryparse)) {
//                        player.sendNotification(NotifactionTypes.Warning, "Der Input muss eine Zahl sein", "falscher Input");
//                        return true;
//                    }
//                    if (newAmount <= 0) {
//                        player.sendNotification(NotifactionTypes.Warning, "Der Input muss größer 0 sein", "falscher Input");
//                        return true;
//                    } else {
//                        amount = newAmount;
//                    }
//                }
//                if (amount <= 0) { amount = 1; }
//                order.amount -= amount;
//                var configItem = InventoryController.AllConfigItems[order.itemid];
//                var counter = 0;
//                var item = (Item)null;

//                if(configItem.codeItem == "PlaceableObjectItem" && configItem.additionalInfo.Contains("Marihuana")) {
//                    item = new PlaceableObjectItem(configItem);
//                } else {
//                    item = InventoryController.createGenericStackableItem(configItem, 1, -1);
//                }
//                var dealerInv = getDealerInventory(dealerId);
//                while (counter < amount) {
//                    var itemCheck = dealerInv.addItem(item, true);
//                    counter++;
//                }
//                InventoryController.showMoveInventory(player, playerinv, dealerInv);
//                if (order.amount <= 0) {
//                    db.gangorders.Remove(order);
//                } else {
//                    db.gangorders.Update(order);
//                }
//                db.SaveChanges();
//                return true;
//            }
//        }
//        private bool onDealerSellDrugs(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var dealerId = data["DealerId"];
//            var gangId = getGangId(player);
//            var inventory = (Inventory)getDealerInventory(dealerId);
//            var weedCounter = 0;
//            var cocaineCounter = 0;
//            var allItems = inventory.getAllItems();
//            foreach (var item in allItems.ToArray()) {
//                if (item is PlasticBag) {
//                    var bagItem = (Item)item.Data["Item"];
//                    if (bagItem is Weed) {
//                        weedCounter += bagItem.StackAmount ?? 0;
//                    }
//                    if (bagItem is Cocaine) {
//                        cocaineCounter += bagItem.StackAmount ?? 0;
//                    }
//                    inventory.removeItem(item);
//                }
//            }
//            var finalMoney = (weedCounter * 10) + (cocaineCounter * 20);
//            player.sendNotification(NotifactionTypes.Success, $"Du hast jetzt bei mir {finalMoney}$ offen", $"{finalMoney}$");
//            refreshGangDealerBalance(gangId, dealerId, finalMoney);
//            return true;
//        }

//        private bool onDealerShowInventory(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            player.sendNotification(NotifactionTypes.Info, "Ich nehme alles nur in Plastikbeuteln!", "Nur Plastikbeutel");
//            var dealerId = data["DealerId"];
//            var inventory = getDealerInventory(dealerId);
//            InventoryController.showMoveInventory(player, player.getInventory(), inventory);
//            return true;
//        }

//        private bool onDealerTakeover(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            player.sendNotification(NotifactionTypes.Info, "Geb mir mal ein paar Minuten bevor ich euren Shit kaufe, ich muss checken ob meine Geschäftspartner noch kommen", "");
//            var dealerId = data["DealerId"];
//            var takeover = new DealerTakeover {
//                dealerId = dealerId,
//                dealerPos = getDealerPosition(dealerId),
//                player = player,
//                dealerTakeover = true,
//                dealerThreat = false,
//                finishTime = DateTime.Now.AddMinutes(2), //TODO: 25
//            };
//            var flag = (int)getDealerFlag(dealerId);
//            if (flag >= 0) {
//                var numberEntry = gangNumberList.FirstOrDefault(x => x.gangId == flag);
//                if (numberEntry != null) {
//                    PhoneController.sendSMSToNumber(getDealerNumber(dealerId), numberEntry.phoneNumber1, "Jo bei unserem  Kontakt will jemand anderes gerade Geschäfte machen. Checkt das mal aus.");
//                    PhoneController.sendSMSToNumber(getDealerNumber(dealerId), numberEntry.phoneNumber2, "Jo bei unserem  Kontakt will jemand anderes gerade Geschäfte machen. Checkt das mal aus.");
//                    PhoneController.sendSMSToNumber(getDealerNumber(dealerId), numberEntry.phoneNumber3, "Jo bei unserem  Kontakt will jemand anderes gerade Geschäfte machen. Checkt das mal aus.");
//                    PhoneController.sendSMSToNumber(getDealerNumber(dealerId), numberEntry.phoneNumber4, "Jo bei unserem  Kontakt will jemand anderes gerade Geschäfte machen. Checkt das mal aus.");
//                    PhoneController.sendSMSToNumber(getDealerNumber(dealerId), numberEntry.phoneNumber5, "Jo bei unserem  Kontakt will jemand anderes gerade Geschäfte machen. Checkt das mal aus.");
//                }
//            }
//            takeoverList.Add(takeover);
//            return true;

//        }

//        private bool onDealerThreat(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            if (player.CurrentWeapon == 2725352035) {
//                player.sendNotification(NotifactionTypes.Info, "Mit was willst du mich denn bedrohen?", "Keine Waffe");
//                return true;
//            }
//            player.sendNotification(NotifactionTypes.Info, "Jo Shit! Entspann dich Homie. Geb mir ein paar Minuten… du bekommst was du willst!", "");
//            var dealerId = data["DealerId"];
//            var threat = new DealerTakeover {
//                dealerId = dealerId,
//                dealerPos = getDealerPosition(dealerId),
//                player = player,
//                dealerTakeover = false,
//                dealerThreat = true,
//                finishTime = DateTime.Now.AddMinutes(1), //15
//            };
//            var flag = getDealerFlag(dealerId);
//            if (flag >= 0) {
//                var numberEntry = gangNumberList.FirstOrDefault(x => x.gangId == flag);
//                if (numberEntry != null) {
//                    PhoneController.sendSMSToNumber(getDealerNumber(dealerId), numberEntry.phoneNumber1, "Jo Homie! Ey mir wurde bescheid gegeben das da jemand Stress bei unserem gemeinsamen Freund, kümmer dich drum bevor es zuspät ist!");
//                    PhoneController.sendSMSToNumber(getDealerNumber(dealerId), numberEntry.phoneNumber2, "Jo Homie! Ey mir wurde bescheid gegeben das da jemand Stress bei unserem gemeinsamen Freund, kümmer dich drum bevor es zuspät ist!");
//                    PhoneController.sendSMSToNumber(getDealerNumber(dealerId), numberEntry.phoneNumber3, "Jo Homie! Ey mir wurde bescheid gegeben das da jemand Stress bei unserem gemeinsamen Freund, kümmer dich drum bevor es zuspät ist!");
//                    PhoneController.sendSMSToNumber(getDealerNumber(dealerId), numberEntry.phoneNumber4, "Jo Homie! Ey mir wurde bescheid gegeben das da jemand Stress bei unserem gemeinsamen Freund, kümmer dich drum bevor es zuspät ist!");
//                    PhoneController.sendSMSToNumber(getDealerNumber(dealerId), numberEntry.phoneNumber5, "Jo Homie! Ey mir wurde bescheid gegeben das da jemand Stress bei unserem gemeinsamen Freund, kümmer dich drum bevor es zuspät ist!");
//                }
//            }
//            takeoverList.Add(threat);
//            return true;
//        }

//        private bool onDealerTalk(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> data) {
//            var currentDealerId = (int)int.Parse(data["AdditionalInfo"]);
//            var companies = CompanyController.getCompanies(player);
//            var policeCheck = false;
//            var gangCheck = false;
//            var illegalContent = getDealerChecked(currentDealerId);
//            var gangId = 0; //TODO Replace with 0
//            var dictionary = new Dictionary<string, dynamic> { { "DealerId", currentDealerId } };
//            foreach (var company in companies) {
//                if (company.Type == Model.Company.CompanyType.Police) {
//                    policeCheck = true;
//                }
//                if (company.Type == Model.Company.CompanyType.Gang) {
//                    gangCheck = true;
//                    gangId = company.Id;
//                }
//            }
//            var menu = new Menu($"{getDealerSide(currentDealerId)} Dealer", "Der Dealer deines vertrauens");
//            if (!(policeCheck || gangCheck)) {
//                menu.addMenuItem(new StaticMenuItem("Verzieh dich", "Du kannst hier nichts machen", "", MenuItemStyle.red));
//            }
//            if (policeCheck) {
//                menu.addMenuItem(new ClickMenuItem("Durchsuchen", "Durchsucht das Inventar", "", "DEALER_POLICE_CHECK").withData(dictionary));
//                if (illegalContent) {
//                    menu.addMenuItem(new ClickMenuItem("Befragen", "Fragt den Dealer nach seinen Verkäufern", "", "DEALER_POLICE_ASK").withData(dictionary));
//                    menu.addMenuItem(new ClickMenuItem("Festnehmen", "Nimmt den Dealer fest", "", "DEALER_POLICE_ARREST", MenuItemStyle.red).withData(dictionary));
//                }
//            }
//            if (gangCheck) {
//                if (getDealerFlag(currentDealerId) == gangId) {
//                    menu.addMenuItem(new StaticMenuItem("Heute nur Gras und Koks", "Du kannst dem Dealer heute nur Gras und Koks geben", "", MenuItemStyle.green)); //CHANGE IF YOU WANT TO SELL NEW DRUGS
//                    menu.addMenuItem(new StaticMenuItem($"Offenes Geld: {getGangDealerBalance(gangId, currentDealerId)}", "Mit dem offenen Geld kannst du was kaufen", ""));
//                    menu.addMenuItem(new ClickMenuItem($"Ware übergeben", "Drogen an den Dealer verkaufen", "", "DEALER_SHOW_INVENTORY").withData(dictionary));
//                    if (getDealerInventory(currentDealerId).getAllItems().Count >= 1) {
//                        menu.addMenuItem(new ClickMenuItem("Ware verkaufen", "Verkauft die übergebene Ware", "", "DEALER_SELL_DRUGS").withData(dictionary));
//                    }
//                    var shopMenu = getDealerShop(currentDealerId, gangId);
//                    menu.addMenuItem(new MenuMenuItem("Dealer Angebot", shopMenu)); //MENUMENUITEM
//                    var orderMenu = getOrderMenu(gangId, currentDealerId);
//                    menu.addMenuItem(new MenuMenuItem("Gekaufte Ware", orderMenu));
//                } else if (!(getCaptureStatus(currentDealerId))) {
//                    if (getGangDealerBalance(gangId, currentDealerId) != -1) {
//                        menu.addMenuItem(new ClickMenuItem("Dealer einnehmen", "Ermöglicht es dir bei dem Dealer zu verkaufen", "", "DEALER_TAKEOVER").withData(dictionary));
//                    }
//                    menu.addMenuItem(new ClickMenuItem("Dealer bedrohen", "Bei Erfolg verschwindet der Dealer für diesen Tag", "", "DEALER_THREAT", MenuItemStyle.red).withData(dictionary));
//                }


//            }
//            player.showMenu(menu);
//            return true;
//        }

//        public static void spawnDealer() {
//            using (var db = new ChoiceVDb()) {
//                setDealerInventorySize();
//                var dealerCounter = 1;
//                while (dealerCounter <= 6) {
//                    var counter = 0;
//                    var usedIds = new List<int>();
//                    foreach (var position in positionList) {
//                        if (position.dealerId == dealerCounter) {
//                            usedIds.Add(position.id);
//                            counter++;
//                        }
//                    }
//                    var random = new Random();
//                    var listId = random.Next(0, counter);
//                    var dealerId = usedIds.ElementAt(listId);
//                    var dealer = positionList.FirstOrDefault(x => x.id == dealerId);
//                    var dealerPed = getDealerModel(dealerCounter);
//                    var ped = PedController.createPed(dealerPed, dealer.position, dealer.heading, "DEALER_TALK", dealerCounter.ToString());
//                    pedList.Add(ped);
//                    var allPlayer = ChoiceVAPI.GetAllPlayers();
//                    foreach (var player in allPlayer) {
//                        player.sendNotification(NotifactionTypes.Warning, $"Dealer gespawned bei ID: {dealerId} mit {dealerCounter}", "");
//                    }
//                    foreach (var gang in db.ganglocationprogress) {
//                        if (dealerCounter == 1 && gang.westside >= 0) {
//                            sendGangDealerSms(dealerCounter, dealer, gang);
//                        }
//                        if (dealerCounter == 2 && gang.southside >= 0) {
//                            sendGangDealerSms(dealerCounter, dealer, gang);
//                        }
//                        if (dealerCounter == 3 && gang.eastside >= 0) {
//                            sendGangDealerSms(dealerCounter, dealer, gang);
//                        }
//                        if (dealerCounter == 4 && gang.northside >= 0) {
//                            sendGangDealerSms(dealerCounter, dealer, gang);
//                        }
//                        if (dealerCounter == 5 && gang.sandyshores >= 0) {
//                            sendGangDealerSms(dealerCounter, dealer, gang);
//                        }
//                        if (dealerCounter == 6 && gang.paletobay >= 0) {
//                            sendGangDealerSms(dealerCounter, dealer, gang);
//                        }
//                    } 
//                    var shopCounter = 0;
//                    var finalShopList = new List<DealerShop>();
//                    var tempShopList = shopList;
//                    while (shopCounter < 2) {
//                        var randomEntryNumber = random.Next(0, tempShopList.Count);
//                        var entry = tempShopList.ElementAt(randomEntryNumber);
//                        if (finalShopList.Contains(entry)) {
//                            while (finalShopList.Contains(entry)) {
//                                var randomNumber = random.Next(0, tempShopList.Count);
//                                entry = tempShopList.ElementAt(randomNumber);
//                            }
//                        }
//                        finalShopList.Add(entry);
//                        shopCounter++;
//                    }
//                    var dealerShop = new DealerShopList {
//                        dealerId = dealerCounter,
//                        list = finalShopList,
//                    };
//                    dealerShopList.Add(dealerShop);
//                    dealerCounter++;
//                }
//            }
//        }



//        #endregion

//        #region Helper

//        private void onDisconnect(IPlayer player, string reason) {
//            var takeoverCheck = takeoverList.FirstOrDefault(x => x.player == player);
//            if (takeoverCheck != null) {
//                takeoverList.Remove(takeoverCheck);
//            }
//        }
//        private bool onSetPhoneNumber(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            using (var db = new ChoiceVDb()) {
//                var numberId = data["PhoneNumberType"];
//                var gangId = getGangId(player);
//                InputMenuItem.InputMenuItemEvent inputItem = menuItemCefEvent as InputMenuItem.InputMenuItemEvent;
//                var input = inputItem.input;
//                var tryParse = long.TryParse(input, out var number);
//                if (!(tryParse)) {
//                    player.sendNotification(NotifactionTypes.Warning, "Der Input muss eine Zahl sein!", "");
//                    return true;
//                } else {
//                    var entry = db.gangphonenumbers.FirstOrDefault(x => x.gangId == gangId);
//                    if (entry != null) {
//                        if (numberId == 1) { entry.phonenumber1 = number; }
//                        if (numberId == 2) { entry.phonenumber2 = number; }
//                        if (numberId == 3) { entry.phonenumber3 = number; }
//                        if (numberId == 4) { entry.phonenumber4 = number; }
//                        if (numberId == 5) { entry.phonenumber5 = number; }
//                    }
//                    db.gangphonenumbers.Update(entry);
//                    db.SaveChanges();
//                    refreshDealerDb();
//                }

//                return true;
//            }
//        }
//        public static Menu getPhoneMenu(IPlayer player) {
//            var menu = new Menu("Nummer dalassen", "");
//            var phoneEntry = gangNumberList.FirstOrDefault(x => x.gangId == getGangId(player));
//            if (phoneEntry != null) {
//                menu.addMenuItem(new InputMenuItem("Erste Nummer", "", $"{phoneEntry.phoneNumber1}", "Dealer_setPhoneNumber").withData(new Dictionary<string, dynamic> { { "PhoneNumberType", 1 } }));
//                menu.addMenuItem(new InputMenuItem("Zweite Nummer", "", $"{phoneEntry.phoneNumber2}", "Dealer_setPhoneNumber").withData(new Dictionary<string, dynamic> { { "PhoneNumberType", 2 } }));
//                menu.addMenuItem(new InputMenuItem("Dritte Nummer", "", $"{phoneEntry.phoneNumber3}", "Dealer_setPhoneNumber").withData(new Dictionary<string, dynamic> { { "PhoneNumberType", 3 } }));
//                menu.addMenuItem(new InputMenuItem("Vierte Nummer", "", $"{phoneEntry.phoneNumber4}", "Dealer_setPhoneNumber").withData(new Dictionary<string, dynamic> { { "PhoneNumberType", 4 } }));
//                menu.addMenuItem(new InputMenuItem("Fünfte Nummer", "", $"{phoneEntry.phoneNumber5}", "Dealer_setPhoneNumber").withData(new Dictionary<string, dynamic> { { "PhoneNumberType", 5 } }));
//            }
//            return menu;
//        }
//        public static void onStartUp() {
//            using (var db = new ChoiceVDb()) {
//                foreach (var level in db.configdealerlevel) {
//                    var listEntry = new DealerLevel {
//                        level = level.level,
//                        gangNeededXp = level.gangneededxp,
//                        dealerNeededXp = level.dealerneededxp,
//                    };
//                    levelList.Add(listEntry);
//                }
//                foreach (var level in db.configdealershop) {
//                    var listEntry = new DealerShop {
//                        neededGangLevel = level.neededganglevel,
//                        configItemId = level.configitemid,
//                        amount = level.amount,
//                        neededDealerLevel = level.neededdealerlevel,
//                        sellOnDealer = level.dealersell,
//                        sellOnPepe = level.pepesell,
//                        price = level.price,
//                        deliveryTime = level.deliverytime,
//                    };
//                    shopList.Add(listEntry);
//                }
//                foreach (var level in db.configdealerpositions) {
//                    var listEntry = new DealerPositions {
//                        id = level.id,
//                        dealerId = level.dealerid ?? 0,
//                        position = (Position)level.position.FromJson(),
//                        heading = level.heading,
//                        positionSms = level.positioninfo,
//                    };
//                    positionList.Add(listEntry);
//                }
//                foreach (var spawnEntry in db.dealerspawndate) {
//                    var listEntry = new DealerSpawns {
//                        id = spawnEntry.id,
//                        spawnTime = spawnEntry.spawndate,
//                        despawnTime = spawnEntry.despawndate,
//                    };
//                    spawnList.Add(listEntry);
//                }
//                foreach (var configDealer in db.configdealer) {
//                    var inventory = (Inventory)null;
//                    if (configDealer.inventoryid == 0) {
//                        inventory = InventoryController.createInventory(-1, 15, InventoryTypes.Dealer);
//                        inventory.BlockStatements.Add(new InventoryAddBlockStatement(inventory, new Predicate<Item>(i => !(i is PlasticBag && i.Data["Item"] is Cocaine || i.Data["Item"] is Weed))));
//                        configDealer.inventoryid = inventory.Id;
//                        db.configdealer.Update(configDealer);
//                    } else {
//                        inventory = InventoryController.loadInventory(configDealer.inventoryid);
//                    }
//                    var inventoryEntry = new DealerInventory {
//                        id = configDealer.id,
//                        dealerInventory = inventory,
//                        illegalContentCheck = false,
//                    };
//                    inventoryList.Add(inventoryEntry);
//                    var dealerNumber = new DealerNumber {
//                        dealerId = configDealer.id,
//                        phoneNumber = configDealer.phonenumber,
//                    };
//                    dealerNumberList.Add(dealerNumber);
//                }

//                pepeShapeInventory = InventoryController.createInventory(-1, 999999, InventoryTypes.Dealer);
//                refreshDealerDb();
//                CollisionShape.Create(new Position(141, -3100, 7), 5, 5, 0, true, false, true, "PEPE_SHAPE_INTERACT");
//                InvokeController.AddTimedInvoke("Dealer-Spawn-Invoke", (ivk) => {
//                    spawnCheck();
//                    orderCheck();
//                }, TimeSpan.FromSeconds(15), true);
//            }
//        }
//        public static void sendGangDealerSms(int counter, DealerPositions dealer, ganglocationprogress gang) {
//            using (var db = new ChoiceVDb()) {
//                var gangNumbers = gangNumberList.FirstOrDefault(x => x.gangId == gang.gangid);
//                if (gangNumbers != null) {
//                    if (gangNumbers.phoneNumber1 < 999999) { return; }
//                    PhoneController.sendSMSToNumber(555555067111, gangNumbers.phoneNumber1, dealer.positionSms);
//                    if (gangNumbers.phoneNumber2 < 999999) { return; }
//                    PhoneController.sendSMSToNumber(555555067111, gangNumbers.phoneNumber2, dealer.positionSms);
//                    if (gangNumbers.phoneNumber3 < 999999) { return; }
//                    PhoneController.sendSMSToNumber(555555067111, gangNumbers.phoneNumber3, dealer.positionSms);
//                    if (gangNumbers.phoneNumber4 < 999999) { return; }
//                    PhoneController.sendSMSToNumber(555555067111, gangNumbers.phoneNumber4, dealer.positionSms);
//                    if (gangNumbers.phoneNumber5 < 999999) { return; }
//                    PhoneController.sendSMSToNumber(555555067111, gangNumbers.phoneNumber5, dealer.positionSms);
//                }
//            }
//        }

//        public static void sendGangOrderSms(int pepe, int dealerId, int gangId) {
//            using (var db = new ChoiceVDb()) {
//                var gangNumbers = gangNumberList.FirstOrDefault(x => x.gangId == gangId);
//                if (gangNumbers != null && pepe == 1) {
//                    if (gangNumbers.phoneNumber1 < 999999) { return; }
//                    PhoneController.sendSMSToNumber(555555067111, gangNumbers.phoneNumber1, "Jo Pepe hier, deine Bestellung ist am Hafen!");
//                    if (gangNumbers.phoneNumber2 < 999999) { return; }
//                    PhoneController.sendSMSToNumber(555555067111, gangNumbers.phoneNumber2, "Jo Pepe hier, deine Bestellung ist am Hafen!");
//                    if (gangNumbers.phoneNumber3 < 999999) { return; }
//                    PhoneController.sendSMSToNumber(555555067111, gangNumbers.phoneNumber3, "Jo Pepe hier, deine Bestellung ist am Hafen!");
//                    if (gangNumbers.phoneNumber4 < 999999) { return; }
//                    PhoneController.sendSMSToNumber(555555067111, gangNumbers.phoneNumber4, "Jo Pepe hier, deine Bestellung ist am Hafen!");
//                    if (gangNumbers.phoneNumber5 < 999999) { return; }
//                    PhoneController.sendSMSToNumber(555555067111, gangNumbers.phoneNumber5, "Jo Pepe hier, deine Bestellung ist am Hafen!");
//                }
//            }
//        }
//        public static void setDealerInventorySize() {
//            var counter = 1;
//            while (counter <= 6) {
//                var dealerLevel = getDealerLevel(counter);
//                var dealerInventory = getDealerInventory(counter);
//                dealerInventory.MaxWeight += dealerLevel * 0.5f;
//                counter++;
//            }
//        }
//        public static bool newSideCheck(IPlayer player) {
//            using (var db = new ChoiceVDb()) {
//                var progressLevel = db.gangprogress.FirstOrDefault(x => x.gangid == getGangId(player));
//                if (progressLevel != null) {
//                    if (1 == 1) {
//                        return true;
//                    } else {
//                        return true;
//                    }
//                } else {
//                    return false;
//                }
//            }
//        }

//        public static Position getDealerPosition(int dealerId) {
//            var dealerModel = getDealerModel(dealerId);
//            var dealerPed = pedList.FirstOrDefault(x => x.Model == dealerModel);
//            if (dealerPed != null) {
//                return dealerPed.Position;
//            } else {
//                return new Position(0, 0, 0);
//            }
//        }

//        public static void refreshDealerDb() {
//            using (var db = new ChoiceVDb()) {
//                foreach (var gang in db.gangprogress) { //GangLvlUp
//                    var levelCheck = levelList.FirstOrDefault(x => x.level == gang.ganglevel);
//                    if (levelCheck != null && gang.xpprogress >= levelCheck.gangNeededXp) {
//                        gang.ganglevel += 1;
//                        gang.xpprogress -= levelCheck.gangNeededXp;
//                        db.Update(gang);
//                    }
//                }
//                foreach (var dealer in db.configdealer) { //DealerLvlUp
//                    var levelCheck = levelList.FirstOrDefault(x => x.level == dealer.currentlevel);
//                    if (levelCheck != null && dealer.xpprogress >= levelCheck.dealerNeededXp) {
//                        dealer.currentlevel += 1;
//                        dealer.xpprogress -= levelCheck.dealerNeededXp;
//                        db.Update(dealer);
//                    }
//                }
//                gangNumberList.Clear();
//                foreach (var number in db.gangphonenumbers) {
//                    var gangNumber = new GangNumbers {
//                        gangId = number.gangId,
//                        phoneNumber1 = number.phonenumber1,
//                        phoneNumber2 = number.phonenumber2,
//                        phoneNumber3 = number.phonenumber3,
//                        phoneNumber4 = number.phonenumber4,
//                        phoneNumber5 = number.phonenumber5,
//                    };
//                    gangNumberList.Add(gangNumber);
//                }
//                db.SaveChanges();
//            }
//        }
//        public static int getGangId(IPlayer player) {
//            var companyList = CompanyController.getCompanies(player);
//            var gangIdCheck = companyList.FirstOrDefault(x => x.Type == Model.Company.CompanyType.Gang);
//            if (gangIdCheck != null) {
//                return gangIdCheck.Id;
//            } else {
//                return 0; //TODO CHANGE TO 0
//            }
//        }
//        public static int getGangLevel(int gangId) {
//            using (var db = new ChoiceVDb()) {
//                var gang = db.gangprogress.FirstOrDefault(x => x.gangid == gangId);
//                if (gang != null) {
//                    return gang.ganglevel;
//                } else {
//                    return -1;
//                }
//            }
//        }

//        public static int getDealerId(GangAreas gangarea) {
//            if (gangarea == GangAreas.Northside) { return 4; }
//            if (gangarea == GangAreas.Eastside) { return 3; }
//            if (gangarea == GangAreas.Southside) { return 2; }
//            if (gangarea == GangAreas.Westside) { return 1; }
//            if (gangarea == GangAreas.SandyShores) { return 5; }
//            if (gangarea == GangAreas.PaletoBay) { return 6; } else {
//                return -1;
//            }
//        }

//        public static string getDealerSide(int dealerId) {
//            if (dealerId == 1) { return "Westside"; }
//            if (dealerId == 2) { return "Southside"; }
//            if (dealerId == 3) { return "Eastside"; }
//            if (dealerId == 4) { return "Northside"; }
//            if (dealerId == 5) { return "Sandyshores"; }
//            if (dealerId == 6) { return "Paletobay"; }
//            return "";
//        }

//        public static int getDealerLevel(int dealerId) {
//            using (var db = new ChoiceVDb()) {
//                var dealer = db.configdealer.FirstOrDefault(x => x.id == dealerId);
//                if (dealer != null) {
//                    return dealer.currentlevel;
//                } else {
//                    return -1;
//                }
//            }
//        }

//        public static string getDealerModel(int dealerId) {
//            using (var db = new ChoiceVDb()) {
//                var ped = db.configdealer.FirstOrDefault(x => x.id == dealerId);
//                if (ped != null) {
//                    return ped.dealerped;
//                } else {
//                    return "Null";
//                }
//            }
//        }

//        public static int getDrugXp(DrugTypes drugType) {
//            if (drugType == DrugTypes.Cocaine) { return 3; } //3 Xp per Kilo Cocaine
//            if (drugType == DrugTypes.Weed) { return 1; } //1 Xp per Kilo Weed 
//            else { return 0; }
//        }

//        public static Menu getPepeShopMenu(IPlayer player) {
//            using (var db = new ChoiceVDb()) {
//                var gangLevel = getGangLevel(getGangId(player));
//                var shopMenu = new Menu("Pepes Schmuggelware", "Das beste vom Besten!");
//                foreach (var shopItem in shopList) {
//                    var configItem = InventoryController.AllConfigItems[shopItem.configItemId];
//                    var item = new Item(configItem);
//                    var orderAmount = shopItem.amount;
//                    var orderCheck = db.gangorders.FirstOrDefault(x => x.itemid == item.ConfigId && x.deliverdate.Date == DateTime.Today.AddDays(shopItem.deliveryTime));
//                    if (orderCheck != null) {
//                        orderAmount -= orderCheck.amount;
//                    }
//                    if (gangLevel >= shopItem.neededGangLevel && shopItem.sellOnPepe == 1) {
//                        if (orderAmount > 1) {
//                            //TODO: Fix InputItem Description
//                            shopMenu.addMenuItem(new InputMenuItem($"{item.Name}", $"Lieferzeit: {shopItem.deliveryTime} Tage", $"   Max: {orderAmount} | ${shopItem.price} /x", "DEALER_ORDER").withData(new Dictionary<string, dynamic> { { "Item", item }, { "ItemListEntry", shopItem }, { "Pepe", 1 }, { "MaxAmount", orderAmount } }));

//                        } else if (orderAmount > 0) {
//                            shopMenu.addMenuItem(new ClickMenuItem($"{item.Name}", $"Lieferzeit: {shopItem.deliveryTime} Tage", $"${shopItem.price} /x", "DEALER_ORDER").withData(new Dictionary<string, dynamic> { { "Item", item }, { "ItemListEntry", shopItem }, { "Pepe", 1 }, { "MaxAmount", orderAmount } }));
//                        }
//                    }
//                }
//                return shopMenu;
//            }
//        }

//        public static void spawnCheck() {
//            using (var db = new ChoiceVDb()) {
//                var latestSpawn = db.dealerspawndate.FirstOrDefault(x => x.spawndate.Date == DateTime.Today);
//                if (latestSpawn != null) {
//                    if (DateTime.Now > latestSpawn.spawndate && DateTime.Now < latestSpawn.despawndate) {
//                        if (!(dealerSpawned)) {
//                            spawnDealer();
//                            dealerSpawned = true;
//                        }
//                        //If not spawn and set spawned true
//                    }
//                    if (DateTime.Now > latestSpawn.spawndate && DateTime.Now > latestSpawn.despawndate) {
//                        if (dealerSpawned) {
//                            var counter = 1;
//                            foreach (var pedPos in pedList.ToArray()) {
//                                var ped = PedController.AllPeds.FirstOrDefault(x => x == pedPos);
//                                PedController.destroyPed(ped);
//                                counter++;
//                                pedList.Remove(pedPos);
//                            }
//                            dealerShopList.Clear();
//                            dealerSpawned = false;
//                            createNewSpawnDate();
//                        }
//                        //Check if spawned
//                        //If spawn despawn and set spawned false
//                    }
//                } else {
//                    createNewSpawnDate();
//                }
//            }
//        }

//        public static void orderCheck() {
//            using (var db = new ChoiceVDb()) {
//                foreach (var order in db.gangorders) {
//                    if (DateTime.Now > order.deliverdate && order.deliverdate.AddMinutes(16) > DateTime.Now) {
//                        sendGangOrderSms(order.pepe, order.dealerid, order.gangid);
//                    }
//                }
//            }
//        }

//        private static void takeoverCheck() {
//            if (takeoverList.Count >= 1) {
//                foreach (var takeover in takeoverList.ToArray()) {
//                    if (takeover.player.Position.Distance(takeover.dealerPos) > 5) {
//                        takeover.player.sendNotification(NotifactionTypes.Warning, "Du hast dich zu weit vom Dealer entfernt!", "Zu weit");
//                        takeoverList.Remove(takeover);
//                    }
//                    if (DateTime.Now >= takeover.finishTime) {
//                        if (takeover.dealerTakeover) {
//                            setDealerFlag(takeover.dealerId, getGangId(takeover.player));
//                            takeover.player.sendNotification(NotifactionTypes.Success, "Alles klar dann kauf ich den Scheiß jetzt von euch", "Neuer Dealer");
//                            takeoverList.Remove(takeover);
//                        }
//                        if (takeover.dealerThreat) {
//                            var dealerModel = getDealerModel(takeover.dealerId);
//                            var dealerPed = pedList.FirstOrDefault(x => x.Model == dealerModel);
//                            if (dealerPed != null) {
//                                var random = new Random();
//                                var randomInt = random.Next(1, 100);
//                                if (randomInt <= 20) {
//                                    var money = getGangDealerBalance(getDealerFlag(takeover.dealerId) ,takeover.dealerId);
//                                    if (money >= 1) {
//                                        var playerMoney = money * 0.05;
//                                        takeover.player.sendNotification(NotifactionTypes.Info, "Shit, das ist alles was ich am Start habe!", "Geld vom Dealer");
//                                        takeover.player.addCash(Convert.ToDecimal(playerMoney));
//                                    } else {
//                                        takeover.player.sendNotification(NotifactionTypes.Info, "Jo ich hab wirklich nichts!", "Dealer weg");
//                                    }
//                                }
//                                PedController.destroyPed(dealerPed);
//                                takeover.player.sendNotification(NotifactionTypes.Success, "Jo ich hab wirklich nichts!!", "Dealer weg");
//                                takeoverList.Remove(takeover);
//                                pedList.Remove(dealerPed);
//                            }
//                        }
//                    }
//                }
//            }
//        }

//        public static void createNewSpawnDate() {
//            using (var db = new ChoiceVDb()) {
//                var currentDay = DateTime.Today.AddHours(18);
//                var random = new Random();
//                var addedMinutes = random.Next(1, 270);
//                var spawnDate = currentDay.AddMinutes(addedMinutes);
//                var despawnDate = spawnDate.AddMinutes(90);
//                var spawnEntry = new dealerspawndate {
//                    spawndate = spawnDate,
//                    despawndate = despawnDate,
//                };
//                db.dealerspawndate.Add(spawnEntry);
//                db.SaveChanges();
//            }
//            refreshDealerDb();
//        }

//        public static Inventory getDealerInventory(int dealerId) {
//            var inventoryCheck = inventoryList.FirstOrDefault(x => x.id == dealerId);
//            if (inventoryCheck != null) {
//                return inventoryCheck.dealerInventory;
//            } else {
//                return null;
//            }
//        }

//        public static bool getDealerChecked(int dealerId) {
//            var inventoryCheck = inventoryList.FirstOrDefault(x => x.id == dealerId);
//            if (inventoryCheck != null) {
//                return inventoryCheck.illegalContentCheck;
//            } else {
//                return false;
//            }

//        }

//        public static int getGangDealerBalance(int gangId, int dealerId) {
//            using (var db = new ChoiceVDb()) {
//                var returnInt = 0;
//                var gangCheck = db.ganglocationprogress.FirstOrDefault(x => x.gangid == gangId);
//                if (gangCheck != null) {
//                    if (dealerId == 1) { returnInt = gangCheck.westside; }
//                    if (dealerId == 2) { returnInt = gangCheck.southside; }
//                    if (dealerId == 3) { returnInt = gangCheck.eastside; }
//                    if (dealerId == 4) { returnInt = gangCheck.northside; }
//                    if (dealerId == 5) { returnInt = gangCheck.sandyshores; }
//                    if (dealerId == 6) { returnInt = gangCheck.paletobay; }
//                }
//                return returnInt;
//            }
//        }

//        public static void removeGangDealerBalance(int gangId, int dealerId, int amount) {
//            using (var db = new ChoiceVDb()) {
//                var gangCheck = db.ganglocationprogress.FirstOrDefault(x => x.gangid == gangId);
//                if (gangCheck != null) {
//                    if (dealerId == 1) { gangCheck.westside -= amount; }
//                    if (dealerId == 2) { gangCheck.southside -= amount; }
//                    if (dealerId == 3) { gangCheck.eastside -= amount; }
//                    if (dealerId == 4) { gangCheck.northside -= amount; }
//                    if (dealerId == 5) { gangCheck.sandyshores -= amount; }
//                    if (dealerId == 6) { gangCheck.paletobay -= amount; }
//                }
//            }
//        }

//        public static int getDealerFlag(int dealerId) {
//            using (var db = new ChoiceVDb()) {
//                var dealerCheck = db.configdealer.FirstOrDefault(x => x.id == dealerId);
//                if (dealerCheck != null) {
//                    return dealerCheck.gangflag;
//                } else {
//                    return -1;
//                }
//            }
//        }

//        public static void setDealerFlag(int dealerId, int gangId) {
//            using (var db = new ChoiceVDb()) {
//                var dealerCheck = db.configdealer.FirstOrDefault(x => x.id == dealerId);
//                if (dealerCheck != null) {
//                    dealerCheck.gangflag = gangId;
//                    db.configdealer.Update(dealerCheck);
//                    db.SaveChanges();
//                }
//            }
//        }

//        public static Menu getDealerShop(int dealerId, int gangId) {
//            using (var db = new ChoiceVDb()) {
//                var shopMenu = new Menu("Dealerware", "Alles 100% echt!");
//                var gangLevel = getGangLevel(gangId);
//                var dealerLevel = getDealerLevel(dealerId);
//                var counter = 0;
//                var dealerListCheck = dealerShopList.FirstOrDefault(x => x.dealerId == dealerId);
//                if (dealerListCheck != null) {
//                    var shop = dealerListCheck.list;
//                    foreach (var entry in shop) {
//                        var configItem = InventoryController.AllConfigItems[entry.configItemId];
//                        var item = new Item(configItem);
//                        var orderAmount = entry.amount;
//                        var orderCheck = db.gangorders.FirstOrDefault(x => x.itemid == item.ConfigId && x.deliverdate.Date == DateTime.Today.AddDays(entry.deliveryTime));
//                        if (orderCheck != null) {
//                            orderAmount -= orderCheck.amount;
//                        }
//                        if (gangLevel >= entry.neededGangLevel && dealerLevel >= entry.neededDealerLevel && entry.sellOnDealer == 1) {
//                            if (orderAmount > 1) {
//                                //TODO: Fix InputItem Description
//                                shopMenu.addMenuItem(new InputMenuItem($"{item.Name}", $"Lieferzeit: {entry.deliveryTime} Tage", $"   Max: {orderAmount} | ${entry.price} /x", "DEALER_ORDER").withData(new Dictionary<string, dynamic> { { "Item", item }, { "ItemListEntry", entry }, { "Pepe", 0 }, { "DealerId", dealerId }, { "MaxAmount", orderAmount } }));
//                                counter++;
//                            } else if (orderAmount > 0) {
//                                shopMenu.addMenuItem(new ClickMenuItem($"{item.Name}", $"Lieferzeit: {entry.deliveryTime} Tage", $"${entry.price} /x", "DEALER_ORDER").withData(new Dictionary<string, dynamic> { { "Item", item }, { "ItemListEntry", entry }, { "Pepe", 0 }, { "DealerId", dealerId }, { "MaxAmount", orderAmount } }));
//                                counter++;
//                            }
//                        }
//                    }
//                    if (counter == 0) {
//                        shopMenu.addMenuItem(new StaticMenuItem("Heute habe ich nichts für euch", "", ""));
//                    }
//                }
//                return shopMenu;
//            }
//        }

//        public static Menu getOrderMenu(int gangId, int dealerId) {
//            using (var db = new ChoiceVDb()) {
//                var orderMenu = new Menu("Gekaufte Ware", "Die Ware die ihr bestellt habt");
//                var counter = 0;
//                foreach (var order in db.gangorders) {
//                    if (order.deliverdate < DateTime.Now && order.gangid == gangId) {
//                        counter++;
//                        var configItem = InventoryController.AllConfigItems[order.itemid];
//                        if (order.amount == 1) {
//                            orderMenu.addMenuItem(new ClickMenuItem($"{configItem.name}", "", "Anzahl: 1", "DEALER_ITEM_TAKE").withData(new Dictionary<string, dynamic> { { "Order", order }, { "DealerId", dealerId } }));
//                        } else {
//                            orderMenu.addMenuItem(new InputMenuItem($"{configItem.name}", "", $"Anzahl: {order.amount}", "DEALER_ITEM_TAKE").withData(new Dictionary<string, dynamic> { { "Order", order }, { "DealerId", dealerId } }));
//                        }
//                    }
//                } 
//                if (counter == 0) {
//                    orderMenu.addMenuItem(new StaticMenuItem("Heute habe ich nichts für dich", "Der Dealer hat nichts für dich", ""));
//                }
//                return orderMenu;
//            }
//        }

//        public static List<IPlayer> getOnlineGangMember(int gangId) {
//            var returnList = new List<IPlayer>();
//            var playerList = ChoiceVAPI.GetAllPlayers();
//            foreach (var player in playerList) {
//                var companyList = CompanyController.getCompanies(player);
//                var company = companyList.FirstOrDefault(x => x.Id == gangId);
//                if (company != null) {
//                    playerList.Add(player);
//                }
//            }
//            return returnList;
//        }

//        public static void refreshGangDealerBalance(int gangId, int dealerId, int money) {
//            using (var db = new ChoiceVDb()) {
//                var gangCheck = db.ganglocationprogress.FirstOrDefault(x => x.gangid == gangId);
//                if (gangCheck != null) {
//                    if (dealerId == 1) { gangCheck.westside += money; }
//                    if (dealerId == 2) { gangCheck.southside += money; }
//                    if (dealerId == 3) { gangCheck.eastside += money; }
//                    if (dealerId == 4) { gangCheck.northside += money; }
//                    if (dealerId == 5) { gangCheck.sandyshores += money; }
//                    if (dealerId == 6) { gangCheck.paletobay += money; }
//                    db.Update(gangCheck);
//                    db.SaveChanges();
//                }
//            }
//        }

//        public static bool getCaptureStatus(int dealerId) {
//            var captureCheck = takeoverList.FirstOrDefault(x => x.dealerId == dealerId);
//            if (captureCheck != null) {
//                return true;
//            } else {
//                return false;
//            }
//        }

//        public static long getDealerNumber(int dealerId) {
//            var dealer = dealerNumberList.FirstOrDefault(x => x.dealerId == dealerId);
//            if (dealer != null) {
//                return dealer.phoneNumber;
//            } else {
//                return 0;
//            }
//        }
//        #endregion

//    }

//    public class DealerLevel {
//        public int level { get; set; }
//        public int gangNeededXp { get; set; }
//        public int dealerNeededXp { get; set; }
//    }

//    public class DealerShop {
//        public int neededGangLevel { get; set; }
//        public int configItemId { get; set; }
//        public int amount { get; set; }
//        public int neededDealerLevel { get; set; }
//        public int sellOnDealer { get; set; }
//        public int sellOnPepe { get; set; }
//        public float price { get; set; }
//        public int deliveryTime { get; set; }
//    }

//    public class DealerShopList {
//        public int dealerId { get; set; }
//        public List<DealerShop> list { get; set; }
//    }

//    public class DealerPositions {
//        public int id { get; set; }
//        public int dealerId { get; set; }
//        public Position position { get; set; }
//        public int heading { get; set; }
//        public string positionSms { get; set; }
//    }

//    public class DealerSpawns {
//        public int id { get; set; }
//        public DateTime spawnTime { get; set; }
//        public DateTime despawnTime { get; set; }

//    }

//    public class DealerInventory {
//        public int id { get; set; }
//        public Inventory dealerInventory { get; set; }
//        public bool illegalContentCheck { get; set; }

//    }

//    public class DealerTakeover {
//        public int dealerId { get; set; }
//        public Position dealerPos { get; set; }
//        public IPlayer player { get; set; }
//        public bool dealerTakeover { get; set; }
//        public bool dealerThreat { get; set; }
//        public DateTime finishTime { get; set; }

//    }

//    public class DealerNumber {
//        public int dealerId { get; set; }
//        public long phoneNumber { get; set; }
//    }

//    public class GangNumbers {
//        public int gangId { get; set; }
//        public long phoneNumber1 { get; set; }
//        public long phoneNumber2 { get; set; }
//        public long phoneNumber3 { get; set; }
//        public long phoneNumber4 { get; set; }
//        public long phoneNumber5 { get; set; }

//    }


//}
