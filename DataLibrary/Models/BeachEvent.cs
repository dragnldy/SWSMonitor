using DataLibrary;
using DataLibrary.ModelExtensions;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Models;

public partial class BeachEvent : BeachEventBase
{
    public new const string TableName = "BeachEvent";

    [DBIgnore]
    public string BeachName { get; set; } = string.Empty;
    [DBIgnore]
    public DateTime SurveyDate { get; set; }

    [DBIgnore]
    public List<BackShoreContent> BackShoreList
    {
        get => BackShore.DecodeBackshoreList(BackshoreContents).ToList();
    }

    [DBIgnore]
    public List<MonitorBase> MonitorList
    {
        get => SurveyMonitor.DecodeMonitorList(Monitors).ToList();
    }
    [DBIgnore]
    public List<SpeciesListBase> SpeciesList
    {
        get => SpeciesListBase.DecodeSpeciesList(SpeciesObserved).ToList();
    }

    private string LookupCommonName(int id)
    {
        // The API service will probably not cache the species data, so we need to check if it's loaded before trying to access it. 
        if (StaticData.Species is null || !StaticData.Species.Any())
            return string.Empty;
        Species? glossary = StaticData.Species!.FirstOrDefault(s => s.ID == id);
        return glossary != null ? glossary.CommonNameOrDescription : string.Empty;
    }

    [JsonIgnore]
    public bool HasBullKelpBeds
    {
        get => BullKelpBeds.HasValue && BullKelpBeds.Value > 0;
        set { BullKelpBeds = value ? 1 : 0; }
    }

    [JsonIgnore]
    public bool HasBivalveDig
    {
        get => BivalveDig.HasValue && BivalveDig.Value > 0;
        set { BivalveDig = value ? 1 : 0; }
    }

    [JsonIgnore]
    public bool WerePhotosTaken
    {
        get => PhotosTaken.HasValue && PhotosTaken.Value > 0;
        set { PhotosTaken = value ? 1 : 0; }
    }

    [JsonIgnore]
    public bool WasRedTide
    {
        get => RedTide.HasValue && RedTide.Value > 0;
        set { RedTide = value ? 1 : 0; }
    }

    [JsonIgnore]
    public bool WasSpeciesListGenerated
    {
        get => SpeciesListGenerated.HasValue && SpeciesListGenerated.Value > 0;
        set { SpeciesListGenerated = value ? 1 : 0; }
    }

    public BeachEvent(long id, long surveyid, string beachName, DateTime surveyDate) : base(id, surveyid)
    {
        BeachName = beachName;
        SurveyDate = surveyDate;
    }
}
/// <summary>
/// Represents a BeachEvent.
/// NOTE: This class is generated from a T4 template - you should not modify it manually.
/// </summary>
public partial class BeachEventBase : ITableBase
{
    public const string TableName = "BeachEvent";

    public long ID { get; set; }

    public long SurveyID { get; set; }

    public DateTime? EntryDate { get; set; }


    public string? Monitors { get; set; } = string.Empty;

    public string? SpeciesObserved { get; set; } = string.Empty;

    public Single? AirTemp { get; set; }

    public string? BackshoreContents { get; set; } = null;

    public string? BackshoreEnvironment { get; set; }

    public string? BackshoreVegetation { get; set; }

    public Single? BarometricPressure { get; set; }

    public string? BeachProfileNotes { get; set; }

    public int? BivalveDig { get; set; }

    public string? Bluff { get; set; }

    public string? Bulkhead { get; set; }

    public string? BulkheadCondition { get; set; }

    public int? BullKelpBeds { get; set; }


    public  string? CloudCover { get; set; }

    public  double? CorrectedTideHeight { get; set; }

    public  string? ErosionSinceLast { get; set; }

    public int? PhotosTaken { get; set; }

    public  string? Pictures { get; set; }

    public  string? Precipitation { get; set; }

    public string? QuadratNotes { get; set; }

    public int? RedTide { get; set; }

    public  int? Salinity { get; set; }

    public  string? Seagrasspercent { get; set; }

    public  int? SeagrassBedsAtLowerTideLevels { get; set; }

    public  string? Seaweedpercent { get; set; }

    public  int? SeaweedsAtLowerTideLevels { get; set; }

    public  int? Spartina { get; set; }

    public int? SpeciesListGenerated { get; set; }

    public  Single? TideHeightAtEnd { get; set; }

    public  string? Ulvapercent { get; set; }

    public  int? UlvaALowerTideLevels { get; set; }

    public  double? VerticalHeight { get; set; }

    public  Single? WaterTemp { get; set; }

    public  int? Weather { get; set; }

    public  string? Wind { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BeachEvent"/> class.
    /// </summary>
    public BeachEventBase(long id, long surveyid)
    {
        ID = id;
        SurveyID = surveyid;
    }
}
