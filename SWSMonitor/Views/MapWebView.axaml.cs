using Avalonia;
using Avalonia.Browser;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.JSInterop;
using Models;
using SWSMonitor.ViewModels;
using SWSMonitor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace SWSMonitor;

public partial class MapWebView : UserControl
{
    public static double MapStartPositionLong;
    public static double MapStartPositionLat;
    public static int MapStartZoom = 10;

    public static int lastXOffset = 0;
    public static int lastYOffset = 0;


    private bool _beachesLoaded = false;
    private BeachData _lastSelectedBeach = null;

    public MapWebView()
    {
        InitializeComponent();
        this.DataContext = MapWebViewModel.Instance;
        MapWebViewModel.Instance!.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(MapWebViewModel.ActiveOnly))
            {
                SetupMarkers();
            }
            else if (e.PropertyName == nameof(MapWebViewModel.SelectedBeach))
            {
                AdjustLastSelected();
            }
        };
        MapStartPositionLong = MapWebViewModel.Instance.MapPositionLong;
        MapStartPositionLat = MapWebViewModel.Instance.MapPositionLat;
    }

    private void AdjustLastSelected()
    {
        if (_lastSelectedBeach is not null && _lastSelectedBeach != MapWebViewModel.Instance.SelectedBeach)
        {
            EmbedLeaflet.Instance!.RestoreMarkerToNormal(_lastSelectedBeach);
            AdjustMarker(_lastSelectedBeach);
        }
        _lastSelectedBeach = MapWebViewModel.Instance.SelectedBeach;
        EmbedLeaflet.Instance!.ChangeMarkerToSelected(_lastSelectedBeach);
        AdjustMarker(_lastSelectedBeach);

        if (_lastSelectedBeach is not null && MapWebViewModel.Instance!.SurveyDates is not null)
        {
            List<SurveyBase> surveydates = MapWebViewModel.Instance!.SurveyDates!.ToList();
            SurveyBase? surveydateitem = surveydates!.FirstOrDefault(n => n.BeachName == _lastSelectedBeach!.BeachName);
            this.SurveyDatesDataGrid.SelectedItem = surveydateitem;
            this.SurveyDatesDataGrid.ScrollIntoView(surveydateitem, null);
        }
    }
    private void AdjustMarker(BeachData beach)
    {

        if (!beach.IsMonitored && MapWebViewModel.Instance.ActiveOnly)
            EmbedLeaflet.Instance!.HideMarkers(new List<BeachData> { beach });
        else
            EmbedLeaflet.Instance!.ShowMarkers(new List<BeachData> { beach });
    }

    protected override void OnLoaded(RoutedEventArgs args)
    {
        // Always call the base method first to raise the Loaded event
        base.OnLoaded(args);
//        _ = SetupTestImage();

        if (EmbedLeaflet.Instance is not null)
        {
            // Setup a timer for 10 seconds
            var timer = new System.Timers.Timer(1000); // 1000 milliseconds = 1 seconds
            timer.Elapsed += (sender, e) =>
            {
                timer.Stop(); // Stop the timer so it only fires once
                timer.Dispose(); // Dispose the timer
                SetupMarkers();
                MainViewModel.Current!.ShowNoBusyPopup();
            };
            timer.AutoReset = false; // Ensure it only fires once
            timer.Start();

        }

    }

    private async Task SetupTestImage()
    {
        Bitmap? _testImage = await GooglePhotoService.GetGooglePhoto("2026", "Ala Spit");
        if (_testImage is not null)
        {
            MapWebViewModel.Instance!.Bitmapimage = _testImage;
        };
    }

    private void SetupMarkers()
    {
        bool activeOnly = MapWebViewModel.Instance!.ActiveOnly;
        if (!_beachesLoaded)
        {
            EmbedLeaflet.Instance!.LoadMapMarkersAfterTimeout(StaticData.Beaches!);
        }
        List<BeachData> beachesToMark = StaticData.Beaches!.Where(n => !n.IsMonitored).ToList();
        if (activeOnly)
        {
            EmbedLeaflet.Instance!.HideMarkers(beachesToMark);
        }
        else
        {
            EmbedLeaflet.Instance!.ShowMarkers(beachesToMark);
        }
        _beachesLoaded = true;

        if (MapWebViewModel.Instance.SelectedBeach != null)
        {
            EmbedLeaflet.Instance!.ChangeMarkerToSelected(MapWebViewModel.Instance.SelectedBeach);
            _lastSelectedBeach = MapWebViewModel.Instance.SelectedBeach;
        }
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

    private void BeachesDataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
    }
    private void SurveyDatesDataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
    }

    private void OuterView_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
    }
}
public class EmbedLeaflet : NativeControlHost
{
    public static EmbedLeaflet? Instance = null;
    public static INativeMapControl? Implementation { get; set; }

    public EmbedLeaflet()
    {
        Instance = this;
    }

    protected override void OnLoaded(RoutedEventArgs args)
    {
        // Always call the base method first to raise the Loaded event
        base.OnLoaded(args);
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
        {
            topLevel.SizeChanged += TopLevel_SizeChanged;
            // Translates the control's top-left corner (0,0) relative to the window/toplevel root
            Point? relativePoint = this.TranslatePoint(new Point(0, 0), topLevel);
            MapWebView.lastXOffset = (int)relativePoint.Value.X;
            MapWebView.lastYOffset = (int)relativePoint.Value.Y;
        }
    }
    private void TopLevel_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        // Translates the control's top-left corner (0,0) relative to the window/toplevel root
        Point? relativePoint = this.TranslatePoint(new Point(0, 0), topLevel);

        if (relativePoint.HasValue)
        {
            if ((int)MapWebView.lastXOffset != (int)relativePoint.Value.X || (int)MapWebView.lastYOffset != (int)relativePoint.Value.Y)
            {
                RepositionMap((int)relativePoint.Value.X - MapWebView.lastXOffset,
                    (int)relativePoint.Value.Y - MapWebView.lastYOffset);

                MapWebView.lastXOffset = (int)relativePoint.Value.X;
                MapWebView.lastYOffset = (int)relativePoint.Value.Y;
            }
        }


    }

    internal void RepositionMap(int x, int y)
    {
        NativeMethods.RepositionDivRelative(StaticData.MapContainerName, x, y);
    }

    public void LoadMapMarkersAfterTimeout(IEnumerable<BeachData> beaches)
    {
        foreach (var beach in beaches)
        {
            MarkersInterop.AddMarker2Map(StaticData.MapContainerName, beach.Lat, beach.Long, beach.IsMonitored, beach.BeachName, beach.ID.ToString());
        }
    }

    public void ShowMarkers(IEnumerable<BeachData> beaches)
    {
        foreach (var beach in beaches)
        {
            MarkersInterop.ShowMarker(beach.ID.ToString());
        }
    }

    public void HideMarkers(IEnumerable<BeachData> beaches)
    {
        foreach (var beach in beaches)
        {
            MarkersInterop.HideMarker(beach.ID.ToString());
        }
    }

    public void RestoreMarkerToNormal(BeachData lastSelectedBeach)
    {
        MarkersInterop.ChangeMarker2Original(lastSelectedBeach.ID.ToString());
    }

    public void ChangeMarkerToSelected(BeachData newSelection)
    {
        MarkersInterop.ChangeMarker2Selected(newSelection.ID.ToString());
    }

    [JSInvokable]
    public Task OnMarkerClick(double lat, double lng, int id)
    {
        if (MapWebViewModel.Instance!.Beaches is not null)
        {
            BeachData? newSelection = MapWebViewModel.Instance!.Beaches!.FirstOrDefault(b => b.ID == id);
            if (newSelection is null)
            {
                throw new Exception($"Bad selection for marker id {id}");
            }

            // fire off event notification that selected beach has changed
            StaticData.SetSelectedBeach(newSelection);
        }
        return Task.CompletedTask;
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        IPlatformHandle handle = Implementation?.CreateControl(parent, () => base.CreateNativeControlCore(parent))
            ?? base.CreateNativeControlCore(parent);

        double mapstartlong = MapWebView.MapStartPositionLong;
        double mapstartlat = MapWebView.MapStartPositionLat;
        int mapstartzoom = MapWebView.MapStartZoom;
        MapWebView.lastXOffset = (int)MainViewModel.XOffsetMainView + 50;
        MapWebView.lastYOffset = (int)MainViewModel.YOffsetMainView + 35;


        var mapdiv = MarkersInterop.CreateAndInitializeMap(StaticData.MapContainerName, mapstartlat, mapstartlong, mapstartzoom,
            MapWebView.lastXOffset, MapWebView.lastYOffset, 350, 760);

        if (mapdiv is null)
        {
            TraceLogger.LogErrorAuto("Failed to create and initialize map div.");
            return null;
        }

        return new JSObjectControlHandle(mapdiv);
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        base.DestroyNativeControlCore(control);
    }
}

public interface INativeMapControl
{
    /// <param name="parent"></param>
    /// <param name="createDefault"></param>
    IPlatformHandle CreateControl(IPlatformHandle parent, Func<IPlatformHandle> createDefault);
}

internal static partial class MarkersInterop
{
    [JSImport("add_marker", "mapInterop.js")]
    public static partial JSObject AddMarker2Map(string elementId, double lat, double lng, bool isActive, string popupText, string id);

    [JSImport("change_marker_to_original", "mapInterop.js")]
    public static partial JSObject ChangeMarker2Original(string id);

    [JSImport("change_marker_to_selected", "mapInterop.js")]
    public static partial JSObject ChangeMarker2Selected(string id);

    [JSImport("show_marker", "mapInterop.js")]
    public static partial JSObject ShowMarker(string id);

    [JSImport("hide_marker", "mapInterop.js")]
    public static partial JSObject HideMarker(string id);
    [JSImport("createAndInitializeMap", "mapInterop.js")]
    public static partial JSObject CreateAndInitializeMap(string elementId, double lat, double lng, int zoom,
   int xoffset, int yoffset, int width, int height);

}
