using System.Text.Json.Serialization;
using ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels;

namespace ChoiceVSharedApiModels.SupportKeyInfo;

public class SupportKeyInfoApiModel {
    public SupportKeyInfoApiModel() { }

    public SupportKeyInfoApiModel(
        int id, 
        int senderCharacterId,
        string senderCharacterName,
        int senderAccountId,
        string senderAccountName, 
        DateTime creationDate,
        string message,
        SupportKeySurroundingInfo surroundingData) {
        Id = id;
        SenderCharacterId = senderCharacterId;
        SenderCharacterName = senderCharacterName;
        SenderAccountId = senderAccountId;
        SenderAccountName = senderAccountName;
        CreatedAt = creationDate;
        Message = message;
        SurroundingData = surroundingData;
    }
    
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("senderCharacterId")]
    public int SenderCharacterId { get; set; }
    
    [JsonPropertyName("senderCharacterName")]
    public string SenderCharacterName { get; set; }
    
    [JsonPropertyName("senderAccountId")]
    public int SenderAccountId { get; set; }
    
    [JsonPropertyName("senderAccountName")]
    public string SenderAccountName { get; set; }
    
    [JsonPropertyName("CreatedAt")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("surroundingData")]
    public SupportKeySurroundingInfo SurroundingData { get; set; }
}
