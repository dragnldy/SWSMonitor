using DataLibrary.DataSources;
using DataLibrary.DataSources.ApiClients;
using Models;
using System.Net;

namespace DataLibrary.Crud;

public static class SpeciesCrud
{
    public static async Task<List<Species>> ReadAllSpeciesAsync(IDataSourceConfig config)
    {
        if (config == null)   return new List<Species>();

        if (config is MySqlConfig mySqlConfig)
        {
            IEnumerable<Species> species = await DataHelper.ReadAllEntriesAsync<Species>(config, Species.TableName);
            return species.OrderBy(sp => sp.ScientificName).ToList();
        } 
        else if (config is ApiClientConfig apiClientConfig)
        {
            SpeciesApiClient speciesClient = new SpeciesApiClient(apiClientConfig);
            var speciesList = await speciesClient.GetAllSpeciesAsync();
            return speciesList ?? new List<Species>();
        }

        throw new NotImplementedException();
    }
    public static async Task<Species> ReadSpeciesByIdAsync(IDataSourceConfig config, long id)
    {
        if (config is MySqlConfig mySqlConfig)
        {
            IEnumerable<Species> species = await DataHelper.ReadEntries<Species>(config, Species.TableName, id, keyfield: "ID");
            return species.FirstOrDefault();
        }
        else if (config is ApiClientConfig apiClientConfig)
        {
            SpeciesApiClient speciesClient = new SpeciesApiClient(apiClientConfig);
            var species = await speciesClient.GetSpeciesByIdAsync(id);
            return species;
        }
        throw new NotImplementedException();
    }
    public static async Task<Species?> ReadSpeciesByNameAsync(IDataSourceConfig config, string scientificName)
    {
        if (!MySqlHelperUtils.IsSafeFromInjectionString(scientificName))
        {
            TraceLogger.LogWarningAuto("Scientific name contains potentially unsafe characters.");
            return null;
        }
        if (config is MySqlConfig mySqlConfig)
        {
            IEnumerable<Species> species = await DataHelper.ReadEntries<Species>(config, Species.TableName, sqlSelection: $"WHERE ScientificName = '{scientificName}'");
            return species.FirstOrDefault();
        }
        else if (config is ApiClientConfig apiClientConfig)
        {
            SpeciesApiClient speciesClient = new SpeciesApiClient(apiClientConfig);
            var species = await speciesClient.GetSpeciesByNameAsync(scientificName);
            return species;
        }
        throw new NotImplementedException();
    }


    public static async Task<(bool, Species)> UpdateOrCreateSpeciesAsync(IDataSourceConfig config, Species species)
    {
        if (config is ApiClientConfig apiClientConfig)
        {
            SpeciesApiClient speciesClient = new SpeciesApiClient(apiClientConfig);

            var createdSpecies = await speciesClient.CreateOrUpdateSpeciesAsync( species);
            if (createdSpecies != null)
            {
                return (true, createdSpecies);
            }
            return (false, species);
        }
        else if (config is MySqlConfig mySqlConfig)
        {
            if (species is null) return (false, species);
            if (species.ID <= 0)
            {
                // New record
                long newID = await MySqlHelperUtils.InsertOrUpdateRecord<Species>(
                    species, keyfield: "ID", action: "Insert", currentId: species.ID);
                species.ID = (int)newID;
                return (true, species);
            }
            else
            { 
                long ID = await MySqlHelperUtils.InsertOrUpdateRecord<Species>(
                    species, query: "", keyfield: "ID", action: "Replace", currentId: species.ID);
                species.ID = (int)ID;
                return (true, species);
            }
        }
        return (false, species);
    }

    public static async Task<bool> DeleteSpeciesAsync(IDataSourceConfig config, int speciesId)
    {
        if (config is MySqlConfig mySqlConfig)
        {
            await MySqlHelperUtils.ExecuteNonQueryAsync(mySqlConfig, $"DELETE FROM `{Species.TableName}` WHERE ID = {speciesId}");
            return true;
   
        } else if (config is ApiClientConfig apiClientConfig)  {

            SpeciesApiClient speciesClient = new SpeciesApiClient(apiClientConfig); 
            var result = await speciesClient.DeleteSpeciesAsync(speciesId);
            return true;
        }
        return false;

    }

}
