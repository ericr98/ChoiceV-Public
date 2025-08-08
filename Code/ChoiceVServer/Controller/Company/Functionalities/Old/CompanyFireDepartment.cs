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
//    //Ölspur die Feuerwehr wegmachen muss
//    public class CompanyFireDepartmentController : ChoiceVScript {

//        public CompanyFireDepartmentController() { }

//        public static bool isFirefighter(IPlayer player) {
//            var companies = CompanyController.getCompanies(player);
//            if (companies != null) {
//                return companies.FirstOrDefault(c => c.Type == CompanyType.Fire) != null;
//            }

//            return false;
//        }

//        public static bool isFirefighterInDuty(IPlayer player) {
//            var companies = CompanyController.getCompanies(player);
//            if (companies != null) {
//                Company company = companies.FirstOrDefault(c => c.Type == CompanyType.Fire);

//                if (company != null)
//                    return company.findEmployee(player.getCharacterId()).InDuty;
//            }

//            return false;
//        }

//        public static CompanyFireDepartment getFireDepartmentCompany(IPlayer player) {
//            var companies = CompanyController.getCompanies(player);
//            if (companies != null) {
//                return companies.FirstOrDefault(c => c is CompanyFireDepartment) as CompanyFireDepartment;
//            }

//            return null;
//        }
//    }

//    public class CompanyFireDepartment : Company {

//        public CompanyFireDepartment(company dbComp, system system, List<CompanyPermission> viablePermissions, List<companysetting> allSettings) : base(dbComp, system, viablePermissions, allSettings) { }
//    }
//}
