using DataLibrary.DataSources;
using DataLibrary.DataSources.ApiClients;
using Models;
using System.Net;

namespace DataLibrary.Crud;

public static class BeachEventCrud
{
    // This class is responsible for reading and writing BeachEvent records to and from data sources like Airtable.
    // It uses the AirtableHelper to interact with the Airtable API.
    // Or the MySqlHelperUtils to interact with a MySQL database.
    /// <summary>
    /// Reads all BeachEvent data from Airtable or MySQL based on the provided configuration.
    /// </summary>
    /// <param name="config">The configuration for connecting to Airtable or MySql.</param>
    /// <returns>A list of BeachEvent objects.</returns>
    /// 
    public static async Task<IEnumerable<BeachEventBase>> ReadAllBeachEventsAsync(IDataSourceConfig config)
    {

        List<BeachEventBase> allBeachEvents = new List<BeachEventBase>();
        if (config is MySqlConfig mySqlConfig)
        {
            List<DataRecord> results = await MySqlHelperUtils.ReadTable(config, BeachEventBase.TableName);
            return DeserializeRecords(results);
        }
        else if (config is ApiClientConfig apiConfig)
        {
            BeachEventApiClient beachEventClient = new BeachEventApiClient(apiConfig);
            var results = await beachEventClient.GetAllBeachEventsAsync();
            return results;
        }
        throw new ArgumentException("Invalid configuration type provided. Expected MySqlConfig.", nameof(config));

        //var beachEvents = await BeachEventCrud.ReadBeachEventBasesAsync(config);
        //var surveybase = await SurveyCrud.ReadAllSurveyRecordsAsync(config);

        //// Populate the beach name and date for each event by matching the survey base records.
        //foreach (var beachEvent in beachEvents)
        //{
        //    var matchingSurvey = surveybase.FirstOrDefault(s => s.ID == beachEvent.SurveyID);
        //    if (matchingSurvey != null)
        //    {
        //        beachEvent.BeachName = matchingSurvey.BeachName;
        //        beachEvent.SurveyDate = matchingSurvey.SurveyDate;
        //    }
        //    else
        //    {
        //        TraceLogger.LogWarningAuto($"No matching survey found for BeachEvent with SurveyID {beachEvent.SurveyID}");
        //        beachEvent.BeachName = "Unknown Beach";
        //        beachEvent.SurveyDate = DateTime.MinValue;
        //    }
        //}
        //return beachEvents;
    }

    #region Read Methods- Gets all Beach Event records- Not used for data entry app
    public static async Task<IEnumerable<BeachEventBase>> ReadBeachEventBasesAsync(IDataSourceConfig config)
    {

        List<BeachEvent> allBeachEventInfos = new List<BeachEvent>();
        if (config is MySqlConfig mySqlConfig)
        {
            List<DataRecord> results = await MySqlHelperUtils.ReadTable(config, BeachEventBase.TableName);
            return DeserializeRecords(results);
        }
        else if (config is ApiClientConfig apiConfig)
        {
            BeachEventApiClient beachEventClient = new BeachEventApiClient(apiConfig);
            var results = await beachEventClient.GetAllBeachEventsAsync();
            return results;
        }
        throw new ArgumentException("Invalid configuration type provided. Expected MySqlConfig.", nameof(config));
    }
    #endregion Read Methods- Gets all Beach Event records- Not used for data entry app

    #region Read Methods- gets all beach event records for a specific beach- Not used for data entry app
    public static async Task<List<BeachEventBase>> ReadEventsForBeach(IDataSourceConfig config, string beachName)
    {
        if (config is null || string.IsNullOrEmpty(beachName))
        {
            throw new ArgumentNullException("Beach info is missing");
        }

        List<DataRecord> records = new();
        if (config is MySqlConfig mySqlConfig)
        {
            if (MySqlHelperUtils.IsSafeFromInjectionString(beachName) == false)
            {
                TraceLogger.LogWarningAuto("Potentially unsafe input detected. Operation aborted.");
                return new List<BeachEventBase>();
            }
            records = await MySqlHelperUtils.ReadTable(config, BeachEvent.TableName,
                $"BeachName = '{beachName}'");
            var numrecords = records.Count;
            if (numrecords == 0)
            {
                return new List<BeachEventBase>();
            }
            IEnumerable<BeachEventBase> beachEvents = DeserializeRecords(records);
            return beachEvents.OrderBy(o => o.SurveyID).ToList();
        }
        else if (config is ApiClientConfig apiConfig)
        {
            BeachEventApiClient beachEventClient = new BeachEventApiClient(apiConfig);
            var results = await beachEventClient.GetEventsForBeachAsync(beachName);
            return results;
        }
        throw new ArgumentException("Invalid configuration type provided. Expected MySqlConfig.", nameof(config));

    }
    #endregion Read Methods- gets all beach event records for a specific beach- Not used for data entry app

    #region Routine used by Read Methods to deserialize records- can replace this with generic method in DataHelper
    private static List<BeachEvent> DeserializeRecords(List<DataRecord> results)
    {
        //ToDo: Replace this with generic method in DataHelper
        if (results == null)
        {
            System.Diagnostics.Debug.WriteLine("No records retrieved");
        }
        try
        {
            List<BeachEvent> allBeachEventInfos = new List<BeachEvent>();
            // Get records from results
            foreach (var record in results)
            {

                // Deserialize the record manually into my BeachData object
                DataHelper.LoadClass<BeachEvent>(record, out object? myRecord);
                if (myRecord != null)
                {
                    allBeachEventInfos.Add((BeachEvent)myRecord);
                }
            }
            return allBeachEventInfos.Count > 0 ? allBeachEventInfos : new List<BeachEvent>(); // Return empty list if no records found
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading Airtable records: {ex.Message}");
        }
        return new List<BeachEvent>();
    }
    #endregion Routine used by Read Methods to deserialize records- can replace this with generic method in DataHelper

    #region Method to read a single Beach Event record by SurveyID- used in data entry app
    public static async Task<BeachEvent?> ReadBeachEventBySurveyId(IDataSourceConfig config, long surveyId)
    {
        if (config is null)
        {
            throw new ArgumentNullException("Config must be supplied");
        }

        if (surveyId < 0)
            return null;

        List<DataRecord> records = new();
        if (config is MySqlConfig mySqlConfig)
        {
            records = await MySqlHelperUtils.ReadTable(config, BeachEvent.TableName,
                $"SurveyID = {surveyId}");

            if (records.Count == 0)
            {
                return null;
            }
            IEnumerable<BeachEvent> beachEvents = DeserializeRecords(records);
            BeachEvent? beachEvent = beachEvents.FirstOrDefault();
            if (beachEvent is null) return null;

            SurveyBase survey = await SurveyCrud.ReadSurveyRecordByIdAsync(config, surveyId);
            if (survey is null) { beachEvent.BeachName = "Unknown Beach"; return beachEvent; }

            beachEvent.BeachName = survey.BeachName;
            beachEvent.SurveyDate = survey.SurveyDate;

            return beachEvent;
        }
        else if (config is ApiClientConfig apiConfig)
        {
            BeachEventApiClient beachEventClient = new BeachEventApiClient(apiConfig);

            var results = await beachEventClient.GetBeachEventBySurveyIdAsync(surveyId);
            return results;
        }

        throw new ArgumentException("Invalid configuration type provided. Expected MySqlConfig.", nameof(config));
    }


    // Returns the BeachEventBase with the given event ID which is not the same as the SurveyID
    public static async Task<BeachEventBase?> ReadBeachEventById(IDataSourceConfig config, long eventId)
    {
        if (config is null)
        {
            throw new ArgumentNullException("Config must be supplied");
        }

        if (eventId < 0)
            return null;

        if (config is MySqlConfig mySqlConfig)
        {
            IEnumerable<BeachEventBase> events = await DataHelper.ReadEntries<BeachEventBase>(config, BeachEventBase.TableName, eventId, keyfield: "ID");
            var beachEvent = events.FirstOrDefault();
            if (beachEvent is null || beachEvent.ID != eventId)
            {
                TraceLogger.LogErrorAuto($"Beach event not found with ID: {eventId}");
                return null;
            }
            return beachEvent;
        }
        else if (config is ApiClientConfig apiConfig)
        {
            BeachEventApiClient beachEventClient = new BeachEventApiClient(apiConfig);

            var results = await beachEventClient.GetBeachEventByEventIdAsync(eventId);
            return results;
        }

        throw new ArgumentException("Invalid configuration type provided. Expected MySqlConfig.", nameof(config));
    }
    #endregion Method to read a single Beach Event record by SurveyID- used in data entry app

    #region Method to save Beach Event record- used in data entry app
    public static async Task<BeachEventBase?> SaveBeachEventAsync(IDataSourceConfig config, BeachEventBase beachEventBase)
    {
        try
        {
            if (beachEventBase.ID <= 0) beachEventBase.EntryDate = DateTime.Now;
            if (config is MySqlConfig)
            {
                long ID = await MySqlHelperUtils.InsertOrUpdateRecord<BeachEventBase>(
                beachEventBase, keyfield: "ID", action: beachEventBase.ID > 0 ? "Replace" : "Insert", currentId: beachEventBase.ID);

                beachEventBase.ID = ID;
                if (ID > 0)
                {
                    return beachEventBase;
                }
                return null;
            }
            else if (config is ApiClientConfig apiConfig)
            {
                BeachEventApiClient apiClient = new BeachEventApiClient(apiConfig);
                BeachEventBase? newEvent = await apiClient.CreateOrUpdateBeachEventAsync(beachEventBase);
                return newEvent;
            }
            throw new ArgumentException("Invalid configuration type provided. Expected MySqlConfig.");

        }
        catch (Exception exc)
        {
            System.Diagnostics.Debug.WriteLine("Error");
            return null;
        }
        return null;
    }

    public static async Task<HttpStatusCode> DeleteBeachEventBySurveyIdAsync(IDataSourceConfig config, long surveyId)
    {
        if (config is MySqlConfig mySqlConfig)
        {
            // Delete the BeachEvent record with given SurveyID
            await MySqlHelperUtils.ExecuteNonQueryAsync(config, $"DELETE FROM `{BeachEventBase.TableName}` WHERE SurveyID = {surveyId}");
            return HttpStatusCode.NoContent;
        }
        else if (config is ApiClientConfig apiConfig)
        {
            BeachEventApiClient apiClient = new BeachEventApiClient(apiConfig);
            HttpStatusCode result = await apiClient.DeleteBeachEventBySurveyIdAsync(surveyId);
            return result;
        }
        throw new ArgumentException("Invalid configuration type provided. Expected MySqlConfig or ApiClientConfig.");
    }

    public static async Task<HttpStatusCode> DeleteBeachEventByIdAsync(IDataSourceConfig config, long eventId)
    {
        if (config is MySqlConfig mySqlConfig)
        {
            // Delete the BeachEvent record with given EventID
            await MySqlHelperUtils.ExecuteNonQueryAsync(config, $"DELETE FROM `{BeachEventBase.TableName}` WHERE ID = {eventId}");
            return HttpStatusCode.NoContent;
        }
        else if (config is ApiClientConfig apiConfig)
        {
            BeachEventApiClient apiClient = new BeachEventApiClient(apiConfig);
            HttpStatusCode result = await apiClient.DeleteBeachEventByEventIdAsync(eventId);
            return result;
        }
        throw new ArgumentException("Invalid configuration type provided. Expected MySqlConfig or ApiClientConfig.");
    }


    #endregion Method to save Beach Event record- used in data entry app
}

