using Avalonia.Media.Imaging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SWSMonitor;

public class GooglePhotoInfo
{
    public string Path { get; set; } = string.Empty; // folder path to get to the photo
    public string Name { get; set; } = string.Empty; // name of the photo file
    public string Url { get; set; } = string.Empty; // URL to access the photo

    [JsonIgnore]
    public string YearOfPhoto { get; set; } = string.Empty;
    [JsonIgnore]
    public string BeachNameOfPhoto { get; set; } = string.Empty;
    [JsonIgnore]
    public string Id { get; set; } = string.Empty;

}

public class GooglePhotoService
{
    // Google app project that returns json file containing URL's for all photos in the Google Photos album
    public const string PhotoAppProjectUrl =
        "https://script.google.com/macros/s/AKfycby2Ssdhhcp45cuEqZR-oQpfgP4i6c6AIbB-8ut_GbJwjLRJIxKUj_lwd_6eyrC4QATy/exec";

    private static readonly HttpClient _photoClient = new HttpClient();
    
    public static List<GooglePhotoInfo> Photos { get; set; } = new List<GooglePhotoInfo>();

    public GooglePhotoService()
    {
        try
        {
            _ = GetGooglePhotoList();

            //var assetLoader = Avalonia.Platform.AssetLoader.Open(new Uri("avares://SWSMonitor/Assets/photolibrary.json"));

            //using StreamReader reader = new StreamReader(assetLoader);
            //string jsonList = reader.ReadToEnd();
            //LoadPhotoList(jsonList);

            //TraceLogger.LogWarningAuto($"Loaded {Photos.Count} photos from Avalonia assets",
            //    nameof(GooglePhotoService), nameof(GooglePhotoService), 0);
        }
        catch (Exception ex)
        {
                TraceLogger.LogErrorAuto($"Failed to read photolibrary.json from Avalonia assets: {ex.Message}",
                    nameof(GooglePhotoService), nameof(GooglePhotoService), 0);
        }
    }

    private bool LoadPhotoList(string jsonList)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        try
        {
            Photos = JsonSerializer.Deserialize<List<GooglePhotoInfo>>(jsonList, options) ?? new List<GooglePhotoInfo>();

            foreach (var photo in Photos)
            {
                // Extract year and beach name from the path
                // Currently formatted as /photos/2023/Island/BeachName/photo.jpg
                var pathParts = photo.Path.Split('/');
                if (pathParts.Length >= 5)
                {
                    var root = pathParts[1]; // should be 'photos'
                    var year = pathParts[2]; // should be the year
                    var island = pathParts[3];
                    var beach = pathParts[4];
                    photo.YearOfPhoto = year;
                    photo.BeachNameOfPhoto = beach;
                }
                // Not get the id from the urls
                // &id=1eutCSMNNXRC1eIZRNyK_k-LCOLfXuulv
                int idStart = photo.Url.IndexOf("id=");
                if (idStart > 0)
                {
                    photo.Id = photo.Url.Substring(idStart + 3);
                }
            }
        }
        catch(Exception ex)
        {
            TraceLogger.LogErrorAuto($"Failed to deserialize photo list: {ex.Message}");
            return false;
        }
        return Photos.Any();
    }
    public async Task GetGooglePhotoList()
    {
        try
        {
            // Make an HTTP GET request to the Google Photos app project URL
            HttpResponseMessage response = await _photoClient.GetAsync(PhotoAppProjectUrl);
            if (response.IsSuccessStatusCode)
            {
                // If the request is successful, read the response content as a string
                string jsonResponse = await response.Content.ReadAsStringAsync();
                LoadPhotoList(jsonResponse);
            }
            else
            {
                TraceLogger.LogErrorAuto($"Failed to retrieve photo list. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto($"An error occurred while retrieving the photo list: {ex.Message}");
        }
    }
    public static async Task<Bitmap?> GetGooglePhoto(string year, string beachname)
    {
        GooglePhotoInfo? photoInfo = Photos.FirstOrDefault(p => !p.Name.ToLower().EndsWith(".heic") && !p.Name.ToLower().EndsWith(".heif") &&
        p.YearOfPhoto == year && p.BeachNameOfPhoto == beachname);
        if (photoInfo == null || string.IsNullOrEmpty(photoInfo.Url)) return null;

        Bitmap? bitmap = await LoadGoogleDriveImageAsync(photoInfo.Id, photoInfo.Name);
        if (bitmap is not null)
        {
            return bitmap;
        }
        else
        {
            // Handle errors (e.g., log them, throw exceptions, etc.)
            TraceLogger.LogErrorAuto(
                $"Failed to retrieve photo.");
        }
        return null;
    }
    public static async Task<Bitmap?> LoadGoogleDriveImageAsync(string fileid, string fileName, string apiKey = "AIzaSyCfwa6xSNwown7QWAKhI2B7QJPy-uk2uQs")
    {
        // Safe endpoint for programmatic and unauthenticated cross-domain file piping
        string requestUrl = $"https://www.googleapis.com/drive/v3/files/{fileid}?alt=media&key={apiKey}";
        using HttpClient client = new HttpClient();
        try
        {
            byte[] bytes = await client.GetByteArrayAsync(requestUrl);

            string fileExtension = Path.GetExtension(fileName).ToLower();

            switch (fileExtension)
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".bmp": // default supported formats for Bitmap
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        Bitmap bitmap = new Bitmap(ms);
                        return bitmap;
                    }
                    break;

                case ".heic":
                case ".heif": // Apple file formats, not natively supported by System.Drawing.Bitmap
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        //SKBitmap? bitmap2 = HeicDecoder.Decode(ms);
                        //return ConvertSkBitmapToBitmapUsingStream(bitmap2);
                        return null;
                    }
                    default:
                    TraceLogger.LogErrorAuto($"Unsupported image format: {fileExtension}");
                    return null; // Unsupported format
            }
        }

        catch (Exception ex)
        {
            // Handle errors (e.g., log them, throw exceptions, etc.)
            TraceLogger.LogErrorAuto(
                $"Failed to retrieve photo. {ex.ToString()}");

            return null; // Handle network or permission errors
        }
        return null;
    }
    public static Bitmap ConvertSkBitmapToBitmapUsingStream(SKBitmap skBitmap)
    {
        using (var image = SKImage.FromBitmap(skBitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
        using (var stream = new MemoryStream())
        {
            data.SaveTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return new Bitmap(stream);
        }
    }
}
