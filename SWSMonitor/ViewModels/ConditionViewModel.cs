using Models;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization; // Add this using directive at the top if not present

namespace SWSMonitor.ViewModels;
/// <summary>
///  This is our ViewModel for the first page
/// </summary>
public class ConditionViewModel : WizardViewModelBase, INotifyDataErrorInfo
{
    public static ConditionViewModel? Instance = null;
    private readonly ErrorsViewModel _errorsViewModel;

    // public ViewModelActivator Activator { get; } = new ViewModelActivator();

    // Unique identifier for the routable view model.
    public string UrlPathSegment => "ConditionView";
    public static string Title => "Conditions";

    private bool _canGoBack = true;
    private bool _canGoNext = true;

    [JsonIgnore]    
    public bool CanEditSurvey
    {
        get => (HostScreen as HomeViewModel)?.CanEditSurvey ?? false;
    }


    #region CTOR
    public ConditionViewModel()
    {
        // This is just here for design-time support
    }
    public ConditionViewModel(ViewModelBase screen)
    {
        ConditionViewModel.Instance = this;
        _errorsViewModel = new ErrorsViewModel();
        _errorsViewModel.ErrorsChanged += ErrorsViewModel_ErrorsChanged;

        HostScreen = screen;
        PageTitle = "Conditions";
        PropertyChanged += ConditionViewModel_PropertyChanged;

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


    #endregion

    private void ConditionViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isLoading)
            return;
    }

    const string AirMask = "#0.0";
    const string WaterMask = "#0.0";
    const string SalinityMask = "#0";
    const string BarometerMask = "#0.00";
    const string ProfileMask = "#0.0";


    private string _originalBeachEvent = string.Empty;

    #region Load and Save
    private void OnActivated()
    {
        _isLoading = true;
        BeachEventBase eventinfo = InitBeachInfo();
        if (eventinfo is null)
            return;
        _errorsViewModel.ClearErrors();

        // Get all the beach event info for conditions
        AirTemp = FormatNumber(eventinfo.AirTemp, 1, AirMask);
        WaterTemp = FormatNumber(eventinfo.WaterTemp,1,WaterMask);
        Salinity = FormatNumber(eventinfo.Salinity,0,SalinityMask);
        BarometricPressure = FormatNumber(eventinfo.BarometricPressure,2,BarometerMask);
        ProfileStartHt = FormatNumber(eventinfo.VerticalHeight,1, ProfileMask);
        TideAtEnd = FormatNumber(eventinfo.TideHeightAtEnd,1, ProfileMask);
        CorrectedTide = FormatNumber(eventinfo.CorrectedTideHeight,1, ProfileMask);

        CloudCover = eventinfo!.CloudCover = CleanupString(eventinfo!.CloudCover);
        Precipitation = eventinfo!.Precipitation = CleanupString(eventinfo!.Precipitation);
        Wind = eventinfo!.Wind = CleanupString(eventinfo!.Wind);

        _isLoading = false;
        _originalBeachEvent = JsonSerializer.Serialize(eventinfo);
    }

    private string? CleanupString(string? instring)
    {
        if (string.IsNullOrWhiteSpace(instring))
            return null;
        return instring.Trim();
    }

    private string FormatNumber(double? measurement, int numdecimal, string mask="00.0")
    {
        if (!measurement.HasValue)
            return string.Empty;
        return Math.Round(measurement.Value, numdecimal).ToString(mask);
    }

    private BeachEventBase InitBeachInfo()
    {
        var loadedSurvey = (HostScreen as HomeViewModel)!.LoadedSurvey;

        if (loadedSurvey!.BeachEvent is null)
        {
            loadedSurvey.BeachEvent = new BeachEventBase(0l,loadedSurvey.ID);
        }
        return loadedSurvey.BeachEvent;
    }

    public override void SaveChanges()
    {
        var loadedSurvey = (HostScreen as HomeViewModel)!.LoadedSurvey;
        if (loadedSurvey is null || !CanEditSurvey)
        {
            IsDirty = false;
            return;
        }

        // Save the notes and team members back to the loaded survey
        BeachEventBase eventinfo = (BeachEventBase)loadedSurvey.BeachEvent!;
        eventinfo.AirTemp = float.TryParse(AirTemp, out float tempA) ? tempA : null;
        eventinfo.WaterTemp = float.TryParse(WaterTemp, out float tempW) ? tempW : null; 
        eventinfo.Salinity = int.TryParse(Salinity, out int tempS) ? tempS : null;;
        eventinfo.BarometricPressure = float.TryParse(BarometricPressure, out float tempB) ? tempB : null; 
        eventinfo.VerticalHeight = float.TryParse(ProfileStartHt, out float tempP) ? tempP : null; 
        eventinfo.TideHeightAtEnd = float.TryParse(TideAtEnd, out float tempT) ? tempT : null;
        eventinfo.CorrectedTideHeight = float.TryParse(CorrectedTide, out float tempC) ? tempC : null;

        eventinfo.CloudCover = CleanupString(CloudCover);
        eventinfo.Precipitation = CleanupString(Precipitation);
        eventinfo.Wind = CleanupString(Wind);

        string updatedBeachEvent = JsonSerializer.Serialize(eventinfo);
        bool changesMade = updatedBeachEvent != _originalBeachEvent;
        if (changesMade)
        {
            loadedSurvey!.SaveRequired.Add(ComponentsToSaveEnum.BeachEvent);
            IsDirty = true;
        }
    }

    #endregion Load and Save

    #region Database properties
    private string _airTemp = string.Empty;
    public string AirTemp
    {
        get => _airTemp;
        set
        {
            if (value is not null)
                value = value.Trim();

            if (!_isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(AirTemp));
                if (!string.IsNullOrEmpty(value))
                {
                    if (!GoodDouble(value, -50, 50))
                    {
                        _errorsViewModel.AddError(nameof(AirTemp), "Air Temp must be between -50 and 50");
                        value = _airTemp;
                    }
                }
            }

            this.RaiseAndSetIfChanged(ref _airTemp, value);
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

    private string _waterTemp = string.Empty;
    public string WaterTemp
    {
        get => _waterTemp;
        set
        {
            if (value is not null)
                value = value.Trim();

            if (!_isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(WaterTemp));
                if (!string.IsNullOrEmpty(value))
                {
                    if (!GoodDouble(value, -50, 50))
                    {
                        _errorsViewModel.AddError(nameof(WaterTemp), "Water Temp must be between -50 and 50");
                        value = _waterTemp;
                    }
                }
            }

            this.RaiseAndSetIfChanged(ref _waterTemp, value);
        }
    }

    private string _salinity = string.Empty;
    public string Salinity
    {
        get => _salinity;
        set
        {
            if (value is not null)
                value = value.Trim();

            if (!_isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(Salinity));
                if (!string.IsNullOrEmpty(value))
                {
                    if (!GoodInteger(value, 0, 100))
                    {
                        _errorsViewModel.AddError(nameof(Salinity), "Salinity must be between 0 and 100");
                        value = _salinity;
                    }
                }
            }

            this.RaiseAndSetIfChanged(ref _salinity, value);
        }
    }

    private string _barometricPressure = string.Empty;
    public string BarometricPressure
    {
        get => _barometricPressure;
        set
        {
            if (value is not null)
                value = value.Trim();

            if (!_isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(BarometricPressure));
                if (!string.IsNullOrEmpty(value))
                {
                    if (!GoodDouble(value, 20, 35))
                    {
                        _errorsViewModel.AddError(nameof(BarometricPressure), "Barometric Pressure must be between 20 and 35");
                        value = _barometricPressure;
                    }
                }
            }

            this.RaiseAndSetIfChanged(ref _barometricPressure, value);
        }
    }

    private string _profileStartHt = string.Empty;
    public string ProfileStartHt
    {
        get => _profileStartHt;
        set
        {
            if (value is not null)
                value = value.Trim();

            if (!_isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(ProfileStartHt));
                if (!string.IsNullOrEmpty(value))
                {
                    if (!GoodDouble(value, 0, 50))
                    {
                        _errorsViewModel.AddError(nameof(ProfileStartHt), "Profile Start Height must be between 0 and 50");
                        value = _profileStartHt;
                    }
                }
            }

            this.RaiseAndSetIfChanged(ref _profileStartHt, value);
        }
    }


    private string _tideAtEnd = string.Empty;
    public string TideAtEnd
    {
        get => _tideAtEnd;
        set
        {
            if (value is not null)
                value = value.Trim();

            if (!_isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(TideAtEnd));
                if (!string.IsNullOrEmpty(value))
                {
                    if (!GoodDouble(value, -20, 20))
                    {
                        _errorsViewModel.AddError(nameof(TideAtEnd), "Tide at End must be between -20 and 20");
                        value = _tideAtEnd;
                    }
                }
            }

            this.RaiseAndSetIfChanged(ref _tideAtEnd, value);
        }
    }

    private string _correctedTide = string.Empty;
    public string CorrectedTide
    {
        get => _correctedTide;
        set
        {
            if (value is not null)
                value = value.Trim();

            if (!_isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(CorrectedTide));
                if (!string.IsNullOrEmpty(value))
                {
                    if (!GoodDouble(value, -20, 20))
                    {
                        _errorsViewModel.AddError(nameof(CorrectedTide), "Corrected Tide must be between -20 and 20");
                        value = _correctedTide;
                    }
                }
            }

            this.RaiseAndSetIfChanged(ref _correctedTide, value);
        }
    }

    private string? _cloudCover = string.Empty;
    public string? CloudCover
    {
        get => _cloudCover;
        set { this.RaiseAndSetIfChanged(ref _cloudCover, value); }

    }

    private string? _precipitation = string.Empty;
    public string? Precipitation
    {
        get => _precipitation;
        set { this.RaiseAndSetIfChanged(ref _precipitation, value); }
    }

    private string? _wind = string.Empty;
    public string? Wind
    {
        get => _wind;
        set { this.RaiseAndSetIfChanged(ref _wind, value); }
    }

    #endregion Database properties

    #region Control Properties
    [JsonIgnore]
    public List<string> CloudCoverOptions { get; } =
    [
        "Sunny",
        "Partly Cloudy",
        "Overcast",
        "Fog",
    ];

    [JsonIgnore]
    public List<string> PrecipitationOptions { get; } =
    [
        "None",
        "Light Rain",
        "Heavy Rain",
    ];

    [JsonIgnore]
    public List<string> WindOptions { get; } =
    [
        "Calm",
        "Breezy",
        "Windy",
    ];
    #endregion Control Properties


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