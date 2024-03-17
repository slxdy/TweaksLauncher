using Il2CppInterop.Runtime.Injection;
using System.Runtime.InteropServices;

namespace Il2CppLauncher.Il2CppInteropImpl;

internal unsafe class DobbyDetourProvider : IDetourProvider
{
    public IDetour Create<TDelegate>(nint original, TDelegate target) where TDelegate : Delegate
    {
        return new DobbyDetour(original, target);
    }

    public class DobbyDetour : IDetour
    {
        private readonly Delegate detourObj;

        public nint Target { get; private set; }

        public nint Detour { get; private set; }

        public nint OriginalTrampoline { get; private set; }

        public bool IsApplied { get; private set; }

        public DobbyDetour(nint target, Delegate detour)
        {
            detourObj = detour;
            Target = target;
            Detour = Marshal.GetFunctionPointerForDelegate(detour);
        }

        public void Apply()
        {
            if (IsApplied)
                return;

            OriginalTrampoline = Dobby.Prepare(Target, Detour);
            Dobby.Commit(Target);
            IsApplied = true;
        }

        public void Dispose()
        {
            if (!IsApplied)
                return;

            Dobby.Destroy(Target);
            IsApplied = false;
        }

        public T GenerateTrampoline<T>() where T : Delegate
        {
            OriginalTrampoline = Dobby.Prepare(Target, Detour);

            return Marshal.GetDelegateForFunctionPointer<T>(OriginalTrampoline);
        }
    }
}
