using Il2CppInterop.Runtime;
using Il2CppLauncher.Modding;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Il2CppLauncher;

internal static partial class UnityPlayer
{
    [NotNull] private static Dobby.Patch<Il2CppInitSig>? il2cppInit = null;
    [NotNull] private static Dobby.Patch<Il2CppRuntimeInvokeSig>? il2cppRuntimeInvoke = null;

    private static nint activateSceneChangedPtr;

    private static readonly ModuleLogger logger = new("UnityPlayer");

    [LibraryImport("UnityPlayer.dll")]
    private static partial int UnityMain(nint hInstance, nint hPrevInstance, [MarshalAs(UnmanagedType.LPWStr)] ref string lpCmdline, int nShowCmd);

    public static int Start(string[] args)
    {
        if (il2cppInit != null)
            return -1;

        il2cppInit = new Dobby.Patch<Il2CppInitSig>("GameAssembly.dll", "il2cpp_init", OnIl2CppInit);
        il2cppRuntimeInvoke = new Dobby.Patch<Il2CppRuntimeInvokeSig>("GameAssembly.dll", "il2cpp_runtime_invoke", OnIl2CppRuntimeInvoke);

        PInvoke.SetStdHandle(Windows.Win32.System.Console.STD_HANDLE.STD_OUTPUT_HANDLE, (HANDLE)0);
        PInvoke.SetStdHandle(Windows.Win32.System.Console.STD_HANDLE.STD_ERROR_HANDLE, (HANDLE)0);

        var unityArgs = string.Join(' ', args.Select(x => x.Contains(' ') ? $"\"{x}\"" : x));
        return UnityMain(NativeLibrary.GetMainProgramHandle(), 0, ref unityArgs, 1);
    }

    private unsafe static nint OnIl2CppRuntimeInvoke(nint method, nint obj, nint param, ref nint exc)
    {
        if (method == activateSceneChangedPtr)
        {
            il2cppRuntimeInvoke.Dispose();
            ModLoader.InitMods();
        }

        return il2cppRuntimeInvoke.Original(method, obj, param, ref exc);
    }

    private static nint OnIl2CppInit(nint domainName)
    {
        logger.Log("Creating Il2Cpp domain");

        il2cppInit.Dispose();

        var result = il2cppInit.Original(domainName);

        var type = IL2CPP.GetIl2CppClass("UnityEngine.CoreModule.dll", "UnityEngine.SceneManagement", "SceneManager");
        if (type == default)
        {
            type = IL2CPP.GetIl2CppClass("UnityEngine.dll", "UnityEngine.SceneManagement", "SceneManager");
            if (type == default)
                throw new Exception("UnityEngine module bad format.");
        }

        activateSceneChangedPtr = IL2CPP.GetIl2CppMethod(type, false, "Internal_ActiveSceneChanged", "System.Void", "UnityEngine.SceneManagement.Scene", "UnityEngine.SceneManagement.Scene");
        if (activateSceneChangedPtr == 0)
            throw new Exception("UnityEngine module bad format. Method for init not found.");

        ModLoader.Init();

        return result;
    }

    internal delegate nint Il2CppInitSig(nint domainName);
    internal delegate nint Il2CppRuntimeInvokeSig(nint method, nint obj, nint param, ref nint exc);
}
