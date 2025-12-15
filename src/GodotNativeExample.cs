using System;
using System.Runtime.InteropServices;
using Godot;

namespace LibGodotSharpExample;

/// <summary>
/// Example demonstrating libgodot API integration with GodotSharp.
/// 
/// API Reference: https://github.com/godotengine/godot/blob/08e6cd181f98f9ca3f58d89af0a54ce3768552d3/core/extension/libgodot.h
/// 
/// The libgodot API provides:
/// - libgodot_create_godot_instance: Creates a Godot instance
/// - libgodot_destroy_godot_instance: Destroys a Godot instance
/// 
/// The GodotInstance object (returned from create) has methods:
/// - start(): Starts the instance
/// - iteration(): Processes one frame
/// - stop(): Stops the instance
/// 
/// NOTE: This example shows the API structure. Full integration requires a GDExtension
/// initialization function to be passed to libgodot_create_godot_instance.
/// For a working example, see: https://github.com/migeran/libgodot
/// </summary>
public class GodotNativeExample
{
    /// <summary>
    /// Gets the expected library name for the current platform
    /// </summary>
    private static string GetExpectedLibraryName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "godot.windows.template_release.x86_64.dll";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "libgodot.linuxbsd.template_release.x86_64.so";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "libgodot.macos.template_release.universal.dylib";
        else
            return "libgodot (unknown platform)";
    }

    // Platform-specific native function imports for libgodot API
    // API reference: https://github.com/godotengine/godot/blob/master/core/extension/libgodot.h
    // We need separate declarations for each platform because DllImport requires compile-time constants

    /// <summary>
    /// Creates a new Godot instance (Windows)
    /// </summary>
    [DllImport("godot.windows.template_release.x86_64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libgodot_create_godot_instance")]
    private static extern IntPtr LibGodotCreateInstance_Windows(int argc, IntPtr argv, IntPtr initFunc);

    /// <summary>
    /// Destroys an existing Godot instance (Windows)
    /// </summary>
    [DllImport("godot.windows.template_release.x86_64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libgodot_destroy_godot_instance")]
    private static extern void LibGodotDestroyInstance_Windows(IntPtr instance);

    /// <summary>
    /// Creates a new Godot instance (Linux)
    /// </summary>
    [DllImport("libgodot.linuxbsd.template_release.x86_64.so", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libgodot_create_godot_instance")]
    private static extern IntPtr LibGodotCreateInstance_Linux(int argc, IntPtr argv, IntPtr initFunc);

    /// <summary>
    /// Destroys an existing Godot instance (Linux)
    /// </summary>
    [DllImport("libgodot.linuxbsd.template_release.x86_64.so", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libgodot_destroy_godot_instance")]
    private static extern void LibGodotDestroyInstance_Linux(IntPtr instance);

    /// <summary>
    /// Creates a new Godot instance (macOS)
    /// </summary>
    [DllImport("libgodot.macos.template_release.universal.dylib", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libgodot_create_godot_instance")]
    private static extern IntPtr LibGodotCreateInstance_macOS(int argc, IntPtr argv, IntPtr initFunc);

    /// <summary>
    /// Destroys an existing Godot instance (macOS)
    /// </summary>
    [DllImport("libgodot.macos.template_release.universal.dylib", CallingConvention = CallingConvention.Cdecl, EntryPoint = "libgodot_destroy_godot_instance")]
    private static extern void LibGodotDestroyInstance_macOS(IntPtr instance);

    // Platform-agnostic wrappers that call the correct platform-specific function
    private static IntPtr LibGodotCreateInstance(int argc, IntPtr argv, IntPtr initFunc)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return LibGodotCreateInstance_Windows(argc, argv, initFunc);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return LibGodotCreateInstance_Linux(argc, argv, initFunc);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return LibGodotCreateInstance_macOS(argc, argv, initFunc);
        else
            throw new PlatformNotSupportedException($"Platform not supported: {RuntimeInformation.OSDescription}");
    }

    private static void LibGodotDestroyInstance(IntPtr instance)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            LibGodotDestroyInstance_Windows(instance);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            LibGodotDestroyInstance_Linux(instance);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            LibGodotDestroyInstance_macOS(instance);
        else
            throw new PlatformNotSupportedException($"Platform not supported: {RuntimeInformation.OSDescription}");
    }

    /// <summary>
    /// Main entry point - demonstrates libgodot API structure
    /// </summary>
    public static void Run()
    {
        Console.WriteLine("=== libgodot API Integration Example ===\n");
        Console.WriteLine("Demonstrates the correct libgodot API (Godot 4.6+)");
        Console.WriteLine("API Reference: https://github.com/godotengine/godot/blob/08e6cd181f98f9ca3f58d89af0a54ce3768552d3/core/extension/libgodot.h\n");

        Console.WriteLine($"Detected platform: {RuntimeInformation.OSDescription}");
        Console.WriteLine($"Expected library: {GetExpectedLibraryName()}\n");

        try
        {
            Console.WriteLine("Attempting to create Godot instance...");
            Console.WriteLine("\nNOTE: libgodot_create_godot_instance requires:");
            Console.WriteLine("  1. A GDExtension initialization function");
            Console.WriteLine("  2. Proper Godot project setup (project.godot, etc.)");
            Console.WriteLine("\nWithout these, Godot will fail to initialize.\n");
            
            // The correct libgodot API requires:
            // 1. Command line arguments (argc, argv)
            // 2. GDExtension initialization function pointer
            
            // Prepare argv - try to pass arguments that might help
            // Note: Godot will still look for project files
            IntPtr argv = Marshal.StringToHGlobalAnsi("LibGodotSharpExample");
            
            try
            {
                // NOTE: The third parameter (p_init_func) is a GDExtension initialization function
                // This is typically provided by a GDExtension plugin
                // Without a proper init function and project setup, this will fail
                IntPtr gdInstance = LibGodotCreateInstance(1, argv, IntPtr.Zero);
                
                if (gdInstance == IntPtr.Zero)
                {
                    Console.WriteLine("✗ Failed to create Godot instance");
                    Console.WriteLine("\nThis is expected behavior because:");
                    Console.WriteLine("  • No GDExtension initialization function provided");
                    Console.WriteLine("  • No Godot project files (.pck or project.godot)");
                    Console.WriteLine("  • libgodot requires a complete Godot project context\n");
                    ShowAPIDocumentation();
                    return;
                }
                
                Console.WriteLine("✓ Godot instance created successfully!");
                
                // With a valid GodotInstance, you would:
                // 1. Cast it to a GodotInstance object (through GodotSharp)
                // 2. Call instance.Start()
                // 3. Loop calling instance.Iteration()
                // 4. Call instance.Stop()
                
                // Cleanup
                LibGodotDestroyInstance(gdInstance);
                Console.WriteLine("✓ Godot instance destroyed");
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }
        catch (DllNotFoundException ex)
        {
            Console.WriteLine($"✗ Native library not found: {ex.Message}\n");
            Console.WriteLine($"Looking for: {GetExpectedLibraryName()}");
            Console.WriteLine("\nTo build the native library:");
            Console.WriteLine("  dotnet run --project GodotBuilder/GodotBuilder.csproj\n");
            ShowAPIDocumentation();
        }
        catch (EntryPointNotFoundException ex)
        {
            Console.WriteLine($"✗ Entry point not found: {ex.Message}\n");
            Console.WriteLine("This error occurs if:");
            Console.WriteLine("  1. The DLL is not built with libgodot support");
            Console.WriteLine("  2. The Godot version doesn't support libgodot (need 4.6+)");
            Console.WriteLine("  3. The DLL was built without library_type=shared_library\n");
            ShowAPIDocumentation();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error during Godot instance creation:");
            Console.WriteLine($"   {ex.Message}\n");
            
            if (ex.Message.Contains(".pck") || ex.Message.Contains("project data"))
            {
                Console.WriteLine("This error indicates Godot is looking for project files.");
                Console.WriteLine("\nlibgodot requires a complete Godot project context:");
                Console.WriteLine("  1. A Godot project directory with project.godot");
                Console.WriteLine("  2. A .pck file (packed project) or project source files");
                Console.WriteLine("  3. A GDExtension initialization function");
                Console.WriteLine("\nUsing libgodot as a pure library (without a Godot project) is not");
                Console.WriteLine("supported in the current Godot architecture. libgodot is designed to");
                Console.WriteLine("embed a full Godot project into another application.\n");
            }
            
            ShowAPIDocumentation();
        }
    }

    /// <summary>
    /// Shows the correct libgodot API documentation and usage
    /// </summary>
    private static void ShowAPIDocumentation()
    {
        Console.WriteLine("\n=== libgodot API Documentation ===\n");
        Console.WriteLine("The libgodot API (Godot 4.6+) provides two main functions:");
        Console.WriteLine();
        Console.WriteLine("1. libgodot_create_godot_instance:");
        Console.WriteLine("   GDExtensionObjectPtr libgodot_create_godot_instance(");
        Console.WriteLine("       int p_argc,");
        Console.WriteLine("       char *p_argv[],");
        Console.WriteLine("       GDExtensionInitializationFunction p_init_func");
        Console.WriteLine("   );");
        Console.WriteLine();
        Console.WriteLine("   Creates a GodotInstance object. Requires:");
        Console.WriteLine("   - Command line arguments (argc, argv)");
        Console.WriteLine("   - GDExtension initialization function (from your plugin/app)");
        Console.WriteLine("   - A Godot project (project.godot or .pck file)");
        Console.WriteLine();
        Console.WriteLine("2. libgodot_destroy_godot_instance:");
        Console.WriteLine("   void libgodot_destroy_godot_instance(GDExtensionObjectPtr p_godot_instance);");
        Console.WriteLine();
        Console.WriteLine("   Destroys the GodotInstance and cleans up.");
        Console.WriteLine();
        Console.WriteLine("The returned GodotInstance object has methods:");
        Console.WriteLine("   - bool start()      : Starts the Godot instance");
        Console.WriteLine("   - bool iteration()  : Processes one frame");
        Console.WriteLine("   - void stop()       : Stops the instance");
        Console.WriteLine();
        Console.WriteLine("IMPORTANT: libgodot Usage Model");
        Console.WriteLine("--------------------------------");
        Console.WriteLine("libgodot is designed to embed a complete Godot project into another");
        Console.WriteLine("application (like a Swift app or C++ application). It is NOT a pure");
        Console.WriteLine("library that you can call from C# without a Godot project.");
        Console.WriteLine();
        Console.WriteLine("To use libgodot, you need:");
        Console.WriteLine("  1. A complete Godot project (with project.godot or exported .pck)");
        Console.WriteLine("  2. A GDExtension that provides the initialization function");
        Console.WriteLine("  3. The host application (C++/Swift/etc.) that calls libgodot");
        Console.WriteLine();
        Console.WriteLine("Example workflow:");
        Console.WriteLine("  1. Create a Godot project with your game/app logic");
        Console.WriteLine("  2. Create a GDExtension plugin for host app integration");
        Console.WriteLine("  3. Export your project or use it in-place");
        Console.WriteLine("  4. From your C#/C++/Swift app, call libgodot_create_godot_instance");
        Console.WriteLine("  5. The Godot project runs embedded in your application");
        Console.WriteLine();
        Console.WriteLine("For working examples, see:");
        Console.WriteLine("  - https://github.com/migeran/libgodot (C++/Swift examples)");
        Console.WriteLine("  - https://github.com/godotengine/godot/blob/master/core/extension/libgodot.h");
        Console.WriteLine();
        Console.WriteLine("Alternative Approach:");
        Console.WriteLine("  If you want to use GodotSharp from C# without libgodot, consider:");
        Console.WriteLine("  - Creating a standard Godot project");
        Console.WriteLine("  - Using C# scripts within Godot");
        Console.WriteLine("  - Running Godot normally (not as an embedded library)");
    }
}
