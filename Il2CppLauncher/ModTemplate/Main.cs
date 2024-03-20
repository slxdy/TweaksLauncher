using Il2CppLauncher;

//namespace

public class Main : IMod
{
    // Register hooks and events in this function
    public static void Initialize()
    {
        ModLogger.Log("Hello World!");
    }
}