using System.Collections.Generic;

namespace ChoiceVServer.Controller.Companies {
    public abstract class CompanyFunctionality {
        public string Identifier { get => getIdentifier(); }
        protected Company Company;

        //Mock creation
        public CompanyFunctionality() { }

        public CompanyFunctionality(Company company) {
            Company = company;
        }


        public abstract string getIdentifier();

        public abstract void onLoad();
        public abstract void onRemove();

        public record SelectionInfo(string Identifier, string Name, string Description);
        public abstract SelectionInfo getSelectionInfo();

        public virtual List<string> getSinglePermissionsGranted() { return new List<string>(); }

        public virtual void onLastEmployeeLeaveDuty() { }
        public virtual void onFirstEmployeeEnterDuty() { }
    }
}
