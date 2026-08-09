using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DataLibrary;

public static class TraceLogger
{
    private static  string _logFilePath = string.Empty;

    public static void LogErrorAuto(string message,
        [CallerFilePath] string file = "",
        [CallerMemberName] string method = "", 
        [CallerLineNumber] int line = 0)
    {
        LogError($"{Path.GetFileNameWithoutExtension(file)}.{method}", $"Line: {line}", message);
    }
    public static void LogInformation(string message,
        [CallerFilePath] string file = "",
        [CallerMemberName] string method = "",
        [CallerLineNumber] int line = 0)
    {
        Trace.TraceInformation(Echo2LogFile("Info", $"{Path.GetFileNameWithoutExtension(file)}.{method}", $"Line: {line}", message));
    }
    public static void LogWarningAuto(string message,
        [CallerFilePath] string file = "",
        [CallerMemberName] string method = "",
        [CallerLineNumber] int line = 0)
    {
        LogWarning($"{Path.GetFileNameWithoutExtension(file)}.{method}", $"Line: {line}", message);
    }
    public static void LogError(string v1, string v2, string v3)
    {
        Trace.TraceError(Echo2LogFile("Error", v1,v2,v3));
    }

    private static string? Echo2LogFile(string level, string v1, string v2, string v3)
    {
        // Remove all but the class and method name from string V1
        // Remove directory information from V1
        if (v1.Contains("\\"))
            v1 = v1.Split('\\').Last();

        string error = $"{level} in {v1}.{v2}: {v3}";
        if (!OperatingSystem.IsBrowser() && !string.IsNullOrEmpty(_logFilePath))
        {
            File.AppendAllText(_logFilePath, $"{DateTime.Now.ToShortDateString()}:{DateTime.Now.ToShortTimeString()}-{error}{Environment.NewLine}");
        }
        else if (OperatingSystem.IsBrowser())
        {
            Console.WriteLine(error);
        }
        return error;
    }

    public static void LogWarning(string v1, string v2, string v3)
    {
        Trace.TraceWarning(Echo2LogFile("Warning", v1, v2, v3));
    }

    public static void SetupTrace(string logfile = "")
    {
        // Note when running in the browser, output goes to the developer's tool console

        _logFilePath = logfile;
        if (!OperatingSystem.IsBrowser() && !string.IsNullOrEmpty(_logFilePath))
        {
            if (!File.Exists(logfile))
            {
                string directory =  Path.GetDirectoryName(logfile);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                Echo2LogFile("Information","TraceLogger", "SetupTrace", $"Creating log file at {logfile}");
            }

            Trace.Listeners.Add(new TextWriterTraceListener(logfile + ".trace"));
        }
        Trace.AutoFlush = true;
        TraceLogger.LogInformation("Setting up Error Logging");
    }
}
/*
 * 
Files: TextWriterTraceListener.
Event Logs: EventLogTraceListener.
Console: ConsoleTraceListener (useful for console applications).
Trace.Write, Trace.WriteLine: Writes a message to the listeners.
Trace.WriteIf, Trace.WriteLineIf: Writes a message only if a specified condition is true.
Trace.Assert: Checks a condition and outputs a message and optionally displays a message box if the condition is false, typically used for logic errors during development.
Trace.TraceInformation, Trace.TraceWarning, Trace.TraceError: Writes messages with specific event types. 
 *
 */
