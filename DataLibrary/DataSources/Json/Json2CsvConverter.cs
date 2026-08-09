using CsvHelper;
using CsvHelper.Configuration;
using DataLibrary.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace DataLibrary.DataSources.Json;

public static class Json2CsvConverter
{
    private const char DELIMITER = ',';

    public static string ConvertViewRecord2CsvString(ViewRecord viewRecords)
    {
        if (viewRecords is null || !viewRecords.Records.Any())
        {
            TraceLogger.LogWarningAuto($"No records found in the view {viewRecords?.ViewName ?? "UnknownView"} to convert to CSV.");
        }
        // Get the fields from the first record to use as headers (assuming all records have the same structure)
        List<string> headers = viewRecords.Records.First().Fields.Keys.ToList();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = DELIMITER.ToString()
        };

        // Write the CSV to a string
        using (var writer = new StringWriter())
        using (var csv = new CsvWriter(writer, config))
        {
            // Write the Header Row
            foreach (var header in headers)
            {
                csv.WriteField(header);
            }
            csv.NextRecord();


            foreach (var record in viewRecords.Records)
            {
                try
                {
                    foreach (var header in headers)
                    {
                        // Write value or empty string if key doesn't exist for this record
                        if (record.Fields.ContainsKey(header))
                        {
                            if (header.ToLower().Equals("surveydate") || header.ToLower().Equals("entrydate"))
                            {
                                if (record.Fields[header] == null)
                                {
                                    csv.WriteField(string.Empty);
                                    continue;
                                }
                                var datestring = record.Fields[header];
                                if (datestring is not null)
                                {
                                    string[] dateparts = datestring.ToString().Split(' ');
                                    DateOnly date = DateOnly.Parse(dateparts[0]);
                                    csv.WriteField(date.ToString());
                                    continue;
                                }
                                else
                                {
                                    csv.WriteField(string.Empty);
                                    continue;
                                }

                            }
                            csv.WriteField(record.Fields[header] ?? string.Empty);
                        }
                        else
                        {
                            csv.WriteField(string.Empty);
                        }
                    }
                    csv.NextRecord();
                }
                catch (Exception ex)
                {
                    TraceLogger.LogErrorAuto($"Error converting ViewRecord to CSV string: {ex.Message}");
                    return string.Empty;
                }

            }
            csv.Flush();
            return writer.ToString();
        }
    }
    public static bool ConvertJson2CsvFile(string jsonString, string csvFilePath)
    {
        try
        {
            // Parse JSON into an array of objects
            var jsonArray = JArray.Parse(jsonString);

            // Extract and flatten headers dynamically
            var headers = GetDistinctHeaders(jsonArray);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = DELIMITER.ToString()
            };


            // Write the CSV file
            using (var writer = new StreamWriter(csvFilePath))
            using (var csv = new CsvWriter(writer, config))
            {
                // Write the Header Row
                foreach (var header in headers)
                {
                    csv.WriteField(header);
                }
                csv.NextRecord();

                // Write Data Rows
                foreach (JObject rowObject in jsonArray)
                {
                    var flatRow = FlattenJObject(rowObject);
                    foreach (var header in headers)
                    {
                        // Write value or empty string if key doesn't exist for this record
                        flatRow.TryGetValue(header, out var value);
                        csv.WriteField(value ?? string.Empty);
                    }
                    csv.NextRecord();
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto($"Error converting JSON to CSV: {ex.Message}");
            return false;
        }
    }

    public static string ConvertJson2CsvString(string jsonString)
    {
        try
        {
            // Parse JSON into an array of objects
            var jsonArray = JArray.Parse(jsonString);

            // Extract and flatten headers dynamically
            var headers = GetDistinctHeaders(jsonArray);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = DELIMITER.ToString()
            };

            // Write the CSV to a string
            using (var writer = new StringWriter())
            using (var csv = new CsvWriter(writer, config))
            {
                // Write the Header Row
                foreach (var header in headers)
                {
                    csv.WriteField(header);
                }
                csv.NextRecord();

                // Write Data Rows
                foreach (JObject rowObject in jsonArray)
                {
                    var flatRow = FlattenJObject(rowObject);
                    foreach (var header in headers)
                    {
                        // Write value or empty string if key doesn't exist for this record
                        flatRow.TryGetValue(header, out var value);
                        csv.WriteField(value ?? string.Empty);
                    }
                    csv.NextRecord();
                }

                csv.Flush();
                return writer.ToString();
            }
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto($"Error converting JSON to CSV string: {ex.Message}");
            return string.Empty;
        }
    }

    private static List<string> GetDistinctHeaders(JArray jsonArray)
    {
        var headers = new HashSet<string>();
        foreach (JObject obj in jsonArray)
        {
            var flatObj = FlattenJObject(obj);
            foreach (var key in flatObj.Keys)
            {
                headers.Add(key);
            }
        }
        return headers.ToList();
    }

    private static Dictionary<string, string> FlattenJObject(JObject obj, string prefix = "")
    {
        var flatDict = new Dictionary<string, string>();
        foreach (var prop in obj.Properties())
        {
            string key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";

            if (prop.Value is JObject nestedObj)
            {
                // Recursively flatten nested JSON objects using dot notation (e.g., parent.child)
                var nestedDict = FlattenJObject(nestedObj, key);
                foreach (var kvp in nestedDict)
                {
                    flatDict[kvp.Key] = kvp.Value;
                }
            }
            else if (prop.Value is JArray array && array.All(v => v is JValue))
            {
                // Handle simple arrays (e.g., joining them as comma-separated values)
                flatDict[key] = string.Join(DELIMITER, array.Select(v => v.ToString()));
            }
            else
            {
                // Handle primitive values
                flatDict[key] = prop.Value.ToString();
            }
        }
        return flatDict;
    }
}
