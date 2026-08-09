using DataLibrary.Crud;
using System.Text.Json.Serialization;

namespace Models;

public enum ComponentsToSaveEnum
{
    Base,
    BeachEvent,  // changes to backshore, conditions, beach settings, etc
    Monitors,
    Backshore,
    Profile,
    Quadrat,
    SpeciesList,
    Species,
    Volunteers,
    All =99
}
public class SurveyBase : ITableBase
{
    /// <summary>
    /// Header class/record for a beach survey
    /// </summary>
    public const string TableName = "Surveys";
    public DateTime? EntryDate { get; set; }

    public long ID { get; set; } // Primary/Reference ID
    public string BeachName { get; set; } // Name of the beach being surveyed

    public string StartTime { get; set; } = ""; // store as string
    public string EndTime { get; set; } = "";
    public DateTime SurveyDate {get; set; } // Date of the survey (no time)

    [JsonIgnore]
    public DateTime? StartTimeAsDate { get; set; }  // Time survey started
    [JsonIgnore]
    public DateTime? EndTimeAsDate { get; set; } // Time survey ended
    public int Tide1Ht { get; set; } // Tide for first set of quadrats- traditionally +1 ft
    public int Tide2Ht { get; set; } // Traditionally 0 ft
    public int Tide3Ht { get; set; } // Traditionally -1 ft
    [JsonIgnore]
    public int Exported2UW { get; set; } // Flag if survey has been exported to UW (0=No, 1=Yes)


    [JsonIgnore]
    public HashSet<ComponentsToSaveEnum> SaveRequired { get; set; } = new(); // Flag if different parts of survey need to be saved  
    [JsonIgnore]
    public bool Completed { get; set; } // Flag if survey data entry is completed
    [JsonIgnore]
    public bool Archived { get; set; } // Flag when survey data is archived and removed from main tables

    internal static void CopyProps(SurveyBase source, SurveyBase destination)
    {
        // Copy all the common properties found in SurveyBase
        DataHelper.CopyProperties<SurveyBase, SurveyBase>(source, destination);
    }
}
