using System.Text.Json.Serialization;

namespace DataLibrary.ModelExtensions;

// Holds application settings that can be used across the application
public class AppSettings
{

    public string? SyncfusionLicense { get; set; } = null; // Syncfusion license key, if any
    public string? Platform { get; set; } = "WebAssembly"; // Default platform
    public string? UseAirtable { get; set; } = "true"; // Default to true, can be overridden by appsettings.json
    public string? UseJson { get; set; } = "false"; // Default to false, can be overridden by appsettings.json
    public string? UseSql { get; set; } = "false"; // Default to false, can be overridden by appsettings.json
    public string? DefaultConnectionString { get; set; } = null; // Default connection string for SQL databases, if needed

    public string? ApiKey { get; set; } = null; // API key for external services, if needed
    public string? BaseId { get; set; } = null; // Base ID for Airtable or other services, if needed

    public bool isTrue(string? value)
    {
        return !string.IsNullOrEmpty(value) && value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}