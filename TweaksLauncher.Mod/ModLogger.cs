using System.Reflection;
using System.Runtime.CompilerServices;

namespace TweaksLauncher;

public static class ModLogger
{
    internal static OnLog? onLog;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Log(object? message, string? baseColor = null)
    {
        if (onLog == null)
            return;

        var modAssembly = Assembly.GetCallingAssembly();

        onLog(message, baseColor, modAssembly);
    }

    internal delegate void OnLog(object? message, string? baseColor, Assembly modAssembly);
}