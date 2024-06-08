#if IL2CPP

using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections;
using System.Drawing;

namespace TweaksLauncher.Il2Cpp;

[Il2CppImplements(typeof(Il2CppSystem.Collections.IEnumerator))]
internal class Il2CppEnumeratorWrapper : Il2CppSystem.Object
{
    private readonly IEnumerator enumerator = null!;

    static Il2CppEnumeratorWrapper()
    {
        ClassInjector.RegisterTypeInIl2Cpp<Il2CppEnumeratorWrapper>();
    }

    public Il2CppEnumeratorWrapper(IntPtr ptr) : base(ptr) { }

    public Il2CppEnumeratorWrapper(IEnumerator _enumerator) : base(ClassInjector.DerivedConstructorPointer<Il2CppEnumeratorWrapper>())
    {
        ClassInjector.DerivedConstructorBody(this);
        enumerator = _enumerator;
    }

    public Il2CppSystem.Object? Current
    {
        get => enumerator.Current switch
        {
            IEnumerator next => new Il2CppEnumeratorWrapper(next),
            Il2CppSystem.Object il2cppObject => il2cppObject,
            null => null,
            _ => throw new NotSupportedException($"{enumerator.GetType()}: Unsupported type {enumerator.Current.GetType()}"),
        };
    }

    public bool MoveNext()
    {
        try
        {
            return enumerator.MoveNext();
        }
        catch (Exception e)
        {
            ModHandler.Log(e.ToString(), Color.Red);

            return false;
        }
    }

    public void Reset() => enumerator.Reset();
}

#endif