using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SWSMonitor.ViewModels;
using ReactiveUI.Avalonia;

namespace SWSMonitor;

public partial class TestView : ReactiveUserControl<TestViewModel>
{
    public TestView()
    {
        InitializeComponent();
        var vm = new TestViewModel();
        this.ViewModel = vm;
        this.DataContext = vm;
    }
}