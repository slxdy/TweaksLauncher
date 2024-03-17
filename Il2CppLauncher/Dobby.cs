using System.Runtime.InteropServices;

namespace Il2CppLauncher;

internal unsafe static partial class Dobby
{
    //private static readonly string libPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dobby_x64.dll");

    //static Dobby()
    //{
    //    NativeLibrary.Load(libPath);
    //}

    [LibraryImport("dobby_x64", EntryPoint = "DobbyPrepare")]
    private static partial int Prepare(nint target, nint detour, nint* original);

    [LibraryImport("dobby_x64", EntryPoint = "DobbyCommit")]
    public static partial int Commit(nint target);

    [LibraryImport("dobby_x64", EntryPoint = "DobbyDestroy")]
    public static partial int Destroy(nint target);

    public static nint Prepare(nint target, nint detour)
    {
        nint original = 0;
        Prepare(target, detour, &original);
        return original;
    }

    public static T Patch<T>(nint target, T detour) where T : Delegate
    {
        var original = Prepare(target, Marshal.GetFunctionPointerForDelegate(detour));
        Commit(target);

        return Marshal.GetDelegateForFunctionPointer<T>(original);
    }

    public static T Patch<T>(string moduleName, string functionName, T detour) where T : Delegate
    {
        return Patch(NativeLibrary.GetExport(NativeLibrary.Load(moduleName), functionName), detour);
    }
}
