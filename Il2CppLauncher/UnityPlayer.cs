using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Il2CppLauncher;

internal static partial class UnityPlayer
{
    [NotNull] private static Il2CppInitSig? il2cppInit = null;
    private static ModuleLogger logger = new("UnityPlayer");

    [LibraryImport("UnityPlayer.dll")]
    private static partial int UnityMain(nint hInstance, nint hPrevInstance, [MarshalAs(UnmanagedType.LPWStr)] ref string lpCmdline, int nShowCmd);

    public static int Start(string[] args)
    {
        if (il2cppInit != null)
            return -1;

        il2cppInit = Dobby.Patch<Il2CppInitSig>("GameAssembly.dll", "il2cpp_init", OnIl2CppInit);

        PInvoke.SetStdHandle(Windows.Win32.System.Console.STD_HANDLE.STD_OUTPUT_HANDLE, (HANDLE)0);
        PInvoke.SetStdHandle(Windows.Win32.System.Console.STD_HANDLE.STD_ERROR_HANDLE, (HANDLE)0);

        var unityArgs = string.Join(' ', args.Select(x => x.Contains(' ') ? $"\"{x}\"" : x));
        return UnityMain(Process.GetCurrentProcess().Id, 0, ref unityArgs, 1);
    }

    private static nint OnIl2CppInit(nint a)
    {
        logger.Log("Il2Cpp Init");

        var result = il2cppInit(a);

        Program.InitializeModding();

        return result;
    }

    internal delegate nint Il2CppInitSig(nint a);
}
