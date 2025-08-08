namespace ChoiceVExternApi;

public record RouteHandlerConfig(
    string routePrefix, 
    ulong? guildId,
    string? botToken, 
    bool isDevServer
);
