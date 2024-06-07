namespace TweaksLauncher;

/// <summary>
/// The main interface that defines a mod type. Gives the mod the opportunity to initialize early. Multiple mod types are allowed.
/// </summary>
public interface IMod
{
    /// <summary>
    /// Runs early, when Unity has initialized, but before the first game scene is loaded
    /// </summary>
    /// <param name="mod">The returned mod instance</param>
    public static abstract void Initialize(LoadedMod mod);
}
