using System.Text.Json.Serialization;

namespace ChoiceVSharedApiModels.Characters;

public class CharacterChangeDimensionResultApiModel {
    
    public CharacterChangeDimensionResultApiModel(){}
    
    public CharacterChangeDimensionResultApiModel(int oldDimension, int newDimension, int characterId) {
        OldDimension = oldDimension;
        NewDimension = newDimension;
        CharacterId = characterId;
    }
    
    [JsonPropertyName("oldDimension")]
    public int OldDimension { get; set; }
    
    [JsonPropertyName("newDimension")]
    public int NewDimension { get; set; }
    
    [JsonPropertyName("characterId")]
    public int CharacterId { get; set; }
}
