using Avalonia.Browser;
using Avalonia.Platform;
using System;
using System.Runtime.InteropServices.JavaScript;

namespace SWSMonitor;

public class EmbedLeafletBrowser : INativeMapControl
{
    public IPlatformHandle CreateControl(IPlatformHandle parent, Func<IPlatformHandle> createDefault)
    {
        double mapstartlong = MapWebView.MapStartPositionLong;
        double mapstartlat = MapWebView.MapStartPositionLat;
        int mapstartzoom = MapWebView.MapStartZoom;

        var mapdiv = EmbedInterop.CreateAndInitializeMap("leftlet-map", mapstartlat, mapstartlong, mapstartzoom);
        return new JSObjectControlHandle(mapdiv);
    }
}

internal static partial class EmbedInterop
{
    [JSImport("createAndInitializeMap", "mapInterop.js")]
    public static partial JSObject CreateAndInitializeMap(string elementId, double lat, double lng, int zoom);
}
