using System.Text.Json.Serialization;

namespace ChoiceVSharedApiModels.Characters;

public class CharacterSetPermadeathActivatedResultApiModel {
    public CharacterSetPermadeathActivatedResultApiModel(){}

    public CharacterSetPermadeathActivatedResultApiModel(bool onlinePlayerUpdated, bool oldState, bool newState, int characterId) {
        OnlinePlayerUpdated = onlinePlayerUpdated;
        OldState = oldState;
        NewState = newState;
        CharacterId = characterId;
    }
    
    [JsonPropertyName("currentlyOnline")]
    public bool OnlinePlayerUpdated { get; set; }
    
    [JsonPropertyName("oldState")]
    public bool OldState { get; set; }
    
    [JsonPropertyName("newState")]
    public bool NewState { get; set; }
    
    [JsonPropertyName("characterId")]
    public int CharacterId { get; set; }
}
