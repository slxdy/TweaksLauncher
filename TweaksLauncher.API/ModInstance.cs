using HarmonyLib;

namespace TweaksLauncher;

/// <summary>
/// Stores tools created for mods.
/// </summary>
public class ModInstance
{
    /// <summary>
    /// Harmony instance created for the mod.
    /// </summary>
    public Harmony HarmonyInstance { get; private set; }

    internal ModInstance(Harmony harmony)
    {
        HarmonyInstance = harmony;
    }
}
