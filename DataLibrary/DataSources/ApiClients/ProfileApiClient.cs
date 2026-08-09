using Models;
using System.Net.Http.Json;

namespace DataLibrary.DataSources.ApiClients;

/// <summary>
/// Client for consuming the Profile CQRS API
/// </summary>
public class ProfileApiClient
{
    private HttpClient _publicClient;
    private HttpClient _apiClient;
    private string _publicBaseUrl;
    private string _apiBaseUrl;

    public ProfileApiClient(ApiClientConfig clientConfig)
    {
        _publicClient = clientConfig.PublicClient;
        _apiClient = clientConfig.APIClient;

        _publicBaseUrl = $"{clientConfig.PublicUrl}/profiles";
        _apiBaseUrl = $"{clientConfig.ApiUrl}/profiles";
    }

    #region Query (Read Operations)

    /// <summary>
    /// Get slim profile entries (just the profile entry- not details or surface details)
    /// </summary>
    public async Task<List<ProfileBase>?> GetProfilesAsync(long? surveyId = 0l)
    {
        try
        {
            var response = await _publicClient.GetAsync($"{_publicBaseUrl}");

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return new List<ProfileBase>();
            }

            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractProfileEntries(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<ProfileBase>();
        }
    }

    private async Task<List<ProfileBase>?> ExtractProfileEntries(HttpResponseMessage response)
    {
        if (response is null) return new List<ProfileBase>();

        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(jsonResult)) return new List<ProfileBase>();

        // Deserialize JSON string to Profile
        return  System.Text.Json.JsonSerializer.Deserialize<List<ProfileBase>>(
            jsonResult
            , new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ProfileBase>();
    }

    /// <summary>
    /// Get profile entries by survey ID
    /// </summary>
    public async Task<List<ProfileBase>?> GetProfilesBySurveyIdAsync(long surveyId)
    {
        try
        {
            var response = await _publicClient.GetAsync($"{_publicBaseUrl}/survey/{surveyId}");

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return new List<ProfileBase>();
            }

            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractProfileEntries(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<ProfileBase>();
        }
    }

    #endregion Query (Read Operations)

    #region Command Methods (Create/Update Operations)

    public async Task<List<ProfileBase>?> CreateProfileEntries(long surveyId, List<ProfileBase> profileEntries)
    {
        var response = await _apiClient!.PostAsJsonAsync($"/api/profiles/survey/{surveyId}", profileEntries);

        ApiClientHelper.ProcessResponseStatus(response);
        return await ExtractProfileEntries(response);
    }

    // Will delete all entries for a survey that aren't in the ID list
    internal async Task<bool> DeleteProfileEntriesAsync(long surveyId, List<long> idlist)
    {
        // We use the 'put' rather than the delete endpoint so can put the list of ids in the payload
        var response = await _apiClient!.PutAsJsonAsync($"/api/profiles/delete/{surveyId}", idlist);
        ApiClientHelper.ProcessResponseStatus(response);
        return response.IsSuccessStatusCode;
    }

    #endregion
}
