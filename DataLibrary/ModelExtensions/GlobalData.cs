using DataLibrary.Models;
using Models;

namespace DataLibrary.ModelExtensions;
public static class GlobalConstants
{
    public static int DefaultTide1Ht = 1;
    public static int DefaultTide2Ht = 0;
    public static int DefaultTide3Ht = -1;

}
public class GlobalData
{
    // Holds all global data for easy saving/loading via JSON
    public List<BeachData>? Beaches { get; set; } = new List<BeachData>();
    public List<Volunteer>? Volunteers { get; set; } = new List<Volunteer>();
    public List<SurveyBase>? Surveys { get; set; } = new List<SurveyBase>();
    public List<Species>? Species { get; set; } = new List<Species>();
    public List<string> Contents { get; set; } = new List<string>();
    public List<string> QuadratNotes { get; set; } = new List<string>();
    public List<CityState>? CityStates { get; set; } = new List<CityState>();
    public List<LookupTable>? LookupTables { get; set; } = new List<LookupTable>();


}
