using Models;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SWSMonitor.ViewModels;

public class Content : ReactiveObject
{
    private bool loading = true;

    public string? SelectedContentItem
    {
        get => Contents;
        set 
        { 
        }
    }


    private string _contents = string.Empty;
    public string Contents
    {
        get => _contents;
        set { if (value == "*") return;
            bool changed = !loading && value != _contents;
            this.RaiseAndSetIfChanged(ref _contents, value);
            if (changed)
            {
                _parent.RaisePropertyChanged("SelectedContentItem");
            }

        }
    }

    private bool _canEdit = false;
    public bool CanEdit
    {
        get => _canEdit;
        set { this.RaiseAndSetIfChanged(ref _canEdit, value); }
    }

    private bool _isPlaceHolder = false;
    public bool IsPlaceHolder
    {
        get => _isPlaceHolder;
        set { this.RaiseAndSetIfChanged(ref _isPlaceHolder, value); }
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
    public Content(string contents,WizardViewModelBase parent, bool canEdit,  bool isPlaceHolder = false)
    {
        Contents = contents;
        CanEdit = canEdit;
        _parent = parent;
        _isPlaceHolder = isPlaceHolder;
        loading = false;
    }
}
/// <summary>
///  This is our ViewModel for the first page
/// </summary>
public class BeachSettingViewModel : WizardViewModelBase, IActivatableViewModel, INotifyDataErrorInfo
{
    public static BeachSettingViewModel? Instance = null;
    private readonly ErrorsViewModel _errorsViewModel;

    public ViewModelActivator Activator { get; } = new ViewModelActivator();

    // Unique identifier for the routable view model.
    public string UrlPathSegment => "BeachSettingView";
    /// <summary>
    /// The Title of this page
    /// </summary>
    public static string Title => "Beach Setting";


    private bool _canGoBack = true;
    private bool _canGoNext = true;

    [JsonIgnore]
    public bool CanEditSurvey
    {
        get => (HostScreen as HomeViewModel)?.CanEditSurvey ?? false;
    }


    #region CTOR
    public BeachSettingViewModel()
    {
        // This is just here for design-time support
    }
    public BeachSettingViewModel(ViewModelBase screen)
    {
        BeachSettingViewModel.Instance = this;
        _errorsViewModel = new ErrorsViewModel();
        _errorsViewModel.ErrorsChanged += ErrorsViewModel_ErrorsChanged;

        HostScreen = screen;
        PageTitle = "Beach Setting";

        PropertyChanged += BeachSettingViewModel_PropertyChanged;

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


    #endregion CTOR

    private void BeachSettingViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isLoading)
            return;
        switch (e.PropertyName)
        {
            case "SelectedContentItem":
                TestContentIfChanged();
                break;

        }
    }

    #region Control Properties and Collections

    [JsonIgnore]
    public ObservableCollection<string> BulkheadMaterials { get; } = new ObservableCollection<string>()
    {
        "None",
        "Wood",
        "Cement",
        "Rock",
        "Other"
    };

    [JsonIgnore]
    private string _selectedMaterial = string.Empty;
    public string SelectedMaterial
    {
        get => _selectedMaterial;
        set { this.RaiseAndSetIfChanged(ref _selectedMaterial, value); }
    }

    [JsonIgnore]
    private Content _selectedContents = null!;
    public Content SelectedContents
    {
        get => _selectedContents;
        set { this.RaiseAndSetIfChanged(ref _selectedContents, value); }
    }

    #endregion Control Properties

    #region Save and Load
    private string _originalBeachEvent = string.Empty;

    private void OnActivated()
    {
        _isLoading = true;
        BeachEventBase? eventinfo = InitBeachInfo();
        InitBackshoreInfo();

        // Get all the beach event info for beach settings

        _isLoading = false;
    }

    private BeachEventBase? InitBeachInfo()
    {
        var loadedSurvey = (HostScreen as HomeViewModel)!.LoadedSurvey;

        if (loadedSurvey!.BeachEvent is null)
        {
            loadedSurvey.BeachEvent = new BeachEvent(0l, loadedSurvey.ID, loadedSurvey.BeachName, loadedSurvey.SurveyDate);
        }

        BeachEventBase? eventinfo = loadedSurvey.BeachEvent;

        // We need to normalize the treatment of empty string fields- some were treated as null and some as empty string.

        BackshoreEnvironment = eventinfo.BackshoreEnvironment = CleanupString(eventinfo.BackshoreEnvironment);
        BulkheadMaterial =eventinfo.Bulkhead = CleanupString(eventinfo.Bulkhead);
        ErosionSinceLast = eventinfo.ErosionSinceLast = CleanupString(eventinfo.ErosionSinceLast);
        BulkheadCondition = eventinfo.BulkheadCondition = CleanupString(eventinfo.BulkheadCondition);
        BackshoreEnvironment = eventinfo.BackshoreEnvironment = CleanupString(eventinfo.BackshoreEnvironment);
        SeagrassPercent = eventinfo.Seagrasspercent = CleanupString(eventinfo.Seagrasspercent);

        // Save the current state of the beach event so we can test later for changes that require saving
        if (eventinfo != null) _originalBeachEvent = JsonSerializer.Serialize(eventinfo);

        IsDirty = false;
        return loadedSurvey.BeachEvent;
    }

    private string? CleanupString(string? instring)
    {
        if (string.IsNullOrWhiteSpace(instring))
            return null;
        return instring.Trim();
    }

    private List<Content> InitBackshoreInfo()
    {
        var loadedSurvey = (HostScreen as HomeViewModel)!.LoadedSurvey;
        // Save the current list so we can test later for changes that require saving
        var originalContents = loadedSurvey!.BeachEvent!.BackshoreContents ?? null;

        if (StaticData.GlobalData.Contents.Count > 0)
            StaticData.GlobalData.Contents = StaticData.GlobalData.Contents.Union(BackShore.TypicalContents).ToList();
        else
            StaticData.GlobalData.Contents = new List<string>(BackShore.TypicalContents).ToList();

        Contents = new ObservableCollection<Content>();

        IEnumerable<BackShoreContent> backshoreList = BackShore.DecodeBackshoreList(originalContents);
        // Normalize the list in case there are any issues with nulls or empty strings or ordering
        loadedSurvey!.BeachEvent!.BackshoreContents = BackShore.EncodeBackshoreList(backshoreList);

        backshoreList.ToList().ForEach(n => Contents.Add(new Content(n.BackShoreContents, this, CanEditSurvey)));
        if (Contents.Count > 0)
            StaticData.GlobalData.Contents = StaticData.GlobalData.Contents.Union(Contents.Select(s => s.Contents)).ToList();

        if (CanEditSurvey)
        {
            // Add a placeholder for 'other'
            Content placeHolder = new Content(
                "", this, CanEditSurvey, isPlaceHolder: true);
            Contents.Add(placeHolder);
        }
        SelectedContents = Contents.Last();

        return Contents.ToList();

    }

    public override void SaveChanges()
    {
        var loadedSurvey = (HostScreen as HomeViewModel)!.LoadedSurvey;
        if (loadedSurvey is null || !CanEditSurvey)
        {
            IsDirty = false;
            return;
        }
        var beachEvent = loadedSurvey?.BeachEvent;

        // Save the backshore contents and detect changes that require saving
        int index = 0;
        IEnumerable<BackShoreContent> newBackshoreContents = 
            Contents.Where(n => !n.IsPlaceHolder && !string.IsNullOrEmpty(n.Contents)).OrderBy(n => n.Contents).Select(s => new BackShoreContent(++index, s.Contents));
        IEnumerable<string> newContents = newBackshoreContents.Select(b => b.BackShoreContents);

        loadedSurvey!.BeachEvent!.BackshoreContents = BackShore.EncodeBackshoreList(newBackshoreContents);

        // Save other properties
        beachEvent.Bulkhead = CleanupString(BulkheadMaterial);
        beachEvent.ErosionSinceLast = CleanupString(ErosionSinceLast);
        beachEvent.BulkheadCondition = CleanupString(_bulkheadCondition);
        beachEvent.BackshoreEnvironment = CleanupString(_backshoreEnvironment);
        beachEvent.Seagrasspercent = CleanupString(SeagrassPercent);

        string currentBeachEvent = JsonSerializer.Serialize(beachEvent);
        bool changesMade = !string.Equals(_originalBeachEvent, currentBeachEvent, StringComparison.InvariantCulture);

        if (changesMade)
        {
            loadedSurvey.SaveRequired.Add(ComponentsToSaveEnum.BeachEvent);
            IsDirty = true;
        }
    }

    #endregion Load and Save

    internal void DeleteContentObservation(Content observation)
    {
        if (Contents.Contains(observation))
        {
            Contents.Remove(observation);
            SelectedContents = Contents.Last();
        }
    }

    private const string BULKHEAD_SOUND = "sound";
    private const string BULKHEAD_FAILING = "failing";

    private const string BACKSHORE_NATURAL = "natural";
    private const string BACKSHORE_ALTERED = "altered";

    #region Properties

    private string? _backshoreEnvironment = null;
        public string? BackshoreEnvironment
    {
        get => _backshoreEnvironment;
        set
        {
            this.RaiseAndSetIfChanged(ref _backshoreEnvironment, value);
            if (value is not null)
            {
                if (_backshoreEnvironment.Equals(BACKSHORE_NATURAL, StringComparison.OrdinalIgnoreCase))
                {
                    IsBackshoreNatural = true;
                    IsBackshoreAltered = false;
                }
                else if (_backshoreEnvironment.Equals(BACKSHORE_ALTERED, StringComparison.OrdinalIgnoreCase))
                {
                    IsBackshoreAltered = true;
                    IsBackshoreNatural = false;
                }
            }
        }
    }

    private string? _bulkheadCondition = null;
    public string? BulkheadCondition
    {
        get => _bulkheadCondition;
        set
        {
            this.RaiseAndSetIfChanged(ref _bulkheadCondition, value);
            if (value is not null)
            {
                if (_bulkheadCondition.Equals(BULKHEAD_SOUND, StringComparison.OrdinalIgnoreCase))
                {
                    IsBulkheadSound = true;
                    IsBulkheadFailing = false;
                }
                else if (_bulkheadCondition.Equals(BULKHEAD_FAILING, StringComparison.OrdinalIgnoreCase))
                {
                    IsBulkheadFailing = true;
                    IsBulkheadSound = false;
                }

            }
        }
    }


    private bool _isBackshoreAltered = false;
    public bool IsBackshoreAltered
    {
        get => _backshoreEnvironment is not null && _backshoreEnvironment.Equals(BACKSHORE_ALTERED, StringComparison.OrdinalIgnoreCase);
        set { this.RaiseAndSetIfChanged(ref _isBackshoreAltered, value);
            if (value) _backshoreEnvironment = BACKSHORE_ALTERED;
            if (!value && !string.IsNullOrEmpty(_backshoreEnvironment) && _backshoreEnvironment.Equals(BACKSHORE_ALTERED, StringComparison.OrdinalIgnoreCase))
                _backshoreEnvironment = null;

        }
    }

    private bool _isBackshoreNatural = false;
    public bool IsBackshoreNatural
    {
        get => _backshoreEnvironment is not null && _backshoreEnvironment.Equals(BACKSHORE_NATURAL, StringComparison.OrdinalIgnoreCase);
        set { this.RaiseAndSetIfChanged(ref _isBackshoreNatural, value);
            if (value) _backshoreEnvironment = BACKSHORE_NATURAL;
            if (!value && !string.IsNullOrEmpty(_backshoreEnvironment) && _backshoreEnvironment.Equals(BACKSHORE_NATURAL, StringComparison.OrdinalIgnoreCase))
                _backshoreEnvironment = null;
        }
    }

    [JsonIgnore]
    private ObservableCollection<Content> _contents = new ();
    public ObservableCollection<Content> Contents
    {
        get => _contents;
        set { this.RaiseAndSetIfChanged(ref _contents, value); }
    }


    private string? _bulkheadMaterial = null;
    public string? BulkheadMaterial
    {
        get => _bulkheadMaterial;
        set { this.RaiseAndSetIfChanged(ref _bulkheadMaterial, value); }
    }

    private bool _isBulkheadSound = false;
    public bool IsBulkheadSound
    {
        get => _bulkheadCondition is not null && _bulkheadCondition.Equals(BULKHEAD_SOUND, StringComparison.OrdinalIgnoreCase);
        set { this.RaiseAndSetIfChanged(ref _isBulkheadSound, value);
            if (value) _bulkheadCondition = BULKHEAD_SOUND;
            if (!value && !string.IsNullOrEmpty(_bulkheadCondition) && _bulkheadCondition.Equals(BULKHEAD_SOUND, StringComparison.OrdinalIgnoreCase)) 
                _bulkheadCondition = null;
        }
    }

    private bool _isBulkheadFailing = false;
    public bool IsBulkheadFailing
    {
        get => _bulkheadCondition is not null && _bulkheadCondition.Equals(BULKHEAD_FAILING, StringComparison.OrdinalIgnoreCase);
        set { this.RaiseAndSetIfChanged(ref _isBulkheadFailing, value);
            if (value) _bulkheadCondition = BULKHEAD_FAILING;
            if (!value && !string.IsNullOrEmpty(_bulkheadCondition) && _bulkheadCondition.Equals(BULKHEAD_FAILING, StringComparison.OrdinalIgnoreCase))
                _bulkheadCondition = null;

            if (!value) IsBulkheadSound = true;
        }
    }

    private string? _erosionSinceLast = null;
    public string? ErosionSinceLast
    {
        get => _erosionSinceLast;
        set { this.RaiseAndSetIfChanged(ref _erosionSinceLast, value); }
    }

    [JsonIgnore]
    private ObservableCollection<string> _erosionSinceCollection = new ObservableCollection<string>() 
    {    "None",
        "inches",
        "feet",
        "Major Slide",
        "Unknown"
    };

    [JsonIgnore]
    public ObservableCollection<string> ErosionSinceCollection
    {
        get => _erosionSinceCollection;
        set { this.RaiseAndSetIfChanged(ref _erosionSinceCollection, value); }
    }

    [JsonIgnore]
    private ObservableCollection<string> _eelGrassCollection = new ObservableCollection<string>()
    {
        "None",
        "1-5%",
        "6-25%",
        "25-50%",
        "51-75%",
        ">75%"
    };

    [JsonIgnore]
    public ObservableCollection<string> EelGrassCollection
    {
        get => _eelGrassCollection;
        set { this.RaiseAndSetIfChanged(ref _eelGrassCollection, value); }

    }

    private string? _seagrassPercent = null;
    public string? SeagrassPercent
    {
        get => _seagrassPercent;
        set { this.RaiseAndSetIfChanged(ref _seagrassPercent, value); }
    }
    #endregion Properties



    #region Search functionality

    public Func<string?, CancellationToken, Task<IEnumerable<object>>> ContentsSearchFunction => ContentsSearchAsync;


    private bool _searchStartsWith = true;
    private async Task<IEnumerable<object>> ContentsSearchAsync(string searchText, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return new List<string>(); // Return empty if no search text
        }
        List<string> results = new List<string>();
        foreach (var content in StaticData.GlobalData.Contents.Where(n => searchText.Equals("*") || 
                                   n.StartsWith(searchText, StringComparison.OrdinalIgnoreCase)))
        {
            if (!Contents.Any(s=>s.Contents.Equals(content,StringComparison.OrdinalIgnoreCase)))
            {
                results.Add(content);
            }
        }
        return results.OrderBy(n=>n);
    }

    internal void TestContentIfChanged()
    {
        if (SelectedContents is null)
        {
            // Should not happen theoretically
            return;
        }
        if (string.IsNullOrEmpty(SelectedContents.Contents) && SelectedContents.IsPlaceHolder)
        {
            // Nothing to do if placeholder
            return;
        }
        string newContents = SelectedContents.Contents;
        if (SelectedContents.IsPlaceHolder)
        {
            // need to add a new placeholder
            AddAPlaceholder();
            SelectedContents.IsPlaceHolder = false;

            SelectedContents = Contents.Last();

        }
        if (StaticData.GlobalData.Contents.Contains(newContents))
            StaticData.GlobalData.Contents.Add(newContents);
    }
    private Content AddAPlaceholder()
    {
        Content placeHolder = new Content(
            "", this, CanEditSurvey, isPlaceHolder: true);
        Contents.Add(placeHolder);
        return placeHolder;
    }

    #endregion Search functionality

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
    #endregion INotifyDataErrorInfo
}
