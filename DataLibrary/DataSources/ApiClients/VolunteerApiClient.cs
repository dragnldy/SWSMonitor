using Models;
using System.Net;
using System.Net.Http.Json;

namespace DataLibrary.DataSources.ApiClients;

/// <summary>
/// Client for consuming the Volunteer CQRS API
/// </summary>
public class VolunteerApiClient
{
    private HttpClient _publicClient;
    private HttpClient _apiClient;
    private string _publicBaseUrl;
    private string _apiBaseUrl;

    public VolunteerApiClient(ApiClientConfig clientConfig)
    {
        _publicClient = clientConfig.PublicClient;
        _apiClient = clientConfig.APIClient;

        _publicBaseUrl = $"{clientConfig.PublicUrl}/volunteers";
        _apiBaseUrl = $"{clientConfig.ApiUrl}/volunteers";
    }

    #region Query Examples (Read Operations)
    // Methods that return complete volunteer records are restricted to the authenticated API routes.
    // This is because the public routes may be exposed to unauthorized users, and we want to limit the amount of information that can be accessed by those users. The public routes should only return limited information about volunteers, such as their names and IDs, while the authenticated API routes can return complete records with all details.

    // Get all volunteer records with minimal information (for public use)
    public async Task<List<Volunteer>?> GetAllVolunteersPublicAsync()
    {
        try
        {
            var response = await _publicClient.GetAsync(_publicBaseUrl);
            ApiClientHelper.ProcessResponseStatus(response);
            return await ExtractPublicVolunteerList(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<Volunteer>();
        }
    }

    private async Task<List<Volunteer>?> ExtractPublicVolunteerList(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to List<Volunteer>
        var volunteers = System.Text.Json.JsonSerializer.Deserialize<List<Volunteer>>(
            jsonResult,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        // TraceLogger.LogInformation(jsonResult);
        TraceLogger.LogInformation($"Volunteers count {volunteers?.Count ?? 0}");

        return volunteers ?? new List<Volunteer>();
    }

    /// <summary>
    /// Get all volunteers
    /// </summary>
    public async Task<List<Volunteer>?> GetAllVolunteersAsync()
    {
        try
        {
            var response = await _apiClient.GetAsync(_apiBaseUrl);
            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractVolunteerList(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return new List<Volunteer>();
        }
    }

    private static async Task<List<Volunteer>?> ExtractVolunteerList(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to List<Volunteer>
        var volunteers = System.Text.Json.JsonSerializer.Deserialize<List<Volunteer>>(
            jsonResult,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        // TraceLogger.LogInformation(jsonResult);
        TraceLogger.LogInformation($"Volunteers count {volunteers?.Count ?? 0}");

        return volunteers ?? new List<Volunteer>();
    }

    // This is only used by the ApiSurvey in theory, but we can implement it if needed. It is not currently used by any of the existing code.
    internal async Task<Volunteer?> GetVolunteerByNameAsync(string firstlast)
    {
        try
        {
            IEnumerable<Volunteer> volunteers = await GetAllVolunteersAsync() ?? new List<Volunteer>();
            return volunteers.FirstOrDefault(v => v.FirstLast == firstlast);

        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return null;
        }
    }

    /// <summary>
    /// Get a specific volunteer by ID
    /// </summary>
    public async Task<Volunteer?> GetVolunteerByIdAsync(int id)
    {
        try
        {
            var response = await _apiClient.GetAsync($"{_apiBaseUrl}/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            ApiClientHelper.ProcessResponseStatus(response);

            return await ExtractVolunteer(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return null;
        }
    }

    private static async Task<Volunteer?> ExtractVolunteer(HttpResponseMessage response)
    {
        // Read response as string first
        string jsonResult = await response.Content.ReadAsStringAsync();

        // Deserialize JSON string to Volunteer
        var volunteer = System.Text.Json.JsonSerializer.Deserialize<Volunteer>(
            jsonResult,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return volunteer;
    }

    #endregion

    #region Command Examples (Write Operations)

    /// <summary>
    /// Create a new volunteer
    /// </summary>
    public async Task<Volunteer?> UpdateOrCreateVolunteerAsync(Volunteer volunteer)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _apiBaseUrl)
            {
                Content = JsonContent.Create(volunteer)
            };
            ApiClientHelper.AddApiKeyHeader(httpRequest);

            var response = await _apiClient.SendAsync(httpRequest);
            if (response is null || response.StatusCode == HttpStatusCode.BadRequest)
                return null;
            ApiClientHelper.ProcessResponseStatus(response);
            return await ExtractVolunteer(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
        }
        return null;
    }

    /// <summary>
    /// Update an existing volunteer
    /// </summary>
    public async Task<Volunteer?> UpdateVolunteerAsync(int id, Volunteer volunteer)
    {
        try
        {
            // Ensure the ID matches
            if (id != volunteer.ID)
            {
                throw new ArgumentException("ID mismatch between route and request body");
            }

            var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"{_apiBaseUrl}/{id}")
            {
                Content = JsonContent.Create(volunteer)
            };
            ApiClientHelper.AddApiKeyHeader(httpRequest);

            var response = await _apiClient.SendAsync(httpRequest);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException($"Volunteer with ID {id} not found");
            }

            ApiClientHelper.ProcessResponseStatus(response);
            return await ExtractVolunteer(response);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.ToString());
            return null;
        }
    }

    /// <summary>
    /// Delete a volunteer
    /// </summary>
    public async Task<HttpStatusCode> DeleteVolunteerAsync(int id)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/{id}");
            ApiClientHelper.AddApiKeyHeader(httpRequest);

            var response = await _apiClient.SendAsync(httpRequest);

            if (response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.NoContent)
            {
                return response.StatusCode;
            }
            TraceLogger.LogWarning(nameof(DeleteVolunteerAsync), "Got unexpected result while deleting",
                response.StatusCode.ToString());
            return response.StatusCode;
        }
        catch (Exception ex)
        {
            TraceLogger.LogError("VolunteerApiClient", "DeleteVolunteerAsync", ex.Message);
            return HttpStatusCode.InternalServerError;
        }
    }

    #endregion

}
