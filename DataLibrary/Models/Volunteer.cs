using DataLibrary;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Models;

public class VolunteerBase
{
    public const string TableName = "Volunteers";

    public int ID { get; set; }
    public DateTime? EntryDate { get; set; }

    private string? _appRole = "";
    public string? AppRole
    {
        get => _appRole;
        set
        {
            if (_appRole != value)
            {
                _appRole = value;
            }
        }
    }

    [JsonIgnore]
    public AppRoleEnum Privilege
    {
        get
        {
            TraceLogger.LogWarningAuto($"AppRole '{AppRole}' ");
            if (string.IsNullOrEmpty(AppRole)) return AppRoleEnum.Public;
            switch(AppRole.ToLower())
            {
                case "admin":
                    return AppRoleEnum.Admin;
                case "edit":
                    return AppRoleEnum.Edit;
                case "view":
                    return AppRoleEnum.View;
            }
            return AppRoleEnum.Public;
        }
    }

    private string? _firstLast = "";
    public string FirstLast
    {
        get => _firstLast!;
        set
        {
            if (_firstLast != value)
            {
                _firstLast = value;
            }
        }
    }

    private string? _city;
    public string? City
    {
        get => _city;
        set
        {
            if (_city != value)
            {
                _city = value;
            }
        }
    }

    private string? _island;
    public string? Island
    {
        get => _island;
        set
        {
            if (_island != value)
            {
                _island = value;
            }
        }
    }

    private string? _email = "";
    public string? Email
    {
        get => _email;
        set
        {
            if (_email != value)
            {
                _email = value;
            }
        }
    }

    public int _active;
    public int Active
    {
        get => _active;
        set
        {
            if (_active != value)
            {
                _active = value;
            }
        }
    }
    public int _lead;
    public int Lead
    {
        get => _lead;
        set
        {
            if (_lead != value)
            {
                _lead = value;
            }
        }
    }
    public int _speciesExpert;
    public int SpeciesExpert
    {
        get => _speciesExpert;
        set
        {
            if (_speciesExpert != value)
            {
                _speciesExpert = value;
            }
        }
    }
}


/// <summary>
/// Represents a Volunteers.
/// NOTE: This class is generated from a T4 template - you should not modify it manually.
/// </summary>
public class Volunteer : VolunteerBase, ITableBase, INotifyPropertyChanged
{
 
    public event PropertyChangedEventHandler? PropertyChanged;

    private string? _firstName;
    public string? FirstName
    {
        get => _firstName;
        set
        {
            if (_firstName != value)
            {
                _firstName = value;
                RaisePropertyChanged(nameof(FirstName));
            }
        }
    }
    private string? _lastName;
    public string? LastName
    {
        get => _lastName;
        set
        {
            if (_lastName != value)
            {
                _lastName = value;
                RaisePropertyChanged(nameof(LastName));
            }
        }
    }

    [JsonIgnore]
    public  string PreferredPhone
    {
        get => (string.IsNullOrEmpty(_cellPhone)? _phone : _cellPhone) ?? string.Empty;
    }
    private string? _phone;
    public string? Phone
    {
        get => _phone;
        set
        {
            if (_phone != value)
            {
                _phone = value;
                RaisePropertyChanged(nameof(Phone));
            }
        }
    }

    private string? _cellPhone;
    public string? CellPhone
    {
        get => _cellPhone;
        set
        {
            if (_cellPhone != value)
            {
                _cellPhone = value;
                RaisePropertyChanged(nameof(CellPhone));
            }
        }
    }



    private string? _address;
    public string? Address
    {
        get => _address;
        set
        {
            if (_address != value)
            {
                _address = value;
                RaisePropertyChanged(nameof(Address));
            }
        }
    }

    private string? _state;
    public string? State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                RaisePropertyChanged(nameof(State));
            }
        }
    }
    private double? _zip;
    public double? Zip
    {
        get => _zip;
        set
        {
            if (_zip != value)
            {
                _zip = value;
                RaisePropertyChanged(nameof(Zip));
            }
        }
    }


    [JsonIgnore]
    public bool IsActive
    {
        get => _active == 1;
        set
        {
            _active = value? 1 : 0;
            RaisePropertyChanged(nameof(Active));
            RaisePropertyChanged(nameof(IsActive));
        }
    } 
    [JsonIgnore]
    public bool IsLead
    {
        get => _lead == 1;
        set
        {
            _lead = value ? 1 : 0;
            RaisePropertyChanged(nameof(Lead));
            RaisePropertyChanged(nameof(IsLead));
        }
    }

    [JsonIgnore]
    public bool IsSpeciesExpert
    {
        get => _speciesExpert == 1;
        set
        {
            _speciesExpert = value ? 1 : 0;
            RaisePropertyChanged(nameof(SpeciesExpert));
            RaisePropertyChanged(nameof(IsSpeciesExpert));
        }
    }

    private DateTime? _startDate;
    public DateTime? StartDate
    {
        get => _startDate;
        set
        {
            if (_startDate != value)
            {
                _startDate = value;
                RaisePropertyChanged(nameof(StartDate));
            }
        }
    }

    private string _formattedStartDate = "";
    [JsonIgnore]
    public string FormattedStartDate
    {
        get
        {
            if (StartDate.HasValue)
            {
                _formattedStartDate = StartDate.Value.ToString("yyyy");
            }
            else
            {
                _formattedStartDate = "";
            }
            return _formattedStartDate;
        }
    }


    private string? _volunteerNotes;
    public string? VolunteerNotes
    {
        get => _volunteerNotes;
        set
        {
            if (_volunteerNotes != value)
            {
                _volunteerNotes = value;
                RaisePropertyChanged(nameof(VolunteerNotes));
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Volunteer"/> class.
    /// </summary>
    public Volunteer()
    {
        // Initialize properties if needed
    }

    public void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (propertyName is not null)
        {
            // Invoke the property changed event
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
