using Avalonia.Threading;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SWSMonitor.ViewModels;

public class MainViewModel : MainWindowModel
{
    public void SetBusy(bool isBusy)
    {
        IsPopupOpen = isBusy;
    }

    public override async Task ShowBusyPopup(string? message) 
    {
        Dispatcher.UIThread.Post(() =>
        {
            SetBusy(true);
            MainView.ViewInstance.LoadingText.Text = message;
            MainView.ViewInstance.PopupOverlay.IsVisible = true;
            MainView.ViewInstance.PopupOverlay.InvalidateVisual();
        });

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            MainView.ViewInstance.PopupOverlay.InvalidateVisual();
            // Heavy work or UI updates happen here
            await Task.Delay(100);
        });
    }
    public override async Task ShowNoBusyPopup()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            SetBusy(false);
            MainView.ViewInstance.PopupOverlay.IsVisible = false;
            MainView.ViewInstance.PopupOverlay.InvalidateVisual();
        });
    }

    public static MainViewModel? Current;

    private bool _isPaneOpen = false;
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


    private ObservableCollection<BrowserItemViewModel> _topMenuItems;
    public ObservableCollection<BrowserItemViewModel> TopMenuItems
    {
        get => _topMenuItems;
        set => this.RaiseAndSetIfChanged(ref _topMenuItems, value);
    }

    private ObservableCollection<BrowserItemViewModel> _bottomMenuItems;
    public ObservableCollection<BrowserItemViewModel> BottomMenuItems
    {
        get => _bottomMenuItems;
        set => this.RaiseAndSetIfChanged(ref _bottomMenuItems, value);
    }

    private BrowserItemViewModel? _selectedMenuItem;
    public BrowserItemViewModel? SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedMenuItem, value);
        }
    }

    private BrowserItemViewModel? _selectedBottomMenuItem;
    public BrowserItemViewModel? SelectedBottomMenuItem
    {
        get => _selectedBottomMenuItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedBottomMenuItem, value);
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

    private string _popupMessage = "Patience...";
    public string PopupMessage
    {
        get => _popupMessage;
        set => this.RaiseAndSetIfChanged(ref _popupMessage, value);
    }

    private string _title = "Beach Survey Version X.Y";
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public MainViewModel ()
    {
        Current = this;
        StaticData.MainWindowModel = this;

        this.PropertyChanged += MainViewModel_PropertyChanged;

        _currentPage = new SplashScreenViewModel();

        Title = $"Beach Survey {typeof(App).Assembly.GetName().Version.ToString().Substring(0, 3)}";

    }

    public async Task InitializeAsync()
    {
        await LoadGlobals();
    }

    private async void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch(e.PropertyName)
        {
            case "SelectedMenuItem":
                await OnSelectedMenuItemChanged(SelectedMenuItem);
                break;
            case "SelectedBottomMenuItem":
            default:
                break;
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
            ShowStatus($"Data glossary loaded.", true);
            StaticData.FinishLoadingGlobals();
            LoadMenuItems();
            if (SplashScreenViewModel.Instance is not null)
                SplashScreenViewModel.Instance.SetTitleVisible(true);
        });

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
        TopMenuItems = new ObservableCollection<BrowserItemViewModel>
        {
            //// Allowed for all users
            //new BrowserItemViewModel("Login", @"avares://SWSMonitor/Assets/user-login.svg", new LoginViewModel(), AppRoleEnum.Public),
            //// Only allowed for logged-in users with at least 'view' role
            new BrowserItemViewModel("Home", @"avares://SWSMonitor/Assets/home.svg", new SplashScreenViewModel(), AppRoleEnum.Public),
            // Allowed for all users
            new BrowserItemViewModel("Maps", @"avares://SWSMonitor/Assets/map.svg",new MapWebViewModel(), AppRoleEnum.Public),
            new BrowserItemViewModel("Data", @"avares://SWSMonitor/Assets/View-Details.svg",new DynamicGridViewModel(), AppRoleEnum.Public),  
            new BrowserItemViewModel("Species", @"avares://SWSMonitor/Assets/Fish.svg",new GlossariesViewModel(), AppRoleEnum.Public),
            new BrowserItemViewModel("Beaches", @"avares://SWSMonitor/Assets/beach.svg",new BeachesViewModel(), AppRoleEnum.Public),
            new BrowserItemViewModel("Surveys", @"avares://SWSMonitor/Assets/Surveys.svg", new HomeViewModel(), AppRoleEnum.Public),
            new BrowserItemViewModel("People", @"avares://SWSMonitor/Assets/people.svg", new PeopleViewModel(), AppRoleEnum.Edit),
        };

        BottomMenuItems = new ObservableCollection<BrowserItemViewModel>
        {
            // Only allowed for logged-in users with 'edit' 'admin' role
//            new BrowserItemViewModel("Settings", @"avares://SWSMonitor/Assets/settings.svg",new SettingsViewModel(), AppRoleEnum.Admin)
        };

    }

    public void StartFirstPage()
    {
        SelectedMenuItem = TopMenuItems[0]; // Set initial page
    }
    public async Task OnSelectedMenuItemChanged(BrowserItemViewModel? value)
    {
        if (value != null)
        {
            if (CurrentPage is HomeViewModel homePage)
            {
                homePage.RefreshData();
            }
            if (/*CurrentPage != value.ContentViewModel &&*/
                value.ContentViewModel is not SplashScreenViewModel &&
                value.ContentViewModel is not MapWebViewModel &&
                value.ContentViewModel is not MapsViewModel)
                await ShowBusyPopup($"Loading Data...");
            else
                await ShowNoBusyPopup();
            CurrentPage = value.ContentViewModel;
        }
    }
    public async Task OnSelectedBottomMenuItemChanged(BrowserItemViewModel? value)
    {
        if (value != null)
        {
            CurrentPage = value.ContentViewModel;
        }
    }

    public void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    public void ShowStatus(string message, bool useTimer = false)
    {
        if (_currentPage is SplashScreenViewModel splashScreen)
        {
            splashScreen.SplashMessage = message;
            splashScreen.RaisePropertyChanged("SplashMessage");
        }

        if (!useTimer) return;
    }

    private DispatcherTimer _timer;

    public void ClosePopup()
    {
        IsPopupOpen = false;
    }

    internal bool DoesCurrentUserHaveRole(AppRoleEnum roleRequired)
    {
        // Check if current user has the required role
        // If no user is logged in, they are 'Public' role
        AppRoleEnum currentUserRole = StaticData.UserIsSignedIn ? StaticData.UserRole : AppRoleEnum.Public;
        return (int)currentUserRole >= (int)roleRequired;
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

public class DesignMainViewModel
{
    public DesignMainViewModel() : base()
    {
        // You can set design-time specific properties here if needed
    }
}

public partial class BrowserItemViewModel : ViewModelBase, INotifyPropertyChanged
{
    // Added setters so XAML compile-time bindings (x:DataType) don't report missing setters
    public string Header { get; set; }
    //    public Bitmap ImageIcon { get; set; }

    public string SvgImagePath { get; set; }
    public ViewModelBase ContentViewModel { get; set; }

    public bool IsAllowedForCurrentUser { get => MainViewModel.Current!.DoesCurrentUserHaveRole(RoleRequired); }

    public AppRoleEnum RoleRequired { get; set; }

    public void UpdateUsersRole(AppRoleEnum newRole)
    {
        // Called after user logs in or out to refresh menu item access
        // Call this to trigger menu changes
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAllowedForCurrentUser)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public BrowserItemViewModel(string header, string svgImagePath, ViewModelBase contentViewModel, AppRoleEnum roleRequired)
    {
        Header = header;
        SvgImagePath = svgImagePath;
        ContentViewModel = contentViewModel;
        RoleRequired = roleRequired;
    }

}



