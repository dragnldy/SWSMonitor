using Avalonia.Threading;
using DataLibrary.Crud;
using Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SWSMonitor.ViewModels;

public class SpeciesObservation : ReactiveObject
{
    private string? _selectedSpeciesItem = null;
    public string? SelectedSpeciesItem
    {
        get => _selectedSpeciesItem;
        set => this.RaiseAndSetIfChanged(ref _selectedSpeciesItem, value);
    }

    private bool _isPlaceHolder = false;
    public bool IsPlaceHolder
    {
        get => _isPlaceHolder;
        set
        {
            this.RaiseAndSetIfChanged(ref _isPlaceHolder, value);
            CanDelete = !_isPlaceHolder;
        }
    }

    private bool _canDelete = true;
    public bool CanDelete
    {
        get => !_isPlaceHolder;
        set => this.RaiseAndSetIfChanged(ref _canDelete, value);
    }

    private string? _species;
    public string? Species
    {
        get => _species;
        set => this.RaiseAndSetIfChanged(ref _species, value);
    }   

    private string?  _notes = string.Empty;
    public string? Notes
    {
        get => _notes;
        set {
            if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(Species))
            {
                value = string.Empty;
            }
            this.RaiseAndSetIfChanged(ref _notes, value); }
    }
    private string? _commonNameOrDescription = string.Empty;
    public string? CommonNameOrDescription
    {
        get => _commonNameOrDescription;
        set => this.RaiseAndSetIfChanged(ref _commonNameOrDescription, value);
    }

    public int? SpeciesLinkId { get; set; } = -1;

    public SpeciesObservation()
    {
        this.PropertyChanged += SpeciesObservation_PropertyChanged;
    }

    private void SpeciesObservation_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName.Equals(nameof(Species)))
        {
            this.RaisePropertyChanged(nameof(CommonNameOrDescription));
            this.RaisePropertyChanged(nameof(Notes));
        }
    }

    internal void ResetSpecies()
    {
        throw new NotImplementedException();
    }
}
public class SpeciesListViewModel : WizardViewModelBase
{
    public static SpeciesListViewModel? Current = null;
    #region CTOR
    public SpeciesListViewModel()
    {
        Current = this;
        // This is just here for design-time support
    }

    public SpeciesListViewModel(ViewModelBase screen)
    {
        Current = this;
        HostScreen = screen;
        PageTitle = "Species List";
        PropertyChanged += SpeciesListViewModel_PropertyChanged;
        ObservedSpecies = new();
        AddAPlaceholder();


    }
    public override void OnNavigatingFrom()
    {
        if (CanEditSurvey)
            SaveChanges();
        base.OnNavigatingFrom();
    }
    public override void OnNavigatingTo()
    {
        _isLoading = true;
        BeachEventBase? eventinfo = InitBeachInfo();
        _isLoading = false;
        base.OnNavigatingTo();
    }
    #endregion CTOR

    #region Load and Save

    string? _originalSpeciesList = null;
    private BeachEventBase? InitBeachInfo()
    {
        var loadedSurvey = (HostScreen as HomeViewModel)?.LoadedSurvey;
        if (loadedSurvey is null) return null;
        BeachEventBase? beachEvent = loadedSurvey!.BeachEvent;

        SetUpCommands(_canGoBack, _canGoNext);

        _originalSpeciesList = NormalizeSpeciesList(beachEvent, noSpeciesId: true);

        ObservedSpecies.Clear();

        List<SpeciesListBase> speciesList = SpeciesListBase.DecodeSpeciesList(beachEvent.SpeciesObserved).ToList();
        foreach (SpeciesListBase obs in speciesList)
        {
            // Filter out any duplicates
            if (ObservedSpecies.Any(n => n.Species.Equals(obs.Species, StringComparison.OrdinalIgnoreCase)
                                      && n.Notes.Equals(obs.Notes, StringComparison.OrdinalIgnoreCase)))
                continue;
            if (string.IsNullOrEmpty(obs.Species))
                continue;

            // Existing data may not have links before
            if (obs.SpeciesLinkId is null || obs.SpeciesLinkId <= 0)
                obs.SpeciesLinkId = LookupSpeciesLinkId(obs.SpeciesLinkId, obs.Species);

            SpeciesObservation speciesObs = new SpeciesObservation()
            {
                Species = obs.Species,
                Notes = obs.Notes,
                SpeciesLinkId = obs.SpeciesLinkId,
                CommonNameOrDescription = LookupCommonName(obs.SpeciesLinkId),
                SelectedSpeciesItem = obs.Species
            };
            ObservedSpecies.Add(speciesObs);
        }
        this.RaisePropertyChanged(nameof(ObservedSpecies));
        SpeciesObservation placeholder = AddAPlaceholder();
        SelectedSpeciesObservation = null;
        SelectedSpeciesObservation = placeholder;
        this.RaisePropertyChanged(nameof(SelectedSpeciesObservation));

        return beachEvent;
    }

    private string? NormalizeSpeciesList(BeachEventBase beachEvent, bool noSpeciesId = true)
    {
        List<SpeciesListBase> speciesList = SpeciesListBase.DecodeSpeciesList(beachEvent.SpeciesObserved).ToList();
        // Normalize the list to ensure consistent formatting 
        string? observedSpeciesString = CleanupString(SpeciesListBase.EncodeSpeciesList(speciesList, noSpeciesId: true));
        return JsonSerializer.Serialize(observedSpeciesString);

    }

    public override void SaveChanges()
    {
        if (!CanEditSurvey) return;

        var loadedSurvey = (HostScreen as HomeViewModel)?.LoadedSurvey;
        if (loadedSurvey is null) return;

        BeachEventBase? beachEvent = loadedSurvey!.BeachEvent;
        if (beachEvent is null) return;

        List<string> savedlist = new();

        foreach (var obs in ObservedSpecies.Where(n => !string.IsNullOrEmpty(n.Species) && !n.IsPlaceHolder))
        {
            savedlist.Add(SpeciesListBase.EncodeSpecies(new SpeciesListBase() { Species = obs.Species, Notes = obs.Notes, SpeciesLinkId = obs.SpeciesLinkId }));
        }
        if (savedlist.Any())
            beachEvent!.SpeciesObserved = CleanupString(string.Join(";", savedlist.OrderBy(n => n)));
        else
            beachEvent!.SpeciesObserved = null;


        string newObservations = NormalizeSpeciesList(beachEvent, noSpeciesId: true);

        if (!_originalSpeciesList.Equals(newObservations))
        {
            loadedSurvey.BeachEvent.SpeciesObserved = newObservations;
            loadedSurvey.SaveRequired.Add(ComponentsToSaveEnum.BeachEvent);
        }
    }

    #endregion Load and Save

    #region Support methods
    private string? CleanupString(string? instring)
    {
        if (string.IsNullOrWhiteSpace(instring))
            return null;
        return instring.Trim();
    }

    private int? LookupSpeciesLinkId(int? speciesLinkId, string species)
    {
        if (speciesLinkId.HasValue && speciesLinkId > 0)
            return speciesLinkId;
        var glossary = StaticData.Species.FirstOrDefault(n => n.ScientificName.Equals(species, StringComparison.OrdinalIgnoreCase));
        if (glossary is not null)
            return glossary.ID;
        return null;
    }

    private string LookupCommonName(int? speciesLinkId)
    {
        if (speciesLinkId.HasValue && speciesLinkId.Value > 0)
            return StaticData.Species.FirstOrDefault(n => n.ID == speciesLinkId.Value)?.CommonNameOrDescription ?? string.Empty;

        return string.Empty;
    }

    private SpeciesObservation AddAPlaceholder()
    {
        SpeciesObservation placeHolder = new SpeciesObservation() { IsPlaceHolder = true, Species = string.Empty, Notes = string.Empty,
            CommonNameOrDescription = "placeholder",
            SelectedSpeciesItem = string.Empty,
            SpeciesLinkId = -1 };
        ObservedSpecies.Add(placeHolder);
        return placeHolder;
    }

    private void SpeciesListViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
    }

    #endregion Support methods

    private bool _loading = true;
    public bool Loading
    {
        get => _loading;
        set => _loading = value;
    }

    private bool _canGoBack = true;
    private bool _canGoNext = true;

    public bool CanEditSurvey
    {
        get => (HostScreen as HomeViewModel)?.CanEditSurvey ?? false;
    }

    public bool UserIsAdmin { get => StaticData.UserRole == AppRoleEnum.Admin; }


    // Searches can either be 'starts with' or 'contains'
    private bool _searchStartsWith = true;
    public bool SearchStartsWith
    {
        get => _searchStartsWith;
        set { this.RaiseAndSetIfChanged(ref _searchStartsWith, value); }
    }

    private int _minimumPrefixCharacters = 2;
    public int MinimumPrefixCharacters
    {
        get => _minimumPrefixCharacters;
        set { this.RaiseAndSetIfChanged(ref _minimumPrefixCharacters, value); }
    }

    private string _actionMessageText = string.Empty;
    public string ActionMessageText
    {
        get => _actionMessageText;
        set { this.RaiseAndSetIfChanged(ref _actionMessageText, value); }
    }

    private bool _isActionPopupOpen = false;
    public bool IsActionPopupOpen
    {
        get => _isActionPopupOpen;
        set { this.RaiseAndSetIfChanged(ref _isActionPopupOpen, value); }
    }

    private bool _isDeletePopupOpen = false;
    public bool IsDeletePopupOpen
    {
        get => _isDeletePopupOpen;
        set { this.RaiseAndSetIfChanged(ref _isDeletePopupOpen, value); }
    }

    private string _errorMessageText = string.Empty;
    public string ErrorMessageText
    {
        get => _errorMessageText;
        set { this.RaiseAndSetIfChanged(ref _errorMessageText, value); }
    }

    private bool _isErrorMessageOpen = false;
    public bool IsErrorMessageOpen
    {
        get => _isErrorMessageOpen;
        set { this.RaiseAndSetIfChanged(ref _isErrorMessageOpen, value); }
    }
    public ObservableCollection<SpeciesObservation> ObservedSpecies { get; set; } = new();

    private SpeciesObservation _selectedSpeciesObservation = null;
    public SpeciesObservation SelectedSpeciesObservation
    {
        get => _selectedSpeciesObservation;
        set
        {
            TraceLogger.LogWarningAuto($"SelectedSpeciesObservation changed to: {value?.Species}");
            this.RaiseAndSetIfChanged(ref _selectedSpeciesObservation, value);
        }
    }

    public Func<string?, CancellationToken, Task<IEnumerable<object>>> SpeciesSearchFunction => SpeciesSearchAsync;


    private async Task<IEnumerable<object>> SpeciesSearchAsync(string searchText, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return new List<string>(); // Return empty if no search text
        }
        IEnumerable<string> results;
        if (SearchStartsWith)
            results = StaticData.Species!.Where(n => n.ScientificName!.StartsWith(searchText, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(n.CommonNameOrDescription) && n.CommonNameOrDescription.StartsWith(searchText, StringComparison.OrdinalIgnoreCase)))
            .Select(s => s.ScientificName).OrderBy(n => n).ToList();
        else
            results = StaticData.Species!.Where(n => n.ScientificName.Contains(searchText, StringComparison.OrdinalIgnoreCase) || 
                (!string.IsNullOrEmpty(n.CommonNameOrDescription) && !n.CommonNameOrDescription.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
                    .Select(s => s.ScientificName).OrderBy(n => n).ToList();
        return results.Where(n=>NotPreviouslyObserved(n));
    }

    private bool NotPreviouslyObserved(string species)
    {
        return !ObservedSpecies.Any(o => o.Species!.Equals(species));
    }

    internal async void GetSpeciesLinkID(SpeciesObservation detail)
    {
        if (detail is null || string.IsNullOrEmpty(detail.Species)) return;

        Species? possibleSpecies = StaticData.Species!.FirstOrDefault(n=>
            n.ScientificName.Equals(detail.Species, StringComparison.OrdinalIgnoreCase));

        if (possibleSpecies is not null)
        {
            detail.SpeciesLinkId = possibleSpecies.ID;
            detail!.CommonNameOrDescription = possibleSpecies.CommonNameOrDescription;
            SelectedSpeciesObservation = detail;
        }
    }


    internal void DeleteSpeciesObservation(SpeciesObservation observation)
    {
        if (observation is not null)
        { 
            SpeciesObservation? obstodelete = ObservedSpecies!.FirstOrDefault(n => n.Species.Equals(observation!.Species, StringComparison.OrdinalIgnoreCase)
                && n.Notes!.Equals(observation!.Notes, StringComparison.OrdinalIgnoreCase));

            if (obstodelete is not null)
            {
                    Dispatcher.UIThread.Invoke(async () =>
                    {
                        if (!obstodelete.IsPlaceHolder)
                        {
                            ObservedSpecies.Remove(obstodelete);
                            SelectedSpeciesObservation = ObservedSpecies.First();
                        }
                        else
                        { 
                            obstodelete.Species = string.Empty;
                        }
                        IsActionPopupOpen = false;
                    });
            }
        }
    }

    internal SpeciesObservation? _detailToRemove = null;

    internal SpeciesObservation? _detailToAdd = null;
    internal bool TestSpeciesFound(SpeciesObservation? detail)
    {
        if (!string.IsNullOrEmpty(detail.Species))
        {
            bool speciesNotFound = !StaticData.Species.Any(n =>
                n.ScientificName.Equals(detail.Species, StringComparison.InvariantCultureIgnoreCase));

            if (speciesNotFound)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    ActionMessageText = $"Species Not Found - [{detail.Species}] - Add it to the glossary?";
                    _detailToAdd = detail;
                    IsActionPopupOpen = true;
                }, DispatcherPriority.Background);
        }
            else
            {
                AddNewSpeciesObservation(detail);
            }
        }
        return true;
    }

    internal bool AddNewSpeciesObservation(SpeciesObservation? detail)
    {
        if (string.IsNullOrEmpty(detail.Species)) return false;

        IsDirty = true;
        if (detail.IsPlaceHolder)
        {
            // need to add a new placeholder
            var placeHolder = AddAPlaceholder();
            detail.IsPlaceHolder = false;
            SelectedSpeciesObservation = placeHolder;
            SpeciesListView.Instance?.ScrollIntoView(SelectedSpeciesObservation);
        }
        GetSpeciesLinkID(detail);
        Dispatcher.UIThread.Post(() =>
        {
            IsActionPopupOpen = false;
        }, DispatcherPriority.Background);

        return true;
    }

    #region Interface Implementations
    // Unique identifier for the routable view model.
    public string UrlPathSegment => "SpeciesListView";

    #endregion Interface Implementations
}
