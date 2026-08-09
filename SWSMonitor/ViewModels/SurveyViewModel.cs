using Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Security.Cryptography.X509Certificates;

namespace SWSMonitor.ViewModels;

/// <summary>
///  This is our ViewModel for the first page
/// </summary>
public class SurveyViewModel : WizardViewModelBase
{

    // private readonly IDataService _dataService;

    public static string LastIsland = "Whidbey";
    public static string LastBeach = "Ala Spit";
    public static string LastSurveyDate = null;

    //public ViewModelActivator Activator { get; } = new ViewModelActivator();

    #region CTOR
    public SurveyViewModel()
    {
        // This is just here for design-time support
    }

    // Use Dependency Injection Constructor
    public SurveyViewModel(ViewModelBase screen) //, IDataService dataService)
    {
        HostScreen = screen;
        //_dataService = dataService;

//        Activator = new ViewModelActivator();

        PageTitle = "Survey Selection";
        PropertyChanged += SurveyViewModel_PropertyChanged;
        CanEditSurvey = false;

        if (!StaticData.AllGlobalsLoaded)
        {
            StaticData.PreLoadGlobalsAsync().Wait();
        }
        if (StaticData.Beaches is not null && StaticData.Beaches.Any())
        {
            if (SurveySites is null)
            {
                LoadIslandBeaches();
            }
            if (!string.IsNullOrEmpty(LastBeach))
                SelectedBeach = SurveySites.FirstOrDefault(b => b.BeachName.Equals(LastBeach, StringComparison.InvariantCultureIgnoreCase));

        }
       
    }

    public override void OnNavigatingTo()
    {
        _userCanEdit = StaticData.UserCanEdit;
        SetUpCommands(_canGoBack, _canGoNext);
        if (!string.IsNullOrEmpty(StaticData.Editor))
        {
            EditorNameText = StaticData.Editor;
        }
        if (!string.IsNullOrEmpty(StaticData.EditReason))
        {
            EditReasonText = StaticData.EditReason;
        }

    }
    public override void OnNavigatingFrom()
    {
    }

    #endregion CTOR

    public ObservableCollection<BeachData> SurveySites { get; set; }
    public ObservableCollection<DateTime> SurveyDates { get; set; }

    private bool _isExistingSurvey = false;
    public bool IsExistingSurvey
    {
        get => _isExistingSurvey;
        set { this.RaiseAndSetIfChanged(ref _isExistingSurvey, value); 
            this.RaisePropertyChanged(nameof(UserCanEdit));
            this.RaisePropertyChanged(nameof(UserCanCreateStudy));
        }
    }

    private bool _isNewSurvey = true;
    public bool IsNewSurvey
    {
        get => _isNewSurvey;
        set { this.RaiseAndSetIfChanged(ref _isNewSurvey, value); }
    }

    private bool _userCanEdit = false;
    public bool UserCanEdit
    {
        get => _userCanEdit && _isExistingSurvey;
        set { this.RaiseAndSetIfChanged(ref _userCanEdit, value); }
    }

    private bool _userCanCreateStudy = false;
    public bool UserCanCreateStudy
    {
        get => _userCanEdit && !_isExistingSurvey;
    }

    private bool _doesSurveyNeedConfirmed = false;
    public bool DoesSurveyNeedConfirmed
    {
        get => _doesSurveyNeedConfirmed;
        set { this.RaiseAndSetIfChanged(ref _doesSurveyNeedConfirmed, value); }
    }

    private bool _isSurveySelected = false;
    public bool IsSurveySelected
    {
        get => _isSurveySelected;
        set { this.RaiseAndSetIfChanged(ref _isSurveySelected, value); }
    }

    private bool _isSurveyLoading = false;
    public bool IsSurveyLoading
    {
        get => _isSurveyLoading;
        set { this.RaiseAndSetIfChanged(ref _isSurveyLoading, value); }
    }

    private bool _isSurveyReady = false;
    public bool IsSurveyReady
    {
        get => _isSurveyReady;
        set { this.RaiseAndSetIfChanged(ref _isSurveyReady, value); }
    }

    private DateTime? _surveyDate = DateTime.Now;
    public DateTime? SurveyDate
    {
        get => _surveyDate;
        set { this.RaiseAndSetIfChanged(ref _surveyDate, value); }
    }

    bool _isCamano = LastIsland == "Camano";
    public bool IsCamano
    {
        get => _isCamano;
        set { if (value) { LastBeach = string.Empty; LastIsland = "Camano"; } this.RaiseAndSetIfChanged(ref _isCamano, value); }
    }

    bool _isWhidbey = LastIsland == "Whidbey";
    public bool IsWhidbey
    {
        get => _isWhidbey;
        set { if (value) { LastBeach = string.Empty; LastIsland = "Whidbey"; } this.RaiseAndSetIfChanged(ref _isWhidbey, value);  }
    }

    public bool BeachIsValid
    {
        get => _selectedBeach is not null;
    }

    private string _editReasonText = string.Empty;
    public string EditReasonText
    {
        get => _editReasonText;
        set { this.RaiseAndSetIfChanged(ref _editReasonText, value); }
    }

    private string _editorNameText = string.Empty;
    public string EditorNameText
    {
        get => _editorNameText;
        set { this.RaiseAndSetIfChanged(ref _editorNameText, value); }
    }

    private bool _canEditSurvey = false;
    public bool CanEditSurvey
    {
        get => _canEditSurvey;
        set { this.RaiseAndSetIfChanged(ref _canEditSurvey, value); }
    }

    private string _beachSearchText = string.Empty;
    public string BeachSearchText
    {
        get => _beachSearchText;
        set { this.RaiseAndSetIfChanged(ref _beachSearchText, value); }
    }

    private BeachData? _selectedBeach = null;
    public BeachData? SelectedBeach
    {
        get => _selectedBeach;
        set { if (value is not null) { LastBeach = value.BeachName; } this.RaiseAndSetIfChanged(ref _selectedBeach, value); }
    }

    private DateTime? _selectedDate = null;
    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set { this.RaiseAndSetIfChanged(ref _selectedDate, value); }
    }

    private string _surveyStatusInfo = "No Survey Loaded";
    public string SurveyStatusInfo
    {
        get { return _surveyStatusInfo; }
        set { this.RaiseAndSetIfChanged(ref _surveyStatusInfo, value); }
    }

    // Unique identifier for the routable view model.
    public string UrlPathSegment => "SurveyView";

    private bool _canGoBack = false;
    private bool _canGoNext = false;


    private bool _okToProceed = false;

    public void EditSurvey()
    {
        LoadSelectedSurvey();
        SetUpCommands(_canGoBack, false);
        (HostScreen as HomeViewModel)!.CanEditSurvey = true;
        CanEditSurvey = true;
        _okToProceed = !string.IsNullOrEmpty(EditorNameText) && !string.IsNullOrEmpty(EditReasonText);

    }
    public void ViewSurvey()
    {
        LoadSelectedSurvey();
        SetUpCommands(_canGoBack, true);
        CanEditSurvey = false;
        _okToProceed = true;
        (HostScreen as HomeViewModel)!.CanEditSurvey = false;
    }

    public void CreateSurvey()
    {
        DoesSurveyNeedConfirmed = true;
        IsNewSurvey = false;
        CanEditSurvey = false;
    }

    public override void SaveChanges()
    {
        // Nothing to save on this page
        return;
    }
    public void ConfirmNewSurvey()
    {
        SurveyBase existingstudy = StaticData.Surveys!.FirstOrDefault(n => n.BeachName.Equals(SelectedBeach.BeachName) &&
            n.SurveyDate.Year ==  SurveyDate.Value.Year &&
            n.SurveyDate.DayOfYear == SurveyDate.Value.DayOfYear);

        if (existingstudy is not null)
        {
            // Show error message
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Error",
                "A survey for the selected beach and date already exists. Please choose a different date.",
                ButtonEnum.Ok);
            _ = box.ShowAsync();
            return;
        }

        SetSelectedSurveyInfo(SelectedBeach, SurveyDate);
        CreateSelectedSurvey();
        _okToProceed = !string.IsNullOrEmpty(EditorNameText) && !string.IsNullOrEmpty(EditReasonText);
        SetUpCommands(_canGoBack, _okToProceed);
        (HostScreen as HomeViewModel)!.CanEditSurvey = true;
        CanEditSurvey = true;
    }

    public void CancelNewSurvey()
    {
        DoesSurveyNeedConfirmed = false;
        IsNewSurvey = true;
        _okToProceed = false;
    }


    private void SetSelectedSurveyInfo(BeachData? selectedBeach, DateTime? surveyDate)
    {
        (HostScreen as HomeViewModel)!.SetSelectedSurveyInfo(selectedBeach, surveyDate);
    }

    private async void SurveyViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorNameText) || e.PropertyName == nameof(EditReasonText))
        {
            _okToProceed = !string.IsNullOrEmpty(EditorNameText) && !string.IsNullOrEmpty(EditReasonText);
            SetUpCommands(true, _okToProceed);
        }

        if (e.PropertyName == nameof(SurveyDate))
        {
            if (SurveyDate.HasValue && SurveyDates.Any())
            {
                if (SurveyDates.Any(ev => ev == SurveyDate))
                {
                    IsExistingSurvey = true;
                    IsNewSurvey = false;
                    DoesSurveyNeedConfirmed = false;
                }
                else
                {
                    IsExistingSurvey = false;
                    IsNewSurvey = true;
                    DoesSurveyNeedConfirmed = false;
                }
                this.RaisePropertyChanged(nameof(IsExistingSurvey));
                this.RaisePropertyChanged(nameof(IsNewSurvey));
                this.RaisePropertyChanged(nameof(DoesSurveyNeedConfirmed));
            }
        }
        if (e.PropertyName == nameof(SelectedDate))
        {
            if (SelectedDate is not null)
            {
                SurveyDate = SelectedDate;
                this.RaisePropertyChanged(nameof(SurveyDate));
            }
        }
        if (e.PropertyName == nameof(SelectedBeach))
        {
            if (SelectedBeach is null || SelectedBeach.BeachName is null) return;
            try
            {
                if (StaticData.Surveys is not null && SelectedBeach is not null)
                {

                    IEnumerable<DateTime> existingDates = StaticData.Surveys.Where(s =>
                        s.BeachName.Equals(SelectedBeach?.BeachName ?? string.Empty, StringComparison.InvariantCultureIgnoreCase))
                        .Select(d => d.SurveyDate).Distinct().OrderByDescending(d => d);

                    if (existingDates is not null)
                    {
                        SurveyDates = new ObservableCollection<DateTime>(existingDates);
                        if (SelectedDate is not null && existingDates.Any(ed => ed == SelectedDate))
                        {
                            IsExistingSurvey = true;
                            IsNewSurvey = false;
                            DoesSurveyNeedConfirmed = false;
                        }
                        else
                        {
                            IsExistingSurvey = false;
                            IsNewSurvey = true;
                            DoesSurveyNeedConfirmed = false;
                        }
                    }
                    else
                    {
                        SurveyDates = new();
                    }
                    this.RaisePropertyChanged(nameof(SurveyDates));
                    this.RaisePropertyChanged(nameof(IsExistingSurvey));
                    this.RaisePropertyChanged(nameof(IsNewSurvey));
                    this.RaisePropertyChanged(nameof(DoesSurveyNeedConfirmed));
                }
            }
            catch (Exception ex)
            {

            }
        }
        if (e.PropertyName == nameof(IsWhidbey) || e.PropertyName == nameof(IsCamano))
        {
            LoadIslandBeaches();
        }
    }

    private void LoadSelectedSurvey()
    {
        IsSurveySelected = true;
        IsSurveyLoading = true;
        IsSurveyReady = false;
        StaticData.SurveyLoaded += OnSurveyLoaded;
        (HostScreen as HomeViewModel)!.LoadSelectedSurveyInfo(SelectedBeach, SurveyDate.Value);
    }

    private void OnSurveyLoaded(bool isLoaded)
    {
        // If false will indicate load failed
        StaticData.SurveyLoaded -= OnSurveyLoaded;
        this.RaisePropertyChanged(nameof(IsSurveyLoading));
        if (isLoaded)
        {
            IsSurveyLoading = false;
            IsSurveyReady = true;
            SurveyStatusInfo = "Historical Survey Loaded... Click 'Next' to continue.  'Back' to to Select Different Study";
            SetUpCommands(true, _okToProceed);
            if (_okToProceed || !StaticData.RequireAuditTrail)
            {
                // Proceed automatically to the next page 
                (HostScreen as HomeViewModel)!.NavigatePage(true);
            }
        }
    }

    private void CreateSelectedSurvey()
    {

         (HostScreen as HomeViewModel)!.InitializeNewSurveyInfo(SelectedBeach, SurveyDate.Value);
        SurveyStatusInfo = "New Survey Initialized... Click 'Next' to continue.  'Back' to Select Different Study";
        SetUpCommands(true, true);
        IsSurveySelected = true;
        IsSurveyLoading = false;
        IsSurveyReady = true;
        if (!StaticData.RequireAuditTrail)
        {
            // Proceed automatically to the next page 
            (HostScreen as HomeViewModel)!.NavigatePage(true);
        }
    }

    private void LoadIslandBeaches()
    {
        var allBeaches = StaticData.Beaches;

        // Filter beaches based on the selected island...
        var filtered = allBeaches.Where(b => b.Island == (IsWhidbey ? "Whidbey" : "Camano"));
        SurveySites = new ObservableCollection<BeachData>(filtered);

        List<BeachData> beachList = filtered.OrderBy(o => o.BeachName).ToList();

        SurveySites = new ObservableCollection<BeachData>(beachList);
        this.RaisePropertyChanged(nameof(SurveySites));
        if (string.IsNullOrEmpty(LastBeach))
            SelectedBeach = SurveySites.FirstOrDefault();
        else
            SelectedBeach = SurveySites.FirstOrDefault(n => n.BeachName.Equals(LastBeach));
        this.RaisePropertyChanged(nameof(SelectedBeach));
    }

}