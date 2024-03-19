using Il2CppInterop.Runtime;
using Il2CppLauncher.Modding;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Il2CppLauncher;

internal static partial class UnityPlayer
{
    [NotNull] private static Dobby.Patch<Il2CppInitSig>? il2cppInit = null;
    [NotNull] private static Dobby.Patch<Il2CppRuntimeInvokeSig>? il2cppRuntimeInvoke = null;

    private static ModuleLogger logger = new("UnityPlayer");

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
        return UnityMain(Process.GetCurrentProcess().Id, 0, ref unityArgs, 1);
    }

    private unsafe static nint OnIl2CppRuntimeInvoke(nint method, nint obj, nint param, ref nint exc)
    {
        var name = Marshal.PtrToStringAnsi(IL2CPP.il2cpp_method_get_name(method));
        var type = IL2CPP.il2cpp_method_get_class(method);
        var typeName = Marshal.PtrToStringAnsi(IL2CPP.il2cpp_class_get_name(type));
        var typeNamespace = Marshal.PtrToStringAnsi(IL2CPP.il2cpp_class_get_namespace(type));

        logger.Log($"IL2CPP Invoking: {typeNamespace}.{typeName}::{name}");
        //if (name == "Awake")
        //{
        //    il2cppRuntimeInvoke.Dispose();

        //    ModLoader.InitMods();

        //    return IL2CPP.il2cpp_runtime_invoke(method, obj, (void**)param, ref exc);
        //}

        return il2cppRuntimeInvoke.Original(method, obj, param, ref exc);
    }

    private static nint OnIl2CppInit(nint a)
    {
        logger.Log("Il2Cpp Init");

        var result = il2cppInit.Original(a);

        ModLoader.Init();

        return result;
    }

    internal delegate nint Il2CppInitSig(nint a);
    internal delegate nint Il2CppRuntimeInvokeSig(nint method, nint obj, nint param, ref nint exc);
}
