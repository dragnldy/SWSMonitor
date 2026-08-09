using DataLibrary.DataSources;
using Models;

namespace DataLibrary.Crud;
public static class QuadratDataCrud
{
    // This class is responsible for reading and writing Quadrat records to and from data sources like Airtable.
    // It uses the AirtableHelper to interact with the Airtable API.
    // Or the MySqlHelper to interact with a MySQL database.
    /// <summary>
    /// Reads all quadrat data from Airtable or MySQL based on the provided configuration.
    /// </summary>
    /// <param name="config">The configuration for connecting to Airtable or MySql.</param>
    /// <returns>A list of QuadratData objects.</returns>
    public static async Task<IEnumerable<QuadratData>> ReadAllQuadratDatax(object config)
    {

        List<QuadratData> allQuadrats = new List<QuadratData>();
        if (config is AirTableConfig airtableConfig)
        {
            List<DataRecord> results = await AirtableHelper.ReadTable(config, QuadratData.TableName);
            return DeserializeRecords(results);
        }
        else if (config is MySqlConfig mySqlConfig)
        {
            List<DataRecord> results = await MySqlHelper.ReadTable(config, QuadratData.TableName);
            return DeserializeRecords(results);
        }
        else throw new ArgumentException("Invalid configuration type provided. Expected AirTableConfig or MySqlConfig.", nameof(config));
    }
    private static List<QuadratData> DeserializeRecords(List<DataRecord> results)
    {
        if (results == null)
        {
            System.Diagnostics.Debug.WriteLine("No records retrieved");
            return GetTestQuadratData(); // Return test data if no records found
        }
        try
        {
            List<QuadratData> allQuadrats = new List<QuadratData>();
            // Get records from results
            foreach (var record in results)
            {

                // Deserialize the record manually into my BeachData object
                DataHelper.LoadClass<QuadratData>(record, out object? myRecord);
                if (myRecord != null)
                {
                    allQuadrats.Add((QuadratData)myRecord);
                }
            }
            return allQuadrats.Count > 0 ? allQuadrats : GetTestQuadratData(); // Return test data if no records found
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading Airtable records: {ex.Message}");
        }
        return GetTestQuadratData(); // Return test data if no records found
    }
    internal static List<QuadratData> GetTestQuadratData()
    {
        return new List<QuadratData>()
        {
            new QuadratData
            {
                ID = 1,
                BeachName = "Test Beach",
                Date = new DateTime(2023, 10, 1),
                Quadrat = "Q1",
                Species = "Test Species",
                SpeciesLinkId = 1234567890,
                Tide = "High",
            }
        };
    }
}
