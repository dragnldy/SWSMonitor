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

public class SpeciesDetail : ReactiveObject
{

    public bool CanEdit { get; set; }
    private bool _loading => _parent?._isLoading ?? true;


    private bool _isPlaceHolder = false; // used to flag the blank entry that is placed at the beginning of the list
    public bool IsPlaceHolder
    {
        get => _isPlaceHolder;
        set { this.RaiseAndSetIfChanged(ref _isPlaceHolder, value); this.RaisePropertyChanged(nameof(CanDelete)); }

    }

    // Flag set when no matching species found during search
    private bool _speciesNotFound = false;
    public bool SpeciesNotFound
    {
        get => _speciesNotFound;
        set { this.RaiseAndSetIfChanged(ref _speciesNotFound, value); }
    }

    private string _selectedItem = "";
    public string SelectedItem
    {
        get => _selectedItem;
        set {
            this.RaiseAndSetIfChanged(ref _selectedItem, value); 
        }
    }

    private string originalSpecies = string.Empty;
    internal void ResetSpecies()
    {
        Species = originalSpecies;
    }

    private string _species = string.Empty;
    public string Species
    {
        get => _species;
        set {
            if (!_loading && _species != value && _isPlaceHolder)
            {
                _parent.AddPlaceHolder(false);
                IsPlaceHolder = false;
            }
            this.RaiseAndSetIfChanged(ref _species, value); if (!_loading) IsDirty = true; 
        }
    }

    private string _t1Q1 = string.Empty;
    public string T1Q1
    {
        get => _t1Q1;
        set { this.RaiseAndSetIfChanged(ref _t1Q1, value); if (!_loading) IsDirty = true; }
    }
    private string _t1Q2 = string.Empty;
    public string T1Q2
    {
        get => _t1Q2;
        set { this.RaiseAndSetIfChanged(ref _t1Q2, value); if (!_loading) IsDirty = true; }
    }

    private string _t1Q3 = string.Empty;
    public string T1Q3
    {
        get => _t1Q3;
        set { this.RaiseAndSetIfChanged(ref _t1Q3, value); if (!_loading) IsDirty = true; }
    }

    private string _t2Q1 = string.Empty;
    public string T2Q1
    {
        get => _t2Q1;
        set { this.RaiseAndSetIfChanged(ref _t2Q1, value); if (!_loading) IsDirty = true; }
    }

    private string _t2Q2 = string.Empty;
    public string T2Q2
    {
        get => _t2Q2;
        set { this.RaiseAndSetIfChanged(ref _t2Q2, value); if (!_loading) IsDirty = true; }
    }

    private string _t2Q3 = string.Empty;
    public string T2Q3
    {
        get => _t2Q3;
        set { this.RaiseAndSetIfChanged(ref _t2Q3, value); if (!_loading) IsDirty = true; }
    }

    private string _t3Q1 = string.Empty;
    public string T3Q1
    {
        get => _t3Q1;
        set { this.RaiseAndSetIfChanged(ref _t3Q1, value); if (!_loading) IsDirty = true; }
    }

    private string _t3Q2 = string.Empty;
    public string T3Q2
    {
        get => _t3Q2;
        set { this.RaiseAndSetIfChanged(ref _t3Q2, value); if (!_loading) IsDirty = true; }
    }

    private string _t3Q3 = string.Empty;
    public string T3Q3
    {
        get => _t3Q3;
        set { this.RaiseAndSetIfChanged(ref _t3Q3, value); if (!_loading) IsDirty = true; }
    }

    private string _notes = string.Empty;
    public string Notes
    {
        get => _notes;
        set { this.RaiseAndSetIfChanged(ref _notes, value); if (!_loading) IsDirty = true; }
    }

    private string _qanotes = string.Empty;
    public string QANotes
    {
        get => _qanotes;
        set { this.RaiseAndSetIfChanged(ref _qanotes, value); if (!_loading) IsDirty = true; }
    }

 
    private bool _isDirty = false;
    public bool IsDirty
    {
        get => _isDirty;
        private set
        {
            if (!_loading && value == true)
            {
                _parent.IsDirty = true;
                this.RaiseAndSetIfChanged(ref _isDirty, value);
            }
        }
    }

    public bool CanDelete => CanEdit && !IsPlaceHolder;

    private QuadratViewModel _parent;
    public SpeciesDetail(string species, string notes, bool canEdit, bool isPlaceHolder, QuadratViewModel parent)
    {
        _parent = parent;
        Species = species;
        originalSpecies = species;
        Notes = notes;
        CanEdit = canEdit;
        _isPlaceHolder = isPlaceHolder;
    }
}

public class QuadratViewModel : WizardViewModelBase, IActivatableViewModel
{
    // Unique identifier for the routable view model.
    public string UrlPathSegment => "QuadratView";

    private bool _canGoBack = true;
    private bool _canGoNext = true;

    public bool CanEditSurvey
    {
        get => (HostScreen as HomeViewModel)?.CanEditSurvey ?? false;
    }

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

    public ObservableCollection<SpeciesDetail> SpeciesDetails { get; set; } = new ObservableCollection<SpeciesDetail>();

    private SpeciesDetail _selectedDetail = null;
    public SpeciesDetail SelectedDetail
    {
        get => _selectedDetail;
        set { this.RaiseAndSetIfChanged(ref _selectedDetail, value); }
    }
    private int _selectedIndex = -1;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set { this.RaiseAndSetIfChanged(ref _selectedIndex, value); }
    }
    /// <summary>
    /// The Title of this page
    /// </summary>
    public static string Title => "Quadrats";

    public ViewModelActivator Activator { get; } = new ViewModelActivator();

    public QuadratViewModel()
    {
        // This is just here for design-time support
    }
    public QuadratViewModel(ViewModelBase screen)
    {
        HostScreen = screen;
        PageTitle = "Quadrats";
        PropertyChanged += QuadratViewModel_PropertyChanged;
        Activator = new ViewModelActivator();
    }

    public override void OnNavigatingTo()
    {
        SetUpCommands(_canGoBack, _canGoNext);
        OnActivated();
        base.OnNavigatingTo();
    }

    public override void OnNavigatingFrom()
    {
        if (CanEditSurvey)
            SaveChanges();
        base.OnNavigatingFrom();
    }

    private void QuadratViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_isLoading || e.PropertyName == nameof(IsDirty))
            return;

        switch (e.PropertyName)
        {
            case nameof(SpeciesDetails):
                break;

        }
    }


    string? _originalQuadratDetails = null;
    private void OnActivated()
    {
        IsDirty = false;
        _originalQuadratDetails = SaveNormalizedQuadrats();
        InitSpeciesInfo();
    }

    private string? SaveNormalizedQuadrats()
    {
        var loadedSurvey = (HostScreen as HomeViewModel)!.LoadedSurvey;
        if (loadedSurvey is null) return null;

        if (loadedSurvey!.QuadratEntries == null)
        {
            loadedSurvey.QuadratEntries = new List<QuadratBase>();
        }

        // Normalize the strings
        List<string> originalDetails = new();
        foreach (QuadratBase entry in loadedSurvey.QuadratEntries)
        {
            IEnumerable<QuadratDetail> quadratList = QuadratEntry.DecodeQuadratDetailList(entry.QuadratDetails);
            entry.QuadratDetails = CleanupString(QuadratDetail.EncodeQuadratDetails(quadratList));
            if (!string.IsNullOrEmpty(entry.QuadratDetails))
                originalDetails.Add(entry.QuadratDetails);
        }

        return JsonSerializer.Serialize(originalDetails);
    }

    private string? CleanupString(string? instring)
    {
        if (string.IsNullOrWhiteSpace(instring))
            return null;
        return instring.Trim();
    }

    #region Load Method
    private void InitSpeciesInfo()
    {
        _isLoading = true;
        var loadedSurvey = (HostScreen as HomeViewModel)!.LoadedSurvey;
        if (loadedSurvey!.QuadratEntries == null)
        {
            loadedSurvey.QuadratEntries = new List<QuadratBase>();
        }
        SpeciesDetails.Clear();

        foreach (var qe in loadedSurvey.QuadratEntries.OrderBy(qe=>qe.QuadratID))
        {
            List<QuadratDetail> details = QuadratEntry.DecodeQuadratDetailList(qe.QuadratDetails).ToList();
            foreach (var qd in details)
            {
                string speciesObs = qd.Species;
                string quadratNotes = string.IsNullOrEmpty(qd.QuadratNotes) ? string.Empty : qd.QuadratNotes;

                SpeciesDetail detail = SpeciesDetails.FirstOrDefault(sd => 
                    sd.Species.Equals(speciesObs, StringComparison.InvariantCultureIgnoreCase) &&
                    sd.Notes.Equals(quadratNotes, StringComparison.InvariantCultureIgnoreCase));

                if (detail == null)
                {
                    detail = new SpeciesDetail(speciesObs, quadratNotes,CanEditSurvey, false, this);
                    SpeciesDetails.Add(detail);
                }

                string formattedCount = FormatQuadratCount(qd.ActualNumber, qd.PercentObserved);
                switch (qe.QuadratID)
                {
                    case (int)TideTypeEnum.Tp1_Q1:
                        detail.T1Q1 = formattedCount;
                        break;
                    case (int)TideTypeEnum.Tp1_Q2:
                        detail.T1Q2 = formattedCount;
                        break;
                    case (int)TideTypeEnum.Tp1_Q3:
                        detail.T1Q3 = formattedCount;
                        break;
                    case (int)TideTypeEnum.T0_Q1:
                        detail.T2Q1 = formattedCount;
                        break;
                    case (int)TideTypeEnum.T0_Q2:
                        detail.T2Q2 = formattedCount;
                        break;
                    case (int)TideTypeEnum.T0_Q3:
                        detail.T2Q3 = formattedCount;
                        break;
                    case (int)TideTypeEnum.Tn1_Q1:
                        detail.T3Q1 = formattedCount;
                        break;
                    case (int)TideTypeEnum.Tn1_Q2:
                        detail.T3Q2 = formattedCount;
                        break;
                    case (int)TideTypeEnum.Tn1_Q3:
                        detail.T3Q3 = formattedCount;
                        break;

                    default:
                        break;
                }
            }
        }
        // The initial blank entry is used to add new observations
        if (CanEditSurvey)
            AddPlaceHolder(true);

        SelectedDetail = SpeciesDetails.FirstOrDefault();

        _isLoading = false;
    }

    private string FormatQuadratCount(int? actualnumber, float? percentobserved)
    {
        string formatted = string.Empty;
        if (actualnumber.HasValue && actualnumber > 0)
        {
            formatted = actualnumber.Value.ToString() + "#";
        }
        else if (percentobserved.HasValue && percentobserved > 0.0)
        {
            formatted = (percentobserved.Value*100).ToString("N1") + "%";
        }
        return formatted;
    }
    #endregion Load Method

    List<QuadratBase> _currentEntries = new();
    Dictionary<int,List<QuadratDetail>> _detailsByQuadrat = new Dictionary<int, List<QuadratDetail>>();

    public override void SaveChanges()
    {
        if (!CanEditSurvey) return;

        // Save changes to the database or data source
        var loadedSurvey = (HostScreen as HomeViewModel)!.LoadedSurvey;

        _detailsByQuadrat.Clear();
        _currentEntries.Clear();

        // Each detail is a pivot table with all quadrats as columns. We need to pivot it back to a table with one entry per quadrat, and the details for that quadrat encoded in the QuadratDetails field
        foreach (var detail in SpeciesDetails)
        {
            if (string.IsNullOrEmpty(detail.Species) || detail.Species.Equals(EMPTY_SPECIES_STRING)) continue;
            FormatQuadratCount(detail.Species, (int)TideTypeEnum.Tp1_Q1, detail.T1Q1, detail.Notes, detail.QANotes);
            FormatQuadratCount(detail.Species, (int)TideTypeEnum.Tp1_Q2, detail.T1Q2, detail.Notes, detail.QANotes);
            FormatQuadratCount(detail.Species, (int)TideTypeEnum.Tp1_Q3, detail.T1Q3, detail.Notes, detail.QANotes);
            FormatQuadratCount(detail.Species, (int)TideTypeEnum.T0_Q1, detail.T2Q1, detail.Notes, detail.QANotes);
            FormatQuadratCount(detail.Species, (int)TideTypeEnum.T0_Q2, detail.T2Q2, detail.Notes, detail.QANotes);
            FormatQuadratCount(detail.Species, (int)TideTypeEnum.T0_Q3, detail.T2Q3, detail.Notes, detail.QANotes);
            FormatQuadratCount(detail.Species, (int)TideTypeEnum.Tn1_Q1, detail.T3Q1, detail.Notes, detail.QANotes);
            FormatQuadratCount(detail.Species, (int)TideTypeEnum.Tn1_Q2, detail.T3Q2, detail.Notes, detail.QANotes);
            FormatQuadratCount(detail.Species, (int)TideTypeEnum.Tn1_Q3, detail.T3Q3, detail.Notes, detail.QANotes);
        }

        foreach (var kvp in _detailsByQuadrat)
        {
            int quadratId = kvp.Key;

            QuadratBase existingbase = loadedSurvey.QuadratEntries.FirstOrDefault(n => n.QuadratID == quadratId);
            List<QuadratDetail> details = kvp.Value;
            string? quadratdetails = CleanupString(QuadratDetail.EncodeQuadratDetails(details));

            if (existingbase is null)
            {
                QuadratBase entry = new QuadratBase
                {
                    ID = 0,
                    SurveyID = loadedSurvey.ID,
                    QuadratID = quadratId,
                    Tide = ParseTide((TideTypeEnum)quadratId, getTidePart: true),
                    Quadrat = ParseTide((TideTypeEnum)quadratId, getTidePart: false),
                    QuadratDetails = quadratdetails
                };
                _currentEntries.Add(entry);
            }
            else
            {
                existingbase.QuadratDetails = quadratdetails;
                _currentEntries.Add(existingbase);
            }
        }

        loadedSurvey.QuadratEntries = _currentEntries.OrderBy(n => n.QuadratID).ToList();

        string? newQuadratDetails = SaveNormalizedQuadrats();
        if (!newQuadratDetails.Equals(_originalQuadratDetails))
            loadedSurvey.SaveRequired.Add(ComponentsToSaveEnum.Quadrat);

    }

    private void FormatQuadratCount(string species, int quadratId, string formattedCount, string notes, string qanotes)
    {
        if (string.IsNullOrEmpty(formattedCount))
            return;

        QuadratDetail quadratdetail = MakeDetailForQuadrat(quadratId, formattedCount, species, notes, qanotes, dense: false);

        if (!_detailsByQuadrat.ContainsKey(quadratId))
        {
            _detailsByQuadrat[quadratId] = new List<QuadratDetail> { quadratdetail };
        }
        else
        {
            _detailsByQuadrat[quadratId].Add(quadratdetail);
        }
    }

    private QuadratDetail MakeDetailForQuadrat(int quadratId, string countin, string species, string notes, string qanotes, bool dense)
    {
        int? count = null;
        float? percent = null;
        if (!string.IsNullOrEmpty(countin))
        {
            if (countin.EndsWith('%'))
                percent = float.Parse(countin.TrimEnd('%')) / 100;
            else if (countin.EndsWith("#"))
                count = int.Parse(countin.TrimEnd('#'));
        }
        QuadratDetail details = new QuadratDetail
        {
            Species = CleanupString(species),
            QuadratNotes = CleanupString(notes),
            QANotes = CleanupString(qanotes),
            ActualNumber = (short?)count,
            PercentObserved =  percent,
            Dense = dense ? 1 : 0
        };
        return details;
    }

    private string ParseTide(TideTypeEnum quadType,bool getTidePart)
    {
        if (!getTidePart) // if they want quadrat- just get last 2 digits
        {
            string quad = quadType.ToString();
            return quad.Substring(quad.Length - 2);
        }
        // Otherwise want the tide component
        switch (quadType)
        {
            case TideTypeEnum.Tp1_Q1:
            case TideTypeEnum.Tp1_Q2:
            case TideTypeEnum.Tp1_Q3:
            case TideTypeEnum.Tp1_Q4:
                return "TideHt1, +1 Ft";
            case TideTypeEnum.T0_Q1:
            case TideTypeEnum.T0_Q2:
            case TideTypeEnum.T0_Q3:
            case TideTypeEnum.T0_Q4:
                return "TideHt1, +1 Ft";
            case TideTypeEnum.Tn1_Q1:
            case TideTypeEnum.Tn1_Q2:
            case TideTypeEnum.Tn1_Q3:
            case TideTypeEnum.Tn1_Q4:
                return "TideHt3, -1 Ft";
            default:
                return "Unknown Quadrat";
        }
    }

    #region SpeciesList
    internal void RemoveSpecies(SpeciesDetail detail)
    {
        if (SpeciesDetails.Contains(detail))
        {
            SpeciesDetails.Remove(detail);
            if (SpeciesDetails.Count() == 0)
            {
                _isLoading = true;
                AddPlaceHolder(true);
            }
            IsDirty = true;
        }
    }

    private string _speciesToAdd = string.Empty;
    public string SpeciesToAdd
    {
        get => _speciesToAdd;
        set => this.RaiseAndSetIfChanged(ref _speciesToAdd, value);
    }

    public Func<string?, CancellationToken, Task<IEnumerable<object>>> SpeciesSearchFunction => SpeciesSearchAsync;
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> NotesSearchFunction => NotesSearchAsync;


    private async Task<IEnumerable<object>> SpeciesSearchAsync(string searchText, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return new List<string>(); // Return empty if no search text
        }
        IEnumerable<string> results;
        if (SearchStartsWith)
            results = StaticData.Species!.Where(n => n.ScientificName!.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
            .Select(s => s.ScientificName).OrderBy(n => n).ToList();
        else
            results = StaticData.Species!.Where(n => n.ScientificName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .Select(s => s.ScientificName).OrderBy(n => n).ToList();

        return results;
    }


    private async Task<IEnumerable<object>> NotesSearchAsync(string? searchText, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return new List<string>(); // Return empty if no search text
        }

        if (SearchStartsWith)
        {
            return StaticData.QuadratNotes!.Where(n => n.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n).ToList();
        }
        return StaticData.QuadratNotes!.Where(n => n.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n).ToList();

    }

    const string EMPTY_SPECIES_STRING = "";
    const int SPECIES_NAME_MINIMUM = 3;
    const int SPECIES_NOTES_MINIMUM = 3;

    internal bool AddPlaceHolder(bool forcenewdetail)
    {
        if (_isLoading && !forcenewdetail)
            return false;
        var detail = new SpeciesDetail(EMPTY_SPECIES_STRING, string.Empty, CanEditSurvey, isPlaceHolder: true, this);
        SpeciesDetails.Add(detail);
        return true;
    }

    internal async Task TestSpeciesFound(SpeciesDetail? detail)
    {
        if (!string.IsNullOrEmpty(detail.Species))
        {
            detail.SpeciesNotFound = !StaticData.Species.Any(n =>
                n.ScientificName.Equals(detail.Species, StringComparison.InvariantCultureIgnoreCase));

            if (detail.SpeciesNotFound)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Not Found",
                    "Species Not Found- Add it to the glossary?",
                    ButtonEnum.YesNo);
                ButtonResult response = await box.ShowAsync();
                if (response == ButtonResult.Yes)
                {
                    bool success = await AddNewSpecies(detail);
                    if (success)
                        detail.SpeciesNotFound = false;
                }
                if (detail.SpeciesNotFound)
                {
                    detail.ResetSpecies();
                    detail.SpeciesNotFound = false;
                }
            }
        }
        //if (!string.IsNullOrEmpty(detail.Notes) && detail.Notes.Length > SPECIES_NOTES_MINIMUM)
        //{
        //    if (!StaticData.QuadratNotes.Any(n =>
        //            n.Equals(detail.Notes, StringComparison.InvariantCultureIgnoreCase)))
        //    {
        //        StaticData.QuadratNotes.Add(detail.Notes);
        //    }
        //}

    }

    internal void MoveToNextDetail(SpeciesDetail? detail)
    {
        if (detail == null) return;

        int currentIndex = SpeciesDetails.IndexOf(detail);
        if (currentIndex < SpeciesDetails.Count - 1)
        {
            SelectedDetail = SpeciesDetails[currentIndex + 1];
            this.RaisePropertyChanged(nameof(SelectedDetail));
        }
    }

    internal void MoveToPreviousDetail(SpeciesDetail? detail)
    {
        if (detail == null) return;

        int currentIndex = SpeciesDetails.IndexOf(detail);
        if (currentIndex > 0)
        {
            SelectedDetail = SpeciesDetails[currentIndex - 1];
            this.RaisePropertyChanged(nameof(SelectedDetail));
        }
    }

    internal async Task<bool> AddNewSpecies(SpeciesDetail detail)
    {
        if (string.IsNullOrEmpty(detail.Species) ||
            detail.Species.Length < SPECIES_NAME_MINIMUM || StaticData.Species.Any(n => n.ScientificName.Equals(detail.Species, StringComparison.InvariantCultureIgnoreCase)))
            return false; // nothing to do

        var loadedSurvey = (HostScreen as HomeViewModel)!.LoadedSurvey;

        Species newSpecies = new Species()
        {
            ID = -1,
            ScientificName = detail.Species,
            UsedBySurveys = 1,
            ProfileData = 1,
              ChangeDate = DateTime.Today,
            ChangeReason = $"Added during data entry for Survey ID: {loadedSurvey!.ID} for Beach: {loadedSurvey.BeachName} Date: {loadedSurvey.SurveyDate}",
        };
        (bool success, Species created) = await SpeciesCrud.UpdateOrCreateSpeciesAsync(StaticData.DataSourceConfig, newSpecies);
        if (success)
        {
            StaticData.Species.Add(created);
        }
        return success;
    }
    #endregion SpeciesList
}
