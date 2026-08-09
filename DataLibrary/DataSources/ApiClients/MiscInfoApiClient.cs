using DataLibrary.Models;
using Models;
using System.Net;

namespace DataLibrary.DataSources.ApiClients;

public class MiscInfoApiClient
{
    /// <summary>
    /// Get all miscellaneos app information
    /// </summary>
    public static async Task<MiscInfo?> GetAllMiscInfoAsync(IDataSourceConfig config)
    {
        if (config == null || config is not ApiClientConfig clientConfig) return null;

        var url = clientConfig.PrivateUrl + "/miscinfo";

        try
        { 
            var response = await clientConfig.PublicClient.GetAsync(url);
            ApiClientHelper.ProcessResponseStatus(response);
            if (response.StatusCode == HttpStatusCode.NotFound) return null;
            string content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content)) return null;
            MiscInfo? miscInfo = System.Text.Json.JsonSerializer.Deserialize<MiscInfo?>(content,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            return miscInfo;
        }
        catch (System.Text.Json.JsonException ex)
        {
            // Handle JSON deserialization error
            Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to contact api server: " + url + ex.Message);
        }

        return null;
    }
}
