using Avalonia.Controls;

namespace SWSMonitor;

public partial class MapsView : UserControl
{
    public MapsView()
    {
        InitializeComponent();
//        PART_WebView.WebViewNewWindowRequested += PART_WebView_WebViewNewWindowRequested;
    }

    private void WebView_NavigationCompleted(object? sender, object args)
    {
        //if (args.IsSuccess)
        //{
        //    // Navigation completed successfully
        //}
    }

    private void PART_WebView_WebViewNewWindowRequested(object? sender, object e)
    {
//        e.UrlLoadingStrategy = WebViewCore.Enums.UrlRequestStrategy.OpenInNewWindow;
    }

}