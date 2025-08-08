#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Character;
using ChoiceVServer.Model.Database;
using ChoiceVSharedApiModels.Characters;
using ChoiceVSharedApiModels.Characters.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChoiceVServer.Controller.Web.ExternApi.Character;

public static class CharacterHelper {
    public static CharacterApiModel convertToApiModel(this character character) {
        var player = ChoiceVAPI.FindPlayerByAccountId(character.accountId);
        var isCurrentlyOnline = player is not null;
        var permadeathActivated = character.dead == 1;

        var hasCrimeFlagActive = ((CharacterFlag)character.flag).HasFlag(CharacterFlag.CrimeFlag);

        var response = new CharacterApiModel(
            character.id,
            character.accountId,
            character.title,
            character.firstname,
            character.lastname,
            character.middleNames,
            character.hunger,
            character.thirst,
            character.energy,
            character.health,
            character.birthdate,
            character.position,
            character.rotation,
            character.gender,
            character.cash,
            character.lastLogin,
            character.lastLogout,
            character.dimension,
            permadeathActivated,
            hasCrimeFlagActive,
            isCurrentlyOnline
        );
        return response;
    }

    public static async Task<List<CharacterApiModel>> getAllCharactersAsync() {
        await using var db = new ChoiceVDb();

        var response = await db.characters
            .Select(x => x.convertToApiModel())
            .ToListAsync();

        return response;
    }

    public static async Task<List<CharacterApiModel>> getAllCharactersByAccountIdAsync(int accountId) {
        await using var db = new ChoiceVDb();

        var response = await db.characters
            .Where(x => x.accountId == accountId)
            .Select(x => x.convertToApiModel())
            .ToListAsync();

        return response;
    }

    public static async Task<CharacterApiModel?> getCharacterByIdAsync(int characterId) {
        await using var db = new ChoiceVDb();

        var dbChar = await db.characters.FirstOrDefaultAsync(x => x.id == characterId);
        if(dbChar is null) return null;

        var response = dbChar.convertToApiModel();

        return response;
    }

    /// <summary>
    /// Asynchronously sets the permadeath state for a character identified by characterId.
    /// </summary>
    /// <param name="characterId">The ID of the character.</param>
    /// <param name="state">The desired permadeath state (true for activated, false for deactivated).</param>
    /// <returns>
    /// A tuple where:
    /// - The first bool indicates if the operation was successful (i.e., the character was updated).
    /// - The second bool indicates if the player was online and the CharacterData could be updated.
    /// </returns>
    public static async Task<(bool, bool)> setPermadeathActivatedAsync(int characterId, bool state) {
        character? character;
        await using(var db = new ChoiceVDb()) {
            character = db.characters
                .Include(c => c.characterdata)
                .FirstOrDefault(c => c.id == characterId);
        }

        if(character is null) return (false, false);

        return await character.setPermadeathActivatedAsync(state);
    }

    /// <summary>
    /// Asynchronously sets the permadeath state for the given character instance.
    /// </summary>
    /// <param name="character">The character instance.</param>
    /// <param name="state">The desired permadeath state (true for activated, false for deactivated).</param>
    /// <returns>
    /// A tuple where:
    /// - The first bool indicates if the operation was successful (i.e., the character was updated).
    /// - The second bool indicates if the player was online and the CharacterData could be updated.
    /// </returns>
    public static async Task<(bool, bool)> setPermadeathActivatedAsync(this character character, bool state) {
        var updateCharacterData = false;
        try {
            switch(state) {
                case true when character.dead == 1:
                case false when character.dead == 0:
                    return (updateCharacterData, false);
            }

            await using(var db = new ChoiceVDb()) {
                character.dead = state ? 1 : 0;

                db.characters.Update(character);
                await db.SaveChangesAsync();
                updateCharacterData = true;
            }

            var player = ChoiceVAPI.FindPlayerByCharId(character.id);
            if(player == null) return (updateCharacterData, false);

            var playerCharacterData = player.getCharacterData();
            playerCharacterData.PermadeathActivated = state;
            player.setCharacterData(playerCharacterData);
            return (updateCharacterData, true);
        } catch(Exception e) {
            return (updateCharacterData, false);
        }
    }

    public static async Task<(bool, bool, CharacterFlagApiEnum?, CharacterFlagApiEnum?)> setCrimeFlagActiveAsync(int characterId, bool state) {
        character? character;
        await using(var db = new ChoiceVDb()) {
            character = db.characters
                .Include(c => c.characterdata)
                .FirstOrDefault(c => c.id == characterId);
        }

        if(character is null) return (false, false, null, null);

        return await character.setCrimeFlagActiveAsync(state);
    }

    public static async Task<(bool, bool, CharacterFlagApiEnum?, CharacterFlagApiEnum?)> setCrimeFlagActiveAsync(this character character, bool state) {
        var updateCharacterData = false;
        CharacterFlagApiEnum? oldFlag = null;
        CharacterFlagApiEnum? newFlag = null;
        
        try {
            var currentFlag = (CharacterFlag)character.flag;
            oldFlag = currentFlag.convertToApiModel();

            switch(state) {
                case true when currentFlag.HasFlag(CharacterFlag.CrimeFlag):
                case false when !currentFlag.HasFlag(CharacterFlag.CrimeFlag):
                    return (updateCharacterData, false, oldFlag, newFlag);
            }

            await using(var db = new ChoiceVDb()) {
                character.flag ^= (int)CharacterFlag.CrimeFlag;

                db.characters.Update(character);
                await db.SaveChangesAsync();
                updateCharacterData = true;
                newFlag = ((CharacterFlag)character.flag).convertToApiModel();
            }

            var player = ChoiceVAPI.FindPlayerByCharId(character.id);
            if(player == null) return (updateCharacterData, false, oldFlag, newFlag);

            var playerCharacterData = player.getCharacterData();
            playerCharacterData.CharacterFlag = (CharacterFlag)character.flag;
            player.setCharacterData(playerCharacterData);
            return (updateCharacterData, true, oldFlag, newFlag);
        } catch(Exception e) {
            return (updateCharacterData, false, oldFlag, newFlag);
        }
    }

    public static CharacterFlagApiEnum convertToApiModel(this CharacterFlag characterFlag) {
        var newFlag = (int)characterFlag;
        return (CharacterFlagApiEnum)newFlag;
    }
}
