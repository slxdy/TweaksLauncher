using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32.Foundation;

namespace TweaksLauncher;

internal unsafe static class ModuleSpoofer
{
    [NotNull] private static Dobby.Patch<LdrGetDllFullNameSig>? ldrGetDllFullName = null;

    private static UNICODE_STRING* fakeExeName;
    private static nint ourHandle;
    private static bool inited;

    public static void Spoof(string fakeExePath)
    {
        if (inited)
            return;

        inited = true;

        ourHandle = NativeLibrary.GetMainProgramHandle();

        fakeExePath = Path.GetFullPath(fakeExePath);
        var length = (ushort)Encoding.Unicode.GetByteCount(fakeExePath);

        fakeExeName = (UNICODE_STRING*)Marshal.AllocHGlobal(sizeof(UNICODE_STRING));
        *fakeExeName = new()
        {
            Length = length,
            MaximumLength = length,
            Buffer = (char*)Marshal.StringToHGlobalUni(fakeExePath)
        };

        ldrGetDllFullName = Dobby.CreatePatch<LdrGetDllFullNameSig>("ntdll", "LdrGetDllFullName", OnLdrGetDllFullName);
    }

    private static uint OnLdrGetDllFullName(nint hModule, UNICODE_STRING* lpFilename)
    {
        if (hModule == 0 || hModule == ourHandle)
        {
            Imports.RtlCopyUnicodeString(lpFilename, fakeExeName);


            if (lpFilename->MaximumLength < fakeExeName->Length + sizeof(char))
                return 0xC0000023; // STATUS_BUFFER_TOO_SMALL

            return 0;
        }

        return ldrGetDllFullName.Original(hModule, lpFilename);
    }

    internal delegate uint LdrGetDllFullNameSig(nint hModule, UNICODE_STRING* lpFilename);
}
