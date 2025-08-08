using System.Collections.Generic;
using System.Linq;
using ChoiceVSharedApiModels.Companys;
using Google.Protobuf.WellKnownTypes;
using Enum = System.Enum;

namespace ChoiceVServer.Controller.Web.ExternApi.Company;

public static class CompanyHelper {

    public static List<CompanyApiModel> convertToApiModel(this List<Companies.Company> companys) {
        return companys.Select(x => x.convertToApiModel()).ToList();
    }

    public static CompanyApiModel convertToApiModel(this Companies.Company company) {
        var typeParsed = Enum.TryParse<CompanyTypeApiEnum>(company.CompanyType.ToString(), out var typeEnum);

        var response = new CompanyApiModel(
            company.Id,
            company.Name,
            company.ShortName,
            company.CityName,
            company.StreetName,
            typeParsed ? typeEnum : CompanyTypeApiEnum.Undefined,
            company.MaxEmployees,
            company.CompanyBankAccount,
            company.Employees.convertToApiModel(),
            company.Reputation,
            company.RiskLevel
        );

        return response;
    }

    public static List<CompanyEmployeeApiModel> convertToApiModel(this List<Companies.CompanyEmployee> company) {
        return company.Select(x => x.convertToApiModel()).ToList();
    }

    public static CompanyEmployeeApiModel convertToApiModel(this Companies.CompanyEmployee employee) {
        var response = new CompanyEmployeeApiModel(
            employee.Id,
            employee.CharacterId,
            employee.CharacterName,
            employee.Salary,
            employee.SelectedBankAccount,
            employee.PhoneNumber,
            employee.InDuty
        );

        return response;
    }
}
