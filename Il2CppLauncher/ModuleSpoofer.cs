using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Diagnostics.ToolHelp;

namespace Il2CppLauncher;

internal unsafe static class ModuleSpoofer
{
    [NotNull] private static LdrGetDllFullNameSig? ldrGetDllFullName = null;
    [NotNull] private static K32EnumProcessModulesSig? k32EnumProcessModules = null;
    [NotNull] private static K32EnumProcessModulesExSig? k32EnumProcessModulesEx = null;
    [NotNull] private static Module32Sig? module32FirstW = null;
    [NotNull] private static Module32Sig? module32NextW = null;

    private static nint fakeExeHandle;
    private static int currentPID;
    private static nint ourHandle;
    [NotNull] private static string? dotnetDir = null;
    [NotNull] private static string? baseDir = null;
    private static bool inited;

    public static void Spoof(string fakeExePath)
    {
        if (inited)
            return;

        inited = true;

        ourHandle = NativeLibrary.GetMainProgramHandle();
        currentPID = Process.GetCurrentProcess().Id;
        fakeExeHandle = NativeLibrary.Load(fakeExePath);
        dotnetDir = Path.GetFullPath(GetModuleName(NativeLibrary.Load("coreclr")) + "/../../../..") + Path.DirectorySeparatorChar;
        baseDir = AppDomain.CurrentDomain.BaseDirectory;

        ldrGetDllFullName = Dobby.Patch<LdrGetDllFullNameSig>("ntdll", "LdrGetDllFullName", OnLdrGetDllFullName);
        k32EnumProcessModules = Dobby.Patch<K32EnumProcessModulesSig>("KernelBase", "K32EnumProcessModules", OnK32EnumProcessModules);
        k32EnumProcessModulesEx = Dobby.Patch<K32EnumProcessModulesExSig>("KernelBase", "K32EnumProcessModulesEx", OnK32EnumProcessModulesEx);
        module32FirstW = Dobby.Patch<Module32Sig>("Kernel32", "Module32FirstW", OnModule32FirstW);
        module32NextW = Dobby.Patch<Module32Sig>("Kernel32", "Module32NextW", OnModule32NextW);
    }
    private static bool OnModule32FirstW(nint hSnapshot, MODULEENTRY32W* lpme)
    {
        if (!module32FirstW(hSnapshot, lpme))
        {
            *lpme = default;
            return false;
        }

        if (lpme->th32ProcessID != currentPID)
            return true;

        if (lpme->hModule == ourHandle || IsModuleHidden(ConvertNativePath(&lpme->szExePath)))
            return OnModule32NextW(hSnapshot, lpme);

        return true;
    }

    private static bool OnModule32NextW(nint hSnapshot, MODULEENTRY32W* lpme)
    {
        if (!module32NextW(hSnapshot, lpme))
        {
            *lpme = default;
            return false;
        }

        if (lpme->th32ProcessID != currentPID)
            return true;

        if (lpme->hModule == ourHandle || IsModuleHidden(ConvertNativePath(&lpme->szExePath)))
            return OnModule32NextW(hSnapshot, lpme);

        return true;
    }

    private static string ConvertNativePath(__char_260* path)
    {
        return Marshal.PtrToStringUni((nint)path) ?? string.Empty;
    }

    private static bool OnK32EnumProcessModules(nint hProcess, nint* lphModule, uint cb, uint* lpcbNeeded)
    {
        if (PInvoke.GetProcessId((HANDLE)hProcess) != currentPID)
            return k32EnumProcessModules(hProcess, lphModule, cb, lpcbNeeded);

        var size = 1024 * sizeof(nint);
        nint* modules;
        while (true)
        {
            modules = (nint*)Marshal.AllocHGlobal(size);

            uint neededSize;
            if (!k32EnumProcessModules(hProcess, modules, (uint)size, &neededSize))
            {
                Marshal.FreeHGlobal((nint)modules);
                return false;
            }

            if (neededSize < size)
            {
                size = (int)neededSize;
                break;
            }

            size += 64 * sizeof(nint);
            Marshal.FreeHGlobal((nint)modules);
        }

        size = FilterModuleList(modules, size);

        if (lphModule != (nint*)0)
        {
            var returnedLength = Math.Min(size, (int)cb) / sizeof(nint);
            for (var i = 0; i < returnedLength; i++)
            {
                lphModule[i] = modules[i];
            }
        }

        Marshal.FreeHGlobal((nint)modules);

        if (lpcbNeeded != (uint*)0)
            *lpcbNeeded = (uint)size;

        return true;
    }

    private static bool OnK32EnumProcessModulesEx(nint hProcess, nint* lphModule, uint cb, uint* lpcbNeeded, uint dwFilterFlag)
    {
        if (PInvoke.GetProcessId((HANDLE)hProcess) != currentPID)
            return k32EnumProcessModulesEx(hProcess, lphModule, cb, lpcbNeeded, dwFilterFlag);

        var size = 1024 * sizeof(nint);
        nint* modules;
        while (true)
        {
            modules = (nint*)Marshal.AllocHGlobal(size);

            uint neededSize;
            if (!k32EnumProcessModulesEx(hProcess, modules, (uint)size, &neededSize, dwFilterFlag))
            {
                Marshal.FreeHGlobal((nint)modules);
                return false;
            }

            if (neededSize < size)
            {
                size = (int)neededSize;
                break;
            }

            size += 64 * sizeof(nint);
            Marshal.FreeHGlobal((nint)modules);
        }

        size = FilterModuleList(modules, size);

        if (lphModule != (nint*)0)
        {
            var returnedLength = Math.Min(size, (int)cb) / sizeof(nint);
            for (var i = 0; i < returnedLength; i++)
            {
                lphModule[i] = modules[i];
            }
        }

        Marshal.FreeHGlobal((nint)modules);

        if (lpcbNeeded != (uint*)0)
            *lpcbNeeded = (uint)size;

        return true;
    }

    private static int FilterModuleList(nint* modules, int sizeBytes)
    {
        var size = sizeBytes / sizeof(nint);
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var moveDownBy = 0;

        for (var i = 0; i < size; i++)
        {
            var handle = modules[i];

            if (handle != ourHandle)
            {
                var name = GetModuleName(handle);
                if (!IsModuleHidden(name))
                {
                    modules[i - moveDownBy] = handle;
                    continue;
                }
            }

            moveDownBy++;
        }

        return sizeBytes - moveDownBy * sizeof(nint);
    }

    private static bool IsModuleHidden(string name)
    {
        return name.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase)
            || name.StartsWith(dotnetDir, StringComparison.OrdinalIgnoreCase)
            || name.EndsWith("\\Microsoft.Extensions.DotNetDeltaApplier.dll", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetModuleName(nint handle)
    {
        uint size = 512;
        var buffer = new char[size];
        int returnedSize;
        fixed (char* p = buffer)
        {
            returnedSize = (int)PInvoke.GetModuleFileName((HMODULE)handle, p, size);
        }

        return new string(buffer, 0, returnedSize);
    }

    private static nint OnLdrGetDllFullName(nint hModule, nint lpFilename)
    {
        if (hModule == 0 || hModule == ourHandle)
            return ldrGetDllFullName(fakeExeHandle, lpFilename);

        return ldrGetDllFullName(hModule, lpFilename);
    }

    internal delegate nint LdrGetDllFullNameSig(nint hModule, nint lpFilename);
    internal delegate bool K32EnumProcessModulesSig(nint hProcess, nint* lphModule, uint cb, uint* lpcbNeeded);
    internal delegate bool K32EnumProcessModulesExSig(nint hProcess, nint* lphModule, uint cb, uint* lpcbNeeded, uint dwFilterFlag);
    internal delegate bool Module32Sig(nint hSnapshot, MODULEENTRY32W* lpme);
}
