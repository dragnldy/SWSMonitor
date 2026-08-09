using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;

public partial class ChromeInterop
{
    // Imports the JavaScript function from chromeinterop.js
    [JSImport("chromeInterop.getUserInfo", "chromeinterop.js")]
    private static partial Task<string> GetChromeUserInfoAsync();

    public static async Task<ChromeUser?> GetUserData()
    {
        // Fetch raw JSON string from browser context
        string jsonResult = await GetChromeUserInfoAsync();

        // Deserialize into C# object
        return JsonSerializer.Deserialize<ChromeUser>(jsonResult, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}

public class ChromeUser
{
    public string Email { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty; // Gaia ID
}