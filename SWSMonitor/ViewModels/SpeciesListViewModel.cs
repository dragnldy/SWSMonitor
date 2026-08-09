using DataLibrary.Crud;
using Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
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
        set => this.RaiseAndSetIfChanged(ref _isPlaceHolder, value);
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
                Console.Beep();
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
}
public class SpeciesListViewModel : WizardViewModelBase, IActivatableViewModel
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
        string? observedSpeciesString = beachEvent.SpeciesObserved = CleanupString(SpeciesListBase.EncodeSpeciesList(speciesList, noSpeciesId: true));
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
            loadedSurvey.SaveRequired.Add(ComponentsToSaveEnum.BeachEvent);
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
        if (e.PropertyName.Equals(nameof(SelectedSpeciesObservation)))
        {

        }
        if (e.PropertyName.Equals(nameof(SelectedFromDropDown)))
        {
        }
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

    public ObservableCollection<SpeciesObservation> ObservedSpecies { get; set; } = new();

    private SpeciesObservation selectedSpeciesObservation = null;
    public SpeciesObservation SelectedSpeciesObservation
    {
        get => selectedSpeciesObservation;
        set
        {
            this.RaiseAndSetIfChanged(ref selectedSpeciesObservation, value);
        }
    }

    private string _selectedFromDropDown;
    public string SelectedFromDropDown
    {
        get => _selectedFromDropDown;
        set { this.RaiseAndSetIfChanged(ref _selectedFromDropDown, value); }
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
            .Select(s => FormatSpecies(s.ScientificName, s.CommonNameOrDescription)).OrderBy(n => n).ToList();
        else
            results = StaticData.Species!.Where(n => n.ScientificName.Contains(searchText, StringComparison.OrdinalIgnoreCase) || 
                (!string.IsNullOrEmpty(n.CommonNameOrDescription) && !n.CommonNameOrDescription.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
                    .Select(s => FormatSpecies(s.ScientificName, s.CommonNameOrDescription)).OrderBy(n => n).ToList();
        return results.Where(n=>NotPreviouslyObserved(n));
    }

    private bool NotPreviouslyObserved(string speciesAndCommon)
    {
        return !ObservedSpecies.Any(o => FormatSpecies(o.Species, o.CommonNameOrDescription).Equals(speciesAndCommon));
    }

    private string FormatSpecies(string scientificName, string? commonNameOrDescription)
    {
        string combined = scientificName;
        //if (!string.IsNullOrEmpty(commonNameOrDescription))
        //{
        //    combined += $";{commonNameOrDescription}";
        //}
        return combined;
    }

    internal async Task TestSpeciesIfChanged(string? text)
    {
        string[] parts = text.Split(';');
        if (parts.Length > 1)
            text = parts[0];
        if (SelectedSpeciesObservation is null)
        {
            // Should not happen theoretically
            return; 
        }
        if (string.IsNullOrEmpty(text) && SelectedSpeciesObservation.IsPlaceHolder)
        {
            // Nothing to do if placeholder
            return; 
        }
        if (SelectedSpeciesObservation.Species.Equals(text))
        {
            // No change- nothing to do
            return;
            
        }

        IsDirty = true;

        if (SelectedSpeciesObservation.IsPlaceHolder)
        {
            // need to add a new placeholder
            AddAPlaceholder();
            SelectedSpeciesObservation.IsPlaceHolder = false;

        }
        GetSpeciesLinkID(text ?? string.Empty);
//        this.RaisePropertyChanged(nameof(SelectedSpeciesObservation));
    }



    internal async void GetSpeciesLinkID(string text)
    {
        if (!string.IsNullOrWhiteSpace(text) && SelectedSpeciesObservation is not null )
        {
            SelectedSpeciesObservation!.Species = text;
            List<Species> possibleSpecies = StaticData.Species.Where(n=>n.ScientificName.Equals(text, StringComparison.OrdinalIgnoreCase)).ToList();
            if (possibleSpecies.Count == 1)
            {
                SelectedSpeciesObservation!.Species = text;
                SelectedSpeciesObservation!.SpeciesLinkId = possibleSpecies.First().ID;
                SelectedSpeciesObservation!.CommonNameOrDescription = possibleSpecies.First().CommonNameOrDescription;
            }
            else if (possibleSpecies.Count == 0)
            {
                // If we get here, species not found
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Not Found",
                    "Species Not Found- Add it to the glossary?",
                    ButtonEnum.YesNo);
                ButtonResult response = await box.ShowAsync();
                if (response != ButtonResult.Yes)
                {
                    SelectedSpeciesObservation!.SpeciesLinkId = 0;
                    SelectedSpeciesObservation!.CommonNameOrDescription = string.Empty;
                    SelectedSpeciesObservation.Species = string.Empty;
                    SelectedSpeciesObservation.SelectedSpeciesItem = string.Empty;
                    return;
                }
                int newID = await AddSpeciesFound(text);
                SelectedSpeciesObservation!.SpeciesLinkId = newID;
                SelectedSpeciesObservation!.CommonNameOrDescription = string.Empty;
                SelectedSpeciesObservation!.SelectedSpeciesItem = text;
                if (newID == 0)
                {
                    SelectedSpeciesObservation.Species = string.Empty;
                    SelectedSpeciesObservation.SelectedSpeciesItem = string.Empty;
                }
            }
        }
    }

    internal async Task<int> AddSpeciesFound(string speciesToAdd)
    {
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
            return created.ID;
        }
        return 0;
    }


    internal void DeleteSpeciesObservation(SpeciesObservation observation)
    {
        if (ObservedSpecies.Contains(observation))
        {
            ObservedSpecies.Remove(observation);
            SelectedSpeciesObservation = ObservedSpecies.First();
        }
    }

    #region Interface Implementations
    // Unique identifier for the routable view model.
    public string UrlPathSegment => "SpeciesListView";

    public ViewModelActivator Activator { get; } = new ViewModelActivator();
    #endregion Interface Implementations
}
