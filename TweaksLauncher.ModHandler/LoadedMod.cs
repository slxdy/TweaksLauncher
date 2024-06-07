using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using System.Collections.ObjectModel;
using System.Reflection;

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

        // Automatically register Il2Cpp types
        foreach (var type in modAssembly.GetTypes())
        {
            if (!type.IsGenericType && !type.IsGenericTypeDefinition && type.IsAssignableTo(typeof(Il2CppObjectBase)))
            {
                ClassInjector.RegisterTypeInIl2Cpp(type);
            }
        }

        // Automatically register all Harmony patches
        Harmony = Harmony.CreateAndPatchAll(modAssembly, modAssembly.FullName);

        ModInterfaces = modInterfaces;
    }

    public void Init()
    {
        foreach (var iMod in ModInterfaces)
            iMod.Initialize(this);
    }

    public void InitUnitySingletons()
    {
        foreach (var type in ModAssembly.GetTypes())
        {
            if (type.GetCustomAttribute<UnitySingletonAttribute>() != null && type.IsAssignableTo(UnityTools.MonoBehaviour))
            {
                UnityTools.CreateComponentSingleton(type);
            }
        }
    }

    public override string ToString()
    {
        return $"[ {Name}, v{ModAssembly.GetName().Version ?? new Version()} ]";
    }
}
