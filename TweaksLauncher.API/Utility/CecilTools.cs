using Mono.Cecil;

namespace TweaksLauncher.Utility;

internal static class CecilTools
{
    public class RegistryAssemblyResolver : DefaultAssemblyResolver
    {
        public void Register(AssemblyDefinition assembly)
        {
            RegisterAssembly(assembly);
        }
    }
}
