using EasyNetLog;
using System.Diagnostics;

namespace Il2CppLauncher;

internal class ModuleLogger(string moduleName, string? moduleColor = null)
{
    private static EasyNetLogger logger = new(x => $"[<color=gray>{DateTime.Now:HH:mm:ss.fff}</color>]{x}", true);

    public string ModuleName { get; private set; } = moduleName;

    public string ModuleColor { get; private set; } = moduleColor ?? "magenta";

    public void Log(string? message, string? baseColor = null)
    {
        message ??= "null";
        baseColor ??= "#66c0f4";

        logger.Log($"[<color={ModuleColor}>{ModuleName}</color>] <color={baseColor}>{message}</color>");
    }

    public void Log()
    {
        logger.Log(string.Empty);
    }

    public void Log(object? obj, string? baseColor = null)
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

        var msg = sender is Process process ? $"[<color=yellow>{process.ProcessName}</color>] {e.Data}" : e.Data;
        Log(msg);
    }
}
