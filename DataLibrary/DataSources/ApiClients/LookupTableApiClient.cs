using Models;
using System.Net;
using System.Net.Http.Json;

namespace DataLibrary.DataSources.ApiClients;

/// <summary>
/// Client for consuming the LookupTable CQRS API
/// </summary>
public class LookupTableApiClient
{
    private HttpClient _publicClient;
    private HttpClient _apiClient;
    private string _publicBaseUrl;
    private string _apiBaseUrl;

    public LookupTableApiClient(ApiClientConfig clientConfig)
    {
        _publicClient = clientConfig.PublicClient;
        _apiClient = clientConfig.APIClient;

        _publicBaseUrl = $"{clientConfig.PublicUrl}/lookuptables";
        _apiBaseUrl = $"{clientConfig.ApiUrl}/lookuptables";
    }

    #region Query Examples (Read Operations)

    /// <summary>
    /// Get all lookup tables
    /// </summary>
    public async Task<List<LookupTable>?> GetAllLookupTablesAsync()
    {
        try
        {
            var response = await _publicClient.GetAsync(_publicBaseUrl);
            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractLookupTableList(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<LookupTable>();
        }
    }

    private static async Task<List<LookupTable>?> ExtractLookupTableList(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to List<LookupTable>
        var lookupTables = System.Text.Json.JsonSerializer.Deserialize<List<LookupTable>>(
            jsonResult,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        // TraceLogger.LogInformation(jsonResult);
        TraceLogger.LogInformation($"LookupTables count {lookupTables?.Count ?? 0}");

        return lookupTables ?? new List<LookupTable>();
    }

    /// <summary>
    /// Get a specific lookup table by ID
    /// </summary>
    public async Task<LookupTable?> GetLookupTableByIdAsync(int id)
    {
        try
        {
            var response = await _publicClient.GetAsync($"{_publicBaseUrl}/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractLookupTable(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return null;
        }
    }

    private static async Task<LookupTable?> ExtractLookupTable(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to LookupTable
        var lookupTable = System.Text.Json.JsonSerializer.Deserialize<LookupTable>(
            jsonResult,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return lookupTable;
    }

    /// <summary>
    /// Get lookup tables by category
    /// </summary>
    public async Task<List<LookupTable>?> GetLookupTablesByCategoryAsync(string category)
    {
        try
        {
            var response = await _publicClient.GetAsync($"{_publicBaseUrl}/category/{Uri.EscapeDataString(category)}");
            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractLookupTableList(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<LookupTable>();
        }
    }

    /// <summary>
    /// Get all distinct lookup categories
    /// </summary>
    public async Task<List<string>?> GetDistinctCategoriesAsync()
    {
        try
        {
            var response = await _publicClient.GetAsync($"{_publicBaseUrl}/categories");
            ApiClientHelper.ProcessResponseStatus(response);

            string jsonResult = await response.Content.ReadAsStringAsync();
            var categories = System.Text.Json.JsonSerializer.Deserialize<List<string>>(
                jsonResult,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return categories ?? new List<string>();
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<string>();
        }
    }

    /// <summary>
    /// Get lookup tables with taxonomy data (where LookupExtra is not empty)
    /// </summary>
    public async Task<List<LookupTable>?> GetLookupTablesWithTaxonomyAsync()
    {
        try
        {
            var response = await _publicClient.GetAsync($"{_publicBaseUrl}/taxonomy");
            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractLookupTableList(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<LookupTable>();
        }
    }

    #endregion

    #region Command Examples (Write Operations)

    /// <summary>
    /// Create a new lookup table entry
    /// </summary>
    public async Task<LookupTable?> CreateLookupTableAsync(LookupTable request)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _apiBaseUrl)
            {
                Content = JsonContent.Create(request)
            };
            ApiClientHelper.AddApiKeyHeader(httpRequest);

            var response = await _apiClient.SendAsync(httpRequest);
            ApiClientHelper.ProcessResponseStatus(response);
            return await ExtractLookupTable(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
        }
        return null;
    }

    /// <summary>
    /// Update an existing lookup table entry
    /// </summary>
    public async Task<LookupTable?> UpdateLookupTableAsync(int id, LookupTable request)
    {
        try
        {
            // Ensure the ID matches
            if (id != request.ID)
            {
                throw new ArgumentException("ID mismatch between route and request body");
            }

            var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"{_apiBaseUrl}/{id}")
            {
                Content = JsonContent.Create(request)
            };
            ApiClientHelper.AddApiKeyHeader(httpRequest);

            var response = await _apiClient.SendAsync(httpRequest);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException($"LookupTable with ID {id} not found");
            }

            ApiClientHelper.ProcessResponseStatus(response);
            return await ExtractLookupTable(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return null;
        }
    }

    /// <summary>
    /// Delete a lookup table entry
    /// </summary>
    public async Task<HttpStatusCode> DeleteLookupTableAsync(int id)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/{id}");
            ApiClientHelper.AddApiKeyHeader(httpRequest);

            var response = await _apiClient.SendAsync(httpRequest);

            if (response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return response.StatusCode;
            }
            ApiClientHelper.ProcessResponseStatus(response);
            return response.StatusCode;
        }
        catch (Exception ex)
        {
            TraceLogger.LogError("LookupTableApiClient", "DeleteLookupTableAsync", ex.Message);
            return HttpStatusCode.InternalServerError;
        }
    }

    #endregion

}
