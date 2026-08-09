using DataLibrary.ModelExtensions;
using Microsoft.Extensions.Logging;
using Models;
using System.Net;
using System.Net.Http.Json;

namespace DataLibrary.DataSources.ApiClients;

/// <summary>
/// Client for consuming the BeachEvent CQRS API
/// </summary>
public class BeachEventApiClient
{
    private HttpClient _publicClient;
    private HttpClient _apiClient;
    private string _publicBaseUrl;
    private string _apiBaseUrl;

    public BeachEventApiClient(ApiClientConfig clientConfig)
    {
        _publicClient = clientConfig.PublicClient;
        _apiClient = clientConfig.APIClient;

        _publicBaseUrl = $"{clientConfig.PublicUrl}/beachevents";
        _apiBaseUrl = $"{clientConfig.ApiUrl}/beachevents";
    }

    #region Query (Read Operations)

    /// <summary>
    /// Get all beach events
    /// </summary>
    public async Task<List<BeachEventBase>?> GetAllBeachEventsAsync()
    {
        try
        {
            var response = await _publicClient.GetAsync(_publicBaseUrl);
            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractBeachEventInfoList(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<BeachEventBase>();
        }
    }

    private static async Task<List<BeachEventBase>?> ExtractBeachEventInfoList(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to List<BeachEvent>
        var beachEvents = System.Text.Json.JsonSerializer.Deserialize<List<BeachEventBase>>(
            jsonResult
            , new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        TraceLogger.LogInformation($"Beach events count {beachEvents?.Count ?? 0}");

        return beachEvents ?? new List<BeachEventBase>();
    }

    /// <summary>
    /// Get a specific beach event by EventID
    /// </summary>
    public async Task<BeachEvent?> GetBeachEventByEventIdAsync(long eventId)
    {
        var response = await _apiClient.GetAsync($"{_apiBaseUrl}/event/{eventId}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        ApiClientHelper.ProcessResponseStatus(response);

        return await ExtractBeachEventInfo(response);
    }

    public async Task<BeachEvent?> GetBeachEventBySurveyIdAsync(long surveyId)
    {
        var response = await _publicClient.GetAsync($"{_publicBaseUrl}/survey/{surveyId}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        ApiClientHelper.ProcessResponseStatus(response);

        return await ExtractBeachEventInfo(response);

    }


    /// <summary>
    /// Get beach events by beach name
    /// </summary>
    public async Task<List<BeachEventBase>?> GetEventsForBeachAsync(string beachName)
    {
        try
        {
            // Sanitize and URL encode the beach name for safe API transmission
            if (string.IsNullOrWhiteSpace(beachName))
            {
                TraceLogger.LogWarning("BeachEventApiClient", "GetEventsForBeachAsync", "Beach name is null or empty");
                return new List<BeachEventBase>();
            }

            // Trim whitespace and URL encode the beach name
            string sanitizedBeachName = Uri.EscapeDataString(beachName.Trim());

            var response = await _publicClient.GetAsync($"{_publicBaseUrl}/beach/{sanitizedBeachName}");
            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractBeachEventInfoList(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<BeachEventBase>();
        }
    }

    private static async Task<BeachEvent?> ExtractBeachEventInfo(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to BeachEvent
        var beachEvent = System.Text.Json.JsonSerializer.Deserialize<BeachEvent>(
            jsonResult
            , new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return beachEvent;
    }

    #endregion

    #region Command (Write Operations)

    /// <summary>
    /// Create a new beach event
    /// </summary>
    public async Task<BeachEventBase?> CreateOrUpdateBeachEventAsync(BeachEventBase request)
    {
        try
        {
            var response = await _apiClient!.PostAsJsonAsync($"{_apiBaseUrl}", request);
            ApiClientHelper.ProcessResponseStatus(response);
            return await ExtractBeachEventBase(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
        }
        return null;
    }

    /// <summary>
    /// Delete a beach event
    /// </summary>
    public async Task<HttpStatusCode> DeleteBeachEventBySurveyIdAsync(long surveyId)
    {
        try
        {
            var response = await _apiClient.DeleteAsync($"{_apiBaseUrl}/survey/{surveyId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return response.StatusCode;
            }
            return response.StatusCode;
        }
        catch (Exception ex)
        {
            TraceLogger.LogError("BeachEventInfoApiClient", "DeleteBeachEventBySurveyIdAsync", ex.Message);
            return System.Net.HttpStatusCode.InternalServerError;
        }
    }

    public async Task<HttpStatusCode> DeleteBeachEventByEventIdAsync(long eventId)
    {
        try
        {
            var response = await _apiClient.DeleteAsync($"{_apiBaseUrl}/{eventId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return response.StatusCode;
            }
            return response.StatusCode;
        }
        catch (Exception ex)
        {
            TraceLogger.LogError("BeachEventInfoApiClient", "DeleteBeachEventByEventIdAsync", ex.Message);
            return System.Net.HttpStatusCode.InternalServerError;
        }
    }


    private static async Task<BeachEventBase?> ExtractBeachEventBase(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to BeachEvent
        var beachEvent = System.Text.Json.JsonSerializer.Deserialize<BeachEventBase>(
            jsonResult
            , new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return beachEvent;
    }

    #endregion
}
