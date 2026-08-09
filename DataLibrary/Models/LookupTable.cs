using Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Models;

public class LookupTable : ITableBase
{
    public const string TableName = "LookupTables";
    public DateTime? EntryDate { get; set; }

    public int ID { get; set; } = 0;
    public string? LookupCategory { get; set; } = "";
    public string? LookupValue { get; set; } = "";
    public string? LookupCode { get; set; }
    public string? LookupExtra { get; set; }

    // For species related lookups this is the coded taxonomy genus===>kingdom
    [JsonIgnore]
    public string? Taxonomy { get => LookupExtra; set => LookupExtra = value; }
}
