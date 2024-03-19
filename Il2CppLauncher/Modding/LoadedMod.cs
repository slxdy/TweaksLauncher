using System.Collections.ObjectModel;
using System.Reflection;

namespace Il2CppLauncher.Modding;

internal class LoadedMod
{
    public required string Name { get; init; }
    public required string ModPath { get; init; }
    public required ModuleLogger Logger { get; init; }
    public required Assembly ModAssembly { get; init; }
    public required ReadOnlyCollection<ModInterface> ModInterfaces { get; init; }

    public override string ToString()
    {
        // TODO: Add mod version
        var relPath = Path.GetRelativePath(Program.Context.LauncherGameDirectory, ModPath);
        return $"[ {Name}, {relPath} ]";
    }
}
