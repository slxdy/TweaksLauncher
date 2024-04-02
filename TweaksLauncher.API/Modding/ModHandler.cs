using Il2CppInterop.HarmonySupport;
using Il2CppInterop.Runtime.Startup;
using System.Drawing;
using System.Reflection;
using TweaksLauncher.Modding.Il2CppInteropImpl;

namespace TweaksLauncher.Modding;

internal static class ModHandler
{
    private static readonly ModuleLogger logger = new("Mod Handler");
    private static bool inited;
    private static bool modsInited;
    private static readonly List<LoadedMod> loadedMods = [];

    public static string GlobalModsDirectory { get; private set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GlobalMods");

    public static void Init()
    {
        if (inited)
            return;

        inited = true;

        logger.Log("Initializing");

        ProxyResolver.Init();

        Il2CppInteropRuntime.Create(new()
        {
            UnityVersion = Launcher.Context.UnityVersion,
            DetourProvider = new DobbyDetourProvider()
        })
            .AddHarmonySupport()
            .Start();

        UnityTools.Init();

        Directory.CreateDirectory(GlobalModsDirectory);
        LoadModsFromPath(GlobalModsDirectory);
        LoadModsFromPath(Launcher.Context.ModsDirectory);
    }

    public static void FirstSceneLoaded()
    {
        foreach (var mod in loadedMods)
            mod.InitUnitySingletons();

        UnityEvents.InvokeFirstSceneLoad();
    }

    public static void InitMods()
    {
        if (!inited || modsInited)
            return;

        modsInited = true;

        foreach (var mod in loadedMods)
        {
            foreach (var iMod in mod.ModInterfaces)
            {
                try
                {
                    iMod.Initialize(mod.ModInstance);
                }
                catch (Exception ex)
                {
                    logger.Log($"Mod interface failed to initialize: '{iMod.InterfaceType.FullName}'", Color.Red);
                    logger.Log(ex, Color.Red);
                }
            }
        }
    }

    private static void LoadModsFromPath(string modsDir)
    {
        if (!Directory.Exists(modsDir))
            return;

        foreach (var dll in Directory.EnumerateFiles(modsDir, "*.dll"))
        {
            TryLoadMod(dll);
        }

        foreach (var modDir in Directory.EnumerateDirectories(modsDir))
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
            logger.Log($"Could not find mod at: {path}", Color.Red);
            return null;
        }

        path = Path.GetFullPath(path);

        if (loadedMods.Exists(x => path.Equals(x.ModPath, StringComparison.OrdinalIgnoreCase)))
        {
            logger.Log($"Could not load mod from: {path}", Color.Red);
            logger.Log($"This mod is already loaded.", Color.Red);
            return null;
        }

        Assembly modAssembly;
        try
        {
            modAssembly = Assembly.LoadFrom(path);
        }
        catch
        {
            logger.Log($"Could not load mod from: {path}", Color.Red);
            logger.Log($"Mod is not a .net assembly.", Color.Red);
            return null;
        }

        var name = modAssembly.GetName().Name;
        if (name == null)
        {
            logger.Log($"Could not load mod from: {path}", Color.Red);
            logger.Log($"Mod's assembly does not have a name.", Color.Red);
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
            logger.Log($"Could not load mod from: {path}", Color.Red);
            logger.Log($"Mod does not implement any IMod interfaces.", Color.Red);
            return null;
        }

        var mod = new LoadedMod(name, path, modAssembly, modInterfaces.AsReadOnly());
        loadedMods.Add(mod);

        logger.Log($"Mod loaded: {mod}", Color.LightGreen);

        if (modsInited)
            mod.Init();

        return mod;
    }

    internal static void HandleLog(object? message, Color baseColor, Assembly modAssembly)
    {
        var mod = loadedMods.Find(x => x.ModAssembly == modAssembly);
        if (mod == null)
            return;

        mod.Logger.Log(message, baseColor);
    }
}
