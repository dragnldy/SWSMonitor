namespace DataLibrary.DataSources.ApiClients;

public static class ApiInit
{
    // These clients just have raw base address
    static HttpClient? _client;
    static HttpClient? _authenticatedClient;
    // These clients have the public and api routes added
    static HttpClient? _publicClient;
    static HttpClient? _apiClient;

    // API Key for testing - should match what's configured in the API
    public const string TestApiKey = "dev-test-key-12345";

    // Base address for the API
    const string BaseAddress = "https://localhost:7193";
}
/// <summary>
/// Configuration for API clients with support for dependency injection
/// </summary>
public class ApiClientConfig : IDataSourceConfig
{

    private readonly HttpClient _client;
    private readonly HttpClient _authenticatedClient;

    public ApiClientConfig(HttpClient client, HttpClient authenticatedClient)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _authenticatedClient = authenticatedClient ?? throw new ArgumentNullException(nameof(authenticatedClient));
    }

    public ApiClientConfig(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _authenticatedClient = client;
    }

    public ApiClientConfig()
    {
        _client = new HttpClient();
        _authenticatedClient = new HttpClient();
    }

    public HttpClient PublicClient => _client;
    public HttpClient APIClient => _authenticatedClient;
    public string PublicUrl => _client.BaseAddress?.ToString() ?? string.Empty;
    public string ApiUrl => _authenticatedClient.BaseAddress?.ToString() ?? string.Empty;
    public string ViewUrl => PublicUrl.Replace("public", "view");
    public string PrivateUrl => PublicUrl.Replace("public", "private");
}