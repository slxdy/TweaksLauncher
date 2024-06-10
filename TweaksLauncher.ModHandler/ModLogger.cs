using System.Reflection;
using System.Runtime.CompilerServices;

namespace TweaksLauncher;

/// <summary>
/// A global logger for mods.
/// </summary>
public static class ModLogger
{
    /// <summary>
    /// Prints a log to the console.
    /// </summary>
    /// <param name="message">The message to print. If the message is an object, <see cref="object.ToString"/> will be used.</param>
    /// <param name="baseColor">Color of the message.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Log(object? message, LogColor baseColor = default)
    {
        var modAssembly = Assembly.GetCallingAssembly();

        ModHandler.HandleLog(message, baseColor, modAssembly);
    }
}