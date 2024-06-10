using HarmonyLib;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using UnityEngine;

#if IL2CPP
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime;
#endif

namespace TweaksLauncher;

public class LoadedMod
{
    /// <summary>
    /// Name of the mod.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Path to the assembly of the mod.
    /// </summary>
    public string ModPath { get; private set; }

    /// <summary>
    /// The loaded .net assembly of the mod.
    /// </summary>
    public Assembly ModAssembly { get; private set; }

    /// <summary>
    /// Harmony instance created for the mod.
    /// </summary>
    public Harmony Harmony { get; private set; }

    internal ReadOnlyCollection<ModInterface> ModInterfaces { get; private set; }

    internal LoadedMod(string name, string modPath, Assembly modAssembly, ReadOnlyCollection<ModInterface> modInterfaces)
    {
        Name = name;
        ModPath = modPath;
        ModAssembly = modAssembly;

#if IL2CPP
        // Automatically register Il2Cpp types
        foreach (var type in modAssembly.GetTypes())
        {
            if (!type.IsGenericType && !type.IsGenericTypeDefinition && type.IsAssignableTo(typeof(Il2CppObjectBase)))
            {
                ClassInjector.RegisterTypeInIl2Cpp(type);
            }
        }
#endif

        // Automatically register all Harmony patches
        Harmony = Harmony.CreateAndPatchAll(modAssembly, modAssembly.FullName);

        ModInterfaces = modInterfaces;
    }

    public void Init()
    {
        foreach (var iMod in ModInterfaces)
        {
            try
            {
                iMod.Initialize(this);
            }
            catch (MissingMethodException)
            {
                ModHandler.Log($"Mod interface doesn't implement an 'Initialize' method: '{iMod.InterfaceType.FullName}'", LogColor.Red);
            }
            catch (Exception ex)
            {
                ModHandler.Log($"Mod interface failed to initialize: '{iMod.InterfaceType.FullName}'", LogColor.Red);
                ModHandler.Log(ex.ToString(), LogColor.Red);
            }
        }
    }

    public void InitUnitySingletons()
    {
        foreach (var type in ModAssembly.GetTypes())
        {
            if (type.GetCustomAttributes(typeof(UnitySingletonAttribute), false).Length != 0 && typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                var obj = new GameObject();
                UnityEngine.Object.DontDestroyOnLoad(obj);
#if IL2CPP
                obj.AddComponent(Il2CppType.From(type));
#else
                obj.AddComponent(type);
#endif
            }
        }
    }

    public override string ToString()
    {
        return $"[ {Name}, v{ModAssembly.GetName().Version ?? new Version()} ]";
    }
}
