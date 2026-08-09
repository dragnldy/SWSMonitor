using DataLibrary.DataSources;
using DataLibrary.DataSources.ApiClients;
using DataLibrary.ModelExtensions;
using Models;
using System.Net;

namespace DataLibrary.Crud;

public static class BackShoreCrud
{
    #region Read Methods- Not used by data entry but supplied by the API from a view
    public static async Task<IEnumerable<BackShore>?> ReadAllBackShoreAsync(IDataSourceConfig? config)
    {
        if (config is MySqlConfig)
        {
            return await DataHelper.ReadAllEntriesAsync<BackShore>(config, BackShore.TableName);
        }
        else if (config is ApiClientConfig apiConfig)
        {
            BackshoreApiClient client = new BackshoreApiClient(apiConfig);
            List<BackShore>? results = await client.GetAllBackshoreAsync();
            return results;
        }
        TraceLogger.LogWarningAuto("Invalid configuration type provided.");
        throw new Exception("Unsupported configuration type");
    }
    #endregion

    #region Read Methods- Used by data entry
    public static async Task<IEnumerable<BackShore>> ReadBackShoreBySurveyId(IDataSourceConfig config, long surveyID)
    {
        if (config is MySqlConfig)
        {
            return await DataHelper.ReadEntries<BackShore>(config, BackShore.TableName, surveyID);
        }
        else if (config is ApiClientConfig apiConfig)
        {
            BackshoreApiClient client = new BackshoreApiClient(apiConfig);
            List<BackShore>? results = await client.GetBackshoreForSurveyAsync(surveyID);
            return results;
        }
        TraceLogger.LogWarningAuto("Invalid configuration type provided.");
        throw new Exception("Unsupported configuration type");
    }
    #endregion

}