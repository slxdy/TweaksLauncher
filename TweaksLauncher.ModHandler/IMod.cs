namespace TweaksLauncher;

/// <summary>
/// The main interface that defines a mod type. Gives the mod the opportunity to initialize early. Multiple mod types are allowed.
/// <para>
/// <b>Requires a </b><c><see langword="static void"/> Initialize(<see cref="LoadedMod"/> mod)</c><b> method implementation.</b>
/// </para>
/// <para>
/// Example:
/// <code>
///<see langword="using"/> <see cref="TweaksLauncher"/>;
///<br></br><br></br>
///<see langword="namespace"/> MyMod;
///<br></br><br></br>
///<see langword="public class"/> Main : <see cref="IMod"/><br></br>
///{
///    // Runs early, when Unity has initialized, but before the first game scene is loaded.
///    <see langword="public static void"/> Initialize(<see cref="LoadedMod"/> mod)
///    {
///        <see cref="ModLogger"/>.Log("Hello World!");
///    }
///}
/// </code>
/// </para>
/// </summary>
public interface IMod
{
#if NETCOREAPP
    /// <summary>
    /// Runs early, when Unity has initialized, but before the first game scene is loaded
    /// </summary>
    /// <param name="mod">The current mod instance</param>
    public static abstract void Initialize(LoadedMod mod);
#endif
}
