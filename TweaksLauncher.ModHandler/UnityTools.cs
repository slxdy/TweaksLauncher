using System;
using System.Reflection;

namespace TweaksLauncher;

internal static class UnityTools
{
    private static Assembly? coreModule = null;

    private static Type gameObject = null!;
    private static MethodInfo dontDestroyOnLoad = null!;
    private static MethodInfo addComponent1 = null!;

    public static MethodInfo? Internal_ActiveSceneChanged { get; private set; }

    public static Type MonoBehaviour { get; private set; } = null!;

    public static void Init()
    {
        if (coreModule != null)
            return;

        coreModule = Assembly.Load("UnityEngine.CoreModule");
        if (coreModule == null)
            return;

        gameObject = coreModule.GetType("UnityEngine.GameObject")!;
        addComponent1 = gameObject?.GetMethod("AddComponent", [])!;
        dontDestroyOnLoad = coreModule.GetType("UnityEngine.Object")?.GetMethod("DontDestroyOnLoad")!;

        Internal_ActiveSceneChanged = coreModule.GetType("UnityEngine.SceneManagement.SceneManager")?.GetMethod("Internal_ActiveSceneChanged", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        MonoBehaviour = coreModule?.GetType("UnityEngine.MonoBehaviour")!;
    }

    public static object? CreateGameObject()
    {
        if (coreModule == null)
            return null;

        return Activator.CreateInstance(gameObject);
    }

    public static void AddComponent(object gameObject, Type componentType)
    {
        addComponent1.MakeGenericMethod(componentType).Invoke(gameObject, null);
    }

    public static void DontDestroyOnLoad(object gameObject)
    {
        dontDestroyOnLoad.Invoke(null, [gameObject]);
    }

    public static void CreateComponentSingleton(Type componentType)
    {
        var go = CreateGameObject();
        if (go == null)
            return;

        DontDestroyOnLoad(go);
        AddComponent(go, componentType);
    }
}
