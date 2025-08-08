using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Controller.Companies.Company;
using static ChoiceVServer.Controller.CompanyController;

namespace ChoiceVServer.Controller {
    internal class CompanyExternalCompanyInteractController : ChoiceVScript{
        private static List<AllExternalCompanySelfMenuRegister> AllCompanySelfMenuRegisters = new List<AllExternalCompanySelfMenuRegister>();

        internal record AllExternalCompanySelfMenuRegister(string Identifier, GeneratedCompanySelfMenuElementDelegate Generator, CompanyMenuElementCallbackDelegate Callback, string PermissionRequirement = null);
        internal static void addExternalCompanySelfMenuRegister(string identifier, GeneratedCompanySelfMenuElementDelegate generator, CompanyMenuElementCallbackDelegate callback, string permissionRequirement = null) {
            AllCompanySelfMenuRegisters.Add(new AllExternalCompanySelfMenuRegister(identifier, generator, callback, permissionRequirement));
        }

        public static List<AllExternalCompanySelfMenuRegister> getAllExternalCompanySelfMenuRegisters() {
            return AllCompanySelfMenuRegisters;
        }

    }
}
