using DataLibrary.Crud;
using Models;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace SWSMonitor.ViewModels;

public class BeachViewModel : ReactiveObject, INotifyDataErrorInfo
{
    public static BeachViewModel? Instance = null;
    private readonly ErrorsViewModel _errorsViewModel;


    private BeachesViewModel? _parentViewModel = BeachesViewModel.Instance;
    private bool isLoading = true;
    private bool isDirty = false;


    public bool UserIsAdmin { get => StaticData.UserRole == AppRoleEnum.Admin; }

    #region CTOR
    public BeachViewModel()
    {
        Instance = this;
        _errorsViewModel = new ErrorsViewModel();
        _errorsViewModel.ErrorsChanged += ErrorsViewModel_ErrorsChanged;
        
        DnrClassOptions.AddRange(BeachData.dnrClasses.Values.Where(n=>!n.Equals("N/A")));
        BulkheadConstructionOptions.AddRange(BeachData.bulkheadConstructions);
        PropertyChanged += BeachViewModel_PropertyChanged;
    }
    #endregion CTOR

    private void BeachViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
        }
    }
    /// <summary>
    /// The Title of this page
    /// </summary>
    public string PageTitle => "Add/Edit Beach";

    /// <summary>
    /// The content of this page
    /// </summary>
    public string Message => "Add and Edit Beach";

    #region Properties for adding/editing beach
    
    public string AirTableId { get; set; } = "";

    private string? _additionalNotes = null;
    public string? AdditionalNotes
    {
        get => _additionalNotes;
        set
        {
            _additionalNotes = value;
            this.RaisePropertyChanged(nameof(AdditionalNotes));
        }
    }

    private string? _beachDirections = null;
    public string? BeachDirections
    {
        get => _beachDirections;
        set
        {
            _beachDirections = value;
            this.RaisePropertyChanged(nameof(BeachDirections));
        }
    }

    private bool _isExistingBeach = false;
    [JsonIgnore]
    public bool IsExistingBeach
    {
        get => _isExistingBeach;
        set
        {
            _isExistingBeach = value;
            this.RaisePropertyChanged(nameof(IsExistingBeach));
        }
    }

    private string _beachName = "";
    public string BeachName
    {
        get => _beachName;
        set
        {
            if (!isLoading)
            {
                if (value is not null)
                    value = value.Trim();

                _errorsViewModel.ClearErrors(nameof(BeachName));
                if (string.IsNullOrEmpty(value))
                {
                    _errorsViewModel.AddError(nameof(BeachName), "Beach Name cannot be blank.");
                }
                else
                {
                    // Check for duplicates
                    var existing = StaticData.Beaches.FirstOrDefault(n => n.BeachName.Equals(value, StringComparison.OrdinalIgnoreCase) && n.ID != this.ID);
                    if (existing != null)
                    {
                        _errorsViewModel.AddError(nameof(BeachName), "Beach Name already exists. Please choose a different name.");
                    }
                }
            }
            _beachName = value;
            _parentViewModel.CanSave = !HasErrors;
            this.RaisePropertyChanged(nameof(BeachName));
        }
    }


    public int? Bulkhead { get; set; }

    [JsonIgnore]
    public bool? HasBulkhead
    {
        get => Bulkhead.HasValue && Bulkhead.Value > 0;
        set
        {
            Bulkhead = (value.HasValue && value.Value) ? 1 : 0;
            this.RaisePropertyChanged(nameof(HasBulkhead));
        }
    }
        
    private string? _bulkHeadConstruction = null;
    public string? BulkHeadConstruction
    {
        get => _bulkHeadConstruction;
        set
        {
            _bulkHeadConstruction = value;
            this.RaisePropertyChanged(nameof(BulkHeadConstruction));
        }
    }

    public int? County { get; set; } = 1;

    public int? CurrentlyMonitored { get; set; }

    [JsonIgnore]
    public bool? IsCurrentlyMonitored
    {
        get => CurrentlyMonitored.HasValue && CurrentlyMonitored.Value > 0;
        set
        {
            CurrentlyMonitored = (value.HasValue && value.Value) ? 1 : 0;
            this.RaisePropertyChanged(nameof(IsCurrentlyMonitored));
        }
    }


    public int? DnrClass { get; set; }

    [JsonIgnore]
    public string? DecodedDnr
    {
        get => BeachData.GetDecodedDnr(DnrClass);
        set
        {
            var dnrKey = BeachData.GetEncodedDnr(value ?? "");
            DnrClass = dnrKey;
            this.RaisePropertyChanged(nameof(DecodedDnr));
        }
    }

    public DateTime? EntryDate { get; set; } = DateTime.Now;

    public int ID { get; set; } = 0;

    private string? _island = null;
    public string? Island
    {
        get => _island;
        set
        {
            _island = value;
            this.RaisePropertyChanged(nameof(Island));
        }
    }

    public string? Latitude { get; set; }

    public string? Longitude { get; set; }

    [JsonIgnore]
    public double? Lat 
    { 
        get => BeachData.TryParseGeometry(Latitude, out var lat) ? lat : 0.0;
        set           
        {
            _errorsViewModel.ClearErrors(nameof(Lat));

            if (value is null)
            {
                _errorsViewModel.AddError(nameof(Lat), "Latitude cannot be null.");
                return;
            }
            if (value.Value < 47.5 || value.Value > 48.5)
            {
                _errorsViewModel.AddError(nameof(Lat), "Latitude is out of range for Island County)");
                return;
            }
            // Left 48.229987, -122.801009  Right 48.059326, -122.336982  Top 48.452879, -122.641943  Bottom 47.897942, -122.383128
            Latitude = BeachData.FormatGeometry(value.Value);
            this.RaisePropertyChanged(nameof(Lat));
        }
    }

    [JsonIgnore]
    public double? Long
    {
        get => BeachData.TryParseGeometry(Longitude, out var lon) ? lon : 0.0;
        set
        {
            _errorsViewModel.ClearErrors(nameof(Long));
            if (value is null)
            {
                _errorsViewModel.AddError(nameof(Long), "Longitude cannot be null.");
                return;
            }
            if (value.Value < -123.0 || value.Value >= -122.0)
            {
                _errorsViewModel.AddError(nameof(Long), "Longitude is out of range for Island County.");
                return;
            }
            Longitude = BeachData.FormatGeometry(value.Value);
            this.RaisePropertyChanged(nameof(Long));
        }
    }

    [JsonIgnore]
    public bool IsMonitored => IsCurrentlyMonitored ?? false;

    [JsonIgnore]
    public string DecodedTideChart => string.IsNullOrEmpty(TideChart) ? "None supplied" : TideChart;


    public string? _profileDirections = null;
    public string? ProfileDirections
    {
        get => _profileDirections;
        set
        {
            _profileDirections = value;
            this.RaisePropertyChanged(nameof(ProfileDirections));
        }
    }


    private decimal? _profileLineStart = null;
    public decimal? ProfileLineStart
    {
        get => _profileLineStart;
        set
        {
            _errorsViewModel.ClearErrors(nameof(ProfileLineStart));
            if (value is not null)
            {
                if (value.Value < 0 || value.Value > 50)
                {
                    _errorsViewModel.AddError(nameof(ProfileLineStart), "Profile Line Start must be between 0 and 50.");
                    return;
                }
            }
            _profileLineStart = value;
            this.RaisePropertyChanged(nameof(ProfileLineStart));
        }
    }

    private int? _surveyWidth = null;
    public int? SurveyWidth
    {
        get => _surveyWidth;
        set
        {
            _errorsViewModel.ClearErrors(nameof(SurveyWidth));
            if (value is not null)
            {
                if (value.Value <0 || value.Value > 50)
                {
                    _errorsViewModel.AddError(nameof(SurveyWidth), "Survey Width must be between 0 and 50.");
                    return;
                }
            }
            _surveyWidth = value;
            this.RaisePropertyChanged(nameof(SurveyWidth));
        }
    }

    private string? _tideChart = null;  
    public string? TideChart
    {
        get => _tideChart;
        set
        {
            _tideChart = value;
            this.RaisePropertyChanged(nameof(TideChart));
        }
    }   


    private string? _vertRef = null;
    public string? VertRef
    {
        get => _vertRef;
        set
        {
            _vertRef = value;
            this.RaisePropertyChanged(nameof(VertRef));
        }
    }


    #endregion Properties for adding/editing beach

    [JsonIgnore]
    public List<string> DnrClassOptions { get; set; } = new List<string>();

    [JsonIgnore]
    public List<string> BulkheadConstructionOptions { get; set; } = new List<string>();

    [JsonIgnore]
    public List<string> IslandOptions { get; set; } = new List<string>() { "Whidbey","Camano" 
        /*, "Fidalgo", "Guemes", "Lopez", "San Juan", "Orcas", "Shaw", "Vashon", "Maury", "Bainbridge", "Fox", "Anderson" */
    };

    [JsonIgnore]
    public BeachData? SelectedBeach
    {
        get
        {
            if (isLoading)
            {
                return null;
            }
            BeachData beach = new BeachData();
            DataHelper.CopyProperties<BeachViewModel, BeachData>(this, beach);
            return beach;
        }
        set
        {
            if (value != null)
            {
                LoadTargetBeach(value);
            }
        }
    }


    internal async Task<(bool, BeachData?)> SaveBeach()
    {
        if (SelectedBeach is null)
        {
            return (false, null);
        }
        if (string.IsNullOrEmpty(BeachName.Trim()))
        {
            _errorsViewModel.AddError(nameof(BeachName), "Beach Name cannot be blank.");
            return (false, null);
        }
        return await BeachDataCrud.UpdateOrCreateBeachDataAsync(StaticData.DataSourceConfig, SelectedBeach);
    }

    internal void LoadTargetBeach(string target)
    {
        isLoading = true;
        BeachData? beach = StaticData.Beaches.FirstOrDefault(n => n.BeachName == target);
        if (beach != null)
        {
            SelectedBeach = beach;
            IsExistingBeach = true;
        }
        else
        {
            // Need to clear out existing data
            beach = new BeachData() { ID = -1, BeachName = target };
            IsExistingBeach = false;
        }
        LoadTargetBeach(beach, target);
    }

    public bool LoadTargetBeach(BeachData? beach, string beachName = "Fix Me")
    {
        isLoading = true;
        _errorsViewModel.ClearErrors();
        if (beach == null)
        {
            // Need to clear out existing data
            beach = new BeachData() { ID = -1, BeachName = beachName };
        }
        DataHelper.CopyProperties<BeachData, BeachViewModel>(beach, this);
        _parentViewModel?.CanSave = !string.IsNullOrEmpty(BeachName);
        this.RaisePropertyChanged(nameof(Lat));
        this.RaisePropertyChanged(nameof(Long));
        this.RaisePropertyChanged(nameof(HasBulkhead));
        this.RaisePropertyChanged(nameof(IsMonitored));
        isLoading = false;
        isDirty = false;
        return true;
    }

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion INotifyPropertyChanged

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