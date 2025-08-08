using ChoiceVServer.Base;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller {
    class TaxController : ChoiceVScript {
        public TaxController() {
            InvokeController.AddTimedInvoke("CompanyTaxChecking", checkCompanyTaxes, TAX_CHECKING_COMPANY, true);
            InvokeController.AddTimedInvoke("PlayerTaxChecking", checkPlayerTaxes, TAX_CHECKING_COMPANY, true);
        }

        private void checkCompanyTaxes(IInvoke ivk) {
            //TaxIrragulations irragulation = TaxIrragulations.None;
            ////Companies
            //foreach (var company in CompanyController.AllCompanies) {
            //    var tax = company.CompanyTax;
            //    if (tax != null && tax.NextTaxDue <= DateTime.Now) {
            //        var amount = tax.calculateToPayTaxes(ref irragulation);
            //        var succeded = BankController.transferMoney(company.CompanyBankAccount, STATE_BANK_ACCOUNT, amount, COMPANY_TRANSFER_TAX_MESSAGE + company.Name, null);

            //        //Handle that Company does not have enough money to pay tax, which should not be possible if everything went legal
            //        if (!succeded) {
            //            //TODO Company has not enough money
            //        }

            //        //Handle any irragulation detected with the tax proccess
            //        switch (irragulation) {
            //            //TODO Inform about irragulations
            //        }

            //        tax.resetTax();
            //    }
            //}        
        }

        private void checkPlayerTaxes(IInvoke ivk) {
            //throw new NotImplementedException();
        }
    }
}
