using DataLibrary.DataSources.CloudAuth;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices.JavaScript;

namespace DataLibrary.Utilities;

public static partial class GoogleCredentials
{
    /// <summary>
    /// Handles the Google FedCM credential response and exchanges the authorization code for user info
    /// </summary>
    public static async Task<GoogleAuthUser?> HandleGoogleCredentialAsync(string credential)
    {
        GoogleAuthUser? user = null;

        string cs = StaticData.MiscInfo.GetEnvVariable("gakcs");
        try
        {
            if (string.IsNullOrWhiteSpace(credential))
            {
                TraceLogger.LogErrorAuto("Empty credential received from Google FedCM");
                return null;
            }

            // The credential from FedCM is an authorization code that needs to be exchanged
            // Get Google OAuth configuration
            var cloudAuthConfig = StaticData.ServiceProvider?.GetService<ICloudAuthConfig>() as GoogleAuthConfig;
            if (cloudAuthConfig == null)
            {
                TraceLogger.LogErrorAuto("Google Auth Config not available");
                return null;
            }

            var clientId = cloudAuthConfig.HGCSClientID;
            if (StaticData.RunningInBrowser)
                clientId = cloudAuthConfig.HGCSWebClientID;

            var clientSecret = cloudAuthConfig.HGCSUnpacked;

            if (string.IsNullOrWhiteSpace(clientId))
            {
                TraceLogger.LogErrorAuto("Google Client ID not configured");
                return null;
            }

            var code = ParseCodeFromCredential(credential);
            // Exchange authorization code for access token
            var tokenResponse = await ExchangeAuthorizationCodeAsync(code, clientId, cs);

            if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                TraceLogger.LogErrorAuto("Failed to exchange authorization code for access token");
                return null;
            }

            // Get user info using the access token
            GoogleAuthUser? userInfo = await GetUserInfoAsync(tokenResponse.AccessToken);

            if (userInfo != null)
            {
                TraceLogger.LogInformation($"Successfully authenticated user: {userInfo.Email}");
                return userInfo;
            }

            return null;
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto($"Error handling Google credential: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    public static string ParseCodeFromCredential(string credential)
    {
        string[] credentialParts = credential.Split(',');
        foreach (var part in credentialParts)
        {
            if (part.ToLower().Contains("code"))
            {
                string[] codeParts = part.Split(':', StringSplitOptions.RemoveEmptyEntries);
                var code = codeParts[1].Trim(new char[] { '\"', ' ' }).Trim();
                return code;
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// Exchanges the authorization code for an access token
    /// </summary>
    private static async Task<GoogleTokenResponse?> ExchangeAuthorizationCodeAsync(
        string authorizationCode,
        string clientId,
        string clientSecret)
    {
        try
        {
            using var httpClient = new HttpClient();

            var requestContent = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("redirect_uri", "postmessage"), // For FedCM
            new KeyValuePair<string, string>("grant_type", "authorization_code")
        });

            var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", requestContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                TraceLogger.LogErrorAuto($"Token exchange failed: {response.StatusCode} - {errorContent}");
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<GoogleTokenResponse>(responseContent);

            return tokenResponse;
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto($"Error exchanging authorization code: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets user information using the access token
    /// </summary>
    private static async Task<GoogleAuthUser?> GetUserInfoAsync(string accessToken)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                TraceLogger.LogErrorAuto($"Failed to get user info: {response.StatusCode} - {errorContent}");
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var userInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<GoogleAuthUser>(responseContent);

            return userInfo;
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto($"Error getting user info: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Response from Google OAuth token endpoint
    /// </summary>
    private class GoogleTokenResponse
    {
        [Newtonsoft.Json.JsonProperty("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [Newtonsoft.Json.JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [Newtonsoft.Json.JsonProperty("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [Newtonsoft.Json.JsonProperty("scope")]
        public string Scope { get; set; } = string.Empty;

        [Newtonsoft.Json.JsonProperty("id_token")]
        public string? IdToken { get; set; }

        [Newtonsoft.Json.JsonProperty("refresh_token")]
        public string? RefreshToken { get; set; }
    }
}