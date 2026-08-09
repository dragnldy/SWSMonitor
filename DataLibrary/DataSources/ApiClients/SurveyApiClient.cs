using DataLibrary.DataSources.FeatureClients;
using Models;
using System.Net;
using System.Net.Http.Json;

namespace DataLibrary.DataSources.ApiClients;

/// <summary>
/// Client for consuming the Survey CQRS API
/// </summary>
public class SurveyApiClient
{
    private HttpClient _publicClient;
    private HttpClient _apiClient;
    private string _publicBaseUrl;
    private string _apiBaseUrl;

    public SurveyApiClient(ApiClientConfig clientConfig)
    {
        _publicClient = clientConfig.PublicClient;
        _apiClient = clientConfig.APIClient;

        _publicBaseUrl = $"{clientConfig.PublicUrl}/surveys";
        _apiBaseUrl = $"{clientConfig.ApiUrl}/surveys";
    }

    #region Query Examples (Read Operations)

    /// <summary>
    /// Get all surveys
    /// </summary>
    public async Task<List<SurveyBase>?> GetAllSurveysAsync()
    {
        try
        {
            var response = await _publicClient.GetAsync(_publicBaseUrl);
            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractSurveyBaseList(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<SurveyBase>();
        }
    }

    private static async Task<List<SurveyBase>?> ExtractSurveyBaseList(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to List<SurveyBase>
        var surveys = System.Text.Json.JsonSerializer.Deserialize<List<SurveyBase>>(
            jsonResult,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        //TraceLogger.LogInformation(jsonResult);
        TraceLogger.LogInformation($"Surveys count {surveys?.Count ?? 0}");

        return surveys ?? new List<SurveyBase>();
    }

    /// <summary>
    /// Get a specific survey by ID
    /// </summary>
    public async Task<SurveyBase?> GetSurveyByIdAsync(long id)
    {
        var response = await _publicClient.GetAsync($"{_publicBaseUrl}/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        ApiClientHelper.ProcessResponseStatus(response);

        return await ExtractSurveyBase(response);
    }

    private static async Task<SurveyBase?> ExtractSurveyBase(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to SurveyBase
        var survey = System.Text.Json.JsonSerializer.Deserialize<SurveyBase>(
            jsonResult,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return survey;
    }

    /// <summary>
    /// Get surveys by beach name
    /// </summary>
    public async Task<List<SurveyBase>?> GetSurveysByBeachAsync(string beachName)
    {
        try
        {
            var response = await _publicClient.GetAsync($"{_publicBaseUrl}/beach/{Uri.EscapeDataString(beachName)}");
            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractSurveyBaseList(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<SurveyBase>();
        }
    }

    public async Task<List<SurveyBase>> GetSurveysByFilterAsync(string filter)
    {
        try
        {
            // Searching with a filter endpoint is restricted for security reasons
            var response = await _apiClient.GetAsync($"{_apiBaseUrl}/filter?filter={Uri.EscapeDataString(filter)}");
            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractSurveyBaseList(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<SurveyBase>();
        }
    }

    #endregion

    #region Command Examples (Write Operations)

    /// <summary>
    /// Create a new survey
    /// </summary>
    public async Task<SurveyBase?> UpdateOrCreateSurveyAsync(SurveyBase surveyBase)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _apiBaseUrl)
            {
                Content = JsonContent.Create(surveyBase)
            };
            ApiClientHelper.AddApiKeyHeader(httpRequest);

            var response = await _apiClient.SendAsync(httpRequest);
            if (response == null || response.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            ApiClientHelper.ProcessResponseStatus(response);
            return await ExtractSurveyBase(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
        }
        return null;
    }

    /// <summary>
    /// Delete a survey
    /// </summary>
    public async Task<HttpStatusCode> DeleteSurveyAsync(long id)
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
            TraceLogger.LogError("SurveyApiClient", "DeleteSurveyAsync", ex.Message);
            return HttpStatusCode.InternalServerError;
        }
    }

    #endregion
}

