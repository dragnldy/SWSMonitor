using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SWSMonitor.ViewModels;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Google.Apis.Requests.BatchRequest;

namespace SWSMonitor;

public partial class QuadratView : ReactiveUserControl<QuadratViewModel>
{
    public QuadratView()
    {
        // InitializeComponent();
        this.WhenActivated((ReactiveUI.Primitives.Disposables.MultipleDisposable disposables) => { });
        AvaloniaXamlLoader.Load(this);
    }
    private SpeciesDetail? GetButtonParameters(object? sender)
    {
        if (sender is Button button)
        {
            if (button.CommandParameter is not null && button.CommandParameter is SpeciesDetail detail)
                return detail;
        }
        return null;
    }


    private void HandleNonNumericInput(object? sender, KeyEventArgs? e)
    {
        string? letter = e.KeySymbol;
        bool rejectKey;
        if (string.IsNullOrEmpty(letter))
        {
            rejectKey = true;
        }
        else if (e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape)
        {
            TextBox? tb = e.Source as TextBox;
            TopLevel? tl = TopLevel.GetTopLevel(this);
            tl!.Focus();
            rejectKey = true;
        }
        else
        {
            rejectKey = !char.IsNumber(letter[0]);
        }
        Debug.WriteLine($"Key: {e.Key}, Symbol: <{letter}>, rejected: {rejectKey}");
        e.Handled = rejectKey; char.IsNumber(e.KeySymbol[0]);
    }

    private void MaskedTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        string? letter = e.KeySymbol;
        bool rejectKey = false;

        if (e.Key == Key.Tab)
        {
            NavigateFromTab(sender as TextBox, e);
            return;
        }
        if (string.IsNullOrEmpty(letter))
        {
            rejectKey = false;
        }
        else if (e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape)
        {
            TextBox? tb = e.Source as TextBox;
            TopLevel? tl = TopLevel.GetTopLevel(this);
            tl!.Focus();
            rejectKey = true;
        }
        else
        {
            char key = letter[0];
            if (key == '#' || key == '%' || key == '.' || key == '+')
            {
                rejectKey = false;
            }
            else
            {
                rejectKey = !char.IsNumber(key);
            }
        }
        Debug.WriteLine($"Key: {e.Key}, Symbol: <{letter}>, rejected: {rejectKey}");
        e.Handled = rejectKey;
    }

    private void NavigateFromTab(object sender, KeyEventArgs e)
    {
        if (sender is AutoCompleteBox || sender is TextBox)
            NavigateFromTab(sender as Control, e);
        return;
    }

    private void NavigateFromTab(Control currentControl, KeyEventArgs e)
    {
        // Handle both tab and backtab navigation in a grid within a ListBoxItem

        if (e.Key != Key.Tab) { return; }

        bool isbacktab = !((e.KeyModifiers & KeyModifiers.Shift) == 0);

        int tabIndex = currentControl.TabIndex;
        if (tabIndex <= 0) { return; }

        var currentListBoxItem = currentControl.GetVisualAncestors().OfType<ListBoxItem>().FirstOrDefault();
        if (currentListBoxItem == null) { return; }

        Control? nextControl = null;

        if (!isbacktab)
        {
            nextControl = currentListBoxItem.GetVisualDescendants()
            .OfType<Control>()
            .Where(c => c.TabIndex > tabIndex && c.TabIndex < 99999 && c.Focusable && c.IsVisible)
            .OrderBy(c => c.TabIndex)
            .FirstOrDefault();
            // We were already at the last control- need to move up to next listbox item
            if (nextControl == null)
            {
                // For now just reset t the first control in the current item
                nextControl = currentListBoxItem.GetVisualDescendants()
                    .OfType<Control>()
                    .Where(c => c.TabIndex < 99999 && c.Focusable && c.IsVisible)
                    .OrderBy(c => c.TabIndex)
                    .FirstOrDefault();

                //    var detail = currentControl.Tag as SpeciesDetail;
                //    ((QuadratViewModel)this.DataContext).MoveToNextDetail(detail);
                //    currentListBoxItem = currentControl.GetVisualAncestors().OfType<ListBoxItem>().FirstOrDefault();
                //    nextControl = currentListBoxItem.GetVisualDescendants()
                //    .OfType<Control>()
                //    .Where(c => c.Focusable && c.IsVisible)
                //    .OrderBy(c => c.TabIndex)
                //    .FirstOrDefault();
            }
        }
        else
        {
            nextControl = currentListBoxItem.GetVisualDescendants()
            .OfType<Control>()
            .Where(c => c.TabIndex < tabIndex && c.TabIndex >= 0 && c.Focusable && c.IsVisible)
            .OrderByDescending(c => c.TabIndex)
            .FirstOrDefault();
            if (nextControl == null)
            {
                // For now just reset t the first control in the current item
                nextControl = currentListBoxItem.GetVisualDescendants()
                    .OfType<Control>()
                    .Where(c => c.TabIndex < 99999 && c.Focusable && c.IsVisible)
                    .OrderByDescending(c => c.TabIndex)
                    .FirstOrDefault();
                //if (nextControl == null)
                //{
                //    var detail = currentControl.Tag as SpeciesDetail;
                //    ((QuadratViewModel)this.DataContext).MoveToPreviousDetail(detail);

                //    currentListBoxItem = currentControl.GetVisualAncestors().OfType<ListBoxItem>().FirstOrDefault();
                //    nextControl = currentListBoxItem.GetVisualDescendants()
                //    .OfType<Control>()
                //    .Where(c => c.Focusable && c.IsVisible)
                //    .OrderBy(c => c.TabIndex)
                //    .FirstOrDefault();
            }
        }

        if (nextControl != null)
        {
            nextControl.Focus();
            e.Handled = true; // Mark the event as handled
            return;
        }

        //var listBox = currentListBoxItem.GetVisualAncestors().OfType<ListBox>().FirstOrDefault();
        //var currentIndex = listBox.ItemContainerGenerator.IndexFromContainer(currentListBoxItem);

        //currentIndex = isbacktab? --currentIndex: ++currentIndex;
        //if (currentIndex < 0 || currentIndex >= listBox.Items.Count() - 1)
        //{
        //    e.Handled = false;
        //    return;
        //}
        //var nextListBoxItem = listBox.ItemContainerGenerator.ContainerFromIndex(currentIndex) as ListBoxItem;
        //if (nextListBoxItem != null)
        //{
        //    // Focus the first focusable control within the next ListBoxItem
        //    // (focusManager as FocusManager).Focus(nextListBoxItem, NavigationMethod.Directional);
        //    nextListBoxItem.Focus();
        //    e.Handled = true; // Mark the event as handled
        //    return;
        //}
        e.Handled = false;
    }

    //private async Task AddSpeciesButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    //{
    //    SpeciesDetail? detail = GetButtonParameters(sender);
    //    if (detail is not null)
    //    {
    //        ((QuadratViewModel)this.DataContext).AddNewSpecies(detail);
    //    }
    //    return;
    //}

    // second parameter used to be GotFocusEventArgs but that is not available in Avalonia, so using FocusChangedEventArgs instead and ignoring it since we don't need it
    private void Grid_GotFocus(object? sender, FocusChangedEventArgs e)
    {
        SpeciesDetail? detail = (sender as Grid).Tag as SpeciesDetail;
        if (detail != ((QuadratViewModel)this.DataContext).SelectedDetail)
            ((QuadratViewModel)this.DataContext).SelectedDetail = detail;
    }
    private void Grid_LostFocusx(object? sender, FocusChangedEventArgs e)
    {
        SpeciesDetail? detail = (sender as Grid).Tag as SpeciesDetail;
        ((QuadratViewModel)this.DataContext).TestSpeciesFound(detail);
    }

    private void species_search_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            NavigateFromTab(sender, e);
            return;
        }
        e.Handled = false;
    }

    private void TextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            NavigateFromTab(sender as TextBox, e);
            return;
        }
        e.Handled = false;
    }

    private void species_search_LostFocus(object? sender, FocusChangedEventArgs e)
    {
        if (e.Source is TextBox textBox && e.NewFocusedElement is not null)
        {
            if (textBox.DataContext is SpeciesDetail detail)
            { 
                if (e.NewFocusedElement is ListBoxItem listItem)
                {
                    string picked = listItem.Content?.ToString() ?? string.Empty;
                }
                else
                {
                    this.ViewModel!.TestSpeciesFound(detail);
                }
            }
        }
    }
    private void Cancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        (this.DataContext as QuadratViewModel)?.IsErrorMessageOpen = false;
        (this.DataContext as QuadratViewModel)?.IsActionPopupOpen = false;
        e.Handled = true;

    }

    internal bool _addingSpecies = false;
    internal bool _removingDetail = false;
    private void Action_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_addingSpecies)
        {
            SpeciesDetail? detail = (this.DataContext as QuadratViewModel)!._detailToAdd;
            bool success = (this.DataContext as QuadratViewModel)!.AddNewSpecies(detail);
            if (!success)
                detail.ResetSpecies();
            detail.SpeciesNotFound = false;
            _addingSpecies = false;
        }
        else if (_removingDetail)
        {
            ((QuadratViewModel)this.DataContext).RemoveSpecies();
            _removingDetail = false;
        }
        (this.DataContext as QuadratViewModel)!.IsActionPopupOpen = false;
        return;
    }
    public void RemoveSpecies_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        SpeciesDetail? detail = GetButtonParameters(sender);
        if (detail is not null)
        {
            ((QuadratViewModel)this.DataContext)._detailToRemove = detail;
            _removingDetail = true;
            ((QuadratViewModel)this.DataContext).ActionMessageText = "Are you sure you want to delete this observation?";
            ((QuadratViewModel)this.DataContext).IsActionPopupOpen = true;
        }
    }
}