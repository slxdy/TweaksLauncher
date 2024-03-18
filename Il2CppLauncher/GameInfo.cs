using System.Diagnostics;

namespace Il2CppLauncher;

public class GameInfo
{
    public required string GameDirectory { get; init; }
    public required string GameDataDirectory { get; init; }
    public required string GameExePath { get; init; }
    public required string GameAssemblyPath { get; init; }
    public required string LauncherGameDirectory { get; init; }
    public required Version UnityVersion { get; init; }

    internal static GameInfo? Read(string gameDirectory)
    {
        if (!Directory.Exists(gameDirectory))
            return null;

        gameDirectory = Path.GetFullPath(gameDirectory);

        var dataDir = Directory.EnumerateDirectories(gameDirectory, "*_Data").FirstOrDefault();

        if (dataDir == null)
            return null;

        var gameName = dataDir[..^5];
        var exe = gameName + ".exe";

        var assembly = Path.Combine(gameDirectory, "GameAssembly.dll");
        if (!File.Exists(assembly))
            return null;

        assembly = Path.GetFullPath(assembly);

        var unityPlayer = Path.Combine(gameDirectory, "UnityPlayer.dll");
        if (!File.Exists(unityPlayer))
            return null;

        unityPlayer = Path.GetFullPath(unityPlayer);

        if (!Version.TryParse(FileVersionInfo.GetVersionInfo(unityPlayer).FileVersion, out var unityVersion))
            return null;

        var launcherDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, gameName);
        Directory.CreateDirectory(launcherDir);

        return new()
        {
            GameDirectory = gameDirectory,
            GameDataDirectory = dataDir,
            GameExePath = exe,
            GameAssemblyPath = assembly,
            LauncherGameDirectory = launcherDir,
            UnityVersion = unityVersion
        };
    }
}
