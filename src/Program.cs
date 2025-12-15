using System;
using System.Runtime.InteropServices;

namespace LibGodotSharpExample;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("LibGodotSharp Example");
        Console.WriteLine("====================");
        Console.WriteLine();
        
        // Parse command line arguments
        bool runNative = args.Length == 0 || (args.Length > 0 && args[0] == "--native");
        bool runDemo = args.Length > 0 && args[0] == "--demo";
        
        if (args.Length > 0 && args[0] == "--help")
        {
            ShowHelp();
            return;
        }
        
        try
        {
            if (runDemo)
            {
                // Run the API demonstration (no native library required)
                Console.WriteLine("Running GodotSharp API demonstration...");
                Console.WriteLine("This shows how to use the GodotSharp assemblies.\n");
                GodotSharpExample.Run();
            }
            else
            {
                // Run the complete native example (creates actual window)
                Console.WriteLine("Running complete example with Godot window...");
                Console.WriteLine("This creates a window with a rotating 3D cube.\n");
                GodotNativeExample.Run();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"\nError: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }
    
    static void ShowHelp()
    {
        Console.WriteLine("Usage: dotnet run [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  (none) or --native    Run complete example with Godot window (default)");
        Console.WriteLine("  --demo                Run GodotSharp API demonstration only");
        Console.WriteLine("  --help                Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run                  # Creates Godot window with rotating cube");
        Console.WriteLine("  dotnet run --native         # Same as above");
        Console.WriteLine("  dotnet run --demo           # Shows API usage without native window");
        Console.WriteLine();
        Console.WriteLine("The native example requires the libgodot native library:");
        Console.WriteLine("  - Windows: godot.windows.template_release.x86_64.dll");
        Console.WriteLine("  - Linux:   libgodot.linuxbsd.template_release.x86_64.so");
        Console.WriteLine("  - macOS:   libgodot.macos.template_release.universal.dylib");
        Console.WriteLine();
        Console.WriteLine("To build the native library:");
        Console.WriteLine("  dotnet run --project GodotBuilder/GodotBuilder.csproj");
    }
}
