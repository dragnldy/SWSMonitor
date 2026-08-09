using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SWSMonitor.ViewModels;

namespace SWSMonitor;

public partial class BeachSettingView : ReactiveUserControl<BeachSettingViewModel>
{
    public BeachSettingView()
    {
        //        InitializeComponent();
        this.WhenActivated((ReactiveUI.Primitives.Disposables.MultipleDisposable disposables) => { }); 
        AvaloniaXamlLoader.Load(this);
    }

    private async void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (e.Source is Control control && control.Tag is Content observation)
        {
            if (observation is not null)
            {
                if (observation.IsPlaceHolder)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard(
                        "Error",
                        "You cannot delete the place holder observation.",
                        ButtonEnum.Ok);
                    var result = await box.ShowAsync();
                    e.Handled = true;
                    return;
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard(
                        "Confirm",
                        "Are you sure you want to delete this observation?",
                        ButtonEnum.YesNoAbort);
                    var result = await box.ShowAsync();
                    if (result == ButtonResult.Yes)
                    {
                        this.ViewModel?.DeleteContentObservation(observation);
                    }
                    e.Handled = true;
                }

            }
        }
    }

    private void contents_search_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            //NavigateFromTab(sender, e);
            return;
        }
        e.Handled = false;
    }

    private void ContentsDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
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
        if (e.Column.Header.ToString() == "Contents")
        {
            // Get the data context of the row that was clicked
            if (e.Row.DataContext is Content clickedItem)
            {
                // Manually update the SelectedItem binding
                this.ViewModel!.SelectedContents = clickedItem;
            }
        }
    }
    private void Contents_search_GotFocus(object? sender, FocusChangedEventArgs e)
    {
        if (sender is not null && sender is AutoCompleteBox textBox)
        {
        }
    }

    private void contents_search_LostFocus(object? sender, FocusChangedEventArgs e)
    {
        if (!listOpened && e.Source is TextBox textBox && sender is AutoCompleteBox autoCompleteBox)
        {
            if (autoCompleteBox.SelectedItem == null && !string.IsNullOrWhiteSpace(autoCompleteBox.Text))
            {
                // Clear the invalid text or display an error
                autoCompleteBox.Text = string.Empty;
                // Optionally: show an error message
            }
        }
        return;
    }

    private bool listOpened = false;
    private void contents_search_DropDownOpened(object? sender, System.EventArgs e)
    {
        if (sender is AutoCompleteBox autoCompleteBox)
        {
            if (!listOpened)
            {
                return;
            }
            string text = autoCompleteBox.Text;
            autoCompleteBox.SelectedItem = text;
        }
        listOpened = false;
    }

    private void contents_search_DropDownClosed(object? sender, System.EventArgs e)
    {
        listOpened = true;
    }

}
