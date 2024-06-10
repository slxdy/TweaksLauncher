using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;


#if IL2CPP
using TweaksLauncher.Il2Cpp;
using Il2CppInterop.HarmonySupport;
using Il2CppInterop.Runtime.Startup;
#endif

#if MONO
using System.Runtime.InteropServices;
#endif

namespace TweaksLauncher;

public unsafe static class ModHandler
{
#if !MONO
    private static Action<string?, byte, byte, byte, string?, byte, byte, byte> logFunc = null!;
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
        , Action<string?, byte, byte, byte, string?, byte, byte, byte> logFunc
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
            Log($"Could not find the Unity version.", LogColor.Red);
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

        LoadModsFromPath(GlobalModsDirectory);
        LoadModsFromPath(ModsDirectory);

        harmony = new Harmony("TweaksLauncher.ModHandler");
        WatchSceneChange();
    }

    // Need a separate method to prevent Start() from trying to resolve SceneManager early
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void WatchSceneChange()
    {
        harmony.Patch(typeof(SceneManager).GetMethod("Internal_ActiveSceneChanged", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public), prefix: new(OnInternalActiveSceneChanged));
    }

    internal static void Log(string? message, LogColor baseColor = default, string? moduleName = null, LogColor moduleColor = default)
    {
#if MONO
        LogInternal(message, baseColor.R, baseColor.G, baseColor.B, moduleName, moduleColor.R, moduleColor.G, moduleColor.B);
#else
        logFunc(message, baseColor.R, baseColor.G, baseColor.B, moduleName, moduleColor.R, moduleColor.G, moduleColor.B);
#endif
    }

#if MONO
    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern void LogInternal(string? message, byte baseColorR, byte baseColorG, byte baseColorB, string? moduleName, byte moduleColorR, byte moduleColorG, byte moduleColorB);
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
            Log($"Could not find mod at: {path}", LogColor.Red);
            return null;
        }

        path = Path.GetFullPath(path);

        if (loadedMods.Exists(x => path.Equals(x.ModPath, StringComparison.OrdinalIgnoreCase)))
        {
            Log($"Could not load mod from: {path}", LogColor.Red);
            Log($"This mod is already loaded.", LogColor.Red);
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
                Log($"Could not load mod from: {path}", LogColor.Red);
                Log($"Mod's assembly does not have a name.", LogColor.Red);
                return null;
            }
        }
        catch
        {
            Log($"Could not load mod from: {path}", LogColor.Red);
            Log($"Invalid .net assembly.", LogColor.Red);
            return null;
        }

        Type[] types;

        try
        {
            types = modAssembly.GetTypes();
        }
        catch
        {
            Log($"Could not load mod from: {path}", LogColor.Red);
            Log($"Something went wrong while reading the assembly. The assembly might be targetting the wrong .net runtime version.", LogColor.Red);
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
            Log($"Could not load mod from: {path}", LogColor.Red);
            Log($"Mod does not implement any IMod interfaces.", LogColor.Red);
            return null;
        }

        var mod = new LoadedMod(name, path, modAssembly, modInterfaces.AsReadOnly());
        loadedMods.Add(mod);

        Log($"Mod loaded: {mod}", LogColor.LightGreen);

        if (modsInited)
            mod.Init();

        return mod;
    }

    internal static void HandleLog(object? message, LogColor baseColor, Assembly modAssembly)
    {
        var mod = loadedMods.Find(x => x.ModAssembly == modAssembly);
        if (mod == null)
            return;

        Log(message?.ToString(), baseColor, mod.Name, LogColor.LightGreen);
    }
}
