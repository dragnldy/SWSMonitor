using Avalonia.Markup.Xaml;
using SWSMonitor.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace SWSMonitor;

public partial class SettingsView : ReactiveUserControl<SettingsViewModel>
{
    public SettingsView()
    {
        // InitializeComponent();
        this.WhenActivated((ReactiveUI.Primitives.Disposables.MultipleDisposable disposables) => { }); 
        AvaloniaXamlLoader.Load(this);
    }

    private void DoArchive(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Create zipped json files and upload to Google Drive
        this.ViewModel!.ArchiveProgressMessage = string.Empty;
        this.ViewModel!.DoArchive();
    }
    private void DoImport(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.ViewModel!.ArchiveProgressMessage = string.Empty;
        this.ViewModel!.DoArchive();
    }

    // Create CSV file of quadrat for export to UW
    private void DoUWExport(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.ViewModel!.ArchiveProgressMessage = string.Empty;
        this.ViewModel!.DoArchive();
    }
    private void OpenUWExport_PopUp(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.ViewModel!.SetupParameters();
        this.ViewModel!.IsPopupOpen = true;
    }

    private void ClosePopup_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.ViewModel!.IsPopupOpen = false;
    }

    private void ExportToUW_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }

}