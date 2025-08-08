//using AltV.Net.Data;
//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Admin.Tools;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.InventorySystem;
//using ChoiceVServer.Model;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Model.FsDatabase;
//using ChoiceVServer.Model.InventorySystem;
//using ChoiceVServer.Model.Menu;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static ChoiceVServer.Model.Menu.InputMenuItem;
//using static ChoiceVServer.Model.Menu.ListMenuItem;
//using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;
//using static ChoiceVServer.Controller.Company.Functionalities.Old.PrisonController;

//namespace ChoiceVServer.Controller.Company.Functionalities.Old {
//    public class PrisonController : ChoiceVScript {
//        public class PrisonInmate {
//            public int Id { get; private set; }
//            public CompanyPrison Company { get; private set; }
//            public int CharId { get; private set; }
//            public string Name { get; private set; }
//            public int TimeLeftOffline { get; set; }
//            public int TimeLeftOnline { get; set; }
//            public bool FreeToGo { get => TimeLeftOffline <= 0 && TimeLeftOnline <= 0; }
//            public bool GotFood { get; private set; }
//            public int InventoryId { get; private set; }

//            public PrisonInmate(int id, CompanyPrison company, IPlayer player, int timeleft, int inventoryId) {
//                Id = id;
//                Company = company;
//                CharId = player.getCharacterId();
//                Name = player.getCharacterName();
//                TimeLeftOffline = (int)(timeleft * (1 - TIMELEFT_ACTIVE_PERCENTAGE));
//                TimeLeftOnline = (int)(timeleft * TIMELEFT_ACTIVE_PERCENTAGE);
//                GotFood = false;

//                InventoryId = inventoryId;
//            }

//            public PrisonInmate(int id, CompanyPrison company, int charId, string charName, int timeLeftOnline, int timeLeftOffline, int inventoryId) {
//                Id = id;
//                Company = company;
//                CharId = charId;
//                Name = charName;
//                TimeLeftOffline = timeLeftOffline;
//                TimeLeftOnline = timeLeftOnline;
//                GotFood = false;

//                InventoryId = inventoryId;
//            }

//            public override string ToString() {
//                return $"company: {Company.Name}, charId: {CharId}, timeLeftOffline: {TimeLeftOffline}, timeLeftOnlne: {TimeLeftOnline}";
//            }
//        }

//        public static float TIMELEFT_ACTIVE_PERCENTAGE = 0.15f;
//        public static float PASSIV_TIME_MULTIPLIER = 3f;

//        private static int UPDATE_INMATES_MINUTES = 1;
//        public static List<PrisonInmate> Inmates;

//        //Spinde werden immer an eine Person vergeben(Random (!)), und wenn alle voll, wird ein zufälliger doppelt vergeben.Bei dem können nach E drücken werden dann alle Doppelbelegungen angezeigt


//        //Dietrich kann einen Spind aufbrechen.Falls Spind von niemanden aufgebrochen wird, dann können zufällige Items gefunden werdenj


//        //In Betten können Sachen versteckt werden (equip_cellbed)

//        //Türen können über Zentral Ding geöffnet werden


//        //Geld über "Punktekonto", automatisch verdient nach Arbeit


//        //Alle 30min machts Gong und man kann sich gratis Essen abholen


//        //Minijobs:


//        //Abgetrennter Abteil.
//        //Wenn letzter Wärter raus, dann alle Türen zugesperrt.
//        //Von innen kann man immer raus


//        //Wenn kein Wärter da:
//        //Man redet mit NPC (Mine, Werkstatt, Wäscherei), falls noch Platz frei, dann wird man durch die Tür geportet (durchgelassen), sollte alles geloggt werden.
//        //Danach wird Auftrag gestartet, und nach Abschluss muss man zu Tür zurück und wird rausgeportet und kriegt Geld auf Punktekonto.


//        //Wenn Wärter da:
//        //Wärter wählt Spieler aus und verteilt so Aufträge


//        //ABlauf:
//        //Eingang in Prison -> Spot in dem registriert wird.
//        //Spieler wird im System registriert und alle seine Sachen werden gespeichert.
//        //Beim registrieren wird Schrank zugewiesen (im Spind sind: Zahnbürste, Klopapier, Sportoutfit (?))
//        //Spieler kriegt Knast Outfit

//        public PrisonController() {
//            Inmates = new List<PrisonInmate>();

//            InvokeController.AddTimedInvoke("PRISON_UPDATER", updateInmates, TimeSpan.FromMinutes(UPDATE_INMATES_MINUTES), true);

//            EventController.addMenuEvent("IMPRISON_PLAYER", onImprisonPlayer);
//            EventController.addMenuEvent("FREE_PLAYER", onFreePlayer);

//            EventController.addMenuEvent("PRISON_OPEN_PRISONER_CONTAINER", onOpenPrisonerContainer);
//            EventController.addMenuEvent("PRISON_OPEN_FOOD_INVENTORY", onPrisonOpenFoodInventory);

//            //TODO ACTIVATE AGAIn
//            //EventController.MainReadyDelegate += onMainReady;
//        }

//        private void onMainReady() {
//            using (var db = new ChoiceVDb()) {
//                foreach (var dbInmate in db.prisoninmates) {
//                    var company = (CompanyPrison)CompanyController.findCompanyById(dbInmate.companyId);
//                    var inmate = new PrisonInmate(dbInmate.id, company, dbInmate.charId, dbInmate.name, dbInmate.timeLeftOnline, dbInmate.timeLeftOffline, dbInmate.inventoryId);
//                    company.Inmates.Add(inmate);
//                    Inmates.Add(inmate);

//                    Logger.logTrace($"Prison Inmate loaded: {inmate}");
//                }
//            }

//            Logger.logDebug($"{Inmates.Count} Inmates loaded");
//        }

//        private void updateInmates(IInvoke obj) {
//            //update times
//            using (var db = new ChoiceVDb()) {
//                foreach (var inmate in Inmates) {
//                    var row = db.prisoninmates.Find(inmate.Id);
//                    var player = ChoiceVAPI.FindPlayerByCharId(inmate.CharId);

//                    if (player != null) {
//                        if (inmate.TimeLeftOnline > 0) {
//                            inmate.TimeLeftOnline -= UPDATE_INMATES_MINUTES;
//                        } else {
//                            inmate.TimeLeftOffline -= UPDATE_INMATES_MINUTES;
//                        }
//                    } else {
//                        inmate.TimeLeftOffline -= UPDATE_INMATES_MINUTES;
//                    }

//                    if (inmate.TimeLeftOnline <= 0) {
//                        inmate.TimeLeftOnline = 0;
//                    }

//                    if (inmate.TimeLeftOffline <= 0) {
//                        inmate.TimeLeftOffline = 0;
//                    }

//                    row.timeLeftOffline = inmate.TimeLeftOffline;
//                    row.timeLeftOnline = inmate.TimeLeftOnline;
//                }

//                db.SaveChanges();
//            }

//            var companies = CompanyController.findCompaniesByType<CompanyPrison>(c => true);
//            foreach (var company in companies) {
//                company.update();
//            }
//        }

//        public static void imprisonPlayer(CompanyPrison company, IPlayer player, TimeSpan imprisonDuration) {
//            var inventoryContainer = InventoryController.createInventory(player.getCharacterId(), Constants.PLAYER_INVENTORY_MAX_WEIGHT, InventoryTypes.PrisonBox);

//            foreach (var item in player.getInventory().getAllItems().Reverse<Item>()) {
//                if (item is EquipItem) {
//                    var equipI = item as EquipItem;
//                    equipI.fastUnequip(player);
//                }

//                player.getInventory().moveItem(inventoryContainer, item, item.StackAmount ?? 1);
//            }

//            using (var db = new ChoiceVDb()) {
//                var dbInmate = new prisoninmate {
//                    charId = player.getCharacterId(),
//                    name = player.getCharacterName(),
//                    companyId = company.Id,
//                    inventoryId = inventoryContainer.Id,
//                    timeLeftOffline = (int)(imprisonDuration.TotalMinutes * TIMELEFT_ACTIVE_PERCENTAGE),
//                    timeLeftOnline = (int)(imprisonDuration.TotalMinutes * (1 - TIMELEFT_ACTIVE_PERCENTAGE)),
//                };

//                db.prisoninmates.Add(dbInmate);
//                db.SaveChanges();

//                var inmate = new PrisonInmate(dbInmate.id, company, player, (int)imprisonDuration.TotalMinutes, inventoryContainer.Id);
//                company.Inmates.Add(inmate);
//                Inmates.Add(inmate);
//            }

//            player.sendNotification(Constants.NotifactionTypes.Warning, $"Du wurdest in das {company.Name} eingesperrt. Alle deine Sachen wurden bei den Wachen abgegeben.", "Eingesperrt worden", Constants.NotifactionImages.Prison);
//        }

//        public static List<PrisonInmate> getPrisonersOfCompany(CompanyPrison company) {
//            return Inmates.Where(i => i.Company == company).ToList();
//        }

//        public static PrisonInmate getPrisonerByCharId(CompanyPrison company, int charId) {
//            return Inmates.FirstOrDefault(i => i.Company == company && i.CharId == charId);
//        }

//        private bool onImprisonPlayer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var company = (CompanyPrison)data["Company"];
//            var prisoner = (IPlayer)data["Player"];

//            var evt = data["PreviousCefEvent"] as MenuStatsMenuItemEvent;

//            try {
//                var timeEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
//                var time = int.Parse(timeEvt.input);

//                imprisonPlayer(company, prisoner, TimeSpan.FromMinutes(time));

//                player.sendNotification(Constants.NotifactionTypes.Info, "Person erfolgreich eingesperrt!", "Person eingesperrt", Constants.NotifactionImages.Prison);
//            } catch (Exception) {
//                player.sendBlockNotification("Falsche Eingabe!", "");
//            }

//            return true;
//        }

//        private bool onFreePlayer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var company = (CompanyPrison)data["Company"];
//            var inmate = (PrisonInmate)data["Imprisoned"];

//            inmate.TimeLeftOnline = 0;
//            inmate.TimeLeftOffline = 0;

//            return true;
//        }

//        private bool onOpenPrisonerContainer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var company = (CompanyPrison)data["Company"];
//            var inmate = (PrisonInmate)data["Imprisoned"];

//            var inv = InventoryController.loadInventory(inmate.InventoryId);

//            if (inv != null) {
//                InventoryController.showMoveInventory(player, player.getInventory(), inv, null, null, inmate.Name, true);
//            } else {
//                player.sendBlockNotification("Etwas ist schiefgelaufen. Melde dich beim Dev-Team. Code: PrisonHawk", "Code: PrisonHawk");
//            }

//            return true;
//        }

//        private bool onPrisonOpenFoodInventory(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var company = (CompanyPrison)data["Company"];

//            if (company.FoodInventory != null) {
//                var inv = InventoryController.createInventory(company.Id, 1000, InventoryTypes.Company);
//                company.setSetting("FOOD_INVENTORY_ID", inv.Id.ToJson());
//            }

//            InventoryController.showMoveInventory(player, player.getInventory(), company.FoodInventory, null, null, "Essensausgabe", true);

//            return true;
//        }
//    }

//    public enum GetOutPointType : int {
//        Normal = 0,
//        LastPoint = 1,
//    }

//    public class CompanyPrison : Company {
//        private class GetOutPoint {
//            public int Id;
//            public CompanyPrison Company;
//            public CollisionShape FromShape;

//            public GetOutPointType Type;
//            public Position ToPos;

//            public GetOutPoint(int id, GetOutPointType type, CompanyPrison company, CollisionShape fromShape, Position toPos) {
//                Id = id;
//                Type = type;
//                Company = company;
//                FromShape = fromShape;
//                FromShape.OnEntityEnterShape += onEntityEnter;

//                ToPos = toPos;
//            }

//            public GetOutPoint(CompanyPrison company, string shortSaveString) {
//                Company = company;

//                var split = shortSaveString.Split('%');
//                Id = split[0].FromJson<int>();
//                FromShape = CollisionShape.Create(split[1]);
//                FromShape.OnEntityEnterShape += onEntityEnter;

//                ToPos = split[2].FromJson<Position>();
//            }

//            private void onEntityEnter(CollisionShape shape, IEntity entity) {
//                var player = entity as IPlayer;
//                var inmate = Company.getPrisonerByCharId(player.getCharacterId());

//                if (inmate != null && inmate.FreeToGo) {
//                    player.sendNotification(Constants.NotifactionTypes.Info, "Bleibe 10sek stehen um durch die Tür \"geleitet\" zu werden", "Gefängnisausgang", Constants.NotifactionImages.Prison);
//                    InvokeController.AddTimedInvoke("EXIT_PORTER", (i) => {
//                        if (FromShape.AllEntities.Contains(entity)) {
//                            player.Position = ToPos;
//                            player.sendNotification(Constants.NotifactionTypes.Info, "Du wurdest vom Gefängniswärter durch die Tür geleitet. Vergiss nicht deine Sachen abzuholen!", "Gefängnisausgang", Constants.NotifactionImages.Prison);

//                            if (Type == GetOutPointType.LastPoint) {
//                                Company.onPrisonerFinishSentence(player, inmate);
//                            }
//                        }
//                    }, TimeSpan.FromSeconds(10), false);
//                }
//            }

//            public string toShortSave() {
//                return $"{Id}%{FromShape.toShortSave()}%{ToPos.ToJson()}%{((int)Type).ToJson()}";
//            }
//        }

//        public List<PrisonInmate> Inmates;

//        private List<GetOutPoint> GetOutPoints;

//        private Ped GetOutPed;

//        private CollisionShape PrisonShape;
//        private CollisionShape FoodGetPoint;
//        private CollisionShape ImprisonSpot;

//        public Inventory FoodInventory { get; set; }

//        private bool IsEatingTime { get => DateTime.Now < EatingTimeEnd; }

//        private int EatingStartMinutes;
//        private TimeSpan EatingTimeDuration;
//        private DateTime EatingTimeEnd;

//        public CompanyPrison(company dbComp, system system, List<companysetting> allSettings) : base(dbComp, system, allSettings) {
//            Inmates = new List<PrisonInmate>();

//            //Prison outline
//            var outline = getSetting("PRISON_OUTLINE");
//            if (outline != default) {
//                PrisonShape = CollisionShape.Create(outline);
//                PrisonShape.OnEntityExitShape += onPrisonExit;
//            }

//            //Food getPoint
//            var foodPoint = getSetting("FOOD_POINT");
//            if (foodPoint != default) {
//                FoodGetPoint = CollisionShape.Create(foodPoint);
//                FoodGetPoint.OnCollisionShapeInteraction += onFoodGetPointInteract;
//            }

//            //Imprison spot
//            var impPoint = getSetting("IMPRISON_POINT");
//            if (impPoint != default) {
//                ImprisonSpot = CollisionShape.Create(impPoint);
//                ImprisonSpot.OnCollisionShapeInteraction += onImprisonSpotInteract;
//            }

//            //GetOutPed
//            var getOutPedPos = getSetting<Position>("GUARD_POSITION");
//            if (getOutPedPos != default) {
//                var getOutPedRot = getSetting<float>("GUARD_ROTATION");
//                GetOutPed = PedController.createPed(null, "s_m_m_prisguard_01", getOutPedPos, getOutPedRot);
//                GetOutPed.addModule(new NPCMenuItemsModule(GetOutPed, new List<PlayerMenuItemGenerator> { onPrisonGuardInteraction }));
//            }

//            EatingStartMinutes = getSetting<int>("PRISON_EATING_START");
//            EatingTimeDuration = TimeSpan.FromMinutes(getSetting<int>("PRISON_EATING_DURATION"));

//            var pointsCount = getSetting("GET_OUT_POINTS_COUNT");
//            if (pointsCount == null) {
//                setSetting("GET_OUT_POINTS_COUNT", 0.ToJson());
//            }

//            GetOutPoints = new List<GetOutPoint>();
//            var points = getSettings("GET_OUT_POINTS");
//            foreach (var point in points) {
//                GetOutPoints.Add(new GetOutPoint(this, point.settingsValue));
//            }

//            var inventoryId = getSetting<int>("FOOD_INVENTORY_ID");
//            if (inventoryId != default) {
//                FoodInventory = InventoryController.loadInventory(inventoryId);
//            }

//            //Peds
//            registerCompanyAdminElement(
//                "PRISON_PED_CREATION",
//                getPedCreatorGenerator,
//                onPedCreator
//            );

//            //Collisionshapes
//            registerCompanyAdminElement(
//                "PRISON_SHAPE_CREATION",
//                getCollisionShapeCreatorGenerator,
//                onCollisionShapeCreator
//            );

//            //GetOutPoints
//            registerCompanyAdminElement(
//                "PRISON_GET_OUT_CREATION",
//                getGetOutPointGenerator,
//                onGetOutPointCreator
//            );

//            //Eatingstuff
//            registerCompanyAdminElement(
//                "PRISON_EAT_CREATION",
//                getEatingStuffGenerator,
//                onEatingStuff
//            );
//        }

//        public PrisonInmate getPrisonerByCharId(int charId) {
//            return Inmates.FirstOrDefault(i => i.CharId == charId);
//        }

//        #region AdminSelect

//        #region Ped

//        private MenuElement getPedCreatorGenerator() {
//            var menu = new Menu("Peds setzen", "Setze die verschiedenen Peds");
//            menu.addMenuItem(new ClickMenuItem("Gefängniswärter setzen", "Setze den Gefängniswärter. Nimmt die aktuelle Position und Rotation!", "", "PRISON_GUARD"));
//            return menu;
//        }

//        private void onPedCreator(IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            switch (subEvent) {
//                case "PRISON_GUARD":
//                    setSetting("GUARD_POSITION", player.Position.ToJson());
//                    setSetting("GUARD_ROTATION", player.Rotation.Yaw.ToJson());
//                    break;
//            }
//        }

//        #endregion

//        #region CollisionShape

//        private MenuElement getCollisionShapeCreatorGenerator() {
//            var menu = new Menu("Kollisionen setzen", "Setze die verschiedenen Kollisionen");
//            menu.addMenuItem(new ClickMenuItem("Gefängnis setzen", "Setze die Outline des Gefängnisses", "", "PRISON_OUTLINE"));
//            menu.addMenuItem(new ClickMenuItem("Einsperrposition setzen", "Setze den Ort des Einsperrens", "", "IMPRISON_SPOT"));

//            var count = 0;
//            var getOutMenu = new Menu("Ausgangspunkte setzen", "Setze die Portpunkte an denen der Spieler");
//            foreach (var point in GetOutPoints) {
//                var data = new Dictionary<string, dynamic> { { "Point", point } };
//                var pointMenu = new Menu($"Punkt {count}", "Verwalte den Punkt");
//                pointMenu.addMenuItem(new ClickMenuItem("Zur Kollision porten", "Porte die zur Kollision an dem die Spieler sich porten kann", "", "GET_OUT_POINT_PORT1").withData(data));
//                pointMenu.addMenuItem(new ClickMenuItem("Zum Punkt porten", "Porte die zum Punkt an dem die Spieler rauskommen", "", "GET_OUT_POINT_PORT2").withData(data));
//                pointMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche den Portpunkt", "", "GET_OUT_POINT_DELETE").withData(data).needsConfirmation("Punkt löschen?", "Punkt wirklich löschen?"));

//                getOutMenu.addMenuItem(new MenuMenuItem(getOutMenu.Name, getOutMenu));
//            }
//            getOutMenu.addMenuItem(new ClickMenuItem("Punkt hinzufügen", "Füge einen Ausgangspunkt hinzu", "", "GET_OUT_POINT_ADD"));

//            return menu;
//        }

//        private void onCollisionShapeCreator(IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            switch (subEvent) {
//                case "PRISON_OUTLINE":
//                    player.sendNotification(Constants.NotifactionTypes.Info, "Setze nun die Kollision des Gefängnisses. Das ganze Gefängnis sollte großzügig drinnen sein", "");
//                    CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
//                        PrisonShape = CollisionShape.Create(p, w, h, r, true, false, true);
//                        PrisonShape.OnEntityExitShape += onPrisonExit;

//                        setSetting("PRISON_OUTLINE", PrisonShape.toShortSave());
//                        player.sendNotification(Constants.NotifactionTypes.Success, "Gefängnisoutline gesetzt ", "");
//                    });
//                    break;
//                case "IMPRISON_SPOT":
//                    player.sendNotification(Constants.NotifactionTypes.Info, "Setze nun die Kollision zum Einsperren von Spielern", "");
//                    CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
//                        ImprisonSpot = CollisionShape.Create(p, w, h, r, true, false, true);
//                        ImprisonSpot.OnCollisionShapeInteraction += onImprisonSpotInteract;

//                        setSetting("IMPRISON_POINT", ImprisonSpot.toShortSave());
//                        player.sendNotification(Constants.NotifactionTypes.Success, "Einsperrenspot gesetzt ", "");
//                    });
//                    break;
//            }
//        }

//        #endregion

//        #region Get Out Points

//        private MenuElement getGetOutPointGenerator() {
//            var menu = new Menu("Ausgangspunkte setzen", "Setze die Portpunkte an denen der Spieler");
//            foreach (var point in GetOutPoints) {
//                var data = new Dictionary<string, dynamic> { { "Point", point }, };
//                var pointMenu = new Menu($"Punkt {point.Id}", "Verwalte den Punkt");
//                pointMenu.addMenuItem(new ClickMenuItem("Zur Kollision porten", "Porte die zur Kollision an dem die Spieler sich porten kann", "", "POINT_PORT1").withData(data));
//                pointMenu.addMenuItem(new ClickMenuItem("Zum Punkt porten", "Porte die zum Punkt an dem die Spieler rauskommen", "", "POINT_PORT2").withData(data));
//                pointMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche den Portpunkt", "", "POINT_DELETE", MenuItemStyle.red).withData(data).needsConfirmation("Punkt löschen?", "Punkt wirklich löschen?"));

//                menu.addMenuItem(new MenuMenuItem(pointMenu.Name, pointMenu));
//            }

//            var list = ((GetOutPointType[])Enum.GetValues(typeof(GetOutPointType))).Select(t => t.ToString()).ToArray();
//            menu.addMenuItem(new ListMenuItem("Punkt hinzufügen", "Füge einen Ausgangspunkt hinzu. STEHE AN DER AUSGANGSPOSITION!", list, "POINT_ADD", MenuItemStyle.green));

//            return menu;
//        }

//        private void onGetOutPointCreator(IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            switch (subEvent) {
//                case "POINT_PORT1":
//                    var point = (GetOutPoint)data["Point"];
//                    player.Position = new Position(point.FromShape.Position.X, point.FromShape.Position.Y, point.FromShape.Position.Z + 1);
//                    break;
//                case "POINT_PORT2":
//                    var point2 = (GetOutPoint)data["Point"];
//                    player.Position = new Position(point2.ToPos.X, point2.ToPos.Y, point2.ToPos.Z + 1);
//                    break;
//                case "POINT_DELETE":
//                    var point3 = (GetOutPoint)data["Point"];
//                    deleteSetting($"POINT_{point3.Id}", "GET_OUT_POINTS");
//                    player.sendBlockNotification("Punkt erfolgreich gelöscht!", "Punkt gelöscht");
//                    break;
//                case "POINT_ADD":
//                    var evt = menuItemCefEvent as ListMenuItemEvent;
//                    var type = Enum.Parse<GetOutPointType>(evt.currentElement);
//                    var toPos = player.Position.ToJson().FromJson();
//                    player.sendNotification(Constants.NotifactionTypes.Info, "Ausgangsposition gesetzt, setze nun den Portshape!", "Position gespeichert");
//                    CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
//                        using (var db = new ChoiceVDb()) {
//                            var currentId = getSetting("GET_OUT_POINTS_COUNT").FromJson<int>();

//                            var point = new GetOutPoint(currentId + 1, type, this, CollisionShape.Create(p, w, h, r, true, false, false), toPos);
//                            setSetting((currentId + 1).ToJson(), point.toShortSave(), "GET_OUT_POINTS");
//                            setSetting("GET_OUT_POINTS_COUNT", (currentId + 1).ToJson());

//                            GetOutPoints.Add(point);
//                            player.sendNotification(Constants.NotifactionTypes.Success, "Punkt erfolgreich hinzugefügt!", "Punkt gespeichert");
//                        }
//                    });
//                    break;
//            }
//        }

//        #endregion

//        #region EatingStuff

//        private MenuElement getEatingStuffGenerator() {
//            var menu = new Menu("Essensausgabe bearbeiten", "Setze die verschiedenen Kollisionen");
//            menu.addMenuItem(new ClickMenuItem("Kollision setzen", "Setze die Essenausgabe des Gefängnisses", "", "FOOD_GET_POINT"));
//            menu.addMenuItem(new InputMenuItem("Ausgabestart setzen", "Der Ausgabestart ist immer zu einer bestimmen Minute der Stunde. z.B. xx:45 Uhr", "", "FOOD_GET_START").needsConfirmation("Ausgabe setzen?", "Ausgabe wirklich setzen?"));
//            menu.addMenuItem(new InputMenuItem("Essenszeit setzen", "Setze wie lange die Essensausgabe dauert. Angabe in Minuten!", "z.B. 15", "FOOD_GET_DURATION").needsConfirmation("Ausgabe setzen?", "Ausgabe wirklich setzen?"));
//            return menu;
//        }

//        private void onEatingStuff(IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            switch (subEvent) {
//                case "FOOD_GET_POINT":
//                    player.sendNotification(Constants.NotifactionTypes.Info, "Setze nun die Kollision der Essensausgabe", "");
//                    CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
//                        FoodGetPoint = CollisionShape.Create(p, w, h, r, true, false, true);
//                        FoodGetPoint.OnCollisionShapeInteraction += onFoodGetPointInteract;

//                        setSetting("FOOD_POINT", FoodGetPoint.toShortSave());
//                        player.sendNotification(Constants.NotifactionTypes.Success, "Einsperrenspot gesetzt ", "");
//                        player.sendNotification(Constants.NotifactionTypes.Success, "Essenausgabe gesetzt ", "");
//                    });
//                    break;
//                case "FOOD_GET_START":
//                    var evt = data["PreviousItemEvent"] as InputMenuItemEvent;
//                    try {
//                        setSetting("PRISON_EATING_START", int.Parse(evt.input).ToJson());
//                        player.sendNotification(Constants.NotifactionTypes.Success, "Essensausgabestart gesetzt ", "");
//                    } catch (Exception) {
//                        player.sendBlockNotification("Eingabe falsch! Gib eine Zahl ein", "");
//                    }
//                    break;
//                case "FOOD_GET_DURATION":
//                    var durEvt = data["PreviousItemEvent"] as InputMenuItemEvent;
//                    try {
//                        setSetting("PRISON_EATING_DURATION", int.Parse(durEvt.input).ToJson());
//                        player.sendNotification(Constants.NotifactionTypes.Success, "Essenausgabezeit gesetzt ", "");
//                    } catch (Exception) {
//                        player.sendBlockNotification("Eingabe falsch! Gib eine Zahl ein", "");
//                    }
//                    break;
//            }
//        }

//        #endregion

//        #endregion

//        private void onFoodGetPointInteract(IPlayer player) {
//            var menu = new Menu("Essensausgabe", "Was möchtest du tun?");

//            var employee = findEmployee(player.getCharacterId());
//            if (employee != null && hasEmployeePermission(employee, "PRISON_FOOD")) {
//                var data = new Dictionary<string, dynamic> { { "Company", this } };
//                menu.addMenuItem(new ClickMenuItem("Essensausgabe öffnen", "Öffne die Essensausgabe", "", "PRISON_OPEN_FOOD_INVENTORY").withData(data));
//            }

//            var inmate = getPrisonerByCharId(player.getCharacterId());
//            if (inmate != null) {
//                if (!inmate.GotFood) {
//                    menu.addMenuItem(new ClickMenuItem("Essen abholen", "Hole dir dein Essen ab.", "", "PRISON_GET_FOOD"));
//                } else {
//                    menu.addMenuItem(new StaticMenuItem("Essen abholen", "Du hast dir dein Essen schon abgeholt!", "", MenuItemStyle.red));
//                }
//            }

//            player.showMenu(menu);
//        }

//        private void onImprisonSpotInteract(IPlayer player) {
//            var menu = new Menu("Gefangenenverwaltung", "Verwalte Gefangene und sperre neu ein");

//            var employee = findEmployee(player.getCharacterId());
//            if (employee != null) {
//                if (hasEmployeePermission(employee, "PRISON_IMPRISON")) {
//                    var prisoners = ImprisonSpot.getAllEntities().Where(e => e is IPlayer && HandCuffController.hasHandsUp(e as IPlayer)).ToList();
//                    var prisonerMenu = new Menu("Person einsperren", "Welche Person willst du einsperren?");
//                    foreach (IPlayer prisoner in prisoners) {
//                        var data = new Dictionary<string, dynamic> { { "Company", this }, { "Player", prisoner } };
//                        var subMenu = new Menu(prisoner.getCharacterName(), "Trage die Informationen ein");
//                        subMenu.addMenuItem(new InputMenuItem("Haftzeit einstellen", "Gib an wieviele Hafteinheiten der Spieler eingesperrt werden soll", "", ""));
//                        subMenu.addMenuItem(new MenuStatsMenuItem("Person einsperren", "Sperre die besagte Person ein", "IMPRISON_PLAYER", MenuItemStyle.yellow).withData(data).needsConfirmation($"{player.getCharacterName()} einsperren?", "Person wirklich einsperren?"));

//                        prisonerMenu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
//                    }
//                    menu.addMenuItem(new MenuMenuItem(prisonerMenu.Name, prisonerMenu));
//                }

//                if (hasEmployeePermission(employee, "PRISON_CONTROL")) {
//                    var imprisonedMenu = new Menu("Gefangene verwalten", "Verwalte aktuelle Gefangene");
//                    foreach (var imprisoned in Inmates) {
//                        var data = new Dictionary<string, dynamic> { { "Company", this }, { "Imprisoned", imprisoned } };

//                        var subMenu = new Menu(imprisoned.Name, "Verwalte diesen Gefangenen");
//                        var activTime = TimeSpan.FromMinutes(imprisoned.TimeLeftOnline).TotalHours;
//                        subMenu.addMenuItem(new StaticMenuItem("Aktivzeit ändern", $"Die Person hat noch aktuell {Math.Round(activTime)}h Aktivzeit übrig", $"{Math.Round(activTime, 1)}h"));

//                        var passivTime = TimeSpan.FromMinutes(imprisoned.TimeLeftOffline).TotalHours;
//                        subMenu.addMenuItem(new StaticMenuItem("Passivzeit ändern", $"Die Person hat noch {Math.Round(passivTime)}h Passivzeit übrig", $"{Math.Round(passivTime, 1)}h"));

//                        subMenu.addMenuItem(new ClickMenuItem("abgegebene Sachen", "Öffne die Box mit den abgegebenen Sachen des Gefangenen", "", "PRISON_OPEN_PRISONER_CONTAINER").withData(data));

//                        subMenu.addMenuItem(new ClickMenuItem("Freilassen", $"Lasse die Person umgehend frei", "", "FREE_PLAYER", MenuItemStyle.red).withData(data).needsConfirmation($"{imprisoned.Name} freilassen?", "Person wirklich freilassen?"));
//                        imprisonedMenu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
//                    }
//                    menu.addMenuItem(new MenuMenuItem(imprisonedMenu.Name, imprisonedMenu));
//                }
//            } else {
//                var inmate = getPrisonerByCharId(player.getCharacterId());

//                if (inmate != null && inmate.FreeToGo) {
//                    var data = new Dictionary<string, dynamic> { { "Company", this }, { "Imprisoned", inmate } };

//                    menu.addMenuItem(new ClickMenuItem("Sachen abholen", "Öffne die Box mit deinen abgegebenen Sachen", "", "PRISON_OPEN_PRISONER_CONTAINER").withData(data));

//                }
//            }

//            player.showMenu(menu);
//        }

//        public MenuItem onPrisonGuardInteraction(IPlayer player) {
//            var prisoner = getPrisonerByCharId(player.getCharacterId());

//            if (prisoner != null) {
//                var menu = new Menu("Gefängniswärter", "Was möchtest du tun?");

//                var activTime = TimeSpan.FromMinutes(prisoner.TimeLeftOnline).TotalHours;
//                menu.addMenuItem(new StaticMenuItem("Aktivzeit", $"Du hast noch {Math.Round(activTime)}h aktive Gefangenenzeit übrig", $"{Math.Round(activTime, 1)}h"));

//                var passivTime = TimeSpan.FromMinutes(prisoner.TimeLeftOnline).TotalHours;
//                menu.addMenuItem(new StaticMenuItem("Passive Zeit", $"Du hast noch {Math.Round(passivTime)}h passive Gefangenenzeit übrig. Passivzeit kann aktiv mit dem Faktor {PASSIV_TIME_MULTIPLIER}x abgesessen werden.", $"{Math.Round(passivTime, 1)}h"));

//                return new MenuMenuItem(menu.Name, menu);
//            } else {
//                player.sendBlockNotification("Du bist kein Gefangener in diesem Gefängnis", "Kein Gefangener", Constants.NotifactionImages.Prison);
//                return null;
//            }
//        }

//        private void onPrisonerFinishSentence(IPlayer player, PrisonInmate inmate) {
//            //TODO CALL PRISONCONTROLLER TO REMOVE PRISONINMATE
//        }

//        private void onPrisonExit(CollisionShape shape, IEntity entity) {
//            //TODO LOOK IF PRISONER; IF YES HE ESCAPES
//        }

//        public void update() {
//            if (DateTime.Now.Minute > EatingStartMinutes && !IsEatingTime) {
//                EatingTimeEnd = DateTime.Now + EatingTimeDuration;

//                //TODO BELL SOUND
//            }
//        }
//    }
//}
