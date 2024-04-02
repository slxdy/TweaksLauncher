using System.Reflection;

namespace TweaksLauncher.Modding;

internal static class ProxyResolver
{
    private static bool inited;

    public static void Init()
    {
        if (inited)
            return;

        inited = true;

        AppDomain.CurrentDomain.AssemblyResolve += ResolveProxy;
    }

    private static Assembly? ResolveProxy(object? sender, ResolveEventArgs args)
    {
        AssemblyName name;
        try
        {
            name = new AssemblyName(args.Name);
        }
        catch
        {
            return null;
        }

        var proxyPath = Path.Combine(Launcher.Context.ProxiesDirectory, name.Name + ".dll");
        if (!File.Exists(proxyPath))
            return null;

        return Assembly.LoadFrom(proxyPath);
    }
}
