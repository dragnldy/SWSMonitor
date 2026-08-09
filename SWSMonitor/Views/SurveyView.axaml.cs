using Avalonia.Markup.Xaml;
using SWSMonitor.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace SWSMonitor;

public partial class SurveyView : ReactiveUserControl<SurveyViewModel>
{
    public SurveyView()
    {
        //        InitializeComponent();
        this.WhenActivated((ReactiveUI.Primitives.Disposables.MultipleDisposable disposables) => { }); 
        AvaloniaXamlLoader.Load(this);
    }

    private void ViewSurvey_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var vm = this.DataContext as SurveyViewModel;
        vm.ViewSurvey();
    }

    private void EditSurvey_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var vm = this.DataContext as SurveyViewModel;
        vm.EditSurvey();
    }

    private void CreateSurvey_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var vm = this.DataContext as SurveyViewModel;
        vm.CreateSurvey();
    }

    private void ConfirmNewSurvey_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var vm = this.DataContext as SurveyViewModel;
        vm.ConfirmNewSurvey();
    }

    private void CancelNewSurvey_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var vm = this.DataContext as SurveyViewModel;
        vm.CancelNewSurvey();
    }
}