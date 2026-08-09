using DataLibrary;
using System.Runtime.InteropServices.JavaScript;

namespace DataLibrary.ApiServices;

/// <summary>
/// Browser-based secure storage using Web Storage API (localStorage with Base64 encoding)
/// For WASM applications - uses JavaScript interop with main.js
/// </summary>
public partial class BrowserSecureStorageService : ISecureStorageService
{
    private const string StoragePrefix = "BeachSurvey_Secure_";

    public async Task SetItemAsync(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        try
        {
            // Encode the value to Base64 for basic obfuscation
            // Note: This is NOT true encryption, just obfuscation
            var encodedValue = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value));

            // Call JavaScript localStorage.setItem via interop
            await SetLocalStorageItem(StoragePrefix + key, encodedValue);

            TraceLogger.LogInformation($"Stored item with key: {StoragePrefix + key}");
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto($"Error storing item: {ex.Message}");
            throw;
        }
    }

    public async Task<string?> GetItemAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        try
        {
            // Call JavaScript localStorage.getItem via interop
            var encodedValue = await GetLocalStorageItem(StoragePrefix + key);

            if (string.IsNullOrEmpty(encodedValue))
                return null;

            // Decode from Base64
            var decodedBytes = Convert.FromBase64String(encodedValue);
            return System.Text.Encoding.UTF8.GetString(decodedBytes);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto($"Error retrieving item: {ex.Message}");
            return null;
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        try
        {
            // Call JavaScript localStorage.removeItem via interop
            await RemoveLocalStorageItem(StoragePrefix + key);

            TraceLogger.LogInformation($"Removed item with key: {StoragePrefix + key}");
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto($"Error removing item: {ex.Message}");
        }
    }

    public async Task<bool> ContainsKeyAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        try
        {
            var value = await GetItemAsync(key);
            return !string.IsNullOrEmpty(value);
        }
        catch
        {
            return false;
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            // Call JavaScript function to clear all items with prefix
            await ClearStorageWithPrefix(StoragePrefix);

            TraceLogger.LogInformation($"Cleared all items with prefix: {StoragePrefix}");
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto($"Error clearing storage: {ex.Message}");
        }
    }

    #region JavaScript Interop - Import functions from storage.js

    /// <summary>
    /// Calls setItem function in storage.js
    /// </summary>
    [JSImport("setItem", "storage.js")]
    private static partial Task SetLocalStorageItem(string key, string value);

    /// <summary>
    /// Calls getItem function in storage.js
    /// </summary>
    [JSImport("getItem", "storage.js")]
    private static partial Task<string?> GetLocalStorageItem(string key);

    /// <summary>
    /// Calls removeItem function in storage.js
    /// </summary>
    [JSImport("removeItem", "storage.js")]
    private static partial Task RemoveLocalStorageItem(string key);

    /// <summary>
    /// Calls clearStorageWithPrefix function in storage.js
    /// </summary>
    [JSImport("clearStorageWithPrefix", "storage.js")]
    private static partial Task ClearStorageWithPrefix(string prefix);

    #endregion
}
