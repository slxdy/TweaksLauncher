using System.Drawing;

namespace TweaksLauncher;

internal static class CrashHandler
{
    private static readonly ModuleLogger logger = new("Crash Handler");

    private static bool inited;

    public static void Init()
    {
        if (inited)
            return;

        inited = true;

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        logger.Log(e.ExceptionObject, Color.Red);

        if (e.IsTerminating)
            Console.ReadKey();
    }
}
