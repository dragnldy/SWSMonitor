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

public class SpeciesReadOnlyViewModel: ReactiveObject
{
    public static SpeciesReadOnlyViewModel? Instance = null;
    private GlossariesViewModel? _parentViewModel = GlossariesViewModel.Instance;

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
        }
    }

    private string? _kingdom = null;
    public string? Kingdom
    {
        get => _kingdom;
        set
        {
            this.RaiseAndSetIfChanged(ref _kingdom, value);
        }
    }

    private string? _phylum = null;
    public string? Phylum
    {
        get => _phylum;
        set
        {
            this.RaiseAndSetIfChanged(ref _phylum, value);
        }
    }

    private string? _subphylum = null;
    public string? Subphylum
    {
        get => _subphylum;
        set
        {
            this.RaiseAndSetIfChanged(ref _subphylum, value);
        }
    }

    private string? _class = string.Empty;
    public string? Class
    {
        get => _class;
        set
        {
            this.RaiseAndSetIfChanged(ref _class, value);
        }
    }

    private string? _order = null;
    public string? Order
    {
        get => _order;
        set
        {
            this.RaiseAndSetIfChanged(ref _order, value);
        }
    }

    private string? _family = null;
    public string? Family
    {
        get => _family;
        set
        {
            this.RaiseAndSetIfChanged(ref _family, value);
        }
    }

    private string? _genus = null;
    public string? Genus
    {
        get => _genus;
        set
        {
            this.RaiseAndSetIfChanged(ref _genus, value);
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

            this.RaiseAndSetIfChanged(ref _scientificName, value);
        }
    }

    private string? _commonNameOrDescription = null;
    public string? CommonNameOrDescription
    {
        get => _commonNameOrDescription;
        set
        {
            this.RaiseAndSetIfChanged(ref _commonNameOrDescription, value);
        }
    }

    private int? _complexityRank = null;
    public int? ComplexityRank
    {
        get => _complexityRank;
        set
        {
            this.RaiseAndSetIfChanged(ref _complexityRank, value);
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
        }
    }

    #region CTOR
    public SpeciesReadOnlyViewModel()
    {
        SpeciesReadOnlyViewModel.Instance = this;
    }
    #endregion CTOR

    private void SpeciesReadOnlyViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
    }

    #region Load and Save Species
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
        DataHelper.CopyProperties<Species, SpeciesReadOnlyViewModel>(species, this);
        isLoading = false;
        return true;
    }
    #endregion Load and Save Species

}