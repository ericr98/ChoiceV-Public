using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamSpeak3QueryApi.Net.Specialized;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DiscordBot.Controller {
    public class TeamspeakController : BotScript {
        private static TeamSpeakClient TeamSpeakClient;

        public TeamspeakController() {
            startTeamspeakBot();
        }

        private static async Task startTeamspeakBot() {
            TeamSpeakClient = new TeamSpeakClient(Config.TeamspeakAddress, Config.TeamspeakPort);

            await TeamSpeakClient.Connect();
            await TeamSpeakClient.Login(Config.TeamspeakUser, Config.TeamspeakPassword);
            await TeamSpeakClient.UseServer(1);
            await TeamSpeakClient.ChangeNickName("Whitelist-Bot");
        }

        public async static Task<bool> sendTeamspeakMessage(string identity, string message) {
            try {
                var clientId = await TeamSpeakClient.GetClientIds(identity);

                await TeamSpeakClient.SendMessage(message, MessageTarget.Private, clientId.ClientId);
                return true;
            } catch (Exception ex) {
                if(ex.InnerException != null) {
                    await startTeamspeakBot();
                    return await sendTeamspeakMessage(identity, message);
                } else {
                    return false;
                }
            }
        }

        public async static Task giveTeamspeakSuceedRank(string identity) {
            try {
                var clientId = await TeamSpeakClient.GetClientIds(identity);

                foreach(var id in Config.TeamspeakSucceedRoleIds.Split(",")) {
                    var teamspeakRank = int.Parse(id);
                    await TeamSpeakClient.AddServerGroup(teamspeakRank, clientId.ClientId);
                }
            } catch(Exception ex) {
                if(ex.InnerException != null) {
                    await startTeamspeakBot();
                    await giveTeamspeakSuceedRank(identity);
                }
            }
        }
    }
}
