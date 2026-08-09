using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Models;
public class SpeciesListBase
{
    public string? Species { get; set; } = "";
    public string? Notes { get; set; } = "";
    public int? SpeciesLinkId { get; set; } = -1;

    public SpeciesListBase() { } // empty constructor for deserialization
    public SpeciesListBase(string encodedpecies)
    {
        if (!string.IsNullOrEmpty(encodedpecies))
        {
            if (SpeciesFormatter.IsMatch(encodedpecies))
            {
                var match = SpeciesFormatter.Match(encodedpecies);
                Species = match.Groups["species"].Value.Trim();
                Notes = match.Groups["notes"].Value.Trim('{', '}');
                SpeciesLinkId = int.TryParse(match.Groups["speciesid"].Value, out var parsedId) ? parsedId : null;
            }
            else
            {
                Species = encodedpecies; // If it doesn't match the pattern, treat the whole string as the species name.
            }
        }
    }

    public static Regex SpeciesFormatter = new Regex(@"^(?<species>[^#\{]+)\s*(?:#(?<speciesid>[0-9]+))?\s*(?:\{(?<notes>[^\}]*)\})?$");

    public static string EncodeSpecies(SpeciesListBase species, bool noSpeciesId = false)
    {
        string encoded = species.Species ?? "";
        if (!noSpeciesId && species.SpeciesLinkId.HasValue && species.SpeciesLinkId.Value > 0)
            encoded += $"#{species.SpeciesLinkId.Value}";
        if (!string.IsNullOrEmpty(species.Notes))
            encoded += $" {{{species.Notes}}}";
        return encoded;
    }
    public static string? EncodeSpeciesList(IEnumerable<SpeciesListBase> speciesList, bool noSpeciesId = false)
    {
        if (speciesList == null || !speciesList.Any()) return null;
        return string.Join(';', speciesList.Select(s => EncodeSpecies(s, noSpeciesId)).OrderBy(n => n));
    }
    public static IEnumerable<SpeciesListBase> DecodeSpeciesList(string? speciesList)
    {
        IEnumerable<SpeciesListBase> emptyList = new List<SpeciesListBase>();
        return speciesList is not null
            ? speciesList.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
             .Select(s => new SpeciesListBase(s))
            : emptyList;
    }

}
public class SpeciesList: SpeciesListBase
{
    public const string TableName = "SpeciesListView";
    public long SurveyID { get; set; } = -1L;
    public string BeachName { get; set; } = string.Empty;
    public DateTime SurveyDate { get; set; } = new DateTime();
    [JsonIgnore]
    public string? CommonNameOrDescription { get; set; } = "";
}
