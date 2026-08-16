using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SWSMonitor.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Models;
using DynamicData.Kernel;

namespace SWSMonitor;

public partial class TeamView : ReactiveUserControl<TeamViewModel>
{
    public TeamView()
    {
        //        InitializeComponent();
        this.WhenActivated((ReactiveUI.Primitives.Disposables.MultipleDisposable disposables) => { });
        AvaloniaXamlLoader.Load(this);
        var firstControl = this.FindControl<Control>("FirstTimeBox");

        // Attach to the AttachedToVisualTree event
        if (firstControl != null)
        {
            firstControl.AttachedToVisualTree += (sender, e) =>
            {
                // Set focus once the control is in the visual tree
                firstControl.Focus();
            };
        }
    }

    public void RemoveMember_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SurveyMember? member = GetButtonParameters(sender);
        if (member is not null)
        {
            this.ViewModel!.RemoveSelectedMember(member);
        }
    }
    private void volunteer_search_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            //NavigateFromTab(sender, e);
            return;
        }
        e.Handled = false;
    }

    private void volunteer_search_LostFocus(object? sender, FocusChangedEventArgs e)
    {
        if (e.Source is TextBox textBox && e.NewFocusedElement is not null)
        {
            if (!string.IsNullOrEmpty(textBox.Text))
            {
                this.ViewModel!.AddMember(textBox.Text);
            }
            if (e.NavigationMethod  == NavigationMethod.Tab || e.NavigationMethod == NavigationMethod.Directional)
            {
                textBox.Focus();
                e.Handled = true;
            }
        }
    }

    private SurveyMember? GetButtonParameters(object? sender)
    {
        if (sender is Button button)
        {
            if (button.CommandParameter is not null && button.CommandParameter is SurveyMember member)
                return member;
        }
        return null;
    }

    // Used to be GotFocusEventArgs but that doesn't exist in Avalonia, so using RoutedEventArgs instead and it seems to work fine
    private void TimeBox_GotFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not null && sender is TextBox timeBox)
        {
            timeBox.SelectAll();
        }
    }

    private void TimeTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        NumbersOnlyTextBox_KeyDown(sender, e, new char[] { ':' });
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
}