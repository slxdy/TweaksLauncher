using HarmonyLib;

namespace TweaksLauncher;

public class ModInstance
{
    public Harmony HarmonyInstance { get; private set; }

    internal ModInstance(Harmony harmony)
    {
        HarmonyInstance = harmony;
    }
}
