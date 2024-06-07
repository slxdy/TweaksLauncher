namespace TweaksLauncher;

/// <summary>
/// Common Unity events that can be used in mods
/// </summary>
public static class UnityEvents
{
    /// <summary>
    /// Occurs when the first game scene is loaded.
    /// </summary>
    public static event Action? FirstSceneLoad;

    internal static void InvokeFirstSceneLoad()
    {
        FirstSceneLoad?.Invoke();
    }
}
