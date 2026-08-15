using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using SWSMonitor.Models;
using SWSMonitor.ViewModels;
using System.Threading.Tasks;

namespace SWSMonitor;

public partial class HomeView : ReactiveUserControl<HomeViewModel>
{
    public MainWindowModel? _mainWindow = null;
    public HomeView()
    {
        MainWindowModel main = StaticData.MainWindowModel as MainWindowModel;
        _mainWindow = main;
        AvaloniaXamlLoader.Load(this);
    }

    public async Task SetBusy(bool isBusy)
    {
        //this.Cursor = isBusy
        //    ? new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Wait)
        //    : Avalonia.Input.Cursor.Default;
        if (isBusy)
            await _mainWindow.ShowBusyPopup("Loading Surveys View...");
        else
            await _mainWindow.ShowNoBusyPopup();
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        await SetBusy(false);
        // Control is fully ready, layout has occurred, and templates are applied.
        // Go to either the first wizard page or the last loaded

        HomeViewModel vm = this.DataContext as HomeViewModel;
        vm.ReturnToWizardPage();

    }



    private void GoBackButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        HomeViewModel vm = this.DataContext as HomeViewModel;
        vm.NavigatePage(goForward: false);
    }
    private void GoNextButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        HomeViewModel vm = this.DataContext as HomeViewModel;
        vm.NavigatePage(goForward: true);
    }

    private void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        HomeViewModel vm = this.DataContext as HomeViewModel;
        vm.SaveChanges(true);
    }

    private void NavigateToTeams(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        HomeViewModel vm = this.DataContext as HomeViewModel;
        vm.NavigatePageById(pageId: WizardPagesEnum.TeamViewModel);
    }

    private void NavigateToConditions(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        HomeViewModel vm = this.DataContext as HomeViewModel;
        vm.NavigatePageById(pageId: WizardPagesEnum.ConditionViewModel);
    }

    private void NavigateToBeachSetting(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        HomeViewModel vm = this.DataContext as HomeViewModel;
        vm.NavigatePageById(pageId: WizardPagesEnum.BeachSettingViewModel);
    }

    private void NavigateToProfiles(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        HomeViewModel vm = this.DataContext as HomeViewModel;
        vm.NavigatePageById(pageId: WizardPagesEnum.ProfileViewModel);
    }

    private void NavigateToQuadrats(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        HomeViewModel vm = this.DataContext as HomeViewModel;
        vm.NavigatePageById(pageId: WizardPagesEnum.QuadratViewModel);
    }
    private void NavigateToSpeciesList(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        HomeViewModel vm = this.DataContext as HomeViewModel;
        vm.NavigatePageById(pageId: WizardPagesEnum.SpeciesListViewModel);
    }

    private void NavigateToSelectDifferentStudy(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        HomeViewModel vm = this.DataContext as HomeViewModel;
        vm.NavigatePageById(pageId: WizardPagesEnum.SurveyViewModel);
    }

    private void Action_Click(object? sender, RoutedEventArgs e)
    {
        if (this.ViewModel!.LoadedSurvey is not null)
        {
            this.ViewModel!.SaveSurvey();
            this.ViewModel!.CloseActionPopup();
        }
        return;
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        this.ViewModel!.RevertChanges();
        this.ViewModel!.CloseActionPopup();
    }
}
