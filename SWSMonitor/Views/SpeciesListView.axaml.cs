using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SWSMonitor.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System.Threading.Tasks;

namespace SWSMonitor;

public partial class SpeciesListView : ReactiveUserControl<SpeciesListViewModel>
{
    public SpeciesListView()
    {
        // InitializeComponent();
        this.DataContext = SpeciesListViewModel.Current;
        this.WhenActivated((ReactiveUI.Primitives.Disposables.MultipleDisposable disposables) => { });
        AvaloniaXamlLoader.Load(this);
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

    private void species_search_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            //NavigateFromTab(sender, e);
            return;
        }
        e.Handled = false;
    }

    private void SpeciesListDataGrid_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        // Don't do anything here because the pointer pressed event takes care of it?
    }

    private void Table_OnCellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        //if (e.PointerPressedEventArgs.Source is Control control)
        //{
        //    if (control.Tag is not null)
        //    {
        //        this.ViewModel!.SelectedSpeciesObservation = control.Tag as SpeciesObservation;
        //    }
        //}
        // Check if the event is for the specific column you want to trigger selection
        if (e.Column.Header.ToString() == "Species" || e.Column.Header.ToString() == "Notes")
        {
            // Get the data context of the row that was clicked
            if (e.Row.DataContext is SpeciesObservation clickedItem)
            {
                // Manually update the SelectedItem binding
                this.ViewModel!.SelectedSpeciesObservation = clickedItem;
            }
        }
    }

    AutoCompleteBox? _autoCompleteBox;
    private void species_search_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!listOpened && e.Source is TextBox textBox && sender is AutoCompleteBox autoCompleteBox)
        {
            _autoCompleteBox = autoCompleteBox;
            TestSpeciesAsync(autoCompleteBox.Text);
            e.Handled = true;
        }
        return;
    }

    private void species_search_LostFocus(object? sender, Avalonia.Input.FocusChangingEventArgs e)
    {
        if (!listOpened && e.Source is TextBox textBox && sender is AutoCompleteBox autoCompleteBox)
        {
            _autoCompleteBox = autoCompleteBox;
            TestSpeciesAsync(autoCompleteBox.Text);
            e.Handled = true;
        }
        return;
    }

    private async Task TestSpeciesAsync(string? text)
    {
        if (string.IsNullOrEmpty(this.ViewModel!.SelectedSpeciesObservation.SelectedSpeciesItem))
            this.ViewModel!.SelectedSpeciesObservation.SelectedSpeciesItem = text;

        await this.ViewModel!.TestSpeciesIfChanged(text);
        this.ViewModel!.SelectedSpeciesObservation?.Species = this.ViewModel!.SelectedSpeciesObservation?.SelectedSpeciesItem;

        //if (_autoCompleteBox is not null)
        //    _autoCompleteBox.Text = this.ViewModel!.SelectedSpeciesObservation?.SelectedSpeciesItem;
    }
    private bool listOpened = false;
    private void species_search_DropDownClosed(object? sender, System.EventArgs e)
    {
        listOpened = false;
    }

    private void species_search_DropDownOpened(object? sender, System.EventArgs e)
    {
        listOpened = true;
    }

    private void species_search_LostFocus(object? sender, FocusChangedEventArgs e)
    {
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

    private void DontDeleteObs_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        (this.DataContext as SpeciesListViewModel)?.IsErrorMessageOpen = false;
        (this.DataContext as SpeciesListViewModel)?.IsDeletePopupOpen = false;
        e.Handled = true;
    }
}