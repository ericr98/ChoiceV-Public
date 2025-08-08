using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChoiceVServer.Controller.Web.ExternApi.Account;
using ChoiceVServer.Controller.Web.ExternApi.Character;
using ChoiceVServer.Model.Database;
using ChoiceVSharedApiModels.Accounts;
using ChoiceVSharedApiModels.Characters;
using ChoiceVSharedApiModels.SupportKeyInfo;
using ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ChoiceVServer.Controller.Web.ExternApi.SupportKeyInfo;

public static class SupportKeyInfoHelper {
    
    public static async Task<List<SupportKeyInfoApiModel>> convertToApiModel(this IEnumerable<supportkeyinfo> models) {
        if(models == null) {
            throw new ArgumentNullException(nameof(models), "Model list cannot be null");
        }
        
        var result = await Task.WhenAll(models.Select(model => model.convertToApiModel()));
        return result.ToList();
    }
     
    public static async Task<SupportKeyInfoApiModel> convertToApiModel(this supportkeyinfo model) {
        if(model == null) {
            throw new ArgumentNullException(nameof(model), "Model cannot be null");
        }

        var character = await CharacterHelper.getCharacterByIdAsync(model.sender);
        
        var account = new AccountApiModel();
        if(character is not null) {
            account = await AccountHelper.getByIdConvertedAsync(character.AccountId);
        }

        var surroundingData = JsonSerializer.Deserialize<SupportKeySurroundingInfo>(model.surroundingData);

        return new SupportKeyInfoApiModel(
            model.id,
            character?.Id ?? model.sender,
            character?.FullName ?? "",
            account?.Id ?? -1,
            account?.Name ?? "",
            model.createdDate,
            model.message,
            surroundingData
        );
    }
}
