using System;
using System.Text;
using System.Text.Json.Serialization;

namespace Models;

/// <summary>
/// Represents a SpeciesList.
/// NOTE: This class is generated from a T4 template - you should not modify it manually.
/// </summary>
public class Species : ITableBase
{
    public const string TableName = "Species";
    public DateTime? EntryDate { get; set; }

    // from WORMS database, http://www.marinespecies.org
    public int? AphiaID { get; set; }

    public  DateTime? ChangeDate { get; set; }

    public  string? ChangeReason { get; set; }

    public  string? Class { get; set; }

    public  string? CommonNameOrDescription { get; set; }

    public  int? ComplexityRank { get; set; }

    public  string? Family { get; set; }

    public  string? FormerScientificName { get; set; }

    public  string? Genus { get; set; }

    public required int ID { get; set; }

    public int? Invasive { get; set; }
    [JsonIgnore]
    public  bool IsInvasive { get => Invasive.HasValue && Invasive.Value == 1;
        set => Invasive = value ? 1 : 0;
    }

    public  string? Kingdom { get; set; }

    public int? NonNative {get; set; }

    [JsonIgnore]
    public  bool IsNonNative { get=> NonNative.HasValue && NonNative.Value == 1;
        set => NonNative = value ? 1 : 0;
    }

    public  string? Order { get; set; }

    public  string? Phylum { get; set; }

    public int? ProfileData { get; set; }

    [JsonIgnore]
    public bool UseForProfileData { get => ProfileData.HasValue && ProfileData.Value == 1; set => ProfileData = value ? 1 : 0; }
    
    public int? UsedBySurveys { get; set; }

    [JsonIgnore]
    public bool IsUsedBySurveys { get => UsedBySurveys.HasValue && UsedBySurveys.Value == 1; set => UsedBySurveys = value ? 1 : 0; }

    public  string ScientificName { get; set; }

    public  string? Subphylum { get; set; }

    public  string? TaxonCommonName { get; set; }

    // from ITIS database, http://www.itis.gov/
    public  int? TSN { get; set; }

    [JsonIgnore]
    public string ? FullTaxonomy
    {
        get
        {
            StringBuilder sb = new();
            if (!string.IsNullOrEmpty(Genus) && !Genus.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                sb.Append($"{Genus}");
            else
                sb.Append("_");
            if (!string.IsNullOrEmpty(Family) && !Family.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                sb.Append($"<{Family}");
            else
                sb.Append($"<_");
            if (!string.IsNullOrEmpty(Order) && !Order.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                sb.Append($"<{Order}");
            else
                sb.Append($"<_");
            if (!string.IsNullOrEmpty(Class) && !Class.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                sb.Append($"<{Class}");
            else
                sb.Append($"<_");
            if (!string.IsNullOrEmpty(Subphylum) && !Subphylum.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                sb.Append($"<{Subphylum}");
            else
                sb.Append($"<_");
            if (!string.IsNullOrEmpty(Phylum) && !Phylum.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                sb.Append($"<{Phylum}");
            else
                sb.Append($"<_");
            if (!string.IsNullOrEmpty(Kingdom) && !Kingdom.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                sb.Append($"<{Kingdom}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Species"/> class.
    /// </summary>
    public Species()
    {
        // Initialize properties if needed
    }
}
