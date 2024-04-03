using GameFinder.RegistryUtils;
using GameFinder.StoreHandlers.Steam;
using GameFinder.StoreHandlers.Steam.Models.ValueTypes;
using NexusMods.Paths;

namespace TweaksLauncher.Utility;

internal static class SteamTools
{
    private static readonly SteamHandler handler = new(FileSystem.Shared, WindowsRegistry.Shared);

    public static string? GetPathFromShortcut(string shortcutPath)
    {
        if (!shortcutPath.EndsWith(".url", StringComparison.OrdinalIgnoreCase))
            return null;

        string contents;
        try
        {
            contents = File.ReadAllText(shortcutPath);
        }
        catch
        {
            return null;
        }

        var pattern = "URL=steam://rungameid/";
        var startIdx = contents.IndexOf(pattern);
        if (startIdx == -1)
            return null;

        startIdx += pattern.Length;

        var endIdx = contents.IndexOf('\r', startIdx);
        if (endIdx == -1)
            return null;

        var length = endIdx - startIdx;

        if (!uint.TryParse(contents.AsSpan(startIdx, length), out var appId))
            return null;

        var game = handler.FindOneGameById(AppId.From(appId), out _);
        return game?.Path.GetFullPath();
    }
}
