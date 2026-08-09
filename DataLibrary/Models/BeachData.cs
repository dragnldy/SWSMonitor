using System;
using System.Text.Json.Serialization;

namespace Models;

    /// <summary>
    /// Represents a BeachData.
    /// NOTE: This class is generated from a T4 template - you should not modify it manually.
    /// </summary>
public partial class BeachData : ITableBase
{
    public const string TableName = "BeachData";

    public int ID { get; set; } = 0;

    public string? AdditionalNotes { get; set; }

    public  string? BeachDirections { get; set; }

    public string BeachName { get; set; } = "";

    public int? Bulkhead { get; set; }

    [JsonIgnore]
    public bool? HasBulkhead => Bulkhead.HasValue && Bulkhead.Value > 0;

    public string? BulkHeadConstruction { get; set; }

    public  int? County { get; set; }

    public int? CurrentlyMonitored { get; set; }

    [JsonIgnore]
    public bool? IsCurrentlyMonitored => CurrentlyMonitored.HasValue && CurrentlyMonitored.Value > 0;

    public  int? DnrClass { get; set; }

    public  DateTime? EntryDate { get; set; }

    public  string? Island { get; set; }

    public  string? Latitude { get; set; }

    public  string? Longitude { get; set; }

    public  string? ProfileDirections { get; set; }

    public  decimal? ProfileLineStart { get; set; }

    public  int? SurveyWidth { get; set; }

    public  string? TideChart { get; set; }

    public  string? VertRef { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BeachData"/> class.
    /// </summary>
    public BeachData()
    {
        // Initialize properties if needed
    }
}
