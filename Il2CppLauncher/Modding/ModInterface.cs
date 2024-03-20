namespace Il2CppLauncher.Modding;

internal abstract class ModInterface
{
    public static ModInterface? GetFromType(Type type)
    {
        if (!type.IsAssignableTo(typeof(IMod)))
            return null;

        var resultType = typeof(ModInterface<>).MakeGenericType(type);

        return (ModInterface)Activator.CreateInstance(resultType)!;
    }

    public abstract void Initialize();
}

internal class ModInterface<T> : ModInterface where T : IMod
{
    public override void Initialize()
    {
        T.Initialize();
    }
}