using Pastel;
using System.Diagnostics;
using System.Drawing;

namespace TweaksLauncher;

internal static class Logger
{
    private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "latest.log");
    private static readonly StreamWriter? logStream;

    static Logger()
    {
        try
        {
            logStream = File.CreateText(logFilePath);
        }
        catch
        {
            Log("WARNING! Cannot access the log file because another program is already using it. Any future console logs won't be logged to a file.", Color.Yellow);
        }
    }

    public static void Log(string? message, byte baseColorR, byte baseColorG, byte baseColorB, string? moduleName, byte moduleColorR, byte moduleColorG, byte moduleColorB)
    {
        Log(message, Color.FromArgb(baseColorR, baseColorG, baseColorB), moduleName, Color.FromArgb(moduleColorR, moduleColorG, moduleColorB));
    }

    public static void Log(string? message, Color baseColor = default, string? moduleName = null, Color moduleColor = default)
    {
        if (message == null && moduleName == null)
        {
            if (logStream != null)
            {
                logStream.WriteLine();
                logStream.Flush();
            }

            Console.WriteLine();

            return;
        }

        var time = DateTime.Now.ToString("HH:mm:ss.fff");

        message ??= "null";

        if (logStream != null)
        {
            var fileLog = $"[{time}]";
            if (moduleName != null)
                fileLog += $"[{moduleName}]";

            fileLog += ' ' + message;

            logStream.WriteLine(fileLog);
            logStream.Flush();
        }

        if (message != string.Empty)
        {
            if (baseColor.R == 0 && baseColor.G == 0 && baseColor.B == 0)
                baseColor = Color.LightCyan;

            message = message.Pastel(baseColor);
        }

        var consoleLog = $"[{time.Pastel(Color.DarkGray)}]";
        if (moduleName != null)
        {
            if (moduleColor.R == 0 && moduleColor.G == 0 && moduleColor.B == 0)
                moduleColor = Color.Magenta;

            consoleLog += $"[{moduleName.Pastel(moduleColor)}]";
        }

        consoleLog += ' ' + message;

        Console.WriteLine(consoleLog);
    }

    public static void LogProcess(Process process)
    {
        process.OutputDataReceived += OnProcessLog;
        process.ErrorDataReceived += OnProcessLog;
    }

    private static void OnProcessLog(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Data))
            return;

        Log(e.Data, moduleName: (sender as Process)?.ProcessName);
    }
}