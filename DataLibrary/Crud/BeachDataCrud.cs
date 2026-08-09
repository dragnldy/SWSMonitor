using DataLibrary.DataSources;
using DataLibrary.DataSources.ApiClients;
using Models;
using System.Net;

namespace DataLibrary.Crud;
public static class BeachDataCrud
{
    // This class is responsible for reading and writing BeachData records to and from data sources like MySql and and API server.
    // MySqlHelperUtils class if accessing MySql and APIClientHelper if accessing API server
    
    #region Read Methods
    public static async Task<List<BeachData>> ReadAllBeachDataAsync(IDataSourceConfig config)
    {
        if (config is MySqlConfig mySqlConfig)
        {
            List<DataRecord> results = await MySqlHelperUtils.ReadTable(config, BeachData.TableName);
            return DeserializeRecords(results);
        }
        else if (config is ApiClientConfig clientConfig)
        {
            BeachDataApiClient beachClient = new BeachDataApiClient(clientConfig);
            List<BeachData> results = await beachClient.GetAllBeachesAsync();
            return results;
        }
        throw new Exception("These operations are only supported for MySqlConfig and ApiClientConfig");
    }

    public static async Task<IEnumerable<BeachData>> ReadAllBeachDataByIslandAsync(IDataSourceConfig config, string island)
    {
        if (string.IsNullOrEmpty(island)) return new List<BeachData>();

        IEnumerable<BeachData> allBeaches = await ReadAllBeachDataAsync(config);
        return allBeaches.Where(b => b.Island!.Equals(island,StringComparison.OrdinalIgnoreCase));
    }


    // use this for the APIClient calls
    public static async Task<BeachData> ReadBeachDataByIdAsync(IDataSourceConfig config, int id)
    {
        if (config is MySqlConfig mySqlConfig)
        {
            string filter = $"ID = '{id}'";
            List<DataRecord> results = await MySqlHelperUtils.ReadTable(config, BeachData.TableName, recordSelect: filter);
            IEnumerable<BeachData> beaches = DeserializeRecords(results);
            return beaches.FirstOrDefault();
        }
        else if (config is ApiClientConfig apiConfig)
        {
            BeachDataApiClient beachClient = new BeachDataApiClient(apiConfig);
            BeachData results = await beachClient.GetBeachByIdAsync(id);
            return results;
        }
        throw new Exception("These operations are only supported for MySqlConfig and ApiClientConfig");
    }

    public static async Task<BeachData> ReadBeachDataByNameAsync(IDataSourceConfig config, string beachName)
    {
        // just filter from (hopefully cached) list of beaches
        IEnumerable<BeachData> beaches = await ReadAllBeachDataAsync(config);
        return beaches.FirstOrDefault(b => b.BeachName!.Equals(beachName, StringComparison.OrdinalIgnoreCase));
    }

    public static async Task<BeachData> ReadBeachDataByFilterAsync(IDataSourceConfig config, string filter)
    {
        if (config is MySqlConfig mySqlConfig)
        {
            List<DataRecord> results = await MySqlHelperUtils.ReadTable(config, BeachData.TableName, recordSelect: filter);
            IEnumerable<BeachData> beaches = DeserializeRecords(results);
            return beaches.FirstOrDefault();
        }
        else if (config is ApiClientConfig clientConfig)
        {
            BeachDataApiClient beachClient = new BeachDataApiClient(clientConfig);
            // NOTE filter should be the beachId for API calls, so we can call the GetBeachByIdAsync method
            if (filter.StartsWith("ID = "))
            {
                filter = filter.Replace("ID = ", "").Trim('\'');
                if (!int.TryParse(filter, out int beachId))
                {
                    TraceLogger.LogError(nameof(BeachDataCrud), "ReadBeachDataByIdAsync", "Invalid beach ID format.");
                    throw new ArgumentException("Invalid beach ID format.");
                }
                BeachData results = await beachClient.GetBeachByIdAsync(beachId);
                return results;
            }
        }
        throw new Exception("These operations are only supported for MySqlConfig and ApiClientConfig");
    }

    private static List<BeachData> DeserializeRecords(List<DataRecord> results)
    {
        return DeserializeRecords(results.Select(r => new DataRecord { Fields = r.Fields }));
    }

    private static List<BeachData> DeserializeRecords(IEnumerable<DataRecord> results)
    {
        List<BeachData> allBeaches = new List<BeachData>();
        if (results == null)
        {
            System.Diagnostics.Debug.WriteLine("No records retrieved");
            return new List<BeachData>(); 
        }
        try
        {
            // Get records from Airtable asynchronously
            foreach (var record in results)
            {
                // Deserialize the record manually into my BeachData object
                DataHelper.LoadClass<BeachData>(record, out object? myRecord);
                if (myRecord != null)
                {
                    allBeaches.Add((BeachData)myRecord);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading records: {ex.Message}");
        }
        return allBeaches;
    }
    #endregion Read Methods

    #region Create/update and Delete Methods
    public static async Task<(bool, BeachData?)> UpdateOrCreateBeachDataAsync(IDataSourceConfig config, BeachData beach)
    {
        if (config is MySqlConfig mySqlConfig)
        {
            if (beach is null) return (false, beach );
            if (beach.ID <= 0)
            {
                // New record
                long newID = await MySqlHelperUtils.InsertOrUpdateRecord<BeachData>(
                    beach, query: "", keyfield: "ID", action: "Insert");
                beach.ID = (int)newID;
                return (true, beach);
            }

            long ID = await MySqlHelperUtils.InsertOrUpdateRecord<BeachData>(
                beach, query: "", keyfield: "", action: "Replace");

            return (true, beach);
        }
        else if (config is ApiClientConfig clientConfig)
        {
            BeachDataApiClient beachClient = new BeachDataApiClient(clientConfig);
            if (beach.ID <= 0)
            {
                BeachData newBeach = await beachClient.CreateBeachAsync(beach);
                return (newBeach != null, newBeach);
            }
            else
            {
                BeachData updatedBeach = await beachClient.UpdateBeachAsync(beach.ID, beach);
                return (updatedBeach != null, updatedBeach);
            }
        }
        throw new Exception("These operations are only supported for MySqlConfig and ApiClientConfig");
    }

    public static async Task<bool> DeleteBeachDataAsync(IDataSourceConfig config, int beachId)
    {
        if (config is MySqlConfig mySqlConfig)
        {
            // Delete from database
            await MySqlHelperUtils.ExecuteNonQueryAsync(mySqlConfig, $"DELETE FROM `{BeachData.TableName}` WHERE ID = {beachId}");
            // Remove from static list
            if (StaticData.Beaches is not null)
            {
                StaticData.Beaches.RemoveAll(n => n.ID == beachId);
            }
            return true;
        }
        else if (config is ApiClientConfig clientConfig)
        {
            BeachDataApiClient beachClient = new BeachDataApiClient(clientConfig);
            HttpStatusCode response = await beachClient!.DeleteBeachAsync(beachId);
            // Remove from static list
            if (StaticData.Beaches is not null)
            {
                StaticData.Beaches.RemoveAll(n => n.ID == beachId);
            }

            return response == HttpStatusCode.NoContent;
        }
        throw new Exception("These operations are only supported for MySqlConfig and ApiClientConfig");
    }
    #endregion Create/update and Delete Methods
}

