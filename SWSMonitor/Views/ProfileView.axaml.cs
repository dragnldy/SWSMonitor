using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SWSMonitor.ViewModels;
using System.Diagnostics;

namespace SWSMonitor;

public partial class ProfileView : ReactiveUserControl<ProfileViewModel>
{
    public ProfileView()
    {
        this.WhenActivated((ReactiveUI.Primitives.Disposables.MultipleDisposable disposables) => { }); 
        AvaloniaXamlLoader.Load(this);
    }
    public void RemoveDetail_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Detail? detail = GetButtonParameters(sender);
        if (detail is not null)
        {
            this.ViewModel!.RemoveDetail(detail);
        }
    }
    private void SpeciesSearch_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not null && sender is AutoCompleteBox selector)
        {
            if (e.AddedItems is not null && e.AddedItems.Count > 0)
            {
                this.ViewModel!.AddMember(e.AddedItems[0].ToString());
                e.AddedItems.RemoveAt(0);
                this.ViewModel!.SpeciesToAdd = string.Empty;
            }
            e.Handled = true;
        }
    }

    private Detail? GetButtonParameters(object? sender)
    {
        if (sender is Button button)
        {
            if (button.CommandParameter is not null && button.CommandParameter is Detail detail)
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

    private void RemoveProfile_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.ViewModel!.RemoveProfile();
    }

    private void InsertProfile_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.ViewModel!.AddProfile();
    }

    private void CopyProfile_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.ViewModel!.AddProfile(copyCurrent: true);
    }

    private void IntegerTextBox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        NumbersOnlyTextBox_KeyDown(sender, e, new char[] { '+', '-' });
    }

    private void NumericTextBox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        NumbersOnlyTextBox_KeyDown(sender, e, new char[] { '+', '-', '.' });
    }
    private void NumbersOnlyTextBox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e, char[] allowed)
    {
        string? letter = e.KeySymbol;
        bool rejectKey = false;

        if (string.IsNullOrEmpty(letter))
        {
            e.Handled = rejectKey = false;
            return;
        }
        if (allowed.Length > 0)
        {
            char ch = letter[0];
            for (int i = 0; i < allowed.Length; i++)
            {
                if (allowed[i] == ch)
                {
                    e.Handled = false;
                    return;
                }
            }
        }

        if (e.Key == Key.Space)
        {
            // Spaces are not allowed
            e.Handled = true;
            return;
        }
        if (e.Key == Key.Escape || e.Key == Key.Tab || e.Key == Key.Space)
        {
            // Escape is used to restore contents
            e.Handled = false;
        }
        else if (e.Key == Key.Enter || e.Key == Key.Return)
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
        e.Handled = rejectKey;
    }

    private async void species_search_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (e.Source is TextBox textBox && !listOpened)
        {
            if (textBox.DataContext is ProfileViewModel vm)
            {
                textBox.Text = await this.ViewModel!.TestSpeciesFound(vm.SpeciesToAdd);
                this.ViewModel!.SpeciesToAdd = string.Empty;
            }
        }
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
}