using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.Companies {
    public record CompanyEmployeeUnpayedDuty(int DbId, DateTime DutyStart, DateTime DutyEnd);

    public class CompanyEmployee {
        public int Id;
        public int CharacterId;
        public string CharacterName;

        public decimal Salary;

        public long SelectedBankAccount;

        public long PhoneNumber;

        public bool InDuty;

        public List<CompanyEmployeeUnpayedDuty> UnpaidDuties;

        public CompanyEmployee(int charId, string charName, decimal salary, long bankAccount, long phoneNumber) {
            CharacterId = charId;
            CharacterName = charName;
            Salary = salary;
            SelectedBankAccount = bankAccount;
            PhoneNumber = phoneNumber;

            UnpaidDuties = new List<CompanyEmployeeUnpayedDuty>();
            InDuty = false;
        }

        public CompanyEmployee(companyemployee dbEmployee) {
            Id = dbEmployee.id;
            CharacterId = dbEmployee.charId;
            CharacterName = dbEmployee.charName;
            Salary = dbEmployee.salary;
            SelectedBankAccount = dbEmployee.selectedBankAccount;
            PhoneNumber = dbEmployee.selectedPhoneNumer;

            UnpaidDuties = dbEmployee.companyemployeesduties.Where(d => !d.successfullyTransfered && d.dutyEnd != null)
                .Select(d => new CompanyEmployeeUnpayedDuty(d.id, d.dutyStart, d.dutyEnd ?? d.dutyStart)).ToList();
            InDuty = false;
        }
    }
}
