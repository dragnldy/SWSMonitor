using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Models;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SWSMonitor.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;

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

    private async void species_search_LostFocus(object? sender, FocusChangedEventArgs e)
    {
        if (e.Source is TextBox textBox && e.NewFocusedElement is not null)
        {
            if (textBox.DataContext is ProfileViewModel vm)
            {
                if (e.NewFocusedElement is ListBoxItem listItem)
                {
                    if (listItem.Content is string)
                    {
                        string picked = listItem.Content?.ToString() ?? string.Empty;
                        this.ViewModel!.AddMember(picked);
                        e.Handled = true;
                    }
                }
                //else
                //{
                //    this.ViewModel!.TestSpeciesFound(vm.SpeciesToAdd);
                //    this.ViewModel!.SpeciesToAdd = string.Empty;
                //}
            }
        }
    }

    private void Action_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.ViewModel!.AddNewSpecies();
        this.ViewModel!.IsActionPopupOpen = false;
    }

    private void Cancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.ViewModel!.ClearSpeciesField();
        this.ViewModel!.IsActionPopupOpen = false;
    }

    private void CancelError_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.ViewModel!.ClearSpeciesField();
        this.ViewModel!.IsErrorMessageOpen = false;
    }

    private void species_search_KeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is AutoCompleteBox autotextBox && e.Source is TextBox textBox)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Tab || e.Key == Key.Back)
            {
                string textboxtext = textBox.Text;
                e.Handled = true;
                if (!string.IsNullOrEmpty(textboxtext))
                {
                    Species? speciesFound = StaticData.Species.FirstOrDefault(n => n.ScientificName.Equals(
                        textboxtext, StringComparison.InvariantCultureIgnoreCase));
                    if (speciesFound is null)
                        this.ViewModel!.SpeciesNotFound(textboxtext);
                    else
                        this.ViewModel!.AddMember(speciesFound.ScientificName); // force standardized capitalization
                }
            }
            else if (e.Key == Key.Escape)
            {
                this.ViewModel!.ClearSpeciesField();
            }
        }
    }
}