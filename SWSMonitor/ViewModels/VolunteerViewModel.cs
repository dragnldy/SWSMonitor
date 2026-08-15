using DataLibrary.Crud;
using Models;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SWSMonitor.ViewModels;

public class Role
{
    public string RoleName { get; set; }
    public AppRoleEnum RoleEnum { get; set; }
    public Role(string name, AppRoleEnum enums)
    {
        RoleName = name;
        RoleEnum = enums;
    }
}
public class VolunteerViewModel : ReactiveObject, INotifyDataErrorInfo
{
    public static VolunteerViewModel? Instance = null;

    private readonly ErrorsViewModel _errorsViewModel;
    private PeopleViewModel? _parentViewModel = PeopleViewModel.Instance;


    private bool isDirty = false;
    private bool isLoading = true;

    #region Load and Save Volunteer
    public async Task<(bool,Volunteer)> SaveVolunteer()
    {
        if (string.IsNullOrEmpty(FirstLast))
            return (false, new Volunteer() { ID = -1, FirstLast = "Failed" });

        // Update existing volunteer
        var existing = StaticData.Volunteers.FirstOrDefault((n => n.ID == ID));
        if (existing == null)
        {
            existing = new Volunteer() { ID = 0, FirstLast = FirstLast };
        }
        DataHelper.CopyProperties<VolunteerViewModel, Volunteer>(this, existing);
        (bool success, Volunteer updated) = await VolunteersCrud.UpdateOrCreateVolunteerAsync(StaticData.DataSourceConfig, existing);
        isDirty = false;
        return (true, updated);
    }
    public bool LoadTargetVolunteer(string firstLast)
    {
        isLoading = true;
        IsExistingVolunteer = true;

        Volunteer volunteer = StaticData.Volunteers.FirstOrDefault(n => n.FirstLast.Equals(firstLast, StringComparison.InvariantCulture));
        if (volunteer == null)
        {
            Console.WriteLine($"Missing volunteer info. {firstLast}");
            TraceLogger.LogWarningAuto("Beep");
        }
        return LoadTargetVolunteer(volunteer, isExisting: true);
    }

    public bool LoadTargetVolunteer(Volunteer? volunteer, bool isExisting = true)
    {
        isLoading = true;
        IsExistingVolunteer = isExisting;
        if (volunteer == null)
        {
            // Need to clear out existing data
            volunteer = new Volunteer() { ID = -1, FirstLast = string.Empty };
        }
        DataHelper.CopyProperties<Volunteer, VolunteerViewModel>(volunteer, this);
        _errorsViewModel.ClearErrors();

        // Set SelectedAppRole based on AppRole string
        var role = _appRoles.FirstOrDefault(r => r.RoleName.Equals(_appRole, StringComparison.InvariantCultureIgnoreCase));
        if (role != null)
        {
            _selectedAppRole = role;
        }
        else
        {
            _selectedAppRole = _appRoles[0]; // default to Public
        }
        isLoading = false;
        isDirty = false;
        this.RaisePropertyChanged(nameof(IsExistingVolunteer));
        this.RaisePropertyChanged(nameof(SelectedAppRole));
        return true;
    }
    #endregion Load and Save Volunteer


    #region Control properties
    private ObservableCollection<Role> _appRoles = new ObservableCollection<Role>() {

        new Role("Public",AppRoleEnum.Public),
        new Role("Viewer", AppRoleEnum.View),
        new Role("Editor", AppRoleEnum.Edit)
    };

    [JsonIgnore]
    public ObservableCollection<Role> AppRoles { get => _appRoles; }

    private bool _isExistingVolunteer = false;
    [JsonIgnore]
    public bool IsExistingVolunteer
    {
        get => _isExistingVolunteer;
        set
        {
            this.RaiseAndSetIfChanged(ref _isExistingVolunteer, value);
        }
    }

    private Role _selectedAppRole = new Role("Public", AppRoleEnum.Public);
    [JsonIgnore]
    public Role SelectedAppRole
    {
        get => _selectedAppRole;
        set
        {
            if (value != _selectedAppRole)
            {
                SetIsDirty();
                if (value is not null)
                    _appRole = value.RoleName;
            }
            this.RaiseAndSetIfChanged(ref _selectedAppRole, value);
        }
    }

    private IEnumerable<string> _citystates = new List<string>();
    [JsonIgnore]
    public IEnumerable<string> CityStates
    {
        get => _citystates;
        set { if (_citystates != value) { _citystates = value; OnPropertyChanged(nameof(CityStates)); } }
    }

    private string _selectedCityState = string.Empty;
    [JsonIgnore]
    public string SelectedCityState
    {
        get => _selectedCityState;
        set { if (_selectedCityState != value) {
                _selectedCityState = value;
                if (!string.IsNullOrEmpty(value) && value.Contains('\t'))
                {
                    var parts = value.Split('\t');
                    if (parts.Length == 2)
                    {
                        City = parts[0];
                        State = parts[1];
                    }
                    _selectedCityState = City;
                    this.RaisePropertyChanged(nameof(City));
                    this.RaisePropertyChanged(nameof(State));
                }
                OnPropertyChanged(nameof(SelectedCityState)); } }
    }
    #endregion Control properties

    #region CTOR
    public VolunteerViewModel()
    {
        VolunteerViewModel.Instance = this;
        _errorsViewModel = new ErrorsViewModel();
        _errorsViewModel.ErrorsChanged += ErrorsViewModel_ErrorsChanged;


        if (StaticData.UserRole == AppRoleEnum.Admin)
        {
            // Only show Admin role if current user is Admin
            _appRoles.Add(new Role("Admin", AppRoleEnum.Admin));
        }
        CityStates = StaticData.CityStates!.Select(cs => $"{cs.City}\t{cs.State}");
        PropertyChanged += VolunteerViewModel_PropertyChanged;
    }
    #endregion CTOR

    #region Data Properties
    private int _id = -1;
    public int ID
    {
        get => _id;
        set
        {
            if (value != _id) { SetIsDirty(); }
            this.RaiseAndSetIfChanged(ref _id, value);
        }
    }

    private string _appRole = string.Empty;
    public string AppRole
    {
        get => _appRole;
        set
        {
            if (value != _appRole) { SetIsDirty(); }
            this.RaiseAndSetIfChanged(ref _appRole, value);
        }
    }

    private string _firstlast = string.Empty;
    public string FirstLast
    {
        get => _firstlast;
        // Check if there is an existing volunteer with this first/last- only allowed one instance of it
        set 
        {
            bool foundErrors = false;
            if (!isLoading)
            {
                if (value is not null)
                    value = value.Trim();
                _errorsViewModel.ClearErrors(nameof(FirstLast));

                if (string.IsNullOrEmpty(value))
                {
                    _errorsViewModel.AddError(nameof(FirstLast), "Must supply unique first and last name");
                    foundErrors = true;
                }
                else
                {
                    // Check for duplicates
                    var existing = StaticData.Volunteers.Any(n => n.FirstLast.Equals(value, StringComparison.InvariantCulture) && n.ID != this.ID);
                    if (existing)
                    {
                        _errorsViewModel.AddError(nameof(FirstLast), "User with this name already exists. Please choose a different name.");
                        foundErrors = true;
                    }
                }
                if (!foundErrors && !_firstlast.Equals(value))
                {
                    UpdateFirstLast(value);
                }
            }

            SetIsDirty();
            _parentViewModel.CanSave = !HasErrors;
            this.RaiseAndSetIfChanged(ref _firstlast, value); 
        }
    }

    private void UpdateFirstLast(string newFirstLast)
    {
        if (string.IsNullOrEmpty(newFirstLast))
            return;

        // use new first and last names to update the separate First and Last fields
        // But only if they are currently empty
        (string first, string last) = GetFirstLast(newFirstLast);

        if (string.IsNullOrEmpty(_firstName))
        {
            FirstName = first;
        }
        if (string.IsNullOrEmpty(_lastName))
        {
            LastName = last;
        }
    }

    private (string first, string last) GetFirstLast(string newFirstLast)
    {
        string[] parts = newFirstLast.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0 && parts.Length>=2) 
        {
            return (parts[0], parts[parts.Length -1]);
        }
        return (newFirstLast, string.Empty);
    }

    private void SetIsDirty()
    {
        isDirty = !isLoading;

    }

    private string _firstName = string.Empty;
    public string FirstName
    {
        get => _firstName;
        set
        {
            if (value != _firstName) { SetIsDirty(); }
            this.RaiseAndSetIfChanged(ref _firstName, value);
        }
    }

    private string _lastName = string.Empty;
    public string LastName
    {
        get => _lastName;
        set 
        {
            if (value != _lastName) { SetIsDirty(); }
            this.RaiseAndSetIfChanged(ref _lastName, value);
        }
    }

    private bool IsGoodEmail(string value)
    {
        bool isGood = DataValidation.CleanAndValidateEmail(value);
        if (!isGood)
        {
            TraceLogger.LogWarningAuto("Beep");
        }
        return isGood;
    }

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set
        {
            bool foundErrors = false;
            if (value is not null)
                value = value.Trim();

            if (!isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(Email));

                if (!IsGoodEmail(value))
                {
                    _errorsViewModel.AddError(nameof(Email), "Invalid email");
                    foundErrors = true;
                }
            }

            if (!foundErrors && !isLoading && value != _email)
            {
                SetIsDirty();
            }

            this.RaiseAndSetIfChanged(ref _email, value);
        }
    }

    Regex regexPhone = new Regex(@"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$");
    Regex regexAlt = new Regex(@"^(\+\d{1,2}\s?)?1?\-?\.?\s?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$");
    private bool IsGoodPhone(string value)
    {
        if (string.IsNullOrEmpty(value))
            return true;
        var matches = regexPhone.Match(value);
        if (!matches.Success)
        {
            TraceLogger.LogWarningAuto("Beep");
        }
        return matches.Success;
    }

    private string _phone = string.Empty;
    public string Phone
    {
        get => _phone;
        set
        {
            bool foundErrors = false;
            if (value is not null)
                value = value.Trim();

            if (!isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(Phone));

                if (!IsGoodPhone(value))
                {
                    _errorsViewModel.AddError(nameof(Phone), "Invalid Phone Number");
                    foundErrors = true;
                }
            }

            if (!foundErrors && !isLoading && value != _phone)
            {
                SetIsDirty();
            }

            this.RaiseAndSetIfChanged(ref _phone, value);
        }
    }

    private string _cellphone = string.Empty;
    public string CellPhone
    {
        get => _cellphone;
        set
        {
            bool foundErrors = false;
            if (value is not null)
                value = value.Trim();

            if (!isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(CellPhone));

                if (!IsGoodPhone(value))
                {
                    _errorsViewModel.AddError(nameof(CellPhone), "Invalid Phone Number");
                    foundErrors = true;
                }
            }
   
            if (!foundErrors && !isLoading && value != _cellphone)
            {
                SetIsDirty();
            }
            this.RaiseAndSetIfChanged(ref _cellphone, value);
        }
    }

    private string _address = string.Empty;
    public string Address
    {
        get => _address;
        set
        {
            if (value != _address) { SetIsDirty(); }
            this.RaiseAndSetIfChanged(ref _address, value);
        }
    }

    private string _city = string.Empty;
    public string City
    {
        get => _city;
        set
        {
            if (value != _city) { SetIsDirty(); }
            this.RaiseAndSetIfChanged(ref _city, value);
        }
    }

    private string _state = string.Empty;
    public string State
    {
        get => _state;
        set
        {
            if (value != _state) { SetIsDirty(); }
            this.RaiseAndSetIfChanged(ref _state, value);
        }
    }

    Regex regexZip = new Regex(@"^\d{5}(-\d{4})?$");
    private bool IsGoodZip(string value)
    {
        if (string.IsNullOrEmpty(value))
            return true;
        var matches = regexZip.Match(value);
        if (!matches.Success)
        {
            TraceLogger.LogWarningAuto("Beep");
        }
        return matches.Success;
    }

    private string _zip = string.Empty;
    public string Zip
    {
        get => _zip;
        set
        {
            bool foundErrors = false;
            if (value is not null)
                value = value.Trim();

            if (!isLoading)
            {
                _errorsViewModel.ClearErrors(nameof(Zip));

                if (!IsGoodZip(value))
                {
                    _errorsViewModel.AddError(nameof(Zip), "Invalid Zip Code");
                    foundErrors = true;
                }
            }
            if (!foundErrors && !isLoading && value != _zip)
            {
                SetIsDirty();
            }
            this.RaiseAndSetIfChanged(ref _zip, value);
        }
    }

    private int _lead = 0;
    public int Lead
    {
        get => _lead;
        set
        {
            if (value != _lead) { SetIsDirty(); }
            this.RaiseAndSetIfChanged(ref _lead, value);
        }
    }

    private int _speciesExpert = 0;
    public int SpeciesExpert
    {
        get => _speciesExpert;
        set
        {
            if (value != _speciesExpert)
            {
                SetIsDirty();
            }
            this.RaiseAndSetIfChanged(ref _speciesExpert, value);
        }
    }
    public int _active = 0;
    public int Active
    {
        get => _active;
        set
        {
            if (value != _active)
            {
                SetIsDirty();
            }
            this.RaiseAndSetIfChanged(ref _active, value);
        }
    }

    private string _volunteerNotes = string.Empty;
    public string VolunteerNotes
    {
        get => _volunteerNotes;
        set
        {
            if (value != _volunteerNotes) { SetIsDirty(); }
            this.RaiseAndSetIfChanged(ref _volunteerNotes, value);
        }
    }

    private string _island = string.Empty;
    public string Island
    {
        get => _island;
        set
        {
            if (value != _island) { SetIsDirty(); }
            this.RaiseAndSetIfChanged(ref _island, value);
        }
    }

    #endregion Data Properties

    #region Property Changed Event
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    public event PropertyChangedEventHandler? PropertyChanged;
    private void VolunteerViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
    }


    #endregion Property Changed Event


    #region Search Functions for AutoCompleteBox

    public Func<string?, CancellationToken, Task<IEnumerable<object>>> PopulateCitiesAsync => CitySearchAsync;
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> PopulateStatesAsync => StateSearchAsync;

     private async Task<IEnumerable<object>> CitySearchAsync(string? searchText, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested || string.IsNullOrWhiteSpace(searchText) || StaticData.CityStates is null)
            return Array.Empty<object>();

        var results = StaticData.CityStates
            .Where(cs => !string.IsNullOrEmpty(cs.City) && cs.City.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
            .Select(cs => cs.City!)
            .Distinct()
            .OrderBy(s => s)
            .Cast<object>()
            .ToList();

        return await Task.FromResult(results);
    }

    private async Task<IEnumerable<object>> StateSearchAsync(string? searchText, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested || string.IsNullOrWhiteSpace(searchText) || StaticData.CityStates is null)
            return Array.Empty<object>();

        var results = StaticData.CityStates
            .Where(cs => !string.IsNullOrEmpty(cs.State) && cs.State.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
            .Select(cs => cs.State!)
            .Distinct()
            .OrderBy(s => s)
            .Cast<object>()
            .ToList();

        return await Task.FromResult(results);
    }
    #endregion Search Functions for AutoCompleteBox

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