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

        private string _splashMessage2 = "Use this site to view and download data from our Intertidal Monitoring Projects";
        public string SplashMessage2
        {
            get => _splashMessage2;
            set { this.RaiseAndSetIfChanged(ref _splashMessage2, value); }
        }

        private string _splashMessage3 = "Please use the side-bar menu for navigation.";
        public string SplashMessage3
        {
            get => _splashMessage3;
            set { this.RaiseAndSetIfChanged(ref _splashMessage3, value); }
        }

        public string SWSLink => "Visit Sound Water Stewards to find out more about our projects";
        public string SWSUrl => "https://soundwaterstewards.org/projects/intertidal-monitoring/";

        private string _userEmail = "Loading...";
        public string UserEmail
        {
            get => _userEmail;
            set => this.RaiseAndSetIfChanged(ref _userEmail, value);
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
