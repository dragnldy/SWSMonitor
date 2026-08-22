using DataLibrary.Crud;
using Models;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace SWSMonitor.ViewModels;

public partial class GlossariesViewModel : ViewModelBase, INotifyPropertyChanged
{
    public static GlossariesViewModel? Instance;


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

    private string _popupMessage = string.Empty;
    public string PopupMessage
    {
        get => _popupMessage;
        set { this.RaiseAndSetIfChanged(ref _popupMessage, value); }
    }


    // this is for error messages
    private bool _popupIsOpen = false;
    public bool PopupIsOpen
    {
        get => _popupIsOpen;
        set { this.RaiseAndSetIfChanged(ref _popupIsOpen, value); }
    }

    // Can't change the name of existing species because it might be in use in previous surveys
    private bool _canEditScientificName = false;
    public bool CanEditScientificName
    {
        get => _canEditScientificName || !UserIsAdmin;
        set { this.RaiseAndSetIfChanged(ref _canEditScientificName, value); }
    }

    private bool _isPopupOpen = false;
    public bool IsPopupOpen
    {
        get => _isPopupOpen;
        set { this.RaiseAndSetIfChanged(ref _isPopupOpen, value); }
    }

    private bool _isReadOnlyPopupOpen = false;
    public bool IsReadOnlyPopupOpen
    {
        get => _isReadOnlyPopupOpen;
        set { this.RaiseAndSetIfChanged(ref _isReadOnlyPopupOpen, value); }
    }


    private Species? _selectedSpecies = null;
    public Species? SelectedSpecies
    {
        get => _selectedSpecies;
        set { this.RaiseAndSetIfChanged(ref _selectedSpecies, value); }
    }

    public bool UserIsAdmin { get => StaticData.UserRole == AppRoleEnum.Admin; }
    public bool UserHasViewOnlyRole { get => (int)StaticData.UserRole < (int)AppRoleEnum.Edit; }
    public bool UserHasEditRole { get => StaticData.UserRole >= AppRoleEnum.Edit; }


    public ObservableCollection<Species> Species { get; } = new();



    #region CTOR
    public GlossariesViewModel()
    {
        Instance = this;
        InitializeSpecies();
        PropertyChanged += GlossariesViewModel_PropertyChanged;
    }
    #endregion CTOR

    private void GlossariesViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ActiveOnly):
                FilterSpeciesList(ActiveOnly);
                break;

        }
    }
    
    private string _tsnFilterText = string.Empty;
    public string TsnFilterText
    {
        get => _tsnFilterText;
        set
        {
            if (_tsnFilterText != value)
            {
                this.RaiseAndSetIfChanged(ref _tsnFilterText, value);
                TsnFilter();
            }
        }
    }

    private string _scientificNameFilterText = string.Empty;
    public string ScientificNameFilterText
    {
        get => _scientificNameFilterText;
        set
        {
            if (_scientificNameFilterText != value)
            {
                this.RaiseAndSetIfChanged(ref _scientificNameFilterText, value);
                ScientificNameFilter();
            }
        }
    }

    private string _commonNameFilterText = string.Empty;
    public string CommonNameFilterText
    {
        get => _commonNameFilterText;
        set
        {
            if (_commonNameFilterText != value)
            {
                this.RaiseAndSetIfChanged(ref _commonNameFilterText, value);
                CommonNameFilter();
            }
        }
    }

    private void TsnFilter()
    {
        Species.Clear();
        var filtered = StaticData.Species
            .Where(n => n.TSN.HasValue && n.TSN.ToString().IndexOf(TsnFilterText, StringComparison.OrdinalIgnoreCase) != -1)
            .OrderBy(n => n.ScientificName);
        foreach (var species in filtered)
        {
            Species.Add(species);
        }
    }
    private void ScientificNameFilter()
    {
        Species.Clear();
        var filtered = StaticData.Species
            .Where(n => n.ScientificName.IndexOf(ScientificNameFilterText, StringComparison.OrdinalIgnoreCase) != -1)
            .OrderBy(n => n.ScientificName);
        foreach (var species in filtered)
        {
            Species.Add(species);
        }
    }
    private void CommonNameFilter()
    {
        Species.Clear();
        var filtered = StaticData.Species
            .Where(n => !string.IsNullOrEmpty(n.CommonNameOrDescription) && n.CommonNameOrDescription.IndexOf(CommonNameFilterText, StringComparison.OrdinalIgnoreCase) != -1)
            .OrderBy(n => n.ScientificName);
        foreach (var species in filtered)
        {
            Species.Add(species);
        }
    }

    private void FilterSpeciesList(bool activeOnly)
    {
        Species.Clear();
        var filteredSpecies = activeOnly
            ? StaticData.Species.Where(s => s.IsUsedBySurveys).OrderBy(s => s.ScientificName)
            : StaticData.Species.OrderBy(s => s.ScientificName);
        foreach (var species in filteredSpecies)
        {
            Species.Add(species);
        }
    }
    private async void InitializeSpecies()
    {
        if (StaticData.Species is null || StaticData.Species.Count == 0)
        {
            StaticData.Species = await SpeciesCrud.ReadAllSpeciesAsync(StaticData.DataSourceConfig);
        }
        FilterSpeciesList(ActiveOnly);

        
    }

    internal void ReloadSpecies()
    {
        Species.Clear();
        foreach (var species in StaticData.Species)
        {
            Species.Add(species);
        }
    }

    /// <summary>
    /// The Title of this page
    /// </summary>
    public string PageTitle => "Species Glossary Management";

    /// <summary>
    /// The content of this page
    /// </summary>
    public string Message => "Add and Edit Species";

}