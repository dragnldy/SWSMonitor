using Models;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SWSMonitor.ViewModels;

public class ChecklistItem : ReactiveObject
{
    private bool loading = true;

    private string _itemName = string.Empty;
    public string ItemName
    {
        get => _itemName;
        set { this.RaiseAndSetIfChanged(ref _itemName, value); }
    }
    private bool _isChecked = false;
    public bool IsChecked
    {
        get => _isChecked;
        set { this.RaiseAndSetIfChanged(ref _isChecked, value); IsDirty = true; }
    }

    private bool _canEdit = false;
    public bool CanEdit
    {
        get => _canEdit;
        set { this.RaiseAndSetIfChanged(ref _canEdit, value); }
    }

    public ChecklistEnum enumValue;

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

    WizardViewModelBase _parent;
    public ChecklistItem(string itemName, bool isChecked, ChecklistEnum enumvalue, WizardViewModelBase parent, bool canEdit=true)
    {
        ItemName = itemName;
        IsChecked = isChecked;
        CanEdit = canEdit;
        enumValue = enumvalue;
        _parent = parent;
        loading = false;
    }
}
// Note- several others on form but no place to store the data as far as I can tell
// Team List, Team Photo, Profile Form, Quad Inverts, Quad Seaweed
// Also Bivalve Dig is not on current form coversheet but data in the database
public enum ChecklistEnum
{
    Bivalve_Dig,
    Quad_Photos,
    Species_List
}
public class SurveyMember : ReactiveObject
{
    private bool loading = true;
    private bool isDirty = false;

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set { this.RaiseAndSetIfChanged(ref _name, value); IsDirty = true; }
    }

    private bool _isLead = false;
    public bool IsLead
    {
        get => _isLead;
        set { this.RaiseAndSetIfChanged(ref _isLead, value); IsDirty = true; }
    }

    private bool _isSpeciesExpert = false;
    public bool IsSpeciesExpert
    {
        get => _isSpeciesExpert;
        set { this.RaiseAndSetIfChanged(ref _isSpeciesExpert, value); IsDirty = true; }
    }

    private bool _canEdit = true;
    public bool CanEdit
    {
        get => _canEdit;
        set { this.RaiseAndSetIfChanged(ref _canEdit, value); }
    }

    //private int _id = 0;
    //public int ID
    //{
    //    get => _id;
    //    set { this.RaiseAndSetIfChanged(ref _id, value); }
    //}
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
    public SurveyMember(string name, bool? isLead, bool? isSpeciesExpert, WizardViewModelBase parent, bool canEdit)
    {
        Name = name;
        IsLead = isLead.HasValue? isLead.Value : false;
        IsSpeciesExpert = isSpeciesExpert.HasValue? isSpeciesExpert.Value : false;
        CanEdit = canEdit;
        _parent = parent;
        loading = false;
    }
    public SurveyMember(string? codedMonitor, WizardViewModelBase parent, bool canEditSurvey)
    {
        MonitorBase monitorBase = new MonitorBase(codedMonitor);
        if (monitorBase is null || monitorBase.Monitor is null) return;

        Name = monitorBase.Monitor;
        IsLead = monitorBase.IsLead;
        IsSpeciesExpert = monitorBase.IsSpeciesExpert;
        CanEdit = canEditSurvey;
        _parent = parent;   
    }

    public string EncodeMonitor()
    {
        return SurveyMonitor.EncodeMonitor(Name, IsLead, IsSpeciesExpert);
    }
}

public class TeamViewModel : WizardViewModelBase, INotifyDataErrorInfo
{
    public static TeamViewModel? Instance = null;
    private readonly ErrorsViewModel _errorsViewModel;

    public ViewModelActivator Activator { get; } = new ViewModelActivator();


    #region CTOR

    public TeamViewModel()
    {
        // Just here for design time support
    }

    public TeamViewModel(ViewModelBase hostScreen)
    {
        TeamViewModel.Instance = this;
        _errorsViewModel = new ErrorsViewModel();
        _errorsViewModel.ErrorsChanged += ErrorsViewModel_ErrorsChanged;

        HostScreen = hostScreen;
        PageTitle = "Team Selection";
        PropertyChanged += TeamViewModel_PropertyChanged;
       // Activator = new ViewModelActivator();

    }

    public override void OnNavigatingTo()
    {
        SetUpCommands(_canGoBack, _canGoNext);
        OnActivated();
    }
    public override void OnNavigatingFrom()
    {
        if (CanEditSurvey)
            SaveChanges();
    }

    #endregion CTOR

    #region control properties

    private bool _isPopupOpen = false;
    [JsonIgnore]
    public bool IsPopupOpen
    {
        get => _isPopupOpen;
        set { this.RaiseAndSetIfChanged(ref _isPopupOpen, value); }
    }


    // We will collect and store data in both the SurveyBase and BeachEvent objects 

    [JsonIgnore]
    string _originalSurveyBase = string.Empty;

    [JsonIgnore]
    string _originalBeachEvent = string.Empty;



    // Unique identifier for the routable view model.
    public string UrlPathSegment => "TeamView";
    [JsonIgnore]
    private bool _canGoBack = true;
    public bool CanGoBack
    {
        get { return _canGoBack; }
        set { this.RaiseAndSetIfChanged(ref _canGoBack, value); }
    }


    private bool _canGoNext = true;
    [JsonIgnore]
    public bool CanGoNext
    {
        get { return _canGoNext; }
        set { this.RaiseAndSetIfChanged(ref _canGoNext, value); }
    }
    [JsonIgnore]
    public bool CanEditSurvey
    {
        get => (HostScreen as HomeViewModel)?.CanEditSurvey ?? false;
    }

    [JsonIgnore]
    public ObservableCollection<ChecklistItem> ChecklistItems { get; } = new();
    #endregion control properties

    #region Database Properties

    private string? _notes = null;
    public string? Notes
    {
        get => _notes;
        set { this.RaiseAndSetIfChanged(ref _notes, value); }
    }


    private string? _quadratNotes = null;
    public string? QuadratNotes
    {
        get => _quadratNotes;
        set { this.RaiseAndSetIfChanged(ref _quadratNotes, value); }
    }

    private string _t1 = "1";
    public string TideHt1
    {
        get => _t1;
        set {
            if (value is not null)
                value = value.Trim();

            if (!_isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(TideHt1));
                if (!string.IsNullOrEmpty(value))
                {
                    if (!GoodInt(value, -9, 9))
                    {
                        _errorsViewModel.AddError(nameof(TideHt1), "Tide Ht must be between -9 and 9");
                        value = _t1;
                    }
                }
            }
          
            this.RaiseAndSetIfChanged(ref _t1, value);
            this.RaisePropertyChanged(nameof(TideHt1));
        }
    }

    private bool GoodInt(string value, int min, int max)
    {
        if (string.IsNullOrEmpty(value))
            return true;
        if (int.TryParse(value, out int num))
            return num >= min && num <= max;
        return false;
    }

    private string _t2 = "0";
    public string TideHt2
    {
        get => _t2;
        set
        {
            if (value is not null)
                value = value.Trim();

            if (!_isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(TideHt2));
                if (!string.IsNullOrEmpty(value))
                {
                    if (!GoodInt(value, -9, 9))
                    {
                        _errorsViewModel.AddError(nameof(TideHt2), "Tide Ht must be between -9 and 9");
                        value = _t2;
                    }
                }
            }

            this.RaiseAndSetIfChanged(ref _t2, value);
            this.RaisePropertyChanged(nameof(TideHt2));
        }
    }
    private string _t3 = "-1";
    public string TideHt3
    {
        get => _t3;
        set
        {
            if (value is not null)
                value = value.Trim();

            if (!_isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(TideHt3));
                if (!string.IsNullOrEmpty(value))
                {
                    if (!GoodInt(value, -9, 9))
                    {
                        _errorsViewModel.AddError(nameof(TideHt3), "Tide Ht must be between -9 and 9");
                        value = _t3;
                    }
                }
            }

            this.RaiseAndSetIfChanged(ref _t3, value);
            this.RaisePropertyChanged(nameof(TideHt3));
        }

    }

    #region Times

    private string _startTime = string.Empty;
    public string StartTime
    {
        get => _startTime;
        set {
            if (value is not null)
                value = value.Trim();

            if (!_isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(StartTime));
                if (!string.IsNullOrEmpty(value))
                {
                    if (!IsGoodTime(value))
                    {
                        _errorsViewModel.AddError(nameof(StartTime), "24 hour time must be formatted hh:mm");
                        value = _startTime;
                    }
                }
            }

            this.RaiseAndSetIfChanged(ref _startTime, value); 
        }

    }

    Regex regexTime = new Regex("^(?:2[0-3]|[01]?\\d):[0-5]\\d$");
    private bool IsGoodTime(string value)
    {
        if (string.IsNullOrEmpty(value))
            return true;
        var matches = regexTime.Match(value);
        return matches.Success;
    }

    private string _endTime = string.Empty;
    public string EndTime
    {
        get => _endTime;
        set {
            if (value is not null)
                value = value.Trim();

            if (!_isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(EndTime));
                if (!string.IsNullOrEmpty(value))
                {
                    if (!IsGoodTime(value))
                    {
                        _errorsViewModel.AddError(nameof(EndTime), "24 hour time must be formatted hh:mm");
                        value = _endTime;
                    }
                }
            }

            this.RaiseAndSetIfChanged(ref _endTime, value); }
    }
    private string ExtractTime(DateTime? datevalue)
    {
        if (!datevalue.HasValue)
            return string.Empty;
        return datevalue.Value.ToString(@"hh\:mm");
    }

    private DateTime? ExtractDate(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        if (TimeSpan.TryParse(value, out TimeSpan time))
            return DateTime.MinValue + time;
        return null;
    }

    #endregion Times

    #endregion Database Properties


    private void TeamViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isLoading || e.PropertyName == nameof(IsDirty))
            return;
    }

    #region Load and Save

    private void OnActivated()
    {
        try
        {
            _isLoading = true;
            var loadedSurvey = (HostScreen as HomeViewModel)!.LoadedSurvey;
            var beachEvent = loadedSurvey?.BeachEvent;

            GenerateChecklist(beachEvent);
            SurveyMembers = new();

            string originalmembers = beachEvent?.Monitors;

            IEnumerable<MonitorBase> monitorList = MonitorBase.DecodeMonitorList(originalmembers);
            // Normalize the list in case there are any issues with nulls or empty strings or ordering
            loadedSurvey!.BeachEvent!.Monitors = CleanupString(MonitorBase.EncodeMonitorList(monitorList));

            monitorList.ToList().ForEach(n => SurveyMembers.Add(new SurveyMember(n.Monitor, n.IsLead, n.IsSpeciesExpert, this, CanEditSurvey)));

            this.RaisePropertyChanged(nameof(SurveyMembers));
            this.Notes = beachEvent.BeachProfileNotes = CleanupString(beachEvent!.BeachProfileNotes);
            this.QuadratNotes = beachEvent!.QuadratNotes = CleanupString(beachEvent.QuadratNotes);

            // Access saved their times with fake date in front
            loadedSurvey!.StartTime = PreCleanTimes(loadedSurvey!.StartTime);
            loadedSurvey!.EndTime = PreCleanTimes(loadedSurvey!.EndTime);

            StartTime = loadedSurvey!.StartTime;
            EndTime = loadedSurvey!.EndTime;

            TideHt1 = loadedSurvey!.Tide1Ht.ToString();
            TideHt2 = loadedSurvey!.Tide2Ht.ToString();
            TideHt3 = loadedSurvey!.Tide3Ht.ToString();

            beachEvent.BackshoreContents = CleanupString(beachEvent.BackshoreContents);
            beachEvent.SpeciesObserved = CleanupString(beachEvent!.SpeciesObserved);

            _originalSurveyBase = JsonSerializer.Serialize((SurveyBase)loadedSurvey);
            _originalBeachEvent = JsonSerializer.Serialize(beachEvent);

            _errorsViewModel.ClearErrors();
            _isLoading = false;
            IsDirty = false;
        }
        catch (Exception ex)
        {
            // Handle any exceptions that occur during activation
            TraceLogger.LogWarningAuto($"Error during activation: {ex.Message}");
        }
    }

    private string PreCleanTimes(string time)
    {
        if (string.IsNullOrEmpty(time)) return time;
        if (time.StartsWith("1899"))
            time = time.Replace("1899-12-30 ", "");
        if (time.EndsWith(":00") && time.Length == 8)
            time = time[..5];

        return time;
    }

    private string? CleanupString(string? instring)
    {
        if (string.IsNullOrWhiteSpace(instring))
            return null;
        return instring.Trim();
    }

    private void GenerateChecklist(BeachEventBase? beachEvent)
    {
        ChecklistItems!.Clear();

        if (beachEvent is null) return;

        foreach (var item in Enum.GetValues<ChecklistEnum>())
        {
            string itemstr = item.ToString().Replace("_", " ");
            ChecklistItems.Add(new ChecklistItem(itemstr,IsItemChecked(item, beachEvent), item, this, CanEditSurvey));
        }
    }

    private bool IsItemChecked(ChecklistEnum item, BeachEventBase? beachEvent)
    {
        if (beachEvent is null)
            return false;
        switch (item)
        {
            case ChecklistEnum.Bivalve_Dig:
                return beachEvent.BivalveDig != null && beachEvent.BivalveDig == 1;

            case ChecklistEnum.Quad_Photos:
                return beachEvent.PhotosTaken != null && beachEvent.PhotosTaken == 1;

            case ChecklistEnum.Species_List:
                return beachEvent.SpeciesListGenerated != null && beachEvent.SpeciesListGenerated == 1;

            default:
                return false;
        }
    }
    public override void SaveChanges()
    {
        // Save the notes and team members back to the loaded survey
        var loadedSurvey = (HostScreen as HomeViewModel)!.LoadedSurvey;

        if (loadedSurvey is not null)
        {
            if (loadedSurvey.BeachEvent is null)
            {
                loadedSurvey.BeachEvent = new(0l, loadedSurvey.ID);
            }
            var beachEvent = loadedSurvey?.BeachEvent;

            beachEvent!.BeachProfileNotes = CleanupString(Notes);
            beachEvent.QuadratNotes = CleanupString(QuadratNotes);
            // Monitors are in the Survey.Monitors list
            IEnumerable<string> selectedMembers = SurveyMembers.Where(n => !string.IsNullOrEmpty(n.Name)).OrderBy(n => n.Name).Select(sm => sm.EncodeMonitor());
            beachEvent.Monitors = CleanupString(string.Join(";", selectedMembers)); ;

            loadedSurvey.EndTime = EndTime;
            loadedSurvey.StartTime = StartTime;
            loadedSurvey.Tide1Ht = GetIntValue(TideHt1);
            loadedSurvey.Tide2Ht = GetIntValue(TideHt2);
            loadedSurvey.Tide3Ht = GetIntValue(TideHt3);
            SaveChecklist();

            string _newSurveyBase = JsonSerializer.Serialize((SurveyBase)loadedSurvey);
            string _newBeachEvent = JsonSerializer.Serialize(beachEvent);

            bool changesMade = _newSurveyBase != _originalSurveyBase || _newBeachEvent != _originalBeachEvent;

            if (changesMade)
            {
                loadedSurvey.SaveRequired.Add(ComponentsToSaveEnum.Base);
                loadedSurvey.SaveRequired.Add(ComponentsToSaveEnum.BeachEvent);
                IsDirty = true;
            }
        }
    }

    private int GetIntValue(string intAsString)
    {
        if (!string.IsNullOrEmpty(intAsString) && int.TryParse(intAsString, out int intValue))
            return intValue;
        return 0;
    }

    private void SaveChecklist()
    {
        var loadedSurvey = (HostScreen as HomeViewModel)!.LoadedSurvey;
        if (loadedSurvey is null || loadedSurvey.BeachEvent is null)
            return;

        var info = loadedSurvey.BeachEvent;
        info.BivalveDig = GetCheckState(ChecklistEnum.Bivalve_Dig);
        info.PhotosTaken = GetCheckState(ChecklistEnum.Quad_Photos);
        info.SpeciesListGenerated = GetCheckState(ChecklistEnum.Species_List);
    }

    private int GetCheckState(ChecklistEnum thisitem)
    {
        var checkitem = ChecklistItems.FirstOrDefault(c => c.enumValue == thisitem);
        if (checkitem is not null)
            return checkitem.IsChecked ? 1 : 0;
        return 0;
    }

    #endregion Load and Save


    #region Species Expert

    private ObservableCollection<string> _speciesExperts = new ObservableCollection<string>();
    [JsonIgnore]
    public ObservableCollection<string> SpeciesExperts
    {
        get => _speciesExperts;
        set { this.RaiseAndSetIfChanged(ref _speciesExperts, value); }
    }

    #endregion Species Expert

    #region Survey Members

    private string _memberToAdd = string.Empty;
    [JsonIgnore]
    public string MemberToAdd
    {
        get => _memberToAdd;
        set { this.RaiseAndSetIfChanged(ref _memberToAdd, value); }
    }

    private ObservableCollection<SurveyMember>? _surveyMembers = new ObservableCollection<SurveyMember>();
    [JsonIgnore]
    public ObservableCollection<SurveyMember>? SurveyMembers
    {
        get => _surveyMembers;
        set { this.RaiseAndSetIfChanged(ref _surveyMembers, value); 
            if (!_isLoading) 
                IsDirty = true; 
        }
    }

    private int _selectedMemberIndex = -1;
    public int SelectedMemberIndex
    {
        get => _selectedMemberIndex;
        set { _selectedMemberIndex = -1; this.RaisePropertyChanged(nameof(SelectedMemberIndex)); }
    }

    private SurveyMember? _selectedSurveyMember = null;
    public SurveyMember? SelectedSurveyMember
    {
        get => _selectedSurveyMember;
        set { _selectedSurveyMember = null; this.RaisePropertyChanged(nameof(SelectedSurveyMember)); }
    }

    internal SurveyMember AddMember(string memberName)
    {
        var foundMember = SurveyMembers.FirstOrDefault(sm => sm.Name.Equals(memberName, StringComparison.InvariantCultureIgnoreCase));
        if (foundMember is null)
        {
            var volunteer = StaticData.Volunteers.FirstOrDefault(v=>v.FirstLast.Equals(memberName, StringComparison.InvariantCultureIgnoreCase));
            if (volunteer is not null)
            {
                foundMember = new SurveyMember(memberName, false, false, this, CanEditSurvey);
                SurveyMembers.Insert(0,foundMember);
                if (!_isLoading)
                    IsDirty = true;
            }
        }
        return foundMember;
    }

    internal void RemoveSelectedMember(SurveyMember? member)
    {
        if (member is null || SurveyMembers is null) return;
        if (!string.IsNullOrEmpty(member.Name))
        {
            SurveyMembers.Remove(member);
            IsDirty = true;
        }
    }

    [JsonIgnore]
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> VolunteerSearchFunction => PopulateSuggestionsAsync;

    private async Task<IEnumerable<object>> PopulateSuggestionsAsync(string? searchText, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return new List<string>(); // Return empty if no search text
        }

        List<Volunteer> allPossibleSuggestions = StaticData.Volunteers.Where(n => n.FirstLast.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
        // Don't return any that are already in the list of selected members
        var matches = allPossibleSuggestions.Where(s => !SurveyMembers.Where(sm => sm.Name.Equals(s.FirstLast)).Any()).Select(v => v.FirstLast).OrderBy(n => n).ToList();
        //int matchCount = matches.Count();
        //if (matchCount == 1)
        //{
        //    // Auto-select the single match
        //    var singleMatch = matches.First();
        //    var member = AddMember(singleMatch);
        //    MemberToAdd = string.Empty;
        //}
        return matches;
    }
    #endregion Survey Members

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