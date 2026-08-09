using Avalonia.Interactivity;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SWSMonitor.ViewModels;

public partial class SettingsViewModel : ViewModelBase, INotifyPropertyChanged
{
    private bool _requireAuthentication =  false;
    public bool RequireAuthentication
    {
        get => _requireAuthentication;
        set {
                this.RaiseAndSetIfChanged(ref _requireAuthentication, value);
                StaticData.RequireAuthentication = value;
        }
    }
    private bool _requireAuditTrail = false;
    public bool RequireAuditTrail
    {
        get => _requireAuditTrail;
        set
        {
            this.RaiseAndSetIfChanged(ref _requireAuditTrail, value);
            StaticData.RequireAuditTrail = value;
        }
    }

    private bool _includeGlobalDataInArchive = true;
    public bool IncludeGlobalDataInArchive
    {
        get => _includeGlobalDataInArchive;
        set => this.RaiseAndSetIfChanged(ref _includeGlobalDataInArchive, value);
    }

    private bool _includeSurveyDataInArchive = false;
    public bool IncludeSurveyDataInArchive
    {
        get => _includeSurveyDataInArchive;
        set => this.RaiseAndSetIfChanged(ref _includeSurveyDataInArchive, value);
    }

    public string _archiveStatusMessage = "Ready to archive.";
    public string ArchiveStatusMessage
    {
        get => _archiveStatusMessage;
        set => this.RaiseAndSetIfChanged(ref _archiveStatusMessage, value);
    }
    public string _archiveProgressMessage = string.Empty;
    public string ArchiveProgressMessage
    {
        get => _archiveProgressMessage;
        set => this.RaiseAndSetIfChanged(ref _archiveProgressMessage, value);
    }

    private int _archiveProgressValue = 0;
    public int ArchiveProgressValue
    {
        get => _archiveProgressValue;
        set => this.RaiseAndSetIfChanged(ref _archiveProgressValue, value);
    }

    private bool _archiveInProgress = false;
    public bool ArchiveInProgress
    {
        get => _archiveInProgress;
        set => this.RaiseAndSetIfChanged(ref _archiveInProgress, value);
    }

    private bool _isPopupOpen = false;
    public bool IsPopupOpen
    {
        get => _isPopupOpen;
        set
        {
            if (_isPopupOpen != value)
            {
                _isPopupOpen = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _canSave = false;
    public bool CanSave
    {
        get => _canSave;
        set
        {
            if (_canSave != value)
            {
                _canSave = value;
                OnPropertyChanged();
            }
        }
    }

    // Newly added properties for UWExport
    private DateTime? _uwExportStartDate;
    public DateTime? UWExportStartDate
    {
        get => _uwExportStartDate;
        set
        {
            if (_uwExportStartDate != value)
            {
                _uwExportStartDate = value;
                OnPropertyChanged();
            }
        }
    }

    private DateTime? _uwExportEndDate;
    public DateTime? UWExportEndDate
    {
        get => _uwExportEndDate;
        set
        {
            if (_uwExportEndDate != value)
            {
                _uwExportEndDate = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _uwExportIncludeHeader = true;
    public bool UWExportIncludeHeader
    {
        get => _uwExportIncludeHeader;
        set
        {
            if (_uwExportIncludeHeader != value)
            {
                _uwExportIncludeHeader = value;
                OnPropertyChanged();
            }
        }
    }

    public SettingsViewModel()
    {
        // Initialize properties from StaticData
        RequireAuthentication = StaticData.RequireAuthentication;
        RequireAuditTrail = StaticData.RequireAuditTrail;
    }

    internal void DoArchive()
    {
        //ArchiveProgressValue = 0;
        //Archiver archiver = new Archiver();
        //DriveService? driveService = archiver.InitializeGoogleDriveConnector();
        //if (IncludeGlobalDataInArchive)
        //{
        //    ArchiveStatusMessage = "Archiving Global Data to Google Drive...";
        //    archiver.ArchiveGlobalData();
        //    ArchiveStatusMessage = "Global Data Archived";
        //}
        //if (IncludeSurveyDataInArchive)
        //{
        //    ArchiveStatusMessage = "Archiving Survey Data to Google Drive...";
        //    ArchiveInProgress = true;
        //    ArchiveProgressValue = 0;
        //    StartTimer();
        //    _ = archiver.ArchiveStudyData();
        //}
    }
    internal void StartTimer()
    {
        //var timer = new DispatcherTimer
        //{
        //    Interval = TimeSpan.FromMilliseconds(50)
        //};

        //timer.Tick += (s, e) =>
        //{
        //    if (_signalCompletedFunction())
        //    {
        //        ArchiveStatusMessage = "Survey Data Archived";
        //        ArchiveInProgress = false;
        //        timer.Stop();
        //    }
        //    else
        //    {
        //       ArchiveProgressValue = Archiver.ProgressValue;
        //    }
        //};

        //timer.Start();
    }

    //private bool _signalCompletedFunction()
    //{
    //    return Archiver.ProgressValue >= 100;
    //}

    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal void SetupParameters()
    {

    }
}