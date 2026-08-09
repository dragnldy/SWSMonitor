using DataLibrary.DataSources;
using DataLibrary.DataSources.ApiClients;
using DataLibrary.Models;
using Models;
using System.Diagnostics;
using System.Net;

namespace DataLibrary.Crud;

public static class CityStateCrud
{
    #region Read Methods
    /// <summary>
    /// Reads all CityState records from the data source
    /// Note: CityStates is actually a view, not a table
    /// </summary>
    public static async Task<List<CityState>> ReadAllCityStatesAsync(IDataSourceConfig config)
    {
        if (config == null)
        {
            return new List<CityState>();
        }

        if (config is MySqlConfig mySqlConfig)
        {
            IEnumerable<CityState> citystates = await DataHelper.ReadAllEntriesAsync<CityState>(config, CityState.TableName);
            return citystates.OrderBy(cs => cs.State).ThenBy(cs => cs.City).ToList();
        }
        else if (config is ApiClientConfig apiConfig)
        {
            CityStateApiClient CityStateClient = new CityStateApiClient(apiConfig);
            var results = await CityStateClient.GetAllCityStatesAsync();
            return results?.OrderBy(cs => cs.State).ThenBy(cs => cs.City).ToList() ?? new List<CityState>();
        }

        throw new ArgumentException("Invalid configuration type provided. Expected MySqlConfig or ApiClientConfig.");
    }

    /// <summary>
    /// Finds a specific CityState by city and state combination
    /// </summary>
    public static async Task<CityState?> ReadCityStateByIdAsync(IDataSourceConfig config, int id)
    {
        if (config == null || id <= 0) return null;

        if (config is MySqlConfig mySqlConfig)
        {
            IEnumerable<CityState> citystates = await DataHelper.ReadEntries<CityState>(config, CityState.TableName, (long) id, keyfield: "ID");
            return citystates.FirstOrDefault();
        }
        else if (config is ApiClientConfig apiConfig)
        {
            CityStateApiClient cityStatesClient = new CityStateApiClient(apiConfig);
            return await cityStatesClient.GetCityStateByIdAsync(id);
        }

        throw new ArgumentException("Invalid configuration type provided. Expected MySqlConfig or ApiClientConfig.");
    }

    public static async Task<CityState?> ReadCityStateByCityAsync(IDataSourceConfig config, string city)
    {
        if (config is null || string.IsNullOrEmpty(city)) return null;

        if (config is MySqlConfig mySqlConfig)
        {
            // Need to eventually cache this
            IEnumerable<CityState> citystates = await DataHelper.ReadEntries<CityState>(config, CityState.TableName, sqlSelection: $"WHERE City = '{city}'");
            return citystates.FirstOrDefault();
        } 
        else if (config is ApiClientConfig apiConfig) 
        {
            CityStateApiClient cityStatesClient = new CityStateApiClient(apiConfig);
            var citystates = await cityStatesClient.GetAllCityStatesAsync();
            return citystates.FirstOrDefault(cs => cs.City.Equals(city, StringComparison.OrdinalIgnoreCase));
        }

        throw new ArgumentException("Invalid configuration type provided. Expected MySqlConfig or ApiClientConfig.");
    }

    #endregion Read Methods

    #region Create/Update Methods
    /// <summary>
    /// Creates a new CityState record or updates an existing one
    /// </summary>
    public static async Task<(bool success, CityState? cityState)> UpdateOrCreateCityStateAsync(IDataSourceConfig config, CityState? cityState)
    {
        if (cityState == null) return (false, cityState);

        if (cityState.ID <= 0)
        {
            cityState.EntryDate = DateTime.Now;
        }

        if (config is MySqlConfig mySqlConfig)
        {
            string action = cityState.ID <= 0 ? "Insert" : "Replace";
            long id = await MySqlHelperUtils.InsertOrUpdateRecordWithIdAsync<CityState>(
                cityState,  keyfield: "ID", action: action, currentId: cityState.ID);
            if (cityState.ID <= 0)
                cityState.ID = (int)id;
            return (true, cityState);
        }
        else if (config is ApiClientConfig apiConfig)
        {
            CityStateApiClient cityStatesClient = new CityStateApiClient(apiConfig);
            CityState? created = await cityStatesClient.UpdateOrCreateCityStateAsync(cityState);
            if (created != null)
            {
                return (true, created);
            }
            return (false, cityState);
        }
        throw new ArgumentException("Invalid configuration type provided. Expected MySqlConfig or ApiClientConfig.");
    }
    #endregion Create/Update Methods

    #region Delete Methods
    /// <summary>
    /// Deletes a single CityState record
    /// Note: For MySql, this uses internal methods and should be called from within the DataLibrary assembly
    /// </summary>
    public static async Task<bool> DeleteCityStateAsync(IDataSourceConfig config, CityState cityState)
    {
        if (cityState == null)
        {
            return false;
        }
        return await DeleteCityStateAsync(config, cityState.ID);
    }
    public static async Task<bool> DeleteCityStateAsync(IDataSourceConfig config, int id)
    { 
        try
        {
            if (id <= 0)
                return false;

            if (config is MySqlConfig mySqlConfig)
            {
                var rowsdeleted = await MySqlHelperUtils.ExecuteNonQueryAsync(config, $"DELETE FROM `{CityState.TableName}` WHERE ID = {id}");
                return (rowsdeleted >= 1);
            }
            else if (config is ApiClientConfig apiConfig)
            {
                CityStateApiClient cityStatesClient = new CityStateApiClient(apiConfig);
                var result = await cityStatesClient.DeleteCityStateAsync(id);

                return result == HttpStatusCode.OK || result == HttpStatusCode.NoContent;
            }
            throw new ArgumentException("Invalid configuration type provided. Expected MySqlConfig or ApiClientConfig.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error deleting CityState: {ex.Message}");
            return false;
        }

        return false;
    }

    #endregion Delete Methods
}
