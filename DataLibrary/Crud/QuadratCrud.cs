using DataLibrary.DataSources;
using DataLibrary.DataSources.ApiClients;
using DataLibrary.ModelExtensions;
using Models;

namespace DataLibrary.Crud
{
    public static class QuadratCrud
    {
        #region Read Methods

        public static async Task<IEnumerable<QuadratBase>> ReadAllQuadrats(IDataSourceConfig config)
        {
            IEnumerable<QuadratBase> quadrats = new List<QuadratBase>();
            if (config is MySqlConfig mySqlConfig)
            {
               quadrats = await DataHelper.ReadAllEntriesAsync<QuadratBase>(config, QuadratEntry.TableName);

            }
            else if (config is ApiClientConfig apiConfig)
            {
                QuadratApiClient client = new QuadratApiClient(apiConfig);
                quadrats = await client.GetAllQuadratsAsync();
            }
            return quadrats;
        }

        public static async Task<List<QuadratBase>> ReadQuadratEntriesForSurveyAsync(IDataSourceConfig config, long surveyId)
        {
            if (surveyId <= 0 || config is null)
                throw new ArgumentException("Valid surveyId and configuration must be supplied");
            IEnumerable<QuadratBase> quadratEntries = new List<QuadratBase>();
            if (config is MySqlConfig mySqlConfig)
            {
               quadratEntries = (await DataHelper.ReadFilteredEntries<QuadratBase>(config, QuadratEntry.TableName, $" WHERE SurveyID = {surveyId}")).ToList();
            } else if (config is ApiClientConfig apiConfig)
            {
                QuadratApiClient client = new QuadratApiClient(apiConfig);
                IEnumerable<QuadratBase> apiQuadrats = await client.GetQuadratsBySurveyIdAsync(surveyId);
                quadratEntries = apiQuadrats.ToList();
            }
            return quadratEntries.ToList();
        }

        #endregion Read Methods


        #region Create/Update Methods

        public static async Task<List<QuadratBase>> UpdateOrCreateQuadratsAsync(IDataSourceConfig config, long surveyId, List<QuadratBase> quadratEntries)
        {

            if (config == null || quadratEntries == null)
                throw new ArgumentException("You must create the survey first and provide a valid surveyId");
            if (surveyId <= 0)
                throw new ArgumentException("Survey ID must be greater than zero.");

            // Get a list of current ID's for the Quadrat entries for the survey so we can determine which ones need to be removed
            List<long> currentIds = new();
            foreach (var entry in quadratEntries)
            {
                entry.EntryDate = DateTime.Now;

                if (entry.ID > 0) currentIds.Add(entry.ID);
            }
            await DeleteFromQuadratEntriesNotInListAsync(config, surveyId, currentIds);

            if (config is MySqlConfig mySqlConfig)
            {

                foreach (var quadratEntry in quadratEntries)
                {
                    (long entryId, string guid) = await MySqlHelperUtils.InsertOrUpdateRecordAsync<QuadratBase>(
                        quadratEntry,
                        action: quadratEntry.ID > 0 ? Actions.Replace : Actions.Insert,
                        keyfield: "ID",
                        keytype: KeyTypes.Long,
                        currentId: quadratEntry.ID);
                    quadratEntry.ID = entryId;
                }
            }
            else if (config is ApiClientConfig apiClientConfig)
            {
                QuadratApiClient apiClient = new QuadratApiClient(apiClientConfig);
                var quadrats = await apiClient!.CreateOrUpdateQuadratsAsync(surveyId, quadratEntries);
                quadratEntries = quadrats.ToList();
            }

            return quadratEntries ?? new List<QuadratBase>();
        }

        public static async Task<bool> DeleteFromQuadratEntriesNotInListAsync(IDataSourceConfig config, long surveyId, List<long> pelist)
        {
            if (config is MySqlConfig mySqlConfig)
            {
                // Delete the Quadrat entries for the survey that are NOT in the list. If the list is empty, it will delete all entries for the survey
                var result = await MySqlHelperUtils.DeleteFromTableNotInIdList(mySqlConfig, "QuadratEntries", surveyId, pelist);
                return true;
            }
            else if (config is ApiClientConfig apiClientConfig)
            {
                QuadratApiClient apiClient = new QuadratApiClient(apiClientConfig);
                return await apiClient.DeleteQuadratEntriesAsync(surveyId, pelist);
            }
            throw new NotImplementedException("Delete operation is not implemented for this data source.");
        }

    }
        #endregion Create/Update Methods

}
