using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;

namespace Models;

public partial class BeachData // Inherits from the generated BeachData class
{
    // Additional properties or methods that are computed or hard wired

    [JsonIgnore]
    public double Lat => TryParseGeometry(Latitude, out var lat) ? lat : 0.0;
    [JsonIgnore]
    public double Long => TryParseGeometry(Longitude, out var lon) ? lon : 0.0;
    [JsonIgnore]
    public bool IsMonitored => IsCurrentlyMonitored ?? false;
    [JsonIgnore]
    public string DecodedDnr => GetDecodedDnr(DnrClass);
    [JsonIgnore]
    public string DecodedTideChart => string.IsNullOrEmpty(TideChart) ? "None supplied" : TideChart;

    public static bool TryParseGeometry(string? value, out double result)
    {
        // latitude and longitude are stored as strings in the database, so we need to parse them
        // they are expressed as integer degrees followed by comma (',') and then decimal minutes

        string[] parts = value?.Split(',') ?? Array.Empty<string>();
        if (parts.Length != 2 || !double.TryParse(parts[0], out double degrees) || !double.TryParse(parts[1], out double minutes))
        {
            result = 0.0;
            return false;
        }
        if (degrees < 0)
        {
            result = degrees - Math.Round((minutes / 60.0), 5); // Convert to decimal degrees
        }
        else
        {
            result = degrees + Math.Round((minutes / 60.0), 5); // Convert to decimal degrees  
        }
        return true;
    }
    public static string GetDecodedDnr(int? dnrClass)
    {
        if (dnrClass is null || !dnrClasses.ContainsKey(dnrClass.Value))
            return string.Empty;

        return dnrClasses[dnrClass.Value];
    }

    public static int? GetEncodedDnr(string dnrDescription)
    {
        foreach (var kvp in dnrClasses)
        {
            if (kvp.Value.Equals(dnrDescription, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Key;
            }
        }
        return null; // Not found
    }

    public static string? FormatGeometry(double value)
    {
        // Convert decimal degrees to degrees and decimal minutes
        double absValue = Math.Abs(value);
        int degrees = (int)absValue;
        double minutes = (absValue - degrees) * 60.0;
        string formatted = $"{(value < 0 ? "-" : "")}{degrees},{Math.Round(minutes, 5)}";
        return formatted;
    }

    public static Dictionary<int, string> dnrClasses = new Dictionary<int, string>() {
        { 1, "Estuarine Mixed Coarse Open" }, { 2, "Estuarine Sand Open" }, {3,"Estuarine Mixed Fine and Mud"},
        {4,"Estuarine Mixed Fine Partly Enclosed" }, {5,"N/A" }, {6,"Estuarine Gravel Open"},
        {7,"Estuarine Gravel Open East"}, {8,"N/A" },{9,"N/A" }, {10,"Marine Cobble Partially exposed" },
        {11,"Marine Sand Partially exposed"} };
    public static List<string> bulkheadConstructions = new List<string>()
    {
        "None",
        "Wood",
        "Cement",
        "Rock",
        "Steel",
        "Other",
        "Unknown"
    };
}
