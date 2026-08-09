using DataLibrary.DataSources;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DataLibrary;

internal class UnPacked
{
    public string HGCS { get; set; }
}
public class JsonConfig : IDataSourceConfig
{
    public string HGCSDeskTop = @"564315507236-lpbuaeve4ptabb1b4m980abdvoqg4sut.";

    public string HGCSUnpacked = string.Empty;
    public string HGAK { get; set; } = string.Empty;
    public string HGS { get; set; } = string.Empty;
    public string HGSA { get; set; } = string.Empty;

    public string ArchiveFolderPath = string.Empty;
    public string GlobalZipId = string.Empty;
    public string SurveyZipId = string.Empty;

    [JsonIgnore]
    public GoogleDriveApiClient? GoogleClient { get; set; } = null;
    [JsonIgnore]
    public ServiceAccountCredential? DriveCredential { get; set; } = null;
    [JsonIgnore]
    public DriveService? DriveService { get; set; } = null;

    [JsonIgnore]
    Regex regex = new Regex("client_secret[:](?'code'[A-Za-z0-9-]{35})");
    public void UnPack()
    {
        if (!string.IsNullOrEmpty(HGCSDeskTop) && HGCSDeskTop.EndsWith("."))
        {
            HGCSDeskTop += "apps.googleusercontent.com";
        }
        if (!string.IsNullOrEmpty(HGS))
        {
            Match matches = regex.Match(HGS.Replace("\"",""));
            if (matches.Success)
            {
                HGCSUnpacked = matches.Groups["code"].Value.ToString();
            }
            else
            {
                throw new Exception("Error unpacking HGS");
            }
        }
    }
}
