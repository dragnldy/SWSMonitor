using SWSMonitor.ViewModels;
using ReactiveUI.Avalonia;

namespace SWSMonitor;

public partial class LoginView : ReactiveUserControl<LoginViewModel>
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void SignInButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.ViewModel!.AttemptSignIn();

    }

    private void SignOutButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.ViewModel!.AttemptSignOut();
    }
}