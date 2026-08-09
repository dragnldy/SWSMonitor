using DataLibrary.Crud;
using Newtonsoft.Json.Linq; // Example JSON library

namespace DataLibrary.DataSources;

public static class AirTableRest
{

    public static async Task<List<DataRecord>> ReadTable(object connectionInfo, string tableName, string? recordId = null)
    {
        if (connectionInfo is not AirTableConfig config)
        {
            throw new ArgumentException("Invalid connection info provided. Expected AirTableConfig.", nameof(connectionInfo));
        }
        AirTableConfig airtableConfig = connectionInfo as AirTableConfig;

        string offset = null;
        List<DataRecord> allRecords = new List<DataRecord>();

        do
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {airtableConfig.ApiKey}");

                string apiUrl = $"https://api.airtable.com/v0/{airtableConfig.BaseId}/{tableName}";

                // If a recordId is provided, append it to the URL
                if (!string.IsNullOrEmpty(recordId))
                    apiUrl += $"/{recordId}";
                else
                if (!string.IsNullOrEmpty(offset))
                {
                    apiUrl += "?offset=" + offset;
                }

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                /*
                To detect if your Airtable API usage is being throttled, look for a 429 error code in your API responses.
                This indicates that you've exceeded the rate limit, which is 5 requests per second per base and 50 requests per second for personal access tokens.
                If you receive this error, you'll need to implement a retry mechanism with exponential backoff to avoid further throttling.
                From Airtable documentation: If you exceed this rate, you will receive a 429 status code and must wait 30 seconds before subsequent requests will succeed. 
                API integrations should pause and wait before retrying the API request
                */

                if ((long)response.StatusCode == 429)
                {
                    Thread.Sleep(30500); // wait 30.5 seconds
                    // Don't change the current offset and try again
                    continue;
                }
                else
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        // 1. Parse raw json as a JObject 
                        JObject airtableResponse = JObject.Parse(jsonResponse);

                        if (airtableResponse is not null)
                        {
                            offset = (string)airtableResponse["offset"];
                            foreach (var record in airtableResponse["records"].Select(r => r.ToObject<DataRecord>()))
                            {
                                    allRecords.Add(record);
                            }
                        }
                    }
                    else
                    {
                        // Handle error
                        System.Diagnostics.Debug.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
            }
        } while (!string.IsNullOrEmpty(offset));

        return allRecords;
    }
}

