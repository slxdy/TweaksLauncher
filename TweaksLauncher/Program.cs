using MonoMod.RuntimeDetour;
using System.Diagnostics.CodeAnalysis;

namespace TweaksLauncher;

internal unsafe static class Program
{
    private static readonly ModuleLogger logger = new("TweaksLauncher");

    public static string ExePath { get; private set; } = Environment.ProcessPath!;

    [NotNull]
    internal static LauncherContext? Context { get; private set; }

    private static int Main(string[] args)
    {
        Console.Title = "TweaksLauncher";

        CrashHandler.Init();

        if (args.Length >= 1)
        {
            var gamePath = args[0];

            if (gamePath.Equals("createmod", StringComparison.OrdinalIgnoreCase))
                return DevTools.CreateMod() ? 0 : -1;

            if (File.Exists(gamePath))
            {
                gamePath = Path.GetDirectoryName(gamePath);
            }

            if (Directory.Exists(gamePath))
            {
                if (InitContext(gamePath))
                {
                    var gameArgs = new string[args.Length - 1];
                    Array.Copy(args, 1, gameArgs, 0, gameArgs.Length);

                    return StartGame(gameArgs);
                }
                else
                {
                    logger.Log($"No valid Unity game found at: '{gamePath}'", "red");
                }
            }
            else
            {
                logger.Log($"Could not find the game directory at: '{gamePath}'", "red");
            }
        }

        logger.Log("To launch Steam games through the mod loader, right click on your game in the Steam library, ");
        logger.Log("go to <color=green>Properties -> General -> Launch Options</color>.");
        logger.Log("Set the launch options to:");
        logger.Log($"\"{ExePath}\" %command%", "green");
        logger.Log();
        logger.Log($"To run a game manually, run in the console:");
        logger.Log($"\"{ExePath}\" \"Path/To/Game/Directory\"", "green");

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

        // Gotta do this before hiding modules.
        DetourContext.GetDefaultFactory();

        Directory.SetCurrentDirectory(Context.GameDirectory);
        ModuleSpoofer.Spoof(Context.GameExePath);

        return UnityPlayer.Start(args);
    }
}
