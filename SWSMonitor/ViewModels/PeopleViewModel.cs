using DataLibrary.Crud;
using Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SWSMonitor.ViewModels;

public partial class PeopleViewModel : ViewModelBase, INotifyPropertyChanged
{
    public static PeopleViewModel? Instance;

    private bool _canSave = true;
    public bool CanSave
    {
        get => _canSave;
        set { this.RaiseAndSetIfChanged(ref _canSave, value); }
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

    private bool _isPopupOpen = false;
    public bool IsPopupOpen
    {
        get => _isPopupOpen;
        set { this.RaiseAndSetIfChanged(ref _isPopupOpen, value); }
    }

    private Volunteer? _selectedVolunteer = null;
    public Volunteer? SelectedVolunteer
    {
        get => _selectedVolunteer;
        set { this.RaiseAndSetIfChanged(ref _selectedVolunteer, value); }
    }

    public bool UserIsAdmin { get => (int)StaticData.UserRole == (int)AppRoleEnum.Admin; }
    public bool UserHasViewRole { get => (bool)((int)StaticData.UserRole >= (int)AppRoleEnum.View); }
    public bool UserHasViewOnlyRole { get => (int)StaticData.UserRole == (int)AppRoleEnum.View; }
    public bool UserHasEditRole { get => StaticData.UserRole >= AppRoleEnum.Edit; }

    public ObservableCollection<Volunteer> Volunteers { get; } = new();

    #region CTOR
    public PeopleViewModel()
    {
        Instance = this;
        PropertyChanged += PeopleViewModel_PropertyChanged;
        //if (StaticData.CityStates is null || StaticData.CityStates.Count == 0)
        //{
        //    // Load CityStates
        //    _ = CityStateCrud.ReadAllCityStatesAsync(StaticData.DataSourceConfig).ContinueWith(t =>
        //    {
        //        if (t.IsCompletedSuccessfully)
        //        {
        //            StaticData.CityStates = t.Result;
        //        }
        //    });
        //}
    }
    #endregion CTOR

    private void PeopleViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ActiveOnly):
                FilterVolunteerList(ActiveOnly);
                break;

        }
    }
    private void FilterVolunteerList(bool activeOnly)
    {
        Volunteers.Clear(); 
        foreach (var volunteer in StaticData.Volunteers.Where(n => !activeOnly || n.Active == 1))
        {
            Volunteers.Add(volunteer);
        }
    }

    private string _firstLastFilterText = string.Empty;
    public string FirstLastFilterText
    {
        get => _firstLastFilterText;
        set
        {
            if (_firstLastFilterText != value)
            {
                this.RaiseAndSetIfChanged(ref _firstLastFilterText, value);
                FirstLastFilter();
            }
        }
    }
    private string _emailFilterText = string.Empty;
    public string EmailFilterText
    {
        get => _emailFilterText;
        set
        {
            if (_emailFilterText != value)
            {
                this.RaiseAndSetIfChanged(ref _emailFilterText, value);
                EmailFilter();
            }
        }
    }

    private string _cityFilterText = string.Empty;
    public string CityFilterText
    {
        get => _cityFilterText;
        set
        {
            if (_cityFilterText != value)
            {
                this.RaiseAndSetIfChanged(ref _cityFilterText, value);
                CityFilter();
            }
        }
    }

    private string _islandFilterText = string.Empty;
    public string IslandFilterText
    {
        get => _islandFilterText;
        set
        {
            if (_islandFilterText != value)
            {
                this.RaiseAndSetIfChanged(ref _islandFilterText, value);
                IslandFilter();
            }
        }
    }

    private void IslandFilter()
    {
        Volunteers.Clear();
        var filtered = StaticData.Volunteers
            .Where(n => !string.IsNullOrEmpty(n.Island) && n.Island.IndexOf(IslandFilterText, StringComparison.OrdinalIgnoreCase) != -1)
            .OrderBy(n => n.Island);
        foreach (var volunteer in filtered)
        {
            Volunteers.Add(volunteer);
        }
    }

    private void CityFilter()
    {
        Volunteers.Clear();
        var filtered = StaticData.Volunteers
            .Where(n => !string.IsNullOrEmpty(n.City) && n.City.IndexOf(CityFilterText, StringComparison.OrdinalIgnoreCase) != -1)
            .OrderBy(n => n.City);
        foreach (var volunteer in filtered)
        {
            Volunteers.Add(volunteer);
        }
    }

    private void EmailFilter()
    {
        Volunteers.Clear();
        var filtered = StaticData.Volunteers
            .Where(n => !string.IsNullOrEmpty(n.Email) && n.Email.IndexOf(EmailFilterText, StringComparison.OrdinalIgnoreCase) != -1)
            .OrderBy(n => n.Email);
        foreach (var volunteer in filtered)
        {
            Volunteers.Add(volunteer);
        }
    }

    private void FirstLastFilter()
    {
        Volunteers.Clear();
        var filtered = StaticData.Volunteers
            .Where(n => !string.IsNullOrEmpty(n.FirstLast) && n.FirstLast.IndexOf(FirstLastFilterText, StringComparison.OrdinalIgnoreCase) != -1)
            .OrderBy(n => n.FirstLast);
        foreach (var volunteer in filtered)
        {
            Volunteers.Add(volunteer);
        }
    }


    public async Task InitializeVolunteers()
    {
        try
        {
            if (StaticData.Volunteers is null || StaticData.Volunteers.Count == 0)
            {
                TraceLogger.LogWarningAuto("No volunteers loaded...");
                StaticData.Volunteers = await VolunteersCrud.ReadAllVolunteersAsync(StaticData.DataSourceConfig);
            }
            FilterVolunteerList(ActiveOnly);
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto("Error loading volunteers");
        }
    }

    /// <summary>
    /// The Title of this page
    /// </summary>
    public string PageTitle => "Volunteer Management";

    /// <summary>
    /// The content of this page
    /// </summary>
    public string Message => "Add and Edit Volunteers";


}