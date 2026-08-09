using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace SWSMonitor;

public partial class MapWebView : UserControl
{
    public MapWebView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs args)
    {
        // Always call the base method first to raise the Loaded event
        base.OnLoaded(args);

        LaunchUrl("https://swsmonitor.github.io/monitor4web");
        // LaunchUrl("https://soundwaterstewards.org/projects/intertidal-monitoring/");
    }

    private async void LaunchUrl(string url)
    {
        // Get the TopLevel container associated with this control
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel?.Launcher != null)
        {
            var targetUri = new Uri(url);

            // Launches the URL in a new browser tab
            await topLevel.Launcher.LaunchUriAsync(targetUri);
        }
    }

}