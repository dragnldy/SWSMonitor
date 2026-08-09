using DataLibrary.DataSources.ApiClients;

namespace DataLibrary.ApiServices;

/// <summary>
/// Service for managing API Key authentication in a secure manner
/// Handles storage, retrieval, and validation of API keys
/// </summary>
public class ApiKeyService
{
    // Get the API key from miscellaneous settings provided by the server at the beginning of app
    public bool UpdateApiKeySettings()
    {
        ClearApiKeySettings();
        var miscinfoa = StaticData.MiscInfo.GetEnvVariable("apsk");
        if (!string.IsNullOrEmpty(miscinfoa))
        {
            ApiClientHelper.API_KEY = miscinfoa;
            if (StaticData.DataSourceConfig is ApiClientConfig apiConfig)
            {
                apiConfig.APIClient.DefaultRequestHeaders.Add("X-API-Key", ApiClientHelper.API_KEY);
            }
            TraceLogger.LogInformation("API Key updated from server settings");
        }
        else
        {
            TraceLogger.LogWarningAuto("No API Key found in server settings");
            return false;
        }
        return true;
    }

    public void ClearApiKeySettings()
    {
        ApiClientHelper.API_KEY = string.Empty;
        if (StaticData.DataSourceConfig is ApiClientConfig apiConfig)
        {
            apiConfig.APIClient.DefaultRequestHeaders.Remove("X-API-Key");
        }
    }

    //private readonly ISecureStorageService _secureStorage;
    //private const string ApiKeyStorageKey = "ApiKey";
    //private const string ApiKeyExpirationKey = "ApiKeyExpiration";

    //// Optional: Set API key expiration time (e.g., 30 days)
    //private readonly TimeSpan _apiKeyLifetime = TimeSpan.FromDays(120);

    //public ApiKeyService(ISecureStorageService secureStorage)
    //{
    //    _secureStorage = secureStorage ?? throw new ArgumentNullException(nameof(secureStorage));
    //}

    ///// <summary>
    ///// Checks if an API key is currently stored and valid
    ///// </summary>
    //public async Task<bool> IsApiKeyConfiguredAsync()
    //{
    //    var apiKey = await _secureStorage.GetItemAsync(ApiKeyStorageKey);

    //    if (string.IsNullOrEmpty(apiKey))
    //        return false;

    //    // Check if API key has expired
    //    var expirationString = await _secureStorage.GetItemAsync(ApiKeyExpirationKey);
    //    if (!string.IsNullOrEmpty(expirationString))
    //    {
    //        if (DateTime.TryParse(expirationString, out var expiration))
    //        {
    //            if (DateTime.UtcNow > expiration)
    //            {
    //                // API key has expired, remove it
    //                await ClearApiKeyAsync();
    //                return false;
    //            }
    //        }
    //    }

    //    return true;
    //}

    ///// <summary>
    ///// Stores the API key securely and applies it to the API client
    ///// </summary>
    //public async Task SetApiKeyAsync(string apiKey)
    //{
    //    if (string.IsNullOrWhiteSpace(apiKey))
    //        throw new ArgumentException("API key cannot be empty", nameof(apiKey));

    //    // Store the API key
    //    await _secureStorage.SetItemAsync(ApiKeyStorageKey, apiKey);

    //    // Store expiration date
    //    var expiration = DateTime.UtcNow.Add(_apiKeyLifetime);
    //    await _secureStorage.SetItemAsync(ApiKeyExpirationKey, expiration.ToString("O"));

    //    // Apply to API client
    //    ApiClientHelper.API_KEY = apiKey;

    //    TraceLogger.LogInformation("API Key configured successfully");
    //}

    ///// <summary>
    ///// Loads the API key from secure storage and applies it to the API client
    ///// Returns true if successful, false if no key is found or key is expired
    ///// </summary>
    //public async Task<bool> LoadApiKeyAsync()
    //{
    //    if (!await IsApiKeyConfiguredAsync())
    //    {
    //        TraceLogger.LogInformation("No valid API key found in storage");
    //        return false;
    //    }

    //    var apiKey = await _secureStorage.GetItemAsync(ApiKeyStorageKey);

    //    if (!string.IsNullOrEmpty(apiKey))
    //    {
    //        ApiClientHelper.API_KEY = apiKey;
    //        TraceLogger.LogInformation("API Key loaded from storage");
    //        return true;
    //    }

    //    return false;
    //}

    ///// <summary>
    ///// Removes the API key from storage and clears it from the API client
    ///// </summary>
    //public async Task ClearApiKeyAsync()
    //{
    //    await _secureStorage.RemoveItemAsync(ApiKeyStorageKey);
    //    await _secureStorage.RemoveItemAsync(ApiKeyExpirationKey);
    //    ApiClientHelper.API_KEY = string.Empty;

    //    TraceLogger.LogInformation("API Key cleared");
    //}

    ///// <summary>
    ///// Gets the current API key (if any) without loading from storage
    ///// Returns the in-memory value from ApiClientHelper
    ///// </summary>
    //public string? GetCurrentApiKey()
    //{
    //    return string.IsNullOrEmpty(ApiClientHelper.API_KEY)
    //        ? null
    //        : ApiClientHelper.API_KEY;
    //}

    ///// <summary>
    ///// Validates the API key by making a test request to the API
    ///// </summary>
    //public async Task<bool> ValidateApiKeyAsync()
    //{
    //    if (string.IsNullOrEmpty(ApiClientHelper.API_KEY))
    //        return false;
    //    return true;
    //    //try
    //    //{
    //    //    // Make a test request to validate the API key
    //    //    // You can create a specific endpoint for this, or use an existing one
    //    //    var client = new BeachDataApiClient(
    //    //        new System.Net.Http.HttpClient 
    //    //        { 
    //    //            BaseAddress = new Uri(ApiClientHelper.API_BASE_URL) 
    //    //        });

    //    //    // Try to get data - if API key is invalid, this will throw
    //    //    var beaches = await client.DeleteBeachAsync(999999);
    //    //    return beaches != null;
    //    //}
    //    //catch (Exception ex)
    //    //{
    //    //    TraceLogger.LogErrorAuto($"API key validation failed: {ex.Message}");
    //    //    return false;
    //    //}
    //}

    ///// <summary>
    ///// Updates an existing API key with a new value
    ///// </summary>
    //public async Task<bool> UpdateApiKeyAsync(string newApiKey)
    //{
    //    if (string.IsNullOrWhiteSpace(newApiKey))
    //        return false;

    //    await SetApiKeyAsync(newApiKey);

    //    return true;

    //    // Optionally validate the new key
    //    // return await ValidateApiKeyAsync();
    //}
}
