using System.Text.Json.Serialization;
using ChoiceVSharedApiModels.Characters.Enums;

namespace ChoiceVSharedApiModels.Characters;

public class CharacterSetCrimeFlagActiveResultApiModel {
    
    public CharacterSetCrimeFlagActiveResultApiModel(){}

    public CharacterSetCrimeFlagActiveResultApiModel(bool onlinePlayerUpdated, CharacterFlagApiEnum oldState, CharacterFlagApiEnum newState, int characterId) {
        OnlinePlayerUpdated = onlinePlayerUpdated;
        OldState = oldState;
        NewState = newState;
        CharacterId = characterId;
    }
    
    [JsonPropertyName("currentlyOnline")]
    public bool OnlinePlayerUpdated { get; set; }
    
    [JsonPropertyName("oldState")]
    public CharacterFlagApiEnum OldState { get; set; }
    
    [JsonPropertyName("newState")]
    public CharacterFlagApiEnum NewState { get; set; }
    
    [JsonPropertyName("characterId")]
    public int CharacterId { get; set; }
}
