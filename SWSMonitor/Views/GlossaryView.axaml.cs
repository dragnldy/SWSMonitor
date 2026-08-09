using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SWSMonitor.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace SWSMonitor;

public partial class GlossaryView : ReactiveUserControl<SpeciesViewModel>
{
    public GlossaryView()
    {
        //        InitializeComponent();
        this.WhenActivated((ReactiveUI.Primitives.Disposables.MultipleDisposable disposables) => { }); 
        AvaloniaXamlLoader.Load(this);

        var vm = new SpeciesViewModel() { ID = 0 };
        this.ViewModel = vm;
        this.DataContext = vm;
    }

    private void PositiveIntegerTextBox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        NumbersOnlyTextBox_KeyDown(sender, e, new char[] { '+' });
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
        if (e.Key == Key.Escape || e.Key == Key.Tab)
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


    private void search_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            //NavigateFromTab(sender, e);
            return;
        }
        e.Handled = false;
    }
}