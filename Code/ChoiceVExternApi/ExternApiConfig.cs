using System.Reflection;

namespace ChoiceVExternApi;

public record ExternApiConfig(
    string? apiUrl,
    string? neededAuthUsername,
    string? neededAuthPassword, 
    string? discordBotToken,
    ulong? discordServerId, 
    bool isDevServer,
    Assembly handlerAssembly
);
