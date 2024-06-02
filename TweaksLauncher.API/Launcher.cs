using MonoMod.RuntimeDetour;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace TweaksLauncher;

internal unsafe static class Launcher
{
    private static readonly ModuleLogger logger = new("TweaksLauncher");

    public static string ExePath { get; private set; } = Environment.ProcessPath!;

    [NotNull]
    internal static LauncherContext? Context { get; private set; }

    public static int Main(string[] args)
    {
        if (!ModuleLogger.LogFileInitialized)
        {
            logger.Log("WARNING! Cannot access the log file because another program is already using it. Any future logs won't be logged to a file.", Color.Yellow);
        }

        Console.Title = "TweaksLauncher";

        CrashHandler.Init();

        if (args.Length >= 1)
        {
            var gamePath = args[0];

            if (gamePath.Equals("createmod", StringComparison.OrdinalIgnoreCase))
                return DevTools.CreateMod() ? 0 : -1;

            if (InitContext(gamePath))
            {
                var gameArgs = new string[args.Length - 1];
                Array.Copy(args, 1, gameArgs, 0, gameArgs.Length);

                return StartGame(gameArgs);
            }
            else
            {
                logger.Log($"No valid Unity game found at: '{gamePath}'", Color.Red);
            }
        }

        logger.Log("To launch Steam games through the mod loader, right click on your game in the Steam library, ");
        logger.Log("go to Properties -> General -> Launch Options.");
        logger.Log("Set the launch options to:");
        logger.Log($"\"{ExePath}\" %command%", Color.Green);
        logger.Log();
        logger.Log($"To run a game manually, drag your game shortcut/executable file to the launcher's executable.");

        Console.ReadKey();

        return 0;
    }

    public static bool InitContext(string gamePath)
    {
        if (Context != null)
            return false;

        Context = LauncherContext.Read(gamePath);

        return Context != null;
    }

    private static int StartGame(string[] args)
    {
        Console.Title = Context.GameName;

        logger.Log($"Game Exe: '{Context.GameExePath}'");
        logger.Log($"Unity Version: '{Context.UnityVersion}'");

        ProxyGenerator.Generate();

        DevTools.BuildProjectsForCurrentGame();

        DetourContext.GetDefaultFactory();

        Directory.SetCurrentDirectory(Context.GameDirectory);
        ModuleSpoofer.Spoof(Context.GameExePath);

        return UnityPlayer.Start(args);
    }
}
