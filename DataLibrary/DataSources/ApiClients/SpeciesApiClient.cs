using Models;
using System.Net;
using System.Net.Http.Json;

namespace DataLibrary.DataSources.ApiClients;

/// <summary>
/// Client for consuming the Species CQRS API
/// </summary>
public class SpeciesApiClient
{
    private HttpClient _publicClient;
    private HttpClient _apiClient;
    private string _publicBaseUrl;  
    private string _apiBaseUrl;

    public SpeciesApiClient(ApiClientConfig clientConfig)
    {
        _publicClient = clientConfig.PublicClient;
        _apiClient = clientConfig.APIClient;

        _publicBaseUrl = $"{clientConfig.PublicUrl}/species";
        _apiBaseUrl = $"{clientConfig.ApiUrl}/species";
    }
    #region Query Examples (Read Operations)

        /// <summary>
        /// Get all species
        /// </summary>
        public async Task<List<Species>?> GetAllSpeciesAsync()
    {
        try
        {
            var response = await _publicClient.GetAsync(_publicBaseUrl);
            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractSpeciesList(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<Species>();
        }
    }

    private static async Task<List<Species>?> ExtractSpeciesList(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to List<Species>
        var speciesList = System.Text.Json.JsonSerializer.Deserialize<List<Species>>(
            jsonResult,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        // TraceLogger.LogInformation(jsonResult);
        TraceLogger.LogInformation($"Species count {speciesList?.Count ?? 0}");

        return speciesList ?? new List<Species>();
    }

    /// <summary>
    /// Get a specific species by ID
    /// </summary>
    public async Task<Species?> GetSpeciesByIdAsync(long id)
    {
        try
        {
            var response = await _publicClient.GetAsync($"{_publicBaseUrl}/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractSpecies(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return null;
        }
    }

    private static async Task<Species?> ExtractSpecies(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to Species
        var species = System.Text.Json.JsonSerializer.Deserialize<Species>(
            jsonResult,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return species;
    }
    internal async Task<Species?> GetSpeciesByNameAsync(string scientificName)
    {
        if (!MySqlHelperUtils.IsSafeFromInjectionString(scientificName))
        {
            TraceLogger.LogWarningAuto($"Input contains potentially unsafe characters {scientificName}");
            return null;
        }
        var sanitizedScientificName = MySqlHelperUtils.SanitizeInputSqlString(scientificName);
        var escapedName = Uri.EscapeDataString(sanitizedScientificName);
        var response = await _publicClient.GetAsync($"{_publicBaseUrl}/name/{escapedName}");

        try
        { 
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractSpecies(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return null;
        }
    }

    #endregion

    #region Command Examples (Write Operations)

    /// <summary>
    /// Create a new species
    /// </summary>
    public async Task<Species?> CreateOrUpdateSpeciesAsync(Species request)
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
            return await ExtractSpecies(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
        }
        return null;
    }


    /// <summary>
    /// Delete a species
    /// </summary>
    public async Task<HttpStatusCode> DeleteSpeciesAsync(int id)
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
            TraceLogger.LogError("SpeciesApiClient", "DeleteSpeciesAsync", ex.Message);
            return HttpStatusCode.InternalServerError;
        }
    }

    /// <summary>
    /// Mark species as used by surveys
    /// </summary>
    public async Task<Species?> MarkAsUsedBySurveysAsync(int id)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"{_apiBaseUrl}/{id}/markused")
            {
                Content = JsonContent.Create(new { UsedBySurveys = 1 })
            };
            ApiClientHelper.AddApiKeyHeader(httpRequest);

            var response = await _apiClient.SendAsync(httpRequest);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException($"Species with ID {id} not found");
            }

            ApiClientHelper.ProcessResponseStatus(response);
            return await ExtractSpecies(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return null;
        }
    }

    #endregion
}
