using Avalonia.Interactivity;
using Avalonia.Threading;
using SWSMonitor.Models;
using DataLibrary.Crud;
using DataLibrary.ModelExtensions;
using Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SWSMonitor.ViewModels;

public partial class HomeViewModel :ViewModelBase, IScreen, INotifyPropertyChanged
{
    #region CTOR
    public HomeViewModel()
    {
        PropertyChanged += HomeViewModel_PropertyChanged;
    }

    #endregion CTOR

    public void ReturnToWizardPage()
    {
        WizardPagesEnum currentPage = GetCurrentPageId();

        Dispatcher.UIThread.Invoke(() =>
        {
            Router.Navigate.Execute(GetViewModelById(currentPage));
        });
    }

    private const WizardPagesEnum FirstPageToShow = WizardPagesEnum.SurveyViewModel;
    public RoutingState Router { get; } = new RoutingState();

    private string _goBackButtonText = "Go Back";
    public string GoBackButtonText
    {
        get { return _goBackButtonText; }
        set { this.RaiseAndSetIfChanged(ref _goBackButtonText, value); }
    }

    private string _goNextButtonText = "Go Next";
    public string GoNextButtonText
    {
        get { return _goNextButtonText; }
        set { this.RaiseAndSetIfChanged(ref _goNextButtonText, value); }
    }

    private IRoutableViewModel GetViewModelById(WizardPagesEnum pageId)
    {
        var pageName = Enum.GetName(typeof(WizardPagesEnum), pageId);

        var viewModelType = Type.GetType($"SWSMonitor.ViewModels.{pageName}");
        if (viewModelType is null)
        {
            // Fallback to SurveyViewModel if type not found
            if (pageId != FirstPageToShow)
                return GetViewModelById(FirstPageToShow); // Rewind to first page
            return null;
        }
        IRoutableViewModel viewModel =  Activator.CreateInstance(viewModelType, this) as IRoutableViewModel;
        return viewModel;
    }
    private bool _canGoBack = false;
    public bool CanGoBack
    {
        get { return _canGoBack; }
        set { this.RaiseAndSetIfChanged(ref _canGoBack, value); }
    }
    private bool _canGoNext = true;
    public bool CanGoNext
    {
        get { return _canGoNext; }
        set { this.RaiseAndSetIfChanged(ref _canGoNext, value); }
    }

    private bool _canChangeSurvey = false;
    public bool CanChangeSurvey
    {
        get { return !_canGoBack && IsSurveyLoaded; }
    }

    private string _pageTitle = "Beach Survey Wizard";
    public string PageTitle
    {
        get { return _pageTitle; }
        set { this.RaiseAndSetIfChanged(ref _pageTitle, value); }
    }

    private string _selectedSurveyInfo = "No Survey Selected";
    public string SelectedSurveyInfo
    {
        get { return _selectedSurveyInfo; }
        set { this.RaiseAndSetIfChanged(ref _selectedSurveyInfo, value); }
    }


    private Survey? _loadedSurvey = null;
    public Survey? LoadedSurvey
    {
        get { return _loadedSurvey; }
        set { this.RaiseAndSetIfChanged(ref _loadedSurvey, value); this.RaisePropertyChanged(nameof(CanChangeSurvey)); }
    }

    private bool _isSurveyLoaded = false;
    public bool IsSurveyLoaded
    {
        get { return _isSurveyLoaded && _loadedSurvey is not null; }
        set { this.RaiseAndSetIfChanged(ref _isSurveyLoaded, value); }
    }

    public bool IsDirty
    {
        get { return _loadedSurvey is not null && LoadedSurvey.SaveRequired.Any(); }
    }


    private bool _canEditSurvey = false;
    public bool CanEditSurvey
    {
        get => _canEditSurvey;
        set { this.RaiseAndSetIfChanged(ref _canEditSurvey, value); }
    }

    public async Task<bool> NavigatePageById(WizardPagesEnum pageid)
    {
        await Task.Run(async () => await NavigatePagByIdAsync(pageid).ContinueWith(
            result => {
                // Update the UI on the UI thread
                Dispatcher.UIThread.Invoke(() =>
                {
                    Router.NavigateAndReset.Execute(result.Result);
                });
            }
        ));
        return true;
    }

    private async Task<IRoutableViewModel> NavigatePagByIdAsync(WizardPagesEnum pageId)
    {
        try
        {
            if (pageId < WizardPagesEnum.FirstPage)
            {
                if (CanEditSurvey)
                {
                    var currentPage = Router.GetCurrentViewModel();
                    if (CanEditSurvey && currentPage is WizardViewModelBase wizardPage)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            wizardPage.SaveChanges();
                        });
                    }

                    if (pageId < WizardPagesEnum.FirstPage)
                    {
                        pageId = await CheckForSaveRequired(pageId);
                    }
                }

                if (pageId < WizardPagesEnum.FirstPage) // we may have aborted the move do to need to save
                    ClearCurrentSurvey();
            }
            else if (pageId >= WizardPagesEnum.LastPage)
            {
                pageId = WizardPagesEnum.FirstPage; // Go back to first page
            }
            nextView = GetViewModelById(pageId);
        }
        catch (Exception ex)
        {
            var message = "Error parsing current page type and converting to enum";
            nextView = GetViewModelById(FirstPageToShow);
        }
        return nextView;
    }

    public async Task<bool> NavigatePage(bool goForward)
    {
        await Task.Run(async () => await NavigatePageAsync(goForward).ContinueWith(
            result => {
                // Update the UI on the UI thread
                Dispatcher.UIThread.Invoke(() =>
                {
                    Router.Navigate.Execute(result.Result);
                });
            }
        ));
        return true;
    }

    private IRoutableViewModel nextView = null;

    private async Task<IRoutableViewModel> NavigatePageAsync(bool goForward)
    {
        IRoutableViewModel nextView = null;
        try
        { 
            WizardPagesEnum pageId = GetCurrentPageId();
            // Increment or decrement pageId based on navigation direction
            pageId = goForward ? ++pageId : --pageId;
            nextView = await NavigatePagByIdAsync(pageId);
        }
        catch (Exception ex)
        {
            var message = "Error parsing current page type and converting to enum";
            nextView = GetViewModelById(FirstPageToShow);
        }
        return nextView;
    }

    private async Task<WizardPagesEnum> CheckForSaveRequired(WizardPagesEnum pageId)
    {
        if (!CanEditSurvey || LoadedSurvey is null || !LoadedSurvey.SaveRequired.Any()) return pageId;

        Console.Beep();
        // Run messagebox/show logic on UI thread and await it.
        var result = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Caution",
                "Unsaved changes will be lost. Do you want to continue?",
                ButtonEnum.YesNo);
            return await box.ShowAsync();
        });
        if (result == ButtonResult.No)
        {
            // Stay on the current page
            return GetCurrentPageId();
        }
        return pageId;
    }

    private void ClearCurrentSurvey()
    {
        LoadedSurvey?.SaveRequired.Clear();
        this.RaisePropertyChanged(nameof(IsDirty));
        LoadedSurvey = null;
        IsSurveyLoaded = false;
    }

    private WizardPagesEnum GetCurrentPageId()
    {
        var currentPage = Router.GetCurrentViewModel();
        if (currentPage is not null)
        {
            var pageName = currentPage.GetType().Name;
            return (WizardPagesEnum)Enum.Parse(typeof(WizardPagesEnum), pageName);
        }
        return FirstPageToShow;
    }
    private void HomeViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CanGoBack))
        {
            var temp = CanGoBack;
            //CanGoBack = Router.NavigationStack.Count > 0;
        }
    }

    public void SetSelectedSurveyInfo(BeachData? selectedBeach, DateTime? surveyDate, long surveyID = 0)
    {
        SelectedSurveyInfo = selectedBeach != null  && surveyDate.HasValue?
            $"{selectedBeach.BeachName}: {surveyDate.Value.ToString("yyyy-MM-dd")}" :
            "No Survey Selected";
        if (surveyID > 0 && selectedBeach is not null)
            SelectedSurveyInfo += $" ID: [{surveyID}]";
    }

    internal void LoadSelectedSurveyInfo(BeachData selectedBeach, DateTime surveyDate)
    {
        SurveyCrud.ReadSurveyData(StaticData.DataSourceConfig, selectedBeach.BeachName, surveyDate).ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                //                    System.Diagnostics.Debug.WriteLine($"Error loading survey: {t.Exception?.Message}");
                IsSurveyLoaded = false;
                LoadedSurvey = null;
            }
            else
            {
                LoadedSurvey = t.Result;
                IsSurveyLoaded = LoadedSurvey is not null;

                SetSelectedSurveyInfo(selectedBeach, surveyDate, LoadedSurvey?.ID ?? 0);
                StaticData.FinishLoadingSurvey(IsSurveyLoaded);
            }
        });
    }
    internal void InitializeNewSurveyInfo(BeachData selectedBeach, DateTime surveyDate)
    {
        SetSelectedSurveyInfo(selectedBeach, surveyDate, -1);
        LoadedSurvey = new Survey
        {
            ID = -1l,
            BeachName = selectedBeach.BeachName,
            SurveyDate = surveyDate,
            Tide1Ht = GlobalConstants.DefaultTide1Ht,
            Tide2Ht = GlobalConstants.DefaultTide2Ht,
            Tide3Ht = GlobalConstants.DefaultTide3Ht
        };
        // Use a hashset to avoid duplicates
        LoadedSurvey.SaveRequired.Add(ComponentsToSaveEnum.Base);
        LoadedSurvey.SaveRequired.Add(ComponentsToSaveEnum.BeachEvent);
        IsSurveyLoaded = LoadedSurvey is not null;
    }

    internal void SaveChanges(bool forcesave = false)
    {
        if (IsSurveyLoaded)
        {
            WizardViewModelBase currentPage = (Router.GetCurrentViewModel() as WizardViewModelBase);
            if (currentPage is not null)
            {
                if (CanEditSurvey && LoadedSurvey is not null)
                    currentPage.SaveChanges();
            }
            if (!forcesave) return;
            _ = SurveyCrud.SaveSurvey(LoadedSurvey);
        }
        this.RaisePropertyChanged(nameof(IsDirty));
    }

    internal void RefreshData()
    {
        // Todo: Implement any logic needed to refresh data when returning to the home page, if necessary.
    }
}