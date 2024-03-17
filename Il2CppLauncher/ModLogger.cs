using EasyNetLog;

namespace Il2CppLauncher;

public class ModLogger(string moduleName)
{
    private static EasyNetLogger logger = new(x => $"[<color=magenta>{DateTime.Now:HH:mm:ss.fff}</color>]{x}", true);

    public void Log(string? message, string? baseColor = null)
    {
        message ??= "null";
        baseColor ??= "#66c0f4";

        logger.Log($"[<color=green>{moduleName}</color>] <color={baseColor}>{message}</color>");
    }

    public void Log()
    {
        logger.Log(string.Empty);
    }

    public void Log(object? obj, string? baseColor = null)
    {
        Log(obj?.ToString(), baseColor);
    }
}
