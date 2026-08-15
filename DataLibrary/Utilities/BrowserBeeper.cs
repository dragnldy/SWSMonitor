using System.Runtime.InteropServices.JavaScript;

namespace DataLibrary.Utilities;

public static partial class SoundManager
{
    // Imports the JavaScript function from browserBeep.js
    [JSImport("playBeep", "browserBeep.js")]
    public static partial void PlayBeep();

    public static void Beep()
    {
        if (StaticData.RunningInBrowser)
            PlayBeep();
        else
            TraceLogger.LogWarningAuto("Beep");
    }
}