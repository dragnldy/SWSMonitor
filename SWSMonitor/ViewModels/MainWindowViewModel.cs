using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SWSMonitor.ViewModels;

public partial class MainWindowViewModel : MainWindowModel
{
    public static MainWindowViewModel? Current;

    public override async Task ShowBusyPopup(string? message)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (!string.IsNullOrEmpty(message))
                MainWindowViewModel.Current.LoadingMessage = message;
            MainWindowViewModel.Current.IsPopupOpen = true;
        });
    }
    public override async Task ShowNoBusyPopup()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            MainWindowViewModel.Current.IsPopupOpen = false;
        });
    }


    private bool _isPaneOpen;
    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set => this.RaiseAndSetIfChanged(ref _isPaneOpen, value);
    }

    private string _loadingMessage = "";
    public string LoadingMessage
    {
        get => _loadingMessage;
        set => this.RaiseAndSetIfChanged(ref _loadingMessage, value);
    }


    private ObservableCollection<MenuItemViewModel> _topMenuItems;
    public ObservableCollection<MenuItemViewModel> TopMenuItems
    {
        get => _topMenuItems;
        set => this.RaiseAndSetIfChanged(ref _topMenuItems, value);
    }

    private ObservableCollection<MenuItemViewModel> _bottomMenuItems;
    public ObservableCollection<MenuItemViewModel> BottomMenuItems
    {
        get => _bottomMenuItems;
        set => this.RaiseAndSetIfChanged(ref _bottomMenuItems, value);
    }

    private MenuItemViewModel? _selectedMenuItem;
    public MenuItemViewModel? SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedMenuItem, value);
            OnSelectedMenuItemChanged(value);
        }
    }

    private MenuItemViewModel? _selectedBottomMenuItem;
    public MenuItemViewModel? SelectedBottomMenuItem
    {
        get => _selectedBottomMenuItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedBottomMenuItem, value);
            OnSelectedBottomMenuItemChanged(value);
        }
    }

    private ViewModelBase _currentPage; // Base class for your page ViewModels
    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    private string _header = "Beach Survey";
    public string Header
    {
        get => _header;
        set => this.RaiseAndSetIfChanged(ref _header, value);
    }


    private bool _isPopupOpen = false;
    public bool IsPopupOpen
    {
        get => _isPopupOpen;
        set => this.RaiseAndSetIfChanged(ref _isPopupOpen, value);
    }

    private string _title = "Beach Survey Version X.Y";
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public MainWindowViewModel()
    {
        Current = this;
        StaticData.MainWindowModel = this;

        _currentPage = new SplashScreenViewModel();
        Title = $"Beach Survey {typeof(App).Assembly.GetName().Version.ToString().Substring(0,3)}";

        if (!Avalonia.Controls.Design.IsDesignMode)
        {
            /*
            GoogleDriveApiClient googleClient = new GoogleDriveApiClient(apiKey: StaticData.JsonConfig.HGAK, secret: StaticData.JsonConfig.HGSA);
            StaticData.JsonConfig.GoogleClient = googleClient;
            StaticData.JsonConfig.DriveCredential = googleClient.GetCredentials();
            StaticData.JsonConfig.DriveService = googleClient.GetDriveService(); */
            _ = LoadGlobals();
        }
    }

    public async Task LoadGlobals()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ShowStatus("Loading Glossaries...", false);
        });

        await Task.Run(async () => await StaticData.PreLoadGlobalsAsync());

        if (!StaticData.AllGlobalsLoaded)
            await Task.Run(async () => await CheckGlobalsLoaded());

        if (!StaticData.AllGlobalsLoaded)
            ShowStatus("Error Loading Glossaries...", false);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ShowStatus("Data glossary loaded.", true);
            StaticData.FinishLoadingGlobals();
            LoadMenuItems();
        });

        await Task.Delay(3000);
        StartFirstPage();
    }

    private async Task CheckGlobalsLoaded()
    {
        // This may not be required any longer if we await the PreLoadGlobalsAsync task, but just in case, we'll check a few times with a delay
        int attempts = 10;
        while (!StaticData.AllGlobalsLoaded && attempts > 0)
        {
            await Task.Delay(1000);
            attempts--;
        }
    }


    public void LoadMenuItems()
    {
        TopMenuItems = new ObservableCollection<MenuItemViewModel>
        {
            // Allowed for all users
            new MenuItemViewModel("Login", @"avares://BeachSurvey/Assets/user-login.svg", new LoginViewModel(), AppRoleEnum.Public),
            // Only allowed for logged-in users with at least 'view' role
            new MenuItemViewModel("Home", @"avares://BeachSurvey/Assets/home.svg", new HomeViewModel(), AppRoleEnum.View),
            // Allowed for all users
            new MenuItemViewModel("Maps", @"avares://BeachSurvey/Assets/map.svg",new MapsViewModel(), AppRoleEnum.Public),
            // Allowed for all users
            new MenuItemViewModel("Data", @"avares://BeachSurvey/Assets/View-Details.svg",new DynamicGridViewModel(), AppRoleEnum.Public),
            // Only allowed for logged-in users with at least 'edit' role
            new MenuItemViewModel("People", @"avares://BeachSurvey/Assets/people.svg",new PeopleViewModel(), AppRoleEnum.Edit),
            // Allowed for all users for view- no edit though
            new MenuItemViewModel("Species", @"avares://BeachSurvey/Assets/Fish.svg",new GlossariesViewModel(), AppRoleEnum.Public),
            // Allowed for all users for view- no edit though
            new MenuItemViewModel("Beaches", @"avares://BeachSurvey/Assets/beach.svg",new BeachesViewModel(), AppRoleEnum.Public),
        };
        BottomMenuItems = new ObservableCollection<MenuItemViewModel>
        {
            // Only allowed for logged-in users with 'admin' role
            new MenuItemViewModel("Settings", @"avares://BeachSurvey/Assets/settings.svg",new SettingsViewModel(), AppRoleEnum.Admin)
        };

    }

    public void StartFirstPage()
    {
        SelectedMenuItem = TopMenuItems[0]; // Set initial page
    }
    public void OnSelectedMenuItemChanged(MenuItemViewModel? value)
    {
        if (value != null)
        {
            if (CurrentPage is HomeViewModel homePage)
            {
                homePage.SaveChanges(false);
                homePage.ReturnToWizardPage();
            }
            CurrentPage = value.ContentViewModel;
        }
    }
    public void OnSelectedBottomMenuItemChanged(MenuItemViewModel? value)
    {
        if (value != null)
        {
            CurrentPage = value.ContentViewModel;
        }
    }

    [RelayCommand]
    private void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    public void ShowStatus(string message, bool useTimer = false)
    {
        if (_currentPage is SplashScreenViewModel splashScreen)
        {
            splashScreen.SplashMessage = message;
            splashScreen.RaisePropertyChanged("SpashMessage");
        }

        if (!useTimer) return;

        // ReturnToWizardPage and start the timer
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5) // Set the duration (e.g., 3 seconds)
        };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private DispatcherTimer _timer;

    private void OnTimerTick(object sender, EventArgs e)
    {
        // Stop the timer
        _timer.Stop();
        _timer.Tick -= OnTimerTick; // Unsubscribe to prevent memory leaks

        ClosePopup();


    }

    public void ClosePopup()
    {
        IsPopupOpen = false;
    }

    internal bool DoesCurrentUserHaveRole(AppRoleEnum roleRequired)
    {
      // Check if current user has the required role
        // If no user is logged in, they are 'Public' role
        AppRoleEnum currentUserRole = StaticData.UserIsSignedIn ? StaticData.UserRole : AppRoleEnum.Public;
        return currentUserRole >= roleRequired;
    }

    public void UpdateMenuItemsForUserRole(AppRoleEnum newRole)
    {
        // Notify each menu item to update its access based on the new role
        if (TopMenuItems == null || BottomMenuItems == null) return;
        foreach (var menuItem in TopMenuItems)
        {
            menuItem.UpdateUsersRole(newRole);
        }
        foreach (var menuItem in BottomMenuItems)
        {
            menuItem.UpdateUsersRole(newRole);
        }
    }
}

public class DesignMainWindowViewModel
{
    public DesignMainWindowViewModel() : base()
    {
        // You can set design-time specific properties here if needed
    }
}
public partial class MenuItemViewModel : ViewModelBase, INotifyPropertyChanged
{
    // Added setters so XAML compile-time bindings (x:DataType) don't report missing setters
    public string Header { get; set; }
//    public Bitmap ImageIcon { get; set; }

    public string SvgImagePath { get; set; }
    public ViewModelBase ContentViewModel { get; set; }

    public bool IsAllowedForCurrentUser { get => MainWindowViewModel.Current!.DoesCurrentUserHaveRole(RoleRequired); }

    public AppRoleEnum RoleRequired { get; set; }

    public void UpdateUsersRole(AppRoleEnum newRole)
    {
        // Called after user logs in or out to refresh menu item access
        // Call this to trigger menu changes
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAllowedForCurrentUser)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public MenuItemViewModel(string header, string svgImagePath, ViewModelBase contentViewModel, AppRoleEnum roleRequired)
    {
        Header = header;
        SvgImagePath = svgImagePath;
        ContentViewModel = contentViewModel;
        RoleRequired = roleRequired;
    }

}
