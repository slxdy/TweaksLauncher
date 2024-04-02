using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Reflection;

namespace TweaksLauncher.Modding;

internal class LoadedMod
{
    public string Name { get; private set; }
    public string ModPath { get; private set; }
    public ModuleLogger Logger { get; private set; }
    public Assembly ModAssembly { get; private set; }
    public ReadOnlyCollection<ModInterface> ModInterfaces { get; private set; }
    public ModInstance ModInstance { get; private set; }

    internal LoadedMod(string name, string modPath, Assembly modAssembly, ReadOnlyCollection<ModInterface> modInterfaces)
    {
        Name = name;
        ModPath = modPath;
        ModAssembly = modAssembly;

        Logger = new ModuleLogger(name, Color.Green);

        // Automatically register Il2Cpp types
        foreach (var type in modAssembly.GetTypes())
        {
            if (!type.IsGenericType && !type.IsGenericTypeDefinition && type.IsAssignableTo(typeof(Il2CppObjectBase)))
            {
                ClassInjector.RegisterTypeInIl2Cpp(type);
            }
        }

        // Automatically register all Harmony patches
        var har = Harmony.CreateAndPatchAll(modAssembly, modAssembly.FullName);

        ModInterfaces = modInterfaces;
        ModInstance = new(har);
    }

    public void Init()
    {
        foreach (var iMod in ModInterfaces)
            iMod.Initialize(ModInstance);
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
