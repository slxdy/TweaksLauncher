using Il2CppLauncher.Modding.Il2CppInteropImpl;
using System.Reflection;

namespace Il2CppLauncher.Modding;

internal static class ModLoader
{
    private static ModuleLogger logger = new("Mod Loader");
    private static bool inited;
    private static bool modsInited;
    private static List<LoadedMod> loadedMods = [];

    public static void Init()
    {
        if (inited)
            return;

        inited = true;

        logger.Log("Initializing");

        ProxyResolver.Init();
        Il2CppInteropImplementation.InitRuntime();

        ModLogger.onLog += HandleLog;

        LoadModsFromPath();
    }

    public static void InitMods()
    {
        if (!inited || modsInited)
            return;

        modsInited = true;

        ForEachIMod(x => x.Initialize());
    }

    private static void ForEachIMod(Action<ModInterface> action)
    {
        foreach (var mod in loadedMods)
        {
            foreach (var iMod in mod.ModInterfaces)
            {
                action(iMod);
            }
        }
    }

    private static void LoadModsFromPath()
    {
        if (!Directory.Exists(Program.Context.ModsDirectory))
            return;

        foreach (var dll in Directory.EnumerateFiles(Program.Context.ModsDirectory, "*.dll"))
        {
            TryLoadMod(dll);
        }

        foreach (var modDir in Directory.EnumerateDirectories(Program.Context.ModsDirectory))
        {
            var dirName = Path.GetFileName(modDir);
            var modPath = Path.Combine(modDir, dirName + ".dll");
            TryLoadMod(modPath);
        }
    }

    public static LoadedMod? TryLoadMod(string path)
    {
        if (!File.Exists(path))
        {
            logger.Log($"Could not find mod at: {path}", "red");
            return null;
        }

        path = Path.GetFullPath(path);

        if (loadedMods.Exists(x => path.Equals(x.ModPath, StringComparison.OrdinalIgnoreCase)))
        {
            logger.Log($"Could not load mod from: {path}", "red");
            logger.Log($"This mod is already loaded.", "red");
            return null;
        }

        Assembly modAssembly;
        try
        {
            modAssembly = Assembly.LoadFrom(path);
        }
        catch
        {
            logger.Log($"Could not load mod from: {path}", "red");
            logger.Log($"Mod is not a .net assembly.", "red");
            return null;
        }

        var name = modAssembly.GetName().Name;
        if (name == null)
        {
            logger.Log($"Could not load mod from: {path}", "red");
            logger.Log($"Mod's assembly does not have a name.", "red");
            return null;
        }

        var modInterfaces = new List<ModInterface>();
        foreach (var type in modAssembly.GetTypes())
        {
            var mi = ModInterface.GetFromType(type);
            if (mi == null)
                continue;

            modInterfaces.Add(mi);
        }

        if (modInterfaces.Count == 0)
        {
            logger.Log($"Could not load mod from: {path}", "red");
            logger.Log($"Mod does not implement any IMod interfaces.", "red");
            return null;
        }

        var modLogger = new ModuleLogger(name, "green");

        var mod = new LoadedMod()
        {
            Name = name,
            Logger = modLogger,
            ModAssembly = modAssembly,
            ModPath = path,
            ModInterfaces = modInterfaces.AsReadOnly()
        };
        loadedMods.Add(mod);

        logger.Log($"Mod loaded: <color=green>{mod}</color>");

        if (modsInited)
            foreach (var iMod in mod.ModInterfaces)
                iMod.Initialize();

        return mod;
    }

    private static void HandleLog(object? message, string? baseColor, Assembly modAssembly)
    {
        var mod = loadedMods.Find(x => x.ModAssembly == modAssembly);
        if (mod == null)
            return;

        mod.Logger.Log(message, baseColor);
    }
}
