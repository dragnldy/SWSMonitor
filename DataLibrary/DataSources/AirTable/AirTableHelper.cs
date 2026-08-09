using DataLibrary.Crud;
using Models;
using System.Reflection;

namespace DataLibrary.DataSources;


public class AirTableConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseId { get; set; } = string.Empty;
}

// Interface with Airtable via their API using the AirtableApiClient library
// This cannot be used with Blazor apps as it uses async methods and the library is not compatible with Blazor WebAssembly
// It is designed to be used in console, MAUI or server-side applications

// API library uses https://github.com/ngocnicholas/airtable.net
// Documentation https://github.com/ngocnicholas/airtable.net/wiki/Documentation
// Airtable API info is at https://airtable.com/appt1WQ2fOuI6Y7V6/api/docs#javascript/introduction
public static class AirtableHelper
{
    public static DateTime LastApiRequest { get; set; } = DateTime.MinValue;
    public static int ApiRequestCount { get; set; } = 0;

    public static async Task<List<DataRecord>> ReadTable(object connectionInfo, string tableName, string? recordId = null, string? filter = null)
    {
        if (connectionInfo is null || connectionInfo is not AirTableConfig config)
        {
            throw new ArgumentException("Invalid connection info provided. Expected AirTableConfig.", nameof(connectionInfo));
        }

        if (LastApiRequest == DateTime.MinValue)
        {
            LastApiRequest = DateTime.Now.AddSeconds(-1.0); // Initialize the last request time
        }
        try
        {
            AirTableConfig airtableConfig = connectionInfo as AirTableConfig;

            // Airtable only returns a maximum of 100 records per request, so you may need to handle pagination
            // Also throttling is required for large datasets
            string offset = string.Empty;
            var allRecords = new List<DataRecord>();

            using (AirtableBase airtableBase = new AirtableBase(airtableConfig?.ApiKey, airtableConfig?.BaseId))
            {
                do
                {
                    try
                    {
                        Task<AirtableListRecordsResponse> records = airtableBase.ListRecords(tableName, offset,filterByFormula: filter);
                        AirtableListRecordsResponse response = await records;
                        if (!response.Success)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error retrieving records: {response.AirtableApiError}");
                            return new List<DataRecord>(); // Return empty list on error
                        }
                        if (response.Success)
                        {
                            foreach (var record in response.Records)
                            {
                                // Deserialize the record manually into my DataRecord object
                                DataRecord? myRecord = DataHelper.LoadDataRecord(record);
                                if (myRecord != null)
                                {
                                    allRecords.Add(myRecord);
                                }
                            }
                            offset = response.Offset;
                            // Test for throttling
                            ApiRequestCount++;

                            /*
                            To detect if your Airtable API usage is being throttled, look for a 429 error code in your API responses.
                            This indicates that you've exceeded the rate limit, which is 5 requests per second per base and 50 requests per second for personal access tokens.
                            If you receive this error, you'll need to implement a retry mechanism with exponential backoff to avoid further throttling.
                            From Airtable documentation: If you exceed this rate, you will receive a 429 status code and must wait 30 seconds before subsequent requests will succeed. 
                            API integrations should pause and wait before retrying the API request
                            */

                            if (DateTime.Now.Subtract(LastApiRequest).TotalSeconds < 1 && ApiRequestCount > 5)
                            {
                                // If the last request was less than a second ago, and we have made too many requests then wait for a second
                                await Task.Delay(1000);
                                ApiRequestCount = 0; // Reset the request count
                                LastApiRequest = DateTime.Now.AddSeconds(-1.0); // Update the last request time
                            }

                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is AirtableTooManyRequestsException)
                        {
                            // Handle throttling error
                            System.Diagnostics.Debug.WriteLine("Throttling detected. Waiting for 30 seconds before retrying...");
                            await Task.Delay(30500); // wait 30.5 seconds
                            continue; // Retry the request
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Error retrieving records: {ex.Message}");
                            return new List<DataRecord>(); // Return empty list on error
                        }
                    }
                } while (!string.IsNullOrEmpty(offset));
                return allRecords;
            }
        }
        catch (Exception exc)
        {
            System.Diagnostics.Debug.WriteLine(exc.ToString());
        }
        return new List<DataRecord>();
    }

    public static Dictionary<string,object> GetTableFields<T>(T table)
    {
        Dictionary<string, object> fields = new();

        // Use reflection to iterate through the properties and add as named field
        if (table is null)
            return fields;

        Type tType = table.GetType();

        // Get public instance properties, skip indexers
        var properties = tType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                              .Where(p => p.GetIndexParameters().Length == 0);

        foreach (var prop in properties)
        {
            try
            {
                // Skip properties decorated with any attribute named "JsonIgnoreAttribute"
                var hasJsonIgnore = prop.GetCustomAttributes(true).Any(a => a.GetType().Name == "JsonIgnoreAttribute");
                if (hasJsonIgnore)
                    continue;

                object? value = prop.GetValue(table);

                // Add to dictionary (null values are allowed)
                fields[prop.Name] = value ?? null!;
            }
            catch (TargetInvocationException)
            {
                // If property getter throws, skip the property
                continue;
            }
            catch
            {
                // For any other reflection failure, skip the property to avoid breaking callers
                continue;
            }
        }

        return fields;
    }
    // Create or update a new record
    public static async Task<AirtableCreateUpdateReplaceRecordResponse> CreateOrReplaceRecordAsync<T>(AirTableConfig config, string tablename, T datarecord )
    {
        bool isNewRecord = false;
        string airTableId = string.Empty;
        Dictionary<string,object> newData = GetTableFields(datarecord);
        var fields = new Fields();
        foreach (var key in newData.Keys)
        {
            if (key.Equals("AirTableId", StringComparison.CurrentCultureIgnoreCase))
            {
                isNewRecord = string.IsNullOrEmpty(newData[key].ToString());
                airTableId = newData[key].ToString();
            }
            else
                fields.AddField(key, newData[key]);
        }

        var airtableClient = new AirtableBase(config.ApiKey, config.BaseId);
        if (isNewRecord)
        {
            var createTask = airtableClient.CreateRecord(tablename, fields);
            AirtableCreateUpdateReplaceRecordResponse createResponse = await createTask;
            return createResponse;
        }
        else
        {
            var updateTask = airtableClient.UpdateRecord(tablename, fields, id: airTableId);
            AirtableCreateUpdateReplaceRecordResponse createResponse = await updateTask;
            return createResponse;
        }
    }

    public static async Task<AirtableDeleteRecordResponse> DeleteRecordAsync(AirTableConfig? config, string tableName, string tobeDeleted)
    {
        var airtableClient = new AirtableBase(config.ApiKey, config.BaseId);
        return await airtableClient.DeleteRecord(tableName, tobeDeleted);
    }

    public static async Task UpdateCollection<T>(long surveyId, string tableName, List<T> collection)
    {
        try
        {
            DateTime currententrytime = DateTime.Now;
            foreach (var item in collection)
            {
                ((ITableBase)item).EntryDate = currententrytime;
                // Update each item still in the list- calling routine must set the entrydate to current date/time
                AirtableCreateUpdateReplaceRecordResponse response =
                        await CreateOrReplaceRecordAsync<T>(
                            StaticData.DataSourceConfig as AirTableConfig, tableName, item);
                if (response != null && response.Success)
                {
                    ((ITableBase)item).AirTableId = response.Record.Id;
                }
                else
                {

                }
            }
            // Get all the records for this event to find any that were deleted
            IEnumerable<T> allItems =
                await DataHelper.ReadEntries<T>(
                    StaticData.DataSourceConfig as AirTableConfig, tableName, surveyId);

            foreach (ITableBase item in allItems)
            {
                if (item.EntryDate.HasValue && AirTableNotLessThanLocal(item.EntryDate.Value, currententrytime))
                    continue;
                AirtableDeleteRecordResponse response =
                    await AirtableHelper.DeleteRecordAsync(
                    StaticData.DataSourceConfig as AirTableConfig, tableName, item.AirTableId);
                if (response == null || !response.Success)
                {
                }
            }
        }
        catch (Exception ex) {
            throw new Exception($"error updating collection for {tableName}");
        }
    }

    private static bool AirTableNotLessThanLocal(DateTime airTableDate, DateTime latestDate)
    {
        // Airtable returns dates as UTC and there is a rounding error involved when exact compares
        return airTableDate.ToLocalTime() >= latestDate.AddSeconds(-30);
    }


    //var query = new AirtableListRecordsOptions
    //{
    //    FilterByFormula = "AND({Status}='Active', {Age}>30)",
    //    Sort = new List<Sort> { new Sort("Name", SortDirection.Desc) },
    //    MaxRecords = 100
    //};

}

