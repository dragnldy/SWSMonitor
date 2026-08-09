using DataLibrary.DataSources;
using DataLibrary.DataSources.ApiClients;
using DataLibrary.ModelExtensions;
using DataLibrary.Models;
using Models;
using MySqlConnector;

namespace DataLibrary.Crud;

public class SurveyCrud
{
    #region Load all  Survey Data by Beach and Date- loads all data for the survey unless BaseOnly is true
    public static async Task<Survey> ReadSurveyData(IDataSourceConfig config, string beachName, DateTime surveyDate, bool BaseOnly = false)
    {
        if (config is null || string.IsNullOrEmpty(beachName))
        {
            throw new ArgumentNullException("surveybase info missing");
        }
        long surveyId = -1L;

        // First try to retrieve surveybase from in-memory list
        SurveyBase? surveyBase = null;
        if (StaticData.Surveys is not null && StaticData.Surveys.Count > 0)
            surveyBase = StaticData.Surveys!.FirstOrDefault(s => s.BeachName!.Equals(beachName, StringComparison.OrdinalIgnoreCase)
                && s.SurveyDate == surveyDate);

        // If not found in memory, read from data source
        if (surveyBase is null)
        {
            surveyBase = await ReadSurveyBaseByBeachAndDateAsync(config, beachName, surveyDate);
        }
        // If still not found, create a new surveybase base
        if (surveyBase is null)
        {
            // ID is determined when we save it
            surveyBase = new SurveyBase()
            {
                ID = surveyId,
                BeachName = beachName,
                SurveyDate = surveyDate,
                Exported2UW = 0
            };
        }
        if (BaseOnly)
        {
            // Return only the base surveybase info
            Survey survey = new Survey(surveyBase);
            return survey;
        }
        return await LoadSurveyData(config , surveyBase);
    }
    #endregion

    #region Load all Survey Data by SurveyID- loads all data for the survey
    public static async Task<Survey> ReadSurveyData(IDataSourceConfig config, long surveyId, bool BaseOnly = false)
    {
        if (config is null)
        {
            throw new ArgumentNullException("surveybase info missing");
        }

        // First try to retrieve surveybase from in-memory list
        SurveyBase? surveyBase = null;
        if (StaticData.Surveys is not null && StaticData.Surveys.Count > 0)
            surveyBase = StaticData.Surveys!.FirstOrDefault(s => s.ID == surveyId);

        // If not found in memory, read from data source
        if (surveyBase is null)
        {
            surveyBase = await ReadSurveyRecordByIdAsync(config, surveyId);
        }

        if (BaseOnly)
        {
            // Return only the base surveybase info
            Survey survey = new Survey(surveyBase);
            return survey;
        }
        return await LoadSurveyData(config, surveyBase);
    }
    #endregion

    #region Load all Survey Data by SurveyBase- loads all data for the survey
    public static async Task<Survey> ReadSurveyData(IDataSourceConfig config, SurveyBase? surveyBase, bool BaseOnly = false)
    {
        if (config is null || surveyBase is null)
        {
            throw new ArgumentNullException("surveybase info missing");
        }

        if (BaseOnly)
        {
            // Return only the base surveybase info
            Survey survey = new Survey(surveyBase);
            return survey;
        }
        return await LoadSurveyData(config, surveyBase);
    }

    #endregion

    #region Load all survey data when supplied with surveybase record- common logic for all three load routines
    public static async Task<Survey> LoadSurveyData(IDataSourceConfig config, SurveyBase? surveyBase)
    {
        if (surveyBase is null)
        {
            throw new ArgumentNullException("surveybase info missing");
        }
        // ID is determined when we save it
        Survey survey = new Survey(surveyBase);

        try
        {
            survey.BeachEvent = await BeachEventCrud.ReadBeachEventBySurveyId(config, survey.ID);
            survey.ProfileEntries = await ProfileCrud.ReadProfilesForSurveyAsync(config, survey.ID);
            survey.QuadratEntries =  await QuadratCrud.ReadQuadratEntriesForSurveyAsync(config, survey.ID);
            return survey;
        }
        catch (Exception exc)
        {
            System.Diagnostics.Debug.WriteLine("Error");
        }
        return null;
    }
    #endregion

    #region Reads All SurveyBase Records- used to get alist of surveys by beach and date
    public static async Task<List<SurveyBase>> ReadAllSurveyRecordsAsync(IDataSourceConfig? config)
    {
        if (config is null)
        {
            throw new ArgumentNullException("config info missing");
        }
        List<DataRecord> records = new();
        if (config is MySqlConfig mySqlConfig)
        {
            records = await DataSources.MySqlHelperUtils.ReadTable(config, SurveyBase.TableName);
            var numrecords = records.Count;
            if (numrecords == 0)
            {
                return new List<SurveyBase>();
            }
            IEnumerable<SurveyBase> surveys = DeserializeRecords(records);
            return surveys.ToList();
        }
        else if (config is ApiClientConfig apiClientConfig)
        {
            SurveyApiClient surveyClient = new SurveyApiClient(apiClientConfig);
            List<SurveyBase> results = await surveyClient.GetAllSurveysAsync();
            return results;
        }
        return new List<SurveyBase>();
    }
    #endregion

    #region Read SurveyBase Record by Beach and Date
    public static async Task<SurveyBase?> ReadSurveyBaseByBeachAndDateAsync(IDataSourceConfig config, string beachName, DateTime surveyDate)
    {
        string filter = $"BeachName = '{beachName}'";
        filter = FormatDateForFilter(config, filter, surveyDate);

        List<SurveyBase> surveys = new List<SurveyBase>();

        if (config is MySqlConfig mySqlConfig)
        {
            filter = $"WHERE {filter}";
            surveys = await ReadSurveyRecordsByFilterAsync(config, filter);
        }
        else if (config is ApiClientConfig apiClientConfig)
        {
            SurveyApiClient surveyClient = new SurveyApiClient(apiClientConfig);
            surveys = await surveyClient.GetSurveysByFilterAsync(filter);
        }

        var numrecords = surveys.Count();
        if (numrecords == 0)
        {
            return null;
        }
        return surveys.FirstOrDefault(n => n.BeachName.Equals(beachName, StringComparison.InvariantCultureIgnoreCase) &&
                                         n.SurveyDate.Year == surveyDate.Year && n.SurveyDate.DayOfYear == surveyDate.DayOfYear);
    }

    private static string FormatDateForFilter(IDataSourceConfig config, string filter, DateTime surveyDate)
    {
        {
            return $"{filter} AND SurveyDate = '{surveyDate:yyyy-MM-dd}'";
        }
    }
    #endregion

    #region Reads specific SurveyBase Record- used to get survey with specific ID
    public static async Task<SurveyBase> ReadSurveyRecordByIdAsync(IDataSourceConfig? config, long surveyid)
    {
        if (config is null)
        {
            throw new ArgumentNullException("config info missing");
        }
        IEnumerable<SurveyBase> surveys = await ReadSurveyRecordsByFilterAsync(config, $"ID = {surveyid}");

        var numrecords = surveys.Count();
        if (numrecords == 0)
        {
            return null;
        }
        return surveys.FirstOrDefault();
    }
    #endregion

    #region Reads filtered SurveyBase Records- used to get survey with specific filter criteria
    public static async Task<List<SurveyBase>> ReadSurveyRecordsByFilterAsync(IDataSourceConfig? config, string filter)
    {
        if (config is null)
        {
            throw new ArgumentNullException("config info missing");
        }
        List<DataRecord> records = new();
        if (config is MySqlConfig mySqlConfig)
        {
            records = await DataLibrary.DataSources.MySqlHelperUtils.ReadTable(config, SurveyBase.TableName, filter);
            var numrecords = records.Count;
            if (numrecords == 0)
            {
                return new List<SurveyBase>();
            }
            IEnumerable<SurveyBase> surveys = DeserializeRecords(records);
            return surveys.ToList();
        }
        else if (config is ApiClientConfig apiClientConfig)
        {
            SurveyApiClient surveyClient = new SurveyApiClient(apiClientConfig);
            List<SurveyBase> results = await surveyClient.GetSurveysByFilterAsync(filter);
            return results;
        }

        return new List<SurveyBase>();
    }
    #endregion


    #region Read Survey History for Beach so can get a list of dates surveyed
    public static async Task<List<SurveyBase>> ReadSurveyHistory(IDataSourceConfig? config, BeachData? beachData)
    {
        return await ReadSurveyHistory(config, beachData.BeachName);
    }
    public static async Task<List<SurveyBase>> ReadSurveyHistory(IDataSourceConfig? config, string beachname)
    {
        if (config is null || string.IsNullOrEmpty(beachname))
        {
            throw new ArgumentNullException("beach info missing");
        }
        if (StaticData.Surveys is not null && StaticData.Surveys.Count > 0)
        {
            return StaticData.Surveys.Where(s => s.BeachName.Equals(beachname, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }
        else
        {
            IEnumerable<SurveyBase> surveys = await ReadSurveyRecordsByFilterAsync(config, $"BeachName = '{beachname}'");
            return surveys.ToList();
        }
        return new List<SurveyBase>();
    }
    #endregion

    #region Helper routine to deserialize records- should update to generic method in DataHelper
    private static List<SurveyBase> DeserializeRecords(List<DataRecord> results)
    {
        //Todo: update to use generic method in DataHelper
        if (results == null)
        {
            System.Diagnostics.Debug.WriteLine("No records retrieved");
            return GetTestSurveyBaseData(); // Return empty list if no records found
        }
        try
        {
            List<SurveyBase> allsurveys = new List<SurveyBase>();
            // Get records from results
            foreach (var record in results)
            {

                // Deserialize the record manually into my SurveyBase object
                DataHelper.LoadClass<SurveyBase>(record, out object? myRecord);
                if (myRecord != null)
                {
                    allsurveys.Add((SurveyBase)myRecord);
                }
            }
            return allsurveys.Count > 0 ? allsurveys : GetTestSurveyBaseData(); // Return test data if no records found
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading Airtable records: {ex.Message}");
        }
        return GetTestSurveyBaseData(); // Return test data if no records found
    }
    #endregion Deserialize Records

    private static List<SurveyBase> GetTestSurveyBaseData()
    {
        return new List<SurveyBase>();
    }

    #region Create/Update Survey Records
    public static async Task<bool> SaveSurvey(Survey? loadedSurvey)
    {
        if (loadedSurvey == null) { return false; }

        // send the surveybase info to temporary cache that will be sent to permanent storage in the background
        // this allows us to collect data offline when necessary
     //   if (ExportSurvey.ExportToCache(loadedSurvey))
        {
            // update the in-memory list of surveys
            SurveyBase? existingSurvey = StaticData.Surveys?.FirstOrDefault(
                s => s.BeachName!.Equals(loadedSurvey.BeachName, StringComparison.OrdinalIgnoreCase)
                && s.SurveyDate == loadedSurvey.SurveyDate);
            if (existingSurvey != null)
            {
                SurveyBase.CopyProps(loadedSurvey, existingSurvey);
            }
            else
            {
                SurveyBase newSurvey = new();
                SurveyBase.CopyProps(loadedSurvey, newSurvey);
                StaticData.Surveys?.Add(newSurvey);
                existingSurvey = StaticData.Surveys?.FirstOrDefault(
                    s => s.BeachName!.Equals(loadedSurvey.BeachName, StringComparison.OrdinalIgnoreCase)
                    && s.SurveyDate == loadedSurvey.SurveyDate);
                loadedSurvey.SaveRequired.Add(ComponentsToSaveEnum.Base);
                loadedSurvey.SaveRequired.Add(ComponentsToSaveEnum.BeachEvent);
            }

            if (loadedSurvey.SaveRequired.Contains(ComponentsToSaveEnum.Base) == true)
            {
                // Save the basic surveybase info and get the surveybase ID if new surveybase
                var surveybase = await UpdateOrCreateSurveyBaseAsync(StaticData.DataSourceConfig, existingSurvey);
                loadedSurvey.ID = surveybase.ID;
                existingSurvey!.ID = surveybase.ID;
                loadedSurvey.SaveRequired.Remove(ComponentsToSaveEnum.Base);
            }
            if (loadedSurvey.SaveRequired.Contains(ComponentsToSaveEnum.BeachEvent) == true)
            {
                await UpdateBeachEvents(loadedSurvey);
                loadedSurvey.SaveRequired.Remove(ComponentsToSaveEnum.BeachEvent);
            }

            if (loadedSurvey.SaveRequired.Contains(ComponentsToSaveEnum.Profile) == true)
            {
                await UpdateProfiles(loadedSurvey);
                loadedSurvey.SaveRequired.Remove(ComponentsToSaveEnum.Profile);
            }
            if (loadedSurvey.SaveRequired.Contains(ComponentsToSaveEnum.Quadrat) == true)
            {
                await UpdateQuadrats(loadedSurvey);
                loadedSurvey.SaveRequired.Remove(ComponentsToSaveEnum.Quadrat);
            }

            if (StaticData.DataSourceConfig is MySqlConfig config)
            {
                MySqlConnection connection = MySqlHelperUtils.UpdateConnector(config);
                connection.Close();
            }
            return true;
        }
    }
    #endregion Create/Update Survey Records

    #region Create/Update Dependent Data Records

    private static async Task UpdateQuadrats(Survey loadedSurvey)
    {
        if (loadedSurvey.QuadratEntries is null)
        {
            loadedSurvey.QuadratEntries = new List<QuadratBase>();
        }
        loadedSurvey.QuadratEntries.ForEach(n => n.SurveyID = loadedSurvey.ID);
        await QuadratCrud.UpdateOrCreateQuadratsAsync(StaticData.DataSourceConfig, loadedSurvey.ID, loadedSurvey.QuadratEntries);
    }

    private static async Task UpdateProfiles(Survey loadedSurvey)
    {
        if (loadedSurvey.ProfileEntries is null)
        {
            loadedSurvey.ProfileEntries = new List<ProfileBase>();
        }
        loadedSurvey.ProfileEntries.ForEach(n => n.SurveyID = loadedSurvey.ID);
        await ProfileCrud.CreateOrUpdateProfilesAsync(StaticData.DataSourceConfig, loadedSurvey.ID, loadedSurvey.ProfileEntries);
    }

    private static async Task UpdateBeachEvents(Survey loadedSurvey)
    {
        if (loadedSurvey.BeachEvent is null)
            loadedSurvey.BeachEvent = new BeachEvent(id: 0, surveyid: 0, beachName: "Unknown", surveyDate: DateTime.MinValue);
        loadedSurvey.BeachEvent.SurveyID = loadedSurvey.ID;

        await BeachEventCrud.SaveBeachEventAsync(StaticData.DataSourceConfig, loadedSurvey.BeachEvent);
    }
    #endregion

    #region Create/Update Survey Base Record

    public static async Task<long> SaveSurvey(IDataSourceConfig? dataSourceConfig, Survey loadedSurvey)
    {
        throw new NotImplementedException("Use SaveSurvey(Survey) instead to save to cache and then permanent storage in the background");
        SurveyBase baseSurvey = new SurveyBase();
        SurveyBase.CopyProps(loadedSurvey, baseSurvey);
        SurveyBase surveybase = await UpdateOrCreateSurveyBaseAsync(dataSourceConfig, baseSurvey);
        return surveybase.ID;
    }

    public static async Task<SurveyBase?> UpdateOrCreateSurveyBaseAsync(IDataSourceConfig  dataSourceConfig, SurveyBase surveybase)
    {
        try
        {
            surveybase.EntryDate = DateTime.Now;
            long surveyID = surveybase.ID;
            {
                string action = surveybase.ID <= 0 ? "Insert" : "Replace";
                if (dataSourceConfig is MySqlConfig sqlconfig)
                {
                    surveyID = await DataLibrary.DataSources.MySqlHelperUtils.InsertOrUpdateRecordByIdAsync<SurveyBase>(sqlconfig, surveybase, currentId: surveybase.ID);
                    if (surveyID > 0)
                    {
                        surveybase.ID = surveyID;
                        return surveybase;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Error inserting or updating surveybase record to MySQL");
                        return null;
                    }
                }
                else if (dataSourceConfig is ApiClientConfig apiClientConfig)
                {
                    SurveyApiClient surveyClient = new SurveyApiClient(apiClientConfig);
                    SurveyBase? newSurvey = await surveyClient.UpdateOrCreateSurveyAsync(surveybase!);
                    if (newSurvey != null && newSurvey.ID > 0)
                    {
                        surveybase.ID = newSurvey.ID;
                        return newSurvey;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Error inserting or updating surveybase record to API");
                        return null;
                    }
                }
            }
        }
        catch (Exception exc)
        {
            System.Diagnostics.Debug.WriteLine("Error");
            return null;
        }
        return surveybase;
    }
    #endregion Create/Update Survey Base Record

    #region Delete SurveyBase and Survey with Details

    public static async Task<bool> DeleteSurveyBase(IDataSourceConfig? dataSourceConfig, long surveyID)
    {
        try
        {
            await DataLibrary.DataSources.MySqlHelperUtils.ExecuteNonQueryAsync(dataSourceConfig,$"DELETE FROM `{SurveyBase.TableName}` WHERE Id = {surveyID}");
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    #endregion Delete

}
