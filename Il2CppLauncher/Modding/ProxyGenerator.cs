using Cpp2IL.Core;
using Il2CppInterop.Generator;
using Il2CppInterop.Generator.Runners;
using System.IO.Compression;

namespace Il2CppLauncher.Modding;

internal static class ProxyGenerator
{
    private static string targetDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Unity");
    private static HttpClient http = new();
    private static ModuleLogger logger = new("Proxy Generator");

    public unsafe static bool Generate()
    {
        var gameAssemblySize = new FileInfo(Program.Context.GameAssemblyPath).Length;
        var gameAssemblyInfo = Path.Combine(Program.Context.LauncherGameDirectory, "gameassembly.info");
        if (File.Exists(gameAssemblyInfo) && Directory.Exists(Program.Context.ProxiesDirectory))
        {
            try
            {
                using var infoReader = File.OpenRead(gameAssemblyInfo);
                var expectedSize = new BinaryReader(infoReader).ReadInt64();
                if (gameAssemblySize == expectedSize)
                    return true;
            }
            catch { }
        }

        logger.Log($"Generating proxy assemblies, this might take a minute...");
        try
        {
            var unityLibsPath = GetUnityAssemblies(Program.Context.UnityVersion);
            if (unityLibsPath == null)
                return false;

            Cpp2IlApi.InitializeLibCpp2Il(Program.Context.GameAssemblyPath, Program.Context.GlobalMetadataPath,
                [Program.Context.UnityVersion.Major, Program.Context.UnityVersion.Minor, Program.Context.UnityVersion.Build],
                false);

            var dummyAssemblies = Cpp2IlApi.MakeDummyDLLs();

            Il2CppInteropGenerator.Create(new()
            {
                GameAssemblyPath = Program.Context.GameAssemblyPath,
                Source = dummyAssemblies,
                OutputDir = Program.Context.ProxiesDirectory,
                UnityBaseLibsDir = unityLibsPath
            })
                .AddInteropAssemblyGenerator()
                .Run();

            foreach (var asm in dummyAssemblies)
                asm.Dispose();

            using var infoWriter = File.OpenWrite(gameAssemblyInfo);
            new BinaryWriter(infoWriter).Write(gameAssemblySize);
        }
        catch (Exception ex)
        {
            logger.Log("Failed to generate proxy assemblies.", "red");
            logger.Log(ex, "red");
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
        var versionDir = Path.Combine(targetDirectory, versionString);
        if (Directory.Exists(versionDir))
            return versionDir;

        var resp = await http.GetAsync($"https://github.com/LavaGang/Unity-Runtime-Libraries/raw/master/{versionString}.zip", HttpCompletionOption.ResponseHeadersRead);
        if (!resp.IsSuccessStatusCode)
            return null;

        using var content = new MemoryStream(await resp.Content.ReadAsByteArrayAsync());

        var zip = new ZipArchive(content, ZipArchiveMode.Read);

        zip.ExtractToDirectory(versionDir);
        return versionDir;
    }
}