#if IL2CPP

using System.Collections;
using TweaksLauncher.Il2Cpp;

namespace TweaksLauncher;

public static class Il2CppExtensions
{
    public static UnityEngine.Coroutine StartCoroutine(this UnityEngine.MonoBehaviour obj, IEnumerator coroutine)
    {
        return obj.StartCoroutine(new Il2CppSystem.Collections.IEnumerator(new Il2CppEnumeratorWrapper(coroutine).Pointer));
    }
}

#endif