using System;

namespace Models;

/// <summary>
/// Represents a Synchronizations.
/// NOTE: This class is generated from a T4 template - you should not modify it manually.
/// </summary>
public class Synchronizations : ITableBase
{
    public const string TableName = "Synchronizations";
    // Airtable attaches an internal id to each record in a table
    // We need this for updating/deleting records though not required for reading
    public string AirTableId { get; set; } = "";
    public DateTime? EntryDate { get; set; }

    public required long Id { get; set; }

    public required string Source { get; set; }

    public required string Destination { get; set; }

    public  string? DestinationId { get; set; }

    public required DateTime MinDate { get; set; }

    public required DateTime MaxDate { get; set; }

    public required DateTime SyncDate { get; set; }

    public  ulong? RecordCount { get; set; }

    public required string Status { get; set; }

    public  string? Uploader { get; set; }

    public  string? Organization { get; set; }

    public  string? Subbasin { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Synchronizations"/> class.
    /// </summary>
    public Synchronizations()
    {
        // Initialize properties if needed
    }
}
