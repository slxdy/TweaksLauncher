using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TweaksLauncher;

internal static class UnityTools
{
    private static Assembly? coreModule = null;

    [NotNull]
    private static Type? gameObject = null;
    [NotNull]
    private static MethodInfo? dontDestroyOnLoad = null;
    [NotNull]
    private static MethodInfo? addComponent1 = null;

    public static Type? MonoBehaviour { get; private set; }

    public static void Init()
    {
        if (coreModule != null)
            return;

        coreModule = Assembly.Load("UnityEngine.CoreModule");
        if (coreModule == null)
            return;

        gameObject = coreModule.GetType("UnityEngine.GameObject");
        addComponent1 = gameObject?.GetMethod("AddComponent", 1, []);
        dontDestroyOnLoad = coreModule.GetType("UnityEngine.Object")?.GetMethod("DontDestroyOnLoad");

        MonoBehaviour = coreModule?.GetType("UnityEngine.MonoBehaviour");
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
