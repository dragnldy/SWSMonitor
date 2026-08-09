using DataLibrary.DataSources;
using DataLibrary.DataSources.ApiClients;
using Models;

namespace DataLibrary.Crud;

public static class SurveyMonitorCrud
{
    #region Read Methods
    /// <summary>
    /// Reads Monitor records from a view for a specific survey
    /// </summary>
    public static async Task<List<SurveyMonitor>> ReadSurveyMonitorsBySurveyIdAsync(IDataSourceConfig config, long surveyId)
    {
        if (config == null || surveyId <= 0)
        {
            return new List<SurveyMonitor>();
        }
        if (config is MySqlConfig mySqlConfig)
        {
            IEnumerable<SurveyMonitor> monitors = await DataHelper.ReadEntries<SurveyMonitor>(config, SurveyMonitor.TableName,keyfield: "SurveyID", id: surveyId);
            return monitors.OrderBy(mn => mn.Monitor).ToList();
        }
        else if (config is ApiClientConfig apiConfig)
        {
            SurveyMonitorApiClient apiClient = new SurveyMonitorApiClient(apiConfig);
            var result = await apiClient.GetSurveyMonitorsBySurveyIdAsync(surveyId);

            return result ?? new List<SurveyMonitor>();
        }

        throw new NotSupportedException("Unsupported data source configuration for reading MonitorNames by survey ID");
    }

    #endregion Read Methods
}
