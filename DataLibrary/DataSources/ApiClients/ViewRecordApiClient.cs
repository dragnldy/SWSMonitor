using DataLibrary.Models;
using System.Net;

namespace DataLibrary.DataSources.ApiClients;

public class ViewRecordApiClient
{
    private HttpClient _publicClient;
    private string _viewBaseUrl;

    public ViewRecordApiClient(ApiClientConfig clientConfig)
    {
        _publicClient = clientConfig.PublicClient;
        _viewBaseUrl = $"{clientConfig.PublicUrl.Replace("public","view")}";
    }


    #region Query Examples (Read Operations)

    /// <summary>
    /// Get all data records from the view
    /// </summary>
    public async Task<ViewRecord?> GetAllViewRecordsAsJsonAsync(string viewName)
    {
        var results = await GetViewRecordsAsync(viewName, "json");
        return !string.IsNullOrEmpty(results) ? ExtractViewRecords(results) : null;
    }
    public async Task<string?> GetAllViewRecordsAsCsvAsync(string viewName)
    {
        var results = await GetViewRecordsAsync(viewName, "csv");
        return results;
    }

    public async Task<string> GetViewRecordsAsync(string viewName, string? format = "json")
    {
        if (viewName.EndsWith("view", StringComparison.OrdinalIgnoreCase))
            viewName = viewName.Substring(0, viewName.Length - 4); // Remove "view" suffix if present

        var url = $"{_viewBaseUrl}/{viewName}?format={format}";
        var response = await _publicClient.GetAsync(url);
        ApiClientHelper.ProcessResponseStatus(response);
        if (response.StatusCode == HttpStatusCode.NotFound) return string.Empty;
        return await response.Content.ReadAsStringAsync();
    }

    private static ViewRecord? ExtractViewRecords(string jsonResult)
    {
        // Read response as string first
        var viewRecord = System.Text.Json.JsonSerializer.Deserialize<ViewRecord> (
            jsonResult,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );        
        return viewRecord;
    }
    #endregion
}