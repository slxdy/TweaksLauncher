using HarmonyLib;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using TweaksLauncher.Modding;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace TweaksLauncher;

internal static partial class UnityPlayer
{
    [NotNull] private static Dobby.Patch<Il2CppInitSig>? il2cppInit = null;

    private static bool initSceneLoaded;

    private static Harmony? harmony;

    private static readonly ModuleLogger logger = new("UnityPlayer");

    [LibraryImport("UnityPlayer.dll")]
    private static partial int UnityMain(nint hInstance, nint hPrevInstance, [MarshalAs(UnmanagedType.LPWStr)] ref string lpCmdline, int nShowCmd);

    public static int Start(string[] args)
    {
        if (il2cppInit != null)
            return -1;

        il2cppInit = Dobby.CreatePatch<Il2CppInitSig>("GameAssembly.dll", "il2cpp_init", OnIl2CppInit);

        PInvoke.SetStdHandle(Windows.Win32.System.Console.STD_HANDLE.STD_OUTPUT_HANDLE, (HANDLE)0);
        PInvoke.SetStdHandle(Windows.Win32.System.Console.STD_HANDLE.STD_ERROR_HANDLE, (HANDLE)0);

        var unityArgs = string.Join(' ', args.Select(x => x.Contains(' ') ? $"\"{x}\"" : x));
        return UnityMain(NativeLibrary.GetMainProgramHandle(), 0, ref unityArgs, 1);
    }

    private static nint OnIl2CppInit(nint domainName)
    {
        logger.Log("Creating Il2Cpp Domain");

        il2cppInit.Destroy();

        var result = il2cppInit.Original(domainName);

        ModHandler.Init();

        harmony = new Harmony("UnityPlayer Hooks");
        harmony.Patch(UnityTools.Internal_ActiveSceneChanged, prefix: new(OnInternalActiveSceneChanged));

        return result;
    }

    private static void OnInternalActiveSceneChanged()
    {
        if (harmony == null)
            return;

        if (!initSceneLoaded)
        {
            initSceneLoaded = true;
            ModHandler.InitMods();
        }
        else
        {
            harmony.UnpatchSelf();
            ModHandler.FirstSceneLoaded();
        }
    }

    internal delegate nint Il2CppInitSig(nint domainName);
    internal delegate nint Il2CppRuntimeInvokeSig(nint method, nint obj, nint param, nint exc);
}
