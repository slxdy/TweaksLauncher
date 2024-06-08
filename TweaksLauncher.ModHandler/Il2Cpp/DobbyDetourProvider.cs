#if IL2CPP

using Il2CppInterop.Runtime.Injection;
using System;
using System.Runtime.InteropServices;

namespace TweaksLauncher.Il2Cpp;

internal unsafe class DobbyDetourProvider : IDetourProvider
{
    public IDetour Create<TDelegate>(nint original, TDelegate target) where TDelegate : Delegate
    {
        return new DobbyDetour(original, target);
    }

    public class DobbyDetour(nint target, Delegate detour) : IDetour
    {
        public Delegate ManagedDetour { get; private set; } = detour;

        public nint Target { get; private set; } = target;

        public nint Detour { get; private set; } = Marshal.GetFunctionPointerForDelegate(detour);

        public nint OriginalTrampoline { get; private set; }

        public bool IsApplied { get; private set; }

        public bool IsPrepared { get; private set; }

        public void Apply()
        {
            if (IsApplied)
                return;

            if (!IsPrepared)
            {
                OriginalTrampoline = Dobby.Prepare(Target, Detour);
                IsPrepared = true;
            }

            _ = Dobby.Commit(Target);
            IsApplied = true;
        }

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);

            if (!IsApplied)
                return;

            _ = Dobby.Destroy(Target);
            IsApplied = false;
        }

        public T GenerateTrampoline<T>() where T : Delegate
        {
            if (!IsPrepared)
            {
                OriginalTrampoline = Dobby.Prepare(Target, Detour);
                IsPrepared = true;
            }

            return Marshal.GetDelegateForFunctionPointer<T>(OriginalTrampoline);
        }
    }
}

#endif