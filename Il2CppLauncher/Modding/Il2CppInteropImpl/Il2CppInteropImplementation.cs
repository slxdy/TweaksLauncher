using Il2CppInterop.HarmonySupport;
using Il2CppInterop.Runtime.Startup;

namespace Il2CppLauncher.Modding.Il2CppInteropImpl;

internal class Il2CppInteropImplementation
{
    private static ModuleLogger logger = new("Il2CppInterop");

    internal static void InitRuntime()
    {
        Il2CppInteropRuntime.Create(new()
        {
            UnityVersion = Program.Context.UnityVersion,
            DetourProvider = new DobbyDetourProvider()
        })
            .AddHarmonySupport()
            .Start();

        logger.Log("Il2CppInterop Initialized");
    }
}
