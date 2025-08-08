using AltV.Net.Elements.Entities;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.FsDatabase;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Companies;

public enum BuildingType {
    Tiny = 0, //Only 1 Employee
    Small = 1, //Up to 3 Employees
    Medium = 2, //Up to 6 Employees
    Large = 3, //Up to 10 Employees
    Huge = 4 //Unlimited Employees
}

public enum CompanyType {
    Standard = 0,
    Police = 1,
    Medic = 2,
    Fire = 3,
    Merryweather = 4,
    Fbi = 5,
    Gang = 6,
    Workshop = 7,
    Gasstation = 8,
    Sheriff = 9,
    Garage = 10,
    CityAdministration = 11,
    Shipping = 12,
    Tattoo = 13,
    Prison = 14,
    CarShop = 15,
    Tuning = 16,
    CarColoring = 17,
}

public delegate void CompanyHireEmployeeDelegate(Company company, CompanyEmployee newEmployee, IPlayer player, bool asCeo);
public delegate void CompanyFireEmployeeDelegate(Company company, CompanyEmployee fireEmployee, bool fireTraceLess);
public delegate void CompanyDataChangeDelegate(Company company, string dataName, dynamic dataValue);
public delegate bool CompanyDataRemoveDelegate(Company company, string dataName);

public delegate void CompanyLogDelegate(Company company, string logKey, string logMessage);
public delegate configcompanysetting CompanyChangeSettingDelegate(Company company, string name, string value, string category);
public delegate void CompanyDeleteSettingDelegate(Company company, string name, string category);
public delegate MenuElement GeneratedCompanyAdminMenuElementDelegate(IPlayer player);
public delegate MenuElement GeneratedCompanyMenuElementDelegate(IPlayer player, IPlayer target);
public delegate MenuElement GeneratedCompanyVehicleMenuElementDelegate(IPlayer player, ChoiceVVehicle target);
public delegate MenuElement GeneratedCompanySelfMenuElementDelegate(Company company, IPlayer player);
public delegate void CompanyEmployeeEnterDuty(Company company, CompanyEmployee employee);
public delegate void CompanyEmployeeExitDuty(Company company, CompanyEmployee employee);

public class Company {
    private static TimeSpan MAX_COMPANY_PAID_TIMESPAN = TimeSpan.FromHours(2);

    private static int CompanyMenuElementId;

    public static CompanyHireEmployeeDelegate CompanyHireEmployeeDelegate;
    public static CompanyFireEmployeeDelegate CompanyFireEmployeeDelegate;

    public static CompanyDataChangeDelegate CompanyDataChangeDelegate;
    public static CompanyDataRemoveDelegate CompanyDataRemoveDelegate;

    public static CompanyLogDelegate CompanyLogDelegate;

    public static CompanyChangeSettingDelegate CompanyChangeSettingDelegate;
    public static CompanyDeleteSettingDelegate CompanyDeleteSettingDelegate;

    public static CompanyEmployeeEnterDuty CompanyEmployeeEnterDuty;
    public static CompanyEmployeeExitDuty CompanyEmployeeExitDuty;

    private readonly SortedDictionary<string, CompanyMenuElement> AllMenuElements;

    

    public int Id { get; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string CityName { get; set; }
    public string StreetName { get; set; }
    //public CompanyType Type { get; set; }
    public int MaxEmployees { get; set; }
    public long CompanyBankAccount { get; set; }
    public List<CompanyEmployee> Employees { get; private set; }
    public CompanyTax CompanyTax { get; }

    public Dictionary<string, List<configcompanysetting>> AllSettings { get; }

    public int Reputation { get; }
    public int RiskLevel { get; }

    public List<int> Vehicles { get; private set; }

    public List<int> BuyableMetroCategories { get; private set; }

    //FS Stuff
    public int FsSystemId { get; }
    public int FsStandardRankId { get; }
    public int FsCEORankId { get; }
    
    public BuildingType BuildingType;

    public CompanyType CompanyType;

    public ExtendedDictionary<string, dynamic> Data;

    protected List<InventorySpot> ItemsWarehouse;

    public List<CompanyModule> Modules;

    public Vector3 Position; //For e.g Blip
    public InventorySpot Safe;

    public bool IsSubCompany = false;

    public List<CompanyFunctionality> Functionalities = new();

    public Company(configcompany dbComp, system fsSystem, List<configcompanysetting> allSettings) {
        Id = dbComp.id;
        Name = dbComp.name;
        ShortName = dbComp.shortName;
        CityName = dbComp.companyCity;
        StreetName = dbComp.companyStreet;
        MaxEmployees = dbComp.maxEmployees;
        Reputation = dbComp.reputation;
        RiskLevel = dbComp.riskLevel;
        Position = dbComp.position.FromJson();
        CompanyBankAccount = dbComp.companyBankAccount ?? -1;

        CompanyType = (CompanyType)dbComp.companyType;

        //Load Employees from Database
        Employees = new List<CompanyEmployee>();
        foreach(var employee in dbComp.companyemployees.Where(e => !e.fired)) {
            Employees.Add(new CompanyEmployee(employee));
        }

        CompanyTax = new CompanyTax(dbComp.companyTax ?? 0);

        //Load Settings from Database
        AllSettings = new Dictionary<string, List<configcompanysetting>>();
        foreach(var setting in allSettings) {
            if(AllSettings.ContainsKey(setting.settingsCategory)) {
                var list = AllSettings[setting.settingsCategory];
                list.Add(setting);
            } else {
                var list = new List<configcompanysetting> {
                    setting
                };

                AllSettings[setting.settingsCategory] = list;
            }
        }

        //Load CompanyData from Database
        if(dbComp.companydata != null) {
            var dic = new Dictionary<string, dynamic>();
            foreach(var row in dbComp.companydata) {
                dic[row.dataName] = row.dataValue.FromJson<dynamic>();
            }

            Data = new ExtendedDictionary<string, dynamic>(dic);
            Data.OnValueChanged += (key, value) => {
                CompanyDataChangeDelegate?.Invoke(this, key, value);
            };

            Data.OnValueRemoved += (key) => {
                if(CompanyDataRemoveDelegate != null) {
                    return CompanyDataRemoveDelegate.Invoke(this, key);
                } else {
                    return false;
                }

            };
        }

        if(!Data.Items.ContainsKey("NextPayDate")) {
            Data["NextPayDate"] = DateTime.Now + TimeSpan.FromHours(24);
        }

        AllMenuElements = [];

        //Filesystem Stuff
        if(fsSystem != null) {
            FsSystemId = fsSystem.id;
            FsStandardRankId = fsSystem.permission_ranks.FirstOrDefault(r => r.isStandard == 1).id;
            FsCEORankId = fsSystem.permission_ranks.FirstOrDefault(r => r.isCEO == 1).id;
        }

        #region Self-Menu Stuff

        registerCompanySelfElement("COMPANY_DUTY",
            getCompanyDutyGenerator,
            onCompanyDuty
        );

        foreach(var externalRegister in CompanyExternalCompanyInteractController.getAllExternalCompanySelfMenuRegisters()) {
            registerCompanySelfElement(externalRegister.Identifier,
                externalRegister.Generator,
                externalRegister.Callback,
                externalRegister.PermissionRequirement
            );
        }

        #endregion

        #region Company Interaction Stuff

        registerCompanyInteractElement(
            "ZZ_COMPANY_HIRE_EMPLOYEE_INTERACTION",
            getCompanyHireEmployeeInteractionMenu,
            onCompanyHireEmployee,
            "HIRE_EMPLOYEE"
        );

        #endregion

        #region Admin Create Stuff

        var itemsWarehouseIds = getSettings("COMPANY_ITEM_SPOTS").Select(si => int.Parse(si.settingsValue)).ToList();
        ItemsWarehouse = new();

        foreach(var id in itemsWarehouseIds) {
            ItemsWarehouse.Add(InventorySpot.getById(id));
        }

        var safeId = getSetting<int>("COMPANY_SAFE_SPOT");
        Safe = InventorySpot.getById(safeId);
        if(Safe != null) {
            Safe.setOpenPredicate(
                p => CompanyController.hasPlayerPermission(p, this, "SAFE_ACCESS"),
                "Du hast keinen Zugriff auf den Firmensafe!"
            );
        }
        
        registerCompanyAdminElement(
            "SUPPORT_HIRE_FIRE_SELF",
            getCompanySupportHireFireSelfMenuElement,
            onCompanySupportHireFireSelfMenuElementSelect);

        registerCompanyAdminElement(
            "COMPANY_INVENTORY_CREATION",
            getInventorySpotsAdminElement,
            onInventorySpotAdminElementSelect
        );

        registerCompanyAdminElement(
            "COMPANY_LOCKER_CREATION",
            getLockerAdminElement,
            onLockerAdminElementSelect
        );

        #endregion

    }

    public void init() {
        onLastEmployeeLeaveDuty();
    }
    
    /// <summary>
    ///     Gets the respective EmployeeModel Object for a Character
    /// </summary>
    public bool hasEmployee(int characterId) {
        return findEmployee(characterId) != null;
    }

    /// <summary>
    ///     Gets the respective EmployeeModel Object for a Character
    /// </summary>
    public CompanyEmployee findEmployee(int characterId) {
        return Employees.FirstOrDefault(emp => emp.CharacterId == characterId);
    }

    /// <summary>
    ///     Gets Employees by a given predicate
    /// </summary>
    public IEnumerable<CompanyEmployee> getEmployees(Predicate<CompanyEmployee> predicate) {
        return Employees.Where(e => predicate(e));
    }

    /// <summary>
    ///     Hires a Character
    /// </summary>
    /// <param name="jobLevelId">The id of the JobLevel, the character should start in</param>
    /// <param name="salary">The initial salary for the new Employee</param>
    public CompanyEmployee hireEmployee(IPlayer player, long selectedBankAccount, long phoneNumber, int salary = 0, bool hireCEO = false) {
        if(Employees.FirstOrDefault(empl => empl.CharacterId == player.getCharacterId()) != null) {
            return null;
        }

        var newEmployee = new CompanyEmployee(
            player.getCharacterId(),
            player.getCharacterName(),
            salary,
            selectedBankAccount,
            phoneNumber
        );

        Employees.Add(newEmployee);

        CompanyHireEmployeeDelegate?.Invoke(this, newEmployee, player, hireCEO);

        return newEmployee;
    }

    ///// <summary>
    ///// Fires a employee with a given predicate
    ///// </summary>
    //public bool fireEmployee(Predicate<CompanyEmployee> predicate) {
    //    return fireEmployee(Employees.FirstOrDefault(item => (item != null) && predicate(item)));
    //}

    /// <summary>
    /// Fires a specified Employee
    /// </summary>
    public bool fireEmployee(CompanyEmployee employee, bool fireTraceless = false) {
        if(Employees.Remove(employee)) {
            if(employee.InDuty) {
                stopDuty(employee);
            }

            CompanyFireEmployeeDelegate?.Invoke(this, employee, fireTraceless);
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    ///     Start the employee duty
    /// </summary>
    public void startDuty(CompanyEmployee employee) {
        employee.InDuty = true;
        onEmployeeEnterDuty();
        if(Employees.Count(e => e.InDuty) == 1) {
            onFirstEmployeeEnterDuty();
        }
        CompanyEmployeeEnterDuty?.Invoke(this, employee);
    }

    /// <summary>
    ///     Stop the employee duty
    /// </summary>
    public void stopDuty(CompanyEmployee employee) {
        if(employee == null) {
            return;
        }

        employee.InDuty = false;
        onEmployeeLeaveDuty(employee);
        if(Employees.FirstOrDefault(e => e.InDuty) == null) {
            onLastEmployeeLeaveDuty();
        }

        CompanyEmployeeExitDuty?.Invoke(this, employee);
    }

    /// <summary>
    ///     Sets the salary for a specific Employee
    /// </summary>
    public void setEmployeeSalary(CompanyEmployee employee, decimal newSalary) {
        employee.Salary = newSalary;
    }

    /// <summary>
    ///     Logs a message for player use later
    /// </summary>
    public void logMessage(string key, string message) {
        CompanyLogDelegate?.Invoke(this, key, message);
    }

    public void setSetting(string name, string value, string category = "NONE") {
        var sett = CompanyChangeSettingDelegate.Invoke(this, name, value, category);

        if(AllSettings.ContainsKey(category)) {
            var list = AllSettings[category];
            var already = list.FirstOrDefault(s => s.settingsName == name);
            if(already != null) {
                already.settingsValue = value;
            } else {
                list.Add(sett);
            }
        } else {
            var newList = new List<configcompanysetting>();
            newList.Add(sett);
            AllSettings[category] = newList;
        }
    }
    
    
    public bool hasSetting(string name, string category = "NONE") {
        if(AllSettings.ContainsKey(category)) {
            var list = AllSettings[category];
            var already = list.FirstOrDefault(s => s.settingsName == name);
            if(already != null) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    ///     Gets all settings of a specific category
    /// </summary>
    public List<configcompanysetting> getSettings(string settingsCategory) {
        if(AllSettings.ContainsKey(settingsCategory)) {
            return AllSettings[settingsCategory];
        }
        return new List<configcompanysetting>();
    }

    /// <summary>
    ///     Gets a settings without category
    /// </summary>
    public string getSetting(string settingsName) {
        if(AllSettings.ContainsKey("NONE")) {
            var setting = AllSettings["NONE"].FirstOrDefault(s => s.settingsName == settingsName);
            if(setting != null) {
                return setting.settingsValue;
            }
            return null;
        }
        return null;
    }

    /// <summary>
    ///     Gets a settings without category
    /// </summary>
    public T getSetting<T>(string settingsName) {
        if(AllSettings.ContainsKey("NONE")) {
            var setting = AllSettings["NONE"].FirstOrDefault(s => s.settingsName == settingsName);
            if(setting != null) {
                return setting.settingsValue.FromJson<T>();
            }
            return default;
        }
        return default;
    }

    /// <summary>
    ///     Gets a settings without category
    /// </summary>
    public void deleteSetting(string settingsName, string categoryName = "NONE") {
        if(AllSettings.ContainsKey(categoryName)) {
            var settings = AllSettings[categoryName];
            var setting = settings.FirstOrDefault(s => s.settingsName == settingsName);

            if(setting != null) {
                settings.Remove(setting);
                CompanyDeleteSettingDelegate.Invoke(this, settingsName, categoryName);
            }
        }
    }

    /// <summary>
    ///     Gets a settings without category
    /// </summary>
    public void deleteSettings(string categoryName) {
        if(AllSettings.ContainsKey(categoryName)) {
            var settings = AllSettings[categoryName].Reverse<configcompanysetting>();

            foreach(var setting in settings) {
                deleteSetting(setting.settingsName, categoryName);
            }
        }
    }

    public record PaydayDutyUpdate(int DBId, decimal? EarnedMoney, long? TransferedBankAccount, bool SuccessfullyTransfered, string? FailMessage);
    public virtual Dictionary<int, PaydayDutyUpdate> payDay(bankaccount serverAccount) {
        if(Data["NextPayDate"] <= DateTime.Now) {
            var list = new Dictionary<int, PaydayDutyUpdate>();

            foreach(var employee in Employees) {
                //For each day 
                foreach(var day in employee.UnpaidDuties.GroupBy(ud => ud.DutyEnd.Date)) {
                    var totalAmount = TimeSpan.Zero;
                    var date = day.Key;

                    foreach(var duty in day) {
                        totalAmount += duty.DutyEnd - duty.DutyStart;
                    }

                    var companySalary = 0m;
                    var serverSalary = 0m;
                    if(totalAmount <= MAX_COMPANY_PAID_TIMESPAN) {
                        companySalary = Math.Round(employee.Salary * (decimal)totalAmount.TotalHours, 2);
                    } else {
                        companySalary = Math.Round(employee.Salary * (decimal)MAX_COMPANY_PAID_TIMESPAN.TotalHours, 2);
                        var left = (decimal)(totalAmount - MAX_COMPANY_PAID_TIMESPAN).TotalHours;

                        var counter = 0m;
                        while(left - 1 > 0) {
                            serverSalary += employee.Salary * (1 / (counter + 1));
                            left--;
                            counter++;
                        }

                        serverSalary += employee.Salary * (1 / (counter + 1)) * (1 - left);
                    }

                    serverSalary = Math.Round(serverSalary, 2);

                    if(!BankController.transferMoney(CompanyBankAccount, employee.SelectedBankAccount, companySalary, $"Gehalt {date:dd.MM} {Name}", out var returnMessage, false)) {
                        foreach(var duty in day) {
                            list.Add(duty.DbId, new PaydayDutyUpdate(duty.DbId, null, null, false, returnMessage));
                        }
                    } else {
                        foreach(var duty in day) {
                            list.Add(duty.DbId, new PaydayDutyUpdate(duty.DbId, companySalary, employee.SelectedBankAccount, true, null));
                            employee.UnpaidDuties.Remove(duty);
                        }

                        BankController.transferMoney(serverAccount.id, employee.SelectedBankAccount, serverSalary, $"Zusatz-Gehalt {date:dd.MM} {Name}", out var _, false);
                    }
                }
            }

            Data["NextPayDate"] = DateTime.Now + TimeSpan.FromHours(24);

            return list;
        }

        return null;
    }

    public void onEmployeeLeaveDuty(CompanyEmployee employee) { }

    public void onEmployeeEnterDuty() { }
    
    public void onLastEmployeeLeaveDuty() {
        Functionalities.ForEach(f => f.onLastEmployeeLeaveDuty());
    }
    
    public void onFirstEmployeeEnterDuty() {
        Functionalities.ForEach(f => f.onFirstEmployeeEnterDuty());
    }

    public bool callMenuElement(IPlayer player, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var id = (int)data["CompanyMenuElementId"];
        var element = AllMenuElements.Values.FirstOrDefault(e => e.Id == id);
        if(element != null) {
            var subEvent = (string)data["CompanyMenuElementSubEvent"];
            element.Callback.Invoke(this, player, subEvent, data, menuItemCefEvent);
            return true;
        }

        return false;
    }

    public void registerCompanyAdminElement(string identifier, GeneratedCompanyAdminMenuElementDelegate generator, CompanyMenuElementCallbackDelegate callback) {
        AllMenuElements.Add(identifier, new CompanyMenuElement(generator, callback));
    }

    public void registerCompanyInteractElement(string identifier, GeneratedCompanyMenuElementDelegate generator, CompanyMenuElementCallbackDelegate callback, string permissionIdentifier = null) {
        AllMenuElements.Add(identifier, new CompanyMenuElement(generator, callback, permissionIdentifier));
    }

    public void registerCompanyVehicleInteractElement(string identifier, GeneratedCompanyVehicleMenuElementDelegate generator, CompanyMenuElementCallbackDelegate callback, string permissionIdentifier = null) {
        AllMenuElements.Add(identifier, new CompanyMenuElement(generator, callback, permissionIdentifier));
    }

    public void registerCompanySelfElement(string identifier, GeneratedCompanySelfMenuElementDelegate generator, CompanyMenuElementCallbackDelegate callback, string permissionIdentifier = null) {
        AllMenuElements.Add(identifier, new CompanyMenuElement(generator, callback, permissionIdentifier));
    }

    public void unregisterCompanyElement(string identifier) {
        AllMenuElements.Remove(identifier);
    }

    public MenuElement getCompanyMenuElementByIdentifier(string identifier, bool isAdmin, IPlayer player, IPlayer target) {
        if(AllMenuElements.ContainsKey(identifier)) {
            var el = AllMenuElements[identifier];
            if(isAdmin == el.IsAdmin) {
                var menuElement = el.getMenuElement(this, player, target);
                if(menuElement == null) {
                    return null;
                }
                
                el.changeEventForElements(menuElement);

                return menuElement;
            }
        }

        return null;

    }

    public Menu getCompanyAdminMenu(IPlayer player) {
        var menu = new Menu("Adminmenü", "Editiere die Firma");
        
        foreach(var el in AllMenuElements.Values) {
            if(el.IsAdmin) {
                var menuElement = el.getMenuElement(this, player, null);
                el.changeEventForElements(menuElement);

                if(menuElement is Menu) {
                    var subMenu = menuElement as Menu;
                    menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
                } else {
                    var item = menuElement as MenuItem;
                    menu.addMenuItem(item);
                }
            }
        }

        return menu;
    }

    public List<MenuElement> getCompanyInteractMenuElements(IPlayer player, IEntity target, HashSet<CompanyPermission> playerPermissions, bool isSelfInteract) {
        var list = new List<MenuElement>();

        var isVehicle = target is ChoiceVVehicle;

        foreach(var el in AllMenuElements.Values) {
            if(!el.IsAdmin && el.IsSelfMenu == isSelfInteract && (el.IsVehicleMenu == isVehicle) && (el.PermissionIdentifier == null || playerPermissions.Any(p => p.Identifier == el.PermissionIdentifier))) {
                var menuElement = el.getMenuElement(this, player, target);
                if(menuElement == null) {
                    continue;
                }
                
                el.changeEventForElements(menuElement);

                if(menuElement is Menu) {
                    var subMenu = menuElement as Menu;
                    list.Add(new MenuMenuItem(subMenu.Name, subMenu));
                } else {
                    var item = menuElement as MenuItem;
                    list.Add(item);
                }
            }
        }

        return list;
    }

    public MenuMenuItem getCompanyInteractMenuElement(IPlayer target, string identifier) {
        var menuElement = getCompanyMenuElementByIdentifier(identifier, false, null, target);
        
        if(menuElement is MenuItem) {
            return menuElement as MenuMenuItem;
        } else if (menuElement is Menu menu) {
            return new MenuMenuItem(menu.Name, menu);
        } else if (menuElement is VirtualMenu element) {
            return new MenuMenuItem(element.Name, element);
        }

        return null;
    }

    public delegate void CompanyMenuElementCallbackDelegate(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent);

    public class CompanyMenuElement {
        public CompanyMenuElementCallbackDelegate Callback;

        public GeneratedCompanyAdminMenuElementDelegate GeneratorAdmin;

        public GeneratedCompanyMenuElementDelegate GeneratorNormal;
        public GeneratedCompanyVehicleMenuElementDelegate GeneratorVehicle;
        public GeneratedCompanySelfMenuElementDelegate GeneratorSelf;
        public int Id;

        public bool IsAdmin;
        public bool IsSelfMenu;
        public bool IsVehicleMenu;
        public string PermissionIdentifier;

        public CompanyMenuElement(GeneratedCompanyMenuElementDelegate generator, CompanyMenuElementCallbackDelegate callback, string permissionIdentifier = null) {
            Id = CompanyMenuElementId++;
            GeneratorNormal = generator;
            Callback = callback;
            PermissionIdentifier = permissionIdentifier;
            IsAdmin = false;
            IsSelfMenu = false;
            IsVehicleMenu = false;
        }

        public CompanyMenuElement(GeneratedCompanyVehicleMenuElementDelegate generator, CompanyMenuElementCallbackDelegate callback, string permissionIdentifier = null) {
            Id = CompanyMenuElementId++;
            GeneratorVehicle = generator;
            Callback = callback;
            PermissionIdentifier = permissionIdentifier;
            IsAdmin = false;
            IsSelfMenu = false;
            IsVehicleMenu = true;
        }

        public CompanyMenuElement(GeneratedCompanySelfMenuElementDelegate generator, CompanyMenuElementCallbackDelegate callback, string permissionIdentifier = null) {
            Id = CompanyMenuElementId++;
            GeneratorSelf = generator;
            Callback = callback;
            PermissionIdentifier = permissionIdentifier;
            IsAdmin = false;
            IsSelfMenu = true;
            IsVehicleMenu = false;
        }

        public CompanyMenuElement(GeneratedCompanyAdminMenuElementDelegate generator, CompanyMenuElementCallbackDelegate callback) {
            Id = CompanyMenuElementId++;
            GeneratorAdmin = generator;
            Callback = callback;
            IsAdmin = true;
            IsSelfMenu = false;
            IsVehicleMenu = false;
        }

        public MenuElement getMenuElement(Company company, IPlayer player, IEntity target) {
            if(IsAdmin && GeneratorAdmin != null) {
                return GeneratorAdmin.Invoke(player);
            } else if(GeneratorNormal != null) {
                return GeneratorNormal.Invoke(player, (IPlayer)target);
            } else if(GeneratorVehicle != null) {
                return GeneratorVehicle.Invoke(player, (ChoiceVVehicle)target);
            } else {
                return GeneratorSelf.Invoke(company, player);
            }
        }

        public void changeEventForElements(MenuElement element) {
            if(element is MenuMenuItem menuMenuItem) {
                if(menuMenuItem.SubMenu != null) {
                    foreach(var el in menuMenuItem.SubMenu.getMenuItems()) {
                        changeEventForElements(el);
                    }
                } else if(menuMenuItem.VirtualMenu != null) {
                    menuMenuItem.VirtualMenu.addOnGenerateAction((m) => {
                        changeEventForElements(m);
                    });
                }
            } else if(element is Menu) {
                var menu = element as Menu;

                foreach(var el in menu.getMenuItems()) {
                    changeEventForElements(el);
                }
            } else {
                if(element is UseableMenuItem) {
                    var menuItem = element as UseableMenuItem;
                    menuItem.Data["CompanyMenuElementId"] = Id;
                    if(menuItem.Event != "") {
                        if(menuItem.Data.ContainsKey("ConfirmationEvent")) {
                            menuItem.Data["CompanyMenuElementSubEvent"] = menuItem.Data["ConfirmationEvent"];
                            menuItem.Data["ConfirmationEvent"] = "COMPANY_ELEMENT_SELECT";
                        } else {
                            menuItem.Data["CompanyMenuElementSubEvent"] = menuItem.Event;
                            menuItem.Event = "COMPANY_ELEMENT_SELECT";
                        }
                    }
                }
            }
        }
    }

    #region Company Self Menu Stuff

    private MenuElement getCompanyDutyGenerator(Company company, IPlayer player) {
        var employee = findEmployee(player.getCharacterId());

        if(employee != null && employee.InDuty) {
            return new ClickMenuItem("Vom Dienst abmelden", "Melde dich vom Dienst zu dieser Firma ab! Du stempelst dich damit aus", "", "STOP_DUTY");
        } else {
            var companies = CompanyController.getCompanies(player);
            var dutyStr = "Akt.: Keine";

            var comp = companies.FirstOrDefault(c => {
                if(c == this) {
                    return false;
                }
                var employee = c.findEmployee(player.getCharacterId());
                if(employee != null && employee.InDuty) {
                    return true;
                }
                return false;
            });

            if(comp != null) {
                dutyStr = $"Akt.: {comp.Name}";
            }

            return new ClickMenuItem("Zum Dienst melden", "Du meldest dich bei dieser Firma zum Dienst. Du kannst nur in einer Firma gleichzeitig eingestempelt sein!", dutyStr, "START_DUTY").withData(new Dictionary<string, dynamic> { { "CurrentDuty", comp } });
        }
    }

    private void onCompanyDuty(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var employee = findEmployee(player.getCharacterId());

        if(employee != null) {
            if(subEvent == "STOP_DUTY") {
                stopDuty(employee);
                player.sendNotification(Constants.NotifactionTypes.Info, $"Du hast den Dienst bei {Name} beendet!", "Dienst beendet", Constants.NotifactionImages.Company);

                Logger.logDebug(LogCategory.Player, LogActionType.Event, player, $"Player stopped duty at company {Id}");
            } else if(subEvent == "START_DUTY") {
                var current = (Company)data["CurrentDuty"];

                if(current != null) {
                    current.stopDuty(current.findEmployee(player.getCharacterId()));
                    player.sendNotification(Constants.NotifactionTypes.Warning, $"Du hast den Dienst bei {current.Name} beendet!", "Dienst beendet", Constants.NotifactionImages.Company);

                    Logger.logDebug(LogCategory.Player, LogActionType.Event, player, $"Player stopped duty at company {Id}");
                }

                startDuty(employee);
                player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast den Dienst bei {Name} angefangen!", "Dienst angefangen", Constants.NotifactionImages.Company);

                Logger.logDebug(LogCategory.Player, LogActionType.Event, player, $"Player started duty at company {Id}");
            }
        }
    }


    #endregion

    #region Company Interaction Stuff

    private MenuElement getCompanyHireEmployeeInteractionMenu(IPlayer player, IPlayer target) {
        if(!IsSubCompany && !hasEmployee(target.getCharacterId())) {
            var data = new Dictionary<string, dynamic> { { "Target", target } };
            if(!(player.getCharacterData().AdminMode && player.getAdminLevel() >= 2)) {
                return new ClickMenuItem("Einstellen!", "Stelle die Person in deine Firma ein!", "", "HIRE_EMPLOYEE", MenuItemStyle.green).withData(data).needsConfirmation("Wirklich einstellen?", "Willst du die Person wirklich einstellen?");
            } else {
                return new ClickMenuItem("Als CEO Einstellen! (Supportmenü)", "Stelle die Person in deine Firma ein!", "", "HIRE_EMPLOYEE", MenuItemStyle.green).withData(data).needsConfirmation("Wirklich einstellen?", "Willst du die Person wirklich einstellen?");
            }
        } else {
            return null;
        }
    }

    private void onCompanyHireEmployee(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        if(subEvent == "HIRE_EMPLOYEE") {
            var target = (IPlayer)data["Target"];

            if(target.hasLiteMode()) {
                player.sendNotification(Constants.NotifactionTypes.Warning, "Die Verwaltungssoftware hat das Einstellungsverfahren abgebochen, da der Einreisestatus der Person noch nicht geklärt ist! (LiteMode)", "Litemode Block");
                player.sendBlockNotification("Die Verwaltungssoftware hat das Einstellungsverfahren abgebochen, dein Einreisestatus noch nicht geklärt ist! (LiteMode)", "Litemode Block");
                return;
            }

            if(target != null) {
                data["Company"] = this;
                data["Employeer"] = player;
                data["AsCEO"] = player.getCharacterData().AdminMode && player.getAdminLevel() >= 2;

                var menu = MenuController.getConfirmationMenu("Firmen-Anstellung annehmen?", "Nimm das Einstellungsgesuch an", "CONFIRM_HIRE_PERSON", data);
                target.showMenu(menu);

                if(MaxEmployees > Employees.Count) {
                    player.sendNotification(Constants.NotifactionTypes.Info, "Der Spieler wird nicht aus der Tageskasse bezahlt!", "Tageskasse voll!", Constants.NotifactionImages.Company);
                }
            } else {
                player.sendBlockNotification("Keine Spieler gefunden!", "Keine Spieler!", Constants.NotifactionImages.Company);
            }

        }
    }

    #endregion

    #region Admin Create Stuff
    
    private MenuElement getCompanySupportHireFireSelfMenuElement(IPlayer player) {
        if(!hasEmployee(player.getCharacterId())) {
            return new ClickMenuItem("Selber im CEO Rang einstellen", "Stelle dich selber als CEO ein", "", "HIRE", MenuItemStyle.green)
                .needsConfirmation("Wirklich einstellen?", "Wirklich als CEO einstellen?");
        } else { 
            return new ClickMenuItem("Selber rückstandslos entlassen", "Entlasse dich aus der Firma ohne eine Mitarbeiterkartei zu behalten", "", "FIRE", MenuItemStyle.red)
                .needsConfirmation("Wirklich entlassen?", "Wirklich selbst rückstandslos entlassen?");
        }
    }
    private void onCompanySupportHireFireSelfMenuElementSelect(Company company, IPlayer player, string subevent, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
        if(subevent == "HIRE") {
            hireEmployee(player, -1, 0, 0, true);
            player.sendNotification(Constants.NotifactionTypes.Info, "Du wurdest als CEO eingestellt", "CEO eingestellt", Constants.NotifactionImages.Company);
        } else {
            fireEmployee(findEmployee(player.getCharacterId()), true);
            player.sendNotification(Constants.NotifactionTypes.Info, "Du wurdest rückstandslos entlassen", "Entlassen", Constants.NotifactionImages.Company);
        }
    }
    
    private MenuElement getInventorySpotsAdminElement(IPlayer player) {
        var menu = new Menu("Inventoryspot", "Setze die Inventoryspots");

        var spotMenu = new Menu("Inventoryspot erstellen", "Gib die Daten ein?", false);
        spotMenu.addMenuItem(new InputMenuItem("Name", "Der Name des Spots. Wird angezeigt", "", ""));
        spotMenu.addMenuItem(new InputMenuItem("Maximalgewicht", "Das maximale Gewicht, welches der Spot tragen kann", "", InputMenuItemTypes.number, ""));
        spotMenu.addMenuItem(new InputMenuItem("Türgruppe", "Spot ist dann nur verfügbar, wenn mind. eine Tür der Türgruppe offen ist. Am besten immer Außentürgruppe setzen", "", ""));
        spotMenu.addMenuItem(new MenuStatsMenuItem("Inventarspot Erstellung starten", "", "COMPANY_INVENTORY_CREATE", MenuItemStyle.green));

        menu.addMenuItem(new MenuMenuItem(spotMenu.Name, spotMenu));

        //menu.addMenuItem(new InputMenuItem("Itemlager setzen", "Setze Spots in welchen Items gelagert werden können", "Name", "COMPANY_INVENTORY_CREATE", MenuItemStyle.green));

        var subMenu = new Menu("Spots Liste", "Was möchstest du tun?");
        var itemsWarehouseIds = getSetting<List<int>>("COMPANY_ITEM_SPOTS");
        List<int> list;
        if (itemsWarehouseIds != null) {
            list = itemsWarehouseIds;
        } else {
            list = [];
        }

        foreach (var spotId in list) {
            var spot = InventorySpot.getById(spotId);

            var data = new Dictionary<string, dynamic> {
                { "Spot", spot }
            };

            if(spot == null) {
                itemsWarehouseIds.Remove(spotId);
                setSetting("COMPANY_ITEM_SPOTS", itemsWarehouseIds.ToJson());
                player.sendBlockNotification($"Spot mit Id {spotId}, nicht gefunden. Er wurde aus der Liste entfernt!", "Spot nicht gefunden", Constants.NotifactionImages.Company);
                continue;
            }
            subMenu.addMenuItem(new ClickMenuItem(spot.Name, $"Porte dich zur aktuellen Position des Spots, Id: {spot.Id}, DoorGroup: {spot.DoorGroup}", "", "COMPANY_INVENTORY_PORT").withData(data));
        }

        menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));

        menu.addMenuItem(new ClickMenuItem("Firmensafe setzen", "Setze einen speziellen Spot an den nur bestimmte Mitarbeiter dürfen", "", "COMPANY_SAFE_CREATE", MenuItemStyle.green));


        return menu;
    }

    private void onInventorySpotAdminElementSelect(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        switch(subEvent) {
            case "COMPANY_INVENTORY_PORT":
                var spot = (InventorySpot)data["Spot"];
                player.Position = spot.CollisionShape.Position;
                break;
            case "COMPANY_INVENTORY_CREATE":
                var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
                var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
                var weightEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
                var groupEvt = evt.elements[2].FromJson<InputMenuItemEvent>();

                CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                    var spot = InventorySpot.create(InventorySpotType.Company, nameEvt.input, p, w, h, r, int.Parse(weightEvt.input), null, null, groupEvt.input);
                    ItemsWarehouse.Add(spot);
                    var itemsWarehouseIds = getSetting<List<int>>("COMPANY_ITEM_SPOTS");
                    if(itemsWarehouseIds != null) {
                        itemsWarehouseIds.Add(spot.Id);
                        setSetting("COMPANY_ITEM_SPOTS", itemsWarehouseIds.ToJson());
                    } else {
                        setSetting("COMPANY_ITEM_SPOTS", new List<int> { spot.Id }.ToJson());
                    }

                    player.sendNotification(Constants.NotifactionTypes.Success, "Spot erfolgreich erstellt", "Spot erstellt", Constants.NotifactionImages.Company);
                });
                break;
            case "COMPANY_SAFE_CREATE":
                CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                    Safe = InventorySpot.create(InventorySpotType.Company, "Safe", p, w, h, r, 1000, null);
                    Safe.setOpenPredicate(
                        p => CompanyController.hasPlayerPermission(p, this, "SAFE_ACCESS"),
                        "Du hast keinen Zugriff auf den Firmensafe!"
                    );

                    setSetting("COMPANY_SAFE_SPOT", Safe.Id.ToJson());

                    player.sendNotification(Constants.NotifactionTypes.Success, "Spot erfolgreich erstellt", "Spot erstellt", Constants.NotifactionImages.Company);
                });
                break;
        }
    }


    private MenuElement getLockerAdminElement(IPlayer player) {
        return LockerController.getGenerateMenuForLockerCreation(this);
    }

    private void onLockerAdminElementSelect(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        EventController.triggerMenuEvent(player, subEvent, -1, data, menuItemCefEvent);
    }

    public void addFunctionality(CompanyFunctionality functionality) {
        Functionalities.Add(functionality);
        functionality.onLoad();
    }

    public void removeFunctionality(string identifier) {
        var functionalities = Functionalities.Where(f => f.Identifier == identifier);
        foreach(var funct in functionalities) {
            funct.onRemove();
        }

        Functionalities.RemoveAll(f => f.Identifier == identifier);
    }

    public bool hasFunctionality(string identifier) {
        return Functionalities.Any(f => f.Identifier == identifier);
    }

    public bool hasFunctionality<T>() {
        return Functionalities.Any(f => f is T);
    }
    
    public bool hasFunctionality<T>(Predicate<T> predicat) {
        return Functionalities.Any(f => f is T t && predicat(t));
    }

    public T getFunctionality<T>() {
        var functionality = Functionalities.FirstOrDefault(f => f is T);
        functionality.TryCast<T>(out var castFunctionality);

        return castFunctionality;
    }

    public List<string> getViablePermissions() {
        return Functionalities.SelectMany(f => f.getSinglePermissionsGranted()).ToList();
    }

    #endregion
}
