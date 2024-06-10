using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;

namespace TweaksLauncher;

internal static unsafe class MonoHandler
{
    [NotNull] private static Dobby.Patch<MonoJitInitSig>? monoJitInit = null;

    private static LogImplSig? logImpl;

    internal static int Start()
    {
        monoJitInit = Dobby.CreatePatch<MonoJitInitSig>(Program.runtimePath, "mono_jit_init_version", OnDomainInit);

        return GameManager.Start();
    }

    private static void LoadModHandler(nint domain)
    {
        var handlerPath = Path.Combine(Program.baseDir, "bin", "Mono", "TweaksLauncher.dll");
        if (!File.Exists(handlerPath))
        {
            Logger.Log($"Could not find the mod handler. Please reinstall TweaksLauncher.", Color.Red);
            return;
        }

        Mono.Init();

        var modHandlerAsm = Mono.mono_domain_assembly_open(domain, handlerPath);
        if (modHandlerAsm == 0)
        {
            Logger.Log($"Failed to load Mod Handler into the Mono domain.", Color.Red);
            return;
        }

        var modHandlerImg = Mono.mono_assembly_get_image(modHandlerAsm);
        var modHandlerClass = Mono.mono_class_from_name(modHandlerImg, "TweaksLauncher", "ModHandler");
        var startMethod = Mono.mono_class_get_method_from_name(modHandlerClass, "Start", 3);

        if (startMethod == 0)
        {
            Logger.Log($"Failed to get the Mod Handler's Start method'.", Color.Red);
            return;
        }

        logImpl = new LogImplSig(LogImpl);
        var logImplPtr = Marshal.GetFunctionPointerForDelegate(logImpl);
        Mono.mono_add_internal_call("TweaksLauncher.ModHandler::LogInternal", logImplPtr);

        var baseDir = Mono.mono_string_new(domain, Program.baseDir);
        var gameName = Mono.mono_string_new(domain, Program.gameName);
        var gameDir = Mono.mono_string_new(domain, Program.gamePath);

        var args = stackalloc nint[3];
        args[0] = baseDir;
        args[1] = gameName;
        args[2] = gameDir;

        Mono.mono_runtime_invoke(startMethod, 0, args, null);
    }

    private static void LogImpl(nint monoMessage, byte baseColorR, byte baseColorG, byte baseColorB, nint monoModuleName, byte moduleColorR, byte moduleColorG, byte moduleColorB)
    {
        Logger.Log(monoMessage == 0 ? null : Mono.mono_string_to_utf16(monoMessage), baseColorR, baseColorG, baseColorB, monoModuleName == 0 ? null : Mono.mono_string_to_utf16(monoModuleName), moduleColorR, moduleColorG, moduleColorB);
    }

    private static nint OnDomainInit(nint domainName, nint a)
    {
        monoJitInit.Destroy();

        Logger.Log("Creating Mono Domain");

        var domain = monoJitInit.Original(domainName, a);

        LoadModHandler(domain);

        return domain;
    }

    internal delegate nint MonoJitInitSig(nint domainName, nint a);
    private delegate void LogImplSig(nint monoMessage, byte baseColorR, byte baseColorG, byte baseColorB, nint monoModuleName, byte moduleColorR, byte moduleColorG, byte moduleColorB);
}
