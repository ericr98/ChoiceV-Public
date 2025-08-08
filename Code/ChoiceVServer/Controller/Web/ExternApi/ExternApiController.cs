using System;
using System.Reflection;
using System.Threading.Tasks;
using ChoiceVServer.Base;
using ChoiceVExternApi;

namespace ChoiceVServer.Controller.Web.ExternApi;

public class ExternApiController : ChoiceVScript {

    public ExternApiController() {
        var handlerAssembly = Assembly.GetExecutingAssembly();
        
        var config = new ExternApiConfig(
            Config.ExternApiUrl,
            Config.ExternApiNeededUsername,
            Config.ExternApiNeededPassword,
            Config.DiscordBotToken,
            Config.DiscordServerId,
            Config.IsDevServer,
            handlerAssembly
        );
        
        var externApi = new ChoiceVExternApi.ExternApi(config);
        
        _ = Task.Run(externApi.startAsync);
    }
}
