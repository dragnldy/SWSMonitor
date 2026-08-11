using DataLibrary.Crud;
using DataLibrary.DataSources;
using DataLibrary.ModelExtensions;
using DataLibrary.Models;
using Models;

namespace DataLibrary;

public static class StaticData
{
    // Only one page at a time can attempt this
    public static readonly SemaphoreSlim _semaphore = new(
          initialCount: 1, maxCount: 1);

    // Flag to indicate all static data successfully loaded
    public static bool IsDataLoaded { get; set; } = false;

    // User authentication and authorization properties
    public static bool UserCanLogin { get; set; } = false; // Only supported browser is Chrome
    public static bool UserIsSignedIn { get; set; } = false;
    public static MiscInfo MiscInfo { get; set; } = new MiscInfo();
    public static bool UserCanEdit { get => _userRole >= AppRoleEnum.Edit;
        set; } = false;
    public static string UserAccount { get; set; } = String.Empty;
    public static string UserName { get; set; } = String.Empty;

    private static AppRoleEnum _userRole = AppRoleEnum.Public;
    public static AppRoleEnum UserRole { get => _userRole; set => _userRole = value; }
    public static bool RequireAuditTrail { get; set; } = false;
    public static bool RequireAuthentication { get; set; } = false;

    public static double MapCenterLatitude { get; set; } = 48.13904296296296;
    public static double MapCenterLongitude { get; set; } = -122.52544333333336;

    public static bool RunningInBrowser { get; set; } = false;
    public static object MainWindowModel { get; set; } = null;
    public static IServiceProvider? ServiceProvider { get; set; }
    public static IDataSourceConfig? DataSourceConfig { get; set; }
//    public static JsonConfig JsonConfig { get; set; } = new JsonConfig();
//    public static bool UseAirtable { get; set; } = true; // Default to true, can be overridden by appsettings.json
//    public static bool UseJson { get; set; } = false; // Default to false, can be overridden by appsettings.json
    public static bool UseMySql { get; set; } = false; // Default to false, can be overridden by appsettings.json
//    public static bool UseArchives { get; set; } = false; // Default to false, can be overridden by appsettings.json

    public static GlobalData GlobalData { get; set; } = new GlobalData();

    public static List<Volunteer> ActiveVolunteers => Volunteers.Where(n => n.IsActive).ToList();

    public static bool IsLoadingBeaches { get; set; } = false;
    public static bool AllGlobalsLoaded { get; set; } = false;
    // Callback/event to signal when beaches have been loaded
    public static event Action<IEnumerable<BeachData>>? BeachesLoaded;
    // Callback/event to signal when selected beach has been changed
    public static event Action<BeachData?>? SelectedBeachChanged;
    public static event Action<bool>? ActiveFilterChanged;
    // Callback/event to signal when a survey has been loaded
    public static event Action<bool>? SurveyLoaded;

    public static List<BeachData>? Beaches { get => StaticData.GlobalData.Beaches;  set => StaticData.GlobalData.Beaches = value; }

    public static List<SurveyBase>? Surveys { get { return StaticData.GlobalData.Surveys; } set => StaticData.GlobalData.Surveys = value; }

    //private static List<BeachEvent>? _beachEventsx;
    //public static List<BeachEvent>? BeachEventsx { get { return _beachEventsx; } set => _beachEventsx = value; }

    public static List<Volunteer> Volunteers { get { return StaticData.GlobalData!.Volunteers; } set => StaticData.GlobalData!.Volunteers = value; }

    public static List<Species> Species { get { return StaticData.GlobalData.Species; } set => StaticData.GlobalData.Species = value; }
    
    // Use to autocomplete notes during data collection
    public static List<String> QuadratNotes { get { return StaticData.GlobalData.QuadratNotes; } set => StaticData.GlobalData.QuadratNotes = value; }

    public static List<CityState> CityStates { get { return StaticData.GlobalData.CityStates; } set => StaticData.GlobalData.CityStates = value; }

    public static List<LookupTable> LookupTables { get { return StaticData.GlobalData.LookupTables; } set => StaticData.GlobalData.LookupTables = value; }

    private static string  _editor = String.Empty;
    public static string Editor { get { return _editor; } set => _editor = value; }

    public static string _editReason = String.Empty;
    public static string EditReason { get { return _editReason; } set => _editReason = value; }

    public static BeachData? SelectedBeach { get; set; } = null;

    public static async Task PreLoadGlobalsAsync()
    {
        
        IsLoadingBeaches = true;
        if (DataSourceConfig is null)
        {
            throw new InvalidOperationException("DataSourceConfig is not set.");
        }
        //if (UseArchives)
        //{
        //    StaticData.GlobalData = new GlobalData();
        //    Archiver archiver = new Archiver();
        //    await archiver.LoadGlobalsFromGoogle();
        //    IsLoadingBeaches = false;
        //    AllGlobalsLoaded = true;
        //}
        if (Beaches is not null && Beaches.Any())
        {
            SelectedBeach = SelectedBeach is null ? Beaches!.OrderBy(b=>b.BeachName).FirstOrDefault() : SelectedBeach;
            IsLoadingBeaches = false;
        }
        else
        {
            try
            {
                // Read beach data from MySQL or API based on configuration
                Beaches = await BeachDataCrud.ReadAllBeachDataAsync(DataSourceConfig);
                Surveys = await SurveyCrud.ReadAllSurveyRecordsAsync(DataSourceConfig);
            }
            catch (Exception ex)
            {
                TraceLogger.LogErrorAuto($"Error reading Beaches: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error reading Beaches: {ex.Message}");
            }
        }
        TraceLogger.LogWarningAuto($"Finished loading beaches {Beaches.Count} and surveys {Surveys.Count}");
        if (Volunteers is null || !Volunteers.Any())
        {
            try
            {
                Volunteers = await VolunteersCrud.ReadAllVolunteersAsync(DataSourceConfig);
            }
            catch (Exception ex)
            {
                TraceLogger.LogErrorAuto($"Error reading Volunteers: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error reading Volunteers: {ex.Message}");
            }
        }
        TraceLogger.LogWarningAuto($"Finished loading volunteers {Volunteers.Count}");
        if (Species is null || !Species.Any())
        {
            try
            {
                Species = await SpeciesCrud.ReadAllSpeciesAsync(DataSourceConfig);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading Species: {ex.Message}");
            }
        }
        if (QuadratNotes is null || !QuadratNotes.Any())
        {
            try
            {
                QuadratNotes = new List<string>();
               // QuadratNotes = await SpeciesCrud.ReadAllSpeciesNotes(DataSourceConfig);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading Species Notes: {ex.Message}");
            }
        }
        if (CityStates is null || !CityStates.Any())
        {
            try
            {
                CityStates.Clear();
                CityStates = await CityStateCrud.ReadAllCityStatesAsync(DataSourceConfig);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading City States: {ex.Message}");
            }
        }
        if (LookupTables is null || !LookupTables.Any())
        {
            try
            {
                LookupTables.Clear();
                LookupTables = await LookupTableCrud.ReadAllLookupTablesAsync(DataSourceConfig);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading Lookup Tables: {ex.Message}");
            }
        }
        IsLoadingBeaches = false;
        AllGlobalsLoaded = true;
        BeachesLoaded?.Invoke(Beaches ?? new List<BeachData>());
        return;
    }
    public static void FinishLoadingGlobals()
    {
        if (Beaches is not null && Beaches.Any())
        {
            SelectedBeach = SelectedBeach is null ? Beaches.OrderBy(b => b.BeachName).FirstOrDefault() : SelectedBeach;
            BeachesLoaded?.Invoke(Beaches);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("No beaches found in the data source.");
        }
    }
    public static void SetSelectedBeach(BeachData beach)
    {
        SelectedBeach = beach ?? SelectedBeach;
        SelectedBeachChanged?.Invoke(SelectedBeach);

    }

    public static void SetActiveFilter(bool activeonly)
    {
        ActiveFilterChanged?.Invoke(activeonly ? true : false);
    }

    public static void FinishLoadingSurvey(bool isSurveyLoaded)
    {
        if (StaticData.SurveyLoaded is not null)
        {
            StaticData.SurveyLoaded?.Invoke(isSurveyLoaded);
        }
    }
}
