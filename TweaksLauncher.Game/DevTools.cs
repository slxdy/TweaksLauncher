using System.Diagnostics;
using System.Drawing;
using System.Text.Json;

namespace TweaksLauncher;

internal static class DevTools
{
    private const string projectConfigFileName = "TweaksLauncher.ModConfig.json";

    private static readonly string devDir = Path.Combine(Program.baseDir, "Dev");

    public static List<ModProject> GetProjectsForGame()
    {
        var result = new List<ModProject>();

        Directory.CreateDirectory(devDir);

        foreach (var dir in Directory.EnumerateDirectories(devDir))
        {
            var mod = ModProject.TryGet(dir);
            if (mod == null || (!string.IsNullOrEmpty(mod.Config.GameName) && !Program.gameName.Equals(mod.Config.GameName, StringComparison.OrdinalIgnoreCase)))
                continue;

            result.Add(mod);
        }

        return result;
    }

    public static void BuildProjectsForCurrentGame()
    {
        var mods = GetProjectsForGame();

        foreach (var mod in mods)
        {
            var outputDir = string.IsNullOrEmpty(mod.Config.GameName) ? Path.Combine(Program.baseDir, "GlobalMods", mod.Name)
                : Path.Combine(Program.launcherGamePath, "Mods", mod.Name);

            mod.TryBuild(outputDir);
        }

        if (mods.Count != 0)
            Dotnet(true, "build-server", "shutdown");
    }

    private static bool Dotnet(bool silent, params string[] arguments)
    {
        using var proc = new Process();
        proc.StartInfo.FileName = "dotnet";
        foreach (var arg in arguments)
            proc.StartInfo.ArgumentList.Add(arg);

        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.RedirectStandardError = true;

        if (!silent)
        {
            Logger.LogProcess(proc);
        }

        proc.Start();

        if (!silent)
        {
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
        }

        proc.WaitForExit();
        return proc.ExitCode == 0;
    }

    internal class ModProject
    {
        public string Name { get; private set; }
        public string SolutionDirectory { get; private set; }
        public string CsprojPath { get; private set; }
        public ModConfig Config { get; private set; }

        private ModProject(string solutionDirectory, string csprojPath, ModConfig config)
        {
            Name = Path.GetFileName(solutionDirectory);
            SolutionDirectory = solutionDirectory;
            CsprojPath = csprojPath;
            Config = config;
        }

        public bool TryBuild(string outputDir)
        {
            Logger.Log($"Building project: '{CsprojPath}'", Color.Cyan);

            var buildConfig = Config.DefaultBuildConfig ?? "Debug";

            if (!Dotnet(false, "build", CsprojPath, "-c", buildConfig, $"-p:OutDir=\"{outputDir}\""))
            {
                Logger.Log($"Failed to build project at '{CsprojPath}'. Ignoring.", Color.Yellow);
                return false;
            }

            return true;
        }

        public static ModProject? TryGet(string directory)
        {
            if (File.Exists(directory))
            {
                var dir = Path.GetDirectoryName(directory);
                if (dir == null)
                    return null;

                directory = dir;
            }
            if (!Directory.Exists(directory))
                return null;

            directory = Path.GetFullPath(directory);

            var configPath = Path.Combine(directory, projectConfigFileName);
            if (!File.Exists(configPath))
                return null;

            ModConfig? config = null;
            try
            {
                config = JsonSerializer.Deserialize<ModConfig>(File.ReadAllText(configPath));
            }
            catch { }

            if (config == null || config.ModCsprojPath == null)
            {
                Logger.Log($"{projectConfigFileName} from project '{directory}' is invalid.", Color.Yellow);
                return null;
            }

            var csprojPath = Path.Combine(directory, config.ModCsprojPath);

            return new(directory, csprojPath, config);
        }
    }

    public class ModConfig
    {
        public string? GameName { get; set; }
        public string? DefaultBuildConfig { get; set; }
        public string? ModCsprojPath { get; set; }
    }
}
