using AirtableApiClient;
using DataLibrary.DataSources;
using Models;
using System.Diagnostics;

namespace DataLibrary.Crud;

public static class QuadratEntryCrudExtended
{
    #region Read Methods
    /// <summary>
    /// Reads all QuadratEntry records from the data source
    /// </summary>
    public static async Task<List<QuadratEntry>> ReadAllQuadratEntries(object config)
    {
        if (config == null)
        {
            return new List<QuadratEntry>();
        }
        IEnumerable<QuadratEntry> quadratEntries = await DataHelper.ReadAllEntries<QuadratEntry>(config, QuadratEntry.TableName);
        return quadratEntries.OrderBy(qe => qe.SurveyID).ThenBy(qe => qe.QuadratID).ToList();
    }

    /// <summary>
    /// Reads QuadratEntry records for a specific quadrat ID
    /// </summary>
    public static async Task<List<QuadratEntry>> ReadQuadratEntriesByQuadratId(object config, long surveyId, int quadratId)
    {
        if (config == null)
        {
            return new List<QuadratEntry>();
        }
        IEnumerable<QuadratEntry> quadratEntries = await DataHelper.ReadEntries<QuadratEntry>(config, QuadratEntry.TableName, id: surveyId);
        return quadratEntries.Where(qe => qe.QuadratID == quadratId).ToList();
    }
    #endregion Read Methods

    #region Create/Update Methods
    /// <summary>
    /// Creates a new QuadratEntry record or updates an existing one
    /// </summary>
    public static async Task<(bool success, QuadratEntry quadratEntry)> CreateOrUpdateQuadratEntry(QuadratEntry quadratEntry)
    {
        if (quadratEntry == null)
        {
            return (false, quadratEntry);
        }

        if (StaticData.DataSourceConfig is MySqlConfig mySqlConfig)
        {
            // QuadratEntry doesn't have a primary key ID field, so we use Replace action
            // which updates if exists or inserts if new based on unique key (SurveyID + QuadratID)
            long result = await MySqlHelper.InsertOrUpdateRecord<QuadratEntry>(
                quadratEntry, query: "", keyfield: "", action: "Replace");

            return (result >= 0, quadratEntry);
        }

        return (false, quadratEntry);
    }

    /// <summary>
    /// Creates multiple QuadratEntry records in bulk
    /// </summary>
    public static async Task<(bool success, int count)> CreateQuadratEntries(IEnumerable<QuadratEntry> quadratEntries)
    {
        if (quadratEntries == null || !quadratEntries.Any())
        {
            return (false, 0);
        }

        int successCount = 0;
        foreach (var quadratEntry in quadratEntries)
        {
            var (success, _) = await CreateOrUpdateQuadratEntry(quadratEntry);
            if (success)
            {
                successCount++;
            }
        }

        return (successCount == quadratEntries.Count(), successCount);
    }
    #endregion Create/Update Methods

    #region Delete Methods
    /// <summary>
    /// Deletes a single QuadratEntry record
    /// Note: For MySql, this uses internal methods and should be called from within the DataLibrary assembly
    /// </summary>
    internal static async Task<bool> DeleteQuadratEntry(QuadratEntry quadratEntry)
    {
        if (quadratEntry == null)
        {
            return false;
        }

        try
        {
            if (StaticData.DataSourceConfig is MySqlConfig mySqlConfig)
            {
                string whereClause = $"SurveyID = {quadratEntry.SurveyID}";

                if (quadratEntry.QuadratID.HasValue)
                {
                    whereClause += $" AND QuadratID = {quadratEntry.QuadratID.Value}";
                }

                MySqlHelper.ExecuteNonQuery($"DELETE FROM `{QuadratEntry.TableName}` WHERE {whereClause}");
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error deleting QuadratEntry: {ex.Message}");
            return false;
        }

        return false;
    }

    /// <summary>
    /// Deletes all QuadratEntry records for a specific survey
    /// Note: For MySql, this uses internal methods and should be called from within the DataLibrary assembly
    /// </summary>
    internal static async Task<bool> DeleteQuadratEntriesBySurvey(long surveyId)
    {
        try
        {
            if (StaticData.DataSourceConfig is MySqlConfig mySqlConfig)
            {
                MySqlHelper.ExecuteNonQuery($"DELETE FROM `{QuadratEntry.TableName}` WHERE SurveyID = {surveyId}");
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error deleting QuadratEntries for survey {surveyId}: {ex.Message}");
            return false;
        }

        return false;
    }
    #endregion Delete Methods
}
