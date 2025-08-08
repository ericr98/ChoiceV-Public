using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ChoiceVExternApi;

public class DigestAuthHandler(DigestAuthHandlerConfig config) {
    private const string Realm = "ChoiceVRealm";

    private readonly Dictionary<string, DateTime> UsedNonces = new();

    /// <summary>
    /// Sends a Digest authentication challenge to the client.
    /// Adds a "WWW-Authenticate" header to the response with a generated nonce and opaque value.
    /// Sets the response status code to 401 Unauthorized.
    /// </summary>
    /// <param name="context">The HTTP listener context.</param>
    public async Task sendDigestChallengeAsync(HttpListenerContext context) {
        var response = context.Response;

        var nonce = generateSecureNonce();
        var opaque = generateSecureKey();
        var digestHeader = $"Digest realm=\"{Realm}\", qop=\"auth\", nonce=\"{nonce}\", opaque=\"{opaque}\"";

        response.AddHeader("WWW-Authenticate", digestHeader);
        response.StatusCode = (int)HttpStatusCode.Unauthorized;

        await response.OutputStream.FlushAsync().ConfigureAwait(false);
        response.Close();
    }

    /// <summary>
    /// Authenticates a request using the Digest authentication scheme.
    /// Validates the nonce and computes the expected response hash to compare with the provided response.
    /// </summary>
    /// <param name="authHeader">The authorization header from the request.</param>
    /// <param name="httpMethod">The HTTP method of the request.</param>
    /// <returns>True if authentication is successful; otherwise, false.</returns>
    public async Task<bool> authenticateAsync(string authHeader, string httpMethod) {
        if(config.neededAuthUsername is null || config.neededAuthPassword is null) return false;

        var digestAuthParams = parseDigestAuthHeader(authHeader, httpMethod);

        if(digestAuthParams is null) return false;

        if(!isNonceValid(digestAuthParams["nonce"])) return false;

        var ha1 = await calculateMd5HashAsync($"{config.neededAuthUsername}:{digestAuthParams["realm"]}:{config.neededAuthPassword}").ConfigureAwait(false);
        var ha2 = await calculateMd5HashAsync($"{digestAuthParams["method"]}:{digestAuthParams["uri"]}").ConfigureAwait(false);
        var expectedResponse = await calculateMd5HashAsync($"{ha1}:{digestAuthParams["nonce"]}:{ha2}").ConfigureAwait(false);

        return expectedResponse.Equals(digestAuthParams["response"], StringComparison.OrdinalIgnoreCase);
    }

    private string generateSecureNonce() {
        var timestamp = DateTime.UtcNow.ToString("o"); // ISO 8601 timestamp
        var randomBytes = new byte[16];
        using(var rng = RandomNumberGenerator.Create()) {
            rng.GetBytes(randomBytes);
        }

        var nonce = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{timestamp}:{Convert.ToBase64String(randomBytes)}"));

        UsedNonces[nonce] = DateTime.UtcNow.AddMinutes(5);

        return nonce;
    }

    private bool isNonceValid(string nonce) {
        if(!UsedNonces.TryGetValue(nonce, out var expiration)) {
            return false; // Nonce not found, potentially replayed
        }

        if(DateTime.UtcNow > expiration) {
            UsedNonces.Remove(nonce);
            return false;
        }

        return true;
    }

    private string generateSecureKey() {
        using var rng = RandomNumberGenerator.Create();

        byte[] randomBytes = new byte[16];
        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }

    private Dictionary<string, string>? parseDigestAuthHeader(string authHeader, string httpMethod) {
        var authParams = new Dictionary<string, string>();
        var parts = authHeader.Replace("Digest ", "").Split(',');

        foreach(var part in parts) {
            var indexOfEqual = part.IndexOf('=');

            if(indexOfEqual == -1) continue;

            var key = part[..indexOfEqual];
            var value = part[(indexOfEqual + 1)..];

            authParams[key.Trim()] = value.Trim().Trim('"');
        }

        if(!authParams.ContainsKey("realm") ||
            !authParams.ContainsKey("nonce") ||
            !authParams.ContainsKey("uri") ||
            !authParams.ContainsKey("response")) {
            return null;
        }

        authParams["method"] = httpMethod;

        return authParams;
    }

    /// <summary>
    /// Calculates the MD5 hash of a given input string asynchronously.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>The MD5 hash as a hexadecimal string.</returns>
    private async Task<string> calculateMd5HashAsync(string input) {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = await Task.Run(() => MD5.HashData(inputBytes));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}
