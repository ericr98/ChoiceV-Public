using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChoiceVSharedApiModels.Discord;
using DSharpPlus.Entities;

namespace ChoiceVServer.Controller.Web.ExternApi.Discord;

public static class DiscordHelper {
    
    public static List<DiscordUserApiModel> convertToApiModel(this IEnumerable<DiscordMember> discordMembers) {
        return discordMembers.Select(x => x.convertToApiModel()).ToList();
    }
    
    public static DiscordUserApiModel convertToApiModel(this DiscordMember discordMember) {
        
        var response = new DiscordUserApiModel(
            discordMember.Id,
            discordMember.Username
        );

        return response;
    }
}
