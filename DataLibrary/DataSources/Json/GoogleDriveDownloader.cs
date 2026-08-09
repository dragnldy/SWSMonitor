using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DataLibrary;

public class GoogleDriveDownloader
{
    public static async Task<bool> DownloadFileAsyncx(string fileId, string savePath, string secretsPath = "")
    {
        UserCredential credential;
        // Load the client secrets file
        using (var stream = new FileStream(secretsPath, FileMode.Open, FileAccess.Read))
        {
            try
            {
                string credPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SWS-BeachSurveys");
                credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { DriveService.Scope.DriveReadonly }, // Set the scope to read-only or full drive access
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading client secrets: " + ex.Message);
                throw;
            }
        }

        try
        {

            // Create the Drive API service
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "BeachSurvey",
            });

            (string fileName,string mimeType) fileInfo = await GetGoogleDriveFileNamex(service, fileId);
            if (fileInfo.mimeType.Equals("unknown", StringComparison.InvariantCultureIgnoreCase))
                return false;

            (string mimeType, bool isBinary) = GetTranslatedMimeTypex(fileInfo.mimeType);

            var saveFile = Path.Combine(Path.GetDirectoryName(savePath) ?? string.Empty, fileInfo.fileName);

            string result = string.Empty;
            bool success = false;
            if (isBinary)
            {
                FilesResource.GetRequest request = service.Files.Get(fileId);
                using (var stream = new MemoryStream())
                {
                    // Download the file content into the memory stream
                    request.MediaDownloader.ProgressChanged += (Google.Apis.Download.IDownloadProgress progress) =>
                    {
                        switch (progress.Status)
                        {
                            case Google.Apis.Download.DownloadStatus.Downloading:
                                // Update UI with download progress (Avalonia specific implementation)
                                result = $"Downloading: {progress.BytesDownloaded} bytes";
                                break;
                            case Google.Apis.Download.DownloadStatus.Completed:
                                result = "Download complete.";
                                success = true;
                                break;
                            case Google.Apis.Download.DownloadStatus.Failed:
                                result = "Download failed.";
                                success = false;
                                break;
                        }
                    };
                    await request.DownloadAsync(stream);
                    if (success)
                    {
                        // Save the downloaded stream to a local file
                        using (var fileStream = new FileStream(saveFile, FileMode.Create, FileAccess.Write))
                        {
                            stream.WriteTo(fileStream);
                        }
                        return true;
                    }

                }
            }
            else
            {
                FilesResource.ExportRequest request = service.Files.Export(fileId, mimeType: mimeType);
                using (var stream = new MemoryStream())
                {
                    // Download the file content into the memory stream
                    request.MediaDownloader.ProgressChanged += (Google.Apis.Download.IDownloadProgress progress) =>
                    {
                        switch (progress.Status)
                        {
                            case Google.Apis.Download.DownloadStatus.Downloading:
                                // Update UI with download progress (Avalonia specific implementation)
                                result = $"Downloading: {progress.BytesDownloaded} bytes";
                                break;
                            case Google.Apis.Download.DownloadStatus.Completed:
                                result = "Download complete.";
                                success = true;
                                break;
                            case Google.Apis.Download.DownloadStatus.Failed:
                                result = "Download failed.";
                                success = false;
                                break;
                        }
                    };
                    await request.DownloadAsync(stream);
                    if (success)
                    {
                        // Save the downloaded stream to a local file
                        using (var fileStream = new FileStream(saveFile, FileMode.Create, FileAccess.Write))
                        {
                            stream.WriteTo(fileStream);
                        }
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("Error downloading file" + result);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error download file: " + ex.Message);
            throw;
        }
        return false;
    }

    private static (string,bool) GetTranslatedMimeTypex(string googleMimeType)
    {
        // translates the three google document types into standard mime types for export
        // flags all the other types as binary to be downloaded directly
        return googleMimeType switch
        {
            "application/vnd.google-apps.spreadsheet" => ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",false),
            "application/vnd.google-apps.document" => ("application/vnd.openxmlformats-officedocument.wordprocessingml.document",false),
            "application/vnd.google-apps.presentation" => ("application/vnd.openxmlformats-officedocument.presentationml.presentation",false),
            _ => (googleMimeType,true),
        };
    }
    public async Task<string> DownloadBinaryFileAsyncx(string fileId)
    {
        var downloadUrl = $"https://drive.google.com/uc?id={fileId}&export=download";
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(downloadUrl);
        var content = await response.Content.ReadAsStringAsync();

        // Check for confirmation page (for large files)
        var confirmMatch = Regex.Match(content, @"confirm=([0-9A-Za-z_]+)");
        if (confirmMatch.Success)
        {
            var confirmToken = confirmMatch.Groups[1].Value;
            var confirmUrl = $"https://drive.google.com/uc?export=download&id={fileId}&confirm={confirmToken}";
            using var confirmResponse = await httpClient.GetAsync(confirmUrl);
            return await SaveToTempFileAsyncx(await confirmResponse.Content.ReadAsStreamAsync());
        }
        else
        {
            // Direct download
            return await SaveToTempFileAsyncx(await response.Content.ReadAsStreamAsync());
        }
    }
    private async Task<string> SaveToTempFileAsyncx(Stream stream)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
        using (var fileStream = File.Create(tempFile))
        {
            await stream.CopyToAsync(fileStream);
        }
        var fileInfo = new FileInfo(tempFile);
        if (fileInfo.Length == 0)
            throw new Exception("Downloaded file is empty.");
        return tempFile;
    }
    private static (string,bool) GetFileMimeTypex(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLower();
        switch(extension)
        {
            case "xlsx":
            case "xls":
                return ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",false);
            case "zip":
                return ("application/zip",true);
            case "pdf":
                return ("application/pdf",false);
            case "json":
                return ("application/json",false); 
            case "docx":
                return ("application/vnd.openxmlformats-officedocument.wordprocessingml.document",false);
            case "odt":
                return ("application/vnd.oasis.opendocument.text",false);
            case "txt":
                return ("text/plain",false);
            case "csv":
                return ("text/csv",false);
            case "tsv":
                return ("text/tab-separated-values",false);
            case "png":
                return ("image/png",true);
            case "jpg":
            case "jpeg":
               return ("image/jpeg",true);
            case "svg":
                return ("image/svg+xml",false);
            default:
                return ("unknown",false);

        }
    }

    private static async Task<(string fileName,string mimeType)> GetGoogleDriveFileNamex(DriveService service, string fileId)
    {
        try
        { 
            // Create a Get request for the specific file ID
            FilesResource.GetRequest request = service.Files.Get(fileId);
            request.Fields = "name,mimeType"; // Request only the 'name' and mimeType fields

            // Execute the request and get the file metadata
            Google.Apis.Drive.v3.Data.File file = await request.ExecuteAsync();

            if (file != null)
            {
                return (file.Name,file.MimeType)
                    ;
            }
            else
            {
                return ("File not found.","unknown");
            }
        }
        catch (Google.GoogleApiException ex)
        {
            // Handle Google Drive API errors
            Console.WriteLine( $"Error retrieving file name: {ex.Message}");
            throw;
        }
        catch (System.Exception ex)
        {
            // Handle other exceptions
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            throw;
        }    
    }
}
