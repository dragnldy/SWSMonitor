using DataLibrary;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Models;


/// <summary>
/// Represents a QuadratDetail.
/// NOTE: This class is not persisted in the database- it is part of the QuadEntry object
/// </summary>
public class QuadratDetail
{
    public int QuadratNo { get; set; } = 0;

    public string Species { get; set; } = string.Empty;

    public int? SpeciesLinkId { get; set; }

    public string? QuadratNotes { get; set; } = string.Empty;

    public string? QANotes { get; set; } = string.Empty; // This is no longer used

    public short? ActualNumber { get; set; } = 0;

    public Single? PercentObserved { get; set; } = 0.0f;  

    public int? Dense { get; set; } = 0;

    [JsonIgnore]
    public bool IsDense
    {
        get => Dense.HasValue && Dense.Value > 0;
        set { Dense = value ? 1 : 0; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuadratDetail"/> class.
    /// </summary>
    public QuadratDetail()
    {
        // Initialize properties if needed
    }
    public QuadratDetail(int quadratno, string species, short? actualnumber, Single? percentobserve, int? dense, string? quadratNotes, string? qaNotes)
    {
        QuadratNo = quadratno;
        Species = species;
        ActualNumber = actualnumber;
        PercentObserved = percentobserve;
        Dense = dense;
        QuadratNotes = quadratNotes;
        // QANotes = qaNotes;
        SpeciesLinkId = StaticData.Species.FirstOrDefault(n => n.ScientificName.Equals(species))?.ID ?? 0;
    }

    public static Regex regexQuadrat = new Regex(@"^(?<species>.+?)(?<dense>\+)?\s*(?:#(?<actualnumber>\d+))?\s*(?:%(?<percentageobserved>[\d.]+))?\s*(?:\{(?<notes>[^}]+)\})?\s*(?:\{QA:\s*(?<qanotes>[^}]+)\})?\s*$");

    public QuadratDetail(string encodedDetail, int quadratno)
    {
        QuadratNo = quadratno;

        if (!string.IsNullOrEmpty(encodedDetail))
        {
            if (regexQuadrat.IsMatch(encodedDetail))
            {
                var match = regexQuadrat.Match(encodedDetail);
                Species = match.Groups.ContainsKey("species") ? match.Groups["species"].Value.TrimEnd() : string.Empty;
                if (match.Groups.ContainsKey("dense") && !string.IsNullOrEmpty(match.Groups["dense"].Value))
                    Dense = 1;

                // Handle notes - check if it's a QA note or regular note
                if (match.Groups.ContainsKey("notes") && !string.IsNullOrEmpty(match.Groups["notes"].Value))
                {
                    var noteValue = match.Groups["notes"].Value;
                    if (noteValue.StartsWith("QA:", StringComparison.OrdinalIgnoreCase))
                    {
                        // Ignore this- no longer in detail 
                        TraceLogger.LogWarningAuto("QA Note detected");
                        // If no separate QA notes group, this is the QA note
                        if (!match.Groups.ContainsKey("qanotes") || string.IsNullOrEmpty(match.Groups["qanotes"].Value))
                        {
                            // QANotes = noteValue.Substring(3).Trim();
                        }
                        else
                        {
                            // This shouldn't happen with the regex, but handle it
                            QuadratNotes = noteValue;
                        }
                    }
                    else
                    {
                        QuadratNotes = noteValue;
                    }
                }

                if (match.Groups.ContainsKey("qanotes") && !string.IsNullOrEmpty(match.Groups["qanotes"].Value))
                {
                    // Ignore this- no longer in detail 
                    TraceLogger.LogWarningAuto("QA Note detected");

                    // QANotes = match.Groups["qanotes"].Value.Trim();
                }
                if (match.Groups.ContainsKey("actualnumber") && short.TryParse(match.Groups["actualnumber"].Value, out short actualNum))
                    ActualNumber = actualNum;
                if (match.Groups.ContainsKey("percentageobserved") && Single.TryParse(match.Groups["percentageobserved"].Value, out Single percentObs))
                    PercentObserved = percentObs;
                SpeciesLinkId = StaticData.Species.FirstOrDefault(n => n.ScientificName.Equals(Species))?.ID ?? 0;
            }
        }
    }
    public static string? EncodeQuadratDetails(IEnumerable<QuadratDetail>? qds)
    {
        if (qds is null || !qds.Any())
            return null;

        List<string?> details = new List<string>();
        foreach (var qd in qds.OrderBy(n => n.Species))
        {
            details.Add(EncodeQuadratDetail(qd));
        }
        return string.Join(";", details);
    }

    public static string? EncodeQuadratDetail(QuadratDetail qd)
    {
        if (qd is null) return null;
        return EncodeQuadratDetail(qd.Species, qd.Dense, qd.ActualNumber, qd.PercentObserved, qd.QuadratNotes);
    }
    public static string EncodeQuadratDetail(string species, int? dense, short? actualNumber, float? percentObserved, string quadratnotes)
    {
        string quadrats = !string.IsNullOrEmpty(species) ? species : string.Empty;
        if ((dense ?? 0) > 0)
            quadrats += "+";
        if ((actualNumber ?? 0) > 0)
            quadrats += $" #{actualNumber}";
        if ((percentObserved ?? 0.0) > 0.0)
            quadrats += $" %{percentObserved}";
        if (!string.IsNullOrEmpty(quadratnotes))
            quadrats += $" {{{quadratnotes}}}";
        return quadrats;
    }



}