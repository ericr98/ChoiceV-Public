using ChoiceVServer.Model.Database;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Linq;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller {
    public class DiscordSlashCommands : ApplicationCommandModule {
        [SlashCommand("setDiscordId", "Setze die DiscordId eines Accounts mithilfe des Socialclubs")]
        public async Task setDiscordId(InteractionContext ctx, [Option("User", "Die DiscordId von diesem User wird dann für den Login benutzt")] DiscordUser user, [Option("Socialclub", "Gib den Account ein, der geupdatet werden soll")] string socialclub) {
            using(var db = new ChoiceVDb()) {
                var account = db.accounts.FirstOrDefault(a => a.socialclubName == socialclub);

                if(account != null) {
                    account.discordId = user.Id.ToString();
                    account.state = 1;

                    db.SaveChanges();
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Der Account mit Id: {account.id} und Socialclub: {account.socialclubName} wurde erfolgreich geupdatet! Er hat nun die Discord Id von User: {user.Username}#{user.Discriminator} ({user.Id})."));
                } else {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Der Socialclub konnte nicht in Verbindung mit einem Account gebracht werden!"));
                }
            }
        }
    }
}
