namespace DataLibrary.DataSources.CloudAuth;

/// <summary>
/// User information from Google OAuth
/// </summary>
public class GoogleAuthUser
{
    [Newtonsoft.Json.JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [Newtonsoft.Json.JsonProperty("email")]
    public string Email { get; set; } = string.Empty;

    [Newtonsoft.Json.JsonProperty("verified_email")]
    public bool VerifiedEmail { get; set; }

    [Newtonsoft.Json.JsonProperty("name")]
    public string? Name { get; set; }

    [Newtonsoft.Json.JsonProperty("given_name")]
    public string? GivenName { get; set; }

    [Newtonsoft.Json.JsonProperty("family_name")]
    public string? FamilyName { get; set; }

    [Newtonsoft.Json.JsonProperty("picture")]
    public string? Picture { get; set; }
}