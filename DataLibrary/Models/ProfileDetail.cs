using DataLibrary;
using System;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Models;

/// <summary>
/// Represents a ProfileDetails.
/// NOTE: This is a virtual record that is decoded from the ProfileEntries. It is not stored in the database as a separate table, but is derived from the ProfileEntries table. 
/// It is used to represent the details of a profile entry in a more structured way
/// </summary>
public class ProfileDetail
{
    public int? EntryNo { get; set; }

    public string? Species { get; set; }

    public string? Notes { get; set; }

    /// Initializes a new instance of the <see cref="ProfileDetail"/> class.
    /// </summary>
    public ProfileDetail()
    {
        // Initialize properties if needed
    }

    private static Regex  regexNotes = new Regex(@"^(?<species>.*?)(?:\s*[{](?<notes>[^}]+)[}])?$");
    public ProfileDetail(string encodedDetail, int? entryno = null)
    {
        EntryNo = entryno;
        if (string.IsNullOrEmpty(encodedDetail))
        {
            Species = string.Empty;
            Notes = string.Empty;
        }
        else
        {
            try
            {
                if (regexNotes.IsMatch(encodedDetail))
                {
                    var match = regexNotes.Match(encodedDetail);
                    Species = match.Groups.ContainsKey("species") ? match.Groups["species"].Value.Trim() : string.Empty;
                    Notes = match.Groups.ContainsKey("notes") ? match.Groups["notes"].Value : string.Empty;
                }
            } 
            catch (Exception ex)
            { 
                TraceLogger.LogWarningAuto($"Failed to parse ProfileDetail from encoded string: {encodedDetail}. Exception: {ex.Message}");
                Species = string.Empty; 
            }
        }
    }
    public static string? EncodeProfileDetails(IEnumerable<ProfileDetail> sds)
    {
        if (!sds.Any()) return null;

        List<string> details = new List<string>();
        foreach (var sd in sds.OrderBy(n => n.Species))
        {
            details.Add(EncodeProfileDetail(sd));
        }
        return string.Join(";", details);
    }

    public static string EncodeProfileDetail(ProfileDetail sd)
    {
        return EncodeProfileDetail(sd.Species, sd.Notes);
    }
    public static string EncodeProfileDetail(string species, string notes)
    {
        string profile = !string.IsNullOrEmpty(species) ? species.Trim() : string.Empty;
        if (!string.IsNullOrEmpty(notes))
            profile += $" {{{notes}}}";
        return profile;
    }
}
