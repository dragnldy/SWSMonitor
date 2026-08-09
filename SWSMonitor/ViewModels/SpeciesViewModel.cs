using DataLibrary.Crud;
using Models;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SWSMonitor.ViewModels;

public class SpeciesViewModel: ReactiveObject, INotifyDataErrorInfo
{
    public static SpeciesViewModel? Instance = null;
    private GlossariesViewModel? _parentViewModel = GlossariesViewModel.Instance;
    private readonly ErrorsViewModel _errorsViewModel;
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool isDirty = false;
    private bool isLoading = true;

    private bool _isExistingSpecies = false;
    [JsonIgnore]
    public bool IsExistingSpecies
    {
        get => _isExistingSpecies;
        set
        {
            this.RaiseAndSetIfChanged(ref _isExistingSpecies, value);
        }
    }

    private int? _aphiaID = null;
    public int? AphiaID
    {
        get => _aphiaID;
        set
        {
            this.RaiseAndSetIfChanged(ref _aphiaID, value);
            isDirty = true;
        }
    }

    private string? _kingdom = null;
    public string? Kingdom
    {
        get => _kingdom;
        set
        {
            this.RaiseAndSetIfChanged(ref _kingdom, value);
            isDirty = true;
        }
    }

    private string? _phylum = null;
    public string? Phylum
    {
        get => _phylum;
        set
        {
            this.RaiseAndSetIfChanged(ref _phylum, value);
            isDirty = true;
        }
    }

    private string? _subphylum = null;
    public string? Subphylum
    {
        get => _subphylum;
        set
        {
            this.RaiseAndSetIfChanged(ref _subphylum, value);
            isDirty = true;
        }
    }

    private string? _class = string.Empty;
    public string? Class
    {
        get => _class;
        set
        {
            this.RaiseAndSetIfChanged(ref _class, value);
            isDirty = true;
        }
    }

    private string? _order = null;
    public string? Order
    {
        get => _order;
        set
        {
            this.RaiseAndSetIfChanged(ref _order, value);
            isDirty = true;
        }
    }

    private string? _family = null;
    public string? Family
    {
        get => _family;
        set
        {
            this.RaiseAndSetIfChanged(ref _family, value);
            isDirty = true;
        }
    }

    private string? _genus = null;
    public string? Genus
    {
        get => _genus;
        set
        {
            this.RaiseAndSetIfChanged(ref _genus, value);
            isDirty = true;
        }
    }

    [JsonIgnore]
    public bool CanEditScientificName => !IsUsedBySurveys;

    private string _scientificName = string.Empty;
    public string ScientificName
    {
        get => _scientificName;
        set
        {
            if (value is not null)
                value = value.Trim();

            if (!isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(ScientificName));
                if (string.IsNullOrEmpty(value))
                {
                    _errorsViewModel.AddError(nameof(ScientificName), "Scientific Name cannot be blank.");
                }
                else
                {
                    // Check for duplicates
                    var existing = StaticData.Species.FirstOrDefault(n => n.ScientificName.Equals(value, StringComparison.OrdinalIgnoreCase) && n.ID != this.ID);
                    if (existing != null)
                    {
                        _errorsViewModel.AddError(nameof(ScientificName), "Scientific Name already exists. Please choose a different name.");
                    }
                }
            }
            _parentViewModel.CanSave = !HasErrors;
            this.RaiseAndSetIfChanged(ref _scientificName, value);
            isDirty = true;
        }
    }

    private string? _commonNameOrDescription = null;
    public string? CommonNameOrDescription
    {
        get => _commonNameOrDescription;
        set
        {
            this.RaiseAndSetIfChanged(ref _commonNameOrDescription, value);
            isDirty = true;
        }
    }

    private int? _complexityRank = null;
    public int? ComplexityRank
    {
        get => _complexityRank;
        set
        {
            this.RaiseAndSetIfChanged(ref _complexityRank, value);
            isDirty = true;
        }
    }

    public required int ID { get; set; }

    private int? _invasive = null;
    public int? Invasive
    {
        get => _invasive;
        set
        {
            this.RaiseAndSetIfChanged(ref _invasive, value);
            isDirty = true;
        }
    }

    [JsonIgnore]
    public bool IsInvasive
    {
        get => Invasive.HasValue && Invasive.Value == 1;
        set { Invasive = value ? 1 : 0; this.RaisePropertyChanged(nameof(IsInvasive)); }
    }


    private int? _nonNative = null;
    public int? NonNative
    {
        get => _nonNative;
        set
        {
            this.RaiseAndSetIfChanged(ref _nonNative, value);
            isDirty = true;
        }
    }

    [JsonIgnore]
    public bool IsNonNative
    {
        get => NonNative.HasValue && NonNative.Value == 1;
        set {NonNative = value ? 1 : 0; this.RaisePropertyChanged(nameof(IsNonNative)); }   
    }

    private int? _profileData = null;
    public int? ProfileData
    {
        get => _profileData;
        set
        {
            this.RaiseAndSetIfChanged(ref _profileData, value);
            isDirty = true;
        }
    }

    [JsonIgnore]
    public bool UseForProfileData 
    { 
        get => ProfileData.HasValue && ProfileData.Value == 1; 
        set {ProfileData = value ? 1 : 0; this.RaisePropertyChanged(nameof(UseForProfileData)); }
    }

    private int? _usedBySurveys = null;
    public int? UsedBySurveys
    {
        get => _usedBySurveys;
        set
        {
            this.RaiseAndSetIfChanged(ref _usedBySurveys, value);
            isDirty = true;
        }
    }

    [JsonIgnore]
    public bool IsUsedBySurveys 
    { 
        get => UsedBySurveys.HasValue && UsedBySurveys.Value == 1; 
        set { UsedBySurveys = value ? 1 : 0; this.RaisePropertyChanged(nameof(IsUsedBySurveys)); }
    }

    private string? _taxonCommonName = null;
    public string? TaxonCommonName
    {
        get => _taxonCommonName;
        set
        {
            this.RaiseAndSetIfChanged(ref _taxonCommonName, value);
            isDirty = true;
        }
    }

    // from ITIS database, http://www.itis.gov/
  private int? _tSN = null;
    public int? TSN
    {
        get => _tSN;
        set
        {
            this.RaiseAndSetIfChanged(ref _tSN, value);
            isDirty = true;
        }
    }

    #region CTOR
    public SpeciesViewModel()
    {
        SpeciesViewModel.Instance = this;
        _errorsViewModel = new ErrorsViewModel();
        _errorsViewModel.ErrorsChanged += ErrorsViewModel_ErrorsChanged;
        PropertyChanged += SpeciesViewModel_PropertyChanged; ;
    }
    #endregion CTOR

    private void SpeciesViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
    }

    #region Load and Save Species
    public async Task<(bool, Species)> SaveSpecies()
    {
        if (string.IsNullOrEmpty(ScientificName))
            return (false, new Species() { ID = -1, ScientificName = "Failed" });

        // Update existing species
        var existing = StaticData.Species.FirstOrDefault(n => n.ID == ID);
        if (existing != null)
        {
            // Update existing species
            DataHelper.CopyProperties<SpeciesViewModel, Species>(this, existing);
            (bool success, Species updated) = await SpeciesCrud.UpdateOrCreateSpeciesAsync(StaticData.DataSourceConfig, existing);
            isDirty = false;
            return (true, updated);
        }
        else
        {
            Species species = new Species() { ID = 0, ScientificName = ScientificName };
            DataHelper.CopyProperties<SpeciesViewModel, Species>(this, species);
            (bool success, Species created) = await SpeciesCrud.UpdateOrCreateSpeciesAsync(StaticData.DataSourceConfig, species);
            isDirty = false;
            return (true, created);
        }
    }
    public bool LoadTargetSpecies(string scientificName)
    {
        isLoading = true;
        Species species = StaticData.Species.FirstOrDefault(n => n.ScientificName.Equals(scientificName, StringComparison.InvariantCulture));
        if (species == null)
        {
            // Need to clear out existing data
            species = new Species() { ID = -1, ScientificName = scientificName };
            IsExistingSpecies = false;

        }
        else
        {
            // We can edit scientific name only if not used by surveys- otherwise need to update all the data records that reference it
            IsExistingSpecies = species.UsedBySurveys.HasValue && species.UsedBySurveys.Value == 1;
        }
        return LoadTargetSpecies(species);
    }

    public bool LoadTargetSpecies(Species? species, string scientificName = "Fix Me")
    {
        isLoading = true;
        if (species == null)
        {
            // Need to clear out existing data
            species = new Species() { ID = -1, ScientificName = scientificName };
        }
        _errorsViewModel.ClearErrors();

        DataHelper.CopyProperties<Species, SpeciesViewModel>(species, this);
        _parentViewModel?.CanSave = !string.IsNullOrEmpty(ScientificName);
        isLoading = false;
        isDirty = false;
        this.RaisePropertyChanged(nameof(CanEditScientificName));
        return true;
    }
    #endregion Load and Save Species

    #region Search Functions for AutoCompleteBox

    public Func<string?, CancellationToken, Task<IEnumerable<object>>> KingdomSearchFunction => KingdomSearchAsync;
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> PhylumSearchFunction => PhylumSearchAsync;
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> ClassSearchFunction => ClassSearchAsync; 
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> SubPhylumSearchFunction => SubPhylumSearchAsync;
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> OrderSearchFunction => OrderSearchAsync;
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> FamilySearchFunction => FamilySearchAsync;
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> GenusSearchFunction => GenusSearchAsync;

    public bool SearchStartsWith { get; set; } = false;

    private async Task<IEnumerable<object>> KingdomSearchAsync(string searchText, CancellationToken cancellationToken)
    {
        // Only a few kingdoms , so ignore search type
        return await TaxonomySearchAsync("*", "Kingdom");

        //IEnumerable<string?> results;
        //string lookupCategory = "Kingdom";
        //results = StaticData.LookupTables!.Where(n => n.LookupCategory!.Equals(lookupCategory, StringComparison.OrdinalIgnoreCase))
        //    .Select(s => s.LookupValue!).OrderBy(n => n).ToList();
        //return results;
    }
    private async Task<IEnumerable<object>> PhylumSearchAsync(string searchText, CancellationToken cancellationToken)
    {
        return await TaxonomySearchAsync(searchText,"Phylum");
    }
    private async Task<IEnumerable<object>> SubPhylumSearchAsync(string searchText, CancellationToken cancellationToken)
    {
        // Only a few subphyla , so ignore search type
        return await TaxonomySearchAsync("*", "Subphylum");
    }
    private async Task<IEnumerable<object>> ClassSearchAsync(string searchText, CancellationToken cancellationToken)
    {
        return await TaxonomySearchAsync(searchText, "Class");
    }
    private async Task<IEnumerable<object>> OrderSearchAsync(string searchText, CancellationToken cancellationToken)
    {
        return await TaxonomySearchAsync(searchText, "Order");
    }
    private async Task<IEnumerable<object>> FamilySearchAsync(string searchText, CancellationToken cancellationToken)
    {
        return await TaxonomySearchAsync(searchText, "Family");
    }
    private async Task<IEnumerable<object>> GenusSearchAsync(string searchText, CancellationToken cancellationToken)
    {
        return await TaxonomySearchAsync(searchText, "Genus");
    }

    private async Task<IEnumerable<object>> TaxonomySearchAsync(string searchText, string lookupCategory)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return new List<string>(); // Return empty if no search text
        }
        IEnumerable<string?> results;
        if (searchText == "*")
        {
            results = StaticData.LookupTables!.Where(n => n.LookupCategory!.Equals(lookupCategory, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.LookupValue!).OrderBy(n => n).ToList();
            return results;
        }

        if (SearchStartsWith)
            results = StaticData.LookupTables!.Where(n => n.LookupCategory!.Equals(lookupCategory, StringComparison.OrdinalIgnoreCase) &&
                        n.LookupValue!.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
            .Select(s => s.LookupValue!).OrderBy(n => n).ToList();
        else
            results = StaticData.LookupTables!.Where(n => n.LookupCategory!.Equals(lookupCategory, StringComparison.OrdinalIgnoreCase) &&
                      n.LookupValue!.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .Select(s => s.LookupValue!).OrderBy(n => n).ToList();
        return results;
    }
    #endregion Search Functions for AutoCompleteBox

    #region INotifyDataErrorInfo
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public bool HasErrors => _errorsViewModel.HasErrors;

    public IEnumerable GetErrors(string propertyName)
    {
        return _errorsViewModel.GetErrors(propertyName);
    }

    private void ErrorsViewModel_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
    {
        ErrorsChanged?.Invoke(this, e);
        _parentViewModel?.CanSave = !HasErrors;
    }
    #endregion INotifyDataErrorInfo
}