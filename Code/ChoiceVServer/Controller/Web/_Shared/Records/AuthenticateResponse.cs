namespace ChoiceVServer.Controller.Web._Shared.Records;

public record AuthenticateResponse(bool Authenticated, string Username, WebhookData? Webhook);