using System;

namespace TweaksLauncher;

internal abstract class ModInterface
{
    public abstract Type InterfaceType { get; }

    public static ModInterface? GetFromType(Type type)
    {
        if (!typeof(IMod).IsAssignableFrom(type))
            return null;

        var resultType = typeof(ModInterface<>).MakeGenericType(type);

        return (ModInterface)Activator.CreateInstance(resultType)!;
    }

    public abstract void Initialize(LoadedMod mod);
}

internal class ModInterface<T> : ModInterface where T : IMod
{
    public override Type InterfaceType => typeof(T);

    public override void Initialize(LoadedMod mod)
    {
#if NETCOREAPP
        T.Initialize(mod);
#else
        InterfaceType.InvokeMember("Initialize",
            System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
            null, null, [mod]);
#endif
    }
}