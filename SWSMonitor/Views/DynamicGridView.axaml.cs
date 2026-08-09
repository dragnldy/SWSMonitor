using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using DataLibrary;
using SWSMonitor.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWSMonitor;

public partial class DynamicGridView : UserControl
{
    public MainWindowModel? _mainWindow = null;

    public DynamicGridView()
    {
        MainWindowModel main = StaticData.MainWindowModel as MainWindowModel;
        _mainWindow = main;

        InitializeComponent();
        DynamicGridViewModel.ViewInstance = this;
        this.Loaded += DynamicGridView_Loaded;
        // Setup columns after the DataContext is ready
        this.DataContextChanged += OnDataContextChanged;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
    }

    private void DynamicGridView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        //        DataContext = DynamicGridViewModel.Instance;
    }

    private async void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is DynamicGridViewModel vm)
        {
            // Clear any previously generated columns 
            for (int i = DynamicDataGrid.Columns.Count - 1; i >= 0; i--)
            {
                if (DynamicDataGrid.Columns[i] is DataGridTextColumn)
                {
                    DynamicDataGrid.Columns.RemoveAt(i);
                }
            }

            foreach (var header in vm.Headers)
            {
                // Dictionary<string, object> records have keys that match the exact header name
                var column = new DataGridTextColumn
                {
                    Header = header,
                    Binding = new Binding($"[{header}]") { Mode = BindingMode.OneWay }, // Binds to the dictionary indexer key
                };

                DynamicDataGrid.Columns.Add(column);
            }

            // Bind the population matrix to the Grid's main source
            // DynamicDataGrid.ItemsSource = vm.Records;
            await _mainWindow?.ShowNoBusyPopup();
        }
        // Control is fully ready, layout has occurred, and templates are applied.
    }

    private void DataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (e.Source is TextBlock tb)
        {
            if (!string.IsNullOrEmpty(tb.Text) && DataContext is DynamicGridViewModel vm)
            {
                vm.SelectedView = tb.Text;
            }
        }
    }

    private async void DownloadButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Implement your download logic here using the viewName
        var viewName = GetViewName(sender, e);
        if (DataContext is DynamicGridViewModel vm)
        {
            _mainWindow.ShowBusyPopup($"Downloading {viewName.Replace("view","")}: {_currentCount} records");
            await vm.DownloadView(viewName, sender as Button);
            _mainWindow.ShowNoBusyPopup();
        }
    }

    int _currentCount;
    string _currentView;
    private string GetViewName(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _currentCount = 0;
        _currentView = string.Empty;

        if (e is not null && e.Source is Button btn && btn.DataContext is IDictionary<string, object> record)
        {
            if (record.TryGetValue("AvailableView", out var viewNameObj))
            {
                if (viewNameObj != null)
                    _currentView = viewNameObj.ToString();
            }
            if (record.TryGetValue("Count", out var viewCount))
            {
                if (viewCount != null)
                {
                    _currentCount = int.Parse(viewCount.ToString());
                }
            }
        }
        return _currentView;
    }
    private async void ViewButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var viewName = GetViewName(sender, e);
        if (!string.IsNullOrEmpty(viewName))
        {
            await GetViewDataAsync(viewName);
        }
    }

    private async Task GetViewDataAsync(string viewName)
    {
        if (DataContext is DynamicGridViewModel vm)
        {
            //            vm.IsInProgress = true;
            var csv = await vm.GenerateDataForViewAsync(viewName, _currentCount);
            vm.IsPopupOpen = true;
            _mainWindow.ShowNoBusyPopup();
        }
    }

    private async void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is DynamicGridViewModel vm)
        {
            vm.IsPopupOpen = false;
        }

        await _mainWindow.ShowNoBusyPopup();
    }
}