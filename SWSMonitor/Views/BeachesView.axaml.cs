using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using DataLibrary.Crud;
using DynamicData;
using Models;
using ReactiveUI.Avalonia;
using SWSMonitor.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SWSMonitor;

public partial class BeachesView : ReactiveUserControl<BeachesViewModel>
{
    public MainWindowModel? _mainWindow = null;
    public BeachesView()
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
            await _mainWindow.ShowBusyPopup("Loading Beaches View...");
        else
            await _mainWindow.ShowNoBusyPopup();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        await SetBusy(false);
        // Control is fully ready, layout has occurred, and templates are applied.
    }
    private void ClosePopup_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.ViewModel!.IsPopupOpen = false;
        this.ViewModel!.PopupIsOpen = false;
    }

    private void UpdateBeach_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        UpdateInBackground();
    }
    private async Task UpdateInBackground()
    {
        BeachViewModel? bview = BeachViewModel.Instance;
        if (bview != null)
        {
            (bool success, BeachData? saved) = await bview.SaveBeach();
            if (!success || saved is null) //  didn't save successfully
            {
                Debug.WriteLine("Save of beach failed");
                TraceLogger.LogWarningAuto("Beep");
                return;
            }
            else
            {
                BeachData current = StaticData.Beaches.FirstOrDefault(n => n.BeachName == saved.BeachName);
                if (current is null)
                {
                    StaticData.Beaches.Add(saved);
                    this.ViewModel!.Beaches.Add(saved);
                }
                else
                {
                    StaticData.Beaches.Replace(current, saved);
                    this.ViewModel!.Beaches.Replace(current, saved);
                }
            }
        }
        this.ViewModel!.IsPopupOpen = false;
        return;
    }

    private void DataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (this.ViewModel!.SelectedBeach is null)
        {
            TraceLogger.LogWarningAuto("Beep"); return;
        }
        if (this.ViewModel!.UserIsAdmin)
            EditBeach();
        else
            ViewBeach();
    }

    private bool EditBeach()
    {
        if (this.ViewModel!.SelectedBeach is null)
        {
            TraceLogger.LogWarningAuto("Beep"); return false;
        }
        string target = this.ViewModel!.SelectedBeach?.BeachName ?? string.Empty;
        BeachViewModel? bview = BeachViewModel.Instance;
        if (bview != null)
            bview.LoadTargetBeach(target);
        this.ViewModel!.IsPopupOpen = true;
        return true;
    }

    private bool ViewBeach()
    {
        if (this.ViewModel!.SelectedBeach is null)
        {
            TraceLogger.LogWarningAuto("Beep"); return false;
        }
        string target = this.ViewModel!.SelectedBeach?.BeachName ?? string.Empty;
        BeachViewModel? bview = BeachViewModel.Instance;
        if (bview != null)
            bview.LoadTargetBeach(target);
        this.ViewModel!.IsPopupOpen = true;
        return true;
    }

    private void AddBeach_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        BeachData newbie = new() { ID = -1, BeachName = "" };
        BeachViewModel? bview = BeachViewModel.Instance;
        if (bview != null)
            bview.LoadTargetBeach("");
        this.ViewModel!.IsPopupOpen = true;
    }

    private void EditButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (e.Source is Button button)
        {
            if (button.Tag is not null && button.Tag is BeachData beach)
            {
                if (beach is not null)
                {
                    this.ViewModel!.SelectedBeach = beach;
                    EditBeach();
                }
            }
        }
    }
    private void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (e.Source is Button button)
        {
            if (button.Tag is not null && button.Tag is BeachData beach)
            {
                if (beach is not null)
                {
                    this.ViewModel!.SelectedBeach = beach;
                    this.ViewModel!.PopupMessage = "Beach will be permanently deleted. Do you want to continue?";
                    this.ViewModel!.PopupIsOpen = true;
                }
            }
        }
    }

    private void ViewButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (e.Source is Button button)
        {
            if (button.Tag is not null && button.Tag is BeachData beach)
            {
                if (beach is not null)
                {
                    this.ViewModel!.SelectedBeach = beach;
                    ViewBeach();
                }
            }
        }
    }

    private void ConfirmDeleteButton_Click(object? sender, RoutedEventArgs e)
    {
        // DeleteInBackground(beach);
        var beach = this.ViewModel!.SelectedBeach;
        this.ViewModel!.PopupIsOpen = false;

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            this.ViewModel!.SelectedBeach = null;
            //  await BeachDataCrud.DeleteBeachDataAsync(StaticData.DataSourceConfig, beach.ID);
            this.ViewModel!.Beaches.Remove(beach);
            StaticData.Beaches.Remove(beach);
        });
    }
}