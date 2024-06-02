using System.Runtime.InteropServices;

namespace TweaksLauncher;

internal static unsafe partial class Imports
{
    [LibraryImport("ntdll")]
    internal static partial void RtlCopyUnicodeString(Windows.Win32.Foundation.UNICODE_STRING* destination, Windows.Win32.Foundation.UNICODE_STRING* source);
}
