using Models;

namespace DataLibrary.DataSources.ApiClients;

public class SpeciesListApiClient
{
    private HttpClient _publicClient;
    private HttpClient _apiClient;
    private string _publicBaseUrl;
    private string _apiBaseUrl;

    public SpeciesListApiClient(ApiClientConfig clientConfig)
    {
        _publicClient = clientConfig.PublicClient;
        _apiClient = clientConfig.APIClient;

        _publicBaseUrl = $"{clientConfig.PublicUrl}/specieslists";
        _apiBaseUrl = $"{clientConfig.ApiUrl}/specieslists";
    }

    #region Query (Read Operations)

    private async Task<List<SpeciesList>?> ExtractSpeciesListDataList(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to List<SpeciesList>
        var SpeciesListList = System.Text.Json.JsonSerializer.Deserialize<List<SpeciesList>>(
            jsonResult
            , new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        // TraceLogger.LogInformation(jsonResult);
        TraceLogger.LogInformation($"SpeciesList count {SpeciesListList.Count}");

        return SpeciesListList ?? new List<SpeciesList>();
    }

    /// <summary>
    /// Get all SpeciesList observations for a specific survey
    /// </summary>
    public async Task<List<SpeciesList>?> GetSpeciesListsForSurveyAsync(long surveyId)
    {
        try
        {
            var response = await _publicClient!.GetAsync($"{_publicBaseUrl}/survey/{surveyId}");
            ApiClientHelper.ProcessResponseStatus(response);
            return await ExtractSpeciesListDataList(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<SpeciesList>();
        }
    }
    #endregion Read Operations
}
