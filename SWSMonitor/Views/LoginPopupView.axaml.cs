using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SWSMonitor.ViewModels;
using ReactiveUI.Avalonia;
using System.ComponentModel;

namespace SWSMonitor;

public partial class LoginPopupView : ReactiveUserControl<LoginPopupViewModel>
{
    public LoginPopupView()
    {
        InitializeComponent();
        var vm = new LoginPopupViewModel();
        this.ViewModel = vm;
        this.DataContext = vm;

    }
    private void SignInButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        (this.ViewModel)!.AttemptSignIn();

    }

    private void SignOutButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        (this.ViewModel)!.AttemptSignOut();
    }

}