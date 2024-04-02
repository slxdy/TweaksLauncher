using System.Runtime.InteropServices;

namespace TweaksLauncher;

/// <summary>
/// A managed wrapper around the Dobby library
/// </summary>
public unsafe static partial class Dobby
{
    static Dobby()
    {
        NativeLibrary.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", "Dobby", Environment.Is64BitProcess ? "win-x64" : "win-x86", "dobby.dll"));
    }

    [LibraryImport("dobby", EntryPoint = "DobbyPrepare")]
    public static partial int Prepare(nint target, nint detour, nint* original);

    [LibraryImport("dobby", EntryPoint = "DobbyCommit")]
    public static partial int Commit(nint target);

    [LibraryImport("dobby", EntryPoint = "DobbyDestroy")]
    public static partial int Destroy(nint target);

    public static nint Prepare(nint target, nint detour)
    {
        nint original = 0;
        if (Prepare(target, detour, &original) != 0)
        {
            throw new AccessViolationException($"Could not prepare patch to target {target:X}");
        }
        return original;
    }

    /// <summary>
    /// Creates a patch for a native function
    /// </summary>
    /// <typeparam name="TDelegate">The delegate for the detour function. Cannot be a generic delegate</typeparam>
    /// <param name="target">The target function pointer</param>
    /// <param name="detour">The detour function</param>
    /// <exception cref="AccessViolationException">Occurs when the patch fails</exception>
    /// <exception cref="ArgumentException">Occurs when the detour delegate is generic</exception>
    public static Patch<TDelegate> CreatePatch<TDelegate>(nint target, TDelegate detour) where TDelegate : Delegate
    {
        var original = Prepare(target, Marshal.GetFunctionPointerForDelegate(detour));
        if (Commit(target) != 0)
        {
            throw new AccessViolationException($"Could not commit patch to target {target:X}");
        }

        var originalDel = Marshal.GetDelegateForFunctionPointer<TDelegate>(original);

        return new Patch<TDelegate>(original, detour, originalDel);
    }

    /// <summary>
    /// Creates a patch for an exported function
    /// </summary>
    /// <typeparam name="TDelegate">The delegate for the detour function. Cannot be a generic delegate</typeparam>
    /// <param name="moduleName">The name of the DLL to patch</param>
    /// <param name="functionName">The name of the exported function to patch</param>
    /// <param name="detour">The detour function</param>
    /// <exception cref="DllNotFoundException">Occurs when the DLL could not be found</exception>
    /// <exception cref="EntryPointNotFoundException">Occurs when the exported function could not be found</exception>
    /// <exception cref="AccessViolationException">Occurs when the patch fails</exception>
    /// <exception cref="ArgumentException">Occurs when the detour delegate is generic</exception>
    public static Patch<TDelegate> CreatePatch<TDelegate>(string moduleName, string functionName, TDelegate detour) where TDelegate : Delegate
    {
        return CreatePatch(NativeLibrary.GetExport(NativeLibrary.Load(moduleName), functionName), detour);
    }

    public class Patch<T> where T : Delegate
    {
        public nint Target { get; private set; }
        public T Detour { get; private set; }
        public T Original { get; private set; }

        internal Patch(nint target, T detour, T original)
        {
            Target = target;
            Detour = detour;
            Original = original;
        }

        /// <summary>
        /// Destroys the existing patch
        /// </summary>
        /// <exception cref="AccessViolationException">Occurs when destroying the patch fails</exception>
        public void Destroy()
        {
            if (Target == 0)
                return;

            var result = Dobby.Destroy(Target);
            if (result is not 0 and not -1)
            {
                throw new AccessViolationException($"Could not destroy patch for target {Target:X}");
            }
            Original = Marshal.GetDelegateForFunctionPointer<T>(Target);
            Target = 0;
        }
    }
}
