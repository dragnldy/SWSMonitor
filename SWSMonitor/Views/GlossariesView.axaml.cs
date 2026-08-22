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

public partial class GlossariesView : ReactiveUserControl<GlossariesViewModel>
{
    public MainWindowModel? _mainWindow = null;

    public GlossariesView()
    {
        MainWindowModel main = StaticData.MainWindowModel as MainWindowModel;
        _mainWindow = main;
        this.DataContext = GlossariesViewModel.Instance;

        this.WhenActivated((ReactiveUI.Primitives.Disposables.MultipleDisposable disposables) => { });
        AvaloniaXamlLoader.Load(this);
    }
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        await _mainWindow?.ShowNoBusyPopup();
        // Control is fully ready, layout has occurred, and templates are applied.
    }

    private void ClosePopup_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.ViewModel!.IsPopupOpen = false;
        this.ViewModel!.IsReadOnlyPopupOpen = false;
        this.ViewModel!.PopupIsOpen = false;
    }

    private void UpdateSpecies_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        UpdateInBackground();
    }
    private async Task UpdateInBackground()
    {
        SpeciesViewModel? gview = SpeciesViewModel.Instance;
        if (gview != null)
        {
            (bool success, Species saved) = await gview.SaveSpecies();
            if (!success || saved == null) //  didn't save successfully
            {
                Debug.WriteLine("Update of species failed");
            }
            else
            {
                Species current = StaticData.Species.FirstOrDefault(n => n.ScientificName == saved.ScientificName);
                if (current is null)
                {
                    StaticData.Species.Add(saved);
                    this.ViewModel!.Species.Add(saved);
                }
                else
                {
                    StaticData.Species.Replace(current, saved);
                    this.ViewModel!.Species.Replace(current, saved);
                }
            }
        }
        this.ViewModel!.IsPopupOpen = false;
        return;
    }

    private void DataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (this.ViewModel!.SelectedSpecies is null)
        {
            TraceLogger.LogWarningAuto("Beep"); return;
        }
        if (this.ViewModel!.UserIsAdmin)
            EditSpecies();
        else
            ViewSpecies();
    }

    private bool EditSpecies()
    {
        if (this.ViewModel!.SelectedSpecies is null)
        {
            TraceLogger.LogWarningAuto("Beep"); return false;
        }
        if (!this.ViewModel!.UserIsAdmin)
            return false;

        string target = this.ViewModel!.SelectedSpecies?.ScientificName ?? string.Empty;
        SpeciesViewModel.Instance.LoadTargetSpecies(target);
        this.ViewModel!.IsPopupOpen = true;
        return true;
    }

    private bool ViewSpecies()
    {
        if (this.ViewModel!.SelectedSpecies is null)
        {
            TraceLogger.LogWarningAuto("Beep"); return false;
        }
        string target = this.ViewModel!.SelectedSpecies?.ScientificName ?? string.Empty;
        SpeciesViewModel? gview = SpeciesViewModel.Instance;
        if (gview != null)
            gview.LoadTargetSpecies(target);

        if (this.ViewModel!.UserIsAdmin)
        {
            SpeciesViewModel.Instance.LoadTargetSpecies(target);
            this.ViewModel!.IsPopupOpen = true;
        }
        else
        {
            SpeciesReadOnlyViewModel.Instance.LoadTargetSpecies(target);
            this.ViewModel!.IsReadOnlyPopupOpen = true;
        }
        return true;
    }

    private void AddSpecies_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (this.ViewModel!.UserIsAdmin == false)
        {
            TraceLogger.LogWarningAuto("Beep"); return;
        }

        SpeciesViewModel? gview = SpeciesViewModel.Instance;
        if (gview != null)
            gview.LoadTargetSpecies("");
        this.ViewModel!.IsPopupOpen = true;
    }

    private void EditButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (e.Source is Button button)
        {
            if (button.Tag is not null && button.Tag is Species species)
            {
                if (species is not null)
                {
                    this.ViewModel!.SelectedSpecies = species;
                    EditSpecies();
                }
            }
        }
    }
    private void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (this.ViewModel!.UserIsAdmin == false)
        {
            TraceLogger.LogWarningAuto("Beep"); return;
        }

        if (e.Source is Button button)
        {
            if (button.Tag is not null && button.Tag is Species species)
            {
                if (species is not null)
                {
                    this.ViewModel!.SelectedSpecies = species;
                    this.ViewModel!.PopupMessage = "Species will be permanently deleted. Do you want to continue?";
                    this.ViewModel!.PopupIsOpen = true;
                }
            }
        }
    }

    private void ViewButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (e.Source is Button button)
        {
            if (button.Tag is not null && button.Tag is Species species)
            {
                if (species is not null)
                {
                    this.ViewModel!.SelectedSpecies = species;
                    ViewSpecies();
                }
            }
        }
    }
    private void ConfirmDeleteButton_Click(object? sender, RoutedEventArgs e)
    {
        // DeleteInBackground(species);
        var species = this.ViewModel!.SelectedSpecies;
        this.ViewModel!.PopupIsOpen = false;
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            // await SpeciesCrud.DeleteSpeciesAsync(StaticData.DataSourceConfig!, species.ID);
            this.ViewModel!.SelectedSpecies = null;
            StaticData.Species.Remove(species);
            this.ViewModel!.Species.Remove(species);
        });

    }

    // second parameter used to be GotFocusEventArgs but that is not available in Avalonia, so using RoutedEventArgs instead and ignoring it since we don't need it
    private void TextBox_GotFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }

    private void TextBox_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }
}