using Avalonia;
using Avalonia.Browser;
using Avalonia.Logging;
using DataLibrary;
using DataLibrary.ApiServices;
using DataLibrary.DataSources;
using DataLibrary.DataSources.ApiClients;
using DataLibrary.DataSources.CloudAuth;
using DataLibrary.DataSources.FileServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using ReactiveUI.Avalonia;
using SWSMonitor;
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

internal sealed partial class Program
{
    private static bool _isNotDesigner = true;
    private static bool _jsModuleInitialized = false;
    public static ServiceProvider? ServiceProvider { get; private set; }



    [JSExport]
    public static async Task Main(string[] args)
    {
        try
        {
            if (OperatingSystem.IsBrowser())
            {
                StaticData.RunningInBrowser = true;
            }
            TraceLogger.SetupTrace("console");

            // Setup DI before building Avalonia app
            await ConfigureServicesAsync();

            StaticData.MiscInfo = await MiscInfoApiClient.GetAllMiscInfoAsync(StaticData.DataSourceConfig);
            if (StaticData.MiscInfo is null)
            {
                TraceLogger.LogErrorAuto("Unable to reach api server or retrieve MiscInfo. Please check your network connection and API settings.");
                throw new Exception("Startup failed: Unable to reach api server or retrieve application info.");
            }
            else
            {
                AppBuilder builder = await BuildAvaloniaApp(isNotDesigner: true);
                await builder.StartBrowserAppAsync("out");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex}");

            // Call JavaScript to display a static DOM error overlay
            ShowStaticErrorPage(ex.Message);
            // This is the absolute "top level" before the process exits
            // Log to a file or external service here
            // File.WriteAllText("crash.log", ex.ToString());
        }
    }

    [JSImport("showErrorMessage", "ErrorInterop")]
    internal static partial void ShowStaticErrorPage(string message);


    /// <summary>
    /// Initializes the JavaScript module for JSImport interop
    /// Must be called before any JSImport methods are invoked
    /// </summary>
    private static async Task InitializeJavaScriptModuleAsync()
    {
#if DEBUG
        string jsprefix = "/";
#else
        string jsprefix = "../";
#endif

        if (!_jsModuleInitialized)
        {
            try
            {
                await JSHost.ImportAsync("ErrorInterop.js", $"{jsprefix}errorInterop.js");

                // Google identification service
                await JSHost.ImportAsync("fedCM.js", $"{jsprefix}fedCM.js");

                //// Import the JS module
                //await JSHost.ImportAsync("downloadHelper.js", $"{jsprefix}downloadHelper.js");

                _jsModuleInitialized = true;

                TraceLogger.LogInformation("JavaScript storage module initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing JavaScript module: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }

    public static async Task<AppBuilder> BuildAvaloniaApp(bool isNotDesigner = false)
    {
        var app = AppBuilder.Configure<App>()
                .LogToTrace(LogEventLevel.Warning | LogEventLevel.Information | LogEventLevel.Error)
                .WithInterFont()
                .UseReactiveUI(_ => { });

        if (_isNotDesigner)
        {
            // Initialize secure storage and load API key
            // This now works because JSHost.ImportAsync was called in Main
            // await InitializeSecureStorageAsync();

            // Run examples after API key is loaded
            // await BeachDataApiExamples.RunExamples();
        }

        return app;
    }

    // API settings constants
    const string USEAPI = "CS_USEAPI";  // If true use the API for data access, otherwise use MySQL
    const string DEFAULTUSEAPI = "false";
    const string BASEURL = "CS_BASEURL"; // API website url
#if DEBUG
    const string DEFAULTBASEURL = "https://dragnstudios.com/"; // "https://localhost:7193/";
#else
    const string DEFAULTBASEURL = "https://dragnstudios.com/";
#endif
    const string BASEAPIKEY = "CS_BASEAPIKEY"; // API Key in case using protected API paths
#if DEBUG
    const string DEFAULTBASEAPIKEY = "none";
#else
    const string DEFAULTBASEAPIKEY = "none"; // provided after user logins with edit or admin privs
#endif

    private static async Task ConfigureServicesAsync()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ApiSettings:{BASEURL}"] = DEFAULTBASEURL,
                [$"ApiSettings:{BASEAPIKEY}"] = DEFAULTBASEAPIKEY // Only for DEBUG
            })
        .Build();

        services.AddSingleton<IConfiguration>(configuration);

        //var downloader = new Downloader();
        //services.AddSingleton<IDownloadService>(downloader);

        // Initialize JavaScript module for interop first
        await InitializeJavaScriptModuleAsync();

        // Register IJSRuntime for Avalonia Browser using JSHost adapter
        services.AddSingleton<IJSRuntime>(sp => new GoogleJSRuntime());

        // Register secure storage services
        services.AddSingleton<ISecureStorageService, BrowserSecureStorageService>();
        //        services.AddSingleton<ApiKeyService>();

        // Note that we don't have access to environment variables
        // Get API settings
        var baseUrl = configuration["ApiSettings:" + BASEURL]
            ?? DEFAULTBASEURL;
        var defaultApiKey = configuration["ApiSettings:" + BASEAPIKEY]
            ?? DEFAULTBASEAPIKEY;

        // Configure named HttpClients
        services.AddHttpClient("PublicClient", client =>
        {
            client.BaseAddress = new Uri($"{baseUrl}public");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient("ApiClient", client =>
        {
            client.BaseAddress = new Uri($"{baseUrl}api");
            client.Timeout = TimeSpan.FromSeconds(30);
            // Note: API key will be added dynamically after loading from storage
        });

        // Register IHttpClientFactory-based ApiClientConfig
        services.AddSingleton<IDataSourceConfig>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var publicClient = httpClientFactory.CreateClient("PublicClient");
            var apiClient = httpClientFactory.CreateClient("ApiClient");

            return new ApiClientConfig(publicClient, apiClient);
        });

        // Register API settings
        services.AddSingleton(new ApiClientSettings
        {
            BaseUrl = baseUrl,
            ApiUrl = $"{baseUrl}api",
            PublicUrl = $"{baseUrl}public",
            DefaultApiKey = defaultApiKey
        });

        services.AddSingleton<ICloudAuthConfig>(sp =>
        {
            return new GoogleAuthConfig(string.Empty, isWebBrowser: true);
        });

        ServiceProvider = services.BuildServiceProvider();

        // Set StaticData.DataSourceConfig from DI
        StaticData.ServiceProvider = ServiceProvider;
        StaticData.DataSourceConfig = ServiceProvider.GetRequiredService<IDataSourceConfig>();

        // Initialize API configuration
        await InitializeApiConfigurationAsync();
    }

    private static async Task InitializeApiConfigurationAsync()
    {
        try
        {
            var settings = ServiceProvider!.GetRequiredService<ApiClientSettings>();

            // Setup static API helper URLs
            ApiClientHelper.BASE_URL = settings.BaseUrl;
            ApiClientHelper.API_BASE_URL = settings.ApiUrl;
            ApiClientHelper.PUBLIC_BASE_URL = settings.PublicUrl;


            // Set StaticData.DataSourceConfig from DI
            StaticData.DataSourceConfig = ServiceProvider.GetRequiredService<IDataSourceConfig>();
            TraceLogger.LogInformation("API Configuration initialized successfully");
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto($"Error initializing API configuration: {ex.Message}");
            throw;
        }
    }


}

//public partial class Downloader : IDownloadService
//{
//    [JSImport("downloadFile", "downloadHelper")]
//    static partial void DownloadFileImpl(string filename, string contentType, string base64Content);

//    public void DownloadFile(string filename, string contentType, string base64Content)
//    {
//        DownloadFileImpl(filename, contentType, base64Content);
//    }
//}


// Helper class for API settings
public class ApiClientSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;
    public string DefaultApiKey { get; set; } = string.Empty;
}