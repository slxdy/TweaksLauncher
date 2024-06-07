using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace TweaksLauncher;

internal static unsafe partial class GameManager
{
    [NotNull] private static Dobby.Patch<LdrGetDllFullNameSig>? ldrGetDllFullName = null;

    private static UNICODE_STRING* fakeExeName;
    private static readonly nint ourHandle = NativeLibrary.GetMainProgramHandle();

    [LibraryImport("UnityPlayer")]
    private static partial int UnityMain(nint hInstance, nint hPrevInstance, [MarshalAs(UnmanagedType.LPWStr)] ref string lpCmdline, int nShowCmd);

    [LibraryImport("ntdll")]
    private static partial void RtlCopyUnicodeString(Windows.Win32.Foundation.UNICODE_STRING* destination, Windows.Win32.Foundation.UNICODE_STRING* source);

    public static int Start()
    {
        DevTools.BuildProjectsForCurrentGame();

        var fakeExePath = Path.Combine(Program.gamePath, Program.gameName + ".exe");
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

        Directory.SetCurrentDirectory(Program.gamePath);

        PInvoke.SetStdHandle(Windows.Win32.System.Console.STD_HANDLE.STD_OUTPUT_HANDLE, (HANDLE)0);
        PInvoke.SetStdHandle(Windows.Win32.System.Console.STD_HANDLE.STD_ERROR_HANDLE, (HANDLE)0);

        var unityArgs = string.Join(' ', Environment.GetCommandLineArgs().Skip(2).Select(x => x.Contains(' ') ? $"\"{x}\"" : x));
        return UnityMain(NativeLibrary.GetMainProgramHandle(), 0, ref unityArgs, 1);
    }

    private static uint OnLdrGetDllFullName(nint hModule, UNICODE_STRING* lpFilename)
    {
        if (hModule == 0 || hModule == ourHandle)
        {
            RtlCopyUnicodeString(lpFilename, fakeExeName);

            if (lpFilename->MaximumLength < fakeExeName->Length + sizeof(char))
                return 0xC0000023; // STATUS_BUFFER_TOO_SMALL

            return 0;
        }

        return ldrGetDllFullName.Original(hModule, lpFilename);
    }

    internal delegate uint LdrGetDllFullNameSig(nint hModule, UNICODE_STRING* lpFilename);
}
