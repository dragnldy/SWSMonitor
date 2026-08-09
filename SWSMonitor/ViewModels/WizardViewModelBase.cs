using Avalonia.Threading;
using ReactiveUI;
using SWSMonitor.Models;
using System.Linq;

namespace SWSMonitor.ViewModels;

/// <summary>
/// An abstract class for enabling page navigation.
/// </summary>
public abstract class WizardViewModelBase : ViewModelBase
{
    // Reference to IScreen that owns the routable view model.
    public ViewModelBase? HostScreen { get; set; } = null;

    public virtual void OnNavigatingTo()
    {
    }
    public virtual void OnNavigatingFrom()
    {
    }

    internal void SetUpCommands(bool? canGoBack, bool? canGoNext)
    {
        HomeViewModel current = HostScreen as HomeViewModel;
        if (current is not null)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                current.CanGoNext = canGoNext ?? true;                // Update the UI on the UI thread
                current.CanGoBack = canGoBack ?? true;
            });
        }
    }

    public abstract void SaveChanges();

    private string _pageTitle = string.Empty;
    public string PageTitle
    {
        get => _pageTitle;
        protected set
        {
            if (HostScreen is not null)
            {
                HomeViewModel current = HostScreen as HomeViewModel;
                current.PageTitle = value;
            }
            this.RaiseAndSetIfChanged(ref _pageTitle, value);
        }
    }

    internal bool _isLoading = false;


    private bool _isDirty = false;
    public bool IsDirty
    {
        get => _isDirty;
        set { 
            this.RaiseAndSetIfChanged(ref _isDirty, value);
            if (HostScreen is not null && HostScreen is HomeViewModel current)
            {
                if (current.LoadedSurvey is not null && current.LoadedSurvey.SaveRequired.Any())
                    current.RaisePropertyChanged("IsDirty");
            }
        }
    }
}
