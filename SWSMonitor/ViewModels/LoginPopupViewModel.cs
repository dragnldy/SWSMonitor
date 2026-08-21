using DataLibrary.DataSources.CloudAuth;
using DataLibrary.Utilities;
using Google.Apis.Util.Store;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SWSMonitor.ViewModels;

public partial class LoginPopupViewModel : ViewModelBase, INotifyPropertyChanged
{

    public const string StoreName = "SWSMonitor.Last.Account";
    public const string StoreKey = "LastUserEmail";

    private bool _isError = false;
    public bool IsError
    {
        get => _isError;
        set
        {
            this.RaiseAndSetIfChanged(ref _isError, value);
        }
    }

    public string ErrorMessage => "Please enter a valid Gmail address to sign in with Google.";

    private ICloudAuthConfig _cloudAuthConfig;

    #region CTOR and Initialization
    public LoginPopupViewModel()
    {
        var cloudAuthConfig =
            StaticData.ServiceProvider?.GetRequiredService<ICloudAuthConfig>()
           ?? throw new InvalidOperationException("ICloudAuthConfig not registered in DI container");

        _cloudAuthConfig = cloudAuthConfig;
        _ = CheckLoggedInUser();
    }

    private async Task CheckLoggedInUser()
    {
        if (StaticData.Volunteers == null)
        {
            TraceLogger.LogErrorAuto("Waited for globals to load but Volunteers is still null.");
            throw new Exception("Volunteers not loaded");
        }
        else
            _ = GetLastLoggedUser();
    }

    private async Task GetLastLoggedUser()
    {
        try
        {
            var store = new FileDataStore(StoreName, false);
            string? lastEmail = null;

            try
            {
                // Attempt to retrieve the stored email. FileDataStore throws if key not present,
                // so wrap in inner try/catch to allow silent miss.
                lastEmail = await store.GetAsync<string>(StoreKey).ConfigureAwait(false);
            }
            catch
            {
                // Key not found or other store-specific issue — treat as no saved user.
                _email = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(lastEmail))
            {
                _email = lastEmail;
                // If this looks like a Gmail address, start OAuth flow instead of using the password
                if (IsGoodGmailAccount(Email))
                {
                    // Fire-and-forget the async OAuth flow to avoid blocking the setter; UI shows progress via RequestingAccess
                    _ = SignInWithGoogleAsync();
                }

            }
        }
        catch (Exception ex)
        {
            // Surface an unobtrusive error to the user
            MainWindowViewModel.Current?.ShowStatus($"Unable to read stored account: {ex.Message}", useTimer: true);
        }
    }

    private bool IsGoodGmailAccount(string email)
    {
        if (!EmailIsGood(email)) return false;

        return (email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase) ||
                email.EndsWith("@googlemail.com", StringComparison.OrdinalIgnoreCase));
    }

    #endregion CTOR and Initialization

    #region Properties

    internal void SignalChanges()
    {
        this.RaisePropertyChanged(nameof(IsSignedIn));
        this.RaisePropertyChanged(nameof(GoodToGo));
        this.RaisePropertyChanged(nameof(SignedInMessage));
        this.RaisePropertyChanged(nameof(RoleMessage));
        this.RaisePropertyChanged(nameof(CanEdit));
    }

    public bool CanEdit => EmailIsGood(Email) /* && PasswordIsGood(Password) */ && UserRole >= AppRoleEnum.Edit;

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set
        {
            if (!EmailIsGood(value))
            {
                value = _email;
            }
            else
            {
                _email = value;
            }
            this.RaiseAndSetIfChanged(ref _email, value);
            this.RaisePropertyChanged(nameof(IsPending));
            this.RaisePropertyChanged(nameof(IsGuest));
        }
    }

    public bool EmailIsGood(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;

        bool isGood = DataValidation.CleanAndValidateEmail(value);
        if (!isGood)
        {
            SoundManager.Beep();
        }
        return isGood;
    }

    public bool IsSignedIn
    {
        get => StaticData.UserIsSignedIn;
        set
        {
            StaticData.UserIsSignedIn = value;
        }
    }

    public string SignedInMessage
    {
        get => IsSignedIn ? $"Congratulations {UserName}!" : "Not Signed In";
    }
    public string RoleMessage
    {
        get => IsSignedIn ? $"You are signed in {GetRole()}" : "Not Signed In";
    }

    private string GetRole()
    {
        switch (StaticData.UserRole)
        {
            case AppRoleEnum.Public:
                return "for general access";
            case AppRoleEnum.View:
                return "for viewing research data";
            case AppRoleEnum.Edit:
                return "for updating research data";
            case AppRoleEnum.Admin:
                return "as an administrator";
        }
        return StaticData.UserRole.ToString();
    }

    private AppRoleEnum _selectedRole = AppRoleEnum.Public;
    public AppRoleEnum SelectedRole
    {
        get => _selectedRole;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedRole, value);
        }
    }

    public string UserName => StaticData.UserName ?? "Not Signed In";

    public AppRoleEnum UserRole => StaticData.UserRole;

    private string _editReasonText = string.Empty;
    public string EditReasonText
    {
        get => StaticData.EditReason;
        set
        {
            StaticData.EditReason = value;
        }
    }

    // User has not supplied email address and is proceeding as guest
    private bool _IsGuest = true;
    public bool IsGuest
    {
        get => !EmailIsGood(Email) /* || !PasswordIsGood(Password) */;
    }
    // User has supplied email but not yet authenticated
    private bool _IsPending = false;
    public bool IsPending
    {
        get => EmailIsGood(Email) && !IsSignedIn /* && !PasswordIsGood(Password) */;
    }

    public bool GoodToGo
    {
        get => IsGoodGmailAccount(Email) && IsSignedIn;
    }

    private bool _userRequestsAccess = false;
    public bool UserRequestsAccess
    {
        get => _userRequestsAccess;
        set
        {
            this.RaiseAndSetIfChanged(ref _userRequestsAccess, value);
        }
    }

    #endregion Properties

    #region Authorization with Google Signon

    /// <summary>
    /// Initiates Google OAuth2 installed-app flow. Returns true if sign-in succeeded.
    /// Requires GOOGLE_CLIENT_ID 
    // Google OAuth Client ID 
    // Come from https://console.cloud.google.com/apis/credentials?referrer=search&hl=en&project=beachsurvey/
    /// </summary>
    public async Task<bool> SignInWithGoogleAsync()
    {
        if (!StaticData.RunningInBrowser && !IsGoodGmailAccount(Email))
        {
            IsError = true;
            return false;
        }

        var result = await UseFedCMToLogin(_cloudAuthConfig);


        SignalChanges();

        return true;
    }
    public static async Task<GoogleAuthUser?> UseFedCMToLogin( ICloudAuthConfig cloudAuthConfig)
    {
        try
        {
            // Get IJSRuntime from DI
            var jsRuntime = StaticData.ServiceProvider?.GetService<IJSRuntime>();
            if (jsRuntime == null)
            {
                TraceLogger.LogErrorAuto("IJSRuntime not available. Cannot authenticate in browser.");
                return null;
            }

            // Use Google Identity Services via JavaScript
            string token = await jsRuntime.InvokeAsync<string>(
                "googleSignIn"
            );

            if (string.IsNullOrEmpty(token?.ToString()))
            {
                MainWindowViewModel.Current?.ShowStatus("Google sign-in cancelled", useTimer: true);
                return null;
            }
            GoogleAuthUser? user = await GoogleCredentials.HandleGoogleCredentialAsync(token);
            user = await ParseUserCredentials(user);
            if (user is null)
            {
                MainWindowViewModel.Current?.ShowStatus("Google sign-in failed", useTimer: true);
                return null;
            }
            var store = new FileDataStore(StoreName, false);
            _ = store.StoreAsync<string>(StoreKey, (user != null) ? user.Email : "");

            MainWindowViewModel.Current?.ShowStatus("Google Sign-In Successful", useTimer: true);
            MainWindowViewModel.Current?.UpdateMenuItemsForUserRole(StaticData.UserRole);
            return user;
        }
        catch (Exception ex)
        {
            MainWindowViewModel.Current?.ShowStatus($"Google sign-in error: {ex.Message}", useTimer: true);
            return null;
        }
    }
    public static async Task<GoogleAuthUser?> ParseUserCredentials(GoogleAuthUser? user)
    {
        if (user is null)
        {
            return null;
        }
        var email = user.Email;
        var givenName = user.GivenName;
        var emailVerified = user.VerifiedEmail;

        if (!string.IsNullOrEmpty(email) && emailVerified)
        {
            StaticData.UserIsSignedIn = true;
            StaticData.UserName = givenName;

            if (StaticData.Volunteers.Any())
            {
                var volunteer = StaticData.Volunteers.Find(v =>
                    !string.IsNullOrEmpty(v.Email) &&
                    v.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

                if (volunteer != null)
                {
                    StaticData.UserRole = volunteer.Privilege;
                    StaticData.UserName = volunteer.FirstLast;
                }
                else
                {
                    StaticData.UserRole = AppRoleEnum.Public;
                }
            }
            else
            {
                StaticData.UserRole = AppRoleEnum.View;
            }
            return user;
        }
        return null;
    }
//    public async Task<bool> SignInWithGoogleAsync()
//    {
//        if (!IsGoodGmailAccount(Email))
//        {
//            IsError = true;
//            return false;
//        }
//        try
//        {
//            // Google client ID- required for both web and desktop
//            var GCID = (_cloudAuthConfig as GoogleAuthConfig)!.HGCSClientID;

    //            // Only desktop uses the secret- web browsers can get by with just the client Id
    //            var GCS = (_cloudAuthConfig as GoogleAuthConfig)!.HGCSUnpacked;

    //            if (string.IsNullOrWhiteSpace(GCID))
    //            {
    //                TraceLogger.LogErrorAuto("Google Client ID is missing. Please check your configuration.");
    //                return false;
    //            }

    //            var secrets = new ClientSecrets { ClientId = GCID, ClientSecret = null };

    //            string[] scopes = new[] { Oauth2Service.Scope.UserinfoEmail, Oauth2Service.Scope.UserinfoProfile ,
    //                "openid" };

    //            UserCredential userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
    //                secrets,
    //                scopes,
    //                Email,
    //                CancellationToken.None,
    //                new FileDataStore("SWSMonitor.Google.Auth.Store", true)
    //            );

    //            var oauthService = new Oauth2Service(new BaseClientService.Initializer
    //            {
    //                HttpClientInitializer = userCredential,
    //                ApplicationName = "BeachSurvey"
    //            });

    //            var userInfo = await oauthService.Userinfo.Get().ExecuteAsync();

    //            if (userInfo?.Email != null && userInfo.VerifiedEmail == true)
    //            {
    //                StaticData.UserIsSignedIn = true;
    //                StaticData.UserName = userInfo.GivenName;

    //                if (StaticData.Volunteers.Any())
    //                {
    //                    var volunteer = StaticData.Volunteers.Find(v => !string.IsNullOrEmpty(v.Email) && v.Email.Equals(userInfo.Email, StringComparison.OrdinalIgnoreCase));
    //                    if (volunteer != null)
    //                    {
    //                        StaticData.UserRole = volunteer.Privilege;
    //                        StaticData.UserName = volunteer.FirstLast;
    //                    }
    //                    else
    //                    {
    //                        StaticData.UserRole = AppRoleEnum.Public; // Default role for unknown users
    //                    }
    //                }
    //                else
    //                {
    //                    StaticData.UserRole = AppRoleEnum.View; // Default role for unknown users
    //                }

    //                var store = new FileDataStore(StoreName, false);
    //                _ = store.StoreAsync<string>(StoreKey, userInfo.Email);

    //                // Raise UI feedback on UI thread
    ////                MainWindowViewModel.Current?.ShowStatus("Google Sign-In Successful", useTimer: true);
    ////                MainWindowViewModel.Current?.UpdateMenuItemsForUserRole(StaticData.UserRole);
    //                return true;
    //            }
    //            else
    //            {
    ////                MainWindowViewModel.Current?.ShowStatus("Google sign-in failed: email not verified", useTimer: true);
    //                return false;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            //MainWindowViewModel.Current?.ShowStatus($"Google sign-in error: {ex.Message}", useTimer: true);
    //            return false;
    //        }
    //        finally
    //        {
    //            SignalChanges();
    //        }
    //    }

    internal void AttemptSignIn()
    {
        SignalChanges();
        _ = SignInWithGoogleAsync();
    }

    internal void AttemptSignOut()
    {
        var credstore = new FileDataStore("SWSMonitor.Google.Auth.Store", true);
        _ = credstore.ClearAsync();

        // Just delete the stored credentials
        var store = new FileDataStore(StoreName, false);
        StaticData.UserIsSignedIn = false;
        StaticData.UserName = string.Empty;
        StaticData.UserRole = AppRoleEnum.Public;
        Email = string.Empty;
        MainWindowViewModel.Current?.ShowStatus("Google sign-Out Successful", useTimer: true);
        MainWindowViewModel.Current?.UpdateMenuItemsForUserRole(StaticData.UserRole);

        _ = store.DeleteAsync<string>(StoreKey);
        SignalChanges();
    }
    #endregion Google Signon
}
