using System.Reflection;

namespace TweaksLauncher;

internal static class Program
{
    private static readonly string apiPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", "TweaksLauncher.API.dll");

    private static int Main(string[] args)
    {
        Assembly.LoadFrom(apiPath);

        return CallMain(args);
    }

    // This requires a separate method, otherwise JIT will try to resolve the reference before we load the assembly.
    // Inlining doesn't seem to be a problem.
    private static int CallMain(string[] args)
    {
        return Launcher.Main(args);
    }
}
