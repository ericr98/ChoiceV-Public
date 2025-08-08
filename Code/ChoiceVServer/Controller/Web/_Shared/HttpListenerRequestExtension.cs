#nullable enable
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using ChoiceVServer.Controller.Web._Shared.Records;

namespace ChoiceVServer.Controller.Web._Shared;

public static class HttpListenerRequestExtension {
    public static AuthenticateResponse? getAuthenticateResponse(this HttpListenerRequest request, Dictionary<string, WebhookData> webhookCallbacks) {
        var authHeader = request.Headers["Authorization"];
        if(authHeader is null) return null;

        var encodedCredentials = authHeader["Basic ".Length..].Trim();
        var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));

        var parts = credentials.Split(':');
        var reqUsername = parts[0];
        var reqPassword = parts[1];

        AuthenticateResponse authenticateResponse;
        if(webhookCallbacks.TryGetValue(reqUsername, out var webhook) && webhook.Password == reqPassword) {
            authenticateResponse = new AuthenticateResponse(true, reqUsername, webhook);
        } else {
            authenticateResponse = new AuthenticateResponse(false, "", null);
        }

        return authenticateResponse;
    }
}
