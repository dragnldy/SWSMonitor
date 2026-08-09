using DataLibrary.Crud;
using Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SWSMonitor.ViewModels;

public class Detail : ReactiveObject
{
    private bool loading = true;
    public bool CanEdit { get; set; }

    private string _species = string.Empty;
    public string Species
    {
        get => _species;
        set { this.RaiseAndSetIfChanged(ref _species, value); IsDirty = true; }
    }

    private string _notes = string.Empty;
    public string Notes
    {
        get => _notes;
        set { this.RaiseAndSetIfChanged(ref _notes, value); IsDirty = true; }
    }
    WizardViewModelBase _parent;

    private bool _isDirty = false;
    public bool IsDirty
    {
        get => _isDirty;
        private set
        {
            if (!loading && value == true)
            {
                _parent.IsDirty = true;
                this.RaiseAndSetIfChanged(ref _isDirty, value);
            }
        }
    }

    public Detail(string species, string notes , WizardViewModelBase parent, bool canEdit)
    {
        Species = species;
        Notes = notes;
        CanEdit = canEdit;
        _parent = parent;
        loading = false;
    }

}

public enum BeachSurfaceEnum
{
    GroundShellDebris,
    ClaySilt,
    Sand,
    Gravel,
    Cobbles,
    Boulders,
    Erratics, // on form as large_rocks
}

public class SurfaceDetail : ReactiveObject
{
    private bool loading = true;

    public bool CanEdit { get; set; }
    public BeachSurfaceEnum SurfaceType { get; set; }
    public string SurfaceTypeName { get; set; } = string.Empty;

    public string HelpText { get; set; } = string.Empty;

    private bool _isPresent = false;
    public bool IsPresent
    {
        get => _isPresent;
        set { this.RaiseAndSetIfChanged(ref _isPresent, value); if (!value) { Percentage70 = false; } IsDirty = true; }
    }

    private bool _percentage70 = false;
    public bool Percentage70
    {
        get => _percentage70;
        set { this.RaiseAndSetIfChanged(ref _percentage70, value); if (value) { IsPresent = true; } IsDirty = true; }
    }
    WizardViewModelBase _parent;

    private bool _isDirty = false;
    public bool IsDirty
    {
        get => _isDirty;
        private set
        {
            if (!loading && value == true)
            {
                _parent.IsDirty = true;
                this.RaiseAndSetIfChanged(ref _isDirty, value);
            }
        }
    }

    public SurfaceDetail(BeachSurfaceEnum surfacetype, string name, bool isPresent, bool percentage70, string helptext, WizardViewModelBase parent, bool canEdit)
    {
        SurfaceType = surfacetype;
        SurfaceTypeName = name;
        IsPresent = isPresent;
        Percentage70 = percentage70;
        CanEdit = canEdit;
        HelpText = helptext;
        _parent = parent;
        loading = false;
    }
}

public class ProfileViewModel : WizardViewModelBase, IRoutableViewModel, IActivatableViewModel, INotifyDataErrorInfo
{
    public static ProfileViewModel? Instance = null;
    private readonly ErrorsViewModel _errorsViewModel;

    public ViewModelActivator Activator { get; } = new ViewModelActivator();

    // Unique identifier for the routable view model.
    public string UrlPathSegment => "ProfileView";

    private bool _canGoBack = true;
    private bool _canGoNext = true;

    public bool CanEditSurvey
    {
        get => (HostScreen as HomeViewModel)?.CanEditSurvey ?? false;
    }

    private ProfileBase? _profile = null;
    [JsonIgnore]
    public ObservableCollection<SurfaceDetail> SurfaceDetails { get; set; } = new ObservableCollection<SurfaceDetail>();
    [JsonIgnore]
    public ObservableCollection<Detail> Details { get; set; } = new ObservableCollection<Detail>();

    [JsonIgnore]
    private int _maxSections = 100;
    public int MaxSections
    {
        get => _maxSections;
        set => this.RaiseAndSetIfChanged(ref _maxSections, value);
    }
    [JsonIgnore]
    private string _totalSections = "100";
    public string TotalSections
    {
        get => _totalSections;
        set => this.RaiseAndSetIfChanged(ref _totalSections, value);
    }

    /// <summary>
    /// The Title of this page
    /// </summary>
    public static string Title => "Profiles";


    #region Profile Properties
    private int? _entryNo = 1;
    public int? EntryNo
    {
        get => _entryNo;
        set
        {
            if (value is not null)
            {

                if (value > 0 && value <= MaxSections)
                {
                    this.RaiseAndSetIfChanged(ref _entryNo, value);
                }
                else
                {
                    // Don't allow invalid value
                    this.RaisePropertyChanged(nameof(EntryNo));
                }
            }
        
        }
    }

    private int GetInt(string value)
    {
        if (int.TryParse(value, out int result))
            return result;
        return 0;
    }

    private double GetDouble(string value)
    {
        if (double.TryParse(value, out double result))
            return result;
        return 0.0;
    }

    private string _length = "10";
    public string Length
    {
        get => _length;
        set
        {
            if (value is not null)
                value = value.Trim();

            if (!_isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(Length));
                if (!string.IsNullOrEmpty(value))
                {
                    if (!GoodInteger(value, 0, 50))
                    {
                        _errorsViewModel.AddError(nameof(Length), "Length must be between 0 and 50");
                        value = _length;
                    }
                }
            }
            this.RaiseAndSetIfChanged(ref _length, value);
        }
    }

    private string _surveyReading = "0.0";
    public string SurveyReading
    {
        get => _surveyReading;
        set {
            if (value is not null)
                value = value.Trim();

            if (!_isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(SurveyReading));
                if (!string.IsNullOrEmpty(value))
                {
                    if (!GoodDouble(value, -20, 20))
                    {
                        _errorsViewModel.AddError(nameof(SurveyReading), "Survey Reading must be between -20 and 20");
                        value = _surveyReading;
                    }
                }
            }

            this.RaiseAndSetIfChanged(ref _surveyReading, value);
        }
    }

    private bool GoodDouble(string value, int v1, int v2)
    {
        if (string.IsNullOrEmpty(value))
            return true;

        if (double.TryParse(value, out double result))
        {
            return result >= v1 && result <= v2;
        }
        return false;
    }

    private bool GoodInteger(string value, int v1, int v2)
    {
        if (string.IsNullOrEmpty(value))
            return true;

        if (int.TryParse(value, out int result))
        {
            return result >= v1 && result <= v2;
        }
        return false;
    }

    #endregion Profile Properties

    private string? _originalProfileEntries = null;

    #region CTOR
    public ProfileViewModel()
    {
        // Just here for design time support
    }

    public ProfileViewModel(IScreen hostScreen)
    {
        ProfileViewModel.Instance = this;
        _errorsViewModel = new ErrorsViewModel();
        _errorsViewModel.ErrorsChanged += ErrorsViewModel_ErrorsChanged;

        HostScreen = hostScreen;
        PageTitle = "Profile";
        PropertyChanged += ProfileViewModel_PropertyChanged;
        Activator = new ViewModelActivator();

        this.WhenActivated((Action<IDisposable> disposables) =>
        {
            SetUpCommands(_canGoBack, _canGoNext);
            OnActivated();
        });
        this.WhenNavigatingFromObservable()
            .Subscribe(_ =>
            {
                if (CanEditSurvey)
                    SaveChanges();
            });

    }
    #endregion CTOR

    private void ProfileViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isLoading || e.PropertyName == nameof(IsDirty))
            return;
        if (e.PropertyName == nameof(EntryNo))
        {
            SaveChanges();
            InitProfileInfo(EntryNo);
        }
    }

    private void OnActivated()
    {
        IsDirty = false;
        _originalProfileEntries = SaveNormalizedProfiles();
        InitProfileInfo(1);
    }

    private string? SaveNormalizedProfiles()
    {
        var loadedSurvey = (HostScreen as HomeViewModel)!.LoadedSurvey;
        if (loadedSurvey is null) return null;

        if (loadedSurvey!.ProfileEntries == null)
        {
            loadedSurvey.ProfileEntries = new List<ProfileBase>();
        }

        // Normalize the strings
        foreach (ProfileBase entry  in loadedSurvey.ProfileEntries)
        {
            IEnumerable<ProfileDetail> detailList = ProfileEntry.DecodeProfileDetailList(entry!.Details);
            entry.Details = CleanupString(ProfileDetail.EncodeProfileDetails(detailList));

            IEnumerable<ProfileSurfaceDetail> surfaceDetailList = ProfileEntry.DecodeProfileSurfaceDetailList(entry.SurfaceDetails);
            entry.SurfaceDetails = CleanupString(ProfileSurfaceDetail.EncodeProfileSurfaceDetails(surfaceDetailList));
        }
        return JsonSerializer.Serialize(loadedSurvey.ProfileEntries);
    }

    private string? CleanupString(string? instring)
    {
        if (string.IsNullOrWhiteSpace(instring))
            return null;
        return instring.Trim();
    }



    #region Load Method
    private void InitProfileInfo(int? entryNo)
    {
        _isLoading = true;
        var loadedSurvey = (HostScreen as HomeViewModel)!.LoadedSurvey;
        if (loadedSurvey!.ProfileEntries == null)
        {
            loadedSurvey.ProfileEntries = new List<ProfileBase>();
        }
        _profile = loadedSurvey.ProfileEntries.FirstOrDefault(n => n.EntryNo == entryNo);
        if (_profile == null)
        {
            if (entryNo < -0)
                entryNo = 1;
            _profile = new ProfileEntry
            {
                SurveyID = loadedSurvey.ID,
                EntryNo = entryNo,
                Length = entryNo!.Value == 1 ? 10 : 0,
                SurveyReading = 0.0
            };
            loadedSurvey.ProfileEntries.Add(_profile);
        }

        EntryNo = _profile.EntryNo.HasValue ? _profile.EntryNo.Value : 1;
        Length = _profile.Length.HasValue ? _profile.Length.Value.ToString("0") : "10";
        SurveyReading = _profile.SurveyReading.HasValue ? _profile.SurveyReading.Value.ToString("0.0") : "0.0";

        // If can edit, then we will allow selection of one more than existing so can add
        MaxSections = loadedSurvey.ProfileEntries.Max(n=>n.EntryNo.Value);
        if (CanEditSurvey)
        {
            MaxSections += 1;
        }

        Details.Clear();
        foreach (var detail in ProfileEntry.DecodeProfileDetailList(_profile.Details))
        {
            if (string.IsNullOrEmpty(detail.Species))
                continue;
            Details.Add(new Detail(detail.Species, detail.Notes ?? string.Empty, this, CanEditSurvey));
        }

        IEnumerable<ProfileSurfaceDetail> storedDetails = ProfileEntry.DecodeProfileSurfaceDetailList(_profile.SurfaceDetails);

        SurfaceDetails.Clear();
        foreach (BeachSurfaceEnum surfaceType in Enum.GetValues(typeof(BeachSurfaceEnum)))
        {
            string surfaceName = string.Empty;
            string helpText = string.Empty;
            switch (surfaceType)
            {
                case BeachSurfaceEnum.GroundShellDebris:
                    surfaceName = "Ground Shell Debris";
                    helpText = "Includes small shell fragments, organic debris, and other fine materials found on the beach surface.";
                    break;
                case BeachSurfaceEnum.ClaySilt:
                    surfaceName = "Clay/Silt";
                    helpText = "A mixture of clay and silt particles, often found in wetland areas.";
                    break;
                case BeachSurfaceEnum.Sand:
                    surfaceName = "Sand";
                    helpText = "(.002\" - .08\") Fine granular material, typically composed of quartz or other minerals.";
                    break;
                case BeachSurfaceEnum.Gravel:
                    surfaceName = "Gravel";
                    helpText = "(/08\" - 2\" ) Coarse material, typically composed of rounded stones.";
                    break;
                case BeachSurfaceEnum.Cobbles:
                    surfaceName = "Cobbles";
                    helpText = "(2\" - 10\") Larger, rounded stones often found in riverbeds or coastal areas.";
                    break;
                case BeachSurfaceEnum.Boulders:
                    surfaceName = "Boulders";
                    helpText = "(> 10\") Very large stones, often found in mountainous or rocky areas.";    
                    break;
                case BeachSurfaceEnum.Erratics:
                    surfaceName = "Erratics";
                    helpText = "(> 3') Large, angular rocks that may have been transported by glacial activity.";
                    break;
                default:
                    surfaceName = surfaceType.ToString();
                    helpText = "Unknown surface type.";
                    break;
            }

            var psd = storedDetails.FirstOrDefault(n =>
                n.BeachSurface.Equals(surfaceName, StringComparison.InvariantCultureIgnoreCase));
            if (psd != null)
                SurfaceDetails.Add(new SurfaceDetail(surfaceType, surfaceName, true, psd.IsG70percent, helpText, this, CanEditSurvey));
            else
                SurfaceDetails.Add(new SurfaceDetail(surfaceType, surfaceName, false, false, helpText, this, CanEditSurvey));
        }
        _isLoading = false;
        TotalSections = $" /{loadedSurvey.ProfileEntries.Count}";
    }

    #endregion Load

    #region Save
    public override void SaveChanges()
    {
        if (!CanEditSurvey) return;

        var loadedSurvey = (HostScreen as HomeViewModel)!.LoadedSurvey;
        if (loadedSurvey!.ProfileEntries is null)
        {
            loadedSurvey.ProfileEntries = new List<ProfileBase>();
        }
        if (_profile is null)
        {
            throw new Exception("No profile entry to save");
        }

        _profile.Length = GetInt(Length);
        _profile.SurveyReading = GetDouble(_surveyReading);
        
        SaveProfileDetails(loadedSurvey);

        string? currentProfileEntries = SaveNormalizedProfiles();

        if (!currentProfileEntries.Equals(_originalProfileEntries))
            loadedSurvey.SaveRequired.Add(ComponentsToSaveEnum.Profile);
    }


    internal void SaveProfileDetails(DataLibrary.ModelExtensions.Survey loadedSurvey)
    {
        List<ProfileBase> updatedProfiles = new();
        ProfileBase profileEntry = loadedSurvey.ProfileEntries.FirstOrDefault(n => n.EntryNo == _profile.EntryNo)!;
        _profile.Length = GetInt(Length);
        _profile.SurveyReading = GetDouble(_surveyReading);

        if (profileEntry is null)
        {
            profileEntry = new ProfileBase
            {
                SurveyID = loadedSurvey.ID,
                EntryNo = _profile.EntryNo,
                Length = _profile.Length,
                SurveyReading = _profile.SurveyReading
            };
        }
        else
        {
            profileEntry.Length = _profile.Length;
            profileEntry.SurveyReading = _profile.SurveyReading;
        }

        updatedProfiles.Add(profileEntry);

        List<ProfileDetail> newDetails = new();
        // Add updated species details
        foreach (var detail in Details)
        {
            if (!string.IsNullOrEmpty(detail.Species))
            {
                ProfileDetail pd = new ProfileDetail
                {
                    Species = detail.Species,
                    Notes = detail.Notes,
                };
                newDetails.Add(pd);
            }
        }
        profileEntry.Details = CleanupString(ProfileDetail.EncodeProfileDetails(newDetails));

        List<ProfileSurfaceDetail> surfaceDetails = new();
        // Add updated surface _currentEntries
        foreach (var surfaceDetail in SurfaceDetails)
        {
            if (surfaceDetail.IsPresent)
            {
                ProfileSurfaceDetail psd = new ProfileSurfaceDetail
                {
                    BeachSurface = surfaceDetail.SurfaceTypeName.ToLower(),
                    IsG70percent = surfaceDetail.Percentage70,
                };
                surfaceDetails.Add(psd);
            }
        }
        profileEntry.SurfaceDetails = CleanupString(ProfileSurfaceDetail.EncodeProfileSurfaceDetails(surfaceDetails));
    }

    #endregion Save

    #region Add & Remove Profiles
    public void RemoveProfile()
    {
        var loadedSurvey = (HostScreen as HomeViewModel)!.LoadedSurvey;
        ProfileBase profile = loadedSurvey!.ProfileEntries!.FirstOrDefault(n => n.EntryNo == EntryNo)!;
        if (profile is null || !profile.EntryNo.HasValue || profile!.EntryNo!.Value == 1)
        {
            Console.Beep();
            return;
        }

        loadedSurvey.ProfileEntries.Remove(profile);
        loadedSurvey.ProfileEntries.ForEach(pe => pe.EntryNo = pe.EntryNo! > profile.EntryNo ? pe.EntryNo! - 1 : pe.EntryNo);

        InitProfileInfo(profile.EntryNo - 1);

    }
    public void AddProfile(bool copyCurrent = false)
    {
        // Insert a new profile after the current one , moving all subsequent entry numbers up by 1
        var loadedSurvey = (HostScreen as HomeViewModel)!.LoadedSurvey;
        ProfileBase profile = loadedSurvey!.ProfileEntries!.FirstOrDefault(n => n.EntryNo == EntryNo)!;
        if (profile is null || !profile.EntryNo.HasValue)
        {
            Console.Beep();
            return;
        }

        int? newEntryNo = EntryNo + 1;

        // Move all subsequent entry numbers up by 1
        loadedSurvey.ProfileEntries.ForEach(pe => pe.EntryNo = pe.EntryNo! > profile.EntryNo ? pe.EntryNo! + 1 : pe.EntryNo);

        SaveProfileDetails(loadedSurvey);

        if (!copyCurrent)
        {
            // Create blank profile info
            SurfaceDetails.Clear();
            Details.Clear();
            _profile = new ProfileEntry
            {
                SurveyID = loadedSurvey.ID,
                EntryNo = newEntryNo,
                Length = profile.Length.HasValue ? profile.Length.Value : 10,
                SurveyReading = 0.0
            };
            loadedSurvey.ProfileEntries.Add(_profile);
        }
        else
        {
            // Copy current profile info
            _profile = new ProfileEntry
            {
                SurveyID = loadedSurvey.ID,
                EntryNo = newEntryNo,
                Length = profile.Length,
                SurveyReading = profile.SurveyReading
            };
            SaveProfileDetails(loadedSurvey);
        }
        InitProfileInfo(newEntryNo);
    }
    #endregion Add & Remove Profiles

    #region SpeciesList
    internal void RemoveDetail(Detail detail)
    {
        if (Details.Contains(detail))
        {
            Details.Remove(detail);
        }
    }

    internal Detail? AddMember(string? species)
    {
        if (!string.IsNullOrEmpty(species))
        {
            var detail = new Detail(species, string.Empty, this, CanEditSurvey);
            Details.Add(detail);
            return detail;
        }
        return null;
    }

    private string _speciesToAdd = string.Empty;
    public string SpeciesToAdd
    {
        get => _speciesToAdd;
        set => this.RaiseAndSetIfChanged(ref _speciesToAdd, value);
    }
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> SpeciesSearchFunction => PopulateSuggestionsAsync;

    private async Task<IEnumerable<object>> PopulateSuggestionsAsync(string? searchText, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return new List<string>(); // Return empty if no search text
        }

        List<string?> allPossibleSuggestions = StaticData.Species!.Where(n => n.ScientificName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .Select(s=>s.ScientificName).Where(aps => !Details.Any(d => d.Species.Equals(aps, StringComparison.InvariantCultureIgnoreCase))).ToList();
        if (!allPossibleSuggestions.Any())
        {
            return new List<string>(); // No matches found
        }

        //if (allPossibleSuggestions.Count() == 1)
        //{
        //        // Auto-select the single match
        //        string singleMatch = allPossibleSuggestions.First();
        //        Detail member = AddMember(singleMatch);
        //        SpeciesToAdd = string.Empty;
        //        return new List<string> { singleMatch };
        //}

        //// Don't return any that are already in the list of selected members
        //IEnumerable<string?> matches = allPossibleSuggestions!.Where(aps => !Details.Any(d => d.Species.Equals(aps, StringComparison.InvariantCultureIgnoreCase)))
        //    .OrderBy(n => n);

        return allPossibleSuggestions.OrderBy(n=>n);
    }
    #endregion SpeciesList

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
        //       _parentViewModel?.CanSave = !HasErrors;
    }

    internal async Task<string> TestSpeciesFound(string speciesToAdd)
    {
        if (StaticData.Species.Any(n => n.ScientificName.Equals(speciesToAdd, StringComparison.InvariantCultureIgnoreCase)))
        {
            if (!Details.Any(d => d.Species.Equals(speciesToAdd, StringComparison.InvariantCultureIgnoreCase)))
                AddMember(speciesToAdd);
            return string.Empty; // already exists
        }
        if (string.IsNullOrEmpty(speciesToAdd) || speciesToAdd.Length<3) 
            return speciesToAdd; // nothing to do

        // If we get here, species not found
        var box = MessageBoxManager.GetMessageBoxStandard(
            "Not Found",
            "Species Not Found- Add it to the glossary?",
            ButtonEnum.YesNo);
        ButtonResult response = await box.ShowAsync();
        if (response != ButtonResult.Yes)
        {
            SpeciesToAdd = string.Empty;
            return speciesToAdd;
        }
        var loadedSurvey = (HostScreen as HomeViewModel)!.LoadedSurvey;

        Species newSpecies = new Species()
        {
            ID = -1,
            ScientificName = speciesToAdd,
            UsedBySurveys = 1,
            ProfileData = 1,
            ChangeDate = DateTime.Today,
            ChangeReason = $"Added during data entry for Survey ID: {loadedSurvey!.ID} for Beach: {loadedSurvey.BeachName} Date: {loadedSurvey.SurveyDate}",
        };
        (bool success, Species created) = await SpeciesCrud.UpdateOrCreateSpeciesAsync(StaticData.DataSourceConfig, newSpecies);
        if (success)
        {
            StaticData.Species.Add(created);
            AddMember(speciesToAdd);
        }
        return speciesToAdd;
    }

    #endregion INotifyDataErrorInfo
}
