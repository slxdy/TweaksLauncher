using HarmonyLib;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;


#if IL2CPP
using TweaksLauncher.Il2Cpp;
using Il2CppInterop.HarmonySupport;
using Il2CppInterop.Runtime.Startup;
#endif

namespace TweaksLauncher;

public unsafe static class ModHandler
{
    private static Action<string?, Color, string?, Color> logFunc = null!;
    private static bool initSceneLoaded;
    private static Harmony harmony = null!;
    private static bool modsInited;
    private static readonly List<LoadedMod> loadedMods = [];

    public static string BaseDirectory { get; private set; } = null!;
    public static string GameDirectory { get; private set; } = null!;
    public static string GameName { get; private set; } = null!;
    public static string LauncherGameDirectory { get; private set; } = null!;
    public static string ModsDirectory { get; private set; } = null!;
    public static string GlobalModsDirectory { get; private set; } = null!;
    public static Version UnityVersion { get; private set; } = null!;

#if IL2CPP
    public static string ProxiesDirectory { get; private set; } = null!;
#endif

    internal static void Start(Action<string?, Color, string?, Color> logFunc, string baseDir, string gameName, string gameDir)
    {
        ModHandler.logFunc = logFunc;

        Log("Initializing Mod Handler");

        var unityPlayerPath = Path.Combine(gameDir, "UnityPlayer.dll");
        if (!Version.TryParse(FileVersionInfo.GetVersionInfo(unityPlayerPath).FileVersion, out var unityVersion))
        {
            Log($"Could not find the Unity version.", Color.Red);
            return;
        }

        UnityVersion = unityVersion;
        BaseDirectory = baseDir;
        GameName = gameName;
        GameDirectory = gameDir;

        GlobalModsDirectory = Path.Combine(baseDir, "GlobalMods");
        LauncherGameDirectory = Path.Combine(baseDir, "Games", gameName);
        ModsDirectory = Path.Combine(LauncherGameDirectory, "Mods");

#if IL2CPP
        ProxiesDirectory = Path.Combine(LauncherGameDirectory, "Proxies");

        foreach (var proxy in Directory.EnumerateFiles(ProxiesDirectory, "*.dll"))
        {
            Assembly.LoadFrom(proxy);
        }

        Il2CppInteropRuntime.Create(new()
        {
            UnityVersion = unityVersion,
            DetourProvider = new DobbyDetourProvider()
        })
            .AddHarmonySupport()
            .Start();
#endif

        UnityTools.Init();

        LoadModsFromPath(GlobalModsDirectory);
        LoadModsFromPath(ModsDirectory);

        harmony = new Harmony("TweaksLauncher.ModHandler");
        harmony.Patch(UnityTools.Internal_ActiveSceneChanged, prefix: new(OnInternalActiveSceneChanged));
    }

    internal static void Log(string? message, Color baseColor = default, string? moduleName = null, Color moduleColor = default)
    {
        logFunc(message, baseColor, moduleName, moduleColor);
    }

    private static void OnInternalActiveSceneChanged()
    {
        if (!initSceneLoaded)
        {
            initSceneLoaded = true;
            InitMods();
        }
        else
        {
            harmony.UnpatchSelf();
            FirstSceneLoaded();
        }
    }

    private static void InitMods()
    {
        if (modsInited)
            return;

        modsInited = true;

        foreach (var mod in loadedMods)
        {
            foreach (var iMod in mod.ModInterfaces)
            {
                try
                {
                    iMod.Initialize(mod);
                }
                catch (Exception ex)
                {
                    Log($"Mod interface failed to initialize: '{iMod.InterfaceType.FullName}'", Color.Red);
                    Log(ex.ToString(), Color.Red);
                }
            }
        }
    }

    private static void FirstSceneLoaded()
    {
        foreach (var mod in loadedMods)
            mod.InitUnitySingletons();

        UnityEvents.InvokeFirstSceneLoad();
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

            if (!File.Exists(modPath))
                continue;

            TryLoadMod(modPath);
        }
    }

    public static LoadedMod? TryLoadMod(string path)
    {
        if (!File.Exists(path))
        {
            Log($"Could not find mod at: {path}", Color.Red);
            return null;
        }

        path = Path.GetFullPath(path);

        if (loadedMods.Exists(x => path.Equals(x.ModPath, StringComparison.OrdinalIgnoreCase)))
        {
            Log($"Could not load mod from: {path}", Color.Red);
            Log($"This mod is already loaded.", Color.Red);
            return null;
        }

        Assembly modAssembly;
        try
        {
            modAssembly = Assembly.LoadFrom(path);
        }
        catch
        {
            Log($"Could not load mod from: {path}", Color.Red);
            Log($"Mod is not a .net assembly.", Color.Red);
            return null;
        }

        var name = modAssembly.GetName().Name;
        if (name == null)
        {
            Log($"Could not load mod from: {path}", Color.Red);
            Log($"Mod's assembly does not have a name.", Color.Red);
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
            Log($"Could not load mod from: {path}", Color.Red);
            Log($"Mod does not implement any IMod interfaces.", Color.Red);
            return null;
        }

        var mod = new LoadedMod(name, path, modAssembly, modInterfaces.AsReadOnly());
        loadedMods.Add(mod);

        Log($"Mod loaded: {mod}", Color.LightGreen);

        if (modsInited)
            mod.Init();

        return mod;
    }

    internal static void HandleLog(object? message, Color baseColor, Assembly modAssembly)
    {
        var mod = loadedMods.Find(x => x.ModAssembly == modAssembly);
        if (mod == null)
            return;

        Log(message?.ToString(), baseColor, mod.Name, Color.LightGreen);
    }
}
