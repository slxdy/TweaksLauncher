using Cpp2IL.Core;
using Cpp2IL.Core.Api;
using Cpp2IL.Core.OutputFormats;
using Cpp2IL.Core.ProcessingLayers;
using Il2CppInterop.Generator;
using Il2CppInterop.Generator.Runners;
using LibCpp2IL;
using Mono.Cecil;
using System.Drawing;
using System.IO.Compression;
using TweaksLauncher.Utility;

namespace TweaksLauncher;

internal static class ProxyGenerator
{
    private static readonly string targetUnityDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Unity");
    private static readonly ModuleLogger logger = new("Proxy Generator");

    public unsafe static bool Generate()
    {
        var gameAssemblySize = new FileInfo(Launcher.Context.GameAssemblyPath).Length;
        var gameAssemblyInfo = Path.Combine(Launcher.Context.LauncherGameDirectory, "gameassembly.info");
        if (File.Exists(gameAssemblyInfo) && Directory.Exists(Launcher.Context.ProxiesDirectory))
        {
            try
            {
                using var infoReader = File.OpenRead(gameAssemblyInfo);
                var expectedSize = new BinaryReader(infoReader).ReadInt64();
                if (gameAssemblySize == expectedSize)
                    return true;

                Directory.Delete(Launcher.Context.ProxiesDirectory, true);
            }
            catch { }
        }

        logger.Log($"Generating proxy assemblies, this might take a minute...");
        try
        {
            var unityLibsPath = GetUnityAssemblies(Launcher.Context.UnityVersion);
            if (unityLibsPath == null)
                return false;

            Cpp2IlApi.Init();

            Cpp2IlApi.InitializeLibCpp2Il(Launcher.Context.GameAssemblyPath, Launcher.Context.GlobalMetadataPath,
                new((ushort)Launcher.Context.UnityVersion.Major, (ushort)Launcher.Context.UnityVersion.Minor, (ushort)Launcher.Context.UnityVersion.Build), false);

            var procLayers = new List<Cpp2IlProcessingLayer> { new AttributeInjectorProcessingLayer() };
            foreach (var layer in procLayers)
                layer.PreProcess(Cpp2IlApi.CurrentAppContext, procLayers);

            foreach (var layer in procLayers)
                layer.Process(Cpp2IlApi.CurrentAppContext);

            var asmResolverDummies = new AsmResolverDllOutputFormatEmpty().BuildAssemblies(Cpp2IlApi.CurrentAppContext);

            LibCpp2IlMain.Reset();
            Cpp2IlApi.CurrentAppContext = null;

            var cecilDummies = new List<AssemblyDefinition>(asmResolverDummies.Count);
            var cecilResolver = new CecilTools.RegistryAssemblyResolver();
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
                GameAssemblyPath = Launcher.Context.GameAssemblyPath,
                Source = cecilDummies,
                OutputDir = Launcher.Context.ProxiesDirectory,
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
            logger.Log("Failed to generate proxy assemblies.", Color.Red);
            logger.Log(ex, Color.Red);
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
        var versionDir = Path.Combine(targetUnityDirectory, versionString);
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
}