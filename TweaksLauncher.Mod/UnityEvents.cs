namespace TweaksLauncher;

public static class UnityEvents
{
    public static event Action? FirstSceneLoad;

    internal static void InvokeFirstSceneLoad()
    {
        FirstSceneLoad?.Invoke();
    }
}
