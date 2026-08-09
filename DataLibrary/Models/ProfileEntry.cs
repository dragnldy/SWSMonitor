using DataLibrary.ModelExtensions;
using System.Text.Json.Serialization;

namespace Models;

/// <summary>
/// Represents a ProfileEntries.
/// NOTE: This class is generated from a T4 template - you should not modify it manually.
/// </summary>
public class ProfileBase : ITableBase
{
    public const string TableName = "ProfileEntries";

    public long ID { get; set; } = 0;

    public long SurveyID { get; set; }
    public int? EntryNo { get; set; }

    public int? Length { get; set; }
    public double? SurveyReading { get; set; }

    public string? Details { get; set; } = null;

    public string? SurfaceDetails { get; set; } = null;

    public DateTime? EntryDate { get; set; }


  
    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileBase"/> class.
    /// </summary>
    public ProfileBase()
    {
        // Initialize properties if needed
    }
}

public class ProfileEntry : ProfileBase
{
    [JsonIgnore, DBIgnore]
    public IEnumerable<ProfileDetail> ProfileDetails
    {
        get
        {
            int index = 0;
            return Details is not null
                ? Details.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(sd => new ProfileDetail(sd, ++index))
                : new List<ProfileDetail>();
        }
    }

    public static IEnumerable<ProfileDetail> DecodeProfileDetailList(string? codedList)
    {
        int index = 0;
        return !string.IsNullOrEmpty(codedList)
            ? codedList.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(sd => new ProfileDetail(sd, ++index))
            : new List<ProfileDetail>();
    }

    [JsonIgnore, DBIgnore]
    public IEnumerable<ProfileSurfaceDetail> ProfileSurfaceDetails
    {
        get
        {
            int index = 0;
            return SurfaceDetails is not null
                ? SurfaceDetails.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(sd => new ProfileSurfaceDetail(sd, ++index))
                : new List<ProfileSurfaceDetail>();
        }
    }
    public static IEnumerable<ProfileSurfaceDetail> DecodeProfileSurfaceDetailList(string? codedList)
    {
        int index = 0;
        return !string.IsNullOrEmpty(codedList)
            ? codedList.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(sd => new ProfileSurfaceDetail(sd, ++index))
            : new List<ProfileSurfaceDetail>();
    }
}
