using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DataLibrary.ApiServices;
using DataLibrary.DataSources.CloudAuth;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SWSMonitor.ViewModels
{
    public partial class SplashScreenViewModel : ViewModelBase, INotifyPropertyChanged
    {

        private bool _isPopupOpen;
        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set => this.RaiseAndSetIfChanged(ref _isPopupOpen, value);
        }

        public static SplashScreenViewModel? Instance;
        public SplashScreenViewModel()
        {
            Instance = this;
        }

        private string _pageTitle = "Setting Things Up";
        public string PageTitle
        {
            get => _pageTitle;
            set { this.RaiseAndSetIfChanged(ref _pageTitle, value); }
        }

        public bool _userCanSignIn = false;
        public bool UserCanSignIn
        {
            get => !StaticData.UserIsSignedIn && StaticData.UserCanLogin && _userCanSignIn;
            set { this.RaiseAndSetIfChanged(ref _userCanSignIn, value); }
        }


        public bool _titleVisible = false;
        public bool TitleVisible
        {
            get => _titleVisible;
            set { this.RaiseAndSetIfChanged(ref _titleVisible, value); }
        }

        private string _splashMessage = "Welcome to  SWS Intertidal Monitoring!";
        public string SplashMessage
        {
            get => _splashMessage;
            set { this.RaiseAndSetIfChanged(ref _splashMessage, value); }
        }

        private string _splashMessage2 = "View and Download\nIntertidal Monitoring Data";
        public string SplashMessage2
        {
            get => _splashMessage2;
            set { this.RaiseAndSetIfChanged(ref _splashMessage2, value); }
        }

        private string _splashMessage3 = "Use the Side-Bar Menu\n For Navigation.\n\nSite Best Viewed\nOn High Resolution Devices";
        public string SplashMessage3
        {
            get => _splashMessage3;
            set { this.RaiseAndSetIfChanged(ref _splashMessage3, value); }
        }

        public string SWSLink => "Visit Sound Water Stewards For Project Information";
        public string SWSUrl => "https://soundwaterstewards.org/projects/intertidal-monitoring/";

        private string _userEmail = "Loading...";
        public string UserEmail
        {
            get => _userEmail;
            set => this.RaiseAndSetIfChanged(ref _userEmail, value);
        }

        // Properties for device and window dimensions
        private double _viewportWidth;
        public double ViewportWidth
        {
            get => _viewportWidth;
            set => this.RaiseAndSetIfChanged(ref _viewportWidth, value);
        }

        private double _viewportHeight;
        public double ViewportHeight
        {
            get => _viewportHeight;
            set => this.RaiseAndSetIfChanged(ref _viewportHeight, value);
        }

        private double _screenWidth;
        public double ScreenWidth
        {
            get => _screenWidth;
            set => this.RaiseAndSetIfChanged(ref _screenWidth, value);
        }

        private double _screenHeight;
        public double ScreenHeight
        {
            get => _screenHeight;
            set => this.RaiseAndSetIfChanged(ref _screenHeight, value);
        }

        private double _clientHeight;
        public double ClientHeight
        {
            get => _clientHeight;
            set => this.RaiseAndSetIfChanged(ref _clientHeight, value);
        }

        private double _clientWidth;
        public double ClientWidth
        {
            get => _clientWidth;
            set => this.RaiseAndSetIfChanged(ref _clientWidth, value);
        }

        /// <summary>
        /// Called when the view is loaded to capture device and window/viewport dimensions
        /// Works for both Desktop and WASM/Browser applications
        /// </summary>
        /// <param name="control">The control to get the TopLevel from</param>
        public void OnLoad(Control control)
        {
            try
            {
                // Get the TopLevel (works for both Desktop Window and Browser ViewPort)
                var topLevel = TopLevel.GetTopLevel(control);

                if (topLevel != null)
                {
                    // Get viewport/window client dimensions
                    ViewportWidth = topLevel.ClientSize.Width;
                    ViewportHeight = topLevel.ClientSize.Height;

                    // Get screen dimensions
                    var screen = topLevel!.Screens!.Primary;
                    if (screen != null)
                    {
                        ScreenWidth = screen.Bounds.Width;
                        ScreenHeight = screen.Bounds.Height;
                    }

                    // Subscribe to size changes to keep dimensions updated
                    topLevel.SizeChanged += (sender, e) =>
                    {
                        ViewportWidth = e.NewSize.Width;
                        ViewportHeight = e.NewSize.Height;
                    };
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error getting dimensions: {ex.Message}");
            }
        }
        public async Task DoLoginAsync()
        {
            try
            {
                GoogleAuthUser user = await GoogleAuthConfig.UseFedCMToLogin();
                StaticData.UserIsSignedIn = false;
                StaticData.UserRole = AppRoleEnum.Public;
                StaticData.UserCanEdit = false;

                if (user is not null)
                {
                    if (StaticData.Volunteers.Any())
                    {
                        var volunteer = StaticData.Volunteers.Find(v => !string.IsNullOrEmpty(v.Email) && v.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase));
                        if (volunteer != null)
                        {
                            StaticData.UserRole = volunteer.Privilege;
                            StaticData.UserName = volunteer.FirstLast;
                            StaticData.UserIsSignedIn = true;
                        }
                    }
                    StaticData.UserCanEdit = (int)StaticData.UserRole >= (int)AppRoleEnum.Edit;
                    MainViewModel.Current.UpdateMenuItemsForUserRole(StaticData.UserRole);

                    var apiKeyService = new ApiKeyService();

                    if (StaticData.UserCanEdit)
                    {
                        apiKeyService.UpdateApiKeySettings();
                    }
                    else
                    {
                        apiKeyService.ClearApiKeySettings();
                    }

                    PageTitle = $"Welcome {StaticData.UserName}";
                    UserCanSignIn = !StaticData.UserIsSignedIn;
                }
                else
                {
                    SplashMessage = $"Error logging in.";
                }
            }
            catch(Exception ex)
            {
                PageTitle = $"Error logging in {ex.Message}";
            }
        }

        internal void ClosePopup()
        {
            IsPopupOpen = false;
        }

        internal void SetTitleVisible(bool v)
        {
            if (StaticData.UserCanLogin)
            {
                _pageTitle = "Browse as our Guest";
                _titleVisible = true;
                UserCanSignIn = true;
            }
            else
            {
                _pageTitle = "Browse as our Guest";
                _titleVisible = true;

            }
        }
    }
}
