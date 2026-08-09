using DataLibrary.DataSources;
using DataLibrary.DataSources.ApiClients;
using Models;
using System.Net;

namespace DataLibrary.Crud;

public static class LookupTableCrud
{
    public static async Task<List<LookupTable>> ReadAllLookupTablesAsync(IDataSourceConfig config)
    {
        if (config == null)
        {
            return new List<LookupTable>();
        }

        if (config is MySqlConfig mySqlConfig)
        {
            IEnumerable<LookupTable> lookupTables = await DataHelper.ReadAllEntriesAsync<LookupTable>(mySqlConfig, LookupTable.TableName);
            return lookupTables.OrderBy(lt => lt.LookupCategory).ToList();
        }
        else if (config is ApiClientConfig apiConfig)
        {
            LookupTableApiClient lookupTableClient = new LookupTableApiClient(apiConfig);
            var apiLookupTables = await lookupTableClient.GetAllLookupTablesAsync();
            return apiLookupTables?.OrderBy(lt => lt.LookupCategory).ToList() ?? new List<LookupTable>();
        }
        throw new Exception("Configuration type must be MySqlConfig or ApiClientConfig");
    }

    public static async Task<List<LookupTable>> ReadLookupTablesByCategoryAsync(IDataSourceConfig config, string category)
    {
        if (config is MySqlConfig mySqlConfig)
        {
            IEnumerable<LookupTable> lookupTables = await DataHelper.ReadFilteredEntries<LookupTable>(
                mySqlConfig, LookupTable.TableName, filter: $" `lookupcategory` = '{category}'");
            return lookupTables.OrderBy(lt => lt.LookupCategory).ToList();
        }
        else if (config is ApiClientConfig apiConfig)
        {
            LookupTableApiClient lookupTableClient = new LookupTableApiClient(apiConfig);
            var apiLookupTables = await lookupTableClient.GetLookupTablesByCategoryAsync(category);
            return apiLookupTables?.OrderBy(lt => lt.LookupCategory).ToList() ?? new List<LookupTable>();
        }
        throw new Exception("Configuration type must be MySqlConfig or ApiClientConfig");
    }

    public static async Task<LookupTable> ReadLookupTablesByIdAsync(IDataSourceConfig config, int id)
    {
        if (config is MySqlConfig mySqlConfig)
        {
            IEnumerable<LookupTable> lookupTables = await DataHelper.ReadFilteredEntries<LookupTable>(
                mySqlConfig, LookupTable.TableName, filter: $" `id` = {id}");
            return lookupTables.FirstOrDefault();
        }
        else if (config is ApiClientConfig apiConfig)
        {
            LookupTableApiClient lookupTableClient = new LookupTableApiClient(apiConfig);
            var apiLookupTable = await lookupTableClient.GetLookupTableByIdAsync(id);
            return apiLookupTable;
        }
        throw new Exception("Configuration type must be MySqlConfig or ApiClientConfig");
    }

    public static async Task<(bool, LookupTable)> UpdateOrCreateLookupTableAsync(IDataSourceConfig config, LookupTable lookupTable)
    {
        if (config is MySqlConfig mySqlConfig)
        {
            if (lookupTable.ID <= 0)
            {
                long result = await MySqlHelperUtils.InsertOrUpdateRecord<LookupTable>(
                    lookupTable, "", "ID", "INSERT", lookupTable.ID);
                if (result >= 0)
                {
                    lookupTable.ID = (int)result;
                    return (true, lookupTable);
                }
                return (false, lookupTable);
            }
            else
            {
                // Update existing lookup table
                long newId = await MySqlHelperUtils.InsertOrUpdateRecord<LookupTable>(lookupTable, "", "ID", "REPLACE", lookupTable.ID);
                return (true, lookupTable);
            }
        }
        else if (config is ApiClientConfig apiConfig)
        {
            LookupTableApiClient lookupTableClient = new LookupTableApiClient(apiConfig);
            if (lookupTable.ID <= 0)
            {
                var created = await lookupTableClient.CreateLookupTableAsync(lookupTable);
                if (created != null)
                {
                    return (true, created);
                }
                return (false, lookupTable);
            }
            else
            {
                // Update existing lookup table
                var updated = await lookupTableClient.UpdateLookupTableAsync(lookupTable.ID, lookupTable);
                if (updated != null)
                {
                    return (true, updated);
                }
                return (false, lookupTable);
            }
        }

        return (false, lookupTable);
    }

    public static async Task<bool> DeleteLookupTable(IDataSourceConfig config, int id)
    {
        if (id <= 0) return false;
        
        if (config is MySqlConfig mySqlConfig)
        {
            // Delete from database
            await MySqlHelperUtils.ExecuteNonQueryAsync(mySqlConfig, $"DELETE FROM `{LookupTable.TableName}` WHERE ID = {id}");
            // Remove from static list on successful deletion
            if (StaticData.LookupTables is not null)
            {
                StaticData.LookupTables.RemoveAll(lt => lt.ID == id);
            }
            return true;

        }
        else if (config is ApiClientConfig apiConfig)
        {
            LookupTableApiClient lookupTableClient = new LookupTableApiClient(apiConfig);
            var result = await lookupTableClient.DeleteLookupTableAsync(id);

            if (result == HttpStatusCode.OK || result == HttpStatusCode.NoContent)
            {
                // Remove from static list on successful deletion
                if (StaticData.LookupTables is not null)
                {
                    StaticData.LookupTables.RemoveAll(lt => lt.ID == id);
                }
            }
            return true;
        }

        throw new Exception("Configuration type must be MySqlConfig or ApiClientConfig");
    }
}
