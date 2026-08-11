using DataLibrary.Crud;
using Models;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace SWSMonitor.ViewModels;

public class MapWebViewModel : ViewModelBase, INotifyPropertyChanged
{
    public static MapWebViewModel? Instance { get; private set; } = null;

    private double _mapPositionLong = 0.0;
    public double MapPositionLong
    {
        get => _mapPositionLong;
        set
        {
            if (value != _mapPositionLong)
            {
                this.RaiseAndSetIfChanged(ref _mapPositionLong, value);
            }
        }

    }

    private double _mapPositionLat = 0.0;
    public double MapPositionLat
    {
        get => _mapPositionLat;
        set
        {
            if (value != _mapPositionLat)
            {
                this.RaiseAndSetIfChanged(ref _mapPositionLat, value);
            }
        }
    }

    private BeachData? _selectedBeach = null;
    public BeachData? SelectedBeach
    {
        get => _selectedBeach;
        set => this.RaiseAndSetIfChanged(ref _selectedBeach, value);    
    }

    private SurveyBase? _selectedSurveyDate = null;
    public SurveyBase? SelectedSurveyDate
    {
        get => _selectedSurveyDate;
        set => this.RaiseAndSetIfChanged(ref _selectedSurveyDate, value);
    }

    private bool _activeOnly = true;
    public bool ActiveOnly
    {
        get => _activeOnly;
        set
        {
            if (value != _activeOnly)
            {
                this.RaiseAndSetIfChanged(ref _activeOnly, value);
            }
        }
    }

    private bool _isWhidbey = true;
    public bool IsWhidbey
    {
        get => _isWhidbey;
        set
        {
            if (value != _isWhidbey)
            {
                this.RaiseAndSetIfChanged(ref _isWhidbey, value);
            }
        }
    }

    private bool _isCamano = false;
    public bool IsCamano
    {
        get => _isCamano;
        set
        {
            if (value != _isCamano)
            {
                this.RaiseAndSetIfChanged(ref _isCamano, value);
            }
        }
    }

    public ObservableCollection<BeachData> Beaches { get; } = new();
    public ObservableCollection<SurveyBase> SurveyDates { get; } = new();

    #region CTOR
    public MapWebViewModel()
    {
        Instance = this;
        InitializeBeaches();
        PropertyChanged += BeachViewModel_PropertyChanged;
    }

    private void FilterBeachList()
    {
        Beaches.Clear();
        SurveyDates.Clear();
        foreach (var beach in StaticData.Beaches.Where(n => 
            (n.IsCurrentlyMonitored.Value || !ActiveOnly) &&
            ((n.Island.Equals("Whidbey") && IsWhidbey) || (n.Island.Equals("Camano") && IsCamano))
            ))
        {
            Beaches.Add(beach);
            foreach (SurveyBase surveybase in StaticData.Surveys!.Where(s => s.BeachName == beach.BeachName).
                OrderByDescending(o => o.SurveyDate))
                SurveyDates.Add(surveybase);
        }
        SelectedBeach = Beaches.FirstOrDefault();
    }

    private async void InitializeBeaches()
    {
        if (StaticData.Beaches is null || StaticData.Beaches.Count == 0)
        {
            StaticData.Beaches = await BeachDataCrud.ReadAllBeachDataAsync(StaticData.DataSourceConfig);
        }
        FilterBeachList();
        MapPositionLat = StaticData.Beaches.Where(b => (b.Lat != null) && b.Lat > 0).Average(b => b.Lat);
        MapPositionLong = StaticData.Beaches.Where(b => (b.Long != null) && Math.Abs(b.Long) > 0).Average(b => b.Long);

    }


    private void BeachViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ActiveOnly):
            case nameof(IsWhidbey):
            case nameof(IsCamano):
                FilterBeachList();
                break;
            case nameof(SelectedBeach):
                break;
        }
    }

    #endregion CTOR

}
