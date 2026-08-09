using DataLibrary.DataSources;
using DataLibrary.DataSources.ApiClients;
using Models;

namespace DataLibrary.Crud;

public class SpeciesListCrud
{

    // Note- this is a view so it is read only
    #region Read Methods- Not used by data entry
    public static async Task<IEnumerable<SpeciesList>?> ReadSpeciesListsForSurveyAsync(IDataSourceConfig config, long surveyID)
    {
        if (config is ApiClientConfig apiConfig)
        {
            SpeciesListApiClient client = new SpeciesListApiClient(apiConfig);
            List<SpeciesList>? results = await client.GetSpeciesListsForSurveyAsync(surveyID);
            return results;

        }
        else if (config is MySqlConfig sqlConfig)
        {
            return await DataHelper.ReadEntries<SpeciesList>(config, SpeciesList.TableName, surveyID);
        }

        throw new NotImplementedException();
    }
    #endregion
}
