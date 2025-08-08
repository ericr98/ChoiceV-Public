using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.FsDatabase;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Controller.Companies.Company;
using static ChoiceVServer.Controller.CompanyExternalCompanyInteractController;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller {
    public record CompanyPermission(string Identifier);

    public class CompanyPermissionElement {
        private Func<MenuItem> MenuItemGenerator = null;
        private Func<Menu> MenuGenerator = null;

        public MenuItem MenuItem { get => getMenuItem(); }
        public CompanyPermission Permission;
        public Predicate<Company> CompanyPredicate;
        public Predicate<IPlayer> TargetPredicate;

        /// <summary>
        /// Adds a Specific Permission Element.
        /// </summary>
        /// <param name="menuItem">MenuItem shown to player</param>
        /// <param name="permission">The needed permission for the player</param>
        /// <param name="companyPredicate">An additional predicate for the company (e.g number of workers > 5)</param>
        public CompanyPermissionElement(Func<MenuItem> generator, CompanyPermission permission, Predicate<IPlayer> targetPredicate = null, Predicate<Company> companyPredicate = null) {
            MenuItemGenerator = generator;
            Permission = permission;

            if(targetPredicate != null) {
                TargetPredicate = targetPredicate;
            } else {
                TargetPredicate = (sender) => true;
            }

            if(companyPredicate != null) {
                CompanyPredicate = companyPredicate;
            } else {
                CompanyPredicate = (sender) => true;
            }
        }

        /// <summary>
        /// Adds a Specific Permission Element.
        /// </summary>
        /// <param name="menu">Menu shown to player</param>
        /// <param name="permission">The needed permission for the player</param>
        /// <param name="shownWhen">An additional predicate for the company (e.g number of workers > 5)</param>
        public CompanyPermissionElement(Func<Menu> generator, CompanyPermission permission, Predicate<IPlayer> targetPredicate = null, Predicate<Company> companyPredicate = null) {
            MenuGenerator = generator;
            Permission = permission;

            if(targetPredicate != null) {
                TargetPredicate = targetPredicate;
            } else {
                TargetPredicate = (sender) => true;
            }

            if(companyPredicate != null) {
                CompanyPredicate = companyPredicate;
            } else {
                CompanyPredicate = (sender) => true;
            }
        }

        private MenuItem getMenuItem() {
            if(MenuItemGenerator != null) {
                return MenuItemGenerator.Invoke();
            } else {
                var menu = MenuGenerator.Invoke();
                return new MenuMenuItem(menu.Name, menu);
            }
        }
    }

    public delegate void AllCompaniesLoadedDelegate();

    public class CompanyController : ChoiceVScript {
        public static TimeSpan COMPANY_UPDATE_TIME = TimeSpan.FromMinutes(0.1);

        public static Dictionary<int, Company> AllCompanies = [];
        public static Dictionary<string, CompanyPermission> AllCompanyPermissions = new Dictionary<string, CompanyPermission>();
        public static List<CompanyPermissionElement> AllCompanyPermissionInteractionElements = new List<CompanyPermissionElement>();

        public static AllCompaniesLoadedDelegate AllCompaniesLoadedDelegate;

        public CompanyController() {
            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnect;
            EventController.PlayerDisconnectedDelegate += onPlayerDisconnect;

            InteractionController.addPlayerInteractionElement(
                new ConditionalGeneratedPlayerInteractionMenuElement(
                    "Firmen-Menü",
                    companyInteractionGenerator,
                    sender => sender is IPlayer player && isInCompany(player),
                    target => true
                )
            );

            InteractionController.addVehicleInteractionElement(
                new ConditionalGeneratedPlayerInteractionMenuElement(
                    "Firmen-Menü",
                    companyInteractionGenerator,
                    sender => sender is IPlayer player && isInCompany(player),
                    target => true
                )
            );

            CharacterController.addSelfMenuElement(
                new ConditionalPlayerSelfMenuElement(
                    "Admin-Firmeneinstellungen",
                    companyAdminOptionGenerator,
                    sender => sender.getCharacterData().AdminMode && (sender as IPlayer).getAdminLevel() >= 3,
                    MenuItemStyle.yellow
                )
            );

            EventController.addMenuEvent("ADD_COMPANY_FUNCTIONALITY", onAddCompanyFunctionality);
            EventController.addMenuEvent("REMOVE_COMPANY_FUNCTIONALITY", onRemoveCompanyFunctionality);

            CharacterController.addSelfMenuElement(
                new ConditionalPlayerSelfMenuElement(
                    "Firmenaktionen",
                    companySelfMenuGenerator,
                    isInCompany
                )
            );

            EventController.addMenuEvent("OPEN_COMPANY_SETTINGS_ADMIN_MENU", onCompanySettingsAdminMenu);

            EventController.addMenuEvent("COMPANY_ELEMENT_SELECT", onCompanyMenuElementSelect);

            Company.CompanyHireEmployeeDelegate += onCompanyHireEmployee;
            Company.CompanyFireEmployeeDelegate += onCompanyFireEmployee;
            Company.CompanyDataChangeDelegate += onCompanyDataChange;
            Company.CompanyDataRemoveDelegate += onCompanyDataRemove;

            Company.CompanyLogDelegate += onCompanyLog;

            Company.CompanyChangeSettingDelegate += onCompanyChangeSetting;
            Company.CompanyDeleteSettingDelegate += onCompanyDeleteSetting;

            Company.CompanyEmployeeEnterDuty += onCompanyEnterDuty;
            Company.CompanyEmployeeExitDuty += onCompanyExitDuty;

            InvokeController.AddTimedInvoke("Company-Updater", updateCompanies, COMPANY_UPDATE_TIME, true);

            EventController.MainReadyDelegate += loadCompanies;

            EventController.addEvent("FIRE_EMPLOYEE", onFireEmployee);
            EventController.addMenuEvent("FIRE_EMPLOYEE_SUBMIT", onFireEmployeeSubmit);

            EventController.addEvent("FS_RANK_UPDATE", onFsRankUpdate);
            EventController.addEvent("FS_EMPLOYEE_UPDATE", onFsEmployeeUpdate);

            setEmployeesToNotHaveDuty();

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    generateCompanyCreator,
                    3,
                    SupportMenuCategories.Firmen,
                    "Firma erstellen"
                )
            );
            EventController.addMenuEvent("SUPPORT_CREATE_COMPANY", onSupportCreateCompany);

        }

        public static void addExternalCompanySelfMenuRegister(string identifier, GeneratedCompanySelfMenuElementDelegate generator, CompanyMenuElementCallbackDelegate callback, string permissionRequirement = null) {
            CompanyExternalCompanyInteractController.addExternalCompanySelfMenuRegister(identifier, generator, callback, permissionRequirement);
        }

        public Menu generateCompanyCreator() {
            var menu = new Menu("Firma erstellen", "Gib die Daten ein");

            menu.addMenuItem(new InputMenuItem("Name", "Der Name der Firma", "", ""));
            menu.addMenuItem(new InputMenuItem("Kurzname", "Eine Kurzbezeichnung der Firma. z.B. Polizei, etc. kann auch Abkürzung sein", "", ""));
            menu.addMenuItem(new InputMenuItem("Abkürzung", "Die Abkürzung der Firma. z.B. ACLS, LSPD", "", ""));
            menu.addMenuItem(new InputMenuItem("Logoname", "Der Bildname des Logos", "", ""));
            var list = Enum.GetValues<CompanyType>().Select(t => t.ToString()).ToArray();
            menu.addMenuItem(new ListMenuItem("Firmentyp", "Gib den Typ der Firma an. Bestimmt die Permissions", list, ""));
            menu.addMenuItem(new InputMenuItem("Bankaccount", "Gib den Bankaccount der Firma an. -1 bedeutet, dass die Firma keinen Account hat, leerlassen für die Erstellung eines neuen Kontos", "", ""));
            menu.addMenuItem(new MenuStatsMenuItem("Firma erstellen", "Erstelle die Firma", "SUPPORT_CREATE_COMPANY", MenuItemStyle.green));

            return menu;
        }

        private bool onSupportCreateCompany(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var shortNameEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var abbreviationEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
            var logoEvt = evt.elements[3].FromJson<InputMenuItemEvent>();
            var typeEvt = evt.elements[4].FromJson<ListMenuItemEvent>();
            var type = Enum.Parse<CompanyType>(typeEvt.currentElement);
            var codeEvt = evt.elements[5].FromJson<InputMenuItemEvent>();
            var bankAccountEvt = evt.elements[6].FromJson<InputMenuItemEvent>();

            if(bankAccountEvt.input != null && bankAccountEvt.input != "") {
                createNewCompany(player, nameEvt.input, shortNameEvt.input, abbreviationEvt.input, logoEvt.input, type, codeEvt.input, player.Position, int.Parse(bankAccountEvt.input));
            } else {
                var company = createNewCompany(player, nameEvt.input, shortNameEvt.input, abbreviationEvt.input, logoEvt.input, type, codeEvt.input, player.Position, null);

                var values = Enum.GetValues<BankCompanies>();
                var random = new Random();
                var bankType = values[random.Next(values.Length)];
                var bankAccount = BankController.createBankAccount(company, 0, BankController.getBankByType(bankType), false);

                using(var db = new ChoiceVDb()) {
                    var dbCompany = db.configcompanies.Find(company.Id);
                    if(dbCompany != null) {
                        dbCompany.companyBankAccount = bankAccount.id;

                        db.SaveChanges();
                    }
                    company.CompanyBankAccount = bankAccount.id;
                }
            }

            return true;
        }

        private configcompanysetting onCompanyChangeSetting(Company company, string name, string value, string category) {
            using(var db = new ChoiceVDb()) {
                var already = db.configcompanysettings.FirstOrDefault(s => s.companyId == company.Id && s.settingsCategory == category && s.settingsName == name);
                if(already != null) {
                    already.settingsName = name;
                    already.settingsValue = value;
                    already.settingsCategory = category;

                    db.SaveChanges();

                    return already;
                } else {
                    var settings = new configcompanysetting {
                        companyId = company.Id,
                        settingsName = name,
                        settingsValue = value,
                        settingsCategory = category,
                    };

                    db.configcompanysettings.Add(settings);

                    db.SaveChanges();

                    return settings;
                }
            }
        }

        private void onCompanyDeleteSetting(Company company, string name, string category) {
            using(var db = new ChoiceVDb()) {
                var setting = db.configcompanysettings.FirstOrDefault(s => s.companyId == company.Id && s.settingsCategory == category && s.settingsName == name);
                db.configcompanysettings.Remove(setting);

                db.SaveChanges();
            }
        }

        private void onPlayerDisconnect(IPlayer player, string reason) {
            var comps = getCompanies(player);
            if(comps != null) {
                foreach(var company in comps) {
                    var employee = company.findEmployee(player.getCharacterId());
                    if(employee.InDuty) {
                        company.stopDuty(employee);
                    }
                }
            }
        }

        private void updateCompanies(IInvoke obj) {
            using(var db = new ChoiceVDb()) {
                var serverAccount = BankController.getControllerBankaccounts(typeof(CompanyController)).First();
                foreach(var company in AllCompanies.Values) {
                    var updateList = company.payDay(serverAccount);

                    if(updateList != null) {
                        var ids = updateList.Keys.ToList();
                        var duties = db.companyemployeesduties.Where(c => ids.Contains(c.id)).ToList();

                        foreach(var duty in duties) {
                            var record = updateList[duty.id];

                            duty.successfullyTransfered = record.SuccessfullyTransfered;
                            duty.transferFailMessage = record.FailMessage;
                            duty.earnedMoney = record.EarnedMoney;
                            duty.transferedBankAccount = record.TransferedBankAccount;
                        }
                    }
                }

                db.SaveChanges();
            }
        }

        private void loadCompanies() {
            if (BankController.getControllerBankaccounts(typeof(CompanyController)).Count == 0) {
                BankController.createBankAccount(typeof(CompanyController), "Haupt-Firmenkonto", BankAccountType.CompanyKonto, 0, BankController.getBankByType(BankCompanies.LibertyBank), true);
            }

            using(var fsDb = new ChoiceVFsDb()) {
                foreach(var row in fsDb.permission_single_permissions) {
                    AllCompanyPermissions.Add(row.identifier, new CompanyPermission(row.identifier));
                }
            }

            using(var dbFs = new ChoiceVFsDb())
            using(var db = new ChoiceVDb()) {
                var table = db.configcompanies
                    .Include(c => c.companyemployees)
                    .ThenInclude(ce => ce.companyemployeesduties)
                    .Include(c => c.companydata)
                    .Include(c => c.configcompanysettings)
                    .Include(c => c.configcompanyfunctionalities);


                var functionalityDict = new Dictionary<string, Type>();
                var functionalityTypes = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => !t.IsAbstract && t.IsClass && typeof(CompanyFunctionality).IsAssignableFrom(t));

                foreach(var type in functionalityTypes) {
                    var instance = (CompanyFunctionality)Activator.CreateInstance(type);
                    functionalityDict.Add(instance.getIdentifier(), type);
                }

                foreach(var row in table) {
                    var fsSystem = dbFs.systems
                        .Include(s => s.permission_ranks)
                            .ThenInclude(r => r.permission_users_to_ranks)
                        .FirstOrDefault(s => s.companyId == row.id);

                    Logger.logDebug(LogCategory.ServerStartup, LogActionType.Created, $"Company loaded: id: {row.id}, name: {row.name}, buildingType: {row.buildingType}");
                    var company = new Company(row, fsSystem, row.configcompanysettings.ToList());
                    AllCompanies.Add(row.id, company);

                    foreach(var functionality in row.configcompanyfunctionalities) {
                        var functionalityType = functionalityDict[functionality.identifier];

                        company.addFunctionality((CompanyFunctionality)Activator.CreateInstance(functionalityType, company));
                        Logger.logDebug(LogCategory.ServerStartup, LogActionType.Created, $"Created functionality {functionality.identifier} for company {company.Id}");
                    }


                    foreach(var employee in row.companyemployees) {
                        var rank = fsSystem.permission_ranks
                                    .FirstOrDefault(r => r.systemId == fsSystem.id && r.permission_users_to_ranks.Any(utr => utr.userId == employee.charId));
                        if(rank != null && rank.salary != employee.salary) {
                            employee.salary = rank.salary;
                            Logger.logWarning(LogCategory.ServerStartup, LogActionType.Updated, $"Found error in employee salary: Updated salary of employee {employee.id} to {rank.salary}");
                        }
                    }

                    company.init();
                }


                db.SaveChanges();
            }

            AllCompaniesLoadedDelegate?.Invoke();
        }

        public static Company findCompany(Predicate<Company> predicate) {
            return AllCompanies.Values.FirstOrDefault(c => predicate(c));
        }

        public static List<Company> findCompanies(Predicate<Company> predicate) {
            return AllCompanies.Values.Where(c => predicate(c)).ToList();
        }

        public static Company findCompanyById(int id) {
            if(AllCompanies.ContainsKey(id)) {
                return AllCompanies[id];
            } else {
                return null;
            }
        }

        public static List<T> findCompaniesByType<T>(Predicate<T> predicate) {
            return AllCompanies.Values.OfType<T>().Where(t => predicate(t)).ToList();
        }

        public static Company createNewCompany(IPlayer player, string name, string shortName, string abbreviation, string logo, CompanyType type, string codeCompany, Position position, int? bankAccount) {
            Company comp = null;
            var typ = Type.GetType("ChoiceVServer.Controller.Companies.Company");

            using(var dbFs = new ChoiceVFsDb())
            using(var db = new ChoiceVDb()) {
                var dbComp = new configcompany {
                    name = name,
                    shortName = shortName,
                    companyType = (int)type,
                    buildingType = (int)BuildingType.Tiny,
                    companyBankAccount = bankAccount,
                    position = position.ToJson(),
                    reputation = 0,
                    riskLevel = 0,
                    maxEmployees = 1,
                };

                db.configcompanies.Add(dbComp);
                db.SaveChanges();

                var system = new system {
                    name = name,
                    shortName = shortName,
                    abbreviation = abbreviation,
                    logo = logo,
                    companyId = dbComp.id,
                    color = "#6f7408",
                    companyType = (int)type

                };

                dbFs.systems.Add(system);

                dbFs.SaveChanges();

                var blBTab = dbFs.tabs.Find("companies/bulletinBoard");
                var raLTab = dbFs.tabs.Find("companies/ranksList");
                var emLTab = dbFs.tabs.Find("companies/employeesList");

                system.tabsNames.Add(blBTab);
                system.tabsNames.Add(raLTab);
                system.tabsNames.Add(emLTab);

                dbFs.permission_ranks.Add(new permission_rank {
                    systemId = system.id,
                    rankName = "Angestellter",
                    groupName = "NONE",
                    clearanceLevel = 0,
                    isStandard = 1,
                    isCEO = 0,
                    salary = 0,
                });

                dbFs.permission_ranks.Add(new permission_rank {
                    systemId = system.id,
                    rankName = "Inhaber",
                    groupName = "NONE",
                    clearanceLevel = 0,
                    isStandard = 0,
                    isCEO = 1,
                    salary = 0,
                });

                dbFs.SaveChanges();

                comp = Activator.CreateInstance(typ, dbComp, system, new List<configcompanysetting>()) as Company;

                AllCompanies.Add(dbComp.id, comp);
            }

            var empl = comp.hireEmployee(player, player.getMainBankAccount(), player.getMainPhoneNumber(), 0, true);

            player.sendNotification(Constants.NotifactionTypes.Success, "Firma wurde erfolgreich erstellt!", "Firma erstellt", Constants.NotifactionImages.Company);
            return comp;
        }

        public static void addCompanyInteractionPermissionElement(CompanyPermissionElement element) {
            AllCompanyPermissionInteractionElements.Add(element);
        }

        public static bool isInCompany(IPlayer player) {
            return player.hasData("PLAYER_COMPANIES");
        }

        public static bool isInCompany(IPlayer player, Company company) {
            return company.hasEmployee(player.getCharacterId());
        }

        public static bool isInDuty(IPlayer player) {
            var companies = getCompanies(player);

            return companies is not null && companies
                .Select(company => company.findEmployee(player.getCharacterId()))
                .Where(employee => employee is not null)
                .Any(employee => employee.InDuty);
        }

        public static bool isInDuty(IPlayer player, Company company) {
            var employee = company.findEmployee(player.getCharacterId());

            return employee is not null && employee.InDuty;

        }

        public static List<Company> getCompanies(IPlayer player) {
            return player.hasData("PLAYER_COMPANIES")
                ? (List<Company>)player.getData("PLAYER_COMPANIES")
                : new List<Company> { };
        }
        
        public static bool hasPlayerCompanyWithPredicate(IPlayer player, Predicate<Company> predicate) {
            return getCompanies(player).Any(c => predicate(c));
        }

        public static List<Company> getCompaniesWithFunctionality<T>() {
            return AllCompanies.Values.Where(c => c.hasFunctionality<T>()).ToList();
        }
        
        public static List<Company> getCompaniesWithFuncationality<T>(Predicate<T> predicate) {
            return AllCompanies.Values.Where(c => c.hasFunctionality<T>(predicate)).ToList();
        }

        public static Company getCompanyById(int id) {
            return AllCompanies.GetValueOrDefault(id);
        }

        public static bool getPlayerInCompanyWithFunctionality<T>(IPlayer player) {
            return getCompanies(player).Any(c => c.hasFunctionality<T>());
        }

        private static HashSet<CompanyPermission> getPlayerPermissionsFromDb(IPlayer player, Company company) {
            var set = new HashSet<CompanyPermission>();
            var charId = player.getCharacterId();
            using(var dbFs = new ChoiceVFsDb()) {
                var rank = dbFs.permission_ranks
                            .Include(r => r.system)
                            .Include(r => r.permission_users_to_ranks)
                           .FirstOrDefault(r => r.system.companyId == company.Id && r.permission_users_to_ranks.Any(utr => !utr.wasFired && utr.userId == charId));

                if(rank != null) {
                    var rankId = rank.id;
                    var companyType = rank.system.companyType;
                    if(rank.isCEO == 1) {
                        set = dbFs.permission_single_permissions
                            .Where(sp => sp.type == "ALL" || company.getViablePermissions().Any(p => p == sp.identifier)).Select(sp => new CompanyPermission(sp.identifier)).ToHashSet();
                    } else {
                        set = dbFs.permission_single_permissions
                            .Include(sp => sp.ranks)
                        .Where(sp => (sp.type == "ALL" || company.getViablePermissions().Any(p => p == sp.identifier)) && sp.ranks.Any(sptr => sptr.id == rankId)).Select(sp => new CompanyPermission(sp.identifier)).ToHashSet();
                    }
                }
            }

            return set;
        }

        ///Only use if a single check for a player permission is needed. Otherwise use getPlayerPermissions and search in there
        private static HashSet<CompanyPermission> getPlayerPermissions(IPlayer player, int companyId) {
            HashSet<CompanyPermission> perms;

            if(player.hasData($"UPDATE_COMPANY_PERMISSIONS_{companyId}")) {
                perms = getPlayerPermissionsFromDb(player, getCompanyById(companyId));
                player.setData($"COMPANY_PERMISSIONS_{companyId}", perms);
                player.resetData($"UPDATE_COMPANY_PERMISSIONS_{companyId}");
            } else {
                if(getCompanies(player).All(c => c.Id != companyId)) {
                    return [];
                }
                
                if(player.hasData($"COMPANY_PERMISSIONS_{companyId}")) {
                    perms = (HashSet<CompanyPermission>)player.getData($"COMPANY_PERMISSIONS_{companyId}");
                } else {
                    perms = getPlayerPermissionsFromDb(player, getCompanyById(companyId));
                    player.setData($"COMPANY_PERMISSIONS_{companyId}", perms);
                }
            }

            return perms;
        }

        /// <summary>
        /// Important! Only use when you want to know if the player in some company has that permission!
        /// If the company context is known use the other variant
        /// </summary>
        public static bool hasPlayerPermission(IPlayer player, string permissionIdentifier, int companyId) {
            return getPlayerPermissions(player, companyId).Any(c => c.Identifier == permissionIdentifier);
        }

        public static bool hasPlayerPermission(IPlayer player, Company company, string permissionIdentifier) {
            return company is not null && getPlayerPermissions(player, company.Id).Any(c => c.Identifier == permissionIdentifier);
        }
        
        public static List<Company> getCompaniesWithPermission(IPlayer player, string permissionIdentifier) {
            return getCompanies(player).Where(c => hasPlayerPermission(player, c, permissionIdentifier)).ToList();
        }

        public static bool hasPlayerPermission(IPlayer player, string permissionIdentifier) {
            return getCompanies(player).Any(c => hasPlayerPermission(player, c, permissionIdentifier));
        }

        public static List<string> getCompanyDocuments(Company company) {
            throw new NotImplementedException();
        }
        
        public static Byte[] getCompanyDocument(Company company, string fileName) {
            throw new NotImplementedException();
        } 

        public static int getAmountOfPlayerInDuty(Type companyType) {
            var comps = AllCompanies.Values.Where(c => c.GetType() == companyType).ToList();

            var counter = 0;
            comps.ForEach(c => counter += c.getEmployees(e => e.InDuty).Count());

            return counter;
        }

        public static List<companylog> getCompanyLogs(Company company, Predicate<companylog> predicate) {
            try {
                using(var db = new ChoiceVDb()) {
                    return db.companylogs.Where(c => c.companyId == company.Id && predicate(c)).ToList();
                }
            } catch(Exception e) {
                Logger.logException(e);
                return null;
            }
        }

        private void onPlayerConnect(IPlayer player, character character) {
            updatePlayerCompanies(player);
        }

        private static void updatePlayerCompanies(IPlayer player) {
            var companies = AllCompanies.Values.Where(c => c.hasEmployee(player.getCharacterId())).ToList();

            if(companies != null && companies.Count > 0) {
                player.setData("PLAYER_COMPANIES", companies);
            } else {
                player.resetData("PLAYER_COMPANIES");
            }
        }

        private bool onFsRankUpdate(IPlayer player, string eventName, object[] args) {
            var systemId = int.Parse(args[0].ToString());
            var rankId = int.Parse(args[1].ToString());

            var company = AllCompanies.Values.FirstOrDefault(c => c.FsSystemId == systemId);

            using(var db = new ChoiceVDb())
            using(var dbFs = new ChoiceVFsDb()) {
                var charsWithRank = dbFs.permission_users_to_ranks.Include(utr => utr.rank).Where(r => r.rankId == rankId).ToList();

                foreach(var ch in charsWithRank) {
                    var employee = company.findEmployee(ch.userId);

                    if(employee != null) {
                        if(!ch.wasFired) {
                            var ingame = ChoiceVAPI.getClientByCharacterId(employee.CharacterId);
                            ingame?.setData($"UPDATE_COMPANY_PERMISSIONS_{company.Id}", true);
                        }
                    }

                    var dbEmployee = db.companyemployees.FirstOrDefault(ce => ce.id == employee.Id);

                    if(dbEmployee != null) {
                        dbEmployee.salary = ch.rank.salary;
                    }
                }

                db.SaveChanges();

                Logger.logDebug(LogCategory.Player, LogActionType.Updated, player, $"Updated rank of company {company.Id} with fsRankId {rankId}. Triggered update of salary and permissions for {charsWithRank.Count} employees (fired ones included)");
            }

            return true;
        }

        private bool onFsEmployeeUpdate(IPlayer player, string eventName, object[] args) {
            var systemId = int.Parse(args[0].ToString());
            var charId = int.Parse(args[1].ToString());

            var company = AllCompanies.Values.FirstOrDefault(c => c.FsSystemId == systemId);

            using(var db = new ChoiceVDb()) {
                var dbEmployee = db.companyemployees.FirstOrDefault(ce => ce.companyId == company.Id && ce.charId == charId);

                if(dbEmployee != null) {
                    var employee = company.findEmployee(charId);
                    if(employee != null) {
                        employee.PhoneNumber = dbEmployee.selectedPhoneNumer;
                        employee.SelectedBankAccount = dbEmployee.selectedBankAccount;

                        Logger.logDebug(LogCategory.Player, LogActionType.Updated, player, $"Updated rank of company {company.Id} with employeeId {employee.Id}. Triggered update of bankaccount and phonenumber");
                    }
                }
            }

            return true;
        }

        #region MenuEvents

        private Menu companyAdminOptionGenerator(IPlayer player) {
            var menu = new Menu("Admin-Firmeneinstellungen", "Welche Firma?");
            foreach(var company in AllCompanies.Values) {
                var subMenu = company.getCompanyAdminMenu(player);

                var functionalityMenu = new Menu("Funktionalitäten editieren", "Was möchtest du tun?");

                var addFunctionalityMenu = new Menu("Funktionalität hinzufügen", "Welche Funktionalität?");

                var functionalityTypes = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => !t.IsAbstract && t.IsClass && typeof(CompanyFunctionality).IsAssignableFrom(t));

                foreach(var type in functionalityTypes) {
                    var instance = (CompanyFunctionality)Activator.CreateInstance(type);

                    if(!company.hasFunctionality(instance.getIdentifier())) {
                        var info = instance.getSelectionInfo();
                        addFunctionalityMenu.addMenuItem(new ClickMenuItem(info.Name, info.Description, "", "ADD_COMPANY_FUNCTIONALITY").withData(new Dictionary<string, dynamic> { { "Company", company }, { "Type", type }, { "Identifier", info.Identifier } }).needsConfirmation($"{info.Name} hinzufügen?", "Funktionalität wirklich hinzufügen?"));
                    }
                }

                functionalityMenu.addMenuItem(new MenuMenuItem(addFunctionalityMenu.Name, addFunctionalityMenu, MenuItemStyle.green));

                foreach(var functionality in company.Functionalities) {
                    var info = functionality.getSelectionInfo();
                    functionalityMenu.addMenuItem(new ClickMenuItem($"{info.Name} entfernen", info.Description, "", "REMOVE_COMPANY_FUNCTIONALITY", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "Company", company }, { "Identifier", info.Identifier }, { "Functionality", functionality } }).needsConfirmation($"{info.Name} hinzufügen?", "Funktionalität wirklich hinzufügen?"));
                }

                subMenu.addMenuItem(new MenuMenuItem(functionalityMenu.Name, functionalityMenu));

                menu.addMenuItem(new MenuMenuItem(company.Name, subMenu));
            }

            return menu;
        }

        private bool onCompanySettingsAdminMenu(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var companies = getCompanies(player);

            if(companies != null) {
                var menu = new Menu("Admin-Firmeneinstellungen", "Welche Firma?");
                foreach(var company in companies) {
                    var subMenu = company.getCompanyAdminMenu(player);
                    menu.addMenuItem(new MenuMenuItem(company.Name, subMenu));
                }

                player.showMenu(menu);
            }

            return true;
        }

        private Menu companyInteractionGenerator(IEntity sender, IEntity targetE) {
            var player = sender as IPlayer;

            var companies = getCompanies(player);
            var menu = new Menu("Firmen-Menü", "Welche Firma?");
            foreach(var company in companies) {
                var subMenu = new Menu(company.Name, "Was möchtest du tun?");
                foreach(var element in company.getCompanyInteractMenuElements(player, targetE, getPlayerPermissions(player, company.Id), false)) {
                    if(element != null) {
                        if(element is Menu) {
                            var men = element as Menu;
                            subMenu.addMenuItem(new MenuMenuItem(men.Name, men));
                        } else {
                            subMenu.addMenuItem(element as MenuItem);
                        }
                    }
                }

                if (targetE is IPlayer target) {
                    foreach (var element in AllCompanyPermissionInteractionElements) {
                        if (element.CompanyPredicate(company) && element.TargetPredicate(target) && hasPlayerPermission(player, company, element.Permission.Identifier)) {
                            //Add Data Elements for MenuItems
                            if (element.MenuItem.Data != null) {
                                element.MenuItem.Data["Company"] = company;
                                element.MenuItem.Data["TargetId"] = target.getCharacterId();
                            } else {
                                var subData = new Dictionary<string, dynamic> {
                                    {"Company", company },
                                    {"TargetId", target.getCharacterId() },
                                };

                                element.MenuItem.Data = subData;
                            }
                            subMenu.addMenuItem(element.MenuItem);
                        }
                    }
                }


                if (subMenu.getMenuItemCount() > 0) {
                    menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
                }
            }

            if(menu.getMenuItemCount() > 0) {
                if(menu.getMenuItemCount() == 1) {
                   return (menu.getMenuItemByIndex(0) as MenuMenuItem).SubMenu;
                } else {
                    return menu;
                }
                return menu;
            } else {
                return null;
            }
        }

        private Menu companySelfMenuGenerator(IPlayer player) {
            var menu = new Menu("Firmenaktionen", "Welche Firma?");

            var companies = getCompanies(player);
            foreach(var company in companies) {
                var els = company.getCompanyInteractMenuElements(player, player, getPlayerPermissions(player, company.Id), true);
                if(els.Count > 0) {
                    var subMenu = new Menu(company.Name, "Was möchtest du tun?");
                    foreach(var element in els) {
                        if(element != null) {
                            if(element is Menu) {
                                var men = element as Menu;
                                subMenu.addMenuItem(new MenuMenuItem(men.Name, men));
                            } else {
                                subMenu.addMenuItem(element as MenuItem);
                            }
                        }
                    }

                    menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
                }
            }

            if(menu.getMenuItemCount() == 0) {
                menu.addMenuItem(new StaticMenuItem("Keine Aktionen verfügbar", "Es sind keine Aktionen verfügbar", "", MenuItemStyle.yellow));
            }

            return menu;
        }

        private void onCompanyHireEmployee(Company company, CompanyEmployee newEmployee, IPlayer player, bool asCeo) {
            var charId = player.getCharacterId();

            using(var dbFs = new ChoiceVFsDb())
            using(var db = new ChoiceVDb()) {
                var dbEmployee = db.companyemployees.FirstOrDefault(e => e.companyId == company.Id && e.charId == player.getCharacterId());
                if(dbEmployee == null) {
                    dbEmployee = new companyemployee {
                        charId = charId,
                        charName = newEmployee.CharacterName,
                        companyId = company.Id,
                        selectedBankAccount = newEmployee.SelectedBankAccount,
                    };

                    db.companyemployees.Add(dbEmployee);
                } else {
                    dbEmployee.fired = false;
                }


                var already = dbFs.employees.FirstOrDefault(e => e.systemId == company.FsSystemId && e.charId == charId);
                if(already != null) {
                    already.status = "Angestellt";
                    already.fired = 0;
                    already.statusChangeDate = DateTime.Now.ToString("yyyy-M-d");
                } else {
                    dbFs.employees.Add(new employee {
                        systemId = company.FsSystemId,
                        charId = charId,
                        memberNumber = "0",
                        gender = "Einzutragen",
                        name = newEmployee.CharacterName,
                        birthdate = player.getCharacterData().DateOfBirth.ToString("yyyy-MM-dd"),
                        bankAccount = newEmployee.SelectedBankAccount,
                        socialSecurityNumber = player.getSocialSecurityNumber(),
                        driversLicense = 0,
                        phoneNumber = newEmployee.PhoneNumber,
                        createDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        statusChangeDate = "",
                        status = "Angestellt",
                        trainingInfo = "",
                        info = ""
                    });
                }

                var alreadyRank = dbFs.permission_users_to_ranks.Include(putr => putr.rank).FirstOrDefault(r => r.userId == charId && r.rank.systemId == company.FsSystemId);
                if(alreadyRank != null) {
                    dbFs.permission_users_to_ranks.Remove(alreadyRank);
                }

                dbFs.permission_users_to_ranks.Add(new permission_users_to_rank {
                    userId = charId,
                    rankId = asCeo ? company.FsCEORankId : company.FsStandardRankId,
                });

                if(player != null) {
                    updatePlayerCompanies(player);
                }

                db.SaveChanges();
                dbFs.SaveChanges();

                newEmployee.Id = dbEmployee.id;
            }
        }

        private bool onFireEmployee(IPlayer player, string eventName, object[] args) {
            var systemId = int.Parse(args[0].ToString());
            var charId = int.Parse(args[1].ToString());

            var company = CompanyController.findCompany(c => c.FsSystemId == systemId);

            if(hasPlayerPermission(player, company, "FIRE_EMPLOYEE")) {
                var employee = company.findEmployee(charId);
                if(employee != null) {
                    var menu = MenuController.getConfirmationMenu($"{employee.CharacterName} entlassen?", "Angestellten wirklich entlassen?", "FIRE_EMPLOYEE_SUBMIT", new Dictionary<string, dynamic> { { "Company", company }, { "Employee", employee } });
                    player.showMenu(menu);
                }
            }

            return true;
        }

        private bool onFireEmployeeSubmit(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var employee = data["Employee"];
            var company = data["Company"];


            onCompanyFireEmployee(company, employee);

            player.sendNotification(NotifactionTypes.Info, "Die Person wurde erfolgreich entlassen!", "Person entlassen", NotifactionImages.Company);

            return true;
        }

        private void onCompanyFireEmployee(Company company, CompanyEmployee fireEmployee, bool fireTraceless = false) {
            using(var dbFs = new ChoiceVFsDb())
            using(var db = new ChoiceVDb()) {
                var dbEmployee = db.companyemployees.Find(fireEmployee.Id);

                if(dbEmployee != null) {
                    if(fireTraceless) {
                        db.companyemployees.Remove(dbEmployee);
                    } else {
                        dbEmployee.fired = true;
                    };
                } else {
                    Logger.logWarning(LogCategory.System, LogActionType.Blocked, $"onCompanyFireEmployee: employee which was fired was not in the db! employee: {fireEmployee.ToJson()}");
                }

                db.SaveChanges();

                var dbFsEmployee = dbFs.employees.Find(company.FsSystemId, fireEmployee.CharacterId);
                if(dbFsEmployee != null) {
                    if(!fireTraceless) {
                        dbFsEmployee.status = "Entlassen";
                        dbFsEmployee.fired = 1;
                        dbFsEmployee.statusChangeDate = DateTime.Now.ToString("yyyy-M-d");
                    } else {
                        dbFs.employees.Remove(dbFsEmployee);                        
                    }
                }


                var rank = dbFs.permission_users_to_ranks.Include(putr => putr.rank).FirstOrDefault(r => r.userId == fireEmployee.CharacterId && r.rank.systemId == company.FsSystemId);
                if(rank != null) {
                    if(!fireTraceless) {
                        rank.wasFired = true;
                    } else {
                        dbFs.permission_users_to_ranks.Remove(rank);
                    }
                }

                dbFs.SaveChanges();
            }

            company.fireEmployee(fireEmployee);

            var player = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.getCharacterId() == fireEmployee.CharacterId);
            if(player != null) {
                updatePlayerCompanies(player);

                player.setData($"UPDATE_COMPANY_PERMISSIONS_{company.Id}", true);
            }

        }

        private void onCompanyLog(Company company, string logKey, string logMessage) {
            try {
                using(var db = new ChoiceVDb()) {
                    var newLog = new companylog {
                        companyId = company.Id,
                        logDate = DateTime.Now,
                        logKey = logKey,
                        logMessage = logMessage,
                    };

                    db.companylogs.Add(newLog);

                    db.SaveChanges();
                }
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        private void onCompanyDataChange(Company company, string dataName, dynamic dataValue) {
            try {
                using(var db = new ChoiceVDb()) {
                    var data = db.companydata.Find(company.Id, dataName);
                    if(data != null) {
                        data.dataValue = ((object)dataValue).ToJson();
                    } else {
                        var dbData = new companydatum {
                            companyId = company.Id,
                            dataName = dataName,
                            dataValue = ((object)dataValue).ToJson(),
                        };

                        db.companydata.Add(dbData);

                        Logger.logDebug(LogCategory.System, LogActionType.Updated, $"onCompanyDataChange: data initial save: companyId: {company.Id}, dataName: {dataName}!");
                    }

                    db.SaveChanges();
                }
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        private bool onCompanyDataRemove(Company company, string dataName) {
            try {
                using(var db = new ChoiceVDb()) {
                    var data = db.companydata.Find(company.Id, dataName);
                    if(data != null) {
                        db.companydata.Remove(data);

                        db.SaveChanges();

                        return true;
                    } else {
                        return false;
                    }
                }
            } catch(Exception e) {
                Logger.logException(e);
                return false;
            }
        }

        private bool onAddCompanyFunctionality(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var company = (Company)data["Company"];
            var identifier = (string)data["Identifier"];
            var type = (Type)data["Type"];

            using(var db = new ChoiceVDb()) {
                var newFunctionality = new configcompanyfunctionality {
                    companyId = company.Id,
                    identifier = identifier,
                };

                db.configcompanyfunctionalities.Add(newFunctionality);
                var functionality = (CompanyFunctionality)Activator.CreateInstance(type, company);

                foreach(var permission in functionality.getSinglePermissionsGranted()) {
                    var newEntry = new configcompanyavailablepermission {
                        companyId = company.Id,
                        permission = permission
                    };

                    db.configcompanyavailablepermissions.Add(newEntry);
                }

                company.addFunctionality(functionality);

                db.SaveChanges();
            }

            player.sendNotification(NotifactionTypes.Success, $"Die Funktionalität wurde erfolgreich hinzugefügt!", "Funktionalität hinzugefügt", NotifactionImages.Company);

            return true;
        }

        private bool onRemoveCompanyFunctionality(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var company = (Company)data["Company"];
            var identifier = (string)data["Identifier"];
            var functionality = (CompanyFunctionality)data["Functionality"];

            using(var db = new ChoiceVDb()) {
                var already = db.configcompanyfunctionalities.FirstOrDefault(cf => cf.companyId == company.Id && cf.identifier == identifier);

                db.configcompanyfunctionalities.Remove(already);
                foreach(var permission in functionality.getSinglePermissionsGranted()) {
                    db.configcompanyavailablepermissions.Remove(db.configcompanyavailablepermissions.FirstOrDefault(cap => cap.companyId == company.Id && cap.permission == permission));
                }

                db.SaveChanges();

                company.removeFunctionality(identifier);
            }

            player.sendNotification(NotifactionTypes.Warning, $"Die Funktionalität wurde erfolgreich entfernt!", "Funktionalität entfernt", NotifactionImages.Company);

            return true;
        }

        #endregion

        private bool onCompanyMenuElementSelect(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            foreach(var company in AllCompanies.Values) {
                if(company.callMenuElement(player, data, menuItemCefEvent)) {
                    return true;
                }
            }

            return false;
        }

        private void onCompanyEnterDuty(Company company, CompanyEmployee employee) {
            using(var db = new ChoiceVDb()) {
                var dbEmployee = db.companyemployees.Include(ce => ce.companyemployeesduties).FirstOrDefault(e => e.id == employee.Id);

                if(dbEmployee != null) {
                    dbEmployee.currentlyInDuty = 1;

                    var already = dbEmployee.companyemployeesduties.FirstOrDefault(ed => ed.dutyEnd == null);
                    if(already == null) {
                        var newDuty = new companyemployeesduty {
                            employeeId = employee.Id,
                            successfullyTransfered = false,
                            dutyStart = DateTime.Now,
                            dutyEnd = null,
                            earnedMoney = null,
                            transferedBankAccount = null,
                        };

                        dbEmployee.companyemployeesduties.Add(newDuty);
                    }

                    db.SaveChanges();
                }
            }
        }

        private void onCompanyExitDuty(Company company, CompanyEmployee employee) {
            using(var db = new ChoiceVDb()) {
                var dbEmployee = db.companyemployees.Include(ce => ce.companyemployeesduties).FirstOrDefault(e => e.id == employee.Id);

                if(dbEmployee != null) {
                    dbEmployee.currentlyInDuty = 0;

                    var current = dbEmployee.companyemployeesduties.FirstOrDefault(ed => ed.dutyEnd == null);
                    if(current != null) {
                        current.dutyEnd = DateTime.Now;
                    }

                    employee.UnpaidDuties.Add(new CompanyEmployeeUnpayedDuty(current.id, current.dutyStart, current.dutyEnd ?? current.dutyStart));

                    db.SaveChanges();
                }

            }
        }

        private void setEmployeesToNotHaveDuty() {
            using(var db = new ChoiceVDb()) {
                foreach(var employee in db.companyemployees.Include(ce => ce.companyemployeesduties)) {
                    employee.currentlyInDuty = 0;

                    var current = employee.companyemployeesduties.FirstOrDefault(ed => ed.dutyEnd == null);
                    if(current != null) {
                        current.dutyEnd = DateTime.Now;
                    }
                }

                db.SaveChanges();
            }
        }
    }
}
