using DataLibrary.Crud;
using DataLibrary.ModelExtensions;
using DataLibrary.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Models;
using System.IO.Compression;
using System.Text.Json;

namespace DataLibrary;

public class Archiver
{
    private DriveService? DriveService = null;
    private GoogleDriveApiClient? GoogleDriveApiClient = null;

    public Archiver(DriveService? driveService = null)
    {
        if (driveService is not null)
            DriveService = driveService;
    }

    public GoogleDriveApiClient? GetGoogleDriveApiClientOnly()
    {
        // For browser or cases where don't need ability to login client or upload files we just need the API client
        return new GoogleDriveApiClient(StaticData.JsonConfig.HGAK, string.Empty);
    }
    public DriveService? InitializeGoogleDriveConnector()
    {
        GoogleDriveApiClient googleDriveApiClient = new GoogleDriveApiClient(StaticData.JsonConfig.HGAK, StaticData.JsonConfig.HGSA);
        GoogleDriveApiClient = googleDriveApiClient;

        DriveService? driveService = googleDriveApiClient.GetDriveService();

        if (driveService == null)
            throw new Exception("Drive Service is not valid");
        DriveService = driveService;

        return driveService;
    }
    public static int ProgressValue { get; set; }
    public void ArchiveGlobalData()
    {
        string globalValues = JsonSerializer.Serialize<GlobalData>(StaticData.GlobalData, ExportSurvey.SerializerOptions);

        byte[] compressedGlobals = ExportSurvey.CompressString(globalValues);

        _ = GoogleDriveApiClient!.UpdateFileAsync(new Google.Apis.Drive.v3.Data.File(), 
            StaticData.JsonConfig.GlobalZipId, compressedGlobals, "application/zip");

    }

    public async Task ArchiveStudyData()
    {
        ProgressValue = 0;
        if (StaticData.Surveys is null || StaticData.Surveys.Count == 0)
            return; // Must not be loaded yet
        int progressSteps = (int)Math.Round(StaticData.Surveys!.Count / 20d,0);

        int lastprogress = 0;
        int progress = 0;

        var outputMemoryStream = new MemoryStream();
        using (var zipArchive = new ZipArchive(outputMemoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var surveybase in StaticData.Surveys ?? new List<SurveyBase>())
            {
                if (++progress > lastprogress + progressSteps)
                {
                    ProgressValue += 5;
                    lastprogress = progress;
                }

                Survey survey = await SurveyCrud.ReadSurveyData(StaticData.DataSourceConfig, surveybase.ID);
                string surveydata = JsonSerializer.Serialize<Survey>(survey, ExportSurvey.SerializerOptions);
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(surveydata);

                // The desired filename within the zip archive
                var entryName = $"{survey.BeachName}_{GetDateAsString(survey.SurveyDate)}";
                // The content of the file as a byte array
                var fileContent = inputBytes; 

                var zipEntry = zipArchive.CreateEntry(entryName);
                using (var entryStream = zipEntry.Open())
                {
                    entryStream.Write(fileContent, 0, fileContent.Length);
                }
            }
        }
        await GoogleDriveApiClient!.UpdateFileAsync(new Google.Apis.Drive.v3.Data.File(),
            StaticData.JsonConfig.SurveyZipId, outputMemoryStream, "application/zip");
        ProgressValue = 101;

    }

    private string GetDateAsString(DateTime date)
    {
        return date.ToString("yyyy-MM-dd");
    }

    public async Task LoadGlobalsFromGoogle()
    {
        if (GoogleDriveApiClient is null)
        {
            if (OperatingSystem.IsBrowser())
                GoogleDriveApiClient = GetGoogleDriveApiClientOnly();
            else
                InitializeGoogleDriveConnector();
        }

        byte[] compressedbytes = await GoogleDriveApiClient.DownloadBinaryFileAsync(StaticData.JsonConfig.GlobalZipId);
        if (compressedbytes.Length > 0)
        {
            string unzipped = ExportSurvey.DecompressString(compressedbytes);
            StaticData.GlobalData = JsonSerializer.Deserialize<GlobalData>(unzipped);
            //StaticData.Beaches = new List<BeachData>();
            //    StaticData.Beaches!.AddRange(StaticData.GlobalData!.Beaches ?? new List<BeachData>());
            //StaticData.Volunteers = new List<Volunteer>();
            //    StaticData.Volunteers!.AddRange(StaticData.GlobalData.Volunteers ?? new List<Volunteer>());
            //StaticData.Surveys = new List<SurveyBase>();
            //    StaticData.Surveys!.AddRange(StaticData.GlobalData.Surveys ?? new List<SurveyBase>());
            //StaticData.CityStates = new List<CityState>();
            //    StaticData.CityStates!.AddRange(StaticData.GlobalData.CityStates ?? new List<CityState>());
            //StaticData.Species = new List<Species>();
            //    StaticData.Species!.AddRange(StaticData.GlobalData.Species ?? new List<Species>());
            //StaticData.QuadratNotes = new List<string>();
            //    StaticData.QuadratNotes!.AddRange(StaticData.GlobalData.QuadratNotes ?? new List<string>());
        }
    }
}
