namespace DataLibrary.ApiServices;

/// <summary>
/// Interface for secure storage operations in browser
/// </summary>
public interface ISecureStorageService
{
    /// <summary>
    /// Stores a value securely in browser storage
    /// </summary>
    Task SetItemAsync(string key, string value);

    /// <summary>
    /// Retrieves a value from secure storage
    /// </summary>
    Task<string?> GetItemAsync(string key);

    /// <summary>
    /// Removes an item from secure storage
    /// </summary>
    Task RemoveItemAsync(string key);

    /// <summary>
    /// Checks if a key exists in storage
    /// </summary>
    Task<bool> ContainsKeyAsync(string key);

    /// <summary>
    /// Clears all items from storage
    /// </summary>
    Task ClearAsync();
}
