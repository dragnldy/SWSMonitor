using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SWSMonitor.ViewModels;
using SWSMonitor.Views;
using DataLibrary.DataSources;
using Microsoft.Extensions.DependencyInjection;

namespace SWSMonitor;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        collection.AddSingleton<MainWindowViewModel>();
        collection.AddTransient<SurveyViewModel>();
        // Abstract the DataService
        collection.AddSingleton<IDataService, SurveyDataService>();
        ServiceProvider services  = collection.BuildServiceProvider();


        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = services.GetRequiredService<MainWindowViewModel>()
            };
 
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new SWSMonitor.MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();

    }
    //if (GlobalValues.ImportsDone)
    //{
    //    ConsoleLogger.ConsoleLog("App: OnFrameworkInitializationCompleted: Imports already done.");
    //}

    //base.OnFrameworkInitializationCompleted();
}