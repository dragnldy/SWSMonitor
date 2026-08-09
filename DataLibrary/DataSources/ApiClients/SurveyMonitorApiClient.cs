using Models;

namespace DataLibrary.DataSources.ApiClients;

/// <summary>
/// Client for consuming the MonitorName CQRS API
/// </summary>
public class SurveyMonitorApiClient
{
    private HttpClient _publicClient;
    private string _publicBaseUrl;

    public SurveyMonitorApiClient(ApiClientConfig clientConfig)
    {
        _publicClient = clientConfig.PublicClient;

        _publicBaseUrl = $"{clientConfig.PublicUrl}/monitors";
    }

    #region Query (Read Operations)

    /// <summary>
    /// Get SurveyMonitors by survey ID
    /// </summary>
    public async Task<List<SurveyMonitor>?> GetSurveyMonitorsBySurveyIdAsync(long surveyId)
    {
        try
        {
            var response = await _publicClient.GetAsync($"{_publicBaseUrl}/survey/{surveyId}");

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return new List<SurveyMonitor>();
            }

            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractSurveyMonitorList(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<SurveyMonitor>();
        }
    }


    private static async Task<List<SurveyMonitor>?> ExtractSurveyMonitorList(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to List<SurveyMonitor>
        var surveyMonitors = System.Text.Json.JsonSerializer.Deserialize<List<SurveyMonitor>>(

            jsonResult
            , new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        // TraceLogger.LogInformation(jsonResult);
        TraceLogger.LogInformation($"SurveyMonitors count {surveyMonitors?.Count ?? 0}");

        return surveyMonitors ?? new List<SurveyMonitor>();
    }

    #endregion

}
