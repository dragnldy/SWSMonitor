using DataLibrary.Models;
using Models;
using System.Net.Http.Json;

namespace DataLibrary.DataSources.ApiClients;


public class QuadratApiClient
{
    private HttpClient _publicClient;
    private HttpClient _apiClient;
    private string _publicBaseUrl;
    private string _apiBaseUrl;

    public QuadratApiClient(ApiClientConfig clientConfig)
    {
        _publicClient = clientConfig.PublicClient;
        _apiClient = clientConfig.APIClient;

        _publicBaseUrl = $"{clientConfig.PublicUrl}/quadrats";
        _apiBaseUrl = $"{clientConfig.ApiUrl}/quadrats";
    }

    #region Query (Read Operations)

    /// <summary>
    /// Get quadrat entries by survey ID
    /// </summary>
    public async Task<IEnumerable<QuadratEntry>> GetQuadratsBySurveyIdAsync(long surveyId)
    {
        List<QuadratEntry> defaultList = new List<QuadratEntry>();
        try
        {
            var response = await _publicClient!.GetAsync($"{_publicBaseUrl}/survey/{surveyId}");
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return default;
            }

            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractQuadratEntries(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<QuadratEntry>();
        }
    }

    private async Task<IEnumerable<QuadratEntry>> ExtractQuadratEntries(HttpResponseMessage response)
    {
        if (response is null) return new List<QuadratEntry>();

        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(jsonResult)) return new List<QuadratEntry>();

        // Deserialize JSON string to Profile
        return System.Text.Json.JsonSerializer.Deserialize<List<QuadratEntry>>(
            jsonResult
            , new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<QuadratEntry>();
    }

    /// <summary>
    /// Get quadrat entries by survey ID
    /// </summary>
    public async Task<IEnumerable<QuadratBase>> GetAllQuadratsAsync()
    {
        List<QuadratBase> defaultList = new List<QuadratBase>();
        try
        {
            var response = await _publicClient!.GetAsync($"{_publicBaseUrl}");
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return defaultList;
            }
            ApiClientHelper.ProcessResponseStatus(response);
            return await ExtractQuadratBase(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return defaultList;
        }
    }

    private async Task<IEnumerable<QuadratBase>> ExtractQuadratBase(HttpResponseMessage response)
    {
        if (response is null) return new List<QuadratBase>();

        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(jsonResult)) return new List<QuadratBase>();

        // Deserialize JSON string to Profile
        return System.Text.Json.JsonSerializer.Deserialize<List<QuadratBase>>(
            jsonResult
            , new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<QuadratBase>();
    }

    #endregion Read Operations


    #region Create/Update/Delete Operations

    public async Task<IEnumerable<QuadratBase>> CreateOrUpdateQuadratsAsync(long surveyId, IEnumerable<QuadratBase> quadratBases)
    {
        var response = await _apiClient!.PostAsJsonAsync($"/api/quadrats/survey/{surveyId}", quadratBases);
        ApiClientHelper.ProcessResponseStatus(response);
        return await ExtractQuadratBase(response);  
    }


    public async Task DeleteQuadratEntriesForSurveyAsync(long surveyID)
    {
        var response = await _apiClient!.DeleteAsync($"/api/quadrats/survey/{surveyID}");
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            return;
        }
    }

    // Will delete all entries for a survey that aren't in the ID list
    // The surveyid is grabbed from the first entry?
    public async Task<bool> DeleteQuadratEntriesAsync(long surveyId, List<long> idlist)
    {
        // We use the 'put' rather than the delete endpoint so can put the list of ids in the payload
        var response = await _apiClient!.PutAsJsonAsync($"/api/quadrats/delete/{surveyId}", idlist);
        ApiClientHelper.ProcessResponseStatus(response);
        return response.IsSuccessStatusCode;
    }

    #endregion
}
