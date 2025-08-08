//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.Controller;
//using ChoiceVServer.Controller.CompanyModel;
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
//using static ChoiceVServer.Base.Constants;
//using static ChoiceVServer.Controller.CharacterController;
//using static ChoiceVServer.Model.Menu.InputMenuItem;

//namespace ChoiceVServer.Controller.Companies {
//    public class CompanyGasstationController : ChoiceVScript {

//        public CompanyGasstationController() { }

//        public static bool isEmployee(IPlayer player, int ownerid) {
//            var companies = CompanyController.getCompanies(player);
//            if (companies != null) {
//                return companies.FirstOrDefault(c => c.Type == CompanyType.Gasstation && c.Id == ownerid) != null;
//            }

//            return false;
//        }

//        public static bool isEmployeeInDuty(IPlayer player, int ownerid) {
//            var companies = CompanyController.getCompanies(player);
//            if (companies != null) {
//                Company company = companies.FirstOrDefault(c => c.Type == CompanyType.Gasstation && c.Id == ownerid);

//                if (company != null)
//                    return company.findEmployee(player.getCharacterId()).InDuty;
//            }

//            return false;
//        }

//        public static CompanyGasstation getGasstationCompany(IPlayer player, int ownerid) {
//            var companies = CompanyController.getCompanies(player);
//            if (companies != null) {
//                return companies.FirstOrDefault(c => c is CompanyGasstation && c.Id == ownerid) as CompanyGasstation;
//            }

//            return null;
//        }
//    }

//    public class CompanyGasstation : Company {

//        public CompanyGasstation(company dbComp, system system, List<CompanyPermission> viablePermissions, List<companysetting> allSettings) : base(dbComp, system, viablePermissions, allSettings) { }
//    }
//}
