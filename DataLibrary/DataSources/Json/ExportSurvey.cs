using DataLibrary.ModelExtensions;
using Microsoft.VisualBasic;
using Models;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace DataLibrary;

public static class ExportSurvey
{
    public static JsonSerializerOptions
        SerializerOptions = new JsonSerializerOptions { WriteIndented = true };

    public static string ExportSurveyAsJson(Survey source, string destinationFolder = "")
    {
        if (string.IsNullOrEmpty(destinationFolder))
            destinationFolder = Path.GetTempPath();
        string fileName = GetFileName(source, destinationFolder);
        if (source is null || string.IsNullOrEmpty(fileName)) return String.Empty;
        try
        {
            string surveyasJson = JsonSerializer.Serialize(source,SerializerOptions);
            if (File.Exists(fileName))
                File.Delete(fileName);
            if (!SaveJasonToCache(fileName, surveyasJson.ToString())) return String.Empty;
        }
        catch (Exception exc)
        {
            System.Diagnostics.Debug.WriteLine(exc);
            return string.Empty;
        }
        return fileName;
    }

    private static string GetFileName(SurveyBase source, string destinationFolder)
    {
        string fileName = $"{source.BeachName}-{source.SurveyDate:yyyy-MM-dd}.json";
        return Path.Combine(destinationFolder, fileName);
    }

    private static bool SaveJasonToCache(string fileName, string? jsontext)
    {
        // get the temporary file directory
        try
        {
            File.WriteAllText(fileName, jsontext ?? string.Empty);
            return true;
        }
        catch (Exception exc)
        {
            System.Diagnostics.Debug.WriteLine(exc);
            return false;
        }
    }
    public static Survey? ImportSurveyFromJson(SurveyBase surveyTemplate, string destinationFolder = "")
    {
        if (string.IsNullOrEmpty(destinationFolder))
            destinationFolder = Path.GetTempPath();

        string fileName = GetFileName(surveyTemplate, destinationFolder);
        try
        {
            if (File.Exists(fileName))
            {
                string text = File.ReadAllText(fileName);
                Survey? importedSurvey = JsonSerializer.Deserialize<Survey>(text);
                return importedSurvey ?? null;
            }
            return null;
        }
        catch (Exception exc)
        {
            System.Diagnostics.Debug.WriteLine(exc);
            return null;
        }
    }

    private static int JulianDay(DateTime date)
    {
        // Convert the DateTime to a Julian Day Number.- we won't do all the fancy leap year stuff
        // This calculation is based on the algorithm from Fliegel and Van Flandern (1968).
        // It's a widely accepted formula for calculating Julian Day Numbers.

        int year = date.Year;
        int month = date.Month;
        int day = date.Day;

        // Adjust month and year for January and February
        if (month <= 2)
        {
            year -= 1;
            month += 12;
        }

        // Calculate A
        double A = Math.Floor((double)year / 100);

        // Calculate B
        double B = 2 - A + Math.Floor(A / 4);

        // Calculate Julian Day Number (JDN) for midnight of the given date
        double JDN = Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1)) + day + B - 1524.5;
        return (int)Math.Round(JDN, 0);
    }

    public static bool ExportToCache(Survey loadedSurvey)
    {
        if (loadedSurvey is null || string.IsNullOrEmpty(loadedSurvey.BeachName)) return false;
        string file = ExportSurveyAsJson(loadedSurvey);
        return !string.IsNullOrEmpty(file);
    }

    public static byte[] CompressString(string str)
    {
        byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(str);
        using (var outputStream = new MemoryStream())
        {
            using (var gzipStream = new System.IO.Compression.GZipStream(outputStream, System.IO.Compression.CompressionMode.Compress))
            {
                gzipStream.Write(inputBytes, 0, inputBytes.Length);
            }
            return outputStream.ToArray();
        }
    }
    public static string DecompressString(byte[] inputBytes)
    {
        try
        {
            using (MemoryStream compressedStream = new MemoryStream(inputBytes))
            {
                using (GZipStream decompressionStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                {
                    using (MemoryStream decompressedStream = new MemoryStream())
                    {
                        decompressionStream.CopyTo(decompressedStream);
                        byte[] decompressedData = decompressedStream.ToArray();
                        return Encoding.UTF8.GetString(decompressedData); // Or another appropriate encoding
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Decompression error: {ex.Message}");
        }
        return string.Empty;
    }
}
