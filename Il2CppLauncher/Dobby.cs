using System.Runtime.InteropServices;

namespace Il2CppLauncher;

internal unsafe static partial class Dobby
{
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

    public class Patch<T> where T : Delegate
    {
        public T Original { get; private set; }
        public T Detour { get; private set; }
        public nint Target { get; private set; }

        public Patch(nint target, T detour)
        {
            Target = target;
            Detour = detour;

            var original = Prepare(target, Marshal.GetFunctionPointerForDelegate(detour));
            Commit(target);

            Original = Marshal.GetDelegateForFunctionPointer<T>(original);
        }

        public Patch(string moduleName, string functionName, T detour) : this(NativeLibrary.GetExport(NativeLibrary.Load(moduleName), functionName), detour)
        {

        }

        public void Dispose()
        {
            if (Target == 0)
                return;

            Destroy(Target);
            Original = Marshal.GetDelegateForFunctionPointer<T>(Target);
            Target = 0;
        }
    }
}
