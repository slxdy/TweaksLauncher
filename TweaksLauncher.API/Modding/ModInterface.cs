namespace TweaksLauncher.Modding;

internal abstract class ModInterface
{
    public abstract Type InterfaceType { get; }

    public static ModInterface? GetFromType(Type type)
    {
        if (!type.IsAssignableTo(typeof(IMod)))
            return null;

        var resultType = typeof(ModInterface<>).MakeGenericType(type);

        return (ModInterface)Activator.CreateInstance(resultType)!;
    }

    public abstract void Initialize(ModInstance mod);
}

internal class ModInterface<T> : ModInterface where T : IMod
{
    public override Type InterfaceType => typeof(T);

    public override void Initialize(ModInstance mod)
    {
        T.Initialize(mod);
    }
}