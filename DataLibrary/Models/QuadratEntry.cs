using DataLibrary.ModelExtensions;
using DataLibrary.Models;
using System.Text.Json.Serialization;

namespace Models;

public class QuadratBase
{
    public const string TableName = "QuadratEntries";

    public long ID { get; set; } = 0;
    public long SurveyID { get; set; }
    public int? QuadratID { get; set; }  // 11,12,13,21,22,23,31,32,33

    public string? Tide { get; set; } // ('T1, + 1 Ft', 'T0, 0 Ft', 'T-1, -1 Ft')
    public string? Quadrat { get; set; } // ('Q1', 'Q2', 'Q3')

    public DateTime? EntryDate { get; set; }

    public string? QuadratDetails { get; set; } = null;

    public QuadratBase()
    {
        // Initialize properties if needed
    }

}

public enum TideTypeEnum
{
    Tp1_Q1 = 11,
    Tp1_Q2 = 12,
    Tp1_Q3 = 13,
    Tp1_Q4 = 14,
    T0_Q1 = 21,
    T0_Q2 = 22,
    T0_Q3 = 23,
    T0_Q4 = 24,
    Tn1_Q1 = 31,
    Tn1_Q2 = 32,
    Tn1_Q3 = 33,
    Tn1_Q4 = 34
}
/// <summary>
/// Represents a QuadratEntry - a unique combination of SurveyID, Tide and Quadrat
/// Usually three tides (+1, 0, -1) per survey, and 3 replicate quadrats (Q1,Q2,Q3) per tide.
/// NOTE: This class is generated from a T4 template - you should not modify it manually.
/// </summary>
public class QuadratEntry : QuadratBase
{
    [JsonIgnore, DBIgnore]
    public IEnumerable<QuadratDetail> QuadratDetailList
    {
        get
        {
            return DecodeQuadratDetailList(QuadratDetails);
            //int index = 0;
            //return QuadratDetails is not null
            //    ? QuadratDetails.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            //        .Select(sd => new QuadratDetail(sd, ++index))
            //    : new List<QuadratDetail>();
        }
    }
    /// </summary>
    public QuadratEntry()
    {
        // Initialize properties if needed
    }
    public QuadratEntry(QuadratBase quadratAsBase)
    {
        ID = quadratAsBase.ID;
        SurveyID = quadratAsBase.SurveyID;
        QuadratID = quadratAsBase.QuadratID;
        Tide = quadratAsBase.Tide;
        Quadrat = quadratAsBase.Quadrat;
        EntryDate = quadratAsBase.EntryDate;
        QuadratDetails = quadratAsBase.QuadratDetails;
    }

    public static IEnumerable<QuadratDetail> DecodeQuadratDetailList(string? codedList)
    {
        int index = 0;
        return !string.IsNullOrEmpty(codedList)
            ? codedList.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(sd => new QuadratDetail(sd, ++index))
            : new List<QuadratDetail>();
    }
}
