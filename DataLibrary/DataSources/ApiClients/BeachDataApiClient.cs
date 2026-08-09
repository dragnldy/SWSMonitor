using Models;
using System.Net;
using System.Net.Http.Json;

namespace DataLibrary.DataSources.ApiClients;

/// <summary>
/// Example client demonstrating how to consume the BeachData CQRS API
/// </summary>
public class BeachDataApiClient
{
    private HttpClient _publicClient;
    private HttpClient _apiClient;
    private string _publicBaseUrl;
    private string _apiBaseUrl;

    public BeachDataApiClient(ApiClientConfig clientConfig)
    {
        _publicClient = clientConfig.PublicClient;
        _apiClient = clientConfig.APIClient;

        _publicBaseUrl = $"{clientConfig.PublicUrl}/beaches";
        _apiBaseUrl = $"{clientConfig.ApiUrl}/beaches";
    }

    #region Query (Read Operations)

        /// <summary>
        /// Get all beaches
        /// </summary>
    public async Task<List<BeachData>?> GetAllBeachesAsync()
    {
        try
        {
            var response = await _publicClient.GetAsync(_publicBaseUrl);
            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractBeachDataList(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<BeachData>();
        }
    }

    private static async Task<List<BeachData>?> ExtractBeachDataList(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to List<BeachData>
        var beaches = System.Text.Json.JsonSerializer.Deserialize<List<BeachData>>(
            jsonResult
            , new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        // TraceLogger.LogInformation(jsonResult);
        TraceLogger.LogInformation($"Beaches count {beaches.Count}");

        return beaches ?? new List<BeachData>();
    }

    /// <summary>
    /// Get a specific beach by ID
    /// </summary>
    public async Task<BeachData?> GetBeachByIdAsync(int id)
    {
        var response = await _publicClient.GetAsync($"{_publicBaseUrl}/{id}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        ApiClientHelper.ProcessResponseStatus(response);

        return await ExtractBeachData(response);
    }

    private static async Task<BeachData?> ExtractBeachData(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to BeachData
        var beach = System.Text.Json.JsonSerializer.Deserialize<BeachData>(
            jsonResult
            , new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return beach;
    }

    #endregion

    #region Command Examples (Write Operations)

    public async Task<BeachData> CreateBeachAsync(BeachData request)
    {
        try
        {
            var response = await _apiClient!.PostAsJsonAsync($"{_apiBaseUrl}", request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                TraceLogger.LogWarningAuto($"Attempt to create duplicate Beach with name {request.BeachName} not found or bad request");
                return null;
            }

            ApiClientHelper.ProcessResponseStatus(response);
            return await ExtractBeachData(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
        }
        return null;
    }

    /// <summary>
    /// Update an existing beach
    /// </summary>
    public async Task<BeachData?> UpdateBeachAsync(int id, BeachData request)
    {
        // Ensure the ID matches
        try
        {

            if (id != request.ID)
            {
                throw new ArgumentException("ID mismatch between route and request body");
            }

            var response = await _apiClient!.PutAsJsonAsync($"{_apiBaseUrl}/{id}", request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                TraceLogger.LogWarningAuto($"Beach with ID {id} not found or bad request");
                return null;
            }
            ApiClientHelper.ProcessResponseStatus(response);
            return await ExtractBeachData(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
        }
        return null;
    }

    /// <summary>
    /// Delete a beach
    /// </summary>
    public async Task<HttpStatusCode> DeleteBeachAsync(int id)
    {
        try
        {
            var response = await _apiClient.DeleteAsync($"{_apiBaseUrl}/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return response.StatusCode;
            }
            return response.StatusCode;
        }
        catch (Exception ex)
        {
            TraceLogger.LogError("BeachDataApiClient", "DeleteBeachAsync", ex.Message);
            return System.Net.HttpStatusCode.InternalServerError;
        }
    }

    #endregion
}


