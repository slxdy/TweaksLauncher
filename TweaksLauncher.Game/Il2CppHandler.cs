using Cpp2IL.Core;
using Cpp2IL.Core.Api;
using Cpp2IL.Core.OutputFormats;
using Cpp2IL.Core.ProcessingLayers;
using Il2CppInterop.Generator;
using Il2CppInterop.Generator.Runners;
using LibCpp2IL;
using Mono.Cecil;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO.Compression;
using System.Reflection;

namespace TweaksLauncher;

internal static class Il2CppHandler
{
    private static readonly string unityVersionsDir = Path.Combine(Program.baseDir, "Unity");

    [NotNull] private static Dobby.Patch<Il2CppInitSig>? il2cppInit = null;

    internal static int Start()
    {
        if (!GenerateProxies())
            return 10;

        il2cppInit = Dobby.CreatePatch<Il2CppInitSig>(Program.runtimePath, "il2cpp_init", OnIl2CppInit);

        return GameManager.Start();
    }

    private static void LoadModHandler()
    {
        var handlerPath = Path.Combine(Program.baseDir, "bin", "IL2CPP", "TweaksLauncher.dll");
        if (!File.Exists(handlerPath))
        {
            Logger.Log($"Could not find the mod handler. Please reinstall TweaksLauncher.", Color.Red);
            return;
        }

        var modHandler = Assembly.LoadFrom(handlerPath);

        modHandler.GetType("TweaksLauncher.ModHandler", true)!
            .InvokeMember("Start", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null,
            [Program.baseDir, Program.gameName, Program.gamePath, new Action<string?, byte, byte, byte, string?, byte, byte, byte>(Logger.Log)]);
    }

    private static nint OnIl2CppInit(nint domainName)
    {
        il2cppInit.Destroy();

        Logger.Log("Creating Il2Cpp Domain");

        var result = il2cppInit.Original(domainName);

        LoadModHandler();

        return result;
    }

    private unsafe static bool GenerateProxies()
    {
        var gameAssemblySize = new FileInfo(Program.runtimePath).Length;
        var gameAssemblyInfo = Path.Combine(Program.launcherGamePath, "gameassembly.info");
        var proxiesDir = Path.Combine(Program.launcherGamePath, "Proxies");
        if (File.Exists(gameAssemblyInfo) && Directory.Exists(proxiesDir))
        {
            try
            {
                using var infoReader = File.OpenRead(gameAssemblyInfo);
                var expectedSize = new BinaryReader(infoReader).ReadInt64();
                if (gameAssemblySize == expectedSize)
                    return true;
            }
            catch { }

            Directory.Delete(proxiesDir, true);
        }

        Logger.Log($"Generating IL2CPP proxy assemblies, this might take a minute...");

        var metadataPath = Path.Combine(Program.gameDataDir, "il2cpp_data", "Metadata", "global-metadata.dat");
        if (!File.Exists(metadataPath))
        {
            Logger.Log($"Could not find the IL2CPP metadata. The engine might be modified", Color.Red);
            return false;
        }

        try
        {
            var unityLibsPath = GetUnityAssemblies(Program.unityVersion);
            if (unityLibsPath == null)
            {
                Logger.Log($"Failed to download Unity assemblies.", Color.Red);
                return false;
            }

            Cpp2IlApi.Init();

            Cpp2IlApi.InitializeLibCpp2Il(Program.runtimePath, metadataPath,
                new((ushort)Program.unityVersion.Major, (ushort)Program.unityVersion.Minor, (ushort)Program.unityVersion.Build), false);

            var procLayers = new List<Cpp2IlProcessingLayer> { new AttributeInjectorProcessingLayer() };
            foreach (var layer in procLayers)
                layer.PreProcess(Cpp2IlApi.CurrentAppContext, procLayers);

            foreach (var layer in procLayers)
                layer.Process(Cpp2IlApi.CurrentAppContext);

            var asmResolverDummies = new AsmResolverDllOutputFormatEmpty().BuildAssemblies(Cpp2IlApi.CurrentAppContext);

            LibCpp2IlMain.Reset();
            Cpp2IlApi.CurrentAppContext = null;

            var cecilDummies = new List<AssemblyDefinition>(asmResolverDummies.Count);
            var cecilResolver = new RegistryAssemblyResolver();
            using var memoryStream = new MemoryStream(100000); //100 kb initial capacity
            foreach (var dummy in asmResolverDummies)
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                memoryStream.SetLength(0);

                dummy.Modules.First().Write(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);

                var cecilDummy = AssemblyDefinition.ReadAssembly(memoryStream, new()
                {
                    AssemblyResolver = cecilResolver
                });

                cecilResolver.Register(cecilDummy);

                cecilDummies.Add(cecilDummy);
            }

            using var interopGenerator = Il2CppInteropGenerator.Create(new()
            {
                GameAssemblyPath = Program.runtimePath,
                Source = cecilDummies,
                OutputDir = proxiesDir,
                UnityBaseLibsDir = unityLibsPath,
                Parallel = true,
                NoXrefCache = true
            });

            interopGenerator.AddInteropAssemblyGenerator();
            interopGenerator.Run();

            foreach (var asm in cecilDummies)
                asm.Dispose();

            using var infoWriter = File.OpenWrite(gameAssemblyInfo);
            new BinaryWriter(infoWriter).Write(gameAssemblySize);
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to generate proxy assemblies.", Color.Red);
            Logger.Log(ex.ToString(), Color.Red);
            return false;
        }

        return true;
    }

    private static string? GetUnityAssemblies(Version unityVersion)
    {
        return GetUnityAssembliesAsync(unityVersion).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    private static async Task<string?> GetUnityAssembliesAsync(Version unityVersion)
    {
        var versionString = $"{unityVersion.Major}.{unityVersion.Minor}.{unityVersion.Build}";
        var versionDir = Path.Combine(unityVersionsDir, versionString);
        if (Directory.Exists(versionDir))
            return versionDir;

        using var http = new HttpClient();
        var resp = await http.GetAsync($"https://github.com/LavaGang/Unity-Runtime-Libraries/raw/master/{versionString}.zip", HttpCompletionOption.ResponseContentRead);
        if (!resp.IsSuccessStatusCode)
            return null;

        using var content = new MemoryStream(await resp.Content.ReadAsByteArrayAsync());

        using var zip = new ZipArchive(content, ZipArchiveMode.Read);

        zip.ExtractToDirectory(versionDir);
        return versionDir;
    }

    public class RegistryAssemblyResolver : DefaultAssemblyResolver
    {
        public void Register(AssemblyDefinition assembly)
        {
            RegisterAssembly(assembly);
        }
    }

    internal delegate nint Il2CppInitSig(nint domainName);
}