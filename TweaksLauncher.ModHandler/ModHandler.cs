using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;

#if IL2CPP
using TweaksLauncher.Il2Cpp;
using Il2CppInterop.HarmonySupport;
using Il2CppInterop.Runtime.Startup;
#endif

#if MONO
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
#endif

namespace TweaksLauncher;

public unsafe static class ModHandler
{
#if !MONO
    private static Action<string?, Color, string?, Color> logFunc = null!;
#endif

    private static bool initSceneLoaded;
    private static Harmony harmony = null!;
    private static bool modsInited;
    private static bool firstSceneLoaded;
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

    internal static void Start(string baseDir, string gameName, string gameDir
#if !MONO
        , Action<string?, Color, string?, Color> logFunc
#endif
        )
    {
#if !MONO
        ModHandler.logFunc = logFunc;
#endif

        Log("Initializing Mod Handler");

        var unityPlayerPath = Path.Combine(gameDir, "UnityPlayer.dll");

        try
        {
            UnityVersion = new Version(FileVersionInfo.GetVersionInfo(unityPlayerPath).FileVersion!);
        }
        catch
        {
            Log($"Could not find the Unity version.", Color.Red);
            return;
        }

        BaseDirectory = baseDir;
        GameName = gameName;
        GameDirectory = gameDir;

        GlobalModsDirectory = Path.Combine(baseDir, "GlobalMods");
        LauncherGameDirectory = Path.Combine(Path.Combine(baseDir, "Games"), gameName);
        ModsDirectory = Path.Combine(LauncherGameDirectory, "Mods");

#if IL2CPP
        ProxiesDirectory = Path.Combine(LauncherGameDirectory, "Proxies");

        foreach (var proxy in Directory.EnumerateFiles(ProxiesDirectory, "*.dll"))
        {
            Assembly.LoadFrom(proxy);
        }

        Il2CppInteropRuntime.Create(new()
        {
            UnityVersion = UnityVersion,
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
#if MONO
        LogInternal(message, baseColor.ToArgb(), moduleName, moduleColor.ToArgb());
#else
        logFunc(message, baseColor, moduleName, moduleColor);
#endif
    }

#if MONO
    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern void LogInternal(string? message, int baseColor, string? moduleName, int moduleColor);
#endif

    private static void OnInternalActiveSceneChanged()
    {
        if (firstSceneLoaded)
            return;

        if (!initSceneLoaded)
        {
            initSceneLoaded = true;
            InitMods();
        }
        else
        {
            firstSceneLoaded = true;
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
            mod.Init();
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

        foreach (var dll in Directory.GetFiles(modsDir, "*.dll"))
        {
            TryLoadMod(dll);
        }

        foreach (var modDir in Directory.GetDirectories(modsDir))
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
        string? name;
        try
        {
            modAssembly = Assembly.LoadFrom(path);
            name = modAssembly.GetName().Name;
            if (name == null)
            {
                Log($"Could not load mod from: {path}", Color.Red);
                Log($"Mod's assembly does not have a name.", Color.Red);
                return null;
            }
        }
        catch
        {
            Log($"Could not load mod from: {path}", Color.Red);
            Log($"Invalid .net assembly.", Color.Red);
            return null;
        }

        Type[] types;

        try
        {
            types = modAssembly.GetTypes();
        }
        catch
        {
            Log($"Could not load mod from: {path}", Color.Red);
            Log($"Something went wrong while reading the assembly. The assembly might be targetting the wrong .net runtime version.", Color.Red);
            return null;
        }

        var modInterfaces = new List<ModInterface>();
        foreach (var type in types)
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
