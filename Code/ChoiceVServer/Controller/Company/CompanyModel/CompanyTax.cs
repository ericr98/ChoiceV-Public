using System;
using System.Collections.Generic;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller.Companies {
    public class CompanyTax {
        public decimal CombinedProfit { get; private set; }
        public decimal CombinedExpenses { get; private set; }
        public decimal LastCombindedProfit { get; private set; }
        public decimal LastCombindedExpenses { get; private set; }

        public List<CompanyProfit> Profits { get; private set; }
        public List<CompanyExpenses> Expenses { get; private set; }

        public decimal LandTax;
        public float TaxPercent;
        public DateTime NextTaxDue;

        public CompanyTax(float taxPercent) {
            TaxPercent = taxPercent;
        }

        /// <summary>
        /// Calculates the Amount of Money the Company has to pay to the state
        /// </summary>
        /// <param name="irragulation">Sets an irragulationFlag if something is weird</param>
        /// <returns></returns>
        public decimal calculateToPayTaxes(ref TaxIrragulations irragulation) {

            if(CombinedProfit > CombinedExpenses) {
                irragulation = TaxIrragulations.MoreExpensesThanProfit;
            } else if(CombinedProfit > LastCombindedProfit * 2) {
                irragulation = TaxIrragulations.ProfitHasDoubled;
            } else if(CombinedExpenses > CombinedExpenses * 2) {
                irragulation = TaxIrragulations.ExpensesHasDoubled;
            } else {
                irragulation = TaxIrragulations.None;
            }

            return (CombinedProfit - CombinedExpenses) * (decimal)TaxPercent + LandTax;
        }

        /// <summary>
        /// Resets the curent Tax Model. Use e.g after Taxes are paid.
        /// </summary>
        public void resetTax() {
            //TODO Save all Data in Log Tables bzw. Log Files

            Profits.Clear();
            Expenses.Clear();
            LastCombindedProfit = CombinedProfit;
            LastCombindedExpenses = CombinedExpenses;
            CombinedProfit = 0;
            CombinedExpenses = 0;

            NextTaxDue = DateTime.Now + COMPANY_TAX_PAYING_TIME;
        }

        /// <summary>
        /// Register a Profit for a Company. Profit may be a bill, the payment of an order or other money giving activities
        /// </summary>
        public void registerProfit(decimal amount, string reason) {
            CombinedProfit = CombinedProfit + amount;
            var newProfit = new CompanyProfit {
                Amount = amount,
                Message = reason,
                IssueDate = DateTime.Now,
            };

            Profits.Add(newProfit);
        }

        /// <summary>
        /// Register an Expense for a company. Expenses will be freed from Tax
        /// </summary>
        public void registerExpense(decimal amount, string reason) {
            CombinedExpenses = CombinedExpenses + amount;
            var newExpense = new CompanyExpenses {
                Amount = amount,
                Message = reason,
                IssueDate = DateTime.Now,
            };

            Expenses.Add(newExpense);
        }

        public class CompanyProfit {
            public decimal Amount;
            public string Message;
            public DateTime IssueDate;
        }

        public class CompanyExpenses {
            public decimal Amount;
            public string Message;
            public DateTime IssueDate;
        }
    }
}
