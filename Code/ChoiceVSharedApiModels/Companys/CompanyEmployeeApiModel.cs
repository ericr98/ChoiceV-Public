namespace ChoiceVSharedApiModels.Companys;

public class CompanyEmployeeApiModel {
    public CompanyEmployeeApiModel() { }
    
    public CompanyEmployeeApiModel(int id, int characterId, string characterName, decimal salary, long selectedBankAccount, long phoneNumber, bool inDuty) {
        Id = id;
        CharacterId = characterId;
        CharacterName = characterName;
        Salary = salary;
        SelectedBankAccount = selectedBankAccount;
        PhoneNumber = phoneNumber;
        InDuty = inDuty;
    }
    
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public string CharacterName { get; set; }
    public decimal Salary { get; set; }
    public long SelectedBankAccount { get; set; }
    public long PhoneNumber { get; set; }
    public bool InDuty { get; set; }
}
