using DataLibrary.ModelExtensions;
using System.Text.Json.Serialization;

namespace Models;

public class MonitorBase
{
    private string? _encodedmonitor = string.Empty;
    public string? Monitor
    {
        get => string.IsNullOrEmpty(_encodedmonitor) ? string.Empty : _encodedmonitor.Trim(new char[] { LeadFlag, SpeciesExpertFlag });
        //set
        //{
        //    _encodedmonitor = value;
        //    if (Lead == 1)
        //        _encodedmonitor += LeadFlag;
        //    if (SpeciesExpert == 1)
        //        _encodedmonitor += SpeciesExpertFlag;
        //}
    }
    [DBIgnore]
    public int Lead { get => !string.IsNullOrEmpty(_encodedmonitor) &&  _encodedmonitor.Contains(LeadFlag) ? 1 : 0; }

    [JsonIgnore]
    public bool IsLead => Lead == 1;

    public int? SpeciesExpert { get => !string.IsNullOrEmpty(_encodedmonitor) && _encodedmonitor.Contains(SpeciesExpertFlag) ? 1 : 0; }
    [JsonIgnore]
    public bool IsSpeciesExpert => SpeciesExpert == 1;

    public MonitorBase(string? codedmonitor)
    {
        _encodedmonitor = codedmonitor;
    }
    public MonitorBase(string? name, bool isLead, bool isSpeciesExpert)
    {
        _encodedmonitor = EncodeMonitor(name, isLead, isSpeciesExpert);
    }

    public static MonitorBase DecodeMonitor(string codedmonitor)
    {
        return new MonitorBase(codedmonitor);
    }

    public static string EncodeMonitor(string name, bool isLead, bool isSpeciesExpert)
    {
        string monitor = name;
        if (isLead) monitor += LeadFlag;
        if (isSpeciesExpert) monitor += SpeciesExpertFlag;
        return monitor;
    }

    public static string? EncodeMonitorList(IEnumerable<MonitorBase> monitors)
    {
        if (monitors == null || !monitors.Any()) return null;
        return string.Join(';', monitors.Select(m => EncodeMonitor(m.Monitor,m.IsLead, m.IsSpeciesExpert)).OrderBy(n=>n));
    }
    public static IEnumerable<MonitorBase> DecodeMonitorList(string? monitors)
    {
        IEnumerable<MonitorBase> emptyList = new List<MonitorBase>();
        return monitors is not null
            ? monitors.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
             .Select(m => new MonitorBase(m))
            : emptyList;
    }

    public static char LeadFlag = '+';
    public static char SpeciesExpertFlag = '$';
}

/// <summary>
/// Represents a Monitor.
/// NOTE: This class is generated from a T4 template - you should not modify it manually.
/// </summary>
public class SurveyMonitor : MonitorBase
{
    public const string TableName = "MonitorsView";

    public long? SurveyID { get; set; } = 0l;

    public string? BeachName { get; set; } = string.Empty;

    public DateTime? SurveyDate { get; set; } = new DateTime();

    public int MonitorNo { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SurveyMonitor"/> class.
    /// </summary>
    public SurveyMonitor(long? surveyID, string ?beachName, DateTime? surveyDate, int monitorNo, string? monitor) : base(monitor)
    {
        SurveyID = surveyID;
        BeachName = beachName;
        SurveyDate = surveyDate;
        MonitorNo = monitorNo;
    }
}
