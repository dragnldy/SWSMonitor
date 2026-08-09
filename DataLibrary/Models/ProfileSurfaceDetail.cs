using DataLibrary;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Models;

/// <summary>
/// Represents a ProfileSurfaceDetails.
/// NOTE: This is a virtual record that is decoded from the ProfileEntries. It is not stored in the database as a separate table, but is derived from the ProfileEntries table. 
/// It is used to represent the details of a profile entry in a more structured way
/// </summary>
public class ProfileSurfaceDetail
{
    public int? EntryNo { get; set; }

    public string BeachSurface { get; set; } = string.Empty;

    public int? G70Percent { get; set; }
    [JsonIgnore]
    public bool IsG70percent
    {
        get => G70Percent.HasValue && G70Percent.Value > 0;
        set { G70Percent = value ? 1 : 0; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileSurfaceDetail"/> class.
    /// </summary>
    public ProfileSurfaceDetail()
    {
        // Initialize properties if needed
    }
    private static Regex regexNotes = new Regex(@"^(?<surface>.*?)(?<dense>[+])$");
    public ProfileSurfaceDetail(string encodedSurface, int? entryno = null)
    {
        EntryNo = entryno;
        if (string.IsNullOrEmpty(encodedSurface))
        {
            BeachSurface = string.Empty;
            IsG70percent = false;
        }
        else
        {
            try
            {
                if (regexNotes.IsMatch(encodedSurface))
                {
                    var match = regexNotes.Match(encodedSurface);
                    BeachSurface = match.Groups.ContainsKey("surface") ? match.Groups["surface"].Value : string.Empty;
                    IsG70percent = match.Groups.ContainsKey("dense") && match.Groups["dense"].Value == "+";

                }
            }
            catch (Exception ex)
            {
                TraceLogger.LogWarningAuto($"Failed to parse ProfileSurfaceDetail from encoded string: {encodedSurface}. Exception: {ex.Message}");
                BeachSurface = string.Empty;
                IsG70percent = false;
            }
        }

        if (string.IsNullOrEmpty(encodedSurface))
        {
            BeachSurface = string.Empty;
            IsG70percent = false;
        }
        else
        {
            if (encodedSurface.EndsWith(G70Flag))
            {
                BeachSurface = encodedSurface.Substring(0, encodedSurface.Length - 1);
                IsG70percent = true;
            }
            else
            {
                BeachSurface = encodedSurface;
                IsG70percent = false;
            }
        }
    }
    public static char G70Flag = '+';
    public static string? EncodeProfileSurfaceDetails(IEnumerable<ProfileSurfaceDetail> sds)
    {
        if (!sds.Any()) return null;
        List<string> surfacedetails = new List<string>();
        foreach (var sd in sds.OrderBy(n=>n.BeachSurface))
        {
            surfacedetails.Add(EncodeProfileSurfaceDetail(sd));
        }
        return string.Join(";", surfacedetails);
    }
    public static string EncodeProfileSurfaceDetail(ProfileSurfaceDetail sd)
    {
        return EncodeProfileSurfaceDetail(sd.BeachSurface, sd.IsG70percent);
    }


    public static string EncodeProfileSurfaceDetail(string surface, bool isG70percent)
    {
        string profile = surface.Trim();
        if (isG70percent)
            profile += G70Flag.ToString();
        return profile;
    }
}
