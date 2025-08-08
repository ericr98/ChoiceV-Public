using ChoiceVServer.Base;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using ChoiceVServer.Controller.Web._Shared;
using ChoiceVServer.Controller.Web._Shared.Records;

namespace ChoiceVServer.Controller.Web {
    public delegate void WebhookCallback(string data);

    public class WebhookController : ChoiceVScript {
        private static readonly Dictionary<string, WebhookData> WebhookCallbacks = new();

        public WebhookController() {
            var thread = new Thread(startWebhookListener);
            thread.Start();
        }

        public static void registerWebhookCallback(string userName, string password, WebhookCallback callback) {
            WebhookCallbacks[userName] = new WebhookData(userName, password, callback);
        }

        private static void startWebhookListener() {
            //TODO Add HTTPS support
            using var listener = new HttpListener();
            listener.Prefixes.Add(Config.WebhookIp);
            listener.Start();

            while(true) {
                var context = listener.GetContext();
                var request = context.Request;

                try {
                    var authenticateResponse = request.getAuthenticateResponse(WebhookCallbacks);

                    if(authenticateResponse?.Webhook is null || !authenticateResponse.Authenticated) {
                        Logger.logError("Unauthorized webhook request received from: " + request.RemoteEndPoint);
                        return;
                    }

                    Logger.logInfo(LogCategory.System, LogActionType.Event, $"Webhook received from: {request.RemoteEndPoint}");

                    string requestBody;
                    using(var body = request.InputStream) {
                        using(var reader = new System.IO.StreamReader(body, request.ContentEncoding)) {
                            requestBody = reader.ReadToEnd();
                        }
                    }

                    authenticateResponse.Webhook.Callback(requestBody);

                    context.Response.Close();
                } catch(Exception e) {
                    Logger.logError("Error while processing webhook request: " + e.Message);
                }
            }
        }
    }
}
