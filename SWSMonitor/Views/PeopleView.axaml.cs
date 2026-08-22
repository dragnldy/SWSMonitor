using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using DataLibrary.Crud;
using DynamicData;
using Models;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SWSMonitor.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SWSMonitor;

public partial class PeopleView : ReactiveUserControl<PeopleViewModel>
{
    public MainWindowModel? _mainWindow = null;

    public PeopleView()
    {
        MainWindowModel main = StaticData.MainWindowModel as MainWindowModel;
        _mainWindow = main;

        this.WhenActivated((ReactiveUI.Primitives.Disposables.MultipleDisposable disposables) => { });
        AvaloniaXamlLoader.Load(this);
    }
    protected override async void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        await _mainWindow?.ShowBusyPopup("Loading People View...");
        base.OnAttachedToVisualTree(e);
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        await (ViewModel as PeopleViewModel)?.InitializeVolunteers();
        await _mainWindow?.ShowNoBusyPopup();
        // Control is fully ready, layout has occurred, and templates are applied.
    }

    private void ClosePopup_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.ViewModel!.IsPopupOpen = false;
        this.ViewModel!.PopupIsOpen = false;
    }

    private void UpdateVolunteer_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        UpdateInBackground();
    }
    private async Task UpdateInBackground()
    {
        VolunteerViewModel? vview = VolunteerViewModel.Instance;
        if (vview != null)
        {
            (bool success, Volunteer saved) = await vview.SaveVolunteer();
            if (!success || saved == null) //  didn't save successfully
            {
                Debug.WriteLine("Update of volunteer failed");
            }
            else
            {
                Volunteer current = StaticData.Volunteers.FirstOrDefault(n => n.FirstLast == saved.FirstLast);
                if (current is null)
                {
                    StaticData.Volunteers.Add(saved);
                    this.ViewModel!.Volunteers.Add(saved);
                }
                else
                {
                    StaticData.Volunteers.Replace(current, saved);
                    this.ViewModel!.Volunteers.Replace(current, saved);
                }
            }

        }
        this.ViewModel!.IsPopupOpen = false;
        return;
    }

    private void DataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (this.ViewModel!.SelectedVolunteer is null)
        {
            TraceLogger.LogWarningAuto("Beep"); return;
        }
        if (this.ViewModel!.UserIsAdmin)
            EditVolunteer();
        else if (StaticData.UserRole >= AppRoleEnum.View)
            ViewVolunteer();
    }

    private bool EditVolunteer()
    {
        if (this.ViewModel!.SelectedVolunteer is null)
        {
            TraceLogger.LogWarningAuto("Beep"); return false;
        }
        string target = this.ViewModel!.SelectedVolunteer?.FirstLast ?? string.Empty;
        VolunteerViewModel? vview = VolunteerViewModel.Instance;
        if (vview != null)
            vview.LoadTargetVolunteer(target);
        this.ViewModel!.IsPopupOpen = true;
        return true;
    }

    private bool ViewVolunteer()
    {
        if (this.ViewModel!.SelectedVolunteer is null)
        {
            TraceLogger.LogWarningAuto("Beep"); return false;
        }
        string target = this.ViewModel!.SelectedVolunteer?.FirstLast ?? string.Empty;
        VolunteerViewModel? vview = VolunteerViewModel.Instance;
        if (vview != null)
            vview.LoadTargetVolunteer(target);
        this.ViewModel!.IsPopupOpen = true;
        return true;
    }

    private void AddVolunteer_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Volunteer newbie = new() { ID = -1, FirstLast = "" };
        VolunteerViewModel? vview = VolunteerViewModel.Instance;
        if (vview != null)
            vview.LoadTargetVolunteer(newbie,isExisting: false);
        this.ViewModel!.IsPopupOpen = true;
    }

    private void EditButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (e.Source is Button button)
        {
            if (button.Tag is not null && button.Tag is Volunteer volunteer)
            {
                if (volunteer is not null)
                {
                    this.ViewModel!.SelectedVolunteer = volunteer;
                    EditVolunteer();
                }
            }
        }
    }
    private void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (e.Source is Button button)
        {
            if (button.Tag is not null && button.Tag is Volunteer volunteer)
            {
                if (volunteer is not null)
                {
                    this.ViewModel!.SelectedVolunteer = volunteer;
                    this.ViewModel!.PopupMessage = "Volunteer will be permanently deleted. Do you want to continue?";
                    this.ViewModel!.PopupIsOpen = true;
                }
            }
        }
    }

    private void ViewButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (e.Source is Button button)
        {
            if (button.Tag is not null && button.Tag is Volunteer volunteer)
            {
                if (volunteer is not null)
                {
                    this.ViewModel!.SelectedVolunteer = volunteer;
                    ViewVolunteer();
                }
            }
        }
    }
    private void ConfirmDeleteButton_Click(object? sender, RoutedEventArgs e)
    {
        // DeleteInBackground(beach);
        var volunteer = this.ViewModel!.SelectedVolunteer;
        this.ViewModel!.PopupIsOpen = false;

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            this.ViewModel!.SelectedVolunteer = null;
            // await VolunteersCrud.DeleteVolunteerAsync(StaticData.DataSourceConfig!, volunteer.ID);
            this.ViewModel!.Volunteers.Remove(volunteer);
            StaticData.Volunteers.Remove(volunteer);
            this.ViewModel!.PopupIsOpen = false;
        });
    }
}
