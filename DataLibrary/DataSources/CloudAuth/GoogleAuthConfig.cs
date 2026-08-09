using DataLibrary.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices.JavaScript;
using System.Text.RegularExpressions;
using Microsoft.JSInterop;

namespace DataLibrary.DataSources.CloudAuth;

public class GoogleAuthConfig : ICloudAuthConfig
{
    // Client ID's don't need to be secret because they are public
    public string HGCSClientID = @"564315507236-lpbuaeve4ptabb1b4m980abdvoqg4sut.apps.googleusercontent.com";
    public string HGCSWebClientID = @"564315507236-sjpac7a4g8li959b1fdols618fs4t78l.apps.googleusercontent.com";

    public bool IsWebBrowser = true;

    public string HGCSUnpacked = string.Empty;
    public string HGS { get; set; } = string.Empty;

    Regex regex = new Regex("client_secret[:](?'code'[A-Za-z0-9-]{35})");

    public GoogleAuthConfig(string hgs, bool? isWebBrowser = false)
    {
        IsWebBrowser = isWebBrowser!.Value;
        HGS = hgs;
        UnPack();
    }

    public string GetClientID()
    {
        return IsWebBrowser ? HGCSWebClientID : HGCSClientID;
    }
    public void UnPack()
    {
        if (!string.IsNullOrEmpty(HGCSClientID) && HGCSClientID.EndsWith("."))
        {
            HGCSClientID += "apps.googleusercontent.com";
        }
        if (!string.IsNullOrEmpty(HGS))
        {
            Match matches = regex.Match(HGS.Replace("\"", ""));
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
    public const string StoreName = "SWSMonitor.Last.Account";
    public const string StoreKey = "LastUserEmail";

    public static async Task<GoogleAuthUser?> UseFedCMToLogin()
    {
        try
        {
            // Get IJSRuntime from DI
            var jsRuntime = StaticData.ServiceProvider?.GetService<IJSRuntime>();
            if (jsRuntime == null)
            {
                TraceLogger.LogErrorAuto("IJSRuntime not available. Cannot authenticate in browser.");
                return null;
            }

            // Use Google Identity Services via JavaScript
            string token = await jsRuntime.InvokeAsync<string>(
                "googleSignIn"
            );

            if (string.IsNullOrEmpty(token?.ToString()))
            {
                return null;
            }

            GoogleAuthUser? user = await GoogleCredentials.HandleGoogleCredentialAsync(token);
            return user;
        }
        catch (Exception ex)
        {
            return null;
        }
        return null;
    }
}
/// <summary>
/// IJSRuntime implementation for Avalonia Browser using JSHost and JSImport/JSExport
/// </summary>
public partial class GoogleJSRuntime : IJSRuntime
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        return InvokeAsync<TValue>(identifier, CancellationToken.None, args);
    }

    public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        try
        {
            var cloudAuthConfig =
            StaticData.ServiceProvider?.GetRequiredService<ICloudAuthConfig>()
            ?? throw new InvalidOperationException("ICloudAuthConfig not registered in DI container");

            var GCID = (cloudAuthConfig as GoogleAuthConfig)!.GetClientID();


            // For Google OAuth, intercept specific calls
            if (identifier == "googleSignIn" && !string.IsNullOrEmpty(GCID))
            {
                var clientId = GCID;
                var token = await InvokeGoogleSignInAsync(clientId);
                return (TValue)(object)token;
            }

            // Default: log and return default value
            Console.WriteLine($"JSRuntime call: {identifier}");
            return default(TValue)!;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JSRuntime error calling {identifier}: {ex.Message}");
            return default(TValue)!;
        }
    }

    [JSImport("triggerGoogleFedCM", "fedCM.js")]
    private static partial Task<string> InvokeGoogleSignInAsync(string clientId);
}

