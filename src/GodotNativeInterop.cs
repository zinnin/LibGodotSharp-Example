using System;
using System.Runtime.InteropServices;

namespace LibGodotSharpExample;

/// <summary>
/// Native interop layer for Godot library
/// This class demonstrates how to P/Invoke into the Godot shared library
/// 
/// When Godot is built as a library (libgodot), it exposes C API functions
/// that can be called from C# using P/Invoke.
/// 
/// Reference: https://github.com/godotengine/godot/pull/110863
/// Example: https://github.com/migeran/libgodot
/// </summary>
public static class GodotNativeInterop
{
    // Library name varies by platform
    #if WINDOWS
    private const string LibGodot = "libgodot.windows.template_release.x86_64.dll";
    #elif OSX
    private const string LibGodot = "libgodot.macos.template_release.universal.dylib";
    #else
    private const string LibGodot = "libgodot.linuxbsd.template_release.x86_64.so";
    #endif
    
    /// <summary>
    /// Initialize the Godot engine
    /// </summary>
    /// <param name="argc">Argument count</param>
    /// <param name="argv">Argument values</param>
    /// <returns>0 on success, error code otherwise</returns>
    [DllImport(LibGodot, CallingConvention = CallingConvention.Cdecl)]
    public static extern int godot_initialize(int argc, IntPtr argv);
    
    /// <summary>
    /// Process one frame in the Godot engine
    /// </summary>
    /// <returns>true if should continue, false if should quit</returns>
    [DllImport(LibGodot, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool godot_iterate();
    
    /// <summary>
    /// Finalize and cleanup the Godot engine
    /// </summary>
    [DllImport(LibGodot, CallingConvention = CallingConvention.Cdecl)]
    public static extern void godot_finalize();
    
    // Additional functions that would be exposed by libgodot
    // These are examples - actual API may vary
    
    /// <summary>
    /// Create a new Godot object by class name
    /// </summary>
    [DllImport(LibGodot, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr godot_object_create(string className);
    
    /// <summary>
    /// Call a method on a Godot object
    /// </summary>
    [DllImport(LibGodot, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr godot_object_call(IntPtr obj, string method, IntPtr args, int argCount);
    
    /// <summary>
    /// Set a property on a Godot object
    /// </summary>
    [DllImport(LibGodot, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void godot_object_set(IntPtr obj, string property, IntPtr value);
    
    /// <summary>
    /// Get a property from a Godot object
    /// </summary>
    [DllImport(LibGodot, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr godot_object_get(IntPtr obj, string property);
}

/// <summary>
/// Higher-level wrapper around the native Godot interop
/// This demonstrates how to build a more C#-friendly API on top of the C API
/// </summary>
public class GodotLibrary
{
    private bool isInitialized = false;
    
    /// <summary>
    /// Initialize the Godot library
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;
            
        try
        {
            // In actual implementation:
            // IntPtr argv = Marshal.StringToHGlobalAnsi("godot");
            // int result = GodotNativeInterop.godot_initialize(1, argv);
            // Marshal.FreeHGlobal(argv);
            // 
            // if (result != 0)
            //     throw new InvalidOperationException($"Failed to initialize Godot: {result}");
            
            isInitialized = true;
        }
        catch (DllNotFoundException)
        {
            throw new InvalidOperationException(
                "Godot library not found. Please build Godot as a library first using build-godot.sh");
        }
    }
    
    /// <summary>
    /// Process one frame
    /// </summary>
    /// <returns>true if should continue</returns>
    public bool Iterate()
    {
        if (!isInitialized)
            throw new InvalidOperationException("Godot not initialized");
            
        // In actual implementation:
        // return GodotNativeInterop.godot_iterate();
        return true;
    }
    
    /// <summary>
    /// Shutdown the Godot library
    /// </summary>
    public void Shutdown()
    {
        if (!isInitialized)
            return;
            
        // In actual implementation:
        // GodotNativeInterop.godot_finalize();
        
        isInitialized = false;
    }
    
    /// <summary>
    /// Create a Godot object by class name
    /// </summary>
    public IntPtr CreateObject(string className)
    {
        if (!isInitialized)
            throw new InvalidOperationException("Godot not initialized");
            
        // In actual implementation:
        // return GodotNativeInterop.godot_object_create(className);
        return IntPtr.Zero;
    }
}