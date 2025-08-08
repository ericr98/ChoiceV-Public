#nullable enable
using System.Net;
using System.Text;
using System.Text.Json;

namespace ChoiceVExternApi.Shared;

public static class HttpListenerContextExtension {
    public static async Task sendResponseAsync(this HttpListenerContext context, object? responseObject, HttpStatusCode statusCode = HttpStatusCode.OK) {
        var response = context.Response;

        response.StatusCode = (int)statusCode;

        response.ContentType = "application/json";
        response.ContentEncoding = Encoding.UTF8;

        byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(responseObject));
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
        response.OutputStream.Close();
    }

    public static string? getQueryParameter(this HttpListenerContext context, string key) {
        var queryParams = context.parseQueryParams();
        var value = queryParams?.GetValueOrDefault(key);
        return value;
    }

    public static T? getQueryParameter<T>(this HttpListenerContext context, string key) {
        var value = context.getQueryParameter(key);
        if(value == null) {
            return default;
        }

        try {
            return (T)Convert.ChangeType(value, typeof(T));
        } catch {
            return default;
        }
    }

    private static Dictionary<string, string>? parseQueryParams(this HttpListenerContext context) {
        var query = context.getQueryString();

        return query?.TrimStart('?')
            .Split('&')
            .Select(param => param.Split('='))
            .ToDictionary(pair => pair[0], pair => pair[1]);
    }

    public static string? getQueryString(this HttpListenerContext context) {
        return context.Request.Url?.Query;
    }

    public static bool isQueryNullOrEmpty(this HttpListenerContext context) {
        return string.IsNullOrEmpty(context.getQueryString());
    }

    public static async Task<T?> getRequestBodyAsync<T>(this HttpListenerContext context) {
        if(!context.Request.HasEntityBody) {
            return default;
        } 
        
        using var bodyReader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
        var requestBody = await bodyReader.ReadToEndAsync().ConfigureAwait(false);

        var model = JsonSerializer.Deserialize<T>(requestBody);
        return model;
    }
}
