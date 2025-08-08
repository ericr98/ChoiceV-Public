using AltV.Net.Elements.Entities;
using System;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Model.Database;
using System.Linq;

namespace ChoiceVServer.Controller;

public class CompanySubventionController : ChoiceVScript {
    public enum SubventionType {
        SupportSetValue = 0,
        TaxiFare = 1,
        NewspaperMachine = 2,
        NewsPaperBag = 3,
    }
    
    private static TimeSpan CHECKER_INTERVAL = TimeSpan.FromMinutes(57);
    
    public CompanySubventionController() {
        InvokeController.AddTimedInvoke("CompanySubsitutionChecker", updateCompanySubvention, CHECKER_INTERVAL, true, true);  
    }
    
    private void updateCompanySubvention(IInvoke obj) {
        using(var db = new ChoiceVDb()) {
            var subventions = db.companysubventions.ToList();
            foreach(var subvention in subventions) {
                if(subvention.amount <= 20) {
                    continue;
                }
                
                var company = CompanyController.getCompanyById(subvention.companyId);
                if(company == null) {
                    Logger.logError($"Company with id {subvention.companyId} not found");
                    continue;
                }

                var randomAmount = Math.Round(subvention.amount * (decimal)(new Random().NextDouble() * 0.2 + 0.05), 2);
                if(BankController.putMoneyInAccount(company.CompanyBankAccount, randomAmount, "Einnahmen durch Passiv-Gewerbe", out var failMessage)) {
                    subvention.amount -= randomAmount; 
                    Logger.logDebug(LogCategory.System, LogActionType.Event, $"Company {company.Id} got {randomAmount} from subvention {subvention.subventionType}");
                }
                
            }

            db.SaveChanges();
        }
    }

    public static void triggerSubventionEvent(IPlayer triggerer, Company company, SubventionType type) {
        if(!makeSubventionTriggererAvailableCheck(triggerer, company, type)) {
            return;
        }
        
        var functionality = company.getFunctionality<CompanySubventionFunctionality>();

        if(functionality == null) {
            Logger.logError($"Trigger substitution event {type} for company {company.Id} substitution functionality is null");
            return;
        }

        var element = functionality.getSubventionElement(type);

        using(var db = new ChoiceVDb()) {
            var dbElement = db.companysubventions.Find(company.Id, (int)(type));
           
            var amount = Math.Round(element.FlatValue * element.Multiplier, 2);
            if(dbElement == null) {
                dbElement = new companysubvention {
                    companyId = company.Id,
                    subventionType = (int)type,
                    amount = amount,
                    eventsCount = 1,
                    totalAmount = amount,
                };
                db.companysubventions.Add(dbElement);
            } else {
                dbElement.amount += amount;
                dbElement.totalAmount += amount;
                dbElement.eventsCount++;
            }

            db.SaveChanges();
        } 
    }
    
    //Check so that each subvention can only be triggered once a day by a player
    public static void triggerManualSubvention(IPlayer triggerer, Company company, SubventionType type, decimal amount) {
        if(!makeSubventionTriggererAvailableCheck(triggerer, company, type)) {
            return;
        }
        
        var functionality = company.getFunctionality<CompanySubventionFunctionality>();
    
        if(functionality == null) {
            Logger.logError($"Trigger substitution event {type} for company {company.Id} substitution functionality is null");
            return;
        }
    
        using(var db = new ChoiceVDb()) {
            var dbElement = db.companysubventions.Find(company.Id, (int)(type));
               
            if(dbElement == null) {
                dbElement = new companysubvention {
                    companyId = company.Id,
                    subventionType = (int)type,
                    amount = amount,
                    eventsCount = 1,
                    totalAmount = amount,
                };
                db.companysubventions.Add(dbElement);
            } else {
                dbElement.amount += amount;
                dbElement.totalAmount += amount;
                dbElement.eventsCount++;
            }
    
            db.SaveChanges();
        } 
    }

    private static bool makeSubventionTriggererAvailableCheck(IPlayer player, Company company, SubventionType type) {
        if(player.hasData($"SUBVENTION_{company.Id}_{(int)(type)}")) {
            var lastTrigger = ((string)player.getData($"SUBVENTION_{company.Id}_{(int)(type)}")).FromJson<DateTime>();
            
            if(DateTime.Now - lastTrigger < TimeSpan.FromHours(24)) {
                return false;
            }
        }
        
        player.setPermanentData($"SUBVENTION_{company.Id}_{(int)(type)}", DateTime.Now.ToJson());
        return true;
    }
    
    internal static (decimal currentAmount, int totalEvents, decimal totalAmount) getCurrentSubventionAmountForCompany(Company company, SubventionType type) {
        using(var db = new ChoiceVDb()) {
            var dbElement = db.companysubventions.Find(company.Id, (int)(type));
            if(dbElement == null) {
                return (0, 0, 0);
            }

            return (dbElement.amount, dbElement.eventsCount, dbElement.totalAmount);
        }
    }

    internal static void changeCurrentSubventionAmount(Company company, SubventionType type, decimal amount) {
        using(var db = new ChoiceVDb()) {
            var dbElement = db.companysubventions.Find(company.Id, (int)(type));
            if(dbElement == null) {
                var newElement = new companysubvention {
                    companyId = company.Id,
                    subventionType = (int)type,
                    amount = amount,
                    eventsCount = 1,
                    totalAmount = amount,
                };
                db.companysubventions.Add(newElement);
            } else {
                var difference = amount - dbElement.amount;
                dbElement.amount = amount;
                dbElement.totalAmount += difference;
            }

            db.SaveChanges();
        }
    }

    internal static void removeSubvention(Company company, SubventionType type) {
        using(var db = new ChoiceVDb()) {
            var dbElement = db.companysubventions.Find(company.Id, (int)(type));
            if(dbElement == null) {
                return;
            }

            db.companysubventions.Remove(dbElement);
            db.SaveChanges();
        }
    }
}
