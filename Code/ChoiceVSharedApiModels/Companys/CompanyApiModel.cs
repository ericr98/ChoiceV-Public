namespace ChoiceVSharedApiModels.Companys;

public class CompanyApiModel {
    public CompanyApiModel() { }


    public CompanyApiModel(
        int id, 
        string name, 
        string shortName, 
        string cityName,
        string streetName, 
        CompanyTypeApiEnum companyType, 
        int maxEmployees, 
        long companyBankAccount,
        List<CompanyEmployeeApiModel> employees,
        int reputation, 
        int riskLevel) {
        Id = id;
        Name = name;
        ShortName = shortName;
        CityName = cityName;
        StreetName = streetName;
        CompanyType = companyType;
        MaxEmployees = maxEmployees;
        CompanyBankAccount = companyBankAccount;
        Employees = employees;
        Reputation = reputation;
        RiskLevel = riskLevel;
    }
    
    public int Id { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string CityName { get; set; }
    public string StreetName { get; set; }
    public CompanyTypeApiEnum CompanyType { get; set; }
    public int MaxEmployees { get; set; }
    public long CompanyBankAccount { get; set; }
    public List<CompanyEmployeeApiModel> Employees { get; set; }
    public int Reputation { get; set; }
    public int RiskLevel { get; set; }
}
