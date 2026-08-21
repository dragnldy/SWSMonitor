using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SWSMonitor.ViewModels;
using System.Threading.Tasks;

namespace SWSMonitor;

public partial class SplashScreenView : ReactiveUserControl<SplashScreenViewModel>
{
    public MainWindowModel? _mainWindow = null;

    public SplashScreenView()
    {
        MainWindowModel main = StaticData.MainWindowModel as MainWindowModel;
        _mainWindow = main;

        this.WhenActivated((ReactiveUI.Primitives.Disposables.MultipleDisposable disposables) => { });
        AvaloniaXamlLoader.Load(this);
    }

    public async Task SetBusy(bool isBusy)
    {
        //this.Cursor = isBusy
        //    ? new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Wait)
        //    : Avalonia.Input.Cursor.Default;
        if (isBusy)
            await _mainWindow.ShowBusyPopup("Loading Splash Screen...");
        else
            await _mainWindow.ShowNoBusyPopup();
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (ViewModel != null)
        {
            ViewModel.OnLoad(this);
        }

        await SetBusy(false);
        // Control is fully ready, layout has occurred, and templates are applied.
        // Call the ViewModel's OnLoad to get device and window dimensions
    }


    private async void Login_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (DataContext is SplashScreenViewModel viewModel)
        {
            if (!StaticData.UserCanLogin)
            {
                // SHouldn't be able to get here as the login button isn't displayed 
                TraceLogger.LogErrorAuto("User attempted to login from nonsupported browser");
                return;
            }
            await viewModel.DoLoginAsync();
        }
    }
}
 