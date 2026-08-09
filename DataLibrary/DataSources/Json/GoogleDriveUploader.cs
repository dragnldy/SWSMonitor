using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;

internal sealed class GoogleDriveUploader
{
    // CoPilot Instructions
    //•	Add NuGet packages: Google.Apis.Drive.v3, Google.Apis.Auth, Google.Apis.Core.
    //•	Use service account JSON for server-to-server uploads.For user OAuth (interactive consent) use GoogleWebAuthorizationBroker (not shown).
    //•	Handle credentials and secrets outside source control.
    private readonly DriveService _driveService;

    private GoogleDriveUploader(DriveService driveService)
    {
        _driveService = driveService ?? throw new ArgumentNullException(nameof(driveService));
    }

    /// <summary>
    /// Create an uploader using a service account JSON key file.
    /// Requires the service account to have Drive access (or domain-wide delegation configured).
    /// </summary>
    public static Task<GoogleDriveUploader> CreateFromServiceAccountAsync(string serviceAccountJsonPath, string applicationName = "App", params string[] scopes)
    {
        if (string.IsNullOrWhiteSpace(serviceAccountJsonPath)) throw 
                new ArgumentException("Path required", nameof(serviceAccountJsonPath));
        scopes ??= new[] { DriveService.Scope.Drive };

        GoogleCredential credential = CredentialFactory.FromFile(serviceAccountJsonPath, JsonCredentialParameters.AuthorizedUserCredentialType);
        //        GoogleCredential credential = GoogleCredential.FromFile(serviceAccountJsonPath).CreateScoped(scopes);
        scopes ??= new[] { DriveService.Scope.Drive };
        var service = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = applicationName
        });
        return Task.FromResult(new GoogleDriveUploader(service));
    }

    /// <summary>
    /// Uploads a file. Returns the Drive file id on success.
    /// </summary>
    public async Task<string> UploadFileAsync(string localFilePath, string? mimeType = null, string? parentFolderId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(localFilePath)) throw new ArgumentException("file path required", nameof(localFilePath));
        if (!System.IO.File.Exists(localFilePath)) throw new FileNotFoundException("File not found", localFilePath);

        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = Path.GetFileName(localFilePath),
        };
        if (!string.IsNullOrWhiteSpace(parentFolderId))
            fileMetadata.Parents = new List<string> { parentFolderId };

        mimeType ??= "application/octet-stream";

        using var stream = System.IO.File.OpenRead(localFilePath);
        var request = _driveService.Files.Create(fileMetadata, stream, mimeType);
        request.Fields = "id,name,mimeType,parents";

        var progress = await request.UploadAsync(cancellationToken).ConfigureAwait(false);
        if (progress.Status == UploadStatus.Completed && request.ResponseBody != null)
        {
            return request.ResponseBody.Id!;
        }

        // Provide helpful message on failure
        var ex = progress.Exception ?? new InvalidOperationException($"Upload failed: {progress.Status}");
        throw ex;
    }

    /// <summary>
    /// Creates a folder if it does not already exist. Returns the folder id.
    /// </summary>
    public async Task<string> CreateFolderIfNotExistsAsync(string folderName, string? parentFolderId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(folderName)) throw new ArgumentException("folder name required", nameof(folderName));

        // Try to find existing folder with the same name in the parent (or root)
        string q = $"mimeType = 'application/vnd.google-apps.folder' and name = '{EscapeQuery(folderName)}' and trashed = false";
        if (!string.IsNullOrWhiteSpace(parentFolderId))
            q += $" and '{parentFolderId}' in parents";

        var listRequest = _driveService.Files.List();
        listRequest.Q = q;
        listRequest.Fields = "files(id, name)";
        var list = await listRequest.ExecuteAsync(cancellationToken).ConfigureAwait(false);
        var existing = list.Files?.FirstOrDefault();
        if (existing is not null && !string.IsNullOrWhiteSpace(existing.Id))
            return existing.Id;

        var fileMetadata = new Google.Apis.Drive.v3.Data.File { Name = folderName, MimeType = "application/vnd.google-apps.folder" };
        if (!string.IsNullOrWhiteSpace(parentFolderId))
            fileMetadata.Parents = new List<string> { parentFolderId };

        var createRequest = _driveService.Files.Create(fileMetadata);
        createRequest.Fields = "id";
        var created = await createRequest.ExecuteAsync(cancellationToken).ConfigureAwait(false);
        return created.Id!;
    }

    private static string EscapeQuery(string value) => value.Replace("'", "\\'");

    /// <summary>
    /// Simple convenience to upload a file to a named folder (create folder if needed).
    /// Returns uploaded file id.
    /// </summary>
    public async Task<string> UploadToNamedFolderAsync(string localFilePath, string folderName, string? mimeType = null, CancellationToken cancellationToken = default)
    {
        var folderId = await CreateFolderIfNotExistsAsync(folderName, null, cancellationToken).ConfigureAwait(false);
        return await UploadFileAsync(localFilePath, mimeType, folderId, cancellationToken).ConfigureAwait(false);
    }
}