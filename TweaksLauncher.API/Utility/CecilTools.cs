using Mono.Cecil;
using System.Reflection;

namespace TweaksLauncher.Utility;

internal static class CecilTools
{
    private static readonly MethodInfo registerAssemblyMet;

    static CecilTools()
    {
        registerAssemblyMet = typeof(DefaultAssemblyResolver).GetMethod("RegisterAssembly", BindingFlags.Instance | BindingFlags.NonPublic)!;
    }

    public static void RegisterAssembly(this DefaultAssemblyResolver resolver, AssemblyDefinition assembly)
    {
        registerAssemblyMet.Invoke(resolver, [assembly]);
    }
}
