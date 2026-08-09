using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Json;
using Google.Apis.Services;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace DataLibrary;

public class GoogleDriveApiClient
{
    private string _apiKey;
    private string _secret;
    private readonly HttpClient _httpClient;
    private ServiceAccountCredential? _driveCredential = null;
    private DriveService? _driveService = null;

    public GoogleDriveApiClient(string apiKey, string secret)
    {
        _apiKey = apiKey;
        _secret = secret;   
        _httpClient = new HttpClient();
    }
    public ServiceAccountCredential? GetCredentials(string keydata = "")
    {
        if (!string.IsNullOrEmpty(keydata))
            _secret = keydata;

        if (string.IsNullOrEmpty(_secret))
            throw new Exception("No service account information available");

        // Get the contents that were retrieved from the credential json file
        // Note that the service account must be granted access to the folder and files to be retrieved
        // Service account credentials involve using the ServiceAccountCredential.Initializer
        string[] scopes = new[] { DriveService.Scope.Drive };
        var credentialParameters = NewtonsoftJsonSerializer.Instance.Deserialize<JsonCredentialParameters>(_secret);

        if (credentialParameters.Type == "service_account")
        {
            var serviceAccountCredential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(credentialParameters.ClientEmail)
                {
                    Scopes = scopes,
                }.FromPrivateKey(credentialParameters.PrivateKey)
            );
            _driveCredential = serviceAccountCredential;
            return serviceAccountCredential;
        }
        Debug.WriteLine("Credentials for a service account must be supplied");
        return null;
    }
    public DriveService? GetDriveService(ServiceAccountCredential? driveCredential = null)
    {
        if (driveCredential is not null)
            _driveCredential = driveCredential;
        else if (_driveCredential is null)
        {
            try
            {
                _driveCredential = GetCredentials();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create Drive credentials from configuration.", ex);
            }
        }
        try
        {
            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = _driveCredential,
                ApplicationName = "BeachSurvey"
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create Drive service.", ex);
        }
        return _driveService;
    }

    public async Task<IEnumerable<GoogleDriveFile>> ListFilesInSharedFolder(string folderId)
    {
        List<GoogleDriveFile> files = new List<GoogleDriveFile>();
        string nextPageToken = "";
        do
        {
            string folderContentsUri = $"https://www.googleapis.com/drive/v3/files?q='{folderId}'+in+parents&key={_apiKey}&fields=nextPageToken,files(id,name,mimeType,size,modifiedTime)";

            if (!string.IsNullOrEmpty(nextPageToken))
            {
                folderContentsUri += $"&pageToken={nextPageToken}";
            }

            HttpResponseMessage response = await _httpClient.GetAsync(folderContentsUri);
            response.EnsureSuccessStatusCode(); // Throws an exception if not successful

            string contentsJson = await response.Content.ReadAsStringAsync();
            JObject contents = JObject.Parse(contentsJson);


            if (contents["files"] is JArray filesArray)
            {
                foreach (var file in filesArray)
                {
                    files.Add(new GoogleDriveFile(
                        file["id"]?.ToString(),
                        file["name"]?.ToString(),
                        file["mimeType"]?.ToString(),
                        file["size"] != null ? long.Parse(file["size"].ToString()) : 0,
                        file["modifiedTime"]?.ToString()
                    ));
                }
            }

            nextPageToken = contents["nextPageToken"]?.ToString();

        } while (!string.IsNullOrEmpty(nextPageToken));
        return files;
    }
    public async Task<string> DownloadUnZippedFileAsync(string fileId, string fileToUnzip = "")
    {
        // downloads a single file from google drive, if it is a zip file it will unzip and return the text contents of the first entry
        string unzippedContents = string.Empty;
        byte[] fileBytes = await DownloadBinaryFileAsync(fileId);
        if (fileBytes == null || fileBytes.Length == 0)
            return string.Empty;

        // Attempt to treat bytes as a zip archive and return the first entry's text content.
        // If the bytes are not a zip archive, fall back to interpreting them as UTF-8 text.
        try
        {
            using var ms = new MemoryStream(fileBytes);
            using var archive = new ZipArchive(ms, ZipArchiveMode.Read, leaveOpen: false);

            foreach (var entry in archive.Entries)
            {
                // skip directory entries
                if (string.IsNullOrEmpty(entry.Name))
                    continue;
                if (!string.IsNullOrEmpty(fileToUnzip) && !entry.Name.Equals(fileToUnzip, StringComparison.OrdinalIgnoreCase))
                    continue;
                // If we fall through to here we have found a suitable entry
                using var entryStream = entry.Open();
                using var reader = new StreamReader(entryStream, Encoding.UTF8);
                return await reader.ReadToEndAsync();
            }

            // no suitable entry found
            return string.Empty;
        }
        catch (InvalidDataException)
        {
            // Not a zip archive — interpret as compressed text file
            string uncompressedText = ExportSurvey.DecompressString(fileBytes);
            return Encoding.UTF8.GetString(fileBytes);
        }
    }
    public async Task<byte[]> DownloadBinaryFileAsync(string fileId)
    {
        // GET https://www.googleapis.com/drive/v3/files/[FILE_ID]?alt=media&key=[YOUR_API_KEY]
        var downloadUrl = $"https://drive.google.com/uc?id={fileId}&export=download&key={_apiKey}";
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

            byte[] contents = await ReadStreamIntoByteArrayAsync(await confirmResponse.Content.ReadAsStreamAsync());
            return contents;
//            return await SaveToTempFileAsync(await confirmResponse.Content.ReadAsStreamAsync());
        }
        else
        {
            // Direct download
            byte[] contents = await ReadStreamIntoByteArrayAsync(await response.Content.ReadAsStreamAsync());
            return contents;
        }
    }
    private async Task<byte[]> ReadStreamIntoByteArrayAsync(Stream stream)
    {
        if (stream == null)
            return Array.Empty<byte>();

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }
    private async Task<string> SaveToTempFileAsync(Stream stream)
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

    public async Task<bool> UpdateFileAsync(string destFileId, string sourceFilePath)
    {
        // updates an existing file on google drive with the contents of the specified local file
        if (!File.Exists(sourceFilePath))
            throw new FileNotFoundException("Source file not found", sourceFilePath);
        if (_driveService == null)
            throw new InvalidOperationException("Drive service is not initialized. Call GetDriveService() first.");
        using var fileStream = new FileStream(sourceFilePath, FileMode.Open);
        (string mimeType, bool success) = GetFileMimeType(sourceFilePath);
        var request = _driveService.Files.Update(new Google.Apis.Drive.v3.Data.File(), destFileId, fileStream, mimeType);
        request.Upload();
        return request.ResponseBody != null;
    }

    public async Task<bool> UpdateFileAsync(Google.Apis.Drive.v3.Data.File file, string destFileId, byte[] source, string mimeType)
    {
        // updates an existing file on google drive with the contents of a byte array- usually zipped contents
        if (source.Length == 0)
            throw new InvalidOperationException("Source byte array is empty.");
        if (_driveService == null)
            throw new InvalidOperationException("Drive service is not initialized. Call GetDriveService() first.");

        try
        {
            using var fileStream = new MemoryStream(source);
            var request = _driveService.Files.Update(new Google.Apis.Drive.v3.Data.File(), destFileId, fileStream, mimeType);
            request.Upload();
            return request.ResponseBody != null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Upload not successful");
        }

    }
    public async Task<bool> UpdateFileAsync(Google.Apis.Drive.v3.Data.File file, string destFileId, MemoryStream sourcestream, string mimeType)
    {
        // updates an existing file on google drive with the contents of a memory stream- usually zipped contents
        if (sourcestream is null)
            throw new InvalidOperationException("Memory stream invalid.");
        if (_driveService == null)
            throw new InvalidOperationException("Drive service is not initialized. Call GetDriveService() first.");

        try
        {
            var request = _driveService.Files.Update(new Google.Apis.Drive.v3.Data.File(), destFileId, sourcestream, mimeType);
            request.Upload();
            return request.ResponseBody != null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Upload not successful");
        }
    }

    private (string, bool) GetTranslatedMimeType(string googleMimeType)
    {
        // translates the three google document types into standard mime types for export
        // flags all the other types as binary to be downloaded directly
        return googleMimeType switch
        {
            "application/vnd.google-apps.spreadsheet" => ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", false),
            "application/vnd.google-apps.document" => ("application/vnd.openxmlformats-officedocument.wordprocessingml.document", false),
            "application/vnd.google-apps.presentation" => ("application/vnd.openxmlformats-officedocument.presentationml.presentation", false),
            _ => (googleMimeType, true),
        };
    }

    private static (string, bool) GetFileMimeType(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLower();
        switch (extension)
        {
            case "xlsx":
            case "xls":
                return ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", false);
            case "zip":
                return ("application/zip", true);
            case "pdf":
                return ("application/pdf", false);
            case "json":
                return ("application/json", false);
            case "docx":
                return ("application/vnd.openxmlformats-officedocument.wordprocessingml.document", false);
            case "odt":
                return ("application/vnd.oasis.opendocument.text", false);
            case "txt":
                return ("text/plain", false);
            case "csv":
                return ("text/csv", false);
            case "tsv":
                return ("text/tab-separated-values", false);
            case "png":
                return ("image/png", true);
            case "jpg":
            case "jpeg":
                return ("image/jpeg", true);
            case "svg":
                return ("image/svg+xml", false);
            default:
                return ("unknown", false);

        }
    }

}
public class GoogleDriveFile
{
    Google.Apis.Drive.v3.Data.File File { get; set; } = new Google.Apis.Drive.v3.Data.File();
    public string Id { get; set; }
    public string Name { get; set; }
    public string MimeType { get; set; }
    public long Size { get; set; }
    public string ModifiedTime { get; set; } // 11/25/2025 9:24:03 PM
    public DateTime ModDate { get => string.IsNullOrEmpty(ModifiedTime) ? DateTime.MinValue : DateTime.Parse(ModifiedTime); }
    public GoogleDriveFile(string id, string name, string mimeType, long size, string modifiedTime)
    {
        Id = id;
        Name = name;
        MimeType = mimeType;
        Size = size;
        ModifiedTime = modifiedTime;
    }
}