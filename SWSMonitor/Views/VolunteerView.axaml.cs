using Avalonia.Controls;
using Avalonia.Input;
using SWSMonitor.ViewModels;
using ReactiveUI.Avalonia;

namespace SWSMonitor;

public partial class VolunteerView : ReactiveUserControl<VolunteerViewModel>
{
    public VolunteerView()
    {
        InitializeComponent();
        var vm = new VolunteerViewModel();
        this.ViewModel = vm;
        this.DataContext = vm;
    }

    private void Island_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.W)
        {
            e.Handled = true;
            if (sender is TextBox textBox)
            {
                textBox.Text = "Whidbey";
            }
        }
        else if (e.Key == Avalonia.Input.Key.C)
        {
            e.Handled = true;
            if (sender is TextBox textBox)
            {
                textBox.Text = "Camano";
            }
        }
        else
        {
            e.Handled = false;
        }
    }
    private void City_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Space && e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Control))
        {
            // Logic to fetch and set suggestions
            // For example, trigger the population manually
            // and open the dropdown
            if (sender is AutoCompleteBox autoCompleteBox)
            {
                // Call the populator logic
                // and then ensure the dropdown is open
                autoCompleteBox.IsDropDownOpen = true;
                e.Handled = true; // Prevent the space from being typed in the box
            }
        }
    }
    private void State_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.W)
        {
            if (sender is TextBox textbox)
            {
                textbox.Text = "WA";
            }
        }
        e.Handled = false;

    }
    private void PhoneTextBox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        NumbersOnlyTextBox_KeyDown(sender, e, new char[] { '(', ')', '-' });
    }

    private void ZipTextBox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        NumbersOnlyTextBox_KeyDown(sender, e, new char[] { });
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
        //        Debug.WriteLine($"Key: {e.Key}, Symbol: <{letter}>, rejected: {rejectKey}");
        e.Handled = rejectKey;
    }

}
