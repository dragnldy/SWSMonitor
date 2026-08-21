using Avalonia.Browser;
using Avalonia.Platform;
using DataLibrary;
using SWSMonitor.ViewModels;
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
        MapWebView.lastXOffset = (int)MainViewModel.XOffsetMainView + 50;
        MapWebView.lastYOffset = (int)MainViewModel.YOffsetMainView + 35;
        

        var mapdiv = EmbedInterop.CreateAndInitializeMap("leftlet-map", mapstartlat, mapstartlong, mapstartzoom,
            MapWebView.lastXOffset, MapWebView.lastYOffset, 350, 760);

        if (mapdiv is null)
        {
            TraceLogger.LogErrorAuto("Failed to create and initialize map div.");
            return null;
        }
        
        return new JSObjectControlHandle(mapdiv);
    }
}

internal static partial class EmbedInterop
{
    [JSImport("createAndInitializeMap", "mapInterop.js")]
    public static partial JSObject CreateAndInitializeMap(string elementId, double lat, double lng, int zoom, 
       int xoffset, int yoffset, int width, int height);
}
