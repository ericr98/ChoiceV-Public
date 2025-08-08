using System.Text.Json.Serialization;

namespace ChoiceVSharedApiModels.Characters;

public class CharacterApiModel
{
    public CharacterApiModel() { }
    
    public CharacterApiModel(
        int id, 
        int accountId, 
        string? title, 
        string firstName, 
        string lastName, 
        string middleName, 
        double hunger, 
        double thirst, 
        double energy, 
        double health, 
        DateOnly birthDate, 
        string position, 
        string rotation, 
        string gender, 
        decimal cash, 
        DateTime lastLogin, 
        DateTime lastLogout, 
        int dimension,
        bool permadeathActivated,
        bool crimeFlagActivated,
        bool isCurrentlyOnline)
    {
        Id = id;
        AccountId = accountId;
        Title = title;
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
        Hunger = hunger;
        Thirst = thirst;
        Energy = energy;
        Health = health;
        BirthDate = birthDate;
        Position = position;
        Rotation = rotation;
        Gender = gender;
        Cash = cash;
        LastLogin = lastLogin;
        LastLogout = lastLogout;
        Dimension = dimension;
        PermadeathActivated = permadeathActivated;
        CrimeFlagActivated = crimeFlagActivated;
        IsCurrentlyOnline = isCurrentlyOnline;
    }

    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("account_id")]
    public int AccountId { get; set; }
    
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }
    
    [JsonPropertyName("last_name")]
    public string LastName { get; set; }
    
    [JsonPropertyName("middle_name")]
    public string MiddleName { get; set; }
    
    [JsonPropertyName("hunger")]
    public double Hunger { get; set; }
    
    [JsonPropertyName("thirst")]
    public double Thirst { get; set; }
    
    [JsonPropertyName("energy")]
    public double Energy { get; set; }
    
    [JsonPropertyName("health")]
    public double Health { get; set; }
        
    [JsonPropertyName("birth_date")]
    public DateOnly BirthDate { get; set; }
        
    [JsonPropertyName("position")]
    public string Position { get; set; }
        
    [JsonPropertyName("rotation")]
    public string Rotation { get; set; }
        
    [JsonPropertyName("gender")]
    public string Gender { get; set; }
        
    [JsonPropertyName("cash")]
    public decimal Cash { get; set; }
        
    [JsonPropertyName("last_login")]
    public DateTime LastLogin { get; set; }
        
    [JsonPropertyName("last_logout")]
    public DateTime LastLogout { get; set; }
        
    [JsonPropertyName("dimension")]
    public int Dimension { get; set; }
    
    [JsonPropertyName("permadeathActivated")]
    public bool PermadeathActivated { get; set; }
    
    [JsonPropertyName("crimeFlagActivated")]
    public bool CrimeFlagActivated { get; set; }
    
    [JsonPropertyName("isCurrentlyOnline")]
    public bool IsCurrentlyOnline { get; set; }
    
    public virtual string FullName => $"{FirstName} {MiddleName} {LastName}";
}