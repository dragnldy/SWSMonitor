using DataLibrary.DataSources;
using DataLibrary.DataSources.ApiClients;
using System.Reflection;
using System.Text.Json;

namespace DataLibrary.Crud;

public class  DataRecord : BaseRecord
{
    
}
public static class DataHelper
{
    public static async Task<IEnumerable<T>> ReadAllEntriesAsync<T>(IDataSourceConfig? config, string tableName)
    {
        if (config is MySqlConfig mySqlConfig)
        {
            List<DataRecord> results = await MySqlHelperUtils.ReadTable(config, tableName);
            return DeserializeRecords<T>(results);
        }
        else if (config is ApiClientConfig apiClient)
        {
            // NOT SUPPORTED- use data specific client
        }
        ReportConfigError(nameof(DataHelper), nameof(ReadAllEntriesAsync), "Invalid configuration type provided.");
        throw new Exception("Unsupported configuration type");
    }


    public static async Task<IEnumerable<DataRecord>> ReadAllViewEntries(IDataSourceConfig? config, string viewName)
    {

        List<DataRecord> allEntries = new List<DataRecord>();
        if (config is MySqlConfig mySqlConfig)
        {
            List<DataRecord> results = await MySqlHelperUtils.ReadTable(config, viewName);
            return results;
        }
        ReportConfigError(nameof(DataHelper), nameof(ReadAllViewEntries), "Invalid configuration type provided.");
        throw new Exception("Unsupported configuration type");
    }


    private static void ReportConfigError(string errClass, string errModule, string error)
    {
        TraceLogger.LogError(errClass, errModule, error);
        throw new Exception("Error- see log");
    }


    internal static async Task<IEnumerable<T>> ReadEntries<T>(IDataSourceConfig config, string tableName, string beachName, DateTime surveyDate)
    {
        if (string.IsNullOrEmpty(tableName) || config is null)
        {
            ReportConfigError(nameof(DataHelper), nameof(ReadEntries), "No table name or configuration type provided.");
        }

        List<DataRecord> records = new();
        if (config is MySqlConfig mySqlConfig)
        {
            string dateString = surveyDate.ToString("yyyy-MM-dd");
            records = await MySqlHelperUtils.ReadTable(mySqlConfig, tableName,
                $"BeachName = '{beachName}' and Date = '{dateString}'");
            return DeserializeRecords<T>(records);
        }
        ReportConfigError(nameof(DataHelper), nameof(ReadEntries), "Invalid configuration type provided.");
        throw new Exception("Unsupported configuration type");
    }


    internal static async Task<IEnumerable<T>> ReadFilteredEntries<T>(IDataSourceConfig config, string tableName, string filter = "")
    {
        if (string.IsNullOrEmpty(tableName) || config is null)
        {
            ReportConfigError(nameof(DataHelper), nameof(ReadFilteredEntries), "No table name or configuration type provided.");
        }

        List<DataRecord> records = new();
        if (config is MySqlConfig mySqlConfig)
        {
            records = await MySqlHelperUtils.ReadTable(mySqlConfig, tableName, filter);
            return DeserializeRecords<T>(records);
        }
        ReportConfigError(nameof(DataHelper), nameof(ReadFilteredEntries), "Invalid configuration type provided.");
        throw new Exception("Unsupported configuration type");
    }


    public static async Task<IEnumerable<T>> ReadEntries<T>(IDataSourceConfig? config, string tableName, long id, string keyfield = "SurveyID")
    {
        if (string.IsNullOrEmpty(tableName) || config is null)
        {
            ReportConfigError(nameof(DataHelper), nameof(ReadEntries), "No table name or configuration type provided.");
        }

        List<DataRecord> records = new();
        if (config is MySqlConfig mySqlConfig)
        {
            records = await MySqlHelperUtils.ReadTable(config, tableName,
                $"{keyfield} = {id}");
            return DeserializeRecords<T>(records);
        }
        ReportConfigError(nameof(DataHelper), nameof(ReadEntries), "Invalid configuration type provided.");
        throw new Exception("Unsupported configuration type");
    }


    internal static async Task<IEnumerable<T>> ReadEntries<T>(IDataSourceConfig? config, string tableName, string sqlSelection = "SurveyID")
    {
        if (string.IsNullOrEmpty(tableName) || config is null)
        {
            ReportConfigError(nameof(DataHelper), nameof(ReadEntries), "No table name or configuration type provided.");
        }

        List<DataRecord> records = new();
        if (config is MySqlConfig mySqlConfig)
        {
            records = await MySqlHelperUtils.ReadTable(config, tableName,
                $"{sqlSelection}");
            return DeserializeRecords<T>(records);
        }
        ReportConfigError(nameof(DataHelper), nameof(ReadEntries), "Invalid configuration type provided.");
        throw new Exception("Unsupported configuration type");
    }

    public static List<T> DeserializeRecords<T>(List<DataRecord> results)
    {
        if (results == null)
        {
            TraceLogger.LogWarning(nameof(DataHelper), nameof(DeserializeRecords), "No records retrieved");
            return new List<T>();
        }
        try
        {
            List<T> allData = new List<T>();
            // Get records from results
            foreach (var record in results)
            {
                // If caller expects strings, extract the first field value as string and add to list
                if (typeof(T) == typeof(string) && record.Fields != null && record.Fields.Count > 0)
                {
                    // Prefer the value (not KeyValuePair.ToString())
                    var firstValue = record.Fields.Values.FirstOrDefault();
                    string data = firstValue?.ToString() ?? string.Empty;
                    allData.Add((T)(object)data);
                    continue;
                }

                // Deserialize the record manually into my object
                DataHelper.LoadClass<T>(record, out object? myRecord);
                if (myRecord != null)
                {
                    allData.Add((T)myRecord);
                }
            }
            return allData;
        }
        catch (Exception ex)
        {
            TraceLogger.LogError(nameof(DataHelper), nameof(DeserializeRecords),$"Error deserializing records {ex.Message}");
        }
        return new List<T>(); // Return empty list if no records found
    }

    static JsonSerializerOptions jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    public static void LoadClass<T>(DataRecord? record, out object resultClass)
    {
        try
        {
//            record.Fields.Add("recordId", record.Id);
            var json = JsonSerializer.Serialize(record.Fields);
            resultClass = (object)(JsonSerializer.Deserialize<T>(json, jsonOptions));
            if (resultClass is null)
            {
                throw new ArgumentException($"Failed to deserialize JSON into {typeof(T).Name}");
            }
        }
        catch (Exception ex)
        {
            string error = $"Error deserializing record into {typeof(T).Name}: {ex.Message}";
            TraceLogger.LogError(nameof(DataHelper), nameof(LoadClass), error);
            throw new ArgumentException(error, ex);
        }
    }
    public static DataRecord LoadDataRecord(BaseRecord record)
    {
        // We only care about the Fields property of the BaseRecord
        DataRecord? dataRecord = new();
        dataRecord.Fields = new Dictionary<string, object>();
        foreach (var field in record.Fields)
        {
            dataRecord.Fields[field.Key] = field.Value;
        }

        return dataRecord;
    }
    public static void CopyProperties<TSource, TDestination>(TSource source, TDestination destination)
    {
        foreach (PropertyInfo sourceProperty in typeof(TSource).GetProperties())
        {
            PropertyInfo destinationProperty = typeof(TDestination).GetProperty(sourceProperty.Name);

            if (destinationProperty != null && destinationProperty.CanWrite && sourceProperty.CanRead &&
                destinationProperty.PropertyType == sourceProperty.PropertyType)
            {
                destinationProperty.SetValue(destination, sourceProperty.GetValue(source));
            }
        }
    }
}
