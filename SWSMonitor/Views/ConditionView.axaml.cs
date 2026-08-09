using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SWSMonitor.ViewModels;

namespace SWSMonitor;

public partial class ConditionView : ReactiveUserControl<ConditionViewModel>
{
    public ConditionView()
    {
        //        InitializeComponent();
        this.WhenActivated((ReactiveUI.Primitives.Disposables.MultipleDisposable disposables) => { }); 
        AvaloniaXamlLoader.Load(this);
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

}