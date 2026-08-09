using System.Runtime.CompilerServices;

namespace DataLibrary.DataSources.ApiClients;

public static class ApiClientHelper
{

    public static string BASE_URL = "https://localhost:7193/";
    public static string PUBLIC_BASE_URL = $"{BASE_URL}/public/";
    public static string API_BASE_URL = $"{BASE_URL}/api/";
    public static string API_KEY = ApiInit.TestApiKey; // Set this from configuration or user settings
    public const string API_KEY_HEADER_NAME = "X-Api-Key";
    public static void ProcessResponseStatus(HttpResponseMessage response,
        [CallerFilePath] string file = "",
        [CallerMemberName] string method = "",
        [CallerLineNumber] int line = 0)
    {
        string request = "";
        if (response is null || response.RequestMessage is null)
        {
            TraceLogger.LogErrorAuto("Response supplied was null");
        }
        else
        {
            request = $"Method: {response?.RequestMessage.Method} Uri: {response?.RequestMessage.RequestUri}";
            try
            {
                response?.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {

                TraceLogger.LogError(Path.GetFileName(file), method, request);
                TraceLogger.LogError(Path.GetFileName(file), method, ex.Message);
                throw new Exception("Error processing response from API. See log for details.", ex);
            }
        }
    }

    /// <summary>
    /// Adds API Key header to HttpRequestMessage if API Key is configured
    /// </summary>
    public static void AddApiKeyHeader(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(API_KEY))
        {
            request.Headers.Add(API_KEY_HEADER_NAME, API_KEY);
        }
    }
}
