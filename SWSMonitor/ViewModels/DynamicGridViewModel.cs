using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using DataLibrary.Crud;
using DataLibrary.DataSources.Json;
using DataLibrary.Models;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWSMonitor.ViewModels;

internal class DynamicGridViewModel : ViewModelBase, INotifyPropertyChanged
{

    public MainWindowModel? _mainWindow = null;

    public static DynamicGridViewModel? Instance = null;
    public static DynamicGridView? ViewInstance = null;
    public static DynamicView? DataViewInstance { get; set; }

    private readonly ErrorsViewModel _errorsViewModel;

    #region CTOR
    public DynamicGridViewModel()
    {
        DynamicGridViewModel.Instance = this;
        MainWindowModel main = StaticData.MainWindowModel as MainWindowModel;
        _mainWindow = main;

        _errorsViewModel = new ErrorsViewModel();
        _errorsViewModel.ErrorsChanged += ErrorsViewModel_ErrorsChanged;
        PropertyChanged += DynamicGridViewModel_PropertyChanged;
        SelectedView = string.Empty;
        _ = Task.Run(async () => await GenerateViewSelectionGrid());
    }
    #endregion CTOR

    private void DynamicGridViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
    }

    #region Dynamic Grid Properties

    public string PageTitle => "Available Data Views";

    public string SelectedView { get; set; }

    private bool _isInProgress = false;
    public bool IsInProgress
    {
        get => _isInProgress;
        set
        {
            _isInProgress = value;
            this.RaisePropertyChanged(nameof(IsInProgress));
        }
    }
    private bool _isPopupOpen = false;
    public bool IsPopupOpen { 
        get => _isPopupOpen;
        set { 
            _isPopupOpen = value;
            this.RaisePropertyChanged(nameof(IsPopupOpen));
        }
    }

    // The collection holding your dynamic rows
    public ObservableCollection<IDictionary<string, object>> Records { get; set; } = new();

    // Track headers so the View knows which columns to build
    public List<string> Headers { get; set; } = new();

    private int _recordCount = 0;
    public int RecordCount { 
        get => _recordCount;
        set { _recordCount = value; this.RaisePropertyChanged(nameof(RecordCount)); this.RaisePropertyChanged(nameof(RecordCountMessage)); }
                
    }

    public string RecordCountMessage
    {
        get => $"{RecordCount} records";
    }

    private async Task GenerateViewSelectionGrid()
    {
        try
        {
            var datarecord = await ViewRecordCrud.ReadView(StaticData.DataSourceConfig, ViewRecord.AvailableViewName);
            if (datarecord is null || !datarecord.Records.Any())
                return;

            // Marshal updates back to the UI thread
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Records.Clear();
                // We can assume the headers are the same for all rows
                Headers = datarecord.Records.First().Fields.Keys.ToList();
                Headers.Add("Count");

                foreach (var record in datarecord.Records)
                {
                    string viewname = record.Fields["AvailableView"]?.ToString() ?? "";
                    int count = datarecord.RecordCounts[viewname];
                    record.Fields["Count"] = count;
                    Records.Add((IDictionary<string, object>)record.Fields);
                }
                this.RaisePropertyChanged(nameof(Records));
            });
        }
        catch (Exception ex)
        {
            _errorsViewModel.AddError(nameof(GenerateViewSelectionGrid), ex.Message);
        }
    }
    #endregion Dynamic Grid Properties

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
    }

    internal async Task DownloadView(string viewName, Button? button)
    {
        if (button is not null && (!string.IsNullOrEmpty(viewName) || !viewName.EndsWith("view")))
        {
            var topLevel = TopLevel.GetTopLevel(button);
            if (topLevel == null) return;


            await GenerateDownloadAsync(viewName, topLevel);
            return;
        }
    }

    internal async Task<bool> GenerateDataForViewAsync(string viewName, int recordCount)
    {
        _mainWindow.ShowBusyPopup($"Loading {viewName.Replace("view","")}: {recordCount} records");
        var datarecord = await GenerateViewData(viewName);
        if (datarecord is null || !datarecord.Records.Any()) return false;
        if (DataViewInstance is null) return false;
        RecordCount = datarecord.Records.Count;

        _mainWindow.ShowBusyPopup($"{RecordCount} data records loaded.");
        Task.Delay(100);

        var dvm = new DynamicViewModel();
        if (dvm is not null)
        {

            dvm.DataHeaders = datarecord.Records.First().Fields.Keys.ToList();
            foreach (var record in datarecord.Records)
            {
                dvm.DataRecords.Add((IDictionary<string, object>)record.Fields);
            }
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                DataViewInstance.DataContext = dvm;
                dvm.RaisePropertyChanged(nameof(dvm.DataRecords));
            });
        }
        return true;
    }

    private async Task GenerateDownloadAsync(string viewName, TopLevel topLevel)
    {
        // 2. Open the platform-native Save File dialog via Avalonia StorageProvider
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save CSV Export File",
            DefaultExtension = ".csv",
            SuggestedFileName = viewName.Replace("view", ""),
            FileTypeChoices = new[]
            {
                new FilePickerFileType("CSV Files (*.csv)") { Patterns = new[] { "*.csv" } }
            }
        });
        try
        {
            if (file is null) return;

            var datarecord = await GenerateViewData(viewName);
            if (datarecord is null || !datarecord.Records.Any()) return;
            string csv = Json2CsvConverter.ConvertViewRecord2CsvString(datarecord);
            if (OperatingSystem.IsWindows())
            {
                // Convert the Avalonia Uri to a local string format path
                string destinationPath = file.Path.LocalPath;
                destinationPath = destinationPath.Replace("/", "\\");
                File.WriteAllText(destinationPath, csv, Encoding.UTF8);
            }
            else if (OperatingSystem.IsBrowser())
            {
                byte[] content = Encoding.UTF8.GetBytes(csv);

                await using var stream = await file.OpenWriteAsync();
                // Write your data into the stream here
                await stream.WriteAsync(content.AsMemory(0, content.Length));

                //string destinationPath = file.Name;
                //ServiceProvider serviceProvider = StaticData.ServiceProvider as ServiceProvider;
                //IDownloadService downloadService = serviceProvider.GetRequiredService<IDownloadService>();
                //if (downloadService != null)
                //{
                //    var base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(csv));
                //    downloadService.DownloadFile(destinationPath, "text/csv", base64Content);
                //}
            }
        }
        catch (Exception ex)
        {
            TraceLogger.LogErrorAuto(ex.Message);
            _errorsViewModel.AddError(nameof(GenerateDownloadAsync), ex.Message);
        }
    }
    private async Task<ViewRecord?> GenerateViewData(string viewName) 
    {
        var datarecord = await ViewRecordCrud.ReadView(StaticData.DataSourceConfig, viewName);
        return datarecord;
    }
    #endregion INotifyDataErrorInfo
}
