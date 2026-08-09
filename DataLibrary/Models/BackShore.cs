namespace Models;

/// <summary>
/// Represents a BackShore.
/// NOTE: is a READ-only class as it is based on a view that flattens out the concatenated list in BeachEvent. We can view it but not affect it directly.
/// </summary>
public class BackShoreContent
{
    public const string TableName = "BackShoreView";

    public long BackShoreNo { get; set; } = 0l;
    public string BackShoreContents { get; set; } = string.Empty;

    public BackShoreContent()
    {
        // We need an empty constructor for deserialization and for the Decode method to work properly.
    }
    public BackShoreContent(long backShoreNo, string backShoreContents)
    {
        BackShoreNo = backShoreNo;
        BackShoreContents = backShoreContents;
    }
}
/// <summary>
/// Initializes a new instance of the <see cref="BackShoreContent"/> class.
/// 
public class BackShore : BackShoreContent
{
    public static List<string> TypicalContents = new List<string>
    {
        "Grasses",
        "Trees",
        "Shrubs",
        "Driftwood",
        "Rock",
    };
    // Backshore is not currently a table- it is a view that flattens out the concatenated list in BeachEvent. So we can view it but not affect it directly.
    public long SurveyID { get; set; } = 0l;
    public string BeachName { get; set; } = string.Empty; 
    public DateTime SurveyDate { get; set; } = new DateTime();
    /// <summary>
    /// Initializes a new instance of the <see cref="BackShore"/> class.
    /// </summary>
    public BackShore()
    {
    }

    public static string? EncodeBackshoreList(IEnumerable<BackShoreContent> backshorecontents)
    {
        if (backshorecontents == null || !backshorecontents.Any()) return null;
        return string.Join(';', backshorecontents.ToList().OrderBy(b =>b.BackShoreContents).Select(b => b.BackShoreContents));
    }
    public static IEnumerable<BackShoreContent> DecodeBackshoreList(string backshorecontents)
    {
        int index = 0;
        return !string.IsNullOrEmpty(backshorecontents)
            ? backshorecontents.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(sd => new BackShoreContent(++index, sd))
            : new List<BackShoreContent>();
    }
}
    