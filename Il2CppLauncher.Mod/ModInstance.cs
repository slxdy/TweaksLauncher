using HarmonyLib;

namespace Il2CppLauncher;

public class ModInstance
{
    public Harmony HarmonyInstance { get; private set; }

    internal ModInstance(Harmony harmony)
    {
        HarmonyInstance = harmony;
    }
}
