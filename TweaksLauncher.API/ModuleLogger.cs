using Pastel;
using System.Diagnostics;
using System.Drawing;

namespace TweaksLauncher;

internal class ModuleLogger(string moduleName, Color moduleColor = default)
{
    private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "latest.log");
    private static StreamWriter? logStream;

    public static bool LogFileInitialized => logStream != null;

    public string ModuleName { get; private set; } = moduleName;

    public Color ModuleColor { get; private set; } = moduleColor == Color.Empty ? Color.Magenta : moduleColor;

    static ModuleLogger()
    {
        try
        {
            logStream = File.CreateText(logFilePath);
        }
        catch { }
    }

    public void Log(string? message, Color baseColor = default, string? subModule = null)
    {
        var time = DateTime.Now.ToString("HH:mm:ss.fff");

        message ??= "null";

        var fileLog = $"[{time}][{ModuleName}]";
        if (subModule != null)
            fileLog += $"[{subModule}]";

        fileLog += ' ' + message;

        if (message != string.Empty)
        {
            if (baseColor == default)
                baseColor = Color.LightCyan;
            message = message.Pastel(baseColor);
        }

        var consoleLog = $"[{time.Pastel(Color.DarkGray)}][{ModuleName.Pastel(ModuleColor)}]";
        if (subModule != null)
            consoleLog += $"[{subModule.Pastel(Color.Yellow)}]";

        consoleLog += ' ' + message;

        Write(consoleLog, fileLog);
    }

    private static void Write(string consoleLog, string fileLog)
    {
        Console.WriteLine(consoleLog);

        logStream?.WriteLine(fileLog);
        logStream?.Flush();
    }

    public void Log()
    {
        Log(string.Empty);
    }

    public void Log(object? obj, Color baseColor = default)
    {
        Log(obj?.ToString(), baseColor);
    }

    public void LogProcess(Process process)
    {
        process.OutputDataReceived += OnProcessLog;
        process.ErrorDataReceived += OnProcessLog;
    }

    private void OnProcessLog(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Data))
            return;

        Log(e.Data, subModule: (sender as Process)?.ProcessName);
    }
}
