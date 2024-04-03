using System.Diagnostics;
using TweaksLauncher.Utility;

namespace TweaksLauncher;

internal class LauncherContext
{
    public required string GameName { get; init; }
    public required string GameDirectory { get; init; }
    public required string GameDataDirectory { get; init; }
    public required string GameExePath { get; init; }
    public required string GameAssemblyPath { get; init; }
    public required string GlobalMetadataPath { get; init; }
    public required string ProxiesDirectory { get; init; }
    public required string ModsDirectory { get; init; }
    public required string LauncherGameDirectory { get; init; }
    public required Version UnityVersion { get; init; }

    internal static LauncherContext? Read(string gameDirectory)
    {
        if (File.Exists(gameDirectory))
        {
            var steamPath = SteamTools.GetPathFromShortcut(gameDirectory);
            if (steamPath != null)
            {
                gameDirectory = steamPath;
            }
            else
            {
                var dir = Path.GetDirectoryName(gameDirectory);
                if (dir == null)
                    return null;

                gameDirectory = dir;
            }
        }
        else if (!Directory.Exists(gameDirectory))
            return null;


        gameDirectory = Path.GetFullPath(gameDirectory);

        var dataDir = Directory.EnumerateDirectories(gameDirectory, "*_Data").FirstOrDefault();
        if (dataDir == null)
            return null;

        var globalMetadata = Path.Combine(dataDir, "il2cpp_data", "Metadata", "global-metadata.dat");
        if (!File.Exists(globalMetadata))
            return null;

        var gameName = dataDir[..^5];
        var exe = gameName + ".exe";
        gameName = Path.GetFileName(gameName);

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

        var launcherDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Games", gameName);

        var modsDir = Path.Combine(launcherDir, "Mods");
        Directory.CreateDirectory(modsDir);

        return new()
        {
            GameName = gameName,
            GameDirectory = gameDirectory,
            GameDataDirectory = dataDir,
            GameExePath = exe,
            GameAssemblyPath = assembly,
            GlobalMetadataPath = globalMetadata,
            LauncherGameDirectory = launcherDir,
            ProxiesDirectory = Path.Combine(launcherDir, "Proxies"),
            ModsDirectory = modsDir,
            UnityVersion = unityVersion
        };
    }
}
