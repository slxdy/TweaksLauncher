using System.Runtime.InteropServices;

namespace TweaksLauncher;

internal static unsafe class Mono
{
    public static nint Domain {  get; private set; }

    public static void Init(nint domainPtr)
    {
        var lib = NativeLibrary.Load(Program.runtimePath);

        mono_domain_assembly_open = Marshal.GetDelegateForFunctionPointer<mono_domain_assembly_open_sig>(NativeLibrary.GetExport(lib, "mono_domain_assembly_open"));
        mono_assembly_get_image = Marshal.GetDelegateForFunctionPointer<mono_assembly_get_image_sig>(NativeLibrary.GetExport(lib, "mono_assembly_get_image"));
        mono_class_from_name = Marshal.GetDelegateForFunctionPointer<mono_class_from_name_sig>(NativeLibrary.GetExport(lib, "mono_class_from_name"));
        mono_class_get_method_from_name = Marshal.GetDelegateForFunctionPointer<mono_class_get_method_from_name_sig>(NativeLibrary.GetExport(lib, "mono_class_get_method_from_name"));
        mono_runtime_invoke = Marshal.GetDelegateForFunctionPointer<mono_runtime_invoke_sig>(NativeLibrary.GetExport(lib, "mono_runtime_invoke"));
        mono_string_new_utf16 = Marshal.GetDelegateForFunctionPointer<mono_string_new_utf16_sig>(NativeLibrary.GetExport(lib, "mono_string_new_utf16"));
        mono_add_internal_call = Marshal.GetDelegateForFunctionPointer<mono_add_internal_call_sig>(NativeLibrary.GetExport(lib, "mono_add_internal_call"));
        mono_string_to_utf16 = Marshal.GetDelegateForFunctionPointer<mono_string_to_utf16_sig>(NativeLibrary.GetExport(lib, "mono_string_to_utf16"));

        Domain = domainPtr;
    }

    public static nint NewString(string ourString)
    {
        return mono_string_new_utf16(Domain, ourString, ourString.Length);
    }

#pragma warning disable IDE1006 // Naming Styles
    internal static mono_domain_assembly_open_sig mono_domain_assembly_open { get; private set; } = null!;
    internal static mono_assembly_get_image_sig mono_assembly_get_image { get; private set; } = null!;
    internal static mono_class_from_name_sig mono_class_from_name { get; private set; } = null!;
    internal static mono_class_get_method_from_name_sig mono_class_get_method_from_name { get; private set; } = null!;
    internal static mono_runtime_invoke_sig mono_runtime_invoke { get; private set; } = null!;
    internal static mono_string_new_utf16_sig mono_string_new_utf16 { get; private set; } = null!;
    internal static mono_add_internal_call_sig mono_add_internal_call { get; private set; } = null!;
    internal static mono_string_to_utf16_sig mono_string_to_utf16 { get; private set; } = null!;
#pragma warning restore IDE1006 // Naming Styles

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    internal delegate nint mono_domain_assembly_open_sig(nint domain, string name);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate nint mono_assembly_get_image_sig(nint assembly);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    internal delegate nint mono_class_from_name_sig(nint image, string nameSpace, string name);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    internal delegate nint mono_class_get_method_from_name_sig(nint clas, string name, int argsCount);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate nint mono_runtime_invoke_sig(nint method, nint obj, nint* args, nint* exception);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    internal delegate nint mono_string_new_utf16_sig(nint domain, string value, int length);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    internal delegate nint mono_add_internal_call_sig(string methodName, nint func);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    internal delegate string? mono_string_to_utf16_sig(nint monoString);
}
