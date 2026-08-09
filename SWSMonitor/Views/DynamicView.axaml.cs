using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using SWSMonitor.ViewModels;
using SWSMonitor.Views;
using System;

namespace SWSMonitor;

public partial class DynamicView : UserControl
{
    public MainWindowModel? _mainWindow = null;
    public DynamicView()
    {
        MainWindowModel main = StaticData.MainWindowModel as MainWindowModel;
        _mainWindow = main;

        InitializeComponent();
        DynamicGridViewModel.DataViewInstance = this;
        // Setup columns after the DataContext is ready
        this.DataContextChanged += OnDataContextChanged;

    }
    protected override async void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        await _mainWindow?.ShowBusyPopup("Loading Dynamic View...");
        base.OnAttachedToVisualTree(e);
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
    }


    private async void OnDataContextChanged(object? sender, EventArgs e)
    {
        try
        {
            if (DataContext is DynamicViewModel vm)
            {
                var source = DynamicViewGrid.ItemsSource;
                DynamicViewGrid.ItemsSource = null;
                // Clear any previously generated columns 
                DynamicViewGrid.Columns.Clear();
                foreach (var header in vm.DataHeaders)
                {
                    // Dictionary<string, object> records have keys that match the exact header name
                    var column = new DataGridTextColumn
                    {
                        Header = header,
                        Binding = new Binding($"[{header}]") { Mode = BindingMode.OneWay }, // Binds to the dictionary indexer key
                    };

                    DynamicViewGrid.Columns.Add(column);
                }
                DynamicViewGrid.ItemsSource = source;
                await _mainWindow?.ShowNoBusyPopup();
            }
        }
        catch (Exception ex)
        {

        }
    }

}