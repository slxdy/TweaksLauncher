using System.Diagnostics;
using System.Drawing;

namespace TweaksLauncher;

internal static class Program
{
    internal static readonly string baseDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", ".."));

    internal static string runtimePath = null!;
    internal static string gamePath = null!;
    internal static string gameDataDir = null!;
    internal static string gameName = null!;
    internal static string launcherGamePath = null!;
    internal static Version unityVersion = null!;

    private static int Main(string[] args)
    {
        Console.Title = "TweaksLauncher";

        Logger.Log($"TweaksLauncher v{typeof(Program).Assembly.GetName().Version}", Color.Magenta);
        Logger.Log(null);

        if (args.Length == 0)
        {
            Logger.Log("To launch Steam games through the mod loader, right click on your game in the Steam library, ");
            Logger.Log("go to Properties -> General -> Launch Options.");
            Logger.Log("Set the launch options to:");
            Logger.Log($"\"{Environment.ProcessPath}\" %command%", Color.Green);
            Logger.Log(null);
            Logger.Log($"To run a game manually, drag your game shortcut/executable file to the launcher's executable.");

            Console.ReadKey();
            return 1;
        }

        gamePath = args[0];

        if (File.Exists(gamePath))
            gamePath = Path.GetDirectoryName(gamePath)!;

        if (!Directory.Exists(gamePath))
        {
            Logger.Log($"The directory '{gamePath}' does not exist.", Color.Red);
            return 2;
        }

        gamePath = Path.GetFullPath(gamePath);

        Logger.Log(gamePath, moduleName: "Game Path", moduleColor: Color.Coral);

        var dataDirs = Directory.GetDirectories(gamePath, "*_Data");

        if (dataDirs.Length == 0)
        {
            Logger.Log($"No Unity game found.", Color.Red);
            return 3;
        }

        if (dataDirs.Length > 1)
        {
            Logger.Log($"Multiple Unity data directories found.", Color.Red);
            return 4;
        }

        gameDataDir = dataDirs[0];

        gameName = Path.GetFileName(gameDataDir)[..^5];

        Logger.Log(gameName, moduleName: "Game Name", moduleColor: Color.Coral);

        launcherGamePath = Path.Combine(baseDir, "Games", gameName);

        var unityPlayer = Path.Combine(gamePath, "UnityPlayer.dll");
        if (!File.Exists(unityPlayer))
        {
            Logger.Log($"Could not find UnityPlayer.dll", Color.Red);
            return 5;
        }

        if (!Version.TryParse(FileVersionInfo.GetVersionInfo(unityPlayer).FileVersion, out unityVersion!))
        {
            Logger.Log($"Could not find the Unity version.", Color.Red);
            return 6;
        }

        RuntimeType runtime;
        runtimePath = Path.Combine(gamePath, "GameAssembly.dll");
        if (File.Exists(runtimePath))
            runtime = RuntimeType.Il2Cpp;
        else
        {
            runtimePath = Path.Combine(gamePath, "MonoBleedingEdge/EmbedRuntime/mono-2.0-bdwgc.dll");
            if (File.Exists(runtimePath))
                runtime = RuntimeType.Mono;
            else
            {
                runtimePath = Path.Combine(gamePath, "Mono/EmbedRuntime/mono.dll");
                if (File.Exists(runtimePath))
                    runtime = RuntimeType.OldMono;
                else
                {
                    Logger.Log($"No valid runtime found.", Color.Red);
                    return 7;
                }
            }
        }

        Logger.Log(runtime.ToString(), moduleName: "Game Runtime", moduleColor: Color.Coral);
        Logger.Log(null);

        Directory.CreateDirectory(Path.Combine(launcherGamePath, "Mods"));
        Directory.CreateDirectory(Path.Combine(baseDir, "GlobalMods"));

        switch (runtime)
        {
            case RuntimeType.Il2Cpp:
                return Il2CppHandler.Start();

            case RuntimeType.Mono:
            case RuntimeType.OldMono:
                return MonoHandler.Start();

            default:
                Logger.Log($"Game runtime not supported.", Color.Red);
                return 8;
        }
    }


}
