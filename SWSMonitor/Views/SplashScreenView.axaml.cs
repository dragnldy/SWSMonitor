using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
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
            //var cloudAuthConfig =
            //    StaticData.ServiceProvider?.GetRequiredService<ICloudAuthConfig>()
            //    ?? throw new InvalidOperationException("ICloudAuthConfig not registered in DI container");


            if (!StaticData.UserCanLogin)
            {
                var result = await Dispatcher.InvokeAsync(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard(
                        "Not Supported",
                        "Error logging in as authorized user\n" +
                        "-- Only supported on Chrome\n-- Requires gmail account.",
                        ButtonEnum.Ok);

                    return await box.ShowAsync();
                });
            }
            await viewModel.DoLoginAsync();
        }
    }
    private async void LoginButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is SplashScreenViewModel viewModel)
        {
            //var cloudAuthConfig =
            //    StaticData.ServiceProvider?.GetRequiredService<ICloudAuthConfig>()
            //    ?? throw new InvalidOperationException("ICloudAuthConfig not registered in DI container");


            if (!StaticData.UserCanLogin)
            {
                var result = await Dispatcher.InvokeAsync(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard(
                        "Not Supported",
                        "Error logging in as authorized user\n" +
                        "-- Only supported on Chrome\n-- Requires gmail account.",
                        ButtonEnum.Ok);

                    return await box.ShowAsync();
                });
            }
            await viewModel.DoLoginAsync();
        }
    }


    private void ClosePopup_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel.ClosePopup();
    }
}
 