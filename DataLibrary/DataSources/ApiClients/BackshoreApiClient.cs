using Models;

namespace DataLibrary.DataSources.ApiClients;

public class BackshoreApiClient
{
    private HttpClient _publicClient;
    private string _publicBaseUrl;
    private string _viewBaseUrl;

    public BackshoreApiClient(ApiClientConfig clientConfig)
    {
        _publicClient = clientConfig.PublicClient;

        _viewBaseUrl = $"{clientConfig.ViewUrl}/backshore";
        _publicBaseUrl = $"{clientConfig.PublicUrl}/backshore";
    }

    #region Query (Read Operations)

    /// <summary>
    /// Get all backshore observations
    /// </summary>
    public async Task<List<BackShore>?> GetAllBackshoreAsync()
    {
        try
        {
            var response = await _publicClient!.GetAsync(_viewBaseUrl);
            ApiClientHelper.ProcessResponseStatus(response);
            return await ExtractBackshoreDataList(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<BackShore>();
        }
    }

    private async Task<List<BackShore>?> ExtractBackshoreDataList(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to List<BackShore>
        var backshoreList = System.Text.Json.JsonSerializer.Deserialize<List<BackShore>>(
            jsonResult
            , new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        // TraceLogger.LogInformation(jsonResult);
        TraceLogger.LogInformation($"BackShore count {backshoreList.Count}");

        return backshoreList ?? new List<BackShore>();
    }

    /// <summary>
    /// Get all backshore observations for a specific survey
    /// </summary>
    public async Task<List<BackShore>?> GetBackshoreForSurveyAsync(long surveyId)
    {
        try
        {
            var response = await _publicClient!.GetAsync($"{_publicBaseUrl}/survey/{surveyId}");
            ApiClientHelper.ProcessResponseStatus(response);
            return await ExtractBackshoreDataList(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<BackShore>();
        }
    }


    #endregion Read Operations
}
