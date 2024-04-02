using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace TweaksLauncher;

internal unsafe static class ModuleSpoofer
{
    [NotNull] private static Dobby.Patch<LdrGetDllFullNameSig>? ldrGetDllFullName = null;

    private static nint fakeExeHandle;
    private static nint ourHandle;
    private static bool inited;

    public static void Spoof(string fakeExePath)
    {
        if (inited)
            return;

        inited = true;

        ourHandle = NativeLibrary.GetMainProgramHandle();
        fakeExeHandle = NativeLibrary.Load(fakeExePath);

        ldrGetDllFullName = Dobby.CreatePatch<LdrGetDllFullNameSig>("ntdll", "LdrGetDllFullName", OnLdrGetDllFullName);
    }

    private static nint OnLdrGetDllFullName(nint hModule, nint lpFilename)
    {
        if (hModule == 0 || hModule == ourHandle)
            return ldrGetDllFullName.Original(fakeExeHandle, lpFilename);

        return ldrGetDllFullName.Original(hModule, lpFilename);
    }

    internal delegate nint LdrGetDllFullNameSig(nint hModule, nint lpFilename);
}
