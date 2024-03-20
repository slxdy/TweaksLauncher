using Il2CppLauncher.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Il2CppLauncher;

internal static class DevTools
{
    private const string projectConfigFileName = "Il2CppLauncher.ModConfig.json";

    private static ModuleLogger logger = new("Dev Tools");
    private static string templateDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ModTemplate");
    private static string devDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dev");
    private static string csprojTemplate = Path.Combine(templateDir, "Project.csproj");
    private static string classTemplate = Path.Combine(templateDir, "Main.cs");

    public static List<ModProject> GetProjectsForGame(string gameName)
    {
        var result = new List<ModProject>();

        if (!Directory.Exists(devDir))
            return result;

        foreach (var dir in Directory.EnumerateDirectories(devDir))
        {
            var mod = ModProject.TryGet(dir);
            if (mod == null || !gameName.Equals(mod.Config.GameName, StringComparison.OrdinalIgnoreCase))
                continue;

            result.Add(mod);
        }

        return result;
    }

    public static void BuildProjectsForCurrentGame()
    {
        var mods = GetProjectsForGame(Program.Context.GameName);

        foreach (var mod in mods)
        {
            var outputDir = Path.Combine(Program.Context.ModsDirectory, mod.Name);
            mod.TryBuild(outputDir);
        }

        Dotnet("build-server", "shutdown");
    }

    public static bool CreateMod()
    {
        logger.Log("Tool for creating Il2CppLauncher mods.");
        logger.Log("Enter the path to the game you wish to mod:");
        var path = Console.ReadLine();
        if (path == null)
            return false;

        path = path.Trim(' ', '"');

        if (!Program.InitContext(path))
        {
            logger.Log("There is no valid Unity game located at the given path.", "red");
            return false;
        }

        if (!ProxyGenerator.Generate())
        {
            logger.Log("Could not create a mod project because proxy generation failed.", "red");
            return false;
        }

        string? name = null;
        while (true)
        {
            logger.Log("Enter the name of your project. The name cannot contain spaces or invalid path characters:");
            var possibleName = Console.ReadLine();
            if (possibleName == null)
                return false;

            var invalidChars = Path.GetInvalidFileNameChars();
            if (possibleName.Any(x => x == ' ' || invalidChars.Contains(x)))
            {
                logger.Log("The name contains illegal characters. Please try again.", "red");
                continue;
            }

            name = possibleName;
            break;
        }

        var solutionDir = Path.Combine(devDir, name);
        var solutionPath = Path.Combine(solutionDir, name + ".sln");

        if (Directory.Exists(solutionDir) && Directory.EnumerateFiles(solutionDir).Any())
        {
            logger.Log("A project with the same name already exists. Please remove it first before Retrying.", "red");
            return false;
        }

        var projPath = Path.Combine(solutionDir, name);
        Directory.CreateDirectory(projPath);

        var csprojReferences = new StringBuilder();

        foreach (var asm in Directory.EnumerateFiles(Program.Context.ProxiesDirectory, "*.dll", SearchOption.AllDirectories))
        {
            var asmName = Path.GetFileNameWithoutExtension(asm);
            if (asmName.StartsWith('_'))
                continue;

            var relPath = Path.GetRelativePath(projPath, asm);

            csprojReferences.AppendLine($"        <Reference Include=\"{asmName}\" HintPath=\"{relPath}\" Private=\"False\" />");
        }

        var csprojPath = Path.Combine(projPath, name + ".csproj");
        var classPath = Path.Combine(projPath, "Main.cs");

        var csprojContent = File.ReadAllText(csprojTemplate).Replace("        <!--proxyReferences-->", csprojReferences.ToString());
        var classContent = File.ReadAllText(classTemplate).Replace("//namespace", $"namespace {name};");

        File.WriteAllText(csprojPath, csprojContent);
        File.WriteAllText(classPath, classContent);

        if (!Dotnet("new", "sln", "-o", solutionDir) || !Dotnet("sln", solutionPath, "add", csprojPath))
        {
            Directory.Delete(solutionDir, true);
            return false;
        }

        logger.Log();

        var configPath = Path.Combine(solutionDir, projectConfigFileName);

        var config = new ModConfig()
        {
            GameName = Program.Context.GameName,
            DefaultBuildConfig = "Debug",
            ModCsprojPath = Path.GetRelativePath(solutionDir, csprojPath)
        };

        var jsonOptions = new JsonSerializerOptions() { WriteIndented = true };
        File.WriteAllText(configPath, JsonSerializer.Serialize(config, jsonOptions));

        logger.Log($"Project '{name}' has been created.", "green");
        logger.Log($"You can find it at '{solutionDir}'");

        Process.Start("explorer", ["/select,", solutionPath]);

        return true;
    }

    private static bool Dotnet(params string[] arguments)
    {
        using var proc = new Process();
        proc.StartInfo.FileName = "dotnet";
        foreach (var arg in arguments)
            proc.StartInfo.ArgumentList.Add(arg);

        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.RedirectStandardError = true;

        logger.LogProcess(proc);

        proc.Start();
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();
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
            var buildConfig = Config.DefaultBuildConfig ?? "Debug";

            if (!Dotnet("build", CsprojPath, "-c", buildConfig, "-o", outputDir))
            {
                logger.Log($"Failed to build project at '{CsprojPath}'. Ignoring.", "yellow");
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
                logger.Log($"{projectConfigFileName} from project '{directory}' is invalid.", "yellow");
                return null;
            }

            var csprojPath = Path.Combine(directory, config.ModCsprojPath);

            return new(directory, csprojPath, config);
        }
    }
}
