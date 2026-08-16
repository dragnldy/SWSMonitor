using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using DataLibrary.Crud;
using Models;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SWSMonitor.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SWSMonitor;

public partial class SpeciesListView : ReactiveUserControl<SpeciesListViewModel>
{
    public static SpeciesListView? Instance;
    public SpeciesListView()
    {
        Instance = this;
        // InitializeComponent();
        this.DataContext = SpeciesListViewModel.Current;
        this.WhenActivated((ReactiveUI.Primitives.Disposables.MultipleDisposable disposables) => { });
        AvaloniaXamlLoader.Load(this);
    }

    private void species_search_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            //NavigateFromTab(sender, e);
            return;
        }
        e.Handled = false;
    }

    private void species_search_LostFocus(object? sender, FocusChangedEventArgs e)
    {
        if (e.Source is TextBox textBox && e.NewFocusedElement is not null)
        {
            if (textBox.DataContext is SpeciesObservation detail)
            {
                if (e.NewFocusedElement is ListBoxItem listItem)
                {
                    string picked = listItem.Content?.ToString() ?? string.Empty;
                    detail.Species = picked;
                }
                this.ViewModel!.TestSpeciesFound(detail);
            }
        }
    }

    private void Cancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        (this.DataContext as SpeciesListViewModel)?.IsErrorMessageOpen = false;
        (this.DataContext as SpeciesListViewModel)?.IsActionPopupOpen = false;
        (this.DataContext as SpeciesListViewModel)?.IsDeletePopupOpen = false;
        e.Handled = true;

    }
    private void DontAdd_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SpeciesObservation? detail = (this.DataContext as SpeciesListViewModel)!._detailToAdd;
        (this.DataContext as SpeciesListViewModel)?.DeleteSpeciesObservation(detail);
        e.Handled = true;

    }

    private void RequestAdd_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SpeciesObservation? detail = (this.DataContext as SpeciesListViewModel)!._detailToAdd;
        var loadedSurvey = HomeViewModel.Instance!.LoadedSurvey;
        Species newSpecies = new Species()
        {
            ID = -1,
            ScientificName = detail.Species,
            UsedBySurveys = 1,
            ProfileData = 1,
            ChangeDate = DateTime.Today,
            ChangeReason = $"Added during data entry for Survey ID: {loadedSurvey!.ID} for Beach: {loadedSurvey.BeachName} Date: {loadedSurvey.SurveyDate}",
        };
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            (bool success, Species created) = await SpeciesCrud.UpdateOrCreateSpeciesAsync(StaticData.DataSourceConfig, newSpecies);
            if (success)
            {
                StaticData.Species.Add(created);
            }
        });
        (this.DataContext as SpeciesListViewModel)!.AddNewSpeciesObservation(detail);
        e.Handled = true;
        return;
    }


    private void DeleteObs_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_observationToDelete is not null)
        {
            (this.DataContext as SpeciesListViewModel)?.DeleteSpeciesObservation(_observationToDelete);
        }
        (this.DataContext as SpeciesListViewModel)?.IsDeletePopupOpen = false;
        e.Handled = true;
    }

    private SpeciesObservation? _observationToDelete = null;
    private async void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (e.Source is Control control && control.Tag is SpeciesObservation observation)
        {
            SpeciesListViewModel viewModel = (this.DataContext as SpeciesListViewModel)!;
            if (observation is not null)
            {
                if (observation.IsPlaceHolder)
                {

                    viewModel.ErrorMessageText = "You cannot delete the place holder observation.";
                    viewModel.IsErrorMessageOpen = true;
                }
                else
                {
                    _observationToDelete = observation;
                    viewModel.IsDeletePopupOpen = true;
                }
            }
        }
        e.Handled = true;
    }

    private void RemoveSpecies_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DeleteButton_Click(sender, e);
    }

    internal void ScrollIntoView(SpeciesObservation selectedSpeciesObservation)
    {
        Dispatcher.UIThread.Post(() =>
        {
            this.ViewModel!.SelectedSpeciesObservation = selectedSpeciesObservation;
            DataGrid? myGrid = this.FindControl<DataGrid>("SpeciesListDataGrid");
            if (myGrid is not null)
            {
                var targetItem = myGrid.ItemsSource.Cast<SpeciesObservation>().FirstOrDefault(
                    n => n.Species == selectedSpeciesObservation.Species);

                if (targetItem != null)
                {
                    myGrid.ScrollIntoView(targetItem, null);
                }
            }
        }, DispatcherPriority.Background);
    }
}