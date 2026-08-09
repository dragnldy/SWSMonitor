using DataLibrary.Models;
using System.Net;
using System.Net.Http.Json;

namespace DataLibrary.DataSources.ApiClients;

/// <summary>
/// Client for consuming the CityStates CQRS API
/// Note: CityStates is typically a view, not a table
/// </summary>
public class CityStateApiClient
{
    private HttpClient _publicClient;
    private HttpClient _apiClient;
    private string _publicBaseUrl;
    private string _apiBaseUrl;

    public CityStateApiClient(ApiClientConfig clientConfig)
    {
        _publicClient = clientConfig.PublicClient;
        _apiClient = clientConfig.APIClient;

        _publicBaseUrl = $"{clientConfig.PublicUrl}/citystates";
        _apiBaseUrl = $"{clientConfig.ApiUrl}/citystates";
    }

    #region Query Examples (Read Operations)

    /// <summary>
    /// Get all city-state combinations
    /// </summary>
    public async Task<List<CityState>?> GetAllCityStatesAsync()
    {
        try
        {
            var response = await _publicClient.GetAsync(_publicBaseUrl);
            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractCityStateList(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<CityState>();
        }
    }

    private static async Task<List<CityState>?> ExtractCityStateList(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to List<CityState>
        var cityStates = System.Text.Json.JsonSerializer.Deserialize<List<CityState>>(
            jsonResult,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        // TraceLogger.LogInformation(jsonResult);
        TraceLogger.LogInformation($"CityStates count {cityStates?.Count ?? 0}");

        return cityStates ?? new List<CityState>();
    }

    /// <summary>
    /// Get a specific city-state by ID
    /// </summary>
    public async Task<CityState?> GetCityStateByIdAsync(int id)
    {
        try
        {
            var response = await _publicClient.GetAsync($"{_publicBaseUrl}/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractCityState(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return null;
        }
    }

    private static async Task<CityState?> ExtractCityState(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to CityState
        var cityState = System.Text.Json.JsonSerializer.Deserialize<CityState>(
            jsonResult,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return cityState;
    }

    #endregion

    #region Command Examples (Write Operations)

    /// <summary>
    /// Create a new city-state entry
    /// Note: May not be supported if CityStates is a read-only view
    /// </summary>
    public async Task<CityState?> UpdateOrCreateCityStateAsync(CityState cityState)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _apiBaseUrl)
            {
                Content = JsonContent.Create(cityState)
            };
            ApiClientHelper.AddApiKeyHeader(httpRequest);

            var response = await _apiClient.SendAsync(httpRequest);
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                TraceLogger.LogErrorAuto($"Attempt to create/update failed for city {cityState.City} with ID {cityState.ID}");
                return null;
            }
            ApiClientHelper.ProcessResponseStatus(response);
            return await ExtractCityState(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
        }
        return null;
    }


    /// <summary>
    /// Delete a city-state entry
    /// Note: May not be supported if CityStates is a read-only view
    /// </summary>
    public async Task<HttpStatusCode> DeleteCityStateAsync(int id)
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
            TraceLogger.LogError("CityStateApiClient", "DeleteCityStateAsync", ex.Message);
            return HttpStatusCode.InternalServerError;
        }
    }

    #endregion

}
