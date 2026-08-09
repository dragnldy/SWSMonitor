using DataLibrary.Crud;
using Models;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace SWSMonitor.ViewModels;

public class BeachesViewModel : ViewModelBase, INotifyPropertyChanged
{
    public static BeachesViewModel? Instance { get; private set; } = null;

    private bool isLoading = true;
    private bool isDirty = false;

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

    private bool _canSave = false;
    public bool CanSave
    {
        get => _canSave;
        set { this.RaiseAndSetIfChanged(ref _canSave, value); }
    }

    private BeachData? _selectedBeach = null;
    public BeachData? SelectedBeach
    {
        get => _selectedBeach;
        set { this.RaiseAndSetIfChanged(ref _selectedBeach, value); }
    }

    public bool UserIsAdmin { get => StaticData.UserRole == AppRoleEnum.Admin; }
    public bool UserHasViewRole { get => (int)StaticData.UserRole < (int)AppRoleEnum.Edit; }
    public bool UserHasEditRole { get => StaticData.UserRole >= AppRoleEnum.Edit; }



    public ObservableCollection<BeachData> Beaches { get; } = new();

    #region CTOR
    public BeachesViewModel()
    {
        Instance = this;
        InitializeBeaches();
        PropertyChanged += BeachViewModel_PropertyChanged;
    }

    #endregion CTOR

    private void BeachViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ActiveOnly):
                FilterBeachList(ActiveOnly);
                break;

        }
    }
    private void FilterBeachList(bool activeOnly)
    {
        Beaches.Clear();
        foreach (var beach in StaticData.Beaches.Where(n => n.IsCurrentlyMonitored.Value || !activeOnly))
        {
            Beaches.Add(beach);
        }
    }

    private async void InitializeBeaches()
    {
        if (StaticData.Beaches is null || StaticData.Beaches.Count == 0)
        {
            StaticData.Beaches = await BeachDataCrud.ReadAllBeachDataAsync(StaticData.DataSourceConfig);
        }
        FilterBeachList(ActiveOnly);
    }

    /// <summary>
    /// The Title of this page
    /// </summary>
    public string PageTitle => "Beach Inventory";

    /// <summary>
    /// The content of this page
    /// </summary>
    public string Message => "Beaches";

}
